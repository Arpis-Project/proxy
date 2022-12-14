using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ProxyVyV.ProxyVyV.GetOneDTE
{
    public class DteDocument
    {
        public DteDocument()
        {
            RequestDomain = "ASSA-CERT";
            TipoDTE = 0;
            Folio = null;
            FechaEmision = DateTime.Now.ToString("yyyy-MM-dd");
            FechaVencimiento = DateTime.Now.ToString("yyyy-MM-dd");
            IndicadorServicio = 3;
            IndTraslado = null;
            TipoDespacho = null;
            FormaPago = 1;
            GlosaTermPago = String.Empty;
            FechaVencimiento = String.Empty;
            DireccionOrigen = String.Empty;
            ComunaOrigen = String.Empty;
            CiudadOrigen = String.Empty;
            CodigoVendedor = String.Empty;
            RutReceptor = String.Empty;
            CodigoInternoReceptor = String.Empty;
            IdAdicionalEmisor = String.Empty;
            RazonSocialReceptor = String.Empty;
            GiroReceptor = String.Empty;
            ContactoReceptor = String.Empty;
            CorreoReceptor = String.Empty;
            DireccionReceptor = String.Empty;
            ComunaReceptor = String.Empty;
            CiudadReceptor = String.Empty;
            DireccionDestino = String.Empty;
            ComunaDestino = String.Empty;
            CiudadDestino = String.Empty;
            PatenteTransporte = String.Empty;
            CondicionVenta = "ANTICIPADO";
            MontoNeto = 0;
            MontoExento = null;
            TasaIVA = 0;
            IVA = 0;
            MontoTotal = 0;
            xNtraRef = String.Empty;
            xNInterna = String.Empty;
            xOC = String.Empty;
            xMontoEscrito = String.Empty;
            xImpresora = String.Empty;
            xObservaciones1 = String.Empty;
            xObservaciones2 = String.Empty;
            DteDetalles = new List<DteDetail>();
            DteDescuentosRecargos = new List<DteDescRec>();
            DteReferencias = new List<DteRef>();
        }
        public string RequestDomain;
        public float TipoDTE { get; set; }
        public float? Folio { get; set; }
        public string FechaEmision { get; set; }
        public int IndicadorServicio { get; set; }
        public float? IndTraslado { get; set; }
        public float? TipoDespacho { get; set; }
        public float FormaPago { get; set; }
        public string GlosaTermPago { get; set; }
        public string CondicionVenta { get; set; }
        public string FechaVencimiento { get; set; }
        public string DireccionOrigen { get; set; }
        public string ComunaOrigen { get; set; }
        public string CiudadOrigen { get; set; }
        public string CodigoVendedor { get; set; }
        public string RutReceptor { get; set; }
        public string CodigoInternoReceptor { get; set; }
        public string IdAdicionalEmisor { get; set; }
        public string RazonSocialReceptor { get; set; }
        public string GiroReceptor { get; set; }
        public string ContactoReceptor { get; set; }
        public string CorreoReceptor { get; set; }
        public string DireccionReceptor { get; set; }
        public string ComunaReceptor { get; set; }
        public string CiudadReceptor { get; set; }
        public string DireccionDestino { get; set; }
        public string ComunaDestino { get; set; }
        public string CiudadDestino { get; set; }
        public string PatenteTransporte { get; set; }
        public decimal MontoNeto { get; set; }
        public decimal? MontoExento { get; set; }
        public decimal TasaIVA { get; set; }
        public decimal IVA { get; set; }
        public decimal MontoTotal { get; set; }
        public string xNtraRef { get; set; }
        public string xNInterna { get; set; }
        public string xOC { get; set; }
        public string xMontoEscrito { get; set; }
        public string xImpresora { get; set; }
        public string xObservaciones1 { get; set; }
        public string xObservaciones2 { get; set; }
        public List<DteDetail> DteDetalles { get; set; }
        public List<DteDescRec> DteDescuentosRecargos { get; set; }
        public List<DteRef> DteReferencias { get; set; }

        public class DteDetail
        {
            public DteDetail()
            {
                NroLinea = 0;
                NombreItem = String.Empty;
                TipoCodigo = "INT1";
                CodigoItem = String.Empty;
                IndicadorExencion = String.Empty;
                Cantidad = 0;
                UnidadMedida = String.Empty;
                PrecioUnitario = 0;
                DescripcionAdicional = String.Empty;
                MontoItem = 0;
                DctoPorcentaje = null;
                DctoMonto = null;
            }
            public float NroLinea { get; set; }
            public string NombreItem { get; set; }
            public string TipoCodigo { get; set; }
            public string CodigoItem { get; set; }
            public string IndicadorExencion { get; set; }
            public decimal Cantidad { get; set; }
            public string UnidadMedida { get; set; }
            public decimal PrecioUnitario { get; set; }
            public string DescripcionAdicional { get; set; }
            public decimal MontoItem { get; set; }
            public decimal? DctoPorcentaje { get; set; }
            public decimal? DctoMonto { get; set; }
        }
        public class DteDescRec
        {
            public DteDescRec()
            {
                NroDescRecargo = 0;
                TipoMovimiento = String.Empty;
                GlosaDescRecargo = String.Empty;
                TipoValor = String.Empty;
                ValorDescRecargo = 0;
            }
            public float NroDescRecargo { get; set; }
            public string TipoMovimiento { get; set; }
            public string GlosaDescRecargo { get; set; }
            public string TipoValor { get; set; }
            public decimal ValorDescRecargo { get; set; }
        }
        public class DteRef
        {
            public DteRef()
            {
                NroLineaReferencia = 0;
                TipoDocumentoReferencia = 0;
                CodigoReferencia = 0;
                RazonReferencia = String.Empty;
                FolioReferencia = String.Empty;
                FechaReferencia = String.Empty;
            }
            public float NroLineaReferencia { get; set; }
            public float TipoDocumentoReferencia { get; set; }
            public float CodigoReferencia { get; set; }
            public string RazonReferencia { get; set; }
            public string FolioReferencia { get; set; }
            public string FechaReferencia { get; set; }
        }
        public bool GeneraBloqueDTE(out string resultado)
        {
            resultado = "";
            //string pathFormatoDocumento = $@"{AppDomain.CurrentDomain.BaseDirectory}DteFormats\{this.TipoDTE}.txt";
            string formatoDocumento = GetOnePlantillaTxt.Cabecera(this.TipoDTE.ToString());
            //if (!File.Exists(pathFormatoDocumento))
            //{
            //    resultado = "Archivo formato DTE no existe: " + pathFormatoDocumento;
            //    return false;
            //}
            //formatoDocumento = File.ReadAllText(pathFormatoDocumento);
            foreach (var prop in this.GetType().GetProperties())
            {
                var a = prop.GetValue(this, null);
                if(prop.Name.ToUpper() == "MONTONETO" || prop.Name.ToUpper() == "TASAIVA" || prop.Name.ToUpper() == "IVA")
                    formatoDocumento = formatoDocumento.Replace($"#{prop.Name.ToUpper()}#", Convert.ToString(prop.GetValue(this, null)).Replace(",", "."));
                else
                    formatoDocumento = formatoDocumento.Replace($"#{prop.Name.ToUpper()}#", Convert.ToString(prop.GetValue(this, null)));
            }
            if (this.DteDetalles.Count > 0)
            {
                //string pathDetalleDocumento = $@"{AppDomain.CurrentDomain.BaseDirectory}DteFormats\DET.txt";
                string detalleDocumento = GetOnePlantillaTxt.Detalle();
                //if (!File.Exists(pathDetalleDocumento))
                //{
                //    resultado = "Archivo formato detalle DTE no existe: " + pathDetalleDocumento;
                //    return false;
                //}
                //detalleDocumento = File.ReadAllText(pathDetalleDocumento);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<DETALLE>");
                sb.AppendLine("Nro.Linea|Cantidad|Tipo Codigo|Codigo del Item|Indicador Exencion|Unidad de Medida|Nombre del Item|Precio Unitario Item|Descuento %|Monto Descuento|Monto Item");
                foreach (var item in this.DteDetalles)
                {
                    string _formato = detalleDocumento;
                    foreach (var prop in item.GetType().GetProperties())
                    {
                        var a = prop.GetValue(item, null);
                        if (prop.Name.ToUpper() == "CANTIDAD" || prop.Name.ToUpper() == "PRECIOUNITARIO" || prop.Name.ToUpper() == "MONTOITEM"
                            || prop.Name.ToUpper() == "DCTOPORCENTAJE" || prop.Name.ToUpper() == "DCTOMONTO")
                            _formato = _formato.Replace($"#{prop.Name.ToUpper()}#", Convert.ToString(prop.GetValue(item, null)).Replace(",", "."));
                        else
                            _formato = _formato.Replace($"#{prop.Name.ToUpper()}#", Convert.ToString(prop.GetValue(item, null)));
                    }
                    sb.AppendLine(_formato);
                }
                formatoDocumento = formatoDocumento + "\r\n" + sb.ToString();
            }
            if (this.DteDescuentosRecargos.Count > 0)
            {
                //string pathDRDocumento = $@"{AppDomain.CurrentDomain.BaseDirectory}DteFormats\DR.txt";
                string drDocumento = GetOnePlantillaTxt.DescuentosRecargos();
                //if (!File.Exists(pathDRDocumento))
                //{
                //    resultado = "Archivo formato descuento recargos DTE no existe: " + pathDRDocumento;
                //    return false;
                //}
                //drDocumento = File.ReadAllText(pathDRDocumento);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<DESCUENTOS O RECARGOS>");
                sb.AppendLine("Nro Desc. Recargo|Tipo Movimiento|Glosa Desc. Recargo|Tipo Valor|Valor Desc. Recargo");
                foreach (var item in this.DteDescuentosRecargos)
                {
                    string _formato = drDocumento;
                    foreach (var prop in item.GetType().GetProperties())
                    {
                        var a = prop.GetValue(item, null);
                        if (prop.Name.ToUpper() == "VALORDESCRECARGO")
                            _formato = _formato.Replace($"#{prop.Name.ToUpper()}#", Convert.ToString(prop.GetValue(item, null)).Replace(",", "."));
                        else
                            _formato = _formato.Replace($"#{prop.Name.ToUpper()}#", Convert.ToString(prop.GetValue(item, null)));
                    }
                    sb.AppendLine(_formato);
                }
                formatoDocumento = formatoDocumento + sb.ToString();
            }
            if (this.DteReferencias.Count > 0)
            {
                //string pathRefDocumento = $@"{AppDomain.CurrentDomain.BaseDirectory}DteFormats\REF.txt";
                string refDocumento = GetOnePlantillaTxt.Referencias();
                //if (!File.Exists(pathRefDocumento))
                //{
                //    resultado = "Archivo formato referencias DTE no existe: " + pathRefDocumento;
                //    return false;
                //}
                //refDocumento = File.ReadAllText(pathRefDocumento);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<REFERENCIA>");
                sb.AppendLine("Nro Linea Referencia|Tipo Documento Referencia|Fecha Referencia|Folio Referencia|Codigo Referencia");
                foreach (var item in this.DteReferencias)
                {
                    string _formato = refDocumento;
                    foreach (var prop in item.GetType().GetProperties())
                    {
                        var a = prop.GetValue(item, null);
                        _formato = _formato.Replace($"#{prop.Name.ToUpper()}#", Convert.ToString(prop.GetValue(item, null)));
                    }
                    sb.AppendLine(_formato);
                }
                formatoDocumento = formatoDocumento + sb.ToString();
            }
            resultado = formatoDocumento;
            return true;
        }
        public bool EmiteDTE(string bloqueDTE,out string msgError, out string folio,out string PDF, out string ted, out string respuesta, out string xmlRes)
        {
            msgError = "";
            respuesta = "";
            xmlRes = "";
            bool resultado = false;
            DteGetOneWS.processDocumentRequest req = new DteGetOneWS.processDocumentRequest();
            req.domain = this.RequestDomain;
            req.doctype = this.TipoDTE.ToString();
            req.id = "";
            req.input = bloqueDTE;
            DteGetOneWS.processDocumentResponse res = new DteGetOneWS.processDocumentResponse();
            folio = "0";
            ted = "";
            //string resultMessage = "";
            PDF = "";
            int stepCounter = 0;
            try
            {
                DteGetOneWS.GOServicePortService portService = new DteGetOneWS.GOServicePortService();
                res = portService.processDocument(req);
                respuesta = res.@return;
                stepCounter = 1;
                msgError = GetNode(res.@return, "mensaje");
                stepCounter = 2;
                string _kdoc = GetNode(res.@return, "doc");
                stepCounter = 3;
                PDF = GetNode(res.@return, "pdf");
                stepCounter = 4;
                _kdoc = _kdoc.Replace("&lt;", "<");
                _kdoc = _kdoc.Replace("&gt;", ">");
                xmlRes = _kdoc;
                stepCounter = 5;
                folio = GetNode(_kdoc, "folio");
                stepCounter = 6;
                ted = retornaTED(_kdoc);
                stepCounter = 7;

                //GetOneResponse _respuesta = new GetOneResponse();
                //XmlSerializer serializer = new XmlSerializer(typeof(GetOneResponse));
                //using (StringReader reader = new StringReader(res.@return))
                //{
                //    _respuesta = (GetOneResponse)(serializer.Deserialize(reader));
                //    resultMessage = _respuesta.mensaje;
                //    try
                //    {
                //        XmlNode[] node = _respuesta.doc as XmlNode[];
                //        GetOneResponseDTE _dte = new GetOneResponseDTE();
                //        XmlSerializer _serializer = new XmlSerializer(typeof(GetOneResponseDTE));
                //        using (StringReader _reader = new StringReader(node[0].InnerText))
                //        {
                //            _dte = (GetOneResponseDTE)(_serializer.Deserialize(_reader));
                //            folio = _dte.Documento.Encabezado.IdDoc.Folio;
                //        }
                //        try
                //        {
                //            XmlNode[] nodePdf = _respuesta.pdf as XmlNode[];
                //            PDF = nodePdf[0].InnerText;
                //            //resultPDF = Encoding.UTF8.GetBytes(nodePdf[0].InnerText);
                //            //resultPDF = Convert.FromBase64String(nodePdf[0].InnerText);
                //            //string filePath = $@"{AppDomain.CurrentDomain.BaseDirectory}\BE-F{resultFolio}.pdf";
                //            //FileStream fileStream = new FileStream(filePath, FileMode.CreateNew);
                //            //BinaryWriter binaryWriter = new BinaryWriter(fileStream);
                //            //binaryWriter.Write(resultPDF, 0, resultPDF.Length);
                //            //binaryWriter.Close();
                //        }
                //        catch (Exception ex)
                //        {
                //            msgError = ex.Message;
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        msgError = ex.Message;
                //    }
                //}
            }
            catch (Exception ex)
            {
                msgError = $"(counter:{stepCounter}) " + ex.Message;
            }
            return resultado;
        }
        public static string GetNode(string bloque, string nodo)
        {
            string resultado = "";
            int pFrom = bloque.ToUpper().IndexOf($"<{nodo.ToUpper()}>") + ($"<{nodo}>").Length;
            int pTo = bloque.ToUpper().LastIndexOf($"</{nodo.ToUpper()}>");
            resultado = bloque.Substring(pFrom, pTo - pFrom);
            return resultado;
        }
        public static string retornaTED(string xml)
        {
            string[] arr = xml.Split(new string[] { "<TED version=\"1.0\">" }, StringSplitOptions.None);
            string ted = arr[1];
            string[] arr1 = ted.Split(new string[] { "<TmstFirma>" }, StringSplitOptions.None);
            string ted2 = arr1[0];
            ted2 = ted2.Replace("\r", "");
            ted2 = ted2.Replace("\n", "");
            ted2 = ted2.Replace("\t", "");
            ted2 = "<TED>" + ted2;
            return ted2;
        }
    }
}
