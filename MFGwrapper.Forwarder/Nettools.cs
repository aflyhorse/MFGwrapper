using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFGwrapper.Forwarder
{
    public static class Nettools
    {
        public static bool isPortInUse(int port)
        {
            var IPEndPoints = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
            return (IPEndPoints.FirstOrDefault(i => i.Port == port) != null);
        }

        public static int? isPortInUse(List<int> ports)
        {
            var IPEndPoints = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
            return IPEndPoints.FirstOrDefault(i => ports.Contains(i.Port))?.Port;
        }
    }
}
