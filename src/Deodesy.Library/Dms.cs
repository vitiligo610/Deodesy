using System.Text.RegularExpressions;
using Deodesy.Library.Helpers;

namespace Deodesy.Library;

/// <summary>
/// Provides methods for handling Degrees, Minutes, Seconds (DMS) conversions and formatting.
/// </summary>
public static class Dms
{
    /// <summary>
    /// Gets or sets the character used to separate degrees, minutes, and seconds in DMS strings.
    /// Defaults to a narrow no-break space (U+202F).
    /// </summary>
    public static char DmsSeparator { get; set; } = '\u202f';

    /// <summary>
    /// Returns a <see cref="Coordinate"/> object by parsing strings of latitude and longitude in DMS format.
    /// </summary>
    /// <param name="latDms">The latitude string in DMS format (e.g., "53°08′45″N").</param>
    /// <param name="lonDms">The longitude string in DMS format (e.g., "001°50′00″W").</param>
    /// <returns>A <see cref="Coordinate"/> object representing the parsed latitude and longitude.</returns>
    /// <exception cref="ArgumentException">Thrown if either latitude or longitude cannot be parsed.</exception>
    /// <example>
    /// <code>
    /// var coord = Dms.ToCoordinate("53°08′45″N", "001°50′00″W");
    /// Console.WriteLine($"Latitude: {coord.Latitude}, Longitude: {coord.Longitude}");
    /// // Output: Latitude: 53.145833333333336, Longitude: -1.8333333333333333
    /// </code>
    /// </example>
    public static Coordinate ToCoordinate(string latDms, string lonDms)
    {
        var lat = Parse(latDms);
        var lon = Parse(lonDms);

        if (double.IsNaN(lat) || double.IsNaN(lon))
        {
            throw new ArgumentException("Invalid DMS string provided for latitude or longitude.");
        }

        return new Coordinate(lat, lon);
    }

    /// <summary>
    /// Parses a string representing degrees/minutes/seconds into numeric degrees.
    /// </summary>
    /// <remarks>
    /// This method is very flexible on formats, allowing signed decimal degrees, or deg-min-sec optionally
    /// suffixed by compass direction (NSEW); a variety of separators are accepted.
    /// </remarks>
    /// <param name="dms">The DMS string to parse (e.g., "-3.62", "3 37 12W", "3°37′12″W").</param>
    /// <returns>The parsed value in decimal degrees, or <see cref="double.NaN"/> if parsing fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the input string is null or empty.</exception>
    /// <example>
    /// <code>
    /// Console.WriteLine(Dms.Parse("53°08′45″N")); // Output: 53.145833333333336
    /// Console.WriteLine(Dms.Parse("3 37 12W"));   // Output: -3.62
    /// Console.WriteLine(Dms.Parse("-120.5"));     // Output: -120.5
    /// </code>
    /// </example>
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

    /// <summary>
    /// Pads a double value with leading zeros for display.
    /// </summary>
    /// <param name="value">The double value to pad.</param>
    /// <param name="dp">The number of decimal places.</param>
    /// <param name="nDigits">The minimum number of digits before the decimal point.</param>
    /// <returns>A string representation of the padded value.</returns>
    public static string PadZeros(double value, int dp = 2, int nDigits = 3)
    {
        return value.ToString($"{new string('0', nDigits)}.{new string('0', dp)}");
    }

    /// <summary>
    /// Converts decimal degrees to deg/min/sec format.
    /// </summary>
    /// <remarks>
    /// Degree, prime, double-prime symbols are added, but sign is discarded, though no compass
    /// direction is added. Degrees are zero-padded to 3 digits.
    /// </remarks>
    /// <param name="deg">The decimal degrees value.</param>
    /// <param name="format">The format to use: "d" (degrees), "dm" (degrees and minutes), or "dms" (degrees, minutes, and seconds).</param>
    /// <param name="dp">The number of decimal places for seconds (or minutes if format is "dm", or degrees if format is "d").</param>
    /// <returns>A string representation of the degrees in the specified DMS format.</returns>
    /// <example>
    /// <code>
    /// Console.WriteLine(Dms.ToDms(53.145833, "dms", 0)); // Output: 053° 008´ 045"
    /// Console.WriteLine(Dms.ToDms(53.145833, "dm", 2));  // Output: 053° 008.75′
    /// Console.WriteLine(Dms.ToDms(53.145833, "d", 4));   // Output: 053.1458°
    /// </code>
    /// </example>
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
                min = Math.Round(deg * 60 % 60, dp);
                deg = Math.Floor(deg);
                if (min >= 60)
                {
                    min = 0;
                    deg++;
                }

                d = PadZeros(deg, 0);
                m = PadZeros(min, dp);
                dms = d + '°' + DmsSeparator + m + '′';
                break;
            case "dms":
            case "deg+min+sec":
                sec = Math.Round(deg * 3600 % 60, dp);
                min = Math.Round(Math.Floor(deg * 3600 / 60) % 60, dp);
                deg = Math.Floor(deg);
                if (sec >= 60)
                {
                    sec = 0;
                    min++;
                }

                if (min >= 60)
                {
                    min = 0;
                    deg++;
                }

                d = PadZeros(deg, 0);
                m = PadZeros(min, 0);
                s = PadZeros(sec, dp);
                dms = d + '°' + DmsSeparator + m + '′' + DmsSeparator + s + '″';
                break;
        }

        return dms;
    }

    /// <summary>
    /// Converts numeric degrees to deg/min/sec latitude format (2-digit degrees, suffixed with N/S).
    /// </summary>
    /// <param name="deg">The decimal degrees latitude value.</param>
    /// <param name="format">The format to use: "d", "dm", or "dms".</param>
    /// <param name="dp">The number of decimal places.</param>
    /// <returns>A string representation of the latitude in DMS format with N/S suffix.</returns>
    /// <example>
    /// <code>
    /// Console.WriteLine(Dms.ToLat(53.145833, "dms", 0));  // Output: 053° 008′ 045″ N
    /// Console.WriteLine(Dms.ToLat(-25.1234, "dm", 2));   // Output: 025° 007.40′ S
    /// </code>
    /// </example>
    public static string ToLat(double deg, string format = "d", int dp = 2)
    {
        return ToDms(Wrap90(deg), format, dp) + DmsSeparator + (deg < 0 ? 'S' : 'N');
    }

    /// <summary>
    /// Converts numeric degrees to deg/min/sec longitude format (3-digit degrees, suffixed with E/W).
    /// </summary>
    /// <param name="deg">The decimal degrees longitude value.</param>
    /// <param name="format">The format to use: "d", "dm", or "dms".</param>
    /// <param name="dp">The number of decimal places.</param>
    /// <returns>A string representation of the longitude in DMS format with E/W suffix.</returns>
    /// <example>
    /// <code>
    /// Console.WriteLine(Dms.ToLon(-1.833333, "dms", 0)); // Output: 001° 050′ 000″ W
    /// Console.WriteLine(Dms.ToLon(150.7654, "dm", 2));  // Output: 150° 045.92′ E
    /// </code>
    /// </example>
    public static string ToLon(double deg, string format = "d", int dp = 2)
    {
        return ToDms(Wrap180(deg), format, dp) + DmsSeparator + (deg < 0 ? 'W' : 'E');
    }

    /// <summary>
    /// Converts numeric degrees to deg/min/sec as a bearing (0°..360°).
    /// </summary>
    /// <param name="deg">The decimal degrees bearing value.</param>
    /// <param name="format">The format to use: "d", "dm", or "dms".</param>
    /// <param name="dp">The number of decimal places.</param>
    /// <returns>A string representation of the bearing in DMS format.</returns>
    /// <example>
    /// <code>
    /// Console.WriteLine(Dms.ToBearing(270.5, "dms", 0)); // Output: 270° 030´ 000"
    /// Console.WriteLine(Dms.ToBearing(-45, "d", 2));       // Output: 315.00°
    /// </code>
    /// </example>
    public static string ToBearing(double deg, string format = "d", int dp = 2)
    {
        return ToDms(Wrap360(deg), format, dp).Replace("360", "0");
    }
    
    /// <summary>
    /// Returns the compass point (to a given precision) for a supplied bearing.
    /// </summary>
    /// <param name="bearing">The bearing in degrees (0-360).</param>
    /// <param name="precision">The precision of the compass point (1 for 8 points, 2 for 16 points, 3 for 32 points). Defaults to 3.</param>
    /// <returns>A string representing the compass point (e.g., "N", "NE", "NNE").</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if precision is not between 1 and 3.</exception>
    /// <example>
    /// <code>
    /// Console.WriteLine(Dms.CompassPoint(0));    // Output: N
    /// Console.WriteLine(Dms.CompassPoint(22.5)); // Output: NNE
    /// Console.WriteLine(Dms.CompassPoint(90));   // Output: E
    /// Console.WriteLine(Dms.CompassPoint(315, 1)); // Output: N
    /// </code>
    /// </example>
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

    /// <summary>
    /// Constrains degrees to the range -90..+90 (for latitude) using a general sawtooth wave function.
    /// </summary>
    /// <param name="degrees">The degrees value to wrap.</param>
    /// <returns>The wrapped degrees value within the range -90 to 90.</returns>
    /// <example>
    /// <code>
    /// Console.WriteLine(Dms.Wrap90(91));  // Output: 89
    /// Console.WriteLine(Dms.Wrap90(-91)); // Output: -89
    /// Console.WriteLine(Dms.Wrap90(180)); // Output: 0
    /// </code>
    /// </example>
    public static double Wrap90(double degrees)
    {
        if (degrees is >= -90 and <= 90) return degrees;
        
        // latitude wrapping requires a triangle wave function; a general triangle wave is
        //     f(x) = 4a/p ⋅ | (x-p/4)%p - p/2 | - a
        // where a = amplitude, p = period, % = modulo; however, C# '%' is a remainder operator
        // not a modulo operator - for modulo, replace 'x%n' with '((x%n)+n)%n'

        double x = degrees; int a = 90, p = 360;
        
        return 4*a/(float) p * Math.Abs((((x-p/4f)%p)+p)%p - p/2f) - a;
    }

    /// <summary>
    /// Constrains degrees to the range -180..+180 (for longitude) using a general sawtooth wave function.
    /// </summary>
    /// <param name="degrees">The degrees value to wrap.</param>
    /// <returns>The wrapped degrees value within the range -180 to 180.</returns>
    /// <example>
    /// <code>
    /// Console.WriteLine(Dms.Wrap180(181));  // Output: -179
    /// Console.WriteLine(Dms.Wrap180(-181)); // Output: 179
    /// Console.WriteLine(Dms.Wrap180(360));  // Output: 0
    /// </code>
    /// </example>
    public static double Wrap180(double degrees)
    {
        if (degrees is >= -180 and <= 180) return degrees;
        
        // longitude wrapping requires a sawtooth wave function; a general sawtooth wave is
        //     f(x) = (2ax/p - p/2) % p - a
        // where a = amplitude, p = period, % = modulo; however, C# '%' is a remainder operator
        // not a modulo operator - for modulo, replace 'x%n' with '((x%n)+n)%n'
        
        double x = degrees; int a = 180, p = 360;
        
        return (((2*a*x/p - p/2f)%p)+p)%p - a;
    }

    /// <summary>
    /// Constrains degrees to the range 0..360 (for bearings) using a general sawtooth wave function.
    /// </summary>
    /// <param name="degrees">The degrees value to wrap.</param>
    /// <returns>The wrapped degrees value within the range 0 to 360.</returns>
    /// <example>
    /// <code>
    /// Console.WriteLine(Dms.Wrap360(361)); // Output: 1
    /// Console.WriteLine(Dms.Wrap360(-1));  // Output: 359
    /// Console.WriteLine(Dms.Wrap360(0));   // Output: 0
    /// </code>
    /// </example>
    public static double Wrap360(double degrees)
    {
        if (degrees is >= 0 and < 360) return degrees;
        
        // bearing wrapping requires a sawtooth wave function with a vertical offset equal to the
        // amplitude and a corresponding phase shift; this changes the general sawtooth wave function from
        //     f(x) = (2ax/p - p/2) % p - a
        // to
        //     f(x) = (2ax/p) % p
        // where a = amplitude, p = period, % = modulo; however, C# '%' is a remainder operator
        // not a modulo operator - for modulo, replace 'x%n' with '((x%n)+n)%n'
        
        double x = degrees; int a = 180, p = 360;
        
        return (((2*a*x/p)%p)+p)%p;
    }
}