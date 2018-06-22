using System;
using System.Runtime.InteropServices;

namespace Improbable.Recast.Types
{
    using DtPolyRef = UInt64;
    
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct FindPathResult
    {
        public uint status;
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.MaxPathLength)]
        public DtPolyRef[] path;
        
        public int pathCount;
    }
}