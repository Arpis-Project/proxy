using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyVyV.ProxyVyV.FaceleDTE
{
    public static class FaceleProcessor
    {
        public static string ObtieneURLWS()
        {
            DteFacele.DoceleOL doceleOL = new DteFacele.DoceleOL();
            return doceleOL.Url;
        }
        public static void GeneraDTE(ref FaceleDocument faceleDoc)
        {
            try
            {
                DteFacele.DoceleOL doceleOL = new DteFacele.DoceleOL();
                string rutEmisor = faceleDoc.rutEmisor;
                int tipoDocumento = faceleDoc.tipoDTE;
                string descripcionOperacion = "";
                long folioDTE;
                bool folioDteSpecified;
                int resultado = doceleOL.generaDTE(ref rutEmisor, ref tipoDocumento, DteFacele.generaDTEFormato.XML, "", faceleDoc.xml, "", out descripcionOperacion, out folioDTE, out folioDteSpecified);
                faceleDoc.estadoOperacion = resultado;
                faceleDoc.descripcionOperacion = descripcionOperacion;
                if(resultado > 0)
                {
                    faceleDoc.folioDTE = folioDTE;
                }
            }
            catch (Exception ex)
            {
                faceleDoc.estadoOperacion = 0;
                faceleDoc.descripcionOperacion = $"GeneraDTE Exception: {ex.Message}";
                return;
            }
        }
        public static void ConfirmaDTE(ref FaceleDocument faceleDoc)
        {
            try
            {
                DteFacele.DoceleOL doceleOL = new DteFacele.DoceleOL();
                string descripcionOperacion = "";
                string stringXml = "";
                byte[] pdf64;
                string responseUrl = "";
                doceleOL.obtieneDTE(faceleDoc.rutEmisor, faceleDoc.tipoDTE, faceleDoc.folioDTE, DteFacele.obtieneDTEFormato.XML, false, false, 1, true, out descripcionOperacion, out stringXml, out pdf64, out responseUrl);
                faceleDoc.responseXml = stringXml;
                //doceleOL.obtieneDTE(faceleDoc.rutEmisor, faceleDoc.tipoDTE, faceleDoc.folioDTE, DteFacele.obtieneDTEFormato.PDF, false, false, 1, true, out descripcionOperacion, out stringXml, out pdf64, out responseUrl);
                //faceleDoc.pdf64 = pdf64;
                doceleOL.obtieneDTE(faceleDoc.rutEmisor, faceleDoc.tipoDTE, faceleDoc.folioDTE, DteFacele.obtieneDTEFormato.URL_PDF, false, false, 1, true, out descripcionOperacion, out stringXml, out pdf64, out responseUrl);
                faceleDoc.responseUrl = responseUrl;
            }
            catch (Exception ex)
            {
                faceleDoc.estadoOperacion = 0;
                faceleDoc.descripcionOperacion = $"ConfirmaDTE Exception: {ex.Message}";
                return;
            }
        }
    }
}
