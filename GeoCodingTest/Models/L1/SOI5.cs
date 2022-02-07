//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GeoCodingTest.Models.L1
{
    using System;
    using System.Collections.Generic;
    
    public partial class SOI5
    {
        public int WizardId { get; set; }
        public int SOINum { get; set; }
        public int DocType { get; set; }
        public int DocEntry { get; set; }
        public int SOILineNum { get; set; }
        public int DocLineNum { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string unitMsr { get; set; }
        public Nullable<decimal> Quantity { get; set; }
        public Nullable<decimal> TotalFrgn { get; set; }
        public Nullable<decimal> LineTotal { get; set; }
        public string Currency { get; set; }
        public Nullable<decimal> Rate { get; set; }
        public Nullable<decimal> BaseRate { get; set; }
        public string BasNumAtCr { get; set; }
        public Nullable<int> BaseDocNum { get; set; }
        public Nullable<int> BaseEntry { get; set; }
        public Nullable<System.DateTime> BasTaxDate { get; set; }
        public Nullable<decimal> FExcBasSum { get; set; }
        public Nullable<decimal> AExcBasSum { get; set; }
        public Nullable<int> ExciseUoM { get; set; }
        public Nullable<decimal> ExcRateUoM { get; set; }
        public Nullable<decimal> ExcRateAdV { get; set; }
        public Nullable<decimal> ExciseSum { get; set; }
        public Nullable<decimal> VatBaseSum { get; set; }
        public Nullable<decimal> VatPrcnt { get; set; }
        public Nullable<decimal> VatSum { get; set; }
        public string TaxCtgr { get; set; }
        public string VatGroup { get; set; }
    }
}