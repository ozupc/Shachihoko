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
    public class CircleLayoutComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public CircleLayoutComponent()
          : base("Circle Layout", "Circle Layout",
              "Circle and Circle and Circle...",
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
            pManager.AddNumberParameter("Radius", "R", "A close value of total radius.", GH_ParamAccess.item, 10.0);
            pManager.AddIntegerParameter("Iteration", "I", "The number of iterations.", GH_ParamAccess.item, 3);
            pManager.AddIntegerParameter("Count", "N", "Number of circles in a circle.", GH_ParamAccess.item, 10);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCircleParameter("Circles", "C", "Circles in trees.", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double Rad = 10.0;
            int itr = 3;
            int count = 10;

            if (!DA.GetData(0, ref Rad)) return;
            if (!DA.GetData(1, ref itr)) return;
            if (!DA.GetData(2, ref count)) return;

            List<List<Circle>> circles = CreateModel(Rad, count, itr);
            DataTree<Circle> circles_tree = ListToDatatree(circles);

            DA.SetDataTree(0, circles_tree);
        }

        private List<List<Circle>> CreateModel(double Rad, int count, int itr)
        {
            //First circles and intersections.

            List<Point3d> first_center_points = new List<Point3d>();
            List<Point3d> first_intersections = new List<Point3d>();
            List<Circle> first_circles = new List<Circle>();
            double radius = new double();
            Point3d o = new Point3d(0.0, 0.0, 0.0);

            for (int i = 0; i < count; i++)
            {
                //First center points.
                double ptx = Rad * Math.Cos(360 * i / (count) * Math.PI / 180);
                double pty = Rad * Math.Sin(360 * i / (count) * Math.PI / 180);
                Point3d center_point = new Point3d(ptx, pty, 0.0);
                first_center_points.Add(center_point);
            }


            for (int i = 0; i < count; i++)
            {
                //First intersections.
                if (i != count - 1)
                {
                    Vector3d p0 = new Vector3d(first_center_points[i]);
                    Vector3d p1 = new Vector3d(first_center_points[i + 1]);
                    Vector3d v = new Vector3d((p0 + p1) / 2);
                    Point3d first_intersection = new Point3d(v.X, v.Y, 0);
                    first_intersections.Add(first_intersection);
                }
                else
                {
                    Vector3d p0 = new Vector3d(first_center_points[i]);
                    Vector3d p1 = new Vector3d(first_center_points[0]);
                    Vector3d v = new Vector3d((p0 + p1) / 2);
                    Point3d first_intersection = new Point3d(v.X, v.Y, 0);
                    first_intersections.Add(first_intersection);

                    radius = Math.Sqrt(Math.Pow((p0.X - p1.X), 2) + Math.Pow((p0.Y - p1.Y), 2));
                }
            }

            for (int i = 0; i < count; i++)
            {
                //First circles.
                Circle first_circle = new Circle(first_center_points[i], radius / 2);
                first_circles.Add(first_circle);
            }

            //Make Circle lists.

            List<List<Circle>> circles = new List<List<Circle>> { first_circles };
            List<List<Point3d>> intersections = new List<List<Point3d>> { first_intersections };
            List<Point3d> center_points = new List<Point3d>();

            for (int i = 0; i < itr; i++)
            {
                List<Point3d> new_intersections = new List<Point3d>();
                List<Circle> new_circles = new List<Circle>();

                //Intersections.
                for (int j = 0; j < count; j++)
                {
                    //Line.
                    Point3d center_point = circles[i][j].Center;
                    center_points.Add(center_point);
                    Line line = new Line(o, center_point);

                    //Intersect point circle and line. 
                    var Event = Rhino.Geometry.Intersect.Intersection.LineCircle(line, circles[i][j], out double t1, out Point3d crossPt_0, out double t2, out Point3d crossPt_1);
                    if (crossPt_0.DistanceTo(o) < crossPt_1.DistanceTo(o))
                    {
                        new_intersections.Add(crossPt_0);
                    }
                    else
                    {
                        new_intersections.Add(crossPt_1);
                    }
                }
                intersections.Add(new_intersections);

                //Circles.
                for (int j = 0; j < count; j++)
                {
                    List<Point3d> new_intersections_sub = new List<Point3d>(new_intersections);
                    List<Point3d> circle_pts = new List<Point3d> { intersections[i][j] };

                    int closest_id_0 = Rhino.Collections.Point3dList.ClosestIndexInList(new_intersections_sub, intersections[i][j]);
                    circle_pts.Add(new_intersections_sub[closest_id_0]);
                    new_intersections_sub.RemoveAt(closest_id_0);
                    int closest_id_1 = Rhino.Collections.Point3dList.ClosestIndexInList(new_intersections_sub, intersections[i][j]);
                    circle_pts.Add(new_intersections_sub[closest_id_1]);

                    Circle new_circle = new Circle(circle_pts[0], circle_pts[1], circle_pts[2]);
                    new_circles.Add(new_circle);
                }
                circles.Add(new_circles);
            }
            return circles;
        }

        private DataTree<Circle> ListToDatatree(List<List<Circle>> circles)
        {
            DataTree<Circle> circles_tree = new DataTree<Circle>();
            for (int i = 0; i < circles.Count; i++)
            {
                GH_Path pth = new GH_Path(i);
                for (int j = 0; j < circles[i].Count; j++)
                {
                    circles_tree.Add(circles[i][j], pth);
                }
            }
            return circles_tree;
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
                return Shachihoko.Properties.Resources.CircleLayout;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("BE3E4B81-26F5-4A82-B483-20CF235B7A2B"); }
        }
    }
}
