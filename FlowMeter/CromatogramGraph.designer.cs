namespace RadElChemBox
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
            this.txtXmax = new System.Windows.Forms.TextBox();
            this.txtXmin = new System.Windows.Forms.TextBox();
            this.txtYmin = new System.Windows.Forms.TextBox();
            this.txtYmax = new System.Windows.Forms.TextBox();
            this.lblCurrent = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.ChromatogramChart)).BeginInit();
            this.SuspendLayout();
            // 
            // ChromatogramChart
            // 
            chartArea1.Name = "Trace";
            this.ChromatogramChart.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.ChromatogramChart.Legends.Add(legend1);
            this.ChromatogramChart.Location = new System.Drawing.Point(42, 1);
            this.ChromatogramChart.Name = "ChromatogramChart";
            this.ChromatogramChart.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.Pastel;
            series1.ChartArea = "Trace";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.IsVisibleInLegend = false;
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.ChromatogramChart.Series.Add(series1);
            this.ChromatogramChart.Size = new System.Drawing.Size(371, 274);
            this.ChromatogramChart.TabIndex = 0;
            this.ChromatogramChart.Text = "chart1";
            // 
            // txtXmax
            // 
            this.txtXmax.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.txtXmax.Location = new System.Drawing.Point(72, 281);
            this.txtXmax.Name = "txtXmax";
            this.txtXmax.Size = new System.Drawing.Size(36, 20);
            this.txtXmax.TabIndex = 2;
            this.txtXmax.TextChanged += new System.EventHandler(this.txtXmax_TextChanged);
            // 
            // txtXmin
            // 
            this.txtXmin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.txtXmin.Location = new System.Drawing.Point(29, 281);
            this.txtXmin.Name = "txtXmin";
            this.txtXmin.Size = new System.Drawing.Size(38, 20);
            this.txtXmin.TabIndex = 3;
            this.txtXmin.TextChanged += new System.EventHandler(this.txtXmin_TextChanged);
            // 
            // txtYmin
            // 
            this.txtYmin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.txtYmin.Location = new System.Drawing.Point(0, 259);
            this.txtYmin.Name = "txtYmin";
            this.txtYmin.Size = new System.Drawing.Size(36, 20);
            this.txtYmin.TabIndex = 4;
            this.txtYmin.TextChanged += new System.EventHandler(this.txtYmin_TextChanged);
            // 
            // txtYmax
            // 
            this.txtYmax.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.txtYmax.Location = new System.Drawing.Point(0, 233);
            this.txtYmax.Name = "txtYmax";
            this.txtYmax.Size = new System.Drawing.Size(36, 20);
            this.txtYmax.TabIndex = 5;
            this.txtYmax.TextChanged += new System.EventHandler(this.txtYmax_TextChanged);
            // 
            // lblCurrent
            // 
            this.lblCurrent.AutoSize = true;
            this.lblCurrent.Location = new System.Drawing.Point(1, 9);
            this.lblCurrent.Name = "lblCurrent";
            this.lblCurrent.Size = new System.Drawing.Size(71, 13);
            this.lblCurrent.TabIndex = 6;
            this.lblCurrent.Text = "Current Value";
            // 
            // CromatogramGraph
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(417, 300);
            this.Controls.Add(this.lblCurrent);
            this.Controls.Add(this.txtYmax);
            this.Controls.Add(this.txtYmin);
            this.Controls.Add(this.txtXmin);
            this.Controls.Add(this.txtXmax);
            this.Controls.Add(this.ChromatogramChart);
            this.Name = "CromatogramGraph";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "UVGraph";
            ((System.ComponentModel.ISupportInitialize)(this.ChromatogramChart)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart ChromatogramChart;
        private System.Windows.Forms.TextBox txtXmax;
        private System.Windows.Forms.TextBox txtXmin;
        private System.Windows.Forms.TextBox txtYmin;
        private System.Windows.Forms.TextBox txtYmax;
        private System.Windows.Forms.Label lblCurrent;

    }
}