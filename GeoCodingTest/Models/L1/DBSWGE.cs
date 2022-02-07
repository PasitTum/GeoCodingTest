using GeoCodingTest.Models.L2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeoCodingTest.Models.L1
{
    public class DBSWGE
    {
        public List<DBSWGE2.OCRD2> OCRD1 { get; set; }
        public List<DBSWGE2.ORIN2> ORIN1 { get; set; }
        public List<DBSWGE2.OSLP2> OSLP1 { get; set; }
        public List<DBSWGE2.OINV2> OINV1 { get; set; }
        public List<DBSWGE2.OHEM2> OHEM1 { get; set; }

        //public List<DBSWGE2.LocationTest2> LocationTest1 { get; set; }
    }
}