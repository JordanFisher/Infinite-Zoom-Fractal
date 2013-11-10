using Microsoft.Xna.Framework;
using System;

namespace FractalGpu
{
    public struct Complex
    {
        public double X, Y;

        public Complex(double a, double b)
        {
            this.X = a;
            this.Y = b;
        }

        public static implicit operator Vector2(Complex c)
        {
            return c.ToVector2();
        }

        public float Length()
        {
            return ToVector2().Length();
        }

        public double LengthSquared()
        {
            return X * X + Y * Y;
        }

        public Vector2 ToVector2()
        {
            return new Vector2((float)X, (float)Y);
        }

        public static Complex operator+(Complex z, Complex w)
        {
            return new Complex(z.X + w.X, z.Y + w.Y);
        }

        public static Complex operator-(Complex z, Complex w)
        {
            return new Complex(z.X - w.X, z.Y - w.Y);
        }

        public static Complex operator*(Complex z, Complex w)
        {
            return new Complex(z.X * w.X - z.Y * w.Y, z.X * w.Y + z.Y * w.X);
        }

        public static Complex operator*(double s, Complex w)
        {
            return new Complex(s * w.X, s * w.Y);
        }

        public static Complex operator*(Complex w, double s)
        {
            return new Complex(s * w.X, s * w.Y);
        }

        public static Complex operator *(float s, Complex w)
        {
            return new Complex((double)s * w.X, (double)s * w.Y);
        }

        public static Complex operator *(Complex w, float s)
        {
            return new Complex((double)s * w.X, (double)s * w.Y);
        }

        public static Complex operator/(Complex z, Complex w)
        {
            return new Complex(z.X * w.X + z.Y * w.Y, z.Y * w.X - z.X * w.Y) / w.LengthSquared();
        }

        public static Complex operator/(Complex z, double s)
        {
            if (s == 0) return (Complex)10000000000000;
            return new Complex(z.X / s, z.Y / s);
        }

        public static implicit operator Complex(Vector2 z)
        {
            return new Complex(z.X, z.Y);
        }

        public static explicit operator Complex(int x)
        {
            return new Complex((float)x, 0f);
        }

        public static explicit operator Complex(float x)
        {
            return new Complex(x, 0);
        }

        public static explicit operator Complex(double x)
        {
            return new Complex(x, 0);
        }

        public override string ToString()
        {
            return string.Format("{0} + {1}i", X, Y);
        }
    }

    public class FractalFunc
    {
        public static Complex Newton(Complex z)
        {
            return z - (z * z * z - (Complex)1) / ((Complex)3 * z * z); 
        }

        public static Complex Fractal(Complex z, Complex c)
        {
            return z * z + c;
        }

        public static Complex GoldenMean_rho = new Complex(-0.74405117795419151, -0.66812262690690249);
        public static Complex GoldenMean(Complex z)
        {
            return z * z + GoldenMean_rho * z;
        }
    }
}