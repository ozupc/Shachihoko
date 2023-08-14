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
using GH_IO;
using GH_IO.Serialization;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Shachihoko
{
    public class TransformVectorFromPlaneComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public TransformVectorFromPlaneComponent()
        : base("Transform Vector from Plane", "Transform Vector from Plane",
              "Vector followed Plane.",
              "Shachihoko", ShachihokoMethod.Category["Vector"])
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
            pManager.AddPlaneParameter("Base Plane", "Base Plane", "Base Plane.", GH_ParamAccess.item, Rhino.Geometry.Plane.WorldXY);
            pManager.AddPlaneParameter("Target Plane", "Target Plane", "Target Plane.", GH_ParamAccess.item);
            pManager.AddVectorParameter("Vector", "V", "Vector.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Vector", "V", "Result Vector.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Rhino.Geometry.Plane beforePlane = new Rhino.Geometry.Plane();
            Rhino.Geometry.Plane afterPlane = new Rhino.Geometry.Plane();
            Vector3d baseVec = new Vector3d();

            if (!DA.GetData(0, ref beforePlane)) return;
            if (!DA.GetData(1, ref afterPlane)) return;
            if (!DA.GetData(2, ref baseVec)) return;

            baseVec.Transform(Rhino.Geometry.Transform.PlaneToPlane(beforePlane, afterPlane));

            DA.SetData(0, baseVec);
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
                return Shachihoko.Properties.Resources.TransformVectorFromPlane;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("32DA3FB9-51F6-4DEE-BC99-D00474FA0CB6"); }
        }
    }
}
