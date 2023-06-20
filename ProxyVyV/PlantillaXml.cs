using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyVyV.ProxyVyV
{
    class PlantillaXml
    {
		public PlantillaXml()
		{
		}
		public string PlantillaCabBoleta()
		{
			string plantillacab = "<EnvioBOLETA version=\"1.0\"  xmlns=\"http://www.sii.cl/SiiDte\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation = \"http://www.sii.cl/SiiDte EnvioBOLETA_v11.xsd\">"+
									 "<SetDTE ID = \"ENVBOL-201903081057482127\">" +
										"<Caratula version = \"1.0\">" +
											"<RutEmisor>#RUTEMISOR#</RutEmisor>" +
											"<RutEnvia>#RUTENVIA#</RutEnvia>" +
											"<RutReceptor>#RUTRECEPTOR#</RutReceptor>" +
											"<FchResol>#FECHARESOL#</FchResol>" +
											"<NroResol>#NUMRESOL#</NroResol>" +
											"<TmstFirmaEnv>#FIRMAEVN#</TmstFirmaEnv>" +
											"<SubTotDTE>" +
												"<TpoDTE>#TPODTE#</TpoDTE>" +
												"<NroDTE>#NRODTE#</NroDTE>" +
											"</SubTotDTE>" +
										"</Caratula>" +
										"<DTE version = \"1.0\">" +
											"<Documento ID = \"#DOCUMENTOID#\">" +
												"<Encabezado>" +
													"<IdDoc>" +
														"<TipoDTE>#TPODTE#</TipoDTE>" +
														"<Folio>#FOLIO#</Folio>" +
														"<FchEmis>#FECEMISION#</FchEmis>" +
														"<IndServicio>#INDSERVICIO#</IndServicio>" +
														"<FchVenc>#FECVENC#</FchVenc>" +
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
														"<CdgIntRecep/>" +
														"<RznSocRecep>#RZNSOCRECEP#</RznSocRecep>" +
														"<Contacto/>" +
														"<DirRecep>#DIRRECEP#</DirRecep>" +
														"<CmnaRecep>#CMNARECEP#</CmnaRecep>" +
														"<CiudadRecep>#CIUDADRECEP#</CiudadRecep>" +
														"<DirPostal/>" +
														"<CmnaPostal/>" +
														"<CiudadPostal/>" +
													"</Receptor>" +
													"<Totales>" +
														"<MntNeto>#MNTNETO#</MntNeto>" +
														"#MNTEXE#" +
														"#IVA#" +
														"<MntTotal>#MNTTOTAL#</MntTotal>" +
													"</Totales>" +
												"</Encabezado>" +
												"#DETALLE#" +
												"#DCT_GLOBAL#" +
											"</Documento>" +
										"</DTE>" +
									"</SetDTE>" +
								"</EnvioBOLETA>";

			return plantillacab;
		}
		public string PlantillaDetBoleta()
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
										"<UnmdItem>#UNMDITEM#</UnmdItem>" +
										"<PrcItem>#PRCITEM#</PrcItem>" +
										"<MontoItem>#MONTOITEM#</MontoItem>" +
										"#DESCUENTO#";
			return plantilladet;
		}
		public string PantillaCabFactura()
        {
			string plantillacab = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>" +
								  "<DTE version=\"1.0\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://www.sii.cl/SiiDte\">" +
								  "<Documento ID =\"#DOCUMENTOID#\">" +
								  "<Encabezado>" +
									  "<IdDoc>" +
										"<TipoDTE>#TPODTE#</TipoDTE>" +
										"<Folio>#FOLIO#</Folio>" +
										"<FchEmis>#FECEMISION#</FchEmis>" +
										"<FchVenc>#FECVENC#</FchVenc>" +
										"<TermPagoGlosa>#TERMPAGOGLOSA#</TermPagoGlosa>" +
									  "</IdDoc>" +
									  "<Emisor>" +
										 "<RUTEmisor>#RUTEMISOR#</RUTEmisor>" +
										 "<RznSoc>#RZNSOC#</RznSoc>" +
										 "<GiroEmis>#GIRO#</GiroEmis>" +
										 "<Acteco>#ACTECO#</Acteco>" +
										 "<CdgSIISucur>#CDGSIISUCUR#</CdgSIISucur>" +
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
									  "<Totales>" +
										  "<MntNeto>#MNTNETO#</MntNeto>" +
										  "<MntExe>#MNTEXE#</MntExe>" +
										  "<TasaIVA>#TASAIVA#</TasaIVA>" +
										  "<IVA>#IVA#</IVA>" +
										  "<MntTotal>#MNTTOTAL#</MntTotal>" +
									  "</Totales>" +
								  "</Encabezado>" +
									"#DETALLE#"+
									"#DCT_GLOBAL#" +
									"#REFERENCIA#" +
								  "</Documento>" +
								"</DTE>";
			return plantillacab;
		}
		public string PantillaDetFactura()
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
		public string PlantillaRefFactura()
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
		public string PlantillaCabGuiaDespacho()
        {
			string plantillacab = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>" +
								  "<DTE version=\"1.0\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://www.sii.cl/SiiDte\">" +
								  "<Documento ID =\"#DOCUMENTOID#\">" +
								  "<Encabezado>" +
									  "<IdDoc>" +
										"<TipoDTE>#TPODTE#</TipoDTE>" +
										"<Folio>#FOLIO#</Folio>" +
										"<FchEmis>#FECEMISION#</FchEmis>" +
										"<IndTraslado>#INDTRASLADO#</IndTraslado>"+
										"<MntBruto>#MNTBRUTO#</MntBruto>"+
										"<FchVenc>#FECVENC#</FchVenc>" +
									  "</IdDoc>" +
									  "<Emisor>" +
										 "<RUTEmisor>#RUTEMISOR#</RUTEmisor>" +
										 "<RznSoc>#RZNSOC#</RznSoc>" +
										 "<GiroEmis>#GIRO#</GiroEmis>" +
										 "<Acteco>#ACTECO#</Acteco>" +
										 "<CdgSIISucur>#CDGSIISUCUR#</CdgSIISucur>" +
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
										  "<DirPostal/>"+
										  "<CmnaPostal/>"+
										  "<CiudadPostal/>"+
										  "<DirDest>#DIRDEST#</DirDest>"+
										  "<CmnaDest>#CMNADEST#</CmnaDest>"+
										  "<CiudadDest>#CIUDADDEST#</CiudadDest>"+
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
								  "</Documento>" +
								"</DTE>";
			return plantillacab;
		}
		public string PantillaDetGuiaDespacho()
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
		public string PlantillaCabNotaCredito()
        {
			string plantillacab = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>" +
								  "<DTE version=\"1.0\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://www.sii.cl/SiiDte\">" +
								  "<Documento ID =\"#DOCUMENTOID#\">" +
								  "<Encabezado>" +
									  "<IdDoc>" +
										"<TipoDTE>#TPODTE#</TipoDTE>" +
										"<Folio>#FOLIO#</Folio>" +
										"<FchEmis>#FECEMISION#</FchEmis>" +
										"<FchVenc>#FECVENC#</FchVenc>" +
									  "</IdDoc>" +
									  "<Emisor>" +
										 "<RUTEmisor>#RUTEMISOR#</RUTEmisor>" +
										 "<RznSoc>#RZNSOC#</RznSoc>" +
										 "<GiroEmis>#GIRO#</GiroEmis>" +
										 "<Acteco>#ACTECO#</Acteco>" +
										 "<CdgSIISucur>#CDGSIISUCUR#</CdgSIISucur>" +
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
										  "<DirPostal/>"+
										  "<CmnaPostal/>"+
										  "<CiudadPostal/>"+
										  "<DirDest/>"+
										  "<CmnaDest/>"+
										  "<CiudadDest/>"+
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
		public string PantillaDetNotaCredito()
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
		public string PlantillaRefNotaCredito()
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

		public string PlantillaDescuentoGlobal()
		{
			string plantillaref = "<descg>" +
										"<Tipo>DESCG</NroLinRef>" +
										"<NroLinDR>1</TpoDocRef>" +
										"<TpoMov>D</FolioRef>" +
										"<GlosaDR>Descuento</FchRef>" +
										"<TpoValor>$</CodRef>" +
										"<ValorDR>#VALOR_DCT_GLOBAL#</CodRef>" +
										"<IndExeDR>0</CodRef>" +
									"</descg>";
			return plantillaref;
		}

	}
}