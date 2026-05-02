using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Deodesy.Library.Helpers;

namespace Deodesy.Library
{
    public static class Dms
    {
        public static char DmsSeparator { get; set; } = '\u202f';

        /**
         * Returns a Coordinate object by parsing strings of latitude and longitude
         */
        public static Coordinate ToCoordinate(string latDms, string lonDms)
        {
            var lat = Parse(latDms);
            var lon = Parse(lonDms);

            if (lat == double.NaN || lon == double.NaN)
            {
                throw new ArgumentException();
            }

            return new Coordinate(lat, lon);
        }

        /**
         * Parses string representing degrees/minutes/seconds into numeric degrees.
         *
         * This is very flexible on formats, allowing signed decimal degrees, or deg-min-sec optionally
         * suffixed by compass direction (NSEW); a variety of separators are accepted. Examples -3.62,
         * '3 37 12W', '3°37′12″W'.
         */
        public static double Parse(string dms)
        {
            Guard.NotNullOrEmpty(dms, nameof(dms));
            
            // check for signed decimal degrees without NSEW, if so return it directly
            if (double.TryParse(dms, out double result) && !double.IsInfinity(result)) return result;

            // strip off any sign or compass dir'n & split out separate d/m/s
            string dmsParts = dms.Trim();
            dmsParts = Regex.Replace(dmsParts, "^-", "");
            dmsParts = Regex.Replace(dmsParts, "[NSEW]$", "", RegexOptions.IgnoreCase);
            List<string> dmsPartsAr = new List<string>(Regex.Split(dmsParts, "[^0-9.,]+"));
            
            if (dmsPartsAr.Count > 0 && dmsPartsAr[dmsPartsAr.Count - 1] == "") dmsPartsAr.RemoveAt(dmsPartsAr.Count - 1); // from trailing symbol
            
            if (dmsPartsAr.Count == 0) return double.NaN;

            // and convert to decimal degrees...
            double deg;
            switch (dmsPartsAr.Count)
            {
                case 3: // interpret 3-part result as d/m/s
                    deg = double.Parse(dmsPartsAr[0]) / 1 + double.Parse(dmsPartsAr[1]) / 60 + double.Parse(dmsPartsAr[2]) / 3600;
                    break;
                case 2: // interpret 2-part result as d/m
                    deg = double.Parse(dmsPartsAr[0]) / 1 + double.Parse(dmsPartsAr[1]) / 60;
                    break;
                case 1: // just d (possibly decimal) or non-separated dddmmss
                    deg = double.Parse(dmsPartsAr[0]);
                    break;
                default:
                    deg = double.NaN;
                    break;
            }

            // take '-', west and south as -ve
            if (Regex.IsMatch(dms.Trim(), "^-|[WS]$", RegexOptions.IgnoreCase)) deg = -deg;

            return deg;
        }

        private static string PadZeros(double value, int dp = 2, int nDigits = 3)
        {
            return value.ToString($"{new string('0', nDigits)}.{new string('#', dp)}");
        }

        /**
         * Converts decimal degrees to deg/min/sec format
         *  - degree, prime, double-prime symbols are added, but sign is discarded, though no compass
         *    direction is added.
         *  - degrees are zero-padded to 3 digits.
         */
        public static string ToDms(double deg, string format = "d", int dp = 2)
        {
            deg = Math.Abs(deg);

            string dms, d, m, s;
            double min, sec;
            
            switch (format)
            {
                default:
                case "d":
                case "deg":
                    d = PadZeros(deg, dp);
                    dms = d + '°';
                    break;
                case "dm":
                case "deg+min":
                    min = deg * 60 % 60;
                    deg = Math.Floor(deg);
                    if (min == 60) deg++;
                    d = PadZeros(deg, dp);
                    m = PadZeros(min < 60 ? min : 0.0, dp);
                    dms = d + '°'+ DmsSeparator + m + '′';
                    break;
                case "dms":
                case "deg+min+sec":
                    sec = deg * 3600 % 60;
                    min = Math.Floor(deg * 3600 / 60) % 60;
                    deg = Math.Floor(deg);
                    if (sec == 60) min++;
                    if (min == 60) deg++;
                    d = PadZeros(deg, dp);
                    m = PadZeros(min < 60 ? min : 0.0, dp);
                    s = PadZeros(sec < 60 ? sec : 0.0, dp);
                    dms = d + '°' + DmsSeparator + m + '′' + DmsSeparator + s + '″';
                    break;
            }

            return dms;
        }

        /**
         * Converts numeric degrees to deg/min/sec latitude (2-digit degrees, suffixed with N/S).
         */
        public static string ToLat(double deg, string format = "d", int dp = 2)
        {
            return ToDms(Wrap90(deg), format, dp) + (deg < 0 ? 'S' : 'N');
        }

        /**
         * Convert numeric degrees to deg/min/sec longitude (3-digit degrees, suffixed with E/W).
         */
        public static string ToLon(double deg, string format = "d", int dp = 2)
        {
            return ToDms(Wrap180(deg), format, dp) + (deg < 0 ? 'W' : 'E');
        }

        /**
         * Converts numeric degrees to deg/min/sec as a bearing (0°..360°).
         */
        public static string ToBearing(double deg, string format = "d", int dp = 2)
        {
            return ToDms(Wrap360(deg), format, dp).Replace("360", "0");
        }
        
        /**
         * Returns compass point (to given precision) for supplied bearing.
         */
        public static string CompassPoint(double bearing, int precision = 3)
        {
            if (precision < 1 || precision > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(precision));
            }

            bearing = Wrap360(bearing);
            var cardinals = new []
            {
                "N", "NNE", "NE", "ENE",
                "E", "ESE", "SE", "SSE",
                "S", "SSW", "SW", "WSW",
                "W", "WNW", "NW", "NNW"
            };

            var n = 4 * Math.Pow(2, precision - 1);
            var index = (int) Math.Floor(Math.Round(bearing * n / 360) % n * 16 / n);
            var cardinal = cardinals[index];

            return cardinal;
        }

        /**
         * Constrain degrees to range -90..+90 (for latitude); e.g. -91 => -89, 91 => 89.
         */
        public static double Wrap90(double degrees)
        {
            if (-90 <= degrees && degrees <= 90) return degrees;
            
            // latitude wrapping requires a triangle wave function; a general triangle wave is
            //     f(x) = 4a/p ⋅ | (x-p/4)%p - p/2 | - a
            // where a = amplitude, p = period, % = modulo; however, C# '%' is a remainder operator
            // not a modulo operator - for modulo, replace 'x%n' with '((x%n)+n)%n'

            double x = degrees; int a = 90, p = 360;
            
            return 4*a/(float) p * Math.Abs((((x-p/4f)%p)+p)%p - p/2f) - a;
        }

        /**
         * Constrain degrees to range -180..+180 (for longitude); e.g. -181 => 179, 181 => -179.
         */
        public static double Wrap180(double degrees)
        {
            if (-180 <= degrees && degrees <= 180) return degrees;
            
            // longitude wrapping requires a sawtooth wave function; a general sawtooth wave is
            //     f(x) = (2ax/p - p/2) % p - a
            // where a = amplitude, p = period, % = modulo; however, C# '%' is a remainder operator
            // not a modulo operator - for modulo, replace 'x%n' with '((x%n)+n)%n'
            
            double x = degrees; int a = 90, p = 360;
            
            return (((2*a*x/p - p/2f)%p)+p)%p - a;
        }

        /**
         * Constrain degrees to range 0..360 (for bearings); e.g. -1 => 359, 361 => 1.
         */
        public static double Wrap360(double degrees)
        {
            if (0 <= degrees && degrees < 360) return degrees;
            
            // bearing wrapping requires a sawtooth wave function with a vertical offset equal to the
            // amplitude and a corresponding phase shift; this changes the general sawtooth wave function from
            //     f(x) = (2ax/p - p/2) % p - a
            // to
            //     f(x) = (2ax/p) % p
            // where a = amplitude, p = period, % = modulo; however, C# '%' is a remainder operator
            // not a modulo operator - for modulo, replace 'x%n' with '((x%n)+n)%n'
            
            double x = degrees; int a = 90, p = 360;
            
            return (((2*a*x/p)%p)+p)%p;
        }
    }
}