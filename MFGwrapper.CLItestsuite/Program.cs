﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFGwrapper.CLItestsuite
{
    class Program
    {
        static void Main(string[] args)
        {
            var wrapper = new Forwarder.Spliter(8123, 8080, 18080);
            wrapper.Start();
            System.Threading.Thread.Sleep(10000000);
        }
    }
}
