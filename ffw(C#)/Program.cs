using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ffw
{
    class Program
    {
        static void Main(string[] args)
        {
            server.session ss = new server.session("127.0.0.1", 9252);

            server.result r = ss.login("lily", "1234561", 5000);
            if (!r.succ)
            {
                Console.WriteLine("ERROR: failed to login: {0}", r.info);
            }
            else
            {
                ss.logout(5010);
            }
            ss.stop();

            Console.WriteLine("INFO: closed");
        }
    }
}
