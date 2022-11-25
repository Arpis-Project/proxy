using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;

namespace ProxyVyV
{
    class ImpresoraOPOS
    {
       
        private string ToXML(object xml)
        {
            var stringwriter = new StringWriter();
            var serializer = new XmlSerializer(xml.GetType());
            serializer.Serialize(stringwriter, xml);
            return stringwriter.ToString();
        }
      
    }
}
