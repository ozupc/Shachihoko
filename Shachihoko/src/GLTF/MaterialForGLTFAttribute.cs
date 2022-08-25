using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Numerics;
using System.Windows.Forms;

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
using Eto.Forms;

namespace Shachihoko
{
    public class MaterialForGLTFAttribute : GH_ComponentAttributes //MaterialForGLTFComponentのAttributesの変更
    {
        public MaterialForGLTFAttribute(MaterialForGLTFComponent owner) : base(owner)
        {

        }

        private Rectangle metallicRoughnessButton_Bounds { get; set; }
        private Rectangle specularGlossinessButton_Bounds { get; set; }

        private GH_Capsule metallicRoughnessButton { get; set; }
        private GH_Capsule specularGlossinessButton { get; set; }

        protected override void Layout()
        {
            base.Layout();

            Rectangle base_Rec = GH_Convert.ToRectangle(Bounds); //余白をRectangleに変更し編集できるように
            base_Rec.Height += 42;

            if (base_Rec.Width < 102)
            {
                base_Rec.Width = 102;
            }

            int button_width = base_Rec.Width - 2;

            Rectangle metallicRoughnessButton_Rec = base_Rec;
            metallicRoughnessButton_Rec.Height = 20;
            metallicRoughnessButton_Rec.Width = button_width;
            metallicRoughnessButton_Rec.X = base_Rec.Left + 1;
            metallicRoughnessButton_Rec.Y = base_Rec.Bottom - 41;
            metallicRoughnessButton_Rec.Inflate(-1, -1);

            Rectangle specularGlossinessButton_Rec = base_Rec;
            specularGlossinessButton_Rec.Height = 20;
            specularGlossinessButton_Rec.Width = button_width;
            specularGlossinessButton_Rec.X = base_Rec.Left + 1;
            specularGlossinessButton_Rec.Y = base_Rec.Bottom - 21;
            specularGlossinessButton_Rec.Inflate(-1, -1);

            Bounds = base_Rec;
            metallicRoughnessButton_Bounds = metallicRoughnessButton_Rec;
            specularGlossinessButton_Bounds = specularGlossinessButton_Rec;
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            switch (channel)
            {
                case GH_CanvasChannel.Objects:
                    base.RenderComponentCapsule(canvas, graphics, true, false, false, true, true, true);
                    MaterialForGLTFComponent materialForGLTFComponent = Owner as MaterialForGLTFComponent;

                    metallicRoughnessButton = GH_Capsule.CreateTextCapsule(metallicRoughnessButton_Bounds, metallicRoughnessButton_Bounds, materialForGLTFComponent.ShaderType == 0 ? GH_Palette.Black : GH_Palette.White, "MetallicRoughnessShader", 2, 0);
                    metallicRoughnessButton.Render(graphics, this.Selected, Owner.Locked, Owner.Hidden);
                    metallicRoughnessButton.Dispose();

                    specularGlossinessButton = GH_Capsule.CreateTextCapsule(specularGlossinessButton_Bounds, specularGlossinessButton_Bounds, materialForGLTFComponent.ShaderType == 1 ? GH_Palette.Black : GH_Palette.White, "SpecularGlossinessShader", 2, 0);
                    specularGlossinessButton.Render(graphics, this.Selected, Owner.Locked, Owner.Hidden);
                    specularGlossinessButton.Dispose();

                    break;
                default:
                    base.Render(canvas, graphics, channel);
                    break;
            }
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            MaterialForGLTFComponent materialForGLTFComponent = Owner as MaterialForGLTFComponent;

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                RectangleF metallicRoughness_RectangleF = metallicRoughnessButton_Bounds;
                RectangleF specularGlossiness_RectangleF = specularGlossinessButton_Bounds;

                if (metallicRoughness_RectangleF.Contains(e.CanvasLocation))
                {
                    if (materialForGLTFComponent.ShaderType == 0)
                    {
                        return GH_ObjectResponse.Handled;
                    }
                    else
                    {
                        //--<inputの初期化>--//
                        ResetInputSetting(materialForGLTFComponent, 0);

                        //--<inputの反映>--//
                        GH_Path path = new GH_Path(0);

                        for (int i = 0; i < MaterialForGLTFComponent.ComponentName["MetallicRoughness"].Count; i++)
                        {
                            Param_GenericObject input = new Param_GenericObject();
                            input.Access = MaterialForGLTFComponent.ComponentGH_ParamAccess["MetallicRoughness"][i];
                            input.Name = MaterialForGLTFComponent.ComponentName["MetallicRoughness"][i];
                            input.NickName = MaterialForGLTFComponent.ComponentName["MetallicRoughness"][i];
                            input.Description = MaterialForGLTFComponent.ComponentDescription["MetallicRoughness"][i];
                            input.Optional = true;

                            materialForGLTFComponent.Params.RegisterInputParam(input, i);
                        }

                        materialForGLTFComponent.ExpireSolution(true);
                        materialForGLTFComponent.Params.OnParametersChanged();

                        return GH_ObjectResponse.Handled;
                    }
                }
                else if (specularGlossiness_RectangleF.Contains(e.CanvasLocation))
                {
                    if (materialForGLTFComponent.ShaderType == 1)
                    {
                        return GH_ObjectResponse.Handled;
                    }
                    else
                    {
                        //--<inputの初期化>--//
                        ResetInputSetting(materialForGLTFComponent, 1);

                        //--<inputの反映>--//
                        GH_Path path = new GH_Path(0);

                        for (int i = 0; i < MaterialForGLTFComponent.ComponentName["SpecularGlossiness"].Count; i++)
                        {
                            Param_GenericObject input = new Param_GenericObject();
                            input.Access = MaterialForGLTFComponent.ComponentGH_ParamAccess["SpecularGlossiness"][i];
                            input.Name = MaterialForGLTFComponent.ComponentName["SpecularGlossiness"][i];
                            input.NickName = MaterialForGLTFComponent.ComponentName["SpecularGlossiness"][i];
                            input.Description = MaterialForGLTFComponent.ComponentDescription["SpecularGlossiness"][i];
                            input.Optional = true;

                            materialForGLTFComponent.Params.RegisterInputParam(input, i);
                        }

                        materialForGLTFComponent.ExpireSolution(true);
                        materialForGLTFComponent.Params.OnParametersChanged();

                        return GH_ObjectResponse.Handled;
                    }
                }
            }
            return base.RespondToMouseDown(sender, e);
        }

        //Inputの初期化
        private void ResetInputSetting(MaterialForGLTFComponent materialForGLTFComponent, int num)
        {
            materialForGLTFComponent.RecordUndoEvent("button" + num.ToString());
            materialForGLTFComponent.ShaderType = num;
            materialForGLTFComponent.ExpireSolution(true);
            for (int i = 0; i < materialForGLTFComponent.Params.Input.Count;)
            {
                materialForGLTFComponent.Params.UnregisterInputParameter(materialForGLTFComponent.Params.Input[i]);
            }
        }
    }
}
