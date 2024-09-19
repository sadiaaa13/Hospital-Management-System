using System.Collections.Generic;

namespace Hospital.Models
{
    public class DoctorProfile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Qualification { get; set; }
        public string Hospital { get; set; }
        public List<string> Specialties { get; set; }
        public string Experience { get; set; }
        public string ContactNumber { get; set; }
        public string Address { get; set; }
        public string ProfileImageUrl { get; set; }
    }
}

