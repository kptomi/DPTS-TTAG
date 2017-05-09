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

        public void setLengthOriginal(Int32 length)
        {
            LengthOriginal = length;
        }

        public void setLengthOptimal(Int32 length)
        {
            LengthOptimal = length;
        }

        public void setLengthApproximative(Int32 length)
        {
            LengthApproximative = length;
        }

        public void setTime_SP(Double time)
        {
            Time_SP = time;
        }

        public void setTime_SP_Prac(Double time)
        {
            Time_SP_Prac = time;
        }

        public void setTime_SP_Theo(Double time)
        {
            Time_SP_Theo = time;
        }

        public void setTime_SP_Both(Double time)
        {
            Time_SP_Both = time;
        }

        public void setTime_Intersect(Double time)
        {
            Time_Intersect = time;
        }
    }
}
