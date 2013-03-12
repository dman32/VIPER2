using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace VIPER2
{
    static class ArrayExtensions
    {

        public static IEnumerable<int> StartingIndex(this byte[] x, byte[] y, int start)
        {
            IEnumerable<int> index = Enumerable.Range(start, x.Length - y.Length + 1);
            for (int i = 0; i < y.Length; i++)
            {
                index = index.Where(n => x[n + i] == y[i]).ToArray();
            }
            return index;
        }

    }
    static class cRIO
    {
        private static byte[] bytes = new byte[1024];
        public static Queue<byte> buffer = new Queue<byte>(), values = new Queue<byte>();
        private static Timer tmrConnection = new Timer(), tmrSend = new Timer();

        public enum RIO_STATE { UNKNOWN, CONNECTING, CONNECTED, DISCONNECTED };
        public static RIO_STATE currentState = RIO_STATE.UNKNOWN;
        public enum PARAMETER_NAME
        {
            TIME, LOADCELL1, LOADCELL2, PRESSURE_DIFF, SPARE1, VEHICLE_MOTION1,
            VEHICLE_MOTION2, INCLINOMETER, VALVE5,
            SWITCH1, SWITCH2, CAN_CYL1, CAN_CYL2, CAN_CYL3, CAN_CYL4,
            ENCODER1, ENCODER2, CAN_AI1, CAN_AI2, CAN_AI3, CAN_AI4,
            CAN_DI1, VALVE1, VALVE2, VALVE3, VALVE4
        };
        public class PARAMETER
        {
            public double dvalue;
            public UInt64 uvalue;
            public PARAMETER(double value)
            {
                dvalue = value;
                uvalue = 0;
            }
        };
        public static Dictionary<PARAMETER_NAME, PARAMETER> parameters = new Dictionary<PARAMETER_NAME, PARAMETER>();
        private static System.Threading.ManualResetEvent mre = new System.Threading.ManualResetEvent(true);
        private static SEALib.TCP.SOCKET client = new SEALib.TCP.SOCKET();
        private const int frameSize = 26, parameterSize = 8;
        private const string startInd = "#VIPER2#", endInd = "%VIPER2%";
        private static byte[] startByteAr = Encoding.UTF8.GetBytes(startInd);
        private static byte[] endByteAr = Encoding.UTF8.GetBytes(endInd);

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
            foreach (PARAMETER_NAME p in Enum.GetValues(typeof(PARAMETER_NAME)))
                if (!parameters.ContainsKey(p))
                    parameters.Add(p, new PARAMETER(-1));
        }

        private static void parse()
        {
           // try
            {
                byte[] currentBuffer = buffer.ToArray();
                int start = currentBuffer.StartingIndex(startByteAr, 0).First();
                if (start >= 0)
                {
                    int end = currentBuffer.StartingIndex(endByteAr, start).First();
                    if (end >= 0)
                    {
                        for (int i = 0; i < start + startInd.Length; i++)
                            buffer.Dequeue();
                        for (int i = 0;i < end - start - startInd.Length;i++)
                            values.Enqueue(buffer.Dequeue());
                        for (int i=0;i < endInd.Length;i++)
                            buffer.Dequeue();
                    }
                } 
            }
           // catch { }
        }
        private static void refreshValues()
        {
            while (values.Count > frameSize * parameterSize)
                for (int i = 0; i < frameSize; i++)
                {
                    byte[] subdata = new byte[parameterSize];
                    for (int j = 0; j < parameterSize; j++)
                        subdata[j] = values.Dequeue();
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(subdata);
                    if (parameters.ContainsKey((PARAMETER_NAME)i))
                    {
                        parameters[(PARAMETER_NAME)i].dvalue = BitConverter.ToDouble(subdata, 0);
                        parameters[(PARAMETER_NAME)i].uvalue = BitConverter.ToUInt64(subdata, 0);

                    }
                }
        }

        private static void tmrSend_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (client.isConnected)
            {
                client.startSend(onSend, Encoding.UTF8.GetBytes(startInd + "test" + endInd));
            }
        }
        private static void tmrConnection_Elapsed(object sender, ElapsedEventArgs e)
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
        //CALLBACKS
        private static void onConnect()
        {
            SEALib.Logging.Write("Connect", true);
        }
        private static void onDisconnect()
        {
            SEALib.Logging.Write("Disconnected", true);
        }
        private static void onSend()
        {
        }
        private static void onReceive(byte[] bytesRec, int numBytesRec)
        {
            mre.WaitOne();
            mre.Reset();
            for (int i = 0; i < numBytesRec; i++)
                buffer.Enqueue(bytesRec[i]);
            parse();
            refreshValues();
            mre.Set();
        }
        private static void onHeartbeatTimeout()
        {
            SEALib.Logging.Write("Heartbeat Timeout", true);
        }
        private static void onTimeout()
        {
            SEALib.Logging.Write("Timeout", true);
        }
    }
}
