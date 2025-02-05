using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePerfect.Helpers
{
    public static class PathHelper
    {
        public static string GetRegExpString(string path)
        {
            return path.Replace(@"\", @"\\\\") + @"\\\\[^\\\\]+\\\\?$";
        }
    }
}
