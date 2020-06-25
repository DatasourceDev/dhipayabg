using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DhipayaBGProcess.Models
{
    public class CustomerPrefix
   {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public string NameEng { get; set; }
        public string NameEng2 { get; set; }
   }
}
