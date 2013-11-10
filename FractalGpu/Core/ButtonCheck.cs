using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace FractalGpu.Core
{
    public enum ControllerButtons { A, B, X, Y, RS, LS, RT, LT, RJ, RJButton, LJ, LJButton, DPad, Start };
    public enum MashType { Hold, Tap, Alternate, HoldDir };
    public struct ButtonData { public bool Down; public bool Pressed; public Vector2 Dir; public float Squeeze; }
    public class ButtonCheck
    {
        public ControllerButtons MyButton1, MyButton2;
        public int MyPlayerIndex;
        public MashType MyType;
        ButtonData Current, Previous;

        public int GapCount, GapAllowance;
        public int Dir;

        public bool Satisfied;

        static public int Direction(Vector2 Dir)
        {
            if (Dir.Length() < .25f) return -1;
            if (Dir.Y < Dir.X && Dir.Y > -Dir.X) return 0;
            if (Dir.Y > Dir.X && Dir.Y > -Dir.X) return 1;
            if (Dir.Y < -Dir.X && Dir.Y > Dir.X) return 2;
            if (Dir.Y < Dir.X && Dir.Y < -Dir.X) return 3;
            return -1;
        }

        static public Vector2 GetDir(int Control)
        {
            Vector2 Dir = ButtonCheck.State(ControllerButtons.DPad, Control).Dir;
            Vector2 HoldDir = ButtonCheck.State(ControllerButtons.LJ, Control).Dir;
            if (Math.Abs(HoldDir.X) > Math.Abs(Dir.X)) Dir.X = HoldDir.X;
            if (Math.Abs(HoldDir.Y) > Math.Abs(Dir.Y)) Dir.Y = HoldDir.Y;

            return Dir;
        }


        static public ButtonData State(ControllerButtons Button, int iPlayerIndex) { return State(Button, iPlayerIndex, false); }
        static public ButtonData State(ControllerButtons Button, int iPlayerIndex, bool Prev)
        {
            ButtonData Data = new ButtonData();

            if (iPlayerIndex == -1 || iPlayerIndex == -2)
            {
                if (!Prev)
                {
                    Data.Dir *= 0;
                    for (int i = 0; i < 4; i++)
                        //if (Generic.PlayerExists[i] || iPlayerIndex == -2)
                        {
                            ButtonData data = State(Button, i);
                            Data.Down = Data.Down || data.Down;
                            Data.Pressed = Data.Pressed || data.Pressed;
                            //Data.Down = Data.Down || State(Button, i).Down;

                            if (Math.Abs(data.Dir.X) > Math.Abs(Data.Dir.X)) Data.Dir.X = data.Dir.X;
                            if (Math.Abs(data.Dir.Y) > Math.Abs(Data.Dir.Y)) Data.Dir.Y = data.Dir.Y;
                        }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                        //if (Generic.PlayerExists[i] || iPlayerIndex == -2)
                        {
                            ButtonData data = State(Button, i, true);
                            Data.Down = Data.Down || data.Down;
                            Data.Pressed = Data.Pressed || data.Pressed;
                            //Data.Down = Data.Down || State(Button, i, true).Down;
                        }
                }
                return Data;
            }
#if WINDOWS

            Keys key = Keys.None;

            //if (iPlayerIndex == 0 || Generic.PlayerExists[iPlayerIndex])
            {
#if PC_VERSION
                if (Button == ControllerButtons.Start) key = Keys.Escape;
                if (Button == ControllerButtons.B) key = Keys.Escape;

                if (Button == ControllerButtons.X) key = Keys.C;
                if (Button == ControllerButtons.Y) key = Keys.V;
                if (Button == ControllerButtons.RT) key = Keys.OemPeriod;
                if (Button == ControllerButtons.LT) key = Keys.OemComma;
#else
                if (Button == ControllerButtons.Start) key = Keys.S;
                if (Button == ControllerButtons.A) key = Keys.Z;
                if (Button == ControllerButtons.B) key = Keys.X;
                if (Button == ControllerButtons.X) key = Keys.C;
                if (Button == ControllerButtons.Y) key = Keys.V;
                if (Button == ControllerButtons.RT) key = Keys.OemPeriod;
                if (Button == ControllerButtons.LT) key = Keys.OemComma;
#endif
            }

            KeyboardState keyboard;
            if (Prev)
                keyboard = Tools.PrevKeyboardState;
            else
                keyboard = Tools.keybState;

#endif
            //#else
            GamePadState Pad;
            if (Prev) Pad = Tools.PrevpadState[iPlayerIndex];
            else Pad = Tools.padState[iPlayerIndex];

            switch (Button)
            {
                case ControllerButtons.Start: Data.Down = (Pad.Buttons.Start == ButtonState.Pressed); break;
                case ControllerButtons.A: Data.Down = (Pad.Buttons.A == ButtonState.Pressed); break;
                case ControllerButtons.B: Data.Down = (Pad.Buttons.B == ButtonState.Pressed); break;
                case ControllerButtons.X: Data.Down = (Pad.Buttons.X == ButtonState.Pressed); break;
                case ControllerButtons.Y: Data.Down = (Pad.Buttons.Y == ButtonState.Pressed); break;
                case ControllerButtons.LJButton: Data.Down = (Pad.Buttons.LeftStick == ButtonState.Pressed); break;
                case ControllerButtons.RJButton: Data.Down = (Pad.Buttons.RightStick == ButtonState.Pressed); break;
                case ControllerButtons.LS: Data.Down = (Pad.Buttons.LeftShoulder == ButtonState.Pressed); break;
                case ControllerButtons.RS: Data.Down = (Pad.Buttons.RightShoulder == ButtonState.Pressed); break;
                case ControllerButtons.LT:
                    {
                        Data.Down = (Pad.Triggers.Left > .5f);
                        Data.Squeeze = Pad.Triggers.Left;
                        break;
                    }
                case ControllerButtons.RT:
                    {
                        Data.Down = (Pad.Triggers.Right > .5f);
                        Data.Squeeze = Pad.Triggers.Right;
                        break;
                    }
                case ControllerButtons.LJ: Data.Dir = Pad.ThumbSticks.Left; break;
                case ControllerButtons.RJ: Data.Dir = Pad.ThumbSticks.Right; break;
                case ControllerButtons.DPad:
                    {
                        Data.Dir = Vector2.Zero;
                        if (Pad.DPad.Right == ButtonState.Pressed) Data.Dir = new Vector2(1, 0);
                        if (Pad.DPad.Up == ButtonState.Pressed) Data.Dir = new Vector2(0, 1);
                        if (Pad.DPad.Left == ButtonState.Pressed) Data.Dir = new Vector2(-1, 0);
                        if (Pad.DPad.Down == ButtonState.Pressed) Data.Dir = new Vector2(0, -1);
                    }
                    break;
            }
            //#endif
#if WINDOWS
#if PC_VERSION
            if (Button == ControllerButtons.A)
            {
                if (Prev)
                    Data.Down = Tools.CurMouseState.LeftButton == ButtonState.Pressed;
                else
                    Data.Down = Tools.PrevMouseState.LeftButton == ButtonState.Pressed;
            }
            else
                Data.Down = keyboard.IsKeyDown(key);

            key = Keys.Escape;
            if (Button == ControllerButtons.A) key = Keys.Z;
            if (Button == ControllerButtons.B) key = Keys.X;
            if (key != Keys.Escape)
                Data.Down = Data.Down || keyboard.IsKeyDown(key);

            if (Button == ControllerButtons.A)
                Data.Down = Data.Down || keyboard.IsKeyDown(Keys.Enter);
#else
            Data.Down |= keyboard.IsKeyDown(key);
#endif

            if (Button == ControllerButtons.LJ)
            {
                Vector2 KeyboardDir = Vector2.Zero;
                if (keyboard.IsKeyDown(Keys.Left)) KeyboardDir = new Vector2(-1, 0);
                if (keyboard.IsKeyDown(Keys.Right)) KeyboardDir = new Vector2(1, 0);
                if (keyboard.IsKeyDown(Keys.Up)) KeyboardDir = new Vector2(0, 1);
                if (keyboard.IsKeyDown(Keys.Down)) KeyboardDir = new Vector2(0, -1);

                if (KeyboardDir.LengthSquared() > Data.Dir.LengthSquared())
                    Data.Dir = KeyboardDir;
            }
#endif

            // Pressed == true if the previous state was not pressed but the current is
            if (Data.Down && !Prev && !State(Button, iPlayerIndex, true).Down)
                Data.Pressed = true;

            return Data;
        }

        public ButtonCheck() { }

        public String GetString()
        {
            String s;

            switch (MyType)
            {
                case MashType.Hold: s = "Hold {p"; break;
                case MashType.Tap: s = "Tap {p"; break;
                case MashType.HoldDir: s = "Hold {p"; break;
                default: s = ""; break;
            }

            s += "Big_Button_" + Tools.ButtonNames[(int)MyButton1] + ",75,75}";

            switch (MyType)
            {
                case MashType.HoldDir: s += " " + Tools.DirNames[Dir]; break;
                default: break;
            }

            return s;
        }

        public void Phsx()
        {
            Satisfied = false;

            Previous = Current;
            Current = ButtonCheck.State(MyButton1, MyPlayerIndex);

            switch (MyType)
            {
                case MashType.Hold:
                    if (Current.Down)
                        Satisfied = true;

                    break;

                case MashType.Tap:
                    if (Current.Down != Previous.Down)
                        GapCount = 0;
                    else
                        GapCount++;
                    if (GapCount < GapAllowance)
                        Satisfied = true;

                    break;

                case MashType.HoldDir:
                    if (Direction(Current.Dir) == Dir)
                        Satisfied = true;
                    break;
            }
        }
    }
}

