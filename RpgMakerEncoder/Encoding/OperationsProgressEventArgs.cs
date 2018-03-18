using System;

namespace RpgMakerEncoder.Encoding
{
    public class OperationsProgressEventArgs : EventArgs
    {
        public int Count { get; set; }
        public int Total { get; set; }
    }
}