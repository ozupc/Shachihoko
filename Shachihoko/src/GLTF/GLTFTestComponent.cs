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
    /*public class GLTFTestComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GLTFTestComponent()
          : base("Test", "Test",
            "",
            "Shachihoko", "Test")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("FolderPath", "FolderPath", "", GH_ParamAccess.item, "D:\\デスクトップ");
            pManager.AddTextParameter("FileName", "FileName", "", GH_ParamAccess.item, "model");
            pManager.AddBooleanParameter("Switch", "Switch", "", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("FilePath", "FilePath", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //---<初期化＆定義>---//
            string folderPath = "";
            string fileName = "";
            string filePath = "";
            bool runSwitch = false;

            if (!DA.GetData(0, ref folderPath)) return;
            if (!DA.GetData(1, ref fileName)) return;
            if (!DA.GetData(2, ref runSwitch)) return;

            filePath = folderPath + System.IO.Path.DirectorySeparatorChar + fileName; //System.IO.Path.DirectorySeparatorChar = パス区切り文字
            //---</初期化＆定義>---//

            if (runSwitch)
            {
                var material1 = new MaterialBuilder()
                .WithDoubleSide(true)
                .WithMetallicRoughnessShader()
                .WithChannelParam(KnownChannel.BaseColor, KnownProperty.RGBA, new Vector4(1, 0, 0, 1));

                var material2 = new MaterialBuilder()
                    .WithDoubleSide(true)
                    .WithMetallicRoughnessShader()
                    .WithChannelParam(KnownChannel.BaseColor, KnownProperty.RGBA, new Vector4(1, 0, 1, 1));

                // create a mesh with two primitives, one for each material

                var mesh = new MeshBuilder<VERTEX>("mesh");

                var prim = mesh.UsePrimitive(material1);
                prim.AddTriangle(new VERTEX(-10, 0, 0), new VERTEX(10, 0, 0), new VERTEX(0, 10, 0));
                prim.AddTriangle(new VERTEX(10, 0, 0), new VERTEX(-10, 0, 0), new VERTEX(0, -10, 0));

                prim = mesh.UsePrimitive(material2);
                prim.AddQuadrangle(new VERTEX(-5, 0, 3), new VERTEX(0, -5, 3), new VERTEX(5, 0, 3), new VERTEX(0, 5, 3));

                // create a scene

                var scene = new SharpGLTF.Scenes.SceneBuilder();

                scene.AddRigidMesh(mesh, Matrix4x4.Identity);

                // save the model in different formats

                var model = scene.ToGltf2();
                //model.SaveAsWavefront(filePath + ".obj");
                //model.SaveGLB(filePath + ".glb");
                model.SaveGLTF(filePath + ".gltf");

                DA.SetData(0, filePath + ".gltf");
            }
            

        }

        //---<メソッド>---//

        /// <summary>
        /// IGH_GeometricGooをMeshに変換.
        /// </summary>
        private Rhino.Geometry.Mesh ConvertIGH_GeometricGooToGHMesh(IGH_GeometricGoo igh_GeometricGoo) //Convertの方法を定義（ConvertAllで使用）
        {
            Rhino.Geometry.Mesh mesh = new Rhino.Geometry.Mesh();
            igh_GeometricGoo.CastTo<Rhino.Geometry.Mesh>(out mesh);

            return mesh;
        }

        //---</メソッド>---///

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1FC9BEB0-937A-4422-B950-9FF2ECE8B040");
    }*/
}