using System.Runtime.InteropServices;

namespace Recast
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SmoothPathResult
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3 * Constants.MaxSmoothPathLength)]
        public float[] path;
        
        public int pathCount;
    }
}