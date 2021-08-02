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

namespace HPLC_Software
{
    public class DeviceReader
    {
        //Constructor
          public DeviceReader(frmGUI GUI)
        {
            MainGUI = GUI;
              
            //Instantiate NI Tasks
            AI = new Task();
            P10 = new Task();
            PFI0 = new Task();
            
                // Create a channel with parameters passed in, expect parameters like "Dev1/ai0"
                AI.AIChannels.CreateVoltageChannel(MainGUI.lstNI.Text + "/ai0", "", (AITerminalConfiguration)(-1), -2, 2, AIVoltageUnits.Volts);
                AI.AIChannels.CreateVoltageChannel(MainGUI.lstNI.Text + "/ai1", "", (AITerminalConfiguration)(-1), -10, 10, AIVoltageUnits.Volts);
                P10.DIChannels.CreateChannel(MainGUI.lstNI.Text + "/port1/line0", "myChannel", ChannelLineGrouping.OneChannelForEachLine);
                PFI0.CIChannels.CreateCountEdgesChannel(MainGUI.lstNI.Text + "/ctr0", "Count Edges", CICountEdgesActiveEdge.Falling, 0, CICountEdgesCountDirection.Up);
                PFI0.Start();

                // Verify the task
                AI.Control(TaskAction.Verify);

                //Instantiate NI Readers
                //AI.Stream.ChannelsToRead = "UVChannel,RadChannel";
                AI_Reader = new AnalogMultiChannelReader(AI.Stream);

                DI_Reader_P10 = new DigitalSingleChannelReader(P10.Stream);
                CounterReader = new CounterReader(PFI0.Stream);
                ReadArray = new double[2];
       }

        //Reference to MainGUI
        frmGUI MainGUI;

        //NI Variables 
        private Task AI; //UV and Rad input task
        private Task P10; //Trigger input task
        private Task PFI0; //Counter

        private AnalogMultiChannelReader AI_Reader; //UV and Rad input reader
        private DigitalSingleChannelReader DI_Reader_P10; //Trigger input reader
        private CounterReader CounterReader; //Counter reader

        //position of the HPLC mixing valve
        bool privateValvePosition = true; 
        
        //Needed for thread-safe GUI update
        delegate void DelegateUpdateGUI(bool Position, double Fraction);
        delegate void DelegateStartChrom();

        //Variables holding current states
        private double CurrentAI0 = 0;
        private double CurrentAI1= 0;
        private int CurrentCounterRate = 0;
        private UInt32 CurrentCounter;
        //Neede for rough counter rate calculation
        private UInt32 PreviousCounterReading = 0;
        //Temporary storage needed to store the array of data read from multiple analog inputs
        private double[] ReadArray; 

        //Reads data from the device, puts it in the array holding raw data
        private void AddValuesToArraysHoldingData(object sender, EventArgs e)
        {
            //Reads current output of UV
            Monitor.Enter(this);
            ReadArray = AI_Reader.ReadSingleSample();
            CurrentAI0 = ReadArray[0];
            CurrentAI1 = ReadArray[1];
            Monitor.Exit(this);

            //Reads current output of Counter and converts it to counter rate
            Monitor.Enter(this);
            CurrentCounter = CounterReader.ReadSingleSampleUInt32();
            Monitor.Exit(this);
            CurrentCounterRate = (int)(CurrentCounter-PreviousCounterReading);
            PreviousCounterReading = CurrentCounter;
        }
        #region Accesors
        public double CurrentAnalogIn_0
        {
            get { return CurrentAI0; }
            set { }
        }
        public double CurrentAnalogIn_1
        {
            get { return CurrentAI1; }
            set { }
        }
        public double CurrentCountRate
        {
            get { return (double)CurrentCounterRate; }
            set { }
        }
            
        //Returns true if in load, returns false if in inject
        public bool TriggerPosition
        {
            get { return DI_Reader_P10.ReadSingleSampleSingleLine(); }
            set { }
        }

        public bool ValvePosition
        {
            get { return privateValvePosition; }
            set { privateValvePosition = value; }
        }
        #endregion


    }
}

