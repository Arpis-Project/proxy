namespace ProxyVyV
{
    using System;
    using System.Runtime.CompilerServices;

    public class OPResponse
    {
        public OPResponse()
        {
            this.code = 0;
            this.strMessage = string.Empty;
            this.exterrormsg = string.Empty;
            this.errormsg = string.Empty;
            this.occ = string.Empty;
            this.externalUniqueNumber = string.Empty;
            this.issuedAt = string.Empty;
            this.amount = string.Empty;
            this.ReverseCode = string.Empty;
        }

        public int code { get; set; }

        public string strMessage { get; set; }

        public string occ { get; set; }

        public string externalUniqueNumber { get; set; }

        public string issuedAt { get; set; }

        public string amount { get; set; }

        public string ReverseCode { get; set; }

        public string exterrormsg { get; set; }

        public string errormsg { get; set; }
    }
}

