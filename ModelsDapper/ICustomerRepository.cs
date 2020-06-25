using Dapper;
using DhipayaBGProcess.Extensions;
using DhipayaBGProcess.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dhipaya.DhipayaBGProcess
{
    public interface ICustomerRepository
    {
        Task<Customer> GetByID(int id);
        Task<int> GetPoint(int id);
        Task<IEnumerable<Customer>> List(ModelReportBaseDTO model);
        IEnumerable<Customer> ListAll(ModelReportBaseDTO model);

        IEnumerable<PrivilegeReportDTO> ListPrivilege();

    }

    public class CustomerRepository : ICustomerRepository
    {
        private string _connString;

        public CustomerRepository(string connString)
        {
            _connString = connString;
        }

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_connString);
            }
        }
        public async Task<Customer> GetByID(int id)
        {
            using (IDbConnection conn = Connection)
            {
                string sQuery = "SELECT * FROM Customers WHERE ID = @ID";
                conn.Open();
                var result = await conn.QueryAsync<Customer>(sQuery, new { ID = id });
                return result.FirstOrDefault();
            }
        }

        public async Task<IEnumerable<Customer>> List(ModelReportBaseDTO model)
        {
            using (IDbConnection conn = Connection)
            {
                if (model.pmax > 0)
                {
                    var sQuery = new StringBuilder();
                    sQuery.Append(" DECLARE @Page int");
                    sQuery.Append(" SET @Page = " + model.pno);
                    sQuery.Append(" DECLARE @Amount int");
                    sQuery.Append(" SET @Amount = " + model.pmax);

                    sQuery.Append(" SELECT c.*, u.*, cl.* FROM(");
                    sQuery.Append(" SELECT c.*,");
                    sQuery.Append(" (Select Sum( p.Point ) from CustomerPoints p where p.CustomerID = c.ID) Point, ");
                    sQuery.Append(" (Select  Sum(  r.Point) from Redeems r where  r.CustomerID = c.ID) RedeemPoint,");
                    sQuery.Append(" ROW_NUMBER() OVER(");
                    if (!string.IsNullOrEmpty(model.orderby))
                        sQuery.Append(" order by c." + model.orderby);
                    else
                        sQuery.Append(" order by c.NameTh, c.SurNameTh");
                    sQuery.Append(" ) AS rownumber FROM Customers c");
                    sQuery.Append(" INNER JOIN Users u on u.ID = c.UserID ");
                    sQuery.Append(" WHERE 1 =1 ");
                    if (model.customerClassID.HasValue)
                        sQuery.Append(" AND c.CustomerClassID = " + model.customerClassID);

                    if (model.search_user_type.HasValue)
                        sQuery.Append(" AND c.UserLevel = " + (int)model.search_user_type);

                    if (model.customer_chanal.HasValue)
                        sQuery.Append(" AND c.Channel = " + (int)model.customer_chanal);

                    if (model.search_birthday.HasValue)
                    {
                        sQuery.Append(" AND Day(c.DOB) = " + model.search_birthday);
                    }
                    if (model.search_birthmonth.HasValue)
                    {
                        sQuery.Append(" AND Month(c.DOB) = " + model.search_birthmonth);
                    }
                    if (model.search_birthyear.HasValue)
                    {
                        sQuery.Append(" AND Year(c.DOB) = " + model.search_birthyear);
                    }
                    if (!string.IsNullOrEmpty(model.search_sdate))
                    {
                        var date = DateUtil.ToDate(model.search_sdate);
                        var datetimester = DateUtil.ToInternalDate(date);
                        sQuery.Append(" AND c.Create_On >= convert(datetime, '" + datetimester + "', 101)");
                    }
                    if (!string.IsNullOrEmpty(model.search_edate))
                    {
                        var date = DateUtil.ToDate(model.search_edate).Value.AddDays(1);
                        var datetimester = DateUtil.ToInternalDate(date);
                        sQuery.Append(" AND c.Create_On <= convert(datetime, '" + datetimester + "', 101)");
                    }
                    if (model.dup == 1)
                    {
                        sQuery.Append(" AND c.Idcard in (select IDCard from CustomerDups)");
                        sQuery.Append(" AND c.Idcard is not null and c.idcard <> ''");
                    }
                    if (!string.IsNullOrEmpty(model.search_text))
                    {
                        var text = model.search_text.Replace(" ", "").ToLower().Trim();
                        sQuery.Append(" and (");
                        sQuery.Append("  REPLACE(c.NameTh,' ','') like N'%" + text + "%'");
                        sQuery.Append(" OR  REPLACE(c.SurNameTh,' ','') like N'%" + text + "%'");
                        sQuery.Append(" OR  REPLACE(c.Email,' ','') like N'%" + text + "%'");
                        sQuery.Append(" OR  REPLACE(u.UserName,' ','') like N'%" + text + "%'");
                        sQuery.Append(" OR  REPLACE(c.MoblieNo,' ','') like N'%" + text + "%'");
                        sQuery.Append(" OR  REPLACE(c.IDCard,' ','') like N'%" + text + "%'");
                        sQuery.Append(" OR  REPLACE(c.NameEn,' ','') like N'%" + text + "%'");
                        sQuery.Append(" OR  REPLACE(c.SurNameEn,' ','') like N'%" + text + "%'");
                        sQuery.Append(" OR  REPLACE(c.RefCode,' ','') like N'%" + text + "%'");
                        sQuery.Append(" OR  REPLACE(c.FriendCode,' ','') like N'%" + text + "%'");
                        sQuery.Append(" OR  REPLACE(c.NameTh + c.SurNameTh,' ','') like N'%" + text + "%'");
                        sQuery.Append(" )");
                    }

                    sQuery.Append(" ) as c");
                    sQuery.Append(" INNER JOIN Users u on u.ID = c.UserID ");
                    sQuery.Append(" INNER JOIN CustomerClasses cl on cl.ID = c.CustomerClassID ");
                    sQuery.Append(" WHERE rownumber >= (@Page)* @Amount-(@Amount - 1) AND rownumber <= (@Page) * @Amount");

                    conn.Open();

                    var sQueryCnt = sQuery.ToString().Replace("c.*, u.*, cl.*", " COUNT(DISTINCT c.ID) ");
                    sQueryCnt = sQueryCnt.Replace("WHERE rownumber >= (@Page)* @Amount-(@Amount - 1) AND rownumber <= (@Page) * @Amount", "");
                    model.totalrow = conn.Query<int>(sQueryCnt.ToString()).FirstOrDefault();


                    if (!string.IsNullOrEmpty(model.orderby))
                        sQuery.Append(" order by c." + model.orderby);
                    else
                        sQuery.Append(" order by c.NameTh, c.SurNameTh");
                    var result = await conn.QueryAsync<Customer, User, CustomerClass, Customer>(
                                        sQuery.ToString(), (customer, user, cClass) =>
                                        {
                                       //customer.Point = GetPoint(customer.ID);
                                       customer.User = user;
                                            customer.CustomerClass = cClass;
                                            return customer;
                                        });
                    conn.Close();


                    return result;
                }
                return null;
            }
        }
        public  IEnumerable<Customer> ListAll(ModelReportBaseDTO model)
        {
            using (IDbConnection conn = Connection)
            {
                var sQuery = new StringBuilder();

                sQuery.Append(" SELECT c.*,p.ProvinceName ,a.AumphurName, t.TumbonName,");
                sQuery.Append(" (isnull((select sum(cp.point) from CustomerPoints cp where cp.CustomerID = c.id),0) - isnull((select sum(r.point) from Redeems r where r.CustomerID = c.ID),0)) as Point,");
                sQuery.Append(" u.*, cl.* ");
                sQuery.Append(" FROM Customers c");
                sQuery.Append(" INNER JOIN Users u on u.ID = c.UserID ");
                sQuery.Append(" INNER JOIN CustomerClasses cl on cl.ID = c.CustomerClassID ");
                sQuery.Append(" left join Provinces p on c.CUR_Province = p.ProvinceID ");
                sQuery.Append(" left join Aumphurs a on c.CUR_Aumper =a.AumphurID ");
                sQuery.Append(" left join Tumbons t on c.CUR_Tumbon = t.TumbonID ");
                sQuery.Append(" WHERE 1 =1 ");
                if (model.customerClassID.HasValue)
                    sQuery.Append(" AND c.CustomerClassID = " + model.customerClassID);

                if (model.search_user_type.HasValue)
                    sQuery.Append(" AND c.UserLevel = " + (int)model.search_user_type);

                if (model.customer_chanal.HasValue)
                    sQuery.Append(" AND c.Channel = " + (int)model.customer_chanal);

                if (model.search_birthday.HasValue)
                {
                    sQuery.Append(" AND Day(c.DOB) = " + model.search_birthday);
                }
                if (model.search_birthmonth.HasValue)
                {
                    sQuery.Append(" AND Month(c.DOB) = " + model.search_birthmonth);
                }
                if (model.search_birthyear.HasValue)
                {
                    sQuery.Append(" AND Year(c.DOB) = " + model.search_birthyear);
                }
                if (!string.IsNullOrEmpty(model.search_sdate))
                {
                    var date = DateUtil.ToDate(model.search_sdate);
                    var datetimester = DateUtil.ToInternalDate(date);
                    sQuery.Append(" AND c.Create_On >= convert(datetime, '" + datetimester + "', 101)");
                }
                if (!string.IsNullOrEmpty(model.search_edate))
                {
                    var date = DateUtil.ToDate(model.search_edate).Value.AddDays(1);
                    var datetimester = DateUtil.ToInternalDate(date);
                    sQuery.Append(" AND c.Create_On <= convert(datetime, '" + datetimester + "', 101)");
                }

                if (!string.IsNullOrEmpty(model.search_text))
                {
                    var text = model.search_text.Replace(" ", "").ToLower().Trim();
                    sQuery.Append(" and (");
                    sQuery.Append("  REPLACE(c.NameTh,' ','') like N'%" + text + "%'");
                    sQuery.Append(" OR  REPLACE(c.SurNameTh,' ','') like N'%" + text + "%'");
                    sQuery.Append(" OR  REPLACE(c.Email,' ','') like N'%" + text + "%'");
                    sQuery.Append(" OR  REPLACE(u.UserName,' ','') like N'%" + text + "%'");
                    sQuery.Append(" OR  REPLACE(c.MoblieNo,' ','') like N'%" + text + "%'");
                    sQuery.Append(" OR  REPLACE(c.IDCard,' ','') like N'%" + text + "%'");
                    sQuery.Append(" OR  REPLACE(c.NameEn,' ','') like N'%" + text + "%'");
                    sQuery.Append(" OR  REPLACE(c.SurNameEn,' ','') like N'%" + text + "%'");
                    sQuery.Append(" OR  REPLACE(c.RefCode,' ','') like N'%" + text + "%'");
                    sQuery.Append(" OR  REPLACE(c.FriendCode,' ','') like N'%" + text + "%'");
                    sQuery.Append(" OR  REPLACE(c.NameTh + c.SurNameTh,' ','') like N'%" + text + "%'");
                    sQuery.Append(" )");
                }
                conn.Open();

                if (!string.IsNullOrEmpty(model.orderby))
                    sQuery.Append(" order by c." + model.orderby);
                else
                    sQuery.Append(" order by c.NameTh, c.SurNameTh");
                var result = conn.Query<Customer, User, CustomerClass, Customer>(
                                    sQuery.ToString(), (customer, user, cClass) =>
                                    {
                                        customer.User = user;
                                        customer.CustomerClass = cClass;
                                        return customer;
                                    });

                conn.Close();


                return result;

            }
        }

        public IEnumerable<PrivilegeReportDTO> ListPrivilege()
        {
            using (IDbConnection conn = Connection)
            {
                var sQuery = new StringBuilder();

                sQuery.Append(" select p.PrivilegeID, m.MerchantName, c.CategoryName, p.PrivilegeName, pro.ProvinceName, p.StartDate, p.EndDate, p.CreditPoint, ");
                sQuery.Append(" CASE");
                sQuery.Append(" WHEN(select count(*) from PrivilegeCustomerClasses pc where pc.PrivilegeID = p.PrivilegeID and pc.CustomerClassID = 1) = 1  THEN 'Y'");
                sQuery.Append(" ELSE ''");
                sQuery.Append(" END as TipSilver,");
                sQuery.Append(" CASE");
                sQuery.Append(" WHEN(select count(*) from PrivilegeCustomerClasses pc where pc.PrivilegeID = p.PrivilegeID and pc.CustomerClassID = 2) = 1  THEN 'Y'");
                sQuery.Append(" ELSE ''");
                sQuery.Append(" END as TipGold,");
                sQuery.Append(" CASE");
                sQuery.Append(" WHEN(select count(*) from PrivilegeCustomerClasses pc where pc.PrivilegeID = p.PrivilegeID and pc.CustomerClassID = 3) = 1  THEN 'Y'");
                sQuery.Append(" ELSE ''");
                sQuery.Append(" END as TipGoldLady,");
                sQuery.Append(" CASE");
                sQuery.Append(" WHEN p.Status = 1  THEN 'Active'");
                sQuery.Append(" WHEN p.Status = 0 THEN 'Inactive'");
                sQuery.Append(" ELSE 'Inactive'");
                sQuery.Append(" END as Status,");
                sQuery.Append(" (select count(*) from Redeems r where r.PrivilegeID = p.PrivilegeID) as RedeemCount");
                sQuery.Append(" from Privileges p");
                sQuery.Append(" inner");
                sQuery.Append(" join Merchants m on p.MerchantID = m.MerchantID");
                sQuery.Append(" inner");
                sQuery.Append(" join MerchantCategories c  on c.CategoryID = m.CategoryID");
                sQuery.Append(" left");
                sQuery.Append(" join Provinces pro on pro.ProvinceID = p.PrivilegeID");
                sQuery.Append(" order by m.MerchantID, p.PrivilegeID");

                conn.Open();

                var result = conn.Query<PrivilegeReportDTO>(sQuery.ToString());

                conn.Close();


                return result;

            }
        }
        public IEnumerable<Customer> ListAllOforIIA(ModelReportBaseDTO model)
        {
            using (IDbConnection conn = Connection)
            {
                var sQuery = new StringBuilder();

                sQuery.Append(" SELECT top 100 c.*");
                sQuery.Append(" FROM Customers c");
                sQuery.Append(" WHERE 1 =1 and c.IDCard is not null and c.IDCard <> '' and IIASyned = 0");               

                if (!string.IsNullOrEmpty(model.search_text))
                {
                    var text = model.search_text.Replace(" ", "").ToLower().Trim();
                    sQuery.Append(" and (");
                    sQuery.Append("  REPLACE(c.NameTh,' ','') like N'%" + text + "%'");
                    sQuery.Append(" OR  REPLACE(c.SurNameTh,' ','') like N'%" + text + "%'");
                    sQuery.Append(" OR  REPLACE(c.Email,' ','') like N'%" + text + "%'");
                    sQuery.Append(" OR  REPLACE(c.MoblieNo,' ','') like N'%" + text + "%'");
                    sQuery.Append(" OR  REPLACE(c.IDCard,' ','') like N'%" + text + "%'");
                    sQuery.Append(" OR  REPLACE(c.NameEn,' ','') like N'%" + text + "%'");
                    sQuery.Append(" OR  REPLACE(c.SurNameEn,' ','') like N'%" + text + "%'");
                    sQuery.Append(" OR  REPLACE(c.RefCode,' ','') like N'%" + text + "%'");
                    sQuery.Append(" OR  REPLACE(c.FriendCode,' ','') like N'%" + text + "%'");
                    sQuery.Append(" OR  REPLACE(c.NameTh + c.SurNameTh,' ','') like N'%" + text + "%'");
                    sQuery.Append(" )");
                }
                sQuery.Append(" order by c.ID");

                conn.Open();
                var result = conn.Query< Customer>(sQuery.ToString());
                conn.Close();


                return result;

            }
        }
        public async Task<int> GetPoint(int id)
        {
            using (IDbConnection conn = Connection)
            {
                conn.Open();

                var sQuery = new StringBuilder();
                sQuery.Append(" select(case when p.Point is null then 0 else p.Point end + case when r.Point is null then 0 else p.Point end) as Point");
                sQuery.Append(" from Customers c");
                sQuery.Append(" left join CustomerPoints p on p.CustomerID = c.ID");
                sQuery.Append(" left join Redeems r on r.CustomerID = c.ID");
                sQuery.Append(" where c.ID = @ID");
                var result = await conn.QueryAsync<int>(sQuery.ToString(), new { ID = id });
                conn.Close();
                return result.FirstOrDefault();
            }
        }
        public IEnumerable<CustomerClass> ListCustomerClass()
        {
            using (IDbConnection conn = Connection)
            {
                conn.Open();

                var sQuery = new StringBuilder();
                sQuery.Append(" select *");
                sQuery.Append(" from CustomerClasses");
                sQuery.Append(" where UnEditable = 0 and Status = 1");
                var result = conn.Query<CustomerClass>(sQuery.ToString());
                conn.Close();
                return result;
            }
        }

        public CustomerClassChange LastCustomerClassChanges(int id)
        {
            using (IDbConnection conn = Connection)
            {
                conn.Open();

                var sQuery = new StringBuilder();
                sQuery.Append(" select top 1 *");
                sQuery.Append(" from CustomerClassChanges ");
                sQuery.Append(" where CustomerID = @ID order by ID desc");
                var result = conn.Query<CustomerClassChange>(sQuery.ToString());
                conn.Close();
                return result.FirstOrDefault();
            }
        }
        public CustomerClass GetCustomerClass(int? id)
        {
            using (IDbConnection conn = Connection)
            {
                conn.Open();

                var sQuery = new StringBuilder();
                sQuery.Append(" select *");
                sQuery.Append(" from GetCustomerClasses ");
                sQuery.Append(" where ID = @ID ");
                var result = conn.Query<CustomerClass>(sQuery.ToString());
                conn.Close();
                return result.FirstOrDefault();
            }
        }
    }
    public class PrivilegeReportDTO
    {
        public string PrivilegeID { get; set; }
        public string MerchantName { get; set; }
        public string CategoryName { get; set; }
        public string PrivilegeName { get; set; }
        public string ProvinceName { get; set; }
        public decimal? CreditPoint { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string TipSilver { get; set; }
        public string TipGold { get; set; }
        public string TipGoldLady { get; set; }
        public string Status { get; set; }
        public decimal? RedeemCount { get; set; }

    }
    public class ModelReportBaseDTO
    {
        public string search_text { get; set; }
        public string search_equal_text { get; set; }
        public int? search_birthday { get; set; }
        public int? search_birthmonth { get; set; }
        public int? search_birthyear { get; set; }
        public string search_sdate { get; set; }
        public string search_edate { get; set; }
        public int? search_privilege { get; set; }
        public int? search_product_id { get; set; }
        public int? search_category_id { get; set; }
        public int? search_trantype { get; set; }
        public int? customerClassID { get; set; }
        public int? customerClassID2 { get; set; }
        public CustomerChanal? customer_chanal { get; set; }
        public UserLevelType? search_user_type { get; set; }
        public RedeemType? search_redeemtype { get; set; }

        public string search_code { get; set; }

        public int? pno { get; set; }
        public int? totalrow { get; set; }
        public int? pmax { get; set; }

        public string orderby { get; set; }
        public int? dup { get; set; }

    }
}
