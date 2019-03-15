using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// A network implements a neural network.
/// Basicly handles boundles of nodes and connections.
/// Every network will have one bias node, wich will be connected to every node.
/// The bias node will always have the value one.
/// </summary>
namespace NeatImplementation {

    enum Result {
        success,
        fail,
    }

    class Network {
        private int InputNodes { get; }
        private int OutputNodes { get; }
        public int OutputLayer { get; set; }

        public int NodeCnt { get; set; }
        public List<Node> Nodes { get; set; }
        public List<Connection> Connections { get; set; }

        // Creates a defaul network, starter network
        public Network(int inputNodes, int outputnodes) {
            Nodes = new List<Node>();
            Connections = new List<Connection>();
            NodeCnt = 0;        //By default 0, if we add more nodes it must increase!
            InputNodes = inputNodes;
            OutputNodes = outputnodes;
            OutputLayer = 1;    //By default 1, if we add more layers it must increase!

            // Create the network, by default without connections, Neat will add the connections
            // Add Input nodes
            for(int inputs = 0; inputs < InputNodes; inputs++) {
                Node n = new Node(NodeCnt);
                NodeCnt++;
                n.Layer = 0;
                Nodes.Add(n);
            }
            // Add Output nodes
            for(int inputs = 0; inputs < OutputNodes; inputs++) {
                Node n = new Node(NodeCnt);
                NodeCnt++;
                n.Layer = OutputLayer;
                Nodes.Add(n);
            }
        }
        
        // For creating child networks
        public Network(int inputNodes, int outputNodes, int outputLayer, int nodeCnt) {
            InputNodes = inputNodes;
            OutputNodes = outputNodes;
            OutputLayer = outputLayer;
            NodeCnt = nodeCnt;
            Nodes = new List<Node>();
            Connections = new List<Connection>();
        }

        public void Mutate() {

            // When no genes, add gene mutation
            if(Connections.Count == 0)
                AddConnection();

            double chance = MyRand.UniformRnd(0, 1);
            // 80% chance to modify weight or bias
            if(chance < 0.8) {
                ModifyConnection();
            }

            chance = MyRand.UniformRnd(0, 1);
            // 8% chance to add new connection
            if(chance < 0.08) {
                AddConnection();
            }

            chance = MyRand.UniformRnd(0, 1);
            // 2% chance to add new node
            if(chance < 0.02) {
                AddNode();
            }

            // No mutations today
        }

        // Add new connection to the network
        public Result AddConnection() {
            // Select a random node which will have an extra connections
            // Do not select nodes in the output layer

            // Check if network is fully connected
            if(IsNetworkFullyConnected())
                return Result.fail;

            Node fromNode = null;
            do {
                // Select a random node
                int rndNodeNumb = MyRand.Next(0, Nodes.Count);
                fromNode = Nodes[rndNodeNumb];

                //Find new node if this node is fully connected, OR in the output layer
            } while(IsNodeInOutputLayer(fromNode)
                     || IsNodeFullyConnected(fromNode)
                   );
            
            // Find a node to connect to
            Node toNode = null;
            do {
                // Select a random node
                int rndNodeNumb = MyRand.Next(0, Nodes.Count);
                toNode = Nodes[rndNodeNumb];

                // Find new node if this node is in the same layer, OR both already connected
            } while(    (toNode.Layer == fromNode.Layer)
                     || (IsConnected(fromNode, toNode))
                   );

            //If the tonode is in a lower layer than the fromnode, change positon
            if(toNode.Layer < fromNode.Layer) {
                Node tmp = toNode;
                toNode = fromNode;
                fromNode = tmp;
            }

            // Add connections between the two nodes with random weight
            Connection newConn = new Connection(fromNode);
            newConn.ONode = toNode;
            fromNode.Connections.Add(newConn);
            Connections.Add(newConn);

            // Assign innovation ID for new connection
            InnovationHandler.GetInnovationID(newConn);

            return Result.success;
        }

        // Add new node on an existing connection
        public Result AddNode() {
            if(Connections.Count == 0) {
                //Cannot Add new node
                return Result.fail;
            }

            //Get a random connection, it can be disabled too
            int rndConnNumb = MyRand.Next(0, Connections.Count);
            Connection rndConn = Connections[rndConnNumb];

            //Deactivate this connection
            rndConn.Enabled = false;

            //Add a new node on it
            Node newNode = new Node(NodeCnt);
            NodeCnt++;
            newNode.Layer = rndConn.INode.Layer + 1;

            //Connect INode and new node
            Connection conn1 = new Connection(rndConn.INode);
            conn1.Weight = 1.0;
            conn1.Bias = 0.0;
            conn1.Enabled = true;
            conn1.ONode = newNode;
            InnovationHandler.GetInnovationID(conn1);
            rndConn.INode.Connections.Add(conn1);

            //TODO i could consider if the connection was disabled to create two "low" effect connections

            //Connect new node and Onode
            Connection conn2 = new Connection(newNode);
            conn2.Weight = rndConn.Weight;
            conn2.Bias = rndConn.Bias;
            conn2.Enabled = true;
            conn2.ONode = rndConn.ONode;
            InnovationHandler.GetInnovationID(conn2);
            newNode.Connections.Add(conn2);

            //If the connection we are breaking is between layers that are next to eachother
            //it means we need to add a new layer
            //else we need to increase the first intermediate layer
            if((rndConn.INode.Layer + 1) == rndConn.ONode.Layer) {
                //New layer is needed

                //Shift every layer by one after the new layer
                foreach(var node in Nodes) {
                    if(node.Layer > rndConn.INode.Layer) {
                        node.Layer++;
                    }
                }

                OutputLayer++;
            }
            else if((rndConn.INode.Layer + 1) < rndConn.ONode.Layer) {
                //Layer should already exist
            }
            else {
                throw new Exception("Error: The connection is wrong!");
            }

            //Finally add new object to the lists
            Nodes.Add(newNode);
            Connections.Add(conn1);
            Connections.Add(conn2);

            return Result.success;
        }

        // Modify an existing connection, can also be turned Off or On
        public Result ModifyConnection() {

            // Check if we have connections or not
            if(Connections.Count == 0) {
                //Cannot Add new node
                return Result.fail;
            }

            // Check if every connection is disabled, like in LoL
            bool allDisabled = true;
            foreach(var c in Connections) {
                if(c.Enabled) {
                    allDisabled = false;
                    break;
                }

            }
            if(allDisabled)
                return Result.fail;

            // Select a connection which is not disabled
            Connection conn = null;
            do {
                int rndConNumb = MyRand.Next(0, Connections.Count);
                conn = Connections[rndConNumb];
            } while(!conn.Enabled);

            // Slightly modify this connection's weight or bias
            if(MyRand.UniformRnd(0, 1) > 0.3) {
                // Weight change
                conn.Weight = MyRand.NormalDist(conn.Weight, 0.1);
                // Not sure if limiting the weight is a good idea or not
                if(conn.Weight > 1.0)
                    conn.Weight = 1.0;
                if(conn.Weight < -1.0)
                    conn.Weight = -1.0;
            }
            else {
                // Bias change
                conn.Bias = MyRand.NormalDist(conn.Bias, 0.1);
                // Not sure if limiting the bias is a good idea or not
                if(conn.Bias > 5.0)
                    conn.Bias = 5.0;
                if(conn.Bias < -5.0)
                    conn.Bias = -5.0;
            }

            return Result.success;
        }

        // Toggle connection
        public Result ToggleConnection() {
            // Check if we have connections or not
            if(Connections.Count == 0) {
                //Cannot Add new node
                return Result.fail;
            }

            int rndConNumb = MyRand.Next(0, Connections.Count);
            Connection conn = Connections[rndConNumb];

            conn.Enabled = !conn.Enabled;

            return Result.success;
        }


        // Crossover this and NetworkB where this is the better parent
        public Network Crossover(Network networkB) {
            // Create a child network, which is very similar to this network
            Network child = new Network(InputNodes,
                                        OutputNodes,
                                        OutputLayer,
                                        NodeCnt);

            // At first we should Add the nodes, but how to choose which nodes to add?
            // Chose the better parent which is this
            foreach(var nodeA in Nodes) {
                Node childN = nodeA.DeepCopy(nodeA.ID);
                // we will add the connections later
                childN.Connections.Clear();
                child.Nodes.Add(childN);
            }

            //Go through the genes, and decide how to use them
            foreach(var conA in Connections) {
                bool enableGene = true;
                Connection gene = null;

                //Check if both parents have this gene
                Connection conB = networkB.FindConnection(conA.InnovID);
                if(conB != null) {
                    //Both have this gene

                    //Check if it is disabled in any of the parent and with 75% chance
                    if((!conA.Enabled || !conB.Enabled)
                        && (MyRand.UniformRnd(0, 1) < Cfg.DisableUnnecessaryGene)
                      ) {
                        // These gene doesnt look necessary
                        enableGene = false;
                    }

                    //Decide who will give the gene
                    if(MyRand.UniformRnd(0, 1) < 0.50) {
                        // Parent A
                        Node iNode = child.getNodeByID(conA.INode.ID);
                        Node oNode = child.getNodeByID(conA.ONode.ID);
                        gene = conA.DeepCopy(iNode, oNode);
                        iNode.Connections.Add(gene);
                    }
                    else {
                        // Parent B
                        Node iNode = child.getNodeByID(conB.INode.ID);
                        Node oNode = child.getNodeByID(conB.ONode.ID);
                        gene = conB.DeepCopy(iNode, oNode);
                        iNode.Connections.Add(gene);
                    }
                }
                else {
                    //Only parentA has this gene, disjoint or excess gene
                    Node iNode = child.getNodeByID(conA.INode.ID);
                    Node oNode = child.getNodeByID(conA.ONode.ID);
                    gene = conA.DeepCopy(iNode, oNode);
                    iNode.Connections.Add(gene);
                }

                gene.Enabled = enableGene;
                child.Connections.Add(gene);
            }

            return child;
        }

        // Creates a copy of a network
        public Network DeepCopy() {
            // Copy easy attributes
            Network net = new Network(InputNodes,
                                      OutputNodes,
                                      OutputLayer,
                                      NodeCnt);

            // Copy nodes, be aware the connections of the nodes are not pointion to a output node yet
            foreach(var node in Nodes) {
                Node newNode = node.DeepCopy(node.ID);
                newNode.Connections.Clear();
                net.Nodes.Add(newNode);
            }

            foreach(var conn in Connections) {
                Connection newConn = null;

                // Get the nodes we want to connect, based of what was connected in the original
                Node iNode = net.getNodeByID(conn.INode.ID);
                Node oNode = net.getNodeByID(conn.ONode.ID);
                // Create a connection which connects them
                newConn = conn.DeepCopy(iNode, oNode);
                // Add this connection to the INode and to the Connections of the Network
                iNode.Connections.Add(newConn);
                net.Connections.Add(newConn);
            }

            return net;
        }

        // Returns a specific node from the network or null if not found
        public Node getNodeByID(int ID) {
            foreach(var node in Nodes) {
                if(node.ID == ID) {
                    return node;
                }
            }

            return null;
        }

        public void TellMeAbout() {
            Console.WriteLine($"Number of nodes: {Nodes.Count}, " +
                              $"Number of connections {Connections.Count}");

            foreach(var node in Nodes) {
                Console.WriteLine($"Node: {node.ID}, " +
                                  $"Layer: {node.Layer}, " +
                                  $"Input: {node.InputValue}, " +
                                  $"Output: {node.OutputValue}, " +
                                  $"Connections: {node.Connections.Count}, " +
                                  $"Address: {node.GetHashCode()}");

                foreach(var conn in node.Connections) {
                    Console.WriteLine($"  Conn: " +
                                  $"InnovID: {conn.InnovID}, " +
                                  $"Connecting: {conn.INode.ID}-{conn.ONode.ID}, " +
                                  $"w: {conn.Weight}, " +
                                  $"b: {conn.Bias}, " +
                                  $"en: {conn.Enabled}, " +
                                  $"Address: {conn.GetHashCode()}");

                    Console.WriteLine($"    Input Node: {conn.INode.ID}, " +
                                  $"Layer: {conn.INode.Layer}, " +
                                  $"Input: {conn.INode.InputValue}, " +
                                  $"Output: {conn.INode.OutputValue}, " +
                                  $"Connections: {conn.INode.Connections.Count}, " +
                                  $"Address: {conn.INode.GetHashCode()}");

                    Console.WriteLine($"    Output Node: {conn.ONode.ID}, " +
                                  $"Layer: {conn.ONode.Layer}, " +
                                  $"Input: {conn.ONode.InputValue}, " +
                                  $"Output: {conn.ONode.OutputValue}, " +
                                  $"Connections: {conn.ONode.Connections.Count}, " +
                                  $"Address: {conn.ONode.GetHashCode()}");
                }
            }
        }

        public List<double> ProvideInputs(List<double> inputs) {
            // Input number is not equal to the number of input nodes
            if(inputs.Count != InputNodes) {
                throw new Exception($"Error: Incorrect input number!");
            }

            // Clear the old input values of the nodes.
            foreach(var node in Nodes) {
                node.InputValue = 0.0;
            }

            // The nodes in the input layer, Structure[0]
            for(int node = 0; node < InputNodes; node++) {
                // Assuming that the order of the input nodes is not changing
                Nodes[node].InputValue = inputs[node];
            }

            Calculate();

            // Get output, Assuming the order of the output layer is fixed and not changing.
            List<double> outputs = new List<double>();
            foreach(var node in Nodes) {
                if(node.Layer == OutputLayer) {
                    // TODO Output nodes dont have output values at the moment, 
                    // so it dont have any transformation
                    // But it is possible to apply any transformation to the output layer
                    outputs.Add(node.InputValue);
                }
            }

            return outputs;
        }


        // -------------------------------------- Private functions
        
        // Finds the connection with the specified innovationID 
        private Connection FindConnection(int innovID) {
            // Check every connection
            foreach(var con in Connections) {
                if(con.InnovID == innovID) {
                    return con;
                }
            }

            return null;
        }

        private bool IsConnected(Node fromNode, Node toNode) {

            bool connected = false;

            foreach(var conn in fromNode.Connections) {
                // If the fromnode has a connection which is connecting to the same node
                // they are connected
                if(conn.ONode.ID == toNode.ID)
                    connected = true;
            }

            foreach(var conn in toNode.Connections) {
                // If the fromnode has a connection which is connecting to the same node
                // they are connected
                if(conn.ONode.ID == fromNode.ID)
                    connected = true;
            }

            return connected;
        }

        /// <summary>
        /// Checks if network is fully connected
        /// </summary>
        /// <returns></returns>
        private bool IsNetworkFullyConnected() {
            //Check if every node has the maximum amount of connections

            bool fullyConnected = true; // Assume it is until proven otherwise

            foreach(var node in Nodes) {
                // If this node is in the output layer, skip it. 
                // Nodes in the output player don't have output connections
                if(node.Layer != OutputLayer) {
                    // If there is a not which is not fully connected, and not in the output layer
                    // that the network is not fully connected either
                    fullyConnected = IsNodeFullyConnected(node);
                }

                // End looping if not fully connected
                if(fullyConnected == false)
                    return fullyConnected;
            }

            return fullyConnected;
        }

        /// <summary>
        /// Check if node is fully connected
        /// </summary>
        /// <param name="nodeA"></param>
        /// <returns></returns>
        private bool IsNodeFullyConnected(Node nodeA) {
            // Check if we can find a node which can be connected to a node 
            // from an upper layer, to which is is not already connected

            foreach(var nodeB in Nodes) {
                // Check if nodeB is in a higher layer
                if((nodeA.Layer < nodeB.Layer)
                    && (!IsConnected(nodeA, nodeB))
                  ) {
                    return false;
                }
            }


            return true;

        }

        /// <summary>
        /// Check if node is in the output layer
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool IsNodeInOutputLayer(Node node) {
            return (node.Layer == OutputLayer);
        }

        private void Calculate() {
            // Feedforward by layers
            for(int layer = 0; layer < OutputLayer; layer++) {
                foreach(var node in Nodes) {
                    if(node.Layer == layer) {
                        node.FeedForward();
                    }
                }
            }

        }
    }
}
