namespace ProxyVyV
{
    using log4net;
    using System;
    using System.Reflection;
    using System.Threading;

    public class CCustomizationInterface : CAbstractCustomizationInterface
    {
        protected CZMQClient Client;
        public string IpAddress = string.Empty;
        public string Port = string.Empty;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public CCustomizationInterface(string AAddress, int APort)
        {
            this.IpAddress = AAddress;
            this.Port = APort.ToString();
            CZMQClient client1 = new CZMQClient(this);
            client1.Address = AAddress;
            client1.Port = APort.ToString();
            this.Client = client1;
        }

        public override void ProcessData()
        {
            string iOBuffer = this.Client.IOBuffer;
            string str2 = this.RouteMessage(iOBuffer);
            this.Client.IOBuffer = str2;
        }

        public override void StartListener()
        {
            if ((this.Client.Address == "") || (this.Client.Port == ""))
            {
                throw new EConfiguration("There is no Address or Port information for the listener.");
            }
            this.Client.ClientThread.Start();
            base.Listening = true;
        }

        public override void StopListener()
        {
            log.Info("About to kill the thread");
            try
            {
                this.Client.ClientThread.Abort();
            }
            catch (ThreadAbortException exception)
            {
                log.Info("Killed the thread");
                log.Info(exception);
            }
        }
    }
}

