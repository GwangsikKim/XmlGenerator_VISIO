using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SmartDesign.MathUtil
{
    public struct Angle
    {
        private double radian;

        public static readonly Angle PI = new Angle(System.Math.PI);
        public static readonly Angle PI2 = new Angle(System.Math.PI * 0.5);
        public static readonly Angle PI4 = new Angle(System.Math.PI * 0.25);

        public Angle(double radian)
        {
            this.radian = radian;
        }

        public static Angle Parse(string s)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentNullException("s");

            double degree = Convert.ToDouble(s);
            return FromDegree(degree);
        }

        public static bool TryParse(string s, out Angle result)
        {
            try
            {
                result = Parse(s);
                return true;
            }
            catch
            {
                result = new Angle(0.0);
            }

            return false;
        }
        
        public static Angle FromDegree(double degree)
        {
            double radian = ConvertDegreeToRadian(degree);
            return new Angle(radian);
        }

        public static Angle FromRadian(double radian)
        {
            return new Angle(radian);
        }

        public static double ConvertRadianToDegree(double radian)
        {
            return 180.0 * radian / System.Math.PI;
        }

        public static double ConvertDegreeToRadian(double degree)
        {
            return System.Math.PI * degree / 180.0;
        }
        
        public double Degree
        {
            get { return ConvertRadianToDegree(radian); }
            set { radian = ConvertDegreeToRadian(value); }
        }

        public double Radian
        {
            get { return radian; }
            set { radian = value; }
        }

        public static Angle operator +(Angle lhs, Angle rhs)
        {
            return new Angle(lhs.Radian + rhs.Radian);
        }

        public static Angle operator -(Angle lhs, Angle rhs)
        {
            return new Angle(lhs.Radian - rhs.Radian);
        }

        public static Angle operator *(double d, Angle rhs)
        {
            return new Angle(d * rhs.Radian);
        }

        public static Angle operator *(Angle lhs, double d)
        {
            return d * lhs;
        }

        public static Angle operator /(Angle lhs, double d)
        {
            return new Angle(lhs.Radian / d);
        }

        public static Angle operator -(Angle a)
        {
            return -1.0 * a;
        }

        public override string ToString()
        {
            return string.Format("{0:f}", Degree);
        }
    }
}
