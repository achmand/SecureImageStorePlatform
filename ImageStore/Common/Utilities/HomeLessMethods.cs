using System;

namespace Common.Utilities
{
    // centralize utility methods, for future changes
    public static class HomelessMethods
    {
        public static DateTime GetCurrentTime() 
        {
            return DateTime.UtcNow;
        }
    }
}
