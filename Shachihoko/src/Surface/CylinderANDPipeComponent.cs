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

namespace Shachihoko.src.Surface
{
    public class CylinderANDPipeComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public CylinderANDPipeComponent()
          : base("Cylinder & Pipe", "Cylinder & Pipe",
              "Cylinder & Pipe",
              "Shachihoko", ShachihokoMethod.Category["Surface"])
        {
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Base", "Base", "Base Plane.", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddNumberParameter("Radius", "Radius", "Radius.", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Length", "Length", "Length.", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Thickness", "Thickness", "Thickness. If Radius = Thickness, make a cylinder model.", GH_ParamAccess.item, 0.1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Geometry", "Geometry", "Resulting Geometry.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///定義
            Plane plane = new Plane();
            double radius = 0.0;
            double length = 0.0;
            double thickness = 0.0;

            if (!DA.GetData(0, ref plane)) return;
            if (!DA.GetData(1, ref radius)) return;
            if (!DA.GetData(2, ref length)) return;
            if (!DA.GetData(3, ref thickness)) return;

            double type = 0.0; //lengthのプラスマイナスで場合分け
            if(length >= 0.0)
            {
                type = 1.0;
            }
            else
            {
                type = -1.0;
            }

            Line line = new Line(plane.Origin, plane.Normal * type, length * type);
            Curve rail = line.ToNurbsCurve();

            Brep pipe = Brep.CreateThickPipe(rail, radius, radius - thickness, true, PipeCapMode.Flat, true, 0.01, 0.01)[0];

            DA.SetData(0, pipe);
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
                return Shachihoko.Properties.Resources.CylinderANDPipe;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7F7AED29-C63B-4843-823F-6B996B9FC67B"); }
        }
    }
}
