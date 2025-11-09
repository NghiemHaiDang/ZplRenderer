using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZplRenderer.Config
{
    public static class AppConstants
    {
        public const string DefaultOutputFolder = "output";
        public const int MaxBufferLines = 2000; // tối ưu nếu cần chunk nhỏ hơn
    }
}
