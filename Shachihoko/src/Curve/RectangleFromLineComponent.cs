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
    public class RectangleFromLineComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public RectangleFromLineComponent()
          : base("Rectangle from Line", "Rectangle from Line",
              "Make a Rectangle from a Line.",
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
            pManager.AddLineParameter("Line", "Line", "Line.", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "Plane", "Plane.", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddNumberParameter("Width", "Width", "Rectangle Width.", GH_ParamAccess.item, 1.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Rectangle", "Rectangle", "Calculated Rectangle.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Line line = new Line();
            Plane plane = new Plane();
            double width = 0.0;

            if (!DA.GetData(0, ref line)) return;
            if (!DA.GetData(1, ref plane)) return;
            if (!DA.GetData(2, ref width)) return;

            if (plane.ZAxis == line.Direction)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Plane must not be Orthogonal.");
                return;
            }

            //移動の設定
            Plane p = new Plane(line.PointAt(0.5), line.Direction, plane.ZAxis);
            Vector3d mover = p.ZAxis;
            mover.Unitize();
            mover = mover * width / 2;

            //頂点
            Point3d pt0 = new Point3d(line.From.X, line.From.Y, line.From.Z);
            Point3d pt1 = new Point3d(line.From.X, line.From.Y, line.From.Z);
            Point3d pt2 = new Point3d(line.To.X, line.To.Y, line.To.Z);
            Point3d pt3 = new Point3d(line.To.X, line.To.Y, line.To.Z);

            pt0.Transform(Rhino.Geometry.Transform.Translation(mover));
            pt1.Transform(Rhino.Geometry.Transform.Translation(-mover));
            pt2.Transform(Rhino.Geometry.Transform.Translation(-mover));
            pt3.Transform(Rhino.Geometry.Transform.Translation(mover));

            List<Point3d> pts = new List<Point3d>();
            pts.Add(pt0);
            pts.Add(pt1);
            pts.Add(pt2);
            pts.Add(pt3);
            pts.Add(pt0);

            //ポリライン化
            Polyline rec = new Polyline(pts);
            Curve rec_crv = rec.ToNurbsCurve();

            DA.SetData(0, rec_crv);
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
                return Shachihoko.Properties.Resources.RectangleFromLine;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("FDDFDE60-A5DB-4C4B-9F11-3CF2B25C147D"); }
        }
    }
}
