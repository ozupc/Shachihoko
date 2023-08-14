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
    public class ExportGLTFAttribute : GH_ComponentAttributes //ExportGLTFComponentのAttributesの変更
    {
        public ExportGLTFAttribute(ExportGLTFComponent owner) : base(owner)
        {

        }

        private Rectangle StaticButton_Bounds { get; set; }
        private Rectangle AnimationButton_Bounds { get; set; }

        private GH_Capsule StaticButton { get; set; }
        private GH_Capsule AnimationButton { get; set; }

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

            Rectangle staticButton_Rec = base_Rec;
            staticButton_Rec.Height = 20;
            staticButton_Rec.Width = button_width;
            staticButton_Rec.X = base_Rec.Left + 1;
            staticButton_Rec.Y = base_Rec.Bottom - 41;
            staticButton_Rec.Inflate(-1, -1);

            Rectangle animationButton_Rec = base_Rec;
            animationButton_Rec.Height = 20;
            animationButton_Rec.Width = button_width;
            animationButton_Rec.X = base_Rec.Left + 1;
            animationButton_Rec.Y = base_Rec.Bottom - 21;
            animationButton_Rec.Inflate(-1, -1);

            Bounds = base_Rec;
            StaticButton_Bounds = staticButton_Rec;
            AnimationButton_Bounds = animationButton_Rec;
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            switch (channel)
            {
                case GH_CanvasChannel.Objects:
                    base.RenderComponentCapsule(canvas, graphics, true, false, false, true, true, true);
                    ExportGLTFComponent exportGLTFComponent = Owner as ExportGLTFComponent;

                    StaticButton = GH_Capsule.CreateTextCapsule(StaticButton_Bounds, StaticButton_Bounds, exportGLTFComponent.ExportStyle == 0 ? GH_Palette.Black : GH_Palette.Normal, "Static", 2, 0);
                    StaticButton.Render(graphics, this.Selected, Owner.Locked, Owner.Hidden);
                    StaticButton.Dispose();

                    AnimationButton = GH_Capsule.CreateTextCapsule(AnimationButton_Bounds, AnimationButton_Bounds, exportGLTFComponent.ExportStyle == 1 ? GH_Palette.Black : GH_Palette.Normal, "Animation", 2, 0);
                    AnimationButton.Render(graphics, this.Selected, Owner.Locked, Owner.Hidden);
                    AnimationButton.Dispose();

                    break;
                default:
                    base.Render(canvas, graphics, channel);
                    break;
            }
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            ExportGLTFComponent exportGLTFComponent = Owner as ExportGLTFComponent;

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                RectangleF static_RectangleF = StaticButton_Bounds;
                RectangleF animation_RectangleF = AnimationButton_Bounds;

                if (static_RectangleF.Contains(e.CanvasLocation))
                {
                    if (exportGLTFComponent.ExportStyle == 0)
                    {
                        return GH_ObjectResponse.Handled;
                    }
                    else
                    {
                        //--<inputの初期化>--//
                        ResetInputSetting(exportGLTFComponent, 0);

                        //--<inputの反映>--//
                        GH_Path path = new GH_Path(0);

                        for (int i = 0; i < ExportGLTFComponent.ComponentName["Static"].Count; i++)
                        {
                            AddParameter(exportGLTFComponent, i, ExportGLTFComponent.ComponentInputParam["Static"][i], ExportGLTFComponent.ComponentName["Static"][i], ExportGLTFComponent.ComponentName["Static"][i], ExportGLTFComponent.ComponentDescription["Static"][i], ExportGLTFComponent.ComponentGH_ParamAccess["Static"][i]);
                        }

                        exportGLTFComponent.ExpireSolution(true);
                        exportGLTFComponent.Params.OnParametersChanged();

                        return GH_ObjectResponse.Handled;
                    }
                }
                else if (animation_RectangleF.Contains(e.CanvasLocation))
                {
                    if (exportGLTFComponent.ExportStyle == 1)
                    {
                        return GH_ObjectResponse.Handled;
                    }
                    else
                    {
                        //--<inputの初期化>--//
                        ResetInputSetting(exportGLTFComponent, 1);

                        //--<inputの反映>--//
                        GH_Path path = new GH_Path(0);

                        for (int i = 0; i < ExportGLTFComponent.ComponentName["Animation"].Count; i++)
                        {
                            AddParameter(exportGLTFComponent, i, ExportGLTFComponent.ComponentInputParam["Animation"][i], ExportGLTFComponent.ComponentName["Animation"][i], ExportGLTFComponent.ComponentName["Animation"][i], ExportGLTFComponent.ComponentDescription["Animation"][i], ExportGLTFComponent.ComponentGH_ParamAccess["Animation"][i]);
                        }

                        exportGLTFComponent.ExpireSolution(true);
                        exportGLTFComponent.Params.OnParametersChanged();

                        return GH_ObjectResponse.Handled;
                    }
                }
            }
            return base.RespondToMouseDown(sender, e);
        }

        //Inputの初期化
        private void ResetInputSetting(ExportGLTFComponent exportGLTFComponent, int num)
        {
            exportGLTFComponent.RecordUndoEvent("button" + num.ToString());
            exportGLTFComponent.ExportStyle = num;
            exportGLTFComponent.ExpireSolution(true);
            for (int i = 0; i < exportGLTFComponent.Params.Input.Count;)
            {
                exportGLTFComponent.Params.UnregisterInputParameter(exportGLTFComponent.Params.Input[i]);
            }
        }

        private void AddParameter(ExportGLTFComponent exportGLTFComponent, int num, string inputParam, string name, string nickname, string description, GH_ParamAccess accessType)
        {
            IGH_Param param;

            switch (inputParam)
            {
                case "Generic":
                    param = new Param_GenericObject();
                    break;
                case "Geometry":
                    param = new Param_Geometry();
                    break;
                case "Text":
                    param = new Param_String();
                    break;
                case "Boolean":
                    param = new Param_Boolean();
                    break;
                case "Number":
                    param = new Param_Number();
                    break;
                case "Matrix":
                    param = new Param_Matrix();
                    break;
                default:
                    throw new ArgumentException($"Unsupported inputParam: {inputParam}");
            }

            ConfigureParam(param, name, nickname, description, accessType);
            exportGLTFComponent.Params.RegisterInputParam(param, num);
        }

        private void ConfigureParam(IGH_Param param, string name, string nickname, string description, GH_ParamAccess accessType)
        {
            param.Access = accessType;
            param.Name = name;
            param.NickName = nickname;
            param.Description = description;
            param.Optional = false;

            if (name == "FolderPath")
            {
                // 特定のパラメータタイプにキャストしてから初期値を設定
                var pathParam = param as GH_Param<GH_String>;
                if (pathParam != null)
                {
                    pathParam.ClearData();
                    pathParam.AddVolatileData(new GH_Path(0), 0, new GH_String(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)));
                }
            }
        }
    }
}
