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
    
    public partial class OBSJ
    {
        public int ID { get; set; }
        public string Type { get; set; }
        public string Desc { get; set; }
        public string Status { get; set; }
        public string Schedule { get; set; }
        public Nullable<System.DateTime> NextDate { get; set; }
        public Nullable<short> NextTime { get; set; }
        public string BFParams { get; set; }
        public string EFParams { get; set; }
        public Nullable<int> BFRetry { get; set; }
        public Nullable<int> EFRetry { get; set; }
        public Nullable<short> RunAs { get; set; }
        public string Message { get; set; }
        public string RunType { get; set; }
    }
}
