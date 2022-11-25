using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using SignatureVyV;
using System.Threading;

namespace SignatureVyV
{
    public class AgenteSignature
    {
        private string server = "127.0.0.1";
        private Int32 port = 11000;

        public AgenteSignature(string server, int port)
        {
            this.server = server ?? throw new ArgumentNullException(nameof(server));
            this.port = port;
        }

        public bool Ejecutar(SignatureDTE documento, out int folio, out string ted, out string error)
        {
            folio = 0;
            ted = "";
            error = "";

            try
            {
           
                //se envia data al agente Signature
                SignatureResponse enviar = Send(documento.ToString());

                //si salio bien se procede a cargar el folio y consolidar la transaccion para obtener el timbre electronico
                if (enviar.Success)
                {
                    error = "";
                    folio = enviar.Result.Folio;

                    SignatureResponse consolidar = Send("{\"Action\":\"consolidate\",\"Args\":\""+ enviar.Result.DocId   + "\"}");

                    //Si comando consolidar fue exitoso se carga el timbre en la respuesta
                    if (consolidar.Success)
                    {
                        ted = consolidar.Result.Code;
                        ted = ted.Replace("\r", "");
                        ted = ted.Replace("\n", "");
                        ted = ted.Replace("\t", "");
                        return true;
                    }
                    //error al consolidar el documento
                    error = consolidar.ErrorMessage;
                    return false;
                }
                else
                {
                    error = enviar.ErrorMessage;
                    // error al enviar a agente.
                    return false;
                }
            }
            catch (Exception e)
            {   //algo salio muy mal
                error =e.Message;
            }
            //si llego aqui tamos mal
            return false;
        }

        
        private SignatureResponse Send(string json)
        {
            SignatureResponse result = new SignatureResponse();
            try
            {
                TcpClient client = new TcpClient(server, port);
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(json);
                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);
             
                String responseData = String.Empty;

                //esperara hasta 3 segundos por una respuesta si es que el agente no reponde de inmediato
                //esto no sale en la documentacion pero evita dolores de cabeza
                for (int i = 0; i < 3000; i++)
                {
                    if (stream.DataAvailable) break;
                    Thread.Sleep(10);
                }

                //lee byte por byte la respuesta y la almacena en el string de respuesta
                int read = stream.ReadByte();
                while (read != -1)
                {
                    Byte[] b = new Byte[1];// ;
                    b[0] = (byte)read;
                    responseData += System.Text.Encoding.ASCII.GetString(b,0,1);
                    read = stream.ReadByte();
                }

                //cierra todo para liberar memoria y avisar al agente Signature que no espere mas comandos
                //es mas limpio y genera menos problemas abrir y cerrar una nueva conexion para cada comando
                stream.Close();
                client.Close();

                //si se obtuvo respuesta se verifica que venga dentro de la data la palabra Succes
                if (!string.IsNullOrEmpty(responseData))
                {
                    if (responseData.Contains("Success"))
                    {   // se deserializa la respuesta y se carga en el objeto result
                        result = new SignatureResponse(responseData);
                        return result;
                    }
                }
            }
            catch (Exception e)
            {   //algo salio muy mal
                result.Success = false;
                result.ErrorMessage = e.Message;
            }

            return result;
        }
    }
}
