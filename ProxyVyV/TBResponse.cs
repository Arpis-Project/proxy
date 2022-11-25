namespace ProxyVyV
{
    using System;
    using System.Runtime.CompilerServices;

    public class TBResponse
    {
        public TBResponse()
        {
            this.code = 0;
            this.strMessage = string.Empty;
            this.invc_sid = string.Empty;
            this.tender_type = string.Empty;
            this.tender_name = string.Empty;
            this.authorization_code = string.Empty;
            this.registry_date = string.Empty;
            this.datatarjeta = string.Empty;
        }

        public int code { get; set; }

        public string strMessage { get; set; }

        public string invc_sid { get; set; }

        public string tender_type { get; set; }

        public string tender_name { get; set; }

        public string authorization_code { get; set; }

        public string registry_date { get; set; }

        public string datatarjeta { get; set; }

        public string cuotas { get; set; }

        public string numero_tarjeta { get; set; }


    }
}

