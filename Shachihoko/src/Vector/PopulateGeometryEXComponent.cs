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
            pManager.AddGeometryParameter("Geometry", "G", "Geometry to populate. (Closed brep or meshe only.)", GH_ParamAccess.item);
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

            Random r_x = new Random(seed + 0);
            Random r_y = new Random(seed + 1);
            Random r_z = new Random(seed + 2);

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

                BoundingBox bb = m.GetBoundingBox(true);


                for (int i = 0; i < count; i++)
                {
                    Point3d pt = new Point3d(GetRandomNumber(r_x, bb.GetCorners()[0].X, bb.GetCorners()[6].X), GetRandomNumber(r_y, bb.GetCorners()[0].Y, bb.GetCorners()[6].Y), GetRandomNumber(r_z, bb.GetCorners()[0].Z, bb.GetCorners()[6].Z));

                    if (m.IsPointInside(pt, 0.01, true))
                    {
                        pts.Add(pt);
                    }
                    else
                    {
                        i--;
                    }
                }
            }

            else if (geo is GH_Brep || geo is GH_Surface)
            {
                Brep b = new Brep();
                geo.CastTo<Brep>(out b);

                if (!(b.IsSolid))
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Geometry shoud be Closed Brep or Mesh.");
                }

                BoundingBox bb = b.GetBoundingBox(true);

                for (int i = 0; i < count; i++)
                {
                    Point3d pt = new Point3d(GetRandomNumber(r_x, bb.GetCorners()[0].X, bb.GetCorners()[6].X), GetRandomNumber(r_y, bb.GetCorners()[0].Y, bb.GetCorners()[6].Y), GetRandomNumber(r_z, bb.GetCorners()[0].Z, bb.GetCorners()[6].Z));
                    if (b.IsPointInside(pt, 0.01, true))
                    {
                        pts.Add(pt);
                    }
                    else
                    {
                        i--;
                    }
                }
            }

            else
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Geometry shoud be Closed Brep or Mesh.");
            }

            DA.SetDataList(0, pts);
        }

        public double GetRandomNumber(Random random, double minimum, double maximum)
        {
            return random.NextDouble() * (maximum - minimum) + minimum;
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
