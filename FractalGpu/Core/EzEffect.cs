using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FractalGpu.Core
{
    public class EzEffect
    {
		public EffectParameter c, h1, h2, h3, h4, Center, Count, CamPos;
		public EffectParameter xTexture, xCameraAspect, xCameraPos;

        public Effect effect;
		public EffectTechnique Simplest;

		public EzEffect(ContentManager Content, string file)
		{
			effect = Content.Load<Effect>(file);
			
			c				= effect.Parameters["c"];

			h1				= effect.Parameters["h1"];
			h2				= effect.Parameters["h2"];
			h3				= effect.Parameters["h3"];
			h4				= effect.Parameters["h4"];
			Center			= effect.Parameters["Center"];
			Count			= effect.Parameters["Count"];

			CamPos			= effect.Parameters["CamPos"];

			xTexture		= effect.Parameters["xTexture"];
			xCameraPos		= effect.Parameters["xCameraPos"];
			xCameraAspect	= effect.Parameters["xCameraAspect"];
		}

		public void Set(Vector2 CameraPos, float CameraZoom, float AspectRatio)
		{
			xCameraPos.SetValue(new Vector4(CameraPos.X, CameraPos.Y, AspectRatio * CameraZoom, CameraZoom));
			xCameraAspect.SetValue(1f);
			effect.CurrentTechnique.Passes[0].Apply();
		}
    }
}