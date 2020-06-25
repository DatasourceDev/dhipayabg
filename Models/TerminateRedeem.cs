﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DhipayaBGProcess.Models
{
   public class TerminateRedeem
   {
      [Key]
      public int ID { get; set; }
      public int PrivilegeID { get; set; }
      public int CustomerID { get; set; }
      public string CustomerClassName { get; set; }
      public string MerchantName { get; set; }
      public string PrivilegeName { get; set; }
      public bool Confirmed { get; set; }
      public decimal Point { get; set; }
      public string RedeemCode { get; set; }
      public string Address { get; set; }
      public RedeemType RedeemType { get; set; }
      public PrivilegeCodeType PrivilegeCodeType { get; set; }
      public DateTime? RedeemDate { get; set; }
      //public DateTime? EndDate { get; set; }

   }
}
