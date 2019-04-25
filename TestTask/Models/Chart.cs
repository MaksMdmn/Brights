using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestTask.Models
{
    public class Chart
    {
        public string[] categories { get; set; }
        public List<Series> series { get; set; }
    }

    public class Series
    {
        public string name { get; set; }
        public int[] data { get; set; }
    }
}