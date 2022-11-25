using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyVyV.ProxyVyV.DTEFacCLResponse
{

    public class FacCLDTEResponse
    {
        public Xml xml { get; set; }
        public WSPLANO WSPLANO { get; set; }
    }

    public class Xml
    {
        public string version { get; set; }
    }

    public class WSPLANO
    {
        public string Resultado { get; set; }
        public string Mensaje { get; set; }
        public Detalle Detalle { get; set; }
    }

    public class Detalle
    {
        public Documento Documento { get; set; }
    }

    public class Documento
    {
        public string Folio { get; set; }
        public string TipoDte { get; set; }
        public string Operacion { get; set; }
        public DateTime Fecha { get; set; }
        public string Resultado { get; set; }
        public string Error { get; set; }
        public string urlOriginal { get; set; }
    }



}
