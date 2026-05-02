using System;
using Deodesy.Library.Helpers;

namespace Deodesy.Library
{
    public class LatLonSpherical
    {
        private const double EarthRadiusKm = 6372.8;

        /**
         * Returns the distance (Km) along the surface of the earth from start point to end point using haversine formula.
         */
        public double Distance(Coordinate startPoint, Coordinate endPoint)
        {
            Guard.NotNull(startPoint, nameof(startPoint));
            Guard.NotNull(endPoint, nameof(endPoint));
            
            var deltaLat = endPoint.LatitudeR - startPoint.LatitudeR;
            var deltaLon = endPoint.LongitudeR - startPoint.LongitudeR;
            var originLat = startPoint.LatitudeR;
            var destinationLat = endPoint.LatitudeR;
            
            var a = Math.Pow(Math.Sin(deltaLat / 2), 2) + Math.Pow(Math.Sin(deltaLon / 2), 2) * Math.Cos(originLat) * Math.Cos(destinationLat);
            var c = 2 * Math.Asin(Math.Sqrt(a));
            return EarthRadiusKm * c;
        }

        /**
         * Returns the distance (Nm) along the surface of the earth from start point to end point using haversine formula.
         */
        public double DistanceInNm(Coordinate startPoint, Coordinate endPoint)
        {
            Guard.NotNull(startPoint, nameof(startPoint));
            Guard.NotNull(endPoint, nameof(endPoint));
            
            var distanceInKm = Distance(startPoint, endPoint);
            return distanceInKm * 0.539956803;
        }

        /**
         * Returns the initial bearing from start point to end point.
         */
        public double Bearing(Coordinate startPoint, Coordinate endPoint)
        {
            Guard.NotNull(startPoint, nameof(startPoint));
            Guard.NotNull(endPoint, nameof(endPoint));
            
            if (startPoint == endPoint) return 0.0;
            
            var deltaLon = endPoint.LongitudeR - startPoint.LongitudeR;
            var y = Math.Sin(deltaLon) * Math.Cos(endPoint.LatitudeR);
            var x = Math.Cos(startPoint.LatitudeR) * Math.Sin(endPoint.LatitudeR) -
                    Math.Sin(startPoint.LatitudeR) * Math.Cos(endPoint.LatitudeR) * Math.Cos(deltaLon);
            var phi = Math.Atan2(y, x);

            return phi.ToDegrees().Wrap360();
        }
        
        /**
         * Returns final bearing arriving at end point from start point; the final bearing will
         * differ from the initial bearing by varying degrees according to distance and latitude.
         */
        public double FinalBearing(Coordinate startPoint, Coordinate endPoint)
        {
            Guard.NotNull(startPoint, nameof(startPoint));
            Guard.NotNull(endPoint, nameof(endPoint));
            
            var bearing = Bearing(startPoint, endPoint) + 180;
            return bearing.Wrap360();
        }
        
        /**
         * Returns the midpoint between start point and end point.
         */
        public Coordinate MidPoint(Coordinate startPoint, Coordinate endPoint)
        {
            Guard.NotNull(startPoint, nameof(startPoint));
            Guard.NotNull(endPoint, nameof(endPoint));

            var deltaLon = endPoint.Longitude - startPoint.Longitude;
            var bx = Math.Cos(endPoint.Latitude) * Math.Cos(deltaLon);
            var by = Math.Cos(endPoint.Latitude) * Math.Sin(deltaLon);

            var lat = Math.Atan2(Math.Sin(startPoint.Latitude) + Math.Sin(endPoint.Latitude),
                Math.Sqrt((Math.Cos(startPoint.LatitudeR) + bx) * (Math.Cos(startPoint.Latitude) + bx) + by * by));

            var lon = startPoint.Longitude + Math.Atan2(by, Math.Cos(startPoint.Latitude) + bx);

            return new Coordinate(lat, lon);
        }

        /**
         * Returns the point at given fraction between start point and end point.
         */
        public Coordinate Intermediate(Coordinate startPoint, Coordinate endPoint, double fraction)
        {
            Guard.NotNull(startPoint, nameof(startPoint));
            Guard.NotNull(endPoint, nameof(endPoint));

            if (startPoint == endPoint) return startPoint;

            var deltaLat = endPoint.LatitudeR - startPoint.LatitudeR;
            var deltaLon = endPoint.LongitudeR - startPoint.LongitudeR;
            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) + Math.Cos(startPoint.LatitudeR) *
                Math.Cos(endPoint.LatitudeR) * Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
            
            var delta = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            var A = Math.Sin((1 - fraction) * delta) / Math.Sin(delta);
            var B = Math.Sin(fraction * delta) / Math.Sin(delta);

            var x = A * Math.Cos(startPoint.LatitudeR) * Math.Cos(startPoint.LongitudeR) +
                    B * Math.Cos(endPoint.LatitudeR) * Math.Cos(endPoint.LongitudeR);
            var y = A * Math.Cos(startPoint.LatitudeR) * Math.Sin(startPoint.LongitudeR) +
                    B * Math.Cos(endPoint.LatitudeR) * Math.Sin(endPoint.LongitudeR);
            var z = A * Math.Sin(startPoint.LatitudeR) + B * Math.Sin(endPoint.LatitudeR);

            var lat = Math.Atan2(z, Math.Sqrt(x * x + y * y));
            var lon = Math.Atan2(y, x);

            return new Coordinate(lat, lon);
        }

        /**
         * Returns the point of intersection of two paths which one starts from firstPoint
         * with firstBearing and the other one starts from secondPoint with secondBearing.
         */
        public Coordinate? Intersection(Coordinate firstPoint, double firstBearing, Coordinate secondPoint,
            double secondBearing)
        {
            Guard.NotNull(firstPoint, nameof(firstPoint));
            Guard.NotNull(secondPoint, nameof(secondPoint));

            var deltaLat = secondPoint.LatitudeR - firstPoint.LatitudeR;
            var deltaLon = secondPoint.LongitudeR - firstPoint.LongitudeR;

            var delta12 = 2 * Math.Asin(Math.Sqrt(Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                                                  Math.Cos(firstPoint.LatitudeR) * Math.Cos(secondPoint.LatitudeR) *
                                                  Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2)));
            if (Math.Abs(delta12) < Math.E) return firstPoint; // same points

            var cosThetaA = (Math.Sin(secondPoint.LatitudeR) - Math.Sin(firstPoint.LatitudeR) * Math.Cos(delta12)) /
                            (Math.Sin(delta12) * Math.Cos(firstPoint.LatitudeR));
            var cosThetaB = (Math.Sin(firstPoint.LatitudeR) - Math.Sin(secondPoint.LatitudeR) * Math.Cos(delta12)) /
                            (Math.Sin(delta12) * Math.Cos(secondPoint.LatitudeR));

            var thetaA = Math.Acos(Math.Min(Math.Max((int)cosThetaA, -1), 1));
            var thetaB = Math.Acos(Math.Min(Math.Max((int)cosThetaB, -1), 1));

            var theta12 = thetaA;
            if (Math.Sin(secondPoint.LongitudeR - firstPoint.LongitudeR) < 0)
                theta12 = 2 * Math.PI - thetaA;

            var theta21 = thetaB;
            if (Math.Sin(secondPoint.LongitudeR - firstPoint.LongitudeR) > 0)
                theta21 = 2 * Math.PI - thetaB;

            var alpha1 = firstBearing.ToRadians() - theta12;
            var alpha2 = theta21 - secondBearing.ToRadians();

            if (Math.Sin(alpha1) == 0.0 && Math.Sin(alpha2) == 0.0) return null;
            if (Math.Sin(alpha1) * Math.Sin(alpha2) < 0) return null;

            var cosAlpha3 = -Math.Cos(alpha1) * Math.Cos(alpha2) +
                            Math.Sin(alpha1) * Math.Sin(alpha2) * Math.Cos(delta12);

            var delta13 = Math.Atan2(Math.Sin(delta12) * Math.Sin(alpha1) * Math.Sin(alpha2),
                Math.Cos(alpha2) + Math.Cos(alpha1) * cosAlpha3);

            var lat = Math.Asin(Math.Min(
                Math.Max(
                    Math.Sin(firstPoint.LatitudeR) * Math.Cos(delta13) + Math.Cos(firstPoint.LatitudeR) *
                    Math.Sin(delta13) * Math.Cos(firstBearing.ToRadians()), -1), 1));

            var deltaLon13 =
                Math.Atan2(Math.Sin(firstBearing.ToRadians()) * Math.Sin(delta13) * Math.Cos(firstPoint.LatitudeR),
                    Math.Cos(delta13) - Math.Sin(firstPoint.LatitudeR) * Math.Sin(lat));

            var lon = firstPoint.LongitudeR + deltaLon13;

            return new Coordinate(lat, lon);
        }

        /**
         * Returns the destination point and final bearing traveling along a
         * (shortest distance) great circle arc for a given start point, initial bearing and distance
         */
        public Coordinate destination(Coordinate startPoint, double distance, double bearing)
        {
            Guard.NotNull(startPoint, nameof(startPoint));

            var sinLat = Math.Sin(startPoint.LatitudeR) * Math.Cos(distance / EarthRadiusKm) +
                         Math.Cos(startPoint.LatitudeR) * Math.Sin(distance / EarthRadiusKm) *
                         Math.Cos(bearing.ToRadians());
            var lat = Math.Asin(sinLat);
            var y = Math.Sin(bearing.ToRadians()) * Math.Sin(distance / EarthRadiusKm) * Math.Cos(startPoint.LatitudeR);
            var x = Math.Cos(distance / EarthRadiusKm) * Math.Sin(startPoint.LatitudeR) * sinLat;
            var lon = startPoint.LongitudeR + Math.Atan2(y, x);

            return new Coordinate(lat, lon);
        }

        /**
         * Returns distance from current point to great circle between start point and end point.
         */
        public double CrossTrackDistance(Coordinate currentPoint, Coordinate startPoint, Coordinate endPoint)
        {
            Guard.NotNull(currentPoint, nameof(currentPoint));
            Guard.NotNull(startPoint, nameof(startPoint));
            Guard.NotNull(endPoint, nameof(endPoint));

            if (currentPoint == startPoint) return 0.0; // same point

            var delta13 = Distance(startPoint, currentPoint) / EarthRadiusKm;
            var theta13 = Bearing(startPoint, currentPoint).ToRadians();
            var theta12 = Bearing(startPoint, endPoint).ToRadians();

            var deltaCrossTrack = Math.Asin(Math.Sin(delta13) * Math.Sin(theta13 - theta12));

            return deltaCrossTrack * EarthRadiusKm;
        }

        /**
         * Returns how far current point is along a path from from start point, heading towards end point.
         * That is, if a perpendicular is drawn from current point to the (great circle) path, the
         * along-track distance is the distance from the start point to where the perpendicular crosses the path.
         */
        public double AlongTrackDistanceTo(Coordinate currentPoint, Coordinate startPoint, Coordinate endPoint)
        {
            Guard.NotNull(currentPoint, nameof(currentPoint));
            Guard.NotNull(startPoint, nameof(startPoint));
            Guard.NotNull(endPoint, nameof(endPoint));
            
            if (currentPoint == startPoint) return 0.0; // same point
            
            var delta13 = Distance(startPoint, currentPoint) / EarthRadiusKm;
            var theta13 = Bearing(startPoint, currentPoint).ToRadians();
            var theta12 = Bearing(startPoint, endPoint).ToRadians();

            var deltaCrossTrack = Math.Asin(Math.Sin(delta13) * Math.Sin(theta13 - theta12));
            var deltaAlongTrack = Math.Acos(Math.Cos(delta13) / Math.Abs(Math.Cos(deltaCrossTrack)));

            return deltaAlongTrack * Math.Sign(Math.Cos(theta12 - theta13)) * EarthRadiusKm;
        }

        /**
         * Returns maximum latitude reached when traveling on a great circle on given bearing from
         * start point (‘Clairaut’s formula’). Negate the result for the minimum latitude (in the
         * Southern Hemisphere).
         *
         * The maximum latitude is independent of longitude; it will be the same for all points on a
         * given latitude.
         */
        public double MaxLatitude(Coordinate startPoint, double bearing)
        {
            Guard.NotNull(startPoint, nameof(startPoint));
            
            var bearingInRad = bearing.ToRadians();
            var maxLat = Math.Acos(Math.Abs(Math.Sin(bearingInRad) * Math.Cos(startPoint.LatitudeR)));
            return maxLat.ToDegrees();
        }

        /**
         * Returns the pair of meridians at which a great circle defined by two points crosses the given
         * latitude. If the great circle doesn't reach the given latitude, null is returned.
         *
         * The maximum latitude is independent of longitude; it will be the same for all points on a
         * given latitude.
         */
        public double[]? CrossingParallels(Coordinate startPoint, Coordinate endPoint, double latitude)
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

            if (z * z > x * x + y * y) return null;

            var deltaM = Math.Atan2(-y, x);
            var deltaLoni = Math.Acos(z / Math.Sqrt(x * x + y * y));

            var deltaI1 = startPoint.LongitudeR + deltaM - deltaLoni;
            var deltaI2 = startPoint.LongitudeR + deltaM + deltaLoni;

            var lon1 = deltaI1.ToDegrees();
            var lon2 = deltaI2.ToDegrees();

            return new [] { lon1.Wrap180(), lon2.Wrap180() };
        }
    }
}