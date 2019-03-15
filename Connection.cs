using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeatImplementation {

    /// <summary>
    /// A connection is connecting two nodes.
    /// It will have information about whose to connect, the weight of the connection, state of the connection.
    /// It will only have references to the nodes, so the nodes are not belongs to it, rather the opposite,
    /// the connection belongs to a node.
    /// </summary>
    class Connection {
        public Node INode { get; set; }
        public Node ONode { get; set; }
        public double Weight { get; set; }
        public bool Enabled { get; set; }
        public int InnovID { get; set; }
        public double Bias { get; set; }

        /// <summary>
        /// Random uniform weight, and enabled
        /// </summary>
        /// <param name="node">The input node</param>
        public Connection(Node node) {
            INode = node;
            Weight = MyRand.UniformRnd(-1, 1);
            Bias = MyRand.UniformRnd(-1, 1);
            Enabled = true;
            ONode = null;   // Output nodes are not necessary defined immediately
            InnovID = 0;    // It will be overwritten
        }

        /// <summary>
        /// Copy connection
        /// </summary>
        /// <param name="iNode"></param>
        /// <param name="oNode"></param>
        /// <returns></returns>
        public Connection DeepCopy (Node iNode, Node oNode) {
            Connection c = new Connection(iNode);
            c.ONode = oNode;
            c.Weight = Weight;
            c.Enabled = Enabled;
            c.InnovID = InnovID;
            c.Bias = Bias;

            return c;
        }
    }
}
