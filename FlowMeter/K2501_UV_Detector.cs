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
//The following code is in part obtained from 
//* Project:    SerialPort Terminal
//* Company:    Coad .NET, http://coad.net
//* Author:     Noah Coad, http://coad.net/noah
//* Created:    March 2005
namespace RadElChemBox
{
    class K2501_UV_Detector
    {
    #region Local Variables

        // The main control for communicating through the RS-232 port
        private SerialPort comport = new SerialPort();

        private List<double> UVData = new List<double>();
        private int[] LastBinaryValueFromPort = new int[40];
        private List<byte> ReadingBuffer = new List<byte>();
        bool FoundFirstByte = false;
        public double Value = 0;
        //Parts of floating point word, the one after trimming the aux bits
        int[] mantissa = new int[24];
        int[] exponent = new int[8];
        int exponenta = 0;

        //Byte currently being considered
        int[] CurrentByte = new int[8];

        // Number of bytes in Com-port buffer
        int bytes;

        string PortName; //The comport name, supplied by the main GUI
        
        #endregion

        #region Constructor
        public K2501_UV_Detector(string ThePort)
        {
            // When data is recieved through the port, call this method
            comport.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            PortName = ThePort;
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
            comport.DataBits = 8;
            comport.StopBits = StopBits.Two;
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

            if (!succeeded) MessageBox.Show("Could not open the UV COM port. Is it already in use?", "COM Port Unavalible", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            
            return succeeded;
        }
        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // If the com port has been closed, do nothing
            if (!comport.IsOpen) return;

            // Obtain the number of bytes waiting in the port's buffer
            bytes = comport.BytesToRead;

            // Create a byte array buffer to hold the incoming data
            byte[] buffer = new byte[bytes];

            // Read the data from the port and store it in our buffer
            comport.Read(buffer, 0, bytes);

            ReadingBuffer.AddRange(buffer);

            //Trim out all residuals from the last uncaptured data entry
            //but make sure we do not just remove entries not yet completely recieved
            
            if (ReadingBuffer.Count >= 5)
            {
                while (ReadingBuffer.Count >= 5)
                {
                    //Starting from the beginning of buffer look for a byte with 7'th bit "1"
                    //Remove all bytes with 7'th bit "0", untill fine the one with "1".
                    while ((!FoundFirstByte)&(ReadingBuffer.Count>0))
                    {
                        DecToBinArray((int)ReadingBuffer[0], CurrentByte);
                        //Only sugnal that the begining of sequence found if the byte is not errror byte
                        if ((CurrentByte[7] == 1) & (CurrentByte[6] == 0) & (CurrentByte[5] == 0) & (CurrentByte[4] == 0))
                        {
                            FoundFirstByte = true;
                        }
                        else ReadingBuffer.RemoveAt(0);
                    }
                    //If we found first byte in 5-byte sequence
                    //move the sequence to the array holding last sequence 
                    if ((ReadingBuffer.Count >= 5)&FoundFirstByte)
                    {
                        //Take the siquence containing value and make an array of 1's and 0's out of it
                        for (int k = 0; k < 5; k++)
                            {
                                DecToBinArray((int)ReadingBuffer[k], CurrentByte);
                                for (int i = 0; i < 8; i++)
                                    {
                                     LastBinaryValueFromPort[8 * k + 7 - i] = CurrentByte[i];
                                    }
                            }
                        //remove bytes already processed
                        ReadingBuffer.RemoveRange(0, 5);
                        //Flag the array for further reading
                        FoundFirstByte = false;
                    }
                }
            }

            mantissa[0] = LastBinaryValueFromPort[4];
            mantissa[1] = LastBinaryValueFromPort[5];
            mantissa[2] = LastBinaryValueFromPort[6];
            mantissa[3] = LastBinaryValueFromPort[7];
            mantissa[4] = LastBinaryValueFromPort[9];
            mantissa[5] = LastBinaryValueFromPort[10];
            mantissa[6] = LastBinaryValueFromPort[11];
            mantissa[7] = LastBinaryValueFromPort[12];
            mantissa[8] = LastBinaryValueFromPort[13];
            mantissa[9] = LastBinaryValueFromPort[14];
            mantissa[10] = LastBinaryValueFromPort[15];
            mantissa[11] = LastBinaryValueFromPort[17];
            mantissa[12] = LastBinaryValueFromPort[18];
            mantissa[13] = LastBinaryValueFromPort[19];
            mantissa[14] = LastBinaryValueFromPort[20];
            mantissa[15] = LastBinaryValueFromPort[21];
            mantissa[16] = LastBinaryValueFromPort[22];
            mantissa[17] = LastBinaryValueFromPort[23];
            mantissa[18] = LastBinaryValueFromPort[25];
            mantissa[19] = LastBinaryValueFromPort[26];
            mantissa[20] = LastBinaryValueFromPort[27];
            mantissa[21] = LastBinaryValueFromPort[28];
            mantissa[22] = LastBinaryValueFromPort[29];
            mantissa[23] = LastBinaryValueFromPort[30];

            exponent[0] = LastBinaryValueFromPort[31];
            exponent[1] = LastBinaryValueFromPort[33];
            exponent[2] = LastBinaryValueFromPort[34];
            exponent[3] = LastBinaryValueFromPort[35];
            exponent[4] = LastBinaryValueFromPort[36];
            exponent[5] = LastBinaryValueFromPort[37];
            exponent[6] = LastBinaryValueFromPort[38];
            exponent[7] = LastBinaryValueFromPort[39];
            
            //Calculate exponent
            exponenta = 0;
            for (int i = 1; i < 8; i++)
            {
                exponenta = exponenta + power(2, (7 - i)) * exponent[i];
            }
            exponenta = exponenta - 128 * exponent[0];
            
            //Calculate the value
            Value = 0;
            for (int i = 1; i < 24; i++)
            {
                Value = Value + Math.Pow(2, (exponenta - i)) * mantissa[i];
            }
            Value = Value - Math.Pow(2, exponenta) * mantissa[0];
        }
        public void SetWavelength(byte [] b)
        {
          byte[] data = new byte[6];
          data[0] = 87;
          data[1] = 76;
          data[2] = b[0];
          data[3] = b[1];
          data[4] = b[2];
          data[5] = 13;
          // Send the binary data out the port
          comport.Write(data, 0, data.Length);
        }

        //Converts a decimal value to an array of 1's and 0's representing corresponding binary number
        //assumes array is of exactly 8 members
        protected void DecToBinArray(int ValueToTransform, int[] ArrayToUse)
        {
            int i = 0;
            for (; ValueToTransform >= 1; i++)
            {
                ArrayToUse[i] = ValueToTransform % 2;
                ValueToTransform = ValueToTransform / 2;
            }
            while (i < 8)
            {
                ArrayToUse[i] = 0;
                i++;
            }
        }

        // Returns an integer power of an integer
        protected int power(int ToExponentiate, int power)
        {
            if (power == 0) return 1;
            if (power == 1) return ToExponentiate;
            int result = ToExponentiate;
            for (int k = 1; k < power; k++)
            {
                result = result * ToExponentiate;
            }
            return result;
        }
}
}
