/*using Grasshopper.Kernel;
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

namespace Export
{
    public class ExportComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ExportComponent()
          : base("Export", "Export",
              "Export",
              "Shachihoko", "Bake")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "Geometry", "Geometry", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Material", "Material", "Material", GH_ParamAccess.tree);
            pManager.AddTextParameter("FileName", "FileName", "FileName", GH_ParamAccess.item);
            pManager.AddTextParameter("Directry", "Directry", "Directry", GH_ParamAccess.item);
            pManager.AddTextParameter("Command", "Command", "Command", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Export", "Export", "Export", GH_ParamAccess.item, false);

            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //定義
            GH_Structure<IGH_GeometricGoo> geometrys_IGH_GeometricGoo = new GH_Structure<IGH_GeometricGoo>(); //GetDataTreeメソッドはGH_Structureで使用する
            GH_Structure<IGH_Goo> materials_IGH_Goo = new GH_Structure<IGH_Goo>(); //GetDataTreeメソッドはGH_Structureで使用する
            string fileName = "";
            string directry = "";
            List<string> commands = new List<string>();
            bool export = false;

            if (!DA.GetDataTree(0, out geometrys_IGH_GeometricGoo)) return;
            if (!DA.GetDataTree(1, out materials_IGH_Goo)) return;
            if (!DA.GetData(2, ref fileName)) return;
            if (!DA.GetData(3, ref directry)) return;
            DA.GetDataList(4, commands);
            if (!DA.GetData(5, ref export)) return;            
            //

            if (export)
            {
                //定義
                List<GeometryBase> geometries_list = new List<GeometryBase>(); //一時保存用
                List<RenderMaterial> materials_list = new List<RenderMaterial>(); //一時保存用
                GH_Path path = new GH_Path();

                List<Guid> guids = new List<Guid>(); //Bake後の削除に使う
                //

                for (int i = 0; i < geometrys_IGH_GeometricGoo.Paths.Count; i++)
                {
                    //pathを定義
                    path = geometrys_IGH_GeometricGoo.Paths[i];
                    //

                    //GH_Structure<IGH_GeometricGoo>[path]をList<GeometryBase>に変換
                    geometries_list = geometrys_IGH_GeometricGoo[path].ConvertAll<GeometryBase>(ConvertToGeometry);

                    //GH_Structure<IGH_Goo>[path]をList<Material>に変換
                    materials_list = materials_IGH_Goo[path].ConvertAll<RenderMaterial>(ConvertToMaterial);
                    //

                    ///Materialの登録
                    //for
                    ///

                    ///Objectを一時的にBakeする
                    //Attributeの定義
                    ObjectAttributes attribute = Rhino.RhinoDoc.ActiveDoc.CreateDefaultAttributes();
                    attribute.MaterialSource = Rhino.DocObjects.ObjectMaterialSource.MaterialFromObject;
                    //

                    for (int j = 0; j < geometrys_IGH_GeometricGoo[path].Count; j++)
                    {
                        //Attribute定義
                        Rhino.RhinoDoc.ActiveDoc.RenderMaterials.Add(materials_list[j]);
                        attribute.MaterialIndex = Rhino.RhinoDoc.ActiveDoc.Materials.Find(materials_list[j].Name, false);
                        //

                        //Bake
                        Guid guid = Rhino.RhinoDoc.ActiveDoc.Objects.Add(geometries_list[j],attribute);
                        guids.Add(guid);
                        //
                    }
                    ///
                }

                ///RhinoでのExport動作
                Rhino.RhinoDoc.ActiveDoc.Objects.Select(guids);

                //commandの作成
                string filePath = Path.Combine(directry, fileName); //ファイルパス
                string cmd = "_-Export " + filePath + " _Enter";
                foreach(string str in commands)
                {
                    cmd += str;
                }
                //

                Rhino.RhinoApp.RunScript(cmd, false);

                Rhino.RhinoDoc.ActiveDoc.Objects.Delete(guids, true);
                ///
            }
        }

        private GeometryBase ConvertToGeometry(IGH_GeometricGoo igh_GeometricGoo) //Convertの方法を定義（ConvertAllで使用）
        {
            GeometryBase geometry = null;
            igh_GeometricGoo.CastTo<GeometryBase>(out geometry);

            return geometry;
        }

        private RenderMaterial ConvertToMaterial(IGH_Goo igH_Goo) //Convertの方法を定義（ConvertAllで使用）
        {
            GH_Material gH_Material = new GH_Material();
            RenderMaterial material = null;
            igH_Goo.CastTo<GH_Material>(out gH_Material);
            gH_Material.CastTo<RenderMaterial>(ref material);

            return material;
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
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7aa8fae1-2bb4-40b5-b27a-1cd380d2d3b4"); }
        }
    }
}*/
