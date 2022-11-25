namespace ProxyVyV
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Ports;
    using System.Management;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;
    using System.Xml;

    public class Sales_Plugin
    {
        public static SerialPort comport = new SerialPort();
        public static bool bolImpresion = true;
        public static bool bolEventNCR = false;
        public static List<string> NCRList = new List<string>();
        public static Queue<byte> recievedData = new Queue<byte>();
        public static string strRespuestaNCR = string.Empty;
        public static string strLogName = "Sales_Plugin";
        public static TLogLevel fLogLevel = TLogLevel.logDEBUG;
        public static string strWorkStation = string.Empty;
        public static string strSubsidiaryNumber = string.Empty;
        public static string strSubsidiaryName = string.Empty;
        public static string strStoreNumber = string.Empty;
        public static string strStoreCode = string.Empty;
        public static string strStoreName = string.Empty;
        public static string strWorkStationNumber = string.Empty;
        public static string strStoreAddress1 = string.Empty;
        public static string strStoreAddress2 = string.Empty;
        public static string strStoreAddress3 = string.Empty;
        public static string strStorePhone1 = string.Empty;
        public static string strGlobStoreCode = string.Empty;
        public static string iniPath;
        public static string strINIFileName = "Sales_Plugin.INI";
        public static string strINITarjetas = "Tarjetas_Credito.INI";
        public static string strINIBancos = "Bancos_Chile.INI";
        public static string strINIDepositos = "Bancos_Depositos.INI";
        public static string strINIFolios = "Correlativos_Documentos.INI";
        public static string strINI_NCR = "Eniac_NCR_FiscalPrinter.INI";
        public static string strStringConexion = string.Empty;
        public static string strImpresora = string.Empty;
        public static string strTemplateFacturaFiscal = string.Empty;
        public static string strTemplateNotaCredito = string.Empty;
        public static string strTemplateBoletaManual = string.Empty;
        public static string strOrsanContrato = string.Empty;
        public static string strOrsanUsuario = string.Empty;
        public static string strPassword = string.Empty;
        public static string strURLOrsan = string.Empty;
        public static string strURL = string.Empty;
        public static string strAsyncAgentDir = string.Empty;
        public static string strMotivosNC = string.Empty;
        public static string strTextoMedioOC = string.Empty;
        public static int intNotificacionFolios = 0;
        public static int intLineasMaximoFacturaFiscal = 0;
        public static int intLineasMaximoNotaCredito = 0;
        public static int intLineasMaximoBoletaManual = 0;
        public static int intIDTipoDocumento = 0;
        public static int intCantidadMaximaCheques = 0;
        public static int intListaPrecioOferta = 0;
        public static int intLineasMaximoBoletaElectronica = 0;
        public static int intLineasMaximoFacturaElectronica = 0;
        public static int intLineasMaximoNotaCreditoElectronica = 0;
        public static string strMontoMaximoCambio = string.Empty;
        public static int intMontoMinimoBoleta = 0;
        public static int intCantidadDiasCambioRUT = 0;
        public static bool bolCambioBoletaFiscal = false;
        public static bool bolCambioFacturaFiscal = false;
        public static bool bolCambioFacturaElectronica = false;
        public static bool bolCambioNotaCredito = false;
        public static bool bolCambioBoletaManual = false;
        public static bool bolCambioBoletaElectronica = false;
        public static decimal decMontoEfectivoAcumulado = 0M;
        public static string strSleepTB = "100000";
        public static string strTipoConexionTB = string.Empty;
        public static int intComNumberTB = 1;
        public static int intComVelocityTB;
        public static string strTipoConexionMC = string.Empty;
        public static string strUtilizaMC = string.Empty;
        public static string strImprimirMulticaja = string.Empty;
        public static string strErrorImpresora = string.Empty;
        public static string strNCR_DefaultChar = string.Empty;
        public static string strNCR_Headers = string.Empty;
        public static string strNCR_Footers = string.Empty;
        public static string strNCR_NumeroTerminal = string.Empty;
        public static int intNCR_ComNumber;
        public static int intNCR_TimeOut;
        public static int intNCR_DescId;
        public static int intNCR_DescId2;
        public static int intNCR_SleepReceive;
        public static int intNCR_TipoPagoGaveta;
        public static int intNCR_TipoTicketCambio;
        public static bool bolNCR_PrintCashierId;
        public static bool bolNCR_PrintCashierName;
        public static bool bolNCR_PrintAssociate;
        public static bool bolNCR_PrintWorkStation;
        public static bool bolNCR_PrintUPC;
        public static bool bolNCR_PrintALU;
        public static bool bolNCR_Negrita;
        public static bool bolNCR_Leyenda;
        public static bool bolNCR_Vuelto;
        public static bool bolNCR_Separadores;
        public static bool bolNCR_UtilizaGaveta;
        public static bool bolNCR_NombreCliente;
        public static bool bolNCR_RUTCliente;
        public static bool bolNCR_TelefonoCliente;
        public static bool bolHold = false;
        public static string strRUTUtilizado = string.Empty;
        public static int intContadorAplicar = 1;
        public static string strItemsRC = string.Empty;
        public static bool bolPrimeraEntrada = true;
        public static bool bolCambioItems = false;
        public static bool bolDevolucionItems = false;
        public static bool bolCambioLocal = false;
        public static bool bolCambioTender = false;
        public static bool bolPagoConCheque = false;
        public static int intListaPrecioTienda = 0;
        public static decimal PrecioBase = 0M;
        public static decimal totalItems = 0M;
        public static decimal totalPagos = 0M;
        public static bool bolCertificado = false;
        public static bool bolCertificadoPromocion = false;
        public static List<string> listCertificados = new List<string>();
        public static List<string> listQtyItems = new List<string>();
        public static XmlDocument xmlCertificados = new XmlDocument();
        public static string strTipoCertificadoPromocion = string.Empty;
        public static string strNumeroCertificadoPromocion = string.Empty;
        public static decimal decMontoCertificadoPromocion = 0M;
        public static string strDatosCertificadoPromocionSAP = string.Empty;
        public static string strTipoAplicacionPromocion = string.Empty;
        public static bool bolTenderCOD = false;
        public static string strTipoSO = string.Empty;
        public static string strRUTSO = string.Empty;
        public static string strImpresoraSO = string.Empty;
        public static bool boolCustomerDefaultSO = false;
        public static int intCopiasSO = 0;
        public static int intCopiasDeleteSO = 0;
        public static string strMontoDespacho = string.Empty;
        public static string strCashierSO = string.Empty;
        public static string strURLMagento = string.Empty;
        public static string strUsuarioMagento = string.Empty;
        public static string strPasswordMagento = string.Empty;
        public static int intMinutosExpiracion = 0;
        public static int intMinutosCancelacion = 0;
        public static string strUsuarioHTTPBasic = string.Empty;
        public static string strPasswordHTTPBasic = string.Empty;
        public static int intLineasMaximaRC = 0;
        public static int intCantidadMaximaRC = 0;
        public static bool bolEfectivoRC = false;
        public static bool bolChequesRC = false;
        public static bool bolCODRC = false;
        public static bool bolTRRC = false;
        public static bool bolCertificadoPromocionRC = false;
        public static bool bolGiftCardRC = false;
        public static bool bolTDBRC = false;
        public static bool bolTDCRC = false;
        public static string strServidorSMTPSO = string.Empty;
        public static string strPuertoSMTPSO = string.Empty;
        public static string strUsuarioSMTPSO = string.Empty;
        public static string strPasswordSMTPSO = string.Empty;
        public static string strCredencialesSMTPSO = string.Empty;
        public static string strSSLSMTPSO = string.Empty;
        public static string strEmailTOSO = string.Empty;
        public static string strEmailFromSO = string.Empty;
        public static bool bolUpdatePkg_NO = false;
        public static List<string> lstItems = new List<string>();
        public const char ACK = '\x0006';
        public const char NAK = '\x0015';
        public const char STX = '\x0002';
        public const char ETX = '\x0003';
        public const char separador = '|';
        private static object LOCK = new object();
        private static string cachedPath = "";
        private const string Const_Workstations_literal = @"\Workstations\";
        private static string cachedName = "";
        private const string Const_Replace_Chars_Name = @".-/\ ";
        private const string Const_Domain_String = "domain";
        private const string Const_Name_String = "name";
        private const string Const_Underscore = "_";
        private const string Const_Unknown = "unknown";
        private const string Const_WMI_Query_For_Name = "SELECT * FROM Win32_ComputerSystem";

        public static string PRISMWorkstationPath
        {
            get
            {
                object lOCK = LOCK;
                lock (lOCK)
                {
                    if (cachedPath.Equals(""))
                    {
                        string path = Environment.CurrentDirectory + @"\Workstations\";
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        cachedPath = path;
                    }
                }
                return cachedPath;
            }
        }

        public static string PRISMWorkstationName
        {
            get
            {
                try
                {
                    object lOCK = LOCK;
                    lock (lOCK)
                    {
                        if (cachedName.Equals(""))
                        {
                            string str = @".-/\ ";
                            string str2 = "";
                            string str3 = "";
                            StringBuilder builder = new StringBuilder();
                            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(new SelectQuery("SELECT * FROM Win32_ComputerSystem")))
                            {
                                foreach (ManagementObject obj3 in searcher.Get())
                                {
                                    str2 = obj3["domain"].ToString();
                                    str3 = obj3["name"].ToString();
                                }
                                builder.Append(str2 + "_" + str3);
                                foreach (char ch in str)
                                {
                                    builder.Replace(ch.ToString(), "_");
                                }
                            }
                            cachedName = builder.ToString();
                        }
                    }
                }
                catch
                {
                    try
                    {
                        return SystemInformation.UserDomainName;
                    }
                    catch
                    {
                        return "unknown";
                    }
                }
                return cachedName;
            }
        }

        public static TBResponse JsonResponse { get; set; }

        [StructLayout(LayoutKind.Sequential)]
        public struct TCustomerData
        {
            public string strCustomerSID;
            public string strCustomerID;
            public string strRUT;
            public string strNombre;
            public string strApellido;
            public string strTelefono;
            public string strDireccion;
            public string strEmail;
            public string strCiudad;
            public string strComuna;
            public string strGiro;
        }
    }
}

