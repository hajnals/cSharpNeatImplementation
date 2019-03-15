using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeatImplementation {

    /// <summary>
    /// A node is an object that is holding value, and is connected to other nodes.
    /// It will have inputs from other nodes, and will output value to other nodes.
    /// When the node is an input node, it wont have input connections.
    /// When the node is an output node, it wont have output connections.
    /// 
    /// Should a node know who is it connected to? 
    /// Or should a higher object held this information?
    /// 
    /// I think it should know the connected nodes to recieve and provide data without outside help.
    /// 
    /// </summary>
    class Node {

        /// <summary>
        /// The output connections of the node
        /// </summary>
        public List<Connection> Connections { get; set; }
        public double InputValue { get; set; }
        public double OutputValue { get; set; }
        public int Layer { get; set; }
        public int ID { get; }

        // Constructor
        public Node(int id) {
            ID = id;

            Connections = new List<Connection>();
            InputValue = 0;
            OutputValue = 0;
        }

        public void FeedForward() {
            // If this is the input layer, dont use sigmoid
            if(Layer == 0)
                OutputValue = InputValue;
            else
                OutputValue = Sigmoid(InputValue);

            foreach(var conn in Connections) {
                if(conn.Enabled)
                    conn.ONode.InputValue += (conn.Weight * OutputValue) + conn.Bias;
            }
        }
        
        /// <summary>
        /// Copies a node, but the output node to the connection must be added later!
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Node DeepCopy(int id) {
            Node n = new Node(id);
            n.Layer = Layer;
            n.OutputValue = OutputValue;
            n.InputValue = InputValue;

            foreach(var conn in Connections) {
                n.Connections.Add(
                    conn.DeepCopy(n, null));    //Connection which is not going to anywhere yet
            }
            return n;
        }

        public static double Sigmoid(double x) {
            return (1 / (1 + Math.Pow(Math.E, -x)));
        }
    }
}
