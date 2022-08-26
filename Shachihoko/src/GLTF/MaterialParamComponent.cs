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
using SharpGLTF.Memory;
using SharpGLTF.Schema2;


namespace Shachihoko
{
    public class MaterialParamComponent : GH_Component, IGH_VariableParameterComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public MaterialParamComponent()
          : base("MaterialParam", "MaterialParam",
            "",
            "Shachihoko", ShachihokoMethod.Category["GLTF"])
        {
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        public override void CreateAttributes()
        {
            m_attributes = new MaterialParamAttribute(this);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            if (ParamType == 0)
            {
                pManager.AddGenericParameter(ComponentName["Vector4"][0], ComponentName["Vector4"][0], ComponentDescription["Vector4"][0], ComponentGH_ParamAccess["Vector4"][0]);
                pManager.AddGenericParameter(ComponentName["Vector4"][1], ComponentName["Vector4"][1], ComponentDescription["Vector4"][1], ComponentGH_ParamAccess["Vector4"][1]);
                pManager.AddGenericParameter(ComponentName["Vector4"][2], ComponentName["Vector4"][2], ComponentDescription["Vector4"][2], ComponentGH_ParamAccess["Vector4"][2]);
                pManager.AddGenericParameter(ComponentName["Vector4"][3], ComponentName["Vector4"][3], ComponentDescription["Vector4"][3], ComponentGH_ParamAccess["Vector4"][3]);
            }
            else if(ParamType == 1)
            {
                pManager.AddGenericParameter(ComponentName["Image"][0], ComponentName["Image"][0], ComponentDescription["Image"][0], ComponentGH_ParamAccess["Image"][0]);
                pManager.AddGenericParameter(ComponentName["Image"][1], ComponentName["Image"][1], ComponentDescription["Image"][1], ComponentGH_ParamAccess["Image"][1]);
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("MaterialParam", "MaterialParam", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //---<初期化>---//
            MaterialParam materialParam = new MaterialParam();

            //---<Param別>---//
            if(ParamType == 0)
            {
                //--<初期化>--//
                double x = 0.0;
                double y = 0.0;
                double z = 0.0;
                double w = 0.0;

                if (!DA.GetData(0, ref x)) return;
                if (!DA.GetData(1, ref y)) return;
                if (!DA.GetData(2, ref z)) return;
                if (!DA.GetData(3, ref w)) return;

                //--<Vector4の作成>--//
                Vector4 vector4 = new Vector4(((float)x), ((float)y), ((float)z), ((float)w));

                //--<代入>--//
                materialParam.Vector4 = vector4;
                materialParam.ParamStyle = 0;

            }
            else if(ParamType == 1)
            {
                //--<初期化>--//
                string filePath = "";
                double optinalNum = 1.0;

                if (!DA.GetData(0, ref filePath)) return;
                DA.GetData(1, ref optinalNum);

                MemoryImage memoryImage = new MemoryImage(filePath);

                //--<代入>--//
                materialParam.MemoryImage = memoryImage;
                materialParam.OptionalNumber = (float)optinalNum;
                materialParam.ParamStyle = 1;
            }
            //---<SetData>---//
            DA.SetData(0, materialParam);
        }

        //---<プロパティ>---//
        //--<Shader>--//
        /// <summary>
        /// 0 = Vector4, 1 = Image.
        /// </summary>
        public int ParamType { get; set; }

        //--<inputリスト>--//
        public static readonly Dictionary<string, List<GH_ParamAccess>> ComponentGH_ParamAccess = new Dictionary<string, List<GH_ParamAccess>>
        {
            {
                "Vector4", new List<GH_ParamAccess>()
                {
                    GH_ParamAccess.item,
                    GH_ParamAccess.item,
                    GH_ParamAccess.item,
                    GH_ParamAccess.item
                }
            },
            {
                "Image", new List<GH_ParamAccess>()
                {
                    GH_ParamAccess.item,
                    GH_ParamAccess.item,
                }
            }
        };

        public static readonly Dictionary<string, List<string>> ComponentName = new Dictionary<string, List<string>>
        {
            {
                "Vector4", new List<string>()
                {
                    "X component",
                    "Y component",
                    "Z component",
                    "W component"
                }
            },
            {
                "Image", new List<string>()
                {
                    "FilePath",
                    "Number"
                }
            }
        };

        public static readonly Dictionary<string, List<string>> ComponentDescription = new Dictionary<string, List<string>>
        {
            {
                "Vector4", new List<string>()
                {
                    "Vector [x] component.",
                    "Vector [y] component.",
                    "Vector [z] component.",
                    "Vector [W] component."
                }
            },
            {
                "Image", new List<string>()
                {
                    "FilePath of an image.",
                    "An optional number. Normal:Scale, Occlussion:Strength."
                }
            }
        };

        public static readonly Dictionary<string, List<double>> Default = new Dictionary<string, List<double>>
        {
            {
                "Vector4", new List<double>()
                {
                    0.0,
                    0.0,
                    0.0,
                    0.0
                }
            }
        };

        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetInt32("ParamType", ParamType);
            return base.Write(writer);
        }

        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            ParamType = reader.GetInt32("ParamType");
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
        public override Guid ComponentGuid => new Guid("AEC00C77-DA90-4480-BA4F-66A4D422108B");        
    }
}