using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Mobile
{
    public static class MobileConstants
    {
        public static readonly string Database = "scraprunner";
        public static readonly string ImagesDirectory = "Images";
        public static readonly string NewContainerKey = "NB#";

        // These could possibly be configurable options instead of constants
        public static readonly int MaxPixelDimension = 1000;
        public static readonly int ImageQuality = 50;
    }
}
