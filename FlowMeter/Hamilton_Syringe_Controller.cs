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
    class Hamilton_Syringe_Controller
        {
    #region Local Variables

        // The main control for communicating through the RS-232 port
        private SerialPort comport = new SerialPort();
        private List<byte> ReadingBuffer = new List<byte>();
  

        string PortName;
        //public event EventHandler<EventArgs> SyringeIsBusy;
        
        #endregion

        #region Constructor
        public Hamilton_Syringe_Controller(string ThePortName)
        {
            PortName = ThePortName;
        }
        #endregion

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
            comport.DataBits = 7;
            comport.StopBits = StopBits.One;
            comport.Parity = Parity.Odd;
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

            if (!succeeded) MessageBox.Show("Could not open the Syringe COM port. Is it already in use?", "COM Port Unavalible", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            
            return succeeded;
        }
        
        public bool Aspirate(string Pump, int steps, int speed)
        {
            if (comport.IsOpen)
            {
                while (PumpIsBusy(Pump))
                { Thread.Sleep(100); }

                comport.Write(Pump + "P" + steps.ToString() + "S" + speed.ToString() + "R\r\n");
                Thread.Sleep(100 + speed * steps);

                while (PumpIsBusy(Pump))
                { Thread.Sleep(50); }
                return true;
            }
            else
            {
                MessageBox.Show("Syringe comport is not open.", "Hamilton Pump " + Pump);
                return false;
            }
        }

        public bool Dispense(string Pump, int steps, int speed)
        {
            if (comport.IsOpen)
            {
                while (PumpIsBusy(Pump))
                { Thread.Sleep(100); }

                comport.Write(Pump + "D" + steps.ToString() + "S" + speed.ToString() + "R\r\n");
                Thread.Sleep(100 + speed * steps);

                while (PumpIsBusy(Pump))
                { Thread.Sleep(50); }
                return true;
            }
            else
            {
                MessageBox.Show("Syringe comport is not open.", "Hamilton Pump " + Pump);
                return false;
            }
        }

        public bool PumpIsBusy(string Pump)
        {

            if(!comport.IsOpen)
            {
                MessageBox.Show("Syringe comport is not open.", "Hamilton Pump " + Pump);
                return false;
            }

            // Obtain the number of bytes waiting in the port's buffer
            int NumberOfBytesInTheBuffer = comport.BytesToRead;

            // Create a byte array buffer to hold the incoming data
            byte[] buffer = new byte[NumberOfBytesInTheBuffer];

            //Send command requesting "Instrument Done" response. Y - done, * - busy 
            comport.Write(Pump + "F\r\n");
            Thread.Sleep(100);

            // Read the data from the port and store it in our buffer
            comport.Read(buffer, 0, NumberOfBytesInTheBuffer);
            if (buffer.Count()>2)
            {
                if (buffer[NumberOfBytesInTheBuffer - 2] == 'Y')
                    { return false;}
                else
                    { return true;}
            }
            else
                {return true; }
        }

        public int PumpCurrentPosition(string Pump)
        {
            int ReturnValue;
            string TempString = "";
            if (!comport.IsOpen)
            {
                MessageBox.Show("Syringe comport is not open.", "Hamilton Pump " + Pump);
                return -1;
            }
            comport.Write(Pump + "YQP\r\n");
            Thread.Sleep(200);
            
            string buffer = comport.ReadExisting();
            char value = (char)06;
            char[] Characters = buffer.ToCharArray();
            for (int i = Characters.Count()-1; i > 0; i--)
            {
                if (Characters[i] == value)
                {
                    for (int k = 1; 
                        k < (Characters.Count() - i - 1);
                        k++)
                    {
                        TempString = TempString + Characters[i + k].ToString();
                    }
                    i = 0;
                }
            }
            if (int.TryParse(TempString, out ReturnValue)) {return ReturnValue;}
            else {return -1;}
        }

        public bool InitializeAllSyringes()
        {
            if (!comport.IsOpen)
            {
                MessageBox.Show("Syringe comport is not open.", "Hamilton Pumps");
                return false;
            }
            comport.Write("1a\r\n");
            Thread.Sleep(200);
            comport.Write(":XR\r\n");
            while (PumpIsBusy("a"))
            {
                Thread.Sleep(500);
            }
            return true;
        }

        public void SetValvePosition(string Pump, int Pos)
        {
            if (!comport.IsOpen)
            {
                MessageBox.Show("Syringe comport is not open.", "Hamilton Pump " + Pump);
            }
            comport.Write(Pump + "LP0" + Pos.ToString()+"R\r\n");
            while(PumpIsBusy(Pump))
            {
                Thread.Sleep(100);
            }

        }

        public void SetValve(string Pump, string ValveType)
        {
            if (!comport.IsOpen)
            {
                MessageBox.Show("Syringe comport is not open.", "Hamilton Pump " + Pump);
            }
            comport.Write(Pump + "LST" + ValveType + "\r\n");
                        
            while (PumpIsBusy(Pump))
            {
                Thread.Sleep(100);
            }
        }

        public int GetValvePosition(string Pump)
        {
            int ReturnValue;
            string TempString = "";
            if (!comport.IsOpen)
            {
                MessageBox.Show("Syringe comport is not open.", "Hamilton Pump " + Pump);
                return -1;
            }


            comport.Write(Pump + "LQP\r\n");
            Thread.Sleep(200);

            string buffer = comport.ReadExisting();
            char value = (char)06;
            char[] Characters = buffer.ToCharArray();
            for (int i = Characters.Count() - 1; i > 0; i--)
            {
                if (Characters[i] == value)
                {
                    for (int k = 1;
                        k < (Characters.Count() - i - 1);
                        k++)
                    {
                        TempString = TempString + Characters[i + k].ToString();
                    }
                    i = 0;
                }
            }
            if (int.TryParse(TempString, out ReturnValue)) { return ReturnValue; }
            else { return -1; }
        }



}
}
