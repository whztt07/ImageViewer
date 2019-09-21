﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.ImageLoader
{
    public class ImageFormat
    {
        public SharpDX.DXGI.Format ExternalFormat { get; set; }

        public bool IsSrgb { get; set; }
        public bool IsCompressed { get; set; }

        public SharpDX.DXGI.Format InternalFormat { get; set; }

        public override string ToString()
        {
            return ExternalFormat.ToString();
        }
    }
}
