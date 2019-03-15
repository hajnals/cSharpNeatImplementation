using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeatImplementation {
    /// <summary>
    /// This class will hold the configuration parameters of the project
    /// </summary>
    static class Cfg {
        // Population config parameters

        // Species config parameters
        public readonly static double ExcessCoeff = 1.0;
        public readonly static double ParamDiffCoeff = 0.5;
        public readonly static double CompatiblityThreshold = 3.0;
        public readonly static double PlayersLeftAlive = 0.5;
        public readonly static int LargGenomeNormalization = 20;
        // Player config parameters
        public readonly static int InputNodes = 2;
        public readonly static int OutputNodes = 1;
        public readonly static bool UseCopyInsteadOfCrossover = true;
        // Network config parameters
        public readonly static double DisableUnnecessaryGene = 0.75;
        // Node config parameters

        // Connection config parameters

    }
}
