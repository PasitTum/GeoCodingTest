using GeoCodingTest.Class;
using GeoCodingTest.Models.L1;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static GeoCodingTest.Models.L2.DBSWGE2;

namespace GeoCodingTest.Controllers
{
    public class MapController : Controller
    {
        InterfaceLog log = new InterfaceLog(ConfigurationManager.AppSettings["LogsPath"]);
        //DateTime date2020 = new DateTime(2020, 07, 01);
        DateTime date2020 = DateTime.ParseExact(ConfigurationManager.AppSettings["Date"], "yyyy-MM-dd", CultureInfo.InvariantCulture);

        // GET: Map
        public ActionResult MapView()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            mWGE mData = new mWGE();

            try
            {              
                using (var db = new DBS_WGE_Entities())
                {
                    int empCode = Convert.ToInt32(Session["EMPNo"]);

                    var oData = (from a in db.C_MCT_MAP
                                 where (1 == 1)
                                 select new C_MCT_MAP2
                                 {
                                     ID = a.ID,
                                     PathImg = a.PathImg,
                                     CardCode = a.CardCode,
                                     CardName = a.CardName,
                                     Address = a.Address,
                                     Remark = a.Remark,
                                     SubmitDate = a.SubmitDate
                                 });

                    oData = oData.OrderByDescending(o => o.ID);

                    if (oData != null)
                    {
                        mData.ListMap = oData.ToList();
                        //log.WriteLog(string.Format("{0}({1}) : {2}", Session["Name"].ToString(), Session["Code"].ToString(), "Map View"));
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
        public ActionResult getMapView(HttpPostedFileBase file, string cardcode, string cardname, string address, string remark,string button,int id)
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using (var db = new DBS_WGE_Entities())
                {
                    if (button == "2" && file != null)
                    {
                        //save path
                        string filename = DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg";
                        var path = AppDomain.CurrentDomain.BaseDirectory + "Image";
                        var path2 = Path.Combine(path, filename);
                        file.SaveAs(path2);
                        string path3 = string.Format("~/Image/{0}", filename);

                        C_MCT_MAP dbs = new C_MCT_MAP();

                        dbs.PathImg = path3;
                        dbs.CardCode = cardcode;
                        dbs.CardName = cardname;
                        dbs.Address = address;
                        dbs.Remark = remark;
                        dbs.SubmitNo = Session["Code"].ToString();
                        dbs.SubmitDate = DateTime.Now;

                        db.C_MCT_MAP.Add(dbs);
                        db.SaveChanges();

                        log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Add "+id+"/"+cardcode+"/"+cardname, System.Reflection.MethodBase.GetCurrentMethod().Name, "success"));
                        TempData["msg"] = "<script>alert('Success');</script>";
                    } //เพิ่ม
                    else if (button == "1")
                    {
                        string path3 = "";

                        if (file != null)
                        {
                            //save path แทรกอันใหม่
                            string filename = DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg";
                            var path = AppDomain.CurrentDomain.BaseDirectory + "Image";
                            var path2 = Path.Combine(path, filename);
                            file.SaveAs(path2);

                            path3 = string.Format("~/Image/{0}", filename);
                        }

                        var oData = (from a in db.C_MCT_MAP
                                     where (a.ID == id)
                                     select a).FirstOrDefault();

                        if (file != null)
                        {    
                            //ลบรูปภาพเดิม 
                            string filename = oData.PathImg;
                            var path = AppDomain.CurrentDomain.BaseDirectory;
                            string str = filename.Replace("~/", path);

                            if (System.IO.File.Exists(str))
                            {
                                System.IO.File.Delete(str);
                            }

                            oData.PathImg = path3;
                        }

                        oData.CardCode = cardcode;
                        oData.CardName = cardname;
                        oData.Address = address;
                        oData.Remark = remark;
                        oData.SubmitNo = Session["Code"].ToString();
                        oData.SubmitDate = DateTime.Now;

                        db.SaveChanges();
                        log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Update " + id + "/" + cardcode + "/" + cardname, System.Reflection.MethodBase.GetCurrentMethod().Name, "success"));
                        TempData["msg"] = "<script>alert('Success');</script>";
                    } //แก้ไข
                    else if (button == "0")
                    {     
                        //ลบรูปภาพ
                        var oData = (from a in db.C_MCT_MAP
                                     where a.ID == id
                                     select a).FirstOrDefault();

                        string filename = oData.PathImg;
                        var path = AppDomain.CurrentDomain.BaseDirectory;
                        string str = filename.Replace("~/", path);                        

                        db.C_MCT_MAP.Remove(oData);

                        if (System.IO.File.Exists(str))
                        {
                            System.IO.File.Delete(str);
                        }

                        db.SaveChanges();
                        log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Delete " + id + "/" + cardcode + "/" + cardname, System.Reflection.MethodBase.GetCurrentMethod().Name, "success"));
                        TempData["msg"] = "<script>alert('Success');</script>";
                    } //ลบ
                }
            }
            catch (Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.ToString()));
                TempData["msg"] = "<script>alert('Error');</script>";
            }


            if (button == "2")
                return RedirectToAction("MapInsert","Map");
            else
                return RedirectToAction("MapView", "Map");
        }

        public ActionResult MapInsert()
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }


            return View();
        }

        [HttpPost]
        public ActionResult MapEdit(string cardcode,mWGE mData)
        {
            if (Session["Name"] == null && (Session["Position"].ToString() == "8" || Session["Department"].ToString() == "3" || Session["Department"].ToString() == "4" || Session["Department"].ToString() == "1"))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using (var db = new DBS_WGE_Entities())
                {
                    var oData = (from a in db.C_MCT_MAP
                                 where (a.CardCode == cardcode)
                                 select new C_MCT_MAP2
                                 {
                                     ID = a.ID,
                                     PathImg = a.PathImg,
                                     CardCode = a.CardCode,
                                     CardName = a.CardName,
                                     Address = a.Address,           
                                     Remark = a.Remark,
                                     SubmitNo = a.SubmitNo,
                                     SubmitDate = a.SubmitDate
                                 });

                    if (oData != null)
                    {
                        mData.ListMap = oData.ToList();
                        //log.WriteLog(string.Format("{0}({1}) : {2}", Session["Name"].ToString(), Session["Code"].ToString(), "Map Edit"));
                    }
                }
            }
            catch(Exception ex)
            {
                log.WriteLog(string.Format("{0}({1}) : {2} {3}/{4}", Session["Name"].ToString(), Session["Code"].ToString(), "Error", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString()));
            }

            return View(mData);
        }
    }
}