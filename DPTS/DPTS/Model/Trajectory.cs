using System;
using System.Collections.Generic;

namespace DPTS.Model
{
    public class Trajectory
    {
        private List<Point> _Points;
        private String _StartTime, _EndTime;
        public String StartTime { get => _StartTime; set => _StartTime = value; }
        public String EndTime { get => _EndTime; set => _EndTime = value; }

        public Trajectory()
        {
            _Points = new List<Point>();
            _StartTime = "";
            _EndTime = "";
        }

        public Point this[Int32 index] { get => _Points[index]; }

        public Int32 NumberOfPoints { get => _Points.Count; }

        public void Add(Point item)
        {
            _Points.Add(item);
        }
    }
}
