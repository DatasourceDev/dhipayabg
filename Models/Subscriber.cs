using DhipayaBGProcess.Extensions;
using DhipayaBGProcess.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace DhipayaBGProcess.Models
{
   public class Subscriber
   {
      [Key]
      [Required(ErrorMessage = "กรุณาระบุอีเมล")]
      public string Email { get; set; }      

      public string Create_By { get; set; }
      public DateTime? Create_On { get; set; }
   }

}
