using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

using Drawing;

namespace FractalGpu
{
    public class Camera
    {
        public Vector4 VertexCam;

        public bool RocketManCamera = false;

        public FancyVector2 FancyPos;

        public bool Shaking;
        public float ShakingIntensity;
        public Vector2 ShakingSaveZoom, ShakingSavePos;

        /// <summary>
        /// If true the camera will not interact with other CameraZones
        /// </summary>
        public bool ZoneLocked = false;

        public PhsxData Data;
        public Vector2 PrevPos, PrevPrevPos;

        public Complex Pos
        {
            get
            {
                return Data.Position;
            }
        }
        

        /// <summary>
        /// The current maximum speed amonst all the alive players.
        /// </summary>
        public Vector2 MaxPlayerSpeed;

        public float Speed, SpeedVel, TargetSpeed;
        public Vector2 Target;
        Vector2 _Zoom;
        public Vector2 Zoom
        {
            get { return _Zoom; }
            set { _Zoom = value; }
        }


        public Vector2 Offset;
        public int ScreenWidth, ScreenHeight;
        public float AspectRatio;

        public Vector2 TR, BL;

        public Vector2 BLCamBound, TRCamBound;

        public bool FollowCenter;

        public void Release()
        {
        }


        public void Move(Vector2 shift)
        {
            Data.Position += shift;
            Target += shift;
            
            TR += shift;
            BL += shift;
            TRCamBound += shift;
            BLCamBound += shift;
            
            PrevPos += shift;
            PrevPrevPos += shift;
        }

        public void Clone(Camera cam) { Clone(cam, false); }
        public void Clone(Camera cam, bool DataOnly)
        {
            MyPhsxType = cam.MyPhsxType;

            Data = cam.Data;
            PrevPos = cam.PrevPos;
            Speed = cam.Speed;
            Target = cam.Target;
            Zoom = cam.Zoom;
            ScreenWidth = cam.ScreenWidth;
            ScreenHeight = cam.ScreenHeight;
            AspectRatio = cam.AspectRatio;
            TR = cam.TR;
            BL = cam.BL;
            TRCamBound = cam.TRCamBound;
            BLCamBound = cam.BLCamBound;
            Shaking = cam.Shaking;
            ShakingIntensity = cam.ShakingIntensity;
            ShakingSaveZoom = cam.ShakingSaveZoom;
            ShakingSavePos = cam.ShakingSavePos;
            VertexCam = cam.VertexCam;
        }

        public Vector2 GetSize()
        {
            return new Vector2(GetWidth(), GetHeight());
        }

        public float GetHeight()
        {
            return TR.Y - BL.Y;
        }

        public float GetWidth()
        {
            return TR.X - BL.X;
        }

        public void Update()
        {
            TR.X = Data.Position.X + AspectRatio / Zoom.X;
            TR.Y = Data.Position.Y + 1f / Zoom.Y;

            BL.X = Data.Position.X - AspectRatio / Zoom.X;
            BL.Y = Data.Position.Y - 1f / Zoom.Y;
        }

        public Camera()
        {
            Init(Tools.Device.PresentationParameters.BackBufferWidth,
                 Tools.Device.PresentationParameters.BackBufferHeight);
        }

        public Camera(int width, int height)
        {
            Init(width, height);
        }

        public void Init(int width, int height)
        {
            Speed = 30;

            BLCamBound = new Vector2(-1000000, -1000000);
            TRCamBound = new Vector2(1000000, 1000000);

            ScreenWidth = width;
            ScreenHeight = height;
            Data.Position = new Vector2(0, 0);
            ShakingSaveZoom = Zoom = new Vector2(.001f, .001f);
            Offset = new Vector2(width / 2, height / 2);
            AspectRatio = (float)ScreenWidth / (float)ScreenHeight;
        }

        public Camera(Camera cam)
        {
            Clone(cam);
            Speed = cam.Speed;

            ScreenHeight = cam.ScreenHeight;
            ScreenWidth = cam.ScreenWidth;
            Data.Position = cam.Data.Position;
            Zoom = cam.Zoom;
            Offset = cam.Offset;
            AspectRatio = cam.AspectRatio;
            TR = cam.TR;
            BL = cam.BL;

            BLCamBound = cam.BLCamBound;
            TRCamBound = cam.TRCamBound;
        }

        Vector2 ShakeOffset;
        int ShakeCount, ShakeLength;
        public void StartShake() { StartShake(1, -1); }
        public void StartShake(float Intensity) { StartShake(Intensity, -1); }
        public void StartShake(float Intensity, int Length)
        {
            ShakeCount = 0;
            ShakeLength = Length;

            if (!Shaking)
            {
                Shaking = true;
                ShakingSaveZoom = Zoom;
                //ShakingSavePos = Data.Position;
            }

            ShakingIntensity = Intensity;
        }

        public void EndShake()
        {
            Shaking = false;
            Zoom = ShakingSaveZoom;
            Data.Position = ShakingSavePos;
        }

        public bool OnScreen(Vector2 pos) { return OnScreen(new Vector2(200, 600)); }
        public bool OnScreen(Vector2 pos, Vector2 GraceSize)
        {
            if (pos.X > TR.X + GraceSize.X) return false;
            if (pos.X < BL.X - GraceSize.X) return false;
            if (pos.Y > TR.Y + GraceSize.Y) return false;
            if (pos.Y < BL.Y - GraceSize.Y) return false;
            return true;
        }

        public void SetVertexCamera()
        {
            VertexCam = new Vector4(Data.Position.X, Data.Position.Y, Zoom.X, Zoom.Y);
            foreach (EzEffect fx in Tools.EffectWad.EffectList) fx.effect.Parameters["xCameraPos"].SetValue(VertexCam);
        }
        
        public void SetVertexZoom(Vector2 factor)
        {
            VertexCam = new Vector4(Data.Position.X, Data.Position.Y, factor.X * Zoom.X, factor.Y * Zoom.Y);
            foreach (EzEffect fx in Tools.EffectWad.EffectList) fx.effect.Parameters["xCameraPos"].SetValue(VertexCam);            
        }

        public void SetVertexCamera(Vector2 shift, Vector2 factor)
        {
            VertexCam = new Vector4(Data.Position.X + shift.X, Data.Position.Y + shift.Y, factor.X * Zoom.X, factor.Y * Zoom.Y);
            foreach (EzEffect fx in Tools.EffectWad.EffectList) fx.effect.Parameters["xCameraPos"].SetValue(VertexCam);
        }

        public Vector2 CurVel()
        {
            //return Data.Position - PrevPos;
            return PrevPos - PrevPrevPos;
        }

        public enum PhsxType { Fixed, Center };
        public PhsxType MyPhsxType = PhsxType.Fixed;
        public void PhsxStep()
        {
            if (SpeedVel != 0)
            {
                float dif = TargetSpeed - Speed;
                Speed += Math.Min(SpeedVel, Math.Abs(dif)) * Math.Sign(dif);
            }

            switch (MyPhsxType)
            {
                case PhsxType.Fixed:
                    Fixed_PhsxStep();
                    break;

                case PhsxType.Center:
                    Center_PhsxStep();
                    break;
            }

            //PrevPos = HoldPrevPos;
            PrevPrevPos = PrevPos;
            PrevPos = Data.Position;
        }

        public void SetPhsxType(PhsxType NewType)
        {
            if (NewType != MyPhsxType)
            {
                switch (NewType)
                {
                }

                MyPhsxType = NewType;
            }
        }

        public void Fixed_PhsxStep()
        {
            if (FancyPos != null)
            {
                Data.Position = FancyPos.Update();                
            }

            Update();
        }

        public void Center_PhsxStep()
        {
            Vector2 vel = CurVel();
            Vector2 dif = Target - (Vector2)Data.Position;
            Vector2 TargetVel = dif; TargetVel.Normalize();
            
            float EffectiveSpeed = Vector2.Dot(vel, TargetVel);
            if (15 * EffectiveSpeed > dif.Length() || dif.Length() < 2 * Speed)
                vel = 1f / 16f * dif;
            else
            {
                TargetVel *= Speed;
                vel += .08f * (TargetVel - vel);
            }

            Data.Position += vel;

            Update();
        }


    }
}
