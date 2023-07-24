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
                "Utility", "10_Utility"
            },
            {
                "Surface", "11_Surface"
            }
        };
    }
}
