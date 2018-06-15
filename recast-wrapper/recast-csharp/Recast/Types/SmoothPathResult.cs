using System.Runtime.InteropServices;

namespace Recast
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct SmoothPathResult
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3 * Constants.MaxSmoothPathLength)]
        public float[] path;
        
        public int pathCount;
    }
}