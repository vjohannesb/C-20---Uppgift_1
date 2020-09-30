using System;
using System.Collections.Generic;
using System.Text;

namespace Uppg1_WorkerService.Models
{
    public class WeatherModel
    {
        public Main main { get; set; }
        public string name { get; set; }
    }

    public class Main
    {
        public double temp { get; set; }
        public double temp_min { get; set; }
        public double temp_max { get; set; }
    }
}
