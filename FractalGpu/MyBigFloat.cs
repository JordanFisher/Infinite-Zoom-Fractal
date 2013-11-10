using System;
using System.Numerics;

namespace BigNum
{
    public class BigFloat
    {
        // Singleton
        //protected void InitSingleton()
        //{
        //    Init();
        //}
        //static readonly BigFloat instance = new BigFloat(0);
        //public static BigFloat Instance { get { return instance; } }


        public static void Init()
        {
            //N = 40;
            //One = BigInteger.Pow(new BigInteger(2), N);
        }

        static int FloatN = 30;
        static int FloatBig = 2 << (FloatN - 1);
        public static int N = 4000;
        public static BigInteger One = BigInteger.Pow(new BigInteger(2), N);

        public BigInteger Val;

        public BigFloat(BigInteger Val)
        {
            this.Val = Val;
        }
        public BigFloat(double value, PrecisionSpec mantissaPrec)
        {
            Val = (new BigInteger(value * FloatBig) << N) >> FloatN;
            //Console.WriteLine("{0}", (float)this);
        }

        //************************* Operators **************************

        public bool IsZero()
        {
            return Val == 0;
        }

        /// <summary>
        /// The addition operator
        /// </summary>
        public static BigFloat operator +(BigFloat n1, BigFloat n2)
        {
            return new BigFloat(n1.Val + n2.Val);
        }

        /// <summary>
        /// The subtraction operator
        /// </summary>
        public static BigFloat operator -(BigFloat n1, BigFloat n2)
        {
            return new BigFloat(n1.Val - n2.Val);
        }

        /// <summary>
        /// The multiplication operator
        /// </summary>
        public static BigFloat operator *(BigFloat n1, BigFloat n2)
        {
            return new BigFloat((n1.Val * n2.Val) >> N);
        }

        /// <summary>
        /// The division operator
        /// </summary>
        public static BigFloat operator /(BigFloat n1, BigFloat n2)
        {
            //Console.WriteLine("{0}", (float)n1);
            //Console.WriteLine("{0}", (float)n2);
            return new BigFloat((n1.Val << N) / n2.Val);
        }

        //public static implicit operator BigFloat(double v)
        //{
        //    return new BigFloat(v, spec);
        //}

        public static explicit operator BigFloat(double v)
        {
            return new BigFloat(v, spec);
        }
        public static explicit operator BigFloat(int v)
        {
            return new BigFloat(v, spec);
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

        public static implicit operator float(BigFloat v)
        {
            return (float)(v.Val >> (N - FloatN)) / FloatBig;
        }

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
            return n1 / new BigFloat(n2, spec);
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
            return n1 - new BigFloat(n2, spec);
        }
    }
}