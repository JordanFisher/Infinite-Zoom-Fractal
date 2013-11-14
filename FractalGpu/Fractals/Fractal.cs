using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FractalGpu.Core;

namespace FractalGpu
{
	public class Fractal
	{
		public virtual Complex ViewWholeFractal_Pos  { get { return Complex.Zero; } }
		public virtual double  ViewWholeFractal_Zoom { get { return .001; } }

		protected EzEffect Fx;
		
		protected Complex c = (Complex)0;
		protected int CorrectionOrder = 4;

		public virtual void SetTime(float t)
		{
		}

		public virtual void SetGpuParameters(Texture2D ReferenceFractal,
											 Expansion ex,
											 int Count,
											 Complex CamPos, double AspectRatio)
		{
			Fx.xTexture.SetValue(ReferenceFractal);

			Fx.c.SetValue(c.ToVector2());

			Fx.h1.SetValue(ex.a1.ToVector2());
			Fx.h2.SetValue(ex.a2.ToVector2());
			Fx.h3.SetValue(ex.a3.ToVector2());
			Fx.h4.SetValue(ex.a4.ToVector2());
			Fx.Center.SetValue(ex.a0.ToVector2());
			Fx.Count.SetValue(Count);

			Fx.CamPos.SetValue(CamPos.ToVector2());
			Fx.Set(Vector2.Zero, 1, (float)AspectRatio);
		}

		public virtual Expansion InitializeExpansion(Complex CamPos, Complex Size)
		{
			Expansion expansion = new Expansion(CamPos, Size);
			expansion.a1  = (Complex)1;
			expansion.a2 = (Complex)0;
			expansion.a3 = (Complex)0;
			expansion.a4 = (Complex)0;

			return expansion;
		}

		public virtual Complex Iterate(Complex z)
		{
			return (Complex)0;
		}

		public virtual void IterateExpansion(ref Expansion ex)
		{
		}
	}
}
