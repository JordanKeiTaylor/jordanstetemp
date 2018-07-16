using System;
using System.Runtime.InteropServices;

namespace Improbable.Recast.Types
{
    using DtPolyRef = UInt64;
        
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct PolyPointResult
    {
        public uint status;
        
        public DtPolyRef polyRef;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] point;

        public override string ToString()
        {
            return $"[PolyPointResult ({status}): ({point[0]}, {point[1]}, {point[2]}). Polyref={polyRef}]";
        }
    }
}