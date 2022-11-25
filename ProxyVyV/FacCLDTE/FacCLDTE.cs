using NLog;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using Newtonsoft.Json;
using System.Xml;

namespace ProxyVyV.ProxyVyV.DTEFacCL
{
    public static class StringExt
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;

            value = value.Norm();

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static string Norm(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;

            value = value.Norm();

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static string Norm(this string value)
        {

            if (string.IsNullOrEmpty(value)) return value;

            try
            {
                return System.Text.RegularExpressions.Regex.Replace(value.Normalize(NormalizationForm.FormD), @"[^A-Za-z0-9|.,_\-@ ]", string.Empty);
            }
            catch (Exception)
            {
                return value;
            }



        }
    }

    public class FacCLDTE
    {
        public FacCLDTE(ProxyParam paramValues)
        {
            ParamValues = paramValues;
        }

        private ProxyParam ParamValues { get; set; }

        public void CreaDocumentoElectronico(JObject json2, string SidDocPRISM, CCustMessage message, JObject jsoncustomer2, JObject jsonreferencia, out string jsonOut)
        {
            jsonOut = "";
            NLog.Logger log = LogManager.GetCurrentClassLogger();
            string status = string.Empty;
            string folio = string.Empty;
            string tedbase64 = string.Empty;


            try
            {



                log.Info("INICIO  CALLDTEFACCL");
                string validacion = string.Empty;
                string errors = string.Empty;
                string numdoccliente = string.Empty;
                string[] textArrayResponse;
                string msg = string.Empty;
                string posflagtipo = string.Empty;
                string xmlres = string.Empty;
                string ted = string.Empty;



                log.Info("INICIO  Documento");
                log.Info(json2.ToString());

                numdoccliente = "";
                if (jsoncustomer2 != null)
                {
                    numdoccliente = jsoncustomer2.GetValue("info1").ToString();
                    if (numdoccliente == "")
                    {
                        numdoccliente = jsoncustomer2.GetValue("info2").ToString();

                    }
                }

                log.Info("numdoc cliente : " + numdoccliente);

                string posflag = json2.GetValue("posflag1").ToString().Trim().Substring(0, 2);

                if (posflag == "39")// BOLETA ELECTRONICA
                {
                    try
                    {
                        log.Info("INICIO  BOLETA ELECTRONICA");

                        FacCLDTEXML Dte = new FacCLDTEXML();

                        Dte.Documento = new DTEDocumento();
                        Dte.version = 1;
                        //ENCABEZADO
                        DTEDocumentoEncabezado Enc = new DTEDocumentoEncabezado();
                        Enc.IdDoc = new DTEDocumentoEncabezadoIdDoc();
                        //IdDOC
                        Enc.IdDoc.TipoDTE = 39;
                        Enc.IdDoc.Folio = 0;
                        Enc.IdDoc.FchEmis = DateTime.Now.Date.ToString("yyyy-MM-dd");
                        Enc.IdDoc.IndServicio = "3";
                        Enc.IdDoc.IndMntNeto = "2";
                        Enc.IdDoc.PeriodoDesde = DateTime.Now.Date.ToString("yyyy-MM-dd");
                        Enc.IdDoc.PeriodoHasta = DateTime.Now.Date.ToString("yyyy-MM-dd");
                        Enc.IdDoc.FchVenc = DateTime.Now.Date.ToString("yyyy-MM-dd");
                        //EMISOR
                        Enc.Emisor = new DTEDocumentoEncabezadoEmisor();
                        Enc.Emisor.RUTEmisor = ParamValues.DTEFacCL_RutEmisor;
                        Enc.Emisor.RznSocEmisor = ParamValues.DTEFacCL_RznSocEmi.Norm(100);
                        Enc.Emisor.GiroEmisor = ParamValues.DTEFacCL_Giro.Norm(80);
                        Enc.Emisor.DirOrigen = ParamValues.DTEFacCL_Direccion.Norm(60);
                        Enc.Emisor.CmnaOrigen = ParamValues.DTEFacCL_Comuna.Norm(20);
                        Enc.Emisor.CiudadOrigen = ParamValues.DTEFacCL_Ciudad.Norm(20);

                        Dte.Documento.Encabezado = Enc;

                        //RECEPTOR
                        if (string.IsNullOrEmpty(numdoccliente)) numdoccliente = ParamValues.DTEFacCL_RutReceptordefault;
                        string nombrecliente = jsoncustomer2.GetValue("first_name").ToString() + " " + jsoncustomer2.GetValue("last_name").ToString();
                        if ((nombrecliente.Trim() == "") || ((nombrecliente == " ")))
                        {
                            nombrecliente = this.ParamValues.DTEFacCL_NombreClientedefault;
                        }

                        DTEDocumentoEncabezadoReceptor Rec = new DTEDocumentoEncabezadoReceptor();
                        Rec.RUTRecep = numdoccliente;
                        Rec.CdgIntRecep = "";
                        Rec.RznSocRecep = nombrecliente;
                        Rec.Contacto = "mail";

                        //Direcciones
                        if (!String.IsNullOrEmpty(jsoncustomer2.GetValue("primary_address_line_1").ToString()))
                        {
                            Rec.DirRecep = jsoncustomer2.GetValue("primary_address_line_1").ToString() + " " + jsoncustomer2.GetValue("btaddressline2").ToString();
                        }
                        if (!String.IsNullOrEmpty(jsoncustomer2.GetValue("primary_address_line_4").ToString()))
                        {
                            Rec.CmnaRecep = jsoncustomer2.GetValue("primary_address_line_4").ToString();
                        }
                        if (!String.IsNullOrEmpty(jsoncustomer2.GetValue("primary_address_line_5").ToString()))
                        {
                            Rec.CiudadRecep = jsoncustomer2.GetValue("primary_address_line_5").ToString();
                        }

                        Dte.Documento.Encabezado.Receptor = Rec;

                        DTEDocumentoEncabezadoTotales Totales = new DTEDocumentoEncabezadoTotales();

                        //TOTALES

                        if (json2.GetValue("saletotalamt").ToString() == "0")
                        {
                            Totales.MntNeto = 1;
                            Totales.MntExe = 0;
                            Totales.TasaIVA = this.ParamValues.DTEFacCL_TasaIVA;
                            Totales.IVA = 0;
                            Totales.MntTotal = 1;
                        }
                        else
                        {
                            int iva = 0;
                            int mntTotal = 0;
                            int mntNeto = 0;
                            log.Info("Totales");
                            iva = json2["saletotaltaxamt"].Value<Int32>();//.["saletotaltaxamt").ToString());
                                                                          //total.TasaIVA = float.Parse(this.ParamValues.DTESignature_TasaIVA);
                            mntTotal = json2["saletotalamt"].Value<Int32>();// Int32.Parse(json2.GetValue("saletotalamt").ToString());
                            mntNeto = mntTotal - iva;

                            Totales.MntNeto = mntNeto;
                            Totales.MntExe = 0;
                            Totales.TasaIVA = this.ParamValues.DTEFacCL_TasaIVA;
                            Totales.IVA = iva;
                            Totales.MntTotal = mntTotal;

                        }

                        Dte.Documento.Encabezado.Totales = Totales;

                        // Creacion Detalle
                        Dte.Documento.Detalle = new List<DTEDocumentoDetalle>();

                        string strjson3 = json2.GetValue("docitem").ToString();
                        JArray jsonArray = JArray.Parse(strjson3);
                        log.Info("Detalle");
                        int contadordetalle = 0;
                        foreach (JObject jsonOperaciones in jsonArray.Children<JObject>())
                        {
                            if (contadordetalle < 61)
                            {
                                contadordetalle = contadordetalle + 1;
                                //string descuento = string.Empty;
                                DTEDocumentoDetalle det = new DTEDocumentoDetalle();
                                //try
                                //{
                                //    descuento = (jsonOperaciones["discamt"]).ToString();
                                //    if (descuento != string.Empty && descuento != "0")
                                //    {
                                //        det.DescuentoMonto = json2["discamt"].Value<Int32>();
                                //    }
                                //}
                                //catch
                                //{
                                //}
                                float QtyItem = jsonOperaciones["qty"].Value<float>();
                                float PrcItem = jsonOperaciones["price"].Value<float>() - jsonOperaciones["taxamt"].Value<float>();
                                det.NroLinDet = contadordetalle;// jsonOperaciones["itempos"].Value<Int32>().ToString();// Int32.Parse((jsonOperaciones["itempos"]).ToString());
                                det.NmbItem = (jsonOperaciones["description1"]).ToString();

                                det.UnmdItem = "UND";
                                det.QtyItem = QtyItem;
                                det.PrcItem = PrcItem;

                                det.MontoItem = Convert.ToInt32(QtyItem * PrcItem);
                                det.CdgItem = new DTEDocumentoDetalleCdgItem();
                                det.CdgItem.TpoCodigo = "ALU";
                                det.CdgItem.VlrCodigo = (jsonOperaciones["alu"]).ToString();

                                Dte.Documento.Detalle.Add(det);

                            }
                            else
                            {
                                msg = "No es posible superar los 60 Items";
                                throw new ApplicationException(msg);
                            }
                        }

                        Dte.Documento.Referencia = new List<DTEDocumentoReferencia>();

                        //aca nos comunicamos 


                        string jsonBase64 = "";
                        string error = "";

                        log.Info("Procesa Documento en WS FACTURACION.cl: ");


                        cl.facturacion.ws.wsplano WS = new cl.facturacion.ws.wsplano();

                        cl.facturacion.ws.logininfo login = new cl.facturacion.ws.logininfo();
                        login.Usuario = Convert.ToBase64String(Encoding.ASCII.GetBytes(ParamValues.DTEFacCL_WS_USUARIO));//  "Q09NQ0FJVA==";
                        login.Rut = Convert.ToBase64String(Encoding.ASCII.GetBytes(ParamValues.DTEFacCL_RutEmisor));     // "MS05";
                        login.Clave = Convert.ToBase64String(Encoding.ASCII.GetBytes(ParamValues.DTEFacCL_WS_CLAVE));    // "cGxhbm85MTA5OA==";
                        login.Puerto = "MA==";
                        login.IncluyeLink = "1";

                        string sxml = Dte.ToXML();
                        log.Info(sxml);
                        string file = Dte.GetBase64(sxml, out error);



                        string response = WS.Procesar(login, file, 2);

                        log.Info("Response: " + response);

                        //jsonRespSEMPOS = "{\"RetCode\":\"0\",\"RetMsge\":\"OK\",\"Internal_Id\":\"590050609000065277\",\"SupplierId\":\"76171658-1\",\"DocType\":\"39\",\"LegalNumber\":\"1241\",\"TimeStamp\":\"2021-02-26T18:10:34\",\"DocTEDQ\":\"PFRFRCB2ZXJzaW9uID0iMS4wIj48REQ+PFJFPjc2MTcxNjU4LTE8L1JFPjxURD4zOTwvVEQ+PEY+MTI0MTwvRj48RkU+MjAyMS0wMi0yNjwvRkU+PFJSPjY2NjY2NjY2LTY8L1JSPjxSU1I+Q0xJRU5URSBCT0xFVEE8L1JTUj48TU5UPjQ5OTA8L01OVD48SVQxPkFCU09SQkVOVEVTIEFSRE8gREFZICBOSUdIVCBQQTwvSVQxPjxDQUYgdmVyc2lvbj1cIjEuMFwiPjxEQT48UkU+NzYxNzE2NTgtMTwvUkU+PFJTPkNPTUVSQ0lBTCBFIElORFVTVFJJQUwgU0lMRkEgUy5BPC9SUz48VEQ+Mzk8L1REPjxSTkc+PEQ+MTI0MTwvRD48SD4yMjQwPC9IPjwvUk5HPjxGQT4yMDIxLTAyLTEyPC9GQT48UlNBUEs+PE0+c25EMmJKSUlmMkx0RWFPYkoxdzA0aExDZjFCSGdWWGptM2tZVTlFOThkMEY5VlRYNlBHTDlaQ2cvZ3ZFNGdRR2FGdDVwYzBxbTZVS1dLbW5QL0pFUVE9PTwvTT48RT5Bdz09PC9FPjwvUlNBUEs+PElESz4xMDA8L0lESz48L0RBPjxGUk1BIGFsZ29yaXRtbz1cIlNIQTF3aXRoUlNBXCI+bk1PazNkSnhEZ0hRUTArSEtVZGtWYWJEQjdobEtKemh0VWEzQVdPbTJXaHMxdUV1dzl6VE1uWUFlTWQ4cXJpWmN3R3g1NVFFWWdFNTZUeEoxaERLdVE9PTwvRlJNQT48L0NBRj48VFNURUQ+MjAyMS0wMi0yNlQxODoxMDozNDwvVFNURUQ+PC9ERD48RlJNVCBhbGdvcml0bW89IlNIQTF3aXRoUlNBIj5pUStzZTY1QlRMRGlzOVBKbjhoQjZock4vV0ZWeENBSnVYUk5SVlBFZHhuQTFqaGY3ZkdPenVXVWJ0cUZ0UlFIcStmbFQ4MnByQ0xHcmRaUTRXNkRHZz09PC9GUk1UPjwvVEVEPg==\"}";

                        if (!string.IsNullOrEmpty(response))
                        {

                            //que trucazo no!!!
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(response);
                            string json = JsonConvert.SerializeXmlNode(doc);
                            DTEFacCLResponse.FacCLDTEResponse facCLDTEResponse = JsonConvert.DeserializeObject<DTEFacCLResponse.FacCLDTEResponse>(json);

                            if (facCLDTEResponse.WSPLANO.Resultado == "True")
                            {
                                status = "0";
                                //folio = (Convert.ToInt64 (facCLDTEResponse.WSPLANO.Detalle.Documento.Folio)- 9200000000).ToString();
                                folio = facCLDTEResponse.WSPLANO.Detalle.Documento.Folio;
                                msg = "";
                                //se debe llamar por el timbre
                                tedbase64 = facCLDTEResponse.WSPLANO.Detalle.Documento.urlOriginal;// EncodeStrToBase64(ted);

                                textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                jsonOut = string.Concat(textArrayResponse);

                            }
                            else
                            {
                                status = "500";
                                msg = facCLDTEResponse.WSPLANO.Detalle.Documento.Error;
                                textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                jsonOut = string.Concat(textArrayResponse);
                                log.Info("Error From SEMPOS: ");
                            }

                            return;
                        }

                        status = "500";
                        msg = "ERROR WS FACCL retorno nulo o vacio:";
                        textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                        jsonOut = string.Concat(textArrayResponse);
                        log.Info("ProcessEvent** Error: " + msg);

                    }
                    catch (Exception e)
                    {


                        status = "555";
                        log.Info("EXCEPTION2");
                        jsonOut = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                        log.Info("ProcessEvent *********** Error General -- " + e.Message);

                    }
                    return;
                }
                else if (posflag == "33")// FACTURA ELECTRONICA
                {
                    try
                    {
                        log.Info("INICIO FACTURA ELECTRONICA");

                        FacCLDTEXML Dte = new FacCLDTEXML();

                        Dte.Documento = new DTEDocumento();
                        Dte.version = 1;
                        //ENCABEZADO
                        DTEDocumentoEncabezado Enc = new DTEDocumentoEncabezado();
                        Enc.IdDoc = new DTEDocumentoEncabezadoIdDoc();
                        //IdDOC
                        Enc.IdDoc.TipoDTE = 33;
                        Enc.IdDoc.Folio = 0;
                        Enc.IdDoc.FchEmis = DateTime.Now.Date.ToString("yyyy-MM-dd");
                        Enc.IdDoc.IndServicio = "3";
                        Enc.IdDoc.IndMntNeto = "2";
                        Enc.IdDoc.PeriodoDesde = DateTime.Now.Date.ToString("yyyy-MM-dd");
                        Enc.IdDoc.PeriodoHasta = DateTime.Now.Date.ToString("yyyy-MM-dd");
                        Enc.IdDoc.FchVenc = DateTime.Now.Date.ToString("yyyy-MM-dd");
                        //EMISOR
                        Enc.Emisor = new DTEDocumentoEncabezadoEmisor();
                        Enc.Emisor.RUTEmisor = ParamValues.DTEFacCL_RutEmisor;
                        Enc.Emisor.RznSocEmisor = ParamValues.DTEFacCL_RznSocEmi.Norm(100);
                        Enc.Emisor.GiroEmisor = ParamValues.DTEFacCL_Giro.Norm(80);
                        Enc.Emisor.DirOrigen = ParamValues.DTEFacCL_Direccion.Norm(60);
                        Enc.Emisor.CmnaOrigen = ParamValues.DTEFacCL_Comuna.Norm(20);
                        Enc.Emisor.CiudadOrigen = ParamValues.DTEFacCL_Ciudad.Norm(20);

                        Dte.Documento.Encabezado = Enc;

                        //RECEPTOR
                        string nombrecliente = jsoncustomer2.GetValue("first_name").ToString() + " " + jsoncustomer2.GetValue("last_name").ToString();
                        if ((nombrecliente.Trim() == "") || ((nombrecliente == " ")))
                        {
                            throw new ApplicationException("Cliente Requerido para Factura Electronica");
                        }

                        DTEDocumentoEncabezadoReceptor Rec = new DTEDocumentoEncabezadoReceptor();
                        Rec.RUTRecep = numdoccliente;
                        Rec.CdgIntRecep = "";
                        Rec.RznSocRecep = nombrecliente;
                        Rec.Contacto = "mail";

                        //Direcciones
                        if (!String.IsNullOrEmpty(jsoncustomer2.GetValue("primary_address_line_1").ToString()))
                        {
                            Rec.DirRecep = jsoncustomer2.GetValue("primary_address_line_1").ToString() + " " + jsoncustomer2.GetValue("primary_address_line_2").ToString();
                        }
                        if (!String.IsNullOrEmpty(jsoncustomer2.GetValue("primary_address_line_4").ToString()))
                        {
                            Rec.CmnaRecep = jsoncustomer2.GetValue("primary_address_line_4").ToString();
                        }
                        if (!String.IsNullOrEmpty(jsoncustomer2.GetValue("primary_address_line_5").ToString()))
                        {
                            Rec.CiudadRecep = jsoncustomer2.GetValue("primary_address_line_5").ToString();
                        }

                        Dte.Documento.Encabezado.Receptor = Rec;

                        DTEDocumentoEncabezadoTotales Totales = new DTEDocumentoEncabezadoTotales();

                        //TOTALES

                        if (json2.GetValue("saletotalamt").ToString() == "0")
                        {
                            Totales.MntNeto = 1;
                            Totales.MntExe = 0;
                            Totales.TasaIVA = this.ParamValues.DTEFacCL_TasaIVA;
                            Totales.IVA = 0;
                            Totales.MntTotal = 1;
                        }
                        else
                        {
                            int iva = 0;
                            int mntTotal = 0;
                            int mntNeto = 0;
                            log.Info("Totales");
                            iva = json2["saletotaltaxamt"].Value<Int32>();//.["saletotaltaxamt").ToString());
                                                                          //total.TasaIVA = float.Parse(this.ParamValues.DTESignature_TasaIVA);
                            mntTotal = json2["saletotalamt"].Value<Int32>();// Int32.Parse(json2.GetValue("saletotalamt").ToString());
                            mntNeto = mntTotal - iva;

                            Totales.MntNeto = mntNeto;
                            Totales.MntExe = 0;
                            Totales.TasaIVA = this.ParamValues.DTEFacCL_TasaIVA;
                            Totales.IVA = iva;
                            Totales.MntTotal = mntTotal;

                        }

                        Dte.Documento.Encabezado.Totales = Totales;

                        // Creacion Detalle
                        Dte.Documento.Detalle = new List<DTEDocumentoDetalle>();

                        string strjson3 = json2.GetValue("docitem").ToString();
                        JArray jsonArray = JArray.Parse(strjson3);
                        log.Info("Detalle");
                        int contadordetalle = 0;
                        foreach (JObject jsonOperaciones in jsonArray.Children<JObject>())
                        {
                            if (contadordetalle < 61)
                            {
                                contadordetalle = contadordetalle + 1;
                                //string descuento = string.Empty;
                                DTEDocumentoDetalle det = new DTEDocumentoDetalle();
                                //try
                                //{
                                //    descuento = (jsonOperaciones["discamt"]).ToString();
                                //    if (descuento != string.Empty && descuento != "0")
                                //    {
                                //        det.DescuentoMonto = json2["discamt"].Value<Int32>();
                                //    }
                                //}
                                //catch
                                //{
                                //}
                                float QtyItem = jsonOperaciones["qty"].Value<float>();
                                float PrcItem = jsonOperaciones["price"].Value<float>() - jsonOperaciones["taxamt"].Value<float>();
                                det.NroLinDet = contadordetalle;// jsonOperaciones["itempos"].Value<Int32>().ToString();// Int32.Parse((jsonOperaciones["itempos"]).ToString());
                                det.NmbItem = (jsonOperaciones["description1"]).ToString();

                                det.UnmdItem = "UND";
                                det.QtyItem = QtyItem;
                                det.PrcItem = PrcItem;

                                det.MontoItem = Convert.ToInt32(QtyItem * PrcItem);
                                det.CdgItem = new DTEDocumentoDetalleCdgItem();
                                det.CdgItem.TpoCodigo = "ALU";
                                det.CdgItem.VlrCodigo = (jsonOperaciones["alu"]).ToString();

                                Dte.Documento.Detalle.Add(det);

                            }
                            else
                            {
                                msg = "No es posible superar los 60 Items";
                                throw new ApplicationException(msg);
                            }
                        }

                        Dte.Documento.Referencia = new List<DTEDocumentoReferencia>();

                        //aca nos comunicamos 


                        string jsonBase64 = "";
                        string error = "";

                        log.Info("Procesa Documento en WS FACTURACION.cl: ");


                        cl.facturacion.ws.wsplano WS = new cl.facturacion.ws.wsplano();

                        cl.facturacion.ws.logininfo login = new cl.facturacion.ws.logininfo();
                        login.Usuario = Convert.ToBase64String(Encoding.ASCII.GetBytes(ParamValues.DTEFacCL_WS_USUARIO));//  "Q09NQ0FJVA==";
                        login.Rut = Convert.ToBase64String(Encoding.ASCII.GetBytes(ParamValues.DTEFacCL_RutEmisor));     // "MS05";
                        login.Clave = Convert.ToBase64String(Encoding.ASCII.GetBytes(ParamValues.DTEFacCL_WS_CLAVE));    // "cGxhbm85MTA5OA==";
                        login.Puerto = "MA==";
                        login.IncluyeLink = "1";

                        string sxml = Dte.ToXML();
                        log.Info(sxml);
                        string file = Dte.GetBase64(sxml, out error);



                        string response = WS.Procesar(login, file, 2);

                        log.Info("Response: " + response);

                        //jsonRespSEMPOS = "{\"RetCode\":\"0\",\"RetMsge\":\"OK\",\"Internal_Id\":\"590050609000065277\",\"SupplierId\":\"76171658-1\",\"DocType\":\"39\",\"LegalNumber\":\"1241\",\"TimeStamp\":\"2021-02-26T18:10:34\",\"DocTEDQ\":\"PFRFRCB2ZXJzaW9uID0iMS4wIj48REQ+PFJFPjc2MTcxNjU4LTE8L1JFPjxURD4zOTwvVEQ+PEY+MTI0MTwvRj48RkU+MjAyMS0wMi0yNjwvRkU+PFJSPjY2NjY2NjY2LTY8L1JSPjxSU1I+Q0xJRU5URSBCT0xFVEE8L1JTUj48TU5UPjQ5OTA8L01OVD48SVQxPkFCU09SQkVOVEVTIEFSRE8gREFZICBOSUdIVCBQQTwvSVQxPjxDQUYgdmVyc2lvbj1cIjEuMFwiPjxEQT48UkU+NzYxNzE2NTgtMTwvUkU+PFJTPkNPTUVSQ0lBTCBFIElORFVTVFJJQUwgU0lMRkEgUy5BPC9SUz48VEQ+Mzk8L1REPjxSTkc+PEQ+MTI0MTwvRD48SD4yMjQwPC9IPjwvUk5HPjxGQT4yMDIxLTAyLTEyPC9GQT48UlNBUEs+PE0+c25EMmJKSUlmMkx0RWFPYkoxdzA0aExDZjFCSGdWWGptM2tZVTlFOThkMEY5VlRYNlBHTDlaQ2cvZ3ZFNGdRR2FGdDVwYzBxbTZVS1dLbW5QL0pFUVE9PTwvTT48RT5Bdz09PC9FPjwvUlNBUEs+PElESz4xMDA8L0lESz48L0RBPjxGUk1BIGFsZ29yaXRtbz1cIlNIQTF3aXRoUlNBXCI+bk1PazNkSnhEZ0hRUTArSEtVZGtWYWJEQjdobEtKemh0VWEzQVdPbTJXaHMxdUV1dzl6VE1uWUFlTWQ4cXJpWmN3R3g1NVFFWWdFNTZUeEoxaERLdVE9PTwvRlJNQT48L0NBRj48VFNURUQ+MjAyMS0wMi0yNlQxODoxMDozNDwvVFNURUQ+PC9ERD48RlJNVCBhbGdvcml0bW89IlNIQTF3aXRoUlNBIj5pUStzZTY1QlRMRGlzOVBKbjhoQjZock4vV0ZWeENBSnVYUk5SVlBFZHhuQTFqaGY3ZkdPenVXVWJ0cUZ0UlFIcStmbFQ4MnByQ0xHcmRaUTRXNkRHZz09PC9GUk1UPjwvVEVEPg==\"}";

                        if (!string.IsNullOrEmpty(response))
                        {

                            //que trucazo no!!!
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(response);
                            string json = JsonConvert.SerializeXmlNode(doc);
                            DTEFacCLResponse.FacCLDTEResponse facCLDTEResponse = JsonConvert.DeserializeObject<DTEFacCLResponse.FacCLDTEResponse>(json);

                            if (facCLDTEResponse.WSPLANO.Resultado == "True")
                            {
                                status = "0";
                                //folio = (Convert.ToInt64 (facCLDTEResponse.WSPLANO.Detalle.Documento.Folio)- 9200000000).ToString();
                                folio = facCLDTEResponse.WSPLANO.Detalle.Documento.Folio;
                                msg = "";
                                //se debe llamar por el timbre
                                tedbase64 = facCLDTEResponse.WSPLANO.Detalle.Documento.urlOriginal;// EncodeStrToBase64(ted);

                                textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                jsonOut = string.Concat(textArrayResponse);

                            }
                            else
                            {
                                status = "500";
                                msg = facCLDTEResponse.WSPLANO.Detalle.Documento.Error;
                                textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                jsonOut = string.Concat(textArrayResponse);
                                log.Info("Error From SEMPOS: ");
                            }

                            return;
                        }

                        status = "500";
                        msg = "ERROR WS FACCL retorno nulo o vacio:";
                        textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                        jsonOut = string.Concat(textArrayResponse);
                        log.Info("ProcessEvent** Error: " + msg);

                    }
                    catch (Exception e)
                    {


                        status = "555";
                        log.Info("EXCEPTION2");
                        jsonOut = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                        log.Info("ProcessEvent *********** Error General -- " + e.Message);

                    }
                    return;
                }
                else if (posflag == "61")// NOTA CREDITO ELECTRONICA
                {
                    try
                    {
                        log.Info("INICIO FACTURA ELECTRONICA");

                        FacCLDTEXML Dte = new FacCLDTEXML();

                        Dte.Documento = new DTEDocumento();
                        Dte.version = 1;
                        //ENCABEZADO
                        DTEDocumentoEncabezado Enc = new DTEDocumentoEncabezado();
                        Enc.IdDoc = new DTEDocumentoEncabezadoIdDoc();
                        //IdDOC
                        Enc.IdDoc.TipoDTE = 61;
                        Enc.IdDoc.Folio = 0;
                        Enc.IdDoc.FchEmis = DateTime.Now.Date.ToString("yyyy-MM-dd");
                        //Enc.IdDoc.IndServicio = 3;
                        //Enc.IdDoc.IndMntNeto = 2;
                        Enc.IdDoc.PeriodoDesde = DateTime.Now.Date.ToString("yyyy-MM-dd");
                        Enc.IdDoc.PeriodoHasta = DateTime.Now.Date.ToString("yyyy-MM-dd");
                        Enc.IdDoc.FchVenc = DateTime.Now.Date.ToString("yyyy-MM-dd");
                        //EMISOR
                        Enc.Emisor = new DTEDocumentoEncabezadoEmisor();
                        Enc.Emisor.RUTEmisor = ParamValues.DTEFacCL_RutEmisor;
                        Enc.Emisor.RznSocEmisor = ParamValues.DTEFacCL_RznSocEmi.Norm(100);
                        Enc.Emisor.GiroEmisor = ParamValues.DTEFacCL_Giro.Norm(80);
                        Enc.Emisor.DirOrigen = ParamValues.DTEFacCL_Direccion.Norm(60);
                        Enc.Emisor.CmnaOrigen = ParamValues.DTEFacCL_Comuna.Norm(20);
                        Enc.Emisor.CiudadOrigen = ParamValues.DTEFacCL_Ciudad.Norm(20);

                        Dte.Documento.Encabezado = Enc;

                        //RECEPTOR
                        string nombrecliente = jsoncustomer2.GetValue("first_name").ToString() + " " + jsoncustomer2.GetValue("last_name").ToString();
                        if ((nombrecliente.Trim() == "") || ((nombrecliente == " ")))
                        {
                            throw new ApplicationException("Cliente Requerido para Nota de Credito Electronica");
                        }

                        DTEDocumentoEncabezadoReceptor Rec = new DTEDocumentoEncabezadoReceptor();
                        Rec.RUTRecep = numdoccliente;
                        Rec.CdgIntRecep = "";
                        Rec.RznSocRecep = nombrecliente;
                        Rec.Contacto = "mail";

                        //Direcciones
                        if (!String.IsNullOrEmpty(jsoncustomer2.GetValue("primary_address_line_1").ToString()))
                        {
                            Rec.DirRecep = jsoncustomer2.GetValue("primary_address_line_1").ToString() + " "+ jsoncustomer2.GetValue("primary_address_line_2").ToString();
                        }
                        if (!String.IsNullOrEmpty(jsoncustomer2.GetValue("primary_address_line_4").ToString()))
                        {
                            Rec.CmnaRecep = jsoncustomer2.GetValue("primary_address_line_4").ToString();
                        }
                        if (!String.IsNullOrEmpty(jsoncustomer2.GetValue("primary_address_line_5").ToString()))
                        {
                            Rec.CiudadRecep = jsoncustomer2.GetValue("primary_address_line_5").ToString();
                        }

                        Dte.Documento.Encabezado.Receptor = Rec;

                        DTEDocumentoEncabezadoTotales Totales = new DTEDocumentoEncabezadoTotales();

                        //TOTALES 61

                        if (json2.GetValue("returnsubtotalwithtax").ToString() == "0")
                        {
                            throw new ApplicationException("Total del Documento no puede ser cero");
                        }
                        else
                        {
                            int iva = 0;
                            int mntTotal = 0;
                            int mntNeto = 0;
                            log.Info("Totales");
                            iva = json2["returntotaltaxamt"].Value<Int32>();//.["saletotaltaxamt").ToString());
                                                                            //total.TasaIVA = float.Parse(this.ParamValues.DTESignature_TasaIVA);
                            mntTotal = json2["returnsubtotalwithtax"].Value<Int32>();// Int32.Parse(json2.GetValue("saletotalamt").ToString());
                            mntNeto = mntTotal - iva;

                            Totales.MntNeto = mntNeto;
                            Totales.MntExe = 0;
                            Totales.TasaIVA = this.ParamValues.DTEFacCL_TasaIVA;
                            Totales.IVA = iva;
                            Totales.MntTotal = mntTotal;

                        }

                        Dte.Documento.Encabezado.Totales = Totales;

                        //Referencia
                        Dte.Documento.Referencia = new List<DTEDocumentoReferencia>();
                        DTEDocumentoReferencia referencia = new DTEDocumentoReferencia();
                        referencia.NroLinRef = 1;
                        referencia.TpoDocRef = jsonreferencia.GetValue("posflag1").ToString().Trim().Substring(0, 2);
                        referencia.CodRef = "1";
                        referencia.RazonRef = "Anula Documento de Referencia";
                        referencia.FolioRef = jsonreferencia.GetValue("trackingno").ToString();
                        DateTime fecharef = DateTime.Parse(jsonreferencia.GetValue("modifieddatetime").ToString());
                        string fecharefstr = String.Format("{0:yyyy-MM-dd}", fecharef);
                        referencia.FchRef = fecharefstr;

                        Dte.Documento.Referencia.Add(referencia);

                        // Creacion Detalle
                        Dte.Documento.Detalle = new List<DTEDocumentoDetalle>();

                        string strjson3 = json2.GetValue("docitem").ToString();
                        JArray jsonArray = JArray.Parse(strjson3);
                        log.Info("Detalle");
                        int contadordetalle = 0;
                        foreach (JObject jsonOperaciones in jsonArray.Children<JObject>())
                        {
                            if (contadordetalle < 61)
                            {
                                contadordetalle = contadordetalle + 1;
                                DTEDocumentoDetalle det = new DTEDocumentoDetalle();
                                float QtyItem = jsonOperaciones["qty"].Value<float>();
                                float PrcItem = jsonOperaciones["price"].Value<float>() - jsonOperaciones["taxamt"].Value<float>();
                                det.NroLinDet = contadordetalle;// jsonOperaciones["itempos"].Value<Int32>().ToString();// Int32.Parse((jsonOperaciones["itempos"]).ToString());
                                det.NmbItem = (jsonOperaciones["description1"]).ToString();

                                det.UnmdItem = "UND";
                                det.QtyItem = QtyItem;
                                det.PrcItem = PrcItem;

                                det.MontoItem = Convert.ToInt32(QtyItem * PrcItem);
                                det.CdgItem = new DTEDocumentoDetalleCdgItem();
                                det.CdgItem.TpoCodigo = "ALU";
                                det.CdgItem.VlrCodigo = (jsonOperaciones["alu"]).ToString();

                                Dte.Documento.Detalle.Add(det);

                            }
                            else
                            {
                                msg = "No es posible superar los 60 Items";
                                throw new ApplicationException(msg);
                            }
                        }



                        //aca nos comunicamos 


                        string jsonBase64 = "";
                        string error = "";

                        log.Info("Procesa Documento en WS FACTURACION.cl: ");


                        cl.facturacion.ws.wsplano WS = new cl.facturacion.ws.wsplano();

                        cl.facturacion.ws.logininfo login = new cl.facturacion.ws.logininfo();
                        login.Usuario = Convert.ToBase64String(Encoding.ASCII.GetBytes(ParamValues.DTEFacCL_WS_USUARIO));//  "Q09NQ0FJVA==";
                        login.Rut = Convert.ToBase64String(Encoding.ASCII.GetBytes(ParamValues.DTEFacCL_RutEmisor));     // "MS05";
                        login.Clave = Convert.ToBase64String(Encoding.ASCII.GetBytes(ParamValues.DTEFacCL_WS_CLAVE));    // "cGxhbm85MTA5OA==";
                        login.Puerto = "MA==";
                        login.IncluyeLink = "1";

                        string sxml = Dte.ToXML();
                        log.Info(sxml);
                        string file = Dte.GetBase64(sxml, out error);



                        string response = WS.Procesar(login, file, 2);

                        log.Info("Response: " + response);

                        //jsonRespSEMPOS = "{\"RetCode\":\"0\",\"RetMsge\":\"OK\",\"Internal_Id\":\"590050609000065277\",\"SupplierId\":\"76171658-1\",\"DocType\":\"39\",\"LegalNumber\":\"1241\",\"TimeStamp\":\"2021-02-26T18:10:34\",\"DocTEDQ\":\"PFRFRCB2ZXJzaW9uID0iMS4wIj48REQ+PFJFPjc2MTcxNjU4LTE8L1JFPjxURD4zOTwvVEQ+PEY+MTI0MTwvRj48RkU+MjAyMS0wMi0yNjwvRkU+PFJSPjY2NjY2NjY2LTY8L1JSPjxSU1I+Q0xJRU5URSBCT0xFVEE8L1JTUj48TU5UPjQ5OTA8L01OVD48SVQxPkFCU09SQkVOVEVTIEFSRE8gREFZICBOSUdIVCBQQTwvSVQxPjxDQUYgdmVyc2lvbj1cIjEuMFwiPjxEQT48UkU+NzYxNzE2NTgtMTwvUkU+PFJTPkNPTUVSQ0lBTCBFIElORFVTVFJJQUwgU0lMRkEgUy5BPC9SUz48VEQ+Mzk8L1REPjxSTkc+PEQ+MTI0MTwvRD48SD4yMjQwPC9IPjwvUk5HPjxGQT4yMDIxLTAyLTEyPC9GQT48UlNBUEs+PE0+c25EMmJKSUlmMkx0RWFPYkoxdzA0aExDZjFCSGdWWGptM2tZVTlFOThkMEY5VlRYNlBHTDlaQ2cvZ3ZFNGdRR2FGdDVwYzBxbTZVS1dLbW5QL0pFUVE9PTwvTT48RT5Bdz09PC9FPjwvUlNBUEs+PElESz4xMDA8L0lESz48L0RBPjxGUk1BIGFsZ29yaXRtbz1cIlNIQTF3aXRoUlNBXCI+bk1PazNkSnhEZ0hRUTArSEtVZGtWYWJEQjdobEtKemh0VWEzQVdPbTJXaHMxdUV1dzl6VE1uWUFlTWQ4cXJpWmN3R3g1NVFFWWdFNTZUeEoxaERLdVE9PTwvRlJNQT48L0NBRj48VFNURUQ+MjAyMS0wMi0yNlQxODoxMDozNDwvVFNURUQ+PC9ERD48RlJNVCBhbGdvcml0bW89IlNIQTF3aXRoUlNBIj5pUStzZTY1QlRMRGlzOVBKbjhoQjZock4vV0ZWeENBSnVYUk5SVlBFZHhuQTFqaGY3ZkdPenVXVWJ0cUZ0UlFIcStmbFQ4MnByQ0xHcmRaUTRXNkRHZz09PC9GUk1UPjwvVEVEPg==\"}";

                        if (!string.IsNullOrEmpty(response))
                        {

                            //que trucazo no!!!
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(response);
                            string json = JsonConvert.SerializeXmlNode(doc);
                            DTEFacCLResponse.FacCLDTEResponse facCLDTEResponse = JsonConvert.DeserializeObject<DTEFacCLResponse.FacCLDTEResponse>(json);

                            if (facCLDTEResponse.WSPLANO.Resultado == "True")
                            {
                                status = "0";
                                //folio = (Convert.ToInt64 (facCLDTEResponse.WSPLANO.Detalle.Documento.Folio)- 9200000000).ToString();
                                folio = facCLDTEResponse.WSPLANO.Detalle.Documento.Folio;
                                msg = "";
                                //se debe llamar por el timbre
                                tedbase64 = facCLDTEResponse.WSPLANO.Detalle.Documento.urlOriginal;// EncodeStrToBase64(ted);

                                textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                jsonOut = string.Concat(textArrayResponse);

                            }
                            else
                            {
                                status = "500";
                                msg = facCLDTEResponse.WSPLANO.Detalle.Documento.Error;
                                textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                jsonOut = string.Concat(textArrayResponse);
                                log.Info("Error From SEMPOS: ");
                            }

                            return;
                        }

                        status = "500";
                        msg = "ERROR WS FACCL retorno nulo o vacio:";
                        textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                        jsonOut = string.Concat(textArrayResponse);
                        log.Info("ProcessEvent** Error: " + msg);

                    }
                    catch (Exception e)
                    {


                        status = "555";
                        log.Info("EXCEPTION2");
                        jsonOut = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                        log.Info("ProcessEvent *********** Error General -- " + e.Message);

                    }
                    return;
                }


                log.Info("FIN");





            }
            catch (Exception e)
            {
                status = "555";
                log.Info("EXCEPTION2");
                jsonOut = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                log.Info("ProcessEvent *********** Error General -- " + e.Message);
            }

        }

        public void CreaGuiaTransfer(JObject json2, string SidDocPRISM, out string jsonOut)
        {
            jsonOut = "";
            NLog.Logger log = LogManager.GetCurrentClassLogger();
            string status = string.Empty;
            string folio = string.Empty;
            string tedbase64 = string.Empty;


            try
            {



                log.Info("INICIO  CALLDTEFACCL GUIA TRANSFER");
                string validacion = string.Empty;
                string errors = string.Empty;
                string numdoccliente = string.Empty;
                string[] textArrayResponse;
                string msg = string.Empty;
                string posflagtipo = string.Empty;
                string xmlres = string.Empty;
                string ted = string.Empty;



                log.Info("INICIO  Documento");
                log.Info(json2.ToString());

                numdoccliente = "";


                try
                {


                    FacCLDTEXML Dte = new FacCLDTEXML();

                    Dte.Documento = new DTEDocumento();
                    Dte.version = 1;
                    //ENCABEZADO
                    DTEDocumentoEncabezado Enc = new DTEDocumentoEncabezado();
                    Enc.IdDoc = new DTEDocumentoEncabezadoIdDoc();
                    //IdDOC
                    Enc.IdDoc.TipoDTE = 52;
                    Enc.IdDoc.Folio = 0;
                    Enc.IdDoc.FchEmis = DateTime.Now.Date.ToString("yyyy-MM-dd");
                    Enc.IdDoc.IndTraslado = "5";
                    Enc.IdDoc.TipoDespacho = "1";
                    Enc.IdDoc.PeriodoDesde = DateTime.Now.Date.ToString("yyyy-MM-dd");
                    Enc.IdDoc.PeriodoHasta = DateTime.Now.Date.ToString("yyyy-MM-dd");
                    Enc.IdDoc.FchVenc = DateTime.Now.Date.ToString("yyyy-MM-dd");
                    //EMISOR
                    Enc.Emisor = new DTEDocumentoEncabezadoEmisor();
                    Enc.Emisor.RUTEmisor = ParamValues.DTEFacCL_RutEmisor;
                    Enc.Emisor.RznSocEmisor = ParamValues.DTEFacCL_RznSocEmi.Norm(100);
                    Enc.Emisor.GiroEmisor = ParamValues.DTEFacCL_Giro.Norm(80);
                    Enc.Emisor.DirOrigen = ParamValues.DTEFacCL_Direccion.Norm(60);
                    Enc.Emisor.CmnaOrigen = ParamValues.DTEFacCL_Comuna.Norm(20);
                    Enc.Emisor.CiudadOrigen = ParamValues.DTEFacCL_Ciudad.Norm(20);

                    Dte.Documento.Encabezado = Enc;

                    //RECEPTOR


                    DTEDocumentoEncabezadoReceptor Rec = new DTEDocumentoEncabezadoReceptor();
                    Rec.RUTRecep = json2.GetValue("origstorezip").ToString(); ;
                    Rec.CdgIntRecep = null;
                    Rec.GiroRecep = ((json2.GetValue("instoreudf2string").ToString() + " " + json2.GetValue("instoreudf3string").ToString()).Trim()).Trim().Norm(40);
                    Rec.RznSocRecep = json2.GetValue("instoreudf1string").ToString(); ;
                    Rec.Contacto = null;

                    Rec.DirRecep = json2.GetValue("instoreaddress1").ToString();
                    Rec.CmnaRecep = json2.GetValue("instoreaddress2").ToString();
                    Rec.CiudadRecep = json2.GetValue("instoreaddress3").ToString();



                    Dte.Documento.Encabezado.Receptor = Rec;

                    DTEDocumentoEncabezadoTotales Totales = new DTEDocumentoEncabezadoTotales();

                    //TOTALES


                    int iva = 0;
                    float mntTotal = 0;
                    float mntNeto = 0;
                    log.Info("Totales");
                    mntTotal = json2["docpricetotal"].Value<Int32>();// Int32.Parse(json2.GetValue("saletotalamt").ToString());
                    mntNeto = mntTotal / (1 + (float.Parse(this.ParamValues.DTEFacCL_TasaIVA) / 100));
                    iva = Convert.ToInt32(mntTotal - mntNeto);

                    Totales.MntNeto = Convert.ToInt32(mntNeto);
                    Totales.MntExe = 0;
                    Totales.TasaIVA = this.ParamValues.DTEFacCL_TasaIVA;
                    Totales.IVA = iva;
                    Totales.MntTotal = Convert.ToInt32(mntTotal); ;



                    Dte.Documento.Encabezado.Totales = Totales;

                    // Creacion Detalle
                    Dte.Documento.Detalle = new List<DTEDocumentoDetalle>();

                    string strjson3 = json2.GetValue("slipitem").ToString();
                    JArray jsonArray = JArray.Parse(strjson3);
                    log.Info("Detalle");
                    int contadordetalle = 0;
                    foreach (JObject jsonOperaciones in jsonArray.Children<JObject>())
                    {
                        if (contadordetalle < 61)
                        {
                            contadordetalle = contadordetalle + 1;
                            //string descuento = string.Empty;
                            DTEDocumentoDetalle det = new DTEDocumentoDetalle();

                            float QtyItem = jsonOperaciones["qty"].Value<float>();
                            float PrcItem = jsonOperaciones["price"].Value<float>();
                            PrcItem = PrcItem / (1 + (float.Parse(this.ParamValues.DTEFacCL_TasaIVA) / 100));

                            det.NroLinDet = contadordetalle;
                            det.NmbItem = (jsonOperaciones["description1"]).ToString();

                            det.UnmdItem = "UND";
                            det.QtyItem = QtyItem;
                            det.PrcItem = PrcItem;

                            det.MontoItem = Convert.ToInt32(QtyItem * PrcItem);
                            det.CdgItem = new DTEDocumentoDetalleCdgItem();
                            det.CdgItem.TpoCodigo = "ALU";
                            det.CdgItem.VlrCodigo = (jsonOperaciones["alu"]).ToString();

                            Dte.Documento.Detalle.Add(det);

                        }
                        else
                        {
                            msg = "No es posible superar los 60 Items";
                            throw new ApplicationException(msg);
                        }
                    }
                    Dte.Documento.Referencia = new List<DTEDocumentoReferencia>();

                    //aca nos comunicamos 


                    string jsonBase64 = "";
                    string error = "";

                    log.Info("Procesa Documento en WS FACTURACION.cl: ");


                    cl.facturacion.ws.wsplano WS = new cl.facturacion.ws.wsplano();

                    cl.facturacion.ws.logininfo login = new cl.facturacion.ws.logininfo();
                    login.Usuario = Convert.ToBase64String(Encoding.ASCII.GetBytes(ParamValues.DTEFacCL_WS_USUARIO));//  "Q09NQ0FJVA==";
                    login.Rut = Convert.ToBase64String(Encoding.ASCII.GetBytes(ParamValues.DTEFacCL_RutEmisor));     // "MS05";
                    login.Clave = Convert.ToBase64String(Encoding.ASCII.GetBytes(ParamValues.DTEFacCL_WS_CLAVE));    // "cGxhbm85MTA5OA==";
                    login.Puerto = "MA==";
                    login.IncluyeLink = "1";

                    string sxml = Dte.ToXML();
                    log.Info(sxml);
                    string file = Dte.GetBase64(sxml, out error);



                    string response = WS.Procesar(login, file, 2);

                    log.Info("Response: " + response);

                    //jsonRespSEMPOS = "{\"RetCode\":\"0\",\"RetMsge\":\"OK\",\"Internal_Id\":\"590050609000065277\",\"SupplierId\":\"76171658-1\",\"DocType\":\"39\",\"LegalNumber\":\"1241\",\"TimeStamp\":\"2021-02-26T18:10:34\",\"DocTEDQ\":\"PFRFRCB2ZXJzaW9uID0iMS4wIj48REQ+PFJFPjc2MTcxNjU4LTE8L1JFPjxURD4zOTwvVEQ+PEY+MTI0MTwvRj48RkU+MjAyMS0wMi0yNjwvRkU+PFJSPjY2NjY2NjY2LTY8L1JSPjxSU1I+Q0xJRU5URSBCT0xFVEE8L1JTUj48TU5UPjQ5OTA8L01OVD48SVQxPkFCU09SQkVOVEVTIEFSRE8gREFZICBOSUdIVCBQQTwvSVQxPjxDQUYgdmVyc2lvbj1cIjEuMFwiPjxEQT48UkU+NzYxNzE2NTgtMTwvUkU+PFJTPkNPTUVSQ0lBTCBFIElORFVTVFJJQUwgU0lMRkEgUy5BPC9SUz48VEQ+Mzk8L1REPjxSTkc+PEQ+MTI0MTwvRD48SD4yMjQwPC9IPjwvUk5HPjxGQT4yMDIxLTAyLTEyPC9GQT48UlNBUEs+PE0+c25EMmJKSUlmMkx0RWFPYkoxdzA0aExDZjFCSGdWWGptM2tZVTlFOThkMEY5VlRYNlBHTDlaQ2cvZ3ZFNGdRR2FGdDVwYzBxbTZVS1dLbW5QL0pFUVE9PTwvTT48RT5Bdz09PC9FPjwvUlNBUEs+PElESz4xMDA8L0lESz48L0RBPjxGUk1BIGFsZ29yaXRtbz1cIlNIQTF3aXRoUlNBXCI+bk1PazNkSnhEZ0hRUTArSEtVZGtWYWJEQjdobEtKemh0VWEzQVdPbTJXaHMxdUV1dzl6VE1uWUFlTWQ4cXJpWmN3R3g1NVFFWWdFNTZUeEoxaERLdVE9PTwvRlJNQT48L0NBRj48VFNURUQ+MjAyMS0wMi0yNlQxODoxMDozNDwvVFNURUQ+PC9ERD48RlJNVCBhbGdvcml0bW89IlNIQTF3aXRoUlNBIj5pUStzZTY1QlRMRGlzOVBKbjhoQjZock4vV0ZWeENBSnVYUk5SVlBFZHhuQTFqaGY3ZkdPenVXVWJ0cUZ0UlFIcStmbFQ4MnByQ0xHcmRaUTRXNkRHZz09PC9GUk1UPjwvVEVEPg==\"}";

                    if (!string.IsNullOrEmpty(response))
                    {

                        //que trucazo no!!!
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(response);
                        string json = JsonConvert.SerializeXmlNode(doc);
                        DTEFacCLResponse.FacCLDTEResponse facCLDTEResponse = JsonConvert.DeserializeObject<DTEFacCLResponse.FacCLDTEResponse>(json);

                        if (facCLDTEResponse.WSPLANO.Resultado == "True")
                        {
                            status = "0";
                            //folio = (Convert.ToInt64 (facCLDTEResponse.WSPLANO.Detalle.Documento.Folio)- 9200000000).ToString();
                            folio = facCLDTEResponse.WSPLANO.Detalle.Documento.Folio;
                            msg = "";
                            //se debe llamar por el timbre
                            tedbase64 = facCLDTEResponse.WSPLANO.Detalle.Documento.urlOriginal;// EncodeStrToBase64(ted);

                            textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                            jsonOut = string.Concat(textArrayResponse);

                        }
                        else
                        {
                            status = "500";
                            msg = facCLDTEResponse.WSPLANO.Detalle.Documento.Error;
                            textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                            jsonOut = string.Concat(textArrayResponse);
                            log.Info("Error From SEMPOS: ");
                        }

                        return;
                    }

                    status = "500";
                    msg = "ERROR WS FACCL retorno nulo o vacio:";
                    textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                    jsonOut = string.Concat(textArrayResponse);
                    log.Info("ProcessEvent** Error: " + msg);

                }
                catch (Exception e)
                {


                    status = "555";
                    log.Info("EXCEPTION2");
                    jsonOut = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                    log.Info("ProcessEvent *********** Error General -- " + e.Message);

                }
                return;




                log.Info("FIN");





            }
            catch (Exception e)
            {
                status = "555";
                log.Info("EXCEPTION2");
                jsonOut = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                log.Info("ProcessEvent *********** Error General -- " + e.Message);
            }
        }

        public void CreaGuiaVoucher(JObject json2, string SidDocPRISM, out string jsonOut)
        {
            jsonOut = "";
            NLog.Logger log = LogManager.GetCurrentClassLogger();
            string status = string.Empty;
            string folio = string.Empty;
            string tedbase64 = string.Empty;


            try
            {



                log.Info("INICIO  CALLDTEFACCL GUIA VOUCHER");
                string validacion = string.Empty;
                string errors = string.Empty;
                string numdoccliente = string.Empty;
                string[] textArrayResponse;
                string msg = string.Empty;
                string posflagtipo = string.Empty;
                string xmlres = string.Empty;
                string ted = string.Empty;



                log.Info("INICIO  Documento");
                log.Info(json2.ToString());

                numdoccliente = "";


                try
                {


                    FacCLDTEXML Dte = new FacCLDTEXML();

                    Dte.Documento = new DTEDocumento();
                    Dte.version = 1;
                    //ENCABEZADO
                    DTEDocumentoEncabezado Enc = new DTEDocumentoEncabezado();
                    Enc.IdDoc = new DTEDocumentoEncabezadoIdDoc();
                    //IdDOC
                    Enc.IdDoc.TipoDTE = 52;
                    Enc.IdDoc.Folio = 0;
                    Enc.IdDoc.FchEmis = DateTime.Now.Date.ToString("yyyy-MM-dd");
                    Enc.IdDoc.IndTraslado = "7";
                    Enc.IdDoc.TipoDespacho = "1";
                    Enc.IdDoc.PeriodoDesde = DateTime.Now.Date.ToString("yyyy-MM-dd");
                    Enc.IdDoc.PeriodoHasta = DateTime.Now.Date.ToString("yyyy-MM-dd");
                    Enc.IdDoc.FchVenc = DateTime.Now.Date.ToString("yyyy-MM-dd");
                    //EMISOR
                    Enc.Emisor = new DTEDocumentoEncabezadoEmisor();
                    Enc.Emisor.RUTEmisor = ParamValues.DTEFacCL_RutEmisor;
                    Enc.Emisor.RznSocEmisor = ParamValues.DTEFacCL_RznSocEmi.Norm(100);
                    Enc.Emisor.GiroEmisor = ParamValues.DTEFacCL_Giro.Norm(80);
                    Enc.Emisor.DirOrigen = ParamValues.DTEFacCL_Direccion.Norm(60);
                    Enc.Emisor.CmnaOrigen = ParamValues.DTEFacCL_Comuna.Norm(20);
                    Enc.Emisor.CiudadOrigen = ParamValues.DTEFacCL_Ciudad.Norm(20);

                    Dte.Documento.Encabezado = Enc;

                    //RECEPTOR


                    DTEDocumentoEncabezadoReceptor Rec = new DTEDocumentoEncabezadoReceptor();
                    Rec.RUTRecep = json2.GetValue("vendorpostalcode").ToString(); ;
                    Rec.CdgIntRecep = null;
                    Rec.RznSocRecep = json2.GetValue("vendorudf1string").ToString(); ;
                    Rec.GiroRecep = ((json2.GetValue("vendorudf2string").ToString() + " " + json2.GetValue("vendorudf3string").ToString()).Trim()).Trim().Norm(40);
                    Rec.Contacto = null;

                    Rec.DirRecep = json2.GetValue("vendoraddress4").ToString();
                    Rec.CmnaRecep = json2.GetValue("vendoraddress5").ToString();
                    Rec.CiudadRecep = json2.GetValue("vendoraddress6").ToString();



                    Dte.Documento.Encabezado.Receptor = Rec;

                    DTEDocumentoEncabezadoTotales Totales = new DTEDocumentoEncabezadoTotales();

                    // Creacion Detalle
                    Dte.Documento.Detalle = new List<DTEDocumentoDetalle>();

                    string jsonitem = json2.GetValue("recvitem").ToString();
                    JArray jsonArray = JArray.Parse(jsonitem);
                    log.Info("Detalle");
                    int contadordetalle = 0;
                    int valortotal = 0;

                    foreach (JObject jsonOperaciones in jsonArray.Children<JObject>())
                    {
                        if (contadordetalle < 41)
                        {
                            contadordetalle = contadordetalle + 1;
                            //string descuento = string.Empty;
                            DTEDocumentoDetalle det = new DTEDocumentoDetalle();

                            float QtyItem = jsonOperaciones["qty"].Value<float>();
                            float PrcItem = jsonOperaciones["price"].Value<float>();
                            PrcItem = PrcItem / (1 + (float.Parse(this.ParamValues.DTEFacCL_TasaIVA) / 100));

                            det.NroLinDet = contadordetalle;
                            det.NmbItem = (jsonOperaciones["description1"]).ToString();

                            det.UnmdItem = "UND";
                            det.QtyItem = QtyItem;
                            det.PrcItem = PrcItem;

                            det.MontoItem = Convert.ToInt32(QtyItem * PrcItem);
                            det.CdgItem = new DTEDocumentoDetalleCdgItem();
                            det.CdgItem.TpoCodigo = "ALU";
                            det.CdgItem.VlrCodigo = (jsonOperaciones["alu"]).ToString();

                            Dte.Documento.Detalle.Add(det);
                            valortotal += Convert.ToInt32(jsonOperaciones["extprice"].Value<float>());


                        }
                        else
                        {
                            msg = "No es posible superar los 40 Items";
                            throw new ApplicationException(msg);
                        }
                    }
                    Dte.Documento.Referencia = new List<DTEDocumentoReferencia>();

                    //TOTALES


                    int iva = 0;
                    float mntTotal = valortotal;
                    float mntNeto = 0;
                    log.Info("Totales");

                    mntNeto = mntTotal / (1 + (float.Parse(this.ParamValues.DTEFacCL_TasaIVA) / 100));
                    iva = Convert.ToInt32(mntTotal - mntNeto);

                    Totales.MntNeto = Convert.ToInt32(mntNeto);
                    Totales.MntExe = 0;
                    Totales.TasaIVA = this.ParamValues.DTEFacCL_TasaIVA;
                    Totales.IVA = iva;
                    Totales.MntTotal = Convert.ToInt32(mntTotal); ;



                    Dte.Documento.Encabezado.Totales = Totales;
                    //aca nos comunicamos 


                    string jsonBase64 = "";
                    string error = "";

                    log.Info("Procesa Documento en WS FACTURACION.cl: ");


                    cl.facturacion.ws.wsplano WS = new cl.facturacion.ws.wsplano();

                    cl.facturacion.ws.logininfo login = new cl.facturacion.ws.logininfo();
                    login.Usuario = Convert.ToBase64String(Encoding.ASCII.GetBytes(ParamValues.DTEFacCL_WS_USUARIO));//  "Q09NQ0FJVA==";
                    login.Rut = Convert.ToBase64String(Encoding.ASCII.GetBytes(ParamValues.DTEFacCL_RutEmisor));     // "MS05";
                    login.Clave = Convert.ToBase64String(Encoding.ASCII.GetBytes(ParamValues.DTEFacCL_WS_CLAVE));    // "cGxhbm85MTA5OA==";
                    login.Puerto = "MA==";
                    login.IncluyeLink = "1";

                    string sxml = Dte.ToXML();
                    log.Info(sxml);
                    string file = Dte.GetBase64(sxml, out error);



                    string response = WS.Procesar(login, file, 2);

                    log.Info("Response: " + response);

                    //jsonRespSEMPOS = "{\"RetCode\":\"0\",\"RetMsge\":\"OK\",\"Internal_Id\":\"590050609000065277\",\"SupplierId\":\"76171658-1\",\"DocType\":\"39\",\"LegalNumber\":\"1241\",\"TimeStamp\":\"2021-02-26T18:10:34\",\"DocTEDQ\":\"PFRFRCB2ZXJzaW9uID0iMS4wIj48REQ+PFJFPjc2MTcxNjU4LTE8L1JFPjxURD4zOTwvVEQ+PEY+MTI0MTwvRj48RkU+MjAyMS0wMi0yNjwvRkU+PFJSPjY2NjY2NjY2LTY8L1JSPjxSU1I+Q0xJRU5URSBCT0xFVEE8L1JTUj48TU5UPjQ5OTA8L01OVD48SVQxPkFCU09SQkVOVEVTIEFSRE8gREFZICBOSUdIVCBQQTwvSVQxPjxDQUYgdmVyc2lvbj1cIjEuMFwiPjxEQT48UkU+NzYxNzE2NTgtMTwvUkU+PFJTPkNPTUVSQ0lBTCBFIElORFVTVFJJQUwgU0lMRkEgUy5BPC9SUz48VEQ+Mzk8L1REPjxSTkc+PEQ+MTI0MTwvRD48SD4yMjQwPC9IPjwvUk5HPjxGQT4yMDIxLTAyLTEyPC9GQT48UlNBUEs+PE0+c25EMmJKSUlmMkx0RWFPYkoxdzA0aExDZjFCSGdWWGptM2tZVTlFOThkMEY5VlRYNlBHTDlaQ2cvZ3ZFNGdRR2FGdDVwYzBxbTZVS1dLbW5QL0pFUVE9PTwvTT48RT5Bdz09PC9FPjwvUlNBUEs+PElESz4xMDA8L0lESz48L0RBPjxGUk1BIGFsZ29yaXRtbz1cIlNIQTF3aXRoUlNBXCI+bk1PazNkSnhEZ0hRUTArSEtVZGtWYWJEQjdobEtKemh0VWEzQVdPbTJXaHMxdUV1dzl6VE1uWUFlTWQ4cXJpWmN3R3g1NVFFWWdFNTZUeEoxaERLdVE9PTwvRlJNQT48L0NBRj48VFNURUQ+MjAyMS0wMi0yNlQxODoxMDozNDwvVFNURUQ+PC9ERD48RlJNVCBhbGdvcml0bW89IlNIQTF3aXRoUlNBIj5pUStzZTY1QlRMRGlzOVBKbjhoQjZock4vV0ZWeENBSnVYUk5SVlBFZHhuQTFqaGY3ZkdPenVXVWJ0cUZ0UlFIcStmbFQ4MnByQ0xHcmRaUTRXNkRHZz09PC9GUk1UPjwvVEVEPg==\"}";

                    if (!string.IsNullOrEmpty(response))
                    {

                        //que trucazo no!!!
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(response);
                        string json = JsonConvert.SerializeXmlNode(doc);
                        DTEFacCLResponse.FacCLDTEResponse facCLDTEResponse = JsonConvert.DeserializeObject<DTEFacCLResponse.FacCLDTEResponse>(json);

                        if (facCLDTEResponse.WSPLANO.Resultado == "True")
                        {
                            status = "0";
                            //folio = (Convert.ToInt64 (facCLDTEResponse.WSPLANO.Detalle.Documento.Folio)- 9200000000).ToString();
                            folio = facCLDTEResponse.WSPLANO.Detalle.Documento.Folio;
                            msg = "";
                            //se debe llamar por el timbre
                            tedbase64 = facCLDTEResponse.WSPLANO.Detalle.Documento.urlOriginal;// EncodeStrToBase64(ted);

                            textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                            jsonOut = string.Concat(textArrayResponse);

                        }
                        else
                        {
                            status = "500";
                            msg = facCLDTEResponse.WSPLANO.Detalle.Documento.Error;
                            textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                            jsonOut = string.Concat(textArrayResponse);
                            log.Info("Error From SEMPOS: ");
                        }

                        return;
                    }

                    status = "500";
                    msg = "ERROR WS FACCL retorno nulo o vacio:";
                    textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                    jsonOut = string.Concat(textArrayResponse);
                    log.Info("ProcessEvent** Error: " + msg);

                }
                catch (Exception e)
                {


                    status = "555";
                    log.Info("EXCEPTION2");
                    jsonOut = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                    log.Info("ProcessEvent *********** Error General -- " + e.Message);

                }
                return;




                log.Info("FIN");





            }
            catch (Exception e)
            {
                status = "555";
                log.Info("EXCEPTION2");
                jsonOut = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                log.Info("ProcessEvent *********** Error General -- " + e.Message);
            }
        }
    }
}
