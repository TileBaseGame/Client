using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ffw
{
    namespace server
    {
        public class result
        {
            public bool succ;
            public string info;

            public result()
            {
                succ = true;
                info = "";
            }

            public result(bool succ, string info)
            {
                this.succ = succ;
                this.info = info;
            }
        }

        public class session
        {
            private long running = 0;
            private tcp.client tc = null;
            public Dictionary<int, tcp.response> match = new Dictionary<int, tcp.response>();

            public session(string ip, int port)
            {
                tc = new tcp.client(ip, port);
                tc.connect();
                match.Clear();

                start();
            }   

            public void start()
            {
                running++;
                new Thread(new ParameterizedThreadStart(on_message)).Start(this);
            }

            public void stop()
            {
                running++;
            }

            ~session()
            {
                stop();
                tc.close();
            }

            private tcp.response send(tcp.request req, int timeout /* ms */)
            {
                // 1. add to req-rsp match table
                lock (this)
                {
                    match.Add(req.sn, null);
                }

                // 2. send request
                if (!req.send(tc))
                {
                    lock (this)
                    {
                        match.Remove(req.sn);
                    }
                    return null;
                }

                // 3. block wait
                tcp.response rsp = null;
                int tick = timeout > 100 ? 100 : timeout;
                while (tick > 0)
                {
                    timeout -= tick;
                    Thread.Sleep(tick);
                    tick = timeout > 100 ? 100 : timeout;

                    match.TryGetValue(req.sn, out rsp);
                    if(rsp != null)
                        break;
                }

                lock (this)
                {
                    match.Remove(req.sn);
                }
                return rsp;
            }

            public static void on_message(object ssn)
            {
                session s = (session)ssn;
                long running = s.running;
                while (running == s.running)
                {
                    tcp.response rsp = s.tc.recv();
                    if (rsp == null)
                    {
                        if (!s.tc.isConnected())
                        {
                            Console.WriteLine("WARN: connection off, on_message will exit.");
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    lock (s)
                    {
                        tcp.response t = null;
                        if (s.match.TryGetValue(rsp.sn, out t))
                        {
                            s.match[rsp.sn] = rsp;
                        }
                        else
                        {
                            // FIXME: unhandled message
                            Console.WriteLine("cmd=[{0}], sn=[{1}]", rsp.cmd, rsp.sn);
                        }
                    }
                }
            }

            public result login(string usr, string passwd, int timeout /* ms */)
            {
                if (!tc.isConnected())
                {
                    return new result(false, "connection lost");
                }

                tcp.request req = new tcp.request((short)1001);
                req.encode(usr);
                req.encode(passwd);
                
                tcp.response rsp = send(req, timeout);
                if (rsp == null)
                {
                    return new result(false, "no response");
                }

                int result = rsp.nextInt();
                string errmsg = rsp.nextString();
                Console.WriteLine("cmd=[{0}], sn=[{1}], result=[{2}], errmsg=[{3}]", rsp.cmd, rsp.sn, result, errmsg);
                return new result(result==0, errmsg);
            }

            public result logout(int timeout)
            {
                if (!tc.isConnected())
                {
                    return new result(false, "connection lost");
                }

                tcp.request req = new tcp.request((short)1002);
                tcp.response rsp = send(req, timeout);
                if (rsp == null)
                {
                    return new result(false, "no response");
                }

                int result = rsp.nextInt();
                string errmsg = rsp.nextString();
                Console.WriteLine("cmd=[{0}], sn=[{1}], result=[{2}], errmsg=[{3}]", rsp.cmd, rsp.sn, result, errmsg);
                return new result(result==0, errmsg);
            }

            public result register(string email, string usr, string passwd, int timeout)
            {
                if (!tc.isConnected())
                {
                    return new result(false, "connection lost");
                }

                tcp.request req = new tcp.request((short)1000);
                req.encode(email);
                req.encode(usr);
                req.encode(passwd);

                tcp.response rsp = send(req, timeout);
                if (rsp == null)
                {
                    return new result(false, "no response");
                }

                int result = rsp.nextInt();
                string errmsg = rsp.nextString();
                Console.WriteLine("cmd=[{0}], sn=[{1}], result=[{2}], errmsg=[{3}]", rsp.cmd, rsp.sn, result, errmsg);
                return new result();
            }

        }
    }
}
