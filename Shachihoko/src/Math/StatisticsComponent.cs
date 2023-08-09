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
using MathNet.Numerics.Statistics;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Shachihoko
{
    public class StatisticsComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public StatisticsComponent()
          : base("Statistics", "Stats",
              "Calculate statistics of a List.",
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
            pManager.AddNumberParameter("Input", "I", "Input values for calculating.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Mean", "M", "Mean value. (平均)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Median", "M", "Median value. (中央値)", GH_ParamAccess.item);
            pManager.AddNumberParameter("PopulationVariance", "PV", "PopulationVariance value. (分散)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Variance", "V", "Variance value. (母分散)", GH_ParamAccess.item);
            pManager.AddNumberParameter("PopulationStandardDeviation", "PSD", "PopulationStandardDeviation value. (標準偏差)", GH_ParamAccess.item);
            pManager.AddNumberParameter("StandardDeviation", "SD", "StandardDeviation value. (母標準偏差)", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<double> nums = new List<double>();

            if (!DA.GetDataList(0, nums)) return;

            double Mean = nums.Mean();
            double Median = nums.Median();
            double PopulationVariance = nums.PopulationVariance();
            double Variance = nums.Variance();
            double PopulationStandardDeviation = nums.PopulationStandardDeviation();
            double StandardDeviation = nums.StandardDeviation();

            DA.SetData(0, Mean);
            DA.SetData(1, Median);
            DA.SetData(2, PopulationVariance);
            DA.SetData(3, Variance);
            DA.SetData(4, PopulationStandardDeviation);
            DA.SetData(5, StandardDeviation);
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
                return Shachihoko.Properties.Resources.Statistics;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("5F938877-427C-4353-9F0E-B505B405201D"); }
        }
    }
}
