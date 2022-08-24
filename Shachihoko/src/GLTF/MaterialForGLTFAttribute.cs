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

        private Rectangle button0_Bounds { get; set; }
        private Rectangle button1_Bounds { get; set; }

        private GH_Capsule button0 { get; set; }
        private GH_Capsule button1 { get; set; }

        protected override void Layout()
        {
            base.Layout();

            Rectangle base_Rec = GH_Convert.ToRectangle(Bounds); /*余白をRectangleに変更し編集できるように*/
            base_Rec.Height += 42;
            base_Rec.Width = 62;

            int button_width = base_Rec.Width - 2;


            Rectangle button0_Rec = base_Rec;
            button0_Rec.Height = 20;
            button0_Rec.Width = button_width;
            button0_Rec.X = base_Rec.Left + 1;
            button0_Rec.Y = base_Rec.Bottom - 41;
            button0_Rec.Inflate(-1, -1);

            Rectangle button1_Rec = base_Rec;
            button1_Rec.Height = 20;
            button1_Rec.Width = button_width;
            button1_Rec.X = button0_Rec.Left + 1;
            button1_Rec.Y = base_Rec.Bottom - 21;
            button1_Rec.Inflate(-1, -1);

            Bounds = base_Rec;
            button0_Bounds = button0_Rec;
            button1_Bounds = button1_Rec;
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            switch (channel)
            {
                case GH_CanvasChannel.Objects:
                    base.RenderComponentCapsule(canvas, graphics, true, false, false, true, true, true);
                    MaterialForGLTFComponent materialForGLTFComponent = Owner as MaterialForGLTFComponent;

                    button0 = GH_Capsule.CreateTextCapsule(button0_Bounds, button0_Bounds, materialForGLTFComponent.ShaderType == 0 ? GH_Palette.Black : GH_Palette.White, "MetallicRoughness", 2, 0);
                    button0.Render(graphics, this.Selected, Owner.Locked, Owner.Hidden);
                    button0.Dispose();

                    button1 = GH_Capsule.CreateTextCapsule(button1_Bounds, button1_Bounds, materialForGLTFComponent.ShaderType == 1 ? GH_Palette.Black : GH_Palette.White, "SpecularGlossiness", 2, 0);
                    button1.Render(graphics, this.Selected, Owner.Locked, Owner.Hidden);
                    button1.Dispose();

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
                RectangleF rectangleF_0 = button0_Bounds;
                RectangleF rectangleF_1 = button1_Bounds;

                ///H///
                if (rectangleF_0.Contains(e.CanvasLocation))
                {
                    if (materialForGLTFComponent.ShaderType == 0)
                    {
                        return GH_ObjectResponse.Handled;
                    }
                    else
                    {
                        ResetInputSetting(materialForGLTFComponent, 0); //inputの初期化

                        ///inputの変更///

                        //inputリストの作成
                        List<GH_ParamAccess> gH_ParamAccesses = new List<GH_ParamAccess>();
                        List<string> names = new List<string>();
                        List<string> nickNames = new List<string>();
                        List<string> descriptions = new List<string>();
                        List<double> defaults = new List<double>();

                        gH_ParamAccesses.Add(GH_ParamAccess.item);
                        gH_ParamAccesses.Add(GH_ParamAccess.item);
                        gH_ParamAccesses.Add(GH_ParamAccess.item);
                        gH_ParamAccesses.Add(GH_ParamAccess.item);
                        gH_ParamAccesses.Add(GH_ParamAccess.item);
                        gH_ParamAccesses.Add(GH_ParamAccess.item);
                        gH_ParamAccesses.Add(GH_ParamAccess.item);
                        gH_ParamAccesses.Add(GH_ParamAccess.item);

                        names.Add("高さ");
                        names.Add("幅（上部横）");
                        names.Add("幅（下部横）");
                        names.Add("板厚（上部横）");
                        names.Add("板厚（下部横）");
                        names.Add("板厚（縦）");
                        names.Add("フィレット（上部交点）");
                        names.Add("フィレット（下部交点）");

                        nickNames.Add("高さ");
                        nickNames.Add("幅（上部横）");
                        nickNames.Add("幅（下部横）");
                        nickNames.Add("板厚（上部横）");
                        nickNames.Add("板厚（下部横）");
                        nickNames.Add("板厚（縦）");
                        nickNames.Add("フィレット（上部交点）");
                        nickNames.Add("フィレット（下部交点）");

                        descriptions.Add("高さ. [mm]");
                        descriptions.Add("幅（上部横）. [mm]");
                        descriptions.Add("幅（下部横）. [mm]");
                        descriptions.Add("板厚（上部横）. [mm]");
                        descriptions.Add("板厚（下部横）. [mm]");
                        descriptions.Add("板厚（縦）. [mm]");
                        descriptions.Add("フィレット（上部交点）. [mm]");
                        descriptions.Add("フィレット（下部交点）. [mm]");
                        //

                        //inputの反映
                        GH_Path path = new GH_Path(0);

                        for (int i = 0; i < names.Count; i++)
                        {
                            Param_Number input = new Param_Number();
                            input.Access = gH_ParamAccesses[i];
                            input.Name = names[i];
                            input.NickName = nickNames[i];
                            input.Description = descriptions[i];
                            input.SetPersistentData(path, defaults[i]);

                            materialForGLTFComponent.Params.RegisterInputParam(input, i + 1);
                        }

                        materialForGLTFComponent.ExpireSolution(true);
                        materialForGLTFComponent.Params.OnParametersChanged();
                        //
                        ///

                        return GH_ObjectResponse.Handled;
                    }
                }
                ///
                ///L///
                else if (rectangleF_1.Contains(e.CanvasLocation))
                {
                    if (materialForGLTFComponent.ShaderType == 1)
                    {
                        return GH_ObjectResponse.Handled;
                    }
                    else
                    {
                        ResetInputSetting(materialForGLTFComponent, 1); //inputの初期化

                        ///inputの変更///

                        //inputリストの作成
                        List<GH_ParamAccess> gH_ParamAccesses = new List<GH_ParamAccess>();
                        List<string> names = new List<string>();
                        List<string> nickNames = new List<string>();
                        List<string> descriptions = new List<string>();
                        List<double> defaults = new List<double>();

                        gH_ParamAccesses.Add(GH_ParamAccess.item);
                        gH_ParamAccesses.Add(GH_ParamAccess.item);
                        gH_ParamAccesses.Add(GH_ParamAccess.item);
                        gH_ParamAccesses.Add(GH_ParamAccess.item);
                        gH_ParamAccesses.Add(GH_ParamAccess.item);
                        gH_ParamAccesses.Add(GH_ParamAccess.item);
                        gH_ParamAccesses.Add(GH_ParamAccess.item);

                        names.Add("高さ");
                        names.Add("幅");
                        names.Add("板厚（横）");
                        names.Add("板厚（縦）");
                        names.Add("フィレット（横）");
                        names.Add("フィレット（縦）");
                        names.Add("フィレット（交点）");

                        nickNames.Add("高さ");
                        nickNames.Add("幅");
                        nickNames.Add("板厚（横）");
                        nickNames.Add("板厚（縦）");
                        nickNames.Add("フィレット（横）");
                        nickNames.Add("フィレット（縦）");
                        nickNames.Add("フィレット（交点）");

                        descriptions.Add("高さ. [mm]");
                        descriptions.Add("幅. [mm]");
                        descriptions.Add("板厚（横）. [mm]");
                        descriptions.Add("板厚（縦）. [mm]");
                        descriptions.Add("フィレット（横）. [mm]");
                        descriptions.Add("フィレット（縦）. [mm]");
                        descriptions.Add("フィレット（交点）. [mm]");
                        //

                        //inputの反映
                        GH_Path path = new GH_Path(0);

                        for (int i = 0; i < names.Count; i++)
                        {
                            Param_Number input = new Param_Number();
                            input.Access = gH_ParamAccesses[i];
                            input.Name = names[i];
                            input.NickName = nickNames[i];
                            input.Description = descriptions[i];
                            input.SetPersistentData(path, defaults[i]);

                            materialForGLTFComponent.Params.RegisterInputParam(input, i + 1);
                        }

                        materialForGLTFComponent.ExpireSolution(true);
                        materialForGLTFComponent.Params.OnParametersChanged();
                        //
                        ///

                        return GH_ObjectResponse.Handled;
                    }
                }
                ///
            }
            return base.RespondToMouseDown(sender, e);
        }

        //Inputの初期化
        private void ResetInputSetting(MaterialForGLTFComponent materialForGLTFComponent, int num)
        {
            materialForGLTFComponent.RecordUndoEvent("button" + num.ToString());
            materialForGLTFComponent.ShaderType = num;
            materialForGLTFComponent.ExpireSolution(true);
            for (int i = 1; i < materialForGLTFComponent.Params.Input.Count;)
            {
                materialForGLTFComponent.Params.UnregisterInputParameter(materialForGLTFComponent.Params.Input[i]);
            }
        }
    }
}
