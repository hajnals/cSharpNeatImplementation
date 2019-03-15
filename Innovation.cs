using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeatImplementation {
    static class InnovationHandler {
        // How many innovations occured so far
        private static int innoCounter = 0;

        // Innovations are like connections, but need less information
        private static List<Innovation> InnovationList = new List<Innovation>();

        public static void GetInnovationID(Connection newInnovation) {

            /* Dont go into full detail about innovaton checking, just check the ID-s of the connected nodes.
             * Tt could hapen that the innovation is basucally the same even if the ID-s are different. 
             * We will ignore this case, due to it is very hard to detect, 
             * and has little effect on the on the result, it is actually making the identifying of the same species better, 
             * but its resource consumption drawback is huge. 
             * 
             * It is possible to solve this problem easily by giving out IDs that are containing informations about the 
             * creation of the innovation. For example if we add a new connection between 3 and 6 node,
             * we could name the new innovation 3-6,
             * but if we add a new innovation between 3-6 and 8-12 we have to rename it so, tell where to separate
             * like: 3-6-8-12 would be bad, because we dont know if 3 -- 6-8-12 or 3-6 -- 8-12 or any other combination
             * maybe (3)-(6) and (8)-(12) => ((3)-(6))-((8)-(12)) with this i can always (sure?) tell which of the nodes were connected
             * But i have to create a complicated reguar expression to find the borders, which would also be resource hangry.
             * TODO Maybe i will add this idea later */

            //Parameter error handling
            if(newInnovation.INode == null || newInnovation.ONode == null)
                throw new Exception("New innovation has no Ouput node defined!");

            // Check if this innovation has happened before or not.
            bool isNew = true;
            foreach(var oldInnovation in InnovationList) {
                // Check if this innvation has happened before
                if(oldInnovation.INodeID == newInnovation.INode.ID
                    && oldInnovation.ONodeID == newInnovation.ONode.ID) {
                    // This innovation has happened before.
                    newInnovation.InnovID = oldInnovation.InnovID;
                    isNew = false;
                }
            }

            if(isNew) {
                // It is a new innovation
                newInnovation.InnovID = innoCounter;
                innoCounter++;
                InnovationList.Add(new Innovation(newInnovation.INode.ID, newInnovation.ONode.ID, newInnovation.InnovID));
            }

        }

        /// <summary>
        /// Innovation is basically a reduced verison of a Connection class, 
        /// where we are only interested in the connected nodes.
        /// </summary>
        class Innovation {
            public int INodeID { get; set; }
            public int ONodeID { get; set; }
            public int InnovID { get; set; }

            public Innovation(int iNodeID, int oNodeID, int innovID) {
                INodeID = iNodeID;
                ONodeID = oNodeID;
                InnovID = innovID;
            }
        }
    }
}
