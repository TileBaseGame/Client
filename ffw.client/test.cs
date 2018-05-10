using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using ffw.server;

namespace ffw
{
    // define a class to implement the abstract msghandler
    class user : msghandler
    {
        private session ss = null;
        private string who_first = "";

        private string usr = null;
        private string passwd = null;

        private long commanderId = 0;
        private long[] unitId = { 0, 0, 0, 0, 0 };
        private long[] abilityId = { 0, 0, 0, 0, 0 };

        public user(string ip, int port)
        {
            ss = new session(this, ip, port);
        }

        public void start(string usr, string passwd, long commanderId, long[] unitId, long[] abilityId)
        {
            this.usr = usr;
            this.passwd = passwd;

            this.commanderId = commanderId;

            this.unitId[0] = unitId[0];
            this.unitId[1] = unitId[1];
            this.unitId[2] = unitId[2];
            this.unitId[3] = unitId[3];
            this.unitId[4] = unitId[4];

            this.abilityId[0] = abilityId[0];
            this.abilityId[1] = abilityId[1];
            this.abilityId[2] = abilityId[2];
            this.abilityId[3] = abilityId[3];
            this.abilityId[4] = abilityId[4];

            // 1.login
            result r = ss.login(usr, passwd, 10000);
            if (!r.succ)
            {
                Console.WriteLine("ERROR: failed to login: {0}", r.info);
                return;
            }
            else
            {
                Console.WriteLine("{0} login.", usr);
            }

            // 2. quickplay
            ss.quickplay();
        }

        public void stop()
        {
            ss.logout(10000);
            ss.stop();
        }

        // the message proc
        public override void proc(tcp.response rsp)
        {
            switch (rsp.cmd)
            {
                case (short)server.cmd.RSP_QUICKPLAY: // match ok, print the opponent's name
                    {
                        QuickPlayRsp t = new QuickPlayRsp(rsp);
                        Console.WriteLine("quickplay matched, {0}'s opponent is {1}, {2} will play first.", usr, t.opponent, t.who_first);

                        who_first = t.who_first;
                        result r = ss.setCommander(commanderId);
                        if (!r.succ)
                        {
                            Console.WriteLine("ERROR: failed to set commander: {0}", r.info);
                            return;
                        }
                    }
                    break;
                case (short)server.cmd.RSP_SET_COMMANDER:
                    {
                        SetCommanderRsp t = new SetCommanderRsp(rsp);
                        if (t.succ)
                        {
                            Console.WriteLine("{0} selects commander {1}.", usr, t.commanderId);

                            // after commander selected, select units
                            result r = ss.setUnits(unitId);
                            if (!r.succ)
                            {
                                Console.WriteLine("ERROR: failed to set units: {0}", r.info);
                                return;
                            }
                        }
                        else
                        {
                            Console.WriteLine("{0} fails to select commander {1}, error=[{2}].", usr, t.commanderId, t.info);
                        }
                    }
                    break;
                case (short)server.cmd.RSP_SET_UNITS:
                    {
                        SetUnitsRsp t = new SetUnitsRsp(rsp);
                        if (t.succ)
                        {
                            Console.WriteLine("{0} selects units({1}, {2}, {3}, {4}, {5}).", usr,
                                unitId[0],
                                unitId[1],
                                unitId[2],
                                unitId[3],
                                unitId[4]);

                            // after units selected, select units' abilities
                            result r = ss.setUnitsAbilities(unitId, abilityId);
                            if (!r.succ)
                            {
                                Console.WriteLine("ERROR: failed to set units' abilities: {0}", r.info);
                                return;
                            }
                        }
                        else
                        {
                            Console.WriteLine("{0} fails to select units({1}:{2}, {3}:{4}, {5}:{6}, {7}:{8}, {9}:{10}), error=[{11}].", usr, 
                                unitId[0], t.check[0],
                                unitId[1], t.check[1],
                                unitId[2], t.check[2],
                                unitId[3], t.check[3],
                                unitId[4], t.check[4],
                                t.info);
                        }
                    }
                    break;
                case (short)server.cmd.RSP_SET_UNITS_ABILITIES:
                    {
                        SetUnitsAbilitiesRsp t = new SetUnitsAbilitiesRsp(rsp);
                        if (t.succ)
                        {
                            Console.WriteLine("{0} selects units' abilities({1}:{2}, {3}:{4}, {5}:{6}, {7}:{8}, {9}:{10}).", 
                                usr,
                                unitId[0], abilityId[0],
                                unitId[1], abilityId[1],
                                unitId[2], abilityId[2],
                                unitId[3], abilityId[3],
                                unitId[4], abilityId[4]);

                            if (usr.Equals(who_first))
                            {
                                result r = ss.gameAction(1, 10, 12, new long[] { 1, 2, 3, 4, 5 }, new int[] { 1, 1, 2, 2, 4 });
                                if (!r.succ)
                                {
                                    Console.WriteLine("ERROR: failed to move unit: {0}", r.info);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("{0} fails to select units' abilities({1}:{2}:{3}, {4}:{5}:{6}, {7}:{8}:{9}, {10}:{11}:{12}, {13}:{14}:{15}), error=[{16}].",
                                usr,
                                unitId[0], abilityId[0], t.check[0],
                                unitId[1], abilityId[1], t.check[1],
                                unitId[2], abilityId[2], t.check[2],
                                unitId[3], abilityId[3], t.check[3],
                                unitId[4], abilityId[4], t.check[4], t.info);

                        }
                    }
                    break;
                case (short)server.cmd.RSP_SET_OPPONENT_COMMANDER:
                    {
                        SetOpponentCommanderRsp t = new SetOpponentCommanderRsp(rsp);
                        Console.WriteLine("{0}'s opponent selects commander {1}.", usr, t.commanderId);
                    }
                    break;
                case (short)server.cmd.RSP_SET_OPPONENT_UNITS:
                    {
                        SetOpponentUnitsRsp t = new SetOpponentUnitsRsp(rsp);
                        Console.WriteLine("{0}'s opponent selects units({1}).", usr, t.ToString());
                    }
                    break;
                case (short)server.cmd.RSP_SET_OPPONENT_UNITS_ABILITIES:
                    {
                        SetOpponentUnitsAbilitiesRsp t = new SetOpponentUnitsAbilitiesRsp(rsp);
                        Console.WriteLine("{0}'s opponent selects units' ablities({1}).", usr, t.ToString());
                    }
                    break;
                case (short)server.cmd.RSP_OPPONENT_GAME_ACTION:
                    {
                        OpponentGameActionRsp t = new OpponentGameActionRsp(rsp);
                        Console.WriteLine("{0}'s opponent move unit({1}) to ({2}, {3}), damages=({4}).", usr, t.cur_unit, t.dst_x, t.dst_y, t.ToString());
                    }
                    break;
            }
        }
    }
    
    // the main test entry
    class test
    {
        static void Main(string[] args)
        {
            user u1 = new user("127.0.0.1", 9252);
            user u2 = new user("127.0.0.1", 9252);

            u1.start("lily", "123456", 1, new long[] { 1, 2, 3, 2, 1 }, new long[]{1, 8, 13, 8, 1});
            u2.start("goodman", "good", 2, new long[] { 2, 2, 3, 3, 1 }, new long[] { 10, 10, 15, 15, 2 });

            // pause for a while, both logout
            Thread.Sleep(10000);

            u1.stop();
            u2.stop();
        }
    }
}
