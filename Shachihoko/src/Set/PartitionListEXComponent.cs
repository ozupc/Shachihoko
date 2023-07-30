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
    public class PartitionListEXComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public PartitionListEXComponent()
          : base("Partition List EX", "Partition EX",
              "Partition a list into sub-Ex-lists",
              "Shachihoko", ShachihokoMethod.Category["Set"])
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
            pManager.AddGenericParameter("List", "L", "List to partition", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Size", "S", "Size of partitions", GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("CrossSize", "C", "Size of crossing", GH_ParamAccess.item, 0);
            pManager.AddBooleanParameter("Wrap", "W", "Wrap values", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Chunks", "C", "List chunks", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<object> list = new List<object>();
            int size = 2;
            int cross = 0;
            bool wrap = false;
            int check_listwrap = list.Count % (size - cross);

            if (!DA.GetDataList(0, list)) return;
            if (!DA.GetData(1, ref size)) return;
            if (!DA.GetData(2, ref cross)) return;
            if (!DA.GetData(3, ref wrap)) return;

            if (size <= cross)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Size must be bigger than CrossSize");
                return;
            }
            if (wrap == false && list.Count % (size - cross) != 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Recheck Size and CrossSize");
                return;
            }

            DataTree<object> tree = ListToTree(list, size, cross, wrap);
            DA.SetDataTree(0, tree);
        }

        private DataTree<object> ListToTree(List<object> list, int size, int cross, bool wrap)
        {
            DataTree<object> tree = new DataTree<object>();
            for (int i = 0; i <= list.Count / (size - cross); i++)
            {
                if ((size - cross) * i < list.Count)
                {
                    GH_Path path = new GH_Path(i);
                    for (int j = 0; j < size; j++)
                    {
                        if ((size - cross) * i + j < list.Count)
                        {
                            object item = list[(size - cross) * i + j];
                            tree.Add(item, path);
                        }
                        else if ((size - cross) * i + j >= list.Count && wrap)
                        {
                            object item = list[(size - cross) * i + j - list.Count];
                            tree.Add(item, path);
                        }
                    }
                }

            }
            return tree;
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
                return Shachihoko.Properties.Resources.PartitionList;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("BD8144DC-8E3B-4CC5-B001-35D651FF5024"); }
        }
    }
}
