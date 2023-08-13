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
    public class IntegerRandomComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public IntegerRandomComponent()
          : base("IntegerRandom", "IntRamdom",
              "Generate a list of random numbers using Fisher-Yate Algolithm.",
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
            Interval interval = new Interval(0, 100);

            pManager.AddIntervalParameter("Range", "R", "Domain of random numeric range.", GH_ParamAccess.item,interval);
            pManager.AddIntegerParameter("Number", "N", "Number of random values.", GH_ParamAccess.item, 10);
            pManager.AddIntegerParameter("Seed", "S", "Seed of random engine.", GH_ParamAccess.item, 0);
            pManager.AddBooleanParameter("Same Value", "Same Value", "If true, the same numbers are allowed; if false, they are not allowed.", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Random", "R", "Random numbers.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Interval domain = new Interval();
            int num = 0;
            int seed = 0;
            bool sameValue = true;

            if (!DA.GetData(0, ref domain)) return;
            if (!DA.GetData(1, ref num)) return;
            if (!DA.GetData(2, ref seed)) return;
            if (!DA.GetData(3, ref sameValue)) return;

            if (domain.Length < num && !sameValue)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Number must be smaller than Size of Range when sameValue is false.");
                return;
            }

            List<int> nums = new List<int>();
            HashSet<int> usedNumbers = new HashSet<int>();
            Random rand = new Random(seed);

            for (int i = (int)domain.Min; i < (int)domain.Max; i++)
            {
                nums.Add(i);
            }

            for (int Pos = 0; Pos < num; Pos++)
            {
                int nextPos;
                do
                {
                    nextPos = rand.Next(Pos, nums.Count);
                } while (!sameValue && usedNumbers.Contains(nums[nextPos]));

                usedNumbers.Add(nums[nextPos]);
                int temp = nums[Pos];
                nums[Pos] = nums[nextPos];
                nums[nextPos] = temp;
            }

            DA.SetDataList(0, nums.GetRange(0, num));
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
                return Shachihoko.Properties.Resources.IntegerRandom;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("077A76D2-AE09-45F9-8016-D64A0C8E714A"); }
        }
    }
}
