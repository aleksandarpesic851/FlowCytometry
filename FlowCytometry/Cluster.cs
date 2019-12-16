using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowCytometry
{
    class Cluster
    {
        public int centerX { get; set; }
        public int centerY { get; set; }
        public List<int> points = new List<int>();
        public string clusterName { get; set; }
        public bool IsEqual(int[] point)
        {
            return (point[0] == centerX) && (point[1] == centerY);
        }
        public void AddNewPoint(int point)
        {
            points.Add(point);
        }
    }
}
