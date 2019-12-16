using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowCytometry
{
    class GMM
    {
        int d = 2;
        int K = 3;

        private double[][] w;   //Membership weights
        double[] a;             //Mixture weights
        double[][] u;           //Cluster mean
        double[][] V;           //Cluster variance
        double[] Nk;            //w column sum

        private double[][] pointData;             // origin data
        private int nPtCnt;                       // total number of data 
        private double normalX = 0;             // normalize param
        private double normalY = 0;             // normalize param

        public GMM(List<double[]> data, int k)
        {
            nPtCnt = data.Count;
            K = k;
            //make copy of data
            int i = 0;
            pointData = new double[nPtCnt][];
            for (i = 0; i < nPtCnt; i++)
            {
                pointData[i] = new double[2] { data[i][0], data[i][1] };
                normalX = Math.Max(pointData[i][0], normalX);
                normalY = Math.Max(pointData[i][1], normalY);
            }

            w = MatrixCreate(nPtCnt, K);
            a = new double[] { 1.0 / K, 1.0 / K, 1.0 / K };
            u = MatrixCreate(K, d);
            V = MatrixCreate(K, d, 0.01);
            Nk = new double[K];
        }

        public void CalculateClusters()
        {
            NormalizeData();

            for (int iter = 0; iter < 5; ++iter)
            {
                UpdateMembershipWts(w, pointData, u, V, a);  // E step
                UpdateNk(Nk, w);  // M steps
                UpdateMixtureWts(a, Nk);
                UpdateMeans(u, w, pointData, Nk);
                UpdateVariances(V, u, w, pointData, Nk);
            }
            DetectClusters();
        }

        //Normalize data so that all data points will be lie on [0,1]
        private void NormalizeData()
        {
            foreach (double[] element in pointData)
            {
                element[0] /= normalX;
                element[1] /= normalY;
            }
        }

        // Cluster Data based on membership  weight
        private void DetectClusters()
        {

        }
        #region GMM Module
        private void UpdateMembershipWts(double[][] w, double[][] x, double[][] u, double[][] V, double[] a)
        {
            for (int i = 0; i < nPtCnt; ++i)
            {
                double rowSum = 0.0;
                for (int k = 0; k < K; ++k)
                {
                    double pdf = NaiveProb(x[i], u[k], V[k]);
                    w[i][k] = a[k] * pdf;
                    rowSum += w[i][k];
                }
                for (int k = 0; k < K; ++k)
                    w[i][k] = w[i][k] / rowSum;
            }
        }
        private void UpdateNk(double[] Nk, double[][] w)
        {
            for (int k = 0; k < K; ++k)
            {
                double sum = 0.0;
                for (int i = 0; i < nPtCnt; ++i)
                    sum += w[i][k];
                Nk[k] = sum;
            }
        }
        private void UpdateMixtureWts(double[] a, double[] Nk)
        {
            for (int k = 0; k < K; ++k)
                a[k] = Nk[k] / nPtCnt;
        }
        private void UpdateMeans(double[][] u, double[][] w, double[][] x, double[] Nk)
        {
            double[][] result = MatrixCreate(K, d);
            for (int k = 0; k < K; ++k)
            {
                for (int i = 0; i < nPtCnt; ++i)
                    for (int j = 0; j < d; ++j)
                        result[k][j] += w[i][k] * x[i][j];
                for (int j = 0; j < d; ++j)
                    result[k][j] = result[k][j] / Nk[k];
            }
            for (int k = 0; k < K; ++k)
                for (int j = 0; j < d; ++j)
                    u[k][j] = result[k][j];
        }
        private void UpdateVariances(double[][] V, double[][] u, double[][] w, double[][] x, double[] Nk)
        {
            double[][] result = MatrixCreate(K, d);
            for (int k = 0; k < K; ++k)
            {
                for (int i = 0; i < nPtCnt; ++i)
                    for (int j = 0; j < d; ++j)
                        result[k][j] += w[i][k] * (x[i][j] - u[k][j]) *
                          (x[i][j] - u[k][j]);
                for (int j = 0; j < d; ++j)
                    result[k][j] = result[k][j] / Nk[k];
            }
            for (int k = 0; k < K; ++k)
                for (int j = 0; j < d; ++j)
                    V[k][j] = result[k][j];
        }
        private double ProbDenFunc(double x, double u, double v)
        {
            // Univariate Gaussian
            if (v == 0.0) throw new Exception("0 in ProbDenFun");
            double left = 1.0 / Math.Sqrt(2.0 * Math.PI * v);
            double pwr = -1 * ((x - u) * (x - u)) / (2 * v);
            return left * Math.Exp(pwr);
        }
        private double NaiveProb(double[] x, double[] u, double[] v)
        {
            // Poor man's multivariate Gaussian PDF
            double sum = 0.0;
            for (int j = 0; j < d; ++j)
                sum += ProbDenFunc(x[j], u[j], v[j]);
            return sum / d;
        }
        private double[][] MatrixCreate(int rows, int cols, double v = 0.0)
        {
            double[][] result = new double[rows][];
            for (int i = 0; i < rows; ++i)
                result[i] = new double[cols];
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < cols; ++j)
                    result[i][j] = v;
            return result;
        }
        #endregion GMM Module
    }
}
