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
        private int nRate = 500;                 // ignore cluster when the number of it is less than total/nRate
        private double[][] totalData;             // origin data
        private double bandWidth;               // bandwidth to use in KDE

        public double[,] kde;                  // estimated kernel data
        private List<GridWeight> gridWeights;
        private int[,] gridWeight;
        private double[,] expWeight;
        private int nPtCnt;                     // total number of data 
        private double gridSize;                // grid size to pre-calculate, this is used for speed up
        public int nGridCnt = 300;                   // total number of grids
        private double[] variations;            // the variaction of data

        private double normalX = 0;             // normalize param
        private double normalY = 0;             // normalize param
        private int nRadius = 3;                // Neighbour Radius, which is used for climb hill
        private int nRadiousKDE = 10;           // Neighbour of KDE, this is for speed up
        private List<Cluster> clusters = new List<Cluster>(); // Final Result

        public bool kdeEnable = false;          // true when kde calculation was done
        public bool clusterEnalble = false;     // true when cluster calcuation was done
        public double maxKde = 0;               // max value of kde, this is used for drawing kde heatmap.

        public Custom_Meanshift(List<double[]> data = null)
        {
            SetData(data);
            gridSize = 1.0 / nGridCnt;
        }

        public void SetData(List<double[]> data)
        {
            if (data == null)
                return;
            nPtCnt = data.Count;

            //make copy of data
            int i = 0;
            totalData = new double[nPtCnt][];
            for (i = 0; i < nPtCnt; i++)
            {
                totalData[i] = new double[2] { data[i][0], data[i][1] };
                normalX = Math.Max(totalData[i][0], normalX);
                normalY = Math.Max(totalData[i][1], normalY);
            }
            normalX += 1;
            normalY += 1;
        }

        // ignore current result
        public void DisableCalcResult()
        {
            kdeEnable = false;
            clusterEnalble = false;
        }
        public void CalculateKDE()
        {
            if (kdeEnable)
                return;
            NormalizeData();
            CalculateGridWeight();
            CalculateBandWidth();
            //GetnerateKDE();
            CalculateGaussianDensity();
            GetnerateFullKDE();
            //SmoothingKDE();
            kdeEnable = true;
        }

        public double[] ConvertGridToCoord(int x, int y)
        {
            return new double[2] { x * gridSize * normalX, y * gridSize * normalY };
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
            foreach (double[] element in totalData)
            {
                element[0] /= normalX;
                element[1] /= normalY;
            }
        }

        // Calculate grid weight
        private void CalculateGridWeight()
        {
            gridWeights = new List<GridWeight>();

            gridWeight = new int[nGridCnt, nGridCnt];
            kde = new double[nGridCnt, nGridCnt];
            int x, y;
            foreach (double[] element in totalData)
            {
                x = (int)(element[0] * nGridCnt);
                y = (int)(element[1] * nGridCnt);
                gridWeight[x, y] += 1;
            }

            for (x = 0; x < nGridCnt; x++)
            {
                for (y = 0; y < nGridCnt; y++)
                {
                    if (gridWeight[x, y] < 1)
                        continue;
                    gridWeights.Add(new GridWeight
                    {
                        x = x,
                        y = y,
                        weight = gridWeight[x,y]
                    });
                }
            }
        }

        //Calculate Optimal Bandwidth from data variation
        private void CalculateBandWidth()
        {
            variations = Measures.StandardDeviation(totalData, true);
            double variation = Math.Sqrt(Math.Pow(variations[0], 2) + Math.Pow(variations[1], 2));
            bandWidth = 1.06 * variation * Math.Pow(nPtCnt, -0.2);
            bandWidth = gridSize;
            //gridSize = bandWidth;
            /*            bandWidth = 0.001;
                        gridSize = 0.005;
            */
            //nGridCnt = (int)(1 / gridSize) + 1;
        }

        //Calculate KDE based on Gaussian Kernel
        private void GetnerateKDE()
        {
            int x = 0, y = 0, x1 = 0, y1 = 0;
            kde = new double[nGridCnt, nGridCnt];
            double power = 0;
            List<double[]> neighbour;
            maxKde = 0;

            double R2 = nRadiousKDE * nRadiousKDE;

            for (x = 0; x < nGridCnt; x++)
            {
                for (x1 = Math.Max(0, x - nRadiousKDE); x1 < Math.Min(nGridCnt, x + nRadiousKDE); x1++)
                {
                    for (y = 0; y < nGridCnt; y++)
                    {
                        for (y1 = Math.Max(0, y - nRadiousKDE); y1 < Math.Min(nGridCnt, y + nRadiousKDE); y1++)
                        {
                            if (gridWeight[x1, y1] < 1)
                                continue;
                            power = (x - x1) * (x - x1) + (y - y1) * (y - y1);

                            if (power > R2)
                                continue;

                            kde[x, y] += (1 - power / R2) * gridWeight[x,y];
                        }
                        maxKde = Math.Max(maxKde, kde[x, y]);
                    }
                }

            }
        }

        //Calculate Gaussian density for all grid cell
        private void CalculateGaussianDensity()
        {
            int x, y;
            double power;
            expWeight = new double[nGridCnt, nGridCnt];

            for (x = 0; x < nGridCnt; x++)
            {
                for (y = 0; y < nGridCnt; y++)
                {
                    power = x * x + y * y;
                    power *= gridSize * gridSize;
                    expWeight[x, y] = Math.Exp(-power / (gridSize/6));
                }
            }
        }

        //Calculate KDE based on Gaussian Kernel
        private void GetnerateFullKDE()
        {
            int x = 0, y = 0;
            kde = new double[nGridCnt, nGridCnt];
            maxKde = 0;

            for (x = 0; x < nGridCnt; x++)
            {
                for (y = 0; y < nGridCnt; y++)
                {
                    if (!validPoints(x, y))
                        continue;

                    foreach(GridWeight gridWeight in gridWeights)
                        kde[x, y] += expWeight[Math.Abs(x- gridWeight.x), Math.Abs(y- gridWeight.y)] * gridWeight.weight;
                    maxKde = Math.Max(maxKde, kde[x, y]);
                }

            }
        }

        private bool validPoints(int x, int y)
        {
            int i = 0, k = 0;
            int nSum = 0;

            for (i = -2; i < 3; i ++)
            {
                if (x + i < 0 || x + i > nGridCnt - 1)
                    continue;
                for (k = -2; k < 3; k ++)
                {
                    if (y + k < 0 || y + k > nGridCnt - 1)
                        continue;
                    nSum += gridWeight[x, y];
                }
            }

            return nSum > 0;
        }

        //Make the KDE Curve smooth
        public void SmoothingKDE()
        {
            int x = 0, y = 0;
            int nRad = 5;
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
                            meanVal += tmpArr[i + x][k + y];
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

            for (i = 0; i < nPtCnt; i++)
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

            for (x = Math.Max(0, point[0] - nRadius); x <= Math.Min(nGridCnt - 1, point[0] + nRadius); x++)
            {
                for (y = Math.Max(0, point[1] - nRadius); y <= Math.Min(nGridCnt - 1, point[1] + nRadius); y++)
                {
                    /*                    if ((x- point[0]) * (x- point[0]) + (y- point[1]) * (y- point[1]) > R)
                                            continue;
                    */
                    if (max < kde[x, y])
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
            foreach (Cluster cluster in clusters)
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
                //MergeBottomClusters();
                DetectCellType();
                ManualAction();
            }
        }

        //Remove bottom-left debris
        private void RemoveBLDebris()
        {
            if (clusters.Count < 3)
                return;
            int i;
            int nMinX_T = nGridCnt / 10;

            for (i = clusters.Count - 1; i > -1; i--)
            {
                if (clusters[i].centerX < nMinX_T)
                    clusters.RemoveAt(i);
                if (clusters.Count < 3)
                    break;
            }

            //remove bottom debris
            for (i = clusters.Count - 1; i > -1; i--)
            {
                if (clusters[i].points.Count < totalData.Count() / nRate)
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
            for (i = clusters.Count - 1; i > -1; i--)
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
            int i = 0, nCnt = clusters.Count;
            Cluster currCluster;
            int minPoints = totalData.Count() / nRate;
            int minIdx = 0;

            for (i = nCnt-1; i > -1; i--)
            {
                currCluster = clusters[i];
                if (currCluster.points.Count > minPoints)
                    continue;
                
                if (currCluster.centerY < Global.T_Y_1 * (nGridCnt / normalY))
                    continue;

                minIdx = GetClosestCluster(i, minPoints);
                
                //Merge current cluster to closest cluster
                clusters[minIdx].points.AddRange(currCluster.points);
                clusters.RemoveAt(i);
            }
        }

        private void ManualAction()
        {
            Cluster lCluster = null;
            Cluster mCluster = null;

            foreach(Cluster cluster in clusters)
            {
                if (cluster.clusterName == Global.CELL_NAME[1])
                    lCluster = cluster;
                else if (cluster.clusterName == Global.CELL_NAME[2])
                    mCluster = cluster;
            }

            if (lCluster == null || mCluster == null)
                return;

            int i = 0, point;
            double t = Global.T_Y_1 / normalY;

            for (i = lCluster.points.Count-1; i > -1; i--)
            {
                point = lCluster.points[i];
                if ( totalData[point][0] > 0.67 && totalData[point][1] > t )
                {
                    mCluster.points.Add(point);
                    lCluster.points.RemoveAt(i);
                }
            }
        }

        private int GetClosestCluster(int nOrg, int minPoints)
        {
            int nIdx = 0, k, nCnt = clusters.Count;
            double MaxVal = (normalX * normalX + normalY * normalY) * 2;
            double minDist, dist, meanDist;
            
            minDist = MaxVal;
            for (k = 0; k < nCnt; k++)
            {
                if (nOrg == k || clusters[k].points.Count < minPoints)
                    continue;
                
                meanDist = 0;
                foreach(int orgPoint in clusters[nOrg].points)
                {
                    // Calculate distance between cluster and center of orientation cluster
                    dist = MaxVal;
                    foreach (int point in clusters[k].points)
                    {
                        dist = Math.Min(dist, euclidean(totalData[point], totalData[orgPoint]));
                        //dist = Math.Min(dist, Math.Abs(totalData[point][1] - totalData[orgPoint][1]) * 3 + Math.Abs(totalData[point][0] - totalData[orgPoint][0]));
                    }
                    meanDist += dist;
                }
                meanDist /= clusters[nOrg].points.Count;
                
                if (minDist > meanDist)
                {
                    minDist = meanDist;
                    nIdx = k;
                }
            }

            return nIdx;
        }
        //detect cluster type
        private void DetectCellType()
        {

            DetectCell(0, (int)(Global.T_Y_2 * (nGridCnt / normalY)), nGridCnt);
            DetectCell(1, 0, (int)(Global.T_Y_1 * (nGridCnt / normalY)));
            DetectCell(2, (int)(Global.T_Y_1 * (nGridCnt / normalY)), (int)(Global.T_Y_2 * (nGridCnt / normalY)));
        }

        private void DetectCell(int nType, int nT1, int nT2)
        {
            int i = 0;
            double sumPoints = 0;
            Cluster orientCluster = null;
            for (i = clusters.Count - 1; i > -1; i--)
            {
                // because of right tail of Monocytes, added this extra condition
                if ( (clusters[i].centerX < nGridCnt * 2 / 3 && 
                        clusters[i].centerY >= nT1 && clusters[i].centerY < nT2) ||
                    ( clusters[i].centerX >= nGridCnt * 2 / 3  &&
                        ( (nType == 1 && clusters[i].centerY >= nT1 && clusters[i].centerY < nT2) ||
                        (nType == 0 && clusters[i].centerY >= 5000 * (nGridCnt/ normalY)) ||
                        (nType == 2 && clusters[i].centerY >= nT1 && clusters[i].centerY < 5000 * (nGridCnt / normalY)))
                    ))
                {
                    if (orientCluster == null)
                    {
                        orientCluster = clusters[i];
                        orientCluster.clusterName = Global.CELL_NAME[nType];
                    }
                    else
                    {
                        sumPoints = orientCluster.points.Count + clusters[i].points.Count;
                        orientCluster.centerX = (int)(clusters[i].points.Count / sumPoints * clusters[i].centerX +
                            orientCluster.points.Count / sumPoints * orientCluster.centerX);
                        orientCluster.centerY = (int)(clusters[i].points.Count / sumPoints * clusters[i].centerY +
                            orientCluster.points.Count / sumPoints * orientCluster.centerY);
                        orientCluster.points.AddRange(clusters[i].points);
                        clusters.RemoveAt(i);
                    }
                }
            }

        }
        // detect cluster type based on diff3 gate
        private void DetectCellType_()
        {
            if (!Global.diff3_enable)
                return;

            // Get center of clustrs
            List<double[]> centers = new List<double[]>();
            foreach (Cluster cluster in clusters)
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
                for (i = 0; i < 3; i++)
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
                for (i = 1; i < 3; i++)
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

    class GridWeight
    {
        public int x { get; set; }
        public int y { get; set; }
        public int weight { get; set; }
    }
}
