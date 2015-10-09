using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFGwrapper.Forwarder
{
    static class PACparser
    {
        public static System.Uri PACuri
        { get; set; } = new Uri("https://myfleet.moe/assets/proxy.pac");

        static HashSet<string> Parse()
        {
            var client = new System.Net.WebClient();
            string pac = client.DownloadString(PACuri);
            return null;
        }
    }
}
