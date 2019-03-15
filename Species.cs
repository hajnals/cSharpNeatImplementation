using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeatImplementation {

    class Species {
        // Counts the number of players
        private static int SpecCntr = 0;

        public List<Player> Members { get; set; }
        public Player BestPlayer { get; private set; }
        public double AvgFitness { get; private set; }
        public int ID { get; private set; }
        

        public Species() {
            Members = new List<Player>();
            BestPlayer = null;
            AvgFitness = 0.0;
            
            ID = SpecCntr++;
        }

        public void AddToSpecies(Player player) {
            Members.Add(player);
        }

        /// <summary>
        /// Compares the new player with the best player in this nice, 
        /// and determines wether the new player should belong to this species of not
        /// </summary>
        /// <param name="newPlayer"></param>
        /// <returns></returns>
        public bool IsSameSpecies(Player newPlayer) {
            // If the species empty return true
            if(Members.Count == 0) {
                return true;
            }

            // Update because we have player but no best player selected
            if(BestPlayer == null)
                Update();
            
            double compatibility = 0.0;

            // Get the similarity metrics: disjoint genes and average difference between the parameters
            int disjoint;
            double avgDelta;
            GetSimilarityMetrics(BestPlayer.Brain, newPlayer.Brain, out disjoint, out avgDelta);

            // Normalise large genomes, helps improve compatibility between big networks
            double normaliser = newPlayer.Brain.Connections.Count - Cfg.LargGenomeNormalization;
            if(normaliser < 1) normaliser = 1;

            compatibility = (Cfg.ExcessCoeff * disjoint / normaliser)
                            + (Cfg.ParamDiffCoeff * avgDelta);

            return Cfg.CompatiblityThreshold > compatibility;
        }

        /// <summary>
        /// Adds a new player to the niche by crossover or cloning, could return null!
        /// </summary>
        public Player GetBaby() {
            // Need at least one player in this species
            if(Members.Count < 1)
                return null;

            Player newPlayer = null;

            double chance = MyRand.UniformRnd(0, 1);

            // TODO opt. In this case it is unnecessary to decide which species should this belong
            // with 25% chance just clone a player || When we only have one member
            if(chance < 0.25 || Members.Count == 1) {
                int rndPlayer = MyRand.Next(0, Members.Count);
                newPlayer = Members[rndPlayer].DeepCopy();
                newPlayer.SetFitness(0);
                newPlayer.LifeSpawn = 0;
            }
            // Crossover 75% chance
            else {
                // We cant do crossover if there are less than 2 players
                if(Members.Count < 2)
                    return null;

                Player parentA = null;
                Player parentB = null;
                do {
                    int rndPlayerNumb = MyRand.Next(0, Members.Count);
                    parentA = Members[rndPlayerNumb];
                    rndPlayerNumb = MyRand.Next(0, Members.Count);
                    parentB = Members[rndPlayerNumb];
                } while(parentA.ID == parentB.ID);

                if(parentA.Fitness > parentB.Fitness)
                    newPlayer = parentA.Crossover(parentB);
                else
                    newPlayer = parentB.Crossover(parentA);
            }

            return newPlayer;
        }

        /// <summary>
        /// Kill a certain amount of the players
        /// </summary>
        public void Bloodlust(int share) {

            // Calc how many players should be killed
            int leftAlive = (int) Math.Round(share * Cfg.PlayersLeftAlive);
            
            // Iterate backwards so i can remove elements meanwhile
            for(int i = Members.Count - 1; i > leftAlive - 1; i--) {
                Members.RemoveAt(i);
            }
        }

        /// <summary>
        /// Sorts the list, updates the BestPlayer and the average fitness.
        /// </summary>
        public void Update() {
            SortPlayers();
            SetBestPlayer();
            AvgFitness = GetAvgFitness();
        }

        /* --------------------------------------------------------- Private methods */

        /// <summary>
        /// Sort player accoring to their fittnes in descending order
        /// </summary>
        private void SortPlayers() {
            Members = Members.OrderByDescending(player => player.Fitness).ToList();
        }

        private double GetAvgFitness() {
            double avgFitness = 0.0;

            if(Members.Count == 0) {
                return 0.0;
            }

            foreach(var player in Members) {
                avgFitness += player.Fitness;
            }

            return avgFitness / Members.Count;
        }

        /// <summary>
        /// Set the BestPlayer attribute
        /// </summary>
        private void SetBestPlayer() {
            BestPlayer = Members[0];
        }

        /// <summary>
        /// Returns the number of different genes
        /// </summary>
        /// <param name="netA"></param>
        /// <param name="netB"></param>
        /// <returns></returns>
        private void GetSimilarityMetrics(Network netA, Network netB,
                                     out int disjoint, out double avgDelta) {
            //TODO maybe it would be better to represent disjoint as a number of franction

            disjoint = 0;
            avgDelta = 0.0;

            // Get the number of matching genes
            foreach(var conA in netA.Connections) {
                foreach(var conB in netB.Connections) {
                    if(conA.InnovID == conB.InnovID) {
                        disjoint++;

                        // Add the differences in the parameters
                        avgDelta += Math.Abs(conA.Weight - conB.Weight)
                                     + Math.Abs(conA.Bias - conB.Bias);

                        break;
                    }
                }
            }

            // Do not divide by 0
            if(disjoint == 0)
                avgDelta = 100.0;
            else
                avgDelta = avgDelta / disjoint;

            disjoint = (netA.Connections.Count + netB.Connections.Count) - (2 * disjoint);
        }


    }
}
