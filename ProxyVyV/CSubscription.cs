namespace ProxyVyV
{
    using System;
    using System.Runtime.CompilerServices;

    public class CSubscription
    {
        public string Name { get; set; }

        public Direction Direction { get; set; }

        public bool Get { get; set; }

        public bool Put { get; set; }

        public bool Post { get; set; }

        public bool Delete { get; set; }

        public string Resource { get; set; }

        public string Pattern { get; set; }

        public bool Payload { get; set; }

        public bool Redirected { get; set; }
    }
}

