using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Demo.ViewModel
{
    public class PatientViewModel
    {
        public int p_id { get; set; }
        public string p_name { get; set; }
        public DateTime? checkin_date { get; set; }
        public string doctor_name { get; set; }
        public string disease { get; set; }
        public string room_number { get; set; }
    }
}