﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SoulsFormats
{
    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class FormatReference : Attribute
    {
        public string ReferenceName;
    }
}
