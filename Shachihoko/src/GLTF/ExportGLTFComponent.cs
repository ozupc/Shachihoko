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
    public class ExportGLTFComponent : GH_Component, IGH_VariableParameterComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ExportGLTFComponent()
          : base("Export", "Export",
            "",
            "Shachihoko", ShachihokoMethod.Category["GLTF"])
        {
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        public override void CreateAttributes()
        {
            m_attributes = new ExportGLTFAttribute(this);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            if (ExportStyle == 0)
            {
                for (int i = 0; i < ComponentName["Static"].Count; i++)
                {
                    AddParameter(pManager, ComponentInputParam["Static"][i], ComponentName["Static"][i], ComponentName["Static"][i], ComponentDescription["Static"][i], ComponentGH_ParamAccess["Static"][i]);
                    if (ComponentName["Static"][i] == "FolderPath")
                    {
                        pManager[i].AddVolatileData(new GH_Path(0), 0, new GH_String(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)));
                    }
                }
            }
            else if (ExportStyle == 1)
            {
                for (int i = 0; i < ComponentName["Animation"].Count; i++)
                {
                    AddParameter(pManager, ComponentInputParam["Animation"][i], ComponentName["Animation"][i], ComponentName["Animation"][i], ComponentDescription["Animation"][i], ComponentGH_ParamAccess["Animation"][i]);
                    if (ComponentName["Animation"][i] == "FolderPath")
                    {
                        pManager[i].AddVolatileData(new GH_Path(0), 0, new GH_String(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)));
                    }
                }
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //---<初期化＆定義>---//
            GH_Structure<IGH_GeometricGoo> ghMeshs_IGH_GeometricGoos = new GH_Structure<IGH_GeometricGoo>();
            GH_Structure<IGH_Goo> materialBuilders_IGH_Goo = new GH_Structure<IGH_Goo>();
            List<double> keyFrames = new List<double>();
            GH_Structure<GH_Matrix> ghMatrices = new GH_Structure<GH_Matrix>();
            string folderPath = "";
            string fileName = "";
            string filePath = "";
            bool runSwitch = false;

            List<Rhino.Geometry.Mesh> ghMeshs = new List<Rhino.Geometry.Mesh>(); //一時保管用.
            List<MaterialBuilder> materialBuilders = new List<MaterialBuilder>(); //一時保管用.
            GH_Path path = new GH_Path();

            DataTree<VERTEX> vertexs = new DataTree<VERTEX>();
            List<MeshBuilder<VERTEX>> meshBuilders = new List<MeshBuilder<VERTEX>>();
            ShachihokoMethod shachihokoMethod = new ShachihokoMethod();
            int num = 0;            

            if (ExportStyle == 0)
            {
                if (!DA.GetDataTree(0, out ghMeshs_IGH_GeometricGoos)) return;
                if (!DA.GetDataTree(1, out materialBuilders_IGH_Goo)) return;
                if (!DA.GetData(2, ref folderPath)) return;
                if (!DA.GetData(3, ref fileName)) return;
                if (!DA.GetData(4, ref runSwitch)) return;

                filePath = folderPath + Path.DirectorySeparatorChar + fileName; //System.IO.Path.DirectorySeparatorChar = パス区切り文字.

                //---<実行>---//            
                for (int i = 0; i < ghMeshs_IGH_GeometricGoos.Paths.Count; i++)
                {
                    path = ghMeshs_IGH_GeometricGoos.Paths[i]; //pathを定義.

                    //--<Errorチェック>--//
                    if (materialBuilders_IGH_Goo.PathExists(path) == false)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "MeshとMaterialのツリー構造が一致しません.");
                    }

                    //---<IGH_GeometricGooをGHMeshに変換>---//
                    ghMeshs = ghMeshs_IGH_GeometricGoos[path].ConvertAll(shachihokoMethod.ConvertIGH_GeometricGooToGHMesh);

                    //---<IGH_GooをMaterialBuilderに変換>---//
                    materialBuilders = materialBuilders_IGH_Goo[path].ConvertAll(shachihokoMethod.ConvertIGH_GooToMaterialBuilder);

                    for (int j = 0; j < ghMeshs.Count; j++)
                    {
                        //---<GHMeshをList<List<VERTEX>>に変換>---//
                        vertexs = shachihokoMethod.ConvertGHMeshVertex(ghMeshs[j]);

                        //---<MeshBuilderを作成>---//
                        MeshBuilder<VERTEX> meshBuilder = shachihokoMethod.CreateMeshBuilder(vertexs, materialBuilders[j], num.ToString());
                        num += 1;
                        meshBuilders.Add(meshBuilder);
                    }
                }

                //---<GLTFに書き出し>---//
                if (runSwitch)
                {
                    shachihokoMethod.ExportGLTF(meshBuilders, filePath);
                }
            }
            else if(ExportStyle == 1)
            {
                if (!DA.GetDataTree(0, out ghMeshs_IGH_GeometricGoos)) return;
                if (!DA.GetDataTree(1, out materialBuilders_IGH_Goo)) return;
                if (!DA.GetDataList(2, keyFrames)) return;
                if (!DA.GetDataTree(3, out ghMatrices)) return;
                if (!DA.GetData(3, ref folderPath)) return;
                if (!DA.GetData(4, ref fileName)) return;
                if (!DA.GetData(5, ref runSwitch)) return;

                filePath = folderPath + Path.DirectorySeparatorChar + fileName; //System.IO.Path.DirectorySeparatorChar = パス区切り文字.
                    
                /*
                //---<GLTFに書き出し>---//
                if (runSwitch)
                {
                    shachihokoMethod.ExportGLTF(meshBuilders, filePath);
                }*/
            }
        }      

        ///<summary>
        ///Inputの種類を可変にする関数.
        /// </summary>
        private int AddParameter(GH_InputParamManager pManager, string inputParam, string name, string nickname, string description, GH_ParamAccess accessType)
        {
            switch (inputParam)
            {
                case "Generic":
                    return pManager.AddGenericParameter(name, nickname, description, accessType);
                case "Geometry":
                    return pManager.AddGeometryParameter(name, nickname, description, accessType);
                case "Text":
                    return pManager.AddTextParameter(name, nickname, description, accessType);
                case "Boolean":
                    return pManager.AddBooleanParameter(name, nickname, description, accessType);
                case "Number":
                    return pManager.AddNumberParameter(name, nickname, description, accessType);
                case "Matrix":
                    return pManager.AddMatrixParameter(name, nickname, description, accessType);
                default:
                    return pManager.AddGenericParameter(name, nickname, description, accessType);
            }            
        }

        //---<プロパティ>---//
        //--<ExportStyle>--//
        /// <summary>
        /// 0 = Static, 1 = Animation.
        /// </summary>
        public int ExportStyle { get; set; }

        //--<inputリスト>--//
        public static readonly Dictionary<string, List<string>> ComponentInputParam = new Dictionary<string, List<string>>
        {
            {
                "Static", new List<string>()
                {
                    "Geometry",
                    "Generic",
                    "Text",
                    "Text",
                    "Boolean"
                }
            },
            {
                "Animation", new List<string>()
                {
                    "Geometry",
                    "Generic",
                    "Number",
                    "Matrix",
                    "Text",
                    "Text",
                    "Boolean"
                }
            }
        };

        public static readonly Dictionary<string, List<GH_ParamAccess>> ComponentGH_ParamAccess = new Dictionary<string, List<GH_ParamAccess>>
        {
            {
                "Static", new List<GH_ParamAccess>()
                {
                    GH_ParamAccess.tree,
                    GH_ParamAccess.tree,
                    GH_ParamAccess.item,
                    GH_ParamAccess.item,
                    GH_ParamAccess.item
                }
            },
            {
                "Animation", new List<GH_ParamAccess>()
                {
                    GH_ParamAccess.tree,
                    GH_ParamAccess.tree,
                    GH_ParamAccess.list,
                    GH_ParamAccess.tree,
                    GH_ParamAccess.item,
                    GH_ParamAccess.item,
                    GH_ParamAccess.item
                }
            }
        };

        public static readonly Dictionary<string, List<string>> ComponentName = new Dictionary<string, List<string>>
        {
            {
                "Static", new List<string>()
                {
                    "Mesh",
                    "Material",
                    "FolderPath",
                    "FileName",
                    "Switch"
                }
            },
            {
                "Animation", new List<string>()
                {
                    "Mesh",
                    "Material",
                    "KeyFrame",
                    "Transform",
                    "FolderPath",
                    "FileName",
                    "Switch"
                }
            }
        };

        public static readonly Dictionary<string, List<string>> ComponentDescription = new Dictionary<string, List<string>>
        {
            {
                "Static", new List<string>()
                {
                    "DataTree should be input in a simplified state.\r\nThe data structure should be the same as the \"Material\" DataTree.",
                    "DataTree should be input in a simplified state.\r\nThe data structure should be the same as the \"Mesh\" DataTree.",
                    "FolderPath",
                    "FileName",
                    "Switch"
                }
            },
            {
                "Animation", new List<string>()
                {
                    "DataTree should be input in a simplified state.\r\nThe data structure should be the same as the \"Material\" DataTree.",
                    "DataTree should be input in a simplified state.\r\nThe data structure should be the same as the \"Mesh\" DataTree.",
                    "KeyFrame",
                    "Transform",
                    "FolderPath",
                    "FileName",
                    "Switch"
                }
            }
        };

        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetInt32("ExportStyle", ExportStyle);
            return base.Write(writer);
        }

        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            ExportStyle = reader.GetInt32("ExportStyle");
            return base.Read(reader);
        }

        private void Params_ParameterChanged(object sender, GH_ParamServerEventArgs e)
        {
            throw new NotImplementedException();
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            return new Param_Integer();
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        public void VariableParameterMaintenance()
        {
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
        public override Guid ComponentGuid => new Guid("E6FDD451-D82D-46F6-B20A-9E6803DB12C2");
    }
}