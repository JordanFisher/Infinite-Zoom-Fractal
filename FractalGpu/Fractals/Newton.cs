using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using FractalGpu.Core;

namespace FractalGpu
{
	public class Newton : Fractal
	{
		public Newton()
		{
			Fx = new EzEffect(Tools.TheGame.Content, "Shaders\\Standard");
		}

		public override Complex Iterate(Complex z)
		{
			return z - (z * z * z - (Complex)1) / ((Complex)3 * z * z); 
		}

		public override void SetTime(float t)
		{
		}
	}
}
