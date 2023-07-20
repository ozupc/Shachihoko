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
    public class FrameComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public FrameComponent()
          : base("Frame", "Frame",
              "Frame",
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
            pManager.AddNumberParameter("CrossSectionX", "CrossSectionX", "材の幅方向のサイズ（mm）", GH_ParamAccess.item, 50.0);
            pManager.AddNumberParameter("CrossSectionY", "CrossSectionY", "材の高さ方向のサイズ（mm）", GH_ParamAccess.item, 25.0);
            pManager.AddNumberParameter("SizeX", "SizeX", "モデルのX方向の外寸法（mm）", GH_ParamAccess.item, 1000.0);
            pManager.AddNumberParameter("SizeY", "SizeY", "モデルのY方向の外寸法（mm）", GH_ParamAccess.item, 750.0);
            pManager.AddNumberParameter("SizeZ", "SizeZ", "モデルのZ方向の外寸法（mm）", GH_ParamAccess.item, 500.0);
            pManager.AddIntegerParameter("NumX", "NumX", "モデルのX方向に並ぶ材の数（本）", GH_ParamAccess.item, 3);
            pManager.AddIntegerParameter("NumY", "NumY", "モデルのY方向に並ぶ材の数（本）", GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("NumZ", "NumZ", "モデルのZ方向に並ぶ材の数（本）", GH_ParamAccess.item, 2);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Frame", "Frame", "Frame", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Frame", "Frame", "Frame", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///定義
            double crossSectionX = 0.0;
            double crossSectionY = 0.0;
            double sizeX = 0.0;
            double sizeY = 0.0;
            double sizeZ = 0.0;
            int numX = 0;
            int numY = 0;
            int numZ = 0;
            Brep frame = null;

            if (!DA.GetData(0, ref crossSectionX)) return;
            if (!DA.GetData(1, ref crossSectionY)) return;
            if (!DA.GetData(2, ref sizeX)) return;
            if (!DA.GetData(3, ref sizeY)) return;
            if (!DA.GetData(4, ref sizeZ)) return;
            if (!DA.GetData(5, ref numX)) return;
            if (!DA.GetData(6, ref numY)) return;
            if (!DA.GetData(7, ref numZ)) return;

            //仮
            List<Line> lines = new List<Line>();
            List<Curve> recs = new List<Curve>();

            //材を作成
            List<Brep> beams = new List<Brep>();
            double spanX = sizeX / (numX - 1);
            double spanY = sizeY / (numY - 1);
            double spanZ = sizeZ / (numZ - 1);
            for (int i = 0; i < numX; i++)
            {
                for(int j = 0; j < numZ; j++)
                {
                    Line line = new Line(-sizeX / 2.0 + spanX * i, -sizeY / 2.0, -sizeZ / 2.0 + spanZ * j, -sizeX / 2.0 + spanX * i, sizeY / 2.0, -sizeZ / 2.0 + spanZ * j);
                    Curve rail = line.ToNurbsCurve();
                    Plane plane = new Plane(line.From, Vector3d.YAxis);
                    Rectangle3d rec = new Rectangle3d(plane, crossSectionX, crossSectionY);
                    Curve crossSection = rec.ToNurbsCurve();

                    lines.Add(line);
                    recs.Add(crossSection);

                    //beams.Add(Brep.CreateFromSweep(rail, crossSection, true, 0.0)[0]);
                }
            }
            for (int i = 0; i < numY; i++)
            {
                for (int j = 0; j < numX; j++)
                {
                    Line line = new Line(-sizeX / 2.0 + spanX * j, -sizeY / 2.0 + spanY * i, -sizeZ / 2.0, -sizeX / 2.0 + spanX * j, -sizeY / 2.0 + spanY * i, sizeZ / 2.0);
                    Curve rail = line.ToNurbsCurve();
                    Plane plane = new Plane(line.From, Vector3d.ZAxis);
                    Rectangle3d rec = new Rectangle3d(plane, crossSectionX, crossSectionY);
                    Curve crossSection = rec.ToNurbsCurve();

                    lines.Add(line);
                    recs.Add(crossSection);

                    //beams.Add(Brep.CreateFromSweep(rail, crossSection, true, 0.0)[0]);
                }
            }
            for (int i = 0; i < numZ; i++)
            {
                for (int j = 0; j < numY; j++)
                {
                    Line line = new Line(-sizeX / 2.0, -sizeY / 2.0 + spanY * j, -sizeZ / 2.0 + spanZ * i, sizeX / 2.0, -sizeY / 2.0 + spanY * j, -sizeZ / 2.0 + spanZ * i);
                    Curve rail = line.ToNurbsCurve();
                    Plane plane = new Plane(line.From, Vector3d.XAxis);
                    Rectangle3d rec = new Rectangle3d(plane, crossSectionX, crossSectionY);
                    Curve crossSection = rec.ToNurbsCurve();

                    lines.Add(line);
                    recs.Add(crossSection);

                    //beams.Add(Brep.CreateFromSweep(rail, crossSection, true, 0.0)[0]);
                }
            }

            //frame = Brep.CreateBooleanUnion(beams, 0.0)[0];

            DA.SetDataList(0, lines);
            DA.SetDataList(1, recs);

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
            get { return new Guid("33b15775-a29a-4934-8466-9567befd372c"); }
        }
    }
}
