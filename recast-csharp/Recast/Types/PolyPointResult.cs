
using System.Runtime.InteropServices;

namespace Recast
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PolyPointResult
    {
        public uint status;
        public uint polyRef;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] point;
    }
}