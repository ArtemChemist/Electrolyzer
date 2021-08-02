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

namespace HPLC_Software
{
    class DeviceDriver
    {
        #region Constructor
        public DeviceDriver(frmGUI GUI, DeviceReader Reader)
        {
            MainGUI = GUI;
            P00 = new Task();
            P00.DOChannels.CreateChannel(MainGUI.lstNI.Text+"/port0/line0", "", ChannelLineGrouping.OneChannelForEachLine);
            DO_Writer_P00 = new DigitalSingleChannelWriter(P00.Stream);
            AO0 = new Task();
            AO0.AOChannels.CreateVoltageChannel(MainGUI.lstNI.Text + "/ao0", "", 0, 5, AOVoltageUnits.Volts);
            AO_Writer_AO0 = new AnalogSingleChannelWriter(AO0.Stream);
            
            MyReader = Reader;
            Secundomer.Start();
        }
        #endregion

        #region Local Variables
        //Reference to MainGUI
        frmGUI MainGUI;
        //Reference to the NI reader
        DeviceReader MyReader;

        //Delegates used for thread-safe communicatrion with GUI thread
        delegate void DelegateUpdateGUI(bool Position, double ActualTimeA, double ActualTimeB);
        delegate void DelegateStartChrom();
        DelegateUpdateGUI d; //Delegates used for thread-safeupdate of cosmetics in GUI thread

        //Variables dealing with the solvent composition
        private System.Diagnostics.Stopwatch Secundomer = new System.Diagnostics.Stopwatch();
        public double ActualTimeA = 500; //Actula time valve spent in position A, as measured by stopwatch
        public double ActualTimeB = 500; //Actula time valve spent in position B, as measured by stopwatch
        private bool KeepPumping = true;

        //NI variables
        public Task P00; //Solvent valve task
        private DigitalSingleChannelWriter DO_Writer_P00; //Solvent valve setter
        private Task AO0; //Flow rate
        AnalogSingleChannelWriter AO_Writer_AO0;
        #endregion

        #region Accessors
        public bool PumpRuns
        {
            get { return KeepPumping; }
            set{KeepPumping = value;}
        }
        
        #endregion
       
        //Sets solvent valve to the specified position.
        //Keeps track of time actually spet in this position
        //Updates cosmetics in GUI
        public void SolventValve(bool Position)
        {
            //Set timer measuring the actual time valve spent in on (off) position
            if (Position) ActualTimeB = Secundomer.ElapsedMilliseconds;
            else ActualTimeA = Secundomer.ElapsedMilliseconds;
            Secundomer.Reset();
            Secundomer.Start();
            
            //Update GUI, setting new valve position
            d = new DelegateUpdateGUI(MainGUI.UpdateSolventDeliveryBox);
            if (KeepPumping) { MainGUI.Invoke(d, new object[] { Position, ActualTimeA, ActualTimeB }); }

            //Set the valve to the right position on both GUI and in reader class
            Monitor.Enter(this);
            DO_Writer_P00.WriteSingleSampleSingleLine(true, Position);
            Monitor.Exit(this);
            MyReader.ValvePosition = Position;
        }

        //According to program open in GUI calculates time valve should stay on/off each seconds.
        //Sets valve to the right position for the duration of the analysis
        public void DoIsocratic()
        {
            while (KeepPumping)
            {
                AO_Writer_AO0.WriteSingleSample(true, MainGUI.FlowRate);
                int TimeToStayInA = MainGUI.TimeToStayA - 33;
                int TimeToStayInB = MainGUI.TimeToStayB - 33;
                if (TimeToStayInA > 5)
                {
                    SolventValve(true);
                    Thread.Sleep(TimeToStayInA);
                }
                if (TimeToStayInB > 5)
                {
                    SolventValve(false);
                    Thread.Sleep(TimeToStayInB);
                }
            }
         }
        public void SetFlow(double b)
        {
           AO_Writer_AO0.WriteSingleSample(true, b);
        }

        //Does isocratic elution with parameters supplied in first line of the program
        //Waits for the injection
        //Raises event of injection being turned, making cross-thread call to th erespective method in GUI
        public void WaitForInjection()
        {
            while (MyReader.TriggerPosition)
            {
                Thread.Sleep(1000);
            }
            //Raise event of trigger turened on Main GUI thread
            DelegateStartChrom d = new DelegateStartChrom(MainGUI.TriggerToInject);
            MainGUI.Invoke(d, new object[] { });
        }

    }
}
