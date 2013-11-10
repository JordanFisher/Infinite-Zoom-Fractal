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
    public interface IPos
    {
        Vector2 Pos
        {
            get;
        }
    }

    public class FancyVector2 : IPos
    {
        public AnimationData AnimData;

        public Vector2[] HoldVecs = new Vector2[5];

        public Vector2 RelVal, AbsVal;

        public IPos Center;
        public float Speed;
        public int TimeStamp, LastUpdate = int.MinValue;
        public float t;
        public bool Playing = false;

        public Vector2 Pos
        {
            get { return Update(); }
        }

        public void Release()
        {
            AnimData.Release();
            Center = null;
        }

        public FancyVector2()
        {
        }

        public FancyVector2(IPos Center)
        {
            this.Center = Center;
        }

        /// <summary>
        /// Sets the FancyPos's center FancyPos. Updates relative coordinates so that absolute coordinates are unaffected.
        /// </summary>
        /// <param name="Center"></param>
        public void SetCenter(IPos Center) { SetCenter(Center, false); }
        /// <summary>
        /// Sets the FancyPos's center FancyPos
        /// </summary>
        /// <param name="Center"></param>
        /// <param name="UsePosAsRelPos">Whether to use the current position as relative coordinates in the new system</param>
        public void SetCenter(IPos Center, bool UsePosAsRelPos)
        {
            if (this.Center == Center) return;

            if (!UsePosAsRelPos)
                RelVal = Update() - Center.Pos;
            this.Center = Center;
        }

        public Vector2 GetDest()
        {
            if (!Playing)
                return RelVal;
            else
                return AnimData.Get(0, AnimData.Anims[0].Data.Length - 1);
        }

        public void ToAndBack(Vector2 End, int Frames)
        {
            ToAndBack(RelVal, End, Frames);
        }
        public void ToAndBack(Vector2 Start, Vector2 End, int Frames)
        {
            RelVal = Start;

            AnimData = new AnimationData();
            AnimData.Init();
            AnimData.Set(Start, 0, 0);
            AnimData.Set(End, 0, 1);
            AnimData.Set(Start, 0, 2);

            Speed = 2f / Frames;
            TimeStamp = GetCurStep();
            t = 0;
            Playing = true;
        }

        public void MultiLerp(int Frames, params Vector2[] Params)
        {
            AnimData = new AnimationData();
            AnimData.Init();

            for (int i = 0; i < Params.Length; i++)
                AnimData.Set(Params[i], 0, i);

            Speed = 1f / Frames;
            TimeStamp = GetCurStep();
            t = 0;
            Playing = true;
        }

        public void LerpTo(int EndIndex, int Frames)
        {
            LerpTo(HoldVecs[EndIndex], Frames);
        }
        public void LerpTo(int StartIndex, int EndIndex, int Frames)
        {
            LerpTo(HoldVecs[StartIndex], HoldVecs[EndIndex], Frames);
        }
        public void LerpTo(Vector2 End, int Frames)
        {
            LerpTo(RelVal, End, Frames);
        }
        public void LerpTo(Vector2 Start, Vector2 End, int Frames)
        {
            RelVal = Start;

            AnimData = new AnimationData();
            AnimData.Init();
            AnimData.Set(Start, 0, 0);
            AnimData.Set(End, 0, 1);

            Speed = 1f / Frames;
            TimeStamp = GetCurStep();
            t = 0;
            Playing = true;
        }

        public bool UpdateOnPause = true;
        public bool UpdateWithGame = false;
        int GetCurStep()
        {
            if (UpdateWithGame)
            {
                    return Tools.TheGame.PhsxCount;
            }

            if (UpdateOnPause) return Tools.DrawCount;
            else return Tools.PhsxCount;
        }

        public Vector2 Update()
        {
            int CurStep = GetCurStep();

            if (Playing && CurStep != LastUpdate)
            {
                LastUpdate = CurStep;

                if (UpdateWithGame)
                    t += Speed;
                else
                    t = Speed * (CurStep - TimeStamp);

                if (t > AnimData.Anims[0].Data.Length - 1)
                {
                    Playing = false;
                    RelVal = AnimData.Get(0, AnimData.Anims[0].Data.Length - 1);
                }
                else
                    RelVal = AnimData.Calc(0, t, AnimData.Anims[0].Data.Length, false, true);
            }

            AbsVal = RelVal;
            if (Center != null)
                AbsVal += Center.Pos;
                //AbsVal += Center.RelVal;

            return AbsVal;
        }
    }
}