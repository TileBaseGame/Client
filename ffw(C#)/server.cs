using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ffw
{
    namespace server
    {
        // define constant command code
        enum cmd
        {
            REQ_REGISTER = 1000,
            RSP_REGISTER = 9000,

            REQ_LOGIN = 1001,
            RSP_LOGIN = 9001,

            REQ_LOGOUT = 1002,
            RSP_LOGOUT = 9002,

            REQ_QUICKPLAY = 1003,
            RSP_QUICKPLAY = 9003,
        };

        // define the server's response
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

        // the base class for msg handler
        public abstract class msghandler
        {
            public abstract void proc(tcp.response rsp);
        }

        // define session with server
        public class session
        {
            private long running = 0;
            private tcp.client tc = null;
            public Dictionary<int, tcp.response> match = new Dictionary<int, tcp.response>();

            public delegate void on_event(tcp.response rsp);
            public msghandler handler = null;

            public session(msghandler handler, string ip, int port)
            {
                this.handler = handler;
                tc = new tcp.client(ip, port);
                start();
            }   

            public void start()
            {
                Interlocked.Increment(ref running);

                tc.connect();
                match.Clear();

                new Thread(new ParameterizedThreadStart(on_message)).Start(this);
            }

            public void stop()
            {
                Interlocked.Increment(ref running);
                tc.close();
            }

            private bool send(tcp.request req)
            {
                return req.send(tc);
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
                    if (rsp != null)
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
                        else // process the unresolved messages
                        {
                            if (s.handler != null) s.handler.proc(rsp);
                        }
                    }
                }
            }

            public result login(string usr, string passwd, int timeout /* ms */)
            {
                if (!tc.isConnected())
                {
                    start();
                    if (!tc.isConnected()) return new result(false, "connection lost");
                }

                tcp.request req = new tcp.request((short)cmd.REQ_LOGIN);
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
                    return new result();

                tcp.request req = new tcp.request((short)cmd.REQ_LOGOUT);
                tcp.response rsp = send(req, timeout);
                if (rsp == null)
                    return new result();

                int res = rsp.nextInt();
                string msg = rsp.nextString();
                Console.WriteLine("cmd=[{0}], sn=[{1}], result=[{2}], errmsg=[{3}]", rsp.cmd, rsp.sn, res, msg);
                return new result(res==0, msg);
            }

            public result register(string email, string usr, string passwd, int timeout)
            {
                if (!tc.isConnected())
                {
                    start();
                    if(!tc.isConnected()) return new result(false, "connection lost");
                }

                tcp.request req = new tcp.request((short)cmd.REQ_REGISTER);
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

            public result quickplay()
            {
                if (!tc.isConnected())
                {
                   return new result(false, "connection lost");
                }

                tcp.request req = new tcp.request((short)cmd.REQ_QUICKPLAY);
                if(!send(req))
                {
                    return new result(false, "network error: failed to send");
                }
                return new result();
            }

        }
    }
}
