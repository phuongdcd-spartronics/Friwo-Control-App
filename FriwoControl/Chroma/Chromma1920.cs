using System;
using System.Threading;
using VisaComLib;

namespace FriwoControl.Chroma
{
    public class Chroma_1920
    {
        public static FormattedIO488 Chromma_1920_4 = null;
        public static string IDN;
        public static string Read_String;
        public string Query;
        public static string Error_status_Chroma { set; get; }
        /// <summary>
        ///   Adress_Chroma_1920_4 = "ASRL11::INSTR", choose COM11
        /// </summary>
        /// <returns></returns>
        public bool Init()
        {
            try
            {
                //create the formatted io object
                Chromma_1920_4 = new FormattedIO488();
                //create the resource manager
                ResourceManager mgr = new ResourceManager();
                // Get address and type (GPIB or ASRL), and do an initial check
                //string addr = Addr;
                // Try to open the device
                string optionString = "";
                Chromma_1920_4.IO = (IMessage)mgr.Open(Query, AccessMode.NO_LOCK, 2000, optionString);
                ISerial seri = (ISerial)Chromma_1920_4.IO;
                seri.BaudRate = 9600;
                seri.StopBits = SerialStopBits.ASRL_STOP_ONE;
                seri.Parity = SerialParity.ASRL_PAR_NONE;
                //seri.StopBits = 1;
                Chromma_1920_4.IO.Timeout = 3000;
                Chromma_1920_4.IO.Clear();
                // Check and make sure the correct instrument is addressed
                Chromma_1920_4.WriteString("*RST", true);
                Chromma_1920_4.WriteString("*IDN?", true);
                IDN = Chromma_1920_4.ReadString();
                // Switch the instrument in remote mode
                Chromma_1920_4.WriteString("SYST:REM", true);
                //write log
            }
            catch (Exception e)
            {
                Chromma_1920_4 = null;
                IDN = "";
                return false;
            }
            return true;
        }
        public bool GetId(int comport)
        {
            try
            {
                Query = "ASRL" + comport.ToString() + "::INSTR";
                if (Chromma_1920_4 != null)
                {
                    return true;
                }
                if (Init())
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        /// <summary>
        /// PASS if resull string 116
        /// </summary>
        /// <param name="Chanle"></param>
        /// <returns></returns>
        public Read_result Read_Result(int Chanle)
        {
            Read_result read_Result = new Read_result();
            if (Chromma_1920_4 == null)
            {
                read_Result.status = false;
            }
            try
            {
                string feedback = "";
                string mgs = "";
                bool check = true;
                while (check)
                {
                    Thread.Sleep(300);
                    mgs = "SAF:STAT?";
                    Chromma_1920_4.WriteString(mgs, true);
                    feedback = Chromma_1920_4.ReadString().Trim();
                    if (feedback == "STOPPED") check = false;
                    else check = true;
                }
                mgs = "SAF:CHAN00" + Chanle.ToString() + ":RES:ALL?";
                Chromma_1920_4.WriteString(mgs, true);
                feedback = Chromma_1920_4.ReadString().Trim();
                if (feedback == "116") read_Result.result = "PASSED";
                else read_Result.result = "FAILED";
                mgs = "SAF:CHAN00" + Chanle.ToString() + ":RES:ALL:MMET?";
                Chromma_1920_4.WriteString(mgs, true);
                read_Result.Current = Chromma_1920_4.ReadString().Trim();
                double current_d = Convert.ToDouble(read_Result.Current) * 1000;
                read_Result.Current = current_d.ToString() + "mA";
                read_Result.status = true;
            }
            catch
            {
                read_Result.status = false;
            }
            return read_Result;
        }
        public class Read_result
        {
            public string result { set; get; }
            public string Current { set; get; }
            public bool status { set; get; }
        }
        public bool Run()
        {
            if (Chromma_1920_4 == null)
            {
                return false;
            }
            try
            {
                string mgs = "SAF:STAR";
                Chromma_1920_4.WriteString(mgs, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
