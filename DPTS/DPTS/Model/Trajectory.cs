using System;
using System.Collections.Generic;

namespace DPTS.Model
{
    public class Trajectory
    {
        private List<Point> _Points;
        public String StartTime { get; private set; }
        public String EndTime { get; private set; }

        public Trajectory()
        {
            _Points = new List<Point>();
            StartTime = "";
            EndTime = "";
        }

        public Point this[Int32 index] { get => _Points[index]; }

        public Int32 NumberOfPoints { get => _Points.Count; }

        public void Add(Point item)
        {
            _Points.Add(item);
        }

        public void SetStartTime(String time)
        {
            StartTime = time;
        }

        public void SetEndTime(String time)
        {
            EndTime = time;
        }
    }
}
