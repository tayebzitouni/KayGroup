using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.data
{
    public class UsefulData
    {
     public string connnectionString = "Server=DESKTOP-G5Q1E25;Database=KayGroupDb;User Id=sa;Password=123456;";

        public static DateTime? ParseDate(string dateString)
        {
            if (string.IsNullOrEmpty(dateString)) return null;
            return DateTime.TryParseExact(dateString,
                new[] { "yyyy-MM-dd", "d/M/yyyy", "dd-MM-yyyy" },
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date) ? date : null;
        }

        public static bool IsDateBeforeToday(string dateString)
        {
            var date = ParseDate(dateString);
            return date.HasValue && date.Value.Date < DateTime.Today;
        }
    }


    public static class Session
    {
        public static Utilisatuer? CurrentUser;
    }


}
