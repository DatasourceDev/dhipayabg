using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net;
using Dhipaya.DhipayaBGProcess;
using DhipayaBGProcess.Models;
using OfficeOpenXml;
using DhipayaBGProcess.Extensions;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using DhipayaBGProcess.DAL;
using Microsoft.EntityFrameworkCore;

namespace DhipayaBGProcess
{
    class Program
    {
        /*
           select ID, DOB
           from  Customers 
           where  (Month(DOB) < Month(getdate()) or (Month(DOB) = Month(getdate()) and Day(DOB) <= Day(getdate())  ))
           and (LastgivePointDOB is null or LastgivePointDOB != Year(getdate()))
           order by Month(DOB) desc , Day(DOB)  desc
        */
        public class CustomersDTO : ModelReportBaseDTO
        {
            public IEnumerable<Customer> Customers { get; set; }
        }

        static void exportprivilegeexcel()
        {
            var connString = ConfigurationManager.ConnectionStrings["DBContext"].ConnectionString;
            var cusRepo = new CustomerRepository(connString);
            var privileges = cusRepo.ListPrivilege();

            Console.WriteLine(DateTime.Now + ": Start render privilege Excel (" + privileges.Count() + ")");
            var comlumHeadrs = new string[]{
                   "No.",
                   "ร้านค้า/บริการ",
                   "หมวดหมู่",
                   "สิทธิพิเศษ",
                   "จังหวัด",
                   "วันที่เริ่มต้น",
                   "วันที่สิ้นสุด",
                   "คะแนนที่ใช้แลก",
                   "TIP Silver",
                   "TIP Gold",
                   "TIP Gold Lady",
                   "สถานะ",
                   "จำนวนการใช้สิทธิ์",                  
                };

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                // add a new worksheet to the empty workbook
                var worksheet = package.Workbook.Worksheets.Add("สิทธิพิเศษ"); //Worksheet name
                using (var cells = worksheet.Cells[1, 1, 1, comlumHeadrs.Count()]) //(1,1) (1,5)
                {
                    cells.Style.Font.Bold = true;
                }

                //First add the headers
                for (var i = 0; i < comlumHeadrs.Count(); i++)
                {
                    worksheet.Cells[1, i + 1].Value = comlumHeadrs[i];
                }

                //Add values
                var j = 2;
                string[] chars = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
                foreach (var privilege in privileges)
                {
                    var c = 0;
                    worksheet.Cells[chars[c] + j].Value = j-1; c++;
                    worksheet.Cells[chars[c] + j].Value = privilege.MerchantName.Trim(); c++;
                    worksheet.Cells[chars[c] + j].Value = privilege.CategoryName.Trim(); c++;
                    worksheet.Cells[chars[c] + j].Value = privilege.PrivilegeName; c++;
                    worksheet.Cells[chars[c] + j].Value = privilege.ProvinceName; c++;
                    worksheet.Cells[chars[c] + j].Value = DateUtil.ToDisplayDate(privilege.StartDate); c++;
                    worksheet.Cells[chars[c] + j].Value = DateUtil.ToDisplayDate(privilege.EndDate); c++;
                    worksheet.Cells[chars[c] + j].Value = privilege.CreditPoint; c++;
                    worksheet.Cells[chars[c] + j].Value = privilege.TipSilver; c++;
                    worksheet.Cells[chars[c] + j].Value = privilege.TipGold; c++;
                    worksheet.Cells[chars[c] + j].Value = privilege.TipGoldLady; c++;
                    worksheet.Cells[chars[c] + j].Value = privilege.Status; c++;
                    worksheet.Cells[chars[c] + j].Value = privilege.RedeemCount; c++;
                    j++;

                }
                byte[] result = package.GetAsByteArray();
                //string filePath = "E:\\Dhipaya\\Dhipaya\\ExcelDemo" + DateUtil.ToInternalDate(DateUtil.Now()) + ".xlsx";
                string filePath = ConfigurationManager.AppSettings["filepath"] + "\\สิทธิพิเศษ" + DateUtil.ToInternalDate(DateUtil.Now()) + ".xlsx";


                //write the file to the disk
                File.WriteAllBytes(filePath, result);
                Console.WriteLine(DateTime.Now + ": End");

            }
        }
        static void exportcustomerexcel()
        {
            Console.WriteLine(DateTime.Now + ": Start query Customers");

            var connString = ConfigurationManager.ConnectionStrings["DBContext"].ConnectionString;
            var cusRepo = new CustomerRepository(connString);
            CustomersDTO model = new CustomersDTO();

            var customers = cusRepo.ListAll(new ModelReportBaseDTO());
            using (var _context = new ChFrontContext())
            {
                Console.WriteLine(DateTime.Now + ": Start render Excel (" + customers.Count() + ")");
                var comlumHeadrs = new string[]{
                   "หมายเลขสมาชิก",
                   "ชื่อ",
                   "นามสกุล",
                   "ชื่ออังกฤษ",
                   "นามสกุลอังกฤษ",
                   "หมายเลขบัตรประชาชน",
                   "Passport",
                   "อีเมล",
                   "หมายเลขโทรศัพท์มือถือ",
                   "FriendCode",
                   "วันเกิด",
                   "ปีเกิด",
                   "ประเภทสมาชิก",
                   "ช่องทางการสมัคร",
                   "บ้านเลขที่",
                   "หมู่บ้าน",
                   "หมู่",
                   "ซอย",
                   "ถนน",
                   "อำเภอ",
                   "ตำบล",
                   "จังหวัด",
                   "รหสไปรษณีย์",
                   "คะแนนสะสม",
                   "วันที่สมัคร",
                };

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    // add a new worksheet to the empty workbook
                    var worksheet = package.Workbook.Worksheets.Add("รายชื่อสมาชิก"); //Worksheet name
                    using (var cells = worksheet.Cells[1, 1, 1, comlumHeadrs.Count()]) //(1,1) (1,5)
                    {
                        cells.Style.Font.Bold = true;
                    }

                    //First add the headers
                    for (var i = 0; i < comlumHeadrs.Count(); i++)
                    {
                        worksheet.Cells[1, i + 1].Value = comlumHeadrs[i];
                    }

                    //Add values
                    var j = 2;
                    string[] chars = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
                    foreach (var customer in customers)
                    {
                        var c = 0;
                        worksheet.Cells[chars[c] + j].Value = customer.RefCode; c++;
                        worksheet.Cells[chars[c] + j].Value = customer.NameTh.Trim(); c++;
                        worksheet.Cells[chars[c] + j].Value = customer.SurNameTh.Trim(); c++;
                        worksheet.Cells[chars[c] + j].Value = customer.NameEn; c++;
                        worksheet.Cells[chars[c] + j].Value = customer.SurNameEn; c++;
                        worksheet.Cells[chars[c] + j].Value = customer.IDCard; c++;
                        worksheet.Cells[chars[c] + j].Value = customer.Passport; c++;
                        worksheet.Cells[chars[c] + j].Value = customer.Email; c++;
                        worksheet.Cells[chars[c] + j].Value = customer.MoblieNo; c++;
                        worksheet.Cells[chars[c] + j].Value = customer.FriendCode; c++;
                        worksheet.Cells[chars[c] + j].Value = DateUtil.ToDisplayDateTime(customer.DOB); c++;
                        worksheet.Cells[chars[c] + j].Value = customer.DOB.HasValue ? customer.DOB.Value.Year.ToString() : ""; c++;
                        worksheet.Cells[chars[c] + j].Value = customer.CustomerClass != null ? customer.CustomerClass.Name : "TIP Silver"; c++;
                        worksheet.Cells[chars[c] + j].Value = customer.Channel.toName(); c++;
                        worksheet.Cells[chars[c] + j].Value = customer.CUR_HouseNo; c++;
                        worksheet.Cells[chars[c] + j].Value = customer.CUR_VillageName; c++;
                        worksheet.Cells[chars[c] + j].Value = customer.CUR_Moo; c++;
                        worksheet.Cells[chars[c] + j].Value = customer.CUR_Lane; c++;
                        worksheet.Cells[chars[c] + j].Value = customer.CUR_Road; c++;
                        worksheet.Cells[chars[c] + j].Value = customer.AumphurName; c++;
                        worksheet.Cells[chars[c] + j].Value = customer.TumbonName; c++;
                        worksheet.Cells[chars[c] + j].Value = customer.ProvinceName; c++;
                        worksheet.Cells[chars[c] + j].Value = customer.CUR_ZipCode; c++;
                        worksheet.Cells[chars[c] + j].Value = customer.Point; c++;
                        worksheet.Cells[chars[c] + j].Value = DateUtil.ToDisplayDateTime(customer.Create_On); c++;
                        j++;

                        if (j % 10000 == 0)
                            Console.WriteLine(DateTime.Now + ": row " + j);
                    }
                    byte[] result = package.GetAsByteArray();
                    //string filePath = "E:\\Dhipaya\\Dhipaya\\ExcelDemo" + DateUtil.ToInternalDate(DateUtil.Now()) + ".xlsx";
                    string filePath = ConfigurationManager.AppSettings["filepath"] + "\\รายชื่อสมาชิก" + DateUtil.ToInternalDate(DateUtil.Now()) + ".xlsx";


                    //write the file to the disk
                    File.WriteAllBytes(filePath, result);
                    Console.WriteLine(DateTime.Now + ": End");

                }
            }
            

        }
        static void assignbirthday()
        {

            var connString = ConfigurationManager.ConnectionStrings["DBContext"].ConnectionString;
            var queryString = new StringBuilder();
            queryString.AppendLine("select ID ");
            queryString.AppendLine("from Customers ");
            queryString.AppendLine("where Month(DOB) = Month(Getdate()) ");
            queryString.AppendLine("and day(DOB) = Day(getdate()) ");
            queryString.AppendLine("and(LastgivePointDOB is null or LastgivePointDOB != Year(getdate()))");

            var str = new StringBuilder();
            using (var connection = new SqlConnection(connString))
            {
                var command = new SqlCommand(queryString.ToString(), connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader["ID"];
                        str.Append(id);
                        str.Append("|");
                    }
                }
            }
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["url"] + "/rewardpoint/customerprofile/GenDOBPoint");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = TimeSpan.FromMinutes(60);

                StringContent content = new StringContent(JsonConvert.SerializeObject(str.ToString()), Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PutAsync(client.BaseAddress, content).Result;
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                }
            }
        }
        static void iiasyn()
        {
            var maxrequest = Convert.ToInt32(ConfigurationManager.AppSettings["maxrequest"]);
            var sleepseconds = Convert.ToInt32(ConfigurationManager.AppSettings["sleepseconds"]);
            while (1 == 1)
            {
                try
                {
                    using (var _context = new ChFrontContext())
                    {
                        var lastcnt = _context.Customers.Where(w => !string.IsNullOrEmpty(w.IDCard) & w.IIASyned == false).Count();
                        var laststamp = DateTime.Now;

                        Console.WriteLine(DateTime.Now + ": Start Remain (" + lastcnt + ")");
                        using (var client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(ConfigurationManager.AppSettings["url"] + "/rewardpoint/customerprofile/IIAStatusUpdate?max=" + maxrequest);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            client.Timeout = TimeSpan.FromMinutes(600);

                            StringContent content = new StringContent("", Encoding.UTF8, "application/json");
                            HttpResponseMessage response = client.PostAsync(client.BaseAddress, content).Result;
                            if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                            {

                            }
                            var sumsleepseconds = 0;
                            while (1 == 1)
                            {
                                var curcnt = _context.Customers.Where(w => !string.IsNullOrEmpty(w.IDCard) & w.IIASyned == false).Count();
                                Console.WriteLine(DateTime.Now + ": Current (" + curcnt + "), Expected (" + (lastcnt - maxrequest) + ")");
                                if (curcnt <= lastcnt - maxrequest)
                                {
                                    break;
                                }
                                Console.WriteLine(DateTime.Now + ": Sleep " + sleepseconds + " sec");
                                System.Threading.Thread.Sleep(sleepseconds);
                                sumsleepseconds += sleepseconds;
                                if (sumsleepseconds > ((1000 * 60) * 7))
                                    break;
                            }
                            //Console.WriteLine(response.StatusCode);
                        }
                        Console.WriteLine(DateTime.Now + ": End Used Time (" + (int)DateTime.Now.Subtract(laststamp).TotalMinutes + ")");
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(DateTime.Now +  " Error : " + ex.Message);
                    System.Threading.Thread.Sleep(60000);
                }
            }
        }
        static void iiasynauto()
        {
            while (1 == 1)
            {
                Console.WriteLine(DateTime.Now + ": Start");

                var connString = ConfigurationManager.ConnectionStrings["DBContext"].ConnectionString;
                var cusRepo = new CustomerRepository(connString);
                CustomersDTO model = new CustomersDTO();
                //model.search_text = "TIPSO18050081262";
                var customers = cusRepo.ListAllOforIIA(model);

                if (customers.Count() == 0)
                    break;

                var row = 0;
                Console.WriteLine(DateTime.Now + ": All Row (" + customers.Count() + ")");

                foreach (var customer in customers)
                {
                    var laststamp = DateTime.Now;
                    row++;
                    GetCustomerClass(customer.ID, true, true);
                    Console.WriteLine(DateTime.Now + ": ID " + customer.ID + " Row (" + row + ") Sec: " + (int)DateTime.Now.Subtract(laststamp).TotalSeconds);
                }
                Console.WriteLine(DateTime.Now + ": End");
            }
        }
        static void Main(string[] args)
        {
            //exportcustomerexcel();
            //assignbirthday();
            exportprivilegeexcel();
            //iiasynauto();
            //iiasyn();
        }
        static void GetCustomerClass(int id, bool dosave = false, bool getpoint = true)
        {
            using (var _context = new ChFrontContext())
            {
                Customer customer = _context.Customers.Where(w => w.ID == id).FirstOrDefault();
                var oldclass = customer.CustomerClassID;
                customer.CustomerClassID = 1;
                var connected = false;
                if (!string.IsNullOrEmpty(customer.IDCard))
                {
                    IIAMemberResult iia = null;
                    //if (_conf.Environment == "Dev")
                    //iia = GetRequestPolicyActiveDev(customer.IDCard, customer.NameTh, customer.SurNameTh);
                    //else
                    iia = GetRequestPolicyActive(customer.IDCard, customer.NameTh, customer.SurNameTh);

                    if (iia != null && (iia.resultCode == "Y" | iia.resultCode == "N"))
                    {
                        connected = true;
                    }
                    var avaliable = false;

                    if (iia != null && iia.resultCode == "Y")
                    {
                        var havecustom = false;
                        if (iia.data != null)
                        {
                            foreach (var item in iia.data)
                            {
                                if (DateUtil.ToDate(item.ExpiryDate, monthfirst: true).Value.AddDays(1) >= DateUtil.Now())
                                    avaliable = true;

                                foreach (var c in _context.CustomerClasses.Where(w => !w.UnEditable & w.Status == StatusType.Active))
                                {
                                    if (item.ProjectCode == c.ProjectCode && DateUtil.ToDate(item.ExpiryDate, monthfirst: true).Value.AddDays(1) >= DateUtil.Now())
                                    {
                                        havecustom = true;
                                        customer.CustomerClassID = c.ID;
                                        break;
                                    }
                                }

                            }
                        }
                        if (havecustom == false && avaliable)
                        {
                            customer.CustomerClassID = 2;
                        }

                    }
                    customer.IIASyned = true;
                    if (customer.IIAIgnoreSyned & customer.CustomerClassID == 1 && avaliable)
                    {
                        customer.CustomerClassID = 2;
                    }

                    if (customer.ID > 0 && connected)
                    {
                        if (oldclass != customer.CustomerClassID)
                        {
                            var latest = _context.CustomerClassChanges.Where(w => w.CustomerID == customer.ID).OrderByDescending(o => o.ID).FirstOrDefault();
                            var log = new CustomerClassChange();
                            log.FromID = oldclass;
                            log.ToID = customer.CustomerClassID;
                            var oldc = _context.CustomerClasses.Where(w => w.ID == log.FromID).FirstOrDefault();
                            if (oldc != null)
                                log.From = oldc.Name;
                            var newc = _context.CustomerClasses.Where(w => w.ID == log.ToID).FirstOrDefault();
                            if (newc != null)
                                log.To = newc.Name;

                            log.Create_On = DateUtil.Now();
                            log.Create_By = customer.Update_By;
                            log.CustomerID = customer.ID;

                            if (latest == null || latest.FromID != log.FromID || latest.ToID != log.ToID)
                            {
                                _context.CustomerClassChanges.Add(log);
                            }
                        }

                        if (iia != null && iia.resultCode == "Y" && getpoint)
                        {
                            foreach (var item in iia.data)
                            {
                                if (!string.IsNullOrEmpty(item.PolicyNo))
                                {
                                    var pointed = _context.CustomerPoints.Where(w => w.PolicyNo == item.PolicyNo & w.CustomerID == customer.ID & w.ChannelType == ChannelType.IIA).FirstOrDefault();
                                    if (pointed == null)
                                    {
                                        var trantype = TransacionTypeID.BuyInsure;
                                        if (!string.IsNullOrEmpty(item.PreviousPolicyNo))
                                            trantype = TransacionTypeID.Renew;
                                        IEnumerable<PointCondition> cons = GetPointCondition(_context, customer, trantype, item.InsuranceClass, item.OutletCode, ChannelType.IIA, item.SubClass);
                                        if (cons != null)
                                        {
                                            foreach (var con in cons)
                                            {
                                                var p = GetPoint(_context, con, customer, limited: false);
                                                if (p > 0)
                                                {
                                                    var point = GetCustomerPointByIIA(con, customer, p, (int)trantype, item);
                                                    if (!string.IsNullOrEmpty(item.InsuranceClass))
                                                    {
                                                        var product = _context.Products.Where(w => w.ProductCode == item.InsuranceClass).FirstOrDefault();
                                                        if (product != null)
                                                            point.ProductID = product.ProductID;
                                                    }
                                                    _context.Add(point);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (dosave)
                {
                    customer.Update_On = DateUtil.Now();
                    customer.Update_By = "IIACheck";
                    _context.SaveChanges();
                }
            }
        }
        static int GetPoint(ChFrontContext _context, PointCondition condition, Customer model, decimal? purchaseAmt = 0, bool limited = true)
        {
            int point = 0;
            if (condition != null)
            {
                /*Point Type*/
                if (condition.PointType == PointType.Percentage)
                {
                    if (condition.Percent > 0 && purchaseAmt > 0)
                    {
                        point = NumUtil.ParseInteger(purchaseAmt.Value * (condition.Percent.Value / 100));
                    }
                }
                else if (condition.PointType == PointType.Calculate)
                {
                    if (condition.CalPoint > 0 && condition.CalPointPurchaseAmt > 0 && purchaseAmt > 0)
                    {
                        point = NumUtil.ParseInteger(Math.Floor(purchaseAmt.Value / condition.CalPointPurchaseAmt.Value) * condition.CalPoint.Value);
                    }
                }
                else if (condition.PointType == PointType.Tier)
                {
                    if (purchaseAmt > 0)
                    {
                        var tiers = _context.PointConditionTiers.Where(w => w.ConditionID == condition.ConditionID && w.PurchaseAmtFrom <= purchaseAmt && w.PurchaseAmtTo >= purchaseAmt);
                        foreach (var tier in tiers)
                        {
                            if (tier.NumberType == NumberType.Percent)
                            {
                                if (tier.Percent > 0)
                                    point += NumUtil.ParseInteger(purchaseAmt.Value * (tier.Percent.Value / 100));
                            }
                            else
                            {
                                if (tier.Point > 0)
                                    point += NumUtil.ParseInteger(tier.Point.Value);
                            }
                        }
                    }
                }
                else
                    point = NumUtil.ParseInteger(condition.Point.HasValue ? condition.Point.Value : 0);

                if (limited == true)
                {
                    /*Calculate Limit*/

                    var limit = _context.PointLimits.FirstOrDefault();
                    if (limit != null)
                    {
                        var now = DateUtil.Now();
                        if (limit.Period == Period.Once)
                        {
                            if (limit.LimitedOnce > 0 & point > limit.LimitedOnce)
                            {
                                return limit.LimitedOnce.Value;
                            }
                        }
                        else if (limit.Period == Period.Day)
                        {
                            if (limit.LimitedDay > 0)
                            {
                                var allpoint = _context.CustomerPoints.Where(w => w.CustomerID == model.ID & w.Create_On.Value.Date == now.Date).Sum(w => w.Point);
                                if (allpoint >= limit.LimitedDay)
                                    return 0;
                                else if (point + allpoint > limit.LimitedDay)
                                    return limit.LimitedDay.Value - allpoint;
                            }
                        }
                        else if (limit.Period == Period.Week)
                        {
                            if (limit.LimitedWeek > 0)
                            {
                                var allpoint = _context.CustomerPoints.Where(w => w.CustomerID == model.ID & w.Create_On.Value.Date > now.Date.AddDays(-7) & w.Create_On.Value.Date < now.Date.AddDays(7)).Sum(w => w.Point);
                                if (allpoint >= limit.LimitedWeek)
                                    return 0;
                                else if (point + allpoint > limit.LimitedWeek)
                                    return limit.LimitedWeek.Value - allpoint;
                            }

                        }
                        else if (limit.Period == Period.Month)
                        {
                            if (limit.LimitedMonth > 0)
                            {
                                var allpoint = _context.CustomerPoints.Where(w => w.CustomerID == model.ID & w.Create_On.Value.Month == now.Month).Sum(w => w.Point);
                                if (allpoint >= limit.LimitedMonth)
                                    return 0;
                                else if (point + allpoint > limit.LimitedMonth)
                                    return limit.LimitedMonth.Value - allpoint;
                            }
                        }
                    }
                }


            }

            return point;
        }
        static IEnumerable<PointCondition> GetPointCondition(ChFrontContext _context, Customer model, TransacionTypeID trantype, string code = null, string outlet = null, ChannelType ch = ChannelType.Other, string subclass = null)
        {

            if (model == null)
                return null;
            if (model.Status == UserStatusType.BlockAward)
                return null;

            var conditions = _context.PointConditions
               .Include(i => i.PointConditionProducts)
               .Where(w => w.TransacionTypeID == (int)trantype
               & w.Status == StatusType.Active
               & (!w.StartDate.HasValue || w.StartDate.Value.Date <= DateUtil.Now().Date)
               & (!w.EndDate.HasValue || w.EndDate.Value.Date >= DateUtil.Now().Date));

            if (trantype == TransacionTypeID.BuyInsure | trantype == TransacionTypeID.Renew)
            {
                if (outlet == OutletCode.MobileApplication | outlet == OutletCode.TipInsureWeb)
                {
                    conditions = conditions.Where(w => w.OutletCode == outlet);
                }
                else
                {
                    conditions = conditions.Where(w => w.OutletCode == OutletCode.Other);
                }

                if (ch != ChannelType.Other)
                {
                    conditions = conditions.Where(w => w.ChannelType == ch);
                }
            }

            var currentdate = DateUtil.Now();

            if (!string.IsNullOrEmpty(code) & (trantype == TransacionTypeID.BuyInsure | trantype == TransacionTypeID.Renew))
            {
                var products = _context.Products.Where(w => w.ProductCode == code);
                if (!string.IsNullOrEmpty(subclass))
                    products = products.Where(w => (w.SubProductCode == subclass | string.IsNullOrEmpty(w.SubProductCode)));

                var product = products.FirstOrDefault();
                if (product != null)
                    conditions = conditions.Where(w => w.ConditionCode == code | w.PointConditionProducts.Where(w2 => w2.ProductID == product.ProductID).FirstOrDefault() != null);
                else
                    conditions = conditions.Where(w => w.ConditionCode == code);
            }
            if (ch != ChannelType.IIA)
                conditions = conditions.Where(w => w.PointConditionCustomerClasses.Any(s => s.CustomerClassID == model.CustomerClassID));

            if (model.DOB.HasValue)
            {
                if (model.DOB.Value.Day != currentdate.Day | model.DOB.Value.Month != currentdate.Month)
                    conditions = conditions.Where(w => w.IsForBirthday == false);
            }
            else
                conditions = conditions.Where(w => w.IsForBirthday == false);

            if (currentdate.DayOfWeek == DayOfWeek.Sunday)
                conditions = conditions.Where(w => w.IsAllDay == true | w.IsSun == true);
            else if (currentdate.DayOfWeek == DayOfWeek.Monday)
                conditions = conditions.Where(w => w.IsAllDay == true | w.IsMon == true);
            else if (currentdate.DayOfWeek == DayOfWeek.Tuesday)
                conditions = conditions.Where(w => w.IsAllDay == true | w.IsTue == true);
            else if (currentdate.DayOfWeek == DayOfWeek.Wednesday)
                conditions = conditions.Where(w => w.IsAllDay == true | w.IsWed == true);
            else if (currentdate.DayOfWeek == DayOfWeek.Thursday)
                conditions = conditions.Where(w => w.IsAllDay == true | w.IsThu == true);
            else if (currentdate.DayOfWeek == DayOfWeek.Friday)
                conditions = conditions.Where(w => w.IsAllDay == true | w.IsFri == true);
            else if (currentdate.DayOfWeek == DayOfWeek.Saturday)
                conditions = conditions.Where(w => w.IsAllDay == true | w.IsSat == true);
            return conditions;
        }

        static CustomerPoint GetCustomerPointByIIA(PointCondition item, Customer customer, int pointamt, int typeID, IIAMemberData data)
        {
            var point = new CustomerPoint();
            point.Code = item.ConditionCode;
            point.Name = item.Name;
            point.Point = pointamt;
            point.TransacionTypeID = typeID;
            point.CustomerChanal = customer.Channel;
            point.Source = "iia-offline";
            point.ChannelType = ChannelType.IIA;
            point.CustomerID = customer.ID;
            point.ProjectCode = data.ProjectCode;
            point.ProjectName = data.ProjectName;
            point.PolicyNo = data.PolicyNo;
            point.PreviousPolicyNo = data.PreviousPolicyNo;
            point.OutletCode = data.OutletCode;
            point.InsuranceClass = data.InsuranceClass;
            point.Subclass = data.SubClass;
            point.EffectiveDate = DateUtil.ToDate(data.EffectiveDate, monthfirst: true);
            point.ExpiryDate = DateUtil.ToDate(data.ExpiryDate, monthfirst: true);

            point.Create_On = DateUtil.Now();
            point.Create_By = customer.User.UserName;
            point.Update_On = DateUtil.Now();
            point.Update_By = customer.User.UserName;
            return point;
        }

        static CustomerPoint GetCustomerPoint(PointCondition item, Customer customer, int pointamt, int typeID, CustomerChanal chanal, string source)
        {
            var point = new CustomerPoint();
            point.Code = item.ConditionCode;
            point.Name = item.Name;
            point.Point = pointamt;
            point.TransacionTypeID = typeID;
            point.CustomerChanal = chanal;
            point.Source = source;
            point.ChannelType = ChannelType.Online;
            point.Create_On = DateUtil.Now();
            point.Create_By = customer.User.UserName;
            point.Update_On = DateUtil.Now();
            point.Update_By = customer.User.UserName;
            return point;
        }

        static XmlDocument CreateSoapGetPolicyActive(string id, string name, string surname)
        {
            XmlDocument soapEnvelop = new XmlDocument();
            var requiredXML = new StringBuilder();
            requiredXML.Append(@"<soap-env:Envelope xmlns:soap-env=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"">");
            requiredXML.Append(@"<soap-env:Header/>");
            requiredXML.Append(@"<soap-env:Body>");
            requiredXML.Append(@"<tem:CheckMemberStatus>");
            requiredXML.Append(@"<tem:idCardNo>" + id + "</tem:idCardNo>");
            requiredXML.Append(@"<tem:firstname>" + name + "</tem:firstname>");
            requiredXML.Append(@"<tem:lastname>" + surname + "</tem:lastname>");
            requiredXML.Append(@"</tem:CheckMemberStatus>");
            requiredXML.Append(@"</soap-env:Body>");
            requiredXML.Append(@"</soap-env:Envelope>");
            soapEnvelop.LoadXml(requiredXML.ToString());
            return soapEnvelop;
        }
        static IIAMemberResult GetRequestPolicyActiveDev(string id, string name, string surname)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["url"] + "/rewardpoint/customerprofile/IIACheck?name=" + name + "&surname=" + surname + "&idc=" + id);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                StringContent content = new StringContent(JsonConvert.SerializeObject(new { }), Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(client.BaseAddress, content).Result;
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    var responseresult = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<IIAMemberResult>(responseresult);
                    if (result.data != null)
                        result.data = result.data.Where(w => !string.IsNullOrEmpty(w.PolicyNo)).OrderByDescending(o => DateUtil.ToDate(o.EffectiveDate, monthfirst: true)).ToArray();
                    return result;
                }
            }
            return new IIAMemberResult();
        }
        static IIAMemberResult GetRequestPolicyActive(string id, string name, string surname)
        {
            var _result = new IIAMemberResult();
            XmlDocument soapRequest = CreateSoapGetPolicyActive(id, name, surname);
            using (var client = new HttpClient())
            {
                try
                {
                    var request = new HttpRequestMessage()
                    {
                        RequestUri = new Uri(ConfigurationManager.AppSettings["EndPoint"]),
                        Method = HttpMethod.Post,
                        Content = new StringContent(soapRequest.InnerXml, Encoding.UTF8, "text/xml")
                    };

                    request.Headers.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
                    request.Headers.Add("ContentType", "text/xml;charset=\"utf-8\";");

                    request.Headers.Add("SOAPAction", ConfigurationManager.AppSettings["CheckMemberStatus"]); //I want to call this method 
                    HttpResponseMessage response = client.SendAsync(request).Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        return new IIAMemberResult() { resultCode = "-1", resultDescription = response.RequestMessage.ToString() };
                    }

                    Task<Stream> streamTask = response.Content.ReadAsStreamAsync();
                    Stream stream = streamTask.Result;
                    var sr = new StreamReader(stream);
                    XDocument soapResponse = XDocument.Load(sr);
                    XElement el = soapResponse.Descendants().First(x => x.Name.LocalName == "CheckMemberStatusResult");
                    var val = el.Value;
                    _result = JsonConvert.DeserializeObject<IIAMemberResult>(val);
                    if (_result.data != null)
                        _result.data = _result.data.Where(w => !string.IsNullOrEmpty(w.PolicyNo)).OrderByDescending(o => DateUtil.ToDate(o.EffectiveDate, monthfirst: true)).ToArray();

                    //do some other stuff...
                    return _result;
                }
                catch (Exception ex)
                {
                    _result.resultDescription = ex.Message;
                    _result.resultCode = "-1";
                }
                return _result;
            }
        }

    }
}
