using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ProxyVyV.ProxyVyV.DTEFacCL
{


    // NOTA: El código generado puede requerir, como mínimo, .NET Framework 4.5 o .NET Core/Standard 2.0.
    /// <remarks/>


    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, IncludeInSchema = false, Namespace = "", TypeName = "")]
    [System.Xml.Serialization.XmlRootAttribute(DataType = "", ElementName = "DTE", IsNullable = true, Namespace = "")]

    public partial class FacCLDTEXML
    {

        private DTEDocumento documentoField;

        private decimal versionField;

        /// <remarks/>
        public DTEDocumento Documento
        {
            get
            {
                return this.documentoField;
            }
            set
            {
                this.documentoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }

        public string ToXML()
        {
            string xml = "";
            var serializer = new XmlSerializer(this.GetType());
            var encoding = Encoding.GetEncoding("ISO-8859-1");
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = false,
                Encoding = encoding,
            };

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            using (var stream = new MemoryStream())
            {
                using (var xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
                {
                    serializer.Serialize(xmlWriter, this, ns);
                }
                xml = encoding.GetString(stream.ToArray());
                return xml;
            }



        }

        public string GetBase64(string txt, out string error)
        {
            string base64 = "";
            error = "";

            try
            {
                if (string.IsNullOrEmpty(txt)) return null;
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(txt);
                return System.Convert.ToBase64String(plainTextBytes);

            }
            catch (Exception e)
            {
                error = e.Message;
            }
            return null;
        }

    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DTEDocumento
    {

        private DTEDocumentoEncabezado encabezadoField;

        private List<DTEDocumentoDetalle> detalleField;

        private List<DTEDocumentoReferencia> referenciaField;

        private string idField;

        /// <remarks/>
        public DTEDocumentoEncabezado Encabezado
        {
            get
            {
                return this.encabezadoField;
            }
            set
            {
                this.encabezadoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Detalle")]
        public List<DTEDocumentoDetalle> Detalle
        {
            get
            {
                return this.detalleField;
            }
            set
            {
                this.detalleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Referencia")]
        public List<DTEDocumentoReferencia> Referencia
        {
            get
            {
                return this.referenciaField;
            }
            set
            {
                this.referenciaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DTEDocumentoEncabezado
    {

        private DTEDocumentoEncabezadoIdDoc idDocField;

        private DTEDocumentoEncabezadoEmisor emisorField;

        private DTEDocumentoEncabezadoReceptor receptorField;

        private DTEDocumentoEncabezadoTotales totalesField;

        /// <remarks/>
        public DTEDocumentoEncabezadoIdDoc IdDoc
        {
            get
            {
                return this.idDocField;
            }
            set
            {
                this.idDocField = value;
            }
        }

        /// <remarks/>
        public DTEDocumentoEncabezadoEmisor Emisor
        {
            get
            {
                return this.emisorField;
            }
            set
            {
                this.emisorField = value;
            }
        }

        /// <remarks/>
        public DTEDocumentoEncabezadoReceptor Receptor
        {
            get
            {
                return this.receptorField;
            }
            set
            {
                this.receptorField = value;
            }
        }

        /// <remarks/>
        public DTEDocumentoEncabezadoTotales Totales
        {
            get
            {
                return this.totalesField;
            }
            set
            {
                this.totalesField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DTEDocumentoEncabezadoIdDoc
    {

        private int tipoDTEField;

        private int folioField;

        private string fchEmisField;

        private string indServicioField;

        private string indMntNetoField;

        private string periodoDesdeField;

        private string periodoHastaField;

        private string fchVencField;

        private string indTrasladoField;

        private string tipoDespachoField;


        public string IndTraslado
        {
            get
            {
                return this.indTrasladoField;
            }
            set
            {
                this.indTrasladoField = value;
            }
        }
        public string TipoDespacho
        {
            get
            {
                return this.tipoDespachoField;
            }
            set
            {
                this.tipoDespachoField = value;
            }
        }


        /// <remarks/>
        public int TipoDTE
        {
            get
            {
                return this.tipoDTEField;
            }
            set
            {
                this.tipoDTEField = value;
            }
        }

        /// <remarks/>
        public int Folio
        {
            get
            {
                return this.folioField;
            }
            set
            {
                this.folioField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public string FchEmis
        {
            get
            {
                return this.fchEmisField;
            }
            set
            {
                this.fchEmisField = value;
            }
        }

        /// <remarks/>
        public string IndServicio
        {
            get
            {
                return this.indServicioField;
            }
            set
            {
                this.indServicioField = value;
            }
        }

        /// <remarks/>
        public string IndMntNeto
        {
            get
            {
                return this.indMntNetoField;
            }
            set
            {
                this.indMntNetoField = value;
            }
        }

        /// <remarks/>
        public string PeriodoDesde
        {
            get
            {
                return this.periodoDesdeField;
            }
            set
            {
                this.periodoDesdeField = value;
            }
        }

        /// <remarks/>
        public string PeriodoHasta
        {
            get
            {
                return this.periodoHastaField;
            }
            set
            {
                this.periodoHastaField = value;
            }
        }

        /// <remarks/>
        public string FchVenc
        {
            get
            {
                return this.fchVencField;
            }
            set
            {
                this.fchVencField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DTEDocumentoEncabezadoEmisor
    {

        private string rUTEmisorField;

        private string rznSocEmisorField;

        private string giroEmisorField;

        private string dirOrigenField;

        private string cmnaOrigenField;

        private string ciudadOrigenField;

        /// <remarks/>
        public string RUTEmisor
        {
            get
            {
                return this.rUTEmisorField;
            }
            set
            {
                this.rUTEmisorField = value;
            }
        }

        /// <remarks/>
        public string RznSocEmisor
        {
            get
            {
                return this.rznSocEmisorField;
            }
            set
            {
                this.rznSocEmisorField = value;
            }
        }

        /// <remarks/>
        public string GiroEmisor
        {
            get
            {
                return this.giroEmisorField;
            }
            set
            {
                this.giroEmisorField = value;
            }
        }

        /// <remarks/>
        public string DirOrigen
        {
            get
            {
                return this.dirOrigenField;
            }
            set
            {
                this.dirOrigenField = value;
            }
        }

        /// <remarks/>
        public string CmnaOrigen
        {
            get
            {
                return this.cmnaOrigenField;
            }
            set
            {
                this.cmnaOrigenField = value;
            }
        }

        /// <remarks/>
        public string CiudadOrigen
        {
            get
            {
                return this.ciudadOrigenField;
            }
            set
            {
                this.ciudadOrigenField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DTEDocumentoEncabezadoReceptor
    {

        private string rUTRecepField;

        private string cdgIntRecepField;

        private string rznSocRecepField;

        private string contactoField;

        private string dirRecepField;

        private string cmnaRecepField;

        private string ciudadRecepField;

        private string giroRecepField;

        /// <remarks/>
        public string GiroRecep
        {
            get
            {
                return this.giroRecepField;
            }
            set
            {
                this.giroRecepField = value;
            }
        }

        /// <remarks/>
        public string RUTRecep
        {
            get
            {
                return this.rUTRecepField;
            }
            set
            {
                this.rUTRecepField = value;
            }
        }

        /// <remarks/>
        public string CdgIntRecep
        {
            get
            {
                return this.cdgIntRecepField;
            }
            set
            {
                this.cdgIntRecepField = value;
            }
        }

        /// <remarks/>
        public string RznSocRecep
        {
            get
            {
                return this.rznSocRecepField;
            }
            set
            {
                this.rznSocRecepField = value;
            }
        }

        /// <remarks/>
        public string Contacto
        {
            get
            {
                return this.contactoField;
            }
            set
            {
                this.contactoField = value;
            }
        }

        /// <remarks/>
        public string DirRecep
        {
            get
            {
                return this.dirRecepField;
            }
            set
            {
                this.dirRecepField = value;
            }
        }

        /// <remarks/>
        public string CmnaRecep
        {
            get
            {
                return this.cmnaRecepField;
            }
            set
            {
                this.cmnaRecepField = value;
            }
        }

        /// <remarks/>
        public string CiudadRecep
        {
            get
            {
                return this.ciudadRecepField;
            }
            set
            {
                this.ciudadRecepField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DTEDocumentoEncabezadoTotales
    {
        private string tasaIVAField;

        private int mntNetoField;

        private int mntExeField;

        private int iVAField;

        private int mntTotalField;

        private int montoNFField;

        //private int totalPeriodoField;

        //private int saldoAnteriorField;

        //private int vlrPagarField;

        /// <remarks/>
        public int MntNeto
        {
            get
            {
                return this.mntNetoField;
            }
            set
            {
                this.mntNetoField = value;
            }
        }


        /// <remarks/>
        public string TasaIVA
        {
            get
            {
                return this.tasaIVAField;
            }
            set
            {
                this.tasaIVAField = value;
            }
        }        /// <remarks/>
        public int MntExe
        {
            get
            {
                return this.mntExeField;
            }
            set
            {
                this.mntExeField = value;
            }
        }

        /// <remarks/>
        public int IVA
        {
            get
            {
                return this.iVAField;
            }
            set
            {
                this.iVAField = value;
            }
        }

        /// <remarks/>
        public int MntTotal
        {
            get
            {
                return this.mntTotalField;
            }
            set
            {
                this.mntTotalField = value;
            }
        }

        /// <remarks/>
        public int MontoNF
        {
            get
            {
                return this.montoNFField;
            }
            set
            {
                this.montoNFField = value;
            }
        }

        ///// <remarks/>
        //public int TotalPeriodo
        //{
        //    get
        //    {
        //        return this.totalPeriodoField;
        //    }
        //    set
        //    {
        //        this.totalPeriodoField = value;
        //    }
        //}

        ///// <remarks/>
        //public int SaldoAnterior
        //{
        //    get
        //    {
        //        return this.saldoAnteriorField;
        //    }
        //    set
        //    {
        //        this.saldoAnteriorField = value;
        //    }
        //}

        ///// <remarks/>
        //public int VlrPagar
        //{
        //    get
        //    {
        //        return this.vlrPagarField;
        //    }
        //    set
        //    {
        //        this.vlrPagarField = value;
        //    }
        //}
    }

    /// <remarks/>
    [System.SerializableAttribute()]

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DTEDocumentoDetalle
    {

        private int nroLinDetField;

        private DTEDocumentoDetalleCdgItem cdgItemField;

        private string nmbItemField;

        private string dscItemField;

        private float qtyItemField;

        private string unmdItemField;

        private float prcItemField;

        private int montoItemField;

        /// <remarks/>
        public int NroLinDet
        {
            get
            {
                return this.nroLinDetField;
            }
            set
            {
                this.nroLinDetField = value;
            }
        }

        /// <remarks/>
        public DTEDocumentoDetalleCdgItem CdgItem
        {
            get
            {
                return this.cdgItemField;
            }
            set
            {
                this.cdgItemField = value;
            }
        }

        /// <remarks/>
        public string NmbItem
        {
            get
            {
                return this.nmbItemField;
            }
            set
            {
                this.nmbItemField = value;
            }
        }

        /// <remarks/>
        public string DscItem
        {
            get
            {
                return this.dscItemField;
            }
            set
            {
                this.dscItemField = value;
            }
        }

        /// <remarks/>
        public float QtyItem
        {
            get
            {
                return this.qtyItemField;
            }
            set
            {
                this.qtyItemField = value;
            }
        }

        /// <remarks/>
        public string UnmdItem
        {
            get
            {
                return this.unmdItemField;
            }
            set
            {
                this.unmdItemField = value;
            }
        }

        /// <remarks/>
        public float PrcItem
        {
            get
            {
                return this.prcItemField;
            }
            set
            {
                this.prcItemField = value;
            }
        }

        /// <remarks/>
        public int MontoItem
        {
            get
            {
                return this.montoItemField;
            }
            set
            {
                this.montoItemField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DTEDocumentoDetalleCdgItem
    {

        private string tpoCodigoField;

        private string vlrCodigoField;

        /// <remarks/>
        public string TpoCodigo
        {
            get
            {
                return this.tpoCodigoField;
            }
            set
            {
                this.tpoCodigoField = value;
            }
        }

        /// <remarks/>
        public string VlrCodigo
        {
            get
            {
                return this.vlrCodigoField;
            }
            set
            {
                this.vlrCodigoField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DTEDocumentoReferencia
    {

        private int nroLinRefField;

        private string tpoDocRefField;

        private string folioRefField;

        private string fchRefField;

        private string codRefField;

        private string razonRefField;

        /// <remarks/>
        public string TpoDocRef
        {
            get
            {
                return this.tpoDocRefField;
            }
            set
            {
                this.tpoDocRefField = value;
            }
        }

        /// <remarks/>
        public string FolioRef
        {
            get
            {
                return this.folioRefField;
            }
            set
            {
                this.folioRefField = value;
            }
        }

        /// <remarks/>
        public string FchRef
        {
            get
            {
                return this.fchRefField;
            }
            set
            {
                this.fchRefField = value;
            }
        }

        /// <remarks/>
        public int NroLinRef
        {
            get
            {
                return this.nroLinRefField;
            }
            set
            {
                this.nroLinRefField = value;
            }
        }

        /// <remarks/>
        public string CodRef
        {
            get
            {
                return this.codRefField;
            }
            set
            {
                this.codRefField = value;
            }
        }

        /// <remarks/>
        public string RazonRef
        {
            get
            {
                return this.razonRefField;
            }
            set
            {
                this.razonRefField = value;
            }
        }
    }




}
