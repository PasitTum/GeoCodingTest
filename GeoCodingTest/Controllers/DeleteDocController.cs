using GeoCodingTest.Class;
using GeoCodingTest.Models.L1;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static GeoCodingTest.Models.L2.DBSWGE2;

namespace GeoCodingTest.Controllers
{
    public class DeleteDocController : Controller
    {
        InterfaceLog log = new InterfaceLog(ConfigurationManager.AppSettings["LogsPath"]);
        //DateTime date2020 = new DateTime(2020, 07, 01);
        DateTime date2020 = DateTime.ParseExact(ConfigurationManager.AppSettings["Date"], "yyyy-MM-dd", CultureInfo.InvariantCulture);

        // GET: DeleteDoc
        public ActionResult DeleteBilling()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Position"].ToString() == "8")
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult DeleteBilling(string cardcodeno,string cardname, string cpcode, string cpname, mWGE mData)
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using (var db = new DBS_WGE_Entities())
                {
                    OCRD nData = db.OCRD.Where(o => o.CardName.Contains(cpname) && o.CardCode.StartsWith("c") && o.FatherCard != null).FirstOrDefault();
                    string cpcode1 = nData.FatherCard;

                    var oData = (from a in db.C_MCT_BILLING
                                 join b in db.OCRD on a.CardCode equals b.CardCode 
                                 where (1 == 1)
                                 select new C_MCT_BILLING2
                                 {
                                     CardCode = a.CardCode,
                                     CardName = a.CardName,
                                     InvNo = a.InvNo,
                                     DateBill = a.DateBill,
                                     DueDateBill = a.DueDateBill,
                                     Total = a.Total,
                                     Name = a.Name,
                                     ID = a.ID,
                                     Remark = a.Remark,
                                     TypeID = a.TypeID,
                                     FatherCard = b.FatherCard
                                 });

                    if (!string.IsNullOrEmpty(cardcodeno))
                    {
                        oData = oData.Where(a => a.CardCode == cardcodeno).Distinct().OrderByDescending(x => x.InvNo);
                    }
                    else if (!string.IsNullOrEmpty(cardname))
                    {
                        oData = oData.Where(a => a.CardName.Contains(cardname)).Distinct().OrderByDescending(x => x.InvNo);
                    }
                    else if (!string.IsNullOrEmpty(cpcode))
                    {
                        oData = oData.Where(a => a.FatherCard == cpcode).Distinct().OrderByDescending(x => x.InvNo);
                    }
                    else if (!string.IsNullOrEmpty(cpname))
                    {
                        oData = oData.Where(a => a.FatherCard == cpcode1).Distinct().OrderByDescending(x => x.InvNo);
                    }

                    if (oData != null)
                    {
                        mData.ListBilling = oData.ToList();
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
        public ActionResult DeleteBilling2(string id1)
        {
            try
            {
                if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
                {
                    return RedirectToAction("Login", "Account");
                }

                string[] id2 = id1.Split(',');

                int countid = id2.Count();
                int countid2 = countid - 2;

                List<int> listid = new List<int>();
                for (int i = 2; i < 2 + countid2; i++)
                {
                    listid.Add(Convert.ToInt32(id2[i]));
                }

                var str = String.Join(",", listid);
                mWGE mwge = new mWGE();
                
                using (var db = new DBS_WGE_Entities())
                {
                    var oData = (from a in db.C_MCT_BILLING
                                 where listid.Contains(a.InvNo ?? 0)
                                 select a).ToList();

                    foreach (var oItem in oData)
                    {
                        db.C_MCT_BILLING.Remove(oItem);
                    }
                    db.SaveChanges();
                }

                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Delete", System.Reflection.MethodBase.GetCurrentMethod().Name, str));
                return View();
            }
            catch(Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                return View();
            }
        }

        public ActionResult DeleteReceive()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Position"].ToString() == "8")
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult DeleteReceive(string cardcodeno, string cardname, string cpcode, string cpname, mWGE mData)
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using (var db = new DBS_WGE_Entities())
                {
                    OCRD nData = db.OCRD.Where(o => o.CardName.Contains(cpname) && o.CardCode.StartsWith("c") && o.FatherCard != null).FirstOrDefault();
                    string cpcode1 = nData.FatherCard;

                    var oData = (from a in db.C_MCT_RECEIVE
                                 join b in db.OCRD on a.CardCode equals b.CardCode
                                 where (1 == 1)
                                 select new C_MCT_RECEIVE2
                                 {
                                     CardCode = a.CardCode,
                                     CardName = a.CardName,
                                     InvNo = a.InvNo,
                                     DateBill = a.DateBill,
                                     DueDateBill = a.DueDateBill,
                                     Total = a.TotalRev,
                                     Detail = a.Bank + "/" + a.Branch + "/" + a.ChequeNo,
                                     Name = a.Name,
                                     ID = a.ID,
                                     Remark = a.Remark,
                                     TypeID = a.TypeID,
                                     FatherCard = b.FatherCard
                                 });

                    if (!string.IsNullOrEmpty(cardcodeno))
                    {
                        oData = oData.Where(a => a.CardCode == cardcodeno).Distinct().OrderByDescending(x => x.InvNo);
                    }
                    else if (!string.IsNullOrEmpty(cardname))
                    {
                        oData = oData.Where(a => a.CardName.Contains(cardname)).Distinct().OrderByDescending(x => x.InvNo);
                    }
                    else if (!string.IsNullOrEmpty(cpcode))
                    {
                        oData = oData.Where(a => a.FatherCard == cpcode).Distinct().OrderByDescending(x => x.InvNo);
                    }
                    else if (!string.IsNullOrEmpty(cpname))
                    {
                        oData = oData.Where(a => a.FatherCard == cpcode1).Distinct().OrderByDescending(x => x.InvNo);
                    }

                    if (oData != null)
                    {
                        mData.ListReceive = oData.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
            }

            return View(mData);

        }

        public ActionResult DeleteReceive2(string id1)
        {
            try
            {
                if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
                {
                    return RedirectToAction("Login", "Account");
                }

                var str = "";

                using (var db = new DBS_WGE_Entities())
                {
                    mWGE mwge = new mWGE();

                    string[] id2 = id1.Split(',');

                    int countid = id2.Count();
                    int countid2 = countid - 2;

                    List<int> listid = new List<int>();

                    for (int i = 2; i < 2 + countid2; i++)
                    {
                        listid.Add(Convert.ToInt32(id2[i]));
                    }                  

                    str = String.Join(",", listid);

                    var oData = (from a in db.C_MCT_RECEIVE
                                 where listid.Contains(a.InvNo ?? 0)
                                 select a).ToList();

                    foreach (var oItem in oData)
                    {
                        db.C_MCT_RECEIVE.Remove(oItem);
                    }

                    var oData2 = (from a in db.C_MCT_BILLING
                                  where (listid.Contains(a.InvNo ?? default(int)))
                                  select a).ToList();

                    foreach (var oItem in oData2)
                    {
                        oItem.Status = "1";
                    }

                    db.SaveChanges();
                }

                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Delete", System.Reflection.MethodBase.GetCurrentMethod().Name, str));
                return View();
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                return View();
            }
        }

        public ActionResult DeleteCN()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Position"].ToString() == "8")
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult DeleteCN(string cardcodeno, string cardname, string cpcode, string cpname, mWGE mData)
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using (var db = new DBS_WGE_Entities())
                {
                    OCRD nData = db.OCRD.Where(o => o.CardName.Contains(cpname) && o.CardCode.StartsWith("c") && o.FatherCard != null).FirstOrDefault();
                    string cpcode1 = nData.FatherCard;

                    var oData = (from a in db.C_MCT_CREDITNOTE
                                 join b in db.OCRD on a.CardCode equals b.CardCode
                                 where (1 == 1)
                                 select new C_MCT_CREDITNOTE2
                                 {
                                     CardCode = a.CardCode,
                                     CardName = a.CardName,
                                     CreditNo = a.CreditNo,
                                     DateNote = a.DateNote,
                                     DueDateNote = a.DueDateNote,
                                     Total = a.Total,
                                     Name = a.Name,
                                     ID = a.ID,
                                     FatherCard = b.FatherCard
                                 });

                    if (!string.IsNullOrEmpty(cardcodeno))
                    {
                        oData = oData.Where(a => a.CardCode == cardcodeno).Distinct().OrderByDescending(x => x.CreditNo);
                    }
                    else if (!string.IsNullOrEmpty(cardname))
                    {
                        oData = oData.Where(a => a.CardName.Contains(cardname)).Distinct().OrderByDescending(x => x.CardName);
                    }
                    else if (!string.IsNullOrEmpty(cpcode))
                    {
                        oData = oData.Where(a => a.FatherCard == cpcode).Distinct().OrderByDescending(x => x.CreditNo);
                    }
                    else if (!string.IsNullOrEmpty(cpname))
                    {
                        oData = oData.Where(a => a.FatherCard == cpcode1).Distinct().OrderByDescending(x => x.CreditNo);
                    }

                    if (oData != null)
                    {
                        mData.ListCreditNote = oData.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
            }

            return View(mData);

        }

        public ActionResult DeleteCN2(string id1)
        {
            try
            {
                if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
                {
                    return RedirectToAction("Login", "Account");
                }

                var str = "";

                using (var db = new DBS_WGE_Entities())
                {
                    mWGE mwge = new mWGE();

                    string[] id2 = id1.Split(',');

                    int countid = id2.Count();
                    int countid2 = countid - 2;

                    List<int> listid = new List<int>();

                    for (int i = 2; i < 2 + countid2; i++)
                    {
                        listid.Add(Convert.ToInt32(id2[i]));
                    }

                    str = String.Join(",", listid);

                    var oData = (from a in db.C_MCT_CREDITNOTE
                                 where listid.Contains(a.CreditNo ?? 0)
                                 select a).ToList();

                    foreach (var oItem in oData)
                    {
                        db.C_MCT_CREDITNOTE.Remove(oItem);
                    }

                    db.SaveChanges();
                }

                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Delete", System.Reflection.MethodBase.GetCurrentMethod().Name, str));
                return View();
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                return View();
            }
        }

        public ActionResult DeleteOther()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Position"].ToString() == "8")
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult DeleteOther(string cardcodeno, string cardname, mWGE mData)
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using (var db = new DBS_WGE_Entities())
                {
                    var oData = (from a in db.C_MCT_OTHER
                                 where (1 == 1)
                                 select new C_MCT_OTHER2
                                 {
                                     CardCode = a.CardCode,
                                     CardName = a.CardName,
                                     InvNo = a.InvNo,
                                     DateBill = a.DateBill,
                                     TypeID = a.TypeID,
                                     Total = a.Total,
                                     Name = a.Name,
                                     ID = a.ID,
                                     Remark = a.Remark
                                 });

                    if (!string.IsNullOrEmpty(cardcodeno))
                    {
                        oData = oData.Where(a => a.CardCode == cardcodeno).Distinct().OrderByDescending(x => x.InvNo);
                    }
                    else if (!string.IsNullOrEmpty(cardname))
                    {
                        oData = oData.Where(a => a.CardName.Contains(cardname)).Distinct().OrderByDescending(x => x.CardName);
                    }

                    if (oData != null)
                    {
                        mData.ListOther = oData.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
            }

            return View(mData);

        }

        public ActionResult DeleteOther2(string id1)
        {
            try
            {
                if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
                {
                    return RedirectToAction("Login", "Account");
                }

                var str = "";

                using (var db = new DBS_WGE_Entities())
                {
                    mWGE mwge = new mWGE();

                    string[] id2 = id1.Split(',');

                    int countid = id2.Count();
                    int countid2 = countid - 2;

                    List<int> listid = new List<int>();

                    for (int i = 2; i < 2 + countid2; i++)
                    {
                        listid.Add(Convert.ToInt32(id2[i]));
                    }

                    str = String.Join(",", listid);

                    var oData = (from a in db.C_MCT_OTHER
                                 where listid.Contains(a.InvNo ?? 0)
                                 select a).ToList();

                    foreach (var oItem in oData)
                    {
                        db.C_MCT_OTHER.Remove(oItem);
                    }

                    db.SaveChanges();
                }

                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Delete", System.Reflection.MethodBase.GetCurrentMethod().Name, str));
                return View();
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                return View();
            }
        }

        public ActionResult DeleteDelivery()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
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
                        var oData = (from a in db.C_MCT_DOCUMENT
                                     where (1 == 1)
                                     select new C_MCT_DOCUMENT2
                                     {
                                         TypeID = a.TypeID,
                                         Remark = a.Remark,
                                         ID = a.ID,
                                         Name = a.Name,
                                         SubmitDate = a.SubmitDate                                        
                                     }).OrderByDescending(x => x.ID);

                        if (oData != null)
                        {
                            mData.ListDocument = oData.ToList();
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

        public ActionResult DeleteDelivery2(string id1)
        {
            try
            {
                if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
                {
                    return RedirectToAction("Login", "Account");
                }

                var str = "";

                using (var db = new DBS_WGE_Entities())
                {
                    mWGE mwge = new mWGE();

                    string[] id2 = id1.Split(',');

                    int countid = id2.Count();
                    int countid2 = countid - 2;

                    List<int> listid = new List<int>();

                    for (int i = 2; i < 2 + countid2; i++)
                    {
                        listid.Add(Convert.ToInt32(id2[i]));
                    }

                    str = String.Join(",", listid);

                    var oData = (from a in db.C_MCT_DOCUMENT
                                 where listid.Contains(a.ID)
                                 select a).ToList();

                    foreach (var oItem in oData)
                    {
                        db.C_MCT_DOCUMENT.Remove(oItem);
                    }

                    db.SaveChanges();
                }

                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Delete", System.Reflection.MethodBase.GetCurrentMethod().Name, str));
                return View();
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                return View();
            }
        }


    }
}