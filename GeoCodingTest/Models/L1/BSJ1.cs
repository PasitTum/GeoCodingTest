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
    
    public partial class BSJ1
    {
        public int ID { get; set; }
        public int TaskID { get; set; }
        public string Type { get; set; }
        public string Params { get; set; }
        public Nullable<short> RunAs { get; set; }
        public string Status { get; set; }
        public Nullable<int> PostTask { get; set; }
        public Nullable<int> PreTask { get; set; }
        public Nullable<System.DateTime> LastDate { get; set; }
        public Nullable<short> LastTime { get; set; }
        public Nullable<int> Retry { get; set; }
        public Nullable<int> TimeOut { get; set; }
        public string Message { get; set; }
    }
}
