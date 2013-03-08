/*
 * Author:      Dave Manley
 * Date:        01/14/2013
 * Description: RIO - Library for managing RIO communication & data
 * Dependencies: Configuration, TCP
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;



namespace VIPER2
{
    public static class RIO
    {
        
        public static Double lastCmd1, lastCmd2, lastCmd3, lastCmd4;
        public static int moveCount = 0;
        public static String errorName = "Controller";
        //RIO COMMANDS
        public enum COMMAND {MOVE, SENDCFG, CLEARERRORS, SETCANNODE, DEBUGMODE, EMPTY, STOP, LOCKON, LOCKOFF, HSMON, HSMOFF};
        //RIO STATES
        public static STATE currentState;
        public enum STATE {DISCONNECTED, WAITING, READING_CFG, MOVING, DEBUG, CAN_INIT, SET_CAN_NODE};
        //PARAMETERS READ FROM RIO
        public enum PARAMETER_NAME {
            INCLINOMETER, VEHICLEMOTION1, VEHICLEMOTION2, SPARE1, CPLOADCELL, PDTRANSDUCER, SPARE2, SPARE3, 
            CANCYL1, CANCYL2, CANCYL3, CANCYL4,
            CANSTR1, CANSTR2, CANSTR3, CANSTR4,
            SWITCH, DT, STATUS, 
            ERROR1, ERROR2, ERROR3, ERROR4, ERROR5
    };
        public struct PARAMETER {
            public Double processed;
            public UInt64 uint64;
            //public byte[] raw;
        };
        public static Dictionary<PARAMETER_NAME, PARAMETER> parameters = new Dictionary<PARAMETER_NAME,PARAMETER>();
        public static PARAMETER getParameter(PARAMETER_NAME name)
        {
            if (parameters.ContainsKey(name))
                return parameters[name];
            return new PARAMETER();
        }
        //SET INCOMING PARAMETERS
        public static void setParameters(byte[] data)
        {
            int index = 0;
            for (int i = 0; i < data.Length; )
            {
                byte[] subdata = new byte[8];//64-bit parameters
                for (int j = 0; j < 8; j++)
                {
                    subdata[j] = data[i];
                    i++;
                }
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(subdata);
                PARAMETER p = new PARAMETER();
                //raw values for debug
                //p.raw = new byte[8];
                //Array.Copy(subdata, p.raw, 8);
                p.uint64 = BitConverter.ToUInt64(subdata, 0);
                p.processed = BitConverter.ToDouble(subdata, 0);

                if (parameters.ContainsKey((PARAMETER_NAME)index))
                    parameters[((PARAMETER_NAME)index)] = p;
                else
                    parameters.Add(((PARAMETER_NAME)index), p);
                index++;
            }
        }
        //SEND COMMAND TO RIO
        public static void sendCommand(RIO.COMMAND command, params Double[] dbls)
        {
            /*if (currentState == STATE.DISCONNECTED)
            {
                ErrorMsg.ThrowError("Can't send command: Valve controller is disconnected.", "Valve Control Warning", ErrorMsg.MsgLevel.warning, null);
                return;
            }
            if (currentState != STATE.WAITING && command != COMMAND.STOP && command != COMMAND.EMPTY && command != COMMAND.LOCKOFF1 && command != COMMAND.LOCKOFF2 && command != COMMAND.LOCKON1 && command != COMMAND.LOCKON2 && command != COMMAND.CLEARERRORS)
            {
                ErrorMsg.ThrowError("Can't send command ["+command.ToString()+"]: Valve controller is unable to accept commands in this state ["+currentState.ToString()+"].", "Valve Control Warning", ErrorMsg.MsgLevel.warning, null);
                return;
            }
            String cmd = "";
            switch (command)
            {
                case COMMAND.MOVE:
                    cmd = "####move";
                    moveCount++;
                    lastCmd1 = dbls[0];
                    lastCmd2 = dbls[1];
                    lastCmd3 = dbls[2];
                    lastCmd4 = dbls[3];
                    break;
                case COMMAND.SENDCFG:
                    cmd = "#sendcfg";
                    break;
                case COMMAND.CLEARERRORS:
                    cmd = "#clrerrs";
                    break;
                case COMMAND.SETCANNODE:
                    cmd = "#cannode";
                    break;
                case COMMAND.DEBUGMODE:
                    cmd = "openloop";
                    break;
                case COMMAND.STOP:
                    cmd = "####stop";
                    break;
                case COMMAND.LOCKON:
                    cmd = "#lockon";
                    break;
                case COMMAND.LOCKOFF:
                    cmd = "lockoff";
                    break;
                case COMMAND.EMPTY:
                    cmd = "";
                    break;
                case COMMAND.HSMOFF:
                    cmd = "##hsmoff";
                    break;
                case COMMAND.HSMON:
                    cmd = "###hsmon";
                    break;
                default:
                    ErrorMsg.ThrowError("Command not found.", "Valve Control Error", ErrorMsg.MsgLevel.critical, null);
                    break;
            }
            byte[] transmission = new byte[cmd.Length + dbls.Length*8];
            int byteIndex;
            for (byteIndex = 0; byteIndex < cmd.Length; byteIndex++)
                transmission[byteIndex] = (byte)cmd[byteIndex];
            for (int dblIndex = 0; dblIndex < dbls.Length; dblIndex++)
            {
                BitConverter.GetBytes(dbls[dblIndex]).CopyTo(transmission,byteIndex);
                byteIndex += 8;
            }
            TCP.send(transmission);
        }
        public static void sendConfig()
        {
            Double[] dbls = new Double[32];
            for (int i = 0; i < 32; i++)
            {
                Double db = 0;
                if (i==0)
                    db = 2e99;
                if (i==1)
                    db = 2e-99;
                if (i == 2)
                    db = -2e99;
                if (i == 3)
                    db = -2e-99;
                dbls[i] = db;
                Log.Write(i.ToString() + ": " + db.ToString());
            }

            /*Int16[] ints = new Int16[89];
            ints[0] = Int16.Parse(Configuration.get("v1t1p", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[1] = Int16.Parse(Configuration.get("v2t1p", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[2] = Int16.Parse(Configuration.get("v3t1p", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[3] = Int16.Parse(Configuration.get("v4t1p", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[4] = Int16.Parse(Configuration.get("v1t2p", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[5] = Int16.Parse(Configuration.get("v2t2p", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[6] = Int16.Parse(Configuration.get("v3t2p", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[7] = Int16.Parse(Configuration.get("v4t2p", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[8] = Int16.Parse(Configuration.get("v1t1n", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[9] = Int16.Parse(Configuration.get("v2t1n", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[10] = Int16.Parse(Configuration.get("v3t1n", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[11] = Int16.Parse(Configuration.get("v4t1n", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[12] = Int16.Parse(Configuration.get("v1t2n", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[13] = Int16.Parse(Configuration.get("v2t2n", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[14] = Int16.Parse(Configuration.get("v3t2n", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[15] = Int16.Parse(Configuration.get("v4t2n", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[16] = Int16.Parse(Configuration.get("post1", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[17] = Int16.Parse(Configuration.get("post2", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[18] = timesMillion(Double.Parse(Configuration.get("AI0F_CH1_MOD4_SCALE", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[19] = timesMillion(Double.Parse(Configuration.get("AI1F_CH1_MOD4_SCALE", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[20] = timesMillion(Double.Parse(Configuration.get("AI0F_CH2_MOD4_SCALE", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[21] = timesMillion(Double.Parse(Configuration.get("AI1F_CH2_MOD4_SCALE", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[22] = timesThousand(Double.Parse(Configuration.get("AI0F_CH1_MOD3_SCALE", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[23] = timesThousand(Double.Parse(Configuration.get("AI1F_CH1_MOD3_SCALE", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[24] = timesThousand(Double.Parse(Configuration.get("AI0F_CH2_MOD3_SCALE", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[25] = timesThousand(Double.Parse(Configuration.get("AI1F_CH2_MOD3_SCALE", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[26] = (Int16)(Double.Parse(Configuration.get("AI0F_CH1_MOD3_OFFSET", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[27] = (Int16)(Double.Parse(Configuration.get("AI1F_CH1_MOD3_OFFSET", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[28] = (Int16)(Double.Parse(Configuration.get("AI0F_CH2_MOD3_OFFSET", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[29] = (Int16)(Double.Parse(Configuration.get("AI1F_CH2_MOD3_OFFSET", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[30] = divideTen(Double.Parse(Configuration.get("lcell1limit", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString()));
            ints[31] = divideTen(Double.Parse(Configuration.get("lcell2limit", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString()));
            ints[32] = timesThousand(Double.Parse(Configuration.get("filtc1", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString()));
            ints[33] = timesThousand(Double.Parse(Configuration.get("filtc2", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString()));
            ints[34] = timesThousand(Double.Parse(Configuration.get("filtc3", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString()));
            ints[35] = timesThousand(Double.Parse(Configuration.get("filtc4", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString()));
            ints[36] = Int16.Parse(Configuration.get("timeout", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[37] = 0;
            ints[38] = 0;
            ints[39] = 0;
            ints[40] = timesThousand(Double.Parse(Configuration.get("AI0F_CH1_MOD1_SCALE", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[41] = timesThousand(Double.Parse(Configuration.get("AI1F_CH1_MOD1_SCALE", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[42] = timesThousand(Double.Parse(Configuration.get("AI2F_CH1_MOD1_SCALE", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[43] = timesThousand(Double.Parse(Configuration.get("AI3F_CH1_MOD1_SCALE", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[44] = timesThousand(Double.Parse(Configuration.get("AI0F_CH1_MOD2_SCALE", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[45] = timesThousand(Double.Parse(Configuration.get("AI1F_CH1_MOD2_SCALE", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[46] = timesThousand(Double.Parse(Configuration.get("AI2F_CH1_MOD2_SCALE", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[47] = timesThousand(Double.Parse(Configuration.get("AI3F_CH1_MOD2_SCALE", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[48] = timesThousand(Double.Parse(Configuration.get("AI0F_CH2_MOD1_SCALE", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[49] = timesThousand(Double.Parse(Configuration.get("AI1F_CH2_MOD1_SCALE", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[50] = timesThousand(Double.Parse(Configuration.get("AI2F_CH2_MOD1_SCALE", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[51] = timesThousand(Double.Parse(Configuration.get("AI3F_CH2_MOD1_SCALE", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[52] = timesThousand(Double.Parse(Configuration.get("AI0F_CH2_MOD2_SCALE", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[53] = timesThousand(Double.Parse(Configuration.get("AI1F_CH2_MOD2_SCALE", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[54] = timesThousand(Double.Parse(Configuration.get("AI2F_CH2_MOD2_SCALE", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[55] = timesThousand(Double.Parse(Configuration.get("AI3F_CH2_MOD2_SCALE", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[56] = (Int16)(Double.Parse(Configuration.get("AI0F_CH1_MOD1_OFFSET", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[57] = (Int16)(Double.Parse(Configuration.get("AI1F_CH1_MOD1_OFFSET", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[58] = (Int16)(Double.Parse(Configuration.get("AI2F_CH1_MOD1_OFFSET", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[59] = (Int16)(Double.Parse(Configuration.get("AI3F_CH1_MOD1_OFFSET", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[60] = (Int16)(Double.Parse(Configuration.get("AI0F_CH1_MOD2_OFFSET", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[61] = (Int16)(Double.Parse(Configuration.get("AI1F_CH1_MOD2_OFFSET", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[62] = (Int16)(Double.Parse(Configuration.get("AI2F_CH1_MOD2_OFFSET", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[63] = (Int16)(Double.Parse(Configuration.get("AI3F_CH1_MOD2_OFFSET", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[64] = (Int16)(Double.Parse(Configuration.get("AI0F_CH2_MOD1_OFFSET", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[65] = (Int16)(Double.Parse(Configuration.get("AI1F_CH2_MOD1_OFFSET", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[66] = (Int16)(Double.Parse(Configuration.get("AI2F_CH2_MOD1_OFFSET", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[67] = (Int16)(Double.Parse(Configuration.get("AI3F_CH2_MOD1_OFFSET", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[68] = (Int16)(Double.Parse(Configuration.get("AI0F_CH2_MOD2_OFFSET", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[69] = (Int16)(Double.Parse(Configuration.get("AI1F_CH2_MOD2_OFFSET", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[70] = (Int16)(Double.Parse(Configuration.get("AI2F_CH2_MOD2_OFFSET", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[71] = (Int16)(Double.Parse(Configuration.get("AI3F_CH2_MOD2_OFFSET", Configuration.DictionaryName.VIPER2RawToEngg)[0].ToString()));
            ints[72] = Int16.Parse(Configuration.get("cannode1", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[73] = Int16.Parse(Configuration.get("cannode2", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[74] = Int16.Parse(Configuration.get("cannode3", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[75] = Int16.Parse(Configuration.get("cannode4", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[76] = Int16.Parse(Configuration.get("cannode5", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[77] = Int16.Parse(Configuration.get("cannode6", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[78] = Int16.Parse(Configuration.get("cannode7", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[79] = Int16.Parse(Configuration.get("cannode8", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[80] = Int16.Parse(Configuration.get("cannode9", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[81] = Int16.Parse(Configuration.get("cannode10", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[82] = Int16.Parse(Configuration.get("cannode11", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[83] = Int16.Parse(Configuration.get("cannode12", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[84] = Int16.Parse(Configuration.get("cannode13", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[85] = Int16.Parse(Configuration.get("cannode14", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[86] = Int16.Parse(Configuration.get("cannode15", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[87] = Int16.Parse(Configuration.get("cannode16", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            ints[88] = Int16.Parse(Configuration.get("valverate", Configuration.DictionaryName.VIPER2RioConfig)[0].ToString());
            sendCommand(COMMAND.SENDCFG, ints);
            sendCommand(COMMAND.SENDCFG, dbls);*/
        }
        
        public static void downloadFile()
        {
            /*String inputfilepath = @"C:\VIPER2 Data\Viper2.dat";
            string ftphost = Configuration.get("ipaddress", Configuration.DictionaryName.RioConfig)[0].ToString();
            string ftpfilepath = Configuration.get("datafile", Configuration.DictionaryName.RioConfig)[0].ToString();

            string ftpfullpath = "ftp://" + ftphost + "/" + ftpfilepath;

            using (WebClient request = new WebClient())
            {
                //request.Credentials = new NetworkCredential("UserName", "P@55w0rd");
                byte[] fileData = request.DownloadData(ftpfullpath);

                FileStream file = File.Create(inputfilepath);
                file.Write(fileData, 0, fileData.Length);
                file.Close();
            }*/
        }

         public static Boolean cfgErrorsExist()
         {
             /*UInt16 errors1 = (UInt16)RIO.getParameter(RIO.PARAMETER_NAME.ERROR1).raw;
             UInt16 errors2 = (UInt16)RIO.getParameter(RIO.PARAMETER_NAME.ERROR2).raw;
             UInt16 errors3 = (UInt16)RIO.getParameter(RIO.PARAMETER_NAME.ERROR3).raw;
             UInt16 errors4 = (UInt16)RIO.getParameter(RIO.PARAMETER_NAME.ERROR4).raw;
             UInt16 errors5 = (UInt16)RIO.getParameter(RIO.PARAMETER_NAME.ERROR5).raw;
             return ((errors1 + errors2 + errors3 + errors4 + errors5) > 0);*/
             return false;
         }
    }
}
