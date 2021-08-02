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

    public partial class CromatogramGraph : Form

 {
     
     frmGUI MainGUI;
     Dictionary<string, double[]> DictionaryOfArraysToDraw;
             
        //Constructors

     public CromatogramGraph(frmGUI pub, Dictionary<string, double[]> DAT )
     {
         //subscription to the timer events
         pub.GUITimer.Tick += UpdateGraph;
         this.Disposed += StopGraphing;
         MainGUI = pub;
         DictionaryOfArraysToDraw = DAT;

         InitializeComponent();

         foreach (KeyValuePair<string, double[]> entry in DAT)
         {
            System.Windows.Forms.DataVisualization.Charting.Series NewTrace = new System.Windows.Forms.DataVisualization.Charting.Series();
            NewTrace.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            NewTrace.BorderWidth = 3;
            NewTrace.Name = entry.Key;
            ChromatogramChart.Series.Add(NewTrace);
         }
         ChromatogramChart.Size = new System.Drawing.Size(SystemInformation.PrimaryMonitorSize.Width, SystemInformation.PrimaryMonitorSize.Height / 2);
         ChromatogramChart.ChartAreas[0].AxisX.Maximum = MainGUI.GetRunTime();

         txtXmax.Text = ChromatogramChart.ChartAreas[0].AxisX.Maximum.ToString();
         txtXmin.Text = "0";
     }

        int x = 0;
        void UpdateGraph(object sender, EventArgs e)
        {
            x = MainGUI.CurrentTick();
            if (x <= MainGUI.GetRunTime()) 
            {
                foreach (System.Windows.Forms.DataVisualization.Charting.Series i in this.ChromatogramChart.Series)
                {
                    if (DictionaryOfArraysToDraw.ContainsKey(i.Name))
                    {
                        i.Points.AddXY(x, DictionaryOfArraysToDraw[i.Name][x - 1]);
                    }
                }
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
        #region Min and Max on Graph
        private void txtXmax_TextChanged(object sender, EventArgs e)
        {
            double dblXmax;
            double dblXmin;
            Double.TryParse(txtXmax.Text, out dblXmax);
            Double.TryParse(txtXmin.Text, out dblXmin);
            if (dblXmin < dblXmax)
            {
                ChromatogramChart.ChartAreas[0].AxisX.Minimum = (int)(dblXmin);
                ChromatogramChart.ChartAreas[0].AxisX.Maximum = (int)(dblXmax);

            }
        }

        private void txtXmin_TextChanged(object sender, EventArgs e)
        {
            double dblXmax;
            double dblXmin;
            Double.TryParse(txtXmax.Text, out dblXmax);
            Double.TryParse(txtXmin.Text, out dblXmin);
            if (dblXmin < dblXmax)
            {
                ChromatogramChart.ChartAreas[0].AxisX.Minimum = (int)(dblXmin);
                ChromatogramChart.ChartAreas[0].AxisX.Maximum = (int)(dblXmax);

            }
        }

        private void txtYmin_TextChanged(object sender, EventArgs e)
        {
            double dblYmax;
            double dblYmin;
            Double.TryParse(txtYmax.Text, out dblYmax);
            Double.TryParse(txtYmin.Text, out dblYmin);
            if (dblYmin < dblYmax)
            {
                ChromatogramChart.ChartAreas[0].AxisY.Minimum = dblYmin;
                ChromatogramChart.ChartAreas[0].AxisY.Maximum = dblYmax;
            }
        }

        private void txtYmax_TextChanged(object sender, EventArgs e)
        {
            double dblYmax;
            double dblYmin;
            Double.TryParse(txtYmax.Text, out dblYmax);
            Double.TryParse(txtYmin.Text, out dblYmin);
            if (dblYmin < dblYmax)
            {
                ChromatogramChart.ChartAreas[0].AxisY.Minimum = dblYmin;
                ChromatogramChart.ChartAreas[0].AxisY.Maximum = dblYmax;
            }
        }
        #endregion

 }
}
