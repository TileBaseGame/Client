using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using ffw.server;

namespace ffw
{
    // define a class to implement the abstract msghandler
    class main : msghandler
    {
        // the message proc
        public override void proc(tcp.response rsp)
        {
            switch (rsp.cmd)
            {
                case (short)server.cmd.RSP_QUICKPLAY: // match ok, print the opponent's name
                    {
                        int res = rsp.nextInt();
                        string opponent = rsp.nextString();
                        Console.WriteLine("cmd=[{0}], sn=[{1}], result=[{2}], opponent=[{3}]", rsp.cmd, rsp.sn, res, opponent);
                    }
                    break;
            }
        }

        static void Main(string[] args)
        {
            main m1 = new main();
            main m2 = new main();

            session s1 = new session(m1, "127.0.0.1", 9252);
            session s2 = new session(m2, "127.0.0.1", 9252);

            // 1. lily login
            result r = s1.login("lily", "123456", 10000);
            if (!r.succ)
            {
                Console.WriteLine("ERROR: failed to login: {0}", r.info);
                return;
            }
            s1.quickplay(); // 2. lily want to play game

            // 3. test login
            r = s2.login("test", "t", 10000);
            if (!r.succ)
            {
                Console.WriteLine("ERROR: failed to login: {0}", r.info);
                s1.stop();
            }
            Thread.Sleep(2000);
            s2.quickplay(); // 4. test want to play

            // pause for a while, both logout
            Thread.Sleep(5000);
            s1.logout(5000);
            s2.logout(5000);
            s1.stop();
            s2.stop();
        }
    }
}
