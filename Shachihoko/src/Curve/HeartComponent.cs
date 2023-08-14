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
    public class HeartComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public HeartComponent()
          : base("Heart", "Heart",
              "Drow an heart carve.",
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
            pManager.AddPlaneParameter("Plane", "P", "Base plane.", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddIntegerParameter("Smoothness", "Sm", "The number of points for Polyline. It's recommended to be larger than 30.", GH_ParamAccess.item, 30);
            pManager.AddNumberParameter("Size", "S", "If Switch is true, Width is Size. If it's false, Height is Size.", GH_ParamAccess.item, 1.0);
            pManager.AddBooleanParameter("Switch", "Sw", "If Switch is true, Width is Size. If it's false, Height is Size.", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("HeartCurve", "HC", "Would you love me?", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Plane pl = new Plane(Plane.WorldXY);
            int numPt = 30;
            double size = 1.0;
            bool sw = true;

            if (!DA.GetData(0, ref pl)) return;
            if (!DA.GetData(1, ref numPt)) return;
            if (!DA.GetData(2, ref size)) return;
            if (!DA.GetData(3, ref sw)) return;

            List<Point3d> pts = new List<Point3d>();
            List<double> xs = new List<double>();
            List<double> ys = new List<double>();

            double t = -1.0 * Math.PI;
            double add = 2.0 / numPt * Math.PI;

            IEnumerable<double> xss;
            IEnumerable<double> yss;
            Plane flip = new Plane();
            flip.Flip();

            for (int i = 0; i < numPt + 1; i++)
            {
                double x = 16 * Math.Pow(Math.Sin(t), 3.0);
                double y = 13 * Math.Cos(t) - 5 * Math.Cos(2 * t) - 2 * Math.Cos(3 * t) - Math.Cos(4 * t);
                xs.Add(x);
                ys.Add(y);
                t += add;
            }
            if (sw)
            {
                double basesize = xs.Max();
                xss = xs.Select(h => h / basesize * size);
                yss = ys.Select(h => h / basesize * size);
            }
            else
            {
                double basesize = ys.Max();
                xss = xs.Select(h => h / basesize * size);
                yss = ys.Select(h => h / basesize * size);
            }
            for (int i = 0; i < xs.Count; i++)
            {
                Point3d pt = new Point3d(xss.ElementAt(i), yss.ElementAt(i), 0);
                pts.Add(pt);
            }

            PolylineCurve poly = new PolylineCurve(pts);
            poly.MakeClosed(0.0);

            poly.Transform(Rhino.Geometry.Transform.ChangeBasis(Plane.WorldXY, pl));

            DA.SetData(0, poly);
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
                return Shachihoko.Properties.Resources.Heart;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7F75B654-2FD1-4EC8-BF8C-75BB7476CDA4"); }
        }
    }
}
