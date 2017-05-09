using System;

namespace DPTS.Model
{
    public class Point
    {
        private Single _Longitude, _Latitude;
        public Single Longitude { get => _Longitude; }
        public Single Latitude { get => _Latitude; }
        public Point(Single longitude, Single latitude)
        {
            _Longitude = longitude;
            _Latitude = latitude;
        }
        
        public Boolean Equals(Point other)
        {
            return (Longitude == other.Longitude && Latitude == other.Latitude);
        }

        public static Double GetDirection(Point from, Point to)
        {
            Double direction = Math.Atan((to.Latitude - from.Latitude) / (to.Longitude - from.Longitude));
            // korrekció
            if (direction < 0)
            {
                direction += Math.PI * 2;
            }
            return direction;
        }
    }
}
