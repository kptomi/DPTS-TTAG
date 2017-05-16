using System;

namespace DPTS.ViewModel
{
    public class Result
    {
        public Int32 No { get; private set; }
        public Int32 LengthOriginal { get; private set; }
        public Int32 LengthOptimal { get; private set; }
        public Int32 LengthApproximative { get; private set; }
        public Double Time_SP { get; private set; }
        public Double Time_SP_Prac { get; private set; }
        public Double Time_SP_Theo { get; private set; }
        public Double Time_SP_Both { get; private set; }
        public Double Time_Intersect { get; private set; }

        public Result(Int32 no)
        {
            No = no;
        }

        public void SetLengthOriginal(Int32 length)
        {
            LengthOriginal = length;
        }

        public void SetLengthOptimal(Int32 length)
        {
            LengthOptimal = length;
        }

        public void SetLengthApproximative(Int32 length)
        {
            LengthApproximative = length;
        }

        public void SetTime_SP(Double time)
        {
            Time_SP = time;
        }

        public void SetTime_SP_Prac(Double time)
        {
            Time_SP_Prac = time;
        }

        public void SetTime_SP_Theo(Double time)
        {
            Time_SP_Theo = time;
        }

        public void SetTime_SP_Both(Double time)
        {
            Time_SP_Both = time;
        }

        public void SetTime_Intersect(Double time)
        {
            Time_Intersect = time;
        }
    }
}
