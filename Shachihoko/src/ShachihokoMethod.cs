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
    internal class ShachihokoMethod
    {
        //---<メソッド>---//
        /// <summary>
        /// IGH_GeometricGooをGHMeshに変換.
        /// </summary>
        public Rhino.Geometry.Mesh ConvertIGH_GeometricGooToGHMesh(IGH_GeometricGoo igh_GeometricGoo) //Convertの方法を定義.（ConvertAllで使用.）
        {
            Rhino.Geometry.Mesh mesh = new Rhino.Geometry.Mesh();
            igh_GeometricGoo.CastTo(out mesh);

            return mesh;
        }

        /// <summary>
        /// IGH_GooをMaterialBuilderに変換.
        /// </summary>
        public MaterialBuilder ConvertIGH_GooToMaterialBuilder(IGH_Goo igh_Goo) //Convertの方法を定義.（ConvertAllで使用.）
        {
            MaterialBuilder materialBuilder = new MaterialBuilder();
            igh_Goo.CastTo(out materialBuilder);

            return materialBuilder;
        }

        ///<summary>
        ///GHMeshをList<List<VERTEX>>に変換.
        /// </summary>
        public DataTree<VERTEX> ConvertGHMeshVertex(Rhino.Geometry.Mesh mesh)
        {
            //---<初期化>---//
            DataTree<VERTEX> vertexs = new DataTree<VERTEX>();

            //---<GHMeshの軸変換>--// ※RhinoはZup, GLTFはYup
            Rhino.Geometry.Plane rhinoPlane = new Rhino.Geometry.Plane(Rhino.Geometry.Plane.WorldXY);
            Rhino.Geometry.Plane gltfPlane = new Rhino.Geometry.Plane(Rhino.Geometry.Plane.WorldZX);
            mesh.Transform(Rhino.Geometry.Transform.PlaneToPlane(rhinoPlane, gltfPlane));

            //---<変換>---//
            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                //--<初期化>--//
                GH_Path path = new GH_Path(i);
                MeshFace meshFace = mesh.Faces[i];
                int indexA = meshFace.A;
                int indexB = meshFace.B;
                int indexC = meshFace.C;
                Point3f pointA = mesh.Vertices[indexA];
                Point3f pointB = mesh.Vertices[indexB];
                Point3f pointC = mesh.Vertices[indexC];

                //--<VERTEXに変換>--//
                VERTEX vA = new VERTEX(pointA.X, pointA.Y, pointA.Z);
                VERTEX vB = new VERTEX(pointB.X, pointB.Y, pointB.Z);
                VERTEX vC = new VERTEX(pointC.X, pointC.Y, pointC.Z);

                //--<List(子)に追加>--//
                vertexs.Add(vA, path);
                vertexs.Add(vB, path);
                vertexs.Add(vC, path);

                //--<四角メッシュフェイスの場合>--//
                if (meshFace.IsQuad)
                {
                    //-<初期化>-//
                    int indexD = meshFace.D;
                    Point3f pointD = mesh.Vertices[indexD];

                    //-<VERTEXに変換>-//
                    VERTEX vD = new VERTEX(pointD.X, pointD.Y, pointD.Z);

                    //--<Listに追加>--//
                    vertexs.Add(vD, path);
                }
            }

            //---<return>---//
            return vertexs;
        }

        ///<summary>
        ///List<List<VERTEX>>からMeshBuilderを作成.
        /// </summary>
        public MeshBuilder<VERTEX> CreateMeshBuilder(DataTree<VERTEX> vertexs, MaterialBuilder material, string meshName)
        {
            //---<初期化>---//
            MeshBuilder<VERTEX> mesh = new MeshBuilder<VERTEX>(meshName);
            PrimitiveBuilder<MaterialBuilder, VERTEX, VertexEmpty, VertexEmpty> prim = mesh.UsePrimitive(material);

            //---<VERTEXの登録>---//
            foreach (GH_Path path in vertexs.Paths)
            {
                if (vertexs.Branch(path).Count == 3)
                {
                    prim.AddTriangle(vertexs.Branch(path)[0], vertexs.Branch(path)[1], vertexs.Branch(path)[2]);
                }
                else if (vertexs.Branch(path).Count == 4)
                {
                    prim.AddQuadrangle(vertexs.Branch(path)[0], vertexs.Branch(path)[1], vertexs.Branch(path)[2], vertexs.Branch(path)[3]);
                }
            }

            //---<return>---//
            return mesh;
        }

        ///<summary>
        ///List<MeshBuilder>からGLTFに書き出し.
        /// </summary>
        public void ExportGLTF(List<MeshBuilder<VERTEX>> meshs, string filePath)
        {
            //---<初期化>---//
            SharpGLTF.Scenes.SceneBuilder scene = new SharpGLTF.Scenes.SceneBuilder();

            //---<sceneの作成>---//
            foreach (MeshBuilder<VERTEX> mesh in meshs)
            {
                scene.AddRigidMesh(mesh, Matrix4x4.Identity);
            }

            //---<modelの作成>---//
            ModelRoot model = scene.ToGltf2();

            //---<書き出し>---//
            model.SaveGLTF(filePath + ".gltf");
        }

        ///<summary>
        ///List<Rhino.Geometry.Mesh>から移動ベクトルを書き出し.
        /// </summary>
        public Dictionary<float, List<Vector3f>> CalculateVertexMovements(List<Rhino.Geometry.Mesh> meshList, List<double> keyframes)
        {
            Dictionary<float, List<Vector3f>> result = new Dictionary<float, List<Vector3f>>();

            for (int i = 0; i < meshList.Count - 1; i++) // Loop through each keyframe except the last one
            {
                float currentKeyframe = (float)keyframes[i];
                List<Vector3f> keyframeResult = new List<Vector3f>();

                Rhino.Geometry.Mesh currentMesh = meshList[i];
                Rhino.Geometry.Mesh nextMesh = meshList[i + 1];

                for (int k = 0; k < currentMesh.Vertices.Count; k++) // Loop through each vertex in the mesh
                {
                    Vector3f movementVector = (Vector3f)(nextMesh.Vertices[k] - currentMesh.Vertices[k]);
                    keyframeResult.Add(movementVector);
                }

                result[currentKeyframe] = keyframeResult;
            }

            return result;
        }




        //---<辞書>---//
        public static readonly Dictionary<string, string> Category = new Dictionary<string, string>
        {
            {
                "Bake", "00_Bake"
            },
            {
                "GLTF", "01_GLTF"
            },
            {
                "Set", "10_Set"
            },
            {
                "Surface", "14_Surface"
            },
            {
                "Curve", "13_Curve"
            },
            {
                "Math", "11_Math"
            },
            {
                "Vector", "12_Vector"
            }
        };        
    }
}
