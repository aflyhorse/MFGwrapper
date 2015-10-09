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
        public int MGFPort
        { get; }
        public int ListenPort
        { get; }

        public Spliter(int UpstreamPort, int MGFPort, int ListenPort)
        {
            this.UpstreamPort = UpstreamPort;
            this.MGFPort = MGFPort;
            this.ListenPort = ListenPort;
            Fiddler.FiddlerApplication.SetAppDisplayName("MFGwrapper");
            Fiddler.FiddlerApplication.BeforeRequest += FiddlerApplication_BeforeRequest;
        }

        public void Start()
        {
            Fiddler.FiddlerApplication.Startup(ListenPort,
                Fiddler.FiddlerCoreStartupFlags.ChainToUpstreamGateway & ~Fiddler.FiddlerCoreStartupFlags.DecryptSSL);
        }

        private void FiddlerApplication_BeforeRequest(Fiddler.Session oSession)
        {
            oSession.bBufferResponse = false;
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
