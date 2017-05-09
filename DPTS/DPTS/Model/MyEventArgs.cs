﻿using System;

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

    public enum ResultType { Original, SP, SP_Prac, SP_Theo, SP_Both, Intersect, Reinitialize };

    public class ResultMessageArgs : EventArgs
    {
        private Int32 _ID;
        public Int32 ID { get => _ID; }
        private ResultType _Result_Type;
        public ResultType Result_Type { get => _Result_Type; }
        private Int32 _Length;
        public Int32 Length { get => _Length; }
        private Double _TimeInSecs;
        public Double TimeInSecs { get => _TimeInSecs; }
        public ResultMessageArgs(Int32 id, ResultType resultType, Int32 length, Double timeInSecs)
        {
            _ID = id;
            _Result_Type = resultType;
            _Length = length;
            _TimeInSecs = timeInSecs;
        }
    }
}
