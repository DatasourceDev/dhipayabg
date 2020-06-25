using DhipayaBGProcess.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DhipayaBGProcess.Extensions
{
    public static class EnumStatus
    {
        public static string toName(this CustomerChanal chanal)
        {
            string status = "";
            switch (chanal)
            {
                case CustomerChanal.TIP:
                    status = "TIP Society";
                    break;
                case CustomerChanal.Mobile:
                    status = "Mobile Application";
                    break;
                case CustomerChanal.MobileImport:
                    status = "Mobile Application";
                    break;
                case CustomerChanal.ShareHolderImport:
                    status = "TIP Society";
                    break;
                case CustomerChanal.AmazonImport:
                    status = "TIP Society";
                    break;
                case CustomerChanal.INTIntersect:
                    status = "INT Intersect";
                    break;
                case CustomerChanal.DhiMemberImport:
                    status = "TIP Society";
                    break;
                case CustomerChanal.TipInsure:
                    status = "TIP Insure";
                    break;
                case CustomerChanal.TipInsureImport:
                    status = "TIP Insure";
                    break;
                default:
                    break;
            }
            return status;
        }
    }
}
