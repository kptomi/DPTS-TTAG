using System;
using System.Collections.Generic;

namespace DPTS.Model
{
    public class StringMessageArgs : EventArgs
    {
        private String _Message;
        public String Message { get => _Message; }
        public StringMessageArgs(String message)
        {
            _Message = message;
        }
    }

    public enum TrajectoryType { Original, SimplifiedOptimal, SimplifiedApproximative };

    public class TrajectoryLengthArgs : EventArgs
    {
        private Int32 _Length;
        public Int32 Length { get => _Length; }
        private TrajectoryType _TRType;
        public TrajectoryType TRType { get => _TRType; }
        public TrajectoryLengthArgs(Trajectory trajectory, TrajectoryType trtype)
        {
            _Length = trajectory.NumberOfPoints;
            _TRType = trtype;
        }
    }

    public class ErrorToleranceArgs : EventArgs
    {
        private Double _ErrorTolerance;
        public Double ErrorTolerance { get => _ErrorTolerance; }
        public ErrorToleranceArgs(Double errorTolerance)
        {
            _ErrorTolerance = errorTolerance;
        }
    }
}
