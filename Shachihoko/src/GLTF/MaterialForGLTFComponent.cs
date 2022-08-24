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
    public class MaterialForGLTFComponent : GH_Component, IGH_VariableParameterComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public MaterialForGLTFComponent()
          : base("Material", "Material",
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
            m_attributes = new MaterialForGLTFAttribute(this);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            if (ShaderType == 0)
            {
                pManager.AddGenericParameter(ComponentName["MetallicRoughness"][0], ComponentName["MetallicRoughness"][0], ComponentDescription["MetallicRoughness"][0], ComponentGH_ParamAccess["MetallicRoughness"][0]);
                pManager.AddGenericParameter(ComponentName["MetallicRoughness"][1], ComponentName["MetallicRoughness"][1], ComponentDescription["MetallicRoughness"][1], ComponentGH_ParamAccess["MetallicRoughness"][1]);
                pManager.AddGenericParameter(ComponentName["MetallicRoughness"][2], ComponentName["MetallicRoughness"][2], ComponentDescription["MetallicRoughness"][2], ComponentGH_ParamAccess["MetallicRoughness"][2]);
                pManager.AddGenericParameter(ComponentName["MetallicRoughness"][3], ComponentName["MetallicRoughness"][3], ComponentDescription["MetallicRoughness"][3], ComponentGH_ParamAccess["MetallicRoughness"][3]);
                pManager.AddGenericParameter(ComponentName["MetallicRoughness"][4], ComponentName["MetallicRoughness"][4], ComponentDescription["MetallicRoughness"][4], ComponentGH_ParamAccess["MetallicRoughness"][4]);
            }
            else if(ShaderType == 1)
            {
                pManager.AddGenericParameter(ComponentName["SpecularGlossiness"][0], ComponentName["SpecularGlossiness"][0], ComponentDescription["SpecularGlossiness"][0], ComponentGH_ParamAccess["SpecularGlossiness"][0]);
                pManager.AddGenericParameter(ComponentName["SpecularGlossiness"][1], ComponentName["SpecularGlossiness"][1], ComponentDescription["SpecularGlossiness"][1], ComponentGH_ParamAccess["SpecularGlossiness"][1]);
                pManager.AddGenericParameter(ComponentName["SpecularGlossiness"][2], ComponentName["SpecularGlossiness"][2], ComponentDescription["SpecularGlossiness"][2], ComponentGH_ParamAccess["SpecularGlossiness"][2]);
                pManager.AddGenericParameter(ComponentName["SpecularGlossiness"][3], ComponentName["SpecularGlossiness"][3], ComponentDescription["SpecularGlossiness"][3], ComponentGH_ParamAccess["SpecularGlossiness"][3]);
                pManager.AddGenericParameter(ComponentName["SpecularGlossiness"][4], ComponentName["SpecularGlossiness"][4], ComponentDescription["SpecularGlossiness"][4], ComponentGH_ParamAccess["SpecularGlossiness"][4]);
            }            
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "Material", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //---<初期化>---//
            MaterialBuilder materialBuilder = new MaterialBuilder("");
            Vector4 normal = new Vector4();
            Vector4 occlussion = new Vector4();
            Vector4 emissive = new Vector4();

            //---<Shader別>---//
            if(ShaderType == 0)
            {
                //--<初期化>--//
                Vector4 baseColor = new Vector4();
                Vector4 metallicRoughness = new Vector4();

                if (!DA.GetData(0, ref normal)) return;
                if (!DA.GetData(1, ref occlussion)) return;
                if (!DA.GetData(2, ref emissive)) return;
                if (!DA.GetData(3, ref baseColor)) return;
                if (!DA.GetData(4, ref metallicRoughness)) return;

                //--<MaterialBuilderの設定>--//
                materialBuilder.WithDoubleSide(true);
                materialBuilder.WithMetallicRoughnessShader();
                materialBuilder.WithChannelParam(KnownChannel.Normal, KnownProperty.NormalScale, normal.X);
                materialBuilder.WithChannelParam(KnownChannel.Occlusion, KnownProperty.OcclusionStrength, occlussion.X);
                materialBuilder.WithChannelParam(KnownChannel.Emissive, KnownProperty.RGB, new Vector3(emissive.X, emissive.Y, emissive.Z));
                materialBuilder.WithChannelParam(KnownChannel.BaseColor, KnownProperty.RGBA, baseColor);
                materialBuilder.WithChannelParam(KnownChannel.MetallicRoughness, KnownProperty.MetallicFactor, metallicRoughness.X);
                materialBuilder.WithChannelParam(KnownChannel.MetallicRoughness, KnownProperty.RoughnessFactor, metallicRoughness.Y);
            }
            else if(ShaderType == 1)
            {
                //--<初期化>--//
                Vector4 diffuse = new Vector4();
                Vector4 specularGlossiness = new Vector4();

                if (!DA.GetData(0, ref normal)) return;
                if (!DA.GetData(1, ref occlussion)) return;
                if (!DA.GetData(2, ref emissive)) return;
                if (!DA.GetData(3, ref diffuse)) return;
                if (!DA.GetData(4, ref specularGlossiness)) return;

                //--<MaterialBuilderの設定>--//
                materialBuilder.WithDoubleSide(true);
                materialBuilder.WithMetallicRoughnessShader();
                materialBuilder.WithChannelParam(KnownChannel.Normal, KnownProperty.NormalScale, normal);
                materialBuilder.WithChannelParam(KnownChannel.Occlusion, KnownProperty.OcclusionStrength, occlussion);
                materialBuilder.WithChannelParam(KnownChannel.Emissive, KnownProperty.RGB, emissive);
                materialBuilder.WithChannelParam(KnownChannel.Diffuse, KnownProperty.RGBA, diffuse);
                materialBuilder.WithChannelParam(KnownChannel.SpecularGlossiness, KnownProperty.GlossinessFactor, specularGlossiness);
            }
            DA.SetData(0, materialBuilder);
        }

        //---<プロパティ>---//
        //--<Shader>--//
        /// <summary>
        /// 0 = MetallicRoughnessShader, 1 = SpecularGlossinessShader.
        /// </summary>
        public int ShaderType { get; set; }

        //--<inputリスト>--//
        public static readonly Dictionary<string, List<GH_ParamAccess>> ComponentGH_ParamAccess = new Dictionary<string, List<GH_ParamAccess>>
        {
            {
                "MetallicRoughness", new List<GH_ParamAccess>()
                {
                    GH_ParamAccess.item,
                    GH_ParamAccess.item,
                    GH_ParamAccess.item,
                    GH_ParamAccess.item,
                    GH_ParamAccess.item
                }
            },
            {
                "SpecularGlossiness", new List<GH_ParamAccess>()
                {
                    GH_ParamAccess.item,
                    GH_ParamAccess.item,
                    GH_ParamAccess.item,
                    GH_ParamAccess.item,
                    GH_ParamAccess.item
                }
            }
        };

        public static readonly Dictionary<string, List<string>> ComponentName = new Dictionary<string, List<string>>
        {
            {
                "MetallicRoughness", new List<string>()
                {
                    "Normal",
                    "Occlussion",
                    "Emissive",
                    "BaseColor",
                    "MetallicRoughness"
                }
            },
            {
                "SpecularGlossiness", new List<string>()
                {
                    "Normal",
                    "Occlussion",
                    "Emissive",
                    "Diffuse",
                    "SpecularGlossiness"
                }
            }
        };

        public static readonly Dictionary<string, List<string>> ComponentDescription = new Dictionary<string, List<string>>
        {
            {
                "MetallicRoughness", new List<string>()
                {
                    "X:Scale.",
                    "X:Strength.",
                    "X:Red, Y:Green, Z:Blue.",
                    "X:Red, Y:Green, Z:Blue, W:Alpha.",
                    "X:Metallic Factor, Y:Roughness Factor"
                }
            },
            {
                "SpecularGlossiness", new List<string>()
                {
                    "X:Scale.",
                    "X:Strength.",
                    "X:Red, Y:Green, Z:Blue.",
                    "X:Diffuse Red, Y:Diffuse Green, Z:Diffuse Blue, W:Alpha.",
                    "X:Specular Red, Y:Specular Green, Z:Specular Blue, W:Glossiness."
                }
            }
        };

        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetInt32("ShaderType", ShaderType);
            return base.Write(writer);
        }

        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            ShaderType = reader.GetInt32("ShaderType");
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
        public override Guid ComponentGuid => new Guid("FDE85C04-F9FF-4545-97C2-764DDF2D5938");        
    }
}