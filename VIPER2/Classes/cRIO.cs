using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace VIPER2
{
    static class cRIO
    {
        private const string socketName = "dataclient";
        private static byte[] bytes = new byte[1024];
        private static Timer tmrConnection = new Timer(), tmrSend = new Timer();

        public enum RIO_STATE { UNKNOWN, CONNECTING, CONNECTED, DISCONNECTED };
        public static RIO_STATE currentState = RIO_STATE.UNKNOWN;

        public static void init()
        {
            SEALib.TCP.addClient(socketName, SEALib.Configuration.GetString("crio_config", "ipaddress"), int.Parse(SEALib.Configuration.GetString("crio_config", "dataport")), onConnect, onDisconnect, onReceive, bytes.Length);
            SEALib.TCP.enableHeartbeat(socketName, 3, 50);
            tmrConnection.Interval = 100;
            tmrConnection.Elapsed += new ElapsedEventHandler(tmrConnection_Elapsed);
            tmrConnection.Start();
            tmrSend.Interval = 80;
            tmrSend.Elapsed += new ElapsedEventHandler(tmrSend_Elapsed);
            tmrSend.Start();
        }

        static void tmrSend_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (SEALib.TCP.isConnected(socketName))
                SEALib.TCP.startSend(socketName, onSend, Encoding.UTF8.GetBytes("heartbeat"));
        }
        public static string getBufferedSends()
        {
            return SEALib.TCP.getBufferedSends(socketName).ToString();
        }
        static void tmrConnection_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (SEALib.TCP.isConnected(socketName))
                currentState = RIO_STATE.CONNECTED;
            else if (SEALib.TCP.isConnecting(socketName))
                currentState = RIO_STATE.CONNECTING;
            else
            {
                currentState = RIO_STATE.DISCONNECTED;
                SEALib.TCP.startConnecting(socketName, 100);
            }
        }

        private static void onConnect(string name)
        {
        }
        private static void onDisconnect(string name)
        {
        }
        private static void onSend(string name)
        {
        }
        private static void onReceive(string name, byte[] bytesRec, int numBytesRec)
        {
        }
    }
}
