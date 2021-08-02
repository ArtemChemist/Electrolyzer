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

namespace RadElChemBox
{
    public partial class frmGUI : Form
    {
        public frmGUI()
        {
            InitializeComponent();
            this.PlottingIsDone += FinishPlotting;
            this.Disposed += CancelPlotting;
            lstNI.Text = "Dev2";
            lstUVCOM.Text = "COM11";
            lstSyringePort.Text = "COM5";
            dictInitialization = new Dictionary<string, bool>();
            dictInitialization.Add("Pump", false);
            dictInitialization.Add("UV_Detector", false);
            dictInitialization.Add("Syringes", false);
            dictInitialization.Add("NIBoard", false);
            txtCalibrationFileName.Text = strCalibrationFileName;
        }
        #region Event Declarations
        public event EventHandler<EventArgs> PlottingIsDone;

        #endregion

        #region Variables declaration

        //Hardware variables
        DeviceControler Machine; //The controller of the hardware linked to the NI board
        K2501_UV_Detector K2501; //The controller of the UV detector

        private int RunTime;
        private int TickCounter = 0;
        private string Status = " ";

        //Instances of a class plotting graphs
        CromatogramGraph frmRadDetectors;
        CromatogramGraph frmCounter;

        //Arrays holding raw outputs and timestamps
        private double[] AI0Stream; //Analog signal, line 0
        private double[] AI1Stream; //Analog signal, line 1
        private double[] AI2Stream; //Analog signal, line 2
        private double[] CounterRate; //Counter ratemeter on the NI board
        private System.DateTime[] Time; //Timestamp I created internally for tracking of the reading
        private string[] Log; //Log of the hardware events
        
        //Variables for running counter rate
        double PreviousCounterValue = 0;

        //Dictionary for linking the array to thier names
        //used to communicate with the form drawing graphs

        public Dictionary<string, double[]> RadArraysDictionary = new Dictionary<string, double[]>();
        public Dictionary<string, double[]> CounterArrayDictionary = new Dictionary<string, double[]>();

        //Operational parameters
        double dblGasPressure; //Set air pressure

        //Images for buttons
        Bitmap Dotted4_4 = new Bitmap(RadElChemBox.Properties.Resources.Dotted);
        Bitmap Solid4_4 = new Bitmap(RadElChemBox.Properties.Resources.Solid);
        Bitmap Solid_Loop = new Bitmap(RadElChemBox.Properties.Resources.Loop_Solid);
        Bitmap Dotted_Loop = new Bitmap(RadElChemBox.Properties.Resources.Loop_Dotted);
        Bitmap Distr8_1 = new Bitmap(RadElChemBox.Properties.Resources.Distr_8_01);
        Bitmap Distr8_2 = new Bitmap(RadElChemBox.Properties.Resources.Distr_8_02);
        Bitmap Distr8_3 = new Bitmap(RadElChemBox.Properties.Resources.Distr_8_03);
        Bitmap Distr8_4 = new Bitmap(RadElChemBox.Properties.Resources.Distr_8_04);
        Bitmap Distr8_5 = new Bitmap(RadElChemBox.Properties.Resources.Distr_8_05);
        Bitmap Distr8_6 = new Bitmap(RadElChemBox.Properties.Resources.Distr_8_06);
        Bitmap Distr8_7 = new Bitmap(RadElChemBox.Properties.Resources.Distr_8_07);
        Bitmap Distr8_8 = new Bitmap(RadElChemBox.Properties.Resources.Distr_8_08);
        Bitmap ValveClosed = new Bitmap(RadElChemBox.Properties.Resources.NC);
        Bitmap ValveOpen = new Bitmap(RadElChemBox.Properties.Resources.NO);
        Bitmap Distr6_1 = new Bitmap(RadElChemBox.Properties.Resources.Distr_6_01);
        Bitmap Distr6_2 = new Bitmap(RadElChemBox.Properties.Resources.Distr_6_02);
        Bitmap Distr6_3 = new Bitmap(RadElChemBox.Properties.Resources.Distr_6_03);
        Bitmap Distr6_4 = new Bitmap(RadElChemBox.Properties.Resources.Distr_6_04);
        Bitmap Distr6_5 = new Bitmap(RadElChemBox.Properties.Resources.Distr_6_05);
        Bitmap Distr6_6 = new Bitmap(RadElChemBox.Properties.Resources.Distr_6_06);

        //Dictionaru used to track initialization progress
        private Dictionary<string, bool> dictInitialization;
        
        //Path to the calibration
        private string strCalibrationFileName = "D:\\[Artem]\\C#\\!MySoft\\Electrolyzer\\calibration.csv";
        #endregion
        delegate void dlgUpdateGUI();

        #region Plotting Event Handlers
        private void FinishPlotting(object sender, EventArgs e)
        {
            this.GUITimer.Enabled = false;
            this.GUITimer.Tick -= IncrementTicks;
            lblStatus.Text = "Analysis is done.";
            btnSave_Click(this, e);
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            this.Refresh();
        }
        private void CancelPlotting(object sender, EventArgs e)
        {
            GUITimer.Enabled = false;
            btnStart.Enabled = true;
            btnStop.Enabled = false;
        }
        private void IncrementTicks(object sender, EventArgs e)
        {
            //Collect data form the reader
            AI0Stream[TickCounter] = Machine.Current_Value_On_Rad_Detector_1;
            AI1Stream[TickCounter] = Machine.Current_Value_On_Rad_Detector_2;
            AI2Stream[TickCounter] = Machine.Current_Value_On_Rad_Detector_3;
            Time[TickCounter] = DateTime.Now;
            CounterRate[TickCounter] = Machine.Current_Counter_Value - PreviousCounterValue;
            PreviousCounterValue = Machine.Current_Counter_Value;
            Log[TickCounter] = Status;
            Status = " ";

            //Update interface labeles
            lblRad1.Text = (Math.Round (AI0Stream[TickCounter], 2)).ToString();
            lblRad2.Text = (Math.Round (AI1Stream[TickCounter], 2)).ToString();
            lbldeltaV.Text = (Math.Round(AI2Stream[TickCounter], 2)).ToString();

            //Check if its time to stop it
            if (TickCounter < RunTime)
            {
                TickCounter++;
                
            }
            if (TickCounter >= RunTime)
            {
                PlottingIsDone(this, e);
                GUITimer.Stop();
            }
        }
        #endregion
        #region GUI Event Handlers
        private void UpdateManualTab(object sender, EventArgs e)
        {
     
            switch (Machine.ValveA)
            {
                case "Dotted":
                    btnValveA.BackgroundImage = Dotted_Loop;
                    Status = Status + "A Dotted; ";
                    break;
                case "Solid":
                    btnValveA.BackgroundImage = Solid_Loop;
                    Status = Status + "A Solid; ";
                    break;
                default:
                    MessageBox.Show("Valve A GUI error!");
                    break;
            }

            switch (Machine.ValveB)
            {
                case "Dotted":
                    btnValveB.BackgroundImage = Dotted_Loop;
                    Status = Status + "B Dotted; ";
                    break;
                case "Solid":
                    btnValveB.BackgroundImage = Solid_Loop;
                    Status = Status + "B Solid; ";
                    break;
                default:
                    MessageBox.Show("Valve B GUI error!");
                    break;
            }

            switch (Machine.Valve1)
            {
                case "On":
                    btnValve1.BackColor = System.Drawing.Color.Red;
                    btnValve1.BackgroundImage = ValveClosed;
                    Status = Status + " 1 NC; ";

                    break;
                case "Off":
                    btnValve1.BackColor = System.Drawing.Color.LightGreen;
                    btnValve1.BackgroundImage = ValveOpen;
                    Status = Status + " 1 NO; ";

                    break;
                default:
                    MessageBox.Show("Valve 1 GUI error!");
                    break;
            }

            btnValveD_01.BackColor = System.Drawing.SystemColors.Control;
            btnValveD_02.BackColor = System.Drawing.SystemColors.Control;
            btnValveD_03.BackColor = System.Drawing.SystemColors.Control;
            btnValveD_04.BackColor = System.Drawing.SystemColors.Control;
            btnValveD_05.BackColor = System.Drawing.SystemColors.Control;
            btnValveD_06.BackColor = System.Drawing.SystemColors.Control;

            switch (Machine.ValveD)
            {
                case 1:
                    btnValveD_01.BackColor = System.Drawing.Color.LightGreen;
                    picValveD.BackgroundImage = Distr6_1;
                    Status = Status + "D1; ";
                    break;
                case 2:
                    btnValveD_02.BackColor = System.Drawing.Color.LightGreen;
                    picValveD.BackgroundImage = Distr6_2;
                    Status = Status + "D2; ";
                    break;
                case 3:
                    btnValveD_03.BackColor = System.Drawing.Color.LightGreen;
                    picValveD.BackgroundImage = Distr6_3;
                    Status = Status + "D3; ";
                    break;
                case 4:
                    btnValveD_04.BackColor = System.Drawing.Color.LightGreen;
                    picValveD.BackgroundImage = Distr6_4;
                    Status = Status + "D4; ";
                    break;
                case 5:
                    btnValveD_05.BackColor = System.Drawing.Color.LightGreen;
                    picValveD.BackgroundImage = Distr6_5;
                    Status = Status + "D5; ";
                    break;
                case 6:
                    btnValveD_06.BackColor = System.Drawing.Color.LightGreen;
                    picValveD.BackgroundImage = Distr6_6;
                    Status = Status + "D6; ";
                    break;
                default:
                    MessageBox.Show("Valve D GUI error!");
                    break;
            }
            
        }
        private void UpdateSyringeGraphics(object sender, EventArgs e)
         {
             dlgUpdateGUI dlgUpdate_All_Syringes = new dlgUpdateGUI(UpdateSyringeGraphics);
             this.Invoke(dlgUpdate_All_Syringes, new object[] { });
         }
        private void What_to_Do_When_Syringe_A_Done(object sender, EventArgs e)
        {
            dlgUpdateGUI dlgUpdate_SyringeA = new dlgUpdateGUI(Unfreeze_Syringe_A_Buttons);
            this.Invoke(dlgUpdate_SyringeA, new object[] { });
        }
        private void Unfreeze_Syringe_A_Buttons()
        {
            lblPumpAPosition.Text = Machine.Pump_A_Position.ToString();
            switch (Machine.Pump_A_Valve_Position)
            {
                case (1):
                    btnSyringeAValve.BackgroundImage = Dotted4_4;
                    Status = Status + " Pump A: Valve to Dotted; syringe at " + lblPumpAPosition.Text;
                    break;
                case (4):
                    btnSyringeAValve.BackgroundImage = Solid4_4;
                    Status = Status + " Pump A: Valve to Solid; syringe at " + lblPumpAPosition.Text;
                    break;
                default:
                    MessageBox.Show("Can not get syringe A valve position", "GUI");
                    break;
            }
            grpPumpA.Enabled = true;
            btnAspirtaeA.Enabled = true;
            btnDispenseA.Enabled = true;

        }
        private void What_to_Do_When_Syringe_B_Done(object sender, EventArgs e)
        {
            dlgUpdateGUI dlgUpdate_SyringeB = new dlgUpdateGUI(Unfreeze_Syringe_B_Buttons);
            this.Invoke(dlgUpdate_SyringeB, new object[] { });
        }
        private void Unfreeze_Syringe_B_Buttons()
        {
            lblPumpBPosition.Text = Machine.Pump_B_Position.ToString();
            switch (Machine.Pump_B_Valve_Position)
            {
                case (1):
                    picPumpBValve.BackgroundImage = Distr8_1;
                    Status = Status + " Pump B: Valve to 1; syringe at " + lblPumpBPosition.Text;
                    break;
                case (2):
                    picPumpBValve.BackgroundImage = Distr8_2;
                    Status = Status + " Pump B: Valve to 2; syringe at " + lblPumpBPosition.Text;
                    break;
                case (3):
                    picPumpBValve.BackgroundImage = Distr8_3;
                    Status = Status + " Pump B: Valve to 3; syringe at " + lblPumpBPosition.Text;
                    break;
                case (4):
                    picPumpBValve.BackgroundImage = Distr8_4;                    
                    Status = Status + " Pump B: Valve to 4; syringe at " + lblPumpBPosition.Text;
                    break;
                case (5):
                    picPumpBValve.BackgroundImage = Distr8_5;
                    Status = Status + " Pump B: Valve to 5; syringe at " + lblPumpBPosition.Text;
                    break;
                case (6):
                    picPumpBValve.BackgroundImage = Distr8_6;
                    Status = Status + " Pump B: Valve to 6; syringe at " + lblPumpBPosition.Text;
                    break;
                case (7):
                    picPumpBValve.BackgroundImage = Distr8_7;
                    Status = Status + " Pump B: Valve to 7; syringe at " + lblPumpBPosition.Text;
                    break;
                case (8):
                    picPumpBValve.BackgroundImage = Distr8_8;
                    Status = Status + " Pump B: Valve to 8; syringe at " + lblPumpBPosition.Text;
                    break;
                default:
                    MessageBox.Show("Can not get syringe B valve position", "GUI");
                    break;
            }
            grpPumpB.Enabled = true;
            btnAspirtaeB.Enabled = true;
            btnDispenseB.Enabled = true;
            btnRatchetB.Enabled = true;
            btnRecirculation.Enabled = true;

        }
        private void UpdateSyringeGraphics()
        {
           Unfreeze_Syringe_A_Buttons();
           Unfreeze_Syringe_B_Buttons();
        }
        #endregion

        #region Accessors
        public int GetRunTime()
        {
            double RunTimeInMinuets;
            Double.TryParse(txtRunTime.Text, out RunTimeInMinuets);
            RunTime = (int)(60 * RunTimeInMinuets);
            return RunTime;
        }
        private void txtRunTime_TextChanged(object sender, EventArgs e)
        {
            GetRunTime();
        }

        public int CurrentTick()
        {
            return TickCounter;
        }
        #endregion
        #region Initializations
        private void btnInit_Click(object sender, EventArgs e)
        {
            btnInitializeNI.PerformClick();
            btnInitializeUV.PerformClick();
            btnInitializeSyringes.PerformClick();
            btnSyringeInitialization.PerformClick();
        }
        private void btnInitializeNI_Click(object sender, EventArgs e)
        {
            bool error = false;
            if (Machine == null)
            {
                Machine = new DeviceControler(strCalibrationFileName);
                Configuration();
            }

            try
            {
                Machine.Initialize_NI_Device(lstNI.Text);
                error = false;
            }
            catch (DaqException)
            {
                error = true;
                MessageBox.Show("Can not initialize aqusition board.", "NI Device Unavalible", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }

            if (!error)
            {

                tabTabs.Enabled = true;
                //tabTabs.TabPages[0].Enabled = false;
                UpdateManualTab(this, e);
                Machine.ValvePositionChnaged += UpdateManualTab;
                //Machine.ValveE = 2;
                //Machine.ValveD = 6;
                btnInitializeNI.Enabled = false;
                btnInit.Enabled = false;
                dictInitialization["NIBoard"] = true;
                TryCompleteInitialization();
            }
        }
        private void btnInitializeSyringes_Click(object sender, EventArgs e)
        {
            if (Machine == null)
            {
                Machine = new DeviceControler(strCalibrationFileName);
                Configuration();
            }
            Machine.SyringesChanged += UpdateSyringeGraphics;
            Machine.PumpAFree += What_to_Do_When_Syringe_A_Done;
            Machine.PumpBFree += What_to_Do_When_Syringe_B_Done;
            Machine.Initialize_Syringes(lstSyringePort.Text);
            if (Machine.Initialize_Syringes(lstSyringePort.Text))
            {
                //btnInitializeSyringes.Enabled = false;
                btnInit.Enabled = false;
                tabTabs.Enabled = true;
                //tabTabs.TabPages[0].Enabled = false;
                dictInitialization["Syringes"] = true;
                TryCompleteInitialization();
            }
        }
        private void btnSyringeInitialization_Click(object sender, EventArgs e)
        {
            if (Machine != null)
            {
                Machine.Initialize_Syringes_Pumps();
            }
        }
        private void btnInitializeUV_Click(object sender, EventArgs e)
        {
            bool error = false;
            if (Machine == null)
            {
                Machine = new DeviceControler(strCalibrationFileName);
                Configuration();
            }

            if (K2501 == null) K2501 = new K2501_UV_Detector(lstUVCOM.Text);
            if (K2501.OpenPort())
            {
                error = false;
            }
            else
            {
                error = true;
            }

            if (!error)
            {
                btnInitializeUV.Enabled = false;
                btnInit.Enabled = false;
                tabTabs.Enabled = true;
                tabTabs.TabPages[0].Enabled = false;
                dictInitialization["UV_Detector"] = true;
                TryCompleteInitialization();
            }
        }
        private void TryCompleteInitialization()
        {
            bool InitializationComplete = true;
            foreach (KeyValuePair<string, bool> entry in dictInitialization)
            {
                if (!entry.Value) { InitializationComplete = false; }
            }
            if (InitializationComplete)
            {
                tabTabs.TabPages[0].Enabled = true;
                btnInit.Enabled = false;
                btnStart.Enabled = true;
            }
        }
        private void btnCalibrationFileOpen_Click(object sender, EventArgs e)
        {
            Open.Title = "Select a file";
            Open.FileName = "";
            Open.Filter = "Text files (*.csv)|*.csv";
            Open.FilterIndex = 1;
            if (Open.ShowDialog() != DialogResult.Cancel)
            {
                txtCalibrationFileName.Text = Open.FileName;
                strCalibrationFileName = Open.FileName;
            }

        }

        #endregion

        private void btnStart_Click(object sender, EventArgs e)
        {
            // Do cosmetics
            lblStatus.Text = "Plotting stuff...";

            //Check if all boxes are filled
            if (RunTime<=0)
            {
                txtRunTime.Text = "30";
            }
            //Initialize arrays holding averaged data
            AI0Stream = new double[GetRunTime()];
            AI1Stream = new double[GetRunTime()];
            AI2Stream = new double[GetRunTime()];
            Log = new string[GetRunTime()];
            CounterRate = new double[GetRunTime()];
            CounterRate[0] = 0;
            Time = new DateTime[GetRunTime()];

            
            //Add these arrays to the dictionary, clears it first
            RadArraysDictionary.Clear();
            RadArraysDictionary.Add("Rad 1", AI0Stream);
            RadArraysDictionary.Add("Rad 2", AI1Stream);
            RadArraysDictionary.Add("Rad 3", AI2Stream);
            
            
            //subscription to the timer event
            if (this.GUITimer == null)
            {
                this.GUITimer = new System.Windows.Forms.Timer(this.components);
                this.GUITimer.Interval = 1000;
            }
            this.GUITimer.Tick += IncrementTicks;
            TickCounter = 0;
       
            //Create window for Rad Detectors
            frmRadDetectors = new CromatogramGraph(this, RadArraysDictionary);
            frmRadDetectors.Text = "Analog Detector";
            frmRadDetectors.Location = new Point(0, SystemInformation.PrimaryMonitorSize.Height / 2);

            //Create window for Counter
            CounterArrayDictionary.Clear();
            CounterArrayDictionary.Add("Counter", CounterRate);
            frmCounter = new CromatogramGraph(this, CounterArrayDictionary);
            frmCounter.Text = "Counter";
            frmCounter.Location = new Point(0, SystemInformation.PrimaryMonitorSize.Height / 2);


            //Show graphs
            frmCounter.Show();
            frmRadDetectors.Show();
            this.BringToFront();

            this.GUITimer.Enabled = true;
            this.GUITimer.Start();
         
            //Do cosmetics
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            this.Refresh();

        }
        private void btnStop_Click(object sender, EventArgs e)
         {
             this.GUITimer.Stop();
             this.GUITimer.Dispose();
             btnStart.Enabled = true;
             lblStatus.Text = "Done!";

         }


        //Opens file for saving data
        private void btnOpen_Click(object sender, EventArgs e)
        {
            Open.Title = "Select a file";
            Open.FileName = "";
            Open.Filter = "Text files (*.csv)|*.csv";
            Open.FilterIndex = 1;
            if (Open.ShowDialog() != DialogResult.Cancel) txtFileName.Text = Open.FileName;
        }

        //Saves data into the file
        private void btnSave_Click(object sender, EventArgs e)
        {
            System.IO.StreamWriter OpenFileStream;
            string CurrentString = "This is the first line";
            string tmpTime = "0:0:0.000"; //String representing the actual time the datapoint was taken
            double dblTime1; //For Time delta between two consequtive points 
            double dblTime2; //For Time delta between two consequtive points 
            //Initialize file handler
            
                OpenFileStream = new System.IO.StreamWriter(txtFileName.Text);
                int i = 0;
                int ArrayCount = AI0Stream.Count();
                OpenFileStream.WriteLine("TimeDelta,Time,Rad1,Rad2,Counter,WE-CE,Status");
                while ((CurrentString != null) &(i < (ArrayCount-1)))
                {
                    tmpTime = Time[i].Hour.ToString() + ":" + Time[i].Minute.ToString() + ":" + Time[i].Second.ToString() + "." + Time[i].Millisecond.ToString();
                    dblTime2 = (Time[i + 1].Hour * 3600 + Time[i + 1].Minute * 60 + Time[i + 1].Second + Convert.ToDouble(Time[i + 1].Millisecond) / 1000);
                    dblTime1 = (Time[i].Hour * 3600 + Time[i].Minute * 60 + Time[i].Second + Convert.ToDouble(Time[i].Millisecond) / 1000);
                    CurrentString = (Math.Round((dblTime2 - dblTime1), 4).ToString() + "," + tmpTime + "," + AI0Stream[i].ToString() + "," + AI1Stream[i].ToString() + "," + CounterRate[i].ToString() + ", " + AI2Stream[i].ToString() + ", " + Log[i]);
                    OpenFileStream.WriteLine(CurrentString);
                    i++;
                }
                OpenFileStream.Close();
        }

        //Routines for reading/saving calibration parameters 
        private void Configuration ()
        {

            txtAGasPressure.Text = Machine.AGasPressure.ToString();
            txtBGasPressure.Text = Machine.BGasPressure.ToString();
            txtARad1.Text = Machine.ARad1.ToString();
            txtBRad1.Text = Machine.BRad1.ToString();
            txtARad2.Text = Machine.ARad2.ToString();
            txtBRad2.Text = Machine.BRad2.ToString();
            txtARad3.Text = Machine.ARad3.ToString();
            txtBRad3.Text = Machine.BRad3.ToString();
            txtPumpASyringeVolume.Text = Machine.Syringe_A_Volume.ToString();
            txtPumpBSyringeVolume.Text = Machine.Syringe_B_Volume.ToString();
            txtPumpCSyringeVolume.Text = Machine.Syringe_C_Volume.ToString();
            txtValveAType.Text = Machine.Syringe_A_Valve.ToString();
            txtValveBType.Text = Machine.Syringe_B_Valve.ToString();
            txtValveCType.Text = Machine.Syringe_C_Valve.ToString();

            lblPumpAVolume.Text = "out of "+Machine.Syringe_A_Volume.ToString()+ " mkl";
            lblPumpBVolume.Text = "out of " + Machine.Syringe_B_Volume.ToString() + " mkl";
           
        }
        private void btnSaveCal_Click(object sender, EventArgs e)
         {
            double tmp_AGasPressure;
            Double.TryParse(txtAGasPressure.Text, out tmp_AGasPressure);
            Machine.AGasPressure = tmp_AGasPressure;

            double tmp_BGasPressure;
            Double.TryParse(txtBGasPressure.Text, out tmp_BGasPressure);
            Machine.BGasPressure = tmp_BGasPressure;

            double tmp_ARad1;
            Double.TryParse(txtARad1.Text, out tmp_ARad1);
            Machine.ARad1 = tmp_ARad1;

            double tmp_BRad1;
            Double.TryParse(txtBRad1.Text, out tmp_BRad1);
            Machine.BRad1 = tmp_BRad1;

            double tmp_ARad2;
            Double.TryParse(txtARad2.Text, out tmp_ARad2);
            Machine.ARad2 = tmp_ARad2;

            double tmp_BRad2;
            Double.TryParse(txtBRad2.Text, out tmp_BRad2);
            Machine.BRad2 = tmp_BRad2;

            double tmp_ARad3;
            Double.TryParse(txtARad3.Text, out tmp_ARad3);
            Machine.ARad3 = tmp_ARad3;

            double tmp_BRad3;
            Double.TryParse(txtBRad3.Text, out tmp_BRad3);
            Machine.BRad3 = tmp_BRad3;

            Double tmp_Syringe_A_Volume;
            Double.TryParse(txtPumpASyringeVolume.Text, out tmp_Syringe_A_Volume);
            Machine.Syringe_A_Volume = tmp_Syringe_A_Volume;


            Double tmp_Syringe_B_Volume;
            Double.TryParse(txtPumpBSyringeVolume.Text, out tmp_Syringe_B_Volume);
            Machine.Syringe_B_Volume = tmp_Syringe_B_Volume;

            Double tmp_Syringe_C_Volume;
            Double.TryParse(txtPumpCSyringeVolume.Text, out tmp_Syringe_C_Volume);
            Machine.Syringe_C_Volume = tmp_Syringe_C_Volume;

            Double tmp_Syringe_A_Valve;
            Double.TryParse(txtValveAType.Text, out tmp_Syringe_A_Valve);
            Machine.Syringe_A_Valve = tmp_Syringe_A_Valve;

            Double tmp_Syringe_B_Valve;
            Double.TryParse(txtValveBType.Text, out tmp_Syringe_B_Valve);
            Machine.Syringe_B_Valve = tmp_Syringe_B_Valve;

            Double tmp_Syringe_C_Valve;
            Double.TryParse(txtValveCType.Text, out tmp_Syringe_C_Valve);
            Machine.Syringe_C_Valve = tmp_Syringe_C_Valve;
       
            Machine.SaveCalibration();
            Machine.SetValve("a", txtValveAType.Text);
            Machine.SetValve("b", txtValveBType.Text);
            Machine.SetValve("c", txtValveCType.Text);
         }



 #region Valve Controls

         private void btnValveA_Click(object sender, EventArgs e)
         {
             switch (Machine.ValveA)
             {
                 case "Solid":
                     Machine.ValveA = "Dotted";
                     break;
                 case "Dotted":
                     Machine.ValveA = "Solid";
                     break;
                 default:
                     MessageBox.Show("Error!");
                     break;
             }
         }
         private void btnValveB_Click(object sender, EventArgs e)
        {
            switch (Machine.ValveB)
            {
                case "Solid":
                    Machine.ValveB = "Dotted";
                    break;
                case "Dotted":
                    Machine.ValveB = "Solid";
                    break;
                default:
                    MessageBox.Show("Error!");
                    break;
            }
        }

         private void btnValve1_Click(object sender, EventArgs e)
         {
             switch (Machine.Valve1)
             {
                 case "On":
                     Machine.Valve1 = "Off";
                     break;
                 case "Off":
                     Machine.Valve1 = "On";
                     break;
                 default:
                     MessageBox.Show("Error!");
                     break;
             }
         }

         private void btnValveD_01_Click(object sender, EventArgs e)
         {
             Machine.ValveD = 1;
         }
         private void btnValveD_02_Click(object sender, EventArgs e)
         {
             Machine.ValveD = 2;
         }
         private void btnValveD_03_Click(object sender, EventArgs e)
         {
             Machine.ValveD = 3;
         }
         private void btnValveD_04_Click(object sender, EventArgs e)
         {
             Machine.ValveD = 4;
         }
         private void btnValveD_05_Click(object sender, EventArgs e)
         {
             Machine.ValveD = 5;
         }
         private void btnValveD_06_Click(object sender, EventArgs e)
         {
             Machine.ValveD = 6;
         }


#endregion
#region Unit Operations


         private void btnTrap_Click(object sender, EventArgs e)
         {
             Machine.ValveE = 2;
             Machine.ValveA = "Dotted";
             Machine.Valve1 = "On";
             Machine.Valve2 = "On";
             Double.TryParse(txtPressureToTrap.Text, out dblGasPressure);
             Machine.Gas_Pressure = dblGasPressure;
         }


  
         private void btnRecirculation_Click(object sender, EventArgs e)
         {
             int _volume;
             int _speed;
             int _repetitions;
             int from;
             int to;
             int.TryParse(txtRecircVolume.Text, out _volume);
             int.TryParse(txtRecircSpeed.Text, out _speed);
             int.TryParse(txtRecircRepetitions.Text, out _repetitions);
             int.TryParse(txtRecircFrom.Text, out from);
             int.TryParse(txtRecircTo.Text, out to);

             if ((_volume < 5000) & (_speed < 150))
             {
                 Machine.Recirculate_Pump_B(_volume, _speed, _repetitions,from,to);
                 Status = "Circulating On B: " + txtRecircVolume.Text + "/" + txtRecircSpeed.Text + ", " + txtRecircRepetitions.Text + " times";
                 btnRecirculation.Enabled = false;
                 btnStopRecirculation.Enabled = true;
             }
             else
             {
                 MessageBox.Show("Invalid Parameters");
             }

         }

         private void btnElute_Click(object sender, EventArgs e)
         {
             Machine.ValveC = "Dotted";
             Machine.ValveB = "Solid";
             Machine.ValveA = "Dotted";
             Machine.Valve3 = "On";
             Machine.Pump_A_Valve_Position = 7;
             Machine.Aspirate_Pump_A(8000, 200);
             Thread.Sleep(500);
             Machine.Pump_A_Valve_Position = 3;
             Machine.Dispense_Pump_A(8000, 50);
         }

         private void btnSystemFilling_Click(object sender, EventArgs e)
         {
             int FillVolume;
             int FillSpeed;
             int.TryParse(txtFillSystemVolume.Text, out FillVolume);
             int.TryParse(txtFillSystemSpeed.Text, out FillSpeed);

             Machine.ValveC = "Solid";
             Machine.ValveB = "Solid";
             Machine.ValveA = "Dotted";
             Machine.Valve3 = "Off";
             Machine.Pump_A_Valve_Position = 6;
             Machine.Aspirate_Pump_A(FillVolume, 200);
             Thread.Sleep(500);
             Machine.Pump_A_Valve_Position = 3;
             Machine.Dispense_Pump_A(FillVolume, FillSpeed);
         }

         private void btnWaterToIE_Click(object sender, EventArgs e)
         {
             Machine.ValveE = 5;
             Machine.ValveA = "Dotted";
             Machine.Valve2 = "Off";
         }

         private void btnWashCartridgeMeCN_Click(object sender, EventArgs e)
         {
             Machine.ValveE = 6;
             Machine.ValveA = "Dotted";
             Machine.Valve2 = "Off";
         }
#endregion
#region Syringe Pump Controls

         private void btnAspirtaeA_Click(object sender, EventArgs e)
         {
             btnAspirtaeA.Enabled = false;
             int ValueToAspirate = 0;
             int AspirateSpeed = 0;
             int.TryParse(txtAspirtaeA.Text, out ValueToAspirate);
             int.TryParse(txtPumpAAspirateSpeed.Text, out AspirateSpeed);
             Status = " Pump A aspirates " + txtAspirtaeA.Text + " at " + txtPumpAAspirateSpeed.Text;
             Machine.Aspirate_Pump_A(ValueToAspirate, AspirateSpeed);
         }
         private void btnDispenseA_Click(object sender, EventArgs e)
         {
             btnDispenseA.Enabled = false;
             int ValueToDispense = 0;
             int DispenseSpeed = 0;
             int.TryParse(txtDispenseA.Text, out ValueToDispense);
             int.TryParse(txtPumpADispenseSpeed.Text, out DispenseSpeed);
             Status = " Pump A dispensess " + txtDispenseA.Text + " at " + txtPumpADispenseSpeed.Text;
             Machine.Dispense_Pump_A(ValueToDispense, DispenseSpeed);
         }
         private void btnSyringeAValve_Click(object sender, EventArgs e)
         {
             grpPumpA.Enabled = false;
             switch (Machine.Pump_A_Valve_Position)
             {
                 case (1):
                    Machine.Pump_A_Valve_Position = 4;
                    break;
                 case (4):
                    Machine.Pump_A_Valve_Position = 1;
                    break;
                 default:
                    break;
             }
             grpPumpA.Enabled = true;

         }

         private void btnAspirtaeB_Click(object sender, EventArgs e)
         {
             btnAspirtaeB.Enabled = false;
             int ValueToAspirate = 0;
             int AspirateSpeed = 0;
             int.TryParse(txtAspirtaeB.Text, out ValueToAspirate);
             int.TryParse(txtPumpBAspirateSpeed.Text, out AspirateSpeed);
             Status = " Pump A aspirates " + txtAspirtaeB.Text + " at " + txtPumpBAspirateSpeed.Text;

             Machine.Aspirate_Pump_B(ValueToAspirate, AspirateSpeed);
         }

         private void btnDispenseB_Click(object sender, EventArgs e)
         {
             btnDispenseB.Enabled = false;
             int ValueToDispense = 0;
             int DispenseSpeed = 0;
             int.TryParse(txtDispenseB.Text, out ValueToDispense);
             int.TryParse(txtPumpBDispenseSpeed.Text, out DispenseSpeed);
             Status = " Pump B dispensess " + txtDispenseB.Text + " at " + txtPumpBDispenseSpeed.Text;

             Machine.Dispense_Pump_B(ValueToDispense, DispenseSpeed);
         }

         private void btnPumpB_01_Click(object sender, EventArgs e)
         {
             grpPumpB.Enabled = false;
             Machine.Pump_B_Valve_Position = 1;
             grpPumpB.Enabled = true;

         }
         private void btnPumpB_02_Click(object sender, EventArgs e)
         {
             grpPumpB.Enabled = false;
             Machine.Pump_B_Valve_Position = 2;
             grpPumpB.Enabled = true;
         }
         private void btnPumpB_03_Click(object sender, EventArgs e)
         {
             grpPumpB.Enabled = false;
             Machine.Pump_B_Valve_Position = 3;
             grpPumpB.Enabled = true;
         }
         private void btnPumpB_04_Click(object sender, EventArgs e)
         {
             grpPumpB.Enabled = false;
             Machine.Pump_B_Valve_Position = 4;
             grpPumpB.Enabled = true;
         }
         private void btnPumpB_05_Click(object sender, EventArgs e)
         {
             grpPumpB.Enabled = false;
             Machine.Pump_B_Valve_Position = 5;
             grpPumpB.Enabled = true;
         }
         private void btnPumpB_06_Click(object sender, EventArgs e)
         {
             grpPumpB.Enabled = false;
             Machine.Pump_B_Valve_Position = 6;
             grpPumpB.Enabled = true;
         }
         private void btnPumpB_07_Click(object sender, EventArgs e)
         {
             grpPumpB.Enabled = false;
             Machine.Pump_B_Valve_Position = 7;
             grpPumpB.Enabled = true;
         }
         private void btnPumpB_08_Click(object sender, EventArgs e)
         {
             grpPumpB.Enabled = false;
             Machine.Pump_B_Valve_Position = 8;
             grpPumpB.Enabled = true;
         }




#endregion

#region HPLC pump Controls
         private void btnPumpStart_Click(object sender, EventArgs e)
         {
             switch (Machine.HPLCPumpStatus())
             {
                 case ("On"):
                    Machine.Stop_HPLC_Pump();
                    //btnPumpStart.BackColor = System.Drawing.Color.Green;
                    //btnPumpStart.Text = " Pump is OFF";
                     break;
                 case("Off"):
                     Machine.Start_HPLC_Pump();
                     //btnPumpStart.BackColor = System.Drawing.Color.LightBlue;
                    //btnPumpStart.Text = " Pump is ON";
                     break;
                 case("Overload"):
                     Machine.Start_HPLC_Pump();
                     break;
                 default:
                     break;

             }
         }

#endregion

        private void btnRunVoltage_Click(object sender, EventArgs e)
        {
            Machine.GenerateVoltageWave(0, 5);
        }
        private void btnStopRecirculation_Click(object sender, EventArgs e)
        {
            btnRecirculation.Enabled = true;
            Machine.StopRecirculation("b");
        }
        private void btnRatchetB_Click(object sender, EventArgs e)
        {
             btnRatchetB.Enabled = false;
             int _PushVolume;
             int _PushSpeed;
             int _PullVolume;
             int _PullSpeed;
             int _repetitions;
             int.TryParse(txtPullVolume.Text, out _PullVolume);
             int.TryParse(txtPullSpeed.Text, out _PullSpeed);
             int.TryParse(txtPushVolume.Text, out _PushVolume);
             int.TryParse(txtPushSpeed.Text, out _PushSpeed);
             int.TryParse(txtRatchetRepetitions.Text, out _repetitions);
             Status = "Ratcheting: push " + txtPushVolume.Text + "/" + txtPushSpeed.Text + "; pull: " + txtPullVolume.Text + "/" + txtPullSpeed.Text;

             if (_PushVolume > _PullVolume)
             {
                 Machine.Ratchet_Pump_B(_PushVolume, _PushSpeed, _PullVolume, _PullSpeed, _repetitions);
             }
             else
             {
                 MessageBox.Show("Invalid Parameters");
                 btnRatchetB.Enabled = true;
             }

         }
        private void btnStopRatchetingB_Click(object sender, EventArgs e)
        {
            Machine.StopRatchetingB();
        }
        

        private void btnCleaningAbort_Click(object sender, EventArgs e)
        {
            Machine.StopCleaning();
        }

        private void btnCleaningStart_Click(object sender, EventArgs e)
        {
            Machine.Clean_the_System();
            btnCleaningAbort.Enabled = true;
        }





       


    
            



    }
}
