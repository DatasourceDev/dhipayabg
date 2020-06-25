﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DhipayaBGProcess.Models
{
   public class AboutUs
   {
      [Key]
      public int ID { get; set; }

      public string Url { get; set; }
      public string VideoUrl { get; set; }

      public string Title { get; set; }
      public string Description { get; set; }
      public MediaType MediaType { get; set; }

      public string Create_By { get; set; }
      public DateTime? Create_On { get; set; }
      public string Update_By { get; set; }
      public DateTime? Update_On { get; set; }

   }
}
