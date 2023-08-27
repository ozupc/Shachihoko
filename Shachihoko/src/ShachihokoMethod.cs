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
using SharpGLTF.Scenes;
using SharpGLTF.Animations;

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
        public void ExportGLTF(List<MeshBuilder<VERTEX>> meshes, string filePath, List<Dictionary<double, Matrix4x4>> motions = null)
        {
            // シーンの初期化
            var scene = InitializeScene();

            // 座標変換行列の事前計算
            Matrix4x4 ZtoY = CreateZtoYMatrix();
            Matrix4x4 YtoZ = Matrix4x4.Transpose(ZtoY);

            // 各メッシュに対する処理
            for (int i = 0; i < meshes.Count; i++)
            {
                if (motions != null && i < motions.Count)
                {
                    // アニメーションがある場合、ノードを作成してアニメーションを適用
                    NodeBuilder node = CreateAnimatedNode(i, motions[i], ZtoY, YtoZ);
                    scene.AddRigidMesh(meshes[i], node);
                }
                else
                {
                    // アニメーションがない場合、デフォルトの変換行列を使用
                    scene.AddRigidMesh(meshes[i], Matrix4x4.Identity);
                }
            }

            // モデルの保存
            SaveModel(scene, filePath);
        }

        // シーンを初期化する
        private SharpGLTF.Scenes.SceneBuilder InitializeScene()
        {
            return new SharpGLTF.Scenes.SceneBuilder();
        }

        // ZtoY変換行列を作成する
        private Matrix4x4 CreateZtoYMatrix()
        {
            return new Matrix4x4(
                1, 0, 0, 0,
                0, 0, -1, 0,
                0, 1, 0, 0,
                0, 0, 0, 1
            );
        }

        // アニメーション付きのノードを作成する
        private NodeBuilder CreateAnimatedNode(int index, Dictionary<double, Matrix4x4> motionDict, Matrix4x4 ZtoY, Matrix4x4 YtoZ)
        {
            // ノードの初期設定
            NodeBuilder node = new NodeBuilder($"node_{index}");
            Matrix4x4 baseMatrix = Matrix4x4.Identity;
            var scales = new List<(float, Vector3)>();
            var rotations = new List<(float, System.Numerics.Quaternion)>();
            var translations = new List<(float, Vector3)>();

            // 各フレームでの変換行列を適用
            foreach (var num in motionDict.Keys)
            {
                // 座標変換
                Matrix4x4 motionMatrix = ApplyCoordinateTransformations(motionDict[num], ZtoY, YtoZ);

                // ベース行列の更新
                baseMatrix = Matrix4x4.Multiply(baseMatrix, motionMatrix);

                // スケール、回転、移動の抽出
                ExtractTransforms(baseMatrix, motionMatrix, out Vector3 scale, out System.Numerics.Quaternion rotation, out Vector3 translation);

                scales.Add(((float)num, scale));
                rotations.Add(((float)num, rotation));
                translations.Add(((float)num, translation));
            }

            // アニメーショントラックをノードに割り当て
            AssignTracksToNode(node, scales, rotations, translations);

            return node;
        }

        // 座標変換行列を適用する
        private Matrix4x4 ApplyCoordinateTransformations(Matrix4x4 matrix, Matrix4x4 ZtoY, Matrix4x4 YtoZ)
        {
            return Matrix4x4.Multiply(ZtoY, Matrix4x4.Multiply(matrix, YtoZ));
        }

        // 変換行列からスケール、回転、移動を抽出する
        private void ExtractTransforms(Matrix4x4 baseMatrix, Matrix4x4 motionMatrix, out Vector3 scale, out System.Numerics.Quaternion rotation, out Vector3 translation)
        {
            Matrix4x4.Invert(motionMatrix, out Matrix4x4 invertMatrix);
            Matrix4x4 diffMatrix = Matrix4x4.Multiply(baseMatrix, invertMatrix);
            Matrix4x4.Decompose(Matrix4x4.Transpose(diffMatrix), out scale, out rotation, out translation);
        }

        // ノードにアニメーショントラックを割り当てる
        private void AssignTracksToNode(NodeBuilder node, List<(float, Vector3)> scales, List<(float, System.Numerics.Quaternion)> rotations, List<(float, Vector3)> translations)
        {
            node.SetScaleTrack($"{node.Name}_scale_track", CurveSampler.CreateSampler(scales));
            node.SetRotationTrack($"{node.Name}_rotation_track", CurveSampler.CreateSampler(rotations));
            node.SetTranslationTrack($"{node.Name}_translation_track", CurveSampler.CreateSampler(translations));
        }

        // モデルを保存する
        private void SaveModel(SharpGLTF.Scenes.SceneBuilder scene, string filePath)
        {
            ModelRoot model = scene.ToGltf2();
            model.SaveGLTF($"{filePath}.gltf");
        }
        ///<summary>
        ///ここまでがExportGLTF()のためのメソッド
        /// </summary>


        ///<summary>
        ///(List<GH_Matrix>からキーフレームごとの辞書型に変換.
        /// </summary>
        public Dictionary<double, Matrix4x4> ConvertMatrices(List<GH_Matrix> ghMatrices, List<double> keyFrames)
        {
            if (ghMatrices.Count != keyFrames.Count)
            {
                throw new ArgumentException("ghMatrices and keyFrames must have the same count.");
            }

            Dictionary<double, Matrix4x4> result = new Dictionary<double, Matrix4x4>();
            for (int i = 0; i < ghMatrices.Count; i++)
            {
                double key = keyFrames[i];
                GH_Matrix ghMatrix = ghMatrices[i];

                // Assuming the GH_Matrix is a 4x4 matrix
                Matrix4x4 matrix = new Matrix4x4(
                    (float)ghMatrix.Value[0, 0], (float)ghMatrix.Value[0, 1], (float)ghMatrix.Value[0, 2], (float)ghMatrix.Value[0, 3],
                    (float)ghMatrix.Value[1, 0], (float)ghMatrix.Value[1, 1], (float)ghMatrix.Value[1, 2], (float)ghMatrix.Value[1, 3],
                    (float)ghMatrix.Value[2, 0], (float)ghMatrix.Value[2, 1], (float)ghMatrix.Value[2, 2], (float)ghMatrix.Value[2, 3],
                    (float)ghMatrix.Value[3, 0], (float)ghMatrix.Value[3, 1], (float)ghMatrix.Value[3, 2], (float)ghMatrix.Value[3, 3]);

                result.Add(key, matrix);
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
