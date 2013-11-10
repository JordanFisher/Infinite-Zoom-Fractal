﻿using System;
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
    public struct PieceQuadData
    {
        public Vector2 Left_BL_Shift, Right_BL_Shift, Top_BL_Shift, Bottom_BL_Shift, TR_BL_Shift, TL_BL_Shift, BL_BL_Shift, BR_BL_Shift, Center_BL_Shift;
        public Vector2 Left_TR_Shift, Right_TR_Shift, Top_TR_Shift, Bottom_TR_Shift, TR_TR_Shift, TL_TR_Shift, BL_TR_Shift, BR_TR_Shift, Center_TR_Shift;
        
        public float LeftWidth, RightWidth, TopWidth, BottomWidth;
        public float RepeatWidth, RepeatHeight;

        public Vector2 UV_Multiples;
        public bool MiddleOnly; // True if we only have a top, a middle, and a bottom
        public bool CenterOnly; // True if we only have a center
    }

    public class PieceQuad
    {
        public bool Shadow = false;
        public Vector2 ShadowOffset = Vector2.Zero;
        public Color ShadowColor = Color.Black;

        public enum DrawOrder { CenterOnTop, CenterOnBottom };
        public DrawOrder MyDrawOrder;

        public static PieceQuad BrickWall, BrickPillar_Small, BrickPillar_Medium, BrickPillar_Large, BrickPillar_LargePlus, BrickPillar_Xlarge,
                                Floating_Small, Floating_Medium, Floating_Large, Floating_Xlarge,
                                MovingBlock,
                                Castle,
                                OutsideBlock, TileBlock, Cement,
                                Outside_Smallest, Outside_Smaller, Outside_Small, Outside_Medium, Outside_Large, Outside_XLarge,
                                Inside2_Block, Inside2_Thin,
                                Inside2_Smallest, Inside2_Smaller, Inside2_Small, Inside2_Medium, Inside2_Large, Inside2_XLarge,
                                Inside2_Pillar_Smallest, Inside2_Pillar_Smaller, Inside2_Pillar_Small, Inside2_Pillar_Medium, Inside2_Pillar_Large, Inside2_Pillar_XLarge,
                                SpeechBubble, SpeechBubbleRed,
                                Menu, CharMenu, CharMenu_Top, FreePlayMenu, TitleMenuPieces;

        public SimpleQuad Left, Right, Top, Bottom, TR, TL, BL, BR, Center;

        public bool BottomUp;

        public Vector2 TL_UV, BR_UV;

        public PieceQuadData Data;

        public Color MyColor;

        public BasePoint Base;

        public void Clone(PieceQuad PieceQuadA)
        {
            Data = PieceQuadA.Data;

            MyDrawOrder = PieceQuadA.MyDrawOrder;

            Left = PieceQuadA.Left;
            Right = PieceQuadA.Right;
            Top = PieceQuadA.Top;
            Bottom = PieceQuadA.Bottom;
            TR = PieceQuadA.TR;
            TL = PieceQuadA.TL;
            BL = PieceQuadA.BL;
            BR = PieceQuadA.BR;
            Center = PieceQuadA.Center;

            TL_UV = PieceQuadA.TL_UV;
            BR_UV = PieceQuadA.BR_UV;

            Base = PieceQuadA.Base;

            SetColor(PieceQuadA.MyColor);
        }
        
        public PieceQuad(String file)
        {
            InitAll();
            Init(null, Tools.EffectWad.FindByName("Basic"));

            Read(file);
        }

        /// <summary>
        /// Returns whether the line has a '?' in it
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        bool IsQM(string line)
        {
            if (line.Contains('?')) return true;
            return false;
        }

        public void Read(String file)
        {
            FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None);
            StreamReader reader = new StreamReader(stream);// (stream, Encoding.UTF8);

            String line;
            String type;

            line = reader.ReadLine();
            while (line != null)
            {
                line = Tools.RemoveComment(line);

                if (line.StartsWith("["))
                {
                    int EndIndex = line.IndexOf("]");
                    type = line.Substring(1, EndIndex - 1);
                    line = line.Substring(EndIndex + 1, line.Length - EndIndex - 1);
                    switch (type)
                    {
                        case "Color": SetColor(Tools.ParseToColor(line)); break;

                        case "DrawOrder": MyDrawOrder = (DrawOrder)int.Parse(line); break;

                        case "MiddleOnly": Data.MiddleOnly = bool.Parse(line); break;
                        case "CenterOnly": Data.CenterOnly = bool.Parse(line); break;
                            
                        case "RepeatWidth": Data.RepeatWidth = float.Parse(line); break;
                        case "RepeatHeight": Data.RepeatHeight = float.Parse(line); break;
                        case "LeftWidth": if (IsQM(line)) Data.LeftWidth = Left.MyTexture.Tex.Width; else Data.LeftWidth = float.Parse(line); break;
                        case "RightWidth": if (IsQM(line)) Data.RightWidth = Right.MyTexture.Tex.Width; else Data.RightWidth = float.Parse(line); break;
                        case "TopWidth": if (IsQM(line)) Data.TopWidth = Top.MyTexture.Tex.Height; else Data.TopWidth = float.Parse(line); break;
                        case "BottomWidth": if (IsQM(line)) Data.BottomWidth = Bottom.MyTexture.Tex.Height; else Data.BottomWidth = float.Parse(line); break;

                        case "Left_Hide": Left.Hide = bool.Parse(line); break;
                        case "Left_U_Wrap": Left.U_Wrap = bool.Parse(line); break;
                        case "Left_V_Wrap": Left.V_Wrap = bool.Parse(line); break;                        
                        case "Left_BL_Shift": Data.Left_BL_Shift = Tools.ParseToVector2(line); break;
                        case "Left_TR_Shift": Data.Left_TR_Shift = Tools.ParseToVector2(line); break;
                        case "Left_Texture": Left.MyTexture = Tools.TextureWad.FindByName(Tools.ParseToFileName(line)); break;

                        case "Right_U_Wrap": Right.U_Wrap = bool.Parse(line); break;
                        case "Right_Hide": Right.Hide = bool.Parse(line); break;
                        case "Right_V_Wrap": Right.V_Wrap = bool.Parse(line); break;
                        case "Right_BL_Shift": Data.Right_BL_Shift = Tools.ParseToVector2(line); break;
                        case "Right_TR_Shift": Data.Right_TR_Shift = Tools.ParseToVector2(line); break;
                        case "Right_Texture": Right.MyTexture = Tools.TextureWad.FindByName(Tools.ParseToFileName(line)); break;

                        case "Center_U_Wrap": Center.U_Wrap = bool.Parse(line); break;
                        case "Center_Hide": Center.Hide = bool.Parse(line); break;
                        case "Center_V_Wrap": Center.V_Wrap = bool.Parse(line); break; 
                        case "Center_BL_Shift": Data.Center_BL_Shift = Tools.ParseToVector2(line); break;
                        case "Center_TR_Shift": Data.Center_TR_Shift = Tools.ParseToVector2(line); break;
                        case "Center_Texture": Center.MyTexture = Tools.TextureWad.FindByName(Tools.ParseToFileName(line)); break;

                        case "Top_U_Wrap": Top.U_Wrap = bool.Parse(line); break;
                        case "Top_Hide": Top.Hide = bool.Parse(line); break;
                        case "Top_V_Wrap": Top.V_Wrap = bool.Parse(line); break;
                        case "Top_BL_Shift": Data.Top_BL_Shift = Tools.ParseToVector2(line); break;
                        case "Top_TR_Shift": Data.Top_TR_Shift = Tools.ParseToVector2(line); break;
                        case "Top_Texture": Top.MyTexture = Tools.TextureWad.FindByName(Tools.ParseToFileName(line)); break;

                        case "Bottom_U_Wrap": Bottom.U_Wrap = bool.Parse(line); break;
                        case "Bottom_Hide": Bottom.Hide = bool.Parse(line); break;
                        case "Bottom_V_Wrap": Bottom.V_Wrap = bool.Parse(line); break;
                        case "Bottom_BL_Shift": Data.Bottom_BL_Shift = Tools.ParseToVector2(line); break;
                        case "Bottom_TR_Shift": Data.Bottom_TR_Shift = Tools.ParseToVector2(line); break;
                        case "Bottom_Texture": Bottom.MyTexture = Tools.TextureWad.FindByName(Tools.ParseToFileName(line)); break;

                        case "TL_U_Wrap": TL.U_Wrap = bool.Parse(line); break;
                        case "TL_Hide": TL.Hide = bool.Parse(line); break;
                        case "TL_V_Wrap": TL.V_Wrap = bool.Parse(line); break;
                        case "TL_BL_Shift": Data.TL_BL_Shift = Tools.ParseToVector2(line); break;
                        case "TL_TR_Shift": Data.TL_TR_Shift = Tools.ParseToVector2(line); break;
                        case "TL_Texture": TL.MyTexture = Tools.TextureWad.FindByName(Tools.ParseToFileName(line)); break;

                        case "TR_U_Wrap": TR.U_Wrap = bool.Parse(line); break;
                        case "TR_Hide": TR.Hide = bool.Parse(line); break;
                        case "TR_V_Wrap": TR.V_Wrap = bool.Parse(line); break;
                        case "TR_BL_Shift": Data.TR_BL_Shift = Tools.ParseToVector2(line); break;
                        case "TR_TR_Shift": Data.TR_TR_Shift = Tools.ParseToVector2(line); break;
                        case "TR_Texture": TR.MyTexture = Tools.TextureWad.FindByName(Tools.ParseToFileName(line)); break;

                        case "BL_U_Wrap": BL.U_Wrap = bool.Parse(line); break;
                        case "BL_Hide": BL.Hide = bool.Parse(line); break;
                        case "BL_V_Wrap": BL.V_Wrap = bool.Parse(line); break;
                        case "BL_BL_Shift": Data.BL_BL_Shift = Tools.ParseToVector2(line); break;
                        case "BL_TR_Shift": Data.BL_TR_Shift = Tools.ParseToVector2(line); break;
                        case "BL_Texture": BL.MyTexture = Tools.TextureWad.FindByName(Tools.ParseToFileName(line)); break;

                        case "BR_U_Wrap": BR.U_Wrap = bool.Parse(line); break;
                        case "BR_Hide": BR.Hide = bool.Parse(line); break;
                        case "BR_V_Wrap": BR.V_Wrap = bool.Parse(line); break;
                        case "BR_BL_Shift": Data.BR_BL_Shift = Tools.ParseToVector2(line); break;
                        case "BR_TR_Shift": Data.BR_TR_Shift = Tools.ParseToVector2(line); break;
                        case "BR_Texture": BR.MyTexture = Tools.TextureWad.FindByName(Tools.ParseToFileName(line)); break;

                        case "TextureRoot": SetTexture(Tools.ParseToFileName(line), Data.MiddleOnly); break;

                        case "UV_Multiples": Data.UV_Multiples = Tools.ParseToVector2(line); break;

                        default: throw (new Exception("Unknown variable type in .boxes file"));
                    }
                }

                line = reader.ReadLine();
            }

            reader.Close();
            stream.Close();
        }

        public PieceQuad()
        {
            InitAll();
        }

        void InitAll()
        {
            Left = new SimpleQuad();
            Right = new SimpleQuad();
            Top = new SimpleQuad();
            Bottom = new SimpleQuad();
            TR = new SimpleQuad();
            TL = new SimpleQuad();
            BL = new SimpleQuad();
            BR = new SimpleQuad();
            Center = new SimpleQuad();

            Left.Init();
            Right.Init();
            Top.Init();
            Bottom.Init();
            TR.Init();
            TL.Init();
            BL.Init();
            BR.Init();
            Center.Init();

            Base.Init();            
        }

        public void SetTexture(String root) { SetTexture(root, false); }
        public void SetTexture(String root, bool MiddleOnly)
        {
            this.Data.MiddleOnly = MiddleOnly;

            Center.MyTexture = Tools.TextureWad.FindByName(root + "_middle");
            Bottom.MyTexture = Tools.TextureWad.FindByName(root + "_bottom");
            Top.MyTexture = Tools.TextureWad.FindByName(root + "_top");

            if (!MiddleOnly)
            {
                Right.MyTexture = Tools.TextureWad.FindByName(root + "_rightside");
                Left.MyTexture = Tools.TextureWad.FindByName(root + "_leftside");
                TR.MyTexture = Tools.TextureWad.FindByName(root + "_topright");
                TL.MyTexture = Tools.TextureWad.FindByName(root + "_topleft");
                BR.MyTexture = Tools.TextureWad.FindByName(root + "_bottomright");
                BL.MyTexture = Tools.TextureWad.FindByName(root + "_bottomleft");
            }            
        }

        public void SetAlpha(float Alpha)
        {
            Vector4 clr = MyColor.ToVector4();
            clr.W = Alpha;
            SetColor(new Color(clr));
        }
        public void SetColor(Color color)
        {
            MyColor = color;

            Left.SetColor(color);
            Right.SetColor(color);
            Top.SetColor(color);
            Bottom.SetColor(color);
            TR.SetColor(color);
            TL.SetColor(color);
            BL.SetColor(color);
            BR.SetColor(color);
            Center.SetColor(color);
        }

        public void Init(EzTexture tex, EzEffect fx)
        {
            Data.Left_BL_Shift = Data.Right_BL_Shift = Data.Top_BL_Shift = Data.Bottom_BL_Shift = Data.TR_BL_Shift = Data.TL_BL_Shift = Data.BL_BL_Shift = Data.BR_BL_Shift = Data.Center_BL_Shift = Vector2.Zero;
            Data.Left_TR_Shift = Data.Right_TR_Shift = Data.Top_TR_Shift = Data.Bottom_TR_Shift = Data.TR_TR_Shift = Data.TL_TR_Shift = Data.BL_TR_Shift = Data.BR_TR_Shift = Data.Center_TR_Shift = Vector2.Zero;

            Data.LeftWidth = Data.RightWidth = Data.TopWidth = Data.BottomWidth = 0;
            Data.RepeatWidth = Data.RepeatHeight = 0;

            TL_UV = BR_UV = Vector2.Zero;

            Left.U_Wrap = false;
            Right.U_Wrap = false;
            Top.U_Wrap = false;
            Bottom.U_Wrap = false;
            TR.U_Wrap = false;
            TL.U_Wrap = false;
            BL.U_Wrap = false;
            BR.U_Wrap = false;
            Center.U_Wrap = false;

            Left.V_Wrap = false;
            Right.V_Wrap = false;
            Top.V_Wrap = false;
            Bottom.V_Wrap = false;
            TR.V_Wrap = false;
            TL.V_Wrap = false;
            BL.V_Wrap = false;
            BR.V_Wrap = false;
            Center.V_Wrap = false;




            Left.MyTexture = tex;
            Right.MyTexture = tex;
            Top.MyTexture = tex;
            Bottom.MyTexture = tex;
            TR.MyTexture = tex;
            TL.MyTexture = tex;
            BL.MyTexture = tex;
            BR.MyTexture = tex;
            Center.MyTexture = tex;

            Left.MyEffect = fx;
            Right.MyEffect = fx;
            Top.MyEffect = fx;
            Bottom.MyEffect = fx;
            TR.MyEffect = fx;
            TL.MyEffect = fx;
            BL.MyEffect = fx;
            BR.MyEffect = fx;
            Center.MyEffect = fx;

            Center.U_Wrap = Center.V_Wrap = true;
            Left.V_Wrap = Right.V_Wrap = true;
            Top.U_Wrap = Bottom.U_Wrap = true;

            Data.UV_Multiples = new Vector2(1, 1);
        }

        public void CalcQuads(Vector2 Size)
        {            
            float x1, x2, x3, x4, y1, y2, y3, y4;
            x1 = -Size.X;
            x2 = x1 + Data.LeftWidth;
            x4 = Size.X;
            x3 = x4 - Data.RightWidth;
            y1 = -Size.Y;
            y2 = y1 + Data.BottomWidth;
            y4 = Size.Y;
            y3 = y4 - Data.TopWidth;

            Vector2 SmallShift = new Vector2(1f, 1f);
            BL.FromBounds(new Vector2(x1, y1) + Data.BL_BL_Shift, new Vector2(x2-1, y2-1) + Data.BL_TR_Shift + SmallShift);
            Left.FromBounds(new Vector2(x1, y2) + Data.Left_BL_Shift, new Vector2(x2-1, y3-1) + Data.Left_TR_Shift + SmallShift);
            TL.FromBounds(new Vector2(x1, y3) + Data.TL_BL_Shift, new Vector2(x2-1, y4) + Data.TL_TR_Shift + SmallShift);

            Bottom.FromBounds(new Vector2(x2, y1) + Data.Bottom_BL_Shift, new Vector2(x3-1, y2-1) + Data.Bottom_TR_Shift + SmallShift);
            Center.FromBounds(new Vector2(x2, y2) + Data.Center_BL_Shift, new Vector2(x3-1, y3-1) + Data.Center_TR_Shift + SmallShift);
            Top.FromBounds(new Vector2(x2, y3) + Data.Top_BL_Shift, new Vector2(x3-1, y4) + Data.Top_TR_Shift + SmallShift);

            BR.FromBounds(new Vector2(x3, y1) + Data.BR_BL_Shift, new Vector2(x4, y2-1) + Data.BR_TR_Shift + SmallShift);
            Right.FromBounds(new Vector2(x3, y2) + Data.Right_BL_Shift, new Vector2(x4, y3-1) + Data.Right_TR_Shift + SmallShift);
            TR.FromBounds(new Vector2(x3, y3) + Data.TR_BL_Shift, new Vector2(x4, y4) + Data.TR_TR_Shift + SmallShift);


            //int U = Math.Max(1, (int)((x3 - x2) / RepeatWidth));            
            
            float U, V;

            if (Data.UV_Multiples.X == 0)
                U = (x3 - x2) / Data.RepeatWidth;
            else
            {
                U = (x3 - x2) / Data.RepeatWidth;
                U = Math.Max(1, (int)(U / Data.UV_Multiples.X)) * Data.UV_Multiples.X;
            }

            if (Data.UV_Multiples.Y == 0)
                V = (y3 - y2) / Data.RepeatHeight;
            else
            {
                V = (y3 - y2) / Data.RepeatHeight;
                V = Math.Max(1, (int)(V / Data.UV_Multiples.Y)) * Data.UV_Multiples.Y;
            }

            /*
            if (Sub1VWraps)
                V = (y3 - y2) / RepeatHeight;
            else
            {
                V = (int)((y3 - y2) / RepeatHeight);
                V = Math.Max(1, V);
            }
            */
            Left.UVFromBounds(new Vector2(0, V), new Vector2(1, 0));
            Right.UVFromBounds(new Vector2(0, V), new Vector2(1, 0));
            Top.UVFromBounds(new Vector2(0, 1), new Vector2(U, 0));
            Bottom.UVFromBounds(new Vector2(0, 1), new Vector2(U, 0));
            if (!BottomUp)
                Center.UVFromBounds(new Vector2(0, V), new Vector2(U, 0));
            else
                Center.UVFromBounds(new Vector2(0, 0), new Vector2(U, V));
        }

        public void Update()
        {
            if (Data.CenterOnly)
            {
                Center.Update(ref Base);
            }
            else
            {
                if (Data.MiddleOnly)
                {
                    Center.Update(ref Base);
                    Top.Update(ref Base);
                    Bottom.Update(ref Base);
                }
                else
                {
                    Center.Update(ref Base);
                    Top.Update(ref Base);
                    Bottom.Update(ref Base);
                    Left.Update(ref Base);
                    Right.Update(ref Base);
                    TR.Update(ref Base);
                    TL.Update(ref Base);
                    BL.Update(ref Base);
                    BR.Update(ref Base);
                }
            }

            /*
            Center.Update(ref Base);
            Top.Update(ref Base);
            Bottom.Update(ref Base);
            Left.Update(ref Base);
            Right.Update(ref Base);
            TR.Update(ref Base);
            TL.Update(ref Base);
            BL.Update(ref Base);
            BR.Update(ref Base);*/
        }

        public void Draw()
        {
            Update();

            if (Data.CenterOnly)
            {
                if (Shadow)
                {
                    Center.MyEffect = Tools.EffectWad.EffectList[2];
                    Base.Origin -= ShadowOffset;
                    Center.SetColor(ShadowColor);
                    Center.Update(ref Base);
                    Tools.QDrawer.DrawQuad(Center);
                    Tools.QDrawer.Flush();
                    Center.MyEffect = Tools.EffectWad.FindByName("Basic");
                    Base.Origin += ShadowOffset;
                    Center.SetColor(Color.White);
                }

                Tools.QDrawer.DrawQuad(Center);
            }
            else
            {
                if (Data.MiddleOnly)
                {
                    Tools.QDrawer.DrawQuad(Bottom);
                    Tools.QDrawer.DrawQuad(Center);
                    Tools.QDrawer.DrawQuad(Top);
                }
                else
                {
                    switch (MyDrawOrder)
                    {
                        case DrawOrder.CenterOnTop:
                            Tools.QDrawer.DrawQuad(BL);
                            Tools.QDrawer.DrawQuad(Bottom);
                            Tools.QDrawer.DrawQuad(BR);

                            Tools.QDrawer.DrawQuad(Left);
                            Tools.QDrawer.DrawQuad(Right);
                            Tools.QDrawer.DrawQuad(Center);

                            Tools.QDrawer.DrawQuad(TL);
                            Tools.QDrawer.DrawQuad(TR);
                            Tools.QDrawer.DrawQuad(Top);
                            break;

                        case DrawOrder.CenterOnBottom:
                            Tools.QDrawer.DrawQuad(BL);
                            Tools.QDrawer.DrawQuad(Bottom);
                            Tools.QDrawer.DrawQuad(BR);

                            Tools.QDrawer.DrawQuad(Center);
                            Tools.QDrawer.DrawQuad(Left);
                            Tools.QDrawer.DrawQuad(Right);

                            Tools.QDrawer.DrawQuad(TL);
                            Tools.QDrawer.DrawQuad(TR);
                            Tools.QDrawer.DrawQuad(Top);
                            break;
                    }
                }
            }

            Tools.QDrawer.Flush();
        }
    }
}
