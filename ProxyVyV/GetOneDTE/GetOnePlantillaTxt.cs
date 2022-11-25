using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyVyV.ProxyVyV.GetOneDTE
{
    public static class GetOnePlantillaTxt
    {
        public static string Cabecera(string tipoDocumento)
        {
            switch (tipoDocumento)
            {
                case "39":
                    return @"<ENCABEZADO>
                            Tipo DTE |#TIPODTE#
                            Fecha de Emision |#FECHAEMISION#
                            Rut Receptor |#RUTRECEPTOR#
                            Folio | #FOLIO#
                            Razon Social Receptor |#RAZONSOCIALRECEPTOR#
                            Direccion Receptor |#DIRECCIONRECEPTOR#
                            Comuna Receptor |#COMUNARECEPTOR#
                            Ciudad Receptor |#CIUDADRECEPTOR#
                            Fecha Vencimiento |#FECHAVENCIMIENTO#
                            Direccion Origen |#DIRECCIONORIGEN#
                            Comuna Origen |#COMUNAORIGEN#
                            Ciudad Origen |#CIUDADORIGEN#
                            Indicador Servicio |#INDICADORSERVICIO#
                            Monto Neto|#MONTONETO#
                            IVA|#IVA#
                            Monto Exento|#MONTOEXENTO#
                            Monto Total|#MONTOTOTAL#";
                case "33":
                    return @"<ENCABEZADO>
                            Tipo DTE |#TIPODTE#
                            Fecha de Emision |#FECHAEMISION#
                            Rut Receptor |#RUTRECEPTOR#
                            Folio | #FOLIO#
                            Razon Social Receptor |#RAZONSOCIALRECEPTOR#
                            Giro Receptor |#GIRORECEPTOR#
                            Direccion Receptor |#DIRECCIONRECEPTOR#
                            Comuna Receptor |#COMUNARECEPTOR#
                            Ciudad Receptor |#CIUDADRECEPTOR#
                            Condicion de Venta |#CONDICIONVENTA#
                            Fecha Vencimiento |#FECHAVENCIMIENTO#
                            Direccion Origen |#DIRECCIONORIGEN#
                            Comuna Origen |#COMUNAORIGEN#
                            Ciudad Origen |#CIUDADORIGEN#
                            Tasa IVA |#TASAIVA#
                            Direccion Destino |#DIRECCIONDESTINO#
                            Ciudad Destino |#CIUDADDESTINO#
                            Comuna Destino |#COMUNADESTINO#
                            Forma de Pago |#FORMAPAGO#
                            Monto Neto|#MONTONETO#
                            IVA|#IVA#
                            Monto Exento|#MONTOEXENTO#
                            Monto Total|#MONTOTOTAL#";
                case "61":
                    return @"<ENCABEZADO>
                            Tipo DTE |#TIPODTE#
                            Fecha de Emision |#FECHAEMISION#
                            Rut Receptor |#RUTRECEPTOR#
                            Folio | #FOLIO#
                            Razon Social Receptor |#RAZONSOCIALRECEPTOR#
                            Giro Receptor |#GIRORECEPTOR#
                            Direccion Receptor |#DIRECCIONRECEPTOR#
                            Comuna Receptor |#COMUNARECEPTOR#
                            Ciudad Receptor |#CIUDADRECEPTOR#
                            Condicion de Venta |#CONDICIONVENTA#
                            Fecha Vencimiento |#FECHAVENCIMIENTO#
                            Direccion Origen |#DIRECCIONORIGEN#
                            Comuna Origen |#COMUNAORIGEN#
                            Ciudad Origen |#CIUDADORIGEN#
                            Tasa IVA |#TASAIVA#
                            Direccion Destino |#DIRECCIONDESTINO#
                            Ciudad Destino |#CIUDADDESTINO#
                            Comuna Destino |#COMUNADESTINO#
                            Forma de Pago |#FORMAPAGO#
                            Monto Neto|#MONTONETO#
                            IVA|#IVA#
                            Monto Exento|#MONTOEXENTO#
                            Monto Total|#MONTOTOTAL#";
                case "52":
                    return @"<ENCABEZADO>
                            Tipo DTE |#TIPODTE#
                            Fecha de Emision |#FECHAEMISION#
                            Rut Receptor |#RUTRECEPTOR#
                            Folio | #FOLIO#
                            Razon Social Receptor |#RAZONSOCIALRECEPTOR#
                            Giro Receptor |#GIRORECEPTOR#
                            Direccion Receptor |#DIRECCIONRECEPTOR#
                            Comuna Receptor |#COMUNARECEPTOR#
                            Ciudad Receptor |#CIUDADRECEPTOR#
                            Condicion de Venta |#CONDICIONVENTA#
                            Fecha Vencimiento |#FECHAVENCIMIENTO#
                            Direccion Origen |#DIRECCIONORIGEN#
                            Comuna Origen |#COMUNAORIGEN#
                            Ciudad Origen |#CIUDADORIGEN#
                            Tasa IVA |#TASAIVA#
                            Ind. Traslado de bienes |#INDICADORSERVICIO#
                            Direccion Destino |#DIRECCIONDESTINO#
                            Ciudad Destino |#CIUDADDESTINO#
                            Comuna Destino |#COMUNADESTINO#
                            Forma de Pago |#FORMAPAGO#
                            Monto Neto|#MONTONETO#
                            IVA|#IVA#
                            Monto Exento|#MONTOEXENTO#
                            Monto Total|#MONTOTOTAL#";
                default:
                    return "";
            }
        }
        public static string Detalle()
        {
            return "#NROLINEA#|#CANTIDAD#|#TIPOCODIGO#|#CODIGOITEM#|#INDICADOREXENCION#|#UNIDADMEDIDA#|#NOMBREITEM#|#PRECIOUNITARIO#|#DCTOPORCENTAJE#|#DCTOMONTO#|#MONTOITEM#";
        }
        public static string DescuentosRecargos()
        {
            return "#NRODESCRECARGO#|#TIPOMOVIMIENTO#|#GLOSADESCRECARGO#|#TIPOVALOR#|#VALORDESCRECARGO#";
        }
        public static string Referencias()
        {
            return "#NROLINEAREFERENCIA#|#TIPODOCUMENTOREFERENCIA#|#FECHAREFERENCIA#|#FOLIOREFERENCIA#|#CODIGOREFERENCIA#";
        }
    }
}
