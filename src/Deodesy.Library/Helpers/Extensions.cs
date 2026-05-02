using System;

namespace Deodesy.Library.Helpers
{
    public static class Extensions
    {
        public static double ToFixed(this double value, int dp = 2) => Math.Round(value, dp);
        
        public static double ToRadians(this double value) => value * Math.PI / 180;

        public static double ToDegrees(this double value) => value * 180;

        public static double Wrap90(this double value) => Dms.Wrap90(value);
        
        public static double Wrap180(this double value) => Dms.Wrap180(value);
        
        public static double Wrap360(this double value) => Dms.Wrap360(value);
    }
}