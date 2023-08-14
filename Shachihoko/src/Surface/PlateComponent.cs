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
    public class PlateComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public PlateComponent()
          : base("Plate", "Plate",
              "Make plates for CNC.",
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
            pManager.AddGeometryParameter("Shape", "S", "Bounding shape.\r\nBrep or Mesh to plates.", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "P", "Base plane for plates.", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddIntegerParameter("Count", "C", "Number of plates.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness", "T", "Thickness of plates.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Plates", "P", "Resulting plates.", GH_ParamAccess.list);
            pManager.AddCurveParameter("Contours", "C", "Resulting contours.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IGH_GeometricGoo shape = null;
            Plane plane = new Plane();
            int num = new int();
            double thick = new double();

            if (!DA.GetData(0, ref shape)) return;
            if (!DA.GetData(1, ref plane)) return;
            if (!DA.GetData(2, ref num)) return;
            if (!DA.GetData(3, ref thick)) return;

            //BBoxの取得
            //BBoxの高さ方向を取得
            List<Plane> planes = new List<Plane>();
            List<Extrusion> exs = new List<Extrusion>();
            double dist = new double();
            double min = new double();
            double max = new double();

            GetMinMax(shape, plane, ref min, ref max);

            dist = (max - min) / num;

            double min_copy = min;
            for (int i = 0; i < num; i++)
            {
                min_copy += dist;
                Rhino.Geometry.Transform move = Rhino.Geometry.Transform.Translation(plane.Normal * min_copy);
                Plane p = new Plane(plane);
                p.Transform(move);
                planes.Add(p);
            }

            List<Curve> contours = new List<Curve>();
            if (shape is GH_Brep || shape is GH_Surface || shape is GH_Box)
            {                
                foreach (Plane pl in planes)
                {
                    Brep brep = new Brep();
                    if (shape is GH_Box)
                    {
                        shape.CastTo<Box>(out Box box);
                        brep = box.ToBrep();
                    }
                    else
                    {
                        shape.CastTo<Brep>(out brep);
                    }                    
                    Curve[] c = Brep.CreateContourCurves(brep, pl);
                    for (int i = 0; i < c.Length; i++)
                    {
                        contours.Add(c[i]);
                    }
                }
            }
            else if (shape is GH_Mesh)
            {
                foreach (Plane pl in planes)
                {
                    Mesh mesh = new Mesh();
                    shape.CastTo<Mesh>(out mesh);
                    Curve[] c = Mesh.CreateContourCurves(mesh, pl);
                    for (int i = 0; i < c.Length; i++)
                    {
                        contours.Add(c[i]);
                    }
                }
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Shape should be Brep, Surface or Mesh.");
            }

            foreach (Curve c in contours)
            {
                Rhino.Geometry.Transform t = Rhino.Geometry.Transform.Translation(-plane.Normal * thick / 2);
                c.Transform(t);
                Extrusion ex = new Extrusion();
                ex = Extrusion.Create(c, thick, true);
                exs.Add(ex);
            }

            DA.SetDataList(0, exs);
            DA.SetDataList(1, contours);

            //forでdistを回してコンターの高さを求める
            //thick分小さい数でmove
        }

        private void GetMinMax(IGH_GeometricGoo shape, Plane plane, ref double min, ref double max)
        {
            Plane flip = new Plane(plane);
            flip.Flip();
            BoundingBox bbox = shape.Boundingbox;
            if (plane == Plane.WorldXY || flip == Plane.WorldXY)
            {
                min = bbox.Min.Z;
                max = bbox.Max.Z;
            }
            else if (plane == Plane.WorldYZ || flip == Plane.WorldYZ)
            {
                min = bbox.Min.X;
                max = bbox.Max.X;
            }
            else if (plane == Plane.WorldZX || flip == Plane.WorldZX)
            {
                min = bbox.Min.Y;
                max = bbox.Max.Y;
            }
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
                return Shachihoko.Properties.Resources.Plate;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("283669F4-4442-496C-BAC5-C391ED3E263B"); }
        }
    }
}
