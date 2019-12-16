using Accord.Statistics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FlowCytometry.FCMeasurement;

namespace FlowCytometry
{
    class Custom_Meanshift
    {
        private int nRate = 50;                 // ignore cluster when the number of it is less than total/nRate
        private double[][] totalData;             // origin data
        private double bandWidth;               // bandwidth to use in KDE

        public double[,] kde;                  // estimated kernel data
        private int nPtCnt;                     // total number of data 
        private double gridSize;                // grid size to pre-calculate, this is used for speed up
        public int nGridCnt;                   // total number of grids
        private double[] variations;            // the variaction of data

        private double normalX = 0;             // normalize param
        private double normalY = 0;             // normalize param
        private int nRadius = 7;                // Neighbour Radius, which is used for climb hill
        private int nRadiousKDE = 20;           // Neighbour of KDE, this is for speed up
        private List<Cluster> clusters = new List<Cluster>(); // Final Result

        public bool kdeEnable = false;          // true when kde calculation was done
        public bool clusterEnalble = false;     // true when cluster calcuation was done
        public double maxKde = 0;               // max value of kde, this is used for drawing kde heatmap.

        public Custom_Meanshift(List<double[]> data)
        {
            nPtCnt = data.Count;

            //make copy of data
            int i = 0;
            totalData = new double[nPtCnt][];
            for (i = 0; i < nPtCnt; i ++ )
            {
                totalData[i] = new double[2] { data[i][0], data[i][1] };
                normalX = Math.Max(totalData[i][0], normalX);
                normalY = Math.Max(totalData[i][1], normalY);
            }
        }

        // ignore current result
        public void DisableCalResult()
        {
            kdeEnable = false;
            clusterEnalble = false;
        }
        public void CalculateKDE()
        {
            if (kdeEnable)
                return;
            NormalizeData();
            CalculateBandWidth();
            GetnerateKDE();
            //SmoothingKDE();
            kdeEnable = true;
        }

        public double[] ConvertGridToCoord(int x, int y)
        {
            return new double[2] { x * gridSize * normalX, y * gridSize * normalY};
        }
        public List<Cluster> CalculateCluster()
        {
            if (clusterEnalble)
                return clusters;

            clusters.Clear();
            GenerateCluster();
            AIProc();

            clusterEnalble = true;
            return clusters;
        }

        //Normalize data so that all data points will be lie on [0,1]
        private void NormalizeData()
        {
            foreach(double[] element in totalData)
            {
                element[0] /= normalX;
                element[1] /= normalY;
            }
        }

        //Calculate Optimal Bandwidth from data variation
        private void CalculateBandWidth()
        {
            variations = Measures.StandardDeviation(totalData, true);
            double variation = Math.Sqrt(Math.Pow(variations[0], 2) + Math.Pow(variations[1], 2));
            bandWidth = 0.1 * variation * Math.Pow(nPtCnt, -0.2);
            gridSize = bandWidth;
/*            bandWidth = 0.001;
            gridSize = 0.005;
*/
            nGridCnt = (int)(1 / gridSize) + 1;
        }

        //Calculate KDE based on Gaussian Kernel
        private void GetnerateKDE()
        {
            int x = 0, y = 0;
            kde = new double[nGridCnt, nGridCnt];
            double power = 0;
            List<double[]> neighbour;
            maxKde = 0;

            double R2 = nRadiousKDE * nRadiousKDE * gridSize * gridSize;

            for (x = 0; x < nGridCnt; x ++)
            {
                for (y = 0; y < nGridCnt; y ++)
                {
                    neighbour = GetNeighbour(x, y);
                    foreach(double[] point in neighbour)
                    {
/*                        power = (x * gridSize - point[0]) * (x * gridSize - point[0]) +
                            (y * gridSize - point[1]) * (y * gridSize - point[1]);
                        kde[x, y] += Math.Pow(e, -power / (2 * bandWidth));
*/

                        power = (x * gridSize - point[0]) * (x * gridSize - point[0]) +
                            (y * gridSize - point[1]) * (y * gridSize - point[1]);
                        kde[x, y] += 1 - power / R2;

                    }
                    maxKde = Math.Max(maxKde, kde[x, y]);
                }
            }
        }

        //Make the KDE Curve smooth
        public void SmoothingKDE()
        {
            int x = 0, y = 0;
            int nRad = 2;
            int nCnt = 0, i = 0, k = 0;
            double meanVal = 0;
            double[][] tmpArr = new double[nGridCnt][];

            for (x = 0; x < nGridCnt; x++)
            {
                tmpArr[x] = new double[nGridCnt];
                for (y = 0; y < nGridCnt; y++)
                {
                    tmpArr[x][y] = kde[x, y];
                }
            }

            for (x = 0; x < nGridCnt; x++)
            {
                for (y = 0; y < nGridCnt; y++)
                {
                    meanVal = 0;
                    for (i = -nRad; i <= nRad; i++)
                    {
                        if (x + i < 0 || x + i >= nGridCnt)
                            continue;
                        for (k = -nRad; k <= nRad; k++)
                        {
                            if (y + k < 0 || y + k >= nGridCnt)
                                continue;
                            if (i == 0 && k == 0)
                                continue;
                            meanVal += tmpArr[i+x][k+y];
                            nCnt++;
                        }
                    }

                    meanVal = meanVal * 3 / (4 * nCnt);
                    meanVal += tmpArr[x][y] / 4;

                    kde[x, y] = meanVal;
                }
            }
        }

        // This is used in generate KDE for speed up
        private List<double[]> GetNeighbour(int x, int y)
        {
            List<double[]> neighbour = new List<double[]>();
            double[] center = new double[2] { x * gridSize, y * gridSize };
            double r = nRadiousKDE * nRadiousKDE * gridSize * gridSize;

            foreach (double[] point in totalData)
            {
                if (euclidean(center, point) < r)
                    neighbour.Add(point);
            }
            return neighbour;
        }

        private double euclidean(double[] A, double[] B) => (A[0] - B[0]) * (A[0] - B[0]) + (A[1] - B[1]) * (A[1] - B[1]);

        // calculate cluster and assign the label to every point
        private void GenerateCluster()
        {
            int i = 0;
            int[] oldPoint = new int[2];
            int[] newPoint;

            for ( i = 0; i < nPtCnt; i ++ )
            {
                oldPoint = FindStartPoint(totalData[i][0] / gridSize, totalData[i][1] / gridSize);
                while (true)
                {
                    newPoint = FindClosestMax(oldPoint);

                    if (oldPoint[0] == newPoint[0] && oldPoint[1] == newPoint[1])
                    {
                        AssignPointToCluster(i, oldPoint);
                        break;
                    }
                    if (newPoint[0] == 0 && newPoint[1] == 0)
                    {
                        AssignPointToCluster(i, oldPoint);
                        break;
                    }
                    oldPoint[0] = newPoint[0];
                    oldPoint[1] = newPoint[1];
                }
            }
        }

        private int[] FindStartPoint(double x, double y)
        {
            int[] point = new int[2];
            int sX = (int)x;
            int sY = (int)y;
            point[0] = sX;
            point[1] = sY;

            if (sY == nGridCnt - 1 || sX == nGridCnt - 1)
                return point;

            if (kde[point[0], point[1]] < kde[sX + 1, sY])
            {
                point[0] = sX + 1;
                point[1] = sY;
            }

            if (kde[point[0], point[1]] < kde[sX, sY + 1])
            {
                point[0] = sX;
                point[1] = sY + 1;
            }
            if (kde[point[0], point[1]] < kde[sX + 1, sY + 1])
            {
                point[0] = sX + 1;
                point[1] = sY + 1;
            }
            return point;
        }
        // Find the position of max value in neighbour of input param(position)
        private int[] FindClosestMax(int[] point)
        {
            int x, y;
            int[] maxPoint = new int[2];
            double max = 0;
            int R = nRadius * nRadius;

            for ( x = Math.Max(0, point[0] - nRadius); x <= Math.Min(nGridCnt-1, point[0] + nRadius); x++ )
            {
                for (y = Math.Max(0, point[1] - nRadius); y <= Math.Min(nGridCnt-1, point[1] + nRadius); y++)
                {
/*                    if ((x- point[0]) * (x- point[0]) + (y- point[1]) * (y- point[1]) > R)
                        continue;
*/
                    if (max < kde[x,y])
                    {
                        max = kde[x, y];
                        maxPoint[0] = x;
                        maxPoint[1] = y;
                    }
                }
            }
            return maxPoint;
        }

        //Assign current point to a cluster
        private void AssignPointToCluster(int idx, int[] point)
        {
            // Check whether this is a point of calculated cluster
            foreach(Cluster cluster in clusters)
            {
                if (cluster.IsEqual(point))
                {
                    cluster.AddNewPoint(idx);
                    return;
                }
            }

            // This is new cluster, create new cluster.
            Cluster newCluster = new Cluster
            {
                centerX = point[0],
                centerY = point[1],
            };
            newCluster.AddNewPoint(idx);
            clusters.Add(newCluster);
        }
    
        // Do Extra operation on clusters.
        private void AIProc()
        {
            MergeSmallClusters();
            if (Global.diff3_enable)
            {
                RemoveBLDebris();
                MergeBottomClusters();
                DetectCellType();
            }
        }

        //Remove bottom-left debris
        private void RemoveBLDebris()
        {
            if (clusters.Count < 3)
                return;
            int i;
            int nMinX_T = nGridCnt / 10;

            for( i = clusters.Count-1; i > -1; i -- )
            {
                if (clusters[i].centerX < nMinX_T)
                    clusters.RemoveAt(i);
                if (clusters.Count < 3)
                    break;
            }
        }
        
        //Merge bottom clusters
        private void MergeBottomClusters()
        {
            if (clusters.Count < 2)
                return;

            int bottomT = (int)(0.19 / gridSize), i = 0, bottmIdx = 0;

            //Find bottom cluster
            Cluster bottomCluster = clusters.ElementAt(0);
            Cluster currentCluster;
            foreach (Cluster cluster in clusters)
            {
                if (cluster.centerY < bottomCluster.centerY)
                {
                    bottomCluster = cluster;
                    bottmIdx = i;
                }
                i++;
            }

            if (bottomCluster.centerY > bottomT)
                return;
            double sumPoints = 0;
            for (i = clusters.Count-1; i > -1; i--)
            {
                if (i == bottmIdx)
                    continue;
                currentCluster = clusters[i];
                if (currentCluster.centerY < bottomT)
                {
                    sumPoints = (double)(currentCluster.points.Count + bottomCluster.points.Count);
                    bottomCluster.centerX = (int)(currentCluster.points.Count / sumPoints * currentCluster.centerX + 
                        bottomCluster.points.Count / sumPoints * bottomCluster.centerX);
                    bottomCluster.centerY = (int)(currentCluster.points.Count / sumPoints * currentCluster.centerY +
                        bottomCluster.points.Count / sumPoints * bottomCluster.centerY);
                    bottomCluster.points.AddRange(clusters[i].points);
                    clusters.RemoveAt(i);
                }
            }
        }

        // Merge small clusters to nearest cluster
        private void MergeSmallClusters()
        {
            int i = 0, k = 0, nCnt = 0;
            bool bMerged = true;
            Cluster currCluster;
            int minPoints = totalData.Length / nRate;

            double MaxVal = (normalX * normalX + normalY * normalY) * 2;
            double minDist;
            int minIdx = 0;

            while (true)
            {
                nCnt = clusters.Count;
                if (!bMerged || nCnt < 2)
                    break;

                bMerged = false;
                // Get center of clustrs
                List<double[]> centers = new List<double[]>();
                foreach (Cluster cluster in clusters)
                {
                    centers.Add(ConvertGridToCoord(cluster.centerX, cluster.centerY));
                }

                for (i = 0; i < nCnt; i ++)
                {
                    currCluster = clusters[i];
                    if (currCluster.points.Count > minPoints)
                        continue;

                    // Calculate min distance with other cluster centers
                    minDist = MaxVal;
                    for (k = 0; k < nCnt; k ++)
                    {
                        if (i == k || clusters[k].points.Count < minPoints)
                            continue;
                        if (minDist > euclidean(centers[i], centers[k]))
                        {
                            minDist = euclidean(centers[i], centers[k]);
                            minIdx = k;
                        }
                    }

                    //Merge current cluster to closest cluster
                    clusters[minIdx].points.AddRange(currCluster.points);
                    bMerged = true;
                    clusters.RemoveAt(i);
                    break;
                }
            }
        }

        // detect cluster type based on diff3 gate
        private void DetectCellType()
        {
            if (!Global.diff3_enable)
                return;

            // Get center of clustrs
            List<double[]> centers = new List<double[]>();
            foreach(Cluster cluster in clusters)
            {
                centers.Add(ConvertGridToCoord(cluster.centerX, cluster.centerY));
            }

            if (centers.Count < 1)
                return;

            int nCnt = 1, i = 0, k = 0;
            List<int> assignedTypes = new List<int>();
            List<int> assignedClusters = new List<int>();
            double MaxVal = (normalX * normalX + normalY * normalY) * 2;
            double dist;
            while (true)
            {
                if (nCnt > centers.Count || nCnt > 3)
                    break;

                List<double> minDists = new List<double>();
                List<int> minIdxs = new List<int>();
                for (i = 0; i < 3; i ++)
                {
                    minDists.Add(MaxVal);      // set max value for get min value
                    minIdxs.Add(-1);
                    if (assignedTypes.Contains(i))  // if current type is assigned, continue
                        continue;

                    // calculate min distances with other all clusters
                    for (k = 0; k < centers.Count; k++)
                    {
                        if (assignedClusters.Contains(k))
                            continue;
                        dist = euclidean(centers[k], new double[2] { Global.CELL_CENTER[i].X, Global.CELL_CENTER[i].Y });
                        if (minDists[i] > dist)
                        {
                            minIdxs[i] = k;
                            minDists[i] = dist;
                        }
                    }
                }

                // get the closest distance and index
                double minVal = minDists[0];
                int minIdx = 0;
                for (i = 1; i < 3; i ++)
                {
                    if (minVal > minDists[i])
                    {
                        minVal = minDists[i];
                        minIdx = i;
                    }
                }

                //Assign cluster to cell type
                assignedTypes.Add(minIdx);
                assignedClusters.Add(minIdxs[minIdx]);
                clusters.ElementAt(minIdxs[minIdx]).clusterName = Global.CELL_NAME[minIdx];
                nCnt++;
            }
        }

    }

}
