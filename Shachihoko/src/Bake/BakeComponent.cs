using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Linq;
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
    public class BakeComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public BakeComponent()
          : base("Bake", "Bake",
              "Bake",
              "Shachihoko", ShachihokoMethod.Category["Bake"])
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "Geometry", "Geometry", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Material", "Material", "Material", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Bake", "Bake", "Bake", GH_ParamAccess.item, false);
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
            bool bake = false;

            if (!DA.GetDataTree(0, out geometrys_IGH_GeometricGoo)) return;
            if (!DA.GetDataTree(1, out materials_IGH_Goo)) return;
            if (!DA.GetData(2, ref bake)) return;            
            //

            if (bake)
            {
                //定義
                List<GeometryBase> geometries_list = new List<GeometryBase>(); //一時保存用
                List<Material> materials_list = new List<Material>(); //一時保存用
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
                    materials_list = materials_IGH_Goo[path].ConvertAll<Material>(ConvertToMaterial);

                    ///Objectを一時的にBakeする
                    //Attributeの定義
                    ObjectAttributes attribute = Rhino.RhinoDoc.ActiveDoc.CreateDefaultAttributes();
                    attribute.MaterialSource = Rhino.DocObjects.ObjectMaterialSource.MaterialFromObject;
                    //

                    for (int j = 0; j < geometrys_IGH_GeometricGoo[path].Count; j++)
                    {
                        //Attribute定義
                        int index = RhinoDoc.ActiveDoc.Materials.Add(materials_list[j]);
                        attribute.MaterialIndex = index;
                        //

                        //Bake
                        Guid guid = Rhino.RhinoDoc.ActiveDoc.Objects.Add(geometries_list[j],attribute);
                        guids.Add(guid);
                        //
                    }
                    ///
                }
            }
        }

        private GeometryBase ConvertToGeometry(IGH_GeometricGoo igh_GeometricGoo) //Convertの方法を定義（ConvertAllで使用）
        {
            GeometryBase geometry = null;
            igh_GeometricGoo.CastTo<GeometryBase>(out geometry);

            return geometry;
        }

        private Material ConvertToMaterial(IGH_Goo igH_Goo) //Convertの方法を定義（ConvertAllで使用）
        {
            Material material = null;
            igH_Goo.CastTo<Material>(out material);

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
                return Shachihoko.Properties.Resources.bake;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("5b0e287e-0bd2-468a-8167-9a5a409aac40"); }
        }
    }
}
