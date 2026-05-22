using Deodesy.Library.Helpers;

namespace Deodesy.Library;

/// <summary>
/// Provides functions for spherical geodesy, based on the Haversine formula and other spherical geometry calculations.
/// </summary>
public class LatLonSpherical
{
    private const double EarthRadiusMeters = 6371e3; // Mean radius of earth (m)

    /// <summary>
    /// Calculates the distance (in meters) between two points on the Earth's surface
    /// using the Haversine formula.
    /// </summary>
    /// <remarks>
    /// See https://mathforum.org/library/drmath/view/51879.html for derivation
    /// </remarks>
    /// <param name="startPoint">The starting coordinate.</param>
    /// <param name="endPoint">The ending coordinate.</param>
    /// <returns>The distance in meters.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="startPoint"/> or <paramref name="endPoint"/> is null.</exception>
    /// <example>
    /// <code>
    /// var p1 = new Coordinate(52.205, 0.119);
    /// var p2 = new Coordinate(48.857, 2.351);
    /// double distance = LatLonSpherical.Distance(p1, p2); // 404,279.16 m
    /// </code>
    /// </example>
    public static double Distance(Coordinate startPoint, Coordinate endPoint)
    {
        Guard.NotNull(startPoint, nameof(startPoint));
        Guard.NotNull(endPoint, nameof(endPoint));
        
        // a = sin²(Δφ/2) + cos(φ1)⋅cos(φ2)⋅sin²(Δλ/2)
        // δ = 2·atan2(√(a), √(1−a))
        // see mathforum.org/library/drmath/view/51879.html for derivation
        
        var deltaLat = endPoint.LatitudeR - startPoint.LatitudeR;
        var deltaLon = endPoint.LongitudeR - startPoint.LongitudeR;
        var originLat = startPoint.LatitudeR;
        var destinationLat = endPoint.LatitudeR;
        
        var a = Math.Pow(Math.Sin(deltaLat / 2), 2) + Math.Pow(Math.Sin(deltaLon / 2), 2) * Math.Cos(originLat) * Math.Cos(destinationLat);
        var c = 2 * Math.Asin(Math.Sqrt(a));
        return EarthRadiusMeters * c;
    }

    /// <summary>
    /// Calculates the distance (in nautical miles) between two points on the Earth's surface
    /// using the Haversine formula.
    /// </summary>
    /// <param name="startPoint">The starting coordinate.</param>
    /// <param name="endPoint">The ending coordinate.</param>
    /// <returns>The distance in nautical miles.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="startPoint"/> or <paramref name="endPoint"/> is null.</exception>
    /// <example>
    /// <code>
    /// var p1 = new Coordinate(52.205, 0.119);
    /// var p2 = new Coordinate(48.857, 2.351);
    /// double distanceNm = LatLonSpherical.DistanceNm(p1, p2); // 218.29 nm 
    /// </code>
    /// </example>
    public static double DistanceNm(Coordinate startPoint, Coordinate endPoint)
    {
        Guard.NotNull(startPoint, nameof(startPoint));
        Guard.NotNull(endPoint, nameof(endPoint));
        
        var distanceKm = Distance(startPoint, endPoint) / 1000;
        return distanceKm * 0.539956803; // 1 km = 0.539956803 nautical miles
    }

    /// <summary>
    /// Calculates the initial bearing (direction) from a start point to an end point.
    /// </summary>
    /// <remarks>
    /// See https://mathforum.org/library/drmath/view/55417.html for derivation
    /// </remarks>
    /// <param name="startPoint">The starting coordinate.</param>
    /// <param name="endPoint">The ending coordinate.</param>
    /// <returns>The initial bearing in degrees, in the range 0 to 360. If both points are same, returns null</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="startPoint"/> or <paramref name="endPoint"/> is null.</exception>
    /// <example>
    /// <code>
    /// var p1 = new Coordinate(52.205, 0.119);
    /// var p2 = new Coordinate(48.857, 2.351);
    /// double bearing = LatLonSpherical.InitialBearing(p1, p2); // 156.2°
    /// </code>
    /// </example>
    public static double? InitialBearing(Coordinate startPoint, Coordinate endPoint)
    {
        Guard.NotNull(startPoint, nameof(startPoint));
        Guard.NotNull(endPoint, nameof(endPoint));
        
        if (startPoint == endPoint) return null;
        
        // tanθ = sinΔλ⋅cosφ2 / cosφ1⋅sinφ2 − sinφ1⋅cosφ2⋅cosΔλ
        // see mathforum.org/library/drmath/view/55417.html for derivation
        
        var deltaLon = endPoint.LongitudeR - startPoint.LongitudeR;
        var y = Math.Sin(deltaLon) * Math.Cos(endPoint.LatitudeR);
        var x = Math.Cos(startPoint.LatitudeR) * Math.Sin(endPoint.LatitudeR) -
                Math.Sin(startPoint.LatitudeR) * Math.Cos(endPoint.LatitudeR) * Math.Cos(deltaLon);
        var phi = Math.Atan2(y, x);

        return phi.ToDegrees().Wrap360();
    }
    
    /// <summary>
    /// Calculates the final bearing (direction) when arriving at an end point from a start point.
    /// </summary>
    /// <remarks>
    /// The final bearing will differ from the initial bearing by varying degrees according to distance and latitude.
    /// </remarks>
    /// <param name="startPoint">The starting coordinate.</param>
    /// <param name="endPoint">The ending coordinate.</param>
    /// <returns>The final bearing in degrees, in the range 0 to 360.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="startPoint"/> or <paramref name="endPoint"/> is null.</exception>
    /// <example>
    /// <code>
    /// var p1 = new Coordinate(52.205, 0.119);
    /// var p2 = new Coordinate(48.857, 2.351);
    /// double finalBearing = LatLonSpherical.FinalBearing(p1, p2); // 157.9°
    /// </code>
    /// </example>
    public static double? FinalBearing(Coordinate startPoint, Coordinate endPoint)
    {
        Guard.NotNull(startPoint, nameof(startPoint));
        Guard.NotNull(endPoint, nameof(endPoint));
        
        // The final bearing is the initial bearing from the end point to the start point, plus 180 degrees.
        var bearing = InitialBearing(endPoint, startPoint) + 180;
        
        return bearing?.Wrap360();
    }
    
    /// <summary>
    /// Calculates the midpoint between two points on the Earth's surface.
    /// </summary>
    /// <param name="startPoint">The starting coordinate.</param>
    /// <param name="endPoint">The ending coordinate.</param>
    /// <returns>A <see cref="Coordinate"/> object representing the midpoint.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="startPoint"/> or <paramref name="endPoint"/> is null.</exception>
    /// <example>
    /// <code>
    /// var p1 = new Coordinate(52.205, 0.119);
    /// var p2 = new Coordinate(48.857, 2.351);
    /// var midpoint = LatLonSpherical.MidPoint(p1, p2); // 50.5363°N, 1.2746°E
    /// </code>
    /// </example>
    public static Coordinate MidPoint(Coordinate startPoint, Coordinate endPoint)
    {
        Guard.NotNull(startPoint, nameof(startPoint));
        Guard.NotNull(endPoint, nameof(endPoint));
        
        // φm = atan2( sinφ1 + sinφ2, √( (cosφ1 + cosφ2⋅cosΔλ)² + cos²φ2⋅sin²Δλ ) )
        // λm = λ1 + atan2(cosφ2⋅sinΔλ, cosφ1 + cosφ2⋅cosΔλ)
        // midpoint is sum of vectors to two points: mathforum.org/library/drmath/view/51822.html

        var deltaLon = endPoint.LongitudeR - startPoint.LongitudeR;
        var bx = Math.Cos(endPoint.LatitudeR) * Math.Cos(deltaLon);
        var by = Math.Cos(endPoint.LatitudeR) * Math.Sin(deltaLon);

        var lat = Math.Atan2(Math.Sin(startPoint.LatitudeR) + Math.Sin(endPoint.LatitudeR),
            Math.Sqrt((Math.Cos(startPoint.LatitudeR) + bx) * (Math.Cos(startPoint.LatitudeR) + bx) + by * by));

        var lon = startPoint.LongitudeR + Math.Atan2(by, Math.Cos(startPoint.LatitudeR) + bx);

        return new Coordinate(lat.ToDegrees(), lon.ToDegrees());
    }

    /// <summary>
    /// Calculates the point at a given fraction along the great-circle path between two points.
    /// </summary>
    /// <param name="startPoint">The starting coordinate.</param>
    /// <param name="endPoint">The ending coordinate.</param>
    /// <param name="fraction">The fraction along the path (0.0 for startPoint, 1.0 for endPoint, 0.5 for midpoint).</param>
    /// <returns>A <see cref="Coordinate"/> object representing the intermediate point.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="startPoint"/> or <paramref name="endPoint"/> is null.</exception>
    /// <example>
    /// <code>
    /// var p1 = new Coordinate(52.205, 0.119);
    /// var p2 = new Coordinate(48.857, 2.351);
    /// var intermediatePoint = LatLonSpherical.IntermediatePoint(p1, p2, 0.25); // 51.3721°N, 0.7073°E
    /// </code>
    /// </example>
    public static Coordinate IntermediatePoint(Coordinate startPoint, Coordinate endPoint, double fraction)
    {
        Guard.NotNull(startPoint, nameof(startPoint));
        Guard.NotNull(endPoint, nameof(endPoint));

        if (startPoint == endPoint) return startPoint;

        var deltaLat = endPoint.LatitudeR - startPoint.LatitudeR;
        var deltaLon = endPoint.LongitudeR - startPoint.LongitudeR;
        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) + Math.Cos(startPoint.LatitudeR) *
            Math.Cos(endPoint.LatitudeR) * Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        
        var delta = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)); // angular distance in radians

        var A = Math.Sin((1 - fraction) * delta) / Math.Sin(delta);
        var B = Math.Sin(fraction * delta) / Math.Sin(delta);

        var x = A * Math.Cos(startPoint.LatitudeR) * Math.Cos(startPoint.LongitudeR) +
                B * Math.Cos(endPoint.LatitudeR) * Math.Cos(endPoint.LongitudeR);
        var y = A * Math.Cos(startPoint.LatitudeR) * Math.Sin(startPoint.LongitudeR) +
                B * Math.Cos(endPoint.LatitudeR) * Math.Sin(endPoint.LongitudeR);
        var z = A * Math.Sin(startPoint.LatitudeR) + B * Math.Sin(endPoint.LatitudeR);

        var lat = Math.Atan2(z, Math.Sqrt(x * x + y * y));
        var lon = Math.Atan2(y, x);

        return new Coordinate(lat.ToDegrees(), lon.ToDegrees());
    }
    
    /// <summary>
    /// Calculates the destination point and final bearing when traveling along a great circle arc
    /// for a given start point, initial bearing, and distance.
    /// </summary>
    /// <remarks>
    /// See https://mathforum.org/library/drmath/view/52049.html for derivation
    /// </remarks>
    /// <param name="startPoint">The starting coordinate.</param>
    /// <param name="distance">The distance to travel in same units as earth radius (meters).</param>
    /// <param name="bearing">The initial bearing in degrees from north.</param>
    /// <returns>A <see cref="Coordinate"/> object representing the destination point.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="startPoint"/> is null.</exception>
    /// <example>
    /// <code>
    /// var p1 = new Coordinate(51.47788, -0.00147);
    /// var destPoint = LatLonSpherical.DestinationPoint(p1, 7794, 300.7); // 51.5136°N, 000.0983°W
    /// </code>
    /// </example>
    public static Coordinate DestinationPoint(Coordinate startPoint, double distance, double bearing)
    {
        Guard.NotNull(startPoint, nameof(startPoint));
        
        // sinφ2 = sinφ1⋅cosδ + cosφ1⋅sinδ⋅cosθ
        // tanΔλ = sinθ⋅sinδ⋅cosφ1 / cosδ−sinφ1⋅sinφ2
        // see mathforum.org/library/drmath/view/52049.html for derivation

        var angularDistance = distance / EarthRadiusMeters;

        var sinLat = Math.Sin(startPoint.LatitudeR) * Math.Cos(angularDistance) +
                     Math.Cos(startPoint.LatitudeR) * Math.Sin(angularDistance) *
                     Math.Cos(bearing.ToRadians());
        var lat = Math.Asin(sinLat);
        var y = Math.Sin(bearing.ToRadians()) * Math.Sin(angularDistance) * Math.Cos(startPoint.LatitudeR);
        var x = Math.Cos(angularDistance) - Math.Sin(startPoint.LatitudeR) * sinLat; // Corrected x calculation
        var lon = startPoint.LongitudeR + Math.Atan2(y, x);

        return new Coordinate(lat.ToDegrees(), lon.ToDegrees());
    }

    /// <summary>
    /// Calculates the point of intersection of two great-circle paths defined by points and bearings.
    /// </summary>
    /// <remarks>
    /// See https://www.edwilliams.org/avform.htm#Intersection
    /// </remarks>
    /// <param name="firstPoint">The starting point of the first path.</param>
    /// <param name="firstBearing">The initial bearing of the first path in degrees.</param>
    /// <param name="secondPoint">The starting point of the second path.</param>
    /// <param name="secondBearing">The initial bearing of the second path in degrees.</param>
    /// <returns>A <see cref="Coordinate"/> object representing the intersection point, or null if no intersection or paths are coincident.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="firstPoint"/> or <paramref name="secondPoint"/> is null.</exception>
    /// <example>
    /// <code>
    /// var p1 = new Coordinate(51.8853, 0.2545), brng1 = 108.547;
    /// var p2 = new Coordinate(49.0034, 2.5735), brng2 =  32.435;
    /// var intersection = LatLonSpherical.IntersectionPoint(p1, brng1, p2, brng2); // 50.9078°N, 4.5084°E
    /// </code>
    /// </example>
    public static Coordinate? IntersectionPoint(Coordinate firstPoint, double firstBearing, Coordinate secondPoint,
        double secondBearing)
    {
        Guard.NotNull(firstPoint, nameof(firstPoint));
        Guard.NotNull(secondPoint, nameof(secondPoint));
        
        // see www.edwilliams.org/avform.htm#Intersection

        var deltaLat = secondPoint.LatitudeR - firstPoint.LatitudeR;
        var deltaLon = secondPoint.LongitudeR - firstPoint.LongitudeR;

        var delta12 = 2 * Math.Asin(Math.Sqrt(Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                                              Math.Cos(firstPoint.LatitudeR) * Math.Cos(secondPoint.LatitudeR) *
                                              Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2)));
        if (Math.Abs(delta12) < 1e-12) return firstPoint; // same points (using a small epsilon for comparison)

        // initial/final bearings between points
        var cosThetaA = (Math.Sin(secondPoint.LatitudeR) - Math.Sin(firstPoint.LatitudeR) * Math.Cos(delta12)) /
                        (Math.Sin(delta12) * Math.Cos(firstPoint.LatitudeR));
        var cosThetaB = (Math.Sin(firstPoint.LatitudeR) - Math.Sin(secondPoint.LatitudeR) * Math.Cos(delta12)) /
                        (Math.Sin(delta12) * Math.Cos(secondPoint.LatitudeR));

        // Clamp values to [-1, 1] to avoid NaN from Acos due to floating point inaccuracies
        var thetaA = Math.Acos(Math.Min(Math.Max(cosThetaA, -1.0), 1.0));
        var thetaB = Math.Acos(Math.Min(Math.Max(cosThetaB, -1.0), 1.0));

        var theta12 = thetaA;
        if (Math.Sin(secondPoint.LongitudeR - firstPoint.LongitudeR) < 0)
            theta12 = 2 * Math.PI - thetaA;

        var theta21 = thetaB;
        if (Math.Sin(secondPoint.LongitudeR - firstPoint.LongitudeR) > 0)
            theta21 = 2 * Math.PI - thetaB;

        var alpha1 = firstBearing.ToRadians() - theta12;
        var alpha2 = theta21 - secondBearing.ToRadians();

        if (Math.Sin(alpha1) == 0.0 && Math.Sin(alpha2) == 0.0) return null; // infinite intersections
        if (Math.Sin(alpha1) * Math.Sin(alpha2) < 0) return null; // antipodal intersection (ambiguous)

        var cosAlpha3 = -Math.Cos(alpha1) * Math.Cos(alpha2) +
                        Math.Sin(alpha1) * Math.Sin(alpha2) * Math.Cos(delta12);

        var delta13 = Math.Atan2(Math.Sin(delta12) * Math.Sin(alpha1) * Math.Sin(alpha2),
            Math.Cos(alpha2) + Math.Cos(alpha1) * cosAlpha3);

        var lat = Math.Asin(Math.Min(
            Math.Max(
                Math.Sin(firstPoint.LatitudeR) * Math.Cos(delta13) + Math.Cos(firstPoint.LatitudeR) *
                Math.Sin(delta13) * Math.Cos(firstBearing.ToRadians()), -1.0), 1.0));

        var deltaLon13 =
            Math.Atan2(Math.Sin(firstBearing.ToRadians()) * Math.Sin(delta13) * Math.Cos(firstPoint.LatitudeR),
                Math.Cos(delta13) - Math.Sin(firstPoint.LatitudeR) * Math.Sin(lat));

        var lon = firstPoint.LongitudeR + deltaLon13;

        return new Coordinate(lat.ToDegrees(), lon.ToDegrees());
    }

    /// <summary>
    /// Calculates the cross-track distance from a current point to a great-circle path defined by two other points.
    /// </summary>
    /// <remarks>
    /// The cross-track distance is the shortest distance from the current point to the great-circle path.
    /// </remarks>
    /// <param name="currentPoint">The current coordinate.</param>
    /// <param name="startPoint">The starting coordinate of the great-circle path.</param>
    /// <param name="endPoint">The ending coordinate of the great-circle path.</param>
    /// <returns>The cross-track distance in meters (-ve if to left, +ve if to right of path).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="currentPoint"/>, <paramref name="startPoint"/>, or <paramref name="endPoint"/> is null.</exception>
    /// <example>
    /// <code>
    /// var start = new Coordinate(53.3206, -1.7297);
    /// var end = new Coordinate(53.1887, 0.1334);
    /// var current = new Coordinate(53.2611, -0.7972);
    /// double crossTrackDist = LatLonSpherical.CrossTrackDistance(current, start, end); // -307.5 m
    /// </code>
    /// </example>
    public static double CrossTrackDistance(Coordinate currentPoint, Coordinate startPoint, Coordinate endPoint)
    {
        Guard.NotNull(currentPoint, nameof(currentPoint));
        Guard.NotNull(startPoint, nameof(startPoint));
        Guard.NotNull(endPoint, nameof(endPoint));

        if (currentPoint == startPoint) return 0.0; // same point
        
        var delta13 = Distance(startPoint, currentPoint) / EarthRadiusMeters; // angular distance from start to current
        var theta13 = InitialBearing(startPoint, currentPoint)!.Value.ToRadians(); // bearing from start to current
        var theta12 = InitialBearing(startPoint, endPoint)?.ToRadians() ?? 0.0; // bearing from start to end

        var deltaCrossTrack = Math.Asin(Math.Sin(delta13) * Math.Sin(theta13 - theta12));

        return deltaCrossTrack * EarthRadiusMeters;
    }

    /// <summary>
    /// Calculates the along-track distance from a start point to the point on a great-circle path
    /// that is closest to a current point.
    /// </summary>
    /// <remarks>
    /// This is the distance from the start point to where a perpendicular from the current point
    /// crosses the great-circle path.
    /// </remarks>
    /// <param name="currentPoint">The current coordinate.</param>
    /// <param name="startPoint">The starting coordinate of the great-circle path.</param>
    /// <param name="endPoint">The ending coordinate of the great-circle path.</param>
    /// <returns>The distance along great circle to point nearest to current point in meters.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="currentPoint"/>, <paramref name="startPoint"/>, or <paramref name="endPoint"/> is null.</exception>
    /// <example>
    /// <code>
    /// var start = new Coordinate(53.3206, -1.7297);
    /// var end = new Coordinate(53.1887, 0.1334);
    /// var current = new Coordinate(53.2611, -0.7972);
    /// double alongTrackDist = LatLonSpherical.AlongTrackDistance(current, start, end); // 62,331.49 m
    /// </code>
    /// </example>
    public static double AlongTrackDistance(Coordinate currentPoint, Coordinate startPoint, Coordinate endPoint)
    {
        Guard.NotNull(currentPoint, nameof(currentPoint));
        Guard.NotNull(startPoint, nameof(startPoint));
        Guard.NotNull(endPoint, nameof(endPoint));
        
        if (currentPoint == startPoint) return 0.0; // same point
        
        var delta13 = Distance(startPoint, currentPoint) / EarthRadiusMeters; // angular distance from start to current
        var theta13 = InitialBearing(startPoint, currentPoint)!.Value.ToRadians(); // bearing from start to current
        var theta12 = InitialBearing(startPoint, endPoint)?.ToRadians() ?? 0.0; // bearing from start to end

        var deltaCrossTrack = Math.Asin(Math.Sin(delta13) * Math.Sin(theta13 - theta12));
        var deltaAlongTrack = Math.Acos(Math.Cos(delta13) / Math.Abs(Math.Cos(deltaCrossTrack)));

        return deltaAlongTrack * Math.Sign(Math.Cos(theta12 - theta13)) * EarthRadiusMeters;
    }

    /// <summary>
    /// Calculates the maximum latitude reached when traveling on a great circle from a start point
    /// with a given initial bearing (using Clairaut’s formula).
    /// </summary>
    /// <remarks>
    /// The maximum latitude is independent of longitude; it will be the same for all points on a given latitude.
    /// Negate the result for the minimum latitude (in the Southern Hemisphere).
    /// </remarks>
    /// <param name="startPoint">The starting coordinate.</param>
    /// <param name="bearing">The initial bearing in degrees.</param>
    /// <returns>The maximum latitude in degrees.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="startPoint"/> is null.</exception>
    /// <example>
    /// <code>
    /// var p1 = new Coordinate(51.51, 0.0);
    /// double maxLat = LatLonSpherical.MaxLatitude(p1, 90.0); // 51.51°
    /// </code>
    /// </example>
    public static double MaxLatitude(Coordinate startPoint, double bearing)
    {
        Guard.NotNull(startPoint, nameof(startPoint));
        
        var bearingInRad = bearing.ToRadians();
        var maxLat = Math.Acos(Math.Abs(Math.Sin(bearingInRad) * Math.Cos(startPoint.LatitudeR)));
        return maxLat.ToDegrees();
    }

    /// <summary>
    /// Calculates the pair of meridians at which a great circle defined by two points crosses a given latitude.
    /// </summary>
    /// <param name="startPoint">The starting coordinate of the great circle.</param>
    /// <param name="endPoint">The ending coordinate of the great circle.</param>
    /// <param name="latitude">The latitude (in degrees) for which to find the crossing meridians.</param>
    /// <returns>
    /// An array of two doubles representing the longitudes (in degrees, wrapped to -180 to 180)
    /// of the crossing points, or null if the great circle does not reach the given latitude.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="startPoint"/> or <paramref name="endPoint"/> is null.</exception>
    /// <example>
    /// <code>
    /// var p1 = new Coordinate(53.3206, -1.7297);
    /// var p2 = new Coordinate(53.1887, 0.1334);
    /// double[]? crossingLons = LatLonSpherical.CrossingParallels(p1, p2, 45.5); // 50.4775°S, 32.0812°N
    /// </code>
    /// </example>
    public static double[]? CrossingParallels(Coordinate startPoint, Coordinate endPoint, double latitude)
    {
        Guard.NotNull(startPoint, nameof(startPoint));
        Guard.NotNull(endPoint, nameof(endPoint));
        
        var latR = latitude.ToRadians();
        var deltaLon = endPoint.LongitudeR - startPoint.LongitudeR;

        var x = Math.Sin(startPoint.LatitudeR) * Math.Cos(endPoint.LatitudeR) * Math.Cos(latR) * Math.Sin(deltaLon);
        var y =
            Math.Sin(startPoint.LatitudeR) * Math.Cos(endPoint.LatitudeR) * Math.Cos(latR) * Math.Cos(deltaLon) -
            Math.Cos(startPoint.LatitudeR) * Math.Sin(endPoint.LatitudeR) * Math.Cos(latR);
        var z = Math.Cos(startPoint.LatitudeR) * Math.Cos(endPoint.LatitudeR) * Math.Sin(latR) * Math.Sin(deltaLon);

        if (z * z > x * x + y * y) return null; // Great circle doesn't reach this latitude

        var deltaM = Math.Atan2(-y, x);
        var deltaLoni = Math.Acos(z / Math.Sqrt(x * x + y * y));

        var deltaI1 = startPoint.LongitudeR + deltaM - deltaLoni;
        var deltaI2 = startPoint.LongitudeR + deltaM + deltaLoni;

        var lon1 = deltaI1.ToDegrees();
        var lon2 = deltaI2.ToDegrees();

        return new [] { lon1.Wrap180(), lon2.Wrap180() };
    }
    
    /* Rhumb - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    
    /// <summary>
    /// Calculates the distance (in meters) between two points along a rhumb line.
    /// </summary>
    /// <remarks>
    /// See https://www.edwilliams.org/avform.htm#Rhumb
    /// </remarks>
    /// <param name="startPoint">The starting coordinate.</param>
    /// <param name="endPoint">The ending coordinate.</param>
    /// <returns>The distance in meters.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="startPoint"/> or <paramref name="endPoint"/> is null.</exception>
    /// <example>
    /// <code>
    /// var p1 = new Coordinate(51.127, 1.338);
    /// var p2 = new Coordinate(50.964, 1.853);
    /// double distance = LatLonSpherical.RhumbDistance(p1, p2); // 40,307.75 m
    /// </code>
    /// </example>
    public static double RhumbDistance(Coordinate startPoint, Coordinate endPoint)
    {
        Guard.NotNull(startPoint, nameof(startPoint));
        Guard.NotNull(endPoint, nameof(endPoint));
        
        // see www.edwilliams.org/avform.htm#Rhumb
        
        var deltaLat = endPoint.LatitudeR - startPoint.LatitudeR;
        var deltaLon = Math.Abs(endPoint.LongitudeR - startPoint.LongitudeR);

        // if deltaLon over 180° take shorter rhumb line across the anti-meridian
        if (Math.Abs(deltaLon) > Math.PI)
        {
            deltaLon = deltaLon > 0 ? -(2 * Math.PI - deltaLon) : (2 * Math.PI + deltaLon);
        }
        
        // on Mercator projection, longitude distances shrink by latitude; q is the 'stretch factor'
        // q becomes ill-conditioned along E-W line (0/0); use empirical tolerance to avoid it (note ε is too small)
        var deltaIsometricLat = Math.Log(Math.Tan(Math.PI / 4 + endPoint.LatitudeR / 2) / Math.Tan(Math.PI / 4 + startPoint.LatitudeR / 2));
        var q = Math.Abs(deltaIsometricLat) > 10e-12 ? deltaLat / deltaIsometricLat : Math.Cos(startPoint.LatitudeR);
        
        // distance is pythagoras on 'stretched' Mercator projection, √(Δφ² + q²·Δλ²)
        var angularDistance = Math.Sqrt(deltaLat * deltaLat + q * q * deltaLon * deltaLon);
        
        return angularDistance * EarthRadiusMeters;
    }
    
    /// <summary>
    /// Calculates the bearing (direction) from a start point to an end point along a rhumb line.
    /// </summary>
    /// <param name="startPoint">The starting coordinate.</param>
    /// <param name="endPoint">The ending coordinate.</param>
    /// <returns>The bearing in degrees, in the range 0 to 360. If two points are same, returns null</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="startPoint"/> or <paramref name="endPoint"/> is null.</exception>
    /// <example>
    /// <code>
    /// var p1 = new Coordinate(51.127, 1.338);
    /// var p2 = new Coordinate(50.964, 1.853);
    /// double bearing = LatLonSpherical.RhumbBearing(p1, p2); // 116.7°
    /// </code>
    /// </example>
    public static double? RhumbBearing(Coordinate startPoint, Coordinate endPoint)
    {
        Guard.NotNull(startPoint, nameof(startPoint));
        Guard.NotNull(endPoint, nameof(endPoint));
        
        if (startPoint == endPoint) return null;
        
        var deltaLon = endPoint.LongitudeR - startPoint.LongitudeR;

        if (Math.Abs(deltaLon) > Math.PI)
        {
            deltaLon = deltaLon > 0 ? -(2 * Math.PI - deltaLon) : (2 * Math.PI + deltaLon);
        }
        
        var deltaIsometricLat = Math.Log(Math.Tan(Math.PI / 4 + endPoint.LatitudeR / 2) / Math.Tan(Math.PI / 4 + startPoint.LatitudeR / 2));
        var bearing = Math.Atan2(deltaLon, deltaIsometricLat);

        return bearing.ToDegrees().Wrap360();
    }
    
    /// <summary>
    /// Calculates the destination point and final bearing when traveling along a rhumb line
    /// for a given start point, initial bearing, and distance.
    /// </summary>
    /// <param name="startPoint">The starting coordinate.</param>
    /// <param name="distance">The distance to travel in same units as earth radius (meters).</param>
    /// <param name="bearing">The initial bearing in degrees from north.</param>
    /// <returns>A <see cref="Coordinate"/> object representing the destination point.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="startPoint"/> is null.</exception>
    /// <example>
    /// <code>
    /// var p1 = new Coordinate(51.127, 1.338);
    /// var destPoint = LatLonSpherical.RhumbDestinationPoint(p1, 40300, 116.7); // 50.9642°N, 1.8530°E
    /// </code>
    /// </example>
    public static Coordinate RhumbDestinationPoint(Coordinate startPoint, double distance, double bearing)
    {
        Guard.NotNull(startPoint, nameof(startPoint));

        var angularDistance = distance / EarthRadiusMeters;

        var deltaLat = angularDistance / bearing.ToRadians();
        var endPointLatR = startPoint.LatitudeR + deltaLat;

        if (Math.Abs(endPointLatR) > Math.PI / 2)
        {
            endPointLatR = endPointLatR > 0 ? Math.PI - endPointLatR : - Math.PI - endPointLatR;
        }
        
        var deltaIsometricLat = Math.Log(Math.Tan(Math.PI / 4 + endPointLatR / 2) / Math.Tan(Math.PI / 4 + startPoint.LatitudeR / 2));
        var q = Math.Abs(deltaIsometricLat) > 10e-12 ? deltaLat / deltaIsometricLat : Math.Cos(startPoint.LatitudeR); // E-W course becomes ill-conditioned with 0/0
        
        var deltaLon = angularDistance * Math.Sin(bearing.ToDegrees()) / q;
        var endPointLonR = startPoint.LongitudeR + deltaLon;

        return new Coordinate(endPointLatR.ToDegrees(), endPointLonR.ToDegrees());
    }
    
    /// <summary>
    /// Calculates the midpoint between two points on the Earth's surface.
    /// </summary>
    /// <param name="startPoint">The starting coordinate.</param>
    /// <param name="endPoint">The ending coordinate.</param>
    /// <returns>A <see cref="Coordinate"/> object representing the midpoint.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="startPoint"/> or <paramref name="endPoint"/> is null.</exception>
    /// <example>
    /// <code>
    /// var p1 = new Coordinate(51.127, 1.338);
    /// var p2 = new Coordinate(50.964, 1.853);
    /// var midpoint = LatLonSpherical.MidPoint(p1, p2); // 51.0455°N, 1.5957°E
    /// </code>
    /// </example>
    public static Coordinate RhumbMidPoint(Coordinate startPoint, Coordinate endPoint)
    {
        Guard.NotNull(startPoint, nameof(startPoint));
        Guard.NotNull(endPoint, nameof(endPoint));
        
        // φm = atan2( sinφ1 + sinφ2, √( (cosφ1 + cosφ2⋅cosΔλ)² + cos²φ2⋅sin²Δλ ) )
        // λm = λ1 + atan2(cosφ2⋅sinΔλ, cosφ1 + cosφ2⋅cosΔλ)
        // midpoint is sum of vectors to two points: mathforum.org/library/drmath/view/51822.html

        var deltaLon = endPoint.LongitudeR - startPoint.LongitudeR;
        var bx = Math.Cos(endPoint.LatitudeR) * Math.Cos(deltaLon);
        var by = Math.Cos(endPoint.LatitudeR) * Math.Sin(deltaLon);

        var lat = Math.Atan2(Math.Sin(startPoint.LatitudeR) + Math.Sin(endPoint.LatitudeR),
            Math.Sqrt((Math.Cos(startPoint.LatitudeR) + bx) * (Math.Cos(startPoint.LatitudeR) + bx) + by * by));

        var lon = startPoint.LongitudeR + Math.Atan2(by, Math.Cos(startPoint.LatitudeR) + bx);

        return new Coordinate(lat.ToDegrees(), lon.ToDegrees());
    }

    /// <summary>
    /// Calculates the area of a spherical polygon where the sides of the polygon are great circle
    /// arcs joining the vertices.
    /// </summary>
    /// <remarks>
    /// Uses method due to Karney: https://geographiclib.sourceforge.io/html/js/geographiclib-geodesic_src_PolygonArea.js.html
    /// (Karney's method is probably more efficient than the more widely known L’Huilier’s Theorem) 
    /// </remarks>
    /// <param name="polygon">Array of coordinates defining vertices of the polygon.</param>
    /// <returns>The area of the polygon in the same units as radius (meters).</returns>
    /// <example>
    /// <code>
    /// double area = LatLonSpherical.Area(
    ///     new Coordinate[] { new Coordinate(  0, 0), new Coordinate(1, 0), new Coordinate(0, 1) }
    /// ); // 6.18e9 m²
    /// </code>
    /// </example>
    public static double Area(Coordinate[] polygon)
    {
        // for each edge of the polygon, tan(E/2) = tan(Δλ/2)·(tan(φ₁/2)+tan(φ₂/2)) / (1+tan(φ₁/2)·tan(φ₂/2))
        // where E is the spherical excess of the trapezium obtained by extending the edge to the equator

        if (polygon.Length < 3) return 0;
        
        double total = 0;
        int crossings = 0;

        for (int i = 0; i < polygon.Length; i++)
        {
            var p1 = polygon[i];
            var p2 = polygon[(i + 1) % polygon.Length];

            double deltaLon = NormalizeLongitude(p2.LongitudeR - p1.LongitudeR);

            // Karney-style trapezium excess accumulation
            double tanPart = Math.Tan(deltaLon / 2.0) * (Math.Tan(p1.LatitudeR / 2.0) + Math.Tan(p2.LatitudeR / 2.0));

            double denom = 1.0 + Math.Tan(p1.LatitudeR / 2.0) * Math.Tan(p2.LatitudeR / 2.0);

            double E = 2.0 * Math.Atan2(tanPart, denom);

            total += E;

            crossings += PrimeMeridianCrossing(p1.Longitude, p2.Longitude);
        }

        // Normalize area depending on winding
        if (crossings % 2 == 0) return Math.Abs(total) * EarthRadiusMeters * EarthRadiusMeters;
        if (total < 0) total += 2 * Math.PI;
        else total -= 2 * Math.PI;

        return Math.Abs(total) * EarthRadiusMeters * EarthRadiusMeters;
    }

    private static int PrimeMeridianCrossing(double lon1, double lon2)
    {
        if ((lon1 < 0 && lon2 >= 0) || (lon2 < 0 && lon1 >= 0))
        {
            double d = lon2 - lon1;

            if (d > 180) return -1;

            if (d < -180) return 1;
        }

        return 0;
    }

    private static double NormalizeLongitude(double longitude)
    {
        while (longitude > Math.PI) longitude -= 2 * Math.PI;
        while (longitude < -Math.PI) longitude += 2 * Math.PI;
        
        return longitude;
    }
}