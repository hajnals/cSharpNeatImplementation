using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeatImplementation {

    /// <summary>
    /// Defines a player, which will have a network.
    /// This player will belong to a species.
    /// </summary>
    class Player {
        // Counts the number of players
        private static int PlayerCntr = 0;

        public double Fitness { get; private set; }
        public Network Brain { get; private set; }  //Not a good idea to be able to set it, but needed for deepcopy
        public int ID { get; }
        public uint LifeSpawn { get; set; }  //How many cycles were survived

        // Constructor for a player, will automatically assign ID
        public Player() {
            ID = GetID();
            LifeSpawn = 0;
            Fitness = 0;
            
            Brain = new Network(Cfg.InputNodes, Cfg.OutputNodes);
        }

        public void SetFitness(double value) {
            if(value < 0)
                throw new Exception("Fitness must be positive!");
            Fitness = value;
        }

        public List<double> Think(List<double> input) {
            List<double> result = Brain.ProvideInputs(input);
            // Increase lifespawn
            LifeSpawn++;
            return result;
        }

        public void Mutate() {
            Brain.Mutate();
        }

        public Player DeepCopy() {
            Player p = new Player();
            p.Fitness = Fitness;
            p.LifeSpawn = LifeSpawn;
            p.Brain = Brain.DeepCopy();

            return p;
        }
        
        // Create a new player from two other players
        public Player Crossover(Player parentB) {
            Player child = new Player();

            // Just copying the better parent is much fater, than actually crossovering.
            if(Cfg.UseCopyInsteadOfCrossover) 
                child.Brain = Brain.DeepCopy();
            else 
                child.Brain = Brain.Crossover(parentB.Brain);

            return child;
        }

        // -------------------------- Private methods

        private int GetID() {
            return System.Threading.Interlocked.Increment(ref PlayerCntr);
        }
    }
}
