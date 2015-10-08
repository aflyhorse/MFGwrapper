using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFGwrapper.TCPProxy
{
    class TCPSender
    {
        public System.Net.IPAddress UpstreamIP
        { get; }

        public int UpstreamPort
        { get; }

        private System.Net.Sockets.TcpClient tcpclient;
        private System.Net.Sockets.NetworkStream ns;
        private System.IO.StreamWriter sw;

        public TCPSender(System.Net.IPAddress UpstreamIP, int UpstreamPort)
        {
            this.UpstreamIP = UpstreamIP;
            this.UpstreamPort = UpstreamPort;
            tcpclient = new System.Net.Sockets.TcpClient(new System.Net.IPEndPoint(UpstreamIP, UpstreamPort));
            ns = tcpclient.GetStream();
            sw = new System.IO.StreamWriter(ns);
            sw.AutoFlush = true;
        }

        ~TCPSender()
        {
            sw.Close();
            ns.Close();
            tcpclient.Close();
        }

        public void Send(string str)
        {
            sw.Write(str);
        }

    }
}
