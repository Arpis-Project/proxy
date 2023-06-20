namespace ProxyVyV.ProxyVyV
{
    using global::ProxyVyV.ProxyVyV.DTEFacCL;
    using global::ProxyVyV.ProxyVyV.FaceleDTE;
    using log4net;
    using log4net.Config;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using RestSharp;
    using SignatureVyV;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.Data;
    using System.Drawing;
    using System.IO;
    using System.IO.Ports;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Forms;
    using System.Xml;
    using Transbank.Onepay;
    using Transbank.Onepay.Enums;
    using Transbank.Onepay.Exceptions;
    using Transbank.Onepay.Model;
    using VyVDbNet;

    partial class Init : Form
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private CCustomizationInterface ProxyLink;
        private List<string> TokenData = new List<string>();
        private List<CCustMessage> Messages = new List<CCustMessage>();
        private string GlobalConfigData;
        private string WSConfigData;
        private ProxyParam ParamValues;
        private NotifyIcon NotifyIcon1 = new NotifyIcon();
        private ContextMenu ContextMenu1 = new ContextMenu();
        private readonly object TBLock = new object();
        public static string iniPath;
        public static DataSet dsTarjetas;
        public static DataTable dtTarjetas;
        public static DataSet dsTarjetasTB;
        public static DataTable dtTarjetasTB;
        public SerialPort comPort = new SerialPort();
        private StreamWriter SW = null;
        protected decimal fAmount;
        private string txtMonto;
        private string lblTipoPago;
        private string cbTipoVenta;
        private string lblModalidad;
        private string lblInv_Sid;
        public bool OkToClose = true;
        private string txtNumeroTarjeta;
        private string cbTipoTarjeta;
        private string txtCantidadCuotas;
        private string txtNumeroAutorizacion;
        private string txtMontoCuota;
        private string lblResultadoTarjeta;
        private string lblTender_type;
        private string lblTender_name;
        private string lblFolio = string.Empty;
        public string LogPathTB;
        private string fFileName;
        private string fFileExt;
        public string LogTransbank;
        private bool finalizationSupressed;
        private readonly Options op_options;
        private Log objLog;
        private bool PrimeraVez;
        private IContainer components;
        private TextBox txtMensaje;
        private PlantillaXml plantilla;

        #region Procesamiento Proxy
        public Init()
        {
            object[] objArray1 = new object[] { "Proxy_TB", DateTime.Now.Year, DateTime.Now.Month.ToString().PadRight(2, '0'), DateTime.Now.Day.ToString().PadRight(2, '0'), ".txt" };
            this.LogPathTB = string.Concat(objArray1);
            this.fFileName = "Transbank";
            this.fFileExt = ".LOG";
            this.LogTransbank = string.Empty;
            this.finalizationSupressed = false;
            Options options1 = new Options();
            options1.ApiKey = ConfigurationManager.AppSettings["apikey"];
            options1.SharedSecret = ConfigurationManager.AppSettings["secret"];
            this.op_options = options1;
            this.objLog = new Log();
            this.PrimeraVez = true;
            this.components = null;
            XmlConfigurator.Configure();
            this.InitializeComponent();
            this.ParamValues = new ProxyParam();
            this.ParamValues.LoadConfigurationSystem();
            if (!this.ParamValues.isValid)
            {
                this.txtMensaje.Text = this.txtMensaje.Text + "Error Leyendo la configuraci\x00f3n del proxy";
            }
            else
            {
                CCustomizationInterface interface1 = new CCustomizationInterface(this.ParamValues.PRISMServerIP, Convert.ToInt32(this.ParamValues.PRISMProxyPort));
                interface1.Name = this.ParamValues.PRISMProxyLinkName;
                interface1.Version = this.ParamValues.PRISMProxyVersion;
                interface1.DeveloperID = this.ParamValues.PRISMProxyDeveloperID;
                interface1.CustomizationID = this.ParamValues.PRISMProxyCustomizationID;
                this.ProxyLink = interface1;
                this.SetUp();
                TextBox txtMensaje = this.txtMensaje;
                string[] textArray1 = new string[] { txtMensaje.Text, "Configuraci\x00f3n realizada exitosamente en ", this.ParamValues.PRISMServerIP, "PINPAD BAUDRATE: ", ConfigurationManager.AppSettings["PINPADBaudRate"], "PINPAD PORT: COM", ConfigurationManager.AppSettings["PINPADCOMPorNumber"] };
                txtMensaje.Text = string.Concat(textArray1);
                this.objLog.write("--------------PROXY INICIADO----------------");
            }
        }

        private void ProcessEvent(string ADirection, ref int AStatusCode, string AHTTPVerb, string AResourceName, string AURL, ref string AHeaders, ref string AParams, ref string APayload, string AContentType, ref bool ACanContinue)
        {
            object tBLock = this.TBLock;
            lock (tBLock)
            {
                // this.objLog.write("    --------ProcessEvent ->iniciado\n\r");
                bool flag2 = ACanContinue;
                int num = AStatusCode;
                string str = APayload;
                string str2 = APayload;
                string token = string.Empty;
                string boletapdf = string.Empty;
                string docelectpdf = string.Empty;
                int contadordetalle = 0;
                try
                {
                    /*this.objLog.write("ADirection = " + ADirection);
                    this.objLog.write("AHTTPVerb = " + AHTTPVerb);
                    this.objLog.write("AResourceName = " + AResourceName.ToUpper());
                   */

                    /*           ESTANDAR PARA EL MAANEJO DE LAS DIRECCIONES DE CLIENTES EN DTE
                        
                        DIRECCION --> Address1
                        NUMERO --> Address2
                        DEPTO --> Address3
                        COMUNA -->  Address4
                        CIUDAD -->  Address5
                        REGION -->  Address6
                        CODIGO POSTAL --> postal_code
                     
                     */

                    /*           ESTANDAR PARA EL MAANEJO DE LAS DIRECCIONES DE TIENDAS EN DTE
                        
                        DTEVyV_Tienda = store.GetValue("storename").ToString();
                        DTEVyV_RutEmisor = store.GetValue("zip").ToString();
                        DTEVyV_RznSocEmi = store.GetValue("udf1string").ToString();
                        DTEVyV_Giro = store.GetValue("udf2string").ToString();
                        DTEVyV_Direccion_tienda = store.GetValue("address1").ToString();
                        DTEVyV_DirOrigen = store.GetValue("address4").ToString();
                        DTEVyV_CmnaOrigen = store.GetValue("address5").ToString();
                        DTEVyV_CiudadOrigen = store.GetValue("address6").ToString();
                        DTEVyV_Telefono = store.GetValue("phone2").ToString();
                        DTEVyV_Telefono_tienda = store.GetValue("phone1").ToString();
                     
                     */
                    //this.objLog.write("AResourceName   "+ AResourceName.ToUpper());
                    string str18;
                    string str19;
                    string str20;
                    string str21;
                    CCustMessage message = new CCustMessage(ADirection, AURL, AContentType, AHTTPVerb, AResourceName, AParams, APayload);
                    if (((ADirection == "FromClient") && (AHTTPVerb == "POST")) && (AResourceName.ToUpper() == "CALLTRANSBANKINTEGRADO"))
                    {
                        flag2 = false;
                        // this.objLog.write("*CALLTRANSBANKINTEGRADO* Buscar respuesta en archivo servidor ");
                        this.objLog.writeTB("*CALLTRANSBANKINTEGRADO PayLoad: " + message.Payload + " *** Params: " + message.QueryString);
                        char[] separator = new char[] { ',' };
                        string paramfAmount = string.Empty;
                        string paramTenderType = string.Empty;
                        string paramTenderName = string.Empty;
                        string strSID = string.Empty;
                        string[] strArray2 = message.QueryString.Split(separator);
                        int index = 0;
                        while (true)
                        {
                            if (index >= strArray2.Length)
                            {
                                string[] textArray1 = new string[] { "strAmount:", paramfAmount, "-strTenderType:", paramTenderType, "-strTenderName:", paramTenderName, "-strSID:", strSID };
                                this.objLog.writeTB(string.Concat(textArray1));
                                string str7 = string.Empty;
                                bool flag4 = true;
                                try
                                {
                                    str7 = this.processTransbank(strSID, paramfAmount, paramTenderType, paramTenderName);
                                }
                                catch (PinpadException exception)
                                {
                                    flag4 = false;
                                    this.objLog.writeTB("ProcessEvent || AResourceName:CALLTRANSBANKINTEGRADO  **********ERROR: " + exception.Message);
                                    string[] textArray2 = new string[] { "{\"code\":\"2\",\"strMessage\":\"", exception.Message, "\",\"exterrormsg\":\"", exception.Message, "\",\"errormsg\":\"", exception.Message, "\"}" };
                                    str = string.Concat(textArray2);
                                }
                                if (flag4)
                                {
                                    if (str7 != string.Empty)
                                    {
                                        this.objLog.writeTB("ProcessEvent || AResourceName:CALLTRANSBANKINTEGRADO -- TODO OK -->" + str7);
                                        str = str7;
                                    }
                                    else
                                    {
                                        this.objLog.writeTB("ProcessEvent || AResourceName:CALLTRANSBANKINTEGRADO -- Sin respuesta");
                                        str = "{\"code\":\"1\",\"strMessage\":\"Sin respuesta\",\"exterrormsg\":\"Sin respuesta\",\"errormsg\":\"Sin respuesta\"}";
                                    }
                                }
                                break;
                            }
                            string str8 = strArray2[index];
                            char[] chArray2 = new char[] { '=' };
                            string[] strArray3 = str8.Split(chArray2);
                            string str9 = strArray3[0];
                            if (str9 == "amount")
                            {
                                paramfAmount = strArray3[1];
                            }
                            else if (str9 == "tender_type")
                            {
                                paramTenderType = strArray3[1];
                            }
                            else if (str9 == "tender_name")
                            {
                                paramTenderName = strArray3[1];
                            }
                            else if (str9 == "SId")
                            {
                                strSID = strArray3[1];
                            }
                            index++;
                        }
                    }
                    if (((ADirection == "FromClient") && (AHTTPVerb == "POST")) && (AResourceName.ToUpper() == "CALLCOMANDOSADMINISTRATIVOS"))
                    {
                        flag2 = false;
                        this.objLog.writeTB("*CALLCOMANDOSADMINISTRATIVOS PayLoad: " + message.Payload + " *** Params: " + message.QueryString);
                        char[] separator = new char[] { '=' };
                        string[] parametros = message.QueryString.Split(separator);

                        if (parametros.Length == 2)
                        {
                            string comando = parametros[1];
                            bool respComando = this.ComandosAdministrativos(comando);

                            this.objLog.writeTB("respuesta del comando " + respComando);
                            if (respComando)
                            {
                                flag2 = false;
                                num = 200;
                                string[] textResp = new string[] { "{\"code\":\"0\",\"strMessage\":\"El comando se ha ejecutado exitosamente\" }" };
                                str = string.Concat(textResp);
                            }
                            else
                            {
                                flag2 = false;
                                num = 200;
                                string[] textArray4 = new string[] { "{\"code\":\"1\",\"strMessage\":\"Error al ejecutar el comando. Por favor verifique el archivo log para ver mas detalles.\"" };
                                str = string.Concat(textArray4);
                            }
                        }
                        else
                        {
                            flag2 = false;
                            num = 200;
                            string[] textArray4 = new string[] { "{\"code\":\"1\",\"strMessage\":\"No se han enviado comandos\"" };
                            str = string.Concat(textArray4);
                        }


                    }
                    if (((ADirection == "FromClient") && (AHTTPVerb == "POST")) && (AResourceName.ToUpper() == "CALLONEPAYREFUND"))
                    {
                        flag2 = false;
                        // this.objLog.write("*CALLONEPAYREFUND* Buscar respuesta en archivo servidor ");
                        this.objLog.writeTB("*CALLONEPAYREFUND PayLoad: " + message.Payload + " *** Params: " + message.QueryString);
                        char[] separator = new char[] { ',' };
                        string str10 = string.Empty;
                        string str11 = string.Empty;
                        string str12 = string.Empty;
                        string str13 = string.Empty;
                        string str14 = string.Empty;
                        FillOnePayParameters(message.QueryString.Split(separator), ref str10, ref str11, ref str12, ref str13, ref str14);
                        string[] textArray3 = new string[0x13];
                        textArray3[0] = "****INICIO TRANSACCION REFUND*****\n\r\tocc: ";
                        textArray3[1] = str11;
                        textArray3[2] = Environment.NewLine;
                        textArray3[3] = "\tamount:";
                        textArray3[4] = str10;
                        textArray3[5] = Environment.NewLine;
                        textArray3[6] = "\texternalUniqueNumber:";
                        textArray3[7] = str12;
                        textArray3[8] = Environment.NewLine;
                        textArray3[9] = "\tauthorizationCode:";
                        textArray3[10] = str13;
                        textArray3[11] = Environment.NewLine;
                        textArray3[12] = "\tAPIKEY:";
                        textArray3[13] = this.op_options.ApiKey;
                        textArray3[14] = Environment.NewLine;
                        textArray3[15] = "\tSHARED-SECRET:";
                        textArray3[0x10] = this.op_options.SharedSecret;
                        textArray3[0x11] = Environment.NewLine;
                        textArray3[0x12] = "\t";
                        // this.objLog.writeTB(string.Concat(textArray3));
                        string str15 = string.Empty;
                        bool flag8 = true;
                        try
                        {
                            str15 = this.processOnePay(str10, str11, str12, str13, str14);
                        }
                        catch (OnePayException exception2)
                        {
                            flag8 = false;
                            this.objLog.writeTB("****** " + exception2.Message);
                            string[] textArray4 = new string[] { "{\"code\":\"2\",\"strMessage\":\"", exception2.Message, "\",\"exterrormsg\":\"", exception2.Message, "\",\"errormsg\":\"", exception2.Message, "\"}" };
                            str = string.Concat(textArray4);
                        }
                        if (flag8)
                        {
                            if (str15 != string.Empty)
                            {
                                this.objLog.writeTB("TODO OK -->" + str15);
                                str = str15;
                            }
                            else
                            {
                                this.objLog.writeTB("Sin respuesta");
                                str = "{\"code\":\"1\",\"strMessage\":\"Sin respuesta\",\"exterrormsg\":\"Sin respuesta\",\"errormsg\":\"Sin respuesta\"}";
                            }
                        }
                    }
                    if (((ADirection == "FromClient") && (AHTTPVerb == "POST")) && (AResourceName.ToUpper() == "CALLDTEVYV"))
                    {
                        this.objLog.write("INICIO  CALLDTEVYV");
                        str18 = "";
                        str19 = ""; 
                        str20 = "";
                        str21 = "";
                        string validacion = string.Empty;
                        string errors = string.Empty;
                        string numdoccliente = string.Empty;
                        string folio = string.Empty;
                        string plant = string.Empty;
                        string plantdet = string.Empty;
                        string plantref = string.Empty;
                        string status = string.Empty;
                        string msg = string.Empty;
                        string posflagtipo = string.Empty;
                        string xmlres = string.Empty;
                        string ted = string.Empty;
                        string tedbase64 = string.Empty;
                        ImpresoraOPOS resImpresion = new ImpresoraOPOS();

                        foreach (JObject obj3 in JArray.Parse("[" + message.Payload + "]").Children<JObject>())
                        {
                            foreach (JProperty property in obj3.Properties())
                            {
                                if (property.Name == "documentID")
                                {
                                    token = this.doPRISMLoginAux(this.objLog);
                                    str21 = (string)property.Value;
                                    string objVenta = this.GETDocument(str21, token);
                                    this.objLog.writeDTEVyV("objeto venta "+ objVenta);
                                    JObject json = JObject.Parse(this.GETDocument(str21, token));
                                    errors = json.GetValue("errors").ToString();
                                    if (errors == "")
                                    {
                                        JObject json2 = JObject.Parse(json.GetValue("data")[0].ToString());

                                        /*        Obtencion de descunto por Transaccion      */
                                        string monto_descuento_global = json2.GetValue("discamt").ToString();
                                        string porc_descuento_global = json2.GetValue("discperc").ToString();

                                        this.objLog.writeDTEVyV("monto_descuento_global " + monto_descuento_global);
                                        this.objLog.writeDTEVyV("porc_descuento_global " + porc_descuento_global);
                                        /****************************************************/

                                        this.objLog.writeDTEVyV("leyendo parametros de tienda ");
                                        string idTienda = json2.GetValue("storeno").ToString();
                                        JObject jsonstore = JObject.Parse(this.GETStore(idTienda, token));
                                        JObject store = JObject.Parse(jsonstore.GetValue("data")[0].ToString());
                                        string DTEVyV_Tienda = store.GetValue("storename").ToString();
                                        string DTEVyV_RutEmisor = store.GetValue("zip").ToString().ToUpper();
                                        string DTEVyV_RznSocEmi = store.GetValue("udf1string").ToString();
                                        string DTEVyV_Giro = store.GetValue("udf2string").ToString();
                                        string DTEVyV_Direccion_tienda = store.GetValue("address1").ToString();
                                        string DTEVyV_DirOrigen = store.GetValue("address4").ToString();
                                        string DTEVyV_CmnaOrigen = store.GetValue("address5").ToString();
                                        string DTEVyV_CiudadOrigen = store.GetValue("address6").ToString();
                                        string DTEVyV_Telefono = store.GetValue("phone2").ToString();
                                        string DTEVyV_Telefono_tienda = store.GetValue("phone1").ToString();
                                        string DTEVyV_codigo_sii_sucursal = store.GetValue("udf4string").ToString();
                                        this.objLog.write("termine lectura de Variables de Tienda ");

                                        string posflag = json2.GetValue("posflag1").ToString().Trim().Substring(0, 2);
                                        string idcliente = json2.GetValue("btcuid").ToString();
                                        this.objLog.write("Buscar Cliente ");
                                        JArray jsoncustomerRest1 = JArray.Parse(this.GETCustomerRest(idcliente, token));
                                        JObject jsoncustomerRest = JObject.Parse(jsoncustomerRest1[0].ToString());
                                        this.objLog.write("pase conversion a json de cliente");
                                        // JObject jsoncustomer = JObject.Parse(this.GETCustomer(idcliente, token));
                                        // JObject jsoncustomer2 = JObject.Parse(jsoncustomer.GetValue("data")[0].ToString());
                                        
                                        numdoccliente = jsoncustomerRest.GetValue("info1").ToString().ToUpper();
                                        //this.objLog.write("genere el cliente");
                                        if (numdoccliente == "")
                                        {
                                            numdoccliente = jsoncustomerRest.GetValue("info2").ToString().ToUpper();
                                            if (numdoccliente == "")
                                            {
                                                numdoccliente = this.ParamValues.DTEVyV_RutReceptordefault.ToUpper();
                                            }
                                        }

                                        // this.objLog.write("posflag "+ posflag);
                                        if (posflag == "39")// BOLETA ELECTRONICA
                                        {
                                            try
                                            {
                                                this.objLog.writeDTEVyV("Procesando Boleta");
                                                #region PROCESO BOLETA ELEC
                                                bool aplicaDcto1Peso = false;
                                                plantilla = new PlantillaXml();
                                                plant = this.plantilla.PlantillaCabBoleta();
                                                #region Creacion de XML Cabecera
                                                //CARATULA
                                                plant = plant.Replace("#RUTEMISOR#", DTEVyV_RutEmisor);
                                                plant = plant.Replace("#RUTENVIA#", DTEVyV_RutEmisor);
                                                plant = plant.Replace("#RUTRECEPTOR#", numdoccliente);
                                                DTEValidaciones(numdoccliente, "Rut Receptor", 10, 1);
                                                this.objLog.write("Validando Rut Receptor: "+ numdoccliente);
                                                plant = plant.Replace("#FECHARESOL#", this.ParamValues.DTEVyV_FechaResol);
                                                plant = plant.Replace("#NUMRESOL#", this.ParamValues.DTEVyV_NumResol);
                                                plant = plant.Replace("#FIRMAEVN#", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Replace(" ", "T"));
                                                plant = plant.Replace("#TPODTE#", posflag);
                                                //DOCUMENTO
                                                plant = plant.Replace("#FECEMISION#", DateTime.Now.ToString("yyyy-MM-dd"));
                                                plant = plant.Replace("#INDSERVICIO#", "1");
                                                plant = plant.Replace("#FECVENC#", DateTime.Now.ToString("yyyy-MM-dd"));
                                                //EMISOR
                                                this.objLog.write("Antes validando Razon Social Emisor: " + DTEVyV_RznSocEmi);
                                                plant = plant.Replace("#RZNSOC#", DTEVyV_RznSocEmi);
                                                DTEValidaciones(DTEVyV_RznSocEmi, "Razon Social Emisor", 100, 1);
                                                this.objLog.write("Validando Razon Social Emisor: "+ DTEVyV_RznSocEmi);
                                                plant = plant.Replace("#GIRO#", LargoCadenaMax(DTEVyV_Giro,80));                                                
                                                DTEValidaciones(LargoCadenaMax(DTEVyV_Giro, 80), "Giro Emisor", 80, 1);
                                                this.objLog.write("Validando Giro Emisor: "+ DTEVyV_Giro);
                                                //plant = plant.Replace("#ACTECO#", this.ParamValues.DTEVyV_ACTECO);
                                                plant = plant.Replace("#CDGSIISUCUR#", DTEVyV_codigo_sii_sucursal);
                                                DTEValidaciones(DTEVyV_codigo_sii_sucursal, "Sucursal Emisor", 20, 1);
                                                this.objLog.write("Validando Sucursal Emisor: " + DTEVyV_codigo_sii_sucursal);
                                                plant = plant.Replace("#DIRORIGEN#", DTEVyV_DirOrigen);
                                                plant = plant.Replace("#CMNAORIGEN#", DTEVyV_CmnaOrigen);
                                                plant = plant.Replace("#CIUDADORIGEN#", DTEVyV_CiudadOrigen);
                                                //RECEPTOR
                                                string nombrecliente = jsoncustomerRest.GetValue("first_name").ToString() + " " + jsoncustomerRest.GetValue("last_name").ToString();
                                                if (nombrecliente == "")
                                                {
                                                    plant = plant.Replace("#RZNSOCRECEP#", this.ParamValues.DTEVyV_NombreClientedefault);
                                                    DTEValidaciones(this.ParamValues.DTEVyV_NombreClientedefault, "Razón Social Receptor", 100,1);
                                                }
                                                else
                                                {
                                                    plant = plant.Replace("#RZNSOCRECEP#", nombrecliente);
                                                    DTEValidaciones(nombrecliente, "Razon Social Receptor", 100, 2);
                                                }
                                                plant = plant.Replace("#DIRRECEP#", jsoncustomerRest.GetValue("primary_address_line_1").ToString() + " " + jsoncustomerRest.GetValue("primary_address_line_2").ToString());
                                                plant = plant.Replace("#CMNARECEP#", jsoncustomerRest.GetValue("primary_address_line_4").ToString());
                                                plant = plant.Replace("#CIUDADRECEP#", jsoncustomerRest.GetValue("primary_address_line_5").ToString());
                                                //TOTALES
                                                if (Convert.ToDecimal(json2.GetValue("saletotalamt").ToString()) - Convert.ToDecimal(json2.GetValue("returnsubtotal").ToString()) <= 1)
                                                {
                                                    aplicaDcto1Peso = true;
                                                    plant = plant.Replace("#MNTEXE#", "");
                                                    plant = plant.Replace("#IVA#", "");
                                                    plant = plant.Replace("#MNTTOTAL#", "1");
                                                    plant = plant.Replace("#MNTNETO#","1");
                                                }
                                                else
                                                {
                                                    decimal var_saletotaltaxamt = Convert.ToDecimal(json2.GetValue("saletotaltaxamt").ToString());
                                                    decimal var_returntax1amt = Convert.ToDecimal(json2.GetValue("returntax1amt").ToString());
                                                    decimal var_saletotalamt = Convert.ToDecimal(json2.GetValue("saletotalamt").ToString());
                                                    decimal var_returnsubtotal = Convert.ToDecimal(json2.GetValue("returnsubtotal").ToString());

                                                    plant = plant.Replace("#MNTEXE#", "<MntExe>0</MntExe>");
                                                    plant = plant.Replace("#IVA#", "<IVA>"+Decimal.Round(var_saletotaltaxamt - var_returntax1amt).ToString()+ "</IVA>");
                                                    DTEValidaciones(Decimal.Round(var_saletotaltaxamt - var_returntax1amt).ToString(), "IVA", 18, 1);
                                                    plant = plant.Replace("#MNTTOTAL#", Decimal.Round(var_saletotalamt - var_returnsubtotal).ToString());
                                                    DTEValidaciones(Decimal.Round(var_saletotalamt - var_returnsubtotal).ToString(), "Monto Total", 18, 1);
                                                    int neto = Int32.Parse(Decimal.Round(var_saletotalamt - var_returnsubtotal).ToString()) - Int32.Parse(Decimal.Round(var_saletotaltaxamt - var_returntax1amt).ToString());
                                                    plant = plant.Replace("#MNTNETO#", neto.ToString());
                                                    DTEValidaciones(neto.ToString(), "Monto Neto", 18, 1);
                                                }
                                                #endregion
                                                #region Creacion Xml Detalle
                                                string strjson3 = json2.GetValue("docitem").ToString();
                                                JArray jsonArray = JArray.Parse(strjson3);
                                                int contadorLinea = 0;
                                                foreach (JObject jsonOperaciones in jsonArray.Children<JObject>())
                                                {
                                                    if (contadordetalle < 61)
                                                    {
                                                        contadorLinea = contadorLinea + 1;
                                                        if ((jsonOperaciones["itemtype"]).ToString() != "2")
                                                        {
                                                            string kitflag = "0";
                                                            try
                                                            {
                                                                kitflag = (jsonOperaciones["kitflag"]).ToString();
                                                            }
                                                            catch (Exception e)
                                                            {
                                                                kitflag = "0";
                                                            }
                                                            if (!"5".Equals(kitflag))
                                                            {
                                                                string descuento = string.Empty;
                                                                plantilla = new PlantillaXml();
                                                                plantdet = plantdet + this.plantilla.PlantillaDetBoleta();
                                                                plantdet = plantdet.Replace("#NROLINDET#", contadorLinea.ToString());
                                                                plantdet = plantdet.Replace("#TPOCODIGO#", "INTERNO");
                                                                plantdet = plantdet.Replace("#VLRCODIGO#", (jsonOperaciones["alu"]).ToString());
                                                                DTEValidaciones(jsonOperaciones["alu"].ToString(), "Valor Código", 35, 1);


                                                                string descripcion_prod = jsonOperaciones["description1"].ToString();
                                                                this.objLog.writeDTEVyV("Producto " + descripcion_prod + " itemtype '" + (jsonOperaciones["itemtype"]).ToString() + "'");
                                                                if ((jsonOperaciones["itemtype"]).ToString() == "2")
                                                                {
                                                                    descripcion_prod = "Dev. " + descripcion_prod;
                                                                }

                                                                plantdet = plantdet.Replace("#NMBITEM#", descripcion_prod);
                                                                DTEValidaciones(descripcion_prod, "Nombre Item", 70, 1);
                                                                plantdet = plantdet.Replace("#DESC#", descripcion_prod);
                                                                string cantidad = (jsonOperaciones["qty"]).ToString();
                                                                cantidad = cantidad.Replace(',', '.');
                                                                plantdet = plantdet.Replace("#QTYITEM#", cantidad);
                                                                plantdet = plantdet.Replace("#UNMDITEM#", "Un.");
                                                                string iva = (jsonOperaciones["taxamt"]).ToString();
                                                                string precio_original = Decimal.Round(Convert.ToDecimal((jsonOperaciones["origprice"]).ToString())).ToString();
                                                                string precio = Decimal.Round(Convert.ToDecimal((jsonOperaciones["price"]).ToString())).ToString();
                                                                //int montoitem = ((Int32.Parse(precio) * Int32.Parse(cantidad)) - (Int32.Parse(iva) * Int32.Parse(cantidad)));
                                                                string cantidad_decimal = cantidad.Replace(".", ",");
                                                                double montoitem = (Convert.ToDouble(precio_original) * Convert.ToDouble(cantidad_decimal));
                                                               /* if (!(String.IsNullOrEmpty(monto_descuento_global) || monto_descuento_global == "" || monto_descuento_global == "0"))
                                                                {                                                                    
                                                                    precio = Math.Round(Convert.ToDouble(precio_original)  - Convert.ToDouble(precio_original) * (Convert.ToDouble(porc_descuento_global) / 100)).ToString();
                                                                    montoitem = (Convert.ToDouble(precio) * Convert.ToDouble(cantidad_decimal));

                                                                }
                                                               */

                                                                plantdet = plantdet.Replace("#PRCITEM#", precio_original);
                                                                DTEValidaciones(Decimal.Round(Convert.ToDecimal((precio_original).ToString())).ToString(), "Precio Item", 19, 2);

                                                                try
                                                                {
                                                                    descuento = Decimal.Round(Convert.ToDecimal((jsonOperaciones["discamt"]).ToString())).ToString();

                                                                    if (descuento != string.Empty && descuento != "0")
                                                                    {
                                                                        decimal descuento_decimal = Convert.ToDecimal(descuento);
                                                                        if (!(String.IsNullOrEmpty(monto_descuento_global) || monto_descuento_global == "" || monto_descuento_global == "0"))
                                                                        {
                                                                            descuento_decimal = Math.Round(descuento_decimal + (descuento_decimal * (Convert.ToDecimal(porc_descuento_global) / 100)));
                                                                        }

                                                                        if (descuento_decimal < 0)
                                                                        {
                                                                            descuento_decimal = descuento_decimal * -1;
                                                                            descuento = descuento_decimal.ToString();
                                                                        }
                                                                        plantdet = plantdet.Replace("#MONTOITEM#", Decimal.Round(Convert.ToDecimal(montoitem.ToString()) - Convert.ToDecimal(descuento)).ToString());
                                                                        descuento = "<DescuentoMonto>" + descuento + "</DescuentoMonto></Detalle>";
                                                                        plantdet = plantdet.Replace("#DESCUENTO#", descuento);
                                                                    }
                                                                    else
                                                                    {
                                                                        plantdet = plantdet.Replace("#MONTOITEM#", Decimal.Round(Convert.ToDecimal(montoitem.ToString())).ToString());
                                                                        descuento = "</Detalle>";
                                                                        plantdet = plantdet.Replace("#DESCUENTO#", descuento);
                                                                    }

                                                                }
                                                                catch
                                                                {
                                                                    descuento = "</Detalle>";
                                                                    plantdet = plantdet.Replace("#DESCUENTO#", descuento);
                                                                }
                                                                contadordetalle = contadordetalle + 1;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        msg = "No es posible superar los 60 Items";
                                                        throw new ApplicationException(msg);
                                                    }
                                                }
                                                plant = plant.Replace("#DETALLE#", plantdet);
                                                //agregar referencia de cliente cuando tenga correo.
                                                this.objLog.writeDTEVyV("Validando si corresponde llenar el segmento DscRcgGlobal" );

                                                if(Convert.ToDecimal(json2.GetValue("returnsubtotal").ToString()) > 0)
                                                {
                                                    this.objLog.writeDTEVyV("Entre a returnsubtotal");
                                                    decimal descuentoFinal = Convert.ToDecimal(json2.GetValue("returnsubtotal").ToString());
                                                    if (aplicaDcto1Peso)
                                                    {
                                                        descuentoFinal = descuentoFinal - 1;
                                                    }

                                                    plant = plant.Replace("#DCT_GLOBAL#", "<DscRcgGlobal>"+
                                                                                          "<NroLinDR>1</NroLinDR>"+
                                                                                          "<TpoMov>D</TpoMov>"+
                                                                                          "<GlosaDR>descuento devolucion</GlosaDR>"+
                                                                                          "<TpoValor>$</TpoValor>"+
                                                                                          "<ValorDR>"+ descuentoFinal + "</ValorDR>"+
                                                                                          "</DscRcgGlobal> ");
                                                }
                                                else
                                                {
                                                    this.objLog.writeDTEVyV("Entre a doumento sin devolucion");
                                                    if (!(String.IsNullOrEmpty(monto_descuento_global) || monto_descuento_global == "" || monto_descuento_global == "0"))
                                                    {
                                                        this.objLog.writeDTEVyV("Entre a doumento con DscRcgGlobal");
                                                        plant = plant.Replace("#DCT_GLOBAL#", "<DscRcgGlobal>" +
                                                                                          "<NroLinDR>1</NroLinDR>" +
                                                                                          "<TpoMov>D</TpoMov>" +
                                                                                          "<GlosaDR>descuento transaccion</GlosaDR>" +
                                                                                          "<TpoValor>$</TpoValor>" +
                                                                                          "<ValorDR>" + monto_descuento_global + "</ValorDR>" +
                                                                                          "</DscRcgGlobal> ");

                                                    }
                                                    else
                                                    {
                                                        this.objLog.writeDTEVyV("Entre a doumento sin DscRcgGlobal");
                                                        plant = plant.Replace("#DCT_GLOBAL#", "");
                                                    }
                                                    
                                                }

                                                #endregion
                                                #region Folio
                                                this.objLog.writeDTEVyV("Solicitando Folio  " + DTEVyV_RutEmisor + "tipo_doc "+ posflag);
                                                folio = this.DTESolicitarFolio(DTEVyV_RutEmisor, posflag);
                                                this.objLog.writeDTEVyV("Folio Obtenido  " + folio);
                                                if (folio == "0" )
                                                {
                                                    //this.objLog.write(folio);
                                                    flag2 = false;
                                                    num = 200;
                                                    status = "999";
                                                    msg = "No existen folios disponibles";
                                                    string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                    str = string.Concat(textArray5);
                                                    this.objLog.write("ProcessEvent** Error: " + msg);
                                                }
                                                else
                                                {
                                                    plant = plant.Replace("#DOCUMENTOID#", "R" + DTEVyV_RutEmisor + "T" + posflag + "F" + folio);
                                                    plant = plant.Replace("#FOLIO#", folio);
                                                    plant = plant.Replace("#NRODTE#", folio);
                                                }

                                                this.objLog.writeDTEVyV("Plantilla xml  " + plant);
                                                #endregion
                                                if (folio != "0")
                                                {
                                                    //this.objLog.writeDTEVyV("Enviando XML al DTE");
                                                    boletapdf = this.DTESolicitaBoleta(plant, ref status, ref msg, ref xmlres);
                                                    this.objLog.writeDTEVyV("status DTE " + status);
                                                    this.objLog.writeDTEVyV("respuesta DTE "+ msg);

                                                    if (status == "0" || status == "4")
                                                    {
                                                        flag2 = false;
                                                        num = 200;
                                                        this.objLog.writeDTEVyV(xmlres);
                                                        ted = retornaTED(xmlres);
                                                        tedbase64 = EncodeStrToBase64(ted);
                                                        if(status == "4")
                                                        {
                                                            status = "0";
                                                            msg = "Error al generar el PDF";
                                                        }
                                                        


                                                        string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                        str = string.Concat(textArray5);
                                                    }
                                                    else
                                                    {
                                                        flag2 = false;
                                                        num = 200;
                                                        string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                        str = string.Concat(textArray5);
                                                        this.objLog.write("ProcessEvent** Error: " + "Estatus fuera de rango" + msg);
                                                    }
                                                }
                                                #endregion
                                            }
                                            catch (Exception e)
                                            {
                                                flag2 = false;
                                                num = 200;
                                                status = "555";
                                                this.objLog.write("EXCEPTION2");
                                                str = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                                                this.objLog.write("ProcessEvent *********** Error General -- " + e.Message);
                                                this.txtMensaje.Text = this.txtMensaje.Text + "\nExcepcion: " + e.Message;
                                            }
                                        }
                                        else if (posflag == "33")// FACTURA ELECTRONICA
                                            {
                                            //posflagtipo = "FACTURA ELECTRONICA";},
                                            this.objLog.writeDTEVyV("Procesando Factura");
                                            try
                                                {
                                                    #region PROCESO FACTURA ELECTRONICA
                                                    plantilla = new PlantillaXml();
                                                    plant = this.plantilla.PantillaCabFactura();
                                                    #region Creacion de XML Cabecera
                                                    //this.objLog.write("llegue al documento");
                                                    //DOCUMENTO
                                                    //plant = plant.Replace("#DOCUMENTOID#", "R" + this.ParamValues.DTEVyV_RutEmisor + "T" + posflag + "F" + folio);
                                                    plant = plant.Replace("#TPODTE#", posflag);
                                                    //plant = plant.Replace("#FOLIO#", folio);
                                                    plant = plant.Replace("#FECEMISION#", DateTime.Now.ToString("yyyy-MM-dd"));
                                                    plant = plant.Replace("#FECVENC#", DateTime.Now.ToString("yyyy-MM-dd"));
                                                    //this.objLog.write("llegue al emisor");
                                                    //EMISOR
                                                    plant = plant.Replace("#RUTEMISOR#", DTEVyV_RutEmisor);
                                                    plant = plant.Replace("#RZNSOC#", DTEVyV_RznSocEmi);
                                                    DTEValidaciones(DTEVyV_RznSocEmi, "Razon Social Emisor", 100, 1);
                                                    plant = plant.Replace("#GIRO#", LargoCadenaMax(DTEVyV_Giro, 80));
                                                    DTEValidaciones(LargoCadenaMax(DTEVyV_Giro, 80), "Giro Emisor", 80, 1);
                                                    plant = plant.Replace("#ACTECO#", this.ParamValues.DTEVyV_ACTECO);
                                                    plant = plant.Replace("#CDGSIISUCUR#", DTEVyV_codigo_sii_sucursal);
                                                    DTEValidaciones(DTEVyV_codigo_sii_sucursal, "Sucursal Emisor", 20, 1);
                                                    plant = plant.Replace("#DIRORIGEN#", DTEVyV_DirOrigen);
                                                    plant = plant.Replace("#CMNAORIGEN#", DTEVyV_CmnaOrigen);
                                                    plant = plant.Replace("#CIUDADORIGEN#", DTEVyV_CiudadOrigen);
                                                    this.objLog.write("llegue al receptor");
                                                    //RECEPTOR
                                                
                                                    plant = plant.Replace("#RUTRECEPTOR#", numdoccliente);
                                                    DTEValidaciones(numdoccliente, "Rut Receptor", 10, 1);
                                                    string nombrecliente = jsoncustomerRest.GetValue("first_name").ToString() + " " + jsoncustomerRest.GetValue("last_name").ToString();
                                                    if (nombrecliente == "")
                                                    {
                                                        plant = plant.Replace("#RZNSOCRECEP#", this.ParamValues.DTEVyV_NombreClientedefault);
                                                        DTEValidaciones(this.ParamValues.DTEVyV_NombreClientedefault, "Razon Social Receptor", 100, 1);
                                                    }
                                                    else
                                                    {
                                                        plant = plant.Replace("#RZNSOCRECEP#", nombrecliente);
                                                        DTEValidaciones(nombrecliente, "Razon Social Receptor",100, 2);
                                                    }
                                                    //this.objLog.write("busco el giro");
                                                    //this.objLog.write(jsoncustomer2.ToString());
                                                    string giro = jsoncustomerRest.GetValue("notes").ToString();
                                                    if (giro == "")
                                                    {
                                                        //this.objLog.write("entre en 1");
                                                        plant = plant.Replace("#GIRORECEP#", LargoCadenaMax(this.ParamValues.DTEVyV_GiroClientedefault,40));
                                                        DTEValidaciones(LargoCadenaMax(this.ParamValues.DTEVyV_GiroClientedefault, 40), "Giro Receptor", 40, 1);
                                                    }
                                                    else
                                                    {
                                                        //this.objLog.write("entre en 2");
                                                        plant = plant.Replace("#GIRORECEP#", LargoCadenaMax(giro, 40));
                                                        DTEValidaciones(LargoCadenaMax(giro, 40), "Giro Receptor", 40, 2);
                                                    }
                                                    this.objLog.write("sali del giro");
                                                    this.objLog.write("DIRRECEP: " + jsoncustomerRest.GetValue("primary_address_line_1"));
                                                    plant = plant.Replace("#DIRRECEP#", jsoncustomerRest.GetValue("primary_address_line_1").ToString() + " " + jsoncustomerRest.GetValue("primary_address_line_2").ToString());
                                                    DTEValidaciones(jsoncustomerRest.GetValue("primary_address_line_1").ToString() + " " + jsoncustomerRest.GetValue("primary_address_line_2").ToString(), "Dirección Receptor", 70, 2);
                                                   
                                                    plant = plant.Replace("#CMNARECEP#", jsoncustomerRest.GetValue("primary_address_line_4").ToString());
                                                    this.objLog.write("CMNARECEP: " + jsoncustomerRest.GetValue("primary_address_line_4"));
                                                    DTEValidaciones(jsoncustomerRest.GetValue("primary_address_line_4").ToString(), "Comuna Receptor", 20, 1);
                                                    
                                                    plant = plant.Replace("#CIUDADRECEP#", jsoncustomerRest.GetValue("primary_address_line_5").ToString());
                                                    this.objLog.write("CIUDADRECEP: " + jsoncustomerRest.GetValue("primary_address_line_5"));
                                                    DTEValidaciones(jsoncustomerRest.GetValue("primary_address_line_5").ToString(), "Ciudad Receptor", 20, 1);
                                                    this.objLog.write("llegue a los totales");
                                                    //TOTALES
                                                   //el monto neto se calculara en base a la sumatario de cada Item
                                                   // int neto = Int32.Parse(json2.GetValue("saletotalamt").ToString()) - Int32.Parse(json2.GetValue("saletotaltaxamt").ToString());
                                                   // plant = plant.Replace("#MNTNETO#", neto.ToString());
                                                  // DTEValidaciones(neto.ToString(), "Monto Neto", 18, 3);
                                                    plant = plant.Replace("#MNTEXE#", "0");
                                                    plant = plant.Replace("#TASAIVA#", this.ParamValues.DTEVyV_TasaIVA);
                                                    DTEValidaciones(this.ParamValues.DTEVyV_TasaIVA, "Tasa IVA", 6, 3);
                                                    plant = plant.Replace("#IVA#", json2.GetValue("saletotaltaxamt").ToString());
                                                    DTEValidaciones(json2.GetValue("saletotaltaxamt").ToString(), "IVA", 18, 3);
                                                    plant = plant.Replace("#MNTTOTAL#", json2.GetValue("saletotalamt").ToString());
                                                    DTEValidaciones(json2.GetValue("saletotalamt").ToString(), "Monto Total", 18, 3);
                                                    //this.objLog.write("sali del total");
                                                    #endregion
                                                    #region Creacion Xml Detalle
                                                    string strjson3 = json2.GetValue("docitem").ToString();
                                                    JArray jsonArray = JArray.Parse(strjson3);
                                                    //this.objLog.write("entrare al detalle");
                                                    Decimal monto_neto_suma_detalle = 0;
                                                    foreach (JObject jsonOperaciones in jsonArray.Children<JObject>())
                                                    {
                                                        if (contadordetalle < 41)
                                                        {
                                                            string kitflag = "0";
                                                            try
                                                            {
                                                                kitflag = (jsonOperaciones["kitflag"]).ToString();
                                                            }
                                                            catch (Exception e)
                                                            {
                                                                kitflag = "0";
                                                            }
                                                             if (!"5".Equals(kitflag))
                                                            {
                                                                string descuento = string.Empty;
                                                                plantilla = new PlantillaXml();
                                                                plantdet = plantdet + this.plantilla.PantillaDetFactura();
                                                                plantdet = plantdet.Replace("#NROLINDET#", (jsonOperaciones["itempos"]).ToString());
                                                                plantdet = plantdet.Replace("#TPOCODIGO#", "INTERNO");
                                                                plantdet = plantdet.Replace("#VLRCODIGO#", (jsonOperaciones["alu"]).ToString());
                                                                DTEValidaciones(jsonOperaciones["alu"].ToString(), "Código Item", 35, 1);
                                                                plantdet = plantdet.Replace("#NMBITEM#", (jsonOperaciones["description1"]).ToString());
                                                                DTEValidaciones(jsonOperaciones["description1"].ToString(), "Nombre Item", 70, 1);
                                                                plantdet = plantdet.Replace("#DESC#", (jsonOperaciones["description1"]).ToString());
                                                                string cantidad = (jsonOperaciones["qty"]).ToString();
                                                                cantidad = cantidad.Replace(',', '.');
                                                                plantdet = plantdet.Replace("#QTYITEM#", cantidad);
                                                                string iva = (jsonOperaciones["taxamt"]).ToString();
                                                                string precio = (jsonOperaciones["price"]).ToString();
                                                                string precioori = (jsonOperaciones["origprice"]).ToString();
                                                                Double preciosiniva = Math.Round(Convert.ToDouble(precioori) - Convert.ToDouble(iva));
                                                                string cantidad_decimal = cantidad.Replace(".", ",");
                                                                double montoitem = ((Int32.Parse(precio) * Convert.ToDouble(cantidad_decimal)) - (Convert.ToDouble(iva) * Convert.ToDouble(cantidad_decimal)));

                                                                if (!(String.IsNullOrEmpty(monto_descuento_global) || monto_descuento_global == "" || monto_descuento_global == "0"))
                                                                {

                                                                    preciosiniva = Math.Round(preciosiniva - (preciosiniva * (Convert.ToDouble(porc_descuento_global) / 100)));
                                                                    montoitem = Math.Round((preciosiniva * Convert.ToDouble(cantidad_decimal)));
                                                                }


                                                                
                                                                monto_neto_suma_detalle = monto_neto_suma_detalle + Decimal.Round(Convert.ToDecimal(montoitem.ToString()));

                                                                plantdet = plantdet.Replace("#MONTOITEM#", Decimal.Round(Convert.ToDecimal(montoitem.ToString())).ToString());
                                                                plantdet = plantdet.Replace("#PRCITEM#", preciosiniva.ToString());
                                                                DTEValidaciones(preciosiniva.ToString(), "Precio Item", 19, 2);

                                                                this.objLog.write("CALCULO 5 --> " + preciosiniva);


                                                                 try
                                                                {
                                                                    descuento = (jsonOperaciones["discamt"]).ToString();
                                                                    if (descuento != string.Empty && descuento != "0")
                                                                    {
                                                                        decimal descuento_decimal = Convert.ToDecimal(descuento);
                                                                        if (!(String.IsNullOrEmpty(monto_descuento_global) || monto_descuento_global == "" || monto_descuento_global == "0"))
                                                                        {
                                                                         descuento_decimal = Math.Round(descuento_decimal + (descuento_decimal * Convert.ToDecimal(porc_descuento_global)));
                                                                        }

                                                                        if (descuento_decimal < 0)
                                                                        {
                                                                            descuento_decimal = descuento_decimal * -1;
                                                                            descuento = descuento_decimal.ToString();
                                                                        }

                                                                        int desc = Int32.Parse(descuento) - ((Int32.Parse(descuento) * Int32.Parse(this.ParamValues.DTEVyV_TasaIVA)) / 100);
                                                                        descuento = "<DescuentoMonto>" + desc.ToString() + "</DescuentoMonto></Detalle>";
                                                                        plantdet = plantdet.Replace("#DESCUENTO#", descuento);
                                                                    }
                                                                    else
                                                                    {
                                                                        descuento = "</Detalle>";
                                                                        plantdet = plantdet.Replace("#DESCUENTO#", descuento);
                                                                    }

                                                                }
                                                                catch
                                                                {
                                                                    descuento = "</Detalle>";
                                                                    plantdet = plantdet.Replace("#DESCUENTO#", descuento);
                                                                }
                                                                 contadordetalle = contadordetalle + 1;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            msg = "No es posible superar los 40 Items";
                                                            throw new ApplicationException(msg);
                                                        }
                                                    }

                                                    int neto = Int32.Parse(monto_neto_suma_detalle.ToString());
                                                    plant = plant.Replace("#MNTNETO#", neto.ToString());
                                                    DTEValidaciones(neto.ToString(), "Monto Neto", 18, 3);

                                                plant = plant.Replace("#DETALLE#", plantdet);
                                                    #endregion,
                                                    #region Creacion Xml Referencia
                                                    //string strjson4 = json2.GetValue("docitem").ToString(); //cambiar x referencia
                                                    //JArray jsonArray1 = JArray.Parse(strjson4);
                                                    //foreach (JObject jsonOperaciones in jsonArray1.Children<JObject>())
                                                    //{
                                                    plantilla = new PlantillaXml();
                                                    plantref = string.Empty;
                                                    string email = json2.GetValue("btemail").ToString();
                                                    if (string.IsNullOrEmpty(email))
                                                    {
                                                        email = "1";
                                                    }
                                                    plantref = plantref + this.plantilla.PlantillaRefFactura();
                                                    plantref = plantref.Replace("#NROLINREF#", "1");
                                                    plantref = plantref.Replace("#TPODOCREF#", "MTO");
                                                    //  plantref = plantref.Replace("#FOLIOREF#", folio); el folio se reemplaza mas abajo
                                                    plantref = plantref.Replace("#FCHREF#", DateTime.Now.ToString("yyyy-MM-dd"));
                                                    plantref = plantref.Replace("#RAZONREF#", email);
                                                    //DTEValidaciones(json2.GetValue("btemail").ToString(), "Razon Referencia", 90, 2);
                                                    //}
                                                plant = plant.Replace("#REFERENCIA#", plantref);
                                                #endregion
                                                #region Folio

                                               // this.objLog.writeDTEVyV("Solicitando Folio");
                                                folio = this.DTESolicitarFolio(DTEVyV_RutEmisor, posflag);
                                                this.objLog.writeDTEVyV("Solicitando Obtenido "+ folio);
                                                if (folio == "0")
                                                {
                                                    //this.objLog.write(folio);
                                                    flag2 = false;
                                                    num = 200;
                                                    status = "999";
                                                    msg = "No existen folios disponibles";
                                                    string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                    str = string.Concat(textArray5);
                                                    this.objLog.write("ProcessEvent** Error: " + msg);
                                                }
                                                else
                                                {
                                                    plant = plant.Replace("#DOCUMENTOID#", "R" + DTEVyV_RutEmisor + "T" + posflag + "F" + folio);
                                                    plant = plant.Replace("#FOLIO#", folio);
                                                    plant = plant.Replace("#FOLIOREF#", folio);
                                                }
                                                this.objLog.writeDTEVyV("Plantilla xml  " + plant);
                                                #endregion
                                                if (folio != "0")
                                                {
                                                    //this.objLog.writeDTEVyV("Enviando XML al DTE");
                                                    docelectpdf = this.DTESolicitaDocElectronico(plant, ref status, ref msg, ref xmlres);
                                                    this.objLog.writeDTEVyV("status DTE " + status);
                                                    this.objLog.writeDTEVyV("respuesta DTE " + msg);
                                                    if (status == "0" || status == "4")
                                                    {
                                                        this.objLog.writeDTEVyV(xmlres);
                                                        ted = retornaTED(xmlres);
                                                        tedbase64 = EncodeStrToBase64(ted);
                                                        flag2 = false;
                                                        num = 200;
                                                        if (status == "4")
                                                        {
                                                            status = "0";
                                                            msg = "Error al generar el PDF";
                                                        }
                                                        string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                        str = string.Concat(textArray5);
                                                    }
                                                    else
                                                    {
                                                        flag2 = false;
                                                        num = 200;
                                                        string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                        str = string.Concat(textArray5);
                                                        this.objLog.write("ProcessEvent** Error: " + "Estatus fuera de rango" + msg);
                                                    }
                                                }
                                                    #endregion
                                                }
                                                catch (Exception e)
                                                {
                                                    flag2 = false;
                                                    num = 200;
                                                    status = "555";
                                                    this.objLog.write("EXCEPTION2");
                                                    str = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                                                    this.objLog.write("ProcessEvent *********** Error General -- " + e.Message);
                                                    this.txtMensaje.Text = this.txtMensaje.Text + "\nExcepcion: " + e.Message;
                                                }
                                            }
                                        else if (posflag == "61")// NOTA DE CREDITO ELECTRONICA
                                            {
                                            //posflagtipo = "NOTA DE CREDITO ELECTRONICA";
                                            this.objLog.writeDTEVyV("Procesando Nota de Credito");
                                            try
                                                {
                                                    #region PROCESO NOTA DE CREDITO ELECTRONICA
                                                    plantilla = new PlantillaXml();
                                                    plant = this.plantilla.PlantillaCabNotaCredito();
                                                    #region Creacion de XML Cabecera
                                                    //DOCUMENTO
                                                    plant = plant.Replace("#TPODTE#", posflag);
                                                    plant = plant.Replace("#FECEMISION#", DateTime.Now.ToString("yyyy-MM-dd"));
                                                    plant = plant.Replace("#FECVENC#", DateTime.Now.ToString("yyyy-MM-dd"));
                                                this.objLog.writeDTEVyV("Procesando Nota de Credito 0.1"); 
                                                //EMISOR
                                                plant = plant.Replace("#RUTEMISOR#", DTEVyV_RutEmisor);
                                                    plant = plant.Replace("#RZNSOC#", DTEVyV_RznSocEmi);
                                                this.objLog.writeDTEVyV("Procesando Nota de Credito 0.2");
                                                DTEValidaciones(DTEVyV_RznSocEmi, "Razon Social Emisor", 100, 1);
                                                    plant = plant.Replace("#GIRO#", LargoCadenaMax(DTEVyV_Giro, 80));
                                                this.objLog.writeDTEVyV("Procesando Nota de Credito 0.3");
                                                DTEValidaciones(LargoCadenaMax(DTEVyV_Giro, 80), "Giro Emisor", 80, 1);
                                                    plant = plant.Replace("#ACTECO#", this.ParamValues.DTEVyV_ACTECO);
                                                    plant = plant.Replace("#CDGSIISUCUR#", DTEVyV_codigo_sii_sucursal);
                                                    DTEValidaciones(DTEVyV_codigo_sii_sucursal, "Sucursal Emisor", 20, 1);
                                                    plant = plant.Replace("#DIRORIGEN#", DTEVyV_DirOrigen);
                                                this.objLog.writeDTEVyV("Procesando Nota de Credito 0.4");
                                                plant = plant.Replace("#CMNAORIGEN#", DTEVyV_CmnaOrigen);
                                                    plant = plant.Replace("#CIUDADORIGEN#", DTEVyV_CiudadOrigen);
                                                    this.objLog.writeDTEVyV("Procesando Nota de Credito 2");
                                                //RECEPTOR
                                                plant = plant.Replace("#RUTRECEPTOR#", numdoccliente);
                                                    DTEValidaciones(numdoccliente, "RUT Receptor", 10, 1);
                                                    string nombrecliente = jsoncustomerRest.GetValue("first_name").ToString() + " " + jsoncustomerRest.GetValue("last_name").ToString();
                                                    if (nombrecliente == "")
                                                    {
                                                        plant = plant.Replace("#RZNSOCRECEP#", this.ParamValues.DTEVyV_NombreClientedefault);
                                                        DTEValidaciones(this.ParamValues.DTEVyV_NombreClientedefault, "Razon Social Receptor", 100, 1);
                                                    }
                                                    else
                                                    {
                                                        plant = plant.Replace("#RZNSOCRECEP#", nombrecliente);
                                                        DTEValidaciones(nombrecliente, "Razon Social Receptor", 100, 2);
                                                    }
                                                    string giro = jsoncustomerRest.GetValue("notes").ToString();
                                                    if (giro == "")
                                                    {
                                                        plant = plant.Replace("#GIRORECEP#", LargoCadenaMax(this.ParamValues.DTEVyV_GiroClientedefault, 40));
                                                        DTEValidaciones(LargoCadenaMax(this.ParamValues.DTEVyV_GiroClientedefault, 40), "Giro Receptor", 40, 1);
                                                    }
                                                    else
                                                    {
                                                        plant = plant.Replace("#GIRORECEP#", LargoCadenaMax(giro, 40));
                                                        DTEValidaciones(LargoCadenaMax(giro, 40), "Giro Receptor", 40, 2);
                                                    }
                                                    plant = plant.Replace("#DIRRECEP#", jsoncustomerRest.GetValue("primary_address_line_1").ToString() + " " + jsoncustomerRest.GetValue("primary_address_line_2").ToString());
                                                    DTEValidaciones(jsoncustomerRest.GetValue("primary_address_line_1").ToString() + " " + jsoncustomerRest.GetValue("primary_address_line_2").ToString(), "Ciudad Receptor", 70, 2);
                                                    plant = plant.Replace("#CMNARECEP#", jsoncustomerRest.GetValue("primary_address_line_4").ToString());
                                                    DTEValidaciones(jsoncustomerRest.GetValue("primary_address_line_4").ToString(), "Comuna Receptor", 20, 1);
                                                    plant = plant.Replace("#CIUDADRECEP#", jsoncustomerRest.GetValue("primary_address_line_5").ToString());
                                                    DTEValidaciones(jsoncustomerRest.GetValue("primary_address_line_5").ToString(), "Ciudad Receptor", 20, 1);
                                                //plant = plant.Replace("#DirPostal#", "");
                                                //plant = plant.Replace("#CmnaPostal#", "");
                                                //plant = plant.Replace("#CiudadPostal#", "");
                                                //plant = plant.Replace("#DirDest#", "");
                                                //plant = plant.Replace("#CmnaDest#", "");
                                                //plant = plant.Replace("#CiudadDest#", "");
                                                //TOTALES
                                                //int neto = Int32.Parse(json2.GetValue("returnsubtotalwithtax").ToString()) - Int32.Parse(json2.GetValue("returntotaltaxamt").ToString());
                                                //plant = plant.Replace("#MNTNETO#", neto.ToString());
                                                //DTEValidaciones(neto.ToString(), "Monto Neto", 18, 3);
                                                plant = plant.Replace("#MNTEXE#", "0");
                                                plant = plant.Replace("#TASAIVA#", this.ParamValues.DTEVyV_TasaIVA);
                                                DTEValidaciones(this.ParamValues.DTEVyV_TasaIVA, "Tasa IVA", 6, 3);
                                                plant = plant.Replace("#IVA#", json2.GetValue("returntotaltaxamt").ToString());
                                                DTEValidaciones(json2.GetValue("returntotaltaxamt").ToString(), "IVA", 18, 3);
                                                plant = plant.Replace("#MNTTOTAL#", json2.GetValue("returnsubtotalwithtax").ToString());
                                                DTEValidaciones(json2.GetValue("returnsubtotalwithtax").ToString(), "Monto Total", 18, 3);

                                                #endregion
                                                #region Creacion Xml Detalle
                                                this.objLog.writeDTEVyV("Procesando Nota de Credito 4");
                                                string strjson3 = json2.GetValue("docitem").ToString();
                                                    JArray jsonArray = JArray.Parse(strjson3);
                                                    Decimal monto_neto_suma_detalle = 0;
                                                    int contadorLinea = 0;
                                                    foreach (JObject jsonOperaciones in jsonArray.Children<JObject>())
                                                    {
                                                    this.objLog.writeDTEVyV(strjson3);
                                                    if (contadordetalle < 41)
                                                        {
                                                        contadorLinea = contadorLinea + 1;
                                                        this.objLog.writeDTEVyV("Procesando Nota de Credito cont"+ contadordetalle);
                                                        string kitflag = "0";
                                                            try
                                                            {
                                                                kitflag = (jsonOperaciones["kitflag"]).ToString();
                                                            }
                                                            catch (Exception e)
                                                            {
                                                                kitflag = "0";
                                                            }
                                                        this.objLog.writeDTEVyV("Rev 1");
                                                        if (!"5".Equals(kitflag))
                                                            {
                                                            this.objLog.writeDTEVyV("Rev 1.1");
                                                            string descuento = string.Empty;
                                                                plantilla = new PlantillaXml();
                                                                plantdet = plantdet + this.plantilla.PantillaDetNotaCredito();
                                                                plantdet = plantdet.Replace("#NROLINDET#", contadorLinea.ToString());
                                                                plantdet = plantdet.Replace("#TPOCODIGO#", "INTERNO");
                                                                plantdet = plantdet.Replace("#VLRCODIGO#", (jsonOperaciones["alu"]).ToString());
                                                                DTEValidaciones(jsonOperaciones["alu"].ToString(), "Código Item", 35, 1);
                                                                this.objLog.writeDTEVyV("Rev 1.2");
                                                                 plantdet = plantdet.Replace("#NMBITEM#", (jsonOperaciones["description1"]).ToString());
                                                                DTEValidaciones(jsonOperaciones["description1"].ToString(), "Nombre Item", 70, 1);
                                                                plantdet = plantdet.Replace("#DESC#", (jsonOperaciones["description1"]).ToString());
                                                                
                                                                string cantidad = (jsonOperaciones["qty"]).ToString();
                                                                cantidad = cantidad.Replace(',', '.');
                                                                plantdet = plantdet.Replace("#QTYITEM#", cantidad);
                                                                string iva = (jsonOperaciones["taxamt"]).ToString();
                                                                string precio_original = (jsonOperaciones["origprice"]).ToString();
                                                                string precio = (jsonOperaciones["price"]).ToString();
                                                                string cantidad_decimal = cantidad.Replace(".", ",");
                                                                iva = iva.Replace(".", ",");
                                                            
                                                                this.objLog.writeDTEVyV("Rev 1.3 " + precio_original + "   " + precio + "   "+ cantidad_decimal+ "       "+ iva+ "     "+ cantidad_decimal);
                                                                double montoitem = ((Convert.ToDouble(precio) * Convert.ToDouble(cantidad_decimal)) - (Convert.ToDouble(iva) * Convert.ToDouble(cantidad_decimal)));
                                                                this.objLog.writeDTEVyV("Rev 1.3.1");
                                                                string monto_item_calculado = Math.Round(Convert.ToDecimal(montoitem.ToString())).ToString();
                                                                plantdet = plantdet.Replace("#MONTOITEM#", monto_item_calculado);
                                                                this.objLog.writeDTEVyV("Rev 1.3.2 --> monto_item = "+ monto_item_calculado);
                                                                monto_neto_suma_detalle = monto_neto_suma_detalle + Decimal.Round(Convert.ToDecimal(monto_item_calculado.ToString()));

                                                                this.objLog.writeDTEVyV("Rev 1.3.3");
                                                                string precioori = (jsonOperaciones["price"]).ToString();
                                                                this.objLog.writeDTEVyV("Rev 1.3.4");
                                                                decimal preciosiniva = (Convert.ToDecimal(precioori) - Convert.ToDecimal(iva));
                                                                this.objLog.writeDTEVyV("Rev 1.3.5");
                                                                plantdet = plantdet.Replace("#PRCITEM#", Decimal.Round(preciosiniva).ToString());
                                                                this.objLog.writeDTEVyV("Rev 1.3.6");
                                                                DTEValidaciones(preciosiniva.ToString(), "Precio Item", 19, 2);
                                                                this.objLog.writeDTEVyV("Procesando Nota de Credito 5" );
                                                            try
                                                                {
                                                                    descuento = (jsonOperaciones["discamt"]).ToString();
                                                                    /*No considerar descuento en las Notas de Credito*/
                                                                    descuento = "";
                                                                    if (descuento != string.Empty && descuento != "0")
                                                                    {
                                                                        decimal descuento_decimal = Convert.ToDecimal(descuento);
                                                                        if (descuento_decimal < 0)
                                                                        {
                                                                            descuento_decimal = descuento_decimal * -1;
                                                                            descuento = descuento_decimal.ToString();
                                                                        }
                                                                        descuento = "<DescuentoMonto>" + descuento + "</DescuentoMonto></Detalle>";
                                                                        plantdet = plantdet.Replace("#DESCUENTO#", descuento);
                                                                    }
                                                                    else
                                                                    {
                                                                        descuento = "</Detalle>";
                                                                        plantdet = plantdet.Replace("#DESCUENTO#", descuento);
                                                                    }

                                                                }
                                                                catch
                                                                {
                                                                    descuento = "</Detalle>";
                                                                    plantdet = plantdet.Replace("#DESCUENTO#", descuento);
                                                                }
                                                                contadordetalle = contadordetalle + 1;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            msg = "No es posible superar los 40 Items";
                                                            throw new ApplicationException(msg);
                                                        }
                                                    }
                                                    this.objLog.writeDTEVyV("Procesando Nota de Credito 6");
                                                    this.objLog.writeDTEVyV("6.1.- monto_neto_suma_detalle  "+ monto_neto_suma_detalle);
                                                    decimal neto = Decimal.Round(Convert.ToDecimal(monto_neto_suma_detalle.ToString()));
                                                    this.objLog.writeDTEVyV("6.2.- neto  " + neto);
                                                    plant = plant.Replace("#MNTNETO#", neto.ToString());
                                                    this.objLog.writeDTEVyV("6.3.-plantilla  " + plant);
                                                    plant = plant.Replace("#DETALLE#", plantdet);
                                                #endregion,
                                                #region Creacion Xml Referencia

                                                    this.objLog.writeDTEVyV("6.4.-refsalesid  " + json2.GetValue("refsalesid").ToString());
                                                    String idreferencia = json2.GetValue("refsalesid").ToString();
                                                    this.objLog.writeDTEVyV("6.5.-idreferencia  " + json2.GetValue("refsalesid").ToString());
                                                    JObject jsonref;
                                                    JObject jsonreferencia;
                                                    string posflagref;
                                                   /*Primero Buscaremos la referencia en la BBDD local*/
                                                   jsonref = JObject.Parse(this.GETDocument(idreferencia, token));
                                                   this.objLog.writeDTEVyV("6.6.-jsonref local  " + jsonref);
                                                    try
                                                    {
                                                        jsonreferencia = JObject.Parse(jsonref.GetValue("data")[0].ToString());
                                                        posflagref = jsonreferencia.GetValue("posflag1").ToString().Trim().Substring(0, 2);
                                                    }
                                                    catch(Exception e)
                                                    {
                                                        this.objLog.writeDTEVyV("Documento en referencia no se encuentra en BBDD Local se buscara en Central  ");
                                                        string tokenCentral = this.doPRISMLoginCentral(this.objLog);
                                                        this.objLog.writeDTEVyV("6.5.1.-tokenCentral  " + tokenCentral);
                                                        jsonref = JObject.Parse(this.GETDocumentCentral(idreferencia, tokenCentral));
                                                        this.objLog.writeDTEVyV("6.6.-jsonref central  " + jsonref);
                                                        jsonreferencia = JObject.Parse(jsonref.GetValue("data")[0].ToString());
                                                        posflagref = jsonreferencia.GetValue("posflag1").ToString().Trim().Substring(0, 2);
                                                    }

                                                    this.objLog.writeDTEVyV("Procesando Nota de Credito 7");
                                                    plantilla = new PlantillaXml();
                                                    plantref = plantref + this.plantilla.PlantillaRefNotaCredito();
                                                    plantref = plantref.Replace("#NROLINREF#", "1");
                                                    plantref = plantref.Replace("#TPODOCREF#", posflagref);
                                                    plantref = plantref.Replace("#FOLIOREF#", jsonreferencia.GetValue("docno").ToString());
                                                    DateTime fecharef = DateTime.Parse(jsonreferencia.GetValue("modifieddatetime").ToString());
                                                    string fecharefstr = String.Format("{0:yyyy-MM-dd}", fecharef);
                                                    plantref = plantref.Replace("#FCHREF#", fecharefstr);
                                                    plantref = plantref.Replace("#CODREF#", "1");
                                                this.objLog.writeDTEVyV("Procesando Nota de Credito 7");
                                                plant = plant.Replace("#REFERENCIA#", plantref);
                                                #endregion
                                                #region Folio
                                                //this.objLog.writeDTEVyV("Solicitando Folio");
                                                folio = this.DTESolicitarFolio(DTEVyV_RutEmisor, posflag);
                                                this.objLog.writeDTEVyV("Folio Obtenido " + folio);
                                                this.objLog.writeDTEVyV("Procesando Nota de Credito 8");
                                                if (folio == "0")
                                                    {
                                                        //this.objLog.write(folio);
                                                        flag2 = false;
                                                        num = 200;
                                                        status = "999";
                                                        msg = "No existen folios disponibles";
                                                        string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                        str = string.Concat(textArray5);
                                                        this.objLog.write("ProcessEvent** Error: " + msg);
                                                    }
                                                    else
                                                    {
                                                        plant = plant.Replace("#DOCUMENTOID#", "R" + DTEVyV_RutEmisor + "T" + posflag + "F" + folio);
                                                        plant = plant.Replace("#FOLIO#", folio);
                                                    }
                                                this.objLog.writeDTEVyV("Plantilla xml  " + plant);
                                                #endregion
                                                if (folio != "0")
                                                    {
                                                   // this.objLog.writeDTEVyV("Enviando XML al DTE");
                                                    docelectpdf = this.DTESolicitaDocElectronico(plant, ref status, ref msg, ref xmlres);
                                                    this.objLog.writeDTEVyV("status DTE " + status);
                                                    this.objLog.writeDTEVyV("respuesta DTE " + msg);

                                                    if (status == "0" || status == "4")
                                                    {
                                                        flag2 = false;
                                                        num = 200;
                                                        this.objLog.writeDTEVyV(xmlres);
                                                        ted = retornaTED(xmlres);
                                                        tedbase64 = EncodeStrToBase64(ted);
                                                        if (status == "4")
                                                        {
                                                            status = "0";
                                                            msg = "Error al generar el PDF";
                                                        }
                                                        string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                            str = string.Concat(textArray5);
                                                        }
                                                        else
                                                        {
                                                            flag2 = false;
                                                            num = 200;
                                                            string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                            str = string.Concat(textArray5);
                                                            this.objLog.write("ProcessEvent** Error: " + "Estatus fuera de rango" + msg);
                                                        }
                                                    }
                                                    #endregion
                                                }
                                                catch (Exception e)
                                                {
                                                    flag2 = false;
                                                    num = 200;
                                                    status = "555";
                                                    this.objLog.write("EXCEPTION2");
                                                    str = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                                                    this.objLog.write("ProcessEvent *********** Error General -- " + e.Message);
                                                    this.txtMensaje.Text = this.txtMensaje.Text + "\nExcepcion: " + e.Message;
                                                }
                                            }
                                    }
                                    else
                                    {
                                        flag2 = false;
                                        num = 200;
                                        status = "556";
                                        string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + errors + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                        str = string.Concat(textArray5);
                                        this.objLog.write("ProcessEvent** Error: " + errors);
                                    }
                                }
                            }
                        }
                        //this.objLog.write("SALIMOS");
                    }
                    if (((ADirection == "FromClient") && (AHTTPVerb == "POST")) && (AResourceName.ToUpper() == "CALLDTEVYVTRANSFER"))
                    {
                        str18 = "";
                        str19 = "";
                        str20 = "";
                        str21 = "";
                        string errors = string.Empty;
                        string folio = string.Empty;
                        string plant = string.Empty;
                        string plantdet = string.Empty;
                        string status = string.Empty;
                        string msg = string.Empty;
                        string xmlres = string.Empty;
                        string ted = string.Empty;
                        string tedbase64 = string.Empty;

                       // this.objLog.write("ENTRAMOS");
                        foreach (JObject obj3 in JArray.Parse("[" + message.Payload + "]").Children<JObject>())
                        {
                            foreach (JProperty property in obj3.Properties())
                            {
                                if (property.Name == "documentID")
                                {
                                    token = this.doPRISMLoginAux(this.objLog);
                                    str21 = (string)property.Value;
                                    JObject json = JObject.Parse(this.GETTransfer(str21, token));
                                    errors = json.GetValue("errors").ToString();
                                    if (errors == "")
                                    {
                                        
                                        JObject json2 = JObject.Parse(json.GetValue("data")[0].ToString());
                                        string idTienda = json2.GetValue("outstoreno").ToString();
                                        JObject jsonstore = JObject.Parse(this.GETStore(idTienda, token));
                                        JObject store = JObject.Parse(jsonstore.GetValue("data")[0].ToString());
                                        string DTEVyV_Tienda = store.GetValue("storename").ToString();
                                        string DTEVyV_RutEmisor = store.GetValue("zip").ToString().ToUpper();
                                        string DTEVyV_RznSocEmi = store.GetValue("udf1string").ToString();
                                        string DTEVyV_Giro = store.GetValue("udf2string").ToString();
                                        string DTEVyV_Direccion_tienda = store.GetValue("address1").ToString();
                                        string DTEVyV_DirOrigen = store.GetValue("address4").ToString();
                                        string DTEVyV_CmnaOrigen = store.GetValue("address5").ToString();
                                        string DTEVyV_CiudadOrigen = store.GetValue("address6").ToString();
                                        string DTEVyV_Telefono = store.GetValue("phone2").ToString();
                                        string DTEVyV_Telefono_tienda = store.GetValue("phone1").ToString();


                                        string posflag = "52";
                                        try
                                        {
                                            this.objLog.writeDTEVyV("Guia de Despacho - Transferencias");
                                            plantilla = new PlantillaXml();
                                            plant = this.plantilla.PlantillaCabGuiaDespacho();
                                            #region Creacion de XML Cabecera
                                            //DOCUMENTO
                                            plant = plant.Replace("#TPODTE#", posflag);
                                            plant = plant.Replace("#FECEMISION#", DateTime.Now.ToString("yyyy-MM-dd"));
                                            plant = plant.Replace("#INDTRASLADO#", "5");
                                            plant = plant.Replace("#MNTBRUTO#", "1");
                                            plant = plant.Replace("#FECVENC#", DateTime.Now.ToString("yyyy-MM-dd"));
                                            //EMISOR
                                            plant = plant.Replace("#RUTEMISOR#", json2.GetValue("origstorezip").ToString().ToUpper());
                                            plant = plant.Replace("#RZNSOC#", json2.GetValue("origstoreudf1string").ToString());
                                            DTEValidaciones(json2.GetValue("origstoreudf1string").ToString(), "Razon Social Emisor", 100, 1);
                                            plant = plant.Replace("#GIRO#", LargoCadenaMax((json2.GetValue("origstoreudf2string").ToString() + " " + json2.GetValue("origstoreudf3string").ToString()).Trim(),80) );
                                            DTEValidaciones(LargoCadenaMax((json2.GetValue("origstoreudf2string").ToString() + " " + json2.GetValue("origstoreudf3string").ToString()).Trim(), 80), "Giro Emisor", 80, 1);
                                            plant = plant.Replace("#ACTECO#", this.ParamValues.DTEVyV_ACTECO);
                                            plant = plant.Replace("#CDGSIISUCUR#", json2.GetValue("origstoreudf4string").ToString());
                                            DTEValidaciones(json2.GetValue("origstoreudf4string").ToString(), "Sucursal Emisor", 20, 1);  
                                            plant = plant.Replace("#DIRORIGEN#", json2.GetValue("origstoreaddress1").ToString());
                                            plant = plant.Replace("#CMNAORIGEN#", json2.GetValue("origstoreaddress2").ToString());
                                            plant = plant.Replace("#CIUDADORIGEN#", json2.GetValue("origstoreaddress3").ToString());
                                            //RECEPTOR
                                            plant = plant.Replace("#RUTRECEPTOR#", json2.GetValue("origstorezip").ToString().ToUpper());
                                            DTEValidaciones(json2.GetValue("origstorezip").ToString(), "Rut Receptor", 10, 1);
                                            plant = plant.Replace("#RZNSOCRECEP#", json2.GetValue("instoreudf1string").ToString());
                                            DTEValidaciones(json2.GetValue("instoreudf1string").ToString(), "Razon Social Receptor", 100, 1);
                                            plant = plant.Replace("#GIRORECEP#", LargoCadenaMax((json2.GetValue("instoreudf2string").ToString() + " " + json2.GetValue("instoreudf3string").ToString()).Trim(),40)     );
                                            DTEValidaciones(LargoCadenaMax((json2.GetValue("instoreudf2string").ToString() + " " + json2.GetValue("instoreudf3string").ToString()).Trim(),40)   , "Giro Receptor", 40, 1);
                                            plant = plant.Replace("#DIRRECEP#", json2.GetValue("instoreaddress1").ToString());
                                            DTEValidaciones(json2.GetValue("instoreaddress1").ToString(), "Dirección Receptor", 70, 2);
                                            plant = plant.Replace("#CMNARECEP#", json2.GetValue("instoreaddress2").ToString());
                                            DTEValidaciones(json2.GetValue("instoreaddress2").ToString(), "Comuna Receptor", 20, 1);
                                            plant = plant.Replace("#CIUDADRECEP#", json2.GetValue("instoreaddress3").ToString());
                                            DTEValidaciones(json2.GetValue("instoreaddress3").ToString(), "Ciudad Receptor", 20, 1);
                                            //plant = plant.Replace("#DirPostal#", "");
                                            //plant = plant.Replace("#CmnaPostal#", "");
                                            //plant = plant.Replace("#CiudadPostal#", "");
                                            plant = plant.Replace("#DIRDEST#", json2.GetValue("instoreaddress1").ToString());
                                            plant = plant.Replace("#CMNADEST#", json2.GetValue("instoreaddress2").ToString());
                                            plant = plant.Replace("#CIUDADDEST#", json2.GetValue("instoreaddress3").ToString());
                                            //TOTALES
                                                
                                            int iva = (Int32.Parse(json2.GetValue("docpricetotal").ToString()) * Int32.Parse(this.ParamValues.DTEVyV_TasaIVA)) / 100 ;
                                            int neto = Int32.Parse(json2.GetValue("docpricetotal").ToString()) - iva;
                                            plant = plant.Replace("#MNTNETO#", neto.ToString());
                                            DTEValidaciones(neto.ToString(), "Monto Neto", 18, 3);
                                            plant = plant.Replace("#MNTEXE#", "0");
                                            plant = plant.Replace("#TASAIVA#", this.ParamValues.DTEVyV_TasaIVA);
                                            DTEValidaciones(this.ParamValues.DTEVyV_TasaIVA, "Tasa IVA", 6, 3);
                                            plant = plant.Replace("#IVA#", iva.ToString());
                                            DTEValidaciones(iva.ToString(), "Tasa IVA", 18, 3);
                                            plant = plant.Replace("#MNTTOTAL#", json2.GetValue("docpricetotal").ToString());
                                            DTEValidaciones(json2.GetValue("docpricetotal").ToString(), "Monto Total", 18, 3);
                                            #endregion
                                            #region Creacion Xml Detalle
                                            string jsonitem = json2.GetValue("slipitem").ToString();
                                            JArray jsonArrayitem = JArray.Parse(jsonitem);
                                            foreach (JObject jsonOperaciones in jsonArrayitem.Children<JObject>())
                                            {
                                                if (contadordetalle < 41)
                                                {
                                                    string kitflag = "0";
                                                    try
                                                    {
                                                        kitflag = (jsonOperaciones["kitflag"]).ToString();
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        kitflag = "0";
                                                    }

                                                    if (!"5".Equals(kitflag))
                                                    {
                                                        plantilla = new PlantillaXml();
                                                        plantdet = plantdet + this.plantilla.PantillaDetGuiaDespacho();
                                                        plantdet = plantdet.Replace("#NROLINDET#", (jsonOperaciones["itempos"]).ToString());
                                                        plantdet = plantdet.Replace("#TPOCODIGO#", "INTERNO");
                                                        plantdet = plantdet.Replace("#VLRCODIGO#", (jsonOperaciones["alu"]).ToString());
                                                        DTEValidaciones((jsonOperaciones["alu"]).ToString(), "Código Item", 35, 1);
                                                        plantdet = plantdet.Replace("#NMBITEM#", (jsonOperaciones["description1"]).ToString());
                                                        DTEValidaciones((jsonOperaciones["description1"]).ToString(), "Nombre Item", 70, 1);
                                                        plantdet = plantdet.Replace("#DESC#", (jsonOperaciones["description1"]).ToString());
                                                        string cantidadItem = (jsonOperaciones["qty"]).ToString();
                                                        cantidadItem = cantidadItem.Replace(',', '.');
                                                        plantdet = plantdet.Replace("#QTYITEM#", cantidadItem);


                                                        int ivadet = (Int32.Parse((jsonOperaciones["price"]).ToString()) * Int32.Parse(this.ParamValues.DTEVyV_TasaIVA)) / 100;
                                                        string cantidad = (jsonOperaciones["qty"]).ToString();
                                                        string precio = (jsonOperaciones["price"]).ToString();
                                                        this.objLog.writeDTEVyV("cantidad "+ cantidad);
                                                        string cantidad_decimal = cantidad.Replace(".", ",");
                                                        this.objLog.writeDTEVyV("cantidad_decimal " + cantidad_decimal);
                                                        Decimal montoitem = ((Decimal.Parse(precio) * Decimal.Parse(cantidad_decimal)) - (ivadet * Decimal.Parse(cantidad_decimal)));
                                                        plantdet = plantdet.Replace("#MONTOITEM#", Decimal.Round(Convert.ToDecimal(montoitem.ToString())).ToString());
                                                        int preciosiniva = (Int32.Parse(precio) - ivadet);
                                                        plantdet = plantdet.Replace("#PRCITEM#", preciosiniva.ToString());
                                                        DTEValidaciones(preciosiniva.ToString(), "Precio Item", 19, 2);

                                                    }
                                                }
                                                else
                                                {
                                                    msg = "No es posible superar los 40 Items";
                                                    throw new ApplicationException(msg);
                                                }

                                            }
                                            plant = plant.Replace("#DETALLE#", plantdet);
                                            #endregion,
                                            #region Folio
                                            this.objLog.writeDTEVyV("Solicitando Folio");
                                            folio = this.DTESolicitarFolio(DTEVyV_RutEmisor, posflag);
                                            this.objLog.writeDTEVyV("Folio Obtenido " + folio);
                                            if (folio == "0")
                                            {
                                                //this.objLog.write(folio);
                                                flag2 = false;
                                                num = 200;
                                                status = "999";
                                                msg = "No existen folios disponibles";
                                                string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                str = string.Concat(textArray5);
                                                this.objLog.write("ProcessEvent** Error: " + msg);
                                            }
                                            else
                                            {
                                                plant = plant.Replace("#DOCUMENTOID#", "R" + DTEVyV_RutEmisor + "T" + posflag + "F" + folio);
                                                plant = plant.Replace("#FOLIO#", folio);
                                            }
                                            this.objLog.writeDTEVyV("Plantilla xml  " + plant);
                                            #endregion
                                            if (folio != "0")
                                            {
                                                //this.objLog.writeDTEVyV("Enviando XML al DTE");
                                                docelectpdf = this.DTESolicitaDocElectronico(plant, ref status, ref msg, ref xmlres);
                                                this.objLog.writeDTEVyV("status DTE " + status);
                                                this.objLog.writeDTEVyV("respuesta DTE " + msg);

                                                if (status == "0" || status == "4")
                                                {
                                                    flag2 = false;
                                                    num = 200;
                                                    this.objLog.writeDTEVyV(xmlres);
                                                    ted = retornaTED(xmlres);
                                                    tedbase64 = EncodeStrToBase64(ted);
                                                    if (status == "4")
                                                    {
                                                        status = "0";
                                                        msg = "Error al generar el PDF";
                                                    }
                                                    string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                    str = string.Concat(textArray5);
                                                }
                                                else
                                                {
                                                    flag2 = false;
                                                    num = 200;
                                                    string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                    str = string.Concat(textArray5);
                                                    this.objLog.write("ProcessEvent** Error: " + "Estatus fuera de rango" + msg);
                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            flag2 = false;
                                            num = 200;
                                            status = "555";
                                            this.objLog.write("EXCEPTION2");
                                            str = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                                            this.objLog.write("ProcessEvent *********** Error General -- " + e.Message);
                                            this.txtMensaje.Text = this.txtMensaje.Text + "\nExcepcion: " + e.Message;
                                        }
                                    }
                                    else
                                    {
                                        flag2 = false;
                                        num = 200;
                                        status = "556";
                                        string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + errors + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                        str = string.Concat(textArray5);
                                        this.objLog.write("ProcessEvent** Error: " + errors);
                                    }
                                }
                            }
                        }
                    }
                    if (((ADirection == "FromClient") && (AHTTPVerb == "POST")) && (AResourceName.ToUpper() == "CALLDTEVYVVOUCHER"))
                    {
                        str18 = "";
                        str19 = "";
                        str20 = "";
                        str21 = "";
                        string errors = string.Empty;
                        string folio = string.Empty;
                        string plant = string.Empty;
                        string plantdet = string.Empty;
                        string status = string.Empty;
                        string msg = string.Empty;
                        string xmlres = string.Empty;
                        string ted = string.Empty;
                        string tedbase64 = string.Empty;

                        foreach (JObject obj3 in JArray.Parse("[" + message.Payload + "]").Children<JObject>())
                        {
                            foreach (JProperty property in obj3.Properties())
                            {
                                if (property.Name == "documentID")
                                {
                                    token = this.doPRISMLoginAux(this.objLog);
                                    str21 = (string)property.Value;
                                    JObject json = JObject.Parse(this.GETReceive(str21, token));
                                    errors = json.GetValue("errors").ToString();
                                    if (errors == "")
                                    {
                                       // this.objLog.writeDTEVyV(json.GetValue("data")[0].ToString());
                                        JObject json2 = JObject.Parse(json.GetValue("data")[0].ToString());
                                        string idTienda = json2.GetValue("storeno").ToString();
                                        JObject jsonstore = JObject.Parse(this.GETStore(idTienda, token));
                                        JObject store = JObject.Parse(jsonstore.GetValue("data")[0].ToString());
                                        string DTEVyV_Tienda = store.GetValue("storename").ToString();
                                        string DTEVyV_RutEmisor = store.GetValue("zip").ToString().ToUpper();
                                        string DTEVyV_RznSocEmi = store.GetValue("udf1string").ToString();
                                        string DTEVyV_Giro = store.GetValue("udf2string").ToString();
                                        string DTEVyV_Direccion_tienda = store.GetValue("address1").ToString();
                                        string DTEVyV_DirOrigen = store.GetValue("address4").ToString();
                                        string DTEVyV_CmnaOrigen = store.GetValue("address5").ToString();
                                        string DTEVyV_CiudadOrigen = store.GetValue("address6").ToString();
                                        string DTEVyV_Telefono = store.GetValue("phone2").ToString();
                                        string DTEVyV_Telefono_tienda = store.GetValue("phone1").ToString();

                                        string posflag = "52";
                                        try
                                        {
                                            this.objLog.writeDTEVyV("Guia de Despacho - Vouchers");
                                            plantilla = new PlantillaXml();
                                            plant = this.plantilla.PlantillaCabGuiaDespacho();
                                            #region Creacion de XML Cabecera
                                            //DOCUMENTO
                                            plant = plant.Replace("#TPODTE#", posflag);
                                            plant = plant.Replace("#FECEMISION#", DateTime.Now.ToString("yyyy-MM-dd"));
                                            plant = plant.Replace("#INDTRASLADO#", "7");
                                            plant = plant.Replace("#MNTBRUTO#", "1");
                                            plant = plant.Replace("#FECVENC#", DateTime.Now.ToString("yyyy-MM-dd"));
                                            //EMISOR
                                            plant = plant.Replace("#RUTEMISOR#", json2.GetValue("origzip").ToString().ToUpper());
                                            plant = plant.Replace("#RZNSOC#", json2.GetValue("origstoreudf1string").ToString());
                                            DTEValidaciones(json2.GetValue("origstoreudf1string").ToString(), "Razon Social Emisor", 100, 1);
                                            plant = plant.Replace("#GIRO#", LargoCadenaMax((json2.GetValue("origstoreudf2string").ToString()+" "+ json2.GetValue("origstoreudf3string").ToString()).Trim(),80)   );
                                            DTEValidaciones(LargoCadenaMax((json2.GetValue("origstoreudf2string").ToString() + " " + json2.GetValue("origstoreudf3string").ToString()).Trim(),80)  , "Giro Emisor", 80, 1);
                                            plant = plant.Replace("#ACTECO#", this.ParamValues.DTEVyV_ACTECO);
                                            plant = plant.Replace("#CDGSIISUCUR#", json2.GetValue("origstoreudf4string").ToString());
                                            DTEValidaciones(json2.GetValue("origstoreudf4string").ToString(), "Sucursal Emisor", 20, 1);
                                            plant = plant.Replace("#DIRORIGEN#", json2.GetValue("origaddress1").ToString());
                                            plant = plant.Replace("#CMNAORIGEN#", json2.GetValue("origaddress2").ToString());
                                            plant = plant.Replace("#CIUDADORIGEN#", json2.GetValue("origaddress3").ToString());
                                            //RECEPTOR
                                            plant = plant.Replace("#RUTRECEPTOR#", json2.GetValue("vendorpostalcode").ToString().ToUpper());
                                            DTEValidaciones(json2.GetValue("vendorpostalcode").ToString(), "RUT Receptor", 10, 1);
                                            plant = plant.Replace("#RZNSOCRECEP#", json2.GetValue("vendoraddress4").ToString());
                                            DTEValidaciones(json2.GetValue("vendoraddress4").ToString(), "Razon Social Receptor", 100, 1);
                                            plant = plant.Replace("#GIRORECEP#", LargoCadenaMax((json2.GetValue("vendoraddress5").ToString()+ " " + json2.GetValue("vendoraddress6").ToString()).Trim(),40) );
                                            DTEValidaciones(LargoCadenaMax((json2.GetValue("vendoraddress5").ToString() + " " + json2.GetValue("vendoraddress6").ToString()).Trim(), 40) , "Giro Receptor", 40, 1);
                                            plant = plant.Replace("#DIRRECEP#", json2.GetValue("vendoraddress1").ToString());
                                            DTEValidaciones(json2.GetValue("vendoraddress1").ToString(), "Dirección Receptor", 70, 2);
                                            plant = plant.Replace("#CMNARECEP#", json2.GetValue("vendoraddress2").ToString());
                                            DTEValidaciones(json2.GetValue("vendoraddress2").ToString(), "Comuna Receptor", 20, 1);
                                            plant = plant.Replace("#CIUDADRECEP#", json2.GetValue("vendoraddress3").ToString());
                                            DTEValidaciones(json2.GetValue("vendoraddress3").ToString(), "Ciudad Receptor", 20, 1);
                                            //plant = plant.Replace("#DirPostal#", "");
                                            //plant = plant.Replace("#CmnaPostal#", "");
                                            //plant = plant.Replace("#CiudadPostal#", "");
                                            plant = plant.Replace("#DIRDEST#", json2.GetValue("vendoraddress1").ToString());
                                            plant = plant.Replace("#CMNADEST#", json2.GetValue("vendoraddress2").ToString());
                                            plant = plant.Replace("#CIUDADDEST#", json2.GetValue("vendoraddress3").ToString());
                                            #endregion
                                            #region Creacion Xml Detalle
                                            string jsonitem = json2.GetValue("recvitem").ToString(); 
                                            JArray jsonArrayitem = JArray.Parse(jsonitem);
                                            int valortotal = 0;
                                            foreach (JObject jsonOperaciones in jsonArrayitem.Children<JObject>())
                                            {
                                                if (contadordetalle < 41)
                                                {
                                                    string kitflag = "0";
                                                    try
                                                    {
                                                        kitflag = (jsonOperaciones["kitflag"]).ToString();
                                                    }catch(Exception e)
                                                    {
                                                        kitflag = "0";
                                                    }
                                                   
                                                    if (!"5".Equals(kitflag))
                                                    {
                                                        plantilla = new PlantillaXml();
                                                        plantdet = plantdet + this.plantilla.PantillaDetGuiaDespacho();
                                                        plantdet = plantdet.Replace("#NROLINDET#", (jsonOperaciones["itempos"]).ToString());
                                                        plantdet = plantdet.Replace("#TPOCODIGO#", "INTERNO");
                                                        plantdet = plantdet.Replace("#VLRCODIGO#", (jsonOperaciones["alu"]).ToString());
                                                        DTEValidaciones((jsonOperaciones["alu"]).ToString(), "Código Item", 35, 1);
                                                        plantdet = plantdet.Replace("#NMBITEM#", (jsonOperaciones["description1"]).ToString());
                                                        DTEValidaciones((jsonOperaciones["description1"]).ToString(), "Nombre Item", 70, 1);
                                                        plantdet = plantdet.Replace("#DESC#", (jsonOperaciones["description1"]).ToString());
                                                        string cantidadItem = (jsonOperaciones["qty"]).ToString();
                                                        cantidadItem = cantidadItem.Replace(',', '.');
                                                        plantdet = plantdet.Replace("#QTYITEM#", cantidadItem);
                                                        int ivadet = (Int32.Parse((jsonOperaciones["price"]).ToString()) * Int32.Parse(this.ParamValues.DTEVyV_TasaIVA)) / 100;
                                                        string cantidad = (jsonOperaciones["qty"]).ToString();
                                                        string precio = (jsonOperaciones["price"]).ToString();
                                                        string cantidad_decimal = cantidad.Replace(".", ",");
                                                        Decimal montoitem = ((Decimal.Parse(precio) * Decimal.Parse(cantidad_decimal)) - (ivadet * Decimal.Parse(cantidad_decimal)));
                                                        plantdet = plantdet.Replace("#MONTOITEM#", Decimal.Round(Convert.ToDecimal(montoitem.ToString())).ToString());
                                                        valortotal = valortotal + Int32.Parse((jsonOperaciones["price"]).ToString());
                                                        int preciosiniva = (Int32.Parse(precio) - ivadet);
                                                        plantdet = plantdet.Replace("#PRCITEM#", preciosiniva.ToString());
                                                        DTEValidaciones(preciosiniva.ToString(), "Precio Item", 19, 2);
                                                        contadordetalle = contadordetalle + 1;
                                                    }
                                                }
                                                else
                                                {
                                                    msg = "No es posible superar los 40 Items";
                                                    throw new ApplicationException(msg);
                                                }
                                            }
                                            //TOTALES
                                            int iva = (valortotal * Int32.Parse(this.ParamValues.DTEVyV_TasaIVA)) / 100;
                                            int neto = valortotal - iva;
                                            plant = plant.Replace("#MNTNETO#", neto.ToString());
                                            DTEValidaciones(neto.ToString(), "Monto Neto", 18, 3);
                                            plant = plant.Replace("#MNTEXE#", "0");
                                            plant = plant.Replace("#TASAIVA#", this.ParamValues.DTEVyV_TasaIVA);
                                            DTEValidaciones(this.ParamValues.DTEVyV_TasaIVA, "Tasa IVA", 6, 3);
                                            plant = plant.Replace("#IVA#", iva.ToString());
                                            DTEValidaciones(iva.ToString(), "IVA", 18, 3);
                                            plant = plant.Replace("#MNTTOTAL#", valortotal.ToString());
                                            DTEValidaciones(valortotal.ToString(), "Monto Total", 18, 3);
                                            plant = plant.Replace("#DETALLE#", plantdet);
                                            #endregion,
                                            #region Folio
                                            //this.objLog.writeDTEVyV("Solicitando Folio");
                                            folio = this.DTESolicitarFolio(DTEVyV_RutEmisor, posflag);
                                            this.objLog.writeDTEVyV("Folio Obtenido " + folio);
                                            if (folio == "0")
                                            {
                                                //this.objLog.write(folio);
                                                flag2 = false;
                                                num = 200;
                                                status = "999";
                                                msg = "No existen folios disponibles";
                                                string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                str = string.Concat(textArray5);
                                                this.objLog.write("ProcessEvent** Error: " + msg);
                                            }
                                            else
                                            {
                                                plant = plant.Replace("#DOCUMENTOID#", "R" + DTEVyV_RutEmisor + "T" + posflag + "F" + folio);
                                                plant = plant.Replace("#FOLIO#", folio);
                                            }
                                            this.objLog.writeDTEVyV("Plantilla xml  " + plant);

                                            #endregion
                                            if (folio != "0")
                                            {
                                               // this.objLog.writeDTEVyV("Enviando XML al DTE");
                                                docelectpdf = this.DTESolicitaDocElectronico(plant, ref status, ref msg, ref xmlres);
                                                this.objLog.writeDTEVyV("status DTE " + status);
                                                this.objLog.writeDTEVyV("respuesta DTE " + msg);

                                                if (status == "0" || status == "4")
                                                {
                                                    flag2 = false;
                                                    num = 200;
                                                    this.objLog.writeDTEVyV(xmlres);
                                                    ted = retornaTED(xmlres);
                                                    tedbase64 = EncodeStrToBase64(ted);
                                                    if (status == "4")
                                                    {
                                                        status = "0";
                                                        msg = "Error al generar el PDF";
                                                    }
                                                    string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                    str = string.Concat(textArray5);
                                                }
                                                else
                                                {
                                                    flag2 = false;
                                                    num = 200;
                                                    string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                    str = string.Concat(textArray5);
                                                    this.objLog.write("ProcessEvent** Error: " + "Estatus fuera de rango" + msg);
                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            flag2 = false;
                                            num = 200;
                                            status = "555";
                                            this.objLog.write("EXCEPTION2");
                                            str = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                                            this.objLog.write("ProcessEvent *********** Error General -- " + e.Message);
                                            this.txtMensaje.Text = this.txtMensaje.Text + "\nExcepcion: " + e.Message;
                                        }
                                    }
                                    else
                                    {
                                        flag2 = false;
                                        num = 200;
                                        status = "556";
                                        string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + errors + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                        str = string.Concat(textArray5);
                                        this.objLog.write("ProcessEvent** Error: " + errors);
                                    }
                                }
                            }
                        }
                       // this.objLog.write("SALIMOS");
                    }
                    if (((ADirection == "FromClient") && (AHTTPVerb == "GET")) && (AResourceName.ToUpper() == "CALLFEBOSFELOGIN"))
                    {
                        this.objLog.writeFEBOS("Iniciando Proceso de Llama al Login de FEBOS");
                        string str16 = string.Empty;
                        str16 = this.doPRISMLoginAux(this.objLog);
                        stFEBOSParam param = this.initFEBOSParam();
                        if (str16 == "")
                        {
                            this.objLog.writeFEBOS("Error: No se pudo realizar el login a PRISM para realizar el proceso de Facturacione Electr\x00f3nica");
                            throw new PrismException("No se ha podido encontrar las variables en PRISM para ejecutar el proceso de Autorizacion de Documentos Electr\x00f3nicos");
                        }
                        param = this.getCustomization(this.objLog, str16);
                        if (((param.strFEBOSURLLogin == "") || (param.strFEBOSLoginMail == "")) || (param.strFEBOSLoginPass == ""))
                        {
                            this.objLog.writeFEBOS("Error: No se cosigio URL o Login o Password para conectar con FEBOS");
                            throw new PrismException("No se ha podido encontrar las variables en FEBOS para ejecutar el proceso de Autorizacion de Documentos Electr\x00f3nicos");
                        }
                        if (this.callFEBOSLogin(this.objLog, ref param))
                        {
                            flag2 = false;
                            str = "status:0\x00a5token:" + param.strFEBOSToken + "\x00a5ui:" + param.strFEBOSUI;
                        }
                        else
                        {
                            object[] objArray1 = new object[] { "Error: No se pudo realizar el Login en FEBOS: Codigo - ", param.intCode, ", Mensaje - ", param.strMessage };
                            this.objLog.writeFEBOS(string.Concat(objArray1));
                            flag2 = false;
                            object[] objArray2 = new object[] { "status:1\x00a5mensaje: No se pudo realizar el Login en FEBOS Codigo (", param.intCode, ") - ", param.strMessage };
                            str = string.Concat(objArray2);
                        }
                    }
                    if (((ADirection == "FromClient") && (AHTTPVerb == "GET")) && (AResourceName.ToUpper() == "CALLTOKEN"))
                    {
                        token = this.doPRISMLoginAux(this.objLog);
                        if (token == null)
                        {
                            flag2 = false;
                            num = 200;
                            string status = "550";
                            string errors = "No es posible encontrar token";
                            string tedbase64 = "";
                            string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"MsgEstatus\":" + "\"" + errors + "\"," + "\"TOKEN\":" + "\"" + tedbase64 + "\"" + "}" };
                            str = string.Concat(textArray5);
                        }
                        else
                        {
                            flag2 = false;
                            num = 200;
                            string status = "0";
                            string errors = "";
                            string tokenGen = token;
                            string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"MsgEstatus\":" + "\"" + errors + "\"," + "\"TOKEN\":" + "\"" + tokenGen + "\"" + "}" };
                            str = string.Concat(textArray5);
                        }
                    }
                    if (((ADirection == "FromClient") && (AHTTPVerb == "POST")) && (AResourceName.ToUpper() == "CALLDESCUENTO"))
                    {
                        this.objLog.write("Ejecutando CALLDESCUENTO  ");
                        string tokenAux = this.doPRISMLoginAux(this.objLog);
                        if (tokenAux == null || string.IsNullOrEmpty(tokenAux))
                        {
                            flag2 = false;
                            num = 200;
                            string status = "550";
                            string errors = "No es posible encontrar token";
                            string tedbase64 = "";
                            string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"MsgEstatus\":" + "\"" + errors + "\"," + "\"TOKEN\":" + "\"" + tedbase64 + "\"" + "}" };
                            str = string.Concat(textArray5);
                        }
                        else
                        {
                            flag2 = false;
                            num = 200;
                            string status = "0";
                            string msg = "";
                            string tedbase64 = token;
                            string url = "";
                            string doc_sid = "";
                            string item_sid = "";
                            string item_row_version = "";
                            string descuento = "";
                            string tipo_descuento = "";
                            string razon_descuento = "";

                            foreach (JObject obj3 in JArray.Parse("[" + message.Payload + "]").Children<JObject>())
                            {
                                foreach (JProperty property in obj3.Properties())
                                {
                                    if (property.Name == "url")
                                    {
                                        string url_temp = (string)property.Value;
                                        int found = url_temp.IndexOf("?");
                                        url = url_temp.Substring(0, found);
                                    }
                                    if (property.Name == "doc_sid")
                                    {
                                        doc_sid = (string)property.Value;
                                    }
                                    if (property.Name == "item_sid")
                                    {
                                        item_sid = (string)property.Value;
                                    }
                                    if (property.Name == "item_row_version")
                                    {
                                        item_row_version = (string)property.Value;
                                    }
                                    if (property.Name == "descuento")
                                    {
                                        descuento = (string)property.Value;
                                    }
                                    if (property.Name == "tipo_descuento")
                                    {
                                        tipo_descuento = (string)property.Value;
                                    }
                                    if (property.Name == "razon_descuento")
                                    {
                                        razon_descuento = (string)property.Value;
                                    }
                                }
                            }

                            try
                            {
                                JArray jsonArray = JArray.Parse(this.GETDocItem(this.ParamValues.PRISMURL + "/" + url, tokenAux));
                                JObject jsonItem = new JObject();
                                foreach (JObject json in jsonArray.Children<JObject>())
                                {
                                    jsonItem = json;
                                }

                                string promo_disc_modifiedmanually = jsonItem.GetValue("promo_disc_modifiedmanually").ToString();
                                string row_version = jsonItem.GetValue("row_version").ToString();

                                /*desde ahora la validacion del descuento manual se realiza desde el webcliente por lo que no debe ser validado en el proxy*/
                                promo_disc_modifiedmanually = "false";

                                if ("TRUE".Equals(promo_disc_modifiedmanually.ToUpper()))
                                {
                                    string item = jsonItem.GetValue("item_description1").ToString();
                                    string error = "el Item  " + item + " tiene ingresado un descuento manual por lo que no se puede aplicar en la promocion";
                                    objLog.write(error);
                                    status = "550";
                                    msg = "Error " + error;
                                }
                                else
                                {
                                    string data = "[{\"manual_disc_value\": " + descuento + ", \"manual_disc_type\": " + tipo_descuento + ", \"manual_disc_reason\": \"" + razon_descuento + "\"}]";
                                    var client = new RestClient(this.ParamValues.PRISMURL + "/" + url + "?filter=row_version,eq," + row_version);
                                    var request = new RestRequest(Method.PUT);
                                    request.AddHeader("Auth-Session", tokenAux);
                                    request.AddHeader("Accept", "application/json, version=2");
                                    request.AddHeader("Content-Type", "application/json; charset=utf-8");
                                    request.AddParameter("application/json", data, ParameterType.RequestBody);
                                    IRestResponse response = client.Execute(request);

                                    if (response.StatusCode != HttpStatusCode.OK)
                                    {
                                        status = "550";
                                        msg = "Error " + response.StatusCode;
                                    }
                                    else
                                    {
                                        status = "0";
                                        msg = response.Content;

                                    }
                                }
                                
                            }
                            catch (Exception exception2)
                            {
                                objLog.write("Error: " + exception2.Message);
                                status = "550";
                                msg = "Error " + exception2.Message;

                            }


                            string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"MsgEstatus\":" + msg  + "}" };
                            str = string.Concat(textArray5);
                        }
                    }
                    if (((ADirection == "FromClient") && (AHTTPVerb == "POST")) && (AResourceName.ToUpper() == "CALLFEBOSFEUPLOADTXTDTE"))
                    {
                        str18 = "";
                        str19 = "";
                        str20 = "";
                        str21 = "";
                        foreach (JObject obj3 in JArray.Parse("[" + message.Payload + "]").Children<JObject>())
                        {
                            foreach (JProperty property in obj3.Properties())
                            {
                                if (property.Name == "token")
                                {
                                    str18 = (string)property.Value;
                                }
                                if (property.Name == "uid")
                                {
                                    str19 = (string)property.Value;
                                }
                                if (property.Name == "data")
                                {
                                    str20 = (string)property.Value;
                                }
                                if (property.Name == "internalID")
                                {
                                    str21 = (string)property.Value;
                                }
                            }
                        }

                        string str22 = Base64Decode(str20);
                        this.objLog.writeFEBOS("Se recibi\x00f3 la solicitud para Enviar el Doumento Electr\x00f3nico a FEBOS");
                        token = this.doPRISMLoginAux(this.objLog);
                        stFEBOSParam stResultFebosParam = this.initFEBOSParam();
                        this.objLog.write("TOKEN : " + token);
                        if (token != "")
                        {
                            stResultFebosParam = this.getCustomization(this.objLog, token);
                            this.objLog.write("PARAMETRO RESULTADO : " + stResultFebosParam);
                            if (((stResultFebosParam.strFEBOSURLUploadDTE == "") || ((str18 == "") || (str19 == ""))) || (str20 == ""))
                            {
                                this.objLog.write("IF 1");
                                this.objLog.writeFEBOS("Error: No se consiguieron los parametros para conectar a Febos a realizar el Upload o las variables de token o uid llegaron vacias");
                                flag2 = false;
                                str = "status:1\x00a5mensaje: No se pudo realizar la notificaci\x00f3n del documento electr\x00f3nico en FEBOS";
                                this.objLog.write(str);
                            }
                            else if (this.callFEBOSUploadDTE(this.objLog, ref stResultFebosParam, str18, str19, str20, str21))
                            {
                                this.objLog.write("IF 2");
                                flag2 = false;
                                num = 200;
                                string[] textArray5 = new string[] { "status:0\x00a5febosid:", stResultFebosParam.strFEBOSID, "\x00a5folio:", stResultFebosParam.strFolio, "\x00a5ted:", stResultFebosParam.strTED };
                                str = string.Concat(textArray5);
                                this.objLog.write(str);
                            }
                            else
                            {
                                this.objLog.write("ELSE 3");
                                object[] objArray3 = new object[] { "Error: No se pudo realizar el Upload en FEBOS: Codigo - ", stResultFebosParam.intCode, ", Mensaje - ", stResultFebosParam.strMessage };
                                this.objLog.writeFEBOS(string.Concat(objArray3));
                                flag2 = false;
                                object[] objArray4 = new object[] { "status:1\x00a5mensaje: No se pudo realizar la autorizaci\x00f3n del documento en FEBOS Codigo (", stResultFebosParam.intCode, ") - ", stResultFebosParam.strMessage };
                                str = string.Concat(objArray4);
                                this.objLog.write(str);
                            }
                        }
                        else
                        {
                            goto TR_0007;
                        }

                    }
                    if (((ADirection == "FromClient") && (AHTTPVerb == "POST")) && (AResourceName.ToUpper() == "CALLDTESIGNATURE"))
                    {
                        this.objLog.write("INICIO  CALLDTESIGNATURE");
                        str18 = "";
                        str19 = "";
                        str20 = "";
                        str21 = "";
                        string validacion = string.Empty;
                        string errors = string.Empty;
                        string numdoccliente = string.Empty;
                        string folio = string.Empty;
                        //string plant = string.Empty;
                        //string plantdet = string.Empty;
                        //string plantref = string.Empty;
                        string status = string.Empty;
                        string msg = string.Empty;
                        string posflagtipo = string.Empty;
                        string xmlres = string.Empty;
                        string ted = string.Empty;
                        string tedbase64 = string.Empty;
                        //this.objLog.write("ImpresoraOPOS resImpresion");
                        ImpresoraOPOS resImpresion = new ImpresoraOPOS();
                        //this.objLog.write("JObject obj3 in");
                        foreach (JObject obj3 in JArray.Parse("[" + message.Payload + "]").Children<JObject>())
                        {
                            foreach (JProperty property in obj3.Properties())
                            {
                                if (property.Name == "documentID")
                                {
                                    this.objLog.write("property.Name == documentID se procesa requerimiento");

                                    token = this.doPRISMLoginAux(this.objLog);
                                    str21 = (string)property.Value;
                                   // this.objLog.write("documentID: " + str21);
                                    JObject json = JObject.Parse(this.GETDocument(str21, token));
                                   // this.objLog.write("json: " + json);
                                    errors = json.GetValue("errors").ToString();
                                    //this.objLog.write("errors: " + errors);
                                    if (errors == "")
                                    {
                                        // this.objLog.write("INICIO  Documento");

                                        JObject json2 = JObject.Parse(json.GetValue("data")[0].ToString());
                                        this.objLog.writeDTESignature(" json2 " + json2);
                                        //this.objLog.write(json.GetValue("data")[0].ToString());
                                        string posflag = json2.GetValue("posflag1").ToString().Trim().Substring(0, 2);
                                        //this.objLog.writeDTESignature(" posflag " + posflag);

                                        string idcliente = json2.GetValue("btcuid").ToString();
                                       // this.objLog.writeDTESignature(" idcliente " + idcliente);
                                        JArray jsoncustomerRest1 = JArray.Parse(this.GETCustomerRest(idcliente, token));
                                        JObject jsoncustomerRest = JObject.Parse(jsoncustomerRest1[0].ToString());
                                       // JObject jsoncustomer = JObject.Parse(this.GETCustomer(idcliente, token));
                                       // JObject jsoncustomer2 = JObject.Parse(jsoncustomer.GetValue("data")[0].ToString());
                                        //this.objLog.write(jsoncustomer.GetValue("data")[0].ToString());
                                        numdoccliente = jsoncustomerRest.GetValue("info1").ToString();

                                        //this.objLog.writeDTESignature(" numdoccliente " + numdoccliente);

                                        if (numdoccliente == "")
                                        {
                                            numdoccliente = jsoncustomerRest.GetValue("info2").ToString();
                                            if (numdoccliente == "")
                                            {
                                                numdoccliente = this.ParamValues.DTESignature_RutReceptordefault;
                                            }
                                        }

                                        this.objLog.write("numdoccliente cliente: " + numdoccliente);

                                        if (posflag == "39")// BOLETA ELECTRONICA
                                        {
                                            try
                                            {
                                                this.objLog.write("INICIO  BOLETA ELECTRONICA");

                                                #region PROCESO BOLETA ELEC
                                                AgenteSignature agent = new AgenteSignature(this.ParamValues.DTESignature_Server, this.ParamValues.DTESignature_Port);
                                                SignatureDTE boleta = new SignatureDTE("eBoleta");

                                                #region Creacion Cabecera
                                                                                             
                                                //Validacion de rut
                                                //RECEPTOR
                                                DTEValidaciones(numdoccliente, "Rut Receptor", 10, 1);
                                                boleta.Args.Operation.Receiver.RUTRecep = numdoccliente;
                                               // this.objLog.write("Rut Receptor --> "+ numdoccliente);

                                                //DOCUMENTO
                                                //Fecha de emision , vencimiento, indicador de servicio los carga por defecto al instanciar el objecto
                                                //si fuese necesario se pueden sobreescibir seteandolos con otro valor

                                                //EMISOR
                                                //EL emisor lo carga el agente Asyn de signature no es nesesario enviar datos del emisor

                                                //RECEPTOR
                                                string nombrecliente = jsoncustomerRest.GetValue("first_name").ToString() + " " + jsoncustomerRest.GetValue("last_name").ToString();
                                                if ((nombrecliente.Trim() == "") || ((nombrecliente == " ")))
                                                {
                                                    DTEValidaciones(this.ParamValues.DTESignature_NombreClientedefault, "Razón Social Receptor", 100, 1);
                                                    nombrecliente = this.ParamValues.DTESignature_NombreClientedefault;
                                                }

                                                DTEValidaciones(nombrecliente, "Razon Social Receptor", 100, 2);
                                                boleta.Args.Operation.Receiver.RznSocRecep = nombrecliente;


                                                //Direcciones
                                                if (!String.IsNullOrEmpty(jsoncustomerRest.GetValue("primary_address_line_1").ToString()))
                                                {
                                                    boleta.Args.Operation.Receiver.DirRecep = jsoncustomerRest.GetValue("primary_address_line_1").ToString() + " " + jsoncustomerRest.GetValue("primary_address_line_2").ToString();
                                                }
                                                if (!String.IsNullOrEmpty(jsoncustomerRest.GetValue("primary_address_line_4").ToString()))
                                                {
                                                    boleta.Args.Operation.Receiver.CmnaRecep = jsoncustomerRest.GetValue("primary_address_line_4").ToString();
                                                }
                                                if (!String.IsNullOrEmpty(jsoncustomerRest.GetValue("primary_address_line_5").ToString()))
                                                {
                                                    boleta.Args.Operation.Receiver.CiudadRecep = jsoncustomerRest.GetValue("primary_address_line_5").ToString();
                                                }



                                                //TOTALES
                                                if (json2.GetValue("saletotalamt").ToString() == "0")
                                                {

                                                    Totals total = new Totals();
                                                    total.IVA = 0;
                                                    total.MntTotal = 1;
                                                    total.MntNeto = 1;
                                                    total.TasaIVA = float.Parse(this.ParamValues.DTESignature_TasaIVA);
                                                    boleta.Args.Operation.Totals = total;
                                                }
                                                else
                                                {
                                                    Totals total = new Totals();
                                                    this.objLog.write("Totales");
                                                    total.IVA = json2["saletotaltaxamt"].Value<Int32>();//.["saletotaltaxamt").ToString());
                                                    total.TasaIVA = float.Parse(this.ParamValues.DTESignature_TasaIVA);
                                                    //DTEValidaciones(json2.GetValue("saletotaltaxamt").ToString(), "IVA", 18, 1);
                                                    total.MntTotal = json2["saletotalamt"].Value<Int32>();// Int32.Parse(json2.GetValue("saletotalamt").ToString());
                                                    //DTEValidaciones(json2.GetValue("saletotalamt").ToString(), "Monto Total", 18, 1);
                                                    //int neto = json2["saletotalamt"].Value<Int32>() - json2["saletotaltaxamt"].Value<Int32>();// Int32.Parse(json2.GetValue("saletotalamt").ToString()) - Int32.Parse(json2.GetValue("saletotaltaxamt").ToString());
                                                    total.MntNeto = total.MntTotal - total.IVA;
                                                    //DTEValidaciones(neto.ToString(), "Monto Neto", 18, 1);

                                                    boleta.Args.Operation.Totals = total;
                                                }
                                                #endregion
                                                #region Creacion Detalle
                                                string strjson3 = json2.GetValue("docitem").ToString();
                                                JArray jsonArray = JArray.Parse(strjson3);
                                                this.objLog.write("Detalle");
                                                foreach (JObject jsonOperaciones in jsonArray.Children<JObject>())
                                                {
                                                    if (contadordetalle < 61)
                                                    {
                                                        //string descuento = string.Empty;
                                                        Detalle det = new Detalle();

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

                                                        
                                                        DTEValidaciones(jsonOperaciones["description1"].ToString(), "Nombre Item", 70, 1);
                                                        det.NroLinDet = jsonOperaciones["itempos"].Value<Int32>();// Int32.Parse((jsonOperaciones["itempos"]).ToString());
                                                        det.NmbItem = (jsonOperaciones["description1"]).ToString();
                                                        det.DscItem = (jsonOperaciones["description1"]).ToString();

                                                        //string cantidad = (jsonOperaciones["qty"]).ToString();
                                                        //cantidad = cantidad.Replace(',', '.');
                                                        det.QtyItem = jsonOperaciones["qty"].Value<float>();// float.Parse(cantidad);
                                                        if(this.ParamValues.DTESignature_PreciosConIva == "S")
                                                        {
                                                            det.PrcItem = jsonOperaciones["price"].Value<float>();
                                                        }
                                                        else
                                                        {
                                                            det.PrcItem = jsonOperaciones["price"].Value<float>() + jsonOperaciones["taxamt"].Value<float>();
                                                        }
                                                        //DTEValidaciones(jsonOperaciones["origprice"].ToString(), "Precio Item", 19, 2);
                                                        //string iva = (jsonOperaciones["taxamt"]).ToString().Split(',')[0];
                                                        //string precio = (jsonOperaciones["price"]).ToString();
                                                        //float montoitem = (Int32.Parse(precio) * float.Parse(cantidad));
                                                        //det.MontoItem = Convert.ToInt32(montoitem);
                                                        det.MontoItem = Convert.ToInt32(det.QtyItem * det.PrcItem);



                                                        det.CdgItems.Add(new Cdgitem("INTERNO", (jsonOperaciones["alu"]).ToString()));
                                                        DTEValidaciones(jsonOperaciones["alu"].ToString(), "Valor Código", 35, 1);

                                                        boleta.Args.Operation.Detalles.Add(det);

                                                        contadordetalle = contadordetalle + 1;

                                                    }
                                                    else
                                                    {
                                                        msg = "No es posible superar los 60 Items";
                                                        throw new ApplicationException(msg);
                                                    }
                                                }

                                                //agregar referencia de cliente cuando tenga correo.
                                                //agregar referencia de devolucion.

                                                #endregion
                                                #region Folio
                                                //No es necesario solicitar folio el agente le asiganara uno de los disponibles
                                                #endregion

                                                int folioSignature = 0;
                                                string error = "";
                                                this.objLog.writeDTESignature(boleta.ToString());

                                                if (agent.Ejecutar(boleta, out folioSignature, out ted, out error))
                                                {
                                                    status = "0";
                                                    folio = folioSignature.ToString();
                                                    msg = error;
                                                    flag2 = false;
                                                    num = 200;
                                                    this.objLog.write(ted);
                                                    tedbase64 = EncodeStrToBase64(ted);
                                                    this.objLog.writeDTESignature("folio "+ folio);
                                                    this.objLog.writeDTESignature("TED " + ted);

                                                    string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                    str = string.Concat(textArray5);
                                                }
                                                else
                                                {
                                                    flag2 = false;
                                                    status = "500";
                                                    msg = error;
                                                    num = 200;
                                                    string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                    str = string.Concat(textArray5);
                                                    this.objLog.write("ProcessEvent** Error: " + msg);
                                                    this.objLog.writeDTESignature("error " + error);
                                                }

                                                #endregion
                                            }
                                            catch (Exception e)
                                            {
                                                flag2 = false;
                                                num = 200;
                                                status = "555";
                                                this.objLog.write("EXCEPTION2");
                                                str = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                                                this.objLog.write("ProcessEvent *********** Error General -- " + e.Message);
                                                this.txtMensaje.Text = this.txtMensaje.Text + "\nExcepcion: " + e.Message;
                                            }
                                        }
                                        else if (posflag == "33")// FACTURA ELECTRONICA
                                        {
                                            //posflagtipo = "FACTURA ELECTRONICA";}
                                            try
                                            {
                                                #region PROCESO FACTURA ELECTRONICA
                                                #region Creacion de Cabecera
                                                this.objLog.write("INICIO  FACTURA ELECTRONICA");
                                                AgenteSignature agent = new AgenteSignature(this.ParamValues.DTESignature_Server, this.ParamValues.DTESignature_Port);
                                                SignatureDTE dte = new SignatureDTE("eFactura");

                                                //DOCUMENTO
                                                //Fecha de emision , vencimiento, indicador de servicio los carga por defecto al instanciar el objecto
                                                //si fuese necesario se pueden sobreescibir seteandolos con otro valor

                                                //EMISOR
                                                //EL emisor lo carga el agente Asyn de signature no es nesesario enviar datos del emisor

                                                //RECEPTOR
                                                dte.Args.Operation.IdDoc.FmaPago = 1;
                                                dte.Args.Operation.IdDoc.TipoDespacho = 1;

                                                DTEValidaciones(numdoccliente, "Rut Receptor", 10, 1);
                                                dte.Args.Operation.Receiver.RUTRecep = numdoccliente;
                                                DTEValidaciones(numdoccliente, "Rut Receptor", 10, 1);
                                                string nombrecliente = jsoncustomerRest.GetValue("first_name").ToString() + " " + jsoncustomerRest.GetValue("last_name").ToString();

                                                nombrecliente = nombrecliente.Trim();


                                                DTEValidaciones(nombrecliente, "Razon Social Receptor", 100, 3);
                                                dte.Args.Operation.Receiver.RznSocRecep = nombrecliente;


                                                string giro = jsoncustomerRest.GetValue("notes").ToString().Trim();
                                                if (giro == "")
                                                {
                                                    giro = this.ParamValues.DTEVyV_GiroClientedefault;
                                                }

                                                dte.Args.Operation.Receiver.GiroRecep = giro;
                                                DTEValidaciones(giro, "Giro Receptor", 40, 3);


                                                //Validaciones direcciones
                                                DTEValidaciones(jsoncustomerRest.GetValue("primary_address_line_1").ToString()+ " "+ jsoncustomerRest.GetValue("primary_address_line_2").ToString(), "Dirección Receptor", 70, 2);
                                                DTEValidaciones(jsoncustomerRest.GetValue("primary_address_line_4").ToString(), "Comuna Receptor", 20, 1);
                                                DTEValidaciones(jsoncustomerRest.GetValue("primary_address_line_5").ToString(), "Ciudad Receptor", 20, 1);

                                                //Direcciones
                                                dte.Args.Operation.Receiver.DirRecep = jsoncustomerRest.GetValue("primary_address_line_1").ToString() + " "+ jsoncustomerRest.GetValue("primary_address_line_2").ToString();
                                                dte.Args.Operation.Receiver.CmnaRecep = jsoncustomerRest.GetValue("primary_address_line_4").ToString();
                                                dte.Args.Operation.Receiver.CiudadRecep = jsoncustomerRest.GetValue("primary_address_line_5").ToString();

                                                //TOTALES
                                                this.objLog.write("TOTALES");
                                                //int neto = Int32.Parse(json2.GetValue("saletotalamt").ToString()) - Int32.Parse(json2.GetValue("saletotaltaxamt").ToString());

                                                Totals total = new Totals();
                                                //DTEValidaciones(this.ParamValues.DTESignature_TasaIVA, "Tasa IVA", 6, 3);
                                                total.TasaIVA = float.Parse(this.ParamValues.DTESignature_TasaIVA);
                                                //DTEValidaciones(json2.GetValue("saletotaltaxamt").ToString(), "IVA", 18, 3);
                                                total.IVA = json2["saletotaltaxamt"].Value<Int32>();// Int32.Parse(json2.GetValue("saletotaltaxamt").ToString());
                                                //DTEValidaciones(json2.GetValue("saletotalamt").ToString(), "Monto Total", 18, 3);
                                                total.MntTotal = json2["saletotalamt"].Value<Int32>();// Int32.Parse(json2.GetValue("saletotalamt").ToString());

                                                //DTEValidaciones(neto.ToString(), "Monto Neto", 18, 3);
                                                total.MntNeto = total.MntTotal - total.IVA;// neto;

                                                DTEValidaciones(total.TasaIVA.ToString(), "TasaIVA", 6, 3);
                                                DTEValidaciones(total.IVA.ToString(), "IVA", 18, 3);
                                                DTEValidaciones(total.MntTotal.ToString(), "MntTotal", 18, 3);
                                                DTEValidaciones(total.MntNeto.ToString(), "MntNeto", 18, 3);

                                                dte.Args.Operation.Totals = total;

                                                #endregion
                                                #region Creacion Detalle
                                                string strjson3 = json2.GetValue("docitem").ToString();
                                                JArray jsonArray = JArray.Parse(strjson3);
                                                this.objLog.write("Detalle");
                                                foreach (JObject jsonOperaciones in jsonArray.Children<JObject>())
                                                {
                                                    if (contadordetalle < 41)
                                                    {
                                                        string descuento = string.Empty;
                                                        Detalle det = new Detalle();

                                                        //det.NroLinDet = Int32.Parse((jsonOperaciones["itempos"]).ToString());
                                                        det.NroLinDet = contadordetalle + 1;
                                                        det.NmbItem = (jsonOperaciones["description1"]).ToString();
                                                        det.DscItem = (jsonOperaciones["description1"]).ToString();

                                                        //string cantidad = (jsonOperaciones["qty"]).ToString();
                                                        //cantidad = cantidad.Replace(',', '.');
                                                        //det.QtyItem = float.Parse(cantidad);

                                                        //string iva = (jsonOperaciones["taxamt"]).ToString().Split(',')[0];
                                                        //string precio = (jsonOperaciones["price"]).ToString();
                                                        //string precioori = (jsonOperaciones["origprice"]).ToString();
                                                        //int preciosiniva = (Int32.Parse(precioori) - Int32.Parse(iva));
                                                        //DTEValidaciones(preciosiniva.ToString(), "Precio Item", 19, 2);

                                                        //float montoitem = (float)((Int32.Parse(precio) * Convert.ToDouble(cantidad)) - (Int32.Parse(iva) * Convert.ToDouble(cantidad)));
                                                        //det.PrcItem = preciosiniva;
                                                        //det.MontoItem = Convert.ToInt32(montoitem);
                                                        //det.CdgItems.Add(new Cdgitem("INTERNO", (jsonOperaciones["alu"]).ToString()));
                                                        //DTEValidaciones(jsonOperaciones["alu"].ToString(), "Valor Código", 35, 1);

                                                        
                                                        det.QtyItem = jsonOperaciones["qty"].Value<float>();
                                                        if (this.ParamValues.DTESignature_PreciosConIva == "S")
                                                        {
                                                            det.PrcItem = jsonOperaciones["price"].Value<float>() - jsonOperaciones["taxamt"].Value<float>(); 
                                                        }
                                                        else
                                                        {
                                                            det.PrcItem = jsonOperaciones["price"].Value<float>();
                                                        }

                                                        
                                                        det.MontoItem = Convert.ToInt32(det.PrcItem* det.QtyItem);

                                                        det.CdgItems.Add(new Cdgitem("INTERNO", (jsonOperaciones["alu"]).ToString()));
                                                        DTEValidaciones(jsonOperaciones["alu"].ToString(), "Valor Código", 35, 1);

                                                        //try
                                                        //{
                                                        //    descuento = (jsonOperaciones["discamt"]).ToString();
                                                        //    if (descuento != string.Empty && descuento != "0")
                                                        //    {
                                                        //        det.DescuentoMonto = Int32.Parse(descuento) - ((Int32.Parse(descuento) * Int32.Parse(this.ParamValues.DTESignature_TasaIVA)) / 100);
                                                        //    }
                                                        //}
                                                        //catch
                                                        //{
                                                        //}
                                                        dte.Args.Operation.Detalles.Add(det);
                                                        contadordetalle = contadordetalle + 1;
                                                    }
                                                    else
                                                    {
                                                        msg = "No es posible superar los 40 Items";
                                                        throw new ApplicationException(msg);
                                                    }
                                                }


                                                #endregion,
                                                #region Creacion Referencia
                                                //NO existe referencia definida para email en Signature
                                                //string strjson4 = json2.GetValue("docitem").ToString(); //cambiar x referencia
                                                //JArray jsonArray1 = JArray.Parse(strjson4);
                                                //foreach (JObject jsonOperaciones in jsonArray1.Children<JObject>())
                                                //{
                                                //plantilla = new PlantillaXml();
                                                //plantref = plantref + this.plantilla.PlantillaRefFactura();
                                                //plantref = plantref.Replace("#NROLINREF#", "1");
                                                //plantref = plantref.Replace("#TPODOCREF#", "MTO");
                                                //plantref = plantref.Replace("#FOLIOREF#", folio);
                                                //plantref = plantref.Replace("#FCHREF#", DateTime.Now.ToString("yyyy-MM-dd"));
                                                //plantref = plantref.Replace("#RAZONREF#", json2.GetValue("btemail").ToString());
                                                //DTEValidaciones(json2.GetValue("btemail").ToString(), "Razon Referencia", 90, 2);
                                                //}

                                                #endregion
                                                #region Folio
                                                //No es necesario solicitar folio el agente le asiganara uno de los disponibles
                                                #endregion

                                                int folioSignature = 0;
                                                string error = "";
                                                this.objLog.writeDTESignature(dte.ToString());
                                                if (agent.Ejecutar(dte, out folioSignature, out ted, out error))
                                                {
                                                    status = "0";
                                                    folio = folioSignature.ToString();
                                                    msg = error;
                                                    flag2 = false;
                                                    num = 200;
                                                    this.objLog.write(ted);
                                                    tedbase64 = EncodeStrToBase64(ted);
                                                    this.objLog.writeDTESignature("folio " + folio);
                                                    this.objLog.writeDTESignature("TED " + ted);

                                                    string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                    str = string.Concat(textArray5);
                                                }
                                                else
                                                {
                                                    status = "500";
                                                    flag2 = false;
                                                    num = 200;
                                                    msg = error;
                                                    string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                    str = string.Concat(textArray5);
                                                    this.objLog.write("ProcessEvent** Error: " + msg);
                                                    this.objLog.writeDTESignature("error " + error);
                                                }
                                                #endregion
                                            }
                                            catch (Exception e)
                                            {
                                                flag2 = false;
                                                num = 200;
                                                status = "555";
                                                this.objLog.write("EXCEPTION2");
                                                str = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                                                this.objLog.write("ProcessEvent *********** Error General -- " + e.Message);
                                                this.txtMensaje.Text = this.txtMensaje.Text + "\nExcepcion: " + e.Message;
                                            }
                                        }
                                        else if (posflag == "61")// NOTA DE CREDITO ELECTRONICA
                                        {
                                            //posflagtipo = "NOTA DE CREDITO ELECTRONICA";
                                            try
                                            {
                                                #region PROCESO NOTA DE CREDITO ELECTRONICA
                                                this.objLog.write("INICIO NOTA CREDITO ELECTRONICA");
                                                AgenteSignature agent = new AgenteSignature(this.ParamValues.DTESignature_Server, this.ParamValues.DTESignature_Port);
                                                SignatureDTE dte = new SignatureDTE("encrd");

                                                //DOCUMENTO
                                                //Fecha de emision , vencimiento, indicador de servicio los carga por defecto al instanciar el objecto
                                                //si fuese necesario se pueden sobreescibir seteandolos con otro valor

                                                //EMISOR
                                                //EL emisor lo carga el agente Asyn de signature no es nesesario enviar datos del emisor

                                                //RECEPTOR
                                                dte.Args.Operation.IdDoc.FmaPago = 1;
                                                dte.Args.Operation.IdDoc.TipoDespacho = 1;

                                                this.objLog.write("RECEPTOR");
                                                dte.Args.Operation.Receiver.RUTRecep = numdoccliente;
                                                DTEValidaciones(numdoccliente, "Rut Receptor", 10, 1);
                                                string nombrecliente = jsoncustomerRest.GetValue("first_name").ToString() + " " + jsoncustomerRest.GetValue("last_name").ToString();
                                                nombrecliente = nombrecliente.Trim();

                                                DTEValidaciones(nombrecliente, "Razon Social Receptor", 100, 3);
                                                dte.Args.Operation.Receiver.RznSocRecep = nombrecliente;


                                                string giro = jsoncustomerRest.GetValue("notes").ToString();
                                                if (giro == "")
                                                {
                                                    DTEValidaciones(this.ParamValues.DTESignature_GiroClientedefault, "Giro Receptor", 40, 1);
                                                    dte.Args.Operation.Receiver.GiroRecep = this.ParamValues.DTESignature_GiroClientedefault;
                                                }

                                                dte.Args.Operation.Receiver.GiroRecep = giro;
                                                DTEValidaciones(giro, "Giro Receptor", 40, 3);

                                                this.objLog.write("DIRECCIONES");
                                                //Validaciones direcciones
                                                DTEValidaciones(jsoncustomerRest.GetValue("primary_address_line_1").ToString() + " "+ jsoncustomerRest.GetValue("primary_address_line_2").ToString(), "Dirección Receptor", 70, 2);
                                                DTEValidaciones(jsoncustomerRest.GetValue("primary_address_line_4").ToString(), "Comuna Receptor", 20, 1);
                                                DTEValidaciones(jsoncustomerRest.GetValue("primary_address_line_5").ToString(), "Ciudad Receptor", 20, 1);

                                                //Direcciones
                                                dte.Args.Operation.Receiver.DirRecep = jsoncustomerRest.GetValue("primary_address_line_1").ToString() + " " + jsoncustomerRest.GetValue("primary_address_line_2").ToString();
                                                dte.Args.Operation.Receiver.CmnaRecep = jsoncustomerRest.GetValue("primary_address_line_4").ToString();
                                                dte.Args.Operation.Receiver.CiudadRecep = jsoncustomerRest.GetValue("primary_address_line_5").ToString();

                                                //TOTALES

                                                //int neto = Int32.Parse(json2.GetValue("returnsubtotalwithtax").ToString()) - Int32.Parse(json2.GetValue("returntotaltaxamt").ToString());
                                                //plant = plant.Replace("#MNTNETO#", neto.ToString());
                                                // DTEValidaciones(neto.ToString(), "Monto Neto", 18, 3);
                                                //plant = plant.Replace("#MNTEXE#", "0");
                                                //plant = plant.Replace("#TASAIVA#", this.ParamValues.DTEVyV_TasaIVA);
                                                //DTEValidaciones(this.ParamValues.DTEVyV_TasaIVA, "Tasa IVA", 6, 3);
                                                //plant = plant.Replace("#IVA#", json2.GetValue("returntotaltaxamt").ToString());
                                                //DTEValidaciones(json2.GetValue("returntotaltaxamt").ToString(), "IVA", 18, 3);
                                                //plant = plant.Replace("#MNTTOTAL#", json2.GetValue("returnsubtotalwithtax").ToString());
                                                //DTEValidaciones(json2.GetValue("returnsubtotalwithtax").ToString(), "Monto Total", 18, 3);

                                                //TOTALES
                                                this.objLog.write("TOTALES");
                                                //int neto = Int32.Parse(json2.GetValue("returnsubtotalwithtax").ToString()) - Int32.Parse(json2.GetValue("returntotaltaxamt").ToString());
                                                //json2["returntotaltaxamt"].Value<Int32>()

                                                //int neto = json2["returnsubtotalwithtax"].Value<Int32>() - json2["returntotaltaxamt"].Value<Int32>();
                                                //Totals total = new Totals();
                                                //DTEValidaciones(this.ParamValues.DTESignature_TasaIVA, "Tasa IVA", 6, 3);
                                                //total.TasaIVA = float.Parse(this.ParamValues.DTESignature_TasaIVA);
                                                //DTEValidaciones(json2.GetValue("returntotaltaxamt").ToString(), "IVA", 18, 3);
                                                //total.IVA = json2["returntotaltaxamt"].Value<Int32>();// Int32.Parse(json2.GetValue("returntotaltaxamt").ToString());
                                                //DTEValidaciones(json2.GetValue("returnsubtotalwithtax").ToString(), "Monto Total", 18, 3);
                                                //total.MntTotal = json2["returnsubtotalwithtax"].Value<Int32>();// Int32.Parse(json2.GetValue("returnsubtotalwithtax").ToString());

                                                //DTEValidaciones(neto.ToString(), "Monto Neto", 18, 3);
                                                //total.MntNeto = neto;

                                                //int neto = json2["returnsubtotalwithtax"].Value<Int32>() - json2["returntotaltaxamt"].Value<Int32>();
                                                Totals total = new Totals();
                                                
                                                total.TasaIVA = float.Parse(this.ParamValues.DTESignature_TasaIVA);
                                                total.IVA = json2["returntotaltaxamt"].Value<Int32>();// Int32.Parse(json2.GetValue("returntotaltaxamt").ToString());
                                                total.MntTotal = json2["returnsubtotalwithtax"].Value<Int32>();// Int32.Parse(json2.GetValue("returnsubtotalwithtax").ToString());
                                                total.MntNeto = total.MntTotal- total.IVA;

                                                DTEValidaciones(total.TasaIVA.ToString(), "TasaIVA", 6, 3);
                                                DTEValidaciones(total.IVA.ToString(), "IVA", 18, 3);
                                                DTEValidaciones(total.MntTotal.ToString(), "MntTotal", 18, 3);
                                                DTEValidaciones(total.MntNeto.ToString(), "MntNeto", 18, 3);

                                                dte.Args.Operation.Totals = total;

                                                #endregion
                                                #region Creacion Detalle
                                                this.objLog.write("DETALLE");
                                                string strjson3 = json2.GetValue("docitem").ToString();
                                                JArray jsonArray = JArray.Parse(strjson3);
                                                foreach (JObject jsonOperaciones in jsonArray.Children<JObject>())
                                                {
                                                    if (contadordetalle < 41)
                                                    {
                                                        string descuento = string.Empty;
                                                        Detalle det = new Detalle();

                                                        float cantidad = jsonOperaciones["qty"].Value<float>();// (jsonOperaciones["qty"]).ToString();
                                                        det.QtyItem = cantidad;

                                                        det.NroLinDet = contadordetalle + 1;
                                                        det.NmbItem = (jsonOperaciones["description1"]).ToString();
                                                        det.DscItem = (jsonOperaciones["description1"]).ToString();

                                                        //float iva = jsonOperaciones["taxamt"].Value<float>();// (jsonOperaciones["taxamt"]).ToString().Split(',')[0];
                                                        //float precio = jsonOperaciones["price"].Value<float>();// (jsonOperaciones["price"]).ToString();
                                                        //float precioori = jsonOperaciones["origprice"].Value<float>();// (jsonOperaciones["origprice"]).ToString();
                                                        //float preciosiniva = (precioori - iva);

                                                        //DTEValidaciones(preciosiniva.ToString(), "Precio Item", 19, 2);
                                                        //"qty": 1,
                                                        //"origprice": 2513,
                                                        //"origtaxamt": 477.47,
                                                        //"price": 2513,
                                                        //"taxperc": 19,
                                                        //"taxamt": 477.47,
                                                        //float montoitem = (float)((Int32.Parse(precio) * Convert.ToDouble(cantidad)) - (Int32.Parse(iva) * Convert.ToDouble(cantidad)));
                                                        //float montoitem = (precio * cantidad);// - (iva * cantidad);
                                                        //Unitario
                                                        if (this.ParamValues.DTESignature_PreciosConIva == "S")
                                                        {
                                                            det.PrcItem = jsonOperaciones["price"].Value<float>() - jsonOperaciones["taxamt"].Value<float>();// preciosiniva; 
                                                        }
                                                        else
                                                        {
                                                            det.PrcItem = jsonOperaciones["price"].Value<float>();
                                                        }

                                                        
                                                        //Extendido
                                                        det.MontoItem = Convert.ToInt32(det.PrcItem * det.QtyItem);

                                                        det.CdgItems.Add(new Cdgitem("INTERNO", (jsonOperaciones["alu"]).ToString()));
                                                        DTEValidaciones(jsonOperaciones["alu"].ToString(), "Valor Código", 35, 1);

                                                        //try
                                                        //{
                                                        //    descuento = (jsonOperaciones["discamt"]).ToString();
                                                        //    if (descuento != string.Empty && descuento != "0")
                                                        //    {
                                                        //        float fDescuento = jsonOperaciones["discamt"].Value<float>();
                                                        //        //det.DescuentoMonto = Int32.Parse(descuento) - ((Int32.Parse(descuento) * Int32.Parse(this.ParamValues.DTESignature_TasaIVA)) / 100);
                                                        //        det.DescuentoMonto = Convert.ToInt32(fDescuento - (fDescuento * float.Parse(this.ParamValues.DTESignature_TasaIVA) / 100));
                                                        //    }
                                                        //}
                                                        //catch
                                                        //{
                                                        //}

                                                        dte.Args.Operation.Detalles.Add(det);
                                                        contadordetalle = contadordetalle + 1;
                                                    }
                                                    else
                                                    {
                                                        msg = "No es posible superar los 40 Items";
                                                        throw new ApplicationException(msg);
                                                    }
                                                }

                                                #endregion
                                                #region Creacion Referencia
                                                //this.objLog.write("REFERENCIA");
                                                String idreferencia = json2.GetValue("refsalesid").ToString();
                                                JObject jsonref = JObject.Parse(this.GETDocument(idreferencia, token));
                                                JObject jsonreferencia = JObject.Parse(jsonref.GetValue("data")[0].ToString());
                                                string posflagref = jsonreferencia.GetValue("posflag1").ToString().Trim().Substring(0, 2);
                                                DateTime fecharef = DateTime.Parse(jsonreferencia.GetValue("modifieddatetime").ToString());
                                                string fecharefstr = String.Format("{0:yyyy-MM-dd}", fecharef);
                                                Referencia referencia = new Referencia();
                                                referencia.NroLinRef = 1;
                                                referencia.TpoDocRef = posflagref;
                                                referencia.FolioRef = jsonreferencia.GetValue("docno").ToString();
                                                referencia.RazonRef = "1";
                                                referencia.FchRef = fecharefstr;
                                                referencia.CodRef = 1;
                                                dte.Args.Operation.Referencias.Add(referencia);

                                                #endregion
                                                #region Folio
                                                //No es necesario solicitar folio el agente le asiganara uno de los disponibles
                                                #endregion

                                                int folioSignature = 0;
                                                string error = "";
                                                this.objLog.write("EJECUTAR");
                                                this.objLog.writeDTESignature(dte.ToString());

                                                if (agent.Ejecutar(dte, out folioSignature, out ted, out error))
                                                {
                                                    status = "0";
                                                    folio = folioSignature.ToString();
                                                    msg = error;
                                                    flag2 = false;
                                                    num = 200;
                                                    this.objLog.write(ted);
                                                    tedbase64 = EncodeStrToBase64(ted);
                                                    this.objLog.writeDTESignature("folio " + folio);
                                                    this.objLog.writeDTESignature("TED " + ted);

                                                    string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                    str = string.Concat(textArray5);
                                                }
                                                else
                                                {
                                                    status = "500";
                                                    flag2 = false;
                                                    num = 200;
                                                    msg = error;
                                                    string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                    str = string.Concat(textArray5);
                                                    this.objLog.write("ProcessEvent** Error: " + msg);
                                                    this.objLog.writeDTESignature("error " + error);
                                                }
                                                #endregion
                                            }
                                            catch (Exception e)
                                            {
                                                flag2 = false;
                                                num = 200;
                                                status = "555";
                                                this.objLog.write("EXCEPTION2 NC SIGNATURE");
                                                str = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";

                                                String msgErrorOut = e.Message;
                                                if (e.InnerException != null)
                                                    if (e.InnerException.Message != null)
                                                    {
                                                        msgErrorOut += e.InnerException.Message;
                                                        if (e.InnerException.InnerException != null)
                                                        {
                                                            if (e.InnerException.InnerException.Message != null)
                                                                msgErrorOut += e.InnerException.Message;
                                                        }
                                                    }

                                                this.objLog.write("ProcessEvent *********** Error General -- " + msgErrorOut);
                                                this.txtMensaje.Text = this.txtMensaje.Text + "\nExcepcion: " + e.Message;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        flag2 = false;
                                        num = 200;
                                        status = "556";
                                        string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + errors + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                        str = string.Concat(textArray5);
                                        this.objLog.write("ProcessEvent** Error: " + errors);
                                    }
                                }
                            }
                        }
                       // this.objLog.write("SALIMOS");
                    }
                    if (((ADirection == "FromClient") && (AHTTPVerb == "POST")) && (AResourceName.ToUpper() == "CALLDTESIGNATURETRANSFER"))
                    {
                        //this.objLog.write("ENTRAMOS");

                        str18 = "";
                        str19 = "";
                        str20 = "";
                        str21 = "";
                        string errors = string.Empty;
                        string folio = string.Empty;
                        //string plant = string.Empty;
                        //string plantdet = string.Empty;
                        string status = string.Empty;
                        string msg = string.Empty;
                        string xmlres = string.Empty;
                        string ted = string.Empty;
                        string tedbase64 = string.Empty;


                        foreach (JObject obj3 in JArray.Parse("[" + message.Payload + "]").Children<JObject>())
                        {
                            foreach (JProperty property in obj3.Properties())
                            {
                                if (property.Name == "documentID")
                                {
                                    token = this.doPRISMLoginAux(this.objLog);
                                    str21 = (string)property.Value;
                                    JObject json = JObject.Parse(this.GETTransfer(str21, token));
                                    errors = json.GetValue("errors").ToString();
                                    if (errors == "")
                                    {
                                        JObject json2 = JObject.Parse(json.GetValue("data")[0].ToString());
                                        string posflag = "52";
                                        try
                                        {
                                            //plantilla = new PlantillaXml();
                                            //plant = this.plantilla.PlantillaCabGuiaDespacho();
                                            #region Creacion de Cabecera
                                            //DOCUMENTO
                                            AgenteSignature agent = new AgenteSignature(this.ParamValues.DTESignature_Server, this.ParamValues.DTESignature_Port);
                                            SignatureDTE dte = new SignatureDTE("52");

                                            //DOCUMENTO
                                            if (this.ParamValues.DTESignature_PreciosConIva == "S")
                                            {
                                                //dte.Args.Operation.IdDoc.MntBruto = true;
                                            }
                                            else
                                            {
                                                dte.Args.Operation.IdDoc.MntBruto = true;
                                            }

                                            dte.Args.Operation.IdDoc.IndTraslado = 5;
                                            //Fecha de emision , vencimiento, indicador de servicio los carga por defecto al instanciar el objecto
                                            //si fuese necesario se pueden sobreescibir seteandolos con otro valor

                                            //EMISOR
                                            //EL emisor lo carga el agente Asyn de signature no es nesesario enviar datos del emisor

                                            //RECEPTOR

                                            dte.Args.Operation.Receiver.RUTRecep = json2.GetValue("origstorezip").ToString().ToUpper();
                                            DTEValidaciones(dte.Args.Operation.Receiver.RUTRecep, "Rut Receptor", 10, 1);

                                            dte.Args.Operation.Receiver.RznSocRecep = json2.GetValue("instoreudf1string").ToString();
                                            DTEValidaciones(dte.Args.Operation.Receiver.RznSocRecep, "Razon Social Receptor", 100, 1);

                                            dte.Args.Operation.Receiver.GiroRecep = (json2.GetValue("instoreudf2string").ToString() + " " + json2.GetValue("instoreudf3string").ToString()).Trim();
                                            DTEValidaciones(dte.Args.Operation.Receiver.GiroRecep, "Giro Receptor", 40, 1);

                                            dte.Args.Operation.Receiver.DirRecep = json2.GetValue("instoreaddress1").ToString();
                                            DTEValidaciones(dte.Args.Operation.Receiver.DirRecep, "Dirección Receptor", 70, 2);


                                            dte.Args.Operation.Receiver.CmnaRecep = json2.GetValue("instoreaddress2").ToString();
                                            DTEValidaciones(dte.Args.Operation.Receiver.CmnaRecep, "Comuna Receptor", 20, 1);

                                            dte.Args.Operation.Receiver.CiudadRecep = json2.GetValue("instoreaddress3").ToString();
                                            DTEValidaciones(dte.Args.Operation.Receiver.CiudadRecep, "Ciudad Receptor", 20, 1);

                                            //plant = plant.Replace("#DirPostal#", "");
                                            //plant = plant.Replace("#CmnaPostal#", "");
                                            //plant = plant.Replace("#CiudadPostal#", "");
                                            dte.Args.Operation.Transporte = new Transporte();
                                            dte.Args.Operation.Transporte.DirDest = json2.GetValue("instoreaddress1").ToString();
                                            dte.Args.Operation.Transporte.CmnaDest = json2.GetValue("instoreaddress2").ToString();
                                            dte.Args.Operation.Transporte.CiudadDest = json2.GetValue("instoreaddress3").ToString();

                                            //TOTALES
                                            //int iva = (Int32.Parse(json2.GetValue("docpricetotal").ToString()) * Int32.Parse(this.ParamValues.DTESignature_TasaIVA)) / 100;
                                            //int neto = Int32.Parse(json2.GetValue("docpricetotal").ToString()) - iva;
                                            int iva = (json2["docpricetotal"].Value<Int32>() * Int32.Parse(this.ParamValues.DTESignature_TasaIVA)) / 100;
                                            int neto = json2["docpricetotal"].Value<Int32>() - iva;
                                            

                                            Totals total = new Totals();
                                            DTEValidaciones(this.ParamValues.DTESignature_TasaIVA, "Tasa IVA", 6, 3);
                                            total.TasaIVA = float.Parse(this.ParamValues.DTESignature_TasaIVA);

                                            total.IVA = iva;
                                            DTEValidaciones(json2.GetValue("docpricetotal").ToString(), "Monto Total", 18, 3);
                                            total.MntTotal = json2["docpricetotal"].Value<Int32>();// Int32.Parse(json2.GetValue("docpricetotal").ToString());

                                            DTEValidaciones(neto.ToString(), "Monto Neto", 18, 3);
                                            total.MntNeto = neto;

                                            dte.Args.Operation.Totals = total;

                                            #endregion
                                            #region Creacion  Detalle
                                            string jsonitem = json2.GetValue("slipitem").ToString();
                                            JArray jsonArrayitem = JArray.Parse(jsonitem);
                                            foreach (JObject jsonOperaciones in jsonArrayitem.Children<JObject>())
                                            {
                                                if (contadordetalle < 41)
                                                {


                                                    string descuento = string.Empty;
                                                    Detalle det = new Detalle();

                                                    //string cantidad = (jsonOperaciones["qty"]).ToString();
                                                    //string precio = (jsonOperaciones["price"]).ToString();
                                                    float ivadet = 0;
                                                    if (this.ParamValues.DTESignature_PreciosConIva == "S")
                                                    {
                                                        ivadet = (Int32.Parse((jsonOperaciones["price"]).ToString()) * Int32.Parse(this.ParamValues.DTESignature_TasaIVA)) / 100;
                                                    }
                                                    else
                                                    {
                                                       // float ivadet = (Int32.Parse((jsonOperaciones["price"]).ToString()) * Int32.Parse(this.ParamValues.DTESignature_TasaIVA)) / 100;
                                                    }
                                                   
                                                    //float montoitem = (float)((Int32.Parse(precio) * Convert.ToDouble(cantidad)) - (ivadet * Convert.ToDouble(cantidad)));
                                                    //float preciosiniva = (Int32.Parse(precio) - ivadet);
                                                    //cantidad = cantidad.Replace(',', '.');
                                                    //det.NroLinDet = contadordetalle + 1;
                                                    //det.NmbItem = (jsonOperaciones["description1"]).ToString();
                                                    //det.DscItem = (jsonOperaciones["description1"]).ToString();

                                                    //det.QtyItem = float.Parse(cantidad);

                                                    //det.PrcItem = preciosiniva;
                                                    //det.MontoItem = Convert.ToInt32(montoitem);
                                                    //det.CdgItems.Add(new Cdgitem("INTERNO", (jsonOperaciones["alu"]).ToString()));
                                                    //DTEValidaciones(jsonOperaciones["alu"].ToString(), "Valor Código", 35, 1);

                                                    det.NroLinDet = contadordetalle + 1;
                                                    det.NmbItem = (jsonOperaciones["description1"]).ToString();
                                                    det.DscItem = (jsonOperaciones["description1"]).ToString();

                                                    if (this.ParamValues.DTESignature_PreciosConIva == "S")
                                                    {
                                                        //this.objLog.write("ivadet: " + ivadet.ToString());
                                                        det.QtyItem = jsonOperaciones["qty"].Value<float>();
                                                        det.PrcItem = jsonOperaciones["price"].Value<float>() - ivadet; //float ivadet = (Int32.Parse((jsonOperaciones["price"]).ToString()) * Int32.Parse(this.ParamValues.DTESignature_TasaIVA)) / 100;

                                                    }
                                                    else
                                                    {
                                                        det.QtyItem = jsonOperaciones["qty"].Value<float>();
                                                        det.PrcItem = jsonOperaciones["price"].Value<float>();
                                                    }



                                                    det.MontoItem = Convert.ToInt32(det.QtyItem * det.PrcItem);
                                                    det.CdgItems.Add(new Cdgitem("INTERNO", (jsonOperaciones["alu"]).ToString()));
                                                    DTEValidaciones(jsonOperaciones["alu"].ToString(), "Valor Código", 35, 1);

                                                    dte.Args.Operation.Detalles.Add(det);
                                                    contadordetalle = contadordetalle + 1;
                                                }
                                                else
                                                {
                                                    msg = "No es posible superar los 40 Items";
                                                    throw new ApplicationException(msg);
                                                }

                                            }

                                            #endregion
                                            #region Folio
                                            //No es necesario solicitar folio el agente le asiganara uno de los disponibles
                                            #endregion

                                            int folioSignature = 0;
                                            string error = "";
                                            this.objLog.write("EJECUTAR");
                                            this.objLog.write(dte.ToString());
                                            if (agent.Ejecutar(dte, out folioSignature, out ted, out error))
                                            {
                                                status = "0";
                                                folio = folioSignature.ToString();
                                                msg = error;
                                                flag2 = false;
                                                num = 200;
                                                this.objLog.write(ted);
                                                tedbase64 = EncodeStrToBase64(ted);

                                                string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                str = string.Concat(textArray5);
                                            }
                                            else
                                            {
                                                status = "500";
                                                flag2 = false;
                                                msg = error;
                                                num = 200;
                                                string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                str = string.Concat(textArray5);
                                                this.objLog.write("ProcessEvent** Error: " +  msg);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            flag2 = false;
                                            num = 200;
                                            status = "555";
                                            this.objLog.write("EXCEPTION2");
                                            str = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                                            this.objLog.write("ProcessEvent *********** Error General -- " + e.Message);
                                            this.txtMensaje.Text = this.txtMensaje.Text + "\nExcepcion: " + e.Message;
                                        }
                                    }
                                    else
                                    {
                                        flag2 = false;
                                        num = 200;
                                        status = "556";
                                        string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + errors + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                        str = string.Concat(textArray5);
                                        this.objLog.write("ProcessEvent** Error: " + errors);
                                    }
                                }
                            }
                        }
                       // this.objLog.write("SALIMOS");
                    }
                    if (((ADirection == "FromClient") && (AHTTPVerb == "POST")) && (AResourceName.ToUpper() == "CALLDTESIGNATUREVOUCHER"))
                    {
                        str18 = "";
                        str19 = "";
                        str20 = "";
                        str21 = "";
                        string errors = string.Empty;
                        string folio = string.Empty;
                        //string plant = string.Empty;
                        //string plantdet = string.Empty;
                        string status = string.Empty;
                        string msg = string.Empty;
                        string xmlres = string.Empty;
                        string ted = string.Empty;
                        string tedbase64 = string.Empty;

                        foreach (JObject obj3 in JArray.Parse("[" + message.Payload + "]").Children<JObject>())
                        {
                            foreach (JProperty property in obj3.Properties())
                            {
                                if (property.Name == "documentID")
                                {
                                    token = this.doPRISMLoginAux(this.objLog);
                                    str21 = (string)property.Value;
                                    JObject json = JObject.Parse(this.GETReceive(str21, token));
                                    errors = json.GetValue("errors").ToString();
                                    if (errors == "")
                                    {
                                        JObject json2 = JObject.Parse(json.GetValue("data")[0].ToString());
                                        string posflag = "52";
                                        try
                                        {
                                            #region Creacion de XML Cabecera
                                            //DOCUMENTO
                                            //DOCUMENTO
                                            AgenteSignature agent = new AgenteSignature(this.ParamValues.DTESignature_Server, this.ParamValues.DTESignature_Port);
                                            SignatureDTE dte = new SignatureDTE("52");

                                            //DOCUMENTO
                                            //dte.Args.Operation.IdDoc.MntBruto = true;
                                            dte.Args.Operation.IdDoc.IndTraslado = 7;
                                            //Fecha de emision , vencimiento, indicador de servicio los carga por defecto al instanciar el objecto
                                            //si fuese necesario se pueden sobreescibir seteandolos con otro valor

                                            //EMISOR
                                            //EL emisor lo carga el agente Asyn de signature no es nesesario enviar datos del emisor

                                            //RECEPTOR

                                            dte.Args.Operation.Receiver.RUTRecep = json2.GetValue("vendorpostalcode").ToString().ToUpper();
                                            DTEValidaciones(dte.Args.Operation.Receiver.RUTRecep, "Rut Receptor", 10, 1);

                                            dte.Args.Operation.Receiver.RznSocRecep = json2.GetValue("vendoraddress4").ToString();
                                            DTEValidaciones(dte.Args.Operation.Receiver.RznSocRecep, "Razon Social Receptor", 100, 1);

                                            dte.Args.Operation.Receiver.GiroRecep = (json2.GetValue("vendoraddress5").ToString() + " " + json2.GetValue("vendoraddress6").ToString()).Trim();
                                            DTEValidaciones(dte.Args.Operation.Receiver.GiroRecep, "Giro Receptor", 40, 1);

                                            dte.Args.Operation.Receiver.DirRecep = json2.GetValue("vendoraddress1").ToString();
                                            DTEValidaciones(dte.Args.Operation.Receiver.DirRecep, "Dirección Receptor", 70, 2);


                                            dte.Args.Operation.Receiver.CmnaRecep = json2.GetValue("vendoraddress2").ToString();
                                            DTEValidaciones(dte.Args.Operation.Receiver.CmnaRecep, "Comuna Receptor", 20, 1);

                                            dte.Args.Operation.Receiver.CiudadRecep = json2.GetValue("vendoraddress3").ToString();
                                            DTEValidaciones(dte.Args.Operation.Receiver.CiudadRecep, "Ciudad Receptor", 20, 1);

                                            dte.Args.Operation.Transporte = new Transporte();
                                            dte.Args.Operation.Transporte.DirDest = json2.GetValue("vendoraddress1").ToString();
                                            dte.Args.Operation.Transporte.CmnaDest = json2.GetValue("vendoraddress2").ToString();
                                            dte.Args.Operation.Transporte.CiudadDest = json2.GetValue("vendoraddress3").ToString();
                                            #endregion
                                            #region Creacion Xml Detalle
                                            string jsonitem = json2.GetValue("recvitem").ToString();
                                            JArray jsonArrayitem = JArray.Parse(jsonitem);
                                            int valortotal = 0;
                                            foreach (JObject jsonOperaciones in jsonArrayitem.Children<JObject>())
                                            {
                                                if (contadordetalle < 41)
                                                {
                                                    string descuento = string.Empty;
                                                    Detalle det = new Detalle();

                                                    //string cantidad = (jsonOperaciones["qty"]).ToString();
                                                    //string precio = (jsonOperaciones["price"]).ToString();
                                                    //valortotal = valortotal + Int32.Parse((jsonOperaciones["price"]).ToString());
                                                    //float ivadet = (Int32.Parse((jsonOperaciones["price"]).ToString()) * Int32.Parse(this.ParamValues.DTESignature_TasaIVA)) / 100;
                                                    //float montoitem = (float)((Int32.Parse(precio) * Convert.ToDouble(cantidad)) - (ivadet * Convert.ToDouble(cantidad)));
                                                    //float preciosiniva = (Int32.Parse(precio) - ivadet);
                                                    //cantidad = cantidad.Replace(',', '.');
                                                    //det.NroLinDet = contadordetalle + 1;
                                                    //det.NmbItem = (jsonOperaciones["description1"]).ToString();
                                                    //det.DscItem = (jsonOperaciones["description1"]).ToString();

                                                    //det.QtyItem = float.Parse(cantidad);

                                                    //det.PrcItem = preciosiniva;
                                                    //det.MontoItem = Convert.ToInt32(montoitem);
                                                    //det.CdgItems.Add(new Cdgitem("INTERNO", (jsonOperaciones["alu"]).ToString()));
                                                    //DTEValidaciones(jsonOperaciones["alu"].ToString(), "Valor Código", 35, 1);
                                                                                               
                                                    det.NroLinDet = contadordetalle + 1;
                                                    det.NmbItem = (jsonOperaciones["description1"]).ToString();
                                                    det.DscItem = (jsonOperaciones["description1"]).ToString();
                                                    det.QtyItem = jsonOperaciones["qty"].Value<float>();

                                                    if (this.ParamValues.DTESignature_PreciosConIva == "S")
                                                    {
                                                        float ivadet = (Int32.Parse((jsonOperaciones["price"]).ToString()) * Int32.Parse(this.ParamValues.DTESignature_TasaIVA)) / 100;
                                                        det.PrcItem = jsonOperaciones["price"].Value<float>() - ivadet; ;
                                                    }
                                                    else
                                                    {
                                                        det.PrcItem = jsonOperaciones["price"].Value<float>();
                                                    }


                                                    det.MontoItem = Convert.ToInt32(det.QtyItem * det.PrcItem);
                                                    det.CdgItems.Add(new Cdgitem("INTERNO", (jsonOperaciones["alu"]).ToString()));
                                                    DTEValidaciones(jsonOperaciones["alu"].ToString(), "Valor Código", 35, 1);
                                                    dte.Args.Operation.Detalles.Add(det);
                                                    valortotal += det.MontoItem;
                                                    contadordetalle = contadordetalle + 1;
                                                }
                                                else
                                                {
                                                    msg = "No es posible superar los 40 Items";
                                                    throw new ApplicationException(msg);
                                                }
                                            }


                                            //TOTALES
                                            //int iva = (valortotal * Int32.Parse(this.ParamValues.DTEVyV_TasaIVA)) / 100;
                                            //int neto = valortotal - iva;

                                            //Totals total = new Totals();
                                            //DTEValidaciones(this.ParamValues.DTESignature_TasaIVA, "Tasa IVA", 6, 3);
                                            //total.TasaIVA = float.Parse(this.ParamValues.DTESignature_TasaIVA);

                                            //total.IVA = iva;
                                            //DTEValidaciones(json2.GetValue("docpricetotal").ToString(), "Monto Total", 18, 3);
                                            //total.MntTotal = Int32.Parse(valortotal.ToString());

                                            //DTEValidaciones(neto.ToString(), "Monto Neto", 18, 3);
                                            //total.MntNeto = neto;


                                            int iva = (valortotal * Int32.Parse(this.ParamValues.DTEVyV_TasaIVA)) / 100;
                                            int neto = valortotal - iva;

                                            Totals total = new Totals();
                                            DTEValidaciones(this.ParamValues.DTESignature_TasaIVA, "Tasa IVA", 6, 3);
                                            total.TasaIVA = float.Parse(this.ParamValues.DTESignature_TasaIVA);

                                            total.IVA = iva;
                                            DTEValidaciones(valortotal.ToString(), "Monto Total", 18, 3);
                                            total.MntTotal = Int32.Parse(valortotal.ToString());

                                            DTEValidaciones(neto.ToString(), "Monto Neto", 18, 3);
                                            total.MntNeto = neto;

                                            dte.Args.Operation.Totals = total;

                                            #endregion
                                            #region Folio
                                            //No es necesario solicitar folio el agente le asiganara uno de los disponibles
                                            #endregion

                                            int folioSignature = 0;
                                            string error = "";
                                            this.objLog.write(dte.ToString());
                                            if (agent.Ejecutar(dte, out folioSignature, out ted, out error))
                                            {
                                                status = "0";
                                                folio = folioSignature.ToString();
                                                msg = error;
                                                flag2 = false;
                                                num = 200;
                                                this.objLog.write(ted);
                                                tedbase64 = EncodeStrToBase64(ted);

                                                string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                str = string.Concat(textArray5);
                                            }
                                            else
                                            {
                                                status = "500";
                                                flag2 = false;
                                                num = 200;
                                                msg = error;
                                                string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                str = string.Concat(textArray5);
                                                this.objLog.write("ProcessEvent** Error: " + msg);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            flag2 = false;
                                            num = 200;
                                            status = "555";
                                            this.objLog.write("EXCEPTION2");
                                            str = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                                            this.objLog.write("ProcessEvent *********** Error General -- " + e.Message);
                                            this.txtMensaje.Text = this.txtMensaje.Text + "\nExcepcion: " + e.Message;
                                        }
                                    }
                                    else
                                    {
                                        flag2 = false;
                                        num = 200;
                                        status = "556";
                                        string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + errors + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                        str = string.Concat(textArray5);
                                        this.objLog.write("ProcessEvent** Error: " + errors);
                                    }
                                }
                            }
                        }
                        this.objLog.write("SALIMOS");
                    }
                    if (((ADirection == "FromClient") && (AHTTPVerb == "POST")) && (AResourceName.ToUpper() == "CALLDTEDBNET"))
                    {
                        this.objLog.write("INICIO  CALLDTEDBNET");
                        str21 = "";
                        string validacion = string.Empty;
                        string errors = string.Empty;
                        string numdoccliente = string.Empty;
                        string folio = string.Empty;
                        string status = string.Empty;
                        string msg = string.Empty;
                        string posflagtipo = string.Empty;
                        string xmlres = string.Empty;
                        string ted = string.Empty;
                        string tedbase64 = string.Empty;
                       // this.objLog.write("ImpresoraOPOS resImpresion");
                        ImpresoraOPOS resImpresion = new ImpresoraOPOS();
                       // this.objLog.write("JObject obj3 in");
                        foreach (JObject obj3 in JArray.Parse("[" + message.Payload + "]").Children<JObject>())
                        {
                            foreach (JProperty property in obj3.Properties())
                            {
                                if (property.Name == "documentID")
                                {
                                    this.objLog.write("property.Name == documentID se procesa requerimiento");

                                    token = this.doPRISMLoginAux(this.objLog);
                                    str21 = (string)property.Value;
                                    this.objLog.write("documentID: " + str21);
                                    if (string.IsNullOrEmpty(token)) this.objLog.write("errors: al obtener Token, vacio o nulo");

                                    JObject json = JObject.Parse(this.GETDocument(str21, token));
                                    //this.objLog.write("json: " + json);
                                    errors = json.GetValue("errors").ToString();
                                    this.objLog.write("errors: " + errors);
                                    if (errors == "")
                                    {
                                        this.objLog.write("INICIO  Documento");

                                        JObject json2 = JObject.Parse(json.GetValue("data")[0].ToString());
                                        //this.objLog.write(json.GetValue("data")[0].ToString());
                                        string posflag = Convert.ToString(json2.GetValue("posflag1"));
                                        if ((!string.IsNullOrEmpty(posflag)) && (posflag.Length >= 2))
                                        {
                                            posflag = posflag.Trim().Substring(0, 2);
                                        }



                                        string idcliente = Convert.ToString(json2.GetValue("btcuid"));
                                        this.objLog.write("Cargar cliente "+ idcliente);
                                        JObject jsoncustomer = null;
                                        JObject jsoncustomer2 = null;
                                        JArray jsoncustomerRest1 = null;
                                        JObject jsoncustomerRest = null;
                                        if (!string.IsNullOrEmpty(idcliente))
                                        {
                                            jsoncustomer = JObject.Parse(this.GETCustomer(idcliente, token));
                                            jsoncustomer2 = JObject.Parse(jsoncustomer.GetValue("data")[0].ToString());
                                        }

                                        jsoncustomerRest1 = JArray.Parse(this.GETCustomerRest(idcliente, token));
                                        jsoncustomerRest = JObject.Parse(jsoncustomerRest1[0].ToString());



                                        if ((posflag == "39") || (posflag == "33") || (posflag == "61"))
                                        {
                                            JObject jsonreferencia = null;
                                            DbNetDTE dbNetDTE = new DbNetDTE(this.ParamValues);

                                            if (posflag == "61")
                                            {
                                                String idreferencia = json2.GetValue("refsalesid").ToString();
                                                JObject jsonref = JObject.Parse(this.GETDocument(idreferencia, token));
                                                jsonreferencia = JObject.Parse(jsonref.GetValue("data")[0].ToString());
                                            }

                                            this.objLog.write("CreaDocumentoElectronico");
                                            this.objLog.write("jsoncustomerRest "+ jsoncustomerRest);
                                            /*Esta linea debe ser comentada una vez que se finalice la regeneracion de ventas de SILFA*/
                                            decimal id_reinyeccion = Convert.ToDecimal(str21) - 50000000000000000;
                                            str21 = id_reinyeccion.ToString();
                                            this.objLog.write("SID de regeneracion " + str21);
                                            dbNetDTE.CreaDocumentoElectronico(json2, str21, message, jsoncustomerRest, jsonreferencia, out str);
                                            this.objLog.write("pase CreaDocumentoElectronico "+ str);
                                            flag2 = false;
                                            num = 200;
                                        }

                                    }
                                    else
                                    {
                                        flag2 = false;
                                        num = 200;
                                        status = "556";
                                        string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + errors + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                        str = string.Concat(textArray5);
                                        this.objLog.write("ProcessEvent** Error: " + errors);
                                    }
                                }
                            }
                        }
                     //   this.objLog.write("SALIMOS");
                    }
                    if (((ADirection == "FromClient") && (AHTTPVerb == "POST")) && (AResourceName.ToUpper() == "CALLDTEDBNETTRANSFER"))
                    {
                      //  this.objLog.write("ENTRAMOS");

                        str18 = "";
                        str19 = "";
                        str20 = "";
                        str21 = "";
                        string errors = string.Empty;
                        string folio = string.Empty;
                        string status = string.Empty;
                        string msg = string.Empty;
                        string xmlres = string.Empty;
                        string ted = string.Empty;
                        string tedbase64 = string.Empty;


                        foreach (JObject obj3 in JArray.Parse("[" + message.Payload + "]").Children<JObject>())
                        {
                            foreach (JProperty property in obj3.Properties())
                            {
                                if (property.Name == "documentID")
                                {
                                    token = this.doPRISMLoginAux(this.objLog);
                                    str21 = (string)property.Value;
                                    JObject json = JObject.Parse(this.GETTransfer(str21, token));
                                    errors = json.GetValue("errors").ToString();
                                    if (errors == "")
                                    {
                                        JObject json2 = JObject.Parse(json.GetValue("data")[0].ToString());


                                        DbNetDTE dbNetDTE = new DbNetDTE(this.ParamValues);
                                        dbNetDTE.CreaGuiaTransfer(json2, str21, out str);
                                        flag2 = false;
                                        num = 200;

                                    }
                                    else
                                    {
                                        flag2 = false;
                                        num = 200;
                                        status = "556";
                                        string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + errors + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                        str = string.Concat(textArray5);
                                        this.objLog.write("ProcessEvent** Error: " + errors);
                                    }
                                }
                            }
                        }
                    }
                    if (((ADirection == "FromClient") && (AHTTPVerb == "POST")) && (AResourceName.ToUpper() == "CALLDTEDBNETVOUCHER"))
                    {
                        str18 = "";
                        str19 = "";
                        str20 = "";
                        str21 = "";
                        string errors = string.Empty;
                        string folio = string.Empty;
                        //string plant = string.Empty;
                        //string plantdet = string.Empty;
                        string status = string.Empty;
                        string msg = string.Empty;
                        string xmlres = string.Empty;
                        string ted = string.Empty;
                        string tedbase64 = string.Empty;

                        foreach (JObject obj3 in JArray.Parse("[" + message.Payload + "]").Children<JObject>())
                        {
                            foreach (JProperty property in obj3.Properties())
                            {
                                if (property.Name == "documentID")
                                {
                                    token = this.doPRISMLoginAux(this.objLog);
                                    str21 = (string)property.Value;
                                    JObject json = JObject.Parse(this.GETReceive(str21, token));
                                    errors = json.GetValue("errors").ToString();
                                    if (errors == "")
                                    {
                                        JObject json2 = JObject.Parse(json.GetValue("data")[0].ToString());



                                        DbNetDTE dbNetDTE = new DbNetDTE(this.ParamValues);
                                        dbNetDTE.CreaGuiaVoucher(json2, str21, out str);
                                        flag2 = false;
                                        num = 200;


                                    }
                                    else
                                    {
                                        flag2 = false;
                                        num = 200;
                                        status = "556";
                                        string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + errors + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                        str = string.Concat(textArray5);
                                        this.objLog.write("ProcessEvent** Error: " + errors);
                                    }
                                }
                            }
                        }
                        //this.objLog.write("SALIMOS");
                    }
                    if (((ADirection == "FromClient") && (AHTTPVerb == "POST")) && (AResourceName.ToUpper() == "CALLDTEFACCL"))
                    {
                        this.objLog.write("INICIO  CALLDTEFACCL");
                        str21 = "";
                        string validacion = string.Empty;
                        string errors = string.Empty;
                        string numdoccliente = string.Empty;
                        string folio = string.Empty;
                        string status = string.Empty;
                        string msg = string.Empty;
                        string posflagtipo = string.Empty;
                        string xmlres = string.Empty;
                        string ted = string.Empty;
                        string tedbase64 = string.Empty;
                        this.objLog.write("ImpresoraOPOS resImpresion");
                        ImpresoraOPOS resImpresion = new ImpresoraOPOS();
                        this.objLog.write("JObject obj3 in");
                        foreach (JObject obj3 in JArray.Parse("[" + message.Payload + "]").Children<JObject>())
                        {
                            foreach (JProperty property in obj3.Properties())
                            {
                                if (property.Name == "documentID")
                                {
                                    this.objLog.write("property.Name == documentID se procesa requerimiento");

                                    token = this.doPRISMLoginAux(this.objLog);
                                    str21 = (string)property.Value;
                                    this.objLog.write("documentID: " + str21);
                                    if (string.IsNullOrEmpty(token)) this.objLog.write("errors: al obtener Token, vacio o nulo");

                                    JObject json = JObject.Parse(this.GETDocument(str21, token));
                                    //this.objLog.write("json: " + json);
                                    errors = json.GetValue("errors").ToString();
                                    this.objLog.write("errors: " + errors);
                                    if (errors == "")
                                    {
                                        this.objLog.write("INICIO  Documento");

                                        JObject json2 = JObject.Parse(json.GetValue("data")[0].ToString());
                                        //this.objLog.write(json.GetValue("data")[0].ToString());
                                        string posflag = Convert.ToString(json2.GetValue("posflag1"));
                                        if ((!string.IsNullOrEmpty(posflag)) && (posflag.Length >= 2))
                                        {
                                            posflag = posflag.Trim().Substring(0, 2);
                                        }



                                        string idcliente = Convert.ToString(json2.GetValue("btcuid"));
                                        this.objLog.write("Cargar cliente");
                                        JObject jsoncustomer = null;
                                        JObject jsoncustomer2 = null;
                                        JArray jsoncustomerRest1 = null;
                                        JObject jsoncustomerRest = null;
                                        if (!string.IsNullOrEmpty(idcliente))
                                        {
                                            jsoncustomerRest1 = JArray.Parse(this.GETCustomerRest(idcliente, token));
                                            jsoncustomerRest = JObject.Parse(jsoncustomerRest1[0].ToString());
                                            jsoncustomer = JObject.Parse(this.GETCustomer(idcliente, token));
                                            jsoncustomer2 = JObject.Parse(jsoncustomer.GetValue("data")[0].ToString());
                                        }



                                        if ((posflag == "39") || (posflag == "33") || (posflag == "61"))
                                        {
                                            JObject jsonreferencia = null;
                                            FacCLDTE facCLDTE = new FacCLDTE(this.ParamValues);

                                            if (posflag == "61")
                                            {
                                                String idreferencia = json2.GetValue("refsalesid").ToString();
                                                JObject jsonref = JObject.Parse(this.GETDocument(idreferencia, token));
                                                jsonreferencia = JObject.Parse(jsonref.GetValue("data")[0].ToString());
                                            }

                                            facCLDTE.CreaDocumentoElectronico(json2, str21, message, jsoncustomerRest, jsonreferencia, out str);
                                            flag2 = false;
                                            num = 200;
                                        }

                                    }
                                    else
                                    {
                                        flag2 = false;
                                        num = 200;
                                        status = "556";
                                        string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + errors + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                        str = string.Concat(textArray5);
                                        this.objLog.write("ProcessEvent** Error: " + errors);
                                    }
                                }
                            }
                        }
                        this.objLog.write("SALIMOS");
                    }
                    if (((ADirection == "FromClient") && (AHTTPVerb == "POST")) && (AResourceName.ToUpper() == "CALLDTEFACCLTRANSFER"))
                    {
                        this.objLog.write("ENTRAMOS");

                        str18 = "";
                        str19 = "";
                        str20 = "";
                        str21 = "";
                        string errors = string.Empty;
                        string folio = string.Empty;
                        string status = string.Empty;
                        string msg = string.Empty;
                        string xmlres = string.Empty;
                        string ted = string.Empty;
                        string tedbase64 = string.Empty;


                        foreach (JObject obj3 in JArray.Parse("[" + message.Payload + "]").Children<JObject>())
                        {
                            foreach (JProperty property in obj3.Properties())
                            {
                                if (property.Name == "documentID")
                                {
                                    token = this.doPRISMLoginAux(this.objLog);
                                    str21 = (string)property.Value;
                                    JObject json = JObject.Parse(this.GETTransfer(str21, token));
                                    errors = json.GetValue("errors").ToString();
                                    if (errors == "")
                                    {
                                        JObject json2 = JObject.Parse(json.GetValue("data")[0].ToString());


                                        FacCLDTE facCLDTE = new FacCLDTE(this.ParamValues);
                                        facCLDTE.CreaGuiaTransfer(json2, str21, out str);
                                        flag2 = false;
                                        num = 200;

                                    }
                                    else
                                    {
                                        flag2 = false;
                                        num = 200;
                                        status = "556";
                                        string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + errors + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                        str = string.Concat(textArray5);
                                        this.objLog.write("ProcessEvent** Error: " + errors);
                                    }
                                }
                            }
                        }
                    }
                    if (((ADirection == "FromClient") && (AHTTPVerb == "POST")) && (AResourceName.ToUpper() == "CALLDTEFACCLVOUCHER"))
                    {
                        str18 = "";
                        str19 = "";
                        str20 = "";
                        str21 = "";
                        string errors = string.Empty;
                        string folio = string.Empty;
                        //string plant = string.Empty;
                        //string plantdet = string.Empty;
                        string status = string.Empty;
                        string msg = string.Empty;
                        string xmlres = string.Empty;
                        string ted = string.Empty;
                        string tedbase64 = string.Empty;

                        foreach (JObject obj3 in JArray.Parse("[" + message.Payload + "]").Children<JObject>())
                        {
                            foreach (JProperty property in obj3.Properties())
                            {
                                if (property.Name == "documentID")
                                {
                                    token = this.doPRISMLoginAux(this.objLog);
                                    str21 = (string)property.Value;
                                    JObject json = JObject.Parse(this.GETReceive(str21, token));
                                    errors = json.GetValue("errors").ToString();
                                    if (errors == "")
                                    {
                                        JObject json2 = JObject.Parse(json.GetValue("data")[0].ToString());



                                        FacCLDTE facCLDTE = new FacCLDTE(this.ParamValues);
                                        facCLDTE.CreaGuiaVoucher(json2, str21, out str);
                                        flag2 = false;
                                        num = 200;


                                    }
                                    else
                                    {
                                        flag2 = false;
                                        num = 200;
                                        status = "556";
                                        string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + errors + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                        str = string.Concat(textArray5);
                                        this.objLog.write("ProcessEvent** Error: " + errors);
                                    }
                                }
                            }
                        }
                       // this.objLog.write("SALIMOS");
                    }
                    if (((ADirection == "FromClient") && (AHTTPVerb == "POST")) && (AResourceName.ToUpper() == "CALLDTEGETONE"))
                    {
                        str18 = "";
                        str19 = "";
                        str20 = "";
                        str21 = "";
                        string validacion = string.Empty;
                        string errors = string.Empty;
                        string numdoccliente = string.Empty;
                        string folio = string.Empty;
                        string plant = string.Empty;
                        string plantdet = string.Empty;
                        string plantref = string.Empty;
                        string status = string.Empty;
                        string msg = string.Empty;
                        string posflagtipo = string.Empty;
                        string xmlres = string.Empty;
                        string ted = string.Empty;
                        string tedbase64 = string.Empty;
                        ImpresoraOPOS resImpresion = new ImpresoraOPOS();

                        foreach (JObject obj3 in JArray.Parse("[" + message.Payload + "]").Children<JObject>())
                        {
                            foreach (JProperty property in obj3.Properties())
                            {
                                if (property.Name == "documentID")
                                {
                                    token = this.doPRISMLogin(this.objLog);
                                    str21 = (string)property.Value;
                                    JObject json = JObject.Parse(this.GETDocument(str21, token));
                                    errors = json.GetValue("errors").ToString();
                                    if (errors == "")
                                    {
                                        JObject json2 = JObject.Parse(json.GetValue("data")[0].ToString());
                                        string idTienda = json2.GetValue("storeno").ToString();
                                        JObject jsonstore = JObject.Parse(this.GETStore(idTienda, token));
                                        JObject store = JObject.Parse(jsonstore.GetValue("data")[0].ToString());
                                        string DTEGetOne_DirOrigen = store.GetValue("address4").ToString();
                                        string DTEGetOne_CmnaOrigen = store.GetValue("address5").ToString();
                                        string DTEGetOne_CiudadOrigen = store.GetValue("address6").ToString();
                                        string posflag = json2.GetValue("posflag1").ToString().Trim().Substring(0, 2);
                                        string idcliente = json2.GetValue("btcuid").ToString();
                                        JArray jsoncustomerRest1 = JArray.Parse(this.GETCustomerRest(idcliente, token));
                                        JObject jsoncustomerRest = JObject.Parse(jsoncustomerRest1[0].ToString());
                                        this.objLog.write("pase conversion a json de cliente");
                                        numdoccliente = jsoncustomerRest.GetValue("info1").ToString();
                                        if (numdoccliente == "")
                                        {
                                            numdoccliente = jsoncustomerRest.GetValue("info2").ToString();
                                            if (numdoccliente == "")
                                            {
                                                numdoccliente = this.ParamValues.DTEGetOne_RutReceptordefault;
                                            }
                                        }
                                        if (posflag == "39")// BOLETA ELECTRONICA
                                        {
                                            try
                                            {
                                                this.objLog.writeDTEGetOne("Procesando Boleta");
                                                #region PROCESO BOLETA ELEC
                                                GetOneDTE.DteDocument goDoc = new GetOneDTE.DteDocument();
                                                #region Creacion de XML Cabecera
                                                //CARATULA
                                                this.objLog.writeDTEGetOne($"Cargando Cabecera");
                                                goDoc.RequestDomain = this.ParamValues.DTEGetOne_Domain;
                                                goDoc.TipoDTE = Convert.ToInt32(posflag);
                                                goDoc.FechaEmision = DateTime.Now.ToString("yyyy-MM-dd");
                                                goDoc.IndicadorServicio = 1;
                                                goDoc.FechaVencimiento = DateTime.Now.ToString("yyyy-MM-dd");
                                                goDoc.DireccionOrigen = String.IsNullOrEmpty(DTEGetOne_DirOrigen) ? "Direccion" : DTEGetOne_DirOrigen;
                                                goDoc.ComunaOrigen = String.IsNullOrEmpty(DTEGetOne_CmnaOrigen) ? "Comuna" : DTEGetOne_CmnaOrigen;
                                                goDoc.CiudadOrigen = String.IsNullOrEmpty(DTEGetOne_CiudadOrigen) ? "Ciudad" : DTEGetOne_CiudadOrigen;

                                                //RECEPTOR
                                                this.objLog.writeDTEGetOne($"Cargando Receptor");
                                                string nombrecliente = jsoncustomerRest.GetValue("first_name").ToString() + " " + jsoncustomerRest.GetValue("last_name").ToString();
                                                if(nombrecliente == "")
                                                {
                                                    goDoc.RazonSocialReceptor = this.ParamValues.DTEGetOne_NombreClientedefault;
                                                    goDoc.RutReceptor = this.ParamValues.DTEGetOne_RutReceptordefault;
                                                    goDoc.DireccionReceptor = "Direccion";
                                                    goDoc.ComunaReceptor = "Santiago";
                                                    goDoc.CiudadReceptor = "Santiago";
                                                    DTEValidaciones(this.ParamValues.DTEGetOne_NombreClientedefault, "Razón Social Receptor", 100, 1);
                                                }
                                                else
                                                {
                                                    goDoc.RazonSocialReceptor = nombrecliente;
                                                    DTEValidaciones(nombrecliente, "Razon Social Receptor", 100, 2);
                                                    goDoc.DireccionReceptor = "Direccion";
                                                    goDoc.ComunaReceptor = "Santiago";
                                                    goDoc.CiudadReceptor = "Santiago";
                                                }
                                                //TOTALES
                                                this.objLog.writeDTEGetOne($"Cargando Totales");
                                                if (json2.GetValue("saletotalamt").ToString() == "0")
                                                {
                                                    goDoc.MontoExento = 0;
                                                    goDoc.IVA = 0;
                                                    goDoc.MontoTotal = 1;
                                                    goDoc.MontoNeto = (decimal)0.84;
                                                }
                                                else
                                                {
                                                    goDoc.MontoExento = 0;
                                                    goDoc.IVA = Decimal.Round(Convert.ToDecimal(json2.GetValue("saletotaltaxamt").ToString()));
                                                    goDoc.MontoTotal = Decimal.Round(Convert.ToDecimal(json2.GetValue("saletotalamt").ToString()));
                                                    int neto = Int32.Parse(Decimal.Round(Convert.ToDecimal(json2.GetValue("saletotalamt").ToString())).ToString()) - Int32.Parse(Decimal.Round(Convert.ToDecimal(json2.GetValue("saletotaltaxamt").ToString())).ToString());
                                                    goDoc.MontoNeto = neto;
                                                    DTEValidaciones(Decimal.Round(Convert.ToDecimal(json2.GetValue("saletotaltaxamt").ToString())).ToString(), "IVA", 18, 1);
                                                    DTEValidaciones(Decimal.Round(Convert.ToDecimal(json2.GetValue("saletotalamt").ToString())).ToString(), "Monto Total", 18, 1);
                                                    DTEValidaciones(neto.ToString(), "Monto Neto", 18, 1);
                                                }
                                                #endregion
                                                #region Creacion Xml Detalle
                                                this.objLog.writeDTEGetOne($"Cargando Detalles");
                                                string strjson3 = json2.GetValue("docitem").ToString();
                                                JArray jsonArray = JArray.Parse(strjson3);

                                                foreach (JObject jsonOperaciones in jsonArray.Children<JObject>())
                                                {
                                                    if (contadordetalle < 61)
                                                    {
                                                        decimal descuento = 0;
                                                        DTEValidaciones(jsonOperaciones["alu"].ToString(), "Valor Código", 35, 1);
                                                        DTEValidaciones(jsonOperaciones["description1"].ToString(), "Nombre Item", 70, 1);
                                                        DTEValidaciones(Decimal.Round(Convert.ToDecimal((jsonOperaciones["origprice"]).ToString())).ToString(), "Precio Item", 19, 2);
                                                        string cantidad = (jsonOperaciones["qty"]).ToString();
                                                        string iva = (jsonOperaciones["taxamt"]).ToString().Split(',')[0];
                                                        string precio = Decimal.Round(Convert.ToDecimal((jsonOperaciones["price"]).ToString())).ToString();
                                                        double montoitem = (Int32.Parse(precio) * Convert.ToDouble(cantidad));
                                                        int itemType = jsonOperaciones["itemtype"].Value<int>();
                                                        try
                                                        {
                                                            descuento = Decimal.Round(Convert.ToDecimal((jsonOperaciones["discamt"]).ToString()));
                                                        }
                                                        catch
                                                        {
                                                            descuento = 0;
                                                        }
                                                        if (itemType == 2)
                                                        {
                                                            int QtyItem = jsonOperaciones["qty"].Value<Int32>();
                                                            GetOneDTE.DteDocument.DteDescRec descReturn = new GetOneDTE.DteDocument.DteDescRec();
                                                            descReturn.NroDescRecargo = 1;
                                                            descReturn.TipoMovimiento = "D";
                                                            descReturn.GlosaDescRecargo = LargoCadenaMax(QtyItem.ToString() + " x " + (jsonOperaciones["description1"]).ToString(), 44);
                                                            descReturn.TipoValor = "$";
                                                            descReturn.ValorDescRecargo = Convert.ToInt32(QtyItem * jsonOperaciones["price"].Value<float>());
                                                            goDoc.DteDescuentosRecargos.Add(descReturn);
                                                        }
                                                        else
                                                        {
                                                            GetOneDTE.DteDocument.DteDetail detail = new GetOneDTE.DteDocument.DteDetail();
                                                            detail.NroLinea = Convert.ToInt32(jsonOperaciones["itempos"]);
                                                            detail.Cantidad = Convert.ToDecimal(jsonOperaciones["qty"]);
                                                            detail.CodigoItem = jsonOperaciones["alu"].ToString();
                                                            detail.UnidadMedida = "Un.";
                                                            detail.TipoCodigo = "INTERNO";

                                                            detail.NombreItem = jsonOperaciones["description1"].ToString();
                                                            detail.PrecioUnitario = Decimal.Round(Convert.ToDecimal((jsonOperaciones["origprice"]).ToString()));
                                                            detail.MontoItem = Convert.ToDecimal(montoitem);
                                                            if (descuento > 0)
                                                                detail.DctoMonto = descuento;
                                                            goDoc.DteDetalles.Add(detail);
                                                            contadordetalle = contadordetalle + 1;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        msg = "No es posible superar los 60 Items";
                                                        throw new ApplicationException(msg);
                                                    }
                                                }
                                                //plant = plant.Replace("#DETALLE#", plantdet);
                                                //agregar referencia de cliente cuando tenga correo.
                                                //agregar referencia de devolucion.

                                                #endregion
                                                #region Emite Documento
                                                this.objLog.writeDTEGetOne($"Armando Documento");
                                                string fudString = "";
                                                if (!goDoc.GeneraBloqueDTE(out fudString))
                                                {
                                                    msg = $"Error al armar documento electrónico - {fudString}";
                                                    throw new ApplicationException(msg);
                                                }
                                                else
                                                {
                                                    this.objLog.writeDTEGetOne($"Emitiendo Documento");
                                                    this.objLog.writeDTEGetOne("FUD:  " + fudString);
                                                    string msgErr = "";
                                                    string respuesta = "";
                                                    string xmlRespuesta = "";
                                                    goDoc.EmiteDTE(fudString, out msgErr, out folio, out boletapdf, out ted, out respuesta, out xmlRespuesta);
                                                    //this.objLog.writeDTEGetOne($"-------------------- RESPUESTA --------------------");
                                                    //this.objLog.writeDTEGetOne(respuesta);
                                                    //this.objLog.writeDTEGetOne($"-------------------- XML RESPUESTA --------------------");
                                                    //this.objLog.writeDTEGetOne(xmlRespuesta);
                                                    if (msgErr.ToUpper() != "OK")
                                                    {
                                                        msg = ScreenErrorMessage(msgErr, respuesta);
                                                        //msg = $"Error al emitir documento electronico - {respuesta.Replace("\r\n"," ")}";
                                                        this.objLog.writeDTEGetOne(msg);
                                                        this.objLog.writeDTEGetOne("Respuesta GetOne: " + respuesta);
                                                        throw new ApplicationException(msg);
                                                    }
                                                }
                                                #endregion
                                                if (folio != "0")
                                                {
                                                    this.objLog.writeDTEGetOne("Folio:  " + folio);
                                                    flag2 = false;
                                                    num = 200;
                                                    status = "0";
                                                    //this.objLog.write(xmlres);
                                                    //ted = retornaTED(xmlres);
                                                    tedbase64 = EncodeStrToBase64(ted);

                                                    string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                    str = string.Concat(textArray5);

                                                    this.objLog.writeDTEGetOne("Respuesta:  " + str);
                                                }
                                                else
                                                {
                                                    msg = $"Error al obtener folio";
                                                    this.objLog.writeDTEGetOne(msg);
                                                    throw new ApplicationException(msg);
                                                }
                                                #endregion
                                            }
                                            catch (Exception e)
                                            {
                                                flag2 = false;
                                                num = 200;
                                                status = "555";
                                                this.objLog.write("EXCEPTION2");
                                                str = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                                                this.objLog.write("ProcessEvent *********** Error General -- " + e.Message);
                                                this.txtMensaje.Text = this.txtMensaje.Text + "\nExcepcion: " + e.Message;
                                            }
                                        }
                                        else if (posflag == "33")// FACTURA ELECTRONICA
                                        {
                                            this.objLog.writeDTEVyV("Procesando Factura");
                                            try
                                            {
                                                #region PROCESO FACTURA ELECTRONICA
                                                GetOneDTE.DteDocument goDoc = new GetOneDTE.DteDocument();
                                                #region Creacion de XML Cabecera
                                                //this.objLog.write("llegue al documento");
                                                //DOCUMENTO
                                                this.objLog.writeDTEGetOne($"Cargando Cabecera");
                                                goDoc.RequestDomain = this.ParamValues.DTEGetOne_Domain;
                                                goDoc.TipoDTE = Convert.ToInt32(posflag);
                                                goDoc.FechaEmision = DateTime.Now.ToString("yyyy-MM-dd");
                                                goDoc.FechaVencimiento = DateTime.Now.ToString("yyyy-MM-dd");
                                                goDoc.DireccionOrigen = String.IsNullOrEmpty(DTEGetOne_DirOrigen) ? "Direccion" : DTEGetOne_DirOrigen;
                                                goDoc.ComunaOrigen = String.IsNullOrEmpty(DTEGetOne_CmnaOrigen) ? "Comuna" : DTEGetOne_CmnaOrigen;
                                                goDoc.CiudadOrigen = String.IsNullOrEmpty(DTEGetOne_CiudadOrigen) ? "Ciudad" : DTEGetOne_CiudadOrigen;
                                                //this.objLog.write("llegue al receptor");
                                                //RECEPTOR
                                                this.objLog.writeDTEGetOne($"Cargando Receptor");
                                                goDoc.RutReceptor = numdoccliente;
                                                DTEValidaciones(numdoccliente, "Rut Receptor", 10, 1);
                                                string nombrecliente = json2.GetValue("btfirstname").ToString() + " " + json2.GetValue("btlastname").ToString();
                                                if (nombrecliente == "")
                                                {
                                                    goDoc.RazonSocialReceptor = this.ParamValues.DTEGetOne_NombreClientedefault;
                                                    DTEValidaciones(this.ParamValues.DTEVyV_NombreClientedefault, "Razon Social Receptor", 100, 1);
                                                }
                                                else
                                                {
                                                    goDoc.RazonSocialReceptor = nombrecliente;
                                                    DTEValidaciones(nombrecliente, "Razon Social Receptor", 100, 2);
                                                }
                                                //this.objLog.write("busco el giro");
                                                //this.objLog.write(jsoncustomer2.ToString());
                                                string giro = jsoncustomerRest.GetValue("notes").ToString();
                                                if (giro == "")
                                                {
                                                    goDoc.GiroReceptor = LargoCadenaMax(this.ParamValues.DTEVyV_GiroClientedefault, 40);
                                                    DTEValidaciones(LargoCadenaMax(this.ParamValues.DTEVyV_GiroClientedefault, 40), "Giro Receptor", 40, 1);
                                                }
                                                else
                                                {
                                                    goDoc.GiroReceptor = LargoCadenaMax(giro, 40);
                                                    DTEValidaciones(LargoCadenaMax(giro, 40), "Giro Receptor", 40, 2);
                                                }
                                                goDoc.DireccionReceptor = jsoncustomerRest.GetValue("primary_address_line_1").ToString();
                                                goDoc.ComunaReceptor = jsoncustomerRest.GetValue("primary_address_line_4").ToString();
                                                goDoc.CiudadReceptor = jsoncustomerRest.GetValue("primary_address_line_5").ToString();
                                                goDoc.DireccionDestino = goDoc.DireccionReceptor;
                                                goDoc.ComunaDestino = goDoc.ComunaReceptor;
                                                goDoc.CiudadDestino = goDoc.CiudadReceptor;
                                                DTEValidaciones(goDoc.DireccionReceptor, "Dirección Receptor", 70, 2);
                                                DTEValidaciones(goDoc.ComunaReceptor, "Comuna Receptor", 20, 1);
                                                DTEValidaciones(goDoc.CiudadReceptor, "Ciudad Receptor", 20, 1);
                                                //this.objLog.write("llegue a los totales");
                                                //TOTALES
                                                this.objLog.writeDTEGetOne($"Cargando Totales");
                                                int neto = json2["saletotalamt"].Value<Int32>() - json2["saletotaltaxamt"].Value<Int32>();
                                                goDoc.MontoExento = 0;
                                                goDoc.IVA = json2["saletotaltaxamt"].Value<Decimal>();
                                                goDoc.MontoTotal = json2["saletotalamt"].Value<Decimal>();
                                                goDoc.MontoNeto = neto;
                                                goDoc.TasaIVA = Convert.ToDecimal(this.ParamValues.DTEGetOne_TasaIVA);
                                                DTEValidaciones(this.ParamValues.DTEVyV_TasaIVA, "Tasa IVA", 6, 3);
                                                DTEValidaciones(goDoc.IVA.ToString(), "IVA", 18, 3);
                                                DTEValidaciones(neto.ToString(), "Monto Neto", 18, 3);
                                                DTEValidaciones(goDoc.MontoTotal.ToString(), "Monto Total", 18, 3);
                                                //this.objLog.write("sali del total");
                                                #endregion
                                                #region Creacion Xml Detalle
                                                this.objLog.writeDTEGetOne($"Cargando Detalles");
                                                string strjson3 = json2.GetValue("docitem").ToString();
                                                JArray jsonArray = JArray.Parse(strjson3);
                                                //this.objLog.write("entrare al detalle");
                                                foreach (JObject jsonOperaciones in jsonArray.Children<JObject>())
                                                {
                                                    if (contadordetalle < 41)
                                                    {

                                                        decimal descuento = 0;
                                                        decimal cantidad = jsonOperaciones["qty"].Value<Decimal>();
                                                        int iva = jsonOperaciones["taxamt"].Value<Int32>();
                                                        decimal precio = jsonOperaciones["price"].Value<Decimal>();
                                                        decimal montoitem = (precio * cantidad) - (iva * cantidad);
                                                        decimal netoitem = (precio) - (iva);
                                                        decimal precioori = jsonOperaciones["origprice"].Value<Decimal>();
                                                        int preciosiniva = (Convert.ToInt32(precioori) - iva);
                                                        DTEValidaciones(jsonOperaciones["description1"].ToString(), "Nombre Item", 70, 1);
                                                        DTEValidaciones(jsonOperaciones["alu"].ToString(), "Código Item", 35, 1);
                                                        DTEValidaciones(preciosiniva.ToString(), "Precio Item", 19, 2);
                                                        try
                                                        {
                                                            descuento = jsonOperaciones["discamt"].Value<Decimal>();
                                                            descuento = descuento - ((descuento * Int32.Parse(this.ParamValues.DTEVyV_TasaIVA)) / 100);
                                                        }
                                                        catch
                                                        {
                                                            descuento = 0;
                                                        }
                                                        GetOneDTE.DteDocument.DteDetail detail = new GetOneDTE.DteDocument.DteDetail();
                                                        detail.NroLinea = jsonOperaciones["itempos"].Value<Int32>();
                                                        detail.Cantidad = jsonOperaciones["qty"].Value<Decimal>();
                                                        detail.CodigoItem = jsonOperaciones["alu"].Value<String>();
                                                        detail.UnidadMedida = "Un.";
                                                        detail.TipoCodigo = "INTERNO";

                                                        detail.NombreItem = jsonOperaciones["description1"].Value<String>();
                                                        detail.PrecioUnitario = netoitem;
                                                        //detail.PrecioUnitario = jsonOperaciones["origprice"].Value<Decimal>();//CAMBIO VALORES 2021-05-19
                                                        detail.MontoItem = Convert.ToDecimal(montoitem);
                                                        if (descuento > 0)
                                                        {
                                                            detail.DctoMonto = descuento;

                                                            //TESTING
                                                            detail.PrecioUnitario += detail.DctoMonto.GetValueOrDefault();
                                                        }


                                                        goDoc.DteDetalles.Add(detail);
                                                        contadordetalle = contadordetalle + 1;
                                                    }
                                                    else
                                                    {
                                                        msg = "No es posible superar los 40 Items";
                                                        throw new ApplicationException(msg);
                                                    }
                                                }
                                                #endregion
                                                #region Emite Documento
                                                this.objLog.writeDTEGetOne($"Armando Documento");
                                                string fudString = "";
                                                if (!goDoc.GeneraBloqueDTE(out fudString))
                                                {
                                                    msg = $"Error al armar documento electrónico - {fudString}";
                                                    throw new ApplicationException(msg);
                                                }
                                                else
                                                {
                                                    this.objLog.writeDTEGetOne($"Emitiendo Documento");
                                                    this.objLog.writeDTEGetOne("FUD:  " + fudString);
                                                    string msgErr = "";
                                                    string respuesta = "";
                                                    string xmlRespuesta = "";
                                                    goDoc.EmiteDTE(fudString, out msgErr, out folio, out boletapdf, out ted, out respuesta, out xmlRespuesta);
                                                    if (msgErr.ToUpper() != "OK")
                                                    {
                                                        msg = ScreenErrorMessage(msgErr, respuesta);
                                                        //msg = $"Error al emitir documento electronico - {respuesta.Replace("\r\n", " ")}";
                                                        this.objLog.writeDTEGetOne(msg);
                                                        this.objLog.writeDTEGetOne("Respuesta GetOne: " + respuesta);
                                                        throw new ApplicationException(msg);
                                                    }
                                                }
                                                #endregion
                                                if (folio != "0")
                                                {
                                                    this.objLog.writeDTEGetOne("Folio:  " + folio);
                                                    flag2 = false;
                                                    num = 200;
                                                    status = "0";
                                                    //this.objLog.write(xmlres);
                                                    //ted = retornaTED(xmlres);
                                                    tedbase64 = EncodeStrToBase64(ted);

                                                    string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                    str = string.Concat(textArray5);


                                                    this.objLog.writeDTEGetOne("Respuesta:  " + str);
                                                }
                                                else
                                                {
                                                    msg = $"Error al obtener folio";
                                                    this.objLog.writeDTEGetOne(msg);
                                                    throw new ApplicationException(msg);
                                                }
                                                #endregion
                                            }
                                            catch (Exception e)
                                            {
                                                flag2 = false;
                                                num = 200;
                                                status = "555";
                                                this.objLog.write("EXCEPTION2");
                                                str = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                                                this.objLog.write("ProcessEvent *********** Error General -- " + e.Message);
                                                this.txtMensaje.Text = this.txtMensaje.Text + "\nExcepcion: " + e.Message;
                                            }
                                        }
                                        else if (posflag == "61")// NOTA DE CREDITO ELECTRONICA
                                        {
                                            //posflagtipo = "NOTA DE CREDITO ELECTRONICA";
                                            this.objLog.writeDTEVyV("Procesando Nota de Credito");
                                            try
                                            {
                                                #region PROCESO NOTA DE CREDITO ELECTRONICA
                                                GetOneDTE.DteDocument goDoc = new GetOneDTE.DteDocument();
                                                #region Creacion de XML Cabecera
                                                //DOCUMENTO
                                                this.objLog.writeDTEGetOne($"Cargando Cabecera");
                                                goDoc.RequestDomain = this.ParamValues.DTEGetOne_Domain;
                                                goDoc.TipoDTE = Convert.ToInt32(posflag);
                                                goDoc.FechaEmision = DateTime.Now.ToString("yyyy-MM-dd");
                                                goDoc.FechaVencimiento = DateTime.Now.ToString("yyyy-MM-dd");
                                                goDoc.DireccionOrigen = String.IsNullOrEmpty(DTEGetOne_DirOrigen) ? "Direccion" : DTEGetOne_DirOrigen;
                                                goDoc.ComunaOrigen = String.IsNullOrEmpty(DTEGetOne_CmnaOrigen) ? "Comuna" : DTEGetOne_CmnaOrigen;
                                                goDoc.CiudadOrigen = String.IsNullOrEmpty(DTEGetOne_CiudadOrigen) ? "Ciudad" : DTEGetOne_CiudadOrigen;
                                                //RECEPTOR
                                                this.objLog.writeDTEGetOne($"Cargando Receptor");
                                                goDoc.RutReceptor = numdoccliente;
                                                DTEValidaciones(numdoccliente, "RUT Receptor", 10, 1);
                                                string nombrecliente = jsoncustomerRest.GetValue("first_name").ToString() + " " + jsoncustomerRest.GetValue("last_name").ToString();
                                                if (nombrecliente == "")
                                                {
                                                    goDoc.RazonSocialReceptor = this.ParamValues.DTEGetOne_NombreClientedefault;
                                                    DTEValidaciones(this.ParamValues.DTEVyV_NombreClientedefault, "Razon Social Receptor", 100, 1);
                                                }
                                                else
                                                {
                                                    //plant = plant.Replace("#RZNSOCRECEP#", nombrecliente);
                                                    goDoc.RazonSocialReceptor = nombrecliente;
                                                    DTEValidaciones(nombrecliente, "Razon Social Receptor", 100, 2);
                                                }
                                                string giro = jsoncustomerRest.GetValue("notes").ToString();
                                                if (giro == "")
                                                {
                                                    goDoc.GiroReceptor = LargoCadenaMax(this.ParamValues.DTEVyV_GiroClientedefault, 40);
                                                    DTEValidaciones(LargoCadenaMax(this.ParamValues.DTEVyV_GiroClientedefault, 40), "Giro Receptor", 40, 1);
                                                }
                                                else
                                                {
                                                    goDoc.GiroReceptor = LargoCadenaMax(giro, 40);
                                                    DTEValidaciones(LargoCadenaMax(giro, 40), "Giro Receptor", 40, 2);
                                                }
                                                goDoc.DireccionReceptor = jsoncustomerRest.GetValue("primary_address_line_1").ToString();
                                                goDoc.ComunaReceptor = jsoncustomerRest.GetValue("primary_address_line_4").ToString();
                                                goDoc.CiudadReceptor = jsoncustomerRest.GetValue("primary_address_line_5").ToString();
                                                goDoc.DireccionDestino = goDoc.DireccionReceptor;
                                                goDoc.ComunaDestino = goDoc.ComunaReceptor;
                                                goDoc.CiudadDestino = goDoc.CiudadReceptor;
                                                DTEValidaciones(goDoc.DireccionReceptor, "Dirección Receptor", 70, 2);
                                                DTEValidaciones(goDoc.ComunaReceptor, "Comuna Receptor", 20, 1);
                                                DTEValidaciones(goDoc.CiudadReceptor, "Ciudad Receptor", 20, 1);
                                                //TOTALES
                                                this.objLog.writeDTEGetOne($"Cargando Totales");

                                                int neto = json2["returnsubtotalwithtax"].Value<Int32>() - json2["returntotaltaxamt"].Value<Int32>();
                                                goDoc.MontoExento = 0;
                                                goDoc.IVA = json2["returntotaltaxamt"].Value<Int32>();
                                                goDoc.MontoTotal = json2["returnsubtotalwithtax"].Value<Decimal>();
                                                goDoc.MontoNeto = neto;
                                                goDoc.TasaIVA = Convert.ToDecimal(this.ParamValues.DTEGetOne_TasaIVA);
                                                DTEValidaciones(neto.ToString(), "Monto Neto", 18, 3);
                                                DTEValidaciones(this.ParamValues.DTEVyV_TasaIVA, "Tasa IVA", 6, 3);
                                                DTEValidaciones(goDoc.IVA.ToString(), "IVA", 18, 3);
                                                DTEValidaciones(goDoc.MontoTotal.ToString(), "Monto Total", 18, 3);
                                                #endregion
                                                #region Creacion Xml Detalle
                                                this.objLog.writeDTEGetOne($"Cargando Detalles");
                                                string strjson3 = json2.GetValue("docitem").ToString();
                                                JArray jsonArray = JArray.Parse(strjson3);
                                                foreach (JObject jsonOperaciones in jsonArray.Children<JObject>())
                                                {
                                                    if (contadordetalle < 41)
                                                    {
                                                        decimal descuento = 0;
                                                        DTEValidaciones(jsonOperaciones["alu"].ToString(), "Código Item", 35, 1);
                                                        DTEValidaciones(jsonOperaciones["description1"].ToString(), "Nombre Item", 70, 1);
                                                        decimal cantidad = jsonOperaciones["qty"].Value<Decimal>();
                                                        int iva = jsonOperaciones["taxamt"].Value<Int32>();
                                                        int precio = jsonOperaciones["price"].Value<Int32>();
                                                        double montoitem = ((precio * Convert.ToDouble(cantidad)) - iva * Convert.ToDouble(cantidad));
                                                        double netoitem = ((precio) - iva);

                                                        int precioori = jsonOperaciones["origprice"].Value<Int32>();
                                                        int preciosiniva = precioori - iva;
                                                        DTEValidaciones(preciosiniva.ToString(), "Precio Item", 19, 2);
                                                        try
                                                        {
                                                            descuento = Convert.ToDecimal(jsonOperaciones["discamt"]);
                                                        }
                                                        catch
                                                        {
                                                            descuento = 0;
                                                        }
                                                        GetOneDTE.DteDocument.DteDetail detail = new GetOneDTE.DteDocument.DteDetail();
                                                        detail.NroLinea = Convert.ToInt32(jsonOperaciones["itempos"]);
                                                        detail.Cantidad = Convert.ToDecimal(jsonOperaciones["qty"]);
                                                        detail.CodigoItem = jsonOperaciones["alu"].ToString();
                                                        detail.UnidadMedida = "Un.";
                                                        detail.TipoCodigo = "INTERNO";

                                                        detail.NombreItem = jsonOperaciones["description1"].ToString();
                                                        detail.PrecioUnitario = (decimal)netoitem;
                                                        //detail.PrecioUnitario = Decimal.Round(Convert.ToDecimal((jsonOperaciones["origprice"]).ToString()));//CAMBIO VALORES 2021-05-19
                                                        detail.MontoItem = Convert.ToDecimal(montoitem);
                                                        if (descuento > 0)
                                                        {
                                                            detail.DctoMonto = descuento;

                                                            //TESTING
                                                            detail.PrecioUnitario += detail.DctoMonto.GetValueOrDefault();
                                                        }
                                                        goDoc.DteDetalles.Add(detail);
                                                        contadordetalle = contadordetalle + 1;
                                                    }
                                                    else
                                                    {
                                                        msg = "No es posible superar los 40 Items";
                                                        throw new ApplicationException(msg);
                                                    }
                                                }
                                                //plant = plant.Replace("#DETALLE#", plantdet);
                                                #endregion,
                                                #region Creacion Xml Referencia
                                                this.objLog.writeDTEGetOne($"Cargando Referencia");
                                                String idreferencia = json2.GetValue("refsalesid").ToString();
                                                JObject jsonref = JObject.Parse(this.GETDocument(idreferencia, token));
                                                JObject jsonreferencia = JObject.Parse(jsonref.GetValue("data")[0].ToString());

                                                string posflagref = jsonreferencia.GetValue("posflag1").ToString().Trim().Substring(0, 2);
                                                DateTime fecharef = DateTime.Parse(jsonreferencia.GetValue("modifieddatetime").ToString());
                                                string fecharefstr = String.Format("{0:yyyy-MM-dd}", fecharef);
                                                GetOneDTE.DteDocument.DteRef dteRef = new GetOneDTE.DteDocument.DteRef();
                                                dteRef.NroLineaReferencia = 1;
                                                dteRef.TipoDocumentoReferencia = Convert.ToInt32(posflagref);
                                                dteRef.FolioReferencia = jsonreferencia.GetValue("docno").ToString();
                                                dteRef.FechaReferencia = fecharefstr;
                                                dteRef.CodigoReferencia = 1;
                                                goDoc.DteReferencias.Add(dteRef);
                                                #endregion
                                                #region Emite Documento
                                                this.objLog.writeDTEGetOne($"Armando Documento");
                                                string fudString = "";
                                                if (!goDoc.GeneraBloqueDTE(out fudString))
                                                {
                                                    msg = $"Error al armar documento electrónico - {fudString}";
                                                    throw new ApplicationException(msg);
                                                }
                                                else
                                                {
                                                    this.objLog.writeDTEGetOne($"Emitiendo Documento");
                                                    this.objLog.writeDTEGetOne("FUD:  " + fudString);
                                                    string msgErr = "";
                                                    string respuesta = "";
                                                    string xmlRespuesta = "";
                                                    goDoc.EmiteDTE(fudString, out msgErr, out folio, out boletapdf, out ted, out respuesta, out xmlRespuesta);
                                                    if (msgErr.ToUpper() != "OK")
                                                    {
                                                        msg = ScreenErrorMessage(msgErr, respuesta);
                                                        //msg = $"Error al emitir documento electronico - {respuesta.Replace("\r\n", " ")}";
                                                        this.objLog.writeDTEGetOne(msg);
                                                        this.objLog.writeDTEGetOne("Respuesta GetOne: " + respuesta);
                                                        throw new ApplicationException(msg);
                                                    }
                                                }
                                                #endregion
                                                if (folio != "0")
                                                {
                                                    flag2 = false;
                                                    num = 200;
                                                    status = "0";
                                                    //this.objLog.write(xmlres);
                                                    //ted = retornaTED(xmlres);
                                                    tedbase64 = EncodeStrToBase64(ted);

                                                    string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                    str = string.Concat(textArray5);
                                                    this.objLog.writeDTEGetOne("Respuesta:  " + str);
                                                }
                                                else
                                                {
                                                    msg = $"Error al obtener folio";
                                                    this.objLog.writeDTEGetOne(msg);
                                                    throw new ApplicationException(msg);
                                                }
                                                #endregion
                                            }
                                            catch (Exception e)
                                            {
                                                flag2 = false;
                                                num = 200;
                                                status = "555";
                                                this.objLog.write("EXCEPTION2");
                                                str = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                                                this.objLog.write("ProcessEvent *********** Error General -- " + e.Message);
                                                this.txtMensaje.Text = this.txtMensaje.Text + "\nExcepcion: " + e.Message;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        flag2 = false;
                                        num = 200;
                                        status = "556";
                                        string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + errors + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                        str = string.Concat(textArray5);
                                        this.objLog.write("ProcessEvent** Error: " + errors);
                                    }
                                }
                            }
                        }
                        this.objLog.write("SALIMOS");
                    }
                    if (((ADirection == "FromClient") && (AHTTPVerb == "POST")) && (AResourceName.ToUpper() == "CALLDTEGETONETRANSFER"))
                    {
                        str18 = "";
                        str19 = "";
                        str20 = "";
                        str21 = "";
                        string errors = string.Empty;
                        string folio = string.Empty;
                        string plant = string.Empty;
                        string plantdet = string.Empty;
                        string status = string.Empty;
                        string msg = string.Empty;
                        string xmlres = string.Empty;
                        string ted = string.Empty;
                        string tedbase64 = string.Empty;

                        this.objLog.write("ENTRAMOS");
                        foreach (JObject obj3 in JArray.Parse("[" + message.Payload + "]").Children<JObject>())
                        {
                            foreach (JProperty property in obj3.Properties())
                            {
                                if (property.Name == "documentID")
                                {
                                    token = this.doPRISMLogin(this.objLog);
                                    str21 = (string)property.Value;
                                    JObject json = JObject.Parse(this.GETTransfer(str21, token));
                                    errors = json.GetValue("errors").ToString();
                                    if (errors == "")
                                    {

                                        JObject json2 = JObject.Parse(json.GetValue("data")[0].ToString());
                                        string idTienda = json2.GetValue("outstoreno").ToString();
                                        JObject jsonstore = JObject.Parse(this.GETStore(idTienda, token));
                                        JObject store = JObject.Parse(jsonstore.GetValue("data")[0].ToString());
                                        string DTEVyV_Tienda = store.GetValue("storename").ToString();
                                        string DTEVyV_RutEmisor = store.GetValue("zip").ToString();
                                        string DTEVyV_RznSocEmi = store.GetValue("udf1string").ToString();
                                        string DTEVyV_Giro = store.GetValue("udf2string").ToString();
                                        string DTEVyV_Direccion_tienda = store.GetValue("address1").ToString();
                                        string DTEVyV_DirOrigen = store.GetValue("address4").ToString();
                                        string DTEVyV_CmnaOrigen = store.GetValue("address5").ToString();
                                        string DTEVyV_CiudadOrigen = store.GetValue("address6").ToString();
                                        string DTEVyV_Telefono = store.GetValue("phone2").ToString();
                                        string DTEVyV_Telefono_tienda = store.GetValue("phone1").ToString();


                                        string posflag = "52";
                                        try
                                        {
                                            this.objLog.writeDTEGetOne("Guia de Despacho - Transferencias");
                                            #region Creacion de XML Cabecera
                                            //DOCUMENTO
                                            this.objLog.writeDTEGetOne($"Cargando Cabecera");
                                            GetOneDTE.DteDocument goDoc = new GetOneDTE.DteDocument();
                                            goDoc.RequestDomain = this.ParamValues.DTEGetOne_Domain;
                                            goDoc.TipoDTE = Convert.ToInt32(posflag);
                                            goDoc.FechaEmision = DateTime.Now.ToString("yyyy-MM-dd");
                                            goDoc.FechaVencimiento = DateTime.Now.ToString("yyyy-MM-dd");
                                            goDoc.IndTraslado = 5;
                                            //RECEPTOR
                                            this.objLog.writeDTEGetOne($"Cargando Receptor");
                                            this.objLog.writeDTEGetOne(json2.ToString());
                                            goDoc.RutReceptor = json2.GetValue("origstorezip").ToString();
                                            goDoc.RazonSocialReceptor = json2.GetValue("instoreudf1string").ToString();
                                            goDoc.GiroReceptor = LargoCadenaMax((json2.GetValue("instoreudf2string").ToString() + " " + json2.GetValue("instoreudf3string").ToString()).Trim(), 40);
                                            goDoc.DireccionReceptor = json2.GetValue("instoreaddress1").ToString();
                                            goDoc.ComunaReceptor = json2.GetValue("instoreaddress2").ToString();
                                            goDoc.CiudadReceptor = json2.GetValue("instoreaddress3").ToString();

                                            DTEValidaciones(goDoc.RutReceptor, "Rut Receptor", 10, 1);
                                            DTEValidaciones(goDoc.RazonSocialReceptor, "Razon Social Receptor", 100, 1);
                                            DTEValidaciones(goDoc.CiudadReceptor, "Ciudad Receptor", 20, 1);
                                            DTEValidaciones(goDoc.GiroReceptor, "Giro Receptor", 40, 1);
                                            DTEValidaciones(goDoc.DireccionReceptor, "Dirección Receptor", 70, 2);
                                            DTEValidaciones(goDoc.ComunaReceptor, "Comuna Receptor", 20, 1);

                                            goDoc.DireccionDestino = goDoc.DireccionReceptor;
                                            goDoc.ComunaDestino = goDoc.ComunaReceptor;
                                            goDoc.CiudadDestino = goDoc.CiudadReceptor;

                                            goDoc.DireccionOrigen = DTEVyV_DirOrigen;
                                            goDoc.ComunaOrigen = DTEVyV_CmnaOrigen;
                                            goDoc.CiudadOrigen = DTEVyV_CiudadOrigen;
                                            //TOTALES
                                            this.objLog.writeDTEGetOne($"Cargando Totales");

                                            int iva = json2["docpricetotal"].Value<Int32>() * Int32.Parse(this.ParamValues.DTEGetOne_TasaIVA) / 100;
                                            int neto = json2["docpricetotal"].Value<Int32>() - iva;

                                            goDoc.MontoExento = 0;
                                            goDoc.IVA = iva;
                                            goDoc.MontoTotal = json2["docpricetotal"].Value<Int32>();
                                            goDoc.MontoNeto = neto;
                                            goDoc.TasaIVA = Convert.ToDecimal(this.ParamValues.DTEGetOne_TasaIVA);
                                            DTEValidaciones(this.ParamValues.DTEGetOne_TasaIVA, "Tasa IVA", 6, 3);
                                            DTEValidaciones(iva.ToString(), "Tasa IVA", 18, 3);
                                            DTEValidaciones(neto.ToString(), "Monto Neto", 18, 3);
                                            DTEValidaciones(json2.GetValue("docpricetotal").ToString(), "Monto Total", 18, 3);
                                            #endregion
                                            #region Creacion Xml Detalle
                                            this.objLog.writeDTEGetOne($"Cargando Detalles");
                                            string jsonitem = json2.GetValue("slipitem").ToString();
                                            JArray jsonArrayitem = JArray.Parse(jsonitem);
                                            foreach (JObject jsonOperaciones in jsonArrayitem.Children<JObject>())
                                            {
                                                if (contadordetalle < 41)
                                                {
                                                    //plantilla = new PlantillaXml();
                                                    //plantdet = plantdet + this.plantilla.PantillaDetGuiaDespacho();
                                                    //plantdet = plantdet.Replace("#NROLINDET#", (jsonOperaciones["itempos"]).ToString());
                                                    //plantdet = plantdet.Replace("#TPOCODIGO#", "INTERNO");
                                                    //plantdet = plantdet.Replace("#VLRCODIGO#", (jsonOperaciones["alu"]).ToString());
                                                    //plantdet = plantdet.Replace("#NMBITEM#", (jsonOperaciones["description1"]).ToString());
                                                    //plantdet = plantdet.Replace("#DESC#", (jsonOperaciones["description1"]).ToString());
                                                    //plantdet = plantdet.Replace("#QTYITEM#", (jsonOperaciones["qty"]).ToString());
                                                    int ivadet = jsonOperaciones["price"].Value<Int32>() * Int32.Parse(this.ParamValues.DTEGetOne_TasaIVA) / 100;
                                                    decimal cantidad = jsonOperaciones["qty"].Value<Decimal>();
                                                    decimal precio = jsonOperaciones["price"].Value<Decimal>();
                                                    decimal montoitem = (precio * cantidad) - (ivadet * cantidad);
                                                    //plantdet = plantdet.Replace("#MONTOITEM#", montoitem.ToString());
                                                    int preciosiniva = Convert.ToInt32(precio - ivadet);
                                                    //plantdet = plantdet.Replace("#PRCITEM#", preciosiniva.ToString());
                                                    DTEValidaciones((jsonOperaciones["alu"]).ToString(), "Código Item", 35, 1);
                                                    DTEValidaciones((jsonOperaciones["description1"]).ToString(), "Nombre Item", 70, 1);
                                                    DTEValidaciones(preciosiniva.ToString(), "Precio Item", 19, 2);

                                                    GetOneDTE.DteDocument.DteDetail detail = new GetOneDTE.DteDocument.DteDetail();
                                                    detail.NroLinea = jsonOperaciones["itempos"].Value<Int32>();
                                                    detail.Cantidad = jsonOperaciones["qty"].Value<Decimal>();
                                                    detail.CodigoItem = jsonOperaciones["alu"].Value<String>();
                                                    detail.UnidadMedida = "Un.";
                                                    detail.TipoCodigo = "INTERNO";
                                                    detail.NombreItem = jsonOperaciones["description1"].Value<String>();
                                                    detail.PrecioUnitario = preciosiniva;
                                                    detail.MontoItem = Convert.ToDecimal(montoitem);
                                                    goDoc.DteDetalles.Add(detail);
                                                    contadordetalle = contadordetalle + 1;
                                                }
                                                else
                                                {
                                                    msg = "No es posible superar los 40 Items";
                                                    throw new ApplicationException(msg);
                                                }

                                            }
                                            #endregion,
                                            #region Folio
                                            //this.objLog.writeDTEGetOne("Solicitando Folio");
                                            //folio = this.DTESolicitarFolio(DTEVyV_RutEmisor, posflag);
                                            //this.objLog.writeDTEGetOne("Folio Obtenido " + folio);
                                            //if (folio == "0")
                                            //{
                                            //    //this.objLog.write(folio);
                                            //    flag2 = false;
                                            //    num = 200;
                                            //    status = "999";
                                            //    msg = "No existen folios disponibles";
                                            //    string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                            //    str = string.Concat(textArray5);
                                            //    this.objLog.write("ProcessEvent** Error: " + msg);
                                            //}
                                            //else
                                            //{
                                            //    plant = plant.Replace("#DOCUMENTOID#", "R" + DTEVyV_RutEmisor + "T" + posflag + "F" + folio);
                                            //    plant = plant.Replace("#FOLIO#", folio);
                                            //}
                                            //this.objLog.writeDTEGetOne("Plantilla xml  " + plant);
                                            #endregion
                                            #region Emite Documento
                                            this.objLog.writeDTEGetOne($"Armando Documento");
                                            string fudString = "";
                                            if (!goDoc.GeneraBloqueDTE(out fudString))
                                            {
                                                msg = $"Error al armar documento electrónico - {fudString}";
                                                throw new ApplicationException(msg);
                                            }
                                            else
                                            {
                                                this.objLog.writeDTEGetOne($"Emitiendo Documento");
                                                this.objLog.writeDTEGetOne("FUD:  " + fudString);
                                                string msgErr = "";
                                                string respuesta = "";
                                                string xmlRespuesta = "";
                                                goDoc.EmiteDTE(fudString, out msgErr, out folio, out boletapdf, out ted, out respuesta, out xmlRespuesta);
                                                if (msgErr.ToUpper() != "OK")
                                                {
                                                    msg = $"Error al emitir documento electrónico - {msgErr}";
                                                    this.objLog.writeDTEGetOne(msg);
                                                    throw new ApplicationException(msg);
                                                }
                                            }
                                            #endregion
                                            if (folio != "0")
                                            {
                                                this.objLog.writeDTEGetOne("Folio:  " + folio);
                                                flag2 = false;
                                                num = 200;
                                                status = "0";
                                                //ted = retornaTED(xmlres);
                                                tedbase64 = EncodeStrToBase64(ted);
                                                string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                str = string.Concat(textArray5);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            flag2 = false;
                                            num = 200;
                                            status = "555";
                                            this.objLog.write("EXCEPTION2");
                                            str = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                                            this.objLog.write("ProcessEvent *********** Error General -- " + e.Message);
                                            this.txtMensaje.Text = this.txtMensaje.Text + "\nExcepcion: " + e.Message;
                                        }
                                    }
                                    else
                                    {
                                        flag2 = false;
                                        num = 200;
                                        status = "556";
                                        string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + errors + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                        str = string.Concat(textArray5);
                                        this.objLog.write("ProcessEvent** Error: " + errors);
                                    }
                                }
                            }
                        }
                    }
                    if (((ADirection == "FromClient") && (AHTTPVerb == "POST")) && (AResourceName.ToUpper() == "CALLDTEGETONEVOUCHER"))
                    {
                        str18 = "";
                        str19 = "";
                        str20 = "";
                        str21 = "";
                        string errors = string.Empty;
                        string folio = string.Empty;
                        string plant = string.Empty;
                        string plantdet = string.Empty;
                        string status = string.Empty;
                        string msg = string.Empty;
                        string xmlres = string.Empty;
                        string ted = string.Empty;
                        string tedbase64 = string.Empty;

                        foreach (JObject obj3 in JArray.Parse("[" + message.Payload + "]").Children<JObject>())
                        {
                            foreach (JProperty property in obj3.Properties())
                            {
                                if (property.Name == "documentID")
                                {
                                    token = this.doPRISMLogin(this.objLog);
                                    str21 = (string)property.Value;
                                    JObject json = JObject.Parse(this.GETReceive(str21, token));
                                    errors = json.GetValue("errors").ToString();
                                    if (errors == "")
                                    {
                                        this.objLog.writeDTEGetOne(json.GetValue("data")[0].ToString());
                                        JObject json2 = JObject.Parse(json.GetValue("data")[0].ToString());
                                        string idTienda = json2.GetValue("storeno").ToString();
                                        JObject jsonstore = JObject.Parse(this.GETStore(idTienda, token));
                                        JObject store = JObject.Parse(jsonstore.GetValue("data")[0].ToString());
                                        string DTEVyV_Tienda = store.GetValue("storename").ToString();
                                        string DTEVyV_RutEmisor = store.GetValue("zip").ToString();
                                        string DTEVyV_RznSocEmi = store.GetValue("udf1string").ToString();
                                        string DTEVyV_Giro = store.GetValue("udf2string").ToString();
                                        string DTEVyV_Direccion_tienda = store.GetValue("address1").ToString();
                                        string DTEVyV_DirOrigen = store.GetValue("address4").ToString();
                                        string DTEVyV_CmnaOrigen = store.GetValue("address5").ToString();
                                        string DTEVyV_CiudadOrigen = store.GetValue("address6").ToString();
                                        string DTEVyV_Telefono = store.GetValue("phone2").ToString();
                                        string DTEVyV_Telefono_tienda = store.GetValue("phone1").ToString();

                                        string posflag = "52";
                                        try
                                        {
                                            this.objLog.writeDTEGetOne("Guia de Despacho - Vouchers");
                                            //plantilla = new PlantillaXml();
                                            //plant = this.plantilla.PlantillaCabGuiaDespacho();
                                            #region Creacion de XML Cabecera
                                            //DOCUMENTO
                                            this.objLog.writeDTEGetOne($"Cargando Cabecera");
                                            GetOneDTE.DteDocument goDoc = new GetOneDTE.DteDocument();
                                            goDoc.RequestDomain = this.ParamValues.DTEGetOne_Domain;
                                            goDoc.TipoDTE = Convert.ToInt32(posflag);
                                            goDoc.FechaEmision = DateTime.Now.ToString("yyyy-MM-dd");
                                            goDoc.FechaVencimiento = DateTime.Now.ToString("yyyy-MM-dd");
                                            goDoc.IndTraslado = 7;
                                            //RECEPTOR
                                            this.objLog.writeDTEGetOne($"Cargando Receptor");
                                            goDoc.RutReceptor = json2.GetValue("vendorpostalcode").ToString();
                                            goDoc.RazonSocialReceptor = json2.GetValue("vendoraddress4").ToString();
                                            goDoc.GiroReceptor = LargoCadenaMax((json2.GetValue("vendoraddress5").ToString() + " " + json2.GetValue("vendoraddress6").ToString()).Trim(), 40);
                                            goDoc.DireccionReceptor = json2.GetValue("vendoraddress1").ToString();
                                            goDoc.ComunaReceptor = json2.GetValue("vendoraddress2").ToString();
                                            goDoc.CiudadReceptor = json2.GetValue("vendoraddress3").ToString();

                                            DTEValidaciones(goDoc.RutReceptor, "Rut Receptor", 10, 1);
                                            DTEValidaciones(goDoc.RazonSocialReceptor, "Razon Social Receptor", 100, 1);
                                            DTEValidaciones(goDoc.CiudadReceptor, "Ciudad Receptor", 20, 1);
                                            DTEValidaciones(goDoc.GiroReceptor, "Giro Receptor", 40, 1);
                                            DTEValidaciones(goDoc.DireccionReceptor, "Dirección Receptor", 70, 2);
                                            DTEValidaciones(goDoc.ComunaReceptor, "Comuna Receptor", 20, 1);

                                            goDoc.DireccionDestino = goDoc.DireccionReceptor;
                                            goDoc.ComunaDestino = goDoc.ComunaReceptor;
                                            goDoc.CiudadDestino = goDoc.CiudadReceptor;

                                            goDoc.DireccionOrigen = DTEVyV_DirOrigen;
                                            goDoc.ComunaOrigen = DTEVyV_CmnaOrigen;
                                            goDoc.CiudadOrigen = DTEVyV_CiudadOrigen;
                                            #endregion
                                            #region Creacion Xml Detalle
                                            this.objLog.writeDTEGetOne($"Cargando Detalles");
                                            string jsonitem = json2.GetValue("recvitem").ToString();
                                            JArray jsonArrayitem = JArray.Parse(jsonitem);
                                            int valortotal = 0;
                                            foreach (JObject jsonOperaciones in jsonArrayitem.Children<JObject>())
                                            {
                                                if (contadordetalle < 41)
                                                {
                                                    //plantilla = new PlantillaXml();
                                                    //plantdet = plantdet + this.plantilla.PantillaDetGuiaDespacho();
                                                    //plantdet = plantdet.Replace("#NROLINDET#", (jsonOperaciones["itempos"]).ToString());
                                                    //plantdet = plantdet.Replace("#TPOCODIGO#", "INTERNO");
                                                    //plantdet = plantdet.Replace("#VLRCODIGO#", (jsonOperaciones["alu"]).ToString());
                                                    DTEValidaciones((jsonOperaciones["alu"]).ToString(), "Código Item", 35, 1);
                                                    //plantdet = plantdet.Replace("#NMBITEM#", (jsonOperaciones["description1"]).ToString());
                                                    DTEValidaciones((jsonOperaciones["description1"]).ToString(), "Nombre Item", 70, 1);
                                                    //plantdet = plantdet.Replace("#DESC#", (jsonOperaciones["description1"]).ToString());
                                                    //plantdet = plantdet.Replace("#QTYITEM#", (jsonOperaciones["qty"]).ToString());
                                                    int ivadet = jsonOperaciones["price"].Value<Int32>() * Int32.Parse(this.ParamValues.DTEGetOne_TasaIVA) / 100;
                                                    decimal cantidad = jsonOperaciones["qty"].Value<Decimal>();
                                                    decimal precio = jsonOperaciones["price"].Value<Decimal>();
                                                    decimal montoitem = (precio * cantidad) - (ivadet * cantidad);
                                                    //plantdet = plantdet.Replace("#MONTOITEM#", montoitem.ToString());
                                                    valortotal = valortotal + Int32.Parse((jsonOperaciones["price"]).ToString());
                                                    int preciosiniva = Convert.ToInt32(precio - ivadet);
                                                    //plantdet = plantdet.Replace("#PRCITEM#", preciosiniva.ToString());
                                                    DTEValidaciones(preciosiniva.ToString(), "Precio Item", 19, 2);

                                                    GetOneDTE.DteDocument.DteDetail detail = new GetOneDTE.DteDocument.DteDetail();
                                                    detail.NroLinea = jsonOperaciones["itempos"].Value<Int32>();
                                                    detail.Cantidad = jsonOperaciones["qty"].Value<Decimal>();
                                                    detail.CodigoItem = jsonOperaciones["alu"].Value<String>();
                                                    detail.UnidadMedida = "Un.";
                                                    detail.TipoCodigo = "INTERNO";
                                                    detail.NombreItem = jsonOperaciones["description1"].Value<String>();
                                                    detail.PrecioUnitario = preciosiniva;
                                                    detail.MontoItem = Convert.ToDecimal(montoitem);
                                                    goDoc.DteDetalles.Add(detail);
                                                    contadordetalle = contadordetalle + 1;
                                                }
                                                else
                                                {
                                                    msg = "No es posible superar los 40 Items";
                                                    throw new ApplicationException(msg);
                                                }
                                            }
                                            //TOTALES
                                            this.objLog.writeDTEGetOne($"Cargando Totales");
                                            int iva = (valortotal * Int32.Parse(this.ParamValues.DTEGetOne_TasaIVA)) / 100;
                                            int neto = valortotal - iva;

                                            goDoc.MontoExento = 0;
                                            goDoc.IVA = iva;
                                            goDoc.MontoTotal = valortotal;
                                            goDoc.MontoNeto = neto;
                                            goDoc.TasaIVA = Convert.ToDecimal(this.ParamValues.DTEGetOne_TasaIVA);

                                            DTEValidaciones(neto.ToString(), "Monto Neto", 18, 3);
                                            DTEValidaciones(this.ParamValues.DTEVyV_TasaIVA, "Tasa IVA", 6, 3);
                                            DTEValidaciones(iva.ToString(), "IVA", 18, 3);
                                            DTEValidaciones(valortotal.ToString(), "Monto Total", 18, 3);
                                            #endregion
                                            #region Emite Documento
                                            this.objLog.writeDTEGetOne($"Armando Documento");
                                            string fudString = "";
                                            if (!goDoc.GeneraBloqueDTE(out fudString))
                                            {
                                                msg = $"Error al armar documento electrónico - {fudString}";
                                                throw new ApplicationException(msg);
                                            }
                                            else
                                            {
                                                this.objLog.writeDTEGetOne($"Emitiendo Documento");
                                                this.objLog.writeDTEGetOne("FUD:  " + fudString);
                                                string msgErr = "";
                                                string respuesta = "";
                                                string xmlRespuesta = "";
                                                goDoc.EmiteDTE(fudString, out msgErr, out folio, out boletapdf, out ted, out respuesta, out xmlRespuesta);
                                                if (msgErr.ToUpper() != "OK")
                                                {
                                                    msg = $"Error al emitir documento electrónico - {msgErr}";
                                                    this.objLog.writeDTEGetOne(msg);
                                                    this.objLog.writeDTEGetOne(respuesta);
                                                    throw new ApplicationException(msg);
                                                }
                                            }
                                            #endregion
                                            if (folio != "0")
                                            {
                                                this.objLog.writeDTEGetOne("Folio:  " + folio);
                                                flag2 = false;
                                                num = 200;
                                                status = "0";
                                                //ted = retornaTED(xmlres);
                                                tedbase64 = EncodeStrToBase64(ted);
                                                string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + msg + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                str = string.Concat(textArray5);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            flag2 = false;
                                            num = 200;
                                            status = "555";
                                            this.objLog.write("EXCEPTION2");
                                            str = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                                            this.objLog.write("ProcessEvent *********** Error General -- " + e.Message);
                                            this.txtMensaje.Text = this.txtMensaje.Text + "\nExcepcion: " + e.Message;
                                        }
                                    }
                                    else
                                    {
                                        flag2 = false;
                                        num = 200;
                                        status = "556";
                                        string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + errors + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                        str = string.Concat(textArray5);
                                        this.objLog.write("ProcessEvent** Error: " + errors);
                                    }
                                }
                            }
                        }
                        this.objLog.write("SALIMOS");
                    }
					if (((ADirection == "FromClient") && (AHTTPVerb == "POST")) && (AResourceName.ToUpper() == "CALLDTEFACELE"))
                    {
                        this.objLog.write("INICIO  CALLDTEFACELE");
                        str18 = "";
                        str19 = "";
                        str20 = "";
                        str21 = "";
                        string validacion = string.Empty;
                        string errors = string.Empty;
                        string numdoccliente = string.Empty;
                        string folio = string.Empty;
                        string plant = string.Empty;
                        string plantdet = string.Empty;
                        string plantref = string.Empty;
                        string status = string.Empty;
                        string msg = string.Empty;
                        string posflagtipo = string.Empty;
                        string xmlres = string.Empty;
                        string ted = string.Empty;
                        string tedbase64 = string.Empty;
                        ImpresoraOPOS resImpresion = new ImpresoraOPOS();
                        //this.objLog.write("1");

                        foreach (JObject obj3 in JArray.Parse("[" + message.Payload + "]").Children<JObject>())
                        {
                            //this.objLog.write("2");
                            foreach (JProperty property in obj3.Properties())
                            {
                                //this.objLog.write("3");
                                if (property.Name == "documentID")
                                {
                                    //this.objLog.write("4");
                                    token = this.doPRISMLoginAux(this.objLog);
                                    //this.objLog.write("5");
                                    str21 = (string)property.Value;
                                    JObject json = JObject.Parse(this.GETDocument(str21, token));
                                    //this.objLog.write("6");
                                    errors = json.GetValue("errors").ToString();
                                    //this.objLog.write("7");
                                    if (errors == "")
                                    {
                                        //this.objLog.write("8");
                                        JObject json2 = JObject.Parse(json.GetValue("data")[0].ToString());
                                        this.objLog.write($"------------------ json2 -----------------");
                                        this.objLog.write($"{JsonConvert.SerializeObject(json2, Newtonsoft.Json.Formatting.Indented)}");
                                        this.objLog.write($"------------------ json2 -----------------");
                                        string idTienda = json2.GetValue("storeno").ToString();
                                        JObject jsonstore = JObject.Parse(this.GETStore(idTienda, token));
                                        JObject store = JObject.Parse(jsonstore.GetValue("data")[0].ToString());
                                        string DTEVyV_Tienda = store.GetValue("storename").ToString();
                                        string DTEVyV_RutEmisor = store.GetValue("zip").ToString().ToUpper();
                                        string DTEVyV_RznSocEmi = store.GetValue("udf1string").ToString();
                                        string DTEVyV_Giro = store.GetValue("udf2string").ToString();
                                        string DTEVyV_Direccion_tienda = store.GetValue("address1").ToString();
                                        string DTEVyV_DirOrigen = store.GetValue("address4").ToString();
                                        string DTEVyV_CmnaOrigen = store.GetValue("address5").ToString();
                                        string DTEVyV_CiudadOrigen = store.GetValue("address6").ToString();
                                        string DTEVyV_Telefono = store.GetValue("phone2").ToString();
                                        string DTEVyV_Telefono_tienda = store.GetValue("phone1").ToString();
                                        string DTEVyV_codigo_sii_sucursal = "999999999";// store.GetValue("udf4string").ToString();
                                        FaceleDocument faceleDoc = new FaceleDocument(this.ParamValues.DTEFacele_WSurl, DTEVyV_RutEmisor);
                                        this.objLog.writeDTEFacele($"Facele WebService URL: {FaceleProcessor.ObtieneURLWS()}");
                                        //this.objLog.write("9");

                                        string posflag = json2.GetValue("posflag1").ToString().Trim().Substring(0, 2);
                                        string idcliente = json2.GetValue("btcuid").ToString();
                                        JArray jsoncustomerRest1 = JArray.Parse(this.GETCustomerRest(idcliente, token));
                                        JObject jsoncustomerRest = JObject.Parse(jsoncustomerRest1[0].ToString());
                                        JObject jsoncustomer = JObject.Parse(this.GETCustomer(idcliente, token));
                                        JObject jsoncustomer2 = JObject.Parse(jsoncustomer.GetValue("data")[0].ToString());
                                        numdoccliente = jsoncustomerRest.GetValue("info1").ToString().ToUpper();
                                        if (numdoccliente == "")
                                        {
                                            numdoccliente = jsoncustomerRest.GetValue("info2").ToString().ToUpper();
                                            if (numdoccliente == "")
                                            {
                                                numdoccliente = this.ParamValues.DTEFacele_RutReceptordefault.ToUpper();
                                            }
                                        }

                                        //this.objLog.write("10");
                                        if (posflag == "39")// BOLETA ELECTRONICA FACELE
                                        {
                                            try
                                            {
                                                this.objLog.writeDTEFacele("Procesando Boleta");
                                                #region PROCESO BOLETA ELEC
                                                plant = FacelePlantillaXml.PlantillaCabBoleta();
                                                bool aplicaDcto1Peso = false;

                                                #region Creacion de XML Cabecera
                                                //CARATULA
                                                plant = plant.Replace("#RUTEMISOR#", DTEVyV_RutEmisor);
                                                plant = plant.Replace("#RUTENVIA#", DTEVyV_RutEmisor);
                                                plant = plant.Replace("#RUTRECEPTOR#", numdoccliente);
                                                DTEValidaciones(numdoccliente, "Rut Receptor", 10, 1);
                                                plant = plant.Replace("#FECHARESOL#", this.ParamValues.DTEFacele_FechaResol);
                                                plant = plant.Replace("#NUMRESOL#", this.ParamValues.DTEFacele_NumResol);
                                                plant = plant.Replace("#FIRMAEVN#", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Replace(" ", "T"));
                                                plant = plant.Replace("#TPODTE#", posflag);
                                                //DOCUMENTO
                                                plant = plant.Replace("#FECEMISION#", DateTime.Now.ToString("yyyy-MM-dd"));
                                                plant = plant.Replace("#INDSERVICIO#", "1");
                                                plant = plant.Replace("#FECVENC#", DateTime.Now.ToString("yyyy-MM-dd"));
                                                //EMISOR
                                                plant = plant.Replace("#RZNSOC#", DTEVyV_RznSocEmi);
                                                DTEValidaciones(DTEVyV_RznSocEmi, "Razon Social Emisor", 100, 1);
                                                plant = plant.Replace("#GIRO#", LargoCadenaMax(DTEVyV_Giro, 80));
                                                DTEValidaciones(LargoCadenaMax(DTEVyV_Giro, 80), "Giro Emisor", 80, 1);
                                                plant = plant.Replace("#CDGSIISUCUR#", DTEVyV_codigo_sii_sucursal);
                                                DTEValidaciones(DTEVyV_codigo_sii_sucursal, "Sucursal Emisor", 20, 1);
                                                plant = plant.Replace("#DIRORIGEN#", DTEVyV_DirOrigen);
                                                plant = plant.Replace("#CMNAORIGEN#", DTEVyV_CmnaOrigen);
                                                plant = plant.Replace("#CIUDADORIGEN#", DTEVyV_CiudadOrigen);
                                                //RECEPTOR
                                                string nombrecliente = jsoncustomerRest.GetValue("first_name").ToString() + " " + jsoncustomerRest.GetValue("last_name").ToString();
                                                if (nombrecliente == "")
                                                {
                                                    plant = plant.Replace("#RZNSOCRECEP#", this.ParamValues.DTEFacele_NombreClientedefault);
                                                    DTEValidaciones(this.ParamValues.DTEFacele_NombreClientedefault, "Razón Social Receptor", 100, 1);
                                                }
                                                else
                                                {
                                                    plant = plant.Replace("#RZNSOCRECEP#", nombrecliente);
                                                    DTEValidaciones(nombrecliente, "Razon Social Receptor", 100, 2);
                                                }
                                                plant = plant.Replace("#DIRRECEP#", jsoncustomerRest.GetValue("primary_address_line_1").ToString() + " " + jsoncustomerRest.GetValue("primary_address_line_2").ToString());
                                                plant = plant.Replace("#CMNARECEP#", jsoncustomerRest.GetValue("primary_address_line_4").ToString());
                                                plant = plant.Replace("#CIUDADRECEP#", jsoncustomerRest.GetValue("primary_address_line_5").ToString());
                                                //TOTALES
                                                if (Convert.ToDecimal(json2.GetValue("saletotalamt").ToString()) - Convert.ToDecimal(json2.GetValue("returnsubtotal").ToString()) <= 1)
                                                {
                                                    aplicaDcto1Peso = true;
                                                    plant = plant.Replace("<MntExe>#MNTEXE#</MntExe>", "");
                                                    plant = plant.Replace("<IVA>#IVA#</IVA>", "");
                                                    plant = plant.Replace("#MNTTOTAL#", "1");
                                                    plant = plant.Replace("#MNTNETO#", "1");
                                                }
                                                else
                                                {
                                                    decimal var_saletotaltaxamt = Convert.ToDecimal(json2.GetValue("saletotaltaxamt").ToString());
                                                    decimal var_returntax1amt = Convert.ToDecimal(json2.GetValue("returntax1amt").ToString());
                                                    decimal var_saletotalamt = Convert.ToDecimal(json2.GetValue("saletotalamt").ToString());
                                                    decimal var_returnsubtotal = Convert.ToDecimal(json2.GetValue("returnsubtotal").ToString());

                                                    plant = plant.Replace("#MNTEXE#", "0");
                                                    plant = plant.Replace("#IVA#", Decimal.Round(var_saletotaltaxamt - var_returntax1amt).ToString());
                                                    DTEValidaciones(Decimal.Round(var_saletotaltaxamt - var_returntax1amt).ToString(), "IVA", 18, 1);
                                                    plant = plant.Replace("#MNTTOTAL#", Decimal.Round(var_saletotalamt - var_returnsubtotal).ToString());
                                                    DTEValidaciones(Decimal.Round(var_saletotalamt - var_returnsubtotal).ToString(), "Monto Total", 18, 1);
                                                    int neto = Int32.Parse(Decimal.Round(var_saletotalamt - var_returnsubtotal).ToString()) - Int32.Parse(Decimal.Round(var_saletotaltaxamt - var_returntax1amt).ToString());
                                                    plant = plant.Replace("#MNTNETO#", neto.ToString());
                                                    DTEValidaciones(neto.ToString(), "Monto Neto", 18, 1);
                                                }
                                                #endregion
                                                #region Creacion Xml Detalle
                                                string strjson3 = json2.GetValue("docitem").ToString();
                                                JArray jsonArray = JArray.Parse(strjson3);
                                                int numeroLinea = 0;
                                                //int montoBoletaCambio = 0;
                                                //int descuentoGlobalIVA = 0;
                                                //int descuentoGlobalNETO = 0;
                                                foreach (JObject jsonOperaciones in jsonArray.Children<JObject>())
                                                {
                                                    if (contadordetalle < 61)
                                                    {
                                                        string kitflag = "0";
                                                        try
                                                        {
                                                            kitflag = (jsonOperaciones["kitflag"]).ToString();
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            kitflag = "0";
                                                        }
                                                        if (!"5".Equals(kitflag))
                                                        {
                                                            if((jsonOperaciones["itemtype"]).ToString() == "2")
                                                            {
                                                                //string _precio = Decimal.Round(Convert.ToDecimal((jsonOperaciones["price"]).ToString())).ToString();
                                                                //string _cantidad = (jsonOperaciones["qty"]).ToString();
                                                                //string _cantidad_decimal = _cantidad.Replace(".", ",");
                                                                //montoBoletaCambio += Convert.ToInt32(Int32.Parse(_precio) * Convert.ToDouble(_cantidad_decimal));
                                                                //descuentoGlobalIVA  += Convert.ToInt32(Decimal.Round(Convert.ToDecimal((jsonOperaciones["taxamt"]).ToString())) * Convert.ToDecimal(_cantidad_decimal));
                                                                //descuentoGlobalNETO += montoBoletaCambio - descuentoGlobalIVA;
                                                                continue;
                                                            }
                                                            string descuento = string.Empty;
                                                            plantilla = new PlantillaXml();
                                                            plantdet = plantdet + FacelePlantillaXml.PlantillaDetBoleta();
                                                            //plantdet = plantdet.Replace("#NROLINDET#", (jsonOperaciones["itempos"]).ToString());
                                                            numeroLinea++;
                                                            plantdet = plantdet.Replace("#NROLINDET#", numeroLinea.ToString());
                                                            plantdet = plantdet.Replace("#TPOCODIGO#", "INTERNO");
                                                            plantdet = plantdet.Replace("#VLRCODIGO#", (jsonOperaciones["alu"]).ToString());
                                                            DTEValidaciones(jsonOperaciones["alu"].ToString(), "Valor Código", 35, 1);
                                                            plantdet = plantdet.Replace("#NMBITEM#", (jsonOperaciones["description1"]).ToString());
                                                            DTEValidaciones(jsonOperaciones["description1"].ToString(), "Nombre Item", 70, 1);
                                                            plantdet = plantdet.Replace("#DESC#", (jsonOperaciones["description1"]).ToString());
                                                            string cantidad = (jsonOperaciones["qty"]).ToString();
                                                            cantidad = cantidad.Replace(',', '.');
                                                            plantdet = plantdet.Replace("#QTYITEM#", cantidad);
                                                            plantdet = plantdet.Replace("#UNMDITEM#", "Un.");
                                                            plantdet = plantdet.Replace("#PRCITEM#", Decimal.Round(Convert.ToDecimal((jsonOperaciones["origprice"]).ToString())).ToString());
                                                            DTEValidaciones(Decimal.Round(Convert.ToDecimal((jsonOperaciones["origprice"]).ToString())).ToString(), "Precio Item", 19, 2);
                                                            string iva = (jsonOperaciones["taxamt"]).ToString().Split(',')[0];
                                                            string precio = Decimal.Round(Convert.ToDecimal((jsonOperaciones["price"]).ToString())).ToString();
                                                            //int montoitem = ((Int32.Parse(precio) * Int32.Parse(cantidad)) - (Int32.Parse(iva) * Int32.Parse(cantidad)));
                                                            string cantidad_decimal = cantidad.Replace(".", ",");
                                                            double montoitem = (Int32.Parse(precio) * Convert.ToDouble(cantidad_decimal));
                                                            plantdet = plantdet.Replace("#MONTOITEM#", Decimal.Round(Convert.ToDecimal(montoitem.ToString())).ToString());
                                                            try
                                                            {
                                                                descuento = Decimal.Round(Convert.ToDecimal((jsonOperaciones["discamt"]).ToString())).ToString();
                                                                if (descuento != string.Empty && descuento != "0")
                                                                {
                                                                    decimal descuento_decimal = Convert.ToDecimal(descuento);
                                                                    if (descuento_decimal < 0)
                                                                    {
                                                                        descuento_decimal = descuento_decimal * -1;
                                                                        descuento = descuento_decimal.ToString();
                                                                    }
                                                                    descuento = "<DescuentoMonto>" + descuento + "</DescuentoMonto></Detalle>";
                                                                    plantdet = plantdet.Replace("#DESCUENTO#", descuento);
                                                                }
                                                                else
                                                                {
                                                                    descuento = "</Detalle>";
                                                                    plantdet = plantdet.Replace("#DESCUENTO#", descuento);
                                                                }

                                                            }
                                                            catch
                                                            {
                                                                descuento = "</Detalle>";
                                                                plantdet = plantdet.Replace("#DESCUENTO#", descuento);
                                                            }
                                                            contadordetalle = contadordetalle + 1;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        msg = "No es posible superar los 60 Items";
                                                        throw new ApplicationException(msg);
                                                    }
                                                }
                                                plant = plant.Replace("#DETALLE#", plantdet);
                                                //agregar referencia de cliente cuando tenga correo.
                                                //agregar referencia de devolucion.

                                                #endregion
                                                #region Boleta de cambio
                                                if(Convert.ToDecimal(json2.GetValue("returnsubtotal").ToString()) > 0)
                                                {
                                                    decimal descuentoFinal = Convert.ToDecimal(json2.GetValue("returnsubtotal").ToString());
                                                    if (aplicaDcto1Peso)
                                                    {
                                                        descuentoFinal = descuentoFinal - 1;
                                                    }

                                                    string plantcambio = FacelePlantillaXml.PlantillaDescuentosGlobales();
                                                    plantcambio = plantcambio.Replace("#NROLINEA#", "1");
                                                    plantcambio = plantcambio.Replace("#GLOSA#", "CAMBIO");
                                                    plantcambio = plantcambio.Replace("#MONTO#", descuentoFinal.ToString());

                                                    plant = plant.Replace("#DSCGLOBAL#", plantcambio);
                                                }
                                                else
                                                {
                                                    plant = plant.Replace("#DSCGLOBAL#", "");
                                                }
                                                #endregion
                                                #region Totales
                                                ////TOTALES
                                                //if (json2.GetValue("saletotalamt").ToString() == "0")
                                                //{
                                                //    plant = plant.Replace("#MNTEXE#", "0");
                                                //    plant = plant.Replace("#IVA#", "0");
                                                //    plant = plant.Replace("#MNTTOTAL#", "1");
                                                //    plant = plant.Replace("#MNTNETO#", "1");
                                                //}
                                                //else
                                                //{
                                                //    if(Decimal.Round(Convert.ToDecimal(json2.GetValue("saletotalamt").ToString()) - montoBoletaCambio) == 0)
                                                //    {
                                                //        plant = plant.Replace("<MntExe>#MNTEXE#</MntExe>", "");
                                                //        plant = plant.Replace("<IVA>#IVA#</IVA>", "");
                                                //        plant = plant.Replace("#MNTTOTAL#", "1");
                                                //        plant = plant.Replace("#MNTNETO#", "1");
                                                //    }
                                                //    else
                                                //    {
                                                //        plant = plant.Replace("#MNTEXE#", "0");
                                                //        plant = plant.Replace("#IVA#", Decimal.Round(Convert.ToDecimal(json2.GetValue("saletotaltaxamt").ToString()) - descuentoGlobalIVA).ToString());
                                                //        DTEValidaciones(Decimal.Round(Convert.ToDecimal(json2.GetValue("saletotaltaxamt").ToString())).ToString(), "IVA", 18, 1);
                                                //        plant = plant.Replace("#MNTTOTAL#", Decimal.Round(Convert.ToDecimal(json2.GetValue("saletotalamt").ToString()) - montoBoletaCambio).ToString());
                                                //        DTEValidaciones(Decimal.Round(Convert.ToDecimal(json2.GetValue("saletotalamt").ToString())).ToString(), "Monto Total", 18, 1);
                                                //        int neto = Int32.Parse(Decimal.Round(Convert.ToDecimal(json2.GetValue("saletotalamt").ToString())).ToString()) - Int32.Parse(Decimal.Round(Convert.ToDecimal(json2.GetValue("saletotaltaxamt").ToString())).ToString());
                                                //        plant = plant.Replace("#MNTNETO#", (neto - descuentoGlobalNETO).ToString());
                                                //        DTEValidaciones(neto.ToString(), "Monto Neto", 18, 1);
                                                //    }
                                                //}
                                                #endregion
                                                #region GeneraDTE
                                                faceleDoc.tipoDTE = 39;
                                                faceleDoc.xml = plant;

                                                string sep = "-------------------------";
                                                //this.objLog.writeDTEFacele($"{sep} FaceleDocument: Antes de GeneraDTE {sep}");
                                                //this.objLog.writeDTEFacele(JsonConvert.SerializeObject(faceleDoc,Newtonsoft.Json.Formatting.Indented));
                                                FaceleProcessor.GeneraDTE(ref faceleDoc);

                                                this.objLog.writeDTEFacele($"estadoOperacion: {faceleDoc.estadoOperacion} | descripcionOperacion: {faceleDoc.descripcionOperacion}");
                                                if (faceleDoc.estadoOperacion == 0)//ERROR AL EMITIR
                                                {
                                                    this.objLog.writeDTEFacele($"XmlGenerado: {faceleDoc.xml}");
                                                    flag2 = false;
                                                    num = 200;
                                                    string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + "1" + "\"," + "\"FolioNo\":" + "\"" + faceleDoc.folioDTE + "\"," + "\"MsgEstatus\":" + "\"" + faceleDoc.descripcionOperacion + "\"," + "\"TED\":" + "\"" + faceleDoc.ted + "\"" + "}" };
                                                    str = string.Concat(textArray5);
                                                    this.objLog.write("ProcessEvent** Error: " + "Estatus fuera de rango" + msg);
                                                }
                                                else
                                                {
                                                    //this.objLog.writeDTEFacele($"{sep} FaceleDocument: Antes de ConfirmaDTE {sep}");
                                                    //this.objLog.writeDTEFacele(JsonConvert.SerializeObject(faceleDoc, Newtonsoft.Json.Formatting.Indented));
                                                    FaceleProcessor.ConfirmaDTE(ref faceleDoc);
                                                    this.objLog.writeDTEFacele($"{sep} FaceleDocument: Despues de ConfirmaDTE {sep}");
                                                    this.objLog.writeDTEFacele(JsonConvert.SerializeObject(faceleDoc, Newtonsoft.Json.Formatting.Indented));

                                                    flag2 = false;
                                                    num = 200;

                                                    this.objLog.writeDTEFacele(faceleDoc.responseXml);
                                                    ted = retornaTED(faceleDoc.responseXml);
                                                    tedbase64 = EncodeStrToBase64(ted);

                                                    string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + "0" + "\"," + "\"FolioNo\":" + "\"" + faceleDoc.folioDTE + "\"," + "\"MsgEstatus\":" + "\"" + faceleDoc.descripcionOperacion + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                    str = string.Concat(textArray5);
                                                    this.objLog.writeDTEFacele($"Retornando resultadoString a Prism: {str}");
                                                }
                                                #endregion
                                                #endregion
                                            }
                                            catch (Exception e)
                                            {
                                                flag2 = false;
                                                num = 200;
                                                status = "555";
                                                this.objLog.write("EXCEPTION2");
                                                str = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                                                this.objLog.write("ProcessEvent *********** Error General -- " + e.Message);
                                                this.txtMensaje.Text = this.txtMensaje.Text + "\nExcepcion: " + e.Message;
                                            }
                                        }
                                        else if (posflag == "33")// FACTURA ELECTRONICA FACELE
                                        {
                                            //posflagtipo = "FACTURA ELECTRONICA";},
                                            this.objLog.writeDTEFacele("Procesando Factura");
                                            try
                                            {
                                                #region PROCESO FACTURA ELECTRONICA
                                                plant = FacelePlantillaXml.PantillaCabFactura();
                                                #region Creacion de XML Cabecera
                                                //this.objLog.write("llegue al documento");
                                                //DOCUMENTO
                                                //plant = plant.Replace("#DOCUMENTOID#", "R" + this.ParamValues.DTEVyV_RutEmisor + "T" + posflag + "F" + folio);
                                                plant = plant.Replace("#TPODTE#", posflag);
                                                //plant = plant.Replace("#FOLIO#", folio);
                                                plant = plant.Replace("#FECEMISION#", DateTime.Now.ToString("yyyy-MM-dd"));
                                                plant = plant.Replace("#FMAPAGO#", "2");
                                                plant = plant.Replace("#MEDIOPAGO#", "EF");
                                                plant = plant.Replace("#FECVENC#", DateTime.Now.ToString("yyyy-MM-dd"));
                                                plant = plant.Replace("#TERMPAGOGLOSA#", "dias");
                                                plant = plant.Replace("#TERMPAGODIAS#", "30");
                                                //this.objLog.write("llegue al emisor");
                                                //EMISOR
                                                plant = plant.Replace("#RUTEMISOR#", DTEVyV_RutEmisor);
                                                plant = plant.Replace("#RZNSOC#", DTEVyV_RznSocEmi);
                                                DTEValidaciones(DTEVyV_RznSocEmi, "Razon Social Emisor", 100, 1);
                                                plant = plant.Replace("#GIRO#", LargoCadenaMax(DTEVyV_Giro, 80));
                                                DTEValidaciones(LargoCadenaMax(DTEVyV_Giro, 80), "Giro Emisor", 80, 1);
                                                plant = plant.Replace("#ACTECO#", "513100");// this.ParamValues.DTEFacele_ACTECO);
                                                plant = plant.Replace("#CDGSIISUCUR#", DTEVyV_codigo_sii_sucursal);
                                                DTEValidaciones(DTEVyV_codigo_sii_sucursal, "Sucursal Emisor", 20, 1);
                                                plant = plant.Replace("#DIRORIGEN#", DTEVyV_DirOrigen);
                                                plant = plant.Replace("#CMNAORIGEN#", DTEVyV_CmnaOrigen);
                                                plant = plant.Replace("#CIUDADORIGEN#", DTEVyV_CiudadOrigen);
                                                plant = plant.Replace("#CDGVENDEDOR#", json2.GetValue("employee1name").ToString());
                                                //RECEPTOR
                                                plant = plant.Replace("#RUTRECEPTOR#", numdoccliente);
                                                string cdgIntReceptor = numdoccliente.Substring(0, numdoccliente.IndexOf('-')) + "-C";
                                                plant = plant.Replace("#CDGINTRECEPTOR#", cdgIntReceptor);
                                                DTEValidaciones(numdoccliente, "Rut Receptor", 10, 1);
                                                string nombrecliente = jsoncustomerRest.GetValue("first_name").ToString() + " " + jsoncustomerRest.GetValue("last_name").ToString();
                                                if (nombrecliente == "")
                                                {
                                                    plant = plant.Replace("#RZNSOCRECEP#", this.ParamValues.DTEFacele_NombreClientedefault);
                                                    DTEValidaciones(this.ParamValues.DTEFacele_NombreClientedefault, "Razon Social Receptor", 100, 1);
                                                }
                                                else
                                                {
                                                    plant = plant.Replace("#RZNSOCRECEP#", nombrecliente);
                                                    DTEValidaciones(nombrecliente, "Razon Social Receptor", 100, 2);
                                                }
                                                string giro = jsoncustomerRest.GetValue("notes").ToString();
                                                if (giro == "")
                                                {
                                                    plant = plant.Replace("#GIRORECEP#", LargoCadenaMax(this.ParamValues.DTEFacele_GiroClientedefault, 40));
                                                    DTEValidaciones(LargoCadenaMax(this.ParamValues.DTEFacele_GiroClientedefault, 40), "Giro Receptor", 40, 1);
                                                }
                                                else
                                                {
                                                    plant = plant.Replace("#GIRORECEP#", LargoCadenaMax(giro, 40));
                                                    DTEValidaciones(LargoCadenaMax(giro, 40), "Giro Receptor", 40, 2);
                                                }
                                                //this.objLog.write("sali del giro");
                                                plant = plant.Replace("#DIRRECEP#", jsoncustomerRest.GetValue("primary_address_line_1").ToString() + " " + jsoncustomerRest.GetValue("primary_address_line_2").ToString());
                                                DTEValidaciones(jsoncustomerRest.GetValue("primary_address_line_1").ToString() + " " + jsoncustomerRest.GetValue("primary_address_line_2").ToString(), "Dirección Receptor", 70, 2);
                                                plant = plant.Replace("#CMNARECEP#", jsoncustomerRest.GetValue("primary_address_line_4").ToString());
                                                DTEValidaciones(jsoncustomerRest.GetValue("primary_address_line_4").ToString(), "Comuna Receptor", 20, 1);
                                                plant = plant.Replace("#CIUDADRECEP#", jsoncustomerRest.GetValue("primary_address_line_5").ToString());
                                                DTEValidaciones(jsoncustomerRest.GetValue("primary_address_line_5").ToString(), "Ciudad Receptor", 20, 1);
                                                //this.objLog.write("llegue a los totales");
                                                //TOTALES
                                                //el monto neto se calculara en base a la sumatario de cada Item
                                                // int neto = Int32.Parse(json2.GetValue("saletotalamt").ToString()) - Int32.Parse(json2.GetValue("saletotaltaxamt").ToString());
                                                // plant = plant.Replace("#MNTNETO#", neto.ToString());
                                                // DTEValidaciones(neto.ToString(), "Monto Neto", 18, 3);
                                                plant = plant.Replace("#MNTEXE#", "0");
                                                plant = plant.Replace("#TASAIVA#", this.ParamValues.DTEFacele_TasaIVA);
                                                DTEValidaciones(this.ParamValues.DTEFacele_TasaIVA, "Tasa IVA", 6, 3);
                                                //plant = plant.Replace("#IVA#", json2.GetValue("saletotaltaxamt").ToString());
                                                //DTEValidaciones(json2.GetValue("saletotaltaxamt").ToString(), "IVA", 18, 3);
                                                plant = plant.Replace("#MNTTOTAL#", json2.GetValue("saletotalamt").ToString());
                                                DTEValidaciones(json2.GetValue("saletotalamt").ToString(), "Monto Total", 18, 3);
                                                //this.objLog.write("sali del total");
                                                #endregion
                                                #region Creacion Xml Detalle
                                                string strjson3 = json2.GetValue("docitem").ToString();
                                                JArray jsonArray = JArray.Parse(strjson3);
                                                //this.objLog.write("entrare al detalle");
                                                Decimal monto_neto_suma_detalle = 0;
                                                int numeroLinea = 0;
                                                foreach (JObject jsonOperaciones in jsonArray.Children<JObject>())
                                                {
                                                    if (contadordetalle < 41)
                                                    {
                                                        string kitflag = "0";
                                                        try
                                                        {
                                                            kitflag = (jsonOperaciones["kitflag"]).ToString();
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            kitflag = "0";
                                                        }
                                                        if (!"5".Equals(kitflag))
                                                        {
                                                            string descuento = string.Empty;
                                                            plantdet = plantdet + FacelePlantillaXml.PantillaDetFactura();
                                                            //plantdet = plantdet.Replace("#NROLINDET#", (jsonOperaciones["itempos"]).ToString());
                                                            numeroLinea++;
                                                            plantdet = plantdet.Replace("#NROLINDET#", numeroLinea.ToString());
                                                            plantdet = plantdet.Replace("#TPOCODIGO#", "INTERNO");
                                                            plantdet = plantdet.Replace("#VLRCODIGO#", (jsonOperaciones["alu"]).ToString());
                                                            DTEValidaciones(jsonOperaciones["alu"].ToString(), "Código Item", 35, 1);
                                                            plantdet = plantdet.Replace("#NMBITEM#", (jsonOperaciones["description1"]).ToString());
                                                            DTEValidaciones(jsonOperaciones["description1"].ToString(), "Nombre Item", 70, 1);
                                                            plantdet = plantdet.Replace("#DESC#", (jsonOperaciones["description1"]).ToString());
                                                            string cantidad = (jsonOperaciones["qty"]).ToString();
                                                            cantidad = cantidad.Replace(',', '.');
                                                            plantdet = plantdet.Replace("#QTYITEM#", cantidad);
                                                            string iva = (jsonOperaciones["taxamt"]).ToString().Split(',')[0];
                                                            string precio = (jsonOperaciones["price"]).ToString();
                                                            string cantidad_decimal = cantidad.Replace(".", ",");
                                                            double montoitem = ((Int32.Parse(precio) * Convert.ToDouble(cantidad_decimal)) - (Int32.Parse(iva) * Convert.ToDouble(cantidad_decimal)));
                                                            plantdet = plantdet.Replace("#MONTOITEM#", Decimal.Round(Convert.ToDecimal(montoitem.ToString())).ToString());
                                                            monto_neto_suma_detalle = monto_neto_suma_detalle + Decimal.Round(Convert.ToDecimal(montoitem.ToString()));
                                                            string precioori = (jsonOperaciones["origprice"]).ToString();
                                                            int preciosiniva = (Int32.Parse(precioori) - Int32.Parse(iva));
                                                            plantdet = plantdet.Replace("#PRCITEM#", preciosiniva.ToString());
                                                            DTEValidaciones(preciosiniva.ToString(), "Precio Item", 19, 2);
                                                            try
                                                            {
                                                                descuento = (jsonOperaciones["discamt"]).ToString();
                                                                if (descuento != string.Empty && descuento != "0")
                                                                {
                                                                    decimal descuento_decimal = Convert.ToDecimal(descuento);
                                                                    if (descuento_decimal < 0)
                                                                    {
                                                                        descuento_decimal = descuento_decimal * -1;
                                                                        descuento = descuento_decimal.ToString();
                                                                    }

                                                                    int desc = Int32.Parse(descuento) - ((Int32.Parse(descuento) * Int32.Parse(this.ParamValues.DTEFacele_TasaIVA)) / 100);
                                                                    descuento = "<DescuentoMonto>" + desc.ToString() + "</DescuentoMonto></Detalle>";
                                                                    plantdet = plantdet.Replace("#DESCUENTO#", descuento);
                                                                }
                                                                else
                                                                {
                                                                    descuento = "</Detalle>";
                                                                    plantdet = plantdet.Replace("#DESCUENTO#", descuento);
                                                                }

                                                            }
                                                            catch
                                                            {
                                                                descuento = "</Detalle>";
                                                                plantdet = plantdet.Replace("#DESCUENTO#", descuento);
                                                            }
                                                            contadordetalle = contadordetalle + 1;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        msg = "No es posible superar los 40 Items";
                                                        throw new ApplicationException(msg);
                                                    }
                                                }

                                                int neto = Int32.Parse(monto_neto_suma_detalle.ToString());
                                                plant = plant.Replace("#MNTNETO#", neto.ToString());
                                                DTEValidaciones(neto.ToString(), "Monto Neto", 18, 3);
                                                decimal _iva = decimal.Parse(json2.GetValue("saletotaltaxamt").ToString());
                                                plant = plant.Replace("#IVA#", Math.Truncate(_iva).ToString());
                                                DTEValidaciones(_iva.ToString(), "IVA", 18, 3);

                                                plant = plant.Replace("#DETALLE#", plantdet);
                                                #endregion,
                                                #region Creacion Xml Referencia
                                                //string strjson4 = json2.GetValue("docitem").ToString(); //cambiar x referencia
                                                //JArray jsonArray1 = JArray.Parse(strjson4);
                                                //foreach (JObject jsonOperaciones in jsonArray1.Children<JObject>())
                                                //{
                                                plantref = string.Empty;
                                                //string email = json2.GetValue("btemail").ToString();
                                                //if (string.IsNullOrEmpty(email))
                                                //{
                                                //    email = "1";
                                                //}
                                                //plantref = plantref + FacelePlantillaXml.PlantillaRefFactura();
                                                //plantref = plantref.Replace("#NROLINREF#", "1");
                                                //plantref = plantref.Replace("#TPODOCREF#", "MTO");
                                                ////  plantref = plantref.Replace("#FOLIOREF#", folio); el folio se reemplaza mas abajo
                                                //plantref = plantref.Replace("#FCHREF#", DateTime.Now.ToString("yyyy-MM-dd"));
                                                //plantref = plantref.Replace("#RAZONREF#", email);
                                                ////DTEValidaciones(json2.GetValue("btemail").ToString(), "Razon Referencia", 90, 2);
                                                ////}
                                                plant = plant.Replace("#REFERENCIA#", plantref);
                                                #endregion
                                                #region GeneraDTE
                                                faceleDoc.tipoDTE = 33;
                                                faceleDoc.xml = plant;
                                                string sep = "-------------------------";
                                                this.objLog.writeDTEFacele($"{sep} FaceleDocument: Antes de GeneraDTE {sep}");
                                                this.objLog.writeDTEFacele(JsonConvert.SerializeObject(faceleDoc, Newtonsoft.Json.Formatting.Indented));
                                                FaceleProcessor.GeneraDTE(ref faceleDoc);
                                                this.objLog.writeDTEFacele($"estadoOperacion: {faceleDoc.estadoOperacion} | descripcionOperacion: {faceleDoc.descripcionOperacion}");
                                                if (faceleDoc.estadoOperacion == 0)//ERROR AL EMITIR
                                                {
                                                    flag2 = false;
                                                    num = 200;
                                                    string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + "1" + "\"," + "\"FolioNo\":" + "\"" + faceleDoc.folioDTE + "\"," + "\"MsgEstatus\":" + "\"" + faceleDoc.descripcionOperacion + "\"," + "\"TED\":" + "\"" + faceleDoc.ted + "\"" + "}" };
                                                    str = string.Concat(textArray5);
                                                    this.objLog.write("ProcessEvent** Error: " + "Estatus fuera de rango" + msg);
                                                }
                                                else
                                                {
                                                    flag2 = false;
                                                    num = 200;
                                                    FaceleProcessor.ConfirmaDTE(ref faceleDoc);

                                                    this.objLog.writeDTEFacele(faceleDoc.responseXml);
                                                    ted = retornaTED(faceleDoc.responseXml);
                                                    tedbase64 = EncodeStrToBase64(ted);

                                                    string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + "0" + "\"," + "\"FolioNo\":" + "\"" + faceleDoc.folioDTE + "\"," + "\"MsgEstatus\":" + "\"" + faceleDoc.descripcionOperacion + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                    str = string.Concat(textArray5);
                                                }
                                                #endregion
                                                #endregion
                                            }
                                            catch (Exception e)
                                            {
                                                flag2 = false;
                                                num = 200;
                                                status = "555";
                                                this.objLog.write("EXCEPTION2");
                                                str = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                                                this.objLog.write("ProcessEvent *********** Error General -- " + e.Message);
                                                this.txtMensaje.Text = this.txtMensaje.Text + "\nExcepcion: " + e.Message;
                                            }
                                        }
                                        else if (posflag == "61")// NOTA DE CREDITO ELECTRONICA FACELE
                                        {
                                            //posflagtipo = "NOTA DE CREDITO ELECTRONICA";
                                            this.objLog.writeDTEFacele("Procesando Nota de Credito");
                                            try
                                            {
                                                #region PROCESO NOTA DE CREDITO ELECTRONICA
                                                plant = FacelePlantillaXml.PlantillaCabNotaCredito();
                                                #region Creacion de XML Cabecera
                                                //DOCUMENTO
                                                plant = plant.Replace("#TPODTE#", posflag);
                                                plant = plant.Replace("#FECEMISION#", DateTime.Now.ToString("yyyy-MM-dd"));
                                                plant = plant.Replace("#FMAPAGO#", "2");
                                                plant = plant.Replace("#TERMPAGO#", "30");
                                                plant = plant.Replace("#FECVENC#", DateTime.Now.ToString("yyyy-MM-dd"));
                                                //EMISOR
                                                plant = plant.Replace("#RUTEMISOR#", DTEVyV_RutEmisor);
                                                plant = plant.Replace("#RZNSOC#", DTEVyV_RznSocEmi);
                                                DTEValidaciones(DTEVyV_RznSocEmi, "Razon Social Emisor", 100, 1);
                                                plant = plant.Replace("#GIRO#", LargoCadenaMax(DTEVyV_Giro, 80));
                                                DTEValidaciones(LargoCadenaMax(DTEVyV_Giro, 80), "Giro Emisor", 80, 1);
                                                plant = plant.Replace("#ACTECO#", "513100");// this.ParamValues.DTEFacele_ACTECO);
                                                plant = plant.Replace("#CDGSIISUCUR#", DTEVyV_codigo_sii_sucursal);
                                                DTEValidaciones(DTEVyV_codigo_sii_sucursal, "Sucursal Emisor", 20, 1);
                                                plant = plant.Replace("#DIRORIGEN#", DTEVyV_DirOrigen);
                                                plant = plant.Replace("#CMNAORIGEN#", DTEVyV_CmnaOrigen);
                                                plant = plant.Replace("#CIUDADORIGEN#", DTEVyV_CiudadOrigen);
                                                plant = plant.Replace("#CDGVENDEDOR#", json2.GetValue("employee1name").ToString());
                                                //RECEPTOR
                                                plant = plant.Replace("#RUTRECEPTOR#", numdoccliente);
                                                string cdgIntReceptor = numdoccliente.Substring(0, numdoccliente.IndexOf('-')) + "-C";
                                                plant = plant.Replace("#CDGINTRECEPTOR#", cdgIntReceptor);
                                                DTEValidaciones(numdoccliente, "RUT Receptor", 10, 1);
                                                string nombrecliente = jsoncustomerRest.GetValue("first_name").ToString() + " " + jsoncustomerRest.GetValue("last_name").ToString();
                                                if (nombrecliente == "")
                                                {
                                                    plant = plant.Replace("#RZNSOCRECEP#", this.ParamValues.DTEFacele_NombreClientedefault);
                                                    DTEValidaciones(this.ParamValues.DTEFacele_NombreClientedefault, "Razon Social Receptor", 100, 1);
                                                }
                                                else
                                                {
                                                    plant = plant.Replace("#RZNSOCRECEP#", nombrecliente);
                                                    DTEValidaciones(nombrecliente, "Razon Social Receptor", 100, 2);
                                                }
                                                string giro = jsoncustomerRest.GetValue("notes").ToString();
                                                if (giro == "")
                                                {
                                                    plant = plant.Replace("#GIRORECEP#", LargoCadenaMax(this.ParamValues.DTEFacele_GiroClientedefault, 40));
                                                    DTEValidaciones(LargoCadenaMax(this.ParamValues.DTEFacele_GiroClientedefault, 40), "Giro Receptor", 40, 1);
                                                }
                                                else
                                                {
                                                    plant = plant.Replace("#GIRORECEP#", LargoCadenaMax(giro, 40));
                                                    DTEValidaciones(LargoCadenaMax(giro, 40), "Giro Receptor", 40, 2);
                                                }
                                                plant = plant.Replace("#DIRRECEP#", jsoncustomerRest.GetValue("primary_address_line_1").ToString() + " " + jsoncustomerRest.GetValue("primary_address_line_2").ToString());
                                                DTEValidaciones(jsoncustomerRest.GetValue("primary_address_line_1").ToString() + " " + jsoncustomerRest.GetValue("primary_address_line_2").ToString(), "Ciudad Receptor", 70, 2);
                                                plant = plant.Replace("#CMNARECEP#", jsoncustomerRest.GetValue("primary_address_line_4").ToString());
                                                DTEValidaciones(jsoncustomerRest.GetValue("primary_address_line_4").ToString(), "Comuna Receptor", 20, 1);
                                                plant = plant.Replace("#CIUDADRECEP#", jsoncustomerRest.GetValue("primary_address_line_5").ToString());
                                                DTEValidaciones(jsoncustomerRest.GetValue("primary_address_line_5").ToString(), "Ciudad Receptor", 20, 1);

                                                plant = plant.Replace("#MNTEXE#", "0");
                                                plant = plant.Replace("#TASAIVA#", this.ParamValues.DTEFacele_TasaIVA);
                                                DTEValidaciones(this.ParamValues.DTEFacele_TasaIVA, "Tasa IVA", 6, 3);
                                                //plant = plant.Replace("#IVA#", json2.GetValue("returntotaltaxamt").ToString());
                                                //DTEValidaciones(json2.GetValue("returntotaltaxamt").ToString(), "IVA", 18, 3);
                                                plant = plant.Replace("#MNTTOTAL#", json2.GetValue("returnsubtotalwithtax").ToString());
                                                DTEValidaciones(json2.GetValue("returnsubtotalwithtax").ToString(), "Monto Total", 18, 3);
                                                #endregion
                                                #region Creacion Xml Detalle
                                                string strjson3 = json2.GetValue("docitem").ToString();
                                                JArray jsonArray = JArray.Parse(strjson3);
                                                Decimal monto_neto_suma_detalle = 0;
                                                int numeroLinea = 0;
                                                bool partialReturn = false;
                                                foreach (JObject jsonOperaciones in jsonArray.Children<JObject>())
                                                {
                                                    if (contadordetalle < 41)
                                                    {
                                                        string kitflag = "0";
                                                        try
                                                        {
                                                            kitflag = (jsonOperaciones["kitflag"]).ToString();
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            kitflag = "0";
                                                        }
                                                        if (!"5".Equals(kitflag))
                                                        {
                                                            string descuento = string.Empty;
                                                            plantdet = plantdet + FacelePlantillaXml.PantillaDetNotaCredito();
                                                            //plantdet = plantdet.Replace("#NROLINDET#", (jsonOperaciones["itempos"]).ToString());
                                                            numeroLinea++;
                                                            plantdet = plantdet.Replace("#NROLINDET#", numeroLinea.ToString());
                                                            plantdet = plantdet.Replace("#TPOCODIGO#", "INTERNO");
                                                            plantdet = plantdet.Replace("#VLRCODIGO#", (jsonOperaciones["alu"]).ToString());
                                                            DTEValidaciones(jsonOperaciones["alu"].ToString(), "Código Item", 35, 1);
                                                            plantdet = plantdet.Replace("#NMBITEM#", (jsonOperaciones["description1"]).ToString());
                                                            DTEValidaciones(jsonOperaciones["description1"].ToString(), "Nombre Item", 70, 1);
                                                            plantdet = plantdet.Replace("#DESC#", (jsonOperaciones["description1"]).ToString());
                                                            string cantidad = (jsonOperaciones["qty"]).ToString();
                                                            cantidad = cantidad.Replace(',', '.');
                                                            plantdet = plantdet.Replace("#QTYITEM#", cantidad);
                                                            string iva = (jsonOperaciones["taxamt"]).ToString().Split(',')[0];
                                                            this.objLog.writeDTEFacele($"iva: {iva}");
                                                            string precio = (jsonOperaciones["price"]).ToString();
                                                            this.objLog.writeDTEFacele($"precio: {precio}");
                                                            string cantidad_decimal = cantidad.Replace(".", ",");
                                                            this.objLog.writeDTEFacele($"cantidad_decimal: {cantidad_decimal}");
                                                            double montoitem = ((Int32.Parse(precio) * Convert.ToDouble(cantidad_decimal)) - (Int32.Parse(iva) * Convert.ToDouble(cantidad_decimal)));
                                                            this.objLog.writeDTEFacele($"montoitem: {montoitem}");
                                                            plantdet = plantdet.Replace("#MONTOITEM#", Decimal.Round(Convert.ToDecimal(montoitem.ToString())).ToString());
                                                            monto_neto_suma_detalle = monto_neto_suma_detalle + Decimal.Round(Convert.ToDecimal(montoitem.ToString()));
                                                            string precioori = (jsonOperaciones["origprice"]).ToString();
                                                            this.objLog.writeDTEFacele($"precioori: {precioori}");
                                                            int preciosiniva = (Int32.Parse(precioori) - Int32.Parse(iva));
                                                            this.objLog.writeDTEFacele($"preciosiniva: {preciosiniva}");
                                                            plantdet = plantdet.Replace("#PRCITEM#", preciosiniva.ToString());
                                                            DTEValidaciones(preciosiniva.ToString(), "Precio Item", 19, 2);
                                                            try
                                                            {
                                                                descuento = (jsonOperaciones["discamt"]).ToString();
                                                                if (descuento != string.Empty && descuento != "0")
                                                                {
                                                                    decimal descuento_decimal = Convert.ToDecimal(descuento);
                                                                    if (descuento_decimal < 0)
                                                                    {
                                                                        descuento_decimal = descuento_decimal * -1;
                                                                        descuento = descuento_decimal.ToString();
                                                                    }
                                                                    descuento = "<DescuentoMonto>" + descuento + "</DescuentoMonto></Detalle>";
                                                                    plantdet = plantdet.Replace("#DESCUENTO#", descuento);
                                                                }
                                                                else
                                                                {
                                                                    descuento = "</Detalle>";
                                                                    plantdet = plantdet.Replace("#DESCUENTO#", descuento);
                                                                }

                                                            }
                                                            catch
                                                            {
                                                                descuento = "</Detalle>";
                                                                plantdet = plantdet.Replace("#DESCUENTO#", descuento);
                                                            }

                                                            #region Comprueba devolución completa o parcial
                                                            try
                                                            {
                                                                string availableForReturn = jsonOperaciones["qtyavailableforreturn"].ToString();
                                                                availableForReturn = availableForReturn.Replace(',', '.');
                                                                this.objLog.writeDTEFacele($"checkQty: {availableForReturn} vs {cantidad}");
                                                                if (!String.IsNullOrEmpty(availableForReturn))
                                                                {
                                                                    if (availableForReturn != cantidad)
                                                                    {
                                                                        partialReturn = true;
                                                                    }
                                                                }
                                                            }
                                                            catch (Exception ex)
                                                            {

                                                                throw;
                                                            }
                                                            #endregion
                                                            contadordetalle = contadordetalle + 1;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        msg = "No es posible superar los 40 Items";
                                                        throw new ApplicationException(msg);
                                                    }
                                                }
                                                int neto = Int32.Parse(monto_neto_suma_detalle.ToString());
                                                plant = plant.Replace("#MNTNETO#", neto.ToString());
                                                plant = plant.Replace("#DETALLE#", plantdet);
                                                decimal _iva = decimal.Parse(json2.GetValue("returntotaltaxamt").ToString());
                                                plant = plant.Replace("#IVA#", Math.Truncate(_iva).ToString());
                                                DTEValidaciones(_iva.ToString(), "IVA", 18, 3);
                                                #endregion,
                                                #region Creacion Xml Referencia
                                                String idreferencia = json2.GetValue("refsalesid").ToString();
                                                JObject jsonref = JObject.Parse(this.GETDocument(idreferencia, token));
                                                JObject jsonreferencia = JObject.Parse(jsonref.GetValue("data")[0].ToString());

                                                string posflagref = jsonreferencia.GetValue("posflag1").ToString().Trim().Substring(0, 2);
                                                plantref = plantref + FacelePlantillaXml.PlantillaRefNotaCredito();
                                                plantref = plantref.Replace("#NROLINREF#", "1");
                                                plantref = plantref.Replace("#TPODOCREF#", posflagref);
                                                plantref = plantref.Replace("#FOLIOREF#", jsonreferencia.GetValue("trackingno").ToString());
                                                DateTime fecharef = DateTime.Parse(jsonreferencia.GetValue("modifieddatetime").ToString());
                                                string fecharefstr = String.Format("{0:yyyy-MM-dd}", fecharef);
                                                plantref = plantref.Replace("#FCHREF#", fecharefstr);
                                                plantref = plantref.Replace("#CODREF#", partialReturn ? "3" : "1");

                                                plant = plant.Replace("#REFERENCIA#", plantref);
                                                #endregion
                                                #region GeneraDTE
                                                faceleDoc.tipoDTE = 61;
                                                faceleDoc.xml = plant;
                                                string sep = "-------------------------";
                                                this.objLog.writeDTEFacele($"{sep} FaceleDocument: Antes de GeneraDTE {sep}");
                                                this.objLog.writeDTEFacele(JsonConvert.SerializeObject(faceleDoc, Newtonsoft.Json.Formatting.Indented));
                                                FaceleProcessor.GeneraDTE(ref faceleDoc);
                                                this.objLog.writeDTEFacele($"estadoOperacion: {faceleDoc.estadoOperacion} | descripcionOperacion: {faceleDoc.descripcionOperacion}");
                                                if (faceleDoc.estadoOperacion == 0)//ERROR AL EMITIR
                                                {
                                                    flag2 = false;
                                                    num = 200;
                                                    string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + "1" + "\"," + "\"FolioNo\":" + "\"" + faceleDoc.folioDTE + "\"," + "\"MsgEstatus\":" + "\"" + faceleDoc.descripcionOperacion + "\"," + "\"TED\":" + "\"" + faceleDoc.ted + "\"" + "}" };
                                                    str = string.Concat(textArray5);
                                                    this.objLog.write("ProcessEvent** Error: " + "Estatus fuera de rango" + msg);
                                                }
                                                else
                                                {
                                                    flag2 = false;
                                                    num = 200;
                                                    FaceleProcessor.ConfirmaDTE(ref faceleDoc);

                                                    this.objLog.writeDTEFacele(faceleDoc.responseXml);
                                                    ted = retornaTED(faceleDoc.responseXml);
                                                    tedbase64 = EncodeStrToBase64(ted);

                                                    string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + "0" + "\"," + "\"FolioNo\":" + "\"" + faceleDoc.folioDTE + "\"," + "\"MsgEstatus\":" + "\"" + faceleDoc.descripcionOperacion + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                    str = string.Concat(textArray5);
                                                }
                                                #endregion
                                                #endregion
                                            }
                                            catch (Exception e)
                                            {
                                                flag2 = false;
                                                num = 200;
                                                status = "555";
                                                this.objLog.write("EXCEPTION2");
                                                str = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                                                this.objLog.write("ProcessEvent *********** Error General -- " + e.Message);
                                                this.txtMensaje.Text = this.txtMensaje.Text + "\nExcepcion: " + e.Message;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        flag2 = false;
                                        num = 200;
                                        status = "556";
                                        string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + errors + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                        str = string.Concat(textArray5);
                                        this.objLog.write("ProcessEvent** Error: " + errors);
                                    }
                                }
                            }
                        }
                        //this.objLog.write("SALIMOS");
                    }
                    if (((ADirection == "FromClient") && (AHTTPVerb == "POST")) && (AResourceName.ToUpper() == "CALLDTEFACELETRANSFER"))
                    {
                        str18 = "";
                        str19 = "";
                        str20 = "";
                        str21 = "";
                        string errors = string.Empty;
                        string folio = string.Empty;
                        string plant = string.Empty;
                        string plantdet = string.Empty;
                        string status = string.Empty;
                        string msg = string.Empty;
                        string xmlres = string.Empty;
                        string ted = string.Empty;
                        string tedbase64 = string.Empty;

                        // this.objLog.write("ENTRAMOS");
                        foreach (JObject obj3 in JArray.Parse("[" + message.Payload + "]").Children<JObject>())
                        {
                            foreach (JProperty property in obj3.Properties())
                            {
                                if (property.Name == "documentID")
                                {
                                    token = this.doPRISMLoginAux(this.objLog);
                                    str21 = (string)property.Value;
                                    JObject json = JObject.Parse(this.GETTransfer(str21, token));
                                    errors = json.GetValue("errors").ToString();
                                    if (errors == "")
                                    {

                                        JObject json2 = JObject.Parse(json.GetValue("data")[0].ToString());
                                        string idTienda = json2.GetValue("outstoreno").ToString();
                                        JObject jsonstore = JObject.Parse(this.GETStore(idTienda, token));
                                        JObject store = JObject.Parse(jsonstore.GetValue("data")[0].ToString());
                                        string DTEVyV_Tienda = store.GetValue("storename").ToString();
                                        string DTEVyV_RutEmisor = store.GetValue("zip").ToString().ToUpper();
                                        string DTEVyV_RznSocEmi = store.GetValue("udf1string").ToString();
                                        string DTEVyV_Giro = store.GetValue("udf2string").ToString();
                                        string DTEVyV_Direccion_tienda = store.GetValue("address1").ToString();
                                        string DTEVyV_DirOrigen = store.GetValue("address4").ToString();
                                        string DTEVyV_CmnaOrigen = store.GetValue("address5").ToString();
                                        string DTEVyV_CiudadOrigen = store.GetValue("address6").ToString();
                                        string DTEVyV_Telefono = store.GetValue("phone2").ToString();
                                        string DTEVyV_Telefono_tienda = store.GetValue("phone1").ToString();
                                        FaceleDocument faceleDoc = new FaceleDocument(this.ParamValues.DTEFacele_WSurl, DTEVyV_RutEmisor);


                                        string posflag = "52";
                                        try
                                        {
                                            this.objLog.writeDTEFacele("Guia de Despacho - Transferencias");
                                            //this.objLog.writeDTEFacele($"Configs XML: {JsonConvert.SerializeObject(this.ParamValues, Newtonsoft.Json.Formatting.Indented)}");
                                            plant = FacelePlantillaXml.PlantillaCabGuiaDespacho();
                                            #region Creacion de XML Cabecera
                                            //DOCUMENTO
                                            plant = plant.Replace("#TPODTE#", posflag);
                                            plant = plant.Replace("#FECEMISION#", DateTime.Now.ToString("yyyy-MM-dd"));
                                            plant = plant.Replace("#TIPODESPACHO#", "2");
                                            plant = plant.Replace("#INDTRASLADO#", "5");
                                            plant = plant.Replace("#MNTBRUTO#", "1");
                                            plant = plant.Replace("#FECVENC#", DateTime.Now.ToString("yyyy-MM-dd"));
                                            //EMISOR
                                            plant = plant.Replace("#RUTEMISOR#", json2.GetValue("origstorezip").ToString().ToUpper());
                                            plant = plant.Replace("#RZNSOC#", json2.GetValue("origstoreudf1string").ToString());
                                            DTEValidaciones(json2.GetValue("origstoreudf1string").ToString(), "Razon Social Emisor", 100, 1);
                                            plant = plant.Replace("#GIRO#", LargoCadenaMax((json2.GetValue("origstoreudf2string").ToString() + " " + json2.GetValue("origstoreudf3string").ToString()).Trim(), 80));
                                            DTEValidaciones(LargoCadenaMax((json2.GetValue("origstoreudf2string").ToString() + " " + json2.GetValue("origstoreudf3string").ToString()).Trim(), 80), "Giro Emisor", 80, 1);
                                            //plant = plant.Replace("#ACTECO#", this.ParamValues.DTEFacele_ACTECO);
                                            plant = plant.Replace("#ACTECO#", "513100");
                                            plant = plant.Replace("#CDGSIISUCUR#", json2.GetValue("origstoreudf4string").ToString());
                                            DTEValidaciones(json2.GetValue("origstoreudf4string").ToString(), "Sucursal Emisor", 20, 1);
                                            plant = plant.Replace("#DIRORIGEN#", json2.GetValue("origstoreaddress1").ToString());
                                            plant = plant.Replace("#CMNAORIGEN#", json2.GetValue("origstoreaddress2").ToString());
                                            plant = plant.Replace("#CIUDADORIGEN#", json2.GetValue("origstoreaddress3").ToString());
                                            //RECEPTOR
                                            plant = plant.Replace("#RUTRECEPTOR#", json2.GetValue("origstorezip").ToString().ToUpper());
                                            DTEValidaciones(json2.GetValue("origstorezip").ToString(), "Rut Receptor", 10, 1);
                                            plant = plant.Replace("#RZNSOCRECEP#", json2.GetValue("instoreudf1string").ToString());
                                            DTEValidaciones(json2.GetValue("instoreudf1string").ToString(), "Razon Social Receptor", 100, 1);
                                            plant = plant.Replace("#GIRORECEP#", LargoCadenaMax((json2.GetValue("instoreudf2string").ToString() + " " + json2.GetValue("instoreudf3string").ToString()).Trim(), 40));
                                            DTEValidaciones(LargoCadenaMax((json2.GetValue("instoreudf2string").ToString() + " " + json2.GetValue("instoreudf3string").ToString()).Trim(), 40), "Giro Receptor", 40, 1);
                                            plant = plant.Replace("#DIRRECEP#", json2.GetValue("instoreaddress1").ToString());
                                            DTEValidaciones(json2.GetValue("instoreaddress1").ToString(), "Dirección Receptor", 70, 2);
                                            plant = plant.Replace("#CMNARECEP#", json2.GetValue("instoreaddress2").ToString());
                                            DTEValidaciones(json2.GetValue("instoreaddress2").ToString(), "Comuna Receptor", 20, 1);
                                            plant = plant.Replace("#CIUDADRECEP#", json2.GetValue("instoreaddress3").ToString());
                                            DTEValidaciones(json2.GetValue("instoreaddress3").ToString(), "Ciudad Receptor", 20, 1);
                                            //plant = plant.Replace("#DirPostal#", "");
                                            //plant = plant.Replace("#CmnaPostal#", "");
                                            //plant = plant.Replace("#CiudadPostal#", "");
                                            plant = plant.Replace("#DIRDEST#", json2.GetValue("instoreaddress1").ToString());
                                            plant = plant.Replace("#CMNADEST#", json2.GetValue("instoreaddress2").ToString());
                                            plant = plant.Replace("#CIUDADDEST#", json2.GetValue("instoreaddress3").ToString());
                                            //TOTALES

                                            //int neto = Int32.Parse(json2.GetValue("docpricetotal").ToString()) - iva;
                                            //int iva = (Int32.Parse(json2.GetValue("docpricetotal").ToString()) * Int32.Parse(this.ParamValues.DTEFacele_TasaIVA)) / 100;
                                            //this.objLog.writeDTEFacele($"docpricetotal: ->{Int32.Parse(json2.GetValue("docpricetotal").ToString())}<-");
                                            //this.objLog.writeDTEFacele($"DTEFacele_TasaIVA: ->{this.ParamValues.DTEFacele_TasaIVA}<-");
                                            //this.objLog.writeDTEFacele($"DTEFacele_TasaIVA: ->{Int32.Parse(this.ParamValues.DTEFacele_TasaIVA)}<-");
                                            //this.objLog.writeDTEFacele($"MultIva: ->{(1 + (Int32.Parse(this.ParamValues.DTEFacele_TasaIVA) / 100))}<-");
                                            //decimal factorIva = 1 + (Convert.ToDecimal(this.ParamValues.DTEFacele_TasaIVA) / 100);
                                            //this.objLog.writeDTEFacele($"factorIva: ->{factorIva}<-");
                                            //int neto = Convert.ToInt32(Math.Truncate(Convert.ToDecimal(json2.GetValue("docpricetotal").ToString()) / factorIva));
                                            //int iva = (Int32.Parse(json2.GetValue("docpricetotal").ToString()) - neto);
                                            //plant = plant.Replace("#MNTNETO#", neto.ToString());
                                            //DTEValidaciones(neto.ToString(), "Monto Neto", 18, 3);
                                            //plant = plant.Replace("#MNTEXE#", "0");
                                            //plant = plant.Replace("#TASAIVA#", this.ParamValues.DTEFacele_TasaIVA);
                                            //this.objLog.writeDTEFacele($"TasaIva: ->{this.ParamValues.DTEFacele_TasaIVA}<-");
                                            //this.objLog.writeDTEFacele($"Iva: ->{iva}<-");
                                            //this.objLog.writeDTEFacele($"neto: ->{neto}<-");
                                            //this.objLog.writeDTEFacele($"docpricetotal: ->{json2.GetValue("docpricetotal").ToString()}<-");
                                            //DTEValidaciones(this.ParamValues.DTEFacele_TasaIVA, "Tasa IVA", 6, 3);
                                            //plant = plant.Replace("#IVA#", iva.ToString());
                                            //DTEValidaciones(iva.ToString(), "Tasa IVA", 18, 3);
                                            int neto = 0;
                                            int iva = 0;
                                            plant = plant.Replace("#MNTTOTAL#", json2.GetValue("docpricetotal").ToString());
                                            DTEValidaciones(json2.GetValue("docpricetotal").ToString(), "Monto Total", 18, 3);
                                            #endregion
                                            #region Creacion Xml Detalle
                                            string jsonitem = json2.GetValue("slipitem").ToString();
                                            JArray jsonArrayitem = JArray.Parse(jsonitem);
                                            foreach (JObject jsonOperaciones in jsonArrayitem.Children<JObject>())
                                            {
                                                if (contadordetalle < 41)
                                                {
                                                    string kitflag = "0";
                                                    try
                                                    {
                                                        kitflag = (jsonOperaciones["kitflag"]).ToString();
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        kitflag = "0";
                                                    }

                                                    if (!"5".Equals(kitflag))
                                                    {
                                                        plantdet = plantdet + FacelePlantillaXml.PantillaDetGuiaDespacho();
                                                        plantdet = plantdet.Replace("#NROLINDET#", (jsonOperaciones["itempos"]).ToString());
                                                        plantdet = plantdet.Replace("#TPOCODIGO#", "INTERNO");
                                                        plantdet = plantdet.Replace("#VLRCODIGO#", (jsonOperaciones["alu"]).ToString());
                                                        DTEValidaciones((jsonOperaciones["alu"]).ToString(), "Código Item", 35, 1);
                                                        plantdet = plantdet.Replace("#NMBITEM#", (jsonOperaciones["description1"]).ToString());
                                                        DTEValidaciones((jsonOperaciones["description1"]).ToString(), "Nombre Item", 70, 1);
                                                        plantdet = plantdet.Replace("#DESC#", (jsonOperaciones["description1"]).ToString());
                                                        plantdet = plantdet.Replace("#QTYITEM#", (jsonOperaciones["qty"]).ToString());
                                                        //int ivadet = (Int32.Parse((jsonOperaciones["price"]).ToString()) * Int32.Parse(this.ParamValues.DTEFacele_TasaIVA)) / 100;
                                                        int precioItem = Int32.Parse((jsonOperaciones["price"]).ToString());
                                                        decimal factorIva = 1 + (Convert.ToDecimal(this.ParamValues.DTEFacele_TasaIVA) / 100);
                                                        int ivadet = precioItem - Convert.ToInt32(Int32.Parse((jsonOperaciones["price"]).ToString()) / factorIva);
                                                        string cantidad = (jsonOperaciones["qty"]).ToString();
                                                        string precio = (jsonOperaciones["price"]).ToString();
                                                        string cantidad_decimal = cantidad.Replace(".", ",");
                                                        int montoitem = ((Int32.Parse(precio) * Int32.Parse(cantidad_decimal)) - (ivadet * Int32.Parse(cantidad_decimal)));
                                                        plantdet = plantdet.Replace("#MONTOITEM#", montoitem.ToString());
                                                        int preciosiniva = (Int32.Parse(precio) - ivadet);
                                                        plantdet = plantdet.Replace("#PRCITEM#", preciosiniva.ToString());
                                                        DTEValidaciones(preciosiniva.ToString(), "Precio Item", 19, 2);
                                                        neto += montoitem;
                                                    }
                                                }
                                                else
                                                {
                                                    msg = "No es posible superar los 40 Items";
                                                    throw new ApplicationException(msg);
                                                }
                                            }
                                            plant = plant.Replace("#DETALLE#", plantdet);
                                            #region Totales
                                            plant = plant.Replace("#MNTNETO#", neto.ToString());
                                            DTEValidaciones(neto.ToString(), "Monto Neto", 18, 3);
                                            plant = plant.Replace("#MNTEXE#", "0");
                                            plant = plant.Replace("#TASAIVA#", this.ParamValues.DTEFacele_TasaIVA);
                                            DTEValidaciones(this.ParamValues.DTEFacele_TasaIVA, "Tasa IVA", 6, 3);
                                            iva = Int32.Parse(json2.GetValue("docpricetotal").ToString()) - neto;
                                            plant = plant.Replace("#IVA#", iva.ToString());
                                            DTEValidaciones(iva.ToString(), "Tasa IVA", 18, 3);
                                            #endregion
                                            #endregion
                                            #region GeneraDTE
                                            faceleDoc.tipoDTE = 52;
                                            faceleDoc.xml = plant;
                                            string sep = "-------------------------";
                                            this.objLog.writeDTEFacele($"{sep} FaceleDocument: Antes de GeneraDTE {sep}");
                                            this.objLog.writeDTEFacele(JsonConvert.SerializeObject(faceleDoc, Newtonsoft.Json.Formatting.Indented));
                                            FaceleProcessor.GeneraDTE(ref faceleDoc);
                                            this.objLog.writeDTEFacele($"estadoOperacion: {faceleDoc.estadoOperacion} | descripcionOperacion: {faceleDoc.descripcionOperacion}");
                                            if (faceleDoc.estadoOperacion == 0)//ERROR AL EMITIR
                                            {
                                                flag2 = false;
                                                num = 200;
                                                string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + "1" + "\"," + "\"FolioNo\":" + "\"" + faceleDoc.folioDTE + "\"," + "\"MsgEstatus\":" + "\"" + faceleDoc.descripcionOperacion + "\"," + "\"TED\":" + "\"" + faceleDoc.ted + "\"" + "}" };
                                                str = string.Concat(textArray5);
                                                this.objLog.write("ProcessEvent** Error: " + "Estatus fuera de rango" + msg);
                                            }
                                            else
                                            {
                                                flag2 = false;
                                                num = 200;
                                                FaceleProcessor.ConfirmaDTE(ref faceleDoc);

                                                this.objLog.writeDTEFacele(faceleDoc.responseXml);
                                                ted = retornaTED(faceleDoc.responseXml);
                                                tedbase64 = EncodeStrToBase64(ted);

                                                string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + "0" + "\"," + "\"FolioNo\":" + "\"" + faceleDoc.folioDTE + "\"," + "\"MsgEstatus\":" + "\"" + faceleDoc.descripcionOperacion + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                str = string.Concat(textArray5);
                                            }
                                            #endregion
                                        }
                                        catch (Exception e)
                                        {
                                            flag2 = false;
                                            num = 200;
                                            status = "555";
                                            this.objLog.write("EXCEPTION2");
                                            str = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                                            this.objLog.write("ProcessEvent *********** Error General -- " + e.Message);
                                            this.txtMensaje.Text = this.txtMensaje.Text + "\nExcepcion: " + e.Message;
                                        }
                                    }
                                    else
                                    {
                                        flag2 = false;
                                        num = 200;
                                        status = "556";
                                        string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + errors + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                        str = string.Concat(textArray5);
                                        this.objLog.write("ProcessEvent** Error: " + errors);
                                    }
                                }
                            }
                        }
                    }
                    if (((ADirection == "FromClient") && (AHTTPVerb == "POST")) && (AResourceName.ToUpper() == "CALLDTEFACELEVOUCHER"))
                    {
                        str18 = "";
                        str19 = "";
                        str20 = "";
                        str21 = "";
                        string errors = string.Empty;
                        string folio = string.Empty;
                        string plant = string.Empty;
                        string plantdet = string.Empty;
                        string status = string.Empty;
                        string msg = string.Empty;
                        string xmlres = string.Empty;
                        string ted = string.Empty;
                        string tedbase64 = string.Empty;

                        foreach (JObject obj3 in JArray.Parse("[" + message.Payload + "]").Children<JObject>())
                        {
                            foreach (JProperty property in obj3.Properties())
                            {
                                if (property.Name == "documentID")
                                {
                                    token = this.doPRISMLoginAux(this.objLog);
                                    str21 = (string)property.Value;
                                    JObject json = JObject.Parse(this.GETReceive(str21, token));
                                    errors = json.GetValue("errors").ToString();
                                    if (errors == "")
                                    {
                                        // this.objLog.writeDTEVyV(json.GetValue("data")[0].ToString());
                                        JObject json2 = JObject.Parse(json.GetValue("data")[0].ToString());
                                        this.objLog.write($"------------------ json2 -----------------");
                                        this.objLog.write($"{JsonConvert.SerializeObject(json2, Newtonsoft.Json.Formatting.Indented)}");
                                        this.objLog.write($"------------------ json2 -----------------");
                                        string idTienda = json2.GetValue("storeno").ToString();
                                        JObject jsonstore = JObject.Parse(this.GETStore(idTienda, token));
                                        JObject store = JObject.Parse(jsonstore.GetValue("data")[0].ToString());
                                        string DTEVyV_Tienda = store.GetValue("storename").ToString();
                                        string DTEVyV_RutEmisor = store.GetValue("zip").ToString().ToUpper();
                                        string DTEVyV_RznSocEmi = store.GetValue("udf1string").ToString();
                                        string DTEVyV_Giro = store.GetValue("udf2string").ToString();
                                        string DTEVyV_Direccion_tienda = store.GetValue("address1").ToString();
                                        string DTEVyV_DirOrigen = store.GetValue("address4").ToString();
                                        string DTEVyV_CmnaOrigen = store.GetValue("address5").ToString();
                                        string DTEVyV_CiudadOrigen = store.GetValue("address6").ToString();
                                        string DTEVyV_Telefono = store.GetValue("phone2").ToString();
                                        string DTEVyV_Telefono_tienda = store.GetValue("phone1").ToString();
                                        FaceleDocument faceleDoc = new FaceleDocument(this.ParamValues.DTEFacele_WSurl, DTEVyV_RutEmisor);

                                        string posflag = "52";
                                        try
                                        {
                                            this.objLog.writeDTEFacele("Guia de Despacho - Vouchers");
                                            plant = FacelePlantillaXml.PlantillaCabGuiaDespacho();
                                            #region Creacion de XML Cabecera
                                            //DOCUMENTO
                                            plant = plant.Replace("#TPODTE#", posflag);
                                            plant = plant.Replace("#FECEMISION#", DateTime.Now.ToString("yyyy-MM-dd"));
                                            plant = plant.Replace("#TIPODESPACHO#", "2");
                                            plant = plant.Replace("#INDTRASLADO#", "7");
                                            plant = plant.Replace("#MNTBRUTO#", "1");
                                            plant = plant.Replace("#FECVENC#", DateTime.Now.ToString("yyyy-MM-dd"));
                                            //EMISOR
                                            plant = plant.Replace("#RUTEMISOR#", json2.GetValue("origzip").ToString().ToUpper());
                                            plant = plant.Replace("#RZNSOC#", json2.GetValue("origstoreudf1string").ToString());
                                            DTEValidaciones(json2.GetValue("origstoreudf1string").ToString(), "Razon Social Emisor", 100, 1);
                                            plant = plant.Replace("#GIRO#", LargoCadenaMax((json2.GetValue("origstoreudf2string").ToString() + " " + json2.GetValue("origstoreudf3string").ToString()).Trim(), 80));
                                            DTEValidaciones(LargoCadenaMax((json2.GetValue("origstoreudf2string").ToString() + " " + json2.GetValue("origstoreudf3string").ToString()).Trim(), 80), "Giro Emisor", 80, 1);
                                            plant = plant.Replace("#ACTECO#", "513100");// this.ParamValues.DTEFacele_ACTECO);
                                            plant = plant.Replace("#CDGSIISUCUR#", json2.GetValue("origstoreudf4string").ToString());
                                            DTEValidaciones(json2.GetValue("origstoreudf4string").ToString(), "Sucursal Emisor", 20, 1);
                                            plant = plant.Replace("#DIRORIGEN#", json2.GetValue("origaddress1").ToString());
                                            plant = plant.Replace("#CMNAORIGEN#", json2.GetValue("origaddress2").ToString());
                                            plant = plant.Replace("#CIUDADORIGEN#", json2.GetValue("origaddress3").ToString());
                                            //RECEPTOR
                                            plant = plant.Replace("#RUTRECEPTOR#", json2.GetValue("vendorpostalcode").ToString().ToUpper());
                                            DTEValidaciones(json2.GetValue("vendorpostalcode").ToString(), "RUT Receptor", 10, 1);
                                            plant = plant.Replace("#RZNSOCRECEP#", json2.GetValue("vendoraddress4").ToString());
                                            DTEValidaciones(json2.GetValue("vendoraddress4").ToString(), "Razon Social Receptor", 100, 1);
                                            plant = plant.Replace("#GIRORECEP#", LargoCadenaMax((json2.GetValue("vendoraddress5").ToString() + " " + json2.GetValue("vendoraddress6").ToString()).Trim(), 40));
                                            DTEValidaciones(LargoCadenaMax((json2.GetValue("vendoraddress5").ToString() + " " + json2.GetValue("vendoraddress6").ToString()).Trim(), 40), "Giro Receptor", 40, 1);
                                            plant = plant.Replace("#DIRRECEP#", json2.GetValue("vendoraddress1").ToString());
                                            DTEValidaciones(json2.GetValue("vendoraddress1").ToString(), "Dirección Receptor", 70, 2);
                                            plant = plant.Replace("#CMNARECEP#", json2.GetValue("vendoraddress2").ToString());
                                            DTEValidaciones(json2.GetValue("vendoraddress2").ToString(), "Comuna Receptor", 20, 1);
                                            plant = plant.Replace("#CIUDADRECEP#", json2.GetValue("vendoraddress3").ToString());
                                            DTEValidaciones(json2.GetValue("vendoraddress3").ToString(), "Ciudad Receptor", 20, 1);
                                            //plant = plant.Replace("#DirPostal#", "");
                                            //plant = plant.Replace("#CmnaPostal#", "");
                                            //plant = plant.Replace("#CiudadPostal#", "");
                                            plant = plant.Replace("#DIRDEST#", json2.GetValue("vendoraddress1").ToString());
                                            plant = plant.Replace("#CMNADEST#", json2.GetValue("vendoraddress2").ToString());
                                            plant = plant.Replace("#CIUDADDEST#", json2.GetValue("vendoraddress3").ToString());
                                            int neto = 0;
                                            int iva = 0;
                                            #endregion
                                            #region Creacion Xml Detalle
                                            string jsonitem = json2.GetValue("recvitem").ToString();
                                            JArray jsonArrayitem = JArray.Parse(jsonitem);
                                            int valortotal = 0;
                                            foreach (JObject jsonOperaciones in jsonArrayitem.Children<JObject>())
                                            {
                                                if (contadordetalle < 41)
                                                {
                                                    string kitflag = "0";
                                                    try
                                                    {
                                                        kitflag = (jsonOperaciones["kitflag"]).ToString();
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        kitflag = "0";
                                                    }

                                                    if (!"5".Equals(kitflag))
                                                    {
                                                        plantdet = plantdet + FacelePlantillaXml.PantillaDetGuiaDespacho();
                                                        plantdet = plantdet.Replace("#NROLINDET#", (jsonOperaciones["itempos"]).ToString());
                                                        plantdet = plantdet.Replace("#TPOCODIGO#", "INTERNO");
                                                        plantdet = plantdet.Replace("#VLRCODIGO#", (jsonOperaciones["alu"]).ToString());
                                                        DTEValidaciones((jsonOperaciones["alu"]).ToString(), "Código Item", 35, 1);
                                                        plantdet = plantdet.Replace("#NMBITEM#", (jsonOperaciones["description1"]).ToString());
                                                        DTEValidaciones((jsonOperaciones["description1"]).ToString(), "Nombre Item", 70, 1);
                                                        plantdet = plantdet.Replace("#DESC#", (jsonOperaciones["description1"]).ToString());
                                                        plantdet = plantdet.Replace("#QTYITEM#", (jsonOperaciones["qty"]).ToString());

                                                        int precioItem = Int32.Parse((jsonOperaciones["price"]).ToString());
                                                        decimal factorIva = 1 + (Convert.ToDecimal(this.ParamValues.DTEFacele_TasaIVA) / 100);
                                                        int ivadet = precioItem - Convert.ToInt32(Int32.Parse((jsonOperaciones["price"]).ToString()) / factorIva);

                                                        //int ivadet = (Int32.Parse((jsonOperaciones["price"]).ToString()) * Int32.Parse(this.ParamValues.DTEFacele_TasaIVA)) / 100;
                                                        string cantidad = (jsonOperaciones["qty"]).ToString();
                                                        string precio = (jsonOperaciones["price"]).ToString();
                                                        string cantidad_decimal = cantidad.Replace(".", ",");
                                                        int montoitem = ((Int32.Parse(precio) * Int32.Parse(cantidad_decimal)) - (ivadet * Int32.Parse(cantidad_decimal)));
                                                        plantdet = plantdet.Replace("#MONTOITEM#", montoitem.ToString());
                                                        valortotal = valortotal + Int32.Parse((jsonOperaciones["price"]).ToString());
                                                        int preciosiniva = (Int32.Parse(precio) - ivadet);
                                                        plantdet = plantdet.Replace("#PRCITEM#", preciosiniva.ToString());
                                                        DTEValidaciones(preciosiniva.ToString(), "Precio Item", 19, 2);
                                                        contadordetalle = contadordetalle + 1;
                                                        neto += montoitem;

                                                        this.objLog.writeDTEFacele($"precioItem {precioItem}");
                                                        this.objLog.writeDTEFacele($"factorIva {factorIva}");
                                                        this.objLog.writeDTEFacele($"ivadet {ivadet}");
                                                        this.objLog.writeDTEFacele($"montoitem {montoitem}");
                                                        this.objLog.writeDTEFacele($"neto {neto}");

                                                    }
                                                }
                                                else
                                                {
                                                    msg = "No es posible superar los 40 Items";
                                                    throw new ApplicationException(msg);
                                                }
                                            }
                                            //TOTALES
                                            //int iva = (valortotal * Int32.Parse(this.ParamValues.DTEFacele_TasaIVA)) / 100;
                                            //int neto = valortotal - iva;
                                            //int neto = (valortotal / (1 + (Int32.Parse(this.ParamValues.DTEFacele_TasaIVA) / 100)));
                                            //int iva = (valortotal - neto);
                                            plant = plant.Replace("#MNTNETO#", neto.ToString());
                                            DTEValidaciones(neto.ToString(), "Monto Neto", 18, 3);
                                            plant = plant.Replace("#MNTEXE#", "0");
                                            plant = plant.Replace("#TASAIVA#", this.ParamValues.DTEFacele_TasaIVA);
                                            DTEValidaciones(this.ParamValues.DTEFacele_TasaIVA, "Tasa IVA", 6, 3);
                                            iva = valortotal - neto;
                                            plant = plant.Replace("#IVA#", iva.ToString());
                                            DTEValidaciones(iva.ToString(), "IVA", 18, 3);
                                            plant = plant.Replace("#MNTTOTAL#", valortotal.ToString());
                                            DTEValidaciones(valortotal.ToString(), "Monto Total", 18, 3);
                                            plant = plant.Replace("#DETALLE#", plantdet);
                                            #endregion
                                            #region GeneraDTE
                                            faceleDoc.tipoDTE = 52;
                                            faceleDoc.xml = plant;
                                            string sep = "-------------------------";
                                            this.objLog.writeDTEFacele($"{sep} FaceleDocument: Antes de GeneraDTE {sep}");
                                            this.objLog.writeDTEFacele(JsonConvert.SerializeObject(faceleDoc, Newtonsoft.Json.Formatting.Indented));
                                            FaceleProcessor.GeneraDTE(ref faceleDoc);
                                            this.objLog.writeDTEFacele($"estadoOperacion: {faceleDoc.estadoOperacion} | descripcionOperacion: {faceleDoc.descripcionOperacion}");
                                            if (faceleDoc.estadoOperacion == 0)//ERROR AL EMITIR
                                            {
                                                flag2 = false;
                                                num = 200;
                                                string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + "1" + "\"," + "\"FolioNo\":" + "\"" + faceleDoc.folioDTE + "\"," + "\"MsgEstatus\":" + "\"" + faceleDoc.descripcionOperacion + "\"," + "\"TED\":" + "\"" + faceleDoc.ted + "\"" + "}" };
                                                str = string.Concat(textArray5);
                                                this.objLog.write("ProcessEvent** Error: " + "Estatus fuera de rango" + msg);
                                            }
                                            else
                                            {
                                                flag2 = false;
                                                num = 200;
                                                FaceleProcessor.ConfirmaDTE(ref faceleDoc);

                                                this.objLog.writeDTEFacele(faceleDoc.responseXml);
                                                ted = retornaTED(faceleDoc.responseXml);
                                                tedbase64 = EncodeStrToBase64(ted);

                                                string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + "0" + "\"," + "\"FolioNo\":" + "\"" + faceleDoc.folioDTE + "\"," + "\"MsgEstatus\":" + "\"" + faceleDoc.descripcionOperacion + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                                str = string.Concat(textArray5);
                                            }
                                            #endregion
                                        }
                                        catch (Exception e)
                                        {
                                            flag2 = false;
                                            num = 200;
                                            status = "555";
                                            this.objLog.write("EXCEPTION2");
                                            str = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + e.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                                            this.objLog.write("ProcessEvent *********** Error General -- " + e.Message);
                                            this.txtMensaje.Text = this.txtMensaje.Text + "\nExcepcion: " + e.Message;
                                        }
                                    }
                                    else
                                    {
                                        flag2 = false;
                                        num = 200;
                                        status = "556";
                                        string[] textArray5 = new string[] { "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + errors + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}" };
                                        str = string.Concat(textArray5);
                                        this.objLog.write("ProcessEvent** Error: " + errors);
                                    }
                                }
                            }
                        }
                        // this.objLog.write("SALIMOS");
                    }
                }

                catch (PrismException exception6)
                {
                    this.objLog.write("EXCEPTION1");
                    flag2 = false;
                    num = 0x22b;
                    str = exception6.Message;
                }
                catch (Exception exception4)
                {
                    flag2 = false;
                    num = 200;
                    string status = "555";
                    string folio = "0";
                    string tedbase64 = "";
                    this.objLog.write("EXCEPTION4");
                    str = "{\"Estatus\":" + "\"" + status + "\"," + "\"FolioNo\":" + "\"" + folio + "\"," + "\"MsgEstatus\":" + "\"" + exception4.Message + "\"," + "\"TED\":" + "\"" + tedbase64 + "\"" + "}";
                    this.objLog.write("ProcessEvent *********** Error General -- " + exception4.Message);
                    this.txtMensaje.Text = this.txtMensaje.Text + "\nExcepcion: " + exception4.Message;
                }
                finally
                {
                   // this.objLog.write("FINALLY");
                    ACanContinue = flag2;
                    AStatusCode = num;
                    if (!flag2)
                    {
                        APayload = str;
                      //  this.objLog.write(APayload);
                    }
                   // this.objLog.write("ProcessEvent *********** Queda a la espera de nueva trama " + ACanContinue.ToString());
                }
            TR_0007:;
            }
        }

        public void SetUp()
        {
            this.ProxyLink.OnConfigDataSet = new NotifyEvent(this.ProcessConfigData);
            this.ProxyLink.OnTokenSet = new NotifyEvent(this.ProcessClientToken);
            this.ProxyLink.OnShutdownNotification = new NotifyEvent(this.ProcessShutdown);
            this.ProxyLink.OnEvent = new ProxyEventNotification(this.ProcessEvent);
            this.txtMensaje.Text = this.txtMensaje.Text + "**Set Up **" + DateTime.Now.ToShortTimeString();
            this.ProxyLink.Subscriptions.AddSubscription("EV_DocRequest", Direction.dFromClient, true, true, true, true, "document", "mpRR", true, false);
            this.ProxyLink.Subscriptions.AddSubscription("EV_DocResponse", Direction.dToClient, true, true, true, true, "document", "mpRR", true, false);
            this.ProxyLink.Subscriptions.AddSubscription("EV_FERequest", Direction.dFromClient, true, true, true, true, "callfebosfeLogin", "mpRR", true, true);
            this.ProxyLink.Subscriptions.AddSubscription("EV_FEUploadRequest", Direction.dFromClient, true, true, true, true, "callfebosfeUploadTxtDTE", "mpRR", true, true);
            this.ProxyLink.Subscriptions.AddSubscription("EV_FEDTEVyV", Direction.dFromClient, true, true, true, true, "callDTEVyV", "mpRR", true, true);
            this.ProxyLink.Subscriptions.AddSubscription("EV_FEDTEVyVTRANSFER", Direction.dFromClient, true, true, true, true, "callDTEVyVTransfer", "mpRR", true, true);
            this.ProxyLink.Subscriptions.AddSubscription("EV_FEDTEVyVVOUCHER", Direction.dFromClient, true, true, true, true, "callDTEVyVVoucher", "mpRR", true, true);
            this.ProxyLink.Subscriptions.AddSubscription("EV_VYVTOKEN", Direction.dFromClient, true, true, true, true, "callToken", "mpRR", true, true);
            this.ProxyLink.Subscriptions.AddSubscription("EV_VyVDESCUENTO", Direction.dFromClient, true, true, true, true, "callDescuento", "mpRR", true, true);
            this.ProxyLink.Subscriptions.AddSubscription("EV_TenderRequest", Direction.dFromClient, true, true, true, true, "calltransbankintegrado", "mpRR", true, true);
            this.ProxyLink.Subscriptions.AddSubscription("EV_TenderRequest", Direction.dFromClient, true, true, true, true, "callonepayrefund", "mpRR", true, true);
            this.ProxyLink.Subscriptions.AddSubscription("EV_TenderRequest", Direction.dFromClient, true, true, true, true, "callComandosAdministrativos", "mpRR", true, true);
            this.ProxyLink.Subscriptions.AddSubscription("EV_FEDTESignature", Direction.dFromClient, true, true, true, true, "callDTESignature", "mpRR", true, true);
            this.ProxyLink.Subscriptions.AddSubscription("EV_FEDTESignatureTRANSFER", Direction.dFromClient, true, true, true, true, "callDTESignatureTransfer", "mpRR", true, true);
            this.ProxyLink.Subscriptions.AddSubscription("EV_FEDTESignatureVOUCHER", Direction.dFromClient, true, true, true, true, "callDTESignatureVoucher", "mpRR", true, true);
            this.ProxyLink.Subscriptions.AddSubscription("EV_FEDTEDBNET", Direction.dFromClient, true, true, true, true, "callDTEDBNET", "mpRR", true, true);
            this.ProxyLink.Subscriptions.AddSubscription("EV_FEDTEDBNETRANSFERT", Direction.dFromClient, true, true, true, true, "callDTEDBNETTransfer", "mpRR", true, true);
            this.ProxyLink.Subscriptions.AddSubscription("EV_FEDTEDBNETVOUCHER", Direction.dFromClient, true, true, true, true, "callDTEDBNETVoucher", "mpRR", true, true);
            this.ProxyLink.Subscriptions.AddSubscription("EV_FEDTEFACCL", Direction.dFromClient, true, true, true, true, "callDTEFACCL", "mpRR", true, true);
            this.ProxyLink.Subscriptions.AddSubscription("EV_FEDTEFACCLRANSFERT", Direction.dFromClient, true, true, true, true, "callDTEFACCLTransfer", "mpRR", true, true);
            this.ProxyLink.Subscriptions.AddSubscription("EV_FEDTEFACCLVOUCHER", Direction.dFromClient, true, true, true, true, "callDTEFACCLVoucher", "mpRR", true, true);
            this.ProxyLink.Subscriptions.AddSubscription("EV_FEDTEFacele", Direction.dFromClient, true, true, true, true, "callDTEFacele", "mpRR", true, true);
            this.ProxyLink.Subscriptions.AddSubscription("EV_FEDTEFaceleTRANSFER", Direction.dFromClient, true, true, true, true, "callDTEFaceleTransfer", "mpRR", true, true);
            this.ProxyLink.Subscriptions.AddSubscription("EV_FEDTEFaceleVOUCHER", Direction.dFromClient, true, true, true, true, "callDTEFaceleVoucher", "mpRR", true, true);

            this.ProxyLink.Subscriptions.AddSubscription("EV_TenderRequest", Direction.dFromClient, true, true, true, true, "callpagoqrfpay", "mpRR", true, true);

            this.txtMensaje.Text = this.txtMensaje.Text + "**fin**";
            try
            {
                this.ProxyLink.StartListener();
                this.objLog.write("SetUp *********** OK");
            }
            catch (Exception exception)
            {
                this.objLog.write("SetUp *********** ERROR: " + exception.Message);
                this.txtMensaje.Text = this.txtMensaje.Text + "ERROR: " + exception.Message;
            }
        }

        private void ProcessClientToken()
        {
            this.TokenData.Add("Server=" + this.ProxyLink.PrismServer);
            this.TokenData.Add("Port=" + this.ProxyLink.PrismServerPort.ToString());
            this.TokenData.Add("Token=" + this.ProxyLink.UserToken);
            base.BeginInvoke((MethodInvoker)delegate { this.txtMensaje.Text = this.txtMensaje.Text + "** Client Token **"; });
            //base.BeginInvoke(new Action(() => { this.txtMensaje.Text = this.txtMensaje.Text + "** Client Token **"; }));
            //base.BeginInvoke(() => this.txtMensaje.Text = this.txtMensaje.Text + "** Client Token **"); ORIGINAL DEL DESCOMPILADOR
           // this.objLog.write("    ProcessClientToken ->ok");
        }
         

        #region Procesos Genericos Proxy
        static string UnicodeToUTF8(string from)
        {
            var bytes = Encoding.UTF8.GetBytes(from);
            return new string(bytes.Select(b => (char)b).ToArray());
        }
        static string ReemplazaCaracter(string cadena)
        {
            cadena = cadena.Replace("\\u00D1", "N");
            cadena = cadena.Replace("\\u00F1", "n");
            cadena = cadena.Replace("\\u00C1", "A");
            cadena = cadena.Replace("\\u00E1", "a");
            cadena = cadena.Replace("\\u00C9", "E");
            cadena = cadena.Replace("\\u00E9", "e");
            cadena = cadena.Replace("\\u00CD", "I");
            cadena = cadena.Replace("\\u00ED", "i");
            cadena = cadena.Replace("\\u00D3", "O");
            cadena = cadena.Replace("\\u00F3", "o");
            cadena = cadena.Replace("\\u00DA", "U");
            cadena = cadena.Replace("\\u00FA", "u");
            cadena = cadena.Replace("\\u0026", " ");
            cadena = cadena.Replace("&", "y");
            cadena = cadena.Replace("™", " ");
            cadena = cadena.Replace("€", " ");
            cadena = cadena.Replace("Â", " ");
            cadena = cadena.Replace("®", " ");
            cadena = cadena.Replace("¢", " ");
            cadena = cadena.Replace("â", " ");
            cadena = Regex.Replace(cadena, @"[^\u0000-\u007F]+", string.Empty); /*reemplaza todos los caracteres no ascii*/
            cadena = Regex.Replace(cadena, @"[^\w\d\s.,:\\[\\]\\{\\}@\\$%*()\-\/]+", ""); /*se permiten letras, digitos, espacios en blanco y ciertos caracteres como .,@$%*()*/

            return cadena;
        }

        private void AcercaDe_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Application.ProductName + " v" + Application.ProductVersion, "Acerca de");
        }

        private void Init_Load(object sender, EventArgs e)
        {
            this.ContextMenu1.MenuItems.Add("&Restaurar", new EventHandler(this.Restaurar_Click));
            this.ContextMenu1.MenuItems[0].DefaultItem = true;
            this.ContextMenu1.MenuItems.Add("-");
            this.ContextMenu1.MenuItems.Add("&Acerca de...", new EventHandler(this.AcercaDe_Click));
            this.ContextMenu1.MenuItems.Add("-");
            this.ContextMenu1.MenuItems.Add("&Salir", new EventHandler(this.Salir_Click));
            this.NotifyIcon1.Icon = base.Icon;
            this.NotifyIcon1.ContextMenu = this.ContextMenu1;
            this.NotifyIcon1.Text = Application.ProductName;
            this.NotifyIcon1.Visible = true;
            base.Resize += new EventHandler(this.Init_Resize);
            base.Activated += new EventHandler(this.Init_Activated);
            base.Closing += new CancelEventHandler(this.Init_FormClosing);
            this.NotifyIcon1.DoubleClick += new EventHandler(this.Restaurar_Click);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Init));
            this.txtMensaje = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtMensaje
            // 
            this.txtMensaje.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMensaje.Location = new System.Drawing.Point(0, 0);
            this.txtMensaje.Multiline = true;
            this.txtMensaje.Name = "txtMensaje";
            this.txtMensaje.ReadOnly = true;
            this.txtMensaje.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMensaje.Size = new System.Drawing.Size(301, 83);
            this.txtMensaje.TabIndex = 0;
            // 
            // Init
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(301, 83);
            this.Controls.Add(this.txtMensaje);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Init";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Proxy VyV";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Init_FormClosed);
            this.Load += new System.EventHandler(this.Init_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void ProcessShutdown()
        {
            this.txtMensaje.Text = this.txtMensaje.Text + "**Cerrando**";
            this.objLog.write(this.txtMensaje.Text);
            this.objLog.write("--------------PROXY DETENIDO----------------");
            this.ProxyLink.StopListener();
            base.Close();
            Application.Exit();
        }

        private void Restaurar_Click(object sender, EventArgs e)
        {
            base.Show();
            base.WindowState = FormWindowState.Normal;
            base.Activate();
        }

        private void Salir_Click(object sender, EventArgs e)
        {
            base.Close();
        }

        public static string Base64Decode(string base64EncodedData)
        {
            byte[] bytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(bytes);
        }

        public static byte calculateLRC(byte[] bytes)
        {
            byte num = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                num = (byte)(num ^ bytes[i]);
            }
            return num;
        }

        private void Check4NullStreamWriter(string PathAndName)
        {
            bool flag = false;
            if (ReferenceEquals(this.SW, null))
            {
                flag = true;
            }
            else if (ReferenceEquals(this.SW.BaseStream, null))
            {
                flag = true;
            }
            if (flag)
            {
                this.CreateStreamWriter(PathAndName);
                if (this.finalizationSupressed)
                {
                    GC.ReRegisterForFinalize(this);
                }
            }
        }

        public static string Chr(int intByte)
        {
            byte[] bytes = new byte[] { (byte)intByte };
            return Encoding.GetEncoding("windows-1252").GetString(bytes);
        }

        private void CreateStreamWriter(string PathAndName)
        {
            this.SW = new StreamWriter(PathAndName, true, Encoding.Default);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private string GetDatePostfix() =>
            ("_" + DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0'));

        private void Init_Activated(object sender, EventArgs e)
        {
            if (this.PrimeraVez)
            {
                this.PrimeraVez = false;
                base.Visible = false;
            }
        }

        private void Init_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.ProcessShutdown();
        }

        private void Init_FormClosing(object sender, CancelEventArgs e)
        {
            this.ProcessShutdown();
            this.NotifyIcon1.Visible = false;
            this.NotifyIcon1 = null;
            this.ContextMenu1 = null;
        }

        private void Init_Resize(object sender, EventArgs e)
        {
            if (base.WindowState == FormWindowState.Minimized)
            {
                base.Visible = false;
            }
        }

        private void ProcessConfigData()
        {
            this.GlobalConfigData = this.ProxyLink.GlobalConfigurationData;
            this.WSConfigData = this.ProxyLink.ProxyConfigurationData;
            this.objLog.write("    ProcessConfigData ->ok");
        }

        private void VeficaPath(string paramPath)
        {
            if (!Directory.Exists(paramPath))
            {
                Directory.CreateDirectory(paramPath);
            }
        }

        public string TimeStamp
        {
            get
            {
                DateTime now = DateTime.Now;
                string[] textArray1 = new string[13];
                textArray1[0] = now.Year.ToString();
                textArray1[1] = ".";
                textArray1[2] = now.Month.ToString().PadLeft(2, '0');
                textArray1[3] = ".";
                textArray1[4] = now.Day.ToString().PadLeft(2, '0');
                textArray1[5] = ".";
                textArray1[6] = now.Hour.ToString().PadLeft(2, '0');
                textArray1[7] = ".";
                textArray1[8] = now.Minute.ToString().PadLeft(2, '0');
                textArray1[9] = ".";
                textArray1[10] = now.Second.ToString().PadLeft(2, '0');
                textArray1[11] = ".";
                textArray1[12] = now.Millisecond.ToString().PadLeft(3, '0');
                return string.Concat(textArray1);
            }
        }

        public TBResponse RespuestaBanco { get; set; }

        #endregion

        #region Proceso Transbank
        public bool AbrirPuerto()
        {
            string str3 = string.Empty;
          //  this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** INICIO");
            bool flag = true;
            string str = ConfigurationManager.AppSettings["PINPADBaudRate"];
            string str2 = ConfigurationManager.AppSettings["PINPADCOMPorNumber"];
            bool flag2 = false;
            string[] textArray1 = new string[] { "\t", MethodBase.GetCurrentMethod().Name, "DEL CONFIG * PORT: ", str2, "* Boud Rate: ", str };
          //  this.objLog.writeTB(string.Concat(textArray1));
            if ((str != string.Empty) && (str2 != string.Empty))
            {
                flag = false;
            }
            string[] textArray2 = new string[10];
            textArray2[0] = "\t";
            textArray2[1] = MethodBase.GetCurrentMethod().Name;
            textArray2[2] = "DEFAULT CONFIG: ";
            textArray2[3] = flag.ToString();
            textArray2[4] = " * PORT: ";
            textArray2[5] = flag ? Sales_Plugin.intComNumberTB.ToString() : str2;
            string[] local1 = textArray2;
            local1[6] = " * Boud Rate: ";
            local1[7] = flag ? "115200" : str;
            string[] local2 = local1;
            local2[8] = " * SLEEP: ";
            local2[9] = Sales_Plugin.strSleepTB;
            this.objLog.writeTB(string.Concat(local2));
            if (this.comPort.IsOpen)
            {
                this.comPort.Close();
            }
            if ((flag ? Sales_Plugin.intComNumberTB.ToString() : str2) == string.Empty)
            {
                str3 = "1";
            }
            else
            {
                str3 = str2;
            }
            int num = flag ? 0x1c200 : Convert.ToInt32(str);
            if (num < 0)
            {
                num = 0x1c200;
            }
            object[] objArray1 = new object[] { "\t", MethodBase.GetCurrentMethod().Name, "CURRENT CONFIG *PORT: ", str3, " * Boud Rate: ", num };
            this.objLog.writeTB(string.Concat(objArray1));
            this.comPort.BaudRate = num;
            this.comPort.DataBits = 8;
            this.comPort.StopBits = StopBits.One;
            this.comPort.Parity = Parity.None;
            this.comPort.PortName = "COM" + str3;

            int PINPADTimeOut = 60000;// timeout para PINPAD integrado
            if(ConfigurationManager.AppSettings["PINPADTimeOut"] != null && ConfigurationManager.AppSettings["PINPADTimeOut"] == "")
            {
                PINPADTimeOut = int.Parse(ConfigurationManager.AppSettings["PINPADTimeOut"]);
            }

            this.comPort.ReadTimeout = PINPADTimeOut; // espera por X milisegundos
            this.comPort.WriteTimeout = PINPADTimeOut; // espera por X milisegundos
            try
            {
                this.comPort.Open();
                flag2 = true;
                this.objLog.writeTB("PUERTO ABIERTO");
            }
            catch (UnauthorizedAccessException exception)
            {
                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "Excepcion Abriendo Puerto - UnauthorizedAccessExceptio||" + exception.Message);
            }
            catch (IOException exception2)
            {
                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "Excepcion Abriendo Puerto - IOException||" + exception2.Message);
            }
            catch (ArgumentException exception3)
            {
                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "Excepcion Abriendo Puerto - ArgumentException||" + exception3.Message);
            }
            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** FIN");
            return flag2;
        }

        private bool ComandosAdministrativos(string comando)
        {
            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** INICIO");
            string jPayload = string.Empty;
            bool flag = false;
            object[] objArray1 = new object[] { "\t", MethodBase.GetCurrentMethod().Name, base.GetType(), ".", MethodBase.GetCurrentMethod().Name };
            this.objLog.writeTB(string.Concat(objArray1));
            this.OkToClose = true;
            string resp_pos = string.Empty;

            try
            {
                string[] strArray;
                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "Procesando comando --" + comando);
                if (!this.comPort.IsOpen)
                {
                    this.OkToClose = this.AbrirPuerto();
                }
                if (!this.OkToClose)
                {
                    goto TR_0002;
                }
                else
                {
                    if (comando == "0100") /*Si el comando es Polling*/
                    {
                        if (!this.Pooling())
                        {
                            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "No Hay Comunicaci\x00f3n con el POS");
                            throw new PinpadException("No Hay Comunicaci\x00f3n con el POS de Transbank. Por Favor verifique la Conexi\x00f3n del mismo o Comun\x00edquese con Soporte.");
                        }
                        else
                        {
                            flag = true;
                            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "Respuesta OK " + comando + " = " + resp_pos);
                        }
                        goto TR_000A;
                    }
                    if (comando == "0260|0|" || comando == "0300|") // Si el comando es Detalle de Ventas o Cambiar a POS Normal 
                    {
                        string str7 = comando + "|\x0003";
                        str7 = "\x0002" + str7 + Chr(calculateLRC(Encoding.ASCII.GetBytes(str7)));
                        this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "Comando Administrativo Enviado. " + str7);
                        this.sendCommand(str7);
                        Thread.Sleep(100);
                        int num2 = 0;
                        while (true)
                        {
                            resp_pos = this.receiveCommandNoInteraction();
                            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "Respuesta Comando Administrativo.||" + resp_pos);
                            num2++;
                            flag = true;
                            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "Respuesta OK " + comando + " = " + resp_pos);
                            goto TR_000A;
                        }
                    }
                    else
                    {
                        string str7 = comando + "|\x0003";
                        str7 = "\x0002" + str7 + Chr(calculateLRC(Encoding.ASCII.GetBytes(str7)));
                        this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "Comando Administrativo Enviado. " + str7);
                        this.sendCommand(str7);
                        Thread.Sleep(100);
                        int num2 = 0;

                        while (true)
                        {
                            resp_pos = this.receiveCommand();
                            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "Respuesta Comando Administrativo.||" + resp_pos);
                            num2++;
                            if ((resp_pos != "NAK_RESPUESTA") || (num2 >= 3))
                            {
                                if (((resp_pos == "NAK") || ((resp_pos == "TIMEOUT") || (resp_pos == "NAK_RESPUESTA"))) || (resp_pos == "EXCEPTION"))
                                {
                                    jPayload = (resp_pos != "EXCEPTION") ? ("ERROR Ejecutando Comando Administrativo. Error: " + resp_pos) : "ERROR Ejecutando Comando Administrativo. Error de Conexi\x00f3n con el POS";
                                    this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + jPayload);
                                    this.OkToClose = false;
                                    goto TR_000A;
                                }
                                else
                                {
                                    char[] separator = new char[] { '|' };
                                    strArray = resp_pos.Split(separator);
                                    string str8 = this.ErrorEvaluation(strArray[1]);
                                    if (strArray[1] != "00")
                                    {
                                        this.OkToClose = false;
                                        string[] textArray3 = new string[] { "\t", MethodBase.GetCurrentMethod().Name, "ERROR Ejecutando Comando Administrativo. Error: ", strArray[1], " - ", str8 };
                                        this.objLog.writeTB(string.Concat(textArray3));
                                        jPayload = "ERROR Ejecutando Comando Administrativo. Error: " + strArray[1] + " - " + str8;
                                        goto TR_000A;
                                    }
                                    else
                                    {
                                        flag = true;
                                        this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "Respuesta OK " + comando + " = " + resp_pos);
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                jPayload = "EXCEPCION - COMANDOS ADMINISTRATIVOS  <<<Excepcion>>> proxy- " + exception.Message;
                string[] textArray4 = new string[] { "\t", MethodBase.GetCurrentMethod().Name, "<<<Exception>>> proxy: ", exception.Message };
                this.objLog.writeTB(string.Concat(textArray4));
                this.OkToClose = false;
            }
        TR_000A:
            this.CerrarPuerto();
            goto TR_0002;
        TR_0002:
            if (jPayload != string.Empty)
            {
                throw new PinpadException(jPayload);
            }
           // this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** FIN");
            return flag;
        }

        private bool ActivaPinpad()
        {
            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** INICIO");
            string jPayload = string.Empty;
            string str2 = string.Empty;
            bool flag = false;
            object[] objArray1 = new object[] { "\t", MethodBase.GetCurrentMethod().Name, base.GetType(), ".", MethodBase.GetCurrentMethod().Name };
            this.objLog.writeTB(string.Concat(objArray1));
            this.OkToClose = true;
            try
            {
                string str6;
                string[] strArray;
                string str9;
                string str10;
                string str11;
                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "Validando Datos Generales de la Forma");
                if ((this.txtMonto == "") || (this.txtMonto == "0"))
                {
                    this.OkToClose = false;
                    jPayload = "ERROR: El Campo Monto es Obligatorio y No puede ser 0.";
                }
                if (this.lblModalidad == "Modo Normal")
                {
                    if (this.txtNumeroTarjeta == "")
                    {
                        this.OkToClose = false;
                        jPayload = "ERROR: El Campo N\x00famero Tarjeta es Obligatorio.";
                    }
                    if (this.txtNumeroTarjeta.Length < 4)
                    {
                        this.OkToClose = false;
                        jPayload = "ERROR: El Campo N\x00famero Tarjeta debe tener los 4 \x00faltimos d\x00edgitos de la Tarjeta.";
                    }
                    if ((this.cbTipoTarjeta == "--- SELECCIONE ---") && (this.lblTipoPago == "CREDITO"))
                    {
                        this.OkToClose = false;
                        jPayload = "ERROR: Debe existir un Tipo de Tarjeta.";
                    }
                    if ((this.txtCantidadCuotas == "") || (this.lblTipoPago != "CREDITO"))
                    {
                        this.txtCantidadCuotas = "1";
                    }
                    else if ((Convert.ToInt32(this.txtCantidadCuotas) < 1) || (Convert.ToInt32(this.txtCantidadCuotas) > 0x24))
                    {
                        this.OkToClose = false;
                        jPayload = "ERROR: N\x00famero de Cuotas no Autorizado.";
                    }
                    if (this.txtNumeroAutorizacion == "")
                    {
                        this.OkToClose = false;
                        jPayload = "ERROR: El Campo N\x00famero de Autorizaci\x00f3n es Obligatorio.";
                    }
                }
                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "Fin Proceso de Validaci\x00f3n de Campos de la Forma" + this.OkToClose.ToString());
                if (this.OkToClose)
                {
                    if (this.lblModalidad != "Modo Normal")
                    {
                        this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "Transaccion de Venta Integrada.");
                        str6 = string.Empty;
                        if (!this.comPort.IsOpen)
                        {
                            this.OkToClose = this.AbrirPuerto();
                        }
                        if (!this.OkToClose)
                        {
                            goto TR_0002;
                        }
                        else
                        {
                            str2 = "1";
                            if ((this.lblFolio != string.Empty) && (this.lblFolio.Length > 6))
                            {
                                str2 = "2";
                                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "Numero de Folio Mayor de 6 D\x00edgitos. Se va a Truncar.");
                                this.lblFolio = this.lblFolio.Substring(0, 6);
                                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "Numero de Folio Truncado. ");
                            }
                            str2 = "3";
                            object[] objArray2 = new object[] { "0200|", this.txtMonto, "|", this.lblFolio, "|||", 0, "|\x0003" };
                            string str7 = string.Concat(objArray2);
                            str7 = "\x0002" + str7 + Chr(calculateLRC(Encoding.ASCII.GetBytes(str7)));
                            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "Comando de Venta.||" + str7);
                            this.sendCommand(str7);
                            Thread.Sleep(100);
                            int num2 = 0;

                            while (true)
                            {
                                str6 = this.receiveCommand();
                                num2++;
                                if ((str6 != "NAK_RESPUESTA") || (num2 >= 3))
                                {
                                    this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "Respuesta Venta.||" + str6);
                                    if (((str6 == "NAK") || ((str6 == "TIMEOUT") || (str6 == "NAK_RESPUESTA"))) || (str6 == "EXCEPTION"))
                                    {
                                        jPayload = (str6 != "EXCEPTION") ? ("ERROR Ejecutando Transacci\x00f3n de Venta. Error: " + str6) : "ERROR Ejecutando Transacci\x00f3n de Venta. Error de Conexi\x00f3n con el POS";
                                        this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + jPayload);
                                        this.OkToClose = false;
                                        goto TR_000A;
                                    }
                                    else
                                    {
                                        char[] separator = new char[] { '|' };
                                        strArray = str6.Split(separator);
                                        string str8 = this.ErrorEvaluation(strArray[1]);
                                        if (strArray[1] != "00")
                                        {
                                            this.OkToClose = false;
                                            string[] textArray3 = new string[] { "\t", MethodBase.GetCurrentMethod().Name, "ERROR Ejecutando Transacci\x00f3n de Venta. Error: ", strArray[1], " - ", str8 };
                                            this.objLog.writeTB(string.Concat(textArray3));
                                            jPayload = "ERROR Ejecutando Transacci\x00f3n de Venta. Error: " + strArray[1] + " - " + str8;
                                            goto TR_000A;
                                        }
                                        else
                                        {
                                            str9 = string.Empty;
                                            str9 = (strArray[11] != "CR") ? "1" : "0";
                                            str10 = strArray[14];
                                            str11 = string.Empty;
                                            if (this.LoadINIFileTB(iniPath))
                                            {
                                                foreach (DataRow row in dsTarjetasTB.Tables[0].Rows)
                                                {
                                                    if (str10 == row["TIPO_TARJETA"].ToString())
                                                    {
                                                        str11 = row["CODIGO_TARJETA"].ToString();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "Transaccion de Venta Normal.");
                        string str3 = string.Empty;
                        str3 = (this.cbTipoVenta != "Cr\x00e9dito") ? "1" : "0";
                        string str4 = DateTime.Now.Day.ToString().PadLeft(2, '0') + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Year.ToString();
                        string str5 = DateTime.Now.Hour.ToString().PadLeft(2, '0') + DateTime.Now.Minute.ToString().PadLeft(2, '0') + DateTime.Now.Second.ToString().PadLeft(2, '0');
                        string[] textArray1 = new string[0x15];
                        textArray1[0] = "1~";
                        textArray1[1] = str3;
                        textArray1[2] = "~";
                        textArray1[3] = this.txtNumeroTarjeta;
                        textArray1[4] = "~";
                        textArray1[5] = this.cbTipoTarjeta;
                        textArray1[6] = "~";
                        textArray1[7] = this.txtCantidadCuotas;
                        textArray1[8] = "~";
                        textArray1[9] = this.txtMontoCuota;
                        textArray1[10] = "~";
                        textArray1[11] = this.txtNumeroAutorizacion;
                        textArray1[12] = "~";
                        textArray1[13] = this.txtMonto;
                        textArray1[14] = "~~~";
                        textArray1[15] = str4;
                        textArray1[0x10] = "~";
                        textArray1[0x11] = str5;
                        textArray1[0x12] = "~~";
                        textArray1[0x13] = this.cbTipoTarjeta;
                        textArray1[20] = "~";
                        this.lblResultadoTarjeta = string.Concat(textArray1);
                        this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "lblResultadoTarjeta||" + this.lblResultadoTarjeta);
                        this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "Transaccion de Venta Normal Ejecutada Exitosamente.");
                        TBResponse response1 = new TBResponse();
                        response1.invc_sid = this.lblInv_Sid;
                        response1.tender_type = this.lblTender_type;
                        response1.tender_name = this.lblTender_name;
                        response1.authorization_code = this.txtNumeroAutorizacion;
                        response1.registry_date = DateTime.Now.ToString("ddMMyyyy HH:mm:ss");
                        response1.datatarjeta = this.lblResultadoTarjeta;
                        response1.numero_tarjeta = this.txtNumeroTarjeta;
                        response1.cuotas = this.txtCantidadCuotas;
                        TBResponse response = response1;
                        Sales_Plugin.JsonResponse = response;
                        this.RespuestaBanco = response;
                        flag = true;
                        goto TR_0002;
                    }
                }
                else
                {
                    this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "<<<Error>>> frmCreditCard - btnAceptar_Click Errores en los Registros de Creci\x00f3n de Tarjeta.");
                    jPayload = "Hay Errores en los Registros de Tarjeta de Cr\x00e9dito. Por favor verificar los campos con la exclamaci\x00f3n en rojo.";
                    goto TR_0002;
                }
                goto TR_0037;
            TR_000A:
                this.CerrarPuerto();
                goto TR_0002;
            TR_0037:
                if (str11 == "")
                {
                    str11 = str10;
                }
                string s = str10;
                switch (s)
                {
                    case "TC":
                        str10 = "CMR";
                        break;
                    case "TP":
                        str10 = "LIDER (PRESTO)";
                        break;
                    case "TM":
                        str10 = "PARIS-JUMBO";
                        break;
                    case "AX":
                        str10 = "AMERICAN EXPRESS";
                        break;
                    case "EX":
                        str10 = "EXTRA";
                        break;
                    case "DB":
                        str10 = "REDCOMPRA";
                        break;
                    case "DC":
                        str10 = "DINERS CLUB";
                        break;
                    case "MC":
                        str10 = "MASTER CARD";
                        break;
                    case "VI":
                        str10 = "VISA";
                        break;
                    case "MG":
                        str10 = "MAGNA";
                        break;
                    case "RP":
                        str10 = "RIPLEY (CAR)";
                        break;
                    case "CR":
                        str10 = "CREDENCIAL";
                        break;
                    case "CE":
                        str10 = "CERRADA";
                        break;
                    case "CA":
                        str10 = "CABAL";
                        break;
                }
                string[] textArray2 = new string[0x1c];
                textArray2[0] = "0~";
                textArray2[1] = str9;
                textArray2[2] = "~";
                textArray2[3] = strArray[9];
                textArray2[4] = "~";
                textArray2[5] = str10;
                textArray2[6] = "~";
                textArray2[7] = strArray[7];
                textArray2[8] = "~";
                textArray2[9] = strArray[8];
                textArray2[10] = "~";
                textArray2[11] = strArray[5];
                textArray2[12] = "~";
                textArray2[13] = strArray[6];
                textArray2[14] = "~";
                textArray2[15] = strArray[10];
                textArray2[0x10] = "~";
                textArray2[0x11] = strArray[12];
                textArray2[0x12] = "~";
                textArray2[0x13] = strArray[15];
                textArray2[20] = "~";
                textArray2[0x15] = strArray[0x10];
                textArray2[0x16] = "~";
                textArray2[0x17] = strArray[13];
                textArray2[0x18] = "~";
                textArray2[0x19] = str11;
                textArray2[0x1a] = "~";
                textArray2[0x1b] = str6;
                this.lblResultadoTarjeta = string.Concat(textArray2);
                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "lblResultadoTarjeta||" + this.lblResultadoTarjeta);
                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "Transaccion de Venta Integrada Ejecutada Exitosamente.");
                TBResponse response3 = new TBResponse();
                response3.invc_sid = this.lblInv_Sid;
                response3.tender_type = this.lblTender_type;
                response3.tender_name = this.lblTender_name;
                response3.authorization_code = strArray[5];
                response3.registry_date = DateTime.Now.ToString("ddMMyyyy HH:mm:ss");
                response3.datatarjeta = this.lblResultadoTarjeta;
                response3.numero_tarjeta = strArray[9];
                response3.cuotas = strArray[7];
                TBResponse response2 = response3;
                Sales_Plugin.JsonResponse = response2;
                this.RespuestaBanco = response2;
                flag = true;
                goto TR_000A;
            }
            catch (Exception exception)
            {
                jPayload = "EXCEPCION - PAGO CON TARJETA  <<<Excepcion>>> proxy- " + exception.Message + " - " + str2;
                string[] textArray4 = new string[] { "\t", MethodBase.GetCurrentMethod().Name, "<<<Exception>>> proxy: ", exception.Message, " - ", str2 };
                this.objLog.writeTB(string.Concat(textArray4));
                this.OkToClose = false;
            }
        TR_0002:
            if (jPayload != string.Empty)
            {
                throw new PinpadException(jPayload);
            }
            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** FIN");
            return flag;
        }

        private bool ConectaPinpad()
        {
            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** INICIO");
            bool flag = true;
            try
            {
                this.InicializaConfiguracionPinpad();
                flag = this.ActivaPinpad();
            }
            catch (PinpadException exception1)
            {
                throw exception1;
            }
            catch (Exception exception2)
            {
                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + " *****Error:" + exception2.Message);
                this.objLog.write("TRANSBANK\t" + MethodBase.GetCurrentMethod().Name + "*******Error_TB  ex: " + exception2.Message);
                flag = false;
            }
            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "****** FIN");
            return flag;
        }

        private void InicializaConfiguracionPinpad()
        {
            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** INICIO");
            this.cbTipoVenta = (this.lblTipoPago != "CREDITO") ? "D\x00e9bito" : "Cr\x00e9dito";
            string paramPath = Sales_Plugin.PRISMWorkstationPath + Sales_Plugin.PRISMWorkstationName + @"\Plugins\";
            this.VeficaPath(paramPath);
            iniPath = paramPath + Sales_Plugin.strINITarjetas;
            this.LoadINIFileDesc(iniPath);
            if (Sales_Plugin.strTipoConexionTB != "POS Conectado")
            {
                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "POS Transbank en Modo Normal");
                this.lblModalidad = "Modo Normal";
            }
            else
            {
                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "Evaluando si el POS esta Conectado al Computador");
                if (!this.Pooling())
                {
                    this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "No Hay Comunicaci\x00f3n con el POS");
                    throw new PinpadException("No Hay Comunicaci\x00f3n con el POS de Transbank. Por Favor verifique la Conexi\x00f3n del mismo o Comun\x00edquese con Soporte.");
                }
                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "POS Transbank en Modo Integrado " + this.lblInv_Sid);
                this.lblModalidad = "Modo Integrado";
            }
            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** FIN");
        }

        private void CerrarPuerto()
        {
            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** INICIO");
            if (this.comPort.IsOpen)
            {
                this.comPort.Close();
            }
            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** FIN");
        }

        public bool LoadINIFileDesc(string strPath)
        {
            bool flag;
            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** INICIO");
            string jPayload = string.Empty;
            DataColumn column = new DataColumn("TIPO_TARJETA", typeof(string));
            DataColumn column2 = new DataColumn("CODIGO_TARJETA", typeof(string));
            try
            {
                dsTarjetas = new DataSet();
                dtTarjetas = new DataTable();
                dtTarjetas.TableName = "TARJETAS";
                dsTarjetas.Tables.Add(dtTarjetas);
                DataColumn[] columns = new DataColumn[] { column, column2 };
                dsTarjetas.Tables["TARJETAS"].Columns.AddRange(columns);
                flag = true;
            }
            catch (Exception exception)
            {
                jPayload = "ERROR EN PAGO CON TARJETA DE CREDITO LoadINIFileDesc - Error Leyendo Archivo .INI de Tarjetas: " + exception.Message;
                flag = false;
            }
            finally
            {
                if (column != null)
                {
                    column.Dispose();
                }
                if (column2 != null)
                {
                    column2.Dispose();
                }
                if (dtTarjetas != null)
                {
                    dtTarjetas.Dispose();
                }
                if (jPayload != string.Empty)
                {
                    throw new PinpadException(jPayload);
                }
                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** FIN");
            }
            return flag;
        }

        public bool LoadINIFileTB(string strPath)
        {
            bool flag;
            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** INICIO");
            string jPayload = string.Empty;
            DataColumn column = new DataColumn("TIPO_TARJETA", typeof(string));
            DataColumn column2 = new DataColumn("CODIGO_TARJETA", typeof(string));
            try
            {
                dsTarjetasTB = new DataSet();
                dtTarjetasTB = new DataTable();
                dtTarjetasTB.TableName = "TARJETAS";
                dsTarjetasTB.Tables.Add(dtTarjetasTB);
                DataColumn[] columns = new DataColumn[] { column, column2 };
                dsTarjetasTB.Tables["TARJETAS"].Columns.AddRange(columns);
                flag = true;
            }
            catch (Exception exception)
            {
                jPayload = " ERROR EN PAGO CON TARJETA DE CREDITO LoadINIFileTB - Error Leyendo Archivo .INI de Tarjetas: " + exception.Message;
                flag = false;
            }
            finally
            {
                if (column != null)
                {
                    column.Dispose();
                }
                if (column2 != null)
                {
                    column2.Dispose();
                }
                if (dtTarjetasTB != null)
                {
                    dtTarjetasTB.Dispose();
                }
                if (jPayload != string.Empty)
                {
                    throw new PinpadException(jPayload);
                }
                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** FIN");
            }
            return flag;
        }

        private bool Pooling()
        {
            bool flag3;
            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** INICIO");
            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "EJECUTANDO POOLING -->" + this.lblInv_Sid);
            string str = string.Empty;
            if (!this.comPort.IsOpen && !this.AbrirPuerto())
            {
                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + " NO SE VERIFICO ACCESO AL PUERTO***** FIN");
                flag3 = false;
            }
            else
            {
                string s = "0100\x0003";
                s = "\x0002" + s + Chr(calculateLRC(Encoding.ASCII.GetBytes(s)));
                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "Comando Pooling||" + s);
                try
                {
                    this.sendCommand(s);
                    Thread.Sleep(100);
                    int num = 0;
                    while (true)
                    {
                        str = this.receiveCommandNoInteraction();
                        num++;
                        if ((str != "NAK_RESPUESTA") || (num >= 3))
                        {
                            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "Respuesta Pooling||" + str);
                            this.CerrarPuerto();
                            if (((str == "NAK") || ((str == "TIMEOUT") || (str == "NAK_RESPUESTA"))) || (str == "EXCEPTION"))
                            {
                                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** FIN");
                                flag3 = false;
                            }
                            else if (str != "")
                            {
                                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** FIN");
                                flag3 = false;
                            }
                            else
                            {
                                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** FIN");
                                flag3 = true;
                            }
                            break;
                        }
                    }
                }
                catch (Exception exception)
                {
                    throw new PinpadException("Error ejecutando comandos del pooling - " + exception.Message);
                }
            }
            return flag3;
        }

        private string ProcesaVenta(string strSID, string strTenderType, string strTenderName)
        {
            string respuestaTransaccion = string.Empty;
            object[] objArray1 = new object[] { "\t", MethodBase.GetCurrentMethod().Name, " **** fAmount Original ", this.fAmount };
            this.objLog.writeTB(string.Concat(objArray1));
            this.lblTender_type = strTenderType;
            this.lblTender_name = strTenderName;
            this.txtMonto = this.fAmount.ToString();
            this.lblTipoPago = (strTenderType == "11") ? "DEBITO" : "CREDITO";
            string str2 = strSID;
            this.lblInv_Sid = str2;
            string[] textArray1 = new string[] { "\t", MethodBase.GetCurrentMethod().Name, " **** strInvc_Sid||", str2, "||strTenderType:", strTenderType, "||strTenderName:", strTenderName };
            this.objLog.writeTB(string.Concat(textArray1));
            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + " **** Pantalla de Tarjeta de D\x00e9bito");
            Sales_Plugin.strTipoConexionTB = "POS Conectado";
            try
            {
                if (!this.ConectaPinpad())
                {
                    this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + " **** Se Cancelo el Pago con Tarjeta de Cr\x00e9dito / D\x00e9bito ");
                }
                else
                {
                    this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + " **** Comienza Almacenamiento de Tarjeta");
                    char[] separator = new char[] { '~' };
                    string[] strArray = this.lblResultadoTarjeta.Split(separator);
                    decimal num = Convert.ToDecimal(this.txtMonto);
                    object[] objArray2 = new object[] { "\t", MethodBase.GetCurrentMethod().Name, " **** decMonto||", num };
                    this.objLog.writeTB(string.Concat(objArray2));
                    if (strArray[1] != "0")
                    {
                        if (strArray[1] == "1")
                        {
                            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + " **** Tipo de Tarjeta DEBITO");
                            try
                            {
                                if (Convert.ToInt32(strArray[4]) == 0)
                                {
                                }
                            }
                            catch
                            {
                            }
                            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + " **** Termino Asignaci\x00f3n Datos de la Tarjeta de D\x00e9bito ");
                        }
                    }
                    else
                    {
                        int num2;
                        this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + " **** Tipo de Tarjeta CREDITO");
                        string str3 = strArray[3];
                        if (str3 == "VISA")
                        {
                            str3 = "VISA";
                            num2 = 1;
                        }
                        else if (str3 == "MASTER CARD")
                        {
                            str3 = "MASTER";
                            num2 = 2;
                        }
                        else if (str3 == "AMERICAN EXPRESS")
                        {
                            str3 = "AMEX";
                            num2 = 3;
                        }
                        else if (str3 == "MAGNA")
                        {
                            str3 = "MAGNA";
                            num2 = 4;
                        }
                        else if (str3 == "DINERS CLUB")
                        {
                            str3 = "DINERS";
                            num2 = 5;
                        }
                        else if (str3 == "LIDER (PRESTO)")
                        {
                            str3 = "LIDER (PRESTO)";
                            num2 = 6;
                        }
                        else if (str3 == "RIPLEY (CAR)")
                        {
                            str3 = "RIPLEY (CAR)";
                            num2 = 7;
                        }
                        else if (str3 == "HITES")
                        {
                            str3 = "HITES";
                            num2 = 8;
                        }
                        else if (str3 == "JOHNSON")
                        {
                            str3 = "JOHNSON";
                            num2 = 10;
                        }
                        else if (str3 == "ABC")
                        {
                            str3 = "ABC";
                            num2 = 9;
                        }
                        else if (str3 == "DIN")
                        {
                            str3 = "DIN";
                            num2 = 9;
                        }
                        else if (str3 == "PARIS-JUMBO")
                        {
                            str3 = "PARIS-JUMBO";
                            num2 = 10;
                        }
                        else if (str3 == "REGALIAS AL PERSONAL")
                        {
                            str3 = "REGALIAS AL PERSONAL";
                            num2 = 11;
                        }
                        else if (str3 != "SODEXHO")
                        {
                            num2 = 0;
                        }
                        else
                        {
                            str3 = "SODEXHO";
                            num2 = 12;
                        }
                        this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + " **** strCardType||" + str3);
                        object[] objArray3 = new object[] { "\t", MethodBase.GetCurrentMethod().Name, " **** fCardType||", num2 };
                        this.objLog.writeTB(string.Concat(objArray3));
                        try
                        {
                            if (Convert.ToInt32(strArray[4]) == 0)
                            {
                            }
                        }
                        catch
                        {
                        }
                        this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + " **** Termino Asignaci\x00f3n Datos de la Tarjeta de Cr\x00e9dito");
                    }
                    respuestaTransaccion = this.RespuestaTransaccion;
                }
            }
            catch (PinpadException exception1)
            {
                throw exception1;
            }
            return respuestaTransaccion;
        }

        public string processOnePay(string amount, string occ, string externalUniqueNumber, string authorizationCode, string strTransactionSID)
        {
            string[] textArray1 = new string[0x13];
            textArray1[0] = "****INICIO TRANSACCION REFUND*****\n\r\tocc: ";
            textArray1[1] = occ;
            textArray1[2] = Environment.NewLine;
            textArray1[3] = "\tamount:";
            textArray1[4] = amount;
            textArray1[5] = Environment.NewLine;
            textArray1[6] = "\texternalUniqueNumber:";
            textArray1[7] = externalUniqueNumber;
            textArray1[8] = Environment.NewLine;
            textArray1[9] = "\tauthorizationCode:";
            textArray1[10] = authorizationCode;
            textArray1[11] = Environment.NewLine;
            textArray1[12] = "\tAPIKEY:";
            textArray1[13] = this.op_options.ApiKey;
            textArray1[14] = Environment.NewLine;
            textArray1[15] = "\tSHARED-SECRET:";
            textArray1[0x10] = this.op_options.SharedSecret;
            textArray1[0x11] = Environment.NewLine;
            textArray1[0x12] = "\t";
            this.objLog.writeTB(string.Concat(textArray1));
            if (this.op_isProduction)
            {
                Transbank.Onepay.Onepay.IntegrationType = OnepayIntegrationType.Live;
                Transbank.Onepay.Onepay.ApiKey = this.op_options.ApiKey;
                Transbank.Onepay.Onepay.SharedSecret = this.op_options.SharedSecret;
            }
            OPResponse response = new OPResponse();
            string[] textArray2 = new string[12];
            textArray2[0] = "CONIGURACION ONEPAY: \n\r\tCallbackUrl:";
            textArray2[1] = Transbank.Onepay.Onepay.CallbackUrl;
            textArray2[2] = Environment.NewLine;
            textArray2[3] = "\tCurrentIntegrationTypeUrl:";
            textArray2[4] = Transbank.Onepay.Onepay.CurrentIntegrationTypeUrl;
            textArray2[5] = Environment.NewLine;
            textArray2[6] = "\tIntegrationType:";
            textArray2[7] = Transbank.Onepay.Onepay.IntegrationType.Key;
            textArray2[8] = "\tApiBase";
            textArray2[9] = Transbank.Onepay.Onepay.IntegrationType.ApiBase;
            textArray2[10] = "\tAppKey";
            textArray2[11] = Transbank.Onepay.Onepay.IntegrationType.AppKey;
            this.objLog.writeTB(string.Concat(textArray2));
            try
            {
                RefundCreateResponse response2;
                Refund refund = new Refund();
                this.objLog.writeTB("Ejecutando Refund.Create");
                response2 = this.op_isProduction ? Refund.Create((long)Convert.ToInt32(amount), occ, externalUniqueNumber, authorizationCode, this.op_options) : (response2 = Refund.Create((long)Convert.ToInt32(amount), occ, externalUniqueNumber, authorizationCode));
                object[] objArray1 = new object[0x11];
                objArray1[0] = "Fin ejecuci\x00f3n Refund.Create Response: ";
                objArray1[1] = response2.ToString();
                objArray1[2] = Environment.NewLine;
                objArray1[3] = "\tExternalUniqueNumber:";
                objArray1[4] = response2.ExternalUniqueNumber;
                objArray1[5] = Environment.NewLine;
                objArray1[6] = "\tIssuedAt:";
                objArray1[7] = response2.IssuedAt;
                objArray1[8] = Environment.NewLine;
                objArray1[9] = "\tOcc:";
                objArray1[10] = response2.Occ;
                objArray1[11] = Environment.NewLine;
                objArray1[12] = "\tReverseCode:";
                objArray1[13] = response2.ReverseCode;
                objArray1[14] = Environment.NewLine;
                objArray1[15] = "\tSignature:";
                objArray1[0x10] = response2.Signature;
                this.objLog.writeTB(string.Concat(objArray1));
                response.code = 0;
                response.strMessage = "";
                response.occ = response2.Occ;
                response.externalUniqueNumber = response2.ExternalUniqueNumber;
                response.issuedAt = response2.IssuedAt.ToString();
                response.amount = amount;
                response.ReverseCode = response2.ReverseCode;
            }
            catch (TransbankException exception)
            {
                response.code = 2;
                response.strMessage = exception.Message;
                response.errormsg = exception.Message;
                return JsonConvert.SerializeObject(response);
            }
            finally
            {
                string[] textArray3 = new string[0x13];
                textArray3[0] = "****FIN TRANSACCION REFUND*****\n\r\tocc: ";
                textArray3[1] = occ;
                textArray3[2] = Environment.NewLine;
                textArray3[3] = "\tamount:";
                textArray3[4] = amount;
                textArray3[5] = Environment.NewLine;
                textArray3[6] = "\texternalUniqueNumber:";
                textArray3[7] = externalUniqueNumber;
                textArray3[8] = Environment.NewLine;
                textArray3[9] = "\tauthorizationCode:";
                textArray3[10] = authorizationCode;
                textArray3[11] = Environment.NewLine;
                textArray3[12] = "\tAPIKEY:";
                textArray3[13] = this.op_options.ApiKey;
                textArray3[14] = Environment.NewLine;
                textArray3[15] = "\tSHARED-SECRET:";
                textArray3[0x10] = this.op_options.SharedSecret;
                textArray3[0x11] = Environment.NewLine;
                textArray3[0x12] = "\t";
                this.objLog.writeTB(string.Concat(textArray3));
            }
            return JsonConvert.SerializeObject(response);
        }

        private static void FillOnePayParameters(string[] parametros, ref string op_amount, ref string op_occ, ref string op_externalUniqueNumber, ref string op_authorizationCode, ref string op_strTransactionSID)
        {
            foreach (string str in parametros)
            {
                char[] separator = new char[] { '=' };
                string[] strArray2 = str.Split(separator);
                string str2 = strArray2[0];
                if (str2 == "strAmount")
                {
                    op_amount = strArray2[1];
                }
                else if (str2 == "occ")
                {
                    op_occ = strArray2[1];
                }
                else if (str2 == "externalUniqueNumber")
                {
                    op_externalUniqueNumber = strArray2[1];
                }
                else if (str2 == "authorizationCode")
                {
                    op_authorizationCode = strArray2[1];
                }
                else if (str2 == "strTransactionSID")
                {
                    op_strTransactionSID = strArray2[1];
                }
            }
        }

        protected string processTransbank(string strSID, string paramfAmount, string paramTenderType, string paramTenderName)
        {
            string str = string.Empty;
            this.objLog.writeTB("processTransbank *****INICIO PROCESAMIENTO " + DateTime.Now.ToShortTimeString());
            try
            {
                if ((paramTenderType == "11") || (paramTenderType == "2"))
                {
                    this.objLog.writeTB("\tTENDER NAME: " + paramTenderName);
                    if (paramTenderName.ToUpper().IndexOf("TB_") < 0)
                    {
                        this.fAmount = Convert.ToDecimal(paramfAmount);
                        str = this.ProcesaVenta(strSID, paramTenderType, paramTenderName);
                    }
                }
            }
            catch (PinpadException exception1)
            {
                throw exception1;
            }
            catch (Exception exception2)
            {
                this.objLog.writeTB("processTransbank *****Error: " + exception2.Message);
                this.objLog.write("processTransbank *****Error_TB: " + exception2.Message);
            }
            finally
            {
                this.objLog.writeTB("processTransbank *****FIN PROCESAMIENTO " + DateTime.Now.ToShortTimeString());
            }
            return str;
        }

        private string receiveCommand()
        {
            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** INICIO");
            string s = string.Empty;
            byte[] buffer = new byte[0x7d];
            byte[] bytes = new byte[0x5dc];
            int index = 0;
            int num2 = 0;
            try
            {
                while (true)
                {
                    if (this.comPort.Read(buffer, 0, 1) != -1)
                    {
                        if (buffer[0] == 6)
                        {
                            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** RESPONSE_1 - ACK -" + this.lblInv_Sid);
                            num2 = 1;
                        }
                        else
                        {
                            if (buffer[0] != 0x15)
                            {
                                continue;
                            }
                            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** RESPONSE - NAK - " + this.lblInv_Sid);
                            s = "NAK";
                        }
                    }
                    Thread.Sleep(0x3e8);
                    if (num2 == 1)
                    {
                        index = 0;
                        string str2 = string.Empty;
                        string str3 = string.Empty;

                        while (true)
                        {
                            if (this.comPort.Read(buffer, 0, 1) != -1)
                            {
                                if (buffer[0] == 6)
                                {
                                    continue;
                                }
                                s = s + buffer[0];
                                bytes[index] = buffer[0];
                                if (buffer[0] != 3)
                                {
                                    index++;
                                    continue;
                                }
                                s = Encoding.ASCII.GetString(bytes, 0, index + 1);
                                string[] textArray1 = new string[] { "\t", MethodBase.GetCurrentMethod().Name, "***** RESPONSE_2 - ", s, " - ", this.lblInv_Sid };
                                this.objLog.writeTB(string.Concat(textArray1));
                                s = s.Replace('\x0002'.ToString(), string.Empty);
                                str2 = Chr(calculateLRC(Encoding.ASCII.GetBytes(s)));
                                string[] textArray2 = new string[] { "\t", MethodBase.GetCurrentMethod().Name, "***** LRC TRAMA RESPUESTA: ", str2, " - ", this.lblInv_Sid };
                                this.objLog.writeTB(string.Concat(textArray2));
                                if (str2 == "\0")
                                {
                                    this.comPort.Write('\x0006'.ToString());
                                    this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** REQUEST - ACK - " + this.lblInv_Sid);
                                    return s;
                                }
                                else
                                {
                                    this.comPort.Read(buffer, 0, 1);
                                    str3 = Chr(Convert.ToInt32(buffer[0].ToString()));
                                    string[] textArray3 = new string[] { "\t", MethodBase.GetCurrentMethod().Name, "***** RESPONSE - LRC: ", str3, " - ", this.lblInv_Sid };
                                    this.objLog.writeTB(string.Concat(textArray3));
                                }
                            }

                            if (str2 == str3)
                            {
                                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** REQUEST - ACK - " + this.lblInv_Sid);
                                this.comPort.Write('\x0006'.ToString());
                               
                            }
                            else
                            {
                                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** REQUEST - NAK - " + this.lblInv_Sid);
                                this.comPort.Write('\x0015'.ToString());
                                s = "NAK_RESPUESTA";
                            }
                            break;
                        }
                    }
                    break;
                }
            }
            catch (Exception exception)
            {
                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + " *****Error:" + exception.Message);
                this.objLog.write("TRANSBANK\t" + MethodBase.GetCurrentMethod().Name + "*******Error_TB  ex: " + exception.Message);
                s = "EXCEPTION";
            }
            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** FIN");
            return s;
        }

        private string receiveCommandNoInteraction()
        {
            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** FIN");
            string str = string.Empty;
            byte[] buffer = new byte[0x7d];
            byte[] buffer2 = new byte[0x5dc];
            int num = 0;
            try
            {
                while (true)
                {
                    if (this.comPort.Read(buffer, 0, 1) != -1)
                    {
                        if (buffer[0] == 6)
                        {
                            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** RESPONSE - ACK - " + this.lblInv_Sid);
                        }
                        else if (buffer[0] == 0x15)
                        {
                            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** RESPONSE - NAK - " + this.lblInv_Sid);
                            str = "NAK";
                        }
                        else
                        {
                            Thread.Sleep(100);
                            if ((num + 1) != 0x3e8)
                            {
                                continue;
                            }
                            str = "TIMEOUT";
                        }
                    }
                    Thread.Sleep(0x3e8);
                    break;
                }
            }
            catch (Exception exception)
            {
                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + " *****Error:" + exception.Message);
                this.objLog.write("TRANSBANK\t" + MethodBase.GetCurrentMethod().Name + "*******Error_TB  ex: " + exception.Message);
                str = "EXCEPTION";
            }
            return str;
        }

        private void sendCommand(string strComando)
        {
            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** INICIO");
            try
            {
                string[] textArray1 = new string[] { "\t", MethodBase.GetCurrentMethod().Name, " REQUEST - ", strComando, " - ", this.lblInv_Sid };
                this.objLog.writeTB(string.Concat(textArray1));
                string str = this.comPort.ReadExisting();
                this.comPort.Write(strComando);
            }
            catch (Exception exception)
            {
                this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + " *****Error:" + exception.Message);
                this.objLog.write("TRANSBANK\t" + MethodBase.GetCurrentMethod().Name + "*******Error_TB  ex: " + exception.Message);
            }
            this.objLog.writeTB("\t" + MethodBase.GetCurrentMethod().Name + "***** FIN");
        }

        public string ErrorEvaluation(string InputError)
        {
            string s = InputError;
            switch (s)
            {
                case "00":
                    return "Aprobado";
                case "01":
                    return "Rechazado";
                case "02":
                    return "Host no Responde";
                case "03":
                    return "Conexi\x00f3n Fallo";
                case "04":
                    return "Transacci\x00f3n ya Fue Anulada";
                case "05":
                    return "No existe Transacci\x00f3n para Anular";
                case "06":
                    return "Tarjeta no Soportada";
                case "07":
                    return "Transacci\x00f3n Cancelada desde el POS";
                case "08":
                    return "No puede Anular Transacci\x00f3n Debito";
                case "09":
                    return "Error Lectura Tarjeta";
                case "10":
                    return "Monto menor al m\x00ednimo permitido";
                case "11":
                    return "No existe venta";
                case "12":
                    return "Transacci\x00f3n No Soportada";
                case "13":
                    return "Debe ejecutar cierre";
                case "14":
                    return "No hay Tono";
                case "15":
                    return "Archivo BITMAP.DAT no encontrado. Favor cargue";
                case "16":
                    return "Error Formato Respuesta del HOST";
                case "17":
                    return "Ingreso incorrecto 4 \x00faltimos d\x00edgitos";
                case "18":
                    return "Men\x00fa invalido";
                case "19":
                    return "ERROR_TARJ_DIST";
                case "20":
                    return "Tarjeta Invalida";
                case "21":
                    return "Anulaci\x00f3n D\x00e9bito no permitida";
                case "22":
                    return "Tiempo de Espera Agotado en el POS";
                case "24":
                    return "Impresora del POS sin Papel";
                case "25":
                    return "Fecha Invalida";
                case "26":
                    return "Debe Cargar Llaves";
                case "27":
                    return "Debe Actualizar";
                case "28":
                    return "Debe enviar comercio Prestador";
                case "60":
                    return "Error en N\x00famero de Cuotas";
                case "61":
                    return "Error en Armado de Solicitud";
                case "62":
                    return "Problema con el Pinpad interno";
                case "65":
                    return "Error al Procesar la Respuesta del Host";
                case "67":
                    return "Super\x00f3 N\x00famero M\x00E1ximo de Ventas, Debe Ejecutar Cierre";
                case "68":
                    return "Error Gen\x00E9rico, Falla al Ingresar Montos";
                case "70":
                    return "Error de formato Campo de Boleta MAX 6";
                case "71":
                    return "Error de Largo Campo de Impresi\x00f3n";
                case "72":
                    return "Error de Monto Venta, Debe ser Mayor que 0";
                case "73":
                    return "Terminal ID no configurado";
                case "74":
                    return "Debe Ejecutar CIERRE";
                case "75":
                    return "Comercio no tiene Tarjetas Configuradas";
                case "76":
                    return "Super\x00f3 N\x00famero M\x00E1ximo de Ventas, Debe Ejecutar CIERRE";
                case "77":
                    return "Debe Ejecutar Cierre";
                case "78":
                    return "Esperando Leer Tarjeta";
                case "79":
                    return "Solicitando Confirmar Monto";
                case "81":
                    return "Solicitando Ingreso de Clave";
                case "82":
                    return "Enviando transacci\x00f3n al Host";
                case "88":
                    return "Error Cantidad Cuotas ";
                case "93":
                    return "Rechazo Transacci\x00f3n";
                case "94":
                    return "Error al Procesar Respuesta";
                case "95":
                    return "Error al Imprimir TASA";
                case "96":
                    return "Producto no disponible";
                default:
                    return "Rechazado por transbank";
            }
        }

        public string RespuestaTransaccion
        {
            get
            {
                string str = string.Empty;
                if (this.RespuestaBanco != null)
                {
                    str = JsonConvert.SerializeObject(this.RespuestaBanco);
                }
                return str;
            }
        }

        #endregion

        #region Proceso FEBOS
        public string BuildGetDteInfo(string token, string uid, string febosid)
        {
            string[] textArray1 = new string[] { "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:api=\"http://api.febos.cl/\"><soapenv:Header/><soapenv:Body><api:getDteInfo><token>", token, "</token><uid>", uid, "</uid><febosid>", febosid, "</febosid></api:getDteInfo></soapenv:Body></soapenv:Envelope>" };
            return string.Concat(textArray1);
        }

        public string BuildLOGINMessage(string LoginMail, string LoginPass)
        {
            string[] textArray1 = new string[] { "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:api=\"http://api.febos.cl/\"><soapenv:Header/><soapenv:Body><api:login><mail>", LoginMail, "</mail><pass>", LoginPass, "</pass></api:login></soapenv:Body></soapenv:Envelope>" };
            return string.Concat(textArray1);
        }

        public string BuildUploadDTE4Message(string token, string uid, string dataDTE, string strInternalID)
        {
            string[] textArray1 = new string[9];
            textArray1[0] = "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:api=\"http://api.febos.cl/\"><soapenv:Header/><soapenv:Body><api:uploadTxtDte4><token>";
            textArray1[1] = token;
            textArray1[2] = "</token><uid>";
            textArray1[3] = uid;
            textArray1[4] = "</uid><data>";
            textArray1[5] = dataDTE;
            textArray1[6] = "</data><internalId>";
            textArray1[7] = strInternalID;
            textArray1[8] = "</internalId></api:uploadTxtDte4></soapenv:Body></soapenv:Envelope>";
            return string.Concat(textArray1);
        }

        public string BuildUploadDTEMessage(string token, string uid, string dataDTE, string strInternalID)
        {
            string[] textArray1 = new string[9];
            textArray1[0] = "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:api=\"http://api.febos.cl/\"><soapenv:Header/><soapenv:Body><api:uploadTxtDte><token>";
            textArray1[1] = token;
            textArray1[2] = "</token><uid>";
            textArray1[3] = uid;
            textArray1[4] = "</uid><data>";
            textArray1[5] = dataDTE;
            textArray1[6] = "</data><internalId>";
            textArray1[7] = strInternalID;
            textArray1[8] = "</internalId></api:uploadTxtDte></soapenv:Body></soapenv:Envelope>";
            return string.Concat(textArray1);
        }

        public bool callFEBOSLogin(Log objLog, ref stFEBOSParam stResultFebosParam)
        {
            bool flag = false;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(stResultFebosParam.strFEBOSURLLogin.Replace(@"\", ""));
                request.ContentType = "text/xml";
                request.Method = "POST";
                request.KeepAlive = false;
                string s = this.BuildLOGINMessage(stResultFebosParam.strFEBOSLoginMail, stResultFebosParam.strFEBOSLoginPass);
                byte[] bytes = Encoding.UTF8.GetBytes(s);
                request.ContentLength = bytes.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
                HttpWebResponse response = null;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                    Stream responseStream = response.GetResponseStream();
                    using (response.GetResponseStream())
                    {
                        XmlDocument document = new XmlDocument();
                        document.LoadXml(new StreamReader(responseStream).ReadToEnd());
                        string innerXml = document.GetElementsByTagName("error").Item(0).InnerXml;
                        string str6 = document.GetElementsByTagName("message").Item(0).InnerXml;
                        if ((document.GetElementsByTagName("status").Item(0).InnerXml != "0") || (innerXml != "0"))
                        {
                            stResultFebosParam.intCode = Convert.ToInt16(innerXml);
                            stResultFebosParam.strMessage = str6;
                            objLog.writeFEBOS("Error FEBOS Login: ErrorCode - " + innerXml + ", ErrorMessage -" + str6);
                        }
                        else
                        {
                            string str7 = "";
                            string str8 = "";
                            foreach (XmlNode node in document.GetElementsByTagName("data").Item(0).ChildNodes)
                            {
                                if (node.SelectSingleNode("name").InnerText.Trim().ToLower() == "token")
                                {
                                    str7 = node.SelectSingleNode("value").InnerText.Trim();
                                }
                                if (node.SelectSingleNode("name").InnerText.Trim().ToLower() == "uid")
                                {
                                    str8 = node.SelectSingleNode("value").InnerText.Trim();
                                }
                                if ((str7 != "") && (str8 != ""))
                                {
                                    break;
                                }
                            }
                            stResultFebosParam.strFEBOSToken = str7;
                            stResultFebosParam.strFEBOSUI = str8;
                            string[] textArray1 = new string[] { "Login realizado en FEBOS token  '", str7, "', ui '", str8, "'" };
                            objLog.writeFEBOS(string.Concat(textArray1));
                            flag = true;
                        }
                    }
                    responseStream.Close();
                    responseStream.Dispose();
                }
                catch (WebException exception)
                {
                    response = (HttpWebResponse)exception.Response;
                    stResultFebosParam.intCode = 0x63;
                    stResultFebosParam.strMessage = exception.Message;
                    object[] objArray1 = new object[] { "Error FEBOS Login: ErrorCode - ", stResultFebosParam.intCode, ", ErrorMessage -", stResultFebosParam.strMessage };
                    objLog.writeFEBOS(string.Concat(objArray1));
                }
                response.Close();
                response.Dispose();
            }
            catch (Exception exception2)
            {
                stResultFebosParam.intCode = 0x63;
                stResultFebosParam.strMessage = exception2.Message;
                object[] objArray2 = new object[] { "Error FEBOS Login: ErrorCode - ", stResultFebosParam.intCode, ", ErrorMessage -", stResultFebosParam.strMessage };
                objLog.writeFEBOS(string.Concat(objArray2));
            }
            return flag;
        }

        public bool callFEBOSUploadDTE(Log objLog, ref stFEBOSParam stResultFebosParam, string token, string uid, string dataDTE, string strInternalID)
        {
            bool flag = false;
            try
            {
                string requestUriString = stResultFebosParam.strFEBOSURLUploadDTE.Replace(@"\", "");
                objLog.writeFEBOS("FEBOS URL UploadDTE: " + requestUriString);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUriString);
                request.ContentType = "text/xml";
                request.Method = "POST";
                request.KeepAlive = false;
                string s = this.BuildUploadDTE4Message(token, uid, dataDTE, strInternalID);
                if (this.ParamValues.ModoDebugCompleto == "true")
                {
                    objLog.writeFEBOS("FEBOS DTE: " + Base64Decode(dataDTE));
                    objLog.writeFEBOS("FEBOS UploadDTE Request: " + s);
                }
                byte[] bytes = Encoding.UTF8.GetBytes(s);
                request.ContentLength = bytes.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
                HttpWebResponse response = null;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                    Stream responseStream = response.GetResponseStream();
                    using (response.GetResponseStream())
                    {
                        string str7;
                        string str10;
                        string str11;
                        HttpWebResponse response2;
                        string xml = new StreamReader(responseStream).ReadToEnd();
                        if (this.ParamValues.ModoDebugCompleto == "true")
                        {
                            objLog.writeFEBOS("FEBOS UploadDTE Response: " + xml);
                        }
                        XmlDocument document = new XmlDocument();
                        document.LoadXml(xml);
                        string innerXml = document.GetElementsByTagName("status").Item(0).InnerXml;
                        string str5 = document.GetElementsByTagName("error").Item(0).InnerXml;
                        string str6 = "";
                        try
                        {
                            str6 = document.GetElementsByTagName("message").Item(0).InnerXml;
                        }
                        catch (Exception)
                        {
                        }
                        if ((innerXml != "0") || (str5 != "0"))
                        {
                            string str17 = "";
                            try
                            {
                                foreach (XmlNode node3 in document.GetElementsByTagName("data").Item(0).ChildNodes)
                                {
                                    if (node3.SelectSingleNode("name").InnerText.Trim().ToLower() == "error")
                                    {
                                        str17 = str17 + node3.SelectSingleNode("value").InnerText.Trim() + "\x00a3";
                                    }
                                }
                                if (str17 != "")
                                {
                                    str17 = str17.Substring(0, str17.Length - 1);
                                }
                            }
                            catch (Exception)
                            {
                                str17 = "";
                            }
                            stResultFebosParam.intCode = Convert.ToInt16(str5);
                            stResultFebosParam.strMessage = str6 + "\x00a5detmensaje:" + str17;
                            objLog.writeFEBOS("Error FEBOS UploadDTE: ErrorCode - " + str5 + ", ErrorMessage -" + stResultFebosParam.strMessage);
                            goto TR_0007;
                        }
                        else
                        {
                            str7 = document.GetElementsByTagName("febosid").Item(0).InnerXml;
                            string str8 = "";
                            string str9 = "";
                            str10 = "";
                            str11 = "";
                            try
                            {
                                foreach (XmlNode node in document.GetElementsByTagName("data").Item(0).ChildNodes)
                                {
                                    if (node.SelectSingleNode("name").InnerText.Trim().ToLower() == "ted")
                                    {
                                        str8 = node.SelectSingleNode("value").InnerText.Trim();
                                    }
                                }
                                if (str8 != "")
                                {
                                    str9 = Base64Decode(str8);
                                    str11 = str9;
                                    objLog.writeFEBOS("UploadDTE RESPONSE " + str9);
                                    XmlDocument document2 = new XmlDocument();
                                    document2.LoadXml("<?xml version='1.0' encoding='UTF-8'?>" + str9);
                                    str10 = document2.GetElementsByTagName("F").Item(0).InnerXml;
                                }
                            }
                            catch (Exception)
                            {
                                str9 = "";
                            }
                            if (str10 != "")
                            {
                                goto TR_0008;
                            }
                            else
                            {
                                string str12 = this.BuildGetDteInfo(token, uid, str7);
                                if (this.ParamValues.ModoDebugCompleto == "true")
                                {
                                    objLog.writeFEBOS("FEBOS GetDTEInfo Request: " + str12);
                                }
                                HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create(requestUriString);
                                request2.ContentType = "text/xml";
                                request2.Method = "POST";
                                request2.KeepAlive = false;
                                bytes = Encoding.UTF8.GetBytes(str12);
                                request2.ContentLength = bytes.Length;
                                Stream stream4 = request2.GetRequestStream();
                                stream4.Write(bytes, 0, bytes.Length);
                                stream4.Close();
                                response2 = null;
                                try
                                {
                                    response2 = (HttpWebResponse)request2.GetResponse();
                                    Stream stream5 = response2.GetResponseStream();
                                    using (response2.GetResponseStream())
                                    {
                                        string str13 = new StreamReader(stream5).ReadToEnd();
                                        if (this.ParamValues.ModoDebugCompleto == "true")
                                        {
                                            objLog.writeFEBOS("FEBOS GetDTEInfo Response: " + str13);
                                        }
                                        XmlDocument document3 = new XmlDocument();
                                        document3.LoadXml(str13);
                                        string str15 = document3.GetElementsByTagName("error").Item(0).InnerXml;
                                        string str16 = document3.GetElementsByTagName("message").Item(0).InnerXml;
                                        if ((document3.GetElementsByTagName("status").Item(0).InnerXml == "0") && (str15 == "0"))
                                        {
                                            try
                                            {
                                                foreach (XmlNode node2 in document3.GetElementsByTagName("data").Item(0).ChildNodes)
                                                {
                                                    if (node2.SelectSingleNode("name").InnerText.Trim().ToLower() == "folio")
                                                    {
                                                        str10 = node2.SelectSingleNode("value").InnerText.Trim();
                                                    }
                                                    if (str10 != "")
                                                    {
                                                        break;
                                                    }
                                                }
                                            }
                                            catch (Exception exception)
                                            {
                                                objLog.writeFEBOS("Error ReadGetDTEInfo: " + exception.Message);
                                            }
                                        }
                                    }
                                    stream5.Close();
                                    stream5.Dispose();
                                }
                                catch (Exception exception2)
                                {
                                    objLog.writeFEBOS("Error GetDTEInfo: " + exception2.Message);
                                }
                            }
                        }
                        goto TR_0009;
                    TR_0008:
                        stResultFebosParam.strFEBOSID = str7;
                        stResultFebosParam.strFolio = str10;
                        stResultFebosParam.strTED = str11;
                        string[] textArray1 = new string[] { "UploadDTE realizado con exito en FEBOS febosid  '", str7, "', Folio: '", str10, "'" };
                        objLog.writeFEBOS(string.Concat(textArray1));
                        flag = true;
                        goto TR_0007;
                    TR_0009:
                        response2.Close();
                        response2.Dispose();
                        goto TR_0008;
                    }
                TR_0007:
                    responseStream.Close();
                    responseStream.Dispose();
                }
                catch (WebException exception3)
                {
                    response = (HttpWebResponse)exception3.Response;
                    stResultFebosParam.intCode = 0x63;
                    stResultFebosParam.strMessage = exception3.Message;
                    object[] objArray1 = new object[] { "Error FEBOS UploadDTE: ErrorCode - ", stResultFebosParam.intCode, ", ErrorMessage -", stResultFebosParam.strMessage };
                    objLog.writeFEBOS(string.Concat(objArray1));
                }
                response.Close();
                response.Dispose();
            }
            catch (Exception exception4)
            {
                stResultFebosParam.intCode = 0x63;
                stResultFebosParam.strMessage = exception4.Message;
                object[] objArray2 = new object[] { "Error FEBOS UPLOADDTE: ErrorCode - ", stResultFebosParam.intCode, ", ErrorMessage -", stResultFebosParam.strMessage };
                objLog.writeFEBOS(string.Concat(objArray2));
            }
            return flag;
        }

        public string doPRISMLogin(Log objLog)
        {
          //  objLog.write(this.ParamValues.PRISMURL);
            string str = "";
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(this.ParamValues.PRISMURL + "/v1/rest/auth"),
                DefaultRequestHeaders = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } }
            };
            HttpResponseMessage result = client.GetAsync("").Result;
            if (!result.IsSuccessStatusCode)
            {
                object[] objArray1 = new object[] { "Error LOGIN Auth-Nonce: ", result.StatusCode, ", ", result.ReasonPhrase };
               // objLog.write(string.Concat(objArray1));
            }
            else
            {
                IEnumerable<string> enumerable;
                string str3 = "";

                if (result.Headers.TryGetValues("Auth-Nonce", out enumerable))
                {
                    str3 = enumerable.First<string>();
                }
                string str4 = "";
                if (str3 != "")
                {
                    str4 = ((decimal.Truncate(Convert.ToUInt64(str3) / ((long)13)) % 99999M) * 17M).ToString();
                }
                client.Dispose();
                if ((str3 != "") && (str4 != ""))
                {
                    HttpClient client2 = new HttpClient
                    {
                        BaseAddress = new Uri(this.ParamValues.PRISMURL + "/v1/rest/auth"),
                        DefaultRequestHeaders = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } }
                    };
                   // objLog.write(this.ParamValues.PRISMURL + "/v1/rest/auth");
                   // objLog.write("Auth-Nonce = " +str3);
                   // objLog.write("Auth-Nonce-Response" + str4);
                    client2.DefaultRequestHeaders.Add("Auth-Nonce", str3);
                    client2.DefaultRequestHeaders.Add("Auth-Nonce-Response", str4);
                    HttpResponseMessage message2 = client2.GetAsync("?usr=" + this.ParamValues.PRISMUser + "&pwd=" + this.ParamValues.PRISMPwd).Result;
                    if (!message2.IsSuccessStatusCode)
                    {
                        objLog.write("Error LOGIN buscando Token: " + message2.StatusCode.ToString() + ", " + message2.ReasonPhrase);
                    }
                    else
                    {
                        IEnumerable<string> enumerable2;
                        if (message2.Headers.TryGetValues("Auth-Session", out enumerable2))
                        {
                         //   objLog.write(str);
                            str = enumerable2.First<string>();

                        }
                        client2.Dispose();
                    }
                }
            }
          //  objLog.write("Session = "+str);
            return str;
        }


        public string doPRISMLoginAux(Log objLog)
        {
            string str = "";
           // objLog.write("estoy en doPRISMLoginAux");
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(this.ParamValues.PRISMURL + "/v1/rest/auth"),
                DefaultRequestHeaders = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } }
            };
            HttpResponseMessage result = client.GetAsync("").Result;
           // objLog.write("ejecute la siguiente URL "+ this.ParamValues.PRISMURL + "/v1/rest/auth");
           // objLog.write("la llamada devuelve "+ result.IsSuccessStatusCode);
            if (!result.IsSuccessStatusCode)
            {
                object[] objArray1 = new object[] { "Error LOGIN Auth-Nonce: ", result.StatusCode, ", ", result.ReasonPhrase };
                objLog.write(string.Concat(objArray1));
            }
            else
            {
               // objLog.write("Voy a procesar el Auth-Nonce ");
                IEnumerable<string> enumerable;
                string str3 = "";

                if (result.Headers.TryGetValues("Auth-Nonce", out enumerable))
                {
                    str3 = enumerable.First<string>();
                }
                string str4 = "";
                if (str3 != "")
                {
                    str4 = ((decimal.Truncate(Convert.ToUInt64(str3) / ((long)13)) % 99999M) * 17M).ToString();
                }
                client.Dispose();
                if ((str3 != "") && (str4 != ""))
                {
                    HttpClient client2 = new HttpClient
                    {
                        BaseAddress = new Uri(this.ParamValues.PRISMURL + "/v1/rest/auth"),
                        DefaultRequestHeaders = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } }
                    };
                    client2.DefaultRequestHeaders.Add("Auth-Nonce", str3);
                    client2.DefaultRequestHeaders.Add("Auth-Nonce-Response", str4);
                    HttpResponseMessage message2 = client2.GetAsync("?usr=" + this.ParamValues.PRISMUser + "&pwd=" + this.ParamValues.PRISMPwd).Result;
                    objLog.write(this.ParamValues.PRISMURL + "/v1/rest/auth"+ "?usr=" + this.ParamValues.PRISMUser + "&pwd=" + this.ParamValues.PRISMPwd);
                    objLog.write("estatus llamado "+ message2.IsSuccessStatusCode);
                    if (!message2.IsSuccessStatusCode)
                    {
                        objLog.write("Error LOGIN buscando Token: " + message2.StatusCode.ToString() + ", " + message2.ReasonPhrase);
                    }
                    else
                    {
                        IEnumerable<string> enumerable2;
                        if (message2.Headers.TryGetValues("Auth-Session", out enumerable2))
                        {
                           // objLog.write(str);
                            str = enumerable2.First<string>();

                        }
                        client2.Dispose();

                        var client3 = new RestClient(this.ParamValues.PRISMURL + "/v1/rest/sit?ws=webclient");
                        var request3 = new RestRequest(Method.GET);
                        request3.AddHeader("Auth-Session", str);
                        request3.AddHeader("Accept", "application/json, version=2");
                        request3.AddHeader("Content-Type", "application/json; charset=utf-8");
                        IRestResponse response3 = client3.Execute(request3);

                        if (response3.StatusCode != HttpStatusCode.OK)
                        {
                            objLog.write("StatusCode  1= " + response3.StatusCode);
                        }
                        else
                        {
                            objLog.write("StatusCode 2= " + response3.StatusCode);

                        }

                    }
                }
            }
           // objLog.write("token " + str);
            return str;
        }

        public string doPRISMLoginCentral(Log objLog)
        {
            string str = "";
            objLog.write("estoy en doPRISMLoginCentral "+ this.ParamValues.PRISMURLForReferencia + "/v1/rest/auth");
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(this.ParamValues.PRISMURLForReferencia + "/v1/rest/auth"),
                DefaultRequestHeaders = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } }
            };
            HttpResponseMessage result = client.GetAsync("").Result;
            // objLog.write("ejecute la siguiente URL "+ this.ParamValues.PRISMURL + "/v1/rest/auth");
            // objLog.write("la llamada devuelve "+ result.IsSuccessStatusCode);
            if (!result.IsSuccessStatusCode)
            {
                object[] objArray1 = new object[] { "Error LOGIN Auth-Nonce: ", result.StatusCode, ", ", result.ReasonPhrase };
                objLog.write(string.Concat(objArray1));
            }
            else
            {
                // objLog.write("Voy a procesar el Auth-Nonce ");
                IEnumerable<string> enumerable;
                string str3 = "";

                if (result.Headers.TryGetValues("Auth-Nonce", out enumerable))
                {
                    str3 = enumerable.First<string>();
                }
                string str4 = "";
                if (str3 != "")
                {
                    str4 = ((decimal.Truncate(Convert.ToUInt64(str3) / ((long)13)) % 99999M) * 17M).ToString();
                }
                client.Dispose();
                if ((str3 != "") && (str4 != ""))
                {
                    HttpClient client2 = new HttpClient
                    {
                        BaseAddress = new Uri(this.ParamValues.PRISMURLForReferencia + "/v1/rest/auth"),
                        DefaultRequestHeaders = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } }
                    };
                    client2.DefaultRequestHeaders.Add("Auth-Nonce", str3);
                    client2.DefaultRequestHeaders.Add("Auth-Nonce-Response", str4);
                    HttpResponseMessage message2 = client2.GetAsync("?usr=" + this.ParamValues.PRISMUserForReferencia + "&pwd=" + this.ParamValues.PRISMPwdForReferencia).Result;
                    objLog.write(this.ParamValues.PRISMURL + "/v1/rest/auth" + "?usr=" + this.ParamValues.PRISMUserForReferencia + "&pwd=" + this.ParamValues.PRISMPwdForReferencia);
                    objLog.write("estatus llamado " + message2.IsSuccessStatusCode);
                    if (!message2.IsSuccessStatusCode)
                    {
                        objLog.write("Error LOGIN buscando Token: " + message2.StatusCode.ToString() + ", " + message2.ReasonPhrase);
                    }
                    else
                    {
                        IEnumerable<string> enumerable2;
                        if (message2.Headers.TryGetValues("Auth-Session", out enumerable2))
                        {
                            // objLog.write(str);
                            str = enumerable2.First<string>();

                        }
                        client2.Dispose();

                        var client3 = new RestClient(this.ParamValues.PRISMURLForReferencia + "/v1/rest/sit?ws=webclient");
                        var request3 = new RestRequest(Method.GET);
                        request3.AddHeader("Auth-Session", str);
                        request3.AddHeader("Accept", "application/json, version=2");
                        request3.AddHeader("Content-Type", "application/json; charset=utf-8");
                        IRestResponse response3 = client3.Execute(request3);

                        if (response3.StatusCode != HttpStatusCode.OK)
                        {
                            objLog.write("StatusCode  1= " + response3.StatusCode);
                        }
                        else
                        {
                            objLog.write("StatusCode 2= " + response3.StatusCode);

                        }

                    }
                }
            }
            // objLog.write("token " + str);
            return str;
        }

        public stFEBOSParam getCustomization(Log objLog, string Token)
        {
            stFEBOSParam param = this.initFEBOSParam();
            try
            {
                HttpClient client = new HttpClient
                {
                    BaseAddress = new Uri(this.ParamValues.PRISMURL + "/v1/rest/customization"),
                    DefaultRequestHeaders = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } }
                };
                client.DefaultRequestHeaders.Add("Auth-Session", Token);
                string str3 = string.Empty;
                HttpResponseMessage result = client.GetAsync("?cols=configuration&filter=name,eq," + this.ParamValues.PRISMProxyLinkName).Result;
                if (!result.IsSuccessStatusCode)
                {
                    object[] objArray1 = new object[] { "Error Customization: ", result.StatusCode, ", ", result.ReasonPhrase };
                    objLog.writeFEBOS(string.Concat(objArray1));
                }
                else
                {
                    str3 = result.Content.ReadAsStringAsync().Result;
                    if (str3 != "")
                    {
                        string str4 = "";
                        try
                        {
                            char[] separator = new char[] { ',' };
                            str3 = str3.Replace("[", "").Replace("]", "").Split(separator)[1].Replace("\"configuration\":", "");
                            str4 = "URLLogin";
                            char[] chArray2 = new char[] { ';' };
                            char[] chArray3 = new char[] { '=' };
                            param.strFEBOSURLLogin = str3.Split(chArray2)[0].Split(chArray3)[1].Trim();
                            str4 = "LoginMail";
                            char[] chArray4 = new char[] { ';' };
                            char[] chArray5 = new char[] { '=' };
                            param.strFEBOSLoginMail = str3.Split(chArray4)[1].Split(chArray5)[1].Trim();
                            str4 = "strFEBOSLoginPass";
                            char[] chArray6 = new char[] { ';' };
                            char[] chArray7 = new char[] { '=' };
                            param.strFEBOSLoginPass = str3.Split(chArray6)[2].Split(chArray7)[1].Trim();
                            str4 = "strFEBOSURLUploadDTE";
                            char[] chArray8 = new char[] { ';' };
                            char[] chArray9 = new char[] { '=' };
                            param.strFEBOSURLUploadDTE = str3.Split(chArray8)[3].Split(chArray9)[1].Trim();
                            str4 = "strRutEmisor";
                            char[] chArray10 = new char[] { ';' };
                            char[] chArray11 = new char[] { '=' };
                            param.strRutEmisor = str3.Split(chArray10)[4].Split(chArray11)[1].Trim();
                            str4 = "strRazonSocial";
                            char[] chArray12 = new char[] { ';' };
                            char[] chArray13 = new char[] { '=' };
                            param.strRazonSocial = str3.Split(chArray12)[5].Split(chArray13)[1].Trim();
                            str4 = "strGiroEmisor";
                            char[] chArray14 = new char[] { ';' };
                            char[] chArray15 = new char[] { '=' };
                            param.strGiroEmisor = str3.Split(chArray14)[6].Split(chArray15)[1].Trim();
                            str4 = "strDireccion";
                            char[] chArray16 = new char[] { ';' };
                            char[] chArray17 = new char[] { '=' };
                            param.strDireccion = str3.Split(chArray16)[7].Split(chArray17)[1].Trim();
                            str4 = "strComuna";
                            char[] chArray18 = new char[] { ';' };
                            char[] chArray19 = new char[] { '=' };
                            param.strComuna = str3.Split(chArray18)[8].Split(chArray19)[1].Trim();
                            str4 = "strCiudad";
                            char[] chArray20 = new char[] { ';' };
                            char[] chArray21 = new char[] { '=' };
                            param.strCiudad = str3.Split(chArray20)[9].Split(chArray21)[1].Trim();
                            str4 = "strCodSucursal";
                            char[] chArray22 = new char[] { ';' };
                            char[] chArray23 = new char[] { '=' };
                            param.strCodSucursal = str3.Split(chArray22)[10].Split(chArray23)[1].Trim();
                        }
                        catch (Exception exception)
                        {
                            objLog.writeFEBOS("Error leyendo parametros Customization (" + str4 + "): " + exception.Message);
                        }
                    }
                }
                client.Dispose();
            }
            catch (Exception exception2)
            {
                param.intCode = 0x63;
                param.strMessage = "Error: " + exception2.Message;
            }
            return param;
        }

        public stFEBOSParam initFEBOSParam()
        {
            stFEBOSParam param;
            param.intCode = 0;
            param.strMessage = "";
            param.strFEBOSURLLogin = "";
            param.strFEBOSURLUploadDTE = "";
            param.strFEBOSLoginMail = "";
            param.strFEBOSLoginPass = "";
            param.strRutEmisor = "";
            param.strRazonSocial = "";
            param.strGiroEmisor = "";
            param.strDireccion = "";
            param.strComuna = "";
            param.strCiudad = "";
            param.strCodSucursal = "";
            param.strFEBOSToken = "";
            param.strFEBOSUI = "";
            param.strFEBOSID = "";
            param.strFolio = "";
            param.strTED = "";
            return param;
        }

        private bool op_isProduction =>
          (ConfigurationManager.AppSettings["OPambiente"] == "P");

        [StructLayout(LayoutKind.Sequential)]

        public struct stFEBOSParam
        {
            public int intCode;
            public string strMessage;
            public string strFEBOSURLLogin;
            public string strFEBOSURLUploadDTE;
            public string strFEBOSLoginMail;
            public string strFEBOSLoginPass;
            public string strRutEmisor;
            public string strRazonSocial;
            public string strGiroEmisor;
            public string strDireccion;
            public string strComuna;
            public string strCiudad;
            public string strCodSucursal;
            public string strFEBOSToken;
            public string strFEBOSUI;
            public string strFEBOSID;
            public string strFolio;
            public string strTED;
        }
        #endregion

        #region Proceso DTE VyV
        public void DTEValidaciones(string valor, string tipo, int num, int valida)
        {
            string msg = "OK";
            if (valida == 1)
            {
                if (valor.Trim() == "" || valor.Trim() == null)
                {
                    msg = tipo + " no puede ser vacio o nulo ";
                }
                else
                {
                    int largo = valor.Length;
                    if (largo > num)
                    {
                        msg = tipo + " no puede exceder los " + num + " caracteres";
                    }
                }
            }
            else if (valida == 2)
            {
                int largo = valor.Length;
                if (largo > num)
                {
                    msg = tipo + " no puede exceder los " + num + " caracteres";
                }
            }
            else if (valida == 3)
            {
                if (valor.Trim() == "" || valor.Trim() == "0")
                {
                    msg = tipo + " no puede ser vacio o 0 ";
                }
                else
                {
                    int largo = valor.Length;
                    if (largo > num)
                    {
                        msg = tipo + " no puede exceder los " + num + " caracteres";
                    }
                }
            }
            this.objLog.writeDTEVyV("mensaje metodo DTEValidaciones de tipo : " + tipo + " mensaje: " + msg);
            if (msg != "OK")
            {
                throw new ApplicationException(msg);
            }
        }
        public string DTESolicitaBoleta(string xml, ref string status, ref string msg, ref string xmlres)
        {
            string boleta = string.Empty;
            DteLocalVyV.DTELocal ws = new DteLocalVyV.DTELocal();
            DteLocalVyV.ProcesarTXTBoleta listaBoleta = ws.Carga_TXTBoleta(xml, "XML");
            status = listaBoleta.Estatus.ToString();
            msg = listaBoleta.MsgEstatus.ToString();
            xmlres = listaBoleta.XML.ToString();
            if (listaBoleta.PDF != null)
            {
                byte[] ListaBoleta = listaBoleta.PDF;
                boleta = System.Convert.ToBase64String(ListaBoleta);
            }
            else
            {
                boleta = string.Empty;
            }
            return boleta;
        }
        public string DTESolicitaDocElectronico(string xml, ref string status, ref string msg, ref string xmlres)
        {
            string boleta = string.Empty;
            DteLocalVyV.DTELocal ws = new DteLocalVyV.DTELocal();
            DteLocalVyV.ProcesarTXT listaDocElec = ws.Carga_TXTDTE(xml, "XML");
            status = listaDocElec.Estatus.ToString();
            msg = listaDocElec.MsgEstatus.ToString();
            xmlres = listaDocElec.XML.ToString();
            if (listaDocElec.PDF != null)
            {
                byte[] ListaBoleta = listaDocElec.PDF;
                boleta = System.Convert.ToBase64String(ListaBoleta);
            }
            else
            {
                boleta = string.Empty;
            }
            return boleta;
        }
        public string DTESolicitarFolio(string rutEmpresa, string tipoDocto)
        {
            string ListaFolio = "";
            try
            {
                DteLocalVyV.DTELocal ws = new DteLocalVyV.DTELocal();
                DteLocalVyV.SolicitarFolio listafolio = ws.Solicitar_Folio(rutEmpresa, Convert.ToInt32(tipoDocto));
                ListaFolio = listafolio.Folio.ToString();
            }
            catch (Exception exception2)
            {
                objLog.write("Error al ejecutar Solicitar_Folio: " + exception2.Message);
                ListaFolio = "0";
            }
            return ListaFolio;
        }
        public string GETCustomer(string idCustomer, string token)
        {
            string respuesta = string.Empty;
            if (idCustomer != "" && idCustomer != string.Empty)
            {
                try
                {
                    var client = new RestClient(this.ParamValues.PRISMURL + "/api/common/customer/" + idCustomer + "?cols=*");
                    var request = new RestRequest(Method.GET);
                    client.Timeout = -1;
                    request.AddHeader("Auth-Session", token);
                    request.AddHeader("Accept", "application/json, version=2");
                    request.AddHeader("Content-Type", "application/json; charset=utf-8");
                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        object[] objArray1 = new object[] { "Error Document: ", response.StatusCode };
                    }
                    else
                    {
                        respuesta = response.Content;
                        respuesta = ReemplazaCaracter(respuesta);
                    }
                }
                catch (Exception exception2)
                {
                    objLog.write("Error: " + exception2.Message);
                }
            }
            else
            {
                respuesta = "{\"data\":[{\"info1\": \"\",\"info2\": \"\", \"fullname\": \"\",\"notes\": \"\"}]}";
            }
           
            return respuesta;
        }


        public string GETCustomerRest(string idCustomer, string token)
        {
            string respuesta = string.Empty;
            if (idCustomer != "" && idCustomer != string.Empty)
            {
                try
                {
                   objLog.write(this.ParamValues.PRISMURL + "/v1/rest/customer/" + idCustomer + "?cols=*,address.*");
                    var client = new RestClient(this.ParamValues.PRISMURL + "/v1/rest/customer/" + idCustomer + "?cols=*,address.*");
                    var request = new RestRequest(Method.GET);
                    client.Timeout = -1;
                    request.AddHeader("Auth-Session", token);
                    request.AddHeader("Accept", "application/json, version=2");
                    request.AddHeader("Content-Type", "application/json; charset=utf-8");
                    IRestResponse response = client.Execute(request);
                    
                    if (response.StatusCode != HttpStatusCode.OK) 
                    {
                        objLog.write("error");
                        objLog.write(response.Content);
                        object[] objArray1 = new object[] { "Error Document: ", response.StatusCode };
                    }
                    else
                    {
                        objLog.write(response.Content);
                        respuesta = response.Content;
                        respuesta = ReemplazaCaracter(respuesta);
                    }
                }
                catch (Exception exception2)
                {
                    objLog.write("Error: " + exception2.Message);
                }
            }
            else
            {
                respuesta = "[{\"info1\": \"\",\"info2\": \"\", \"fullname\": \"\",\"notes\": \"\",\"first_name\": \"\",\"last_name\": \"\",\"primary_address_line_1\": \"\",\"primary_address_line_2\": \"\",\"primary_address_line_3\": \"\",\"primary_address_line_4\": \"\",\"primary_address_line_5\": \"\",\"primary_address_line_6\": \"\",\"email_address\": \"\"}]";
            }
            objLog.write("respuesta cliente :" + respuesta);
            return respuesta;
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
        public static string EncodeStrToBase64(string valor)
        {
            byte[] myByte = System.Text.Encoding.UTF8.GetBytes(valor);
            string myBase64 = Convert.ToBase64String(myByte);
            return myBase64;
        }
        public string GETReceive(string idTransaccion, string token)
        {
            string respuesta = string.Empty;

            try
            {
                var client = new RestClient(this.ParamValues.PRISMURL + "/api/backoffice/receiving/" + idTransaccion + "?cols=*,recvItem.*");
                var request = new RestRequest(Method.GET);
                client.Timeout = -1;
                request.AddHeader("Auth-Session", token);
                request.AddHeader("Accept", "application/json, version=2");
                IRestResponse response = client.Execute(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    object[] objArray1 = new object[] { "Error Document: ", response.StatusCode };
                }
                else
                {
                    respuesta = response.Content;
                    respuesta = ReemplazaCaracter(respuesta);
                }
            }
            catch (Exception exception2)
            {
                objLog.write("Error: " + exception2.Message);
            }
            return respuesta;
        }
        public string GETTransfer(string idTransaccion, string token)
        {
            string respuesta = string.Empty;

            try
            {
                var client = new RestClient(this.ParamValues.PRISMURL + "/api/backoffice/transferslip/" + idTransaccion + "?cols=*,slipitem.*");
                var request = new RestRequest(Method.GET);
                client.Timeout = -1;
                request.AddHeader("Auth-Session", token);
                request.AddHeader("Accept", "application/json, version=2");
                IRestResponse response = client.Execute(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    object[] objArray1 = new object[] { "Error Document: ", response.StatusCode };
                }
                else
                {
                    respuesta = response.Content;
                    respuesta = ReemplazaCaracter(respuesta);

                }
            }
            catch (Exception exception2)
            {
                objLog.write("Error: " + exception2.Message);
            }
            return respuesta;
        }
        public string GETDocument(string idTransaccion, string token)
        {
            string respuesta = string.Empty;
            objLog.write("llegue a : GETDocument");
            objLog.write("el token es : "+ token);
            try
            {
                objLog.write(this.ParamValues.PRISMURL + "/api/backoffice/document/" + idTransaccion + "?cols=*,docitem.*");
                var client = new RestClient(this.ParamValues.PRISMURL + "/api/backoffice/document/" + idTransaccion + "?cols=*,docitem.*");
                var request = new RestRequest(Method.GET);
                client.Timeout = -1;
                request.AddHeader("Auth-Session", token);
                request.AddHeader("Accept", "application/json, version=2");
                IRestResponse response = client.Execute(request);
                //objLog.write(" respuesta " + response.Content);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    object[] objArray1 = new object[] { "Error Document: ", response.StatusCode };
                }
                else
                {
                    respuesta = response.Content;
                    respuesta = ReemplazaCaracter(respuesta);
                }
            }
            catch (Exception exception2)
            {
                objLog.write("Error: " + exception2.Message);
            }

            objLog.write(" respuesta GETDocument " + respuesta);
            return respuesta;
        }

        public string GETDocumentCentral(string idTransaccion, string token)
        {
            string respuesta = string.Empty;
            objLog.write("llegue a : GETDocumentCentral primero se buscara el documento de manera local");







            objLog.write("el token es : " + token);
            try
            {
                objLog.write(this.ParamValues.PRISMURLForReferencia + "/api/backoffice/document/" + idTransaccion + "?cols=*,docitem.*");
                var client = new RestClient(this.ParamValues.PRISMURLForReferencia + "/api/backoffice/document/" + idTransaccion + "?cols=*,docitem.*");
                var request = new RestRequest(Method.GET);
                client.Timeout = -1;
                request.AddHeader("Auth-Session", token);
                request.AddHeader("Accept", "application/json, version=2");
                IRestResponse response = client.Execute(request);
                //objLog.write(" respuesta " + response.Content);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    object[] objArray1 = new object[] { "Error Document: ", response.StatusCode };
                }
                else
                {
                    respuesta = response.Content;
                    respuesta = ReemplazaCaracter(respuesta);
                }
            }
            catch (Exception exception2)
            {
                objLog.write("Error: " + exception2.Message);
            }

            objLog.write(" respuesta GETDocument " + respuesta);
            return respuesta;
        }

        public string GETStore(string idStore, string token)
        {
            string respuesta = string.Empty;
            if (idStore != "" && idStore != string.Empty)
            {
                try
                {
                    var client = new RestClient(this.ParamValues.PRISMURL + "/api/common/store?cols=*&filter=(storeno,eq,"+ idStore + ")AND(active,eq,true)");
                    var request = new RestRequest(Method.GET);
                    client.Timeout = -1;
                    request.AddHeader("Auth-Session", token);
                    request.AddHeader("Accept", "application/json, version=2");
                    request.AddHeader("Content-Type", "application/json; charset=utf-8");
                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        object[] objArray1 = new object[] { "Error Store: ", response.StatusCode };
                    }
                    else
                    {
                        respuesta = response.Content;
                        respuesta = ReemplazaCaracter(respuesta);
                    }
                }
                catch (Exception exception2)
                {
                    objLog.write("Error: " + exception2.Message);
                }
            }
            else
            {
                respuesta = "{\"data\":[{}]}";
            }
            this.objLog.write("store: "+ idStore);
            this.objLog.write("objeto: " + respuesta);
            return respuesta;
        }

        public string GETDocItem(string url, string token)
        {
            string respuesta = string.Empty;
            if (url != "" && url != string.Empty)
            {
                try
                {
                    var client = new RestClient(url+ "?cols=*");
                    var request = new RestRequest(Method.GET);
                    client.Timeout = -1;
                    request.AddHeader("Auth-Session", token);
                    request.AddHeader("Accept", "application/json, version=2");
                    request.AddHeader("Content-Type", "application/json; charset=utf-8");
                    IRestResponse response = client.Execute(request);

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        object[] objArray1 = new object[] { "Error DocItem: ", response.StatusCode };
                    }
                    else
                    {
                        respuesta = response.Content;
                        respuesta = ReemplazaCaracter(respuesta);
                    }
                }
                catch (Exception exception2)
                {
                    objLog.write("Error: " + exception2.Message);
                }
            }
            else
            {
                respuesta = "{\"data\":[{}]}";
            }

            return respuesta;
        }


        public string LargoCadenaMax(string cadena, int largoMax)
        {
            string resp = string.Empty;
            if (cadena is null)
            {
                resp = "";
            }
            else
            {
                if (cadena.Length > largoMax)
                {
                    resp = cadena.Substring(0, largoMax);
                }
                else
                {
                    resp = cadena;
                }
            }


            return resp;
        }

        public string ScreenErrorMessage(string msgErr, string respuesta)
        {
            return $"Error al emitir documento electronico - {respuesta.Replace("\r\n", " ")}";
        }
        #endregion

    }
}

