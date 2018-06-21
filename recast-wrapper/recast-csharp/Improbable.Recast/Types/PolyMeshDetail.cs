using System;
using Microsoft.Win32.SafeHandles;

namespace Improbable.Recast.Types
{
    public class PolyMeshDetail : SafeHandleZeroOrMinusOneIsInvalid
    {
        public PolyMeshDetail(IntPtr handle) : base(true)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            RecastLibrary.polymesh_detail_delete(handle);
            return true;
        }
    }
}