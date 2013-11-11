using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using FractalGpu.Core;

namespace FractalGpu
{
	public class Fractal
	{
		public virtual Complex ViewWholeFractal_Pos  { get { return Complex.Zero; } }
		public virtual double  ViewWholeFractal_Zoom { get { return .001; } }

		protected EzEffect Fx;
		
		protected Complex c = (Complex)0;
		protected int CorrectionOrder = 1;

		public virtual void SetTime(float t)
		{
		}

		public virtual void SetGpuParameters(float[] MinDist, Vector2[] MinPoints, int NumMinPoints,
											 Complex h, Complex h2, Complex h3, Complex h4, Complex Center,
											 int count, float D,
											 Complex CamPos, double AspectRatio)
		{
			Fx.c.SetValue(c.ToVector2());

			Fx.MinDist.SetValue(MinDist);
			Fx.MinPoints.SetValue(MinPoints);
			Fx.NumMinPoints.SetValue(NumMinPoints);

			Fx.Rotate.SetValue(h.ToVector2());
			Fx.h2.SetValue(h2.ToVector2());
			Fx.h3.SetValue(h3.ToVector2());
			Fx.h4.SetValue(h4.ToVector2());
			Fx.Center.SetValue(Center.ToVector2());
			Fx.Count.SetValue(count);
			Fx.D.SetValue(D);

			Fx.CamPos.SetValue(CamPos.ToVector2());
			Fx.Set(Vector2.Zero, 1, (float)AspectRatio);
		}

		public virtual void InitializeExpansion(Complex CamPos, ref Complex h, ref Complex h2, ref Complex h3, ref Complex h4, ref Complex Center)
		{
			h =  (Complex)1;
			h2 = (Complex)0;
			h3 = (Complex)0;
			h4 = (Complex)0;
			Center = CamPos;
		}

		public virtual Complex Iterate(Complex z)
		{
			return (Complex)0;
		}

		public virtual void IterateExpansion(Complex Center, ref Complex h, ref Complex h2, ref Complex h3, ref Complex h4)
		{
		}
	}
}
