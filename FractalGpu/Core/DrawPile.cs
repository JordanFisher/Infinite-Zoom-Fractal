using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using Drawing;

namespace FractalGpu
{
    public class DrawPile
    {
        public FancyVector2 FancyPos;
        public Vector2 Pos
        {
            get { return FancyPos.RelVal; }
            set { FancyPos.RelVal = value; }
        }

        List<EzText> MyTextList = new List<EzText>();
        List<QuadClass> MyQuadList = new List<QuadClass>();

        public PieceQuad Backdrop = null;

        public DrawPile()
        {
            FancyPos = new FancyVector2();
        }

        public DrawPile(FancyVector2 Center)
        {
            FancyPos = new FancyVector2(Center);
        }

        public void Add(QuadClass quad)
        {
            quad.MakeFancyPos();
            quad.FancyPos.SetCenter(FancyPos, true);
            MyQuadList.Add(quad);
        }
        
        public void Add(EzText text)
        {
            text.MakeFancyPos();
            text.FancyPos.SetCenter(FancyPos, true);
            MyTextList.Add(text);
        }

        public QuadClass FindQuad(string Name)
        {
            return QuadClass.FindQuad(MyQuadList, Name);
        }

        public Vector2 BackdropShift = Vector2.Zero;
        public void UpdateBackdrop(Vector2 TR_Shift, Vector2 BL_Shift)
        {
            if (Backdrop == null)
                Backdrop = new PieceQuad();

            //Backdrop.Clone(PieceQuad.Menu);
            Backdrop.Clone(PieceQuad.SpeechBubble);

            Vector2 TR = new Vector2(-10000000, -10000000);
            Vector2 BL = new Vector2(10000000, 10000000);
            foreach (QuadClass quad in MyQuadList)
            {
                quad.Update();
                TR = Vector2.Max(TR, quad.TR);
                BL = Vector2.Min(BL, quad.BL);
            }

            foreach (EzText text in MyTextList)
            {
                text.CalcBounds();
                TR = Vector2.Max(TR, text.TR);
                BL = Vector2.Min(BL, text.BL);
            }

            Vector2 Size = TR - BL + TR_Shift - BL_Shift;
            Backdrop.CalcQuads(Size / 2);
            BackdropShift = TR + TR_Shift - Size / 2;
        }

        public void Draw()
        {
            FancyPos.Update();

            if (Backdrop != null)
            {
                Backdrop.Base.Origin = FancyPos.AbsVal + BackdropShift;
                Backdrop.Draw();
            }

            foreach (QuadClass quad in MyQuadList) quad.Draw();
            Tools.QDrawer.Flush();

            foreach (EzText text in MyTextList) text.Draw(Tools.TheGame.MainCamera, false);
            Tools.EndSpriteBatch();
        }
    }
}