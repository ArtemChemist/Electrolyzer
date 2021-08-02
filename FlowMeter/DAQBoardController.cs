using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using NationalInstruments.DAQmx;
using System.Threading;
using System.IO;
using NationalInstruments;


namespace RadElChemBox
{
    class DAQBoardController
    {
        public DAQBoardController(string DeviceName)
        {
            DevicePhysicalAddress = DeviceName;
            //Instantiate the NI tasks and readrers
            //
            //Digital IO Port1
            Task_P00_out = new Task();
            Task_P00_out.DOChannels.CreateChannel(DeviceName + "/port0/line0:7", "", ChannelLineGrouping.OneChannelForAllLines);
            DO_P00 = new DigitalSingleChannelWriter(Task_P00_out.Stream);
            Task_P00_in = new Task();
            Task_P00_in.DOChannels.CreateChannel(DeviceName + "/port0", "", ChannelLineGrouping.OneChannelForAllLines);
            DI_P00 = new DigitalSingleChannelReader(Task_P00_in.Stream);
        
            //
            //Digital IO Port1
            Task_P10_out = new Task();
            Task_P10_out.DOChannels.CreateChannel(DeviceName + "/port1/line0:3", "", ChannelLineGrouping.OneChannelForAllLines);
            DO_P10 = new DigitalSingleChannelWriter(Task_P10_out.Stream);
            Task_P10_in = new Task();
            Task_P10_in.DOChannels.CreateChannel(DeviceName + "/port1", "myChannel", ChannelLineGrouping.OneChannelForAllLines);
            DI_P10 = new DigitalSingleChannelReader(Task_P10_in.Stream);

            //Initialize arrays holding current IO state and read the actual state into them
            Port0 = new bool[8];
            Port1 = new bool[4];
            Port0 = DI_P00.ReadSingleSampleMultiLine();
            Port1 = DI_P10.ReadSingleSampleMultiLine();
            
            //Analog Out port 1
            Task_AO0 = new Task();
            Task_AO0.AOChannels.CreateVoltageChannel(DeviceName + "/ao0", "", 0, 5, AOVoltageUnits.Volts);
            AO0 = new AnalogSingleChannelWriter(Task_AO0.Stream);
            
            //Analog Out port 2
            Task_AO1 = new Task();
            Task_AO1.AOChannels.CreateVoltageChannel(DeviceName + "/ao1", "", 0, 5, AOVoltageUnits.Volts);
            AO1 = new AnalogSingleChannelWriter(Task_AO1.Stream);

            //Analog In all ports
            Task_AI = new Task();
            Task_AI.AIChannels.CreateVoltageChannel(DeviceName + "/ai0", "", (AITerminalConfiguration)(10083), -0.5, 5, AIVoltageUnits.Volts);
            Task_AI.AIChannels.CreateVoltageChannel(DeviceName + "/ai1", "", (AITerminalConfiguration)(10083), -0.5, 5, AIVoltageUnits.Volts);
            Task_AI.AIChannels.CreateVoltageChannel(DeviceName + "/ai2", "", (AITerminalConfiguration)(10106), -5, 5, AIVoltageUnits.Volts);
            AI_Reader = new AnalogMultiChannelReader(Task_AI.Stream);
            Task_AI.Control(TaskAction.Verify);// Verify the task

            Task_PFI0 = new Task();
            Task_PFI0.CIChannels.CreateCountEdgesChannel(DeviceName + "/ctr0", "Count Edges", CICountEdgesActiveEdge.Falling, 0, CICountEdgesCountDirection.Up);
            Task_PFI0.Start();
            CounterReader = new CounterReader(Task_PFI0.Stream);
        }

        #region Local variables
        //NI variables

        string DevicePhysicalAddress;
        private Task Task_P00_out; //Digital Output Port 1, with channels 0 thru 7
        private Task Task_P00_in; //Digital Output Port 1, with channels 0 thru 7

        private DigitalSingleChannelWriter DO_P00; //Port 0 setter
        private DigitalSingleChannelReader DI_P00; //Port 0 reader

        private Task Task_P10_out; //Digital Output Port 2, with channels 0 thru 3
        private Task Task_P10_in; //Digital Output Port 2, with channels 0 thru 3

        private DigitalSingleChannelWriter DO_P10; //Port 1 setter
        private DigitalSingleChannelReader DI_P10; //Port 1 reader

        private Task Task_AO0; //Analog Output Port 1
        private AnalogSingleChannelWriter AO0; //Analog port 0 setter
        private Task Task_AO1; //Analog Output Port 1
        private AnalogSingleChannelWriter AO1; //Analog port 1 setter

        private Task Task_AI; //Analog Inputs Port
        private AnalogMultiChannelReader AI_Reader; //Analog Inputs, all three lines

        private Task Task_PFI0; //Counter
        private CounterReader CounterReader; //Counter reader
       
        //Arrays holding current positions of digital IO ports
        private bool[] Port0;
        private bool[] Port1;


        //event of the valve being chsnged
#endregion

        #region Accesors
        public double Current_Value_On_Analog_In_0
        {
            get { return AI_Reader.ReadSingleSample()[0]; }
            set { }
        }
        public double Current_Value_On_Analog_In_1
        {
            get { return AI_Reader.ReadSingleSample()[1]; }
            set { }
        }
        public double Current_Value_On_Analog_In_2
        {
            get { return AI_Reader.ReadSingleSample()[2]; }
            set { }
        }

        public void Set_Voltage_Analog_Port_0(double VoltageToSet)
        {
            AO0.WriteSingleSample(true, VoltageToSet);
        }
        public void Set_Voltage_Analog_Port_1(double VoltageToSet)
        {
            AO1.WriteSingleSample(true, VoltageToSet);
        }

        public bool DigitalIO_00
        {
            get 
            {
                return DI_P00.ReadSingleSampleMultiLine()[0];
            }
            set 
            {
                switch (value)
                {
                    case true:
                        Port0[0] = true;
                        break;
                    case false:
                        Port0[0] = false;
                        break;
                    default:
                        MessageBox.Show("Port 0 line 0 error!");
                        break;
                }
                DO_P00.WriteSingleSampleMultiLine(true, Port0);
            }
        }
        public bool DigitalIO_01
        {
            get
            {
                return DI_P00.ReadSingleSampleMultiLine()[1];
            }
            set
            {
                switch (value)
                {
                    case true:
                        Port0[1] = true;
                        break;
                    case false:
                        Port0[1] = false;
                        break;
                    default:
                        MessageBox.Show("Port 0 line 1 error!");
                        break;
                }
                DO_P00.WriteSingleSampleMultiLine(true, Port0);
            }
        }
        public bool DigitalIO_02
        {
            get
            {
                return DI_P00.ReadSingleSampleMultiLine()[2];
            }
            set
            {
                switch (value)
                {
                    case true:
                        Port0[2] = true;
                        break;
                    case false:
                        Port0[2] = false;
                        break;
                    default:
                        MessageBox.Show("Port 0 line 2 error!");
                        break;
                }
                DO_P00.WriteSingleSampleMultiLine(true, Port0);
            }
        }
        public bool DigitalIO_03
        {
            get
            {
                return DI_P00.ReadSingleSampleMultiLine()[3];
            }
            set
            {
                switch (value)
                {
                    case true:
                        Port0[3] = true;
                        break;
                    case false:
                        Port0[3] = false;
                        break;
                    default:
                        MessageBox.Show("Port 0 line 3 error!");
                        break;
                }
                DO_P00.WriteSingleSampleMultiLine(true, Port0);
            }
        }
        public bool DigitalIO_04
        {
            get
            {
                return DI_P00.ReadSingleSampleMultiLine()[4];
            }
            set
            {
                switch (value)
                {
                    case true:
                        Port0[4] = true;
                        break;
                    case false:
                        Port0[4] = false;
                        break;
                    default:
                        MessageBox.Show("Port 0 line 4 error!");
                        break;
                }
                DO_P00.WriteSingleSampleMultiLine(true, Port0);
            }
        }
        public bool DigitalIO_05
        {
            get
            {
                return DI_P00.ReadSingleSampleMultiLine()[5];
            }
            set
            {
                switch (value)
                {
                    case true:
                        Port0[5] = true;
                        break;
                    case false:
                        Port0[5] = false;
                        break;
                    default:
                        MessageBox.Show("Port 0 line 5 error!");
                        break;
                }
                DO_P00.WriteSingleSampleMultiLine(true, Port0);
            }
        }
        public bool DigitalIO_06
        {
            get
            {
                return DI_P00.ReadSingleSampleMultiLine()[6];
            }
            set
            {
                switch (value)
                {
                    case true:
                        Port0[6] = true;
                        break;
                    case false:
                        Port0[6] = false;
                        break;
                    default:
                        MessageBox.Show("Port 0 line 6 error!");
                        break;
                }
                DO_P00.WriteSingleSampleMultiLine(true, Port0);
            }
        }
        public bool DigitalIO_07
        {
            get
            {
                return DI_P00.ReadSingleSampleMultiLine()[7];
            }
            set
            {
                switch (value)
                {
                    case true:
                        Port0[7] = true;
                        break;
                    case false:
                        Port0[7] = false;
                        break;
                    default:
                        MessageBox.Show("Port 0 line 7 error!");
                        break;
                }
                DO_P00.WriteSingleSampleMultiLine(true, Port0);
            }
        }
        public bool DigitalIO_08
        {
            get
            {
                return DI_P10.ReadSingleSampleMultiLine()[0];
            }
            set
            {
                switch (value)
                {
                    case true:
                        Port1[0] = true;
                        break;
                    case false:
                        Port1[0] = false;
                        break;
                    default:
                        MessageBox.Show("Port 1 line 0 error!");
                        break;
                }
                DO_P10.WriteSingleSampleMultiLine(true, Port1);
            }
        }
        public bool DigitalIO_09
        {
            get
            {
                return DI_P10.ReadSingleSampleMultiLine()[1];
            }
            set
            {
                switch (value)
                {
                    case true:
                        Port1[1] = true;
                        break;
                    case false:
                        Port1[1] = false;
                        break;
                    default:
                        MessageBox.Show("Port 1 line 1 error!");
                        break;
                }
                DO_P10.WriteSingleSampleMultiLine(true, Port1);
            }
        }
        public bool DigitalIO_10
        {
            get { return DI_P10.ReadSingleSampleMultiLine()[2]; }
            set
            {
                switch (value)
                {
                    case true:
                        Port1[2] = true;
                        break;
                    case false:
                        Port1[2] = false;
                        break;
                    default:
                        MessageBox.Show("Port 1 line 2 error!");
                        break;
                }
                DO_P10.WriteSingleSampleMultiLine(true, Port1);
            }
        }
        public bool DigitalIO_11
        {
            get
            {
                return DI_P10.ReadSingleSampleMultiLine()[3];
            }
            set
            {
                switch (value)
                {
                    case true:
                        Port1[3] = true;
                        break;
                    case false:
                        Port1[3] = false;
                        break;
                    default:
                        MessageBox.Show("Port 1 line 3 error!");
                        break;
                }
                DO_P10.WriteSingleSampleMultiLine(true, Port1);
            }
        }
        public int CurrentCounter
        {
            get { return CounterReader.ReadSingleSampleInt32(); }
            set { }
        }

        public void Run_Wave_On_AO_0(double[] ArrayToWrite)
        {
            Task Task_AO0_PotentialSweep = new Task();
            Task_AO0_PotentialSweep.AOChannels.CreateVoltageChannel(DevicePhysicalAddress + "/ao0", "", 0, 5, AOVoltageUnits.Volts);
            Task_AO0_PotentialSweep.Timing.ConfigureSampleClock("",// external clock source line or use "" for internal clock
              500,// expected rate of external clock or actual rate of internal clock
              SampleClockActiveEdge.Rising,// acquire on rising or falling edge of ticks
              SampleQuantityMode.FiniteSamples,// continuous or finite samples
              5000);// number of finite samples to acquire or used for buffer size if continuous
            AnalogSingleChannelWriter AO0_PotentialSweep = new AnalogSingleChannelWriter(Task_AO0.Stream);
            
            AO0_PotentialSweep.WriteMultiSample(true, ArrayToWrite);
            while (!Task_AO0.IsDone)
            {
                Thread.Sleep(100);
            }
            Task_AO0_PotentialSweep.Dispose();
        }

        #endregion

        
    }
}
