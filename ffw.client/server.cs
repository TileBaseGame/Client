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

            REQ_SET_COMMANDER = 1004,
            RSP_SET_COMMANDER = 9004,
            RSP_SET_OPPONENT_COMMANDER = 9104,

            REQ_SET_UNITS = 1005,
            RSP_SET_UNITS = 9005,
            RSP_SET_OPPONENT_UNITS = 9105,

            REQ_SET_UNITS_ABILITIES = 1006,
            RSP_SET_UNITS_ABILITIES = 9006,
            RSP_SET_OPPONENT_UNITS_ABILITIES = 9106,

            REQ_GAME_ACTION = 1007,
            RSP_GAME_ACTION = 9007,
            RSP_OPPONENT_GAME_ACTION = 9107,

            REQ_QUITPLAY = 1008,
            RSP_QUITPLAY = 9008,
            RSP_OPPONENT_QUITPLAY = 9108,
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

        public class QuickPlayRsp
        {
            public bool succ = true;
            public string opponent = "";
            public string who_first = "";

            public QuickPlayRsp(tcp.response rsp)
            {
                succ = (rsp.nextInt() == 0);
                opponent = rsp.nextString();
                who_first = rsp.nextString();
            }
        }

        public class SetCommanderRsp
        {
            public long commanderId = 0;
            public bool succ = true;
            public string info = "";

            public SetCommanderRsp(tcp.response rsp)
            {
                commanderId = rsp.nextLong();
                succ = (rsp.nextInt() == 0);
                info = rsp.nextString();
            }
        }

        public class SetOpponentCommanderRsp
        {
            public long commanderId = 0;

            public SetOpponentCommanderRsp(tcp.response rsp)
            {
                commanderId = rsp.nextLong();
            }
        }

        public class SetUnitsRsp
        {
            public int[] check = {0, 0, 0, 0, 0};
            public bool succ = true;
            public string info = "";

            public SetUnitsRsp(tcp.response rsp)
            {
                check[0] = rsp.nextInt();
                check[1] = rsp.nextInt();
                check[2] = rsp.nextInt();
                check[3] = rsp.nextInt();
                check[4] = rsp.nextInt();

                succ = (rsp.nextInt() == 0);
                info = rsp.nextString();
            }
        }

        public class SetOpponentUnitsRsp
        {
            public long[] units = { 0, 0, 0, 0, 0 };

            public SetOpponentUnitsRsp(tcp.response rsp)
            {
                units[0] = rsp.nextLong();
                units[1] = rsp.nextLong();
                units[2] = rsp.nextLong();
                units[3] = rsp.nextLong();
                units[4] = rsp.nextLong();
            }

            public override string ToString()
            {
                return String.Format("{0}, {1}, {2}, {3}, {4}",
                    units[0],
                    units[1],
                    units[2],
                    units[3],
                    units[4]);
            } 
        }

        public class SetUnitsAbilitiesRsp
        {
            public int[] check = { 0, 0, 0, 0, 0 };
            public bool succ = true;
            public string info = "";

            public SetUnitsAbilitiesRsp(tcp.response rsp)
            {
                check[0] = rsp.nextInt();
                check[1] = rsp.nextInt();
                check[2] = rsp.nextInt();
                check[3] = rsp.nextInt();
                check[4] = rsp.nextInt();

                succ = (rsp.nextInt() == 0);
                info = rsp.nextString();
            }
        }

        public class SetOpponentUnitsAbilitiesRsp
        {
            public long[] units = { 0, 0, 0, 0, 0 };
            public long[] abilities = { 0, 0, 0, 0, 0 };

            public SetOpponentUnitsAbilitiesRsp(tcp.response rsp)
            {
                units[0] = rsp.nextLong();
                abilities[0] = rsp.nextLong();

                units[1] = rsp.nextLong();
                abilities[1] = rsp.nextLong();

                units[2] = rsp.nextLong();
                abilities[2] = rsp.nextLong();

                units[3] = rsp.nextLong();
                abilities[3] = rsp.nextLong();

                units[4] = rsp.nextLong();
                abilities[4] = rsp.nextLong();
            }

            public override string ToString()
            {
                return String.Format("{0}:{1}, {2}:{3}, {4}:{5}, {6}:{7}, {8}:{9}",
                    units[0], abilities[0],
                    units[1], abilities[1],
                    units[2], abilities[2],
                    units[3], abilities[3],
                    units[4], abilities[4]);
            }
        }

        public class OpponentGameActionRsp
        {
            public long cur_unit;
            public int dst_x, dst_y;
            public long[] units = { 0, 0, 0, 0, 0 };
            public int[] damages = { 0, 0, 0, 0, 0 };

            public OpponentGameActionRsp(tcp.response rsp)
            {
                cur_unit = rsp.nextLong();
                dst_x = rsp.nextInt();
                dst_y = rsp.nextInt();

                units[0] = rsp.nextLong();
                damages[0] = rsp.nextInt();

                units[1] = rsp.nextLong();
                damages[1] = rsp.nextInt();

                units[2] = rsp.nextLong();
                damages[2] = rsp.nextInt();

                units[3] = rsp.nextLong();
                damages[3] = rsp.nextInt();

                units[4] = rsp.nextLong();
                damages[4] = rsp.nextInt();
            }

            public override string ToString()
            {
                return String.Format("{0}:{1}, {2}:{3}, {4}:{5}, {6}:{7}, {8}:{9}",
                    units[0], damages[0],
                    units[1], damages[1],
                    units[2], damages[2],
                    units[3], damages[3],
                    units[4], damages[4]);
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
                int tick = timeout > 500 ? 500 : timeout;
                while (tick > 0)
                {
                    timeout -= tick;
                    Thread.Sleep(tick);
                    tick = timeout > 500 ? 500 : timeout;

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
                        //Console.WriteLine("Raw response: cmd=[{0}], sn=[{1}].", rsp.cmd, rsp.sn);
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

                int res = rsp.nextInt();
                string msg = rsp.nextString();
                // Console.WriteLine("cmd=[{0}], sn=[{1}], result=[{2}], errmsg=[{3}]", rsp.cmd, rsp.sn, res, msg);
                return new result(res==0, msg);
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
                // Console.WriteLine("cmd=[{0}], sn=[{1}], result=[{2}], errmsg=[{3}]", rsp.cmd, rsp.sn, res, msg);
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

                int res = rsp.nextInt();
                string msg = rsp.nextString();
                // Console.WriteLine("cmd=[{0}], sn=[{1}], result=[{2}], errmsg=[{3}]", rsp.cmd, rsp.sn, res, msg);
                return new result(res == 0, msg);
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
                    return new result(false, "network error: failed to send request");
                }
                return new result();
            }

            public result setCommander(long commanderId)
            {
                if (!tc.isConnected())
                {
                    start();
                    if (!tc.isConnected()) return new result(false, "connection lost");
                }

                tcp.request req = new tcp.request((short)cmd.REQ_SET_COMMANDER);
                req.encode(commanderId);

                if (!send(req))
                {
                    return new result(false, "network error: failed to send request");
                }
                return new result();
            }

            public result setUnits(long[] units)
            {
                if (!tc.isConnected())
                {
                    start();
                    if (!tc.isConnected()) return new result(false, "connection lost");
                }

                tcp.request req = new tcp.request((short)cmd.REQ_SET_UNITS);
                req.encode(units[0]);
                req.encode(units[1]);
                req.encode(units[2]);
                req.encode(units[3]);
                req.encode(units[4]);

                if (!send(req))
                {
                    return new result(false, "network error: failed to send request");
                }
                return new result();
            }

            public result setUnitsAbilities(long[] units, long[] abilities)
            {
                if (!tc.isConnected())
                {
                    start();
                    if (!tc.isConnected()) return new result(false, "connection lost");
                }

                tcp.request req = new tcp.request((short)cmd.REQ_SET_UNITS_ABILITIES);
                req.encode(units[0]);
                req.encode(abilities[0]);
                req.encode(units[1]);
                req.encode(abilities[1]);
                req.encode(units[2]);
                req.encode(abilities[2]);
                req.encode(units[3]);
                req.encode(abilities[3]);
                req.encode(units[4]);
                req.encode(abilities[4]);

                if (!send(req))
                {
                    return new result(false, "network error: failed to send request");
                }
                return new result();
            }

            public result gameAction(long curUnit, int dstX, int dstY, long[] units, int[] damages)
            {
                if (!tc.isConnected())
                {
                    start();
                    if (!tc.isConnected()) return new result(false, "connection lost");
                }

                tcp.request req = new tcp.request((short)cmd.REQ_GAME_ACTION);

                req.encode(curUnit);
                req.encode(dstX);
                req.encode(dstY);

                req.encode(units[0]);
                req.encode(damages[0]);
                req.encode(units[1]);
                req.encode(damages[1]);
                req.encode(units[2]);
                req.encode(damages[2]);
                req.encode(units[3]);
                req.encode(damages[3]);
                req.encode(units[4]);
                req.encode(damages[4]);

                if (!send(req))
                {
                    return new result(false, "network error: failed to send request");
                }
                return new result();
            }
        }
    }
}
