using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFGwrapper.TCPProxy
{
    class TCPListener
    {
        public System.Net.IPAddress BindingIP
        { get; }

        public int BindingPort
        { get; }

        private System.Net.Sockets.TcpListener tcplistener;
        private System.Net.Sockets.NetworkStream ns;
        private System.IO.StreamReader sr;

        public TCPListener(System.Net.IPAddress BindingIP, int BindingPort)
        {
            this.BindingIP = BindingIP;
            this.BindingPort = BindingPort;
            tcplistener = new System.Net.Sockets.TcpListener(BindingIP, BindingPort);
        }
    }
}
