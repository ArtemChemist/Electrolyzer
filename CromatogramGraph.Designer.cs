namespace HPLC_Software
{
    partial class CromatogramGraph
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.ChromatogramChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.ChromatogramChart)).BeginInit();
            this.SuspendLayout();
            // 
            // ChromatogramChart
            // 
            chartArea1.Name = "Trace";
            this.ChromatogramChart.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.ChromatogramChart.Legends.Add(legend1);
            this.ChromatogramChart.Location = new System.Drawing.Point(0, 0);
            this.ChromatogramChart.Name = "ChromatogramChart";
            this.ChromatogramChart.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.Pastel;
            series1.ChartArea = "Trace";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.IsVisibleInLegend = false;
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            
            
            this.ChromatogramChart.Series.Add(series1);
            this.ChromatogramChart.Size = new System.Drawing.Size(300, 300);
            this.ChromatogramChart.TabIndex = 0;
            this.ChromatogramChart.Text = "chart1";
            // 
            // CromatogramGraph
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(this.Width,this.Height);
            this.Controls.Add(this.ChromatogramChart);
            this.Name = "CromatogramGraph";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "UVGraph";
            ((System.ComponentModel.ISupportInitialize)(this.ChromatogramChart)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart ChromatogramChart;

    }
}