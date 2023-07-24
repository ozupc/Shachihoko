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
    internal class MaterialParam
    {
        public MaterialParam()
        {

        }

        public Vector4 Vector4 { get; set; }

        public MemoryImage MemoryImage { get; set; }

        /// <summary>
        /// Normal要素のScaleやOcclusion要素のStrengthで使用する.
        /// </summary>
        public float OptionalNumber { get; set; }

        /// <summary>
        /// 0 = Vector4, 1 = Image.
        /// </summary>
        public int ParamStyle { get; set; }

        /// <summary>
        /// 0 = Normal, 1 = Occlusion, 2 = Emissive, 3 = BaseColor, 4 = MetallicRoughness, 5 = Diffuse, 6 = SpecularGlossiness.
        /// </summary>
        public int ParamType { get; set; }
    }
}
