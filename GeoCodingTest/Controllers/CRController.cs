using CrystalDecisions.CrystalReports.Engine;
using GeoCodingTest.Class;
using GeoCodingTest.Models.L1;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static GeoCodingTest.Models.L2.DBSWGE2;

namespace GeoCodingTest.Controllers
{
    public class CRController : Controller
    {
        InterfaceLog log = new InterfaceLog(ConfigurationManager.AppSettings["LogsPath"]);

        // GET: CR
        public ActionResult BillingReport()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public ActionResult BillingDownload(string codeno, DateTime DateFrom, DateTime DateTo, string optionsRadios0, string optionsRadios1)
        {
            DBS_WGE_Entities db = new DBS_WGE_Entities();
            DateTime DateTo1 = DateTo.AddDays(1);

            try
            {
                if (codeno != "")
                {
                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/Report"), "CrystalReport1.rpt"));

                    if (optionsRadios0 == "0")
                    {
                        rd.SetDataSource(db.C_MCT_BILLING.Select(c => new
                        {
                            code = "0000",
                            name = c.Name,
                            invno = c.InvNo ?? 0,
                            cardcode = c.CardCode,
                            cardname = c.CardName,
                            datebill = c.DateBill ?? DateTime.MinValue,
                            typeid = c.TypeID,
                            remark = c.Remark,
                            remark2 = c.Remark2,
                            checkduedate = c.CheckDueDate ?? DateTime.MinValue,
                            submitdate = c.SubmitDate ?? DateTime.MinValue,
                            total = c.Total ?? 0,
                            receivedate = c.Receivedate ?? DateTime.MinValue,
                            refno = c.Refno
                        }).ToList().Where(o => o.submitdate >= DateFrom && o.submitdate < DateTo1 && (o.typeid == "0" || o.typeid == "5" || o.typeid == "6")).OrderByDescending(z => z.submitdate));
                    }
                    else
                    {
                        rd.SetDataSource(db.C_MCT_BILLING.Select(c => new
                        {
                            code = c.Code,
                            name = c.Name,
                            invno = c.InvNo ?? 0,
                            cardcode = c.CardCode,
                            cardname = c.CardName,
                            datebill = c.DateBill ?? DateTime.MinValue,
                            typeid = c.TypeID,                           
                            remark = c.Remark,
                            remark2 = c.Remark2,
                            checkduedate = c.CheckDueDate ?? DateTime.MinValue,
                            submitdate = c.SubmitDate ?? DateTime.MinValue,
                            total = c.Total ?? 0,
                            receivedate = c.Receivedate ?? DateTime.MinValue,
                            refno = c.Refno
                        }).ToList().Where(o => o.code == codeno && o.submitdate >= DateFrom && o.submitdate < DateTo1 && (o.typeid == "0" || o.typeid == "5" || o.typeid == "6")).OrderByDescending(z => z.submitdate));
                    }

                    Response.Buffer = false;
                    Response.ClearContent();
                    Response.ClearHeaders();

                    rd.SetDatabaseLogon("sa", "Wge123456*", "SVERP01", "DBS_WGE");

                    //rd.PrintOptions.PaperOrientation = CrystalDecisions.Shared.PaperOrientation.Landscape;
                    //rd.PrintOptions.ApplyPageMargins(new CrystalDecisions.Shared.PageMargins(5, 5, 5, 5));
                    //rd.PrintOptions.PaperSize = CrystalDecisions.Shared.PaperSize.PaperA5;

                    #region log
                    string des;

                    if (optionsRadios0 == "0")
                    {
                        des = "All Report";
                    }
                    else
                    {
                        des = "Report =>" + codeno;
                    }

                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/date = ({5}=>{6})", Session["Name"].ToString(), Session["Code"].ToString(), "Report", System.Reflection.MethodBase.GetCurrentMethod().Name, des, DateFrom, DateTo));
                    #endregion

                    if (optionsRadios1 == "1")
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        stream.Seek(0, SeekOrigin.Begin);

                        rd.Close();
                        rd.Dispose();

                        return File(stream, "application/pdf", "BillingReport.pdf");
                    }
                    else
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.Excel); //excel
                        stream.Seek(0, SeekOrigin.Begin);

                        rd.Close();
                        rd.Dispose();

                        return File(stream, "application/vnd.ms-excel", "BillingReport.xls");
                    }
                }
                else
                {
                    TempData["msg"] = "<script>alert('ไม่พบข้อมูล!');</script>";
                    return RedirectToAction("BillingReport", "CR");
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                TempData["msg"] = "<script>alert('error!');</script>";
                return RedirectToAction("BillingReport", "CR");
            }
        }

        public ActionResult ReceiveReport()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public ActionResult ReceiveDownload(string codeno, DateTime DateFrom, DateTime DateTo, string optionsRadios0, string optionsRadios1)
        {
            DBS_WGE_Entities db = new DBS_WGE_Entities();
            DateTime DateTo1 = DateTo.AddDays(1);

            try
            {
                if (codeno != "")
                {
                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/Report"), "CrystalReport2.rpt"));

                    if (optionsRadios0 == "0")
                    {
                        rd.SetDataSource(db.C_MCT_RECEIVE.Select(c => new
                        {
                            code = "0000",
                            name = c.Name,
                            invno = c.InvNo ?? 0,
                            cardcode = c.CardCode,
                            cardname = c.CardName,
                            bank = c.Bank,
                            branch = c.Branch,
                            chequeno = c.ChequeNo,
                            totalrev = c.TotalRev ?? 0,
                            datebill = c.DateBill ?? DateTime.MinValue,
                            typeid = c.TypeID,
                            submitdate = c.SubmitDate ?? DateTime.MinValue,
                            chequedate = c.ChequeDate ?? DateTime.MinValue,
                            remark = c.Remark,
                            total = c.Total ?? 0,
                            receivedate=c.Receivedate ?? DateTime.MinValue,
                            refno=c.Refno
                        }).ToList().Where(o => o.submitdate >= DateFrom && o.submitdate < DateTo1).OrderByDescending(z => z.submitdate));
                    }
                    else
                    {
                        rd.SetDataSource(db.C_MCT_RECEIVE.Select(c => new
                        {
                            code = c.Code,
                            name = c.Name,
                            invno = c.InvNo ?? 0,
                            cardcode = c.CardCode,
                            cardname = c.CardName,
                            bank = c.Bank,
                            branch = c.Branch,
                            chequeno = c.ChequeNo,
                            totalrev = c.TotalRev ?? 0,
                            datebill = c.DateBill ?? DateTime.MinValue,
                            typeid = c.TypeID,
                            submitdate = c.SubmitDate ?? DateTime.MinValue,
                            chequedate = c.ChequeDate ?? DateTime.MinValue,
                            remark = c.Remark,
                            total = c.Total ?? 0,
                            receivedate = c.Receivedate ?? DateTime.MinValue,
                            refno = c.Refno
                        }).ToList().Where(o => o.code == codeno && o.submitdate >= DateFrom && o.submitdate < DateTo1).OrderByDescending(z => z.submitdate));
                    }

                    Response.Buffer = false;
                    Response.ClearContent();
                    Response.ClearHeaders();

                    rd.SetDatabaseLogon("sa", "Wge123456*", "SVERP01", "DBS_WGE");

                    #region log
                    string des;

                    if (optionsRadios0 == "0")
                    {
                        des = "All Report";
                    }
                    else
                    {
                        des = "Report =>" + codeno;
                    }

                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/date = ({5}=>{6})", Session["Name"].ToString(), Session["Code"].ToString(), "Report", System.Reflection.MethodBase.GetCurrentMethod().Name, des, DateFrom, DateTo));
                    #endregion

                    //rd.PrintOptions.PaperOrientation = CrystalDecisions.Shared.PaperOrientation.Landscape;
                    //rd.PrintOptions.ApplyPageMargins(new CrystalDecisions.Shared.PageMargins(5, 5, 5, 5));
                    //rd.PrintOptions.PaperSize = CrystalDecisions.Shared.PaperSize.PaperA5;

                    if (optionsRadios1 == "1")
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        stream.Seek(0, SeekOrigin.Begin);

                        rd.Close();
                        rd.Dispose();

                        return File(stream, "application/pdf", "ReceiveReport.pdf");
                    }
                    else
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.Excel); //excel
                        stream.Seek(0, SeekOrigin.Begin);

                        rd.Close();
                        rd.Dispose();

                        return File(stream, "application/vnd.ms-excel", "ReceiveReport.xls");
                    }
                }
                else
                {
                    TempData["msg"] = "<script>alert('ไม่พบข้อมูล!');</script>";
                    return RedirectToAction("ReceiveReport", "CR");
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                TempData["msg"] = "<script>alert('error!');</script>";
                return RedirectToAction("ReceiveReport", "CR");
            }
        }

        public ActionResult FailBillingReport()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public ActionResult FailBillingDownload(string codeno, DateTime DateFrom, DateTime DateTo, string optionsRadios0, string optionsRadios1)
        {
            DBS_WGE_Entities db = new DBS_WGE_Entities();
            DateTime DateTo1 = DateTo.AddDays(1);

            try
            {
                if (codeno != "")
                {
                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/Report"), "CrystalReport3.rpt"));

                    if (optionsRadios0 == "0")
                    {
                        rd.SetDataSource(db.C_MCT_BILLING.Select(c => new
                        {
                            code = "0000",
                            name = c.Name,
                            invno = c.InvNo ?? 0,
                            cardcode = c.CardCode,
                            cardname = c.CardName,
                            datebill = c.DateBill ?? DateTime.MinValue,
                            daterev = c.DateRev ?? DateTime.MinValue,
                            typeid = c.TypeID,
                            remark = c.Remark,
                            submitdate = c.SubmitDate ?? DateTime.MinValue,
                            total = c.Total ?? 0
                        }).ToList().Where(o => o.submitdate >= DateFrom && o.submitdate < DateTo1 && o.typeid != "0" && o.typeid != "5" && o.typeid != "6").OrderByDescending(z => z.submitdate));
                    }
                    else
                    {
                        rd.SetDataSource(db.C_MCT_BILLING.Select(c => new
                        {
                            code = c.Code,
                            name = c.Name,
                            invno = c.InvNo ?? 0,
                            cardcode = c.CardCode,
                            cardname = c.CardName,
                            datebill = c.DateBill ?? DateTime.MinValue,
                            daterev = c.DateRev ?? DateTime.MinValue,
                            typeid = c.TypeID,
                            remark = c.Remark,
                            submitdate = c.SubmitDate ?? DateTime.MinValue,
                            total = c.Total ?? 0
                        }).ToList().Where(o => o.code == codeno && o.submitdate >= DateFrom && o.submitdate < DateTo1 && o.typeid != "0" && o.typeid != "5" && o.typeid != "6").OrderByDescending(z => z.submitdate));
                    }

                    Response.Buffer = false;
                    Response.ClearContent();
                    Response.ClearHeaders();

                    rd.SetDatabaseLogon("sa", "Wge123456*", "SVERP01", "DBS_WGE");

                    #region log
                    string des;

                    if (optionsRadios0 == "0")
                    {
                        des = "All Report";
                    }
                    else
                    {
                        des = "Report =>" + codeno;
                    }

                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/date = ({5}=>{6})", Session["Name"].ToString(), Session["Code"].ToString(), "Report", System.Reflection.MethodBase.GetCurrentMethod().Name, des, DateFrom, DateTo));
                    #endregion

                    //rd.PrintOptions.PaperOrientation = CrystalDecisions.Shared.PaperOrientation.Landscape;
                    //rd.PrintOptions.ApplyPageMargins(new CrystalDecisions.Shared.PageMargins(5, 5, 5, 5));
                    //rd.PrintOptions.PaperSize = CrystalDecisions.Shared.PaperSize.PaperA5;

                    if (optionsRadios1 == "1")
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        stream.Seek(0, SeekOrigin.Begin);

                        rd.Close();
                        rd.Dispose();

                        return File(stream, "application/pdf", "FailBillingReport.pdf");
                    }
                    else
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.Excel); //excel
                        stream.Seek(0, SeekOrigin.Begin);

                        rd.Close();
                        rd.Dispose();

                        return File(stream, "application/vnd.ms-excel", "FailBillingReport.xls");
                    }
                }
                else
                {
                    TempData["msg"] = "<script>alert('ไม่พบข้อมูล!');</script>";
                    return RedirectToAction("FailBillingReport", "CR");
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                TempData["msg"] = "<script>alert('error!');</script>";
                return RedirectToAction("FailBillingReport", "CR");
            }
        }

        public ActionResult DocumentReport()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public ActionResult DocumentDownload(string codeno, DateTime DateFrom, DateTime DateTo, string optionsRadios0, string optionsRadios1)
        {
            DBS_WGE_Entities db = new DBS_WGE_Entities();
            DateTime DateTo1 = DateTo.AddDays(1);

            try
            {
                if (codeno != "")
                {

                    //DateTime datefrom1 = DateTime.ParseExact(DateFrom, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    //DateTime dateto1 = DateTime.ParseExact(DateTo, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/Report"), "CrystalReport4.rpt"));


                    if (optionsRadios0 == "0")
                    {
                        rd.SetDataSource(db.C_MCT_DOCUMENT.Select(c => new
                        {
                            code = "0000",
                            name = c.Name,
                            submitdate = c.SubmitDate ?? DateTime.MinValue,
                            typeid = c.TypeID,
                            remark = c.Remark,
                        }).ToList().Where(o => o.submitdate >= DateFrom && o.submitdate < DateTo1).OrderByDescending(z => z.submitdate));
                    }
                    else
                    {
                        rd.SetDataSource(db.C_MCT_DOCUMENT.Select(c => new
                        {
                            code = c.Code,
                            name = c.Name,
                            submitdate = c.SubmitDate ?? DateTime.MinValue,
                            typeid = c.TypeID,
                            remark = c.Remark,
                        }).ToList().Where(o => o.code == codeno && o.submitdate >= DateFrom && o.submitdate < DateTo1).OrderByDescending(z => z.submitdate));
                    }

                    Response.Buffer = false;
                    Response.ClearContent();
                    Response.ClearHeaders();

                    rd.SetDatabaseLogon("sa", "Wge123456*", "SVERP01", "DBS_WGE");

                    #region log
                    string des;

                    if (optionsRadios0 == "0")
                    {
                        des = "All Report";
                    }
                    else
                    {
                        des = "Report =>" + codeno;
                    }

                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/date = ({5}=>{6})", Session["Name"].ToString(), Session["Code"].ToString(), "Report", System.Reflection.MethodBase.GetCurrentMethod().Name, des, DateFrom, DateTo));
                    #endregion

                    //rd.PrintOptions.PaperOrientation = CrystalDecisions.Shared.PaperOrientation.Landscape;
                    //rd.PrintOptions.ApplyPageMargins(new CrystalDecisions.Shared.PageMargins(5, 5, 5, 5));
                    //rd.PrintOptions.PaperSize = CrystalDecisions.Shared.PaperSize.PaperA5;

                    if (optionsRadios1 == "1")
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        stream.Seek(0, SeekOrigin.Begin);

                        rd.Close();
                        rd.Dispose();

                        return File(stream, "application/pdf", "DocumentReport.pdf");
                    }
                    else
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.Excel); //excel
                        stream.Seek(0, SeekOrigin.Begin);

                        rd.Close();
                        rd.Dispose();

                        return File(stream, "application/vnd.ms-excel", "DocumentReport.xls");
                    }
                }
                else
                {
                    TempData["msg"] = "<script>alert('ไม่พบข้อมูล!');</script>";
                    return RedirectToAction("DocumentReport", "CR");
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                TempData["msg"] = "<script>alert('error!');</script>";
                return RedirectToAction("DocumentReport", "CR");
            }
        }

        public ActionResult CNReport()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public ActionResult CNDownload(string codeno, DateTime DateFrom, DateTime DateTo, string optionsRadios0, string optionsRadios1)
        {
            DBS_WGE_Entities db = new DBS_WGE_Entities();
            DateTime DateTo1 = DateTo.AddDays(1);

            try
            {
                if (codeno != "")
                {

                    //DateTime datefrom1 = DateTime.ParseExact(DateFrom, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    //DateTime dateto1 = DateTime.ParseExact(DateTo, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/Report"), "CrystalReport5.rpt"));


                    if (optionsRadios0 == "0")
                    {
                        rd.SetDataSource(db.C_MCT_CREDITNOTE.Select(c => new
                        {
                            code = "0000",
                            name = c.Name,
                            submitdate = c.SubmitDate ?? DateTime.MinValue,
                            datenote = c.DateNote ?? DateTime.MinValue,
                            duedatenote = c.DueDateNote ?? DateTime.MinValue,
                            total = c.Total ?? 0,
                            cardcode = c.CardCode,
                            cardname = c.CardName,
                            creditno = c.CreditNo ?? 0,
                            remark = c.Remark,
                            receivedate = c.ReceiveDate ?? DateTime.MinValue
                        }).ToList().Where(o => o.submitdate >= DateFrom && o.submitdate < DateTo1).OrderByDescending(z => z.submitdate));
                    }
                    else
                    {
                        rd.SetDataSource(db.C_MCT_CREDITNOTE.Select(c => new
                        {
                            code = c.Code,
                            name = c.Name,
                            submitdate = c.SubmitDate ?? DateTime.MinValue,
                            datenote = c.DateNote ?? DateTime.MinValue,
                            duedatenote = c.DueDateNote ?? DateTime.MinValue,
                            total = c.Total ?? 0,
                            cardcode = c.CardCode,
                            cardname = c.CardName,
                            creditno = c.CreditNo ?? 0,
                            remark = c.Remark,
                            receivedate = c.ReceiveDate ?? DateTime.MinValue
                        }).ToList().Where(o => o.code == codeno && o.submitdate >= DateFrom && o.submitdate < DateTo1).OrderByDescending(z => z.submitdate));
                    }

                    Response.Buffer = false;
                    Response.ClearContent();
                    Response.ClearHeaders();

                    rd.SetDatabaseLogon("sa", "Wge123456*", "SVERP01", "DBS_WGE");

                    #region log
                    string des;

                    if (optionsRadios0 == "0")
                    {
                        des = "All Report";
                    }
                    else
                    {
                        des = "Report =>" + codeno;
                    }

                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/date = ({5}=>{6})", Session["Name"].ToString(), Session["Code"].ToString(), "Report", System.Reflection.MethodBase.GetCurrentMethod().Name, des, DateFrom, DateTo));
                    #endregion

                    //rd.PrintOptions.PaperOrientation = CrystalDecisions.Shared.PaperOrientation.Landscape;
                    //rd.PrintOptions.ApplyPageMargins(new CrystalDecisions.Shared.PageMargins(5, 5, 5, 5));
                    //rd.PrintOptions.PaperSize = CrystalDecisions.Shared.PaperSize.PaperA5;

                    if (optionsRadios1 == "1")
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        stream.Seek(0, SeekOrigin.Begin);

                        rd.Close();
                        rd.Dispose();

                        return File(stream, "application/pdf", "CNReport.pdf");
                    }
                    else
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.Excel); //excel
                        stream.Seek(0, SeekOrigin.Begin);

                        rd.Close();
                        rd.Dispose();

                        return File(stream, "application/vnd.ms-excel", "CNReport.xls");
                    }
                }
                else
                {
                    TempData["msg"] = "<script>alert('ไม่พบข้อมูล!');</script>";
                    return RedirectToAction("CNReport", "CR");
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                TempData["msg"] = "<script>alert('error!');</script>";
                return RedirectToAction("CNReport", "CR");
            }
        }

        public ActionResult OtherReport()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public ActionResult OtherDownload(string codeno, DateTime DateFrom, DateTime DateTo, string optionsRadios0, string optionsRadios1)
        {
            DBS_WGE_Entities db = new DBS_WGE_Entities();
            DateTime DateTo1 = DateTo.AddDays(1);

            try
            {
                if (codeno != "")
                {

                    //DateTime datefrom1 = DateTime.ParseExact(DateFrom, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    //DateTime dateto1 = DateTime.ParseExact(DateTo, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/Report"), "CrystalReport6.rpt"));


                    if (optionsRadios0 == "0")
                    {
                        rd.SetDataSource(db.C_MCT_OTHER.Select(c => new
                        {
                            code = "0000",
                            name = c.Name,
                            submitdate = c.SubmitDate ?? DateTime.MinValue,
                            typeid = c.TypeID,
                            invno = c.InvNo ?? 0,
                            cardcode = c.CardCode,
                            cardname = c.CardName,
                            datebill = c.DateBill ?? DateTime.MinValue,
                            remark = c.Remark,
                            total = c.Total ?? 0
                        }).ToList().Where(o => o.submitdate >= DateFrom && o.submitdate < DateTo1).OrderByDescending(z => z.submitdate));
                    }
                    else
                    {
                        rd.SetDataSource(db.C_MCT_OTHER.Select(c => new
                        {
                            code = c.Code,
                            name = c.Name,
                            submitdate = c.SubmitDate ?? DateTime.MinValue,
                            typeid = c.TypeID,
                            invno = c.InvNo ?? 0,
                            cardcode = c.CardCode,
                            cardname = c.CardName,
                            datebill = c.DateBill ?? DateTime.MinValue,
                            remark = c.Remark,
                            total = c.Total ?? 0
                        }).ToList().Where(o => o.code == codeno && o.submitdate >= DateFrom && o.submitdate < DateTo1).OrderByDescending(z => z.submitdate));
                    }

                    Response.Buffer = false;
                    Response.ClearContent();
                    Response.ClearHeaders();

                    rd.SetDatabaseLogon("sa", "Wge123456*", "SVERP01", "DBS_WGE");

                    #region log
                    string des;

                    if (optionsRadios0 == "0")
                    {
                        des = "All Report";
                    }
                    else
                    {
                        des = "Report =>" + codeno;
                    }

                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/date = ({5}=>{6})", Session["Name"].ToString(), Session["Code"].ToString(), "Report", System.Reflection.MethodBase.GetCurrentMethod().Name, des, DateFrom, DateTo));
                    #endregion

                    //rd.PrintOptions.PaperOrientation = CrystalDecisions.Shared.PaperOrientation.Landscape;
                    //rd.PrintOptions.ApplyPageMargins(new CrystalDecisions.Shared.PageMargins(5, 5, 5, 5));
                    //rd.PrintOptions.PaperSize = CrystalDecisions.Shared.PaperSize.PaperA5;

                    if (optionsRadios1 == "1")
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        stream.Seek(0, SeekOrigin.Begin);

                        rd.Close();
                        rd.Dispose();

                        return File(stream, "application/pdf", "OtherReport.pdf");
                    }
                    else
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.Excel); //excel
                        stream.Seek(0, SeekOrigin.Begin);

                        rd.Close();
                        rd.Dispose();

                        return File(stream, "application/vnd.ms-excel", "OtherReport.xls");
                    }
                }
                else
                {
                    TempData["msg"] = "<script>alert('ไม่พบข้อมูล!');</script>";
                    return RedirectToAction("OtherReport", "CR");
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                TempData["msg"] = "<script>alert('error!');</script>";
                return RedirectToAction("OtherReport", "CR");
            }
        }

        public ActionResult AllReport()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Position"].ToString() == "8" && Session["Position"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult AllDownload(string codeno, DateTime DateFrom, DateTime DateTo, string optionsRadios0, string optionsRadios1)
        {
            DBS_WGE_Entities db = new DBS_WGE_Entities();
            DateTime DateTo0 = DateTo.AddDays(1);

            try
            {
                if (codeno != "")
                {
                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/Report"), "CrystalReportAll.rpt"));

                    if (optionsRadios0 == "0")
                    {
                        string DateFrom1 = DateFrom.ToString("yyyy-MM-dd");
                        string DateTo1 = DateTo0.ToString("yyyy-MM-dd");

                        rd.SetParameterValue("@Code0", "0000");
                        rd.SetParameterValue("@Code", codeno);
                        rd.SetParameterValue("@DateFrom", DateFrom1);
                        rd.SetParameterValue("@DateTo", DateTo1);
                    }
                    else
                    {
                        string DateFrom1 = DateFrom.ToString("yyyy-MM-dd");
                        string DateTo1 = DateTo0.ToString("yyyy-MM-dd");

                        rd.SetParameterValue("@Code0", "1111");
                        rd.SetParameterValue("@Code", codeno);
                        rd.SetParameterValue("@DateFrom", DateFrom1);
                        rd.SetParameterValue("@DateTo", DateTo1);
                    }

                    rd.SetDatabaseLogon("sa", "Wge123456*", "SVERP01", "DBS_WGE");

                    Response.Buffer = false;
                    Response.ClearContent();
                    Response.ClearHeaders();

                    #region log
                    string des;

                    if (optionsRadios0 == "0")
                    {
                        des = "All Report";
                    }
                    else
                    {
                        des = "Report =>" + codeno;
                    }

                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/date = ({5}=>{6})", Session["Name"].ToString(), Session["Code"].ToString(), "Report", System.Reflection.MethodBase.GetCurrentMethod().Name, des, DateFrom, DateTo));
                    #endregion

                    if (optionsRadios1 == "1")
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        stream.Seek(0, SeekOrigin.Begin);

                        rd.Close();
                        rd.Dispose();

                        return File(stream, "application/pdf", "AllReport.pdf");
                    }
                    else
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.Excel); //excel
                        stream.Seek(0, SeekOrigin.Begin);

                        rd.Close();
                        rd.Dispose();

                        return File(stream, "application/vnd.ms-excel", "AllReport.xls");
                    }
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                return View();
            }
        }

        public ActionResult Heatmap()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Position"].ToString() == "8" && Session["Position"] != null)
            {
                mWGE mData = new mWGE();

                try
                {
                    using (var db = new DBS_WGE_Entities())
                    {
                        //int empCode = Convert.ToInt32(Session["EMPNo"]);

                        var oData = (from a in db.C_MCT_BILLING
                                     where (1 == 1)
                                     select new C_MCT_BILLING2
                                     {
                                         Code = a.Code,
                                         Name = a.Name
                                     }).Distinct();

                        //oData = oData.OrderByDescending(o => o.id);

                        if (oData != null)
                        {
                            mData.ListBilling = oData.ToList();
                            //log.WriteLog(string.Format("{0}({1}) : {2}", Session["Name"].ToString(), Session["Code"].ToString(), "Search Billing"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    //log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                }

                return View(mData);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult Heatmap2(string codeno, DateTime DateFrom, DateTime DateTo, mWGE mData)
        {
            try
            {
                using (var db = new DBS_WGE_Entities())
                {
                    DateTime DateTo1 = DateTo.AddDays(1);

                    var oData1 = (from a in db.C_MCT_BILLING
                                  where (a.Code == codeno) && (a.SubmitDate >= DateFrom) && (a.SubmitDate < DateTo1)
                                  select new C_MCT_BILLING2
                                  {
                                      //ID = a.ID,
                                      CardCode = a.CardCode,
                                      CardName = a.CardName,
                                      //InvNo = a.InvNo,
                                      TypeID = a.TypeID,
                                      Code = a.Code,
                                      Name = a.Name,
                                      Remark = a.Remark,
                                      Latitude = a.Latitude,
                                      Longitude = a.Longitude,
                                      SubmitDate = System.Data.Entity.DbFunctions.TruncateTime(a.SubmitDate),
                                      Type = "Billing"
                                  }).Distinct();

                    var oData2 = (from a in db.C_MCT_RECEIVE
                                  where (a.Code == codeno) && (a.SubmitDate >= DateFrom) && (a.SubmitDate < DateTo1)
                                  select new C_MCT_BILLING2
                                  {
                                      //ID = a.ID,
                                      CardCode = a.CardCode,
                                      CardName = a.CardName,
                                      //InvNo = a.InvNo,
                                      TypeID = a.TypeID,
                                      Code = a.Code,
                                      Name = a.Name,
                                      Remark = a.Remark,
                                      Latitude = a.Latitude,
                                      Longitude = a.Longitude,
                                      SubmitDate = System.Data.Entity.DbFunctions.TruncateTime(a.SubmitDate),
                                      Type = "Receive"
                                  }).Distinct();

                    var oData3 = (from a in db.C_MCT_CREDITNOTE
                                  where (a.Code == codeno) && (a.SubmitDate >= DateFrom) && (a.SubmitDate < DateTo1)
                                  select new C_MCT_BILLING2
                                  {
                                      //ID = a.ID,
                                      CardCode = a.CardCode,
                                      CardName = a.CardName,
                                      //InvNo = a.CreditNo,
                                      TypeID = "",
                                      Code = a.Code,
                                      Name = a.Name,
                                      Remark = "",
                                      Latitude = a.Latitude,
                                      Longitude = a.Longitude,
                                      SubmitDate = System.Data.Entity.DbFunctions.TruncateTime(a.SubmitDate),
                                      Type = "CreditNote"
                                  }).Distinct();

                    var oData4 = (from a in db.C_MCT_OTHER
                                  where (a.Code == codeno) && (a.SubmitDate >= DateFrom) && (a.SubmitDate < DateTo1)
                                  select new C_MCT_BILLING2
                                  {
                                      //ID = a.ID,
                                      CardCode = a.CardCode,
                                      CardName = a.CardName,
                                      //InvNo = a.InvNo,
                                      TypeID = a.TypeID,
                                      Code = a.Code,
                                      Name = a.Name,
                                      Remark = a.Remark,
                                      Latitude = a.Latitude,
                                      Longitude = a.Longitude,
                                      SubmitDate = System.Data.Entity.DbFunctions.TruncateTime(a.SubmitDate),
                                      Type = "Other"
                                  }).Distinct();

                    var oData5 = (from a in db.C_MCT_DOCUMENT
                                  where (a.Code == codeno) && (a.SubmitDate >= DateFrom) && (a.SubmitDate < DateTo1)
                                  select new C_MCT_BILLING2
                                  {
                                      //ID = a.ID,
                                      CardCode = "",
                                      CardName = "",
                                      //InvNo = 0,
                                      TypeID = a.TypeID,
                                      Code = a.Code,
                                      Name = a.Name,
                                      Remark = a.Remark,
                                      Latitude = a.Latitude,
                                      Longitude = a.Longitude,
                                      SubmitDate = System.Data.Entity.DbFunctions.TruncateTime(a.SubmitDate),
                                      Type = "Document"
                                  }).Distinct();


                    if (oData1 != null)
                    {
                        var list = (oData1.Concat(oData2).Concat(oData3).Concat(oData4).Concat(oData5)).OrderByDescending(o => o.SubmitDate);

                        mData.ListBilling = list.ToList();
                        log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/date = ({5}=>{6})", Session["Name"].ToString(), Session["Code"].ToString(), "Heatmap", System.Reflection.MethodBase.GetCurrentMethod().Name, codeno, DateFrom, DateTo));

                        if (mData.ListBilling.Count == 0)
                        {
                            TempData["msg"] = "<script>alert('ไม่พบข้อมูล!');</script>";
                            return RedirectToAction("Heatmap", "CR");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('error!!');</script>";
                return RedirectToAction("Heatmap", "CR");
            }

            return View(mData);
        }

        public ActionResult FrequencyReport()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Position"].ToString() == "8" && Session["Position"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult FrequencyReport(DateTime DateFrom, mWGE mData)
        {
            try
            {
                using (var db = new DBS_WGE_Entities())
                {
                    DateTime DateTo1 = DateFrom.AddMonths(1);
                   
                    string monthname = DateFrom.ToString("MMMM");
                    string year = DateFrom.ToString("yyyy");
                    int month = DateTime.ParseExact(monthname, "MMMM", CultureInfo.CurrentCulture).Month;

                    int dayofweek = (int)DateFrom.DayOfWeek;
                    int days = DateTime.DaysInMonth(Convert.ToInt32(year),month);

                    //ateTime d1 = DateTime.ParseExact("2020-11-17", "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    //DateTime d2 = DateTime.ParseExact("2020-11-18", "yyyy-MM-dd", CultureInfo.InvariantCulture);

                    var oData0 = (from a in db.C_MCT_BILLING
                                  where (1 == 1)
                                  select new C_MCT_BILLING2
                                  {
                                      Code = a.Code,
                                      Name = a.Name,
                                      MonthName = monthname,
                                      Year = year,
                                      Days = days,
                                      DayofWeek = dayofweek
                                  }).Distinct().OrderBy(o => o.Code);

                    var oData1 = (from a in db.C_MCT_BILLING
                                  where (a.SubmitDate >= DateFrom) && (a.SubmitDate < DateTo1)
                                  select new C_MCT_BILLING2
                                  {
                                      CardCode = a.CardCode,
                                      CardName = a.CardName,
                                      Code = a.Code,
                                      Name = a.Name,
                                      Type = "Billing",
                                      TypeID = a.TypeID,
                                      SubmitDate = System.Data.Entity.DbFunctions.TruncateTime(a.SubmitDate),
                                      Latitude = a.Latitude,
                                      Longitude = a.Longitude,
                                      Remark = a.Remark
                                  }).Distinct();

                    var oData2 = (from a in db.C_MCT_RECEIVE
                                  where (a.SubmitDate >= DateFrom) && (a.SubmitDate < DateTo1)
                                  select new C_MCT_BILLING2
                                  {
                                      CardCode = a.CardCode,
                                      CardName = a.CardName,
                                      Code = a.Code,
                                      Name = a.Name,
                                      Type = "Receive",
                                      TypeID = a.TypeID,
                                      SubmitDate = System.Data.Entity.DbFunctions.TruncateTime(a.SubmitDate),
                                      Latitude = a.Latitude,
                                      Longitude = a.Longitude,
                                      Remark = a.Remark
                                  }).Distinct();

                    var oData3 = (from a in db.C_MCT_CREDITNOTE
                                  where (a.SubmitDate >= DateFrom) && (a.SubmitDate < DateTo1)
                                  select new C_MCT_BILLING2
                                  {
                                      CardCode = a.CardCode,
                                      CardName = a.CardName,
                                      Code = a.Code,
                                      Name = a.Name,
                                      Type = "CreditNote",
                                      TypeID = "0",
                                      SubmitDate = System.Data.Entity.DbFunctions.TruncateTime(a.SubmitDate),
                                      Latitude = a.Latitude,
                                      Longitude = a.Longitude,
                                      Remark = ""
                                  }).Distinct();

                    var oData4 = (from a in db.C_MCT_OTHER
                                  where (a.SubmitDate >= DateFrom) && (a.SubmitDate < DateTo1)
                                  select new C_MCT_BILLING2
                                  {
                                      CardCode = a.CardCode,
                                      CardName = a.CardName,
                                      Code = a.Code,
                                      Name = a.Name,
                                      Type = "Other",
                                      TypeID = a.TypeID,
                                      SubmitDate = System.Data.Entity.DbFunctions.TruncateTime(a.SubmitDate),
                                      Latitude = a.Latitude,
                                      Longitude = a.Longitude,
                                      Remark = a.Remark
                                  }).Distinct();

                    var oData5 = (from a in db.C_MCT_DOCUMENT
                                  where (a.SubmitDate >= DateFrom) && (a.SubmitDate < DateTo1)
                                  select new C_MCT_BILLING2
                                  {
                                      CardCode = "",
                                      CardName = "",
                                      Code = a.Code,
                                      Name = a.Name,
                                      Type = "Document",
                                      TypeID = a.TypeID,
                                      SubmitDate = System.Data.Entity.DbFunctions.TruncateTime(a.SubmitDate),
                                      Latitude = a.Latitude,
                                      Longitude = a.Longitude,
                                      Remark = a.Remark
                                  }).Distinct();


                    if (oData1 != null)
                    {
                        var list = (oData1.Concat(oData2).Concat(oData3).Concat(oData4).Concat(oData5));
                        mData.ListBilling = list.ToList();

                        mData.ListSetting = oData0.ToList();
                        log.WriteLog(string.Format("{0}({1}) : {2} {3}/date = ({4})", Session["Name"].ToString(), Session["Code"].ToString(), "FrequencyReport", System.Reflection.MethodBase.GetCurrentMethod().Name, DateFrom));

                        if (mData.ListBilling.Count == 0)
                        {
                            TempData["msg"] = "<script>alert('ไม่พบข้อมูล!');</script>";
                            return RedirectToAction("FrequencyReport", "CR");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('error!!');</script>";
                return RedirectToAction("FrequencyReport", "CR");
            }

            return View(mData);
        }
        public ActionResult PostelReport()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
        
        [HttpPost]
        public ActionResult ReportPostel(mWGE data,string optionsRadios0, string optionsRadios1 ,string optionsRadios2)
        {
            
            try
            {
                if(optionsRadios2 == "1")
                {
                    ReportDocument rd = new ReportDocument();
                    rd.Load(Server.MapPath("../Report/ReportPostel_Monthly.rpt"));

                    if (optionsRadios0 == "0")
                    {
                        string mtyear1 = data.month1 + data.year1;
                        string mtyear2 = data.month1 + data.year1;

                        rd.SetParameterValue("@mtyear1", mtyear1);
                        rd.SetParameterValue("@mtyear2", mtyear2);
                    }
                    else
                    {
                        string mtyear1 = data.month1 + data.year1;
                        string mtyear2 = data.month2 + data.year2;

                        rd.SetParameterValue("@mtyear1", mtyear1);
                        rd.SetParameterValue("@mtyear2", mtyear2);
                    }

                    rd.SetDatabaseLogon("sa", "Wge123456*", "SVERP01", "DBS_WGE");

                    Response.Buffer = false;
                    Response.ClearContent();
                    Response.ClearHeaders();

                    //rd.PrintOptions.PaperOrientation = CrystalDecisions.Shared.PaperOrientation.Landscape;
                    //rd.PrintOptions.ApplyPageMargins(new CrystalDecisions.Shared.PageMargins(5, 5, 5, 5));
                    //rd.PrintOptions.PaperSize = CrystalDecisions.Shared.PaperSize.PaperA5;

                    #region log
                    string des;

                    if (optionsRadios0 == "0")
                    {
                        des = "Postel Monthly Report=>" + data.month1 + data.year1;
                    }
                    else
                    {
                        des = "Postel Monthly Report=>" + data.month1 + data.year1 + "ถึง" + data.month2 + data.year2;
                    }

                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Report", System.Reflection.MethodBase.GetCurrentMethod().Name, des));
                    #endregion

                    if (optionsRadios1 == "1")
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        stream.Seek(0, SeekOrigin.Begin);

                        rd.Close();
                        rd.Dispose();

                        return File(stream, "application/pdf", "ReportPostelMonthly.pdf");

                    }
                    else
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.Excel); //excel
                        stream.Seek(0, SeekOrigin.Begin);

                        rd.Close();
                        rd.Dispose();

                        return File(stream, "application/vnd.ms-excel", "ReportPostelMonthly.xls");
                    }
                }
                else if(optionsRadios2 == "2")
                {
                    ReportDocument rd = new ReportDocument();
                    rd.Load(Server.MapPath("../Report/Postel_PR.rpt"));

                    
                        string mtyear1 = data.month1 + data.year1;
                        string mtyear2 = data.month1 + data.year1;

                        rd.SetParameterValue("@mtyear1", mtyear1);
                        rd.SetParameterValue("@mtyear2", mtyear2);
                    DateTime date1 = DateTime.MinValue;
                    int date3 = data.date.Year + 543;
                    string date2 = data.date.Day+"/"+data.date.Month+"/"+date3.ToString();
                    int date4 = DateTime.Now.Year + 543;

                    string date5 = DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + date4.ToString();

                    if (data.date == date1)
                    {
                        
                        rd.SetParameterValue("date", date5);
                    }
                    else
                    {
                        rd.SetParameterValue("date", date2);
                    }
                    rd.SetParameterValue("date2", date5);
                    rd.SetDatabaseLogon("sa", "Wge123456*", "SVERP01", "DBS_WGE");

                    Response.Buffer = false;
                    Response.ClearContent();
                    Response.ClearHeaders();

                    //rd.PrintOptions.PaperOrientation = CrystalDecisions.Shared.PaperOrientation.Landscape;
                    //rd.PrintOptions.ApplyPageMargins(new CrystalDecisions.Shared.PageMargins(5, 5, 5, 5));
                    //rd.PrintOptions.PaperSize = CrystalDecisions.Shared.PaperSize.PaperA5;

                    #region log
                    string des;

                    if (optionsRadios0 == "0")
                    {
                        des = "Postel Monthly Report=>" + data.month1 + data.year1;
                    }
                    else
                    {
                        des = "Postel Monthly Report=>" + data.month1 + data.year1 + "ถึง" + data.month2 + data.year2;
                    }

                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Report", System.Reflection.MethodBase.GetCurrentMethod().Name, des));
                    #endregion

                    if (optionsRadios1 == "1")
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        stream.Seek(0, SeekOrigin.Begin);

                        rd.Close();
                        rd.Dispose();

                        return File(stream, "application/pdf", "Postel_PR.pdf");

                    }
                    else
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.Excel); //excel
                        stream.Seek(0, SeekOrigin.Begin);

                        rd.Close();
                        rd.Dispose();

                        return File(stream, "application/vnd.ms-excel", "Postel_PR.xls");
                    }
                }
                else
                {
                    TempData["msg"] = "<script>alert('error!!');</script>";
                    return RedirectToAction("PostelReport", "CR");
                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('error!!');</script>";
                return RedirectToAction("PostelReport", "CR");
            }

        }

        public ActionResult PostelReportDaily()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }


        [HttpPost]
        public ActionResult ReportPostel_Daily(mWGE data, string optionsRadios0, string optionsRadios1, DateTime DateFrom, DateTime DateTo)
        {
            try
            {
                using(var db = new DBS_WGE_Entities())
                {

                    ReportDocument rd = new ReportDocument();
                    if (optionsRadios0 == "0")
                    {
                        
                        rd.Load(Path.Combine(Server.MapPath("~/Report"), "ReportPostel_Daily.rpt"));
                        rd.SetParameterValue("@datefrom", DateFrom.ToString("yyyy-MM-dd"));
                        rd.SetParameterValue("@dateto", DateTo.ToString("yyyy-MM-dd"));
                    }
                    else if (optionsRadios0 == "1")
                    {
                        rd.Load(Path.Combine(Server.MapPath("~/Report"), "ReportPostel_Year.rpt"));
                        rd.SetParameterValue("@mtyear1", "01"+data.year1);
                        rd.SetParameterValue("@mtyear2", "12"+data.year1);
                    }

                    rd.SetDatabaseLogon("sa", "Wge123456*", "SVERP01", "DBS_WGE");
                    
                    Response.Buffer = false;
                    Response.ClearContent();
                    Response.ClearHeaders();
                    //rd.PrintOptions.PaperOrientation = CrystalDecisions.Shared.PaperOrientation.Landscape;
                    //rd.PrintOptions.ApplyPageMargins(new CrystalDecisions.Shared.PageMargins(5, 5, 5, 5));
                    //rd.PrintOptions.PaperSize = CrystalDecisions.Shared.PaperSize.PaperA5;

                    #region log
                    string des;
                    
                    if (optionsRadios0 == "0")
                    {
                        des = "Postel Monthly Report=>" + data.month1 + data.year1;
                    }
                    else
                    {
                        des = "Postel Monthly Report=>" + data.month1 + data.year1 + "ถึง" + data.month2 + data.year2;
                    }

                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Report", System.Reflection.MethodBase.GetCurrentMethod().Name, des));
                    #endregion
                    
                    if (optionsRadios1 == "1" && optionsRadios0=="0")
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        stream.Seek(0, SeekOrigin.Begin);

                        rd.Close();
                        rd.Dispose();

                        return File(stream, "application/pdf", "ReportPostelDaily.pdf");
                    }
                    else if(optionsRadios1 != "1" && optionsRadios0 == "0")
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.Excel); //excel
                        stream.Seek(0, SeekOrigin.Begin);

                        rd.Close();
                        rd.Dispose();

                        return File(stream, "application/vnd.ms-excel", "ReportPostelDaily.xls");
                    }
                    else if (optionsRadios1 == "1" && optionsRadios0 == "1")
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        stream.Seek(0, SeekOrigin.Begin);

                        rd.Close();
                        rd.Dispose();

                        return File(stream, "application/pdf", "ReportPostelYear.pdf");
                    }
                    else if (optionsRadios1 != "1" && optionsRadios0 == "1")
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.Excel); //excel
                        stream.Seek(0, SeekOrigin.Begin);

                        rd.Close();
                        rd.Dispose();

                        return File(stream, "application/vnd.ms-excel", "ReportPostelYear.xls");
                    }
                    else
                    {
                        return RedirectToAction("PostelReportDaily", "CR");
                    }

                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = "<script>alert('error!!');</script>";
                return RedirectToAction("PostelReportDaily", "CR");
            }
        }
        
        public ActionResult GeneralJob()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
        [HttpPost]
        public ActionResult GeneralJobDownLoad(string codeno, DateTime DateFrom, DateTime DateTo, string optionsRadios0, string optionsRadios1)
        {
            DBS_WGE_Entities db = new DBS_WGE_Entities();
            DateTime DateTo1 = DateTo.AddDays(1);

            try
            {
                if (codeno != "")
                {

                    //DateTime datefrom1 = DateTime.ParseExact(DateFrom, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    //DateTime dateto1 = DateTime.ParseExact(DateTo, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    

                    for(int i=0; i <= 1; i++)
                    {
                        ReportDocument rd = new ReportDocument();
                        if (optionsRadios0 == "0")
                        {

                            rd.Load(Path.Combine(Server.MapPath("~/Report"), "GeneralJob_Report.rpt"));
                            rd.SetParameterValue("datefrom", DateFrom.ToString("dd/MM/yyyy"));
                            rd.SetParameterValue("dateto", DateTo.ToString("dd/MM/yyyy"));

                        }
                        else
                        {
                            rd.Load(Path.Combine(Server.MapPath("~/Report"), "GeneralJob_Report_EMP.rpt"));
                            rd.SetParameterValue("datefrom", DateFrom.ToString("dd/MM/yyyy"));
                            rd.SetParameterValue("dateto", DateTo.ToString("dd/MM/yyyy"));
                            rd.SetParameterValue("code", codeno);

                        }

                        Response.Buffer = false;
                        Response.ClearContent();
                        Response.ClearHeaders();

                        rd.SetDatabaseLogon("sa", "Wge123456*", "SVERP01", "DBS_WGE");

                        #region log
                        string des;

                        if (optionsRadios0 == "0")
                        {
                            des = "GenerelJob All";
                        }
                        else
                        {
                            des = "GenerelReport =>" + codeno;
                        }

                        log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/date = ({5}=>{6})", Session["Name"].ToString(), Session["Code"].ToString(), "Report", System.Reflection.MethodBase.GetCurrentMethod().Name, des, DateFrom, DateTo));
                        #endregion

                        //rd.PrintOptions.PaperOrientation = CrystalDecisions.Shared.PaperOrientation.Landscape;
                        //rd.PrintOptions.ApplyPageMargins(new CrystalDecisions.Shared.PageMargins(5, 5, 5, 5));
                        //rd.PrintOptions.PaperSize = CrystalDecisions.Shared.PaperSize.PaperA5;

                        if (optionsRadios1 == "1")
                        {
                            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                            stream.Seek(0, SeekOrigin.Begin);

                            rd.Close();
                            rd.Dispose();
                            return File(stream, "application/pdf", "GeneralReport.pdf");

                        }
                        else
                        {
                            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.Excel); //excel
                            stream.Seek(0, SeekOrigin.Begin);

                            rd.Close();
                            rd.Dispose();

                            return File(stream, "application/vnd.ms-excel", "GeneralReport.xls");

                        }
                        i++;
                    }
                    return RedirectToAction("GeneralJob", "CR");
                }
                else
                {
                    TempData["msg"] = "<script>alert('ไม่พบข้อมูล!');</script>";
                    return RedirectToAction("GeneralJob", "CR");
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                TempData["msg"] = "<script>alert('error!');</script>";
                return RedirectToAction("GeneralJob", "CR");
            }
        }

        public ActionResult OnlineJob()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
        [HttpPost]
        public ActionResult OnlineJobDownLoad(string codeno, DateTime DateFrom, DateTime DateTo, string optionsRadios0, string optionsRadios1)
        {
            DBS_WGE_Entities db = new DBS_WGE_Entities();
            DateTime DateTo1 = DateTo.AddDays(1);

            try
            {
                if (codeno != "")
                {

                    //DateTime datefrom1 = DateTime.ParseExact(DateFrom, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    //DateTime dateto1 = DateTime.ParseExact(DateTo, "yyyy-MM-dd", CultureInfo.InvariantCulture);



                    ReportDocument rd = new ReportDocument();
                    if (optionsRadios0 == "0")
                    {

                        rd.Load(Path.Combine(Server.MapPath("~/Report"), "OnlineJob.rpt"));
                        rd.SetParameterValue("datefrom", DateFrom.ToString("dd/MM/yyyy"));
                        rd.SetParameterValue("dateto", DateTo.ToString("dd/MM/yyyy"));

                    }
                    else
                    {
                        rd.Load(Path.Combine(Server.MapPath("~/Report"), "OnlineJob_EMP.rpt"));
                        rd.SetParameterValue("datefrom", DateFrom.ToString("dd/MM/yyyy"));
                        rd.SetParameterValue("dateto", DateTo.ToString("dd/MM/yyyy"));
                        rd.SetParameterValue("code", codeno);

                    }

                    Response.Buffer = false;
                    Response.ClearContent();
                    Response.ClearHeaders();

                    rd.SetDatabaseLogon("sa", "Wge123456*", "SVERP01", "DBS_WGE");

                    #region log
                    string des;

                    if (optionsRadios0 == "0")
                    {
                        des = "OnlineJob All";
                    }
                    else
                    {
                        des = "OnlineJobReport =>" + codeno;
                    }

                    log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}/date = ({5}=>{6})", Session["Name"].ToString(), Session["Code"].ToString(), "Report", System.Reflection.MethodBase.GetCurrentMethod().Name, des, DateFrom, DateTo));
                    #endregion

                    //rd.PrintOptions.PaperOrientation = CrystalDecisions.Shared.PaperOrientation.Landscape;
                    //rd.PrintOptions.ApplyPageMargins(new CrystalDecisions.Shared.PageMargins(5, 5, 5, 5));
                    //rd.PrintOptions.PaperSize = CrystalDecisions.Shared.PaperSize.PaperA5;

                    if (optionsRadios1 == "1")
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        stream.Seek(0, SeekOrigin.Begin);

                        rd.Close();
                        rd.Dispose();

                        return File(stream, "application/pdf", "OnlineReport.pdf");
                    }
                    else
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.Excel); //excel
                        stream.Seek(0, SeekOrigin.Begin);

                        rd.Close();
                        rd.Dispose();

                        return File(stream, "application/vnd.ms-excel", "OnlineReport.xls");
                    }
                }
                else
                {
                    TempData["msg"] = "<script>alert('ไม่พบข้อมูล!');</script>";
                    return RedirectToAction("GeneralJob", "CR");
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                TempData["msg"] = "<script>alert('error!');</script>";
                return RedirectToAction("GeneralJob", "CR");
            }
        }
    }
}