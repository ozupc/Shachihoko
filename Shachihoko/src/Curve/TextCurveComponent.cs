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
    public class TextCurveComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public TextCurveComponent()
          : base("Text Curve", "Text Curve",
              "Create Texts Curve.",
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
            pManager.AddTextParameter("Text", "Text", "Base Texts.", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "Plane", "Texts start at the Plane's Origin and follow the Y axis.", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddTextParameter("Font", "Font", "Texts's font.", GH_ParamAccess.item, "Arial");
            pManager.AddNumberParameter("Size", "Size", "Texts's size.", GH_ParamAccess.item, 100.0);
            pManager.AddBooleanParameter("Bold", "Bold", "True: Bold, False: Not Bold.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Italic", "Italic", "True: Italic, False: Not Italic.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Separete", "S", "True: Get separated texts at plane's origin. False: Get a texts line.", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Texts Curve.", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DataTree<Curve> curves = new DataTree<Curve>();

            string text = null;
            Plane plane = new Plane();
            string font = null;
            double size = 100.0;
            bool bold = false;
            bool italic = false;
            bool separate = false;

            if (!DA.GetData(0, ref text)) return;
            if (!DA.GetData(1, ref plane)) return;
            if (!DA.GetData(2, ref font)) return;
            if (!DA.GetData(3, ref size)) return;
            if (!DA.GetData(4, ref bold)) return;
            if (!DA.GetData(5, ref italic)) return;
            if (!DA.GetData(6, ref separate)) return;

            int style;
            if (bold == true && italic == false)
            {
                style = 1;
            }
            else if (bold == false && italic == true)
            {
                style = 2;
            }
            else if (bold == true && italic == true)
            {
                style = 3;
            }
            else
            {
                style = 0;
            }


            if (separate)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    GH_Path path = new GH_Path(i);
                    Curve[] textCurve = Curve.CreateTextOutlines(text[i].ToString(), font, size, style, true, plane, 0.0, 0.0);
                    foreach (Curve c in textCurve)
                    {
                        curves.Add(c, path);
                    }
                }
            }
            else
            {
                GH_Path path = new GH_Path(0);
                Curve[] textCurve = Curve.CreateTextOutlines(text, font, size, style, true, plane, 0.0, 0.0);
                foreach (Curve c in textCurve)
                {
                    curves.Add(c, path);
                }
            }
            DA.SetDataTree(0, curves);
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
                return Shachihoko.Properties.Resources.TextCurve;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4B387736-FC1C-44BA-8779-3A9405739F5B"); }
        }
    }
}
