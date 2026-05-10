namespace Deodesy.Library.Helpers;

/// <summary>
/// Provides extension methods for various numeric operations, particularly for geographical calculations.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Rounds a double value to a specified number of decimal places.
    /// </summary>
    /// <param name="value">The double value to round.</param>
    /// <param name="dp">The number of decimal places to round to. Defaults to 2.</param>
    /// <returns>The rounded double value.</returns>
    /// <example>
    /// <code>
    /// double num = 123.45678;
    /// Console.WriteLine(num.ToFixed(2)); // Output: 123.46
    /// </code>
    /// </example>
    public static double ToFixed(this double value, int dp = 2) => Math.Round(value, dp);
    
    /// <summary>
    /// Converts a degree value to radians.
    /// </summary>
    /// <param name="value">The degree value to convert.</param>
    /// <returns>The converted value in radians.</returns>
    /// <example>
    /// <code>
    /// double degrees = 90;
    /// Console.WriteLine(degrees.ToRadians()); // Output: 1.5707963267948966 (PI/2)
    /// </code>
    /// </example>
    public static double ToRadians(this double value) => value * Math.PI / 180;

    /// <summary>
    /// Converts a radian value to degrees.
    /// </summary>
    /// <param name="value">The radian value to convert.</param>
    /// <returns>The converted value in degrees.</returns>
    /// <example>
    /// <code>
    /// double radians = Math.PI / 2;
    /// Console.WriteLine(radians.ToDegrees()); // Output: 90
    /// </code>
    /// </example>
    public static double ToDegrees(this double value) => value * 180 / Math.PI;

    /// <summary>
    /// Wraps a degree value to the range -90 to +90, typically used for latitude.
    /// </summary>
    /// <param name="value">The degree value to wrap.</param>
    /// <returns>The wrapped degree value.</returns>
    /// <example>
    /// <code>
    /// Console.WriteLine(91.0.Wrap90());  // Output: 89
    /// Console.WriteLine((-91.0).Wrap90()); // Output: -89
    /// </code>
    /// </example>
    public static double Wrap90(this double value) => Dms.Wrap90(value);
    
    /// <summary>
    /// Wraps a degree value to the range -180 to +180, typically used for longitude.
    /// </summary>
    /// <param name="value">The degree value to wrap.</param>
    /// <returns>The wrapped degree value.</returns>
    /// <example>
    /// <code>
    /// Console.WriteLine(181.0.Wrap180());  // Output: -179
    /// Console.WriteLine((-181.0).Wrap180()); // Output: 179
    /// </code>
    /// </example>
    public static double Wrap180(this double value) => Dms.Wrap180(value);
    
    /// <summary>
    /// Wraps a degree value to the range 0 to 360, typically used for bearings.
    /// </summary>
    /// <param name="value">The degree value to wrap.</param>
    /// <returns>The wrapped degree value.</returns>
    /// <example>
    /// <code>
    /// Console.WriteLine(361.0.Wrap360()); // Output: 1
    /// Console.WriteLine((-1.0).Wrap360());  // Output: 359
    /// </code>
    /// </example>
    public static double Wrap360(this double value) => Dms.Wrap360(value);
}