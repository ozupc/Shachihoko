using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Types;
using System.Linq;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

/// <summary>
/// 参考
/// https://digiarchi.jp/grasshopper/create-component-menu/
/// </summary>

namespace Shachihoko
{
    public enum Style { PerObject, UnionBox };

    public class BoundingBoxEXComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        protected Style style = Style.PerObject;

        public BoundingBoxEXComponent()
          : base("Bounding Box Ex", "BBox Ex",
              "Solve oriented geometry bounding boxes and display the size.",
              "Shachihoko", ShachihokoMethod.Category["Surface"])
        {
            UpdateMenu();
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Content", "C", "Geometry to contain.", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "P", "BoundingBox orientation plane.", GH_ParamAccess.item, Plane.WorldXY);

            pManager.HideParameter(1);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBoxParameter("Box", "B", "Aligned bounding box in world coordinates.", GH_ParamAccess.list);
            pManager.AddNumberParameter("X", "X", "X size of box.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Y", "Y", "Y size of box.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Z", "Z", "Z size of box.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///定義///
            List<IGH_GeometricGoo> geometrys = new List<IGH_GeometricGoo>();
            Plane plane = new Plane();

            List<BoundingBox> bboxs = new List<BoundingBox>();
            List<double> xs = new List<double>();
            List<double> ys = new List<double>();
            List<double> zs = new List<double>();
            ///

            if (!DA.GetDataList(0, geometrys)) return;
            if (!DA.GetData(1, ref plane)) return;

            ///BBox（個別）の作成
            for (int i = 0; i < geometrys.Count; i++)
            {
                GeometryBase geometryBase = GH_Convert.ToGeometryBase(geometrys[i]);
                BoundingBox bbox = geometryBase.GetBoundingBox(plane);
                bboxs.Add(bbox);
            }
            ///

            ///場合分け
            if (style == Style.PerObject)
            {
                for (int i = 0; i < bboxs.Count; i++)
                {
                    xs.Add(bboxs[i].Max.X - bboxs[i].Min.X);
                    ys.Add(bboxs[i].Max.Y - bboxs[i].Min.Y);
                    zs.Add(bboxs[i].Max.Z - bboxs[i].Min.Z);
                }
            }
            else if (style == Style.UnionBox)
            {
                List<double> xs_max = new List<double>();
                List<double> ys_max = new List<double>();
                List<double> zs_max = new List<double>();
                List<double> xs_min = new List<double>();
                List<double> ys_min = new List<double>();
                List<double> zs_min = new List<double>();

                for (int i = 0; i < bboxs.Count; i++)
                {
                    xs_max.Add(bboxs[i].Max.X);
                    ys_max.Add(bboxs[i].Max.Y);
                    zs_max.Add(bboxs[i].Max.Z);
                    xs_min.Add(bboxs[i].Min.X);
                    ys_min.Add(bboxs[i].Min.Y);
                    zs_min.Add(bboxs[i].Min.Z);
                }

                Point3d maxPt = new Point3d(xs_max.Max(), ys_max.Max(), zs_max.Max());
                Point3d minPt = new Point3d(xs_min.Min(), ys_min.Min(), zs_min.Min());
                BoundingBox bbox = new BoundingBox(minPt, maxPt);

                bboxs.Clear();
                bboxs.Add(bbox);

                xs.Add(bbox.Max.X - bbox.Min.X);
                ys.Add(bbox.Max.Y - bbox.Min.Y);
                zs.Add(bbox.Max.Z - bbox.Min.Z);
            }
            else
            {

            }

            DA.SetDataList(0, bboxs);
            DA.SetDataList(1, xs);
            DA.SetDataList(2, ys);
            DA.SetDataList(3, zs);
        }

        private void UpdateMenu()
        {
            switch (style)
            {
                case Style.PerObject:
                    Message = "Per Object";
                    break;
                case Style.UnionBox:
                    Message = "Union Box";
                    break;
                default:
                    break;
            }
        }

        protected override void AppendAdditionalComponentMenuItems(System.Windows.Forms.ToolStripDropDown menu)
        {
            System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_0 = GH_DocumentObject.Menu_AppendItem(menu, "Per Object", new System.EventHandler(this.menu_PerObject), true, style == Style.PerObject);
            toolStripMenuItem_0.ToolTipText = "";
            System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_1 = GH_DocumentObject.Menu_AppendItem(menu, "Union Box", new System.EventHandler(this.menu_UnionBox), true, style == Style.UnionBox);
            toolStripMenuItem_1.ToolTipText = "";
        }

        private void menu_PerObject(object sender, System.EventArgs e)
        {
            base.RecordUndoEvent("Style");
            this.style = Style.PerObject;
            this.UpdateMenu();
            this.ExpireSolution(true);
        }
        private void menu_UnionBox(object sender, System.EventArgs e)
        {
            base.RecordUndoEvent("Style");
            this.style = Style.UnionBox;
            this.UpdateMenu();
            this.ExpireSolution(true);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Shachihoko.Properties.Resources.BoundingBoxEX;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("318D082A-3747-44BE-8B4D-92461990969C"); }
        }
    }
}
