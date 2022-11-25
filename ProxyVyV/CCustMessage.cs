namespace ProxyVyV
{
    using System;

    public class CCustMessage
    {
        public string ApiDirection;
        public string URI;
        public string ContentType;
        public string Verb;
        public string Resource;
        public string QueryString;
        public string Payload;

        public CCustMessage(string Direction, string URI, string ContentType, string Verb, string Resource, string QueryString, string Payload)
        {
            this.ApiDirection = Direction;
            this.URI = URI;
            this.ContentType = ContentType;
            this.Verb = Verb;
            this.Resource = Resource;
            this.QueryString = QueryString;
            this.Payload = Payload;
        }
    }
}

