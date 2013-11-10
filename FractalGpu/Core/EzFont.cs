using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Drawing;
using System.IO;

namespace FractalGpu
{
    public class EzFont
    {
        public SpriteFont Font, OutlineFont;
        public float CharacterSpacing;
        public int LineSpacing;

        public EzFont(string FontName)
        {
            Font = Tools.TheGame.Content.Load<SpriteFont>(FontName);
            CharacterSpacing = Font.Spacing;
            LineSpacing = Font.LineSpacing;
        }

        public EzFont(string FontName, float CharacterSpacing, int LineSpacing)
        {
            Initialize(FontName, "", CharacterSpacing, LineSpacing);
        }

        public EzFont(string FontName, string OutlineFontName, float CharacterSpacing, int LineSpacing)
        {
            Initialize(FontName, OutlineFontName, CharacterSpacing, LineSpacing);
        }

        void Initialize(string FontName, string OutlineFontName, float CharacterSpacing, int LineSpacing)
        {
            this.CharacterSpacing = CharacterSpacing;
            this.LineSpacing = LineSpacing;

            Font = Tools.TheGame.Content.Load<SpriteFont>(FontName);
            Font.Spacing = CharacterSpacing;
            Font.LineSpacing = LineSpacing;

            if (OutlineFontName.Length > 1)
            {
                OutlineFont = Tools.TheGame.Content.Load<SpriteFont>(OutlineFontName);
                OutlineFont.Spacing = CharacterSpacing;
                OutlineFont.LineSpacing = LineSpacing;
            }
            else
                OutlineFont = null;
        }
    }
}