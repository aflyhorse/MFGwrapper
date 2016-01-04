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
            bool inUse = false;
            var IPEndPoints = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
            foreach (var IPEndPoint in IPEndPoints)
            {
                if (IPEndPoint.Port == port)
                {
                    inUse = true;
                    break;
                }
            }
            return inUse;
        }
    }
}
