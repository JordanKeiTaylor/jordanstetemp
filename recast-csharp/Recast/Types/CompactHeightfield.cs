using System;
using Microsoft.Win32.SafeHandles;

namespace Recast
{
    public class CompactHeightfield : SafeHandleZeroOrMinusOneIsInvalid
    {
        public CompactHeightfield(IntPtr handle) : base(true)
        {
            SetHandle(handle);    
        }

        protected override bool ReleaseHandle()
        {
            RecastLibrary.compact_heightfield_delete(handle);
            return true;
        }
    }
}