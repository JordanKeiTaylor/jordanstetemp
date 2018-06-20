﻿using System;
using Microsoft.Win32.SafeHandles;

namespace Recast
{
    public class InputGeom : SafeHandleZeroOrMinusOneIsInvalid
    {
        public InputGeom(IntPtr handle) : base(true)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            RecastLibrary.InputGeom_delete(handle);
            return true;
        }
    }
}