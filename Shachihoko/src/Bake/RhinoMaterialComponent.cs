using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.IO;
using Rhino.DocObjects;
using System.Drawing;
using Rhino.Render;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Shachihoko
{
    public class RhinoMaterialComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public RhinoMaterialComponent()
          : base("Material", "Material",
              "Material",
              "Shachihoko", ShachihokoMethod.Category["Bake"])
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddColourParameter("Diffuse", "Diffuse", "光の直接当たる部分の色.", GH_ParamAccess.tree, System.Drawing.Color.White);
            pManager.AddColourParameter("Specular", "Specular", "光源の映り込み. 金属系ならDiffuseColorに設定, プラスチック系ならWhiteに設定.", GH_ParamAccess.tree);
            pManager.AddColourParameter("Ambient", "Ambient", "	光の直接当たらない部分の色. 一般的に濃いグレーを入力する.", GH_ParamAccess.tree);
            pManager.AddColourParameter("Emission", "Emission", "自己発光色.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Transparency", "Transparency", "透明率", GH_ParamAccess.tree, 0.0);
            pManager.AddNumberParameter("Reflectivity", "Reflectivity", "反射率. 金属系なら高い値に, プラスチック系なら中間の値に設定.", GH_ParamAccess.tree, 0.5);
            pManager.AddNumberParameter("Smoothness", "Smoothness", "平滑率. 金属系なら高い値に, プラスチック系なら中間の値に設定.", GH_ParamAccess.tree, 0.0);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("PreviewMaterial", "PreviewMaterial", "PreviewMaterial", GH_ParamAccess.tree);
            pManager.AddGenericParameter("RenderMaterial", "RenderMaterial", "RenderMaterial", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///定義
            GH_Structure<GH_Colour> diffuses_ghStructure = new GH_Structure<GH_Colour>(); //GetDataTreeメソッドはGH_Structureで使用する
            GH_Structure<GH_Colour> speculars_ghStructure = new GH_Structure<GH_Colour>(); //GetDataTreeメソッドはGH_Structureで使用する
            GH_Structure<GH_Colour> ambients_ghStructure = new GH_Structure<GH_Colour>(); //GetDataTreeメソッドはGH_Structureで使用する
            GH_Structure<GH_Colour> emissions_ghStructure = new GH_Structure<GH_Colour>(); //GetDataTreeメソッドはGH_Structureで使用する
            GH_Structure<GH_Number> transparencies_ghStructure = new GH_Structure<GH_Number>(); //GetDataTreeメソッドはGH_Structureで使用する
            GH_Structure<GH_Number> reflectivities_ghStructure = new GH_Structure<GH_Number>(); //GetDataTreeメソッドはGH_Structureで使用する
            GH_Structure<GH_Number> reflectionGlossinesses_ghStructure = new GH_Structure<GH_Number>(); //GetDataTreeメソッドはGH_Structureで使用する

            if (!DA.GetDataTree(0, out diffuses_ghStructure)) return;
            DA.GetDataTree(1, out speculars_ghStructure);
            DA.GetDataTree(2, out ambients_ghStructure);
            DA.GetDataTree(3, out emissions_ghStructure);
            if (!DA.GetDataTree(4, out transparencies_ghStructure)) return;
            if (!DA.GetDataTree(5, out reflectivities_ghStructure)) return;
            if (!DA.GetDataTree(6, out reflectionGlossinesses_ghStructure)) return;

            DataTree<Color> diffuses = ConvertToDatatree_Color(diffuses_ghStructure);
            DataTree<Color> speculars = ConvertToDatatree_Color(speculars_ghStructure);
            DataTree<Color> ambients = ConvertToDatatree_Color(ambients_ghStructure);
            DataTree<Color> emissions = ConvertToDatatree_Color(emissions_ghStructure);
            DataTree<double> transparencies = ConvertToDatatree_Number(transparencies_ghStructure);
            DataTree<double> reflectivities = ConvertToDatatree_Number(reflectivities_ghStructure);
            DataTree<double> refractionGlossinesses = ConvertToDatatree_Number(reflectionGlossinesses_ghStructure);
            ///

            ///Materialの作成
            //定義
            DataTree<Material> Materials = new DataTree<Material>(); //render用
            DataTree<GH_Material> previewMaterials = new DataTree<GH_Material>(); //preview用
            
            GH_Path path = new GH_Path();
            GH_Path path_0 = new GH_Path(0);
            //

            //Material作成
            for(int i=0; i<diffuses.BranchCount; i++)
            {
                Material material = new Material();
                //pathを定義
                path = diffuses.Paths[i];
                //

                //Materialの設定
                for(int j=0; j < diffuses.Branch(path).Count; j++)
                {                    
                    if (diffuses.DataCount == 1)
                    {
                        //名前の設定
                        material.Name = "General Material";
                        //

                        //Diffuseの設定
                        material.DiffuseColor = diffuses.Branch(path_0)[0];
                        //
                    }
                    else
                    {
                        //名前の設定
                        string name = "";
                        for (int k = 0; k < path.Indices.Length; k++)
                        {
                            if (k == 0)
                            {
                                name += path.Indices[k].ToString();
                            }
                            else
                            {
                                name += "-" + path.Indices[k].ToString();
                            }
                        }
                        material.Name = name;
                        //

                        //Diffuseの設定
                        material.DiffuseColor = diffuses.Branch(path)[j];
                        //
                    }

                    if (speculars.DataCount == 0)
                    {

                    }
                    else if (speculars.DataCount == 1)
                    {
                        material.SpecularColor = speculars.Branch(path_0)[0];
                        material.ReflectionColor = speculars.Branch(path_0)[0];
                    }
                    else
                    {
                        material.SpecularColor = speculars.Branch(path)[j];
                        material.ReflectionColor = speculars.Branch(path)[j];
                    }

                    if (ambients.DataCount == 0)
                    {

                    }
                    else if (ambients.DataCount == 1)
                    {
                        material.AmbientColor = ambients.Branch(path_0)[0];
                    }
                    else
                    {
                        material.AmbientColor = ambients.Branch(path)[j];
                    }

                    if (emissions.DataCount == 0)
                    {

                    }
                    else if (emissions.DataCount == 1)
                    {
                        material.EmissionColor = emissions.Branch(path_0)[0];
                    }
                    else
                    {
                        material.EmissionColor = emissions.Branch(path)[j];
                    }

                    if (transparencies.DataCount == 1)
                    {
                        material.Transparency = transparencies.Branch(path_0)[0];
                    }
                    else
                    {
                        material.Transparency = transparencies.Branch(path)[j];
                    }

                    if (reflectivities.DataCount == 1)
                    {
                        material.Reflectivity = reflectivities.Branch(path_0)[0];
                    }
                    else
                    {
                        material.Reflectivity = reflectivities.Branch(path)[j];
                    }

                    if (refractionGlossinesses.DataCount == 1)
                    {
                        material.ReflectionGlossiness = refractionGlossinesses.Branch(path_0)[0];
                    }
                    else
                    {
                        material.ReflectionGlossiness = refractionGlossinesses.Branch(path)[j];
                    }

                    //previewMarerialの作成
                    RenderMaterial renderMaterial = RenderMaterial.CreateBasicMaterial(material);
                    GH_Material gH_Material = new GH_Material(renderMaterial);
                    previewMaterials.Add(gH_Material, path);

                    //renderMaterial用のMaterialの設定
                    Materials.Add(material, path);
                }
                //
            }
            //

            DA.SetDataTree(0, previewMaterials);
            DA.SetDataTree(1, Materials);
        }

        private DataTree<IGH_Goo> SameValueTree(IGH_Goo value,GH_Structure<IGH_Goo> basedTree)
        {
            DataTree<IGH_Goo> tree = new DataTree<IGH_Goo>(); //変換結果
            GH_Path path = new GH_Path();
            ///

            ///treeの作成
            for (int i = 0; i < basedTree.Paths.Count; i++)
            {
                //pathを定義
                path = basedTree.Paths[i];
                //

                //DataTree作成
                for (int j = 0; j < basedTree.get_Branch(path).Count; j++)
                {
                    tree.Add(value, path);
                }
                //
            }
            return tree;
        }

        private DataTree<string> ConvertToDatatree_string(GH_Structure<GH_String> gh_structure)
        {
            DataTree<string> strs = new DataTree<string>(); //変換結果
            List<string> strs_list = new List<string>(); //一時保存用
            GH_Path path = new GH_Path();
            for (int i = 0; i < gh_structure.Paths.Count; i++)
            {
                //pathを定義
                path = gh_structure.Paths[i];
                //

                //GH_Structure<GH_String>[path]をList<string>に変換
                strs_list = gh_structure[path].ConvertAll<string>(ConvertToString);
                //

                //DataTree化
                for (int j = 0; j < strs_list.Count; j++)
                {
                    strs.Add(strs_list[j], path);
                }
                //
                ///
            }

            return strs;
        }

        private DataTree<Color> ConvertToDatatree_Color(GH_Structure<GH_Colour> gh_structure)
        {
            DataTree<Color> colors = new DataTree<Color>(); //変換結果
            List<Color> colors_list = new List<Color>(); //一時保存用
            GH_Path path = new GH_Path();
            for (int i = 0; i < gh_structure.Paths.Count; i++)
            {
                //pathを定義
                path = gh_structure.Paths[i];
                //

                //GH_Structure<GH_Colour>[path]をList<Color>に変換
                colors_list = gh_structure[path].ConvertAll<Color>(ConvertToColor);
                //

                //DataTree化
                for (int j = 0; j < colors_list.Count; j++)
                {
                    colors.Add(colors_list[j], path);
                }
                //
                ///
            }

            return colors;
        }

        private DataTree<double> ConvertToDatatree_Number(GH_Structure<GH_Number> gh_structure)
        {
            DataTree<double> nums = new DataTree<double>(); //変換結果
            List<double> nums_list = new List<double>(); //一時保存用
            GH_Path path = new GH_Path();
            for (int i = 0; i < gh_structure.Paths.Count; i++)
            {
                //pathを定義
                path = gh_structure.Paths[i];
                //

                //GH_Structure<GH_Number>[path]をList<string>に変換
                nums_list = gh_structure[path].ConvertAll<double>(ConvertToNumber);
                //

                //DataTree化
                for (int j = 0; j < nums_list.Count; j++)
                {
                    nums.Add(nums_list[j], path);
                }
                //
                ///
            }

            return nums;
        }

        private string ConvertToString(GH_String gH_String) //Convertの方法を定義（ConvertAllで使用）
        {
            string str = "";
            gH_String.CastTo<string>(ref str);

            return str;
        }

        private Color ConvertToColor(GH_Colour gH_Colour) //Convertの方法を定義（ConvertAllで使用）
        {
            Color color = new Color();
            gH_Colour.CastTo<Color>(ref color);

            return color;
        }

        private double ConvertToNumber(GH_Number gH_Number) //Convertの方法を定義（ConvertAllで使用）
        {
            double num = 0.0;
            gH_Number.CastTo<double>(ref num);

            return num;
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Shachihoko.Properties.Resources.rhinoMaterial;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("db7b672b-73a1-4e8c-b7d3-9ccd489ac48c"); }
        }
    }
}
