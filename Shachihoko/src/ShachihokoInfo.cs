using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Shachihoko
{
    public class ShachihokoInfo : GH_AssemblyInfo
    {
        public override string Name => "Shachihoko";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("87D6DE29-4709-4170-AE74-60CEB16A8D89");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}