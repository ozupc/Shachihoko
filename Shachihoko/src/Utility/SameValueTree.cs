using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.IO;
using Rhino.DocObjects;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Shachihoko
{
    public class SameValueTree : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SameValueTree()
          : base("SameValueTree", "SameValueTree",
              "SameValueTree",
              "Shachihoko", ShachihokoMethod.Category["Utility"])
        {
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Value", "Value", "Value", GH_ParamAccess.item);
            pManager.AddGenericParameter("BasedTree", "BasedTree", "BasedTree", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("ResultTree", "ResultTree", "ResultTree", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///定義
            IGH_Goo value = null;
            GH_Structure<IGH_Goo> basedTree = new GH_Structure<IGH_Goo>();

            if (!DA.GetData(0, ref value)) return;
            if (!DA.GetDataTree(1, out basedTree)) return;

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

            DA.SetDataTree(0, tree);
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
                return Shachihoko.Properties.Resources.sameValue;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("33b15775-a29a-4934-8466-9567befd372c"); }
        }
    }
}
