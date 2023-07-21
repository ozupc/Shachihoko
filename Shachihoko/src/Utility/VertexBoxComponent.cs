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
    public class VertexBoxComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public VertexBoxComponent()
          : base("Vertex Box", "Vertex Box",
              "Vertex Box",
              "Shachihoko", ShachihokoMethod.Category["Utility"])
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
            pManager.AddPlaneParameter("Base", "Base", "Base Plane.", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddNumberParameter("X", "X", "Size of box in {X} direction.", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Y", "Y", "Size of box in {Y} direction.", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Z", "Z", "Size of box in {Z} direction.", GH_ParamAccess.item, 1.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBoxParameter("Box", "Box", "Resulting Box.", GH_ParamAccess.item);
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
            double sizeX = 0.0;
            double sizeY = 0.0;
            double sizeZ = 0.0;

            if (!DA.GetData(0, ref plane)) return;
            if (!DA.GetData(1, ref sizeX)) return;
            if (!DA.GetData(2, ref sizeY)) return;
            if (!DA.GetData(3, ref sizeZ)) return;

            Interval intervalX = new Interval(0, sizeX);
            Interval intervalY = new Interval(0, sizeY);
            Interval intervalZ = new Interval(0, sizeZ);

            Box box = new Box(plane, intervalX, intervalY, intervalZ);

            DA.SetData(0, box);

        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        /*protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Shachihoko.Properties.Resources.sameValue;
            }
        }*/

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2eec2a22-821c-4ebf-9399-4ab512124b70"); }
        }
    }
}
