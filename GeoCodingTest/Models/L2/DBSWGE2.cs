using GeoCodingTest.Models.L1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeoCodingTest.Models.L2
{
    public class DBSWGE2
    {
        public class OUDP2 : OUDP
        {

        }

        public class OHEM2 : OHEM
        {

        }

        public class OSLP2 : OSLP
        {

        }

        public class OCRD2 : OCRD
        {
            public string PymntGroup { get; set; }
            public Nullable<System.DateTime> DocDueDate { get; set; }
        }

        public class ORIN2 : ORIN
        {

        }

        public class OINV2 : OINV
        {
            public int ID { get; set; }
            public Nullable<int> InvNo { get; set; }
            public Nullable<System.DateTime> DateBill { get; set; }
            public Nullable<System.DateTime> DueDateBill { get; set; }
            public Nullable<decimal> Total { get; set; }
            public string Active { get; set; }
            public string FatherName { get; set; }
            public string PymntGroup { get; set; }
            public string U_WGE_Billing { get; set; }
            public string U_WGE_BillingIns { get; set; }
            public string U_WGE_PayDetail { get; set; }
            public string Refno { get; set; }
        }       

        public class LocationTest2 : LocationTest
        {

        }

        public class C_MCT_BILLING2 : C_MCT_BILLING
        {
            public string FatherCard { get; set; }
            public Nullable<int> CreditNo { get; set; }
            public string Type { get; set; }
            public string DateFrom { get; set; }
            public string DateTo { get; set; }
            public string MonthName { get; set; }
            public string Year { get; set; }
            public int Days { get; set; }
            public int DayofWeek { get; set; }
        }

        public class C_MCT_RECEIVE2 : C_MCT_RECEIVE
        {
            public string Detail { get; set; }
            public string FatherCard { get; set; }
        }

        public class C_MCT_CREDITNOTE2 : C_MCT_CREDITNOTE
        {
            public string FatherCard { get; set; }
        }

        public class C_MCT_OTHER2 : C_MCT_OTHER
        {
            public string FatherCard { get; set; }
        }

        public class C_MCT_DOCUMENT2 : C_MCT_DOCUMENT
        {

        }

        public class C_MCT_MAP2 : C_MCT_MAP
        {

        }

        public class OCTG2 : OCTG
        {

        }

        public class C_MCT_BILL_WORKORDER2 : C_MCT_BILL_WORKORDER
        {
            public string date { get; set; }
            public string time { get; set; }
            public string date2 { get; set; }
            public string time2 { get; set; }
            public string des1 { get; set; }
            public string des2 { get; set; }
            public string des3 { get; set; }
            public string button { get; set; }
            //
            public string Type { get; set; }
            public string Description { get; set; }
            //
            public string Bank_Name { get; set; }
            public string Option { get; set; }
            public string Other { get; set; }
            //
            public string TypeP { get; set; }
            public string Name { get; set; }
            public string Postel { get; set; }
            public string Itemcode { get; set; }
            public decimal price { get; set; }
            public string wk_path_img { get; set; }
            public DateTime date_print { get; set; }
            public string EMSRev { get; set; }
            public int ID_detail { get; set; }
            public string jobtitle { get; set; }
            public string AC_Code { get; set; }
            public int Postel_ID { get; set; }
        }

        public class C_MCT_BILL_BANK2 : C_MCT_BILL_BANK
        {

        }

        public class C_MCT_BILL_GENERAL2 : C_MCT_BILL_GENERAL
        {
            
        }

        public class C_MCT_BILL_POSTOFFICE2 : C_MCT_BILL_POSTOFFICE
        {

        }
    }
}