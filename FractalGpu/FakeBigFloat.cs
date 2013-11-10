using System;

namespace BigNum
{
    public class BigFloat
    {
        double val;
        public BigFloat(double value, PrecisionSpec mantissaPrec)
        {
            val = value;
        }

        public static explicit operator BigFloat(double value)
        {
            return new BigFloat(value, spec);
        }

        public bool IsZero() { return val == 0; }

        //************************* Operators **************************

        /// <summary>
        /// The addition operator
        /// </summary>
        public static BigFloat operator +(BigFloat n1, BigFloat n2)
        {
            return (BigFloat)(n1.val + n2.val);
        }

        /// <summary>
        /// The subtraction operator
        /// </summary>
        public static BigFloat operator -(BigFloat n1, BigFloat n2)
        {
            return (BigFloat)(n1.val - n2.val);
        }

        /// <summary>
        /// The multiplication operator
        /// </summary>
        public static BigFloat operator *(BigFloat n1, BigFloat n2)
        {
            return (BigFloat)(n1.val * n2.val);
        }

        /// <summary>
        /// The division operator
        /// </summary>
        public static BigFloat operator /(BigFloat n1, BigFloat n2)
        {
            return (BigFloat)(n1.val / n2.val);
        }

        //public static implicit operator BigFloat(double v)
        //{
        //    return new BigFloat(v, spec);
        //}

        public static implicit operator float(BigFloat f)
        {
            return (float)f.val;
        }

        public static PrecisionSpec spec = new PrecisionSpec(100, PrecisionSpec.BaseType.QWORDS);
        public static BigFloat operator *(double n1, BigFloat n2)
        {
            return new BigFloat(n1, spec) * n2;
        }
        public static BigFloat operator *(BigFloat n1, double n2)
        {
            return new BigFloat(n2, spec) * n1;
        }
        public static BigFloat operator /(double n1, BigFloat n2)
        {
            return new BigFloat(n1, spec) / n2;
        }
        public static BigFloat operator /(BigFloat n1, double n2)
        {
            return n1 / new BigFloat(n2, spec);
        }
        public static BigFloat operator +(double n1, BigFloat n2)
        {
            return new BigFloat(n1, spec) + n2;
        }
        public static BigFloat operator +(BigFloat n1, double n2)
        {
            return new BigFloat(n2, spec) + n1;
        }
        public static BigFloat operator -(double n1, BigFloat n2)
        {
            return new BigFloat(n1, spec) - n2;
        }
        public static BigFloat operator -(BigFloat n1, double n2)
        {
            return new BigFloat(n2, spec) - n1;
        }
        //public static implicit operator float(BigFloat v)
        //{
        //    return (float)v.val;
        //    //return 0;
        //    //return float.Parse(v.ToString());
        //}

        public static BigFloat operator *(int n1, BigFloat n2)
        {
            return new BigFloat(n1, spec) * n2;
        }
        public static BigFloat operator *(BigFloat n1, int n2)
        {
            return new BigFloat(n2, spec) * n1;
        }
        public static BigFloat operator /(int n1, BigFloat n2)
        {
            return new BigFloat(n1, spec) / n2;
        }
        public static BigFloat operator /(BigFloat n1, int n2)
        {
            return new BigFloat(n2, spec) / n1;
        }
        public static BigFloat operator +(int n1, BigFloat n2)
        {
            return new BigFloat(n1, spec) + n2;
        }
        public static BigFloat operator +(BigFloat n1, int n2)
        {
            return new BigFloat(n2, spec) + n1;
        }
        public static BigFloat operator -(int n1, BigFloat n2)
        {
            return new BigFloat(n1, spec) - n2;
        }
        public static BigFloat operator -(BigFloat n1, int n2)
        {
            return new BigFloat(n2, spec) - n1;
        }
    }
}