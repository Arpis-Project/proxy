using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace SignatureVyV
{

    public class SignatureDTE
    {
        public SignatureDTE(String docType)
        {
            this.Args = new Args();
            this.Args.DocType = docType;
            this.Action = "gendoc";
            this.Args.Operation = new Operation();
            this.Args.Operation.IdDoc = new Iddoc();
            this.Args.Operation.IdDoc.IndServicio = 3;
            this.Args.Operation.Totals = new Totals();
            this.Args.Operation.Detalles = new List<Detalle>();
            this.Args.Operation.Personalizados = new Personalizados();
            this.Args.Operation.Receiver = new Receiver();
            this.Args.Operation.Receiver.RUTRecep = "66666666-6";

            this.Args.Operation.Referencias = new List<Referencia>();
            this.Args.Operation.Sender = new Sender();

        }

        public Args Args { get; set; }
        public string Action { get; set; }

        public override string ToString()
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                CheckAdditionalContent = false
            };

            return JsonConvert.SerializeObject(this, settings);

        }

    }

    public class Args
    {
        public Operation Operation { get; set; }
        public string DocType { get; set; }
    }

    public class Operation
    {
        public Receiver Receiver { get; set; }
        public Sender Sender { get; set; }
        public Iddoc IdDoc { get; set; }
        public Totals Totals { get; set; }
        public List<Detalle> Detalles { get; set; }
        public List<Referencia> Referencias { get; set; }
        public Personalizados Personalizados { get; set; }
        public Transporte Transporte { get; set; }
    }

    public class Transporte
    {
        public string Patente { get; set; }
        public string RUTTrans { get; set; }
        public Chofer Chofer { get; set; }
        public string DirDest { get; set; }
        public string CmnaDest { get; set; }
        public string CiudadDest { get; set; }
        //public Aduana Aduana { get; set; }

    }

    public class Chofer
    {
        public string RUTChofer { get; set; }
        public string NombreChofer { get; set; }
    }

    public class Aduana
    {
        int? CodPtoDesemb { get; set; }
        float? MntSeguro { get; set; }
        float? MntFlete { get; set; }
       // ITipoBulto[] TipoBultos { get; set; }
        int? TotBultos { get; set; }
        int? TotItems { get; set; }
        int? CodUnidPesoNeto { get; set; }
        float? PesoNeto { get; set; }
        int? CodUnidPesoBruto { get; set; }
        float? PesoBruto { get; set; }
        int? CodUnidMedTara { get; set; }
        int? Tara { get; set; }
        string IdAdicPtoDesemb { get; set; }
        int? CodPaisDestin { get; set; }
        string IdAdicPtoEmb { get; set; }
        int? CodPtoEmbarque { get; set; }
        string Operador { get; set; }
        string Booking { get; set; }
        string IdAdicTransp { get; set; }
        string NomCiaTransp { get; set; }
        string RUTCiaTransp { get; set; }
        string NombreTransp { get; set; }
        int? CodViaTransp { get; set; }
        float? TotClauVenta { get; set; }
        int? CodClauVenta { get; set; }
        int? CodModVenta { get; set; }
        int? CodPaisRecep { get; set; }

    }

    public class Referencia
    {
        public int NroLinRef { get; set; }
        public string RazonRef { get; set; }
        public string TpoDocRef { get; set; }
        public int? IndGlobal { get; set; }
        public string FolioRef { get; set; }
        public string RUTOtr { get; set; }
        public string FchRef { get; set; }
        public int? CodRef { get; set; }
        //public string CodRef { get; set; }
        public string CodVndor { get; set; }
        public string CodCaja { get; set; }

    }

    public class Receiver
    {
        public string RUTRecep { get; set; }
        public string CdgIntRecep { get; set; }
        public string RznSocRecep { get; set; }
        //public string Contacto { get; set; }
        public string DirRecep { get; set; }
        public string CmnaRecep { get; set; }
        public string CiudadRecep { get; set; }
        public string GiroRecep { get; set; }
        //public string DirPostal { get; set; }
        //public string CmnaPostal { get; set; }
        //public string CiudadPostal { get; set; }
    }

    public class Sender
    {
    }

    public class Iddoc
    {
        public int IndServicio { get; set; }
        public int? IndMntNeto { get; set; }
        public bool? MntBruto { get; set; }
        public int? IndTraslado { get; set; }
        public DateTime? FchEmis { get; set; }
        public DateTime? PeriodoDesde { get; set; }
        public DateTime? PeriodoHasta { get; set; }
        public DateTime? FchVenc { get; set; }
        public int? FmaPago { get; set; }
        public int? TipoDespacho { get; set; }
        
    }

    public class Totals
    {
        public Totals()
        {
        }

        public int MntNeto { get; set; }
        public int IVA { get; set; }
        public int MntTotal { get; set; }
        public float? TasaIVA { get; set; }
    }

    public class Personalizados
    {
    }

    public class Detalle
    {
        public Detalle()
        {
            CdgItems = new List<Cdgitem>();
        }

        public int NroLinDet { get; set; }
        public string NmbItem { get; set; }
        public string DscItem { get; set; }
        //public int? IndExe { get; set; }
        public string UnmdItem { get; set; }
        public float QtyItem { get; set; }
        public float PrcItem { get; set; }
        public float? DescuentoPct { get; set; }
        public int? DescuentoMonto { get; set; }
        public int MontoItem { get; set; }
        public List<Cdgitem> CdgItems { get; set; }

        //public float? RecargoPct { get; set; }
        //public int? RecargoMonto { get; set; }

    }

    public class Cdgitem
    {
        public Cdgitem(string tpoCodigo, string vlrCodigo)
        {
            TpoCodigo = tpoCodigo ?? throw new ArgumentNullException(nameof(tpoCodigo));
            VlrCodigo = vlrCodigo ?? throw new ArgumentNullException(nameof(vlrCodigo));
        }

        public string TpoCodigo { get; set; }
        public string VlrCodigo { get; set; }
    }

}
