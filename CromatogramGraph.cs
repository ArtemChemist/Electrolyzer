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

namespace HPLC_Software
{

    public partial class CromatogramGraph : Form

 {
     
     frmGUI MainGUI;
     double[] ArrayToDraw;
             
        //Constructors

     public CromatogramGraph(frmGUI pub, double[] ATD)
        {
            //subscription to the timer events
            pub.GUITimer.Tick += UpdateGraph;
            this.Disposed += StopGraphing;
            ArrayToDraw = ATD;
            MainGUI = pub;

            InitializeComponent();
            System.Windows.Forms.DataVisualization.Charting.Series Trace = new System.Windows.Forms.DataVisualization.Charting.Series();
            if (ChromatogramChart.IsDisposed )
            {
                this.ChromatogramChart.Series.Add(Trace);
            }
            ChromatogramChart.Series[0].Color = System.Drawing.Color.Blue;
            ChromatogramChart.Series[0].BorderWidth = 5;
            ChromatogramChart.Size = new System.Drawing.Size(SystemInformation.PrimaryMonitorSize.Width, SystemInformation.PrimaryMonitorSize.Height / 2);
            ChromatogramChart.ChartAreas[0].AxisX.Maximum = MainGUI.GetRunTime();
        }

        int x = 0;
        void UpdateGraph(object sender, EventArgs e)
        {
            x = MainGUI.CurrentTick();
            if (x <= MainGUI.GetRunTime()) 
            {
                
                this.ChromatogramChart.Series[0].Points.AddXY(x, ArrayToDraw[x-1]);
                this.ChromatogramChart.Invalidate();
                this.ChromatogramChart.Update();
            }
            else
            {
                MainGUI.GUITimer.Tick -= UpdateGraph;
            }
        }

        void StopGraphing(object sender, EventArgs e)
        {
            MainGUI.GUITimer.Tick -= UpdateGraph;
            this.Disposed -= StopGraphing;
        }
    }
}
