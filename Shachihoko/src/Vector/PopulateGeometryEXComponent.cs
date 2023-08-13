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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Shachihoko
{
    public class PopulateGeometryEXComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public PopulateGeometryEXComponent()
        : base("Populate Geometry EX", "Populate Geometry EX",
              "Populate generic geometry with points.",
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
            pManager.AddGeometryParameter("Geometry", "G", "Geometry to populate. (Closed brep or mesh only.)", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Count", "N", "Number of points to add.", GH_ParamAccess.item, 10);
            pManager.AddIntegerParameter("Seed", "S", "Random seed for insertion.", GH_ParamAccess.item, 0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Population", "P", "Population of inserted points.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> pts = new List<Point3d>();
            int count = 10;
            int seed = 0;
            IGH_GeometricGoo geo = null;

            Random rand = new Random(seed);

            if (!DA.GetData(0, ref geo)) return;
            if (!DA.GetData(1, ref count)) return;
            if (!DA.GetData(2, ref seed)) return;

            if (geo is GH_Mesh)
            {
                Mesh m = new Mesh();
                geo.CastTo<Mesh>(out m);

                if (!(m.IsClosed))
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Geometry shoud be Closed Brep or Mesh.");
                }

                pts = RandomPointsInMesh(m, count, seed);
            }
            else if (geo is GH_Brep)
            {
                Brep b = new Brep();
                geo.CastTo<Brep>(out b);

                if (!(b.IsSolid))
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Geometry shoud be Closed Brep or Mesh.");
                }

                pts = RandomPointsInBrep(b, count, seed);
            }
            else if (geo is GH_Box)
            {
                //Brep b = new Brep();
                geo.CastTo<Box>(out Box box);
                Brep b = box.ToBrep();

                if (!(b.IsSolid))
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Geometry shoud be Closed Brep or Mesh.");
                }

                pts = RandomPointsInBrep(b, count, seed);
            }
            else if (geo is GH_Surface)
            {
                geo.CastTo<Surface>(out Surface surface);

                if (surface.IsClosed(0) || surface.IsClosed(1))
                {
                    Brep b = Brep.CreateFromSurface(surface);
                    geo.CastTo<Brep>(out b);

                    if (!(b.IsSolid))
                    {
                        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Geometry shoud be Closed Brep or Mesh.");
                    }

                    pts = RandomPointsInBrep(b, count, seed);
                }
                else
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Geometry should be a closed surface or mesh.");
                    return;
                }
            }
            else
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Geometry shoud be Closed Brep or Mesh.");
            }

            DA.SetDataList(0, pts);
        }

        private List<Point3d> RandomPointsInBrep(Brep brep, int count, int seed)
        {
            Random rand = new Random(seed);
            BoundingBox boundingBox = brep.GetBoundingBox(false);
            List<Point3d> points = new List<Point3d>();

            while (points.Count < count)
            {
                // Create a random point inside the bounding box of the Brep
                Point3d randomPoint = RandomPointInBoundingBox(boundingBox, rand);

                // Check if the point is inside the Brep
                if (brep.IsPointInside(randomPoint, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, false))
                {
                    points.Add(randomPoint);
                }
            }

            return points;
        }


        private List<Point3d> RandomPointsInMesh(Mesh mesh, int count, int seed)
        {
            Random rand = new Random(seed);
            BoundingBox boundingBox = mesh.GetBoundingBox(false);
            List<Point3d> points = new List<Point3d>();

            while (points.Count < count)
            {
                // Create a random point inside the bounding box of the Mesh
                Point3d randomPoint = RandomPointInBoundingBox(boundingBox, rand);

                // Check if the point is inside the Mesh
                if (mesh.IsPointInside(randomPoint, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, false))
                {
                    points.Add(randomPoint);
                }
            }

            return points;
        }

        private Point3d RandomPointInBoundingBox(BoundingBox boundingBox, Random rand)
        {
            double x = rand.NextDouble() * (boundingBox.Max.X - boundingBox.Min.X) + boundingBox.Min.X;
            double y = rand.NextDouble() * (boundingBox.Max.Y - boundingBox.Min.Y) + boundingBox.Min.Y;
            double z = rand.NextDouble() * (boundingBox.Max.Z - boundingBox.Min.Z) + boundingBox.Min.Z;
            return new Point3d(x, y, z);
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
                return Shachihoko.Properties.Resources.PopulateGeometryEX;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("245DB995-888E-40A8-B9E0-FEC389C76960"); }
        }
    }
}
