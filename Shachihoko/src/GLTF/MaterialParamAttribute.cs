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
    public class MaterialParamAttribute: GH_ComponentAttributes //MaterialParamComponentのAttributesの変更
    {
        public MaterialParamAttribute(MaterialParamComponent owner) : base(owner)
        {

        }

        private Rectangle vector4Button_Bounds { get; set; }
        private Rectangle imageButton_Bounds { get; set; }

        private GH_Capsule vector4Button { get; set; }
        private GH_Capsule imageButton { get; set; }

        protected override void Layout()
        {
            base.Layout();

            Rectangle base_Rec = GH_Convert.ToRectangle(Bounds); //余白をRectangleに変更し編集できるように
            base_Rec.Height += 42;

            if (base_Rec.Width < 42)
            {
                base_Rec.Width = 42;
            }

            int button_width = base_Rec.Width - 2;

            Rectangle vector4Button_Rec = base_Rec;
            vector4Button_Rec.Height = 20;
            vector4Button_Rec.Width = button_width;
            vector4Button_Rec.X = base_Rec.Left + 1;
            vector4Button_Rec.Y = base_Rec.Bottom - 41;
            vector4Button_Rec.Inflate(-1, -1);

            Rectangle imageButton_Rec = base_Rec;
            imageButton_Rec.Height = 20;
            imageButton_Rec.Width = button_width;
            imageButton_Rec.X = base_Rec.Left + 1;
            imageButton_Rec.Y = base_Rec.Bottom - 21;
            imageButton_Rec.Inflate(-1, -1);

            Bounds = base_Rec;
            vector4Button_Bounds = vector4Button_Rec;
            imageButton_Bounds = imageButton_Rec;
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            switch (channel)
            {
                case GH_CanvasChannel.Objects:
                    base.RenderComponentCapsule(canvas, graphics, true, false, false, true, true, true);
                    MaterialParamComponent materialParam = Owner as MaterialParamComponent;

                    vector4Button = GH_Capsule.CreateTextCapsule(vector4Button_Bounds, vector4Button_Bounds, materialParam.ParamType == 0 ? GH_Palette.Black : GH_Palette.White, "Vector4", 2, 0);
                    vector4Button.Render(graphics, this.Selected, Owner.Locked, Owner.Hidden);
                    vector4Button.Dispose();

                    imageButton = GH_Capsule.CreateTextCapsule(imageButton_Bounds, imageButton_Bounds, materialParam.ParamType == 1 ? GH_Palette.Black : GH_Palette.White, "Image", 2, 0);
                    imageButton.Render(graphics, this.Selected, Owner.Locked, Owner.Hidden);
                    imageButton.Dispose();

                    break;
                default:
                    base.Render(canvas, graphics, channel);
                    break;
            }
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            MaterialParamComponent materialParam = Owner as MaterialParamComponent;

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                RectangleF vector4_RectangleF = vector4Button_Bounds;
                RectangleF image_RectangleF = imageButton_Bounds;

                if (vector4_RectangleF.Contains(e.CanvasLocation))
                {
                    if (materialParam.ParamType == 0)
                    {
                        return GH_ObjectResponse.Handled;
                    }
                    else
                    {
                        //--<inputの初期化>--//
                        ResetInputSetting(materialParam, 0);

                        //--<inputの反映>--//
                        GH_Path path = new GH_Path(0);

                        for (int i = 0; i < MaterialParamComponent.ComponentName["Vector4"].Count; i++)
                        {
                            Param_Number input = new Param_Number();
                            input.Access = MaterialParamComponent.ComponentGH_ParamAccess["Vector4"][i];
                            input.Name = MaterialParamComponent.ComponentName["Vector4"][i];
                            input.NickName = MaterialParamComponent.ComponentName["Vector4"][i];
                            input.Description = MaterialParamComponent.ComponentDescription["Vector4"][i];
                            input.SetPersistentData(MaterialParamComponent.ComponentDefault["Vector4"][i]);

                        materialParam.Params.RegisterInputParam(input, i);
                        }

                        materialParam.ExpireSolution(true);
                        materialParam.Params.OnParametersChanged();

                        return GH_ObjectResponse.Handled;
                    }
                }
                else if (image_RectangleF.Contains(e.CanvasLocation))
                {
                    if (materialParam.ParamType == 1)
                    {
                        return GH_ObjectResponse.Handled;
                    }
                    else
                    {
                        //--<inputの初期化>--//
                        ResetInputSetting(materialParam, 1);

                        //--<inputの反映>--//
                        GH_Path path = new GH_Path(0);

                        for (int i = 0; i < MaterialParamComponent.ComponentName["Image"].Count; i++)
                        {
                            if (i == 0)
                            {
                                Param_String input = new Param_String();
                                input.Access = MaterialParamComponent.ComponentGH_ParamAccess["Image"][i];
                                input.Name = MaterialParamComponent.ComponentName["Image"][i];
                                input.NickName = MaterialParamComponent.ComponentName["Image"][i];
                                input.Description = MaterialParamComponent.ComponentDescription["Image"][i];
                                materialParam.Params.RegisterInputParam(input, i);
                            }                            
                            else
                            {
                                Param_Number input = new Param_Number();
                                input.Access = MaterialParamComponent.ComponentGH_ParamAccess["Image"][i];
                                input.Name = MaterialParamComponent.ComponentName["Image"][i];
                                input.NickName = MaterialParamComponent.ComponentName["Image"][i];
                                input.Description = MaterialParamComponent.ComponentDescription["Image"][i];
                                input.SetPersistentData(1.0);
                                input.Optional = true;
                                materialParam.Params.RegisterInputParam(input, i);
                            }
                        }

                        materialParam.ExpireSolution(true);
                        materialParam.Params.OnParametersChanged();

                        return GH_ObjectResponse.Handled;
                    }
                }
            }
            return base.RespondToMouseDown(sender, e);
        }

        //Inputの初期化
        private void ResetInputSetting(MaterialParamComponent materialParamComponent, int num)
        {
            materialParamComponent.RecordUndoEvent("button" + num.ToString());
            materialParamComponent.ParamType = num;
            materialParamComponent.ExpireSolution(true);
            for (int i = 0; i < materialParamComponent.Params.Input.Count;)
            {
                materialParamComponent.Params.UnregisterInputParameter(materialParamComponent.Params.Input[i]);
            }
        }
    }
}
