using System;

namespace Wikiled.Market.Console.Logic
{
    public static class HoursExtension
    {
        public static int GetLastDaysHours(int days)
        {
            var now = DateTime.Now;
            var start = now.Date.AddDays(-days);
            return (int)(now - start).TotalHours;
        }
    }
}
