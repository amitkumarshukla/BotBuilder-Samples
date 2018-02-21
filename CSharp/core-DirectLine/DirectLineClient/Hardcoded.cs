using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectLineSampleClient
{
    public static class Hardcoded
    {
        public static Dictionary<string, string> appNamevsExeMapping = new Dictionary<string, string>()
        {
            {"visual studio",@"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\devenv.exe"},
            {"vs",@"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\devenv.exe"},
            {"msvc",@"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\devenv.exe"},
            {"notepad", @"C:\Windows\System32\notepad.exe" },
            {"ms paint", @"C:\Windows\System32\mspaint.exe" },
            {"paint", @"C:\Windows\System32\mspaint.exe" },
            {"microsoft paint", @"C:\Windows\System32\mspaint.exe" },
            {"command prompt", @"C:\Windows\System32\cmd.exe" },
            {"command window", @"C:\Windows\System32\cmd.exe" },
            {"command", @"C:\Windows\System32\cmd.exe" },
        };
    }
}
