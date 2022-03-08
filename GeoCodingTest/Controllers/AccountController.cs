using GeoCodingTest.Class;
using GeoCodingTest.Models.L1;
using GeoCodingTest.Models.L2;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GeoCodingTest.Controllers
{
    public class AccountController : Controller
    {
        InterfaceLog log = new InterfaceLog(ConfigurationManager.AppSettings["LogsPath"]);

        // GET: Account
        public ActionResult Login()
        {
            Session.Clear();
            Session.Abandon();

            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public ActionResult LoginCheck(string ID, string Password)
        {
            try
            {
                bool valid = false;
                using (PrincipalContext context = new PrincipalContext(ContextType.Domain))
                {
                    valid = context.ValidateCredentials(ID, Password);

                    using (var db = new DBS_WGE_Entities())
                    {
                        using (UserPrincipal user = UserPrincipal.FindByIdentity(context, ID)) //ดึงชื่อจาก AD
                        {                           
                            if (user != null)
                            {                                
                                if (valid == false)
                                {
                                    if (user.IsAccountLockedOut())
                                    {
                                        TempData["msg"] = "<script>alert('รหัสผ่านถูกล็อคกรุณาติดต่อฝ่าย IT');</script>";
                                    }
                                    else
                                    {
                                        TempData["msg"] = "<script>alert('รหัสผ่านไม่ถูกต้อง');</script>";
                                    }
                                }
                                else
                                {
                                    var de = (DirectoryEntry)user.GetUnderlyingObject();
                                    string usercode = "";
                                    //usercode = "46005";
                                    usercode = de.Properties["initials"][0].ToString();
                                    //string a = de.Properties["userPrincipalName"][0].ToString();

                                    OHEM mData = db.OHEM.Where(o => o.ExtEmpNo.Equals(usercode)).FirstOrDefault();
                                    
                                        Session["Code"] = mData.ExtEmpNo;
                                        Session["Name"] = mData.firstName + " " + mData.lastName;
                                    if(ID== "sirikorn.t")
                                    {
                                        Session["Job"] = "Finance Officer";
                                    }
                                    else
                                    {
                                        Session["Job"] = mData.jobTitle;
                                    }
                                        Session["Position"] = mData.position;
                                        Session["Department"] = mData.dept;

                                        if (Session["Position"] == null)
                                        {
                                            Session["Position"] = 99;
                                        }
                                        log.WriteLog(string.Format("{0}({1}) : {2}", Session["Name"].ToString(), Session["Code"].ToString(), "Login to Bill"));
                                        return RedirectToAction("Index", "Home");

                                    /*}*/
                                }
                            }
                            else
                            {
                                TempData["msg"] = "<script>alert('ไม่พบผู้ใช้');</script>";
                            }
                        }
                    }
                }                
            }
            catch (Exception ex)
            {
                log.WriteLog(ex.Message.ToString());
                TempData["msg"] = "<script>alert('โปรดติดต่อแผนก IT เพื่อทำการเพิ่มรหัสเข้าใช้งาน');</script>";
            }

            return RedirectToAction("Login");
            
        }
    }
}
