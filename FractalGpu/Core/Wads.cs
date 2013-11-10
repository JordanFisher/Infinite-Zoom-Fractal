using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FractalGpu;

namespace Drawing
{
    public class EzEffect
    {
        public EzEffectWad MyWad;

        public Effect effect;
        public string Name;
        public EffectParameter xFlip, FlipCenter, xTexture;

        /// <summary>
        /// Whether the effect has the up-to-date parameters set
        /// </summary>
        public bool IsUpToDate;
        public Vector4 MySetCameraPosition;

        public void SetCameraParameters()
        {
            Vector4 CameraPosition = MyWad.CameraPosition;

            if (CameraPosition != MySetCameraPosition)
            {
                effect.Parameters["xCameraPos"].SetValue(CameraPosition);
                MySetCameraPosition = CameraPosition;
            }

            IsUpToDate = true;
        }
    }

    public class EzEffectWad
    {
        public List<EzEffect> EffectList;

        public Vector4 CameraPosition;
        public void SetCameraPosition(Vector4 CameraPosition)
        {
            this.CameraPosition = CameraPosition;

            foreach (EzEffect effect in EffectList)
                effect.IsUpToDate = false;
        }

        Vector4 HoldCamPos;
        public void SetDefaultZoom()
        {
            HoldCamPos = CameraPosition;
            SetCameraPosition(new Vector4(CameraPosition.X, CameraPosition.Y, .001f, .001f));
        }

        public void ResetCameraPos()
        {
            SetCameraPosition(HoldCamPos);
        }

        public EzEffectWad()
        {
            EffectList = new List<EzEffect>();
        }

        public EzEffect FindByName(string name)
        {
            foreach (EzEffect effect in EffectList)
                if (effect.Name.CompareTo(name) == 0)
                    return effect;
            return EffectList[0];
        }

        public void AddEffect(Effect effect, string Name)
        {
            if (EffectList.Exists(match => string.Compare(match.Name, Name, StringComparison.OrdinalIgnoreCase) == 0))
            {
                FindByName(Name).effect = effect;
            }
            else
            {
                EzEffect Neweffect = new EzEffect();
                Neweffect.Name = Name;
                Neweffect.effect = effect;

                Neweffect.xFlip = effect.Parameters["xFlip"];
                Neweffect.FlipCenter = effect.Parameters["FlipCenter"];
                Neweffect.xTexture = effect.Parameters["xTexture"];

                Neweffect.MyWad = this;

                EffectList.Add(Neweffect);
            }
        }
    }
}