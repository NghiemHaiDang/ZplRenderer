using System;

namespace ZplRenderer.Infrastructure.Utils
{
    public static class MemoryOptimizer
    {
        public static void ForceCollect()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
