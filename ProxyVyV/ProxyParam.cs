namespace ProxyVyV
{
    using System;
    using System.Configuration;

    public class ProxyParam
    {
        public string PRISMServerIP = "";
        public string PRISMProxyPort = "";
        public string PRISMProxyLinkName = "";
        public string PRISMProxyVersion = "";
        public string PRISMProxyDeveloperID = "";
        public string PRISMProxyCustomizationID = "";
        public string PRISMURL = "";
        public string PRISMUser = "";
        public string PRISMPwd = "";
        public string ModoDebugCompleto = "";
        public string DTEVyV_RutEmisor = "";
        public string DTEVyV_RznSocEmi = "";
        public string DTEVyV_FechaResol = "";
        public string DTEVyV_ACTECO = "";
        public string DTEVyV_CDGSIISUCUR = "";
        public string DTEVyV_RutReceptordefault = "";
        public string DTEVyV_NumResol = "";
        public string DTEVyV_DirOrigen = "";
        public string DTEVyV_CmnaOrigen = "";
        public string DTEVyV_CiudadOrigen = "";
        public string DTEVyV_Giro = "";
        public string DTEVyV_Telefono = "";
        public string DTEVyV_Tienda = "";
        public string DTEVyV_Telefono_tienda = "";
        public string DTEVyV_Direccion_tienda = "";
        public string DTEVyV_NombreClientedefault = "";
        public string DTEVyV_GiroClientedefault = "";
        public string DTEVyV_TasaIVA = "";
        public string DTEVyV_Impresora = "";
        public string DTEVyV_SII = "";
        public string strPINPADBaudRate;
        public string strPINPADCOMPorNumber;
        public string strPINPADTimeOut;
        public string PRISMURLForReferencia = "";
        public string PRISMUserForReferencia = "";
        public string PRISMPwdForReferencia = "";

        // Signature DTE
        public string DTESignature_Server = "";
        public int DTESignature_Port = 0;
        public string DTESignature_NombreClientedefault = "";
        public string DTESignature_GiroClientedefault = "";
        public string DTESignature_TasaIVA = "";
        public string DTESignature_RutReceptordefault = "";
        public string DTESignature_PreciosConIva = "";
        // DBNET DTE
        public string DTEDbNet_StoreIdPrefijo = "";
        public string DTEDbNet_PosIdPrefijo = "";
        public string DTEDbNet_StoreURL = "";

        public string DTEDbNet_RutEmisor = "";
        public string DTEDbNet_RznSocEmi = "";
        public string DTEDbNet_ACTECO = "";
        public string DTEDbNet_RutReceptordefault = "";
        public string DTEDbNet_Direccion = "";
        public string DTEDbNet_Comuna = "";
        public string DTEDbNet_Ciudad = "";
        public string DTEDbNet_Giro = "";
        public string DTEDbNet_Telefono = "";
        public string DTEDbNet_NombreClientedefault = "";
        public string DTEDbNet_GiroClientedefault = "";
        public string DTEDbNet_TasaIVA = "";

        public string DTEFacCL_WS_USUARIO = "";
        public string DTEFacCL_WS_CLAVE = "";
        public string DTEFacCL_RutEmisor = "";
        public string DTEFacCL_RznSocEmi = "";
        public string DTEFacCL_Giro = "";
        public string DTEFacCL_Direccion = "";
        public string DTEFacCL_Comuna = "";
        public string DTEFacCL_Ciudad = "";
        public string DTEFacCL_RutReceptordefault = "";
        public string DTEFacCL_NombreClientedefault = "";
        public string DTEFacCL_GiroClientedefault = "";
        public string DTEFacCL_TasaIVA = "";

        // Get One DTE
        public string DTEGetOne_Domain = "";
        public string DTEGetOne_RutReceptordefault = "";
        public string DTEGetOne_NombreClientedefault = "";
        public string DTEGetOne_GiroClientedefault = "";
        public string DTEGetOne_TasaIVA = "";

        // Facele DTE
        public string DTEFacele_WSurl = "";
        public string DTEFacele_RutReceptordefault = "";
        public string DTEFacele_FechaResol = "";
        public string DTEFacele_NumResol = "";
        public string DTEFacele_NombreClientedefault = "";
        public string DTEFacele_ACTECO = "";
        public string DTEFacele_GiroClientedefault = "";
        public string DTEFacele_TasaIVA = "";



        public void LoadConfigurationSystem()
        {
            try
            {
                this.PRISMServerIP = ConfigurationManager.AppSettings["PRISMServerIP"];
                this.PRISMProxyPort = ConfigurationManager.AppSettings["PRISMProxyPort"];
                this.PRISMProxyLinkName = ConfigurationManager.AppSettings["PRISMProxyLinkName"];
                this.PRISMProxyVersion = ConfigurationManager.AppSettings["PRISMProxyVersion"];
                this.PRISMProxyDeveloperID = ConfigurationManager.AppSettings["PRISMProxyDeveloperID"];
                this.PRISMProxyCustomizationID = ConfigurationManager.AppSettings["PRISMProxyCustomizationID"];
                this.PRISMURL = ConfigurationManager.AppSettings["PRISMURL"];
                this.PRISMUser = ConfigurationManager.AppSettings["PRISMUser"];
                this.PRISMPwd = ConfigurationManager.AppSettings["PRISMPwd"];
                this.ModoDebugCompleto = ConfigurationManager.AppSettings["ModoDebugCompleto"];
                this.DTEVyV_RutEmisor = ConfigurationManager.AppSettings["DTEVyV_RutEmisor"];
                this.DTEVyV_RznSocEmi = ConfigurationManager.AppSettings["DTEVyV_RznSocEmi"];
                this.DTEVyV_FechaResol = ConfigurationManager.AppSettings["DTEVyV_FechaResol"];
                this.DTEVyV_ACTECO = ConfigurationManager.AppSettings["DTEVyV_ACTECO"];
                this.DTEVyV_CDGSIISUCUR = ConfigurationManager.AppSettings["DTEVyV_CDGSIISUCUR"];
                this.DTEVyV_RutReceptordefault = ConfigurationManager.AppSettings["DTEVyV_RutReceptordefault"];
                this.DTEVyV_NumResol = ConfigurationManager.AppSettings["DTEVyV_NumResol"];
                this.DTEVyV_DirOrigen = ConfigurationManager.AppSettings["DTEVyV_DirOrigen"];
                this.DTEVyV_CmnaOrigen = ConfigurationManager.AppSettings["DTEVyV_CmnaOrigen"];
                this.DTEVyV_CiudadOrigen = ConfigurationManager.AppSettings["DTEVyV_CiudadOrigen"];
                this.DTEVyV_Giro = ConfigurationManager.AppSettings["DTEVyV_Giro"];
                this.DTEVyV_Telefono = ConfigurationManager.AppSettings["DTEVyV_Telefono"];
                this.DTEVyV_Tienda = ConfigurationManager.AppSettings["DTEVyV_Tienda"];
                this.DTEVyV_Telefono_tienda = ConfigurationManager.AppSettings["DTEVyV_Telefono_tienda"];
                this.DTEVyV_Direccion_tienda = ConfigurationManager.AppSettings["DTEVyV_Direccion_tienda"];
                this.DTEVyV_NombreClientedefault = ConfigurationManager.AppSettings["DTEVyV_NombreClientedefault"];
                this.DTEVyV_GiroClientedefault = ConfigurationManager.AppSettings["DTEVyV_GiroClientedefault"];
                this.DTEVyV_TasaIVA = ConfigurationManager.AppSettings["DTEVyV_TasaIVA"];
                this.DTEVyV_Impresora = ConfigurationManager.AppSettings["DTEVyV_Impresora"];
                this.DTEVyV_SII = ConfigurationManager.AppSettings["DTEVyV_SII"];
                this.PRISMURLForReferencia = ConfigurationManager.AppSettings["PRISMURLForReferencia"];
                this.PRISMUserForReferencia = ConfigurationManager.AppSettings["PRISMUserForReferencia"];
                this.PRISMPwdForReferencia = ConfigurationManager.AppSettings["PRISMPwdForReferencia"];

                //SIGNATURE
                this.DTESignature_Server = ConfigurationManager.AppSettings["DTESignature_Server"];
                this.DTESignature_Port = Convert.ToInt32(ConfigurationManager.AppSettings["DTESignature_Port"]);
                this.DTESignature_RutReceptordefault = ConfigurationManager.AppSettings["DTESignature_RutReceptordefault"];
                this.DTESignature_GiroClientedefault = ConfigurationManager.AppSettings["DTESignature_GiroClientedefault"];
                this.DTESignature_NombreClientedefault = ConfigurationManager.AppSettings["DTESignature_NombreClientedefault"];
                this.DTESignature_TasaIVA = ConfigurationManager.AppSettings["DTESignature_TasaIVA"];
                this.DTESignature_PreciosConIva = ConfigurationManager.AppSettings["DTESignature_PreciosConIva"];
                //DBNET

                this.DTEDbNet_StoreIdPrefijo = ConfigurationManager.AppSettings["DTEDbNet_StoreIdPrefijo"];
                this.DTEDbNet_PosIdPrefijo = ConfigurationManager.AppSettings["DTEDbNet_PosIdPrefijo"];
                this.DTEDbNet_StoreURL = ConfigurationManager.AppSettings["DTEDbNet_StoreURL"];


                this.DTEDbNet_RutEmisor = ConfigurationManager.AppSettings["DTEDbNet_RutEmisor"];
                this.DTEDbNet_RznSocEmi = ConfigurationManager.AppSettings["DTEDbNet_RznSocEmi"];
                this.DTEDbNet_ACTECO = ConfigurationManager.AppSettings["DTEDbNet_ACTECO"];
                this.DTEDbNet_RutReceptordefault = ConfigurationManager.AppSettings["DTEDbNet_RutReceptordefault"];
                this.DTEDbNet_Direccion = ConfigurationManager.AppSettings["DTEDbNet_Direccion"];
                this.DTEDbNet_Comuna = ConfigurationManager.AppSettings["DTEDbNet_Comuna"];
                this.DTEDbNet_Ciudad = ConfigurationManager.AppSettings["DTEDbNet_Ciudad"];
                this.DTEDbNet_Giro = ConfigurationManager.AppSettings["DTEDbNet_Giro"];
                this.DTEDbNet_Telefono = ConfigurationManager.AppSettings["DTEDbNet_Telefono"];
                this.DTEDbNet_NombreClientedefault = ConfigurationManager.AppSettings["DTEDbNet_NombreClientedefault"];
                this.DTEDbNet_GiroClientedefault = ConfigurationManager.AppSettings["DTEDbNet_GiroClientedefault"];
                this.DTEDbNet_TasaIVA = ConfigurationManager.AppSettings["DTEDbNet_TasaIVA"];

                this.DTEFacCL_RutEmisor = ConfigurationManager.AppSettings["DTEFacCL_RutEmisor"];
                this.DTEFacCL_RznSocEmi = ConfigurationManager.AppSettings["DTEFacCL_RznSocEmi"];
                this.DTEFacCL_Giro = ConfigurationManager.AppSettings["DTEFacCL_Giro"];
                this.DTEFacCL_Direccion = ConfigurationManager.AppSettings["DTEFacCL_Direccion"];
                this.DTEFacCL_Comuna = ConfigurationManager.AppSettings["DTEFacCL_Comuna"];
                this.DTEFacCL_Ciudad = ConfigurationManager.AppSettings["DTEFacCL_Ciudad"];
                this.DTEFacCL_RutReceptordefault = ConfigurationManager.AppSettings["DTEFacCL_RutReceptordefault"];
                this.DTEFacCL_NombreClientedefault = ConfigurationManager.AppSettings["DTEFacCL_NombreClientedefault"];
                this.DTEFacCL_GiroClientedefault = ConfigurationManager.AppSettings["DTEFacCL_GiroClientedefault"];
                this.DTEFacCL_TasaIVA = ConfigurationManager.AppSettings["DTEFacCL_TasaIVA"];
                this.DTEFacCL_WS_USUARIO = ConfigurationManager.AppSettings["DTEFacCL_WS_USUARIO"];
                this.DTEFacCL_WS_CLAVE = ConfigurationManager.AppSettings["DTEFacCL_WS_CLAVE"];


                this.DTEGetOne_Domain = ConfigurationManager.AppSettings["DTEGetOne_Domain"];
                this.DTEGetOne_RutReceptordefault = ConfigurationManager.AppSettings["DTEGetOne_RutReceptorDefault"];
                this.DTEGetOne_NombreClientedefault = ConfigurationManager.AppSettings["DTEGetOne_NombreClienteDefault"];
                this.DTEGetOne_GiroClientedefault = ConfigurationManager.AppSettings["DTEGetOne_GiroClienteDefault"];
                this.DTEGetOne_TasaIVA = ConfigurationManager.AppSettings["DTEGetOne_TasaIVA"];

                //FACELE
                this.DTEFacele_WSurl = ConfigurationManager.AppSettings["DTEFacele_WSurl"];
                this.DTEFacele_RutReceptordefault = ConfigurationManager.AppSettings["DTEFacele_RutReceptordefault"];
                this.DTEFacele_FechaResol = ConfigurationManager.AppSettings["DTEFacele_FechaResol"];
                this.DTEFacele_NumResol = ConfigurationManager.AppSettings["DTEFacele_NumResol"];
                this.DTEFacele_NombreClientedefault = ConfigurationManager.AppSettings["DTEFacele_NombreClientedefault"];
                this.DTEFacele_ACTECO = ConfigurationManager.AppSettings["DTEFacele_ACTECO"];
                this.DTEFacele_GiroClientedefault = ConfigurationManager.AppSettings["DTEFacele_GiroClientedefault"];
                this.DTEFacele_TasaIVA = ConfigurationManager.AppSettings["DTEFacele_TasaIVA"];

            }
            catch (Exception)
            {
            }
        }

        public bool isValid =>
            ((this.PRISMServerIP != "") && (this.PRISMProxyPort != ""));
    }
}

