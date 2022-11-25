using Newtonsoft.Json.Linq;
using ProxyVyV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBNeT.SEMPOS;
using Newtonsoft.Json;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using NLog;

namespace VyVDbNet
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
                return Regex.Replace(value.Normalize(NormalizationForm.FormD), @"[^A-Za-z0-9|.,_\-@ ]", string.Empty);
            }
            catch (Exception)
            {
                return value;
            }



        }
    }

    public class DbNetDTE
    {
        private Log objLog;
        private string jsonRespSEMPOS = "";
        private bool esperar = true;
        private void Response(string message)
        {
            jsonRespSEMPOS = message;
            esperar = false;
        }


        public DbNetDTE(ProxyParam paramValues)
        {
            ParamValues = paramValues;
        }


        private ProxyParam ParamValues { get; set; }
        public void CreaGuiaVoucher(JObject json2, string SidDocPRISM, out string jsonOut)
        {
            jsonOut = "";
            NLog.Logger log = LogManager.GetCurrentClassLogger();
            string status = string.Empty;
            string folio = string.Empty;
            string tedbase64 = string.Empty;


            try
            {

                this.objLog.writeDTEDBNET("INICIO  GUIA VOUCHER");
                log.Debug(json2.ToString());
                string validacion = string.Empty;
                string errors = string.Empty;
                string numdoccliente = string.Empty;
                string[] textArrayResponse;
                string msg = string.Empty;
                string posflagtipo = string.Empty;
                string xmlres = string.Empty;
                string ted = string.Empty;
                string DTEDbNet_StoreId = ParamValues.DTEDbNet_StoreIdPrefijo + Convert.ToInt32(json2.GetValue("storecode").ToString()).ToString();
                string DTEDbNet_PosId = ParamValues.DTEDbNet_PosIdPrefijo + json2.GetValue("storecode").ToString() + "-" + json2.GetValue("workstation").ToString();


                this.objLog.writeDTEDBNET("INICIO  Documento");



                string json = "{\"StoreId\":\"" + DTEDbNet_StoreId + "\",\"PosId\":\"" + DTEDbNet_PosId + "\",\"StoreURL\":\"" + ParamValues.DTEDbNet_StoreURL + "\"}";
                this.objLog.writeDTEDBNET("SEMInitialize: ");
                this.objLog.writeDTEDBNET("REQUEST: " + json);
                ResponseCallback resp = new ResponseCallback(Response);
                SEMContext.Instance.Initialize(json, resp);
                int timeout = 0;
                while (esperar)
                {
                    timeout++;
                    Thread.Sleep(100);
                    if (timeout >= 100) break;
                }
                this.objLog.writeDTEDBNET("RESPONSE: " + json);
                this.objLog.writeDTEDBNET(jsonRespSEMPOS);

                DocumentDbNet doc = new DocumentDbNet();
                doc.documentId = new DocumentId();
                doc.documentId.DocType = "52";
                doc.documentId.DispatchType = "1";
                doc.documentId.DocumentIdLine = "1";
                doc.documentId.GoodsTransferType = "7";
                doc.documentId.IssueDate = DateTime.Now.ToString("yyyy-MM-dd");
                doc.documentId.InfoType = "DocumentId";

                doc.supplier = new Supplier();
                doc.supplier.SupplierId = ParamValues.DTEDbNet_RutEmisor;
                doc.supplier.SupplierName = ParamValues.DTEDbNet_RznSocEmi.Norm(100);
                doc.supplier.SupplierFax = "";
                doc.supplier.SupplierPhone = ParamValues.DTEDbNet_Telefono.Norm(20);
                doc.supplier.SupplierActivity = ParamValues.DTEDbNet_Giro.Norm(80);
                doc.supplier.SupplierActivityCode = ParamValues.DTEDbNet_ACTECO.Norm(6);
                doc.supplier.SupplierAddress = ParamValues.DTEDbNet_Direccion.Norm(60);
                doc.supplier.SupplierCity = null;// ParamValues.DTEDbNet_Ciudad.Norm(20);
                doc.supplier.SupplierCitySubdivision = ParamValues.DTEDbNet_Comuna.Norm(20);


                doc.customer = new Customer();
                doc.customer.CustomerId = json2.GetValue("vendorpostalcode").ToString();
                doc.customer.CustomerName = json2.GetValue("vendorudf1string").ToString();
                doc.customer.CustomerCitySubdivision = json2.GetValue("vendoraddress6").ToString();
                doc.customer.CustomerLine = "1";
                doc.customer.CustomerAddress = json2.GetValue("vendoraddress4").ToString();
                doc.customer.CustomerActivity = ((json2.GetValue("vendorudf2string").ToString() + " " + json2.GetValue("vendorudf3string").ToString()).Trim()).Trim().Norm(40);
                doc.customer.InfoType = "Customer";



                doc.transportation = new Transportation();
                doc.transportation.DriverName = "No Disponible";
                doc.transportation.TransportationLine = "1";
                doc.transportation.DriverLegalId = ParamValues.DTEDbNet_RutEmisor;
                doc.transportation.DestinationCitySubdivision = json2.GetValue("vendoraddress6").ToString();
                doc.transportation.DestinationAddress = json2.GetValue("vendoraddress4").ToString();
                doc.transportation.CarrierLegalId = ParamValues.DTEDbNet_RutEmisor;
                doc.transportation.InfoType = "Transportation";
                doc.transportation.LicensePlate = "XXXXXX";

                this.objLog.writeDTEDBNET("DETALLE");
                int contadordetalle = 0;
                string jsonitem = json2.GetValue("recvitem").ToString();
                JArray jsonArrayitem = JArray.Parse(jsonitem);
                int valortotal = 0;
                foreach (JObject jsonOperaciones in jsonArrayitem.Children<JObject>())
                {
                    if (contadordetalle < 41)
                    {
                        contadordetalle = contadordetalle + 1;
                        Item det = new Item();
                        float QtyItem = jsonOperaciones["qty"].Value<float>();

                        float PrcItem = jsonOperaciones["price"].Value<float>();
                        PrcItem = PrcItem / (1 + (float.Parse(this.ParamValues.DTEDbNet_TasaIVA) / 100));
                        det.ItemLine = contadordetalle.ToString();
                        det.ItemName = (jsonOperaciones["description1"]).ToString();
                        det.ItemMeasureUnit = "UND";
                        det.LineQuantity = QtyItem.ToString().Replace(',', '.');
                        det.ItemPrice = PrcItem.ToString().Replace(",", ".");
                        det.LineAmount = Convert.ToInt32(QtyItem * PrcItem).ToString();
                        det.ItemCode = (jsonOperaciones["alu"]).ToString();
                        det.ItemCodeType = "ALU";
                        doc.Items.Add(det);
                        valortotal += Convert.ToInt32(jsonOperaciones["extprice"].Value<float>());


                    }
                    else
                    {
                        msg = "No es posible superar los 40 Items";
                        throw new ApplicationException(msg);
                    }

                }

                doc.legalMonetaryTotal = new LegalMonetaryTotal();


                int iva = 0;
                float mntTotal = valortotal;
                float mntNeto = 0;
                this.objLog.writeDTEDBNET("Totales");

                mntNeto = mntTotal / (1 + (float.Parse(this.ParamValues.DTEDbNet_TasaIVA) / 100));
                iva = Convert.ToInt32(mntTotal - mntNeto);
                doc.legalMonetaryTotal = new LegalMonetaryTotal();
                doc.legalMonetaryTotal.TaxableAmount = Convert.ToInt32(mntNeto).ToString();
                doc.legalMonetaryTotal.VatPercent = this.ParamValues.DTESignature_TasaIVA;
                doc.legalMonetaryTotal.ExemptAmount = "0";
                doc.legalMonetaryTotal.VatAmount = iva.ToString();
                doc.legalMonetaryTotal.TotalAmount = Convert.ToInt32(mntTotal).ToString();


                doc.additionalValues = new AdditionalValues();

                doc.additionalValues.Val1 = "";
                doc.additionalValues.Val2 = "";
                doc.additionalValues.Val3 = "";
                doc.additionalValues.Val4 = "";
                doc.additionalValues.Val5 = "";
                doc.additionalValues.Val6 = "";
                doc.additionalValues.Val7 = "";
                doc.additionalValues.Val8 = "";
                doc.additionalValues.Val9 = "";

                //aca comunicamos con la DLL de DBNET y analisamos la respuesta.

                jsonRespSEMPOS = "";
                string jsonBase64 = "";
                string error = "";
                if (!doc.GetBase64(out jsonBase64, out error)) log.Error(error);
                error = "";

                this.objLog.writeDTEDBNET("SEMGenerateDocument: ");
                GenerateDocument generateDocument = new GenerateDocument();

                generateDocument.DocType = doc.documentId.DocType;
                generateDocument.Internal_Id = "52-" + SidDocPRISM;
                generateDocument.SupplierId = ParamValues.DTEDbNet_RutEmisor;
                generateDocument.UserId = DTEDbNet_PosId;
                generateDocument.DocInfo = jsonBase64;


                json = generateDocument.GetJson();
                this.objLog.writeDTEDBNET("REQUEST: " + json);
                SEMContext.Instance.SEMGenerateDocument(json, resp);
                esperar = true;
                timeout = 0;
                while (esperar)
                {
                    timeout++;
                    Thread.Sleep(100);
                    if (timeout >= 100) break;
                }

                this.objLog.writeDTEDBNET("Response: " + jsonRespSEMPOS);

                if (!string.IsNullOrEmpty(jsonRespSEMPOS))
                {
                    ResponseDbNet responseDbNet = JsonConvert.DeserializeObject<ResponseDbNet>(jsonRespSEMPOS);

                    if (responseDbNet.RetCode == "0")
                    {
                        status = "0";
                        folio = responseDbNet.LegalNumber;// folioSignature.ToString();
                        msg = "";
                        this.objLog.writeDTEDBNET(ted);
                        tedbase64 = responseDbNet.DocTEDQ;// EncodeStrToBase64(ted);

                        textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                        jsonOut = string.Concat(textArrayResponse);

                    }
                    else
                    {
                        status = "500";
                        msg = responseDbNet.RetMsge;
                        textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                        jsonOut = string.Concat(textArrayResponse);
                        this.objLog.writeDTEDBNET("Error From SEMPOS: ");
                    }

                    return;
                }

                status = "500";
                msg = "ERROR SEMGenerateDocument retorno nulo o vacio:";
                textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                jsonOut = string.Concat(textArrayResponse);
                this.objLog.writeDTEDBNET("ProcessEvent** Error: " + msg);

            }
            catch (Exception e)
            {


                status = "555";
                this.objLog.writeDTEDBNET("EXCEPTION2");
                jsonOut = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                this.objLog.writeDTEDBNET("ProcessEvent *********** Error General -- " + e.Message);

            }
            return;


        }

        public void CreaGuiaTransfer(JObject json2, string SidDocPRISM, out string jsonOut)
        {
            jsonOut = "";
            NLog.Logger log = LogManager.GetCurrentClassLogger();
            this.objLog = new Log();
            string status = string.Empty;
            string folio = string.Empty;
            string tedbase64 = string.Empty;


            try
            {

                this.objLog.writeDTEDBNET("INICIO  GUIA TRANSFER");
                this.objLog.writeDTEDBNET(json2.ToString());
                string validacion = string.Empty;
                string errors = string.Empty;
                string numdoccliente = string.Empty;
                string[] textArrayResponse;
                string msg = string.Empty;
                string posflagtipo = string.Empty;
                string xmlres = string.Empty;
                string ted = string.Empty;
                string DTEDbNet_StoreId = ParamValues.DTEDbNet_StoreIdPrefijo + Convert.ToInt32(json2.GetValue("outstorecode").ToString()).ToString();
                string DTEDbNet_PosId = ParamValues.DTEDbNet_PosIdPrefijo + json2.GetValue("outstorecode").ToString() + "-" + json2.GetValue("workstation").ToString();


                this.objLog.writeDTEDBNET("INICIO  Documento");



                string json = "{\"StoreId\":\"" + DTEDbNet_StoreId + "\",\"PosId\":\"" + DTEDbNet_PosId + "\",\"StoreURL\":\"" + ParamValues.DTEDbNet_StoreURL + "\"}";
                this.objLog.writeDTEDBNET("SEMInitialize: ");
                this.objLog.writeDTEDBNET("REQUEST: " + json);
                ResponseCallback resp = new ResponseCallback(Response);
                SEMContext.Instance.Initialize(json, resp);
                int timeout = 0;
                while (esperar)
                {
                    timeout++;
                    Thread.Sleep(100);
                    if (timeout >= 100) break;
                }
                this.objLog.writeDTEDBNET("RESPONSE: " + json);
                this.objLog.writeDTEDBNET(jsonRespSEMPOS);

                DocumentDbNet doc = new DocumentDbNet();
                doc.documentId = new DocumentId();
                doc.documentId.DocType = "52";
                doc.documentId.DispatchType = "1";
                doc.documentId.DocumentIdLine = "1";
                doc.documentId.GoodsTransferType = "5";
                doc.documentId.IssueDate = DateTime.Now.ToString("yyyy-MM-dd");
                doc.documentId.InfoType = "DocumentId";

                doc.supplier = new Supplier();
                doc.supplier.SupplierId = ParamValues.DTEDbNet_RutEmisor;
                doc.supplier.SupplierName = ParamValues.DTEDbNet_RznSocEmi.Norm(100);
                doc.supplier.SupplierFax = "";
                doc.supplier.SupplierPhone = ParamValues.DTEDbNet_Telefono.Norm(20);
                doc.supplier.SupplierActivity = ParamValues.DTEDbNet_Giro.Norm(80);
                doc.supplier.SupplierActivityCode = ParamValues.DTEDbNet_ACTECO.Norm(6);
                doc.supplier.SupplierAddress = ParamValues.DTEDbNet_Direccion.Norm(60);
                doc.supplier.SupplierCity = null;// ParamValues.DTEDbNet_Ciudad.Norm(20);
                doc.supplier.SupplierCitySubdivision = ParamValues.DTEDbNet_Comuna.Norm(20);


                doc.customer = new Customer();
                doc.customer.CustomerId = json2.GetValue("origstorezip").ToString();
                doc.customer.CustomerName = json2.GetValue("instoreudf1string").ToString();
                doc.customer.CustomerCitySubdivision = json2.GetValue("instoreaddress2").ToString();
                doc.customer.CustomerLine = "1";
                doc.customer.CustomerAddress = json2.GetValue("instoreaddress1").ToString();
                doc.customer.CustomerActivity = ((json2.GetValue("instoreudf2string").ToString() + " " + json2.GetValue("instoreudf3string").ToString()).Trim()).Trim().Norm(40);
                doc.customer.InfoType = "Customer";



                doc.transportation = new Transportation();
                doc.transportation.DriverName = "No Disponible";
                doc.transportation.TransportationLine = "1";
                doc.transportation.DriverLegalId = ParamValues.DTEDbNet_RutEmisor;
                doc.transportation.DestinationCitySubdivision = json2.GetValue("instoreaddress2").ToString();
                doc.transportation.DestinationAddress = json2.GetValue("instoreaddress1").ToString();
                doc.transportation.CarrierLegalId = ParamValues.DTEDbNet_RutEmisor;
                doc.transportation.InfoType = "Transportation";
                doc.transportation.LicensePlate = "XXXXXX";

                doc.legalMonetaryTotal = new LegalMonetaryTotal();


                int iva = 0;
                float mntTotal = 0;
                float mntNeto = 0;
                this.objLog.writeDTEDBNET("Totales");

                mntTotal = json2["docpricetotal"].Value<Int32>();// Int32.Parse(json2.GetValue("saletotalamt").ToString());
                mntNeto = mntTotal / (1 + (float.Parse(this.ParamValues.DTEDbNet_TasaIVA) / 100));
                iva = Convert.ToInt32(mntTotal - mntNeto);
                doc.legalMonetaryTotal = new LegalMonetaryTotal();
                doc.legalMonetaryTotal.TaxableAmount = Convert.ToInt32(mntNeto).ToString();
                doc.legalMonetaryTotal.VatPercent = this.ParamValues.DTESignature_TasaIVA;
                doc.legalMonetaryTotal.ExemptAmount = "0";
                doc.legalMonetaryTotal.VatAmount = iva.ToString();
                doc.legalMonetaryTotal.TotalAmount = Convert.ToInt32(mntTotal).ToString();


                this.objLog.writeDTEDBNET("DETALLE");
                int contadordetalle = 0;
                string jsonitem = json2.GetValue("slipitem").ToString();
                JArray jsonArrayitem = JArray.Parse(jsonitem);
                foreach (JObject jsonOperaciones in jsonArrayitem.Children<JObject>())
                {
                    if (contadordetalle < 41)
                    {
                        contadordetalle = contadordetalle + 1;
                        Item det = new Item();
                        float QtyItem = jsonOperaciones["qty"].Value<float>();

                        float PrcItem = jsonOperaciones["price"].Value<float>();
                        PrcItem = PrcItem / (1 + (float.Parse(this.ParamValues.DTEDbNet_TasaIVA) / 100));
                        det.ItemLine = contadordetalle.ToString();
                        det.ItemName = (jsonOperaciones["description1"]).ToString();
                        det.ItemMeasureUnit = "UND";
                        det.LineQuantity = QtyItem.ToString().Replace(',', '.');
                        det.ItemPrice = PrcItem.ToString().Replace(",", ".");
                        det.LineAmount = Convert.ToInt32(QtyItem * PrcItem).ToString();
                        det.ItemCode = (jsonOperaciones["alu"]).ToString();
                        det.ItemCodeType = "ALU";
                        doc.Items.Add(det);
                    }
                    else
                    {
                        msg = "No es posible superar los 40 Items";
                        throw new ApplicationException(msg);
                    }

                }

                doc.additionalValues = new AdditionalValues();

                doc.additionalValues.Val1 = "";
                doc.additionalValues.Val2 = "";
                doc.additionalValues.Val3 = "";
                doc.additionalValues.Val4 = "";
                doc.additionalValues.Val5 = "";
                doc.additionalValues.Val6 = "";
                doc.additionalValues.Val7 = "";
                doc.additionalValues.Val8 = "";
                doc.additionalValues.Val9 = "";

                //aca comunicamos con la DLL de DBNET y analisamos la respuesta.

                jsonRespSEMPOS = "";
                string jsonBase64 = "";
                string error = "";
                if (!doc.GetBase64(out jsonBase64, out error)) log.Error(error);
                error = "";

                this.objLog.writeDTEDBNET("SEMGenerateDocument: ");
                GenerateDocument generateDocument = new GenerateDocument();

                generateDocument.DocType = doc.documentId.DocType;
                generateDocument.Internal_Id = "52-" + SidDocPRISM;
                generateDocument.SupplierId = ParamValues.DTEDbNet_RutEmisor;
                generateDocument.UserId = DTEDbNet_PosId;
                generateDocument.DocInfo = jsonBase64;


                json = generateDocument.GetJson();
                this.objLog.writeDTEDBNET("REQUEST: " + json);
                SEMContext.Instance.SEMGenerateDocument(json, resp);
                esperar = true;
                timeout = 0;
                while (esperar)
                {
                    timeout++;
                    Thread.Sleep(100);
                    if (timeout >= 100) break;
                }

                this.objLog.writeDTEDBNET("Response: " + jsonRespSEMPOS);

                if (!string.IsNullOrEmpty(jsonRespSEMPOS))
                {
                    ResponseDbNet responseDbNet = JsonConvert.DeserializeObject<ResponseDbNet>(jsonRespSEMPOS);

                    if (responseDbNet.RetCode == "0")
                    {
                        status = "0";
                        folio = responseDbNet.LegalNumber;// folioSignature.ToString();
                        msg = "";
                        this.objLog.writeDTEDBNET(ted);
                        tedbase64 = responseDbNet.DocTEDQ;// EncodeStrToBase64(ted);

                        textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                        jsonOut = string.Concat(textArrayResponse);

                    }
                    else
                    {
                        status = "500";
                        msg = responseDbNet.RetMsge;
                        textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                        jsonOut = string.Concat(textArrayResponse);
                        this.objLog.writeDTEDBNET("Error From SEMPOS: ");
                    }

                    return;
                }

                status = "500";
                msg = "ERROR SEMGenerateDocument retorno nulo o vacio:";
                textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                jsonOut = string.Concat(textArrayResponse);
                this.objLog.writeDTEDBNET("ProcessEvent** Error: " + msg);

            }
            catch (Exception e)
            {


                status = "555";
                this.objLog.writeDTEDBNET("EXCEPTION2");
                jsonOut = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                this.objLog.writeDTEDBNET("ProcessEvent *********** Error General -- " + e.Message);

            }
            return;


        }

        public void CreaDocumentoElectronico(JObject json2, string SidDocPRISM, CCustMessage message, JObject jsoncustomer2, JObject jsonreferencia, out string jsonOut)
        {
            jsonOut = "";
            NLog.Logger log = LogManager.GetCurrentClassLogger();
            this.objLog = new Log();
            string status = string.Empty;
            string folio = string.Empty;
            string tedbase64 = string.Empty;
            try
            {



                this.objLog.writeDTEDBNET("INICIO  CALLDTEDBNET");
                string validacion = string.Empty;
                string errors = string.Empty;
                string numdoccliente = string.Empty;
                string[] textArrayResponse;
                string msg = string.Empty;
                string posflagtipo = string.Empty;
                string xmlres = string.Empty;
                string ted = string.Empty;
                string DTEDbNet_StoreId = ParamValues.DTEDbNet_StoreIdPrefijo + Convert.ToInt32(json2.GetValue("storecode").ToString()).ToString();
                string DTEDbNet_PosId = ParamValues.DTEDbNet_PosIdPrefijo + json2.GetValue("storecode").ToString() + "-" + json2.GetValue("workstationno").ToString();


                this.objLog.writeDTEDBNET("INICIO  Documento ");
                this.objLog.writeDTEDBNET(json2.ToString());
                this.objLog.writeDTEDBNET("fecha documento "+ json2.GetValue("createddatetime").ToString());
                numdoccliente = "";
                if (jsoncustomer2 != null)
                {
                    numdoccliente = jsoncustomer2.GetValue("info1").ToString();
                    if (numdoccliente == "")
                    {
                        numdoccliente = jsoncustomer2.GetValue("info2").ToString();

                    }
                }

                this.objLog.writeDTEDBNET("numdoc cliente : " + numdoccliente);

                string posflag = json2.GetValue("posflag1").ToString().Trim().Substring(0, 2);

                if (posflag == "39")// BOLETA ELECTRONICA
                {
                    try
                    {
                        this.objLog.writeDTEDBNET("INICIO  BOLETA ELECTRONICA");

                        string json = "{\"StoreId\":\"" + DTEDbNet_StoreId + "\",\"PosId\":\"" + DTEDbNet_PosId + "\",\"StoreURL\":\"" + ParamValues.DTEDbNet_StoreURL + "\"}";
                        this.objLog.writeDTEDBNET("SEMInitialize: ");
                        this.objLog.writeDTEDBNET("REQUEST: " + json);
                        ResponseCallback resp = new ResponseCallback(Response);
                        SEMContext.Instance.Initialize(json, resp);
                        int timeout = 0;
                        while (esperar)
                        {
                            timeout++;
                            Thread.Sleep(100);
                            if (timeout >= 100) break;
                        }
                        this.objLog.writeDTEDBNET("RESPONSE: " + json);
                        this.objLog.writeDTEDBNET(jsonRespSEMPOS);
                        // PROCESO BOLETA ELEC
                        this.objLog.writeDTEDBNET("Paso 1 ");
                        string fecha_documento_ori = json2.GetValue("createddatetime").ToString();
                        string fecha_documento_corta = fecha_documento_ori.Substring(0,10);
                        string dia = fecha_documento_corta.Substring(0,2);
                        string mes = fecha_documento_corta.Substring(3, 2);
                        string ano = fecha_documento_corta.Substring(6, 4);
                        String fecha_formateada = ano + "-" + mes + "-"+ dia;
                        DocumentDbNet doc = new DocumentDbNet();
                        doc.documentId = new DocumentId();
                        doc.documentId.DocType = "39";
                        doc.documentId.IssueDate = fecha_formateada; // DateTime.Now.Date.ToString("yyyy-MM-dd");
                        doc.documentId.ServiceIndicator = "3";
                        doc.documentId.NetAmountIndicator = "2";
                        doc.documentId.DueDate = doc.documentId.IssueDate;
                        doc.supplier = new Supplier();
                        doc.supplier.SupplierId = ParamValues.DTEDbNet_RutEmisor;
                        doc.supplier.SupplierName = ParamValues.DTEDbNet_RznSocEmi.Norm(100);
                        doc.supplier.SupplierFax = "";
                        //doc.supplier.SupplierPhone = ParamValues.DTEDbNet_Telefono.Norm(20);
                        doc.supplier.SupplierActivity = ParamValues.DTEDbNet_Giro.Norm(80);
                        doc.supplier.SupplierActivityCode = null;// ParamValues.DTEDbNet_ACTECO.Norm(6);
                        doc.supplier.SupplierAddress = ParamValues.DTEDbNet_Direccion.Norm(60);
                        doc.supplier.SupplierCity = ParamValues.DTEDbNet_Ciudad.Norm(20);
                        doc.supplier.SupplierCitySubdivision = ParamValues.DTEDbNet_Comuna.Norm(20);

                        this.objLog.writeDTEDBNET("Paso 2 ");
                        //RECEPTOR
                        if (string.IsNullOrEmpty(numdoccliente)) numdoccliente = ParamValues.DTEDbNet_RutReceptordefault;
                        doc.customer = new Customer();
                        doc.customer.CustomerId = numdoccliente;
                        this.objLog.writeDTEDBNET("jsoncustomer2 " + jsoncustomer2);
                        string nombrecliente = jsoncustomer2.GetValue("first_name").ToString() + " " + jsoncustomer2.GetValue("last_name").ToString();
                        if ((nombrecliente.Trim() == "") || ((nombrecliente == " ")))
                        {
                            nombrecliente = this.ParamValues.DTESignature_NombreClientedefault;
                        }
                        doc.customer.CustomerName = nombrecliente;
                        this.objLog.writeDTEDBNET("Paso 3 ");
                        //Direcciones
                        if (!String.IsNullOrEmpty(jsoncustomer2.GetValue("primary_address_line_1").ToString()))
                        {
                            doc.customer.CustomerAddress = jsoncustomer2.GetValue("primary_address_line_1").ToString() + " " + jsoncustomer2.GetValue("primary_address_line_2").ToString();
                        }
                        if (!String.IsNullOrEmpty(jsoncustomer2.GetValue("primary_address_line_4").ToString()))
                        {
                            doc.customer.CustomerCitySubdivision = jsoncustomer2.GetValue("primary_address_line_4").ToString();
                        }
                        //if (!String.IsNullOrEmpty(json2.GetValue("btaddressline3").ToString()))
                        //{
                        //    doc.customer.CustomerCitySubdivision = json2.GetValue("btaddressline3").ToString();
                        //}
                        this.objLog.writeDTEDBNET("Paso 4 ");
                        //TOTALES
                        doc.legalMonetaryTotal = new LegalMonetaryTotal();
                        if (json2.GetValue("saletotalamt").ToString() == "0")
                        {
                            doc.legalMonetaryTotal.TaxableAmount = "1";
                            doc.legalMonetaryTotal.ExemptAmount = "0";
                            doc.legalMonetaryTotal.VatAmount = "0";
                            doc.legalMonetaryTotal.TotalAmount = "1";
                            doc.legalMonetaryTotal.PayableAmount = "1";
                        }
                        else
                        {
                            int iva = 0;
                            int mntTotal = 0;
                            int mntNeto = 0;
                            this.objLog.writeDTEDBNET("Totales");
                            iva = json2["saletotaltaxamt"].Value<Int32>();//.["saletotaltaxamt").ToString());
                                                                          //total.TasaIVA = float.Parse(this.ParamValues.DTESignature_TasaIVA);
                            mntTotal = json2["saletotalamt"].Value<Int32>();// Int32.Parse(json2.GetValue("saletotalamt").ToString());
                            mntNeto = mntTotal - iva;
                            doc.legalMonetaryTotal.TaxableAmount = mntNeto.ToString();
                            doc.legalMonetaryTotal.ExemptAmount = "0";
                            doc.legalMonetaryTotal.VatAmount = iva.ToString();
                            doc.legalMonetaryTotal.TotalAmount = mntTotal.ToString();
                            doc.legalMonetaryTotal.PayableAmount = mntTotal.ToString();
                        }
                        // Creacion Detalle
                        string strjson3 = json2.GetValue("docitem").ToString();
                        JArray jsonArray = JArray.Parse(strjson3);
                        this.objLog.writeDTEDBNET("Detalle");
                        int contadordetalle = 0;
                        foreach (JObject jsonOperaciones in jsonArray.Children<JObject>())
                        {
                            if (contadordetalle < 61)
                            {
                                contadordetalle = contadordetalle + 1;
                                //string descuento = string.Empty;
                                Item det = new Item();
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
                                det.ItemLine = contadordetalle.ToString();// jsonOperaciones["itempos"].Value<Int32>().ToString();// Int32.Parse((jsonOperaciones["itempos"]).ToString());
                                det.ItemName = (jsonOperaciones["description1"]).ToString();
                                //det. = (jsonOperaciones["description1"]).ToString();
                                //string cantidad = (jsonOperaciones["qty"]).ToString();
                                //cantidad = cantidad.Replace(',', '.');
                                det.ItemMeasureUnit = "UND";
                                det.LineQuantity = QtyItem.ToString().Replace(',', '.');// float.Parse(cantidad);
                                det.ItemPrice = PrcItem.ToString().Replace(",", ".");// float.Parse((jsonOperaciones["origprice"]).ToString());
                                                                                     //DTEValidaciones(jsonOperaciones["origprice"].ToString(), "Precio Item", 19, 2);
                                                                                     //string iva = (jsonOperaciones["taxamt"]).ToString().Split(',')[0];
                                                                                     //string precio = (jsonOperaciones["price"]).ToString();
                                                                                     //float montoitem = (Int32.Parse(precio) * float.Parse(cantidad));
                                                                                     //det.MontoItem = Convert.ToInt32(montoitem);
                                det.LineAmount = Convert.ToInt32(QtyItem * PrcItem).ToString();

                                det.ItemCode = (jsonOperaciones["alu"]).ToString();
                                det.ItemCodeType = "ALU";

                                doc.Items.Add(det);



                            }
                            else
                            {
                                msg = "No es posible superar los 60 Items";
                                throw new ApplicationException(msg);
                            }
                        }


                        doc.additionalValues = new AdditionalValues();

                        doc.additionalValues.Val1 = "";
                        doc.additionalValues.Val2 = "";
                        doc.additionalValues.Val3 = "";
                        doc.additionalValues.Val4 = "";
                        doc.additionalValues.Val5 = "";
                        doc.additionalValues.Val6 = "";
                        doc.additionalValues.Val7 = "";
                        doc.additionalValues.Val8 = "";
                        doc.additionalValues.Val9 = "";

                        //agregar referencia de devolucion.

                        //Folio

                        //aca comunicamos con la DLL de DBNET y analisamos la respuesta.

                        jsonRespSEMPOS = "";
                        string jsonBase64 = "";
                        string error = "";
                        if (doc.GetBase64(out jsonBase64, out error)) log.Error(error);
                        error = "";

                        this.objLog.writeDTEDBNET("SEMGenerateDocument: ");
                        GenerateDocument generateDocument = new GenerateDocument();

                        generateDocument.DocType = doc.documentId.DocType;
                        generateDocument.Internal_Id = "39-" + SidDocPRISM;
                        generateDocument.SupplierId = ParamValues.DTEDbNet_RutEmisor;
                        generateDocument.UserId = DTEDbNet_PosId;// "SILFA-POS1";
                        generateDocument.DocInfo = jsonBase64;


                        json = generateDocument.GetJson();
                        this.objLog.writeDTEDBNET("REQUEST: " + json);
                        SEMContext.Instance.SEMGenerateDocument(json, resp);
                        esperar = true;
                        timeout = 0;
                        while (esperar)
                        {
                            timeout++;
                            Thread.Sleep(100);
                            if (timeout >= 100) break;
                        }

                        this.objLog.writeDTEDBNET("Response: " + jsonRespSEMPOS);

                        //jsonRespSEMPOS = "{\"RetCode\":\"0\",\"RetMsge\":\"OK\",\"Internal_Id\":\"590050609000065277\",\"SupplierId\":\"76171658-1\",\"DocType\":\"39\",\"LegalNumber\":\"1241\",\"TimeStamp\":\"2021-02-26T18:10:34\",\"DocTEDQ\":\"PFRFRCB2ZXJzaW9uID0iMS4wIj48REQ+PFJFPjc2MTcxNjU4LTE8L1JFPjxURD4zOTwvVEQ+PEY+MTI0MTwvRj48RkU+MjAyMS0wMi0yNjwvRkU+PFJSPjY2NjY2NjY2LTY8L1JSPjxSU1I+Q0xJRU5URSBCT0xFVEE8L1JTUj48TU5UPjQ5OTA8L01OVD48SVQxPkFCU09SQkVOVEVTIEFSRE8gREFZICBOSUdIVCBQQTwvSVQxPjxDQUYgdmVyc2lvbj1cIjEuMFwiPjxEQT48UkU+NzYxNzE2NTgtMTwvUkU+PFJTPkNPTUVSQ0lBTCBFIElORFVTVFJJQUwgU0lMRkEgUy5BPC9SUz48VEQ+Mzk8L1REPjxSTkc+PEQ+MTI0MTwvRD48SD4yMjQwPC9IPjwvUk5HPjxGQT4yMDIxLTAyLTEyPC9GQT48UlNBUEs+PE0+c25EMmJKSUlmMkx0RWFPYkoxdzA0aExDZjFCSGdWWGptM2tZVTlFOThkMEY5VlRYNlBHTDlaQ2cvZ3ZFNGdRR2FGdDVwYzBxbTZVS1dLbW5QL0pFUVE9PTwvTT48RT5Bdz09PC9FPjwvUlNBUEs+PElESz4xMDA8L0lESz48L0RBPjxGUk1BIGFsZ29yaXRtbz1cIlNIQTF3aXRoUlNBXCI+bk1PazNkSnhEZ0hRUTArSEtVZGtWYWJEQjdobEtKemh0VWEzQVdPbTJXaHMxdUV1dzl6VE1uWUFlTWQ4cXJpWmN3R3g1NVFFWWdFNTZUeEoxaERLdVE9PTwvRlJNQT48L0NBRj48VFNURUQ+MjAyMS0wMi0yNlQxODoxMDozNDwvVFNURUQ+PC9ERD48RlJNVCBhbGdvcml0bW89IlNIQTF3aXRoUlNBIj5pUStzZTY1QlRMRGlzOVBKbjhoQjZock4vV0ZWeENBSnVYUk5SVlBFZHhuQTFqaGY3ZkdPenVXVWJ0cUZ0UlFIcStmbFQ4MnByQ0xHcmRaUTRXNkRHZz09PC9GUk1UPjwvVEVEPg==\"}";

                        if (!string.IsNullOrEmpty(jsonRespSEMPOS))
                        {
                            ResponseDbNet responseDbNet = JsonConvert.DeserializeObject<ResponseDbNet>(jsonRespSEMPOS);

                            if (responseDbNet.RetCode == "0")
                            {
                                status = "0";
                                folio = responseDbNet.LegalNumber;// folioSignature.ToString();
                                msg = "";
                                this.objLog.writeDTEDBNET(ted);
                                tedbase64 = responseDbNet.DocTEDQ;// EncodeStrToBase64(ted);

                                textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                jsonOut = string.Concat(textArrayResponse);

                            }
                            else
                            {
                                status = "500";
                                msg = responseDbNet.RetMsge;
                                textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                jsonOut = string.Concat(textArrayResponse);
                                this.objLog.writeDTEDBNET("Error From SEMPOS: ");
                            }

                            return;
                        }

                        status = "500";
                        msg = "ERROR SEMGenerateDocument retorno nulo o vacio:";
                        textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                        jsonOut = string.Concat(textArrayResponse);
                        this.objLog.writeDTEDBNET("ProcessEvent** Error: " + msg);

                    }
                    catch (Exception e)
                    {


                        status = "555";
                        this.objLog.writeDTEDBNET("EXCEPTION2");
                        jsonOut = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                        this.objLog.writeDTEDBNET("ProcessEvent *********** Error General -- " + e.Message);

                    }
                    return;
                }
                else if (posflag == "33")// FACTURA ELECTRONICA
                {
                    try
                    {
                        this.objLog.writeDTEDBNET("INICIO  FACTURA ELECTRONICA");
                        this.objLog.writeDTEDBNET("SEMInitialize: ");
                        string json = "{\"StoreId\":\"" + DTEDbNet_StoreId + "\",\"PosId\":\"" + DTEDbNet_PosId + "\",\"StoreURL\":\"" + ParamValues.DTEDbNet_StoreURL + "\"}";
                        this.objLog.writeDTEDBNET("REQUEST: " + json);
                        ResponseCallback resp = new ResponseCallback(Response);
                        SEMContext.Instance.Initialize(json, resp);
                        int timeout = 0;
                        while (esperar)
                        {
                            timeout++;
                            Thread.Sleep(100);
                            if (timeout >= 100) break;
                        }
                        this.objLog.writeDTEDBNET("RESPONSE: " + json);
                        this.objLog.writeDTEDBNET(jsonRespSEMPOS);
                        // PROCESO FACTURA ELEC
                        DocumentDbNet doc = new DocumentDbNet();
                        doc.documentId = new DocumentId();
                        doc.documentId.DocType = "33";
                        doc.documentId.IssueDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
                        //doc.documentId.ServiceIndicator = "3";
                        //doc.documentId.NetAmountIndicator = "2";
                        doc.documentId.DueDate = doc.documentId.IssueDate;
                        doc.documentId.PaymentMean = "1";
                        doc.documentId.DispatchType = "1";

                        doc.supplier = new Supplier();
                        doc.supplier.SupplierId = ParamValues.DTEDbNet_RutEmisor;
                        doc.supplier.SupplierName = ParamValues.DTEDbNet_RznSocEmi.Norm(100);
                        doc.supplier.SupplierFax = "";


                        doc.supplier.SupplierPhone = ParamValues.DTEDbNet_Telefono.Norm(20);
                        doc.supplier.SupplierActivity = ParamValues.DTEDbNet_Giro.Norm(80);
                        doc.supplier.SupplierActivityCode = ParamValues.DTEDbNet_ACTECO.Norm(6);
                        doc.supplier.SupplierAddress = ParamValues.DTEDbNet_Direccion.Norm(60);
                        doc.supplier.SupplierCity = null;// ParamValues.DTEDbNet_Ciudad.Norm(20);
                        doc.supplier.SupplierCitySubdivision = ParamValues.DTEDbNet_Comuna.Norm(20);




                        //RECEPTOR


                        string nombrecliente = jsoncustomer2.GetValue("first_name").ToString() + " " + jsoncustomer2.GetValue("last_name").ToString();
                        if ((nombrecliente.Trim() == "") || ((nombrecliente == " ")))
                        {
                            throw new ApplicationException("Cliente Requerido para Factura Electronica");
                        }

                        doc.customer = new Customer();
                        doc.customer.CustomerId = numdoccliente;


                        doc.customer.CustomerName = nombrecliente;
                        //Direcciones
                        if (!String.IsNullOrEmpty(jsoncustomer2.GetValue("primary_address_line_1").ToString()))
                        {
                            doc.customer.CustomerAddress = jsoncustomer2.GetValue("primary_address_line_1").ToString() + " " + jsoncustomer2.GetValue("primary_address_line_2").ToString();
                        }
                        if (!String.IsNullOrEmpty(jsoncustomer2.GetValue("primary_address_line_4").ToString()))
                        {
                            doc.customer.CustomerCitySubdivision = jsoncustomer2.GetValue("primary_address_line_4").ToString();
                        }

                        string giro = jsoncustomer2.GetValue("notes").ToString().Trim();
                        if (giro == "")
                        {
                            giro = this.ParamValues.DTEDbNet_GiroClientedefault;
                        }

                        doc.customer.CustomerActivity = giro;


                        //TOTALES 33
                        doc.legalMonetaryTotal = new LegalMonetaryTotal();
                        if (json2.GetValue("saletotalamt").ToString() == "0")
                        {
                            throw new ApplicationException("Total del Documento no puede ser cero");
                        }
                        else
                        {
                            int iva = 0;
                            int mntTotal = 0;
                            int mntNeto = 0;
                            this.objLog.writeDTEDBNET("Totales");
                            iva = json2["saletotaltaxamt"].Value<Int32>();//.["saletotaltaxamt").ToString());
                                                                          //total.TasaIVA = float.Parse(this.ParamValues.DTESignature_TasaIVA);
                            mntTotal = json2["saletotalamt"].Value<Int32>();// Int32.Parse(json2.GetValue("saletotalamt").ToString());
                            mntNeto = mntTotal - iva;
                            doc.legalMonetaryTotal.TaxableAmount = mntNeto.ToString();
                            doc.legalMonetaryTotal.VatPercent = this.ParamValues.DTESignature_TasaIVA;
                            doc.legalMonetaryTotal.ExemptAmount = "0";
                            doc.legalMonetaryTotal.VatAmount = iva.ToString();
                            doc.legalMonetaryTotal.TotalAmount = mntTotal.ToString();
                            //doc.legalMonetaryTotal.PayableAmount = mntTotal.ToString();
                        }


                        //doc.otherTaxes = new OtherTaxes();
                        //doc.otherTaxes.TaxAmount = "";
                        //doc.otherTaxes.TaxCode = "";
                        //doc.otherTaxes.TaxPercent = "";

                        //doc.transportation = new Transportation();
                        //doc.transportation.CarrierLegalId = "";
                        //doc.transportation.DestinationAddress = "";
                        //doc.transportation.DestinationCitySubdivision = "";
                        //doc.transportation.DriverLegalId = "";
                        //doc.transportation.DriverName = "";
                        //doc.transportation.LicensePlate = "";

                        //doc.references = new List<ReferenceList>();
                        //var reference = new ReferenceList();
                        //reference.ReferenceListLine = "1";
                        //reference.ReferenceDate = "";
                        //reference.ReferenceId = "";
                        //reference.ReferenceReason = "";
                        //reference.ReferenceType = "";

                        //doc.references.Add(reference);

                        //doc.paymentList = new List<PaymentList>();



                        // Creacion Detalle
                        string strjson3 = json2.GetValue("docitem").ToString();
                        JArray jsonArray = JArray.Parse(strjson3);
                        this.objLog.writeDTEDBNET("Detalle");
                        int contadordetalle = 0;
                        foreach (JObject jsonOperaciones in jsonArray.Children<JObject>())
                        {
                            if (contadordetalle < 61)
                            {
                                contadordetalle = contadordetalle + 1;
                                //string descuento = string.Empty;
                                Item det = new Item();
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
                                det.ItemLine = contadordetalle.ToString();// jsonOperaciones["itempos"].Value<Int32>().ToString();// Int32.Parse((jsonOperaciones["itempos"]).ToString());
                                det.ItemName = (jsonOperaciones["description1"]).ToString();
                                //det. = (jsonOperaciones["description1"]).ToString();
                                //string cantidad = (jsonOperaciones["qty"]).ToString();
                                //cantidad = cantidad.Replace(',', '.');
                                det.ItemMeasureUnit = "UND";
                                det.LineQuantity = QtyItem.ToString().Replace(',', '.');// float.Parse(cantidad);
                                det.ItemPrice = PrcItem.ToString().Replace(",", ".");// float.Parse((jsonOperaciones["origprice"]).ToString());
                                                                                     //DTEValidaciones(jsonOperaciones["origprice"].ToString(), "Precio Item", 19, 2);
                                                                                     //string iva = (jsonOperaciones["taxamt"]).ToString().Split(',')[0];
                                                                                     //string precio = (jsonOperaciones["price"]).ToString();
                                                                                     //float montoitem = (Int32.Parse(precio) * float.Parse(cantidad));
                                                                                     //det.MontoItem = Convert.ToInt32(montoitem);
                                det.LineAmount = Convert.ToInt32(QtyItem * PrcItem).ToString();

                                det.ItemCode = (jsonOperaciones["alu"]).ToString();
                                det.ItemCodeType = "ALU";

                                doc.Items.Add(det);



                            }
                            else
                            {
                                msg = "No es posible superar los 60 Items";
                                throw new ApplicationException(msg);
                            }
                        }


                        doc.additionalValues = new AdditionalValues();

                        doc.additionalValues.Val1 = "";
                        doc.additionalValues.Val2 = "";
                        doc.additionalValues.Val3 = "";
                        doc.additionalValues.Val4 = "";
                        doc.additionalValues.Val5 = "";
                        doc.additionalValues.Val6 = "";
                        doc.additionalValues.Val7 = "";
                        doc.additionalValues.Val8 = "";
                        doc.additionalValues.Val9 = "";

                        //agregar referencia de devolucion.

                        //Folio

                        //aca comunicamos con la DLL de DBNET y analisamos la respuesta.

                        jsonRespSEMPOS = "";
                        string jsonBase64 = "";
                        string error = "";
                        if (!doc.GetBase64(out jsonBase64, out error)) log.Error(error);
                        error = "";

                        this.objLog.writeDTEDBNET("SEMGenerateDocument: ");
                        GenerateDocument generateDocument = new GenerateDocument();

                        generateDocument.DocType = doc.documentId.DocType;
                        generateDocument.Internal_Id = "33-" + SidDocPRISM;
                        generateDocument.SupplierId = ParamValues.DTEDbNet_RutEmisor;
                        generateDocument.UserId = DTEDbNet_PosId;// "SILFA-POS1";
                        generateDocument.DocInfo = jsonBase64;


                        json = generateDocument.GetJson();
                        this.objLog.writeDTEDBNET("REQUEST: " + json);
                        SEMContext.Instance.SEMGenerateDocument(json, resp);
                        esperar = true;
                        timeout = 0;
                        while (esperar)
                        {
                            timeout++;
                            Thread.Sleep(100);
                            if (timeout >= 100) break;
                        }

                        this.objLog.writeDTEDBNET("Response: " + jsonRespSEMPOS);

                        //jsonRespSEMPOS = "{\"RetCode\":\"0\",\"RetMsge\":\"OK\",\"Internal_Id\":\"590050609000065277\",\"SupplierId\":\"76171658-1\",\"DocType\":\"39\",\"LegalNumber\":\"1241\",\"TimeStamp\":\"2021-02-26T18:10:34\",\"DocTEDQ\":\"PFRFRCB2ZXJzaW9uID0iMS4wIj48REQ+PFJFPjc2MTcxNjU4LTE8L1JFPjxURD4zOTwvVEQ+PEY+MTI0MTwvRj48RkU+MjAyMS0wMi0yNjwvRkU+PFJSPjY2NjY2NjY2LTY8L1JSPjxSU1I+Q0xJRU5URSBCT0xFVEE8L1JTUj48TU5UPjQ5OTA8L01OVD48SVQxPkFCU09SQkVOVEVTIEFSRE8gREFZICBOSUdIVCBQQTwvSVQxPjxDQUYgdmVyc2lvbj1cIjEuMFwiPjxEQT48UkU+NzYxNzE2NTgtMTwvUkU+PFJTPkNPTUVSQ0lBTCBFIElORFVTVFJJQUwgU0lMRkEgUy5BPC9SUz48VEQ+Mzk8L1REPjxSTkc+PEQ+MTI0MTwvRD48SD4yMjQwPC9IPjwvUk5HPjxGQT4yMDIxLTAyLTEyPC9GQT48UlNBUEs+PE0+c25EMmJKSUlmMkx0RWFPYkoxdzA0aExDZjFCSGdWWGptM2tZVTlFOThkMEY5VlRYNlBHTDlaQ2cvZ3ZFNGdRR2FGdDVwYzBxbTZVS1dLbW5QL0pFUVE9PTwvTT48RT5Bdz09PC9FPjwvUlNBUEs+PElESz4xMDA8L0lESz48L0RBPjxGUk1BIGFsZ29yaXRtbz1cIlNIQTF3aXRoUlNBXCI+bk1PazNkSnhEZ0hRUTArSEtVZGtWYWJEQjdobEtKemh0VWEzQVdPbTJXaHMxdUV1dzl6VE1uWUFlTWQ4cXJpWmN3R3g1NVFFWWdFNTZUeEoxaERLdVE9PTwvRlJNQT48L0NBRj48VFNURUQ+MjAyMS0wMi0yNlQxODoxMDozNDwvVFNURUQ+PC9ERD48RlJNVCBhbGdvcml0bW89IlNIQTF3aXRoUlNBIj5pUStzZTY1QlRMRGlzOVBKbjhoQjZock4vV0ZWeENBSnVYUk5SVlBFZHhuQTFqaGY3ZkdPenVXVWJ0cUZ0UlFIcStmbFQ4MnByQ0xHcmRaUTRXNkRHZz09PC9GUk1UPjwvVEVEPg==\"}";

                        if (!string.IsNullOrEmpty(jsonRespSEMPOS))
                        {
                            ResponseDbNet responseDbNet = JsonConvert.DeserializeObject<ResponseDbNet>(jsonRespSEMPOS);

                            if (responseDbNet.RetCode == "0")
                            {
                                status = "0";
                                folio = responseDbNet.LegalNumber;// folioSignature.ToString();
                                msg = "";
                                this.objLog.writeDTEDBNET(ted);
                                tedbase64 = responseDbNet.DocTEDQ;// EncodeStrToBase64(ted);

                                textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                jsonOut = string.Concat(textArrayResponse);

                            }
                            else
                            {
                                status = "500";
                                msg = responseDbNet.RetMsge;
                                textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                jsonOut = string.Concat(textArrayResponse);
                                this.objLog.writeDTEDBNET("Error From SEMPOS: ");
                            }

                            return;
                        }

                        status = "500";
                        msg = "ERROR SEMGenerateDocument retorno nulo o vacio:";
                        textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                        jsonOut = string.Concat(textArrayResponse);
                        this.objLog.writeDTEDBNET("ProcessEvent** Error: " + msg);

                    }
                    catch (Exception e)
                    {


                        status = "555";
                        this.objLog.writeDTEDBNET("EXCEPTION2");
                        jsonOut = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                        this.objLog.writeDTEDBNET("ProcessEvent *********** Error General -- " + e.Message);

                    }
                    return;
                }
                else if (posflag == "61")// NOTA CREDITO ELECTRONICA
                {
                    try
                    {
                        this.objLog.writeDTEDBNET("INICIO NOTA CREDITO ELECTRONICA");
                        this.objLog.writeDTEDBNET("SEMInitialize: ");
                        string json = "{\"StoreId\":\"" + DTEDbNet_StoreId + "\",\"PosId\":\"" + DTEDbNet_PosId + "\",\"StoreURL\":\"" + ParamValues.DTEDbNet_StoreURL + "\"}";
                        this.objLog.writeDTEDBNET("REQUEST: " + json);
                        ResponseCallback resp = new ResponseCallback(Response);
                        SEMContext.Instance.Initialize(json, resp);
                        int timeout = 0;
                        while (esperar)
                        {
                            timeout++;
                            Thread.Sleep(100);
                            if (timeout >= 100) break;
                        }
                        this.objLog.writeDTEDBNET("RESPONSE: " + json);
                        this.objLog.writeDTEDBNET(jsonRespSEMPOS);
                        // PROCESO FACTURA ELEC
                        DocumentDbNet doc = new DocumentDbNet();
                        doc.documentId = new DocumentId();
                        doc.documentId.DocType = "61";
                        doc.documentId.IssueDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
                        doc.documentId.DueDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
                        //doc.documentId.ServiceIndicator = "3";
                        //doc.documentId.NetAmountIndicator = "2";
                        doc.documentId.DueDate = doc.documentId.IssueDate;
                        doc.documentId.PaymentMean = "1";

                        doc.supplier = new Supplier();
                        doc.supplier.SupplierId = ParamValues.DTEDbNet_RutEmisor;
                        doc.supplier.SupplierName = ParamValues.DTEDbNet_RznSocEmi.Norm(100);
                        doc.supplier.SupplierFax = "";

                        //doc.supplier.SupplierPhone = ParamValues.DTEDbNet_Telefono.Norm(20);
                        doc.supplier.SupplierActivity = ParamValues.DTEDbNet_Giro.Norm(80);
                        doc.supplier.SupplierActivityCode = ParamValues.DTEDbNet_ACTECO.Norm(6);
                        doc.supplier.SupplierAddress = ParamValues.DTEDbNet_Direccion.Norm(60);
                        doc.supplier.SupplierCity = null;// ParamValues.DTEDbNet_Ciudad.Norm(20);
                        doc.supplier.SupplierCitySubdivision = ParamValues.DTEDbNet_Comuna.Norm(20);




                        //RECEPTOR


                        string nombrecliente = jsoncustomer2.GetValue("first_name").ToString() + " " + jsoncustomer2.GetValue("last_name").ToString();
                        if ((nombrecliente.Trim() == "") || ((nombrecliente == " ")))
                        {
                            throw new ApplicationException("Cliente Requerido para Factura Electronica");
                        }

                        doc.customer = new Customer();
                        doc.customer.CustomerId = numdoccliente;


                        doc.customer.CustomerName = nombrecliente;
                        //Direcciones
                        if (!String.IsNullOrEmpty(jsoncustomer2.GetValue("primary_address_line_1").ToString()))
                        {
                            doc.customer.CustomerAddress = jsoncustomer2.GetValue("primary_address_line_1").ToString() + " " + jsoncustomer2.GetValue("primary_address_line_2").ToString();
                        }
                        if (!String.IsNullOrEmpty(jsoncustomer2.GetValue("primary_address_line_4").ToString()))
                        {
                            doc.customer.CustomerCitySubdivision = jsoncustomer2.GetValue("primary_address_line_4").ToString();
                        }

                        string giro = jsoncustomer2.GetValue("notes").ToString().Trim();
                        if (giro == "")
                        {
                            giro = this.ParamValues.DTEDbNet_GiroClientedefault;
                        }

                        doc.customer.CustomerActivity = giro;


                        //TOTALES 61
                        doc.legalMonetaryTotal = new LegalMonetaryTotal();
                        if (json2.GetValue("returnsubtotalwithtax").ToString() == "0")
                        {
                            throw new ApplicationException("Total del Documento no puede ser cero");
                        }
                        else
                        {
                            int iva = 0;
                            int mntTotal = 0;
                            int mntNeto = 0;
                            this.objLog.writeDTEDBNET("Totales");
                            iva = json2["returntotaltaxamt"].Value<Int32>();
                            mntTotal = json2["returnsubtotalwithtax"].Value<Int32>();
                            mntNeto = mntTotal - iva;
                            doc.legalMonetaryTotal.TaxableAmount = mntNeto.ToString();
                            doc.legalMonetaryTotal.VatPercent = this.ParamValues.DTEDbNet_TasaIVA;
                            doc.legalMonetaryTotal.ExemptAmount = "0";
                            doc.legalMonetaryTotal.VatAmount = iva.ToString();
                            doc.legalMonetaryTotal.TotalAmount = mntTotal.ToString();
                            doc.legalMonetaryTotal.PayableAmount = null;
                        }


                        //doc.otherTaxes = new OtherTaxes();
                        //doc.otherTaxes.TaxAmount = "";
                        //doc.otherTaxes.TaxCode = "";
                        //doc.otherTaxes.TaxPercent = "";

                        //doc.transportation = new Transportation();
                        //doc.transportation.CarrierLegalId = "";
                        //doc.transportation.DestinationAddress = "";
                        //doc.transportation.DestinationCitySubdivision = "";
                        //doc.transportation.DriverLegalId = "";
                        //doc.transportation.DriverName = "";
                        //doc.transportation.LicensePlate = "";

                        doc.references = new List<ReferenceList>();
                        var reference = new ReferenceList();
                        reference.ReferenceListLine = "1";
                        DateTime fecharef = DateTime.Parse(jsonreferencia.GetValue("modifieddatetime").ToString());
                        string fecharefstr = String.Format("{0:yyyy-MM-dd}", fecharef);
                        reference.ReferenceDate = fecharefstr;
                        reference.ReferenceId = jsonreferencia.GetValue("docno").ToString();
                        reference.ReferenceCode = "1";
                        reference.ReferenceType = jsonreferencia.GetValue("posflag1").ToString().Trim().Substring(0, 2);

                        doc.references.Add(reference);

                        doc.paymentList = new List<PaymentList>();



                        // Creacion Detalle
                        string strjson3 = json2.GetValue("docitem").ToString();
                        JArray jsonArray = JArray.Parse(strjson3);
                        this.objLog.writeDTEDBNET("Detalle");
                        int contadordetalle = 0;
                        foreach (JObject jsonOperaciones in jsonArray.Children<JObject>())
                        {
                            if (contadordetalle < 61)
                            {
                                contadordetalle = contadordetalle + 1;
                                //string descuento = string.Empty;
                                Item det = new Item();
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
                                det.ItemLine = contadordetalle.ToString();//  jsonOperaciones["itempos"].Value<Int32>().ToString();// Int32.Parse((jsonOperaciones["itempos"]).ToString());
                                det.ItemName = (jsonOperaciones["description1"]).ToString();
                                //det. = (jsonOperaciones["description1"]).ToString();
                                //string cantidad = (jsonOperaciones["qty"]).ToString();
                                //cantidad = cantidad.Replace(',', '.');
                                det.ItemMeasureUnit = "UND";
                                det.LineQuantity = QtyItem.ToString().Replace(',', '.');// float.Parse(cantidad);
                                det.ItemPrice = PrcItem.ToString().Replace(",", ".");// float.Parse((jsonOperaciones["origprice"]).ToString());
                                                                                     //DTEValidaciones(jsonOperaciones["origprice"].ToString(), "Precio Item", 19, 2);
                                                                                     //string iva = (jsonOperaciones["taxamt"]).ToString().Split(',')[0];
                                                                                     //string precio = (jsonOperaciones["price"]).ToString();
                                                                                     //float montoitem = (Int32.Parse(precio) * float.Parse(cantidad));
                                                                                     //det.MontoItem = Convert.ToInt32(montoitem);
                                det.LineAmount = Convert.ToInt32(QtyItem * PrcItem).ToString();

                                det.ItemCode = (jsonOperaciones["alu"]).ToString();
                                det.ItemCodeType = "ALU";

                                doc.Items.Add(det);



                            }
                            else
                            {
                                msg = "No es posible superar los 60 Items";
                                throw new ApplicationException(msg);
                            }
                        }


                        doc.additionalValues = new AdditionalValues();

                        doc.additionalValues.Val1 = "";
                        doc.additionalValues.Val2 = "";
                        doc.additionalValues.Val3 = "";
                        doc.additionalValues.Val4 = "";
                        doc.additionalValues.Val5 = "";
                        doc.additionalValues.Val6 = "";
                        doc.additionalValues.Val7 = "";
                        doc.additionalValues.Val8 = "";
                        doc.additionalValues.Val9 = "";



                        //Folio

                        //aca comunicamos con la DLL de DBNET y analisamos la respuesta.

                        jsonRespSEMPOS = "";
                        string jsonBase64 = "";
                        string error = "";
                        if (doc.GetBase64(out jsonBase64, out error)) log.Error(error);
                        error = "";

                        this.objLog.writeDTEDBNET("SEMGenerateDocument: ");
                        GenerateDocument generateDocument = new GenerateDocument();

                        generateDocument.DocType = doc.documentId.DocType;
                        generateDocument.Internal_Id = "61-" + SidDocPRISM;
                        generateDocument.SupplierId = ParamValues.DTEDbNet_RutEmisor;
                        generateDocument.UserId = DTEDbNet_PosId;// "SILFA-POS1";
                        generateDocument.DocInfo = jsonBase64;


                        json = generateDocument.GetJson();
                        this.objLog.writeDTEDBNET("REQUEST: " + json);
                        SEMContext.Instance.SEMGenerateDocument(json, resp);
                        esperar = true;
                        timeout = 0;
                        while (esperar)
                        {
                            timeout++;
                            Thread.Sleep(100);
                            if (timeout >= 100) break;
                        }

                        this.objLog.writeDTEDBNET("Response: " + jsonRespSEMPOS);

                        //jsonRespSEMPOS = "{\"RetCode\":\"0\",\"RetMsge\":\"OK\",\"Internal_Id\":\"590050609000065277\",\"SupplierId\":\"76171658-1\",\"DocType\":\"39\",\"LegalNumber\":\"1241\",\"TimeStamp\":\"2021-02-26T18:10:34\",\"DocTEDQ\":\"PFRFRCB2ZXJzaW9uID0iMS4wIj48REQ+PFJFPjc2MTcxNjU4LTE8L1JFPjxURD4zOTwvVEQ+PEY+MTI0MTwvRj48RkU+MjAyMS0wMi0yNjwvRkU+PFJSPjY2NjY2NjY2LTY8L1JSPjxSU1I+Q0xJRU5URSBCT0xFVEE8L1JTUj48TU5UPjQ5OTA8L01OVD48SVQxPkFCU09SQkVOVEVTIEFSRE8gREFZICBOSUdIVCBQQTwvSVQxPjxDQUYgdmVyc2lvbj1cIjEuMFwiPjxEQT48UkU+NzYxNzE2NTgtMTwvUkU+PFJTPkNPTUVSQ0lBTCBFIElORFVTVFJJQUwgU0lMRkEgUy5BPC9SUz48VEQ+Mzk8L1REPjxSTkc+PEQ+MTI0MTwvRD48SD4yMjQwPC9IPjwvUk5HPjxGQT4yMDIxLTAyLTEyPC9GQT48UlNBUEs+PE0+c25EMmJKSUlmMkx0RWFPYkoxdzA0aExDZjFCSGdWWGptM2tZVTlFOThkMEY5VlRYNlBHTDlaQ2cvZ3ZFNGdRR2FGdDVwYzBxbTZVS1dLbW5QL0pFUVE9PTwvTT48RT5Bdz09PC9FPjwvUlNBUEs+PElESz4xMDA8L0lESz48L0RBPjxGUk1BIGFsZ29yaXRtbz1cIlNIQTF3aXRoUlNBXCI+bk1PazNkSnhEZ0hRUTArSEtVZGtWYWJEQjdobEtKemh0VWEzQVdPbTJXaHMxdUV1dzl6VE1uWUFlTWQ4cXJpWmN3R3g1NVFFWWdFNTZUeEoxaERLdVE9PTwvRlJNQT48L0NBRj48VFNURUQ+MjAyMS0wMi0yNlQxODoxMDozNDwvVFNURUQ+PC9ERD48RlJNVCBhbGdvcml0bW89IlNIQTF3aXRoUlNBIj5pUStzZTY1QlRMRGlzOVBKbjhoQjZock4vV0ZWeENBSnVYUk5SVlBFZHhuQTFqaGY3ZkdPenVXVWJ0cUZ0UlFIcStmbFQ4MnByQ0xHcmRaUTRXNkRHZz09PC9GUk1UPjwvVEVEPg==\"}";

                        if (!string.IsNullOrEmpty(jsonRespSEMPOS))
                        {
                            ResponseDbNet responseDbNet = JsonConvert.DeserializeObject<ResponseDbNet>(jsonRespSEMPOS);

                            if (responseDbNet.RetCode == "0")
                            {
                                status = "0";
                                folio = responseDbNet.LegalNumber;// folioSignature.ToString();
                                msg = "";
                                this.objLog.writeDTEDBNET(ted);
                                tedbase64 = responseDbNet.DocTEDQ;// EncodeStrToBase64(ted);

                                textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                jsonOut = string.Concat(textArrayResponse);

                            }
                            else
                            {
                                status = "500";
                                msg = responseDbNet.RetMsge;
                                textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                jsonOut = string.Concat(textArrayResponse);
                                this.objLog.writeDTEDBNET("Error From SEMPOS: ");
                            }

                            return;
                        }

                        status = "500";
                        msg = "ERROR SEMGenerateDocument retorno nulo o vacio:";
                        textArrayResponse = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                        jsonOut = string.Concat(textArrayResponse);
                        this.objLog.writeDTEDBNET("ProcessEvent** Error: " + msg);

                    }
                    catch (Exception e)
                    {


                        status = "555";
                        this.objLog.writeDTEDBNET("EXCEPTION2");
                        jsonOut = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                        this.objLog.writeDTEDBNET("ProcessEvent *********** Error General -- " + e.Message);

                    }
                    return;
                }


                this.objLog.writeDTEDBNET("FIN");





            }
            catch (Exception e)
            {
                status = "555";
                this.objLog.writeDTEDBNET("EXCEPTION2");
                jsonOut = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                this.objLog.writeDTEDBNET("ProcessEvent *********** Error General -- " + e.Message);
            }

        }


    }
}