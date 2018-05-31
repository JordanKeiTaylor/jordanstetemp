﻿using System;
using Microsoft.Win32.SafeHandles;

namespace Recast
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