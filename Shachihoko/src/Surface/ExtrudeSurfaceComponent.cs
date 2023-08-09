using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.IO;
using Rhino.DocObjects;
using Rhino;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Shachihoko
{
    public class ExtrudeSurfaceComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ExtrudeSurfaceComponent()
          : base("Extrude Surface", "ExSurf",
              "Extrude surface.",
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
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Surface", "S", "Surface to offset.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Distance", "D", "Distance to offset.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("BothSides", "B", "Offset to both sides.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Caps", "C", "Make caps.", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "Resulting offset solid.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep baseBrep = new Brep();
            double dist = 0;
            bool both = false;
            bool cap = true;
            Brep brep = new Brep();

            if (!DA.GetData(0, ref baseBrep)) return;
            if (!DA.GetData(1, ref dist)) return;
            if (!DA.GetData(2, ref both)) return;
            if (!DA.GetData(3, ref cap)) return;

            double tol = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

            if (both)
            {
                double extrude = Math.Abs(dist) / 2;
                brep = Brep.CreateFromOffsetFace(baseBrep.Faces[0], extrude, tol, both, cap);
            }
            else
            {
                double extrude = dist;
                brep = Brep.CreateFromOffsetFace(baseBrep.Faces[0], extrude, tol, both, cap);
            }

            DA.SetData(0, brep);

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
                return Shachihoko.Properties.Resources.ExtrudeSurface;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("A0A47133-D3D2-4ED2-B3BC-21A739699476"); }
        }
    }
}
