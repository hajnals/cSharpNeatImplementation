using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeatImplementation {
    class Population {
        public List<Species> Species { get; set; }
        public Player BestPlayer { get; set; }
        public int Size { get; set; }

        private double SumAvgFitness { get; set; }

        // Constructor
        public Population(int size) {
            Size = size;
            Species = new List<Species>();
            BestPlayer = null;

            // We will need at least one new species
            Species.Add(new Species());

            // Create initial players, and add them to species
            for(int i = 0; i < Size; i++) {
                Player newPlayer = new Player();
                newPlayer.Mutate();
                // Add this player to a species
                AddToSpecies(newPlayer);
            }

        }

        // After the update every attribute will be available and phresh
        public void Update() {
            double bestFitness = Species[0].Members[0].Fitness;
            SumAvgFitness = 0.0;

            foreach(var species in Species) {
                // Gets the best player in each species, and gets the average fitness
                species.Update();

                // Determine the new best player
                if(bestFitness < species.BestPlayer.Fitness) {
                    bestFitness = species.BestPlayer.Fitness;
                    BestPlayer = species.BestPlayer.DeepCopy();
                }

                SumAvgFitness += species.AvgFitness;
            }
        }

        // Kills the badly graded player/species and creates new ones
        public void UnNaturalSelection() {

            // Kill player and species
            for(int i = Species.Count - 1; i >= 0; i--) {
                // Determine the share of this species from the population
                int share = (int) Math.Floor((Species[i].AvgFitness / SumAvgFitness) * Size);

                // We should delete species with less members
                if(share < Math.Ceiling(2 / (1 - Cfg.PlayersLeftAlive))) {
                    Species.RemoveAt(i);
                }
                else {
                    // Kill the halflings
                    Species[i].Bloodlust(share);
                }
                // DONT put anything after this, i-th element could be removed by now
            }

            // Iterate backwards so i can remove elements meanwhile
            for(int i = 0; i < Species.Count; i++) {
                // Determine the share of this species from the population
                int share = (int) Math.Floor((Species[i].AvgFitness / SumAvgFitness) * Size);

                // Create new players replacing the dead
                int newToCreate = share - Species[i].Members.Count;
                for(int j = 0; j < newToCreate; j++) {
                    Player newPlayer = Species[i].GetBaby();
                    // This shouldn't happen as we took precoutions just before
                    if(newPlayer == null)
                        throw new Exception($"Cannot generate a new player in this niche!");

                    // Mutate the new player
                    newPlayer.Mutate();

                    // Add this new player to a species, 
                    // it could happen that it will have his own, 
                    // or a different from where it was born
                    AddToSpecies(newPlayer);
                }
            }

            // Checking number of players
            int playerNum = 0;
            foreach(var s in Species) {
                playerNum += s.Members.Count;
            }
            if(playerNum > Size)
                throw new Exception("Too many players were assigned!");

            // Because of rounding errors, some spaces are not filled it, fill them in
            for(int space = 0; space < Size - playerNum; space++) {
                // Select a random species
                int i = MyRand.Next(Species.Count);
                // Get baby from the species
                Player newPlayer = Species[i].GetBaby();
                // Mutate the new player
                newPlayer.Mutate();
                // Add this new player to a species, 
                AddToSpecies(newPlayer);
            }
        }

        /// <summary>
        /// Adds the player to a species
        /// </summary>
        /// <param name="p"></param>
        private void AddToSpecies(Player p) {

            bool enlisted = false;

            foreach(var species in Species) {
                // Check if p fits into this species
                if(species.IsSameSpecies(p)) {
                    species.AddToSpecies(p);
                    enlisted = true;
                    break;
                }
            }

            // We might need to create a new species for this player
            if(enlisted == false) {
                Species newSpec = new Species();
                newSpec.AddToSpecies(p);
                Species.Add(newSpec);
            }
        }


    }
}
