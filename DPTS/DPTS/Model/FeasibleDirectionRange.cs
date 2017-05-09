using System;

namespace DPTS.Model
{
    public class FeasibleDirectionRange
    {
        // az 1 távolságra lévő csúcspárok FDR-jei - a tömb indexelése csúcspár nagyobb indexű tagjára utal, tehát a i-edik elem az fdr(i - 1, i)
        private DirectionRange[] _InitRanges;
        private Int32 _CurrentIndex;
        private DirectionRange[] _CurrentRanges;

        public FeasibleDirectionRange(Trajectory trajectory, Double error_tolerance)
        {
            _InitRanges = new DirectionRange[trajectory.NumberOfPoints];
            // (r == 1) eset
            for (Int32 h = 1; h < _InitRanges.Length; ++h)
            {
                Double direction = Point.GetDirection(trajectory[h - 1], trajectory[h]);
                _InitRanges[h] = new DirectionRange(direction, error_tolerance); // FDR mátrix [h - 1, h] eleme
            }

            _CurrentIndex = -1;
            _CurrentRanges = new DirectionRange[_InitRanges.Length];
        }

        public Boolean IsDirectionInRange(Trajectory trajectory, Int32 fromIndex, Int32 toIndex)
        {
            if (_CurrentIndex != fromIndex)
            {
                _CurrentIndex = fromIndex;
                _CurrentRanges = new DirectionRange[_InitRanges.Length];
                _CurrentRanges[fromIndex + 1] = _InitRanges[fromIndex + 1];
                for (Int32 j = fromIndex + 2; j < _CurrentRanges.Length; ++j)
                {
                    _CurrentRanges[j] = DirectionRange.Intersect(_CurrentRanges[j - 1], _InitRanges[j]);
                }
            }
            return _CurrentRanges[toIndex].IsDirectionInRange(Point.GetDirection(trajectory[fromIndex], trajectory[toIndex]));
        }
    }
}
