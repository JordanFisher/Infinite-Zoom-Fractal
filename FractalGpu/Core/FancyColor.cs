using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.IO;

using Drawing;
using FractalGpu;

namespace Drawing
{
    public class FancyColor
    {
        FancyVector2 clr1, clr2;
        public Color CurColor
        {
            get
            {
                return ToColor(clr1.AbsVal, clr2.AbsVal);
            }
        }

        public void Release()
        {
            clr1.Release();
            clr2.Release();
        }

        public FancyColor()
        {
            clr1 = new FancyVector2();
            clr2 = new FancyVector2();
        }

        static Color ToColor(Vector2 v1, Vector2 v2)
        {
            return new Color(new Vector4(v1.X, v1.Y, v2.X, v2.Y));
        }
        static Vector2 Pair1(Vector4 v)
        {
            return new Vector2(v.X, v.Y);
        }
        static Vector2 Pair2(Vector4 v)
        {
            return new Vector2(v.Z, v.W);
        }

        public Color GetDest()
        {
            return ToColor(clr1.GetDest(), clr2.GetDest());
        }

        public void ToAndBack(Vector4 End, int Frames)
        {
            clr1.ToAndBack(Pair1(End), Frames);
            clr2.ToAndBack(Pair2(End), Frames);
        }
        public void ToAndBack(Vector4 Start, Vector4 End, int Frames)
        {
            clr1.ToAndBack(Pair1(Start), Pair1(End), Frames);
            clr2.ToAndBack(Pair2(Start), Pair2(End), Frames);
        }

        public void LerpTo(Vector4 End, int Frames)
        {
            clr1.LerpTo(Pair1(End), Frames);
            clr2.LerpTo(Pair2(End), Frames);
        }
        public void LerpTo(Vector4 Start, Vector4 End, int Frames)
        {
            clr1.LerpTo(Pair1(Start), Pair1(End), Frames);
            clr2.LerpTo(Pair2(Start), Pair2(End), Frames);
        }

        public Color Update()
        {
            return ToColor(clr1.Update(), clr2.Update());
        }
    }
}