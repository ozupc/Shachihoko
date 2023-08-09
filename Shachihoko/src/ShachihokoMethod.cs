using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shachihoko
{
    internal class ShachihokoMethod
    {
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
