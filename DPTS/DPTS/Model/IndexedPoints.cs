using System;

namespace DPTS.Model
{
    public class IndexedPoints
    {
        public Int32 Index { get; private set; }
        public Point TrPoint { get; private set; }
        public IndexedPoints(Int32 index, Point trPoint)
        {
            Index = index;
            TrPoint = trPoint;
        }
    }
}
