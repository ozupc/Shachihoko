using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Numerics;

using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Parameters;

using Rhino.Geometry;
using Rhino.Runtime;

using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using VERTEX = SharpGLTF.Geometry.VertexTypes.VertexPosition;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;


namespace Shachihoko
{
    public class ConvertTransformToMatrix : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ConvertTransformToMatrix()
          : base("Transform", "Transform",
            "Converts Rhino.Geometry.Transform to System.Numerics.Matrix4x4.",
            "Shachihoko", ShachihokoMethod.Category["GLTF"])
        {
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTransformParameter("Transform", "T", "Translate, Rotate, Scale data for each keyframe.", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Matrix", "M", "Converted matrix.\r\nIf you wish to compose a deformation matrix,\r\nenter the multiplication components in the **reverse** order in which you wish to perform the deformations.", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Transform> inputTransforms = new GH_Structure<GH_Transform>();
            if (!DA.GetDataTree(0, out inputTransforms)) return;

            GH_Structure<GH_Matrix> outputMatrices = new GH_Structure<GH_Matrix>();

            foreach (GH_Path path in inputTransforms.Paths)
            {
                GH_Path newPath = path;
                foreach (GH_Transform transform in inputTransforms.get_Branch(path))
                {
                    Transform rhinoTransform = transform.Value;
                    Matrix matrix = ConvertToMatrix(rhinoTransform);
                    outputMatrices.Append(new GH_Matrix(matrix), newPath);
                }
            }

            DA.SetDataTree(0, outputMatrices);
        }

        private Matrix ConvertToMatrix(Transform transform)
        {
            Matrix matrix = new Matrix(4, 4);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    matrix[i, j] = transform[i, j];
                }
            }
            return matrix;
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("FDAD705A-DE80-46CA-B6D3-B9E8CBA8BFE5");
    }
}