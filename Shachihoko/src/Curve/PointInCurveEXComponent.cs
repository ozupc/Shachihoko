using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.IO;
using Rhino.DocObjects;
using Grasshopper.Kernel.Geometry;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using System.Security.Cryptography;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Shachihoko
{
    public class PointInCurveEXComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public PointInCurveEXComponent()
        : base("Point in Curve EX", "Point in Curve EX",
              "Test a point for closed curve containment.",
              "Shachihoko", ShachihokoMethod.Category["Curve"])
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
            pManager.AddCurveParameter("Curve", "Crv", "Boundary region. (A closed curve only.)", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "P", "Plane in in which to compare point and region.", GH_ParamAccess.item, Rhino.Geometry.Plane.WorldXY);
            pManager.AddPointParameter("Point", "Pt", "Point for region inclusion test.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Relationship", "R", "Point/Region rerationship. (0 = outside, 1 = coincident, 2 = inside.)", GH_ParamAccess.list);
            pManager.AddPointParameter("Outside point", "Out", "Outside point.", GH_ParamAccess.list);
            pManager.AddPointParameter("Coinsident point", "Out", "Coincident point.", GH_ParamAccess.list);
            pManager.AddPointParameter("Inside point", "In", "Inside point.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> basept = new List<Point3d>();
            Curve crv = null;
            Rhino.Geometry.Plane plane = new Rhino.Geometry.Plane();

            if (!DA.GetData(0, ref crv)) return;
            if (!DA.GetData(1, ref plane)) return;
            if (!DA.GetDataList(2, basept)) return;

            List<int> r = new List<int>();
            List<Point3d> outside = new List<Point3d>();
            List<Point3d> coinsident = new List<Point3d>();
            List<Point3d> inside = new List<Point3d>();

            foreach (Point3d pt in basept)
            {
                PointContainment pc = crv.Contains(pt, plane, 0.001);
                if (pc == PointContainment.Coincident)
                {
                    r.Add(1);
                    coinsident.Add(pt);
                }
                else if (pc == PointContainment.Inside)
                {
                    r.Add(2);
                    inside.Add(pt);
                }
                else if (pc == PointContainment.Outside)
                {
                    r.Add(0);
                    outside.Add(pt);
                }
                else
                {
                    r.Add(-1);
                }
            }

            DA.SetDataList(0, r);
            DA.SetDataList(1, outside);
            DA.SetDataList(2, coinsident);
            DA.SetDataList(3, inside);
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
                return Shachihoko.Properties.Resources.PointInCurve;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("FC0ADD49-3697-4772-B132-9CD93AEDE339"); }
        }
    }
}
