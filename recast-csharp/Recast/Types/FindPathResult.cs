using System;
using System.Runtime.InteropServices;

namespace Recast
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FindPathResult
    {
        public uint status;
        public IntPtr path;
        public int pathCount;
    }
}