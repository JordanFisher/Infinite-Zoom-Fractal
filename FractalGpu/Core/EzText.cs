using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.IO;

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
    public class EzText
    {
        public bool HitTest(Vector2 pos) { return HitTest(pos, Vector2.Zero); }
        public bool HitTest(Vector2 pos, Vector2 padding)
        {
            CalcBounds();
            if (pos.X > TR.X + padding.X) return false;
            if (pos.X < BL.X - padding.X) return false;
            if (pos.Y > TR.Y + padding.Y) return false;
            if (pos.Y < BL.Y - padding.Y) return false;

            return true;
        }

        class EzTextBit
        {
            public int LineNumber;
            public String str;
            public Vector2 loc, size;
            public Color clr;
        }
        class EzTextPic
        {
            public int LineNumber;
            public EzTexture tex;
            public Rectangle rect;
            public Vector2 size;
        }
        List<EzTextBit> Bits;
        List<EzTextPic> Pics;

        /// <summary>
        /// Replaces the first bit of text, with no reformatting
        /// </summary>
        /// <param name="text"></param>
        public void SubstituteText(string text)
        {
            Bits[0].str = text;
        }

        public PieceQuad Backdrop;

        public bool FixedToCamera;

        public bool ColorizePics;
        Color PicColor;

        EzFont MyFont;
        Color MyColor;
        public Vector4 MyFloatColor;

        public Vector2 TR, BL;

        public void MakeFancyPos()
        {
            if (FancyPos != null) FancyPos.Release();
            FancyPos = new FancyVector2();
            FancyPos.RelVal = _Pos; ;
        }

        public Vector2 _Pos;
        public Vector2 Pos
        {
            get
            {
                if (FancyPos == null)
                    return _Pos;
                else
                    return FancyPos.RelVal;
            }
            set
            {
                if (FancyPos == null)
                    _Pos = value;
                else
                    FancyPos.RelVal = value;
            }
        }
        public float X
        {
            get
            {
                if (FancyPos == null)
                    return _Pos.X;
                else
                    return FancyPos.RelVal.X;
            }
            set
            {
                if (FancyPos == null)
                    _Pos.X = value;
                else
                    FancyPos.RelVal.X = value;
            }
        }
        public float Y
        {
            get
            {
                if (FancyPos == null)
                    return _Pos.Y;
                else
                    return FancyPos.RelVal.Y;
            }
            set
            {
                if (FancyPos == null)
                    _Pos.Y = value;
                else
                    FancyPos.RelVal.Y = value;
            }
        }

        public FancyVector2 FancyPos;
        public float TextBoxWidth;
        public float Height, TextWidth;

        public String MyString;

        public int Code;

        public Vector4 OutlineColor = Color.Black.ToVector4();

        public bool Shadow = false;
        public bool PicShadow = true;
        public Vector2 ShadowOffset = Vector2.Zero;
        public Color ShadowColor = Color.Black;

        public float Alpha = 1;

        public float Scale = 1;

        public void Release()
        {
            if (FancyPos != null) FancyPos.Release(); FancyPos = null;
        }

        public bool Centered;
        public EzText() { }
        public EzText(String str, EzFont font) { MyFont = font; Init(str); }
        public EzText(String str, EzFont font, bool Centered) { MyFont = font; Init(str, 10000, Centered, false, 1); }
        public EzText(String str, EzFont font, float Width, bool Centered) { MyFont = font; Init(str, Width, Centered, false, 1); }
        public EzText(String str, EzFont font, bool Centered, bool YCentered) { MyFont = font; Init(str, 10000, Centered, YCentered, 1); }
        public EzText(String str, EzFont font, float Width, bool Centered, bool YCentered) { MyFont = font; Init(str, Width, Centered, YCentered, 1); }
        public EzText(String str, EzFont font, float Width, bool Centered, bool YCentered, float LineHeightMod) { MyFont = font; Init(str, Width, Centered, YCentered, LineHeightMod); }

        public PieceQuad BackdropTemplate = PieceQuad.Menu;
        public Vector2 BackdropShift = Vector2.Zero;
        public void AddBackdrop() { AddBackdrop(new Vector2(.65f, .65f), Vector2.Zero, Vector2.Zero, new Vector2(10000,10000)); }
        public void AddBackdrop(Vector2 padding) { AddBackdrop(padding, Vector2.Zero, Vector2.Zero, new Vector2(10000, 10000)); }
        public void AddBackdrop(Vector2 mult_padding, Vector2 add_padding) { AddBackdrop(mult_padding, add_padding, Vector2.Zero, new Vector2(10000, 10000)); }
        public void AddBackdrop(Vector2 mult_padding, Vector2 add_padding, Vector2 min_dim, Vector2 max_dim)
        {
            if (Backdrop == null)
                Backdrop = new PieceQuad();
            Backdrop.Clone(BackdropTemplate);

            Vector2 dim;
            dim = mult_padding * GetWorldSize() + add_padding;

            Backdrop.CalcQuads(Vector2.Min(Vector2.Max(dim, min_dim), max_dim));
        }

        Vector2 loc;
        float LineHeight;
        void CheckForLineEnd(Vector2 TextSize)
        {            
            LineHeight = Math.Max(LineHeight, TextSize.Y);
            if (TextSize.X + loc.X > TextBoxWidth && loc.X != 0)
            {
                loc.X = 0;
                loc.Y += LineHeight;
                LineHeight = 0;
            }
        }

        String Parse_PicName;
        Vector2 Parse_PicSize;
        Vector2 Parse_PicShift;
        Color Parse_Color;
        enum ParseData { Pic, Color };
        ParseData Parse_Type;
        void Parse(String str)
        {
            char c = str[1];

            int Comma1, Comma2, Comma3, Comma4;
            String WidthString, HeightString;
            switch (c)
            {
                case 'p':
                    Parse_Type = ParseData.Pic;

                    string[] string_bits = str.Split(',');

                    Comma1 = str.IndexOf(",");
                    Comma2 = str.IndexOf(",", Comma1 + 1);

                    Parse_PicName = str.Substring(2, Comma1 - 2);
                    WidthString = str.Substring(Comma1 + 1, Comma2 - 1 - Comma1);
                    HeightString = str.Substring(Comma2 + 1);

                    if (string_bits.Length > 3)
                        Parse_PicShift = new Vector2(float.Parse(string_bits[3]),
                                                     float.Parse(string_bits[4])) ;//* Tools.TheGame.Resolution.LineHeightMod;
                    else
                        Parse_PicShift = Vector2.Zero;

                    Vector2 size;
                    EzTexture texture = Tools.TextureWad.FindByName(Parse_PicName);
                    float ratio = (float)texture.Tex.Width / (float)texture.Tex.Height;
                    // '?' calculates that number from the texture height/width ratio
                    if (WidthString.Contains('?'))
                    {
                        size = new Vector2(0, float.Parse(HeightString));
                        size.X = size.Y * ratio;
                    }
                    else if (HeightString.Contains('?'))
                    {
                        size = new Vector2(float.Parse(WidthString), 0);
                        size.Y = size.X / ratio;
                    }
                    else
                        size = new Vector2(float.Parse(WidthString), float.Parse(HeightString));
                    Parse_PicSize = size;// *Tools.TheGame.Resolution.LineHeightMod;
                    break;

                // Blank space
                case 's':
                    Parse_Type = ParseData.Pic;
                    Parse_PicName = "Transparent";

                    Comma1 = str.IndexOf(",");

                    WidthString = str.Substring(2, Comma1 - 2);
                    HeightString = str.Substring(Comma1 + 1);
                    Parse_PicSize = new Vector2(float.Parse(WidthString), float.Parse(HeightString));
                    Parse_PicSize *= Tools.TheGame.Resolution.LineHeightMod;
                    break;

                case 'c':
                    Parse_Type = ParseData.Color;

                    Comma1 = str.IndexOf(",");
                    Comma2 = str.IndexOf(",", Comma1 + 1);
                    Comma3 = str.IndexOf(",", Comma2 + 1);

                    String RString = str.Substring(2, Comma1 - 2);
                    String GString = str.Substring(Comma1 + 1, Comma2 - 1 - Comma1);
                    String BString = str.Substring(Comma2 + 1, Comma3 - 1 - Comma2);
                    String AString = str.Substring(Comma3 + 1);

                    Parse_Color = new Color(byte.Parse(RString), byte.Parse(GString), byte.Parse(BString), byte.Parse(AString));
                    break;
            }
        }

        int GetLineEnd(String str)
        {
            int EndIndex = 0;
            int NewEndIndex;
            int BracketIndex, SpaceIndex, DelimiterIndex;

            bool ReachedEnd = false;
            while (!ReachedEnd)
            {
                BracketIndex = str.IndexOf("}", EndIndex);
                SpaceIndex = str.IndexOf(" ", EndIndex);
                DelimiterIndex = str.IndexOf('\n', EndIndex);
                if (BracketIndex == -1 && SpaceIndex == -1) { NewEndIndex = str.Length; ReachedEnd = true; }
                else if (BracketIndex == -1) NewEndIndex = SpaceIndex + 1;
                else if (SpaceIndex == -1) NewEndIndex = BracketIndex + 1;
                else NewEndIndex = Math.Min(BracketIndex, SpaceIndex) + 1;

                if (DelimiterIndex >= 0 && DelimiterIndex < NewEndIndex) { NewEndIndex = DelimiterIndex; ReachedEnd = true; }

                float width = StringSize(str.Substring(0, NewEndIndex)).X;
                if (width > TextBoxWidth && EndIndex > 0) return EndIndex;
                else EndIndex = NewEndIndex;
            }

            return EndIndex;
        }

        Vector2 StringSize(String str)
        {
            Vector2 Size = Vector2.Zero;
            int BeginBracketIndex, EndBracketIndex;
            bool flag = false;

            while (!flag)
            {
                BeginBracketIndex = str.IndexOf("{", 0);
                if (BeginBracketIndex >= 0)
                {
                    EndBracketIndex = str.IndexOf("}", 0);
                    String PicStr = str.Substring(BeginBracketIndex, EndBracketIndex - BeginBracketIndex);
                    Parse(PicStr);
                    str = str.Remove(BeginBracketIndex, EndBracketIndex - BeginBracketIndex + 1);

                    if (Parse_Type == ParseData.Pic)
                    {
                        Size.X += Parse_PicSize.X;
                        Size.Y = Math.Max(Size.Y, Parse_PicSize.Y);
                    }
                }
                else
                    flag = true;
            }

            Vector2 TextSize = MyFont.Font.MeasureString(str);
            TextSize.Y *= Tools.TheGame.Resolution.LineHeightMod;

            Size.X += TextSize.X;
            Size.Y = Math.Max(Size.Y, TextSize.Y);

            return Size;
        }

        float AddLine(String str, float StartX, float StartY, int LineNumber)
        {
            Vector2 loc = new Vector2(StartX, 0);
            float LineHeight = 0;
            int BeginBracketIndex, EndBracketIndex;

            Color CurColor = Color.White;

            while (str.Length > 0)
            {
                BeginBracketIndex = str.IndexOf("{", 0);
                if (BeginBracketIndex == 0)
                {
                    EndBracketIndex = str.IndexOf("}", 0);
                    String PicStr = str.Substring(BeginBracketIndex, EndBracketIndex - BeginBracketIndex);
                    Parse(PicStr);
                    str = str.Remove(BeginBracketIndex, EndBracketIndex - BeginBracketIndex + 1);

                    if (Parse_Type == ParseData.Pic)
                    {
                        EzTextPic pic = new EzTextPic();
                        pic.LineNumber = LineNumber;
                    
                        pic.tex = Tools.TextureWad.FindByName(Parse_PicName);
                        float y = Tools.TheGame.Resolution.LineHeightMod * MyFont.LineSpacing / 2 - Parse_PicSize.Y / 2 + StartY;
                        pic.rect = new Rectangle((int)(loc.X + Parse_PicShift.X), (int)(y + loc.Y + Parse_PicShift.Y), (int)Parse_PicSize.X, (int)Parse_PicSize.Y);
                        pic.size = Parse_PicSize;

                        Pics.Add(pic);

                        loc.X += Parse_PicSize.X;
                        LineHeight = Math.Max(LineHeight, Parse_PicSize.Y * .9f);
                    }
                    if (Parse_Type == ParseData.Color)
                    {
                        CurColor = Parse_Color;
                    }
                }
                else
                {
                    int i;
                    if (BeginBracketIndex < 0) i = str.Length; else i = BeginBracketIndex;
                    
                    EzTextBit bit = new EzTextBit();
                    bit.LineNumber = LineNumber;
                    bit.clr = CurColor;
                    bit.str = str.Substring(0, i);
                    str = str.Remove(0, i);

                    bit.size = MyFont.Font.MeasureString(bit.str);
                    bit.size.Y *= Tools.TheGame.Resolution.LineHeightMod;

                    bit.loc = loc + new Vector2(0, StartY);
                    Bits.Add(bit);

                    loc.X += bit.size.X;
                    LineHeight = Math.Max(LineHeight, bit.size.Y * .9f);
                }                    
            }

            return LineHeight;
        }

        public Vector2 GetWorldSize()
        {
            return new Vector2(GetWorldWidth(), GetWorldHeight());
        }

        public float GetWorldHeight()
        {
            Vector2 vec1 = new Vector2(0, Height);
            Vector2 vec2 = new Vector2(0, 0);
            vec1 = Tools.ToWorldCoordinates(vec1, Tools.TheGame.MainCamera);
            vec2 = Tools.ToWorldCoordinates(vec2, Tools.TheGame.MainCamera);

            return (vec2.Y - vec1.Y);// *Tools.TheGame.Resolution.LineHeightMod;
        }

        public float GetWorldWidth()
        {
            Vector2 vec2 = new Vector2(TextWidth, 0);
            Vector2 vec1 = new Vector2(0, 0);
            vec1 = Tools.ToWorldCoordinates(vec1, Tools.TheGame.MainCamera);
            vec2 = Tools.ToWorldCoordinates(vec2, Tools.TheGame.MainCamera);

            return (vec2.X - vec1.X) * Tools.TheGame.Resolution.LineHeightMod;
        }

        Vector2[] LineSizes = new Vector2[20];
        void Init(String str) { Init(str, 10000, false, false, 1f); }
        void Init(String str, float Width, bool Centered, bool YCentered, float LineHeightMod)
        {
            this.Centered = Centered;

            TextBoxWidth = Width;

            MyString = str;

            MyColor = Color.White;
            MyFloatColor = new Vector4(1, 1, 1, 1);

            Bits = new List<EzTextBit>();
            Pics = new List<EzTextPic>();

            loc = Vector2.Zero;
            LineHeight = 0;

            int i = 0;

            TextWidth = 0;
            float y = 0;
            int LineNumber = 0;
            while (str.Length > 0)
            {
                i = GetLineEnd(str);
                String Line = str.Substring(0, i);
                if (Line.Length > 0 && Line[Line.Length-1] == ' ') Line = Line.Remove(Line.Length-1, 1);
                Vector2 Size = StringSize(Line);
                LineSizes[LineNumber] = Size;
                
                float x = -Size.X / 2;
                TextWidth = Math.Max(TextWidth, Size.X);
                y += LineHeightMod * AddLine(Line, x, y, LineNumber);
                str = str.Remove(0, i);
                if (str.Length > 0 && str[0] == ' ') str = str.Remove(0, 1);
                if (str.Length > 0 && str[0] == '\n') str = str.Remove(0, 1);

                LineNumber++;
            }

            Height = y;

            
            if (YCentered)
            {
                foreach (EzTextBit bit in Bits) bit.loc.Y -= Height / 2;
                foreach (EzTextPic pic in Pics) pic.rect.Y -= (int)(Height / 2);
            }

            if (!Centered)
            {
                foreach (EzTextBit bit in Bits) bit.loc.X += LineSizes[bit.LineNumber].X / 2 * Tools.TheGame.Resolution.LineHeightMod;
                foreach (EzTextPic pic in Pics) pic.rect.X += (int)(LineSizes[pic.LineNumber].X / 2);// * Tools.TheGame.Resolution.LineHeightMod);

                //foreach (EzTextBit bit in Bits) bit.loc.X += TextWidth / 2 * Tools.TheGame.Resolution.LineHeightMod;
                //foreach (EzTextPic pic in Pics) pic.rect.X += (int)(TextWidth / 2);// * Tools.TheGame.Resolution.LineHeightMod);
            }
            else
            {
                foreach (EzTextPic pic in Pics) pic.rect.X = (int)((pic.rect.X - Pos.X + pic.rect.Width / 2f) * Tools.TheGame.Resolution.LineHeightMod + Pos.X - pic.rect.Width / 2f);
            }
        }

        public void Draw(Camera cam) { Draw(cam, true); }
        public void Draw(Camera cam, bool EndBatch)
        {
            if (FancyPos != null)
                _Pos = FancyPos.Update();

            PicColor = Color.Black;//MyColor;
            if (Shadow)
            {
                _Pos -= ShadowOffset;
                if (MyFont.OutlineFont != null || OutlineColor.W == 0)
                    _Draw(cam, EndBatch, true, MyFont.OutlineFont, ShadowColor.ToVector4());
                _Draw(cam, false, PicShadow, MyFont.Font, ShadowColor.ToVector4());
                _Pos += ShadowOffset;
            }

            PicColor = new Color(MyFloatColor);
            if (!ColorizePics)
            {
                PicColor = Color.White;
                PicColor.A = MyColor.A;
            }

            if (MyFont.OutlineFont != null || OutlineColor.W == 0)
                _Draw(cam, EndBatch, true, MyFont.OutlineFont, OutlineColor);
            _Draw(cam, EndBatch, true, MyFont.Font, MyFloatColor);
        }

        public void _Draw(Camera cam, bool EndBatch, bool DrawPics, SpriteFont font, Vector4 color)
        {
            if (MyFloatColor.W <= 0) return;

            MyColor.R = Tools.FloatToByte(color.X);
            MyColor.G = Tools.FloatToByte(color.Y);
            MyColor.B = Tools.FloatToByte(color.Z);
            MyColor.A = Tools.FloatToByte(color.W * Alpha);

            Vector2 Position = _Pos;
            if (FixedToCamera) Position += (Vector2)cam.Data.Position;
            Vector2 Loc = Tools.ToScreenCoordinates(Position, cam);
            Vector2 Loc2 = Tools.ToScreenCoordinates(Position + new Vector2(.5f, .5f), cam);

            //Tools.QDrawer.DrawLine(Position, Position + new Vector2(1000, 0), Color.YellowGreen, 10);

            Tools.StartSpriteBatch();
            foreach (EzTextBit bit in Bits)
                Tools.spriteBatch.DrawString(font, bit.str, bit.loc + Loc, new Color(MyColor.ToVector4() * bit.clr.ToVector4()),
                    0, bit.size * Tools.TheGame.Resolution.TextOrigin, new Vector2(Tools.TheGame.Resolution.LineHeightMod, Tools.TheGame.Resolution.LineHeightMod) * Scale, SpriteEffects.None, 1);
            if (DrawPics)
                foreach (EzTextPic pic in Pics)
                {
                    Rectangle rect = pic.rect;
                    //rect.X = (int)(rect.X * Tools.TheGame.Resolution.LineHeightMod);
                    rect.X += (int)Loc2.X;
                    rect.Y += (int)Loc2.Y;
                    Tools.spriteBatch.Draw(pic.tex.Tex, rect, PicColor);
                }

            if (EndBatch)
                Tools.EndSpriteBatch();
        }

        public void CalcBounds()
        {
            TR = new Vector2(-100000000, -100000000);
            Vector2 pos = Pos;
            if (FixedToCamera) pos += (Vector2)Tools.TheGame.MainCamera.Data.Position;
            
            Vector2 size = GetWorldSize();
            if (Centered)
            {
                TR = pos + size / 2;
                BL = pos - size / 2;
            }
            else
            {
                TR = pos + new Vector2(size.X, size.Y / 2);
                BL = pos + new Vector2(0, -size.Y / 2);
            }
        }
    }
}
