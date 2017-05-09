using System;
using System.Collections.Generic;

namespace DPTS.Model
{
    class Range
    {
        public Double From { get; private set; }
        public Double To { get; private set; }
        public Range(Double from, Double to)
        {
            From = from;
            To = to;
        }
    }

    public class DirectionRange
    {
        private List<Range> _RangeList;

        private DirectionRange()
        {
            _RangeList = new List<Range>();
        }

        public DirectionRange(Double direction, Double error_tolerance)
        {
            if (error_tolerance >= Math.PI)
            {
                // pi-nél nagyobb hibaküszöb esetén a teljes intervallum nem szűkül
                _RangeList = new List<Range> { new Range(0, Math.PI * 2) };
                return;
            }

            // error_tolerance < Math.PI
            Double rangeFrom = direction - error_tolerance;
            // a "mod 2pi" szimulálása
            if (rangeFrom < 0)
            {
                rangeFrom += Math.PI * 2;
            }
            Double rangeTo = direction + error_tolerance;
            // a "mod 2pi" szimulálása
            if (rangeTo > Math.PI * 2)
            {
                rangeTo -= Math.PI * 2;
            }

            // ha a kezdőpont kisebb, mint a végpont, akkor két intervallumra bontjuk
            if (rangeFrom > rangeTo)
            {
                _RangeList = new List<Range> { new Range(0, rangeTo), new Range(rangeFrom, Math.PI * 2) };
            }
            else
            {
                _RangeList = new List<Range> { new Range(rangeFrom, rangeTo) };
            }
        }

        public static DirectionRange Intersect(DirectionRange rangeA, DirectionRange rangeB)
        {
            DirectionRange intersect = new DirectionRange();

            List<Range> rangeListA = rangeA._RangeList;
            List<Range> rangeListB = rangeB._RangeList;

            Int32 counterA = 0;
            Int32 counterB = 0;
            while (counterA < rangeListA.Count && counterB < rangeListB.Count)
            {
                Range intervalA = rangeListA[counterA];
                Range intervalB = rangeListB[counterB];
                // ket intervallum 6-féle helyzetben lehet egymáshoz képest, ezek közül 4-ben van metszés
                if (intervalA.From <= intervalB.From && intervalA.To >= intervalB.To)
                {
                    // az "A" intervallum tartalmazza a "B" intervallumot
                    intersect._RangeList.Add(new Range(intervalB.From, intervalB.To));
                    ++counterB;
                }
                else if (intervalA.From >= intervalB.From && intervalA.To <= intervalB.To)
                {
                    // a "B" intervallum tartalmazza az "A" intervallumot
                    intersect._RangeList.Add(new Range(intervalA.From, intervalA.To));
                    ++counterA;
                }
                else if (intervalA.From <= intervalB.From && intervalA.To >= intervalB.From && intervalA.To <= intervalB.To)
                {
                    // a "B" intervallum az "A" "intervallumban" kezdődik, de később ér véget
                    intersect._RangeList.Add(new Range(intervalB.From, intervalA.To));
                    ++counterA;
                }
                else if (intervalA.From >= intervalB.From && intervalA.From <= intervalB.To && intervalA.To >= intervalB.To)
                {
                    // az "A" intervallum a "B" "intervallumban" kezdődik, de később ér véget
                    intersect._RangeList.Add(new Range(intervalA.From, intervalB.To));
                    ++counterB;
                }
                else if (intervalA.From > intervalB.To)
                {
                    // nincs közös része a két intervallumnak ÉS a "B" intervallum "után" kezdődik az "A"
                    ++counterB;
                }
                else
                {
                    // nincs közös része a két intervallumnak ÉS az "A" intervallum "után" kezdődik a "B"
                    ++counterA;
                }
            }
            return intersect;
        }

        public Boolean IsDirectionInRange(Double direction)
        {
            Boolean isDirectionInRange = false;
            for (Int32 i = 0; !isDirectionInRange && i < _RangeList.Count; ++i)
            {
                Range range = _RangeList[i];
                isDirectionInRange = (direction >= range.From && direction <= range.To);
            }
            return isDirectionInRange;
        }

        public Int32 NumberOFRanges()
        {
            return _RangeList.Count;
        }
    }
}
