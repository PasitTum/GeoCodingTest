using CrystalDecisions.CrystalReports.Engine;
using GeoCodingTest.Class;
using GeoCodingTest.Models.L1;
using GeoCodingTest.Models.L2;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Text;
using static GeoCodingTest.Models.L2.DBSWGE2;


namespace GeoCodingTest.Controllers
{
    public class GeoCodingController : Controller
    {
        InterfaceLog log = new InterfaceLog(ConfigurationManager.AppSettings["LogsPath"]);
        //DateTime date2020 = new DateTime(2020, 07, 01);
        DateTime date2020 = DateTime.ParseExact(ConfigurationManager.AppSettings["Date"], "yyyy-MM-dd", CultureInfo.InvariantCulture);
        DateTime timenow = DateTime.Now;
        /*private void LineNotify(string lineToken, string message)
        {
            try
            {
                message = System.Web.HttpUtility.UrlEncode(message, Encoding.UTF8);
                var request = (HttpWebRequest)WebRequest.Create("https://notify-api.line.me/api/notify");
                var postData = string.Format("message={0}", message);
                var data = Encoding.UTF8.GetBytes(postData);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                request.Headers.Add("Authorization", "Bearer " + lineToken);
                var stream = request.GetRequestStream();
                stream.Write(data, 0, data.Length);
                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        */

        [HttpPost]
        public ActionResult getLocation(string location)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                string[] location1 = location.Split(',');
                string[] latitude1 = location1[0].Split(':');
                string[] longitude1 = location1[1].Split(':');
                string cardcode1 = location1[3];
                string cardname1 = location1[4];
                int invno1 = Convert.ToInt32(location1[5]);

                string[] datebillcheck = location1[6].Split(' ');
                int datebillcheck1 = datebillcheck[0].Count();
                string[] duedatebillcheck = location1[8].Split(' ');
                int duedatebillcheck1 = duedatebillcheck[0].Count();

                string datebill1 = location1[6];
                string duedatebill1 = location1[8];
                string datebill2;
                DateTime datebill3;
                string duedatebill2;
                DateTime duedatebill3;

                if (datebill1.Contains("AM") == true)
                {
                    datebill2 = datebill1.Substring(0, datebillcheck1);
                    datebill3 = DateTime.ParseExact(datebill2, "M/d/yyyy", CultureInfo.InvariantCulture);

                    duedatebill2 = duedatebill1.Substring(0, duedatebillcheck1);
                    duedatebill3 = DateTime.ParseExact(duedatebill2, "M/d/yyyy", CultureInfo.InvariantCulture);
                }
                else
                {
                    datebill2 = datebill1.Substring(0, 10);
                    datebill3 = DateTime.ParseExact(datebill2, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                    duedatebill2 = duedatebill1.Substring(0, 10);
                    duedatebill3 = DateTime.ParseExact(duedatebill2, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                decimal total1 = Convert.ToDecimal(location1[7]);

                string name1 = location1[9];
                int code1 = Convert.ToInt32(location1[10]);
                int typeid1 = Convert.ToInt32(location1[11]);

                using (var db = new DBS_WGE_Entities())
                {

                    LocationTest lt = new LocationTest();
                    lt.Latitude = latitude1[1];
                    lt.Longitude = longitude1[1];
                    lt.CardCode = cardcode1;
                    lt.CardName = cardname1;
                    lt.InvNo = invno1;
                    lt.DateBill = datebill3;
                    lt.Total = total1;
                    lt.DueDateBill = duedatebill3;
                    lt.Name = name1;
                    lt.Code = code1;
                    lt.TypeID = typeid1;
                    lt.SubmitDate = DateTime.Now;

                    db.LocationTest.Add(lt);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(ex.Message.ToString());
            }


            return RedirectToAction("Index");
        }

        // GET: GeoCoding
        public ActionResult ViewTest()
        {
            return View();
        }

        public ActionResult Bill1()
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            mWGE mwge = new mWGE();
            Session["TypeID"] = "1";
            return View(mwge);
        }

        [HttpPost]
        public ActionResult Bill1(int invoiceno, mWGE mData)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using (var db = new DBS_WGE_Entities())
                {
                    var oData = (from a in db.OINV
                                 join b in db.OSLP on a.SlpCode equals b.SlpCode into bb
                                 from b in bb.DefaultIfEmpty()
                                 join c in db.OCRD on a.CardCode equals c.CardCode into cc
                                 from c in cc.DefaultIfEmpty()
                                 where 1 == 1
                                 select new OINV2
                                 {
                                     CardCode = a.CardCode,
                                     CardName = a.CardName,
                                     InvNo = a.DocNum,
                                     DateBill = a.DocDate,
                                     DueDateBill = a.DocDueDate,
                                     Total = a.DocTotalSy - a.PaidSys
                                 });

                    var oData2 = (from e in db.ORIN
                                  join f in db.OSLP on e.SlpCode equals f.SlpCode into ff
                                  from f in ff.DefaultIfEmpty()
                                  join g in db.OCRD on e.CardCode equals g.CardCode into gg
                                  from g in gg.DefaultIfEmpty()
                                  where 1 == 1
                                  select new OINV2
                                  {
                                      CardCode = e.CardCode,
                                      CardName = e.CardName,
                                      InvNo = e.DocNum,
                                      DateBill = e.DocDate,
                                      DueDateBill = e.DocDueDate,
                                      Total = e.DocTotalSy - e.PaidSys
                                  });

                    var allData = oData.Concat(oData2);

                    if (!string.IsNullOrEmpty(mData.DocNum.ToString()))
                    {
                        allData = allData.Where(a => a.InvNo == invoiceno);
                    }
                    if (allData != null)
                    {
                        mData.ListOINV = allData.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(ex.Message.ToString());
            }

            return View(mData);
        }

        public ActionResult Bill2()
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            mWGE mwge = new mWGE();
            Session["TypeID"] = "2";
            return View(mwge);
        }

        [HttpPost]
        public ActionResult Bill2(int invoiceno, mWGE mData)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using (var db = new DBS_WGE_Entities())
                {
                    var oData = (from a in db.OINV
                                 join b in db.OSLP on a.SlpCode equals b.SlpCode into bb
                                 from b in bb.DefaultIfEmpty()
                                 join c in db.OCRD on a.CardCode equals c.CardCode into cc
                                 from c in cc.DefaultIfEmpty()
                                 where 1 == 1
                                 select new OINV2
                                 {
                                     CardCode = a.CardCode,
                                     CardName = a.CardName,
                                     InvNo = a.DocNum,
                                     DateBill = a.DocDate,
                                     DueDateBill = a.DocDueDate,
                                     Total = a.DocTotalSy - a.PaidSys
                                 });

                    var oData2 = (from e in db.ORIN
                                  join f in db.OSLP on e.SlpCode equals f.SlpCode into ff
                                  from f in ff.DefaultIfEmpty()
                                  join g in db.OCRD on e.CardCode equals g.CardCode into gg
                                  from g in gg.DefaultIfEmpty()
                                  where 1 == 1
                                  select new OINV2
                                  {
                                      CardCode = e.CardCode,
                                      CardName = e.CardName,
                                      InvNo = e.DocNum,
                                      DateBill = e.DocDate,
                                      DueDateBill = e.DocDueDate,
                                      Total = e.DocTotalSy - e.PaidSys
                                  });

                    var allData = oData.Concat(oData2);

                    if (!string.IsNullOrEmpty(mData.DocNum.ToString()))
                    {
                        allData = allData.Where(a => a.InvNo == invoiceno);
                    }
                    if (allData != null)
                    {
                        mData.ListOINV = allData.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(ex.Message.ToString());
            }

            return View(mData);
        }

        public ActionResult Delivery()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public ActionResult getDelivery(string location, string Remark, string Bank)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                string[] location1 = location.Split(',');
                string[] latitude1 = location1[0].Split(':');
                string[] longitude1 = location1[1].Split(':');
                string name1 = location1[3];
                string code1 = location1[4];
                string remark1 = Remark;
                string typeid1 = location1[5];

                if (Bank != "-" && typeid1 == "1")
                {
                    remark1 = "ธนาคาร : " + Bank + " / " + remark1;
                }

                using (var db = new DBS_WGE_Entities())
                {

                    C_MCT_DOCUMENT dbs = new C_MCT_DOCUMENT();

                    dbs.Name = name1;
                    dbs.Code = code1;
                    dbs.Remark = remark1;
                    dbs.TypeID = typeid1;
                    dbs.Latitude = latitude1[1];
                    dbs.Longitude = longitude1[1];
                    dbs.SubmitDate = DateTime.Now;

                    db.C_MCT_DOCUMENT.Add(dbs);
                    db.SaveChanges();

                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Add", System.Reflection.MethodBase.GetCurrentMethod().Name, "success"));
                    TempData["msg"] = "<script>alert('Success');</script>";
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString()));
                TempData["msg"] = "<script>alert('Error');</script>";
                /*LineNotify("uNtLlL6x4ju1NDEyocwuUYjbQynXuxcVUGC7E1Sd8aC", "คุณ " + Session["Name"].ToString() + "บันทึกรับส่งเอกสารไม่สำเร็จ!!!!");*/
            }

            return View();
            //return RedirectToAction("Index");
        }
        public ActionResult Online()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public ActionResult getOnline(string location, string Remark, string ID)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                string[] location1 = location.Split(',');
                string[] latitude1 = location1[0].Split(':');
                string[] longitude1 = location1[1].Split(':');
                string name1 = location1[3];
                string code1 = location1[4];
                string remark1 = Remark;
                string typeid1 = "5";
                string choice = location1[5];
                string id = ID;
                float price;

                if(choice == "B")
                {
                    price = 54;
                }
                else if(choice == "2B")
                {
                    price = 81;
                }
                else if(choice == "D")
                {
                    price = 86;
                }
                else if(choice == "2D")
                {
                    price = 117;
                }
                else
                {
                    price = 0;
                }
                
                using (var db = new DBS_WGE_Entities())
                {

                    C_MCT_DOCUMENT dbs = new C_MCT_DOCUMENT();

                    dbs.Name = name1;
                    dbs.Code = code1;
                    dbs.Remark = remark1;
                    dbs.TypeID = typeid1;
                    dbs.Latitude = latitude1[1];
                    dbs.Longitude = longitude1[1];
                    dbs.SubmitDate = DateTime.Now;
                    dbs.size = choice;
                    dbs.order = ID;
                    dbs.price = Convert.ToDecimal(price);
                    dbs.daterecive = DateTime.Now.Date;
                    db.C_MCT_DOCUMENT.Add(dbs);
                    db.SaveChanges();

                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Add", System.Reflection.MethodBase.GetCurrentMethod().Name, "success"));
                    TempData["msg"] = "<script>alert('Success');</script>";
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString()));
                TempData["msg"] = "<script>alert('Error');</script>";
                /*LineNotify("uNtLlL6x4ju1NDEyocwuUYjbQynXuxcVUGC7E1Sd8aC", "คุณ " + Session["Name"].ToString() + "บันทึกรับส่งเอกสารไม่สำเร็จ!!!!");*/
            }

            return View();
            //return RedirectToAction("Index");
        }
        public ActionResult DeliveryFind()
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Position"].ToString() == "8")
            {
                mWGE mData = new mWGE();

                try
                {
                    using (var db = new DBS_WGE_Entities())
                    {
                        int empCode = Convert.ToInt32(Session["EMPNo"]);

                        var oData = (from a in db.C_MCT_DOCUMENT
                                     where (1 == 1)
                                     select new C_MCT_DOCUMENT2
                                     {
                                         ID = a.ID,
                                         Remark = a.Remark,
                                         TypeID = a.TypeID,
                                         Name = a.Name,
                                         SubmitDate = a.SubmitDate,
                                         price = a.price,
                                         size =a.size,
                                         order = a.order,
                                         daterecive = a.daterecive
                                     });

                        oData = oData.OrderByDescending(o => o.ID).Take(1000);

                        if (oData != null)
                        {
                            mData.ListDocument = oData.ToList();
                            log.WriteLog(string.Format("{0}({1}) : {2}", Session["Name"].ToString(), Session["Code"].ToString(), "Search Delivery"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                }

                return View(mData);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult Other()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            mWGE mwge = new mWGE();
            Session["TypeID"] = "3";
            return View(mwge);
        }

        public ActionResult OtherFind()
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Position"].ToString() == "8")
            {
                mWGE mData = new mWGE();

                try
                {
                    using (var db = new DBS_WGE_Entities())
                    {
                        int empCode = Convert.ToInt32(Session["EMPNo"]);

                        var oData = (from a in db.C_MCT_OTHER
                                     where (1 == 1)
                                     select new C_MCT_OTHER2
                                     {
                                         ID = a.ID,
                                         CardCode = a.CardCode,
                                         CardName = a.CardName,
                                         InvNo = a.InvNo,
                                         Remark = a.Remark,
                                         DateBill = a.DateBill,
                                         TypeID = a.TypeID,
                                         Total = a.Total,
                                         Name = a.Name,
                                         SubmitDate = a.SubmitDate
                                     });

                        oData = oData.OrderByDescending(o => o.SubmitDate).Take(1000);

                        if (oData != null)
                        {
                            mData.ListOther = oData.ToList();
                            log.WriteLog(string.Format("{0}({1}) : {2}", Session["Name"].ToString(), Session["Code"].ToString(), "Search Other"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                }

                return View(mData);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult Other(string cardcodeno, string cardname, mWGE mData)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                if (cardname != "")
                {
                    cardcodeno = "";
                }

                using (var db = new DBS_WGE_Entities())
                {
                    var oData = (from a in db.OINV
                                 join b in db.OSLP on a.SlpCode equals b.SlpCode into bb
                                 from b in bb.DefaultIfEmpty()
                                 join c in db.OCRD on a.CardCode equals c.CardCode into cc
                                 from c in cc.DefaultIfEmpty()
                                 join d in db.C_MCT_OTHER on a.DocNum equals d.InvNo into dd
                                 from d in dd.DefaultIfEmpty()
                                 where (a.DocDate >= date2020) && (d.Status != "1")
                                 select new OINV2
                                 {
                                     CardCode = a.CardCode,
                                     CardName = a.CardName,
                                     Address = a.Address,
                                     DocNum = a.DocNum,
                                     DocDate = a.DocDate
                                 });

                    var oData2 = (from e in db.ORIN
                                  join f in db.OSLP on e.SlpCode equals f.SlpCode into ff
                                  from f in ff.DefaultIfEmpty()
                                  join g in db.OCRD on e.CardCode equals g.CardCode into gg
                                  from g in gg.DefaultIfEmpty()
                                  join h in db.C_MCT_OTHER on e.DocNum equals h.InvNo into hh
                                  from h in hh.DefaultIfEmpty()
                                  where e.DocDate >= date2020 && (h.Status != "1")
                                  select new OINV2
                                  {
                                      CardCode = e.CardCode,
                                      CardName = e.CardName,
                                      Address = e.Address,
                                      DocNum = e.DocNum,
                                      DocDate = e.DocDate
                                  });

                    var allData = oData.Concat(oData2);

                    if (!string.IsNullOrEmpty(cardcodeno))
                    {
                        allData = allData.Where(a => a.CardCode == cardcodeno).OrderByDescending(x => x.DocNum);
                    }
                    else if (!string.IsNullOrEmpty(cardname))
                    {
                        allData = allData.Where(a => a.CardName.Contains(cardname)).OrderByDescending(x => x.DocNum);
                    }

                    if (allData != null)
                    {
                        mData.ListOINV = allData.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
            }

            return View(mData);
        }

        public ActionResult Other2(int invno, mWGE mData)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            //Session["InvNo"] = invno.ToString();

            try
            {
                using (var db = new DBS_WGE_Entities())
                {
                    var oData = (from a in db.OINV
                                 join b in db.OSLP on a.SlpCode equals b.SlpCode into bb
                                 from b in bb.DefaultIfEmpty()
                                 join c in db.OCRD on a.CardCode equals c.CardCode into cc
                                 from c in cc.DefaultIfEmpty()
                                 where 1 == 1
                                 select new OINV2
                                 {
                                     CardCode = a.CardCode,
                                     CardName = a.CardName,
                                     Address = a.Address,
                                     DocNum = a.DocNum,
                                     DocDate = a.DocDate,
                                     DocDueDate = a.DocDueDate,
                                     Total = a.DocTotal,
                                     FatherCard = a.FatherCard
                                 });

                    var oData2 = (from e in db.ORIN
                                  join f in db.OSLP on e.SlpCode equals f.SlpCode into ff
                                  from f in ff.DefaultIfEmpty()
                                  join g in db.OCRD on e.CardCode equals g.CardCode into gg
                                  from g in gg.DefaultIfEmpty()
                                  where 1 == 1
                                  select new OINV2
                                  {
                                      CardCode = e.CardCode,
                                      CardName = e.CardName,
                                      Address = e.Address,
                                      DocNum = e.DocNum,
                                      DocDate = e.DocDate,
                                      DocDueDate = e.DocDueDate,
                                      Total = e.DocTotal,
                                      FatherCard = e.FatherCard
                                  });

                    var allData = oData.Concat(oData2);

                    allData = allData.Where(a => a.DocNum == invno);

                    //if (!string.IsNullOrEmpty(mData.CardCode))
                    //{

                    //}
                    if (allData != null)
                    {
                        mData.ListOINV = allData.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
            }

            return View(mData);
        }

        [HttpPost]
        public ActionResult getOther2(string location, DateTime DocDate1, DateTime DocDueDate1, string Remark)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                string[] location1 = location.Split(',');
                string[] latitude1 = location1[0].Split(':');
                string[] longitude1 = location1[1].Split(':');
                string name1 = location1[3];
                string code1 = location1[4];
                int invno1 = Convert.ToInt32(location1[5]);
                string remark1 = Remark;
                int typeid1 = Convert.ToInt32(location1[6]);
                string cardcode1 = location1[7];
                string cardname1 = location1[8];
                string address1 = location1[9];
                double total1 = Convert.ToDouble(location1[10]);

                using (var db = new DBS_WGE_Entities())
                {
                    C_MCT_OTHER dbs = new C_MCT_OTHER();

                    dbs.Name = name1;
                    dbs.Code = code1;
                    dbs.Remark = remark1;
                    dbs.InvNo = invno1;
                    dbs.CardCode = cardcode1;
                    dbs.CardName = cardname1;
                    dbs.Status = "1";

                    if (DocDate1 != null)
                    {
                        dbs.DateBill = DocDate1;
                    }
                    if (DocDueDate1 != null)
                    {
                        dbs.DueDateBill = DocDueDate1;
                    }
                    dbs.Total = Convert.ToDecimal(total1);
                    dbs.TypeID = typeid1.ToString();
                    dbs.Latitude = latitude1[1];
                    dbs.Longitude = longitude1[1];
                    dbs.SubmitDate = DateTime.Now;

                    db.C_MCT_OTHER.Add(dbs);
                    db.SaveChanges();

                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "Add", System.Reflection.MethodBase.GetCurrentMethod().Name, invno1, "success"));
                    TempData["msg"] = "<script>alert('Success');</script>";
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString()));
                TempData["msg"] = "<script>alert('Error');</script>";
            }

            return RedirectToAction("Index");
            //return View();
        }

        public ActionResult CN()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            mWGE mwge = new mWGE();
            Session["TypeID"] = "4";
            return View(mwge);
        }

        [HttpPost]
        public ActionResult CN(string cardcodeno, string cardname, string cpcode, string cpname, string optionsRadios1, mWGE mData)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                if (optionsRadios1 == "c")
                {
                    cpcode = "";
                    cpname = "";

                    if (cardname != "")
                    {
                        cardcodeno = "";
                    }
                }
                else
                {
                    cardcodeno = "";
                    cardname = "";

                    if (cpname != "")
                    {
                        cpcode = "";
                    }
                }

                using (var db = new DBS_WGE_Entities())
                {
                    OCRD nData = db.OCRD.Where(o => o.CardName.Contains(cpname) && o.CardCode.StartsWith("c") && o.FatherCard != null).FirstOrDefault();
                    string cpcode1 = nData.FatherCard;

                    var oData = (from e in db.ORIN
                                 join f in db.OSLP on e.SlpCode equals f.SlpCode into ff
                                 from f in ff.DefaultIfEmpty()
                                 join g in db.OCRD on e.CardCode equals g.CardCode into gg
                                 from g in gg.DefaultIfEmpty()
                                 join h in db.C_MCT_CREDITNOTE on e.DocNum equals h.CreditNo into hh
                                 from h in hh.DefaultIfEmpty()
                                 where (e.DocDate >= date2020) && (h.Status != "1")
                                 select new OINV2
                                 {
                                     CardCode = e.CardCode,
                                     CardName = e.CardName,
                                     Address = e.Address,
                                     DocDate = e.DocDate,
                                     DocNum = e.DocNum,
                                     DocTotal = e.DocTotal,
                                     FatherCard = e.FatherCard
                                 });

                    if (!string.IsNullOrEmpty(cardcodeno))
                    {
                        oData = oData.Where(a => a.CardCode == cardcodeno).OrderByDescending(x => x.DocNum);
                    }
                    else if (!string.IsNullOrEmpty(cardname))
                    {
                        oData = oData.Where(a => a.CardName.Contains(cardname)).OrderByDescending(x => x.DocNum);
                    }
                    else if (!string.IsNullOrEmpty(cpcode))
                    {
                        oData = oData.Where(a => a.FatherCard == cpcode).Distinct().OrderByDescending(x => x.DocNum);
                    }
                    else if (!string.IsNullOrEmpty(cpname))
                    {
                        oData = oData.Where(a => a.FatherCard == cpcode1).Distinct().OrderByDescending(x => x.DocNum);
                    }

                    if (oData != null)
                    {
                        mData.ListOINV = oData.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
            }

            return View(mData);
        }

        public ActionResult CNInfo(int invno, mWGE mData)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using (var db = new DBS_WGE_Entities())
                {
                    var oData = (from a in db.ORIN
                                 join b in db.OSLP on a.SlpCode equals b.SlpCode into bb
                                 from b in bb.DefaultIfEmpty()
                                 join c in db.OCRD on a.CardCode equals c.CardCode into cc
                                 from c in cc.DefaultIfEmpty()
                                 where 1 == 1
                                 select new OINV2
                                 {
                                     CardCode = a.CardCode,
                                     CardName = a.CardName,
                                     Address = a.Address,
                                     DocNum = a.DocNum,
                                     DocDate = a.DocDate,
                                     DocDueDate = a.DocDueDate,
                                     Total = a.DocTotal,
                                     FatherCard = a.FatherCard
                                 });


                    oData = oData.Where(a => a.DocNum == invno);

                    if (oData != null)
                    {
                        mData.ListOINV = oData.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
            }

            return View(mData);
        }

        public ActionResult CNFind()
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Position"].ToString() == "8")
            {
                mWGE mData = new mWGE();

                try
                {
                    using (var db = new DBS_WGE_Entities())
                    {
                        int empCode = Convert.ToInt32(Session["EMPNo"]);

                        var oData = (from a in db.C_MCT_CREDITNOTE
                                     where (1 == 1)
                                     select new C_MCT_CREDITNOTE2
                                     {
                                         ID = a.ID,
                                         CardCode = a.CardCode,
                                         CardName = a.CardName,
                                         CreditNo = a.CreditNo,
                                         DateNote = a.DateNote,
                                         Total = a.Total,
                                         Name = a.Name,
                                         SubmitDate = a.SubmitDate
                                     });

                        oData = oData.OrderByDescending(o => o.ID).Take(1000);

                        if (oData != null)
                        {
                            mData.ListCreditNote = oData.ToList();
                            log.WriteLog(string.Format("{0}({1}) : {2}", Session["Name"].ToString(), Session["Code"].ToString(), "Search Other"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                }

                return View(mData);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult CN1(string location)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            mWGE mData = new mWGE();

            try
            {
                string[] location1 = location.Split(',');
                string[] latitude1 = location1[0].Split(':');
                string[] longitude1 = location1[1].Split(':');
                string name1 = location1[3];
                string code1 = location1[4];
                int countlocation = location1.Count();
                int countinvno = countlocation - 5;

                List<int> invno1 = new List<int>();

                for (int i = 5; i < 5 + countinvno; i++)
                {
                    invno1.Add(Convert.ToInt32(location1[i]));
                }

                //TempData["arr"] = invno1;

                //mData.InvnoBilling = invno1;

                Session.Remove("arr");
                Session["arr"] = invno1;
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
            }

            return new EmptyResult();
            //return View();
            //return RedirectToAction("Billing2","GeoCoding");
        }
        public ActionResult CN2(mWGE mData)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var list = (List<int>)Session["arr"];
            //List<int> list = (List<int>)TempData["arr"];

            try
            {
                using (var db = new DBS_WGE_Entities())
                {
                    var oData = (from e in db.ORIN
                                 join f in db.OSLP on e.SlpCode equals f.SlpCode into ff
                                 from f in ff.DefaultIfEmpty()
                                 join g in db.OCRD on e.CardCode equals g.CardCode into gg
                                 from g in gg.DefaultIfEmpty()
                                 where (1 == 1) && list.Contains(e.DocNum ?? default(int))
                                 select new OINV2
                                 {
                                     CardCode = e.CardCode,
                                     CardName = e.CardName,
                                     Address = e.Address,
                                     DocDate = e.DocDate,
                                     DocNum = e.DocNum,
                                     DocTotal = e.DocTotal,
                                     FatherCard = e.FatherCard
                                 });


                    oData = oData.OrderByDescending(x => x.DocNum);

                    if (oData != null)
                    {
                        mData.ListOINV = oData.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
            }

            return View(mData);
        }


        [HttpPost]
        public ActionResult getCN(string location,DateTime? Dateback,string Remark)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                string[] location1 = location.Split(',');
                string[] latitude1 = location1[0].Split(':');
                string[] longitude1 = location1[1].Split(':');
                string name1 = location1[3];
                string code1 = location1[4];
                int countlocation = location1.Count();
                int countinvno = countlocation - 5;
                string remark1 = Remark;

                List<int> invno1 = new List<int>();

                for (int i = 5; i < 5 + countinvno; i++)
                {
                    invno1.Add(Convert.ToInt32(location1[i]));
                }

                var str = String.Join(",", invno1);
                //int countinvno2 = invno1.Count();
                mWGE mwge = new mWGE();

                using (var db = new DBS_WGE_Entities())
                {
                    var oData = (from e in db.ORIN
                                 join f in db.OSLP on e.SlpCode equals f.SlpCode into ff
                                 from f in ff.DefaultIfEmpty()
                                 join g in db.OCRD on e.CardCode equals g.CardCode into gg
                                 from g in gg.DefaultIfEmpty()
                                 where (1 == 1) && (invno1.Contains(e.DocNum ?? default(int)))
                                 select new ORIN2
                                 {
                                     CardCode = e.CardCode,
                                     CardName = e.CardName,
                                     DocDate = e.DocDate,
                                     DocDueDate = e.DocDueDate,
                                     Address = e.Address,
                                     DocNum = e.DocNum,
                                     DocTotal = e.DocTotal
                                 });

                    oData = oData.OrderByDescending(x => x.DocNum);

                    if (oData != null)
                    {
                        mwge.ListORIN = oData.ToList();
                    }

                    //เช็คบิลซ้ำ---------------------------------------------------------

                    var oDataC = (from a in db.C_MCT_CREDITNOTE
                                  where invno1.Contains(a.CreditNo ?? default(int))
                                  select a).ToList();

                    if (oDataC.Count != 0)
                    {
                        TempData["msg"] = "<script>alert('ไม่สามารถบันทึกซ้ำได้');</script>";
                        return RedirectToAction("Home", "Index");
                    }

                    //---------------------------------------------------------------

                    C_MCT_CREDITNOTE dbs = new C_MCT_CREDITNOTE();

                    foreach (var oItem in mwge.ListORIN)
                    {
                        dbs.CardCode = oItem.CardCode;
                        dbs.CardName = oItem.CardName;
                        dbs.CreditNo = oItem.DocNum;
                        dbs.DateNote = oItem.DocDate;
                        dbs.DueDateNote = oItem.DocDueDate;
                        dbs.Total = oItem.DocTotal;
                        dbs.Code = code1;
                        dbs.Name = name1;
                        dbs.Latitude = latitude1[1];
                        dbs.Longitude = longitude1[1];
                        dbs.SubmitDate = DateTime.Now;
                        dbs.Status = "1";
                        dbs.Remark = remark1;

                        if(Dateback != null)
                        {
                            dbs.ReceiveDate = Dateback;
                        }

                        db.C_MCT_CREDITNOTE.Add(dbs);
                        db.SaveChanges();
                    }

                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "Add", System.Reflection.MethodBase.GetCurrentMethod().Name, str, "success"));
                    TempData["msg"] = "<script>alert('Success');</script>";
                    /*string MessageLine = "";
                    for (var i = 0; i < invno1.Count; i++)
                    {
                        MessageLine += "เลขที่บิล :";
                        MessageLine += invno1[i].ToString();
                        MessageLine += "\n";
                    }
                    string DateRec = dbs.ReceiveDate.Value.ToString("dd/MM/yyyy");
                    LineNotify("AOc565wkKRQomrhJGrgk9gZGEqhEjCLQsZcEKPAGt9Q", "คุณ " + Session["Name"].ToString() + "\nได้ทำการวางCN\n" + "รหัสลูกค้า :" + dbs.CardCode + "\n" + MessageLine + "\nวันที่ " + DateTime.Now + "\nสำเร็จ(!!Success)" + "\n\n "  + "วันที่คืน : " +DateRec);*/
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString()));
                TempData["msg"] = "<script>alert('Error');</script>";
                /*LineNotify("AOc565wkKRQomrhJGrgk9gZGEqhEjCLQsZcEKPAGt9Q", "คุณ " + Session["Name"].ToString() + "บันทึกCNไม่สำเร็จ!!!!");*/
            }

            return View();
        }


        public ActionResult Billing()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public ActionResult Billing(string cardcodeno, string cardname, string cpcode, string cpname, string optionsRadios1, mWGE mData)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                if (optionsRadios1 == "c")
                {
                    cpcode = "";
                    cpname = "";

                    if (cardname != "")
                    {
                        cardcodeno = "";
                    }
                }
                else
                {
                    cardcodeno = "";
                    cardname = "";

                    if (cpname != "")
                    {
                        cpcode = "";
                    }
                }

                using (var db = new DBS_WGE_Entities())
                {
                    OCRD nData = db.OCRD.Where(o => o.CardName.Contains(cpname) && o.CardCode.StartsWith("c") && o.FatherCard != null).FirstOrDefault();
                    string cpcode1 = nData.FatherCard;

                    var oData = (from a in db.OINV
                                 join b in db.OSLP on a.SlpCode equals b.SlpCode into bb
                                 from b in bb.DefaultIfEmpty()
                                 join c in db.OCRD on a.CardCode equals c.CardCode into cc
                                 from c in cc.DefaultIfEmpty()
                                 join d in db.C_MCT_BILLING on a.DocNum equals d.InvNo into dd
                                 from d in dd.DefaultIfEmpty()
                                 where (d.Status != "1") && (d.Status != "2") && (d.Active != "0") && (a.DocDate >= date2020)
                                 select new OINV2
                                 {
                                     CardCode = a.CardCode,
                                     CardName = a.CardName,
                                     Address = a.Address,
                                     DocNum = a.DocNum,
                                     DocDate = a.DocDate,
                                     DocDueDate = a.DocDueDate,
                                     Total = a.DocTotal,
                                     FatherCard = a.FatherCard
                                     //FatherName = c.CardName
                                 });

                    if (!string.IsNullOrEmpty(cardcodeno))
                    {
                        oData = oData.Where(a => a.CardCode == cardcodeno).Distinct().OrderByDescending(x => x.DocNum);
                    }
                    else if (!string.IsNullOrEmpty(cardname))
                    {
                        oData = oData.Where(a => a.CardName.Contains(cardname)).Distinct().OrderByDescending(x => x.DocNum);
                    }
                    else if (!string.IsNullOrEmpty(cpcode))
                    {
                        oData = oData.Where(a => a.FatherCard == cpcode).Distinct().OrderByDescending(x => x.DocNum);
                    }
                    else if (!string.IsNullOrEmpty(cpname))
                    {
                        oData = oData.Where(a => a.FatherCard == cpcode1).Distinct().OrderByDescending(x => x.DocNum);
                    }

                    if (oData != null)
                    {
                        mData.ListOINV = oData.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
            }

            return View(mData);
        }

        public ActionResult BillingInfo(int invno, mWGE mData)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using (var db = new DBS_WGE_Entities())
                {
                    var oData = (from a in db.OINV
                                 join b in db.OSLP on a.SlpCode equals b.SlpCode into bb
                                 from b in bb.DefaultIfEmpty()
                                 join c in db.OCRD on a.CardCode equals c.CardCode into cc
                                 from c in cc.DefaultIfEmpty()
                                 where 1 == 1
                                 select new OINV2
                                 {
                                     CardCode = a.CardCode,
                                     CardName = a.CardName,
                                     Address = a.Address,
                                     DocNum = a.DocNum,
                                     DocDate = a.DocDate,
                                     DocDueDate = a.DocDueDate,
                                     Total = a.DocTotal,
                                     FatherCard = a.FatherCard
                                 });


                    oData = oData.Where(a => a.DocNum == invno);

                    if (oData != null)
                    {
                        mData.ListOINV = oData.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
            }

            return View(mData);
        }

        public ActionResult BillingFind()
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Position"].ToString() == "8")
            {
                mWGE mData = new mWGE();

                try
                {
                    using (var db = new DBS_WGE_Entities())
                    {
                        int empCode = Convert.ToInt32(Session["EMPNo"]);

                        var oData = (from a in db.C_MCT_BILLING
                                     where (1 == 1)
                                     select new C_MCT_BILLING2
                                     {
                                         ID = a.ID,
                                         CardCode = a.CardCode,
                                         CardName = a.CardName,
                                         InvNo = a.InvNo,
                                         Remark = a.Remark,
                                         Remark2 = a.Remark2,
                                         DateBill = a.DateBill,
                                         TypeID = a.TypeID,
                                         Total = a.Total,
                                         Name = a.Name,
                                         SubmitDate = a.SubmitDate,
                                         CheckDueDate = a.CheckDueDate
                                     });

                        oData = oData.OrderByDescending(o => o.ID).Take(1000);

                        if (oData != null)
                        {
                            mData.ListBilling = oData.ToList();
                            log.WriteLog(string.Format("{0}({1}) : {2}", Session["Name"].ToString(), Session["Code"].ToString(), "Search Billing"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                }

                return View(mData);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult Billing1(string Location)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            mWGE mData = new mWGE();

            try
            {
                string[] location1 = Location.Split(',');
                string[] latitude1 = location1[0].Split(':');
                string[] longitude1 = location1[1].Split(':');
                string name1 = location1[3];
                string code1 = location1[4];
                int countlocation = location1.Count();
                int countinvno = countlocation - 5;

                List<int> invno1 = new List<int>();

                for (int i = 5; i < 5 + countinvno; i++)
                {
                    invno1.Add(Convert.ToInt32(location1[i]));
                }

                //TempData["arr"] = invno1;

                //mData.InvnoBilling = invno1;

                Session.Remove("arr");
                Session["arr"] = invno1;
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
            }

            return new EmptyResult();
            //return View();
            //return RedirectToAction("Billing2","GeoCoding");
        }

        public ActionResult Billing2(mWGE mData)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var list = (List<int>)Session["arr"];
            //List<int> list = (List<int>)TempData["arr"];

            try
            {
                using (var db = new DBS_WGE_Entities())
                {
                    var oData = (from a in db.OINV
                                 join b in db.OSLP on a.SlpCode equals b.SlpCode into bb
                                 from b in bb.DefaultIfEmpty()
                                 join c in db.OCRD on a.CardCode equals c.CardCode into cc
                                 from c in cc.DefaultIfEmpty()
                                 where (1 == 1) && (list.Contains(a.DocNum ?? default(int)))
                                 select new OINV2
                                 {
                                     CardCode = a.CardCode,
                                     CardName = a.CardName,
                                     Address = a.Address,
                                     DocNum = a.DocNum,
                                     DocDate = a.DocDate,
                                     DocDueDate = a.DocDueDate,
                                     Total = a.DocTotal
                                 });


                    oData = oData.OrderByDescending(x => x.DocNum);

                    if (oData != null)
                    {
                        mData.ListOINV = oData.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
            }

            return View(mData);
        }

        [HttpPost]
        public ActionResult getBilling(string location, DateTime? dateback, DateTime? ChequeDueDate, DateTime? ChequeDate, bool CheckBox, string Bank, string Remark, string TotalRev, DateTime? Receivedate, string Refno)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                string[] location1 = location.Split(',');
                string[] latitude1 = location1[0].Split(':');
                string[] longitude1 = location1[1].Split(':');
                string name1 = location1[3];
                string code1 = location1[4];
                string remark1 = Remark;
                int radio1 = Convert.ToInt32(location1[5]);
                string[] Bank1 = Bank.Split(',');
                string bank1 = Bank1[0];
                string branch1 = Bank1[1];
                string chequeno1 = Bank1[2];
                string totalrev1 = TotalRev;

                if (totalrev1 == "")
                {
                    totalrev1 = "0";
                }

                int countlocation = location1.Count();
                int countinvno = countlocation - 6;

                List<int> invno1 = new List<int>();

                for (int i = 6; i < 6 + countinvno; i++)
                {
                    invno1.Add(Convert.ToInt32(location1[i]));
                }

                var str = String.Join(",", invno1);
                mWGE mwge = new mWGE();

                using (var db = new DBS_WGE_Entities())
                {
                    var oData = (from a in db.OINV
                                 join b in db.OSLP on a.SlpCode equals b.SlpCode into bb
                                 from b in bb.DefaultIfEmpty()
                                 join c in db.OCRD on a.CardCode equals c.CardCode into cc
                                 from c in cc.DefaultIfEmpty()
                                 where (1 == 1) && (invno1.Contains(a.DocNum ?? default(int)))
                                 select new OINV2
                                 {
                                     CardCode = a.CardCode,
                                     CardName = a.CardName,
                                     Address = a.Address,
                                     DocNum = a.DocNum,
                                     DocDate = a.DocDate,
                                     DocDueDate = a.DocDueDate,
                                     Total = a.DocTotal
                                 });

                    oData = oData.OrderByDescending(x => x.DocNum);

                    if (oData != null)
                    {
                        mwge.ListOINV = oData.ToList();
                    }

                    //เช็คบิลซ้ำ---------------------------------------------------------

                    var oDataC = (from a in db.C_MCT_BILLING
                                  where invno1.Contains(a.InvNo ?? default(int))
                                  select a).ToList();

                    if (oDataC.Count != 0)
                    {
                        var oDataD = oDataC.Where(z => z.Status == "0").ToList();
                        bool x = false;

                        if (oDataD.Count != 0)
                        {
                            x = true;
                        }

                        if (x == false)
                        {
                            TempData["msg"] = "<script>alert('ไม่สามารถบันทึกซ้ำได้');</script>";
                            return RedirectToAction("Home", "Index");
                        }
                    }

                    //---------------------------------------------------------------

                    C_MCT_BILLING dbs = new C_MCT_BILLING();

                    foreach (var oItem in mwge.ListOINV)
                    {
                        dbs.InvNo = oItem.DocNum;
                        dbs.CardCode = oItem.CardCode;
                        dbs.CardName = oItem.CardName;
                        dbs.DateBill = oItem.DocDate;
                        dbs.DueDateBill = oItem.DocDueDate;
                        dbs.Total = oItem.Total;
                        dbs.TypeID = radio1.ToString();

                        if (radio1 == 0 || radio1 == 6)
                        {
                            dbs.Status = "1";

                            if (radio1 == 0) //ครบสมบูรณ์
                            {
                                if (ChequeDueDate != null)
                                {
                                    dbs.CheckDueDate = ChequeDueDate; //วันที่เก็บเช็ค
                                }
                            }
                            else if (radio1 == 6)
                            {
                                if (CheckBox == true)
                                {
                                    dbs.Remark2 = " ( โทรเช็คเอง ) ";
                                }
                            }
                        }
                        else if (radio1 == 5)
                        {
                            dbs.Status = "2";

                            if (CheckBox == true)
                            {
                                dbs.Remark2 = " ( โทรเช็คเอง ) ";
                            }
                        }
                        else
                        {
                            dbs.Status = "0";

                            if (radio1 == 1) //ส่งสินค้าไม่ครบ
                            {
                                if (dateback != null)
                                {
                                    dbs.DateRev = dateback;
                                }
                            }
                        }

                        dbs.Remark = remark1;
                        dbs.Code = code1;
                        dbs.Name = name1;
                        dbs.Latitude = latitude1[1];
                        dbs.Longitude = longitude1[1];
                        dbs.SubmitDate = DateTime.Now;
                        dbs.Refno = Refno;
                        if (Receivedate != null)
                        {
                            dbs.Receivedate = Receivedate;
                        }
                        else
                        {
                            dbs.Receivedate = null;
                        }
                        
                        db.C_MCT_BILLING.Add(dbs);
                        db.SaveChanges();

                        if (radio1 == 0 || radio1 == 5 || radio1 == 6) //อัตเดตทุกตัวที่ยังไม่ผ่าน
                        {
                            var oData2 = (from a in db.C_MCT_BILLING
                                          where (a.Status == "0") && (invno1.Contains(a.InvNo ?? default(int)))
                                          select a).ToList();

                            foreach (var oItem2 in oData2)
                            {
                                oItem2.Active = "0";
                            }

                            db.SaveChanges();
                        }

                        C_MCT_RECEIVE dbs2 = new C_MCT_RECEIVE(); //วางบิลพร้อมเก็บเงิน

                        if (radio1 == 6)
                        {
                            dbs2.InvNo = oItem.DocNum;
                            dbs2.CardCode = oItem.CardCode;
                            dbs2.CardName = oItem.CardName;
                            dbs2.DateBill = oItem.DocDate;
                            dbs2.DueDateBill = oItem.DocDueDate;
                            dbs2.TypeID = "3";
                            dbs2.Total = oItem.Total;
                            dbs2.Remark = remark1;
                            dbs2.Code = code1;
                            dbs2.Name = name1;
                            dbs2.Latitude = latitude1[1];
                            dbs2.Longitude = longitude1[1];
                            dbs2.SubmitDate = DateTime.Now;
                            dbs2.Bank = bank1;
                            dbs2.Branch = branch1;
                            dbs2.ChequeNo = chequeno1;
                            dbs2.TotalRev = Convert.ToDecimal(totalrev1);
                            dbs2.Status = "1";
                            dbs2.Refno = Refno;

                            if (ChequeDate != null)
                            {
                                dbs2.ChequeDate = ChequeDate;
                            }
                            if (Receivedate != null)
                            {
                                dbs2.Receivedate = Receivedate;
                            }
                            else
                            {
                                dbs2.Receivedate = null;
                            }


                            
                            db.C_MCT_RECEIVE.Add(dbs2);
                            db.SaveChanges();

                            //อัพเดตสถานะเพิ่มเติมเมื่อเก็บเงินเรียบร้อยแล้ว
                            var oData2 = (from a in db.C_MCT_BILLING
                                          where (a.Status == "1") && (invno1.Contains(a.InvNo ?? default(int)))
                                          select a).ToList();

                            foreach (var oItem2 in oData2)
                            {
                                if (oItem2.InvNo == oItem.DocNum)
                                {
                                    oItem2.Status = "2";
                                }
                            }
                            
                            db.SaveChanges();
                        }
                    }
                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "Add", System.Reflection.MethodBase.GetCurrentMethod().Name, str, "success"));
                    TempData["msg"] = "<script>alert('Success');</script>";
                    /*string MessageLine = "";
                    for(var i = 0; i < invno1.Count; i++)
                    {
                        MessageLine += "เลขที่บิล :";
                        MessageLine += invno1[i].ToString();
                        MessageLine += "\n";
                    }
                    string DateSuccess = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                    LineNotify("uNtLlL6x4ju1NDEyocwuUYjbQynXuxcVUGC7E1Sd8aC",//line Token 
                        "คุณ " + Session["Name"].ToString() + 
                        "\nได้ทำการวางบิล"+ 
                        "\n\nรหัสลูกค้า :"+ dbs.CardCode+
                        "\n\n" + MessageLine + 
                        "\nวันที่ " + DateSuccess + 
                        "\n\nสำเร็จ(!!Success)"+
                        "\n\nเลขใบวางบิล : "+Refno+
                        "\nยอดรวม"+TotalRev+
                        "\nวันที่คืน : "+dbs.Receivedate.Value.ToString("dd/MM/yyyy")+
                        "\nหมายเหตุ :"+dbs.Remark);*/
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString()));
                TempData["msg"] = "<script>alert('Error');</script>";
                /*LineNotify("uNtLlL6x4ju1NDEyocwuUYjbQynXuxcVUGC7E1Sd8aC","คุณ "+Session["Name"].ToString()+ "บันทึกใบวางบิลไม่สำเร็จ!!!!");*/
            }

            return View();
        }

        public ActionResult Receive()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public ActionResult Receive(string cardcodeno, string cardname, string cpcode, string cpname, string optionsRadios1, mWGE mData)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                if (optionsRadios1 == "c")
                {
                    cpcode = "";
                    cpname = "";

                    if (cardname != "")
                    {
                        cardcodeno = "";
                    }
                }
                else
                {
                    cardcodeno = "";
                    cardname = "";

                    if (cpname != "")
                    {
                        cpcode = "";
                    }
                }

                using (var db = new DBS_WGE_Entities())
                {
                    OCRD nData = db.OCRD.Where(o => o.CardName.Contains(cpname) && o.CardCode.StartsWith("c") && o.FatherCard != null).FirstOrDefault();
                    string cpcode1 = nData.FatherCard;

                    var oData = (from a in db.OINV
                                 join b in db.OSLP on a.SlpCode equals b.SlpCode into bb
                                 from b in bb.DefaultIfEmpty()
                                 join c in db.OCRD on a.CardCode equals c.CardCode into cc
                                 from c in cc.DefaultIfEmpty()
                                 join d in db.C_MCT_BILLING on a.DocNum equals d.InvNo into dd
                                 from d in dd.DefaultIfEmpty()
                                 where (1 == 1) && (d.Status == "1") && (a.DocDate >= date2020)
                                 select new OINV2
                                 {
                                     CardCode = d.CardCode,
                                     CardName = d.CardName,
                                     Address = a.Address,
                                     DocNum = d.InvNo,
                                     DocDate = d.DateBill,
                                     DocDueDate = d.DueDateBill,
                                     Total = d.Total,
                                     ID = d.ID,
                                     FatherCard = c.FatherCard,
                                     Refno= d.Refno
                                 });

                    if (!string.IsNullOrEmpty(cardcodeno))
                    {
                        oData = oData.Where(d => d.CardCode == cardcodeno).OrderByDescending(x => x.ID);
                    }
                    else if (!string.IsNullOrEmpty(cardname))
                    {
                        oData = oData.Where(d => d.CardName.Contains(cardname)).OrderByDescending(x => x.ID);
                    }
                    else if (!string.IsNullOrEmpty(cpcode))
                    {
                        oData = oData.Where(a => a.FatherCard == cpcode).OrderByDescending(x => x.ID);
                    }
                    else if (!string.IsNullOrEmpty(cpname))
                    {
                        oData = oData.Where(a => a.FatherCard == cpcode1).OrderByDescending(x => x.ID);
                    }

                    if (oData != null)
                    {
                        mData.ListOINV = oData.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
            }

            return View(mData);
        }

        public ActionResult ReceiveInfo(int invno, mWGE mData)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using (var db = new DBS_WGE_Entities())
                {
                    var oData = (from a in db.OINV
                                 join b in db.OSLP on a.SlpCode equals b.SlpCode into bb
                                 from b in bb.DefaultIfEmpty()
                                 join c in db.OCRD on a.CardCode equals c.CardCode into cc
                                 from c in cc.DefaultIfEmpty()
                                 join d in db.C_MCT_BILLING on a.DocNum equals d.InvNo into dd
                                 from d in dd.DefaultIfEmpty()
                                 where (1 == 1)
                                 select new OINV2
                                 {
                                     CardCode = d.CardCode,
                                     CardName = d.CardName,
                                     Address = a.Address,
                                     DocNum = d.InvNo,
                                     DocDate = d.DateBill,
                                     DocDueDate = d.DueDateBill,
                                     Total = d.Total,
                                     FatherCard = a.FatherCard
                                 });

                    oData = oData.Where(a => a.DocNum == invno);

                    if (oData != null)
                    {
                        mData.ListOINV = oData.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
            }

            return View(mData);
        }

        public ActionResult ReceiveFind()
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Position"].ToString() == "8")
            {
                mWGE mData = new mWGE();

                try
                {
                    using (var db = new DBS_WGE_Entities())
                    {
                        int empCode = Convert.ToInt32(Session["EMPNo"]);

                        var oData = (from a in db.C_MCT_RECEIVE
                                     where (1 == 1)
                                     select new C_MCT_RECEIVE2
                                     {
                                         ID = a.ID,
                                         CardCode = a.CardCode,
                                         CardName = a.CardName,
                                         InvNo = a.InvNo,
                                         Remark = a.Remark,
                                         DateBill = a.DateBill,
                                         Detail = a.Bank + "/" + a.Branch + "/" + a.ChequeNo + "/" + a.ChequeDate,
                                         TypeID = a.TypeID,
                                         Total = a.Total,
                                         Name = a.Name,
                                         SubmitDate = a.SubmitDate
                                     });

                        oData = oData.OrderByDescending(o => o.ID).Take(1000);

                        if (oData != null)
                        {
                            mData.ListReceive = oData.ToList();
                            log.WriteLog(string.Format("{0}({1}) : {2}", Session["Name"].ToString(), Session["Code"].ToString(), "Search Receive"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                }

                return View(mData);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult Receive2(mWGE mData)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var list = (List<int>)Session["arr2"];

                using (var db = new DBS_WGE_Entities())
                {
                    var oData = (from a in db.OINV
                                 join b in db.OSLP on a.SlpCode equals b.SlpCode into bb
                                 from b in bb.DefaultIfEmpty()
                                 join c in db.OCRD on a.CardCode equals c.CardCode into cc
                                 from c in cc.DefaultIfEmpty()
                                 join d in db.C_MCT_BILLING on a.DocNum equals d.InvNo into dd
                                 from d in dd.DefaultIfEmpty()
                                 where (1 == 1) && (d.Status == "1") && (list.Contains(d.InvNo ?? default(int)))
                                 select new OINV2
                                 {
                                     CardCode = d.CardCode,
                                     CardName = d.CardName,
                                     Address = a.Address,
                                     DocNum = d.InvNo,
                                     DocDate = d.DateBill,
                                     DocDueDate = d.DueDateBill,
                                     Total = d.Total,
                                     Refno = d.Refno
                                 });

                    oData = oData.OrderByDescending(x => x.DocNum);

                    if (oData != null)
                    {
                        mData.ListOINV = oData.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
            }

            return View(mData);
        }

        [HttpPost]
        public ActionResult Receive1(string location)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                string[] location1 = location.Split(',');
                string[] latitude1 = location1[0].Split(':');
                string[] longitude1 = location1[1].Split(':');
                string name1 = location1[3];
                string code1 = location1[4];
                int countlocation = location1.Count();
                int countinvno = countlocation - 5;

                List<int> invno1 = new List<int>();

                for (int i = 5; i < 5 + countinvno; i++)
                {
                    invno1.Add(Convert.ToInt32(location1[i]));
                }

                Session.Remove("arr2");
                Session["arr2"] = invno1;
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
            }

            return new EmptyResult();
        }

        [HttpPost]
        public ActionResult getReceive(string location, DateTime? dateback, DateTime? ChequeDate, string Bank, string Remark, string TotalRev, DateTime? Receivedate)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                string[] location1 = location.Split(',');
                string[] latitude1 = location1[0].Split(':');
                string[] longitude1 = location1[1].Split(':');
                string name1 = location1[3];
                string code1 = location1[4];
                string remark1 = Remark;
                int radio1 = Convert.ToInt32(location1[5]);
                //string bank1 = location1[7];
                //string branch1 = location1[8];
                //string chequeno1 = location1[9];
                //string totalrev1 = location1[10];

                string[] Bank1 = Bank.Split(',');
                string bank1 = Bank1[0];
                string branch1 = Bank1[1];
                string chequeno1 = Bank1[2];
                string totalrev1 = TotalRev;

                if (totalrev1 == "")
                {
                    totalrev1 = "0";
                }

                int countlocation = location1.Count();
                int countinvno = countlocation - 6;

                List<int> invno1 = new List<int>();

                for (int i = 6; i < 6 + countinvno; i++)
                {
                    invno1.Add(Convert.ToInt32(location1[i]));
                }

                var str = String.Join(",", invno1);
                mWGE mwge = new mWGE();

                using (var db = new DBS_WGE_Entities())
                {
                    var oData = (from a in db.OINV
                                 join b in db.OSLP on a.SlpCode equals b.SlpCode into bb
                                 from b in bb.DefaultIfEmpty()
                                 join c in db.OCRD on a.CardCode equals c.CardCode into cc
                                 from c in cc.DefaultIfEmpty()
                                 join d in db.C_MCT_BILLING on a.DocNum equals d.InvNo into dd
                                 from d in dd.DefaultIfEmpty()
                                 where (1 == 1) && (invno1.Contains(d.InvNo ?? default(int))) && (d.Status == "1")
                                 select new OINV2
                                 {
                                     CardCode = d.CardCode,
                                     CardName = d.CardName,
                                     Address = a.Address,
                                     DocNum = d.InvNo,
                                     DocDate = d.DateBill,
                                     DocDueDate = d.DueDateBill,
                                     Total = d.Total,
                                     Refno = d.Refno
                                 });

                    oData = oData.OrderByDescending(x => x.DocNum);

                    if (oData != null)
                    {
                        mwge.ListOINV = oData.ToList();
                    }

                    //เช็คบิลซ้ำ---------------------------------------------------------

                    var oDataC = (from a in db.C_MCT_RECEIVE
                                  where invno1.Contains(a.InvNo ?? default(int))
                                  select a).ToList();

                    if (oDataC.Count != 0)
                    {
                        var oDataD = oDataC.Where(z => z.Status == "0").ToList();
                        bool x = false;

                        if (oDataD.Count != 0)
                        {
                            x = true;
                        }

                        if (x == false)
                        {
                            TempData["msg"] = "<script>alert('ไม่สามารถบันทึกซ้ำได้');</script>";
                            return RedirectToAction("Home", "Index");
                        }
                    }

                    //---------------------------------------------------------------

                    C_MCT_RECEIVE dbs = new C_MCT_RECEIVE();

                    foreach (var oItem in mwge.ListOINV)
                    {
                        dbs.InvNo = oItem.DocNum;
                        dbs.CardCode = oItem.CardCode;
                        dbs.CardName = oItem.CardName;
                        dbs.DateBill = oItem.DocDate;
                        dbs.DueDateBill = oItem.DocDueDate;
                        dbs.Total = oItem.Total;
                        
                        dbs.Refno = oItem.Refno;
                        

                        if (radio1 == 1 || radio1 == 2 || radio1 == 3)
                        {
                            dbs.Status = "1";

                            if (radio1 == 2 || radio1 == 3)
                            {
                                dbs.Bank = bank1;
                                dbs.Branch = branch1;
                                dbs.ChequeNo = chequeno1;
                                dbs.TotalRev = Convert.ToDecimal(totalrev1);

                                if (ChequeDate != null)
                                {
                                    dbs.ChequeDate = ChequeDate;
                                }
                            }
                        }
                        else
                        {
                            dbs.Status = "0";

                            if (radio1 == 4) //บิลไม่ครบ
                            {
                                if (dateback != null)
                                {
                                    dbs.DateRev = dateback;
                                }
                            }
                        }

                        dbs.TypeID = radio1.ToString();
                        dbs.Remark = remark1;
                        dbs.Code = code1;
                        dbs.Name = name1;
                        dbs.Latitude = latitude1[1];
                        dbs.Longitude = longitude1[1];
                        dbs.SubmitDate = DateTime.Now;
                        if(Receivedate != null)
                        {
                            dbs.Receivedate = Receivedate;

                        }


                        db.C_MCT_RECEIVE.Add(dbs);
                        db.SaveChanges();

                        if (radio1 == 1 || radio1 == 2 || radio1 == 3)
                        {
                            var oData2 = (from a in db.C_MCT_BILLING
                                          where (a.Status == "1") && (invno1.Contains(a.InvNo ?? default(int)))
                                          select a).ToList();

                            foreach (var oItem2 in oData2)
                            {
                                if (oItem2.InvNo == oItem.DocNum)
                                {
                                    oItem2.Status = "2";
                                }
                            }

                            db.SaveChanges();
                        }
                    }

                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "Add", System.Reflection.MethodBase.GetCurrentMethod().Name, str, "success"));
                    TempData["msg"] = "<script>alert('Success');</script>";
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString()));
                TempData["msg"] = "<script>alert('Error');</script>";
                /*LineNotify("uNtLlL6x4ju1NDEyocwuUYjbQynXuxcVUGC7E1Sd8aC", "คุณ " + Session["Name"].ToString() + "บันทึกเก็บเงินไม่สำเร็จ!!!!");*/
            }

            return View();
        }

        public ActionResult ConditionBilling()
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public ActionResult ConditionBilling(string cardcodeno, string cardname, mWGE mData)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                if (cardcodeno == "C")
                {
                    cardcodeno = "";
                }
                else if (cardname == "")
                {
                    cardname = "";
                }

                using (var db = new DBS_WGE_Entities())
                {
                    var oData = (from a in db.OCRD
                                 join b in db.OCTG on a.GroupNum equals b.GroupNum
                                 join c in db.OINV on a.CardCode equals c.CardCode into cc
                                 from c in cc.DefaultIfEmpty()
                                 where a.CardCode.StartsWith("C") && c.DocDueDate >= timenow
                                 select new OCRD2
                                 {
                                     CardCode = a.CardCode,
                                     CardName = a.CardName,
                                     GroupNum = a.GroupNum,
                                     DocDueDate = c.DocDueDate,
                                     PymntGroup = b.PymntGroup,
                                     U_WGE_Billing = a.U_WGE_Billing,
                                     U_WGE_PayDetail = a.U_WGE_PayDetail,
                                     U_WGE_BillingIns = a.U_WGE_BillingIns
                                 });

                    if (!string.IsNullOrEmpty(cardcodeno))
                    {
                        oData = oData.Where(a => a.CardCode == cardcodeno);
                    }
                    else if (!string.IsNullOrEmpty(cardname))
                    {
                        oData = oData.Where(a => a.CardName.Contains(cardname));
                    }

                    if (oData != null)
                    {
                        mData.ListOCRD = oData.OrderBy(a => a.DocDueDate).Take(1).ToList();
                        log.WriteLog(string.Format("{0}({1}) : {2}", Session["Name"].ToString(), Session["Code"].ToString(), "Search Condition Billing"));
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
            }

            return View(mData);
        }

        [HttpGet]
        public ActionResult WorkOrder()
        {
            mWGE mData = new mWGE();

            if (Session["Data"] != null)
            {
                string Data1 = Session["Data"].ToString();
                Session["Data"] = null;
                return WorkOrder(Data1, null);
            }
            else
            {
                string id = null;
                return WorkOrder(id,null);
            }

        }

        public ActionResult WorkOrder(string id,string postel)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                mWGE mData = new mWGE();
                using (var db = new DBS_WGE_Entities())
                {
                    if (id != null)
                    {

                        int x = Convert.ToInt32(id);

                        var oData = (from a in db.C_MCT_BILL_WORKORDER
                                     join b in db.C_MCT_BILL_GENERAL on a.WorkOrder_ID equals b.WorkOrder_ID into bb
                                     from b in bb.DefaultIfEmpty()
                                     join c in db.C_MCT_BILL_BANK on a.WorkOrder_ID equals c.WorkOrder_ID into cc
                                     from c in cc.DefaultIfEmpty()
                                     where a.ID == x
                                     select new C_MCT_BILL_WORKORDER2
                                     {
                                         Commander_Code = a.Commander_Code,
                                         Commander_Name = a.Commander_Name,
                                         Commander_Department = a.Commander_Department,
                                         Commander_Datetime = a.Commander_Datetime,
                                         Contact_Name = a.Contact_Name,
                                         Recipient_Datetime = a.Recipient_Datetime,
                                         Recipient_Code = a.Recipient_Code,
                                         Recipient_Name = a.Recipient_Name,
                                         Remark = a.Remark,
                                         Contact_Tel = a.Contact_Tel,
                                         ID = a.ID,
                                         Status = a.Status,
                                         Type_Work = a.Type_Work,
                                         Type = b.Type,
                                         Description = b.Description,
                                         Bank_Name = c.Bank_Name,
                                         Option = c.Option,
                                         Other = c.Other,
                                         RemarkAdmin = a.RemarkAdmin,
                                         wk_path_img = a.wk_path_img
                                     }).ToList();

                        var oData2 = (from a in db.OUDP
                                      select a).ToList();

                        if (oData.Count() > 0)
                        {
                            mData.ListBW2 = oData;
                        }

                        if (oData2.Count() > 0)
                        {
                            mData.ListOUDP = oData2;
                        }

                        var checkview = (from a in db.C_MCT_BILL_WORKORDER
                                        where a.ID ==x select a ).FirstOrDefault();

                        if (Session["Code"].ToString() == "37001")
                        {
                            checkview.MS_1 = "Y";
                        }
                        else if (Session["Code"].ToString() == "47003")
                        {
                            checkview.MS_2 = "Y";
                        }
                        else if (Session["Code"].ToString() == "58010")
                        {
                            checkview.MS_3 = "Y";
                        }
                        else if (Session["Code"].ToString() == "61002")
                        {
                            checkview.MS_4 = "Y";
                        }

                        db.SaveChanges();

                        var checkview2 = (from a in db.C_MCT_BILL_WORKORDER
                                          where a.ID == x select a).FirstOrDefault();
                        mData.MS_1 = checkview2.MS_1;
                        mData.MS_2 = checkview2.MS_2;
                        mData.MS_3 = checkview2.MS_3;
                        mData.MS_4 = checkview2.MS_4;

                        log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "View WorkOrder ", System.Reflection.MethodBase.GetCurrentMethod().Name, checkview2.Work_ID, "success"));
                        return View(mData);

                    }
                    else if(id == null && postel != null)
                    {
                        return Postelget(postel);
                    }
                    else 
                    {
                        var oData2 = (from a in db.OUDP
                                      select a).ToList();

                        mData.ListBW2 = null;
                        mData.date = DateTime.Now;

                        if (oData2.Count() > 0)
                        {
                            mData.ListOUDP = oData2;
                        }
                        return View(mData);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('Error!');</script>";
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                return RedirectToAction("Index", "Home");
            }
        }
        public ActionResult getWorkOrder(mWGE data, HttpPostedFileBase file)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                
                mWGE mData = new mWGE();

                using (var db = new DBS_WGE_Entities())
                {
                    if (data.BW.button == "add")
                    {

                        TimeSpan start = new TimeSpan(11, 0, 0);
                        if (DateTime.Now.TimeOfDay > start)
                        {
                            TempData["msg"] = "<script>alert('รอบจัดส่งทุกๆ วันพุธ และวันศุกร์ ของสัปดาห์ /n !!!หากมีการสร้างรายการหลังเวลา 16:00น ของวันพุธ หรือ ศุกร์ ระบบจะตัดเป็นรอบถัดไปทันที');</script>";
                        }

                        C_MCT_BILL_WORKORDER data_billworkorder = new C_MCT_BILL_WORKORDER();
                        C_MCT_BILL_GENERAL data_billgeneral = new C_MCT_BILL_GENERAL();
                        C_MCT_BILL_BANK data_billbank = new C_MCT_BILL_BANK();
                        C_MCT_BILL_POSTOFFICE data_billpostoffice = new C_MCT_BILL_POSTOFFICE();

                        string path3 = string.Empty;
                        
                        DateTime dt = DateTime.ParseExact(data.BW.date + " " + data.BW.time, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                        
                        
                        string ID = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                        if (data.BW.Type_Work != "P")
                        {

                            var ID_Work = (from a in db.C_MCT_BILL_WORKORDER
                                           where a.Type_Work != "P" && a.Work_ID != null
                                           orderby a.Work_ID descending
                                           select a).FirstOrDefault();
                            data_billworkorder.Work_ID = ID_Work.Work_ID + 1;
                        }

                        if (data.BW.Type_Work == "P")
                        {
                            var ID_Postel = (from a in db.C_MCT_BILL_WORKORDER
                                             where a.Type_Work == "P" && a.Postel_ID != null
                                             orderby a.Postel_ID descending
                                             select a).FirstOrDefault();
                            data_billworkorder.Postel_ID = ID_Postel.Postel_ID + 1;
                        }

                        data_billworkorder.WorkOrder_ID = ID;
                        data_billworkorder.Commander_Code = data.BW.Commander_Code;
                        data_billworkorder.Commander_Name = data.BW.Commander_Name;
                        data_billworkorder.Commander_Datetime = dt;
                        data_billworkorder.Contact_Name = data.BW.Contact_Name;
                        data_billworkorder.Contact_Tel = data.BW.Contact_Tel;
                        data_billworkorder.Type_Work = data.BW.Type_Work;
                        data_billworkorder.Remark = data.BW.Remark;
                        data_billworkorder.Status = "W";
                        if((Session["Position"].ToString()=="8" || Session["Position"].ToString() == "10") && data.BW.Type_Work =="P")
                        {
                            if (data.BW.jobtitle == "Food Industry")
                            {
                                data_billworkorder.Commander_Department = "Food Industry";
                                data_billworkorder.Commander_JobTitle = "Food Industry";
                            }
                            else if (data.BW.jobtitle == "Food Service")
                            {
                                data_billworkorder.Commander_Department = "Food Service";
                                data_billworkorder.Commander_JobTitle = "Food Service";
                            }
                            else if(data.BW.jobtitle=="Event & Promotions")
                            {
                                data_billworkorder.Commander_Department = "Modern Trade";
                                data_billworkorder.Commander_JobTitle = "Event & Promotions";
                            }
                            else if (data.BW.jobtitle == "Merchandiser")
                            {
                                data_billworkorder.Commander_Department = "Modern Trade";
                                data_billworkorder.Commander_JobTitle = "Merchandiser";
                            }
                            else if(data.BW.jobtitle=="Key Account")
                            {
                                data_billworkorder.Commander_Department = "Modern Trade";
                                data_billworkorder.Commander_JobTitle = "Key Account";
                            }
                            else if (data.BW.jobtitle == "Marketting")
                            {
                                data_billworkorder.Commander_Department = "Product Management";
                                data_billworkorder.Commander_JobTitle = "Marketting";
                            }
                            else if (data.BW.jobtitle == "Product Specialist & PR")
                            {
                                data_billworkorder.Commander_Department = "Business Development";
                                data_billworkorder.Commander_JobTitle = "Product Specialist & PR";
                            }
                            else if (data.BW.jobtitle == "Delice")
                            {
                                data_billworkorder.Commander_Department = "Business Development";
                                data_billworkorder.Commander_JobTitle = "Delice";
                            }
                            else if (data.BW.jobtitle == "Food Solutions")
                            {
                                data_billworkorder.Commander_Department = "Food Solutions";
                                data_billworkorder.Commander_JobTitle = "Food Solutions";
                            }
                            else if (data.BW.jobtitle == "Sales Support")
                            {
                                data_billworkorder.Commander_Department = "Sales Support";
                                data_billworkorder.Commander_JobTitle = "Sales Support";
                            }
                            else if (data.BW.jobtitle == "Production")
                            {
                                data_billworkorder.Commander_Department = "Production";
                                data_billworkorder.Commander_JobTitle = "Production";
                            }
                            else if (data.BW.jobtitle == "Executive Management")
                            {
                                data_billworkorder.Commander_Department = "Executive Management";
                                data_billworkorder.Commander_JobTitle = "Executive Management";
                            }
                            else if (data.BW.jobtitle == "Procurement")
                            {
                                data_billworkorder.Commander_Department = "Procurement";
                                data_billworkorder.Commander_JobTitle = "Procurement";
                            }
                            else if (data.BW.jobtitle == "Warehouse")
                            {
                                data_billworkorder.Commander_Department = "Warehouse";
                                data_billworkorder.Commander_JobTitle = "Warehouse";
                            }
                            else if (data.BW.jobtitle == "IT")
                            {
                                data_billworkorder.Commander_Department = "IT";
                                data_billworkorder.Commander_JobTitle = "IT";
                            }
                            else if(data.BW.jobtitle == "General Admin")
                            {
                                data_billworkorder.Commander_Department = "General Admin";
                                data_billworkorder.Commander_JobTitle = "General Admin";
                            }
                            else if (data.BW.jobtitle == "Human Resources")
                            {
                                data_billworkorder.Commander_Department = "Human Resources";
                                data_billworkorder.Commander_JobTitle = "Human Resources";
                            }
                            else if (data.BW.jobtitle == "Corporate Accounting")
                            {
                                data_billworkorder.Commander_Department = "Corporate Accounting";
                                data_billworkorder.Commander_JobTitle = "Corporate Accounting";
                            }
                            else if (data.BW.jobtitle == "Corporate Finance")
                            {
                                data_billworkorder.Commander_Department = "Corporate Finance";
                                data_billworkorder.Commander_JobTitle = "Corporate Finance";
                            }
                            else
                            {
                                data_billworkorder.Commander_Department = data.BW.Commander_Department;
                                data_billworkorder.Commander_JobTitle = data.BW.jobtitle;
                            }
                        }
                        else
                        {
                            data_billworkorder.Commander_Department = data.BW.Commander_Department;
                            data_billworkorder.Commander_JobTitle = data.BW.jobtitle;
                        }
                        if (file != null)
                        {
                            string filetype = file.FileName;
                            string[] filetype2 = filetype.Split('.');
                            //save pathD:\source project แก้ไข\New GeoCodingTest 091121\GeoCodingTest\GeoCodingTest\Image\WK_Order_Img\
                            string filename = DateTime.Now.ToString("yyyyMMddHHmmss") + "." + filetype2.Last();
                            var path = AppDomain.CurrentDomain.BaseDirectory + "Image/WK_Order_Img";
                            var path2 = Path.Combine(path, filename);
                            file.SaveAs(path2);
                            path3 = string.Format("~/Image/WK_Order_Img/{0}", filename);
                            //จบ save path
                            data_billworkorder.wk_path_img = path3;
                        }



                        db.C_MCT_BILL_WORKORDER.Add(data_billworkorder);
                        db.SaveChanges();


                        if (data.BW.Type_Work == "S" || data.BW.Type_Work == "R" || data.BW.Type_Work == "O")
                        {
                            data_billgeneral.WorkOrder_ID = ID;
                            data_billgeneral.Type = data.BW.Type_Work;

                            if (data.BW.Type_Work == "S")
                                data_billgeneral.Description = data.BW.des1;
                            else if (data.BW.Type_Work == "R")
                                data_billgeneral.Description = data.BW.des2;
                            else
                                data_billgeneral.Description = data.BW.des3;

                            db.C_MCT_BILL_GENERAL.Add(data_billgeneral);
                            db.SaveChanges();
                        }
                        else if (data.BW.Type_Work == "B")
                        {
                            data_billbank.WorkOrder_ID = ID;
                            data_billbank.Bank_Name = data.BB.Bank_Name;
                            data_billbank.Option = data.BB.Option;

                            if (data.BB.Option == "OT")
                                data_billbank.Other = data.BB.Other;

                            db.C_MCT_BILL_BANK.Add(data_billbank);
                            db.SaveChanges();
                        }
                        else if (data.BW.Type_Work == "P")
                        {
                            foreach (var item in data.BP)
                            {

                                if (item.Name != null && item.Type != "" && file == null)
                                {
                                    data_billpostoffice.WorkOrder_ID = ID;
                                    data_billpostoffice.Type = item.Type;
                                    data_billpostoffice.Name = item.Name;
                                    data_billpostoffice.Postel = item.Postel;
                                    data_billpostoffice.Itemcode = item.Itemcode;
                                    data_billpostoffice.Price = item.Price;
                                    data_billpostoffice.EMSRev = item.EMSRev;

                                    db.C_MCT_BILL_POSTOFFICE.Add(data_billpostoffice);
                                    db.SaveChanges();
                                }
                            }
                        }



                        log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "Add", System.Reflection.MethodBase.GetCurrentMethod().Name, ID, "success"));

                        TempData["msg"] = "<script>alert('Success!!');</script>";

                        //Session["Data"] = null;
                        //Session["Data"] = data_billworkorder.ID;

                        if (data.BW.Type_Work == "P")
                        {
                            return RedirectToAction("WorkListPostel");
                        }
                        else
                        {
                            var email_oData = (from a in db.C_MCT_BILL_WORKORDER
                                               where a.WorkOrder_ID == ID
                                               select a).FirstOrDefault();

                            if (email_oData != null)
                            {
                                sendemail(email_oData.ID, "add").ConfigureAwait(false);
                            }

                            return RedirectToAction("WorkList");
                        }
                    }//เพิ่ม
                    else if (data.BW.button == "finishjob")
                    {
                        int id = Convert.ToInt32(data.BW.ID);

                        string workid = db.C_MCT_BILL_WORKORDER.Where(z => z.ID == id).Select(x => x.WorkOrder_ID).FirstOrDefault();

                        if (workid != null)
                        {


                            var oData = (from a in db.C_MCT_BILL_WORKORDER
                                         where a.WorkOrder_ID == workid
                                         select a).FirstOrDefault();

                            oData.Status = "F";
                            oData.Recipient_Code = Session["Code"].ToString();
                            oData.Recipient_Name = Session["Name"].ToString();
                            oData.Recipient_Datetime = DateTime.Now;

                            db.SaveChanges();

                            //sendemail(oData.ID, data.BW.button).ConfigureAwait(false);

                            log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "finishjob", System.Reflection.MethodBase.GetCurrentMethod().Name, workid, "success"));

                            TempData["msg"] = "<script>alert('Success!!');</script>";

                            //Session["Data"] = null;
                            //Session["Data"] = id;

                            if (data.BW.Type_Work == "P")
                            {
                                return RedirectToAction("WorkListPostel");
                            }
                            else
                            {
                                return RedirectToAction("WorkList");
                            }
                        }
                        else
                        {

                            log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "Error Finishjob workid = null", System.Reflection.MethodBase.GetCurrentMethod().Name, workid, "Error"));

                            TempData["msg"] = "<script>alert('Error!!');</script>";
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else if (data.BW.button == "update")
                    {
                        C_MCT_BILL_WORKORDER data_billworkorder = new C_MCT_BILL_WORKORDER();
                        C_MCT_BILL_GENERAL data_billgeneral = new C_MCT_BILL_GENERAL();
                        C_MCT_BILL_BANK data_billbank = new C_MCT_BILL_BANK();
                        C_MCT_BILL_POSTOFFICE data_billpostoffice = new C_MCT_BILL_POSTOFFICE();

                        int id = Convert.ToInt32(data.BW.ID);
                        DateTime dt = DateTime.ParseExact(data.BW.date + " " + data.BW.time, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                        string workid = db.C_MCT_BILL_WORKORDER.Where(z => z.ID == id).Select(x => x.WorkOrder_ID).FirstOrDefault();
                        var oData = (from a in db.C_MCT_BILL_WORKORDER
                                     where a.WorkOrder_ID == workid
                                     select a).FirstOrDefault();

                        var oData2 = (from a in db.C_MCT_BILL_GENERAL
                                      where a.WorkOrder_ID == workid
                                      select a).FirstOrDefault();
                        string path3 = string.Empty;
                        

                        if (workid != null)
                        {
                            

                            if (file != null)
                            {

                                string filetype = file.FileName;
                                string[] filetype2 = filetype.Split('.');
                                //save pathD:\source project แก้ไข\New GeoCodingTest 091121\GeoCodingTest\GeoCodingTest\Image\WK_Order_Img\
                                string filename = DateTime.Now.ToString("yyyyMMddHHmmss") + "." + filetype2.Last();
                                var path = AppDomain.CurrentDomain.BaseDirectory + "Image/WK_Order_Img";
                                var path2 = Path.Combine(path, filename);
                                file.SaveAs(path2);
                                path3 = string.Format("~/Image/WK_Order_Img/{0}", filename);
                            }
                            
                            if (file != null)
                            {
                                
                                //ลบรูปภาพเดิม 
                                string filename = oData.wk_path_img;
                                var path = AppDomain.CurrentDomain.BaseDirectory;
                                string str = filename.Replace("~/", path);

                                if (System.IO.File.Exists(str))
                                {
                                    System.IO.File.Delete(str);
                                }

                                oData.wk_path_img = path3;
                            }
                            else if (oData.wk_path_img == null || oData.wk_path_img == string.Empty)
                            {
                                oData.wk_path_img = null;
                            }


                            if (oData2 != null)
                            {
                                db.C_MCT_BILL_GENERAL.Remove(oData2);
                                db.SaveChanges();
                            }

                            var oData3 = (from a in db.C_MCT_BILL_BANK
                                          where a.WorkOrder_ID == workid
                                          select a).FirstOrDefault();

                            if (oData3 != null)
                            {
                                db.C_MCT_BILL_BANK.Remove(oData3);
                                db.SaveChanges();
                            }
                                          
                            
                            oData.Commander_Code = data.BW.Commander_Code;
                            oData.Commander_Name = data.BW.Commander_Name;
                            if (Session["Position"].ToString() == "8" || Session["Position"].ToString() == "10")
                            {
                                if (data.BW.jobtitle == "Food Industry")
                                {
                                    oData.Commander_Department = "Food Industry";
                                    oData.Commander_JobTitle = "Food Industry";
                                }
                                else if (data.BW.jobtitle == "Food Service")
                                {
                                    oData.Commander_Department = "Food Service";
                                    oData.Commander_JobTitle = "Food Service";
                                }
                                else if (data.BW.jobtitle == "Event & Promotions")
                                {
                                    oData.Commander_Department = "Modern Trade";
                                    oData.Commander_JobTitle = "Event & Promotions";
                                }
                                else if (data.BW.jobtitle == "Merchandiser")
                                {
                                    oData.Commander_Department = "Modern Trade";
                                    oData.Commander_JobTitle = "Merchandiser";
                                }
                                else if (data.BW.jobtitle == "Key Account")
                                {
                                    oData.Commander_Department = "Modern Trade";
                                    oData.Commander_JobTitle = "Key Account";
                                }
                                else if (data.BW.jobtitle == "Marketting")
                                {
                                    oData.Commander_Department = "Product Management";
                                    oData.Commander_JobTitle = "Marketting";
                                }
                                else if (data.BW.jobtitle == "Product Specialist & PR")
                                {
                                    oData.Commander_Department = "Business Development";
                                    oData.Commander_JobTitle = "Product Specialist & PR";
                                }
                                else if (data.BW.jobtitle == "Delice")
                                {
                                    oData.Commander_Department = "Business Development";
                                    oData.Commander_JobTitle = "Delice";
                                }
                                else if (data.BW.jobtitle == "Food Solutions")
                                {
                                    oData.Commander_Department = "Food Solutions";
                                    oData.Commander_JobTitle = "Food Solutions";
                                }
                                else if (data.BW.jobtitle == "Sales Support")
                                {
                                    oData.Commander_Department = "Sales Support";
                                    oData.Commander_JobTitle = "Sales Support";
                                }
                                else if (data.BW.jobtitle == "Production")
                                {
                                    oData.Commander_Department = "Production";
                                    oData.Commander_JobTitle = "Production";
                                }
                                else if (data.BW.jobtitle == "Executive Management")
                                {
                                    oData.Commander_Department = "Executive Management";
                                    oData.Commander_JobTitle = "Executive Management";
                                }
                                else if (data.BW.jobtitle == "Procurement")
                                {
                                    oData.Commander_Department = "Procurement";
                                    oData.Commander_JobTitle = "Procurement";
                                }
                                else if (data.BW.jobtitle == "Warehouse")
                                {
                                    oData.Commander_Department = "Warehouse";
                                    oData.Commander_JobTitle = "Warehouse";
                                }
                                else if (data.BW.jobtitle == "IT")
                                {
                                    oData.Commander_Department = "IT";
                                    oData.Commander_JobTitle = "IT";
                                }
                                else if (data.BW.jobtitle == "General Admin")
                                {
                                    oData.Commander_Department = "General Admin";
                                    oData.Commander_JobTitle = "General Admin";
                                }
                                else if (data.BW.jobtitle == "Human Resources")
                                {
                                    oData.Commander_Department = "Human Resources";
                                    oData.Commander_JobTitle = "Human Resources";
                                }
                                else if (data.BW.jobtitle == "Corporate Accounting")
                                {
                                    oData.Commander_Department = "Corporate Accounting";
                                    oData.Commander_JobTitle = "Corporate Accounting";
                                }
                                else if (data.BW.jobtitle == "Corporate Finance")
                                {
                                    oData.Commander_Department = "Corporate Finance";
                                    oData.Commander_JobTitle = "Corporate Finance";
                                }
                            }
                            else
                            {
                                oData.Commander_Department = data.BW.Commander_Department;
                                oData.Commander_JobTitle = data.BW.jobtitle;
                            }
                            if (data.BW.Type_Work == "S" || data.BW.Type_Work == "R" || data.BW.Type_Work == "O" || data.BW.Type_Work == "B")
                            {
                                oData.Contact_Name = data.BW.Contact_Name;
                                oData.Contact_Tel = data.BW.Contact_Tel;
                            }
                          //oData.Commander_Datetime = dt;
                            oData.Type_Work = data.BW.Type_Work;
                            oData.Remark = data.BW.Remark;
                            oData.RemarkAdmin = data.BW.RemarkAdmin;

                            db.SaveChanges();

                            if (data.BW.Type_Work == "S" || data.BW.Type_Work == "R" || data.BW.Type_Work == "O")
                            {
                                data_billgeneral.WorkOrder_ID = workid;
                                data_billgeneral.Type = data.BW.Type_Work;

                                if (data.BW.Type_Work == "S")
                                    data_billgeneral.Description = data.BW.des1;
                                else if (data.BW.Type_Work == "R")
                                    data_billgeneral.Description = data.BW.des2;
                                else
                                    data_billgeneral.Description = data.BW.des3;

                                db.C_MCT_BILL_GENERAL.Add(data_billgeneral);
                                db.SaveChanges();
                            }
                            else if (data.BW.Type_Work == "B")
                            {
                                data_billbank.WorkOrder_ID = workid;
                                data_billbank.Bank_Name = data.BB.Bank_Name;
                                data_billbank.Option = data.BB.Option;

                                if (data.BB.Option == "OT")
                                    data_billbank.Other = data.BB.Other;

                                db.C_MCT_BILL_BANK.Add(data_billbank);
                                db.SaveChanges();
                            }

                            else if (data.BW.Type_Work == "P" || data.BW.Type_Work == "G" || data.BW.Type_Work == "E" || data.BW.Type_Work == "U")
                            {

                                C_MCT_POSTOFFICE_REF data_POSTOFFICE_Report = new C_MCT_POSTOFFICE_REF();


                                decimal d = 0;
                                foreach (var oitem in data.BP)
                                {
                                    if (oitem.Name != null)
                                    {
                                        decimal a = oitem.Price ?? 0;
                                        d += a;
                                    }
                                }
                                
                                int typenull = 0;
                                    foreach (var checktypenull in data.BP)
                                    {
                                        if (checktypenull.Type == null)
                                        {
                                        typenull += 1;
                                        }

                                    }
                                if(typenull > 0)
                                {

                                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "update Error : Typenull>0", System.Reflection.MethodBase.GetCurrentMethod().Name, workid, "Error"));

                                    TempData["msg"] = "<script>alert('โปรดเลือกประเภทงานก่อนบันทึก!');</script>";
                                    return RedirectToAction("WorkListPostel", "GeoCoding");
                                           
                                }
                                var oData7 = (from a in db.C_MCT_BILL_WORKORDER
                                              where a.WorkOrder_ID == workid
                                              select a).FirstOrDefault();

                                foreach (var item in data.BP)
                                {
                                    if (item.Name != null && item.Type != "")
                                    {
                                            var oData9 = (from a in db.C_MCT_BILL_POSTOFFICE
                                                      where a.ID == item.ID
                                                      select a).FirstOrDefault();
                                        if(oData9 != null)
                                        {
                                            oData9.Type = item.Type;
                                            oData9.Postel = item.Postel;
                                            oData9.Name = item.Name;
                                            oData9.Itemcode = item.Itemcode;
                                            oData9.EMSRev = item.EMSRev;
                                            if (item.Price == 0 || item.Price == null)
                                            {
                                                oData9.Price = null;
                                            }
                                            else
                                            {
                                                oData9.Price = item.Price;
                                            }
                                        }
                                        else if(oData9 == null)
                                        {
                                            if (data.BW.Type_Work == "P")
                                            {
                                                data_billpostoffice.WorkOrder_ID = oData7.WorkOrder_ID;
                                                data_billpostoffice.Type = item.Type;
                                                data_billpostoffice.Name = item.Name;
                                                data_billpostoffice.Postel = item.Postel;
                                                db.C_MCT_BILL_POSTOFFICE.Add(data_billpostoffice);
                                            }
                                        }

                                        var oData10 = (from a in db.C_MCT_POSTOFFICE_REF
                                                       where a.post_success_id == item.ID.ToString()
                                                       select a).FirstOrDefault();
                                        if (oData10 != null)
                                        {
                                            oData10.post_type = item.Type;
                                            oData10.post_name = item.Name;
                                            oData10.post_zip = item.Postel;
                                            oData10.post_ems = item.Itemcode;
                                            oData10.post_emsrev = item.EMSRev;
                                            oData10.post_round = oData7.round;
                                            oData10.post_price = item.Price;
                                            oData10.post_date = DateTime.Now;
                                            oData10.post_sumprice = d;
                                            oData10.post_mtyear = oData7.mtyear;
                                            oData10.post_success_id = item.ID.ToString();
                                        }

                                        db.SaveChanges();
                                        
                                    }
                                    else if (item.Name == null)
                                    {
                                        if (data.BW.Type_Work != "P")
                                        {
                                            var oData11 = (from a in db.C_MCT_BILL_POSTOFFICE
                                                      where a.ID == item.ID
                                                      select a).FirstOrDefault();
                                            oData11.Postel = null;
                                            oData11.AC_code = null;
                                            oData11.Ck_print = null;
                                            oData11.EMSRev = null;
                                            oData11.Itemcode = null;
                                            oData11.Price = null;
                                        
                                            var oData12 = (from a in db.C_MCT_POSTOFFICE_REF
                                                           where a.post_success_id == item.ID.ToString()
                                                           select a).FirstOrDefault();

                                            db.C_MCT_POSTOFFICE_REF.Remove(oData12);


                                            db.SaveChanges();
                                        }

                                        else if (data.BW.Type_Work == "P")
                                        {
                                            var oData13 = (from a in db.C_MCT_BILL_POSTOFFICE
                                                           where a.ID == item.ID
                                                           select a).FirstOrDefault();
                                            db.C_MCT_BILL_POSTOFFICE.Remove(oData13);
                                            db.SaveChanges();

                                        }
                                    }
                                }
                                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "Update ", System.Reflection.MethodBase.GetCurrentMethod().Name, data.BW.WorkOrder_ID, "success"));

                                TempData["msg"] = "<script>alert('Success!!');</script>";
                                return RedirectToAction("WorkListPostel");
                            }

                            TempData["msg"] = "<script>alert('Success!!');</script>";


                            if (data.BW.Type_Work == "P")
                            {
                                return RedirectToAction("WorkListPostel");
                            }
                            else
                            {
                                return RedirectToAction("WorkList");
                            }
                        }
                        else
                        {
                            TempData["msg"] = "<script>alert('Error!!');</script>";
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else if (data.BW.button == "acceptjob")
                    {
                        string workid = "";
                        if (data.BW.Type_Work == "G" || data.BW.Type_Work == "E" || data.BW.Type_Work == "U")
                        {
                            string id = data.BW.WorkOrder_ID;
                            workid = db.C_MCT_BILL_WORKORDER.Where(z => z.WorkOrder_ID == id.ToString()).Select(x => x.WorkOrder_ID).FirstOrDefault();
                        }
                        else
                        {
                            int id = Convert.ToInt32(data.BW.ID);
                            workid = db.C_MCT_BILL_WORKORDER.Where(z => z.ID == id).Select(x => x.WorkOrder_ID).FirstOrDefault();
                        }
                        if (workid != null)
                        {
                            if (data.BW.Type_Work == "G" || data.BW.Type_Work == "E" || data.BW.Type_Work == "U")
                            {
                                foreach (var postel in data.BP)
                                {

                                    var PostelDetail = (from a in db.C_MCT_BILL_POSTOFFICE
                                                        where a.AC_code == postel.WorkOrder_ID
                                                        select a).FirstOrDefault();
                                    if (PostelDetail != null)
                                    {
                                        PostelDetail.Ck_print = "D";
                                    }

                                    db.SaveChanges();

                                }

                            }



                            var oData = (from a in db.C_MCT_BILL_WORKORDER
                                         where a.WorkOrder_ID == workid
                                         select a).FirstOrDefault();

                            oData.Status = "G";
                            oData.Recipient_Code = Session["Code"].ToString();
                            oData.Recipient_Name = Session["Name"].ToString();
                            oData.Recipient_Datetime = DateTime.Now;

                            db.SaveChanges();

                            sendemail(oData.ID,data.BW.button).ConfigureAwait(false);


                            log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "acceptjob", System.Reflection.MethodBase.GetCurrentMethod().Name, workid, "success"));

                            TempData["msg"] = "<script>alert('Success!!');</script>";

                            //Session["Data"] = null;
                            //Session["Data"] = id;

                            if (data.BW.Type_Work == "P")
                            {
                                return RedirectToAction("WorkListPostel");
                            }
                            else
                            {
                                return RedirectToAction("WorkList");
                            }
                        }
                        else
                        {
                            
                            log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "acceptjob null Workid", System.Reflection.MethodBase.GetCurrentMethod().Name, workid, "error"));
                            
                            TempData["msg"] = "<script>alert('Error!!');</script>";
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else if (data.BW.button == "rejectjob")
                    {
                        int id = Convert.ToInt32(data.BW.ID);
                        string workid = db.C_MCT_BILL_WORKORDER.Where(z => z.ID == id).Select(x => x.WorkOrder_ID).FirstOrDefault();

                        if (workid != null)
                        {
                            var oData = (from a in db.C_MCT_BILL_WORKORDER
                                         where a.WorkOrder_ID == workid
                                         select a).FirstOrDefault();

                            oData.Status = "C";
                            //oData.Recipient_Code = Session["Code"].ToString();
                            //oData.Recipient_Name = Session["Name"].ToString();
                            //oData.Recipient_Datetime = DateTime.Now;
                            oData.Last_Datetime = DateTime.Now;

                            db.SaveChanges();

                            sendemail(oData.ID, data.BW.button).ConfigureAwait(false); ;


                            log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "rejectjob", System.Reflection.MethodBase.GetCurrentMethod().Name, workid, "success"));

                            TempData["msg"] = "<script>alert('Success!!');</script>";

                            //Session["Data"] = null;
                            //Session["Data"] = id;

                            if (data.BW.Type_Work == "P")
                            {
                                return RedirectToAction("WorkListPostel");
                            }
                            else
                            {
                                return RedirectToAction("WorkList");
                            }
                        }
                        else
                        {
                            log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "rejectjob : null Workid", System.Reflection.MethodBase.GetCurrentMethod().Name, data.BW.WorkOrder_ID, "Error"));

                            TempData["msg"] = "<script>alert('Error!!');</script>";
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else if (data.BW.button == "updatejob")
                    {
                        string workid = "";
                        if (data.BW.Type_Work == "G" || data.BW.Type_Work == "E" || data.BW.Type_Work == "U")
                        {
                            string id = data.BW.WorkOrder_ID;
                            workid = db.C_MCT_BILL_WORKORDER.Where(z => z.WorkOrder_ID == id.ToString()).Select(x => x.WorkOrder_ID).FirstOrDefault();
                        }
                        else
                        {
                            int id = Convert.ToInt32(data.BW.ID);
                            workid = db.C_MCT_BILL_WORKORDER.Where(z => z.ID == id).Select(x => x.WorkOrder_ID).FirstOrDefault();
                        }

                        if (workid != null)
                        {
                            string status = data.BW.Status;
                            var oData = (from a in db.C_MCT_BILL_WORKORDER
                                         where a.WorkOrder_ID == workid
                                         select a).FirstOrDefault();

                            oData.Status = data.BW.Status;

                            if (data.BW.Status == "W")
                            {
                                oData.Recipient_Code = null;
                                oData.Recipient_Name = null;
                                oData.Recipient_Datetime = null;

                            }
                            if (data.BW.Status == "P" || data.BW.Status == "C")
                            {
                                oData.Last_Datetime = DateTime.Now;

                            }
                            if (data.BW.Type_Work == "G" || data.BW.Type_Work == "E" || data.BW.Type_Work == "U")
                            {
                                foreach (var postel in data.BP)
                                {


                                    var Postel1 = (from a in db.C_MCT_BILL_WORKORDER
                                                   where a.WorkOrder_ID == postel.WorkOrder_ID
                                                   select a).FirstOrDefault();
                                    var PostelDetail = (from a in db.C_MCT_BILL_POSTOFFICE
                                                        where a.ID == postel.ID
                                                        select a).FirstOrDefault();
                                    if (data.BW.Status == "P")
                                    {
                                        Postel1.Status = "P";
                                        Postel1.Recipient_Code = Session["Code"].ToString();
                                        Postel1.Recipient_Name = Session["Name"].ToString();
                                        Postel1.Recipient_Datetime = DateTime.Now;

                                        if (PostelDetail != null)
                                        {
                                            PostelDetail.Ck_print = "P";
                                        }
                                    }
                                    else if (data.BW.Status == "C")
                                    {

                                        Postel1.Status = "C";
                                        Postel1.Recipient_Code = Session["Code"].ToString();
                                        Postel1.Recipient_Name = Session["Name"].ToString();
                                        Postel1.Recipient_Datetime = DateTime.Now;

                                        if (PostelDetail != null)
                                        {
                                            PostelDetail.Ck_print = "C";
                                        }
                                    }
                                    else if (data.BW.Status == "W")
                                    {
                                        Postel1.Status = "W";
                                        Postel1.Recipient_Code = null;
                                        Postel1.Recipient_Name = null;
                                        Postel1.Recipient_Datetime = null;

                                        if (PostelDetail != null)
                                        {
                                            PostelDetail.Ck_print = null;
                                        }
                                    }
                                    db.SaveChanges();

                                }
                            }

                            db.SaveChanges();
                            
                            if(data.BW.Status == "P")
                                sendemail(oData.ID, "finishjob").ConfigureAwait(false);
                            else if(data.BW.Status == "C")
                                sendemail(oData.ID, "rejectjob").ConfigureAwait(false);

                            log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "updatejob", System.Reflection.MethodBase.GetCurrentMethod().Name, workid, "success"));
                            TempData["msg"] = "<script>alert('Success!!');</script>";

                            //Session["Data"] = null;
                            //Session["Data"] = id;

                            if (data.BW.Type_Work == "P")
                            {
                                return RedirectToAction("WorkListPostel");
                            }
                            else
                            {
                                return RedirectToAction("WorkList");
                            }
                        }
                        else
                        {
                            log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "updatejob : null Workid", System.Reflection.MethodBase.GetCurrentMethod().Name, data.BW.WorkOrder_ID, "Error"));


                            TempData["msg"] = "<script>alert('Error!!');</script>";
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else if (data.BW.button == "delete")
                    {
                        int id = Convert.ToInt32(data.BW.ID);
                        string workid = db.C_MCT_BILL_WORKORDER.Where(z => z.ID == id).Select(x => x.WorkOrder_ID).FirstOrDefault();

                        if (workid != null)
                        {
                            if (data.BW.Type_Work == "G" || data.BW.Type_Work == "U" || data.BW.Type_Work == "E")
                            {

                                var oData5 = (from a in db.C_MCT_BILL_WORKORDER
                                              join b in db.C_MCT_BILL_POSTOFFICE on a.WorkOrder_ID equals b.AC_code
                                              join c in db.C_MCT_POSTOFFICE_REF on b.ID.ToString() equals c.post_success_id
                                              where a.WorkOrder_ID == workid
                                              select c).ToList();
                                if (oData5 != null)
                                {
                                    db.C_MCT_POSTOFFICE_REF.RemoveRange(oData5);
                                    db.SaveChanges();
                                }

                                var oData6 = (from a in db.C_MCT_BILL_POSTOFFICE
                                              where a.AC_code == workid
                                              select a).ToList();

                                foreach (var oitem in oData6)
                                {
                                    var oData7 = (from a in db.C_MCT_BILL_POSTOFFICE
                                                  where a.ID == oitem.ID
                                                  select a).FirstOrDefault();
                                    oData7.AC_code = null;
                                    oData7.EMSRev = null;
                                    oData7.Itemcode = null;
                                    oData7.Ck_print = null;
                                    oData7.Price = null;
                                    db.SaveChanges();
                                }
                                var oData9 = data.BP.ToList();
                                
                                foreach (var lasttime in data.BP)
                                {
                                    var oData10 = (from a in db.C_MCT_BILL_WORKORDER
                                                  where a.WorkOrder_ID == lasttime.WorkOrder_ID
                                                  select a).FirstOrDefault();
                                    oData10.Last_Datetime = null;
                                    db.SaveChanges();
                                }
                            }

                            var oData = (from a in db.C_MCT_BILL_WORKORDER
                                         where a.WorkOrder_ID == workid
                                         select a).FirstOrDefault();

                            if (oData != null)
                            {
                                sendemail(oData.ID, "delete").ConfigureAwait(false);

                                db.C_MCT_BILL_WORKORDER.Remove(oData);
                                db.SaveChanges();
                            }

                            var oData2 = (from a in db.C_MCT_BILL_GENERAL
                                          where a.WorkOrder_ID == workid
                                          select a).FirstOrDefault();

                            if (oData2 != null)
                            {
                                db.C_MCT_BILL_GENERAL.Remove(oData2);
                                db.SaveChanges();
                            }

                            var oData3 = (from a in db.C_MCT_BILL_BANK
                                          where a.WorkOrder_ID == workid
                                          select a).FirstOrDefault();

                            if (oData3 != null)
                            {
                                db.C_MCT_BILL_BANK.Remove(oData3);
                                db.SaveChanges();
                            }

                            var oData4 = (from a in db.C_MCT_BILL_POSTOFFICE
                                          where a.WorkOrder_ID == workid
                                          select a).ToList();

                            if (oData4 != null)
                            {
                                db.C_MCT_BILL_POSTOFFICE.RemoveRange(oData4);
                                db.SaveChanges();
                            }
                            var oData8 = (from a in db.C_MCT_POSTOFFICE_REF
                                          where a.post_wk_id == workid
                                          select a).ToList();

                            if (oData8 != null)
                            {
                                db.C_MCT_POSTOFFICE_REF.RemoveRange(oData8);
                                db.SaveChanges();
                            }


                            log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "delete", System.Reflection.MethodBase.GetCurrentMethod().Name, workid, "success"));
                            TempData["msg"] = "<script>alert('Delete Success!!');</script>";
                            //return RedirectToAction("WorkList");

                            if (data.BW.Type_Work == "P" || data.BW.Type_Work == "G" || data.BW.Type_Work == "U" || data.BW.Type_Work == "E")
                            {
                                return RedirectToAction("WorkListPostel");
                            }
                            else
                            {

                                return RedirectToAction("WorkList");
                            }
                        }
                        else
                        {
                            log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "Delete : null Workid", System.Reflection.MethodBase.GetCurrentMethod().Name, data.BW.WorkOrder_ID, "Error"));

                            TempData["msg"] = "<script>alert('Error!!');</script>";
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else if (data.BW.button == "export")
                    {
                        if (data.BW.date_print == null || data.BW.date_print == DateTime.MinValue)
                        {
                            TempData["msg"] = "<script>alert('คุณยังไม่ได้ระบุวันที่ส่ง');</script>";
                        }
                        if (data.BW.Type_Work == "E")//EMS
                        {
                           
                            if (data.BW.date_print != null && data.BW.date_print != DateTime.MinValue )
                            {
                                var printdate2 = (from a in db.C_MCT_BILL_WORKORDER
                                                  where a.WorkOrder_ID == data.BW.WorkOrder_ID
                                                  select a).FirstOrDefault();

                                printdate2.print_date = data.BW.date_print;
                                db.SaveChanges();
                            }
                            
                            ReportDocument rd = new ReportDocument();
                            rd.Load(Path.Combine(Server.MapPath("~/Report"), "PostofficeEMS.rpt"));


                            rd.SetParameterValue("@checkbox", data.BW.WorkOrder_ID);


                            rd.SetDatabaseLogon("sa", "Wge123456*", "SVERP01", "DBS_WGE");

                            Response.Buffer = false;
                            Response.ClearContent();
                            Response.ClearHeaders();

                            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                            stream.Seek(0, SeekOrigin.Begin);


                            rd.Close();
                            rd.Dispose();

                            log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "Export : EMS ", System.Reflection.MethodBase.GetCurrentMethod().Name, data.BW.WorkOrder_ID, "success"));

                            return File(stream, "application/pdf", "EMS.pdf");
                        }
                        else if (data.BW.Type_Work == "U")//ลงทะเบียน
                        {
                            if (data.BW.date_print != null && data.BW.date_print != DateTime.MinValue)
                            {
                                var printdate2 = (from a in db.C_MCT_BILL_WORKORDER
                                                  where a.WorkOrder_ID == data.BW.WorkOrder_ID
                                                  select a).FirstOrDefault();

                                printdate2.print_date = data.BW.date_print;
                                db.SaveChanges();
                            }

                            ReportDocument rd = new ReportDocument();
                            rd.Load(Path.Combine(Server.MapPath("~/Report"), "PostofficeRegister.rpt"));

                            rd.SetParameterValue("@checkbox", data.BW.WorkOrder_ID);

                            rd.SetDatabaseLogon("sa", "Wge123456*", "SVERP01", "DBS_WGE");

                            Response.Buffer = false;
                            Response.ClearContent();
                            Response.ClearHeaders();
                            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                            stream.Seek(0, SeekOrigin.Begin);


                            rd.Close();
                            rd.Dispose();

                            log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "Export : Register ", System.Reflection.MethodBase.GetCurrentMethod().Name, data.BW.WorkOrder_ID, "success"));

                            
                            return File(stream, "application/pdf", "register.pdf");
                        }

                        else if (data.BW.Type_Work == "G")//ธรรมดา
                        {

                            if(data.BW.date_print != null && data.BW.date_print != DateTime.MinValue)
                            {
                                var printdate2 = (from a in db.C_MCT_BILL_WORKORDER
                                                  where a.WorkOrder_ID == data.BW.WorkOrder_ID
                                                  select a).FirstOrDefault();

                                printdate2.print_date = data.BW.date_print;
                                db.SaveChanges();
                            }
                            ReportDocument rd = new ReportDocument();
                            rd.Load(Path.Combine(Server.MapPath("~/Report"), "PostofficeNormal.rpt"));

                            rd.SetParameterValue("@checkbox", data.BW.WorkOrder_ID);

                            rd.SetDatabaseLogon("sa", "Wge123456*", "SVERP01", "DBS_WGE");

                            Response.Buffer = false;
                            Response.ClearContent();
                            Response.ClearHeaders();
                            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                            stream.Seek(0, SeekOrigin.Begin);


                            rd.Close();
                            rd.Dispose();

                            log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "Export : Normal ", System.Reflection.MethodBase.GetCurrentMethod().Name, data.BW.WorkOrder_ID, "success"));

                            return File(stream, "application/pdf", "Normal.pdf");
                        }
                        return RedirectToAction("Index", "Home");
                    }
                    else if (data.BW.button == "exportsum")
                    {
                            string tracking_first = data.BW.tracking_first;
                            string tracking_last = data.BW.tracking_last;
                            var tracking = (from a in db.C_MCT_BILL_WORKORDER
                                                  where a.WorkOrder_ID == data.BW.WorkOrder_ID
                                                  select a).FirstOrDefault();
                            tracking.tracking_first = tracking_first;
                            tracking.tracking_last = tracking_last;
                            db.SaveChanges();
                            if (data.BW.tracking_first == null)
                            {
                                tracking_first = "";
                                
                            }
                            if(data.BW.tracking_last==null)
                            {
                                tracking_last = "";
                            }
                               
                            string typework = "";
                            string typework2 = "";
                            if (data.BW.Type_Work == "E")
                            {
                                typework = "ems";
                                typework2 = "emsar";
                            }
                            else if (data.BW.Type_Work == "U")
                            {
                                typework = "register";
                            }
                            else if (data.BW.Type_Work == "G")
                            {
                                typework = "normal";
                            }
                            var ems = (from a in db.C_MCT_BILL_WORKORDER
                                       join b in db.C_MCT_BILL_POSTOFFICE on a.WorkOrder_ID equals b.AC_code
                                       join c in db.C_MCT_POSTOFFICE_REF on b.ID.ToString() equals c.post_success_id
                                       where c.post_round == data.BW.round && c.post_mtyear == data.BW.mtyear && (c.post_type == typework || c.post_type == typework2)
                                       select new C_MCT_BILL_WORKORDER2
                                       {
                                           Type = b.Type,
                                           price= b.Price??0,
                                           round=a.round
                                           
                                       }).ToList();
                            var ems2 = (from a in db.C_MCT_BILL_WORKORDER
                                       join b in db.C_MCT_BILL_POSTOFFICE on a.WorkOrder_ID equals b.AC_code
                                       join c in db.C_MCT_POSTOFFICE_REF on b.ID.ToString() equals c.post_success_id
                                       where (c.post_round > 0 && c.post_round< data.BW.round) && c.post_mtyear == data.BW.mtyear && (c.post_type == typework || c.post_type == typework2)
                                       select new C_MCT_BILL_WORKORDER2
                                       {
                                           Type = b.Type,
                                           price = b.Price ?? 0,
                                           round = a.round

                                       }).ToList();
                            decimal ems_sumprice = 0;
                            decimal emsar_sumprice = 0;
                            decimal normal = 0;
                            decimal register = 0;
                            decimal ems_early = 0;
                            int ems_count = 0;
                            int emsar_count = 0;
                            int register_count = 0;
                            int normal_count = 0;
                            foreach (var sumems in ems)
                            {
                                decimal a = sumems.price;
                                if (sumems.Type == "ems")
                                {
                                    ems_sumprice += a;
                                    ems_count += 1;
                                }
                                else if (sumems.Type == "emsar")
                                {
                                    emsar_sumprice += a;
                                    emsar_count += 2;
                                }
                                else if (sumems.Type == "register")
                                {
                                    register += a;
                                    register_count += 1;
                                }
                                else if (sumems.Type == "normal")
                                {
                                    normal += a;
                                    normal_count += 1;
                                }
                            }
                            if(ems2 != null)
                            {
                                foreach(var sumems_early in ems2)
                                {
                                    decimal a = sumems_early.price;
                                    ems_early += a;
                                }
                            }
                            if (data.BW.print_date == null)
                            {
                                data.BW.print_date = DateTime.Now;
                            }
                            

                            ReportDocument rd = new ReportDocument();
                            rd.Load(Path.Combine(Server.MapPath("~/Report"), "Postelsum.rpt"));

                            rd.SetParameterValue("tracking_first", tracking_first);
                            rd.SetParameterValue("tracking_last", tracking_last);
                            rd.SetParameterValue("mtyear", data.BW.mtyear);
                            rd.SetParameterValue("round", data.BW.round);
                            rd.SetParameterValue("ems", ems_count.ToString());
                            rd.SetParameterValue("emsar", emsar_count.ToString());
                            rd.SetParameterValue("ems_price", ems_sumprice.ToString());
                            rd.SetParameterValue("emsar_price", emsar_sumprice.ToString());
                            rd.SetParameterValue("early", ems_early.ToString());
                            rd.SetParameterValue("sum_now", (ems_sumprice + emsar_sumprice+normal+register).ToString());
                            rd.SetParameterValue("sum_next", ((ems_sumprice + emsar_sumprice+normal+register) + ems_early).ToString());
                            rd.SetParameterValue("print_date", data.BW.print_date.ToString());
                            rd.SetParameterValue("normal", normal.ToString());
                            rd.SetParameterValue("register", register.ToString());
                            rd.SetParameterValue("normal_count", normal_count.ToString());
                            rd.SetParameterValue("register_count", register_count.ToString());
                            rd.SetParameterValue("sum", (ems_count + emsar_count+normal_count+register_count).ToString());
                            
                            Response.Buffer = false;
                            Response.ClearContent();
                            Response.ClearHeaders();

                            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                            stream.Seek(0, SeekOrigin.Begin);


                            rd.Close();
                            rd.Dispose();

                            log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "Export Sum :  ", System.Reflection.MethodBase.GetCurrentMethod().Name, data.BW.WorkOrder_ID, "success"));

                            return File(stream, "application/pdf", "Postelsum.pdf");
                    }
                    else if (data.BW.button == "roundoff")
                    {

                        string workid = "";
                        if (data.BW.Type_Work == "G" || data.BW.Type_Work == "E" || data.BW.Type_Work == "U")
                        {
                            string id = data.BW.WorkOrder_ID;
                            workid = db.C_MCT_BILL_WORKORDER.Where(z => z.WorkOrder_ID == id.ToString()).Select(x => x.WorkOrder_ID).FirstOrDefault();
                        }
                        else
                        {
                            int id = Convert.ToInt32(data.BW.ID);
                            workid = db.C_MCT_BILL_WORKORDER.Where(z => z.ID == id).Select(x => x.WorkOrder_ID).FirstOrDefault();
                        }

                        if (workid != null)
                        {
                            string status = data.BW.Status;
                            var oData = (from a in db.C_MCT_BILL_WORKORDER
                                         where a.WorkOrder_ID == workid
                                         select a).FirstOrDefault();

                            oData.Status = data.BW.Status;

                            if (data.BW.Status == "W")
                            {
                                oData.Recipient_Code = null;
                                oData.Recipient_Name = null;
                                oData.Recipient_Datetime = null;

                            }
                            if (data.BW.Type_Work == "G" || data.BW.Type_Work == "E" || data.BW.Type_Work == "U")
                            {
                                foreach (var postel in data.BP)
                                {


                                    var Postel1 = (from a in db.C_MCT_BILL_WORKORDER
                                                   where a.WorkOrder_ID == postel.WorkOrder_ID
                                                   select a).FirstOrDefault();
                                    var PostelDetail = (from a in db.C_MCT_BILL_POSTOFFICE
                                                        where a.ID == postel.ID
                                                        select a).FirstOrDefault();
                                    if (data.BW.Status == "P")
                                    {
                                        Postel1.Status = "P";
                                        Postel1.Recipient_Code = Session["Code"].ToString();
                                        Postel1.Recipient_Name = Session["Name"].ToString();
                                        Postel1.Recipient_Datetime = DateTime.Now;

                                        if (PostelDetail != null)
                                        {
                                            PostelDetail.Ck_print = "P";
                                        }
                                    }
                                    else if (data.BW.Status == "C")
                                    {

                                        Postel1.Status = "C";
                                        Postel1.Recipient_Code = Session["Code"].ToString();
                                        Postel1.Recipient_Name = Session["Name"].ToString();
                                        Postel1.Recipient_Datetime = DateTime.Now;

                                        if (PostelDetail != null)
                                        {
                                            PostelDetail.Ck_print = "C";
                                        }
                                    }
                                    else if (data.BW.Status == "W")
                                    {
                                        Postel1.Status = "W";
                                        Postel1.Recipient_Code = null;
                                        Postel1.Recipient_Name = null;
                                        Postel1.Recipient_Datetime = null;

                                        if (PostelDetail != null)
                                        {
                                            PostelDetail.Ck_print = null;
                                        }
                                    }
                                    else if (data.BW.Status == "F")
                                    {
                                        Postel1.Status = "A";
                                        Postel1.Recipient_Code = null;
                                        Postel1.Recipient_Name = null;
                                        Postel1.Recipient_Datetime = null;

                                        if (PostelDetail != null)
                                        {
                                            PostelDetail.Ck_print = "Y";
                                        }
                                    }
                                    db.SaveChanges();

                                }
                            }

                            db.SaveChanges();
                            /*
                            if(data.BW.Status == "P")
                                sendemail(oData.ID, "finishjob").ConfigureAwait(false);
                            else if(data.BW.Status == "C")
                                sendemail(oData.ID, "rejectjob").ConfigureAwait(false);
                            */
                            log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "RoundOff: ", System.Reflection.MethodBase.GetCurrentMethod().Name, data.BW.WorkOrder_ID, "success"));

                            TempData["msg"] = "<script>alert('Success!!');</script>";

                            //Session["Data"] = null;
                            //Session["Data"] = id;

                            if (data.BW.Type_Work == "P")
                            {
                                return RedirectToAction("WorkListPostel");
                            }
                            else
                            {
                                return RedirectToAction("WorkList");
                            }
                        }
                        else
                        {
                            TempData["msg"] = "<script>alert('Error!!');</script>";
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else if (data.BW.button == "updateprice")
                    {
                        C_MCT_BILL_WORKORDER data_billworkorder = new C_MCT_BILL_WORKORDER();
                        C_MCT_BILL_GENERAL data_billgeneral = new C_MCT_BILL_GENERAL();
                        C_MCT_BILL_BANK data_billbank = new C_MCT_BILL_BANK();
                        C_MCT_BILL_POSTOFFICE data_billpostoffice = new C_MCT_BILL_POSTOFFICE();

                        int id = Convert.ToInt32(data.BW.ID);
                        DateTime dt = DateTime.ParseExact(data.BW.date + " " + data.BW.time, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                        string workid = db.C_MCT_BILL_WORKORDER.Where(z => z.ID == id).Select(x => x.WorkOrder_ID).FirstOrDefault();
                        var oData = (from a in db.C_MCT_BILL_WORKORDER
                                     where a.WorkOrder_ID == workid
                                     select a).FirstOrDefault();

                        var oData2 = (from a in db.C_MCT_BILL_GENERAL
                                      where a.WorkOrder_ID == workid
                                      select a).FirstOrDefault();
                        
                        if (workid != null)
                        {

                            oData.Last_Datetime = DateTime.Now;
                            oData.Type_Work = data.BW.Type_Work;
                            oData.Remark = data.BW.Remark;
                            oData.RemarkAdmin = data.BW.RemarkAdmin;

                            db.SaveChanges();
                            
                            if (data.BW.Type_Work == "P" || data.BW.Type_Work == "G" || data.BW.Type_Work == "E" || data.BW.Type_Work == "U")
                            {

                                C_MCT_POSTOFFICE_REF data_POSTOFFICE_Report = new C_MCT_POSTOFFICE_REF();


                                decimal d = 0;
                                foreach (var oitem in data.BP)
                                {
                                    if (oitem.Name != null)
                                    {
                                        decimal a = oitem.Price ?? 0;
                                        d += a;
                                    }
                                }
                                int typenull = 0;
                                foreach (var checktypenull in data.BP)
                                {
                                    if (checktypenull.Type == null)
                                    {
                                        typenull += 1;
                                    }

                                }
                                if (typenull > 0)
                                {
                                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "updateprice typenull > 0 : ", System.Reflection.MethodBase.GetCurrentMethod().Name, data.BW.WorkOrder_ID, "Error"));

                                    TempData["msg"] = "<script>alert('โปรดเลือกประเภทงานก่อนบันทึก!');</script>";
                                    return RedirectToAction("WorkListPostel", "GeoCoding");

                                }
                                var oData7 = (from a in db.C_MCT_BILL_WORKORDER
                                              where a.WorkOrder_ID == workid
                                              select a).FirstOrDefault();

                                foreach (var item in data.BP)
                                {
                                    if (item.Name != null && item.Type != "")
                                    {
                                        var oData9 = (from a in db.C_MCT_BILL_POSTOFFICE
                                                      where a.ID == item.ID
                                                      select a).FirstOrDefault();
                                        if (oData9 != null)
                                        {
                                            oData9.Type = item.Type;
                                            oData9.Postel = item.Postel;
                                            oData9.Name = item.Name;
                                            oData9.Itemcode = item.Itemcode;
                                            oData9.EMSRev = item.EMSRev;
                                            
                                            if (item.Price == 0 || item.Price == null)
                                            {
                                                oData9.Price = null;
                                                
                                            }
                                            else
                                            {
                                                oData9.Price = item.Price;
                                            }
                                        }
                                        else if (oData9 == null)
                                        {
                                            if (data.BW.Type_Work == "P")
                                            {
                                                data_billpostoffice.WorkOrder_ID = oData7.WorkOrder_ID;
                                                data_billpostoffice.Type = item.Type;
                                                data_billpostoffice.Name = item.Name;
                                                data_billpostoffice.Postel = item.Postel;
                                                db.C_MCT_BILL_POSTOFFICE.Add(data_billpostoffice);
                                            }
                                        }

                                        var oData10 = (from a in db.C_MCT_POSTOFFICE_REF
                                                       where a.post_success_id == item.ID.ToString()
                                                       select a).FirstOrDefault();
                                        if (oData10 != null)
                                        {
                                            oData10.post_type = item.Type;
                                            oData10.post_name = item.Name;
                                            oData10.post_zip = item.Postel;
                                            oData10.post_ems = item.Itemcode;
                                            oData10.post_emsrev = item.EMSRev;
                                            oData10.post_round = oData7.round;
                                            oData10.post_price = item.Price;
                                            oData10.post_date = DateTime.Now;
                                            oData10.post_sumprice = d;
                                            oData10.post_mtyear = oData7.mtyear;
                                            oData10.post_success_id = item.ID.ToString();
                                        }

                                        db.SaveChanges();

                                    }
                                }

                                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "Updateprice :", System.Reflection.MethodBase.GetCurrentMethod().Name, data.BW.WorkOrder_ID, "success"));

                                TempData["msg"] = "<script>alert('Success!!');</script>";
                                return RedirectToAction("WorkList");
                            }


                            if (data.BW.Type_Work == "P")
                            {
                                return RedirectToAction("WorkListPostel");
                            }
                            else
                            {
                                return RedirectToAction("WorkList");
                            }
                        }
                        else
                        {
                            log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "Export : Register Updateprice : null Workid", System.Reflection.MethodBase.GetCurrentMethod().Name, data.BW.WorkOrder_ID, "Error"));

                            TempData["msg"] = "<script>alert('Error!!');</script>";
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else if(data.BW.button == "updatedev")
                    {
                        C_MCT_BILL_WORKORDER data_billworkorder = new C_MCT_BILL_WORKORDER();
                        C_MCT_BILL_GENERAL data_billgeneral = new C_MCT_BILL_GENERAL();
                        C_MCT_BILL_BANK data_billbank = new C_MCT_BILL_BANK();
                        C_MCT_BILL_POSTOFFICE data_billpostoffice = new C_MCT_BILL_POSTOFFICE();

                        int id = Convert.ToInt32(data.BW.ID);
                        DateTime dt = DateTime.ParseExact(data.BW.date + " " + data.BW.time, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                        string workid = db.C_MCT_BILL_WORKORDER.Where(z => z.ID == id).Select(x => x.WorkOrder_ID).FirstOrDefault();
                        var oData = (from a in db.C_MCT_BILL_WORKORDER
                                     where a.WorkOrder_ID == workid
                                     select a).FirstOrDefault();

                        var oData2 = (from a in db.C_MCT_BILL_GENERAL
                                      where a.WorkOrder_ID == workid
                                      select a).FirstOrDefault();
                        string path3 = string.Empty;


                        if (workid != null)
                        {
                            if (Session["Position"].ToString() == "8" || Session["Position"].ToString() == "10")
                            {
                                if (data.BW.jobtitle == "Food Industry")
                                {
                                    oData.Commander_Department = "Food Industry";
                                    oData.Commander_JobTitle = "Food Industry";
                                }
                                else if (data.BW.jobtitle == "Food Service")
                                {
                                    oData.Commander_Department = "Food Service";
                                    oData.Commander_JobTitle = "Food Service";
                                }
                                else if (data.BW.jobtitle == "Event & Promotions")
                                {
                                    oData.Commander_Department = "Modern Trade";
                                    oData.Commander_JobTitle = "Event & Promotions";
                                }
                                else if (data.BW.jobtitle == "Merchandiser")
                                {
                                    oData.Commander_Department = "Modern Trade";
                                    oData.Commander_JobTitle = "Merchandiser";
                                }
                                else if (data.BW.jobtitle == "Key Account")
                                {
                                    oData.Commander_Department = "Modern Trade";
                                    oData.Commander_JobTitle = "Key Account";
                                }
                                else if (data.BW.jobtitle == "Marketting")
                                {
                                    oData.Commander_Department = "Product Management";
                                    oData.Commander_JobTitle = "Marketting";
                                }
                                else if (data.BW.jobtitle == "Product Specialist & PR")
                                {
                                    oData.Commander_Department = "Business Development";
                                    oData.Commander_JobTitle = "Product Specialist & PR";
                                }
                                else if (data.BW.jobtitle == "Delice")
                                {
                                    oData.Commander_Department = "Business Development";
                                    oData.Commander_JobTitle = "Delice";
                                }
                                else if (data.BW.jobtitle == "Food Solutions")
                                {
                                    oData.Commander_Department = "Food Solutions";
                                    oData.Commander_JobTitle = "Food Solutions";
                                }
                                else if (data.BW.jobtitle == "Sales Support")
                                {
                                    oData.Commander_Department = "Sales Support";
                                    oData.Commander_JobTitle = "Sales Support";
                                }
                                else if (data.BW.jobtitle == "Production")
                                {
                                    oData.Commander_Department = "Production";
                                    oData.Commander_JobTitle = "Production";
                                }
                                else if (data.BW.jobtitle == "Executive Management")
                                {
                                    oData.Commander_Department = "Executive Management";
                                    oData.Commander_JobTitle = "Executive Management";
                                }
                                else if (data.BW.jobtitle == "Procurement")
                                {
                                    oData.Commander_Department = "Procurement";
                                    oData.Commander_JobTitle = "Procurement";
                                }
                                else if (data.BW.jobtitle == "Warehouse")
                                {
                                    oData.Commander_Department = "Warehouse";
                                    oData.Commander_JobTitle = "Warehouse";
                                }
                                else if (data.BW.jobtitle == "IT")
                                {
                                    oData.Commander_Department = "IT";
                                    oData.Commander_JobTitle = "IT";
                                }
                                else if (data.BW.jobtitle == "General Admin")
                                {
                                    oData.Commander_Department = "General Admin";
                                    oData.Commander_JobTitle = "General Admin";
                                }
                                else if (data.BW.jobtitle == "Human Resources")
                                {
                                    oData.Commander_Department = "Human Resources";
                                    oData.Commander_JobTitle = "Human Resources";
                                }
                                else if (data.BW.jobtitle == "Corporate Accounting")
                                {
                                    oData.Commander_Department = "Corporate Accounting";
                                    oData.Commander_JobTitle = "Corporate Accounting";
                                }
                                else if (data.BW.jobtitle == "Corporate Finance")
                                {
                                    oData.Commander_Department = "Corporate Finance";
                                    oData.Commander_JobTitle = "Corporate Finance";
                                }
                            }
                            else
                            {
                                oData.Commander_Department = data.BW.Commander_Department;
                                oData.Commander_JobTitle = data.BW.jobtitle;
                            }
                            oData.Remark = data.BW.Remark;
                            oData.RemarkAdmin = data.BW.RemarkAdmin;

                            db.SaveChanges();
                            
                            TempData["msg"] = "<script>alert('Success!!');</script>";
                            return RedirectToAction("WorkListPostel");
                        }

                        TempData["msg"] = "<script>alert('Success!!');</script>";


                        if (data.BW.Type_Work == "P")
                        {
                            return RedirectToAction("WorkListPostel");
                        }
                        else
                        {
                            return RedirectToAction("WorkList");
                        }
                    }

                    else
                    {
                        log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "getWorkOrder data.BW.button is null :", System.Reflection.MethodBase.GetCurrentMethod().Name, data.BW.WorkOrder_ID, "Error"));

                        TempData["msg"] = "<script>alert('Error!!');</script>";
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = "Error";
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                return RedirectToAction("Index", "Home");
            }

        }

        public ActionResult WorkList()
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                mWGE mData = new mWGE();

                using (var db = new DBS_WGE_Entities())
                {

                    if (Session["Name"] != null && (Session["Position"].ToString() == "8" || Session["Position"].ToString() == "9"))
                    {
                        
                        var oData = (from a in db.C_MCT_BILL_WORKORDER
                                     where (a.Status == "W" || a.Status == "G" || a.Status == "A"||a.Status=="F") && (a.Type_Work != "P") && (a.Work_ID != null)
                                     select a).OrderByDescending(z => z.Work_ID).ToList();

                        var oData2 = (from a in db.C_MCT_BILL_WORKORDER
                                      where (a.Status == "P" || a.Status == "C") && (a.Type_Work != "P") && (a.Work_ID !=null)
                                      select a).OrderByDescending(z => z.Work_ID).ToList();

                        if (oData.Count() > 0)
                        {
                            mData.ListBW = oData;
                        }

                        if (oData2.Count() > 0)
                        {
                            mData.ListBW1 = oData2;
                        }

                        return View(mData);
                    }
                    else
                    {
                        string code = Session["Code"].ToString();

                        var oData = (from a in db.C_MCT_BILL_WORKORDER
                                     where a.Commander_Code == code && (a.Status == "W" || a.Status == "G" || a.Status=="A" || a.Status == "F") && (a.Type_Work != "P")
                                     select a).OrderByDescending(z => z.ID).ToList();

                        var oData2 = (from a in db.C_MCT_BILL_WORKORDER
                                      where a.Commander_Code == code && (a.Status == "P" || a.Status == "C") && (a.Type_Work != "P")
                                      select a).OrderByDescending(z => z.ID).ToList();

                        if (oData.Count() > 0)
                        {
                            mData.ListBW = oData;
                        }

                        if (oData2.Count() > 0)
                        {
                            mData.ListBW1 = oData2;
                        }

                        return View(mData);
                    }

                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('Error!!');</script>";
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public ActionResult WorkOrderPostel()
        {
            mWGE mData = new mWGE();

            if (Session["Data"] != null)
            {
                string Data1 = Session["Data"].ToString();
                Session["Data"] = null;
                return WorkOrderPostel(Data1);
            }
            else
            {
                string id = null;
                return WorkOrderPostel(id);
            }

        }

        public ActionResult Postelget(string id)
        {
            return RedirectToAction("WorkOrderPostel2","GeoCoding",new { id = id });
        }

        public ActionResult WorkOrderPostel2(string id)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                mWGE mData = new mWGE();
                using (var db = new DBS_WGE_Entities())
                {
                    if (id != null)
                    {
                        

                        var oData = (from a in db.C_MCT_BILL_WORKORDER
                                     where a.WorkOrder_ID == id 
                                     select new C_MCT_BILL_WORKORDER2
                                     {
                                         Commander_Code = a.Commander_Code,
                                         Commander_Name = a.Commander_Name,
                                         Commander_Department = a.Commander_Department,
                                         Commander_Datetime = a.Commander_Datetime,
                                         Contact_Name = a.Contact_Name,
                                         Recipient_Datetime = a.Recipient_Datetime,
                                         Recipient_Code = a.Recipient_Code,
                                         Recipient_Name = a.Recipient_Name,
                                         Remark = a.Remark,
                                         Contact_Tel = a.Contact_Tel,
                                         ID = a.ID,
                                         Status = a.Status,
                                         Type_Work = a.Type_Work,
                                         RemarkAdmin = a.RemarkAdmin,
                                         round=a.round,
                                         mtyear=a.mtyear,
                                         print_date=a.print_date,
                                         tracking_first=a.tracking_first,
                                         tracking_last=a.tracking_last
                                     }).ToList();

                        if (oData.Count() > 0)
                        {
                            mData.ListBW2 = oData;
                        }

                        var oData2 = (from a in db.OUDP
                                      select a).ToList();

                        if (oData2.Count() > 0)
                        {
                            mData.ListOUDP = oData2;
                        }

                        string workid = db.C_MCT_BILL_WORKORDER.Where(w => w.WorkOrder_ID == id).Select(z => z.WorkOrder_ID).FirstOrDefault();

                        if (workid != null)
                        {
                            var oData3 = (from a in db.C_MCT_BILL_POSTOFFICE
                                          where a.AC_code == workid orderby a.Itemcode
                                          select a).ToList();

                            if (oData3.Count() > 0)
                            {
                                mData.ListPO1 = oData3;
                            }
                        }

                        string type_work = db.C_MCT_BILL_WORKORDER.Where(w => w.WorkOrder_ID == id).Select(z => z.Type_Work).FirstOrDefault();

                        if (type_work != null)
                        {

                            mData.type_wk =type_work;
                        }
                        string workorder = db.C_MCT_BILL_WORKORDER.Where(w => w.WorkOrder_ID == id).Select(z => z.WorkOrder_ID).FirstOrDefault();

                        if (workorder != null)
                        {
                            mData.Workorder = workorder;
                        }

                        var checkview = (from a in db.C_MCT_BILL_WORKORDER
                                         where a.WorkOrder_ID == id
                                         select a).FirstOrDefault();

                        if (Session["Code"].ToString() == "37001")
                        {
                            checkview.MS_1 = "Y";
                        }
                        else if (Session["Code"].ToString() == "47003")
                        {
                            checkview.MS_2 = "Y";
                        }
                        else if (Session["Code"].ToString() == "58010")
                        {
                            checkview.MS_3 = "Y";
                        }
                        else if (Session["Code"].ToString() == "61002")
                        {
                            checkview.MS_4 = "Y";
                        }

                        db.SaveChanges();

                        var checkview2 = (from a in db.C_MCT_BILL_WORKORDER
                                         where a.WorkOrder_ID == id
                                         select a).FirstOrDefault();

                        mData.MS_1 = checkview2.MS_1;
                        mData.MS_2 = checkview2.MS_2;
                        mData.MS_3 = checkview2.MS_3;
                        mData.MS_4 = checkview2.MS_4;

                        log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "View WorkOrder ", System.Reflection.MethodBase.GetCurrentMethod().Name, checkview2.Work_ID, "success"));
                        return View(mData);
                    }
                    else
                    {
                        var oData2 = (from a in db.OUDP
                                      select a).ToList();

                        mData.ListBW2 = null;
                        mData.date = DateTime.Now;

                        if (oData2.Count() > 0)
                        {
                            mData.ListOUDP = oData2;
                        }

                        return View(mData);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('Error!');</script>";
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult WorkOrderPostel(string id)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                mWGE mData = new mWGE();
                using (var db = new DBS_WGE_Entities())
                {
                    if (id != null)
                    {
                        int x = Convert.ToInt32(id);

                        var oData = (from a in db.C_MCT_BILL_WORKORDER
                                     where a.ID == x
                                     select new C_MCT_BILL_WORKORDER2
                                     {
                                         Commander_Code = a.Commander_Code,
                                         Commander_Name = a.Commander_Name,
                                         Commander_Department = a.Commander_Department,
                                         Commander_Datetime = a.Commander_Datetime,
                                         Contact_Name = a.Contact_Name,
                                         Recipient_Datetime = a.Recipient_Datetime,
                                         Recipient_Code = a.Recipient_Code,
                                         Recipient_Name = a.Recipient_Name,
                                         Remark = a.Remark,
                                         Contact_Tel = a.Contact_Tel,
                                         ID = a.ID,
                                         Status = a.Status,
                                         Type_Work = a.Type_Work,
                                         RemarkAdmin = a.RemarkAdmin,
                                         Last_Datetime=a.Last_Datetime
                                     }).ToList();

                        if (oData.Count() > 0)
                        {
                            mData.ListBW2 = oData;
                        }

                        var oData2 = (from a in db.OUDP
                                      select a).ToList();

                        if (oData2.Count() > 0)
                        {
                            mData.ListOUDP = oData2;
                        }
                      
                        string workid = db.C_MCT_BILL_WORKORDER.Where(w => w.ID == x).Select(z => z.WorkOrder_ID).FirstOrDefault();

                        if (workid != null)
                        {
                            var oData3 = (from a in db.C_MCT_BILL_POSTOFFICE
                                          where a.WorkOrder_ID == workid
                                          select a).ToList();

                            if (oData3.Count() > 0)
                            {
                                mData.ListPO1 = oData3;
                            }
                        }

                        return View(mData);
                    }
                    else
                    {
                        var oData2 = (from a in db.OUDP
                                      select a).ToList();

                        mData.ListBW2 = null;
                        mData.date = DateTime.Now;

                        if (oData2.Count() > 0)
                        {
                            mData.ListOUDP = oData2;
                        }

                        return View(mData);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('Error!');</script>";
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult WorkListPostel()
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                mWGE mData = new mWGE();

                using (var db = new DBS_WGE_Entities())
                {

                    if (Session["Name"] != null && (Session["Position"].ToString() == "8" || Session["Position"].ToString() == "10" ))
                    {
                        var oData = (from a in db.C_MCT_BILL_WORKORDER
                                     where (a.Status == "W" || a.Status == "G" || a.Status == "P" || a.Status == "C" || a.Status == "A") && (a.Type_Work == "P")
                                     select a).OrderByDescending(z => z.ID).ToList();

                        var oData2 = (from a in db.C_MCT_BILL_WORKORDER
                                      where (a.Status == "F" || a.Status == "C") && (a.Type_Work == "P")
                                      select a).OrderByDescending(z => z.ID).ToList();

                        if (oData.Count() > 0)
                        {
                            mData.ListBW = oData;
                        }

                        if (oData2.Count() > 0)
                        {
                            mData.ListBW1 = oData2;
                        }

                        return View(mData);
                    }
                    else
                    {
                        string code = Session["Code"].ToString();

                        var oData = (from a in db.C_MCT_BILL_WORKORDER
                                     where a.Commander_Code == code && (a.Status == "W" || a.Status == "G" || a.Status == "K" || a.Status == "P" || a.Status == "C" ) && (a.Type_Work == "P")
                                     select a).OrderByDescending(z => z.ID).ToList();

                        var oData2 = (from a in db.C_MCT_BILL_WORKORDER
                                      where a.Commander_Code == code && (a.Status == "F" || a.Status == "C" || a.Status == "K") && (a.Type_Work == "P")
                                      select a).OrderByDescending(z => z.ID).ToList();

                        if (oData.Count() > 0)
                        {
                            mData.ListBW = oData;
                        }

                        if (oData2.Count() > 0)
                        {
                            mData.ListBW1 = oData2;
                        }

                        return View(mData);
                    }

                }
            }


            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('Error!!');</script>";
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                return RedirectToAction("Index", "Home");
            }
        }

      
        public ActionResult PostelAdmin()
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                mWGE mData = new mWGE();

                using (var db = new DBS_WGE_Entities())
                {
                    if (Session["Name"] != null && (Session["Position"].ToString() == "8" || Session["Position"].ToString() == "10"))
                    {
                        mData.datefrom = DateTime.Now;
                        mData.dateto = DateTime.Now;

                        return View(mData);
                    }
                    else
                        return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('Error!!');</script>";
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                return RedirectToAction("Index", "Home");
            }
        }


        [HttpPost]
        public ActionResult PostelAdmin(mWGE Data,DateTime datefrom, DateTime dateto, string checkbox, string button1,int round)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using (var db = new DBS_WGE_Entities())
                {

                    mWGE mData = new mWGE();
                    if (Session["Name"] != null && (Session["Position"].ToString() == "8" || Session["Position"].ToString() == "10"))
                    {
                        //สำหรับแสดงข้อมูล
                        if (button1 == "search" )
                        {
                            DateTime mtyr = DateTime.Now;
                            int mt = mtyr.Month;
                            int yr = mtyr.Year;
                            string id = mt + "/" + yr;
                            string type_wk = "";
                            if (checkbox == "ems")
                            {
                                type_wk = "E";
                            }
                            else if (checkbox == "register")
                            {
                                type_wk = "U";
                            }
                            else if (checkbox == "normal")
                            {
                                type_wk = "G";
                            }

                            if (checkbox == "ems")
                            {
                                
                                var dateadd = dateto.AddDays(1);
                                var oData = (from a in db.C_MCT_BILL_WORKORDER
                                             join b in db.C_MCT_BILL_POSTOFFICE on a.WorkOrder_ID equals b.WorkOrder_ID into bb
                                             from b in bb.DefaultIfEmpty()
                                             where !(from o in db.C_MCT_POSTOFFICE_REF select o.post_success_id).Contains(b.ID.ToString()) &&
                                             (a.Type_Work == "P" || a.Type_Work == "G" || a.Type_Work == "U" || a.Type_Work == "E") && a.Commander_Datetime >= datefrom && 
                                             a.Commander_Datetime <= dateadd && 
                                             (b.Type == "ems" || b.Type == "emsar") && (a.Status == "P" || a.Status == "W")
                                             select new C_MCT_BILL_WORKORDER2
                                             {
                                                 ID=a.ID,
                                                 ID_detail=b.ID,
                                                 WorkOrder_ID = a.WorkOrder_ID,
                                                 Commander_Name = a.Commander_Name,
                                                 Commander_Department = a.Commander_Department,
                                                 Name = b.Name,
                                                 Type = b.Type,
                                                 Postel = b.Postel,
                                                 Itemcode = b.Itemcode,
                                                 price = b.Price ?? 0,
                                                 Commander_Datetime = a.Commander_Datetime,
                                                 EMSRev = b.EMSRev
                                             }).OrderBy(z => z.Type );

                                if (oData != null)
                                {
                                    mData.datefrom = datefrom;
                                    mData.dateto = dateto;
                                    mData.checkbox = checkbox;
                                    mData.ListBW2 = oData.ToList();
                                }
                                var r = (from a in db.C_MCT_BILL_WORKORDER
                                         where a.mtyear ==id 
                                         orderby a.round descending
                                         select a).FirstOrDefault();
                                if (r != null)
                                {
                                    if (r.Status == "G" || r.Status == "P" || r.Status == "C" || r.Status == "F")
                                    {
                                        mData.round = r.round + 1 ?? 0;
                                    }
                                    else if (r.Status == "W")
                                    {
                                        mData.round = r.round ?? 0;
                                    }
                                }
                                else if (r == null)
                                {
                                    mData.round = 1;
                                }
                            }
                            else
                            {
                                var dateadd = dateto.AddDays(1);
                                var oData = (from a in db.C_MCT_BILL_WORKORDER
                                             join b in db.C_MCT_BILL_POSTOFFICE on a.WorkOrder_ID equals b.WorkOrder_ID into bb
                                             from b in bb.DefaultIfEmpty()
                                             where !(from o in db.C_MCT_POSTOFFICE_REF select o.post_success_id).Contains(b.ID.ToString()) && 
                                             (a.Type_Work == "P" || a.Type_Work=="G"||a.Type_Work=="U"||a.Type_Work=="E") && a.Commander_Datetime >= datefrom &&
                                             a.Commander_Datetime <= dateadd && checkbox.Contains(b.Type) && 
                                             (a.Status == "P" || a.Status == "W")
                                             select new C_MCT_BILL_WORKORDER2
                                             {
                                                 ID=a.ID,
                                                 ID_detail=b.ID,
                                                 WorkOrder_ID = a.WorkOrder_ID,
                                                 Commander_Name = a.Commander_Name,
                                                 Commander_Department = a.Commander_Department,
                                                 Name = b.Name,
                                                 Type = b.Type,
                                                 Postel = b.Postel,
                                                 Itemcode = b.Itemcode,
                                                 price = b.Price ?? 0,
                                                 Commander_Datetime = a.Commander_Datetime
                                             }).OrderBy(z => z.ID_detail);

                                if (oData != null)
                                {
                                    mData.datefrom = datefrom;
                                    mData.dateto = dateto;
                                    mData.checkbox = checkbox;
                                    mData.ListBW2 = oData.ToList();
                                }
                                var r = (from a in db.C_MCT_BILL_WORKORDER
                                         where a.mtyear == id 
                                         orderby a.round descending
                                         select a).FirstOrDefault();
                                if (r != null)
                                {
                                    if (r.Status == "G" || r.Status == "P" || r.Status == "C"||r.Status=="F")
                                    {
                                        mData.round = r.round + 1 ?? 0;
                                    }
                                    else if (r.Status == "W")
                                    {
                                        mData.round = r.round ?? 0;
                                    }
                                }
                                else if (r == null)
                                {
                                    mData.round = 1;
                                }
                            }
                            
                            return View(mData);
                        }
                        //สำหรับบันทึกข้อมูล
                        else if (button1 == "A")
                        {
                            C_MCT_BILL_WORKORDER data_billworkorder = new C_MCT_BILL_WORKORDER();
                            C_MCT_BILL_GENERAL data_billgeneral = new C_MCT_BILL_GENERAL();
                            C_MCT_BILL_BANK data_billbank = new C_MCT_BILL_BANK();
                            C_MCT_BILL_POSTOFFICE data_billpostoffice = new C_MCT_BILL_POSTOFFICE();
                            C_MCT_POSTOFFICE_REF data_POSTOFFICE_Report = new C_MCT_POSTOFFICE_REF();
                            DateTime mtyr = DateTime.Now;
                            int mt = mtyr.Month;
                            int yr = mtyr.Year;
                            string id = mt+"/"+ yr;
                            short dept = Convert.ToInt16(Session["Department"]);
                            string ID = "";

                            if (Data.BP != null)
                            {
                                //ดึงชื่อแผนกจาก SAP
                                var oData2 = (from a in db.OUDP
                                          select a).ToList();
                            
                            string namedepartment = (oData2 != null && oData2.Count() > 0) ? oData2 .Where(z => z.Code == dept).Select(x => x.Name).FirstOrDefault() : string.Empty;
                            //ดึงชื่อแผนกจาก SAP

                            string type_wk = "";
                            if (checkbox == "ems")
                            {
                                type_wk = "E";
                            }
                            else if (checkbox == "register")
                            {
                                type_wk = "U";
                            }
                            else if (checkbox == "normal")
                            {
                                type_wk = "G";
                            }
                            if (round >= 1 && round <= 20)
                            {
                                var oData4 = (from a in db.C_MCT_BILL_WORKORDER
                                              where a.round == round && a.mtyear == id && a.Type_Work == type_wk
                                              select a).FirstOrDefault();
                                if (oData4 != null)
                                {
                                    if (oData4.Status == "G" || oData4.Status == "P" || oData4.Status == "C" || oData4.Status=="F")
                                    {

                                        TempData["msg"] = "<script>alert('มีการกดรับงานในรอบนี้แล้ว โปรดระบุรอบอื่น');</script>";
                                        return RedirectToAction("PostelAdmin", "GeoCoding");
                                    }
                                }

                                var oData3 = (from a in db.C_MCT_BILL_WORKORDER
                                          where a.round == round && a.mtyear == id && a.Type_Work == type_wk
                                          select a).FirstOrDefault();

                                if (oData3 != null)
                                {
                                    ID = oData3.WorkOrder_ID;
                                }
                                else
                                {
                                    ID = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                                
                                    data_billworkorder.WorkOrder_ID = ID;
                                    data_billworkorder.Commander_Code = Session["Code"].ToString();
                                    data_billworkorder.Commander_Name = Session["Name"].ToString();
                                    data_billworkorder.Commander_Department = namedepartment;
                                    data_billworkorder.Commander_Datetime = DateTime.Now;
                                    data_billworkorder.Contact_Name = "ไปรษณีย์กลาง";
                                    data_billworkorder.Contact_Tel = "026147455";
                                    if (checkbox == "ems")
                                    {
                                        data_billworkorder.Type_Work = "E";
                                    }
                                    else if (checkbox == "register")
                                    {

                                        data_billworkorder.Type_Work = "U";
                                    }
                                    else if (checkbox == "normal")
                                    {

                                        data_billworkorder.Type_Work = "G";
                                    }
                                    data_billworkorder.Remark = null;
                                    data_billworkorder.Status = "W";
                                    data_billworkorder.round = round;
                                    data_billworkorder.mtyear = id;

                                        

                                            var ID_Work = (from a in db.C_MCT_BILL_WORKORDER
                                                           where a.Type_Work != "P" && a.Work_ID != null
                                                           orderby a.Work_ID descending
                                                           select a).FirstOrDefault();
                                            data_billworkorder.Work_ID = ID_Work.Work_ID + 1;
                                        
                                        db.C_MCT_BILL_WORKORDER.Add(data_billworkorder);
                                    db.SaveChanges();
                                }
                            

                                    //รวมราคาจากปัจจุบัน
                                    decimal d = 0;
                                        foreach (var oitem in Data.BP)
                                        {
                                            decimal a = oitem.Price ?? 0;
                                            d += a;
                                        }
                                   

                                    //ดึงราคาจากรอบนั้นๆ
                                    var sumprice1 = (from a in db.C_MCT_POSTOFFICE_REF
                                                     where a.post_round == round && a.post_mtyear == id && a.post_type == checkbox
                                                     select a.post_sumprice).FirstOrDefault();
                                    //นำมาบวกกับยอดปัจจุบัน
                                    decimal sumprice = 0;
                                    if (sumprice1 != null)
                                    {
                                        sumprice = sumprice1.Value + d;
                                    }
                                    else
                                    {
                                        sumprice = d;
                                    }
                                    var sumprice2 = (from a in db.C_MCT_POSTOFFICE_REF
                                                     where a.post_round == round && a.post_mtyear == id && a.post_type == checkbox
                                                     select a).ToList();
                                    //อัพเดทราคาในรอบนั้นๆที่มีการบันทึกแล้ว
                                    if(sumprice2 != null)
                                    {
                                        foreach (var sum in sumprice2)
                                        {
                                            sum.post_sumprice = sumprice;
                                        }
                                    }
                                    if (checkbox == "ems")
                                    {
                                        var sumprice3 = (from a in db.C_MCT_POSTOFFICE_REF
                                                         where a.post_round == round && a.post_mtyear == id && a.post_type == "emsar"
                                                         select a).ToList();
                                        if (sumprice3 != null)
                                        {
                                            foreach (var sum2 in sumprice3)
                                            {
                                                sum2.post_sumprice = sumprice;
                                            }
                                        }
                                    }
                                    //อัพเดท status งานมีการสร้าง order จัดส่งแล้วไม่สามารถลบได้

                                    if(mt>=1 || mt <= 9)
                                    {
                                        
                                    }
                                    foreach (var item in Data.BP)
                                        {
                                            if (item.Name != null && item.Type != "")
                                            {

                                                var MCT_BILL_POSTOFFICE = (from a in db.C_MCT_BILL_POSTOFFICE
                                                                           where a.ID.ToString() == item.ps_code
                                                                           select a).FirstOrDefault();
                                                var MCT_BILL_WORKORDER = (from a in db.C_MCT_BILL_WORKORDER
                                                                          where a.WorkOrder_ID == MCT_BILL_POSTOFFICE.WorkOrder_ID
                                                                          select a).FirstOrDefault();
                                                
                                                 data_POSTOFFICE_Report.post_ems = item.Itemcode;
                                                 data_POSTOFFICE_Report.post_emsrev = item.EMSRev;
                                                 data_POSTOFFICE_Report.post_wk_id = item.WorkOrder_ID;
                                                 data_POSTOFFICE_Report.post_type = item.Type;
                                                 data_POSTOFFICE_Report.post_name = item.Name;
                                                 data_POSTOFFICE_Report.post_zip = item.Postel;
                                                
                                                 data_POSTOFFICE_Report.post_round = round;
                                                 data_POSTOFFICE_Report.post_price = item.Price;
                                                 data_POSTOFFICE_Report.post_date = DateTime.Now;
                                                 data_POSTOFFICE_Report.post_sumprice = sumprice;
                                                 data_POSTOFFICE_Report.post_mtyear = mt + "/" + yr;
                                            
                                                 MCT_BILL_POSTOFFICE.Name = item.Name;
                                                 MCT_BILL_POSTOFFICE.Itemcode = item.Itemcode;
                                                 MCT_BILL_POSTOFFICE.EMSRev = item.EMSRev;
                                                
                                                 MCT_BILL_POSTOFFICE.Ck_print = "Y";
                                                 MCT_BILL_POSTOFFICE.AC_code = ID;
                                                 data_POSTOFFICE_Report.post_success_id = MCT_BILL_POSTOFFICE.ID.ToString();
                                                 
                                                
                                                 MCT_BILL_WORKORDER.Last_Datetime = DateTime.Now;

                                                 db.C_MCT_POSTOFFICE_REF.Add(data_POSTOFFICE_Report);
                                                 db.SaveChanges();
                                            }
                                        }
                                }

                                dateto = DateTime.Now;
                                datefrom = DateTime.Now;
                                TempData["msg"] = "<script>alert('บันทึกสำเร็จ!!!');</script>";
                                return View(Data);

                            }
                            else if(round <1 || round > 20)
                            {
                                TempData["msg"] = "<script>alert('กรุณาใส่ค่าในช่อง ฝากส่งครั้งที่  ต้องไม่เท่ากับหรือน้อยกว่า 0 และ ไม่เกิน 20 !!');</script>";
                                return RedirectToAction("PostelAdmin", "GeoCoding");
                            }
                            else 
                            {
                                TempData["msg"] = "<script>alert('กรุณาค้นหาข้อมูล ก่อนกดบันทึก!!!');</script>";
                                return RedirectToAction("PostelAdmin", "GeoCoding");
                            }
                        }

                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                        return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('Error!!');</script>";
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                return RedirectToAction("Index", "Home");
            }
        }
        
        public ActionResult PostelAdminPrice()
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                mWGE mData = new mWGE();

                using (var db = new DBS_WGE_Entities())
                {
                    if (Session["Name"] != null && (Session["Position"].ToString() == "8" || Session["Position"].ToString() == "10"))
                    {
                        DateTime mtyr = DateTime.Now;
                        int mt = mtyr.Month;
                        int yr = mtyr.Year;
                        string m = mt + "/" + yr;
                        var r = (from a in db.C_MCT_BILL_WORKORDER
                                 where a.mtyear==m
                                 orderby a.round descending
                                 select a).FirstOrDefault();
                        if (r != null)
                        {
                            mData.round = r.round ?? 0;
                        }
                        else if (r == null)
                        {
                            mData.round = 0;
                        }

                        return View(mData);
                    }
                    else
                        return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('Error!!');</script>";
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                return RedirectToAction("Index", "Home");
            }
        }


        [HttpPost]
        public ActionResult PostelAdminPrice(mWGE Data,int checkround,string checkbox,string button1)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                
                using (var db = new DBS_WGE_Entities())
                {
                    mWGE mData = new mWGE();

                    if (Session["Name"] != null && (Session["Position"].ToString() == "8" || Session["Position"].ToString() == "10"))
                    {
                        if (button1 == "search")
                        {
                            DateTime mtyr = DateTime.Now;
                            int mt = mtyr.Month;
                            int yr = mtyr.Year;
                            string id = mt + "/" + yr;
                            var oData = (from a in db.C_MCT_BILL_WORKORDER
                                         join b in db.C_MCT_BILL_POSTOFFICE on a.WorkOrder_ID equals b.AC_code into bb
                                         from b in bb.DefaultIfEmpty()
                                         where a.round == checkround && a.mtyear == checkbox && b.ps_code != "Y"
                                         orderby b.Type, b.Itemcode
                                         select new C_MCT_BILL_WORKORDER2
                                         {
                                             ID = a.ID,
                                             ID_detail = b.ID,
                                             WorkOrder_ID = a.WorkOrder_ID,
                                             Commander_Name = a.Commander_Name,
                                             Commander_Department = a.Commander_Department,
                                             Name = b.Name,
                                             Type = b.Type,
                                             Postel = b.Postel,
                                             Itemcode = b.Itemcode,
                                             price = b.Price ?? 0,
                                             Commander_Datetime = a.Commander_Datetime,
                                             EMSRev = b.EMSRev,
                                             AC_Code = b.WorkOrder_ID,

                                         }).OrderBy(x => x.Itemcode);

                            int i = 0;
                            List<int> index = new List<int>();
                            foreach (var index1 in oData)
                            {
                                i += 1;
                                index.Add(i);
                            }
                            if (oData != null)
                            {
                                mData.N = index.ToList();
                                mData.round = checkround;
                               
                                mData.ListBW2 = oData.ToList();
                            }
                            return View(mData);
                        }
                        else if (button1 == "save")
                        {
                            foreach(var oitem in Data.BP)
                            {
                                var oData1 = (from a in db.C_MCT_BILL_POSTOFFICE
                                              where a.ID.ToString() == oitem.ps_code
                                              select a).FirstOrDefault();
                                oData1.Name = oitem.Name;
                                oData1.Postel = oitem.Postel;
                                oData1.Itemcode = oitem.Itemcode;
                                oData1.EMSRev = oitem.EMSRev;
                                oData1.Price = oitem.Price;
                                oData1.ps_code = "Y";

                                db.SaveChanges();
                                
                            }

                           
                            var result = Data.BP.GroupBy(x => x.AC_code).Select(x => x.Key).ToList();

                            foreach(var oitem1 in result)
                            {
                                var oData2 = (from a in db.C_MCT_BILL_WORKORDER
                                              where a.WorkOrder_ID == oitem1 
                                              select a).FirstOrDefault();

                                oData2.Status = "F";
                                db.SaveChanges();

                                sendemail(oData2.ID, "finishjob").ConfigureAwait(false);
                            }

                            log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/{5}", Session["Name"].ToString(), Session["Code"].ToString(), "Updateprice :", System.Reflection.MethodBase.GetCurrentMethod().Name, "", "success"));

                            TempData["msg"] = "<script>alert('Success!!');</script>";
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('Error!!');</script>";
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult PostelExport()
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                mWGE mData = new mWGE();

                using (var db = new DBS_WGE_Entities())
                {
                    if (Session["Name"] != null && (Session["Position"].ToString() == "8" || Session["Position"].ToString() == "10"))
                    {
                        mData.datefrom = DateTime.Now;
                        mData.dateto = DateTime.Now.AddDays(1);

                        return View(mData);
                    }
                    else
                        return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('Error!!');</script>";
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult PostelExport(DateTime datefrom, DateTime dateto, string[] checkbox, string button)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                mWGE mData = new mWGE();

                using (var db = new DBS_WGE_Entities())
                {
                    if (Session["Name"] != null && (Session["Position"].ToString() == "8" || Session["Position"].ToString() == "10"))
                    {
                        if (button == "search")
                        {
                            var oData = (from a in db.C_MCT_BILL_WORKORDER
                                         join b in db.C_MCT_BILL_POSTOFFICE on a.WorkOrder_ID equals b.AC_code into bb
                                         from b in bb.DefaultIfEmpty()
                                         where a.Commander_Datetime >= datefrom && a.Commander_Datetime < dateto && checkbox.Contains(b.Type) && a.Status == "P"
                                         orderby b.Itemcode
                                         select new C_MCT_BILL_WORKORDER2
                                         {
                                             WorkOrder_ID = a.WorkOrder_ID,
                                             Commander_Name = a.Commander_Name,
                                             Commander_Department = a.Commander_Department,
                                             Name = b.Name,
                                             Type = b.Type,
                                             Postel = b.Postel,
                                             Itemcode = b.Itemcode,
                                             EMSRev=b.EMSRev,
                                             price = b.Price ?? 0,
                                             Commander_Datetime = a.Commander_Datetime,
                                             Postel_ID = a.Work_ID ?? 0
                                         }).OrderBy(z => z.Commander_Datetime);

                            if (oData != null)
                            {
                                mData.datefrom = datefrom;
                                mData.dateto = dateto;
                                mData.chkbox = checkbox;
                                mData.ListBW2 = oData.ToList();
                            }

                            return View(mData);
                        }
                        else if (button == "export")
                        {
                            ReportDocument rd = new ReportDocument();
                            rd.Load(Path.Combine(Server.MapPath("~/Report"), "ReportPostel.rpt"));

                            rd.SetParameterValue("@dateto", dateto.ToString("yyyyMMdd"));
                            rd.SetParameterValue("@datefrom", datefrom.ToString("yyyyMMdd"));
                            rd.SetParameterValue("@checkbox", checkbox);

                            rd.SetParameterValue("@dateto1", dateto);
                            rd.SetParameterValue("@datefrom1", datefrom);

                            rd.SetDatabaseLogon("sa", "Wge123456*", "SVERP01", "DBS_WGE");

                            Response.Buffer = false;
                            Response.ClearContent();
                            Response.ClearHeaders();

                            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.Excel);
                            stream.Seek(0, SeekOrigin.Begin);

                            rd.Close();
                            rd.Dispose();

                            log.WriteLog(string.Format("{0}({1}) : {2} {3}", Session["Name"].ToString(), Session["Code"].ToString(), "Export", System.Reflection.MethodBase.GetCurrentMethod().Name));

                            return File(stream, "application/vnd.ms-excel", "Export_Postel.xls");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                        return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('Error!!');</script>";
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult del_btn(int id, string workorder_id)
        {
            if (Session["Name"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                try
                {
                    using (var db = new DBS_WGE_Entities())
                    {

                        var oData = (from a in db.C_MCT_BILL_POSTOFFICE
                                     where a.ID == id
                                     select a).FirstOrDefault();
                        
                        if (oData != null)
                        {
                            db.C_MCT_BILL_POSTOFFICE.Remove(oData);
                            db.SaveChanges();
                        }

                        log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}{5}/{6}{7}", Session["Name"].ToString(), Session["Code"].ToString(), "Remove", System.Reflection.MethodBase.GetCurrentMethod().Name,"ID :",oData.ID,"Work_ID",oData.WorkOrder_ID ));

                        return new EmptyResult();
                    }
                }
                catch (Exception ex)
                {
                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                    TempData["msg"] = "<script>alert('error!');</script>";
                    return RedirectToAction("Index", "Home");
                }
            }
        }

        public async Task sendemail(int id,string button)
        {
            await Task.Run(() =>
            {
            try
            {
                using (var db = new DBS_WGE_Entities())
                {
                    var oData = (from a in db.C_MCT_BILL_WORKORDER
                                 where a.ID == id
                                 select a).FirstOrDefault();
                    var oData2 = (from a in db.C_MCT_BILL_WORKORDER
                                  join b in db.C_MCT_BILL_POSTOFFICE on a.WorkOrder_ID equals b.WorkOrder_ID
                                  where a.ID == id
                                  select new C_MCT_BILL_WORKORDER2
                                  {
                                      Name = b.Name,
                                      Type = b.Type,
                                      Postel = b.Postel,
                                      Itemcode = b.Itemcode,
                                      EMSRev = b.EMSRev
                                  }).ToList();
                        var oDate3 = (from a in db.C_MCT_BILL_GENERAL
                                      where a.WorkOrder_ID == oData.WorkOrder_ID
                                      select a).FirstOrDefault();
                    string detail = string.Empty;
                    foreach (var oitem in oData2)
                    {
                        string er;
                        if (oitem.EMSRev == null)
                        {
                            er = "";
                        }
                        else
                        {
                            er = oitem.EMSRev;
                        }
                        detail += "ชื่อผู้รับ :" + oitem.Name + " / " + oitem.Postel + "<br />" + "ประเภทงาน : " + oitem.Type + "<br />" + "เลขพัสดุ : " + oitem.Itemcode + "<br />" + "เลขพัสดุคอบกลับ :" + er + "<br /><br />";
                    }

                    if (oData != null)
                    {
                        string des = string.Empty;
                        string type = string.Empty;

                        switch (oData.Type_Work)
                        {
                            case "S":
                                type = "ส่งเอกสาร";
                                break;
                            case "R":
                                type = "รับเอกสาร";
                                break;
                            case "O":
                                type = "อื่นๆ";
                                break;
                            case "B":
                                type = "ธนาคาร";
                                break;
                            case "P":
                                type = "ไปรษณีย์";
                                break;
                        }

                        switch (button)
                        {
                            case "acceptjob":
                                des = string.Format("งานหมายเลข ID: {0} ,ประเภท: {1} / ได้ถูกรับงานเรียบร้อยโดย {2}({3}) วันที่ {4} , เวลา {5}", oData.Work_ID, type, oData.Recipient_Name, oData.Recipient_Code, oData.Recipient_Datetime.Value.ToString("dd/MM/yyyy"), oData.Recipient_Datetime.Value.ToString("HH:mm"));
                                break;
                            case "rejectjob":
                                des = string.Format("งานหมายเลข ID: {0} ,ประเภท: {1} / งานของคุณได้ถูกยกเลิก", oData.Work_ID, type);
                                break;
                            case "finishjob":
                                des = string.Format("งานหมายเลข ID: {0} ,ประเภท: {1} <br />งานของคุณได้ถูกดำเนินการเสร็จเรียบร้อยแล้ว<br />โดย {2}({3}) วันที่ {4} , เวลา {5} <br /> สามารถตรวจสอบเลขพัสดุได้ดังนี้<br /><br />" + detail, oData.ID, type, oData.Recipient_Name, oData.Recipient_Code, oData.Last_Datetime.Value.ToString("dd/MM/yyyy"), oData.Recipient_Datetime.Value.ToString("HH:mm"));
                                break;
                            case "add":
                                des = string.Format("งานหมายเลข ID: {0} ,ประเภท: {1} <br />ได้ถูกเปิดโดยคุณ : {2} - {3} วันที่ {9} , เวลา {10} <br />รายละเอียดงาน : ชื่อผู้ติดต่อ-{4}  เบอร์โทร - {5} <br />รายละเอียด : {6}<br /> รายละเอียดเพิ่มเติม : {7} <br />รายละเอียด Admin : {8} <br /><br />", oData.Work_ID, type, oData.Commander_Code, oData.Commander_Name, oData.Contact_Name , oData.Contact_Tel, oData.Remark, oDate3.Description , oData.RemarkAdmin, oData.Commander_Datetime.Value.ToString("dd/MM/yyyy"), oData.Commander_Datetime.Value.ToString("HH:mm"));
                                break;
                            case "delete":
                                des = string.Format("งานหมายเลข ID: {0} ,ประเภท: {1} / งานของคุณได้ถูกลบโดย : {2}", oData.Work_ID, type, Session["Name"].ToString());
                                break;
                            }

                            string[] email = new string[2];

                            PrincipalContext context = new PrincipalContext(ContextType.Domain);
                            UserPrincipal user = new UserPrincipal(context);
                            user.Enabled = true;
                            PrincipalSearcher search = new PrincipalSearcher(user);

                            var thread = new Thread(() =>
                            {
                                foreach (var item in search.FindAll())
                                {
                                    var de = (DirectoryEntry)item.GetUnderlyingObject();

                                    if (de.Properties["initials"] != null && de.Properties["initials"].Count > 0)
                                    {
                                        if (button == "add")
                                        {
                                            email[0] = "g.bill@winnergroup.co.th";
                                            break;
                                        }
                                        else
                                        {
                                            if (de.Properties["initials"][0].ToString() == oData.Commander_Code)
                                            {
                                                email[0] = de.Properties["userPrincipalName"][0].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            });

                            var thread2 = new Thread(() =>
                            {
                                foreach (var item in search.FindAll())
                                {
                                    var de = (DirectoryEntry)item.GetUnderlyingObject();

                                    if (de.Properties["initials"] != null && de.Properties["initials"].Count > 0)
                                    {
                                        if(button != "add")
                                        {
                                            if (de.Properties["initials"][0].ToString() == oData.Recipient_Code)
                                            {
                                                email[1] = de.Properties["userPrincipalName"][0].ToString();
                                                break;
                                            }
                                        }
                                        else if(button == "delete")
                                        {
                                            email[1] = "g.bill@winnergroup.co.th";
                                            break;
                                        }
                                    }
                                }
                            });

                            thread.Start();
                            thread2.Start();

                            thread.Join();
                            thread2.Join();

                            thread.Abort();
                            thread2.Abort();
                            
                            for (int i = 0; i < email.Length; i++)
                            {
                                if(email[i] != null && email[i] != string.Empty && email[i] != "")
                                {
                                    var senderEmail = new MailAddress("info@winnergroup.co.th", "Bill&Postel");
                                    var receiverEmail = new MailAddress(email[i], "Receiver");
                                    var subject = "Bill & Postel";

                                    var smtp = new SmtpClient
                                    {
                                        Host = "mail.winnergroup.co.th"
                                    };
                                    using (var mess = new MailMessage(senderEmail, receiverEmail)
                                    {
                                        Subject = subject,
                                        Body = des
                                    })
                                    {
                                        mess.IsBodyHtml = true;
                                        smtp.Send(mess);
                                    }
                                }
                                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4} {5} {6}{7}", Session["Name"].ToString(), Session["Code"].ToString(), "Send Email", System.Reflection.MethodBase.GetCurrentMethod().Name, "(success)",email[i],"des ",button));

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //TempData["msg"] = "<script>alert('Error!!');</script>";
                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                }
            });
        }
    }
}