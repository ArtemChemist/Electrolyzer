#region Namespace Inclusions
using System;
using System.Linq;
using System.Data;
using System.Text;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
//using SerialPortTerminal.Properties;
using System.Threading;
using System.IO;
#endregion

namespace RadElChemBox
{
    class K501_Pump
    {
        
        #region Constructor
        public K501_Pump()
        {
        }
        #endregion


        // The main control for communicating through the RS-232 port
        private SerialPort comport = new SerialPort();


        string PortName;
        double dblHPLC_Pump_Flow; //in microliters

        public bool OpenPort()
        {
            bool succeeded = true;

            // If the port is open, close it.
            if (comport.IsOpen)
            {
                comport.Close();
                comport.Dispose();
                comport = new SerialPort();
            }

            // Set the port's settings
            comport.BaudRate = 9600;
            comport.DataBits = 8;
            comport.StopBits = StopBits.One;
            comport.Parity = Parity.None;
            comport.PortName = PortName;

            try
            {
               // Open the port
               comport.Open();
            }
            catch (UnauthorizedAccessException) 
            { 
                succeeded = false;
            }
            catch (IOException) { succeeded = false; }
            catch (ArgumentException) { succeeded = false; }

            if (!succeeded) MessageBox.Show("Could not open the Pump COM port. Is it already in use?", "COM Port Unavalible", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            
            return succeeded;
        }
        
        public bool InitializePump(string NameOfThePort)
        {
            PortName = NameOfThePort;
            if (OpenPort())
            {
                comport.Write("SO\r\n");
                return true;
            }
            else { return false; }
        }

        
        public double Flow

        {
            set
            {
                dblHPLC_Pump_Flow = value;
                comport.Write("F" + dblHPLC_Pump_Flow.ToString() + "\r\n");
                Thread.Sleep(100);
            }

            get
            {
                return dblHPLC_Pump_Flow;
            }
            
        }
        public void Stop()
        {
            comport.Write( "M0\r\n");
            Thread.Sleep(100);
        }

        public void Start()
        {
            comport.Write("M1\r\n");
            Thread.Sleep(100);
        }

        public string HPLCPumpStatus()
        {
            comport.ReadExisting();
            comport.Write("S?\r\n");
            Thread.Sleep(100);

            string buffer = comport.ReadExisting();

            char[] Characters = buffer.ToCharArray();
            for (int i = 0; i < Characters.Length; i++)
            {
                if ((Characters[i] == 'P') || (Characters[i] == 'p'))
                {
                    if (Characters[i+1] == 2) { return "Off"; }
                    if (Characters[i + 1] == 1) { return "Error"; }
                    if (Characters[i] == 'P') { return "Off"; }
                    if (Characters[i] == 'p') { return "On"; }
                }
            }

            return "Error";

        }
        
        public double CurrentPressure()
        {
            double ReturnValue;
            string TempValue="";
            comport.ReadExisting();
            comport.Write("P?\r\n");
            Thread.Sleep(200);
            string buffer = comport.ReadExisting();
            char[] TempString = buffer.ToCharArray();
            for (int i = 1; i < 7; i++)
            {
                TempValue = TempValue + TempString[i];
            }
            Double.TryParse(TempValue, out ReturnValue);
            return ReturnValue;
        }
    }
}
