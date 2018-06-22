using System;
using Microsoft.Win32.SafeHandles;

namespace Improbable.Recast.Types
{
    public class RcContext : SafeHandleZeroOrMinusOneIsInvalid
    {
        public RcContext(IntPtr handle) : base(true)
        {
            SetHandle(handle);
        }
        
        protected override bool ReleaseHandle()
        {
            RecastLibrary.rcContext_delete(handle);
            return true;
        }
    }
}