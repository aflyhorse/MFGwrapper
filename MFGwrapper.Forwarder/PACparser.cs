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

        public static HashSet<string> Parse()
        {
            var client = new System.Net.WebClient();
            string pac = client.DownloadString(PACuri);
            var RegIP = new System.Text.RegularExpressions.Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            var MatchResult = RegIP.Matches(pac);
            var Hostlist = new HashSet<string>();
            foreach (System.Text.RegularExpressions.Match m in MatchResult)
                if (m.Value != "255.255.255.255" && m.Value != "127.0.0.1")
                    Hostlist.Add(m.Value);
            return Hostlist;
        }
    }
}
