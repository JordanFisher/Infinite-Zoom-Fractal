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

using FractalGpu;

namespace FractalGpu
{
    public class QuadClass
    {
        public static QuadClass FindQuad(List<QuadClass> list, string Name)
        {
            return null;
            //return list.Find(delegate(QuadClass quad) { return string.Compare(quad.Name, Name, true) == 0; });
        }

        public bool HitTest(Vector2 pos) { return HitTest(pos, Vector2.Zero); }
        public bool HitTest(Vector2 pos, Vector2 padding)
        {
            Update();
            
            if (pos.X > TR.X + padding.X) return false;
            if (pos.X < BL.X - padding.X) return false;
            if (pos.Y > TR.Y + padding.Y) return false;
            if (pos.Y < BL.Y - padding.Y) return false;

            return true;
        }

        public Vector2 TR { get { return Quad.v1.Vertex.xy; } }
        public Vector2 BL { get { return Quad.v2.Vertex.xy; } }

        public bool Shadow = false;
        public Vector2 ShadowOffset = Vector2.Zero;
        public Color ShadowColor = Color.Black;

        public SimpleQuad Quad;
        
        public BasePoint Base;
        public FancyVector2 FancyPos, FancyScale, FancyAngle;

        public string Name = "";

        public void MakeFancyPos()
        {
            if (FancyPos != null) FancyPos.Release();
            FancyPos = new FancyVector2();
            FancyPos.RelVal = Base.Origin;
        }

        public void Release()
        {
            if (FancyPos != null) FancyPos.Release(); FancyPos = null;
            if (FancyScale != null) FancyScale.Release(); FancyScale = null;
            if (FancyAngle != null) FancyAngle.Release(); FancyAngle = null;
        }

        public Vector2 Pos
        {
            get
            {
                if (FancyPos == null)
                    return Base.Origin;
                else
                    return FancyPos.RelVal;
            }
            set
            {
                if (FancyPos == null)
                    Base.Origin = value;
                else
                    FancyPos.RelVal = value;
            }
        }

        public Vector2 Size
        {
            get
            {
                if (FancyScale == null)
                    return new Vector2(Base.e1.X, Base.e2.Y);
                else
                    return FancyScale.RelVal;
            }
            set
            {
                if (FancyScale == null)
                {
                    Base.e1.X = value.X;
                    Base.e2.Y = value.Y;
                }
                else
                    FancyScale.RelVal = value;
            }
        }


        public QuadClass()
        {
            Initialize(null, false, false);
        }
        public QuadClass(FancyVector2 Center)
        {
            Initialize(Center, false, false);
        }
        public QuadClass(FancyVector2 Center, bool UseFancySize)
        {
            Initialize(Center, UseFancySize, false);
        }
        public QuadClass(FancyVector2 Center, bool UseFancySize, bool UseFancyAngle)
        {
            Initialize(Center, UseFancySize, UseFancyAngle);
        }

        public void Initialize(FancyVector2 Center, bool UseFancySize, bool UseFancyAngle)
        {
            Quad.Init();
            Base.Init();

            Init();

            if (Center != null)
            {
                FancyPos = new FancyVector2();
                FancyPos.Center = Center;
            }

            if (UseFancySize)
            {
                FancyScale = new FancyVector2();
            }

            if (UseFancyAngle)
            {
                FancyAngle = new FancyVector2();
            }
        }

        /// <summary>
        /// The name of the quad's texture. Setting will automatically search the TextureWad for a matching texture.
        /// </summary>
        public string TextureName
        {
            get { return Quad.MyTexture.Name; }
            set { Quad.MyTexture = Tools.TextureWad.FindByName(value); }
        }

        /// <summary>
        /// The name of the quad's Effect. Setting will automatically search the EffectWad for a matching Effect.
        /// </summary>
        public string EffectName
        {
            get { return Quad.MyEffect.Name; }
            set { Quad.MyEffect = Tools.EffectWad.FindByName(value); }
        }

        public void Clone(QuadClass quad)
        {
            quad.Quad.MyTexture = Quad.MyTexture;
            quad.Quad.MyEffect = Quad.MyEffect;
            quad.Base = Base;
        }

        public void SetTexture(string Name)
        {
            Quad.MyTexture = Tools.TextureWad.FindByName(Name);
        }

        public void ScaleToTextureSize()
        {
            if (Quad.MyTexture != null)
            {
                Size = new Vector2(Quad.MyTexture.Tex.Width, Quad.MyTexture.Tex.Height);
                //Base.e1 = new Vector2(Quad.MyTexture.Tex.Width, 0);
                //Base.e2 = new Vector2(0, Quad.MyTexture.Tex.Height);
            }
        }

        public void Scale(float scale)
        {
            Size *= scale;
            //Base.e1 *= scale;
            //Base.e2 *= scale;
        }

        public void ScaleXToMatchRatio()
        {
            if (Quad.MyTexture.Load())
                //Base.e1.X = Base.e2.Y * Quad.MyTexture.Tex.Width / Quad.MyTexture.Tex.Height;
                Size = new Vector2(Size.Y * Quad.MyTexture.Tex.Width / Quad.MyTexture.Tex.Height, Size.Y);
        }

        public void ScaleYToMatchRatio()
        {
            if (Quad.MyTexture.Load())
                //Base.e2.Y = Base.e1.X * Quad.MyTexture.Tex.Height / Quad.MyTexture.Tex.Width;
                Size = new Vector2(Size.X, Size.X * Quad.MyTexture.Tex.Height / Quad.MyTexture.Tex.Width);
        }

        public void RepeatY()
        {
            float V = (Size.Y / Quad.MyTexture.Tex.Height) / (Size.X / Quad.MyTexture.Tex.Width);
            Quad.UVFromBounds(Vector2.Zero, new Vector2(1, V));
            Quad.V_Wrap = true;
        }

        public void PointxAxisTo(float Angle)
        {
            Vector2 Dir = new Vector2((float)Math.Cos(Angle), (float)Math.Sin(Angle));
            PointxAxisTo(Dir);
        }

        public void PointxAxisTo(Vector2 Dir)
        {
            Tools.PointxAxisTo(ref Base.e1, ref Base.e2, Dir);
        }

        public void FullScreen(Camera cam)
        {
            //Base.e1.X = (cam.TR.X - cam.BL.X) / 2;
            //Base.e2.Y = (cam.TR.Y - cam.BL.Y) / 2;
            //Base.Origin = cam.Data.Position;
            Size = new Vector2((cam.TR.X - cam.BL.X) / 2, (cam.TR.Y - cam.BL.Y) / 2);
            Pos = cam.Data.Position;
        }

        public void FromBounds(Vector2 BL, Vector2 TR)
        {
            //Base.Origin = (TR + BL) / 2;
            //Base.e1 = new Vector2((TR.X - BL.X) / 2, 0);
            //Base.e2 = new Vector2(0, (TR.Y - BL.Y) / 2);
            Size = new Vector2((TR.X - BL.X) / 2, (TR.Y - BL.Y) / 2);
            Pos = (TR + BL) / 2;
        }

        public void Init()
        {
            Quad.MyEffect = Tools.EffectWad.FindByName("Basic");
            Quad.MyTexture = Tools.TextureWad.FindByName("White");
        }

        public void Draw() { Draw(true); }
        public void Draw(bool Update)
        {
            if (FancyPos != null)
                Base.Origin = FancyPos.Update();

            if (FancyAngle != null)
            {
                float Angle = FancyAngle.Update().X;
                PointxAxisTo(Angle);
            }

            if (FancyScale != null)
            {
                Vector2 scale = FancyScale.Update();
                Base.e1.X = scale.X;
                Base.e2.Y = scale.Y;
            }

            if (Shadow)
            {
                Quad.MyEffect = Tools.EffectWad.EffectList[1];
                Base.Origin -= ShadowOffset;
                Quad.SetColor(ShadowColor);
                Quad.Update(ref Base);
                Tools.QDrawer.DrawQuad(Quad);
                Tools.QDrawer.Flush();
                Quad.MyEffect = Tools.EffectWad.EffectList[0];
                Base.Origin += ShadowOffset;
                Quad.SetColor(Color.White);
            }

            if (Update)
                Quad.Update(ref Base);
            Tools.QDrawer.DrawQuad(Quad);
        }

        public void Update()
        {
            Quad.Update(ref Base);
        }
    }
}