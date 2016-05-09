using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCCA.Models
{
    public class Center
    {
        //ID used by database
        public int ID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string DirectorName { get; set; }
        public string DirectorNumber { get; set; }
        public string DirectorEmail { get; set; }
        public string URL { get; set; }
        public string Description { get; set; }
    }
}
