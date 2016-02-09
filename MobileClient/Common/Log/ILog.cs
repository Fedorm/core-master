using System;

namespace BitMobile.Common.Log
{
    public interface ILog
    {
        DateTime Date { get; }
        string Event { get; }
        string Content { get; }
    }
}