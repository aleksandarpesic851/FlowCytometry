using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Accord.MachineLearning;
using Accord.Statistics.Distributions.DensityKernels;
using Accord.Statistics;
using Accord.Math;
using System.IO;
using static FlowCytometry.FCMeasurement;

namespace FlowCytometry
{
    public partial class FlowCytometry : Form
    {
        private FCMeasurement fcData = null;
        private List<double[]> totalData = new List<double[]>();
        List<Cluster> clusters = null;
        
        private string gatePath = AppDomain.CurrentDomain.BaseDirectory + "Gates";
        private string fcsPath = AppDomain.CurrentDomain.BaseDirectory + "Fcs";
        private string resPath = AppDomain.CurrentDomain.BaseDirectory + "RES_Fcs\\";

        private string gate1 = "gating Cells.csv";
        private string gate2 = "gating Singlets.csv";
        private string gate3 = "gating Cell Types.csv";
        private string channelNomenclature = "old_names";
        private List<Polygon> arrGatePolygon = null;
        private List<Polygon> arrGate3Polygon = null;

        private Custom_Meanshift meanshift;


        public byte Alpha = 0xff;
        public List<Color> ColorsOfMap = new List<Color>();

        public FlowCytometry()
        {
            InitializeComponent();
            ColorsOfMap.AddRange(new Color[]{
                Color.FromArgb(Alpha, 0, 0, 0) ,//Black
                Color.FromArgb(Alpha, 0, 0, 0xFF) ,//Blue
                Color.FromArgb(Alpha, 0, 0xFF, 0xFF) ,//Cyan
                Color.FromArgb(Alpha, 0, 0xFF, 0) ,//Green
                Color.FromArgb(Alpha, 0xFF, 0xFF, 0) ,//Yellow
                Color.FromArgb(Alpha, 0xFF, 0, 0) ,//Red
                Color.FromArgb(Alpha, 0xFF, 0xFF, 0xFF) // White
            });
        }

        private void FlowCytometry_Load(object sender, EventArgs e)
        {
            checkGateExist();
            DisableMeanshift();
        }

        private void CalculateAllFiles()
        {
            string[] files = Directory.GetFiles(fcsPath);

            string fcsChannel = "FSC1LG,Peak";
            string sscChannel = "SSCLG,Peak";

/*            string fcsChannel = "BS1CH1; fsc1lg-H";
            string sscChannel = "BS1CH4; ssclg-H";
*/
            string filename = "";
            string path = "";
            DateTime start;
            DateTime end;
            double diff;
            string txtDatetime;

            foreach (string file in files)
            {
                start = DateTime.Now;
                filename = file.Substring(file.LastIndexOf("\\") + 1);

                cmbX.Items.Clear();
                cmbY.Items.Clear();
                fcData = new FCMeasurement(file);
                if (!fcData.ChannelsNames.Contains(fcsChannel) || !fcData.ChannelsNames.Contains(fcsChannel))
                    continue;
                
                extractDataFromFCS(fcsChannel, sscChannel);
                
                drawPoints();
                path = resPath + filename + "_0org.png";
                ResChart.SaveImage(path, ChartImageFormat.Png);

                meanshift = new Custom_Meanshift(totalData);
                meanshift.CalculateKDE();
                drawHeatmap();
                path = resPath + filename + "_1heat.png";
                ResChart.SaveImage(path, ChartImageFormat.Png);

                //meanshift = new Custom_Meanshift(totalData);
                //meanshift.SetData(totalData);
                //meanshift.CalculateKDE();
                clusters = meanshift.CalculateCluster();
                drawClusters();
                path = resPath + filename + "_2final.png";
                ResChart.SaveImage(path, ChartImageFormat.Png);

                end = DateTime.Now;
                diff = end.Subtract(start).TotalSeconds;
                txtDatetime = "Start:" +  start.ToString() + ",    End: " + end.ToString() + ",    Duration(s): " + diff;
                path = resPath + filename + "_3time.txt";
                System.IO.File.WriteAllText(path, txtDatetime);

            }
        }
        private void showGateSelectDlg()
        {
            if (!Directory.Exists(gatePath))
            {
                using (var fbd = new FolderBrowserDialog())
                {

                    fbd.Description = "Select gate folder";
                    DialogResult result = fbd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        gatePath = fbd.SelectedPath;
                        checkGateExist();
                    }
                }
            }
        }

        private void checkGateExist()
        {
            string[] files = Directory.GetFiles(gatePath);
            if (!files.Contains(gatePath + "\\" + gate1) || !files.Contains(gatePath + "\\" + gate2) || !files.Contains(gatePath + "\\" + gate3))
            {
                MessageBox.Show("Please choose correct directory");
                gatePath = "";
                showGateSelectDlg();
            }
        }
        private void btnLoad_Click(object sender, EventArgs e)
        {
            btnPlot.Enabled = false;
            loadFile();
        }

        private void btnCluster_Click(object sender, EventArgs e)
        {
            if (totalData.Count < 1)
            {
                MessageBox.Show("Please load file first. You have to select a FCS file by clicking \"Load\" button");
                return;
            }

            if (meanshift == null || !meanshift.kdeEnable)
            {
                meanshift = new Custom_Meanshift(totalData);
                meanshift.CalculateKDE();
            }
            if (!meanshift.clusterEnalble)
            {
                clusters = meanshift.CalculateCluster();
            }
            drawClusters();
        }

        private void btnPlot_Click(object sender, EventArgs e)
        {
            drawPoints();
        }

        private void btnKDEHeat_Click(object sender, EventArgs e)
        {
            if (meanshift == null || !meanshift.kdeEnable)
            {
                meanshift = new Custom_Meanshift(totalData);
                meanshift.CalculateKDE();
            }
            drawHeatmap();
        }
        private void cmbX_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisableMeanshift();

            bool isExtracted = extractDataFromFCS();
            if (isExtracted)
            {
                btnPlot.Enabled = true;
                btnKDEHeat.Enabled = true;
                btnCluster.Enabled = true;
            }
            else
            {
                btnPlot.Enabled = false;
            }
        }

        private void cmbY_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisableMeanshift();
            bool isExtracted = extractDataFromFCS();
            if (isExtracted)
            {
                btnPlot.Enabled = true;
                btnKDEHeat.Enabled = true;
                btnCluster.Enabled = true;
            }
            else
            {
                btnPlot.Enabled = false;
            }
        }

        // Load FCS file from dialog.
        // Update combo box with loaded file data
        private void loadFile()
        {
            cmbX.Items.Clear();
            cmbY.Items.Clear();
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "FCS files|*.fcs";
                if (dlg.ShowDialog() == DialogResult.OK)
                {

                    fcData = new FCMeasurement(dlg.FileName);

                    foreach (String name in fcData.ChannelsNames)
                    {
                        cmbX.Items.Add(name);
                        cmbY.Items.Add(name);
                    }
                    cmbX.SelectedIndex = 0;
                    cmbY.SelectedIndex = 1;
                }
            }
        }

        // Draw origin points
        private void drawPoints()
        {
            ResChart.ChartAreas[0].AxisX.Title = cmbX.Text;
            ResChart.ChartAreas[0].AxisY.Title = cmbY.Text;

            ResChart.Series.Clear();

            ResChart.Series.Add("Points");
            ResChart.Series[0].ChartType = SeriesChartType.Point;
            ResChart.Series[0].MarkerSize = 2;
            ResChart.Series[0].MarkerStyle = MarkerStyle.Circle;
            ResChart.Series[0].MarkerColor = Color.Black;

            foreach (double[] element in totalData)
            {
                ResChart.Series[0].Points.AddXY(element[0], element[1]);
            }
            drawGates();
            ResChart.Refresh();
        }
        
        // Draw Clusters
        private void drawClusters()
        {
            if (clusters == null)
                return;

            ResChart.Series.Clear();
            int i = 0, nCnt = clusters.Count;

            Random random = new Random();

            if (nCnt < 1)
            {
                MessageBox.Show("There is no cluster to display.");
                return;
            }

            foreach(Cluster cluster in clusters)
            {
                if (string.IsNullOrEmpty(cluster.clusterName))
                {
                    ResChart.Series.Add("C" + (i + 1));
                    ResChart.Series[i].MarkerColor = Color.FromArgb((int)(random.NextDouble() * 150),
                        (int)(random.NextDouble() * 150),
                        (int)(random.NextDouble() * 150));
                    ResChart.Series[i].MarkerStyle = MarkerStyle.Circle;
                }
                else
                {
                    ResChart.Series.Add(cluster.clusterName);
                    int idx = Global.CELL_NAME.IndexOf(cluster.clusterName);
                    ResChart.Series[i].MarkerColor = arrGate3Polygon[idx].color;
                    ResChart.Series[i].MarkerStyle = Global.CELL_MARKER[idx];
                }

                ResChart.Series[i].ChartType = SeriesChartType.Point;
                ResChart.Series[i].MarkerSize = 2;

                foreach (int idx in cluster.points)
                {
                    ResChart.Series[i].Points.AddXY(totalData[idx][0], totalData[idx][1]);
                }
                i++;
            }

            drawGates();
            ResChart.Refresh();
        }

        // Draw Gates
        private void drawGates()
        {
            int i = 1;
            // Draw Gates
            if (arrGatePolygon != null)
            {
                foreach (Polygon polygon in arrGatePolygon)
                {
                    ResChart.Series.Add("Gate-" + i);
                    ResChart.Series["Gate-" + i].Color = polygon.color;
                    ResChart.Series["Gate-" + i].ChartType = SeriesChartType.Line;
                    foreach (PointF point in polygon.poly)
                    {
                        ResChart.Series["Gate-" + i].Points.AddXY(point.X, point.Y);
                    }
                    i++;
                }
            }

            // Draw Gate3
            if (arrGate3Polygon != null)
            {
                i = 0;
                foreach (Polygon polygon in arrGate3Polygon)
                {
                    ResChart.Series.Add("Gate:" + Global.CELL_NAME[i]);
                    ResChart.Series["Gate:" + Global.CELL_NAME[i]].Color = polygon.color;
                    ResChart.Series["Gate:" + Global.CELL_NAME[i]].ChartType = SeriesChartType.Line;
                    foreach (PointF point in polygon.poly)
                    {
                        ResChart.Series["Gate:" + Global.CELL_NAME[i]].Points.AddXY(point.X, point.Y);
                    }
                    i++;
                }
            }
        }

        // Draw KDE Heatmap
        private void drawHeatmap()
        {
            if (meanshift == null || !meanshift.kdeEnable)
                return;

            int nCnt = meanshift.nGridCnt, x = 0, y = 0;
            int nMarkerSize = Math.Max(ResChart.Width, ResChart.Height )/ nCnt + 1;
            ResChart.Series.Clear();

            ResChart.Series.Add("Heat");
            ResChart.Series["Heat"].ChartType = SeriesChartType.Point;
            ResChart.Series["Heat"].MarkerSize = nMarkerSize;
            ResChart.Series["Heat"].MarkerStyle = MarkerStyle.Circle;

            double[] xy;
            int nIdx = 0;
            for ( x = 0; x < nCnt; x ++)
            {
                for (y= 0; y < nCnt; y ++)
                {
                    xy = meanshift.ConvertGridToCoord(x, y);
                    ResChart.Series["Heat"].Points.AddXY(xy[0], xy[1]);
                    ResChart.Series["Heat"].Points[nIdx].Color = GetHeatColor(meanshift.kde[x,y], meanshift.maxKde);
                    nIdx++;
                }
            }

            ResChart.Refresh();
        }

        public Color GetHeatColor(double val, double maxVal)
        {
            double valPerc = val / maxVal;// value%
            double colorPerc = 1d / (ColorsOfMap.Count - 2);// % of each block of color. the last is the "100% Color"
            double blockOfColor = valPerc / colorPerc;// the integer part repersents how many block to skip
            int blockIdx = (int)Math.Truncate(blockOfColor);// Idx of 
            double valPercResidual = valPerc - (blockIdx * colorPerc);//remove the part represented of block 
            double percOfColor = valPercResidual / colorPerc;// % of color of this block that will be filled

            Color cTarget = ColorsOfMap[blockIdx];
            Color cNext = ColorsOfMap[blockIdx + 1];

            var deltaR = cNext.R - cTarget.R;
            var deltaG = cNext.G - cTarget.G;
            var deltaB = cNext.B - cTarget.B;

            var R = cTarget.R + (deltaR * percOfColor);
            var G = cTarget.G + (deltaG * percOfColor);
            var B = cTarget.B + (deltaB * percOfColor);

            Color c = ColorsOfMap[0];
            try
            {
                c = Color.FromArgb(Alpha, (byte)R, (byte)G, (byte)B);
            }
            catch
            {
            }
            return c;
        }
        // extract channel data from FCS data
        private bool extractDataFromFCS(string channel1 = "", string channel2 = "")
        {
            if (fcData == null)
            {
                return false;
            }
            if (string.IsNullOrEmpty(channel1) || string.IsNullOrEmpty(channel2))
            {
                channel1 = cmbX.Text;
                channel2 = cmbY.Text;
            }
            int i = 0;

            totalData.Clear();

            if (String.IsNullOrEmpty(channel1) || String.IsNullOrEmpty(channel2))
                return false;

            if (channel1 == channel2)
            {
                MessageBox.Show("Two channels are equal. Please select different channel!");
                return false;
            }

            // Get gates according to channel
            GetGates(channel1, channel2);

            double x, y;
            bool validPoint = false;
            for (i = 0; i < fcData.Counts; i++)
            {
                x = fcData.Channels[channel1].Data.ElementAt(i);
                y = fcData.Channels[channel2].Data.ElementAt(i);

                validPoint = true;
                if (arrGatePolygon != null)
                {
                    validPoint = false;
                    foreach(Polygon polygon in arrGatePolygon)
                    {
                        if (polygon.IsInsidePoly(x, y))
                        {
                            validPoint = true;
                            break;
                        }
                    }
                }

                if (!validPoint)
                    continue;

/*                if (Global.diff3_enable && x < Global.LEFT_BOTTOM_T)
                    continue;
*/    
                totalData.Add(new double[2]
                {
                    fcData.Channels[channel1].Data.ElementAt(i),
                    fcData.Channels[channel2].Data.ElementAt(i)
                });
            }

            if (totalData.Count < 2)
                return false;
            return true;
        }

        // get gate polygon according to channel
        private void GetGates(string channel1 = "", string channel2 = "")
        {
            checkGateExist();

            string[] FCS1_H = new string[] { "FSC1LG,Peak", "BS1CH1; fsc1lg-H", "BS1CH1; fsc1lg-H"};
            string[] SSC_H = new string[] { "SSCLG,Peak", "BS1CH2; ssclg-H", "BS1CH4; ssclg-H" };
            string[] FSC1_A = new string[] { "FSC1LG,Area", "BS1CH1; fsc1lg-A", "BS1CH1; fsc1lg-A" };
/*            string FCS1_H = FCMeasurement.GetChannelName("FCS1peak", channelNomenclature);
            string SSC_H = FCMeasurement.GetChannelName("SSCpeak", channelNomenclature);
            string FSC1_A = FCMeasurement.GetChannelName("FCS1area", channelNomenclature);
*/
            arrGatePolygon = null;
            arrGate3Polygon = null;
            Global.diff3_enable = false;

            if (FCS1_H.Contains(channel1) && SSC_H.Contains(channel2))
            {
                arrGatePolygon = FCMeasurement.loadPolygon(Path.Combine(gatePath, gate1));
                arrGate3Polygon = FCMeasurement.loadPolygon(Path.Combine(gatePath, gate3));
                if (arrGate3Polygon.Count < 3)
                {
                    MessageBox.Show("The Cell Type gate file is incorrect.");
                    return;
                }
                int i = 0;
                for (i = 0; i < 3; i ++)
                {
                    Global.CELL_CENTER[i] = Global.GetCentroid(arrGate3Polygon[i].poly);
                }
                Global.diff3_enable = true;
                Global.T_Y_1 = (int)arrGate3Polygon[2].poly[0].Y;
                Global.T_Y_2 = (int)arrGate3Polygon[0].poly[0].Y;
            } 
            else if (FSC1_A.Contains(channel1) && FCS1_H.Contains(channel2))
            {
                arrGatePolygon = FCMeasurement.loadPolygon(Path.Combine(gatePath, gate2));
            }
        }

        public void WriteExcelFile(String fileName)
        {
            PicoXLSX.Workbook workbook = new PicoXLSX.Workbook(fileName, "FCS_Data");

            foreach (double[] element in totalData)
            {
                workbook.Worksheets[0].AddNextCell(element[0]);
                workbook.Worksheets[0].AddNextCell(element[1]);
                workbook.Worksheets[0].GoToNextRow();
            }

            workbook.Save();
        }

        private void DisableMeanshift()
        {
            btnCluster.Enabled = false;
            btnKDEHeat.Enabled = false;
            if (meanshift == null)
                return;
            meanshift.DisableCalcResult();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            meanshift.SmoothingKDE();
            drawHeatmap();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            CalculateAllFiles();
        }
    }
}
