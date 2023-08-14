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
    public class LineCDLComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public LineCDLComponent()
          : base("Line CDL", "Line CDL",
              "Create a line segment defined by center point, tangent and length.",
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
            pManager.AddPointParameter("Center", "C", "Line center point.", GH_ParamAccess.item, Plane.WorldXY.Origin);
            pManager.AddVectorParameter("Direction", "D", "Line tangent. (direction)", GH_ParamAccess.item, Plane.WorldXY.XAxis);
            pManager.AddNumberParameter("Length", "L", "Line length.", GH_ParamAccess.item, 1.0);

            pManager.HideParameter(0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Line", "L", "Line segment.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d centerPt = new Point3d();
            Vector3d direction = new Vector3d();
            double length = 0.0;

            if (!DA.GetData(0, ref centerPt)) return;
            if (!DA.GetData(1, ref direction)) return;
            if (!DA.GetData(2, ref length)) return;

            //描写
            Point3d startPt = new Point3d(centerPt);
            Point3d endPt = new Point3d(centerPt);
            direction.Unitize();
            direction = direction * length / 2;
            startPt.Transform(Rhino.Geometry.Transform.Translation(-direction));
            endPt.Transform(Rhino.Geometry.Transform.Translation(direction));

            Line line = new Line(startPt, endPt);

            DA.SetData(0, line);
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
                return Shachihoko.Properties.Resources.LineCDL;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("5534ABB0-C115-49AF-8468-106DD28D7E53"); }
        }
    }
}
