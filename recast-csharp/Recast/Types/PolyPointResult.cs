
using System.Runtime.InteropServices;

namespace Recast
{
    public unsafe struct PolyPointResult
    {
        public uint status;
        public uint polyRef;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] point;
    }
}