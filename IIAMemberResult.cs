using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DhipayaBGProcess.Models
{  


    public class IIAMemberResult
    {
        public string resultCode { get; set; }
        public string resultDescription { get; set; }
        public IIAMemberData[] data { get; set; }
    }
    public class IIAMemberData
    {
        public string PolicyNo { get; set; }
        public string EffectiveDate { get; set; }
        public string ExpiryDate { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string SubClass { get; set; }
        public string PolicyPremium { get; set; }
        public string PreviousPolicyNo { get; set; }
        public string OutletCode { get; set; }
        public string InsuranceClass { get; set; }
    }
}
