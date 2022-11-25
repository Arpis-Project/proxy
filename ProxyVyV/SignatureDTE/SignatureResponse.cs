using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignatureVyV
{

    public class SignatureResponse
    {
        public SignatureResponse(string json)
        {
            if (!String.IsNullOrEmpty(json))
                FromString(json);
        }

        public SignatureResponse()
        {
            this.Success = false;
        }

        public bool Success { get; set; }
        public Result Result { get; set; }

        public string ErrorMessage { get; set; }
        public string LogMessage { get; set; }
        public string UIMessage { get; set; }

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

        public void FromString(String json)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                CheckAdditionalContent = false
            };

            SignatureResponse temp = JsonConvert.DeserializeObject<SignatureResponse>(json, settings);
            this.ErrorMessage = temp.ErrorMessage;
            this.Result = temp.Result;
            this.Success = temp.Success;

        }
    }

    public class Result
    {
        public string DocId { get; set; }
        public int Folio { get; set; }
        public string Status { get; set; }
        public string Code { get; set; }
    }


}
