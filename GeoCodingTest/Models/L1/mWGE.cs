using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static GeoCodingTest.Models.L2.DBSWGE2;

namespace GeoCodingTest.Models.L1
{
    public class mWGE
    {
        public List<int> N { get; set; }
        public int round { get; set; } 
        public DateTime datefrom { get; set; }
        public DateTime dateto { get; set; }
        public string [] chkbox { get; set; }
        public string checkbox { get; set; }
        public DateTime date { get; set; }
        public List<int> InvnoBilling { get; set; }
        public string Code { get; set; }
        public int DocNum { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string Address { get; set; }
        public string ZipCode { get; set; }
        public string type_wk { get; set; }
        public string Workorder { get; set; }
        public string month1 { get; set; }
        public string month2 { get; set; }
        public string year1 { get; set; }
        public string year2 { get; set; }
        public Nullable<System.DateTime> DocDate { get; set; }
        public Nullable<System.DateTime> DocDueDate { get; set; }
        public Decimal DocTotal { get; set; }
        public List<LocationTest2> ListLocation { get; set; }
        public List<OINV2> ListOINV { get; set; }
        public List<ORIN2> ListORIN { get; set; }
        public List<C_MCT_BILLING2> ListBilling { get; set; }
        public List<C_MCT_RECEIVE2> ListReceive { get; set; }
        public List<C_MCT_CREDITNOTE2> ListCreditNote { get; set; }
        public List<C_MCT_OTHER2> ListOther { get; set; }
        public List<C_MCT_DOCUMENT2> ListDocument { get; set; }
        public List<C_MCT_MAP2> ListMap { get; set; }
        public List<C_MCT_BILLING2> ListSetting { get; set; }
        public List<OCRD2> ListOCRD { get; set; }
        public C_MCT_BILL_WORKORDER2 BW { get; set; }
        public C_MCT_BILL_GENERAL2 BG { get; set; }
        public C_MCT_BILL_BANK2 BB { get; set; }
        public C_MCT_BILL_POSTOFFICE2[] BP { get; set; }
        public List<C_MCT_BILL_WORKORDER> ListBW { get; set; }
        public List<C_MCT_BILL_WORKORDER> ListBW1 { get; set; }
        public List<C_MCT_BILL_WORKORDER2> ListBW2 { get; set; }
        public List<C_MCT_BILL_POSTOFFICE> ListPO1 { get; set; }
        public List<OUDP> ListOUDP { get; set; }
        public C_MCT_POSTOFFICE_REF PF { get; set; }
        public string MS_1 { get; set; }
        public string MS_2 { get; set; }
        public string MS_3 { get; set; }
        public string MS_4 { get; set; }
        public List<string> dev { get; set; }
        
    }
}