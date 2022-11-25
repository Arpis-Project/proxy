using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyVyV.ProxyVyV
{
    public static class FacelePlantillaXml
    {
		//public FacelePlantillaXml()
		//{
		//}
		public static string PlantillaCabBoleta()
		{
			string plantillacab = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>" +
										"<DTE version=\"1.0\">" +
											"<Documento ID=\"#DOCUMENTOID#\">" +
												"<Encabezado>" +
													"<IdDoc>" +
														"<TipoDTE>#TPODTE#</TipoDTE>" +
														"<Folio>0</Folio>" +
														"<FchEmis>#FECEMISION#</FchEmis>" +
														"<IndServicio>#INDSERVICIO#</IndServicio>" +
													"</IdDoc>" +
													"<Emisor>" +
														"<RUTEmisor>#RUTEMISOR#</RUTEmisor>" +
														"<RznSocEmisor>#RZNSOC#</RznSocEmisor>" +
														"<GiroEmisor>#GIRO#</GiroEmisor>" +
														"<CdgSIISucur>#CDGSIISUCUR#</CdgSIISucur>" +
														"<DirOrigen>#DIRORIGEN#</DirOrigen>" +
														"<CmnaOrigen>#CMNAORIGEN#</CmnaOrigen>" +
														"<CiudadOrigen>#CIUDADORIGEN#</CiudadOrigen>" +
													"</Emisor>" +
													"<Receptor>" +
														"<RUTRecep>#RUTRECEPTOR#</RUTRecep>" +
														"<RznSocRecep>GENERICO</RznSocRecep>" +
														"<DirRecep>DIRECCION</DirRecep>" +
														"<CmnaRecep>SANTIAGO</CmnaRecep>" +
														"<CiudadRecep>SANTIAGO</CiudadRecep>" +
													"</Receptor>" +
													"<Totales>" +
														"<MntNeto>#MNTNETO#</MntNeto>" +
														"<MntExe>#MNTEXE#</MntExe>" +
														"<IVA>#IVA#</IVA>" +
														"<MntTotal>#MNTTOTAL#</MntTotal>" +
													"</Totales>" +
												"</Encabezado>" +
												"#DETALLE#" +
												"#DSCGLOBAL#" +
											"</Documento>" +
										"</DTE>";

			return plantillacab;
		}
		public static string PlantillaDetBoleta()
		{
			string plantilladet = "<Detalle>" +
										"<NroLinDet>#NROLINDET#</NroLinDet>" +
										"<NmbItem>#NMBITEM#</NmbItem>" +
										"<QtyItem>#QTYITEM#</QtyItem>" +
										"<PrcItem>#PRCITEM#</PrcItem>" +
										"<MontoItem>#MONTOITEM#</MontoItem>" +
										"#DESCUENTO#";
			return plantilladet;
		}
		public static string PantillaCabFactura()
        {
			string plantillacab = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>" +
								  "<DTE>" +
								  "<Documento>" +
								  "<Encabezado>" +
									  "<IdDoc>" +
										"<TipoDTE>#TPODTE#</TipoDTE>" +
										"<Folio>0</Folio>" +
										"<FchEmis>#FECEMISION#</FchEmis>" +
										//"<TipoFactEsp>0</TipoFactEsp>" +
										//"<FmaPago>#FMAPAGO#</FmaPago>" +
                                        "<TermPagoGlosa>#TERMPAGOGLOSA#</TermPagoGlosa>" +
                                        "<TermPagoDias>#TERMPAGODIAS#</TermPagoDias>" +
										"<FchVenc>#FECVENC#</FchVenc>" +
                                        //"<MedioPago>#MEDIOPAGO#</MedioPago>" +
                                      "</IdDoc>" +
									  "<Emisor>" +
										 "<RUTEmisor>#RUTEMISOR#</RUTEmisor>" +
										 "<RznSoc>#RZNSOC#</RznSoc>" +
										 "<GiroEmis>#GIRO#</GiroEmis>" +
										 "<Acteco>#ACTECO#</Acteco>" +
										 "<DirOrigen>#DIRORIGEN#</DirOrigen>" +
										 "<CmnaOrigen>#CMNAORIGEN#</CmnaOrigen>" +
										 "<CiudadOrigen>#CIUDADORIGEN#</CiudadOrigen>" +
										 "<CdgVendedor>#CDGVENDEDOR#</CdgVendedor>" +
									  "</Emisor>" +
									  "<Receptor>" +
										  "<RUTRecep>#RUTRECEPTOR#</RUTRecep>" +
										  "<CdgIntRecep>#CDGINTRECEPTOR#</CdgIntRecep>" +
										  "<RznSocRecep>#RZNSOCRECEP#</RznSocRecep>" +
										  "<GiroRecep>#GIRORECEP#</GiroRecep>" +
										  "<DirRecep>#DIRRECEP#</DirRecep>" +
										  "<CmnaRecep>#CMNARECEP#</CmnaRecep>" +
										  "<CiudadRecep>#CIUDADRECEP#</CiudadRecep>" +
									  "</Receptor>" +
									  "<Totales>" +
										  "<MntNeto>#MNTNETO#</MntNeto>" +
										  "<MntExe>#MNTEXE#</MntExe>" +
										  "<TasaIVA>#TASAIVA#</TasaIVA>" +
										  "<IVA>#IVA#</IVA>" +
										  "<MntTotal>#MNTTOTAL#</MntTotal>" +
									  "</Totales>" +
								  "</Encabezado>" +
									"#DETALLE#"+
									"#REFERENCIA#"+
								  "</Documento>" +
								"</DTE>";
			return plantillacab;
		}
		public static string PantillaDetFactura()
        {
			string plantilladet = "<Detalle>" +
									"<NroLinDet>#NROLINDET#</NroLinDet>" +
										"<CdgItem>" +
											"<TpoCodigo>#TPOCODIGO#</TpoCodigo>" +
											"<VlrCodigo>#VLRCODIGO#</VlrCodigo>" +
										"</CdgItem>" +
		  								"<NmbItem>#NMBITEM#</NmbItem>" +
										"<QtyItem>#QTYITEM#</QtyItem>" +
										"<PrcItem>#PRCITEM#</PrcItem>" +
										"<MontoItem>#MONTOITEM#</MontoItem>" +
										"#DESCUENTO#"; 
			return plantilladet;
		}
		public static string PlantillaRefFactura()
		{
			string plantillaref = "<Referencia>" +
										"<NroLinRef>#NROLINREF#</NroLinRef>" +
										"<TpoDocRef>#TPODOCREF#</TpoDocRef>" +
										"<FolioRef>#FOLIOREF#</FolioRef>" +
										"<FchRef>#FCHREF#</FchRef>" +
										"<RazonRef>#RAZONREF#</RazonRef>" +
									"</Referencia>";

			return plantillaref;
		}
		public static string PlantillaCabGuiaDespacho()
        {
			string plantillacab = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>" +
								  "<DTE version=\"1.0\">" +
								  "<Documento ID =\"#DOCUMENTOID#\">" +
								  "<Encabezado>" +
									  "<IdDoc>" +
										"<TipoDTE>#TPODTE#</TipoDTE>" +
										"<Folio>0</Folio>" +
										"<FchEmis>#FECEMISION#</FchEmis>" +
										"<TipoDespacho>#TIPODESPACHO#</TipoDespacho>" +
										"<IndTraslado>#INDTRASLADO#</IndTraslado>" +
									  "</IdDoc>" +
									  "<Emisor>" +
										 "<RUTEmisor>#RUTEMISOR#</RUTEmisor>" +
										 "<RznSoc>#RZNSOC#</RznSoc>" +
										 "<GiroEmis>#GIRO#</GiroEmis>" +
										 "<Acteco>#ACTECO#</Acteco>" +
										 "<DirOrigen>#DIRORIGEN#</DirOrigen>" +
										 "<CmnaOrigen>#CMNAORIGEN#</CmnaOrigen>" +
										 "<CiudadOrigen>#CIUDADORIGEN#</CiudadOrigen>" +
									  "</Emisor>" +
									  "<Receptor>" +
										  "<RUTRecep>#RUTRECEPTOR#</RUTRecep>" +
										  "<RznSocRecep>#RZNSOCRECEP#</RznSocRecep>" +
										  "<GiroRecep>#GIRORECEP#</GiroRecep>" +
										  "<DirRecep>#DIRRECEP#</DirRecep>" +
										  "<CmnaRecep>#CMNARECEP#</CmnaRecep>" +
										  "<CiudadRecep>#CIUDADRECEP#</CiudadRecep>" +
									  "</Receptor>" +
									  "<Transporte>" +
										  "<DirDest>#DIRDEST#</DirDest>" +
										  "<CmnaDest>#CMNADEST#</CmnaDest>" +
										  "<CiudadDest>#CIUDADDEST#</CiudadDest>" +
									  "</Transporte>" +
									  "<Totales>" +
										  "<MntNeto>#MNTNETO#</MntNeto>" +
										  "<MntExe>#MNTEXE#</MntExe>" +
										  "<TasaIVA>#TASAIVA#</TasaIVA>" +
										  "<IVA>#IVA#</IVA>" +
										  "<MntTotal>#MNTTOTAL#</MntTotal>" +
									  "</Totales>" +
								  "</Encabezado>" +
									"#DETALLE#" +
								  "</Documento>" +
								"</DTE>";
			return plantillacab;
		}
		public static string PantillaDetGuiaDespacho()
		{
			string plantilladet = "<Detalle>" +
									"<NroLinDet>#NROLINDET#</NroLinDet>" +
										"<CdgItem>" +
											"<TpoCodigo>#TPOCODIGO#</TpoCodigo>" +
											"<VlrCodigo>#VLRCODIGO#</VlrCodigo>" +
										"</CdgItem>" +
		  								"<NmbItem>#NMBITEM#</NmbItem>" +
										"<DscItem>#DESC#</DscItem>" +
										"<QtyItem>#QTYITEM#</QtyItem>" +
										"<PrcItem>#PRCITEM#</PrcItem>" +
										"<MontoItem>#MONTOITEM#</MontoItem>" +
								   "</Detalle>";
			return plantilladet;
		}
		public static string PlantillaCabNotaCredito()
        {
			string plantillacab = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>" +
								  "<DTE version=\"1.0\">" +
								  "<Documento>" +
								  //"<Documento ID =\"#DOCUMENTOID#\">" +
								  "<Encabezado>" +
									  "<IdDoc>" +
										"<TipoDTE>#TPODTE#</TipoDTE>" +
										"<Folio>0</Folio>" +
										"<FchEmis>#FECEMISION#</FchEmis>" +
										"<FmaPago>#FMAPAGO#</FmaPago>" +
										"<TermPagoDias>#TERMPAGO#</TermPagoDias>" +
										"<FchVenc>#FECVENC#</FchVenc>" +
									  "</IdDoc>" +
									  "<Emisor>" +
										 "<RUTEmisor>#RUTEMISOR#</RUTEmisor>" +
										 "<RznSoc>#RZNSOC#</RznSoc>" +
										 "<GiroEmis>#GIRO#</GiroEmis>" +
										 "<Acteco>#ACTECO#</Acteco>" +
										 "<DirOrigen>#DIRORIGEN#</DirOrigen>" +
										 "<CmnaOrigen>#CMNAORIGEN#</CmnaOrigen>" +
										 "<CiudadOrigen>#CIUDADORIGEN#</CiudadOrigen>" +
										 "<CdgVendedor>#CDGVENDEDOR#</CdgVendedor>" +
									  "</Emisor>" +
									  "<Receptor>" +
										  "<RUTRecep>#RUTRECEPTOR#</RUTRecep>" +
										  "<CdgIntRecep>#CDGINTRECEPTOR#</CdgIntRecep>" +
										  "<RznSocRecep>#RZNSOCRECEP#</RznSocRecep>" +
										  "<GiroRecep>#GIRORECEP#</GiroRecep>" +
										  "<DirRecep>#DIRRECEP#</DirRecep>" +
										  "<CmnaRecep>#CMNARECEP#</CmnaRecep>" +
										  "<CiudadRecep>#CIUDADRECEP#</CiudadRecep>" +
									  "</Receptor>" +
									  "<Totales>" +
										  "<MntNeto>#MNTNETO#</MntNeto>" +
										  "<MntExe>#MNTEXE#</MntExe>" +
										  "<TasaIVA>#TASAIVA#</TasaIVA>" +
										  "<IVA>#IVA#</IVA>" +
										  "<MntTotal>#MNTTOTAL#</MntTotal>" +
									  "</Totales>" +
								  "</Encabezado>" +
									"#DETALLE#" +
									"#REFERENCIA#" +
								  "</Documento>" +
								"</DTE>";
			return plantillacab;
		}
		public static string PantillaDetNotaCredito()
		{
			string plantilladet = "<Detalle>" +
									"<NroLinDet>#NROLINDET#</NroLinDet>" +
										"<CdgItem>" +
											"<TpoCodigo>#TPOCODIGO#</TpoCodigo>" +
											"<VlrCodigo>#VLRCODIGO#</VlrCodigo>" +
										"</CdgItem>" +
		  								"<NmbItem>#NMBITEM#</NmbItem>" +
										"<DscItem>#DESC#</DscItem>" +
										"<QtyItem>#QTYITEM#</QtyItem>" +
										"<PrcItem>#PRCITEM#</PrcItem>" +
										"<MontoItem>#MONTOITEM#</MontoItem>" +
										"#DESCUENTO#";
			return plantilladet;
		}
		public static string PlantillaRefNotaCredito()
		{
			string plantillaref = "<Referencia>" +
										"<NroLinRef>#NROLINREF#</NroLinRef>" +
										"<TpoDocRef>#TPODOCREF#</TpoDocRef>" +
										"<FolioRef>#FOLIOREF#</FolioRef>" +
										"<FchRef>#FCHREF#</FchRef>" +
										"<CodRef>#CODREF#</CodRef>" +
									"</Referencia>";
			return plantillaref;
		}
		public static string PlantillaDescuentosGlobales()
		{
			string plantillaref = "<DscRcgGlobal>" +
										"<NroLinDR>#NROLINEA#</NroLinDR>" +
										"<TpoMov>D</TpoMov>" +
										"<GlosaDR>#GLOSA#</GlosaDR>" +
										"<TpoValor>$</TpoValor>" +
										"<ValorDR>#MONTO#</ValorDR>" +
									"</DscRcgGlobal>";
			return plantillaref;
		}
	}
}