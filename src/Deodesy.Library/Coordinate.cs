using System;
using Deodesy.Library.Helpers;

namespace Deodesy.Library
{
    public class Coordinate
    {
        private const double Tolerance = 1e-6; // accuracy of 10-15cm
        private double _latitude;
        private double _longitude;
        
        public double Latitude
        {
            get => _latitude;
            set => _latitude = Dms.Wrap90(value);
        }

        public double Longitude
        {
            get => _longitude;
            set => _longitude = Dms.Wrap180(value);
        }

        public Coordinate(double latitude = 0, double longitude = 0)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double LatitudeR => Latitude.ToRadians();
        public double LongitudeR => Longitude.ToRadians();

        public static bool operator ==(Coordinate? left, Coordinate? right) =>
            ReferenceEquals(left, right) || (!ReferenceEquals(left, null) && left.Equals(right));
        
        public static bool operator !=(Coordinate left, Coordinate right) =>
            !(left == right);
        
        private bool Equals(Coordinate? other)
        {
            if (other is null) return false;
            return Math.Abs(other.Latitude - Latitude) < Tolerance && Math.Abs(other.Longitude - Longitude) < Tolerance;
        }
    }
}
