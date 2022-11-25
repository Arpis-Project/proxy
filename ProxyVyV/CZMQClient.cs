namespace ProxyVyV
{
    using log4net;
    using NetMQ;
    using NetMQ.Sockets;
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

    public class CZMQClient
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public CZMQClient(CCustomizationInterface AOwner)
        {
            this.Responder = new ResponseSocket(null);
            this.CZMQowner = AOwner;
            this.ClientThread = new Thread(new ThreadStart(this.Execute));
        }

        public void Execute()
        {
            TimeSpan timeout = new TimeSpan(0x927c0L);
            this.Responder.Bind("tcp://" + this.Address + ":" + this.Port);
            while (true)
            {
                string str;
                if (this.Responder.TryReceiveFrameString(timeout, Encoding.Unicode, out str))
                {
                    this.IOBuffer = str;
                    log.Info(str);
                    if (this.IOBuffer != "")
                    {
                        CCustomizationInterface cZMQowner = this.CZMQowner;
                        lock (cZMQowner)
                        {
                            this.CZMQowner.ProcessData();
                            this.Responder.SendFrame(Encoding.Unicode.GetBytes(this.IOBuffer), false);
                        }
                    }
                }
            }
        }

        public CCustomizationInterface CZMQowner { get; set; }

        public string Port { get; set; }

        public string Address { get; set; }

        public string IOBuffer { get; set; }

        public ResponseSocket Responder { get; set; }

        public Thread ClientThread { get; set; }
    }
}

