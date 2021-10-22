using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.MathUtil
{
    public static class Tolerance
    {
        public static double DistanceTolerance = 1.0e-5;
        public static double AngleTolerance = 1.0e-12;
        public static double ValueTolerance = 1.0e-7;        

        public static bool IsZeroDistance(double distance)
        {
            return Math.Abs(distance) < Math.Abs(DistanceTolerance);
        }

        public static bool IsZeroDistance(double distance, double tolerance)
        {
            return Math.Abs(distance) < Math.Abs(tolerance);
        }

        public static bool IsEqualDistance(double lhs, double rhs)
        {
            return Math.Abs(lhs - rhs) < DistanceTolerance;
        }

        public static bool IsLessDistance(double lhs, double rhs)
        {
            return (lhs - rhs) < DistanceTolerance;
        }

        public static bool IsLargerDistance(double lhs, double rhs)
        {
            return (rhs - lhs) > DistanceTolerance;
        }

        public static bool IsZeroAngle(double angle)
        {
            return Math.Abs(angle) < Math.Abs(AngleTolerance);
        }

        public static bool IsZeroAngle(Angle angle)
        {
            return Math.Abs(angle.Radian) < Math.Abs(AngleTolerance);
        }

        public static bool IsEqualAngle(double lhs, double rhs)
        {
            return Math.Abs(lhs - rhs) < AngleTolerance;
        }

        public static bool IsLessAngle(double lhs, double rhs)
        {
            return (lhs - rhs) < AngleTolerance;
        }

        public static bool IsLargerAngle(double lhs, double rhs)
        {
            return (rhs - lhs) > AngleTolerance;
        }

        public static bool IsZeroValue(double value)
        {
            return Math.Abs(value) < Math.Abs(ValueTolerance);
        }

        public static bool IsEqualValue(double lhs, double rhs)
        {
            return Math.Abs(lhs - rhs) < ValueTolerance;
        }

        public static bool IsLessValue(double lhs, double rhs)
        {
            return (lhs - rhs) < ValueTolerance;
        }

        public static bool IsLargerValue(double lhs, double rhs)
        {
            return (rhs - lhs) > ValueTolerance;
        }
    }
}
