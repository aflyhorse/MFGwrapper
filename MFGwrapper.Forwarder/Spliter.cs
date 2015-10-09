using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFGwrapper.Forwarder
{
    public class Spliter
    {
        public int UpstreamPort
        { get; }
        public int MFGPort
        { get; }
        public int ListenPort
        { get; }
        private HashSet<string> HostList;

        public Spliter(int UpstreamPort, int MFGPort, int ListenPort)
        {
            this.UpstreamPort = UpstreamPort;
            this.MFGPort = MFGPort;
            this.ListenPort = ListenPort;
            Fiddler.FiddlerApplication.SetAppDisplayName("MFGwrapper");
            Fiddler.FiddlerApplication.BeforeRequest += FiddlerApplication_BeforeRequest;
        }

        public void Start()
        {
            HostList = PACparser.Parse();
            Fiddler.FiddlerApplication.Startup(ListenPort,
                Fiddler.FiddlerCoreStartupFlags.ChainToUpstreamGateway & ~Fiddler.FiddlerCoreStartupFlags.DecryptSSL);
        }

        private void FiddlerApplication_BeforeRequest(Fiddler.Session oSession)
        {
            oSession.bBufferResponse = false;
            if (HostList.Contains(oSession.host))
                oSession["X-OverrideGateway"] = "localhost:" + MFGPort;
            else
                oSession["X-OverrideGateway"] = "localhost:" + UpstreamPort;
            System.Diagnostics.Debug.WriteLine(oSession.host);
        }

        public void Stop()
        {
            Fiddler.FiddlerApplication.Shutdown();
        }

        ~Spliter()
        {
            Stop();
        }
    }
}
