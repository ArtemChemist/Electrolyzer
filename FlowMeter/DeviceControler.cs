using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Threading;
using System.IO;


namespace RadElChemBox
{
    class DeviceControler
    {
        public DeviceControler(string CalibrationFileName)
        {
            strCalibrationFileName = CalibrationFileName;
            dictCalibration = new Dictionary<string, double>();
            ReadCalibration();
        }
#region Local Variables

        //HARDWARE DRIVERS
        private DAQBoardController NIController;
        private Hamilton_Syringe_Controller Syringes;
        private K501_Pump Pump;

        //EVENTS TELLING GUI THAT IT NEEDS TO UPDATE ITSELF
        public event EventHandler<EventArgs> ValvePositionChnaged;
        public event EventHandler<EventArgs> SyringesChanged;
        public event EventHandler<EventArgs> HPLCPumpChanged;
        public event EventHandler<EventArgs> PumpAFree;
        public event EventHandler<EventArgs> PumpBFree;
        public event EventHandler<EventArgs> PumpCFree;
        
        //CALIBRATION PARAMATERS
        //
        //Calibrations are assumed to be in the format:
        //dblHPLC_Pump_Flow  = dbl_AGasPressure + dbl_BGasPressure*VoltageToBeSet
        private double dbl_AGasPressure;
        private double dbl_BGasPressure;
        private double dbl_ARad1;
        private double dbl_BRad1;
        private double dbl_ARad2;
        private double dbl_BRad2;
        private double dbl_ARad3;
        private double dbl_BRad3;
        private double dbl_Syringe_A_Volume;
        private double dbl_Syringe_B_Volume;
        private double dbl_Syringe_C_Volume;
        private double dbl_Syringe_A_Valve;
        private double dbl_Syringe_B_Valve;
        private double dbl_Syringe_C_Valve;
        private bool blAveragingChecked;

        private Dictionary<string, double> dictCalibration;

        //OPERATION PARAMATERS
        private double dblGasPressure = 0; //Current Gas Pressure
        private string strCalibrationFileName; //Name of the file with calibration parameters
        #endregion

        public bool Initialize_NI_Device(string DeviceName)
        {
            if (NIController == null)
            {
                NIController = new DAQBoardController(DeviceName);
            }
            return true;
        }
        public bool Initialize_Syringes(string ComPort_For_Syringes)
        {
            if (Syringes == null)
            {
                Syringes = new Hamilton_Syringe_Controller(ComPort_For_Syringes);
            }
            Syringes.OpenPort();

            if (SyringesChanged != null) { SyringesChanged(this, new EventArgs()); }

            return true;
         
        }
        public bool Initialize_Syringes_Pumps()
        {
            bool success = true;
            if (Syringes != null)
            {
                success = Syringes.InitializeAllSyringes();
                Syringes.SetValve("a", Convert.ToInt16(dictCalibration["Syringe_a_Valve"]).ToString());
                Syringes.SetValve("b", Convert.ToInt16(dictCalibration["Syringe_b_Valve"]).ToString());
                //Syringes.SetValve("c", Convert.ToInt16(dictCalibration["Syringe_c_Valve"]).ToString());

                if (SyringesChanged != null) { SyringesChanged(this, new EventArgs()); }
                return success;
            }
            else
            {
                if (SyringesChanged != null) { SyringesChanged(this, new EventArgs()); }
                return false;
            }
        }
        public bool Initialize_Pump(string PortForPump)
        {
           if (Pump == null)
           {
               Pump = new K501_Pump();
           }
           if (Pump.InitializePump(PortForPump)) {return true;}
           else {return false;}
        }


#region Calibration
        public double Syringe_A_Volume
        {
            get { return dictCalibration["Syringe_a_Volume"]; }
            set { dictCalibration["Syringe_a_Volume"] = value; }
        }

        public double Syringe_A_Valve
        {
            get { return dictCalibration["Syringe_a_Valve"]; }
            set { dictCalibration["Syringe_a_Valve"] = value; }
        }

        public double Syringe_B_Volume
        {
            get { return dictCalibration["Syringe_b_Volume"]; }
            set { dictCalibration["Syringe_b_Volume"] = value; }
        }

        public double Syringe_B_Valve
        {
            get { return dictCalibration["Syringe_b_Valve"]; }
            set { dictCalibration["Syringe_b_Valve"] = value; }
        }

        public double Syringe_C_Volume
        {
            get { return dictCalibration["Syringe_c_Volume"]; }
            set { dictCalibration["Syringe_c_Volume"] = value; }
        }

        public double Syringe_C_Valve
        {
            get { return dictCalibration["Syringe_c_Valve"]; }
            set { dictCalibration["Syringe_c_Valve"] = value; }
        }

        public double AGasPressure
        {
            get { return dictCalibration["AGasPressure"]; }
            set { dictCalibration["AGasPressure"] = value; }
        }
        public double BGasPressure
        {
            get { return dictCalibration["BGasPressure"]; }
            set { dictCalibration["BGasPressure"] = value; }
        }
        public double ARad1
        {
            get { return dictCalibration["ARad1"]; }
            set { dictCalibration["ARad1"] = value; }
        }
        public double BRad1
        {
            get { return dictCalibration["BRad1"]; }
            set { dictCalibration["BRad1"] = value; }
        }
        public double ARad2
        {
            get { return dictCalibration["ARad2"]; }
            set { dictCalibration["ARad2"] = value; }
        }
        public double BRad2
        {
            get { return dictCalibration["BRad2"]; }
            set { dictCalibration["BRad2"] = value; }
        }
        public double ARad3
        {
            get { return dictCalibration["ARad3"]; }
            set { dictCalibration["ARad3"] = value; }
        }
        public double BRad3
        {
            get { return dictCalibration["BRad3"]; }
            set { dictCalibration["BRad3"] = value; }
        }
        public bool AveragingChecked
        {
            get { return blAveragingChecked; }
            set { blAveragingChecked = value; }
        }

        private void ReadCalibration()
        {
            try
            {
                string CurrentString = "This is the first line";
                //Initialize file handlers
                System.IO.StreamReader CalibrationFileStream = new System.IO.StreamReader(strCalibrationFileName);

                CurrentString = CalibrationFileStream.ReadLine();
                Double.TryParse(CurrentString, out dbl_AGasPressure);
                dictCalibration.Add("AGasPressure", dbl_AGasPressure);

                CurrentString = CalibrationFileStream.ReadLine();
                Double.TryParse(CurrentString, out dbl_BGasPressure);
                dictCalibration.Add("BGasPressure", dbl_BGasPressure);

                CurrentString = CalibrationFileStream.ReadLine();
                Double.TryParse(CurrentString, out dbl_ARad1);
                dictCalibration.Add("ARad1", dbl_ARad1);


                CurrentString = CalibrationFileStream.ReadLine();
                Double.TryParse(CurrentString, out dbl_BRad1);
                dictCalibration.Add("BRad1", dbl_BRad1);

                CurrentString = CalibrationFileStream.ReadLine();
                Double.TryParse(CurrentString, out dbl_ARad2);
                dictCalibration.Add("ARad2", dbl_ARad2);

                CurrentString = CalibrationFileStream.ReadLine();
                Double.TryParse(CurrentString, out dbl_BRad2);
                dictCalibration.Add("BRad2", dbl_BRad2);

                CurrentString = CalibrationFileStream.ReadLine();
                Double.TryParse(CurrentString, out dbl_ARad3);
                dictCalibration.Add("ARad3", dbl_ARad3);

                CurrentString = CalibrationFileStream.ReadLine();
                Double.TryParse(CurrentString, out dbl_BRad3);
                dictCalibration.Add("BRad3", dbl_BRad3);

                CurrentString = CalibrationFileStream.ReadLine();
                Double.TryParse(CurrentString, out dbl_Syringe_A_Volume);
                dictCalibration.Add("Syringe_a_Volume", dbl_Syringe_A_Volume);

                CurrentString = CalibrationFileStream.ReadLine();
                Double.TryParse(CurrentString, out dbl_Syringe_B_Volume);
                dictCalibration.Add("Syringe_b_Volume", dbl_Syringe_B_Volume);

                CurrentString = CalibrationFileStream.ReadLine();
                Double.TryParse(CurrentString, out dbl_Syringe_C_Volume);
                dictCalibration.Add("Syringe_c_Volume", dbl_Syringe_C_Volume);

                CurrentString = CalibrationFileStream.ReadLine();
                Double.TryParse(CurrentString, out dbl_Syringe_A_Valve);
                dictCalibration.Add("Syringe_a_Valve", dbl_Syringe_A_Valve);

                CurrentString = CalibrationFileStream.ReadLine();
                Double.TryParse(CurrentString, out dbl_Syringe_B_Valve);
                dictCalibration.Add("Syringe_b_Valve", dbl_Syringe_B_Valve);

                CurrentString = CalibrationFileStream.ReadLine();
                Double.TryParse(CurrentString, out dbl_Syringe_C_Valve);
                dictCalibration.Add("Syringe_c_Valve", dbl_Syringe_C_Valve);

                CurrentString = CalibrationFileStream.ReadLine();
                if (CurrentString == "yes")
                {
                    blAveragingChecked = true;
                }
                if (CurrentString == "no")
                {
                    blAveragingChecked = false;
                }

                CalibrationFileStream.Close();
            }
            catch
            {
                MessageBox.Show("Problems reading calibration file", "Device controller");
            }

        }
        public void SaveCalibration()
        {
            //Initialize file handler
            System.IO.StreamWriter CalibrationFileStream = new System.IO.StreamWriter(strCalibrationFileName);

            CalibrationFileStream.WriteLine(dictCalibration["AGasPressure"].ToString());
            CalibrationFileStream.WriteLine(dictCalibration["BGasPressure"].ToString());
            CalibrationFileStream.WriteLine(dictCalibration["ARad1"].ToString());
            CalibrationFileStream.WriteLine(dictCalibration["BRad1"].ToString());
            CalibrationFileStream.WriteLine(dictCalibration["ARad2"].ToString());
            CalibrationFileStream.WriteLine(dictCalibration["BRad2"].ToString());
            CalibrationFileStream.WriteLine(dictCalibration["ARad3"].ToString());
            CalibrationFileStream.WriteLine(dictCalibration["BRad3"].ToString());
            CalibrationFileStream.WriteLine(dictCalibration["Syringe_a_Volume"].ToString());
            CalibrationFileStream.WriteLine(dictCalibration["Syringe_b_Volume"].ToString());
            CalibrationFileStream.WriteLine(dictCalibration["Syringe_c_Volume"].ToString());
            CalibrationFileStream.WriteLine(dictCalibration["Syringe_a_Valve"].ToString());
            CalibrationFileStream.WriteLine(dictCalibration["Syringe_b_Valve"].ToString());
            CalibrationFileStream.WriteLine(dictCalibration["Syringe_c_Valve"].ToString());

            if (blAveragingChecked)
            {
                CalibrationFileStream.WriteLine("yes");
            }
            else CalibrationFileStream.WriteLine("no");


            CalibrationFileStream.Close();
        }

        #endregion

#region Analog Driven Devices

        public double Gas_Pressure
        {
            get { return dblGasPressure; }
            set
            {
                dblGasPressure = value;
                double VoltageToSet = (value - dbl_AGasPressure) / dbl_BGasPressure;
                if ((VoltageToSet >= 0) && (VoltageToSet < 5)) { NIController.Set_Voltage_Analog_Port_1(VoltageToSet); }
            }
        }

        public double Current_Value_On_Rad_Detector_1
        {
            get { return dbl_ARad1 + dbl_BRad1 * NIController.Current_Value_On_Analog_In_0; }
            set { }
        }

        public double Current_Value_On_Rad_Detector_2
        {
            get { return dbl_ARad2 + dbl_BRad2 * NIController.Current_Value_On_Analog_In_1; }
            set { }
        }

        public double Current_Value_On_Rad_Detector_3
        {
            get { return dbl_ARad3 + dbl_BRad3 * NIController.Current_Value_On_Analog_In_2; }
            set { }
        }

        public int Current_Counter_Value
        {
            get { return NIController.CurrentCounter; }
            set{}
        }
#endregion

#region Solenoid Valves

        public string Valve1
        {
            get
            {
                switch (NIController.DigitalIO_00)
                {
                    case true:
                        return "On";
                    case false:
                        return "Off";
                    default:
                        return "Error";
                }
            }
            set
            {
                if (value == "On") { NIController.DigitalIO_00 = true; }
                if (value == "Off")
                { 
                    NIController.DigitalIO_00 = false;
                }
                ValvePositionChnaged(this, new EventArgs());
                Thread.Sleep(100);
            }
        }
        public string Valve2
        {
            get
            {
                switch (NIController.DigitalIO_01)
                {
                    case true:
                        return "On";
                    case false:
                        return "Off";
                    default:
                        return "Error";
                }
            }

            set
            {
                if (value == "On")  { NIController.DigitalIO_01 = true;  }
                if (value == "Off") { NIController.DigitalIO_01 = false; }
                ValvePositionChnaged(this, new EventArgs());
                Thread.Sleep(100);
            }
        }
        public string Valve3
        {
            get
            {
                switch (NIController.DigitalIO_02)
                {
                    case true:
                        return "On";
                    case false:
                        return "Off";
                    default:
                        return "Error";
                }
            }

            set
            {
                if (value == "On")  { NIController.DigitalIO_02 = true;  }
                if (value == "Off") { NIController.DigitalIO_02 = false; }
                ValvePositionChnaged(this, new EventArgs());
                Thread.Sleep(100);
            }       
        }

#endregion

#region Loop Valves

        public string ValveA
        {
            get
            {
                switch (NIController.DigitalIO_05)
                {
                    case true:
                        return "Solid";
                    case false:
                        return "Dotted";
                    default:
                        MessageBox.Show("Error in Device Controller Valve A");
                        return "Error in Device Controller Valve A";
                        
                }
            }
            set
            {
                if (value == "Solid") { NIController.DigitalIO_05 = true; }
                if (value == "Dotted") { NIController.DigitalIO_05 = false; }
                Thread.Sleep(200);
                ValvePositionChnaged(this, new EventArgs());
            }
        }
        public string ValveB
        {
            get
            {
                switch (NIController.DigitalIO_03)
                {
                    case true:
                        return "Solid";
                    case false:
                        return "Dotted";
                    default:
                        MessageBox.Show("Error in Device Controller Valve B");
                        return "Error";
                }
            }
            set
            {
                if (value == "Solid") { NIController.DigitalIO_03 = true; }
                if (value == "Dotted") { NIController.DigitalIO_03 = false; }
                Thread.Sleep(200);
                ValvePositionChnaged(this, new EventArgs());
            }
        }
        public string ValveC
        {
            get
            {
                switch (NIController.DigitalIO_04)
                {
                    case true:
                        return "Solid";
                    case false:
                        return "Dotted";
                    default:
                        MessageBox.Show("Error in Device Controller Valve C");
                        return "Error";
                }
            }
            set
            {
                if (value == "Solid") { NIController.DigitalIO_04 = true; }
                if (value == "Dotted") { NIController.DigitalIO_04 = false; }
                Thread.Sleep(200);
                ValvePositionChnaged(this, new EventArgs());
            }
        }

#endregion

#region Distribution Valves

        public int ValveD
        {
          get
            {
               if (     (NIController.DigitalIO_06 == true) & 
                        (NIController.DigitalIO_07 == false)&
                        (NIController.DigitalIO_08 == false)
                  ) {return 1;}
               if (     (NIController.DigitalIO_06 == false)& 
                        (NIController.DigitalIO_07 == true) & 
                        (NIController.DigitalIO_08 == false)
                  ) {return 2;}
               if(      (NIController.DigitalIO_06 == true) & 
                        (NIController.DigitalIO_07 == true) & 
                        (NIController.DigitalIO_08 == false)
                  ) {return 3;}
               if(      (NIController.DigitalIO_06 == false)& 
                        (NIController.DigitalIO_07 == false)& 
                        (NIController.DigitalIO_08 == true)
                 )  {return 4;}
               if(      (NIController.DigitalIO_06 == true) &
                        (NIController.DigitalIO_07 == false)&
                        (NIController.DigitalIO_08 == true)
                 )  {return 5;}
               if (     (NIController.DigitalIO_06 == false) &
                        (NIController.DigitalIO_07 == true) &
                        (NIController.DigitalIO_08 == true)
                 ) { return 6; }
               else { return 0; }
            }
          set
            {
                switch (value)
                {
                    case 1:
                        NIController.DigitalIO_06 = true;
                        NIController.DigitalIO_07 = false;
                        NIController.DigitalIO_08 = false;
                        break;
                    case 2:
                        NIController.DigitalIO_06 = false;
                        NIController.DigitalIO_07 = true;
                        NIController.DigitalIO_08 = false;
                        break;
                    case 3:
                        NIController.DigitalIO_06 = true;
                        NIController.DigitalIO_07 = true;
                        NIController.DigitalIO_08 = false;
                        break;
                    case 4:
                        NIController.DigitalIO_06 = false;
                        NIController.DigitalIO_07 = false;
                        NIController.DigitalIO_08 = true;
                        break;
                    case 5:
                        NIController.DigitalIO_06 = true;
                        NIController.DigitalIO_07 = false;
                        NIController.DigitalIO_08 = true;
                        break;
                    case 6:
                        NIController.DigitalIO_06 = false;
                        NIController.DigitalIO_07 = true;
                        NIController.DigitalIO_08 = true;
                        break;
                    default:
                        MessageBox.Show("Valve D Controller Error!");
                        break;
                }
                Thread.Sleep(200);
                ValvePositionChnaged(this, new EventArgs());
               }
        }
        public int ValveE
        {
          get
            {
                if (     (NIController.DigitalIO_09 == true) &
                         (NIController.DigitalIO_10 == false)&
                         (NIController.DigitalIO_11 == false)
                   ) { return 1; }
                if (     (NIController.DigitalIO_09 == false)&
                         (NIController.DigitalIO_10 == true) &
                         (NIController.DigitalIO_11 == false)
                   ) { return 2; }
                if (     (NIController.DigitalIO_09 == true) &
                         (NIController.DigitalIO_10 == true) &
                         (NIController.DigitalIO_11 == false)
                   ) { return 3; }
                if (     (NIController.DigitalIO_09 == false)&
                         (NIController.DigitalIO_10 == false)&
                         (NIController.DigitalIO_11 == true)
                  ) { return 4; }
                if (     (NIController.DigitalIO_09 == true) &
                         (NIController.DigitalIO_10 == false)&
                         (NIController.DigitalIO_11 == true)
                  ) { return 5; }
                if (     (NIController.DigitalIO_09 == false) &
                         (NIController.DigitalIO_10 == true) &
                         (NIController.DigitalIO_11 == true)
                  ) { return 6; }
                else { return 0; }
            }
          set
                {
                    switch (value)
                    {
                        case 1:
                            NIController.DigitalIO_09 = true;
                            NIController.DigitalIO_10 = false;
                            NIController.DigitalIO_11 = false;
                            break;
                        case 2:
                            NIController.DigitalIO_09 = false;
                            NIController.DigitalIO_10 = true;
                            NIController.DigitalIO_11 = false;
                            break;
                        case 3:
                            NIController.DigitalIO_09 = true;
                            NIController.DigitalIO_10 = true;
                            NIController.DigitalIO_11 = false;
                            break;
                        case 4:
                            NIController.DigitalIO_09 = false;
                            NIController.DigitalIO_10 = false;
                            NIController.DigitalIO_11 = true;
                            break;
                        case 5:
                            NIController.DigitalIO_09 = true;
                            NIController.DigitalIO_10 = false;
                            NIController.DigitalIO_11 = true;
                            break;
                        case 6:
                            NIController.DigitalIO_09 = false;
                            NIController.DigitalIO_10 = true;
                            NIController.DigitalIO_11 = true;
                            break;
                        default:
                            MessageBox.Show("Error!");
                            break;
                    }
                    Thread.Sleep(200);
                    ValvePositionChnaged(this, new EventArgs());
                }
        }

#endregion

#region Syringe pumps

        private Thread RecirculatingA;
        private Thread RecirculatingB;
        private Thread RatchetingB;

        public int Pump_A_Valve_Position
        {
            get
            {
                int position = Syringes.GetValvePosition("a");
                if ((position > 0) & (position < 8))
                {
                    return position;
                }
                else
                {
                    return -1;
                }

            }
            set
            {
                SetValvePosition("a", value);
            }
        }
        public int Pump_B_Valve_Position
        {
            get
            {
                int position = Syringes.GetValvePosition("b");
                if ((position > 0) & (position < 9))
                {
                    return position;
                }
                else
                {
                    return -1;
                }

            }
            set
            {
                SetValvePosition("b", value);
            }
        }
        public int Pump_C_Valve_Position
        {
            get
            {
                int position = Syringes.GetValvePosition("c");
                if ((position > 0) & (position < 9))
                {
                    return position;
                }
                else
                {
                    return -1;
                }

            }
            set
            {
                SetValvePosition("c", value);
            }
        }

        public bool Aspirate_Pump_A(int volume, int speed)
        {
            Thread AspiratingA = new Thread(() => Aspirate(volume, speed, "a"));
            AspiratingA.Start();
            return true;

        }
        public bool Aspirate_Pump_B(int volume, int speed)
        {
            Thread AspiratingB = new Thread(() => Aspirate(volume, speed, "b"));
            AspiratingB.Start();
            return true;
            
        }
        public bool Aspirate_Pump_C(int volume, int speed)
        {
            Thread AspiratingC = new Thread(() => Aspirate(volume, speed, "c"));
            AspiratingC.Start();
            return true;
        }

        public bool Dispense_Pump_A(int volume, int speed)
        {
            Thread DispensingA = new Thread(() => Dispense(volume, speed, "a"));
            DispensingA.Start();
            return true;
        }
        public bool Dispense_Pump_B(int volume, int speed)
        {
            Thread DispensingB = new Thread(() => Dispense(volume, speed, "b"));
            DispensingB.Start();
            return true;
        }
        public bool Dispense_Pump_C(int volume, int speed)
        {
            Thread DispensingC = new Thread(() => Dispense(volume, speed, "c"));
            DispensingC.Start();
            return true;
        }

        public void Recirculate_Pump_A(int volume, int dispensespeed, int repetitions)
        {
            RecirculatingA = new Thread(() => Recirculate("a", volume, dispensespeed, repetitions, 4, 1));
            RecirculatingA.Start();
        }
        public void Recirculate_Pump_B(int volume, int dispensespeed, int repetitions, int AspPos, int DispPos)
        {
            this.Valve1 = "On";
            this.ValveB = "Dotted";
            RecirculatingB = new Thread(() => Recirculate("b", volume, dispensespeed, repetitions, AspPos, DispPos));
            RecirculatingB.Start();
        }
        public void StopRecirculation(string Pump)
        {
            switch (Pump)
            {
                case ("a"):
                    if ((RecirculatingA != null) & RecirculatingA.IsAlive)
                     {
                        RecirculatingA.Abort();
                        PumpAFree(this, new EventArgs());
                     }
                break;
                case ("b"):
                if ((RecirculatingB!=null) & RecirculatingB.IsAlive)
                {
                    RecirculatingB.Abort();
                    PumpAFree(this, new EventArgs());
                }
                break;
            }      
        }
        public void Ratchet_Pump_B(int push_volume, int push_speed, int pull_volume, int pull_speed, int repetitions)
        {
            RatchetingB = new Thread(() => RatchetB(push_volume, push_speed, pull_volume, pull_speed, repetitions));
            RatchetingB.Start();
        }
        public void StopRatchetingB()
        {
            if ((RatchetingB != null) & RatchetingB.IsAlive)
            {
                RatchetingB.Abort();
                PumpBFree(this, new EventArgs());
            }
        }

        public double Pump_A_Position
        {
            get
            {
               return Syringe_A_Volume*Syringes.PumpCurrentPosition("a")/1000;
            }

               set {}
         }
        public double Pump_B_Position
        {
            get
            {
                return Syringe_B_Volume * Syringes.PumpCurrentPosition("b") / 1000;
            }

            set { }
        }
        public double Pump_C_Position
        {
            get
            {
                return Syringe_C_Volume * Syringes.PumpCurrentPosition("c") / 1000;
            }

            set { }
        }

        public void SetValve(string Pump, string ValveType)
        {
            Syringes.SetValve(Pump, ValveType);
        }

        private void Aspirate(int Volume, int Speed, string Pump)
        {
            if (
                ((Syringes.PumpCurrentPosition(Pump) + 1000 * Volume / dictCalibration["Syringe_" + Pump + "_Volume"]) <= 1000) &
                (Volume >= 0)
                )
            {
                Syringes.Aspirate(Pump, (int)(1000 * Volume / dictCalibration["Syringe_" + Pump + "_Volume"]), (int)dictCalibration["Syringe_" + Pump + "_Volume"] / Speed);
                switch (Pump)
                {
                    case ("a"):
                        if (PumpAFree != null)
                        {
                            PumpAFree(this, new EventArgs());
                        }
                        break;
                    case ("b"):
                        if (PumpBFree != null)
                        {
                            PumpBFree(this, new EventArgs());
                        }
                        break;
                    case ("c"):
                        if (PumpCFree != null)
                        {
                            PumpCFree(this, new EventArgs());
                        }
                        break;
                    default:
                        if (SyringesChanged != null)
                        {
                            SyringesChanged(this, new EventArgs());
                        }
                        break;
                }
            }
            else
            {
                MessageBox.Show("Not enough room in the pump " + Pump + ". Pump position " + Syringes.PumpCurrentPosition(Pump).ToString() + ". Requested " + (1000 * Volume / dictCalibration["Syringe_" + Pump + "_Volume"]).ToString(), "Device Controller. Aspirate.");
                //return false;
            }
        }
        public void Dispense(int volume, int speed, string Pump)
        {
            if (
                Syringes.PumpCurrentPosition(Pump) >= (1000 * volume / dictCalibration["Syringe_" + Pump + "_Volume"])
                )
            {
                Syringes.Dispense(Pump, (int)(1000 * volume / dictCalibration["Syringe_" + Pump + "_Volume"]), (int)(dictCalibration["Syringe_" + Pump + "_Volume"] / speed));
                switch (Pump)
                {
                    case ("a"):
                        if (PumpAFree != null)
                        {
                            PumpAFree(this, new EventArgs());
                        }
                        break;
                    case ("b"):
                        if (PumpBFree != null)
                        {
                            PumpBFree(this, new EventArgs());
                        }
                        break;
                    case ("c"):
                        if (PumpCFree != null)
                        {
                            PumpCFree(this, new EventArgs());
                        }
                        break;
                    default:
                        if (SyringesChanged != null)
                        {
                            SyringesChanged(this, new EventArgs());
                        }
                        break;
                }
                
            }
            else
            {
                MessageBox.Show("Not enough liquid in the pump " + Pump + ". Pump position " + Syringes.PumpCurrentPosition(Pump).ToString() + ". Requested " + (1000 * volume / dictCalibration["Syringe_" + Pump + "_Volume"]).ToString(), "Device Controller. Dispense.");
            }
        }
        private void SetValvePosition(string Pump, int position)
        {
            if ((position > 0) & (position < 9))
            {
                Syringes.SetValvePosition(Pump, position);
                if (Syringes.GetValvePosition(Pump) == position)
                {
                    if (SyringesChanged != null)
                    {
                        SyringesChanged(this, new EventArgs());
                    }
                }
                else
                {
                    MessageBox.Show("Cannot set syringe "+Pump+" valve position", "Device Controller");
                }
            }
            switch (Pump)
            {
                case ("a"):
                    if (PumpAFree != null)
                    {
                        PumpAFree(this, new EventArgs());
                    }
                    break;
                case ("b"):
                    if (PumpBFree != null)
                    {
                        PumpBFree(this, new EventArgs());
                    }
                    break;
                case ("c"):
                    if (PumpCFree != null)
                    {
                        PumpCFree(this, new EventArgs());
                    }
                    break;
                default:
                    if (SyringesChanged != null)
                    {
                        SyringesChanged(this, new EventArgs());
                    }
                    break;
            }
        }

        public void RatchetB(int push_volume, int push_speed, int pull_volume, int pull_speed, int repetitions)
        {
            for (int i = 0; i < repetitions; i++)
            {
                while (Syringes.PumpIsBusy("b"))
                { Thread.Sleep(50); }
                Syringes.Dispense("b", (int)(1000 * push_volume / dictCalibration["Syringe_b_Volume"]), (int)(dictCalibration["Syringe_b_Volume"] / push_speed));
                while (Syringes.PumpIsBusy("b"))
                { Thread.Sleep(50); }
                Syringes.Aspirate("b", (int)(1000 * pull_volume / dictCalibration["Syringe_b_Volume"]), (int)dictCalibration["Syringe_b_Volume"] / pull_speed);
            }
            if (PumpBFree != null)
            {
                PumpBFree(this, new EventArgs());
            }
        }

        public void Recirculate(string Pump, int volume, int dispensespeed, int repetitions, int AspPos, int DispPos )
        {
            double SyringeVolume = dictCalibration["Syringe_" + Pump + "_Volume"];

            for (int i = 0; i < repetitions; i++)
            {
                
                Syringes.SetValvePosition(Pump, AspPos);
                SyringesChanged(this, new EventArgs());

                Syringes.Aspirate(Pump, (int)(1000 * volume / SyringeVolume), (int)SyringeVolume / 200);
                SyringesChanged(this, new EventArgs());

                Syringes.SetValvePosition(Pump, DispPos);
                SyringesChanged(this, new EventArgs());

                Syringes.Dispense(Pump, (int)(1000 * volume / SyringeVolume), (int)SyringeVolume / dispensespeed);
                SyringesChanged(this, new EventArgs());
            }
            switch (Pump)
            {
              case ("a"):
                    if (PumpAFree != null)
                    {
                        //PumpDoneEventArgs PumpDoneArgs = new PumpDoneEventArgs();
                        //PumpDoneArgs.Pump = Pump;
                        //PumpAFree(this, PumpDoneArgs);
                        PumpAFree(this, new EventArgs());
                    }
             break;
             case ("b"):
                    
                    if (PumpBFree != null)
                    { PumpBFree(this, new EventArgs()); }
             break;
           }

        }

        public bool Syringes_Are_Busy()
        {
            bool answer = Syringes.PumpIsBusy("a");
            return answer;
        }
#endregion

#region Potentiostat
        public void GenerateVoltageWave(double start, double finish)
        {
            double[] Voltage = new double[5000];
            for (int i = 0; i < 2500; i++)
            {
                Voltage[i] = start + i * (finish - start) / 2500;
                Voltage[4999 - i] = finish - i * (finish - start) / 2500;
            }
            for (int i = 0; i < 2500; i++)
            {
                Voltage[2500+i] = finish - i * (finish - start) / 2500;
            }
            NIController.Run_Wave_On_AO_0(Voltage);
        }
#endregion

#region HPLC Pump
        public double HPLC_Pump_Flow
        {
            get 
            {
                return Math.Round(Pump.Flow/1000, 2);
            }
            set
            {
                if ((value >= 0) & (value < 9.99))
                {
                    Pump.Flow = 1000 * value;
                    HPLCPumpChanged(this, new EventArgs());
                }
            }
        }
        public void Start_HPLC_Pump()
        {
            Pump.Start();
            HPLCPumpChanged(this, new EventArgs());

        }
        public void Stop_HPLC_Pump()
        {
           Pump.Stop();
           HPLCPumpChanged(this, new EventArgs());
        }
        public double HPLCPumpPressure()
        {
            return Pump.CurrentPressure();
        }
        public string HPLCPumpStatus()
        {
            return Pump.HPLCPumpStatus();
        }
        #endregion

        #region Unit Ops
        private Thread Cleaning;
        public void Clean_the_System()
        {
            Cleaning = new Thread(() => Clean());
            Cleaning.Start();
        }
        public void StopCleaning()
        {
            if ((Cleaning != null) & Cleaning.IsAlive)
            {
                Cleaning.Abort();
                PumpBFree(this, new EventArgs());
            }
        }
        public void Clean()
        {
            double SyringeVolume = dictCalibration["Syringe_b_Volume"];
            string Pump = "b";

            //Clean the vial and the pass around
            this.Valve1 = "On";
            this.ValveB = "Solid";
            for (int i = 0; i < 3; i++)
            {
                Syringes.SetValvePosition(Pump, 4);
                SyringesChanged(this, new EventArgs());

                Syringes.Aspirate(Pump, (int)(1000 * 2000 / SyringeVolume), (int)SyringeVolume / 200);
                SyringesChanged(this, new EventArgs());

                Syringes.SetValvePosition(Pump, 6);
                SyringesChanged(this, new EventArgs());

                Syringes.Dispense(Pump, (int)(1000 * 2000 / SyringeVolume), (int)SyringeVolume / 100);
                SyringesChanged(this, new EventArgs());

                Syringes.SetValvePosition(Pump, 3);
                SyringesChanged(this, new EventArgs());

                Syringes.Aspirate(Pump, (int)(1000 * 2000 / SyringeVolume), (int)SyringeVolume / 200);
                SyringesChanged(this, new EventArgs());

                Syringes.SetValvePosition(Pump, 5);
                SyringesChanged(this, new EventArgs());

                Syringes.Dispense(Pump, (int)(1000 * 2000 / SyringeVolume), (int)SyringeVolume / 50);
                SyringesChanged(this, new EventArgs());

            }

            //Clean the cell and the pass to waste
            this.Valve1 = "Off";
            this.ValveB = "Dotted";
            for (int i = 0; i < 10; i++)
            {
                Syringes.SetValvePosition(Pump, 4);
                SyringesChanged(this, new EventArgs());

                Syringes.Aspirate(Pump, (int)(1000 * 2000 / SyringeVolume), (int)SyringeVolume / 200);
                SyringesChanged(this, new EventArgs());

                Syringes.SetValvePosition(Pump, 6);
                SyringesChanged(this, new EventArgs());

                Syringes.Dispense(Pump, (int)(1000 * 2000 / SyringeVolume), (int)SyringeVolume / 50);
                SyringesChanged(this, new EventArgs());

            }

        //Tell GUI we are done
           if (PumpBFree != null) { PumpBFree(this, new EventArgs()); }        
        }
        #endregion
    }
}