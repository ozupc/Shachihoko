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
    public class SinCosTanWaveComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SinCosTanWaveComponent()
          : base("Sin Cos Tan Wave", "SCTW",
              "Compute the Sine/Cosine/Tangent from parameter.",
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
            pManager.AddIntegerParameter("Type", "T", "0 = Sine, 1 = Cosine, 2 = Tangent.", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Wave Start", "S", "Start value of Wave. This's automatically multiplied \"π\".", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Wave End", "E", "End value of Wave. This's automatically multiplied \"π\".", GH_ParamAccess.item, 2.0);
            pManager.AddNumberParameter("Remap Min", "Min", "Min value of result.", GH_ParamAccess.item, -1.0);
            pManager.AddNumberParameter("Remap Max", "Max", "Max value of result.", GH_ParamAccess.item, 1.0);
            pManager.AddIntegerParameter("Count", "C", "Number of Result.", GH_ParamAccess.item, 10);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Result", "R", "Output value.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int tp = 0;
            double ws = 0.0;
            double we = 2.0;
            double min = -1.0;
            double max = 1.0;
            int count = 10;

            List<double> result = new List<double>();

            if (!DA.GetData(0, ref tp)) return;
            if (!DA.GetData(1, ref ws)) return;
            if (!DA.GetData(2, ref we)) return;
            if (!DA.GetData(3, ref min)) return;
            if (!DA.GetData(4, ref max)) return;
            if (!DA.GetData(5, ref count)) return;

            if (tp != 0 && tp != 1 && tp != 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Value of type must be 0 or 1 or 2.");
            }
            if (count <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Value of count must be larger than 0.");
            }

            for (int i = 0; i < count; i++)
            {
                double angle = (ws + ((we - ws) / (count - 1)) * i) * Math.PI;
                double v = 0;
                if (tp == 0)
                {
                    v = Math.Sin(angle);
                }
                else if (tp == 1)
                {
                    v = Math.Cos(angle);
                }
                else if (tp == 2)
                {
                    v = Math.Tan(angle);
                }
                v = Remap(v, -1.0, 1.0, min, max);
                result.Add(v);
            }
            DA.SetDataList(0, result);
        }

        public double Remap(double v, double from1, double to1, double from2, double to2)
        {
            return (v - from1) / (to1 - from1) * (to2 - from2) + from2;
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
                return Shachihoko.Properties.Resources.SinCosTan;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("E83D2132-3C79-47D8-9CB9-36FC49FB5F22"); }
        }
    }
}
