using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace VIPER2
{
    static class cRIO
    {
        private static byte[] bytes = new byte[1024];
        private static Timer tmrConnection = new Timer(), tmrSend = new Timer();

        public enum RIO_STATE { UNKNOWN, CONNECTING, CONNECTED, DISCONNECTED };
        public static RIO_STATE currentState = RIO_STATE.UNKNOWN;

        private static SEALib.TCP.SOCKET client = new SEALib.TCP.SOCKET();
        public static int sends = 0;

        public static void init()
        {
            client.initClient(SEALib.Configuration.GetString("crio_config", "ipaddress"), int.Parse(SEALib.Configuration.GetString("crio_config", "dataport")), onConnect, onDisconnect, onReceive, bytes.Length);
            client.enableHeartbeat(500, onHeartbeatTimeout);
            tmrConnection.Interval = 100;
            tmrConnection.Elapsed += new ElapsedEventHandler(tmrConnection_Elapsed);
            tmrConnection.Start();
            tmrSend.Interval = 30;
            tmrSend.Elapsed += new ElapsedEventHandler(tmrSend_Elapsed);
            tmrSend.Start();
            client.startConnecting(100, onTimeout);
        }

        static void tmrSend_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (client.isConnected)
                client.startSend(onSend, Encoding.UTF8.GetBytes("####"+sends.ToString()+"%%%%"));
        }
        public static string getBufferedSends()
        {
            return client.bufferedSends.ToString();
        }
        static void tmrConnection_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (client.isConnected)
                currentState = RIO_STATE.CONNECTED;
            else if (client.isConnecting)
                currentState = RIO_STATE.CONNECTING;
            else
            {
                currentState = RIO_STATE.DISCONNECTED;
                client.startConnecting(100, onTimeout);
            }
        }

        private static void onConnect()
        {
            SEALib.Logging.Write(" c", false);
        }
        private static void onDisconnect()
        {
            SEALib.Logging.Write(" dc", false);
        }
        private static void onSend()
        {
            //SEALib.Logging.Write("sent");
            sends++;
        }
        private static void onReceive(byte[] bytesRec, int numBytesRec)
        {
            SEALib.Logging.Write(" r: "+numBytesRec.ToString(), false);
        }
        private static void onHeartbeatTimeout()
        {
            SEALib.Logging.Write(" hbto", false);
        }
        private static void onTimeout()
        {
            SEALib.Logging.Write(" to", false);
        }
    }
}
