namespace ProxyVyV
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Xml;

    public abstract class CAbstractCustomizationInterface
    {
        private const string cxGetInfo = "GetInfo";
        private const string cxSetConfig = "SetConfig";
        private const string cxGetSubscriptions = "GetSubscriptions";
        private const string cxSetUserToken = "SetUserToken";
        private const string cxShutdown = "Shutdown";
        private const string cxACK = "<ACK/>";
        private const string cxNAQ = "<NAQ/>";
        private string proxySID;

        public CAbstractCustomizationInterface()
        {
            this.Subscriptions = new CSubscriptionList();
            this.Listening = false;
        }

        protected string DecodePayload(string source)
        {
            string str = source.Trim();
            if (str != "")
            {
                str = str.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&apos;", "'").Replace("&quot;", "\"");
            }
            return str;
        }

        protected string EncodePayload(string source)
        {
            string str = source.Trim();
            if (str != "")
            {
                str = str.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;").Replace("\"", "&quot;");
            }
            return str;
        }

        protected virtual string HandleEvent(string Data)
        {
            bool canContinue = true;
            XmlDocument document = new XmlDocument();
            document.LoadXml(Data);
            string direction = "";
            int statusCode = 0;
            string hTTPVerb = "";
            string resourceName = "";
            string uRL = "";
            string headers = "";
            string @params = "";
            string payload = "";
            string contentType = "";
            foreach (XmlNode node2 in document.FirstChild.ChildNodes)
            {
                if (node2.Name == "DIRECTION")
                {
                    direction = node2.InnerText;
                    continue;
                }
                if (node2.Name == "STATUSCODE")
                {
                    statusCode = int.Parse(node2.InnerText);
                    continue;
                }
                if (node2.Name == "HTTPVERB")
                {
                    hTTPVerb = node2.InnerText;
                    continue;
                }
                if (node2.Name == "RESOURCENAME")
                {
                    resourceName = node2.InnerText;
                    continue;
                }
                if (node2.Name == "URI")
                {
                    uRL = node2.InnerText;
                    continue;
                }
                if (node2.Name == "HEADERS")
                {
                    headers = node2.InnerText;
                    continue;
                }
                if (node2.Name == "PARAMS")
                {
                    @params = node2.InnerText;
                    continue;
                }
                if (node2.Name == "PAYLOAD")
                {
                    payload = this.DecodePayload(node2.InnerText);
                    continue;
                }
                if (node2.Name == "CONTENTTYPE")
                {
                    contentType = node2.InnerText;
                }
            }
            ProxyEventNotification onEvent = this.OnEvent;
            if (onEvent == null)
            {
                ProxyEventNotification local1 = onEvent;
            }
            else
            {
                onEvent(direction, ref statusCode, hTTPVerb, resourceName, uRL, ref headers, ref @params, ref payload, contentType, ref canContinue);
            }
            string[] textArray1 = new string[11];
            textArray1[0] = "<EVENT><HEADERS>";
            textArray1[1] = headers;
            textArray1[2] = "</HEADERS><PARAMS>";
            textArray1[3] = @params;
            textArray1[4] = "</PARAMS><PAYLOAD>";
            textArray1[5] = this.EncodePayload(payload);
            textArray1[6] = "</PAYLOAD><CONTINUE>";
            textArray1[7] = canContinue.ToString();
            textArray1[8] = "</CONTINUE><STATUSCODE>";
            textArray1[9] = statusCode.ToString();
            textArray1[10] = "</STATUSCODE></EVENT>";
            return string.Concat(textArray1);
        }

        protected virtual string HandleGetInfo(string Data)
        {
            string[] textArray1 = new string[9];
            textArray1[0] = "<CONTROL>  <NAME>";
            textArray1[1] = this.Name;
            textArray1[2] = "</NAME>  <VERSION>";
            textArray1[3] = this.Version;
            textArray1[4] = "</VERSION>  <DEVELOPERID>";
            textArray1[5] = this.DeveloperID;
            textArray1[6] = "</DEVELOPERID>  <CUSTOMIZATIONID>";
            textArray1[7] = this.CustomizationID;
            textArray1[8] = "</CUSTOMIZATIONID></CONTROL>";
            string str = string.Concat(textArray1);
            XmlDocument document = new XmlDocument();
            document.LoadXml(Data);
            foreach (XmlNode node2 in document.FirstChild.ChildNodes)
            {
                if (node2.Name == "PROXYSID")
                {
                    this.proxySID = node2.InnerText;
                    break;
                }
            }
            NotifyEvent onGetInfo = this.OnGetInfo;
            if (onGetInfo == null)
            {
                NotifyEvent local1 = onGetInfo;
            }
            else
            {
                onGetInfo();
            }
            return str;
        }

        protected virtual string HandleGetSubscriptions()
        {
            string str;
            if (this.Subscriptions.Count == 0)
            {
                str = "<SUBSCRIPTIONS/>";
            }
            else
            {
                string str2 = "";
                foreach (CSubscription subscription in this.Subscriptions)
                {
                    string[] textArray1 = new string[0x16];
                    textArray1[0] = str2;
                    textArray1[1] = "<SUBSCRIPTION><NAME>";
                    textArray1[2] = subscription.Name;
                    textArray1[3] = "</NAME><DIRECTION>";
                    textArray1[4] = subscription.Direction.ToString();
                    textArray1[5] = "</DIRECTION><HTTPGET>";
                    textArray1[6] = subscription.Get.ToString();
                    textArray1[7] = "</HTTPGET><HTTPPUT>";
                    textArray1[8] = subscription.Put.ToString();
                    textArray1[9] = "</HTTPPUT><HTTPPOST>";
                    textArray1[10] = subscription.Post.ToString();
                    textArray1[11] = "</HTTPPOST><HTTPDELETE>";
                    textArray1[12] = subscription.Delete.ToString();
                    textArray1[13] = "</HTTPDELETE><RESOURCE>";
                    textArray1[14] = subscription.Resource;
                    textArray1[15] = "</RESOURCE><PATTERN>";
                    textArray1[0x10] = subscription.Pattern;
                    textArray1[0x11] = "</PATTERN><PAYLOAD>";
                    textArray1[0x12] = subscription.Payload.ToString();
                    textArray1[0x13] = "</PAYLOAD><REDIRECTED>";
                    textArray1[20] = subscription.Redirected.ToString();
                    textArray1[0x15] = "</REDIRECTED></SUBSCRIPTION>";
                    str2 = string.Concat(textArray1);
                }
                str = "<SUBSCRIPTIONS>" + str2 + "</SUBSCRIPTIONS>";
            }
            return str;
        }

        protected virtual string HandleSetConfig(string Data)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(Data);
            foreach (XmlNode node2 in document.FirstChild.ChildNodes)
            {
                if (node2.Name == "BASECONFIG")
                {
                    this.GlobalConfigurationData = node2.InnerText;
                    continue;
                }
                if (node2.Name == "PROXYCONFIG")
                {
                    this.ProxyConfigurationData = node2.InnerText;
                }
            }
            NotifyEvent onConfigDataSet = this.OnConfigDataSet;
            if (onConfigDataSet == null)
            {
                NotifyEvent local1 = onConfigDataSet;
            }
            else
            {
                onConfigDataSet();
            }
            return "<ACK/>";
        }

        protected virtual string HandleSetUserToken(string Data)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(Data);
            foreach (XmlNode node2 in document.FirstChild.ChildNodes)
            {
                if (node2.Name == "TOKEN")
                {
                    this.UserToken = node2.InnerText;
                    continue;
                }
                if (node2.Name == "PRISMSERVER")
                {
                    this.PrismServer = node2.InnerText;
                    continue;
                }
                if (node2.Name == "PRISMSERVERPORT")
                {
                    this.PrismServerPort = int.Parse(node2.InnerText);
                    continue;
                }
                if (node2.Name == "WORKSTATION")
                {
                    this.WorkstationName = node2.InnerText;
                }
            }
            NotifyEvent onTokenSet = this.OnTokenSet;
            if (onTokenSet == null)
            {
                NotifyEvent local1 = onTokenSet;
            }
            else
            {
                onTokenSet();
            }
            return "<ACK/>";
        }

        protected virtual string HandleShutdown()
        {
            NotifyEvent onShutdownNotification = this.OnShutdownNotification;
            if (onShutdownNotification == null)
            {
                NotifyEvent local1 = onShutdownNotification;
            }
            else
            {
                onShutdownNotification();
            }
            return "<ACK/>";
        }

        public abstract void ProcessData();
        protected virtual string RouteMessage(string Data)
        {
            string str;
            XmlDocument document = new XmlDocument();
            document.LoadXml(Data);
            XmlNode firstChild = document.FirstChild;
            if (firstChild.Name == "EVENT")
            {
                str = this.HandleEvent(Data);
            }
            else
            {
                XmlNode node2 = null;
                foreach (XmlNode node3 in firstChild.ChildNodes)
                {
                    if (node3.Name == "COMMAND")
                    {
                        node2 = node3;
                        break;
                    }
                }
                str = (node2 == null) ? "<NAQ/>" : ((node2.InnerText != "GetInfo") ? ((node2.InnerText != "SetConfig") ? ((node2.InnerText != "GetSubscriptions") ? ((node2.InnerText != "SetUserToken") ? ((node2.InnerText != "Shutdown") ? "<NAQ/>" : this.HandleShutdown()) : this.HandleSetUserToken(Data)) : this.HandleGetSubscriptions()) : this.HandleSetConfig(Data)) : this.HandleGetInfo(Data));
            }
            return str;
        }

        public abstract void StartListener();
        public abstract void StopListener();

        public bool Listening { get; set; }

        public string GlobalConfigurationData { get; set; }

        public string ProxyConfigurationData { get; set; }

        public string ProxySID { get; }

        public string UserToken { get; set; }

        public string PrismServer { get; set; }

        public int PrismServerPort { get; set; }

        public string WorkstationName { get; set; }

        public NotifyEvent OnGetInfo { get; set; }

        public ProxyEventNotification OnEvent { get; set; }

        public NotifyEvent OnConfigDataSet { get; set; }

        public NotifyEvent OnShutdownNotification { get; set; }

        public NotifyEvent OnTokenSet { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public string DeveloperID { get; set; }

        public string CustomizationID { get; set; }

        public CSubscriptionList Subscriptions { get; set; }
    }
}

