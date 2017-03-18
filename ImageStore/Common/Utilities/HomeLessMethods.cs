using System;

namespace Common.Utilities
{
    // centralize utility methods, for future changes
    public static class HomeLessMethods
    {
        public static DateTime GetCurrentTime() 
        {
            return DateTime.UtcNow;
        }
    }
}
