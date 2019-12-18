using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using static FlowCytometry.FCMeasurement;

namespace FlowCytometry
{
    class Global
    {
        public const int LEFT_BOTTOM_T = 2100;
        public static string[] CELL_NAME = new string[] { "Neutrophils", "Lymphocytes", "Monocytes"};
        public static PointF[] CELL_CENTER = new PointF[3];      // centers of polygon
        public static MarkerStyle[] CELL_MARKER = new MarkerStyle[] { MarkerStyle.Diamond, MarkerStyle.Cross, MarkerStyle.Triangle };
        public static bool diff3_enable; // true when diff3 is enabled, i.e FCS-H : SSC:H (Channel)
        public static int T_Y_1 = 0;
        public static int T_Y_2 = 0;
        public static PointF GetCentroid(PointF[] poly)
        {
            float accumulatedArea = 0.0f;
            float centerX = 0.0f;
            float centerY = 0.0f;

            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                float temp = poly[i].X * poly[j].Y - poly[j].X * poly[i].Y;
                accumulatedArea += temp;
                centerX += (poly[i].X + poly[j].X) * temp;
                centerY += (poly[i].Y + poly[j].Y) * temp;
            }

            if (Math.Abs(accumulatedArea) < 1E-7f)
                return PointF.Empty;  // Avoid division by zero

            accumulatedArea *= 3f;
            return new PointF(centerX / accumulatedArea, centerY / accumulatedArea);
        }
    }
}
