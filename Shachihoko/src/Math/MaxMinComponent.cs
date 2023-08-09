using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.IO;
using Rhino.DocObjects;
using System.Linq;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Shachihoko
{
    public class MaxMinComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public MaxMinComponent()
          : base("MaxMin", "MM",
              "Return the greater/lesser of list.",
              "Shachihoko", ShachihokoMethod.Category["Math"])
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
            pManager.AddNumberParameter("Number", "N", "Give me Numbers!", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Max/Min", "MM", "True:Max, False:Min", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Result", "R", "Max/Min number.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Index", "Id", "Max/Min Index.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<double> numbers = new List<double>();
            bool tf = true;
            double num;
            List<int> index = new List<int>();

            if (!DA.GetDataList<double>(0, numbers)) return;
            if (!DA.GetData(1, ref tf)) return;

            int length = numbers.Count();

            if (tf)
            {
                num = numbers.Max();
                for (int i = 0; i < length; i++)
                {
                    if (num == numbers[i])
                    {
                        index.Add(i);
                    }
                }
            }
            else
            {
                num = numbers.Min();
                for (int i = 0; i < length; i++)
                {
                    if (num == numbers[i])
                    {
                        index.Add(i);
                    }
                }
            }

            DA.SetData(0, num);
            DA.SetDataList(1, index);
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
                return Shachihoko.Properties.Resources.MaxMin;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("B2CBB7E2-E8BC-493C-94DA-59866D5C78E1"); }
        }
    }
}
