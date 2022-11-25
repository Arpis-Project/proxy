using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyVyV.ProxyVyV.FaceleDTE
{
    public class FaceleDocument
    {
        public FaceleDocument(string urlWebservice,string rutEmisor)
        {
            this.urlWebservice = urlWebservice;
            this.rutEmisor = rutEmisor;
            this.formato = "XML";
            this.folioDTE = 0;
        }
        public string urlWebservice { get; set; }
        public string rutEmisor { get; set; }
        public int tipoDTE { get; set; }
        public string formato { get; set; }
        public string xml { get; set; } = "";
        public int estadoOperacion { get; set; }
        public string descripcionOperacion { get; set; } = "";
        public long folioDTE { get; set; }
        public string PDF { get; set; } = "";
        public string fechaEmision { get; set; } = "";
        public string ted { get; set; } = "";
        public string responseXml { get; set; } = "";
        public string responseUrl { get; set; }
        public byte[] pdf64 { get; set; }
    }
}
