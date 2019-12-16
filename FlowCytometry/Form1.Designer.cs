namespace FlowCytometry
{
    partial class FlowCytometry
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series5 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series6 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.groupTool = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.btnKDEHeat = new System.Windows.Forms.Button();
            this.btnPlot = new System.Windows.Forms.Button();
            this.btnCluster = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.cmbY = new System.Windows.Forms.ComboBox();
            this.cmbX = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ResChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.groupTool.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ResChart)).BeginInit();
            this.SuspendLayout();
            // 
            // groupTool
            // 
            this.groupTool.Controls.Add(this.button1);
            this.groupTool.Controls.Add(this.btnKDEHeat);
            this.groupTool.Controls.Add(this.btnPlot);
            this.groupTool.Controls.Add(this.btnCluster);
            this.groupTool.Controls.Add(this.btnLoad);
            this.groupTool.Controls.Add(this.cmbY);
            this.groupTool.Controls.Add(this.cmbX);
            this.groupTool.Controls.Add(this.label2);
            this.groupTool.Controls.Add(this.label1);
            this.groupTool.Location = new System.Drawing.Point(12, 15);
            this.groupTool.Name = "groupTool";
            this.groupTool.Size = new System.Drawing.Size(1151, 116);
            this.groupTool.TabIndex = 0;
            this.groupTool.TabStop = false;
            this.groupTool.Text = "Tool Box";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(972, 43);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(150, 49);
            this.button1.TabIndex = 8;
            this.button1.Text = "CalculateALlFiles";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // btnKDEHeat
            // 
            this.btnKDEHeat.Location = new System.Drawing.Point(640, 43);
            this.btnKDEHeat.Name = "btnKDEHeat";
            this.btnKDEHeat.Size = new System.Drawing.Size(133, 49);
            this.btnKDEHeat.TabIndex = 7;
            this.btnKDEHeat.Text = "KDE Heatmap";
            this.btnKDEHeat.UseVisualStyleBackColor = true;
            this.btnKDEHeat.Click += new System.EventHandler(this.btnKDEHeat_Click);
            // 
            // btnPlot
            // 
            this.btnPlot.Enabled = false;
            this.btnPlot.Location = new System.Drawing.Point(473, 43);
            this.btnPlot.Name = "btnPlot";
            this.btnPlot.Size = new System.Drawing.Size(133, 49);
            this.btnPlot.TabIndex = 6;
            this.btnPlot.Text = "Plot";
            this.btnPlot.UseVisualStyleBackColor = true;
            this.btnPlot.Click += new System.EventHandler(this.btnPlot_Click);
            // 
            // btnCluster
            // 
            this.btnCluster.Location = new System.Drawing.Point(816, 43);
            this.btnCluster.Name = "btnCluster";
            this.btnCluster.Size = new System.Drawing.Size(133, 49);
            this.btnCluster.TabIndex = 5;
            this.btnCluster.Text = "Cluster";
            this.btnCluster.UseVisualStyleBackColor = true;
            this.btnCluster.Click += new System.EventHandler(this.btnCluster_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(307, 43);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(133, 49);
            this.btnLoad.TabIndex = 4;
            this.btnLoad.Text = "Load ";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // cmbY
            // 
            this.cmbY.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbY.FormattingEnabled = true;
            this.cmbY.Location = new System.Drawing.Point(108, 75);
            this.cmbY.Name = "cmbY";
            this.cmbY.Size = new System.Drawing.Size(168, 24);
            this.cmbY.TabIndex = 3;
            this.cmbY.SelectedIndexChanged += new System.EventHandler(this.cmbY_SelectedIndexChanged);
            // 
            // cmbX
            // 
            this.cmbX.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbX.FormattingEnabled = true;
            this.cmbX.Location = new System.Drawing.Point(108, 33);
            this.cmbX.Name = "cmbX";
            this.cmbX.Size = new System.Drawing.Size(168, 24);
            this.cmbX.TabIndex = 2;
            this.cmbX.SelectedIndexChanged += new System.EventHandler(this.cmbX_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(38, 78);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "Axis - Y :";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(38, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Axis - X :";
            // 
            // ResChart
            // 
            chartArea2.Name = "ChartArea1";
            this.ResChart.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            this.ResChart.Legends.Add(legend2);
            this.ResChart.Location = new System.Drawing.Point(12, 159);
            this.ResChart.Name = "ResChart";
            series4.ChartArea = "ChartArea1";
            series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastPoint;
            series4.Legend = "Legend1";
            series4.MarkerSize = 2;
            series4.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
            series4.Name = "Cluster1";
            series5.ChartArea = "ChartArea1";
            series5.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastPoint;
            series5.Legend = "Legend1";
            series5.MarkerSize = 2;
            series5.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
            series5.Name = "Cluster2";
            series6.ChartArea = "ChartArea1";
            series6.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastPoint;
            series6.Legend = "Legend1";
            series6.MarkerSize = 2;
            series6.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
            series6.Name = "Cluster3";
            this.ResChart.Series.Add(series4);
            this.ResChart.Series.Add(series5);
            this.ResChart.Series.Add(series6);
            this.ResChart.Size = new System.Drawing.Size(1151, 582);
            this.ResChart.TabIndex = 1;
            this.ResChart.Text = "Result Chart";
            // 
            // FlowCytometry
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1182, 753);
            this.Controls.Add(this.ResChart);
            this.Controls.Add(this.groupTool);
            this.Name = "FlowCytometry";
            this.Text = "FlowCytometry";
            this.Load += new System.EventHandler(this.FlowCytometry_Load);
            this.groupTool.ResumeLayout(false);
            this.groupTool.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ResChart)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupTool;
        private System.Windows.Forms.Button btnCluster;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.ComboBox cmbY;
        private System.Windows.Forms.ComboBox cmbX;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataVisualization.Charting.Chart ResChart;
        private System.Windows.Forms.Button btnPlot;
        private System.Windows.Forms.Button btnKDEHeat;
        private System.Windows.Forms.Button button1;
    }
}

