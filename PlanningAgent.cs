using System.Collections.Generic;
using System.Linq;
using GameManager.EnumTypes;
using GameManager.GameElements;
using UnityEngine;
using System;

/////////////////////////////////////////////////////////////////////////////
// This is the Moron Agent
/////////////////////////////////////////////////////////////////////////////

namespace GameManager
{
    ///<summary>Planning Agent is the over-head planner that decided where
    /// individual units go and what tasks they perform.  Low-level 
    /// AI is handled by other classes (like pathfinding).
    ///</summary> 
    public class PlanningAgent : Agent
    {
        private const int MAX_NBR_WORKERS = 20;

        #region Private Data

        ///////////////////////////////////////////////////////////////////////
        // Handy short-cuts for pulling all of the relevant data that you
        // might use for each decision.  Feel free to add your own.
        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The enemy's agent number
        /// </summary>
        private int enemyAgentNbr { get; set; }

        /// <summary>
        /// My primary mine number
        /// </summary>
        private int mainMineNbr { get; set; }

        /// <summary>
        /// My primary base number
        /// </summary>
        private int mainBaseNbr { get; set; }

        /// <summary>
        /// List of all the mines on the map
        /// </summary>
        private List<int> mines { get; set; }

        /// <summary>
        /// List of all of my workers
        /// </summary>
        private List<int> myWorkers { get; set; }

        /// <summary>
        /// List of all of my soldiers
        /// </summary>
        private List<int> mySoldiers { get; set; }

        /// <summary>
        /// List of all of my archers
        /// </summary>
        private List<int> myArchers { get; set; }

        /// <summary>
        /// List of all of my bases
        /// </summary>
        private List<int> myBases { get; set; }

        /// <summary>
        /// List of all of my barracks
        /// </summary>
        private List<int> myBarracks { get; set; }

        /// <summary>
        /// List of all of my refineries
        /// </summary>
        private List<int> myRefineries { get; set; }

        /// <summary>
        /// List of the enemy's workers
        /// </summary>
        private List<int> enemyWorkers { get; set; }

        /// <summary>
        /// List of the enemy's soldiers
        /// </summary>
        private List<int> enemySoldiers { get; set; }

        /// <summary>
        /// List of enemy's archers
        /// </summary>
        private List<int> enemyArchers { get; set; }

        /// <summary>
        /// List of the enemy's bases
        /// </summary>
        private List<int> enemyBases { get; set; }

        /// <summary>
        /// List of the enemy's barracks
        /// </summary>
        private List<int> enemyBarracks { get; set; }

        /// <summary>
        /// List of the enemy's refineries
        /// </summary>
        private List<int> enemyRefineries { get; set; }

        /// <summary>
        /// List of the possible build positions for a 3x3 unit
        /// </summary>
        private List<Vector3Int> buildPositions { get; set; }

        /// <summary>
        /// Finds all of the possible build locations for a specific UnitType.
        /// Currently, all structures are 3x3, so these positions can be reused
        /// for all structures (Base, Barracks, Refinery)
        /// Run this once at the beginning of the game and have a list of
        /// locations that you can use to reduce later computation.  When you
        /// need a location for a build-site, simply pull one off of this list,
        /// determine if it is still buildable, determine if you want to use it
        /// (perhaps it is too far away or too close or not close enough to a mine),
        /// and then simply remove it from the list and build on it!
        /// This method is called from the Awake() method to run only once at the
        /// beginning of the game.
        /// </summary>
        /// <param name="unitType">the type of unit you want to build</param>
        public void FindProspectiveBuildPositions(UnitType unitType)
        {
            // For the entire map
            for (int i = 0; i < GameManager.Instance.MapSize.x; ++i)
            {
                for (int j = 0; j < GameManager.Instance.MapSize.y; ++j)
                {
                    // Construct a new point near gridPosition
                    Vector3Int testGridPosition = new Vector3Int(i, j, 0);

                    // Test if that position can be used to build the unit
                    if (Utility.IsValidGridLocation(testGridPosition)
                        && GameManager.Instance.IsBoundedAreaBuildable(unitType, testGridPosition))
                    {
                        // If this position is buildable, add it to the list
                        buildPositions.Add(testGridPosition);
                    }
                }
            }
        }

        /// <summary>
        /// Build a building
        /// </summary>
        /// <param name="unitType"></param>
        public void BuildBuilding(UnitType unitType)
        {
            // For each worker
            foreach (int worker in myWorkers)
            {
                // Grab the unit we need for this function
                Unit unit = GameManager.Instance.GetUnit(worker);

                // Make sure this unit actually exists and we have enough gold
                if (unit != null && Gold >= Constants.COST[unitType])
                {
                    // Find the closest build position to this worker's position (DUMB) and 
                    // build the base there
                    foreach (Vector3Int toBuild in buildPositions)
                    {
                        if (GameManager.Instance.IsBoundedAreaBuildable(unitType, toBuild))
                        {
                            Build(unit, toBuild, unitType);
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Attack the enemy
        /// </summary>
        /// <param name="myTroops"></param>
        public void AttackEnemy(List<int> myTroops)
        {
            if (myTroops.Count > 3)
            {
                // For each of my troops in this collection
                foreach (int troopNbr in myTroops)
                {
                    // If this troop is idle, give him something to attack
                    Unit troopUnit = GameManager.Instance.GetUnit(troopNbr);
                    if (troopUnit.CurrentAction == UnitAction.IDLE)
                    {
                        // If there are archers to attack
                        if (enemyArchers.Count > 0)
                        {
                            Attack(troopUnit, GameManager.Instance.GetUnit(enemyArchers[UnityEngine.Random.Range(0, enemyArchers.Count)]));
                        }
                        // If there are soldiers to attack
                        else if (enemySoldiers.Count > 0)
                        {
                            Attack(troopUnit, GameManager.Instance.GetUnit(enemySoldiers[UnityEngine.Random.Range(0, enemySoldiers.Count)]));
                        }
                        // If there are workers to attack
                        else if (enemyWorkers.Count > 0)
                        {
                            Attack(troopUnit, GameManager.Instance.GetUnit(enemyWorkers[UnityEngine.Random.Range(0, enemyWorkers.Count)]));
                        }
                        // If there are bases to attack
                        else if (enemyBases.Count > 0)
                        {
                            Attack(troopUnit, GameManager.Instance.GetUnit(enemyBases[UnityEngine.Random.Range(0, enemyBases.Count)]));
                        }
                        // If there are barracks to attack
                        else if (enemyBarracks.Count > 0)
                        {
                            Attack(troopUnit, GameManager.Instance.GetUnit(enemyBarracks[UnityEngine.Random.Range(0, enemyBarracks.Count)]));
                        }
                        // If there are refineries to attack
                        else if (enemyRefineries.Count > 0)
                        {
                            Attack(troopUnit, GameManager.Instance.GetUnit(enemyRefineries[UnityEngine.Random.Range(0, enemyRefineries.Count)]));
                        }
                    }
                }
            }
            else if (myTroops.Count > 0)
            {
                // Find a good rally point
                Vector3Int rallyPoint = Vector3Int.zero;
                foreach (Vector3Int toBuild in buildPositions)
                {
                    if (GameManager.Instance.IsBoundedAreaBuildable(UnitType.BASE, toBuild))
                    {
                        rallyPoint = toBuild;
                        // For each of my troops in this collection
                        foreach (int troopNbr in myTroops)
                        {
                            // If this troop is idle, give him something to attack
                            Unit troopUnit = GameManager.Instance.GetUnit(troopNbr);
                            if (troopUnit.CurrentAction == UnitAction.IDLE)
                            {
                                Move(troopUnit, rallyPoint);
                            }
                        }
                        break;
                    }
                }
            }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Called at the end of each round before remaining units are
        /// destroyed to allow the agent to observe the "win/loss" state
        /// </summary>
        public override void Learn()
        {
            Debug.Log("Nbr Wins: " + AgentNbrWins);

            //Debug.Log("PlanningAgent::Learn");
            Log("value 1");
            Log("value 2");
            Log("value 3a, 3b");
            Log("value 4");
        }

        /// <summary>
        /// Called before each match between two agents.  Matches have
        /// multiple rounds. 
        /// </summary>
        public override void InitializeMatch()
        {
            Debug.Log("Moron's: " + AgentName);
            //Debug.Log("PlanningAgent::InitializeMatch");
        }

        /// <summary>
        /// Called at the beginning of each round in a match.
        /// There are multiple rounds in a single match between two agents.
        /// </summary>
        public override void InitializeRound()
        {
            //Debug.Log("PlanningAgent::InitializeRound");
            buildPositions = new List<Vector3Int>();

            FindProspectiveBuildPositions(UnitType.BASE);

            // Set the main mine and base to "non-existent"
            mainMineNbr = -1;
            mainBaseNbr = -1;

            // Initialize all of the unit lists
            mines = new List<int>();

            myWorkers = new List<int>();
            mySoldiers = new List<int>();
            myArchers = new List<int>();
            myBases = new List<int>();
            myBarracks = new List<int>();
            myRefineries = new List<int>();

            enemyWorkers = new List<int>();
            enemySoldiers = new List<int>();
            enemyArchers = new List<int>();
            enemyBases = new List<int>();
            enemyBarracks = new List<int>();
            enemyRefineries = new List<int>();
        }

        /// <summary>
        /// Updates the game state for the Agent - called once per frame for GameManager
        /// Pulls all of the agents from the game and identifies who they belong to
        /// </summary>
        public void UpdateGameState()
        {
            // Update the common resources
            mines = GameManager.Instance.GetUnitNbrsOfType(UnitType.MINE);

            // Update all of my unitNbrs
            myWorkers = GameManager.Instance.GetUnitNbrsOfType(UnitType.WORKER, AgentNbr);
            mySoldiers = GameManager.Instance.GetUnitNbrsOfType(UnitType.SOLDIER, AgentNbr);
            myArchers = GameManager.Instance.GetUnitNbrsOfType(UnitType.ARCHER, AgentNbr);
            myBarracks = GameManager.Instance.GetUnitNbrsOfType(UnitType.BARRACKS, AgentNbr);
            myBases = GameManager.Instance.GetUnitNbrsOfType(UnitType.BASE, AgentNbr);
            myRefineries = GameManager.Instance.GetUnitNbrsOfType(UnitType.REFINERY, AgentNbr);

            // Update the enemy agents & unitNbrs
            List<int> enemyAgentNbrs = GameManager.Instance.GetEnemyAgentNbrs(AgentNbr);
            if (enemyAgentNbrs.Any())
            {
                enemyAgentNbr = enemyAgentNbrs[0];
                enemyWorkers = GameManager.Instance.GetUnitNbrsOfType(UnitType.WORKER, enemyAgentNbr);
                enemySoldiers = GameManager.Instance.GetUnitNbrsOfType(UnitType.SOLDIER, enemyAgentNbr);
                enemyArchers = GameManager.Instance.GetUnitNbrsOfType(UnitType.ARCHER, enemyAgentNbr);
                enemyBarracks = GameManager.Instance.GetUnitNbrsOfType(UnitType.BARRACKS, enemyAgentNbr);
                enemyBases = GameManager.Instance.GetUnitNbrsOfType(UnitType.BASE, enemyAgentNbr);
                enemyRefineries = GameManager.Instance.GetUnitNbrsOfType(UnitType.REFINERY, enemyAgentNbr);
                Debug.Log("<color=red>Enemy gold</color>: " + GameManager.Instance.GetAgent(enemyAgentNbr).Gold);
            }
        }

        /// <summary>
        /// Update the GameManager - called once per frame
        /// </summary>
        public override void Update()
        {
            UpdateGameState();

            if (mines.Count > 0)
            {
                mainMineNbr = mines[0];
            }
            else
            {
                mainMineNbr = -1;
            }

            
            // If we have at least one base, assume the first one is our "main" base
            if (myBases.Count > 0)
            {
                mainBaseNbr = myBases[0];
                //Debug.Log("BaseNbr " + mainBaseNbr);
                //Debug.Log("MineNbr " + mainMineNbr);
            }
            int state = 0;
            float stateVal = InitialBuildHeuristic();
            float defend = DefendHeuristic();
            float buildArmy = BuildArmyHeuristic();
            float win = WinHeuristic();
            float recover = RecoverHeuristic();

            if(stateVal < defend)
            {
                state = 1;
                stateVal = defend;
            }
            if (stateVal < buildArmy)
            {
                state = 2;
                stateVal = buildArmy;
            }
            if (stateVal < win) 
            {
                state = 3;
                stateVal = win;
            }
            if(stateVal < recover)
            {
                state = 4;
                stateVal = recover;
            }
            //selecting the state
            switch(state) 
            {
                case 0: 
                    
                        InitialBuildBaseState(); break;
                    
                case 1:
                    
                        DefendingState(); break;
                    
                case 2:
                    
                        BuildArmyState(); break;
                    
                case 3:
                    
                        WinningState(); break;
                    
                case 4:
                    
                        RecoveringState(); break;
                    
            }





        }

        #endregion

        #region Private Methods
        #region Heuristics
        /// <summary>
        /// Do we need to start fresh
        /// </summary>
        /// <returns></returns>
        private float InitialBuildHeuristic()
        {
            float score = 0;
            float bases = (2 - myBases.Count)/2;
            float workers = -((MAX_NBR_WORKERS-myWorkers.Count)/MAX_NBR_WORKERS);
            float barracks = (1 - myBarracks.Count);
            score = Math.Clamp((((bases/2)+(workers/4)+(barracks/4))),0,1);
            //Debug.Log("<color=pink>InitialBuildHeuristic: </color>" + score);
            return score;
        }
        /// <summary>
        /// Do we need to defend based on our opponent, and do we need a better economy?
        /// </summary>
        /// <returns></returns>
        private float DefendHeuristic()
        {
            float score = 0;
            float barracks = GenericHeuristic(myBarracks.Count,enemyBarracks.Count);
            float workers = -((MAX_NBR_WORKERS - myWorkers.Count) / MAX_NBR_WORKERS);
            float refineries = GenericHeuristic(myRefineries.Count,enemyRefineries.Count);
            int ourArmy = mySoldiers.Count + myArchers.Count;
            int theirArmy = enemySoldiers.Count + enemyArchers.Count;
            float threat = GenericHeuristic(ourArmy, theirArmy);
            score = Math.clamp(((barracks / 4) + (workers / 8) + (refineries / 8) + (threat / 2)), 0, 1);
            //Debug.Log("<color=pink>DefendHeuristic: </color>" + score);
            return score;
        }
        /// <summary>
        /// for determining if we should build a base. yes if we dont have one, maybe if we have a good economy, no if else
        /// </summary>
        /// <returns></returns>
        private float BaseBuildHeuristic()
        {
            float score = 0;
            float bases = (2 - myBases.Count) / 2;
            float gold = -(1 / Gold - (2 * Constants.COST[UnitType.BASE]));
            float refineries = - GenericHeuristic(myRefineries.Count, enemyRefineries.Count);
            float barracks = - GenericHeuristic(myBarracks.Count, enemyBarracks.Count);
            float workers = -((MAX_NBR_WORKERS - myWorkers.Count) / MAX_NBR_WORKERS);

            score = Math.Clamp((bases+gold+refineries+barracks+workers), 0, 1);
            //Debug.Log("<color=pink>BaseBuildHeuristic: </color>" + score);
            return score;
        }
        /// <summary>
        /// Do we need a slave? Yes if low slave count, no if at max or need to defend
        /// </summary>
        /// <returns></returns>
        private float WorkerBuildHeuristic()
        {
            float score = 0;
            float workers = ((MAX_NBR_WORKERS - myWorkers.Count) / MAX_NBR_WORKERS);
            float gold = -(1 / Gold - (2 * Constants.COST[UnitType.WORKER]));
            float defense = -DefendHeuristic();
            score = Math.Clamp((workers+gold+defense),0, 1);
            //Debug.Log("<color=pink>WorkerBuildHeuristic: </color>" + score);
            return score;
        }
        /// <summary>
        /// Do we need a refinery? Yes if can, no if not safe
        /// </summary>
        /// <returns></returns>
        private float RefineryBuildHeuristic()
        {
            float score = 0;
            float gold = -(1 / Gold - (2 * Constants.COST[UnitType.REFINERY]));
            float workers = -((MAX_NBR_WORKERS - myWorkers.Count) / MAX_NBR_WORKERS);
            float barracks = -GenericHeuristic(myBarracks.Count, enemyBarracks.Count);
            float refineries = GenericHeuristic(myRefineries.Count, enemyRefineries.Count);

            score = Math.Clamp((gold+workers+barracks+refineries),0, 1);
            //Debug.Log("<color=pink>RefineryBuildHeuristic: </color>" + score);
            return score;
        }

        /// <summary>
        /// Do we need a barracks? yes if opponent has more, no if our economy cant support it
        /// </summary>
        /// <returns></returns>
        private float BarracksBuildHeuristic()
        {
            float score = 0;
            float barracks = GenericHeuristic(myBarracks.Count, enemyBarracks.Count);
            float refineries = -GenericHeuristic(myRefineries.Count, enemyRefineries.Count);
            float gold = -(1 / Gold - (2 * Constants.COST[UnitType.BARRACKS]));
            float workers = -((MAX_NBR_WORKERS - myWorkers.Count) / MAX_NBR_WORKERS);

            score = Math.Clamp((barracks+refineries+gold+workers),0, 1);
            //Debug.Log("<color=pink>BarracksBuildHeuristic: </color>" + score);
            return score;
        }

        /// <summary>
        /// Can we start building our army, no if our economy cant or under direct threat, tes if else
        /// </summary>
        /// <returns></returns>
        private float BuildArmyHeuristic()
        {
            float score = 0;
            float defense = -DefendHeuristic();
            float barracks = -GenericHeuristic(enemyBarracks.Count, myBarracks.Count);
            float refineries = -GenericHeuristic(enemyRefineries.Count, myRefineries.Count);
            
            score = Math.Clamp((defense + barracks+ refineries),0, 1);
            //Debug.Log("<color=pink>BuildArmyHeuristic: </color>" + score);
            return score;
        }
        /// <summary>
        /// Do we need a Soldier, yes if enemy outnumbers us or if can, no if our economy could be better
        /// </summary>
        /// <returns></returns>
        private float SoldierBuildHeuristic()
        {
            float score = 0;
            float threat = GenericHeuristic(mySoldiers.Count, enemySoldiers.Count);
            float gold = -(1 / Gold - (Constants.COST[UnitType.SOLDIER]));
            float barracks = - GenericHeuristic(myBarracks.Count, enemyBarracks.Count);

            score = Math.clamp(((threat)+(gold)+(barracks)),0, 1);
            //Debug.Log("<color=pink>SoldierBuildHeuristic: </color>" + score);
            return score;
        }
        /// <summary>
        /// Do we need an Archer, yes if enemy outnumbers us or if can, no if our economy could be better
        /// </summary>
        /// <returns></returns>
        private float ArcherBuildHeuristic()
        {
            float score = 0;
            float threat = GenericHeuristic(myArchers.Count, enemyArchers.Count);
            float gold = -(1 / Gold - (Constants.COST[UnitType.ARCHER]));
            float barracks = -GenericHeuristic(myBarracks.Count, enemyBarracks.Count);

            score = Math.clamp(((threat) + (gold) + (barracks)), 0, 1);
            //Debug.Log("<color=pink>ArcherBuildHeuristic: </color>" + score);
            return score;
        }
        /// <summary>
        /// Can we get the upper hand? yes if we outnumber no if we need to build up
        /// </summary>
        /// <returns></returns>
        private float WinHeuristic()
        {
            float score = 0;
            int ourArmy = mySoldiers.Count + myArchers.Count;
            int theirArmy = enemySoldiers.Count + enemyArchers.Count;
            float defense = -DefendHeuristic();
            float threat = GenericHeuristic(ourArmy, theirArmy);

            score = Math.clamp(((defense/2)+(1-threat)),0,1);
            //Debug.Log("<color=pink>WinHeuristic: </color>" + score);
            return score;
        }
        /// <summary>
        /// Do we need to rebuild, yes if our economy is bad, no if else
        /// </summary>
        /// <returns></returns>
        private float RecoverHeuristic()
        {
            float score = 0;
            float workers = ((MAX_NBR_WORKERS - myWorkers.Count) / MAX_NBR_WORKERS);
            float barracks = GenericHeuristic(myBarracks.Count, enemyBarracks.Count);
            float bases = (2 - myBases.Count) / 2;

            score = Math.clamp(((workers/6)+(barracks/6)+(bases/3)),0,1);
            //Debug.Log("<color=pink>RecoverHeuristic: </color>" + score);
            return score;
        }

        /// <summary>
        /// Should our troops attack? yes if in need of defense, or winning no if else
        /// </summary>
        /// <returns></returns>
        private float AttackHeuristic()
        {
            float score = 0;
            int ourArmy = mySoldiers.Count + myArchers.Count;
            int theirArmy = enemySoldiers.Count + enemyArchers.Count;
            float threat = GenericHeuristic(theirArmy, ourArmy);
            //Debug.Log("<color=pink>AttackHeuristic: </color>" + score);
            return score;
        }

        /// <summary>
        /// Most of the heuristics use this formula
        /// </summary>
        /// <returns></returns>
        private float GenericHeuristic(int val1, int val2)
        {
            float score = 0;
            score = Math.clamp((((2 * val2) - val1 + 1) / ((2 * val2) + 1)),0,1);
            //Debug.Log("<color=red>GenericHeuristic: </color>" + score);
            return score;
        }
        #endregion
        #region State Machine
        /// <summary>
        /// Build an economic foundation with bases, barracks, and workers to gather resources efficiently.
        /// </summary>
        private void InitialBuildBaseState()
        {
            Debug.Log("<color=green>Entering Initial Build Base State</color>");
            //for decision making
            float defense = DefendHeuristic();
            float worker = WorkerBuildHeuristic();
            float bases = BaseBuildHeuristic();
            float refinery = RefineryBuildHeuristic();
            float barracks = BarracksBuildHeuristic();
            float armyBuild = BuildArmyHeuristic();
            int state = 0;
            float stateVal = 0;
            if(stateVal < defense) 
            {
                stateVal = defense;
            }
            if(stateVal < worker)
            {
                stateVal = worker;
                state = 1;
            }
            if(stateVal < bases) 
            {
                stateVal = bases;
                state = 2;
            }
            if(stateVal < refinery) 
            {
                stateVal = refinery;
                state = 3;
            }
            if(stateVal < barracks)
            {
                stateVal = barracks;
                state = 4;
            }
            if(stateVal < armyBuild)
            {
                stateVal = armyBuild;
                state = 5;
            }
            switch (state) 
            {
                case 0:
                    DefendingState();
                    break;
                case 1:
                    //Train workers from bases until sufficient workers are built
                    foreach (int baseNbr in myBases)
                    {
                        // Get the base unit
                        Unit baseUnit = GameManager.Instance.GetUnit(baseNbr);

                        // If the base exists, is idle, we need a worker, and we have gold
                        if (baseUnit != null && baseUnit.IsBuilt
                                                && baseUnit.CurrentAction == UnitAction.IDLE
                                                && Gold >= Constants.COST[UnitType.WORKER]
                                                && myWorkers.Count < MAX_NBR_WORKERS)
                        {
                            Train(baseUnit, UnitType.WORKER);
                        }
                    }
                    break;
                case 2:
                    if (myBases.Count == 0)
                    {
                        mainBaseNbr = -1;

                        BuildBuilding(UnitType.BASE);
                    }
                    break;
                case 3:
                    if (Gold >= Constants.COST[UnitType.REFINERY])
                    {
                        BuildBuilding(UnitType.REFINERY);
                    }
                    break;
                case 4:
                    if (Gold >= Constants.COST[UnitType.BARRACKS])
                    {
                        BuildBuilding(UnitType.BARRACKS);
                    }
                    break;
                case 5:
                    BuildArmyState();
                    break;

            }
            /*
            if ((myWorkers.Count >= MAX_NBR_WORKERS && myRefineries.Count > 0)|| (enemyBarracks.Count > 0 && myWorkers.Count >= MAX_NBR_WORKERS/2))
            {
                DefendingState();
            }
            //IF number of workers<MAX_NBR_WORKERS
            //Train workers from bases until sufficient workers are built
            foreach (int baseNbr in myBases)
            {
                // Get the base unit
                Unit baseUnit = GameManager.Instance.GetUnit(baseNbr);

                // If the base exists, is idle, we need a worker, and we have gold
                if (baseUnit != null && baseUnit.IsBuilt
                                     && baseUnit.CurrentAction == UnitAction.IDLE
                                     && Gold >= Constants.COST[UnitType.WORKER]
                                     && myWorkers.Count < MAX_NBR_WORKERS
                                     && (mySoldiers.Count+ myArchers.Count >= enemySoldiers.Count + enemyArchers.Count)
                                     )
                {
                    Train(baseUnit, UnitType.WORKER);
                }
            }
            //FOR each worker
            //IF worker is idle
            //Assign worker to gather gold or build a building
            foreach (int worker in myWorkers)
            {
                // Grab the unit we need for this function
                Unit unit = GameManager.Instance.GetUnit(worker);

                // Make sure this unit actually exists and is idle
                if (unit != null && unit.CurrentAction == UnitAction.IDLE && mainBaseNbr >= 0 && mainMineNbr >= 0)
                {
                    // Grab the mine
                    Unit mineUnit = GameManager.Instance.GetUnit(mainMineNbr);
                    Unit baseUnit = GameManager.Instance.GetUnit(mainBaseNbr);
                    if (mineUnit != null && baseUnit != null && mineUnit.Health > 0)
                    {
                        Gather(unit, mineUnit, baseUnit);
                    }
                }
            }
            //IF number of bases< 2
            //Attempt to build a base at the closest valid position
            if (myBases.Count == 0)
            {
                mainBaseNbr = -1;

                BuildBuilding(UnitType.BASE);
            }
            //if in rome, build a refinery (when its convenient)
            if (myWorkers.Count >= MAX_NBR_WORKERS / 2 && Gold >= Constants.COST[UnitType.REFINERY] && myRefineries.Count == 0 && myBarracks.Count > 0)
            {
                BuildBuilding(UnitType.REFINERY);
            }
            //build one barracks
            if (myWorkers.Count >= MAX_NBR_WORKERS / 2 && Gold >= Constants.COST[UnitType.BARRACKS] && myBarracks.Count == 0)
            {
                BuildBuilding(UnitType.BARRACKS);
            }
            //IF base - building requirements are satisfied
            //Transition to Army Building state
            if (myBases.Count > 0 && myBarracks.Count > 0
                                && myRefineries.Count > 0
                                && myWorkers.Count >= MAX_NBR_WORKERS)
            {
                BuildArmyState();
            }
            */
        }


        /// <summary>
        /// Build an army worthy of mordor with a strategic size relative to the opponent's forces.
        /// </summary>
        private void BuildArmyState()
        {
            Debug.Log("<color=green>Entering Build Army State</color>");
            int state = 0;
            float stateVal = 0;
            float soldier = SoldierBuildHeuristic();
            float archer = ArcherBuildHeuristic();
            float win = WinHeuristic();
            float barracks = BarracksBuildHeuristic();
            float defense = DefendHeuristic();
            if(stateVal < soldier)
            {
                stateVal = soldier;
            }
            if(stateVal < archer) 
            {
                stateVal = archer;
                state = 1;
            }
            if(stateVal < win)
            {
                stateVal = win;
                state = 2;
            }
            if(stateVal < barracks)
            {
                stateVal = barracks;
                state = 3;
            }
            if(stateVal < defense)
            {
                stateVal = defense;
                state = 4;
            }
            
            switch (state) 
            {
                case 0:
                    // For each barracks, determine if it should train a soldier or an archer
                    foreach (int barracksNbr in myBarracks)
                    {
                        // Get the barracks
                        Unit barracksUnit = GameManager.Instance.GetUnit(barracksNbr);

                        // If this barracks still exists, is idle, we need soldiers, and have gold
                        if (barracksUnit != null && barracksUnit.IsBuilt
                            && barracksUnit.CurrentAction == UnitAction.IDLE
                            && Gold >= Constants.COST[UnitType.SOLDIER])
                        {
                            Train(barracksUnit, UnitType.SOLDIER);
                        }
                    }
                    break; 
                case 1:
                    foreach (int barracksNbr in myBarracks)
                    {
                        // Get the barracks
                        Unit barracksUnit = GameManager.Instance.GetUnit(barracksNbr);

                        // If this barracks still exists, is idle, we need archers, and have gold
                        if (barracksUnit != null && barracksUnit.IsBuilt
                                 && barracksUnit.CurrentAction == UnitAction.IDLE
                                 && Gold >= Constants.COST[UnitType.ARCHER])
                        {
                            Train(barracksUnit, UnitType.ARCHER);
                        }
                    }
                    break;
                case 2:
                    WinningState();
                    break;
                case 3:
                    if (Gold >= Constants.COST[UnitType.BARRACKS])
                    {
                        BuildBuilding(UnitType.BARRACKS);
                    }
                    break;
                case 4:
                    DefendingState();
                    break;
            }
            /*
            int ArmySoldiers = 35 + enemySoldiers.Count;
            int ArmyArchers = 25 + enemyArchers.Count;
            // For each barracks, determine if it should train a soldier or an archer
            foreach (int barracksNbr in myBarracks)
            {
                // Get the barracks
                Unit barracksUnit = GameManager.Instance.GetUnit(barracksNbr);

                // If this barracks still exists, is idle, we need soldiers, and have gold
                if (barracksUnit != null && barracksUnit.IsBuilt
                    && barracksUnit.CurrentAction == UnitAction.IDLE
                    && Gold >= Constants.COST[UnitType.SOLDIER]
                    && mySoldiers.Count < ArmySoldiers)
                {
                    Train(barracksUnit, UnitType.SOLDIER);
                }
                // If this barracks still exists, is idle, we need archers, and have gold
                if (barracksUnit != null && barracksUnit.IsBuilt
                         && barracksUnit.CurrentAction == UnitAction.IDLE
                         && Gold >= Constants.COST[UnitType.ARCHER]
                         && myArchers.Count < ArmyArchers)
                {
                    Train(barracksUnit, UnitType.ARCHER);
                }

            }

            if (mySoldiers.Count >= ArmySoldiers && myArchers.Count >= ArmyArchers)
            {
                WinningState();
            }
            else if(mySoldiers.Count < enemySoldiers.Count || myArchers.Count < enemyArchers.Count)
            {
                DefendingState();
            }
            */
        }

        /// <summary>
        /// Destroy the enemy's troops and buildings to achieve victory.
        /// </summary>
        private void WinningState()
        {
            Debug.Log("<color=green>Entering Winning State</color>");

            
            AttackEnemy(mySoldiers);
            AttackEnemy(myArchers);
            int state = 0;
            float stateVal = 0;
            float worker = WorkerBuildHeuristic();
            float refinery= RefineryBuildHeuristic();
            float barracks = BarracksBuildHeuristic();
            float defend = DefendHeuristic();
            float bases = BaseBuildHeuristic();

            if(stateVal< worker)
            {
                stateVal = worker;
            }
            if(stateVal< refinery) 
            {
                stateVal = refinery;
                state = 1;
            }
            if(stateVal< barracks)
            {
                stateVal = barracks;
                state = 2;
            }
            if(stateVal< defend)
            {
                stateVal = defend;
                state = 3;
            }
            if(stateVal< bases)
            {
                stateVal = bases;
                state = 4;
            }
            switch (state) 
            {
                case 0:
                    //Train workers from bases until sufficient workers are built
                    foreach (int baseNbr in myBases)
                    {
                        // Get the base unit
                        Unit baseUnit = GameManager.Instance.GetUnit(baseNbr);

                        // If the base exists, is idle, we need a worker, and we have gold
                        if (baseUnit != null && baseUnit.IsBuilt
                                                && baseUnit.CurrentAction == UnitAction.IDLE
                                                && Gold >= Constants.COST[UnitType.WORKER]
                                                && myWorkers.Count < MAX_NBR_WORKERS)
                        {
                            Train(baseUnit, UnitType.WORKER);
                        }
                    }
                    break;
                case 1:
                    if (Gold >= Constants.COST[UnitType.REFINERY])
                    {
                        BuildBuilding(UnitType.REFINERY);
                    }
                    break;
                case 2:
                    if (Gold >= Constants.COST[UnitType.BARRACKS])
                    {
                        BuildBuilding(UnitType.BARRACKS);
                    }
                    break;
                case 3:
                    DefendingState();
                    break;
                case 4:
                    if (myBases.Count == 0)
                    {
                        mainBaseNbr = -1;

                        BuildBuilding(UnitType.BASE);
                    }
                    break;
            }
            /*
            foreach (int baseNbr in myBases)
            {
                // Get the base unit
                Unit baseUnit = GameManager.Instance.GetUnit(baseNbr);

                // If the base exists, is idle, we need a worker, and we have gold
                if (baseUnit != null && baseUnit.IsBuilt
                                     && baseUnit.CurrentAction == UnitAction.IDLE
                                     && Gold >= Constants.COST[UnitType.WORKER]
                                     && myWorkers.Count < MAX_NBR_WORKERS
                                     && (mySoldiers.Count + myArchers.Count >= enemySoldiers.Count + enemyArchers.Count))
                {
                    Train(baseUnit, UnitType.WORKER);
                }
            }
            //if in rome, build a refinery (when its convenient)
            if (myWorkers.Count >= MAX_NBR_WORKERS  && Gold >= Constants.COST[UnitType.REFINERY] && (myBarracks.Count > myRefineries.Count || myRefineries.Count == 0) && myBarracks.Count > 0 && (mySoldiers.Count >= enemySoldiers.Count || myArchers.Count >= enemyArchers.Count))
            {
                BuildBuilding(UnitType.REFINERY);
            }
            
            if (mySoldiers.Count + myArchers.Count  < enemySoldiers.Count  + enemyArchers.Count)
            {
                DefendingState();
            }
            */
        }

        /// <summary>
        /// Defend the base and workers when under attack.
        /// </summary>
        private void DefendingState()
        {
            Debug.Log("<color=green>Entering Defending State</color>");
            int state = 0;
            float stateVal = 0;
            float soldier = SoldierBuildHeuristic();
            float archer = ArcherBuildHeuristic();
            float barracks = BarracksBuildHeuristic();
            float refinery = RefineryBuildHeuristic();
            float attack = AttackHeuristic();
            float recover = RecoverHeuristic();
            float win = WinHeuristic();

            if(stateVal < soldier) 
            {
                stateVal = soldier;
            }
            if(stateVal < archer) 
            {
                stateVal = archer;
                state = 1;
            }
            if(stateVal < barracks)
            {
                stateVal = barracks;
                state = 2;
            }
            if (stateVal < refinery)
            {
                stateVal = refinery;
                state = 3;
            }
            if (stateVal < attack) 
            {
                stateVal = attack;
                state = 4;
            }
            if (stateVal < recover) 
            {
                stateVal = recover;
                state = 5;
            }
            if (stateVal < win) 
            {
                stateVal = win;
                state = 6;
            }

            switch(state) 
            {
                case 0:
                    foreach (int barracksNbr in myBarracks)
                    {
                        // Get the barracks
                        Unit barracksUnit = GameManager.Instance.GetUnit(barracksNbr);

                        // If this barracks still exists, is idle, we need soldiers, and have gold
                        if (barracksUnit != null && barracksUnit.IsBuilt
                            && barracksUnit.CurrentAction == UnitAction.IDLE
                            && Gold >= Constants.COST[UnitType.SOLDIER])
                        {
                            Train(barracksUnit, UnitType.SOLDIER);
                        }
                    }
                    break;
                case 1:
                    foreach (int barracksNbr in myBarracks)
                    {
                        // Get the barracks
                        Unit barracksUnit = GameManager.Instance.GetUnit(barracksNbr);

                        // If this barracks still exists, is idle, we need soldiers, and have gold
                        if (barracksUnit != null && barracksUnit.IsBuilt
                            && barracksUnit.CurrentAction == UnitAction.IDLE
                            && Gold >= Constants.COST[UnitType.SOLDIER])
                        {
                            Train(barracksUnit, UnitType.SOLDIER);
                        }
                    }
                    break;
                case 2:
                    if (Gold >= Constants.COST[UnitType.BARRACKS])
                    {
                        BuildBuilding(UnitType.BARRACKS);
                    }
                    break;
                case 3:
                    if (Gold >= Constants.COST[UnitType.REFINERY])
                    {
                        BuildBuilding(UnitType.REFINERY);
                    }
                    break;
                case 4:
                    AttackEnemy(mySoldiers);
                    AttackEnemy(myArchers);
                    break;
                case 5:
                    RecoveringState();
                    break;
                case 6:
                    WinningState();
                    break;
            }
            /*
            int initArmySoldiers = (enemySoldiers.Count * 2) + 7;
            int initArmyArchers = enemyArchers.Count + 3;

            // For each barracks, determine if it should train a soldier or an archer
            foreach (int barracksNbr in myBarracks)
            {
                // Get the barracks
                Unit barracksUnit = GameManager.Instance.GetUnit(barracksNbr);

                // If this barracks still exists, is idle, we need soldiers, and have gold
                if (barracksUnit != null && barracksUnit.IsBuilt
                    && barracksUnit.CurrentAction == UnitAction.IDLE
                    && Gold >= Constants.COST[UnitType.SOLDIER]
                    && mySoldiers.Count < initArmySoldiers)
                {
                    Train(barracksUnit, UnitType.SOLDIER);
                    

                }
                // If this barracks still exists, is idle, we need archers, and have gold
                if (barracksUnit != null && barracksUnit.IsBuilt
                         && barracksUnit.CurrentAction == UnitAction.IDLE
                         && Gold >= Constants.COST[UnitType.ARCHER]
                         && myArchers.Count < initArmyArchers)
                {
                    Train(barracksUnit, UnitType.ARCHER);
                    
                }

            }
            if (Gold >= Constants.COST[UnitType.BARRACKS] && enemyBarracks.Count > myBarracks.Count)
            {
                BuildBuilding(UnitType.BARRACKS);
            }
            if (Gold >= Constants.COST[UnitType.REFINERY] && myRefineries.Count < myBarracks.Count)
            {
                BuildBuilding(UnitType.REFINERY);
            }
            //if we outnumber the enemy fight them
            if (mySoldiers.Count + myArchers.Count >= enemySoldiers.Count + enemyArchers.Count && mySoldiers.Count > 0)
            {
                AttackEnemy(mySoldiers);
                AttackEnemy(myArchers);
                if (myWorkers.Count < MAX_NBR_WORKERS && (myBarracks.Count == 0 || myBases.Count == 0))
                {
                    RecoveringState();
                }
                else
                {
                    WinningState();
                }

            }

            */
        }

        /// <summary>
        /// Recover from an attack by rebuilding and regrouping
        /// </summary>
        private void RecoveringState()
        {
            Debug.Log("<color=green>Entering Recovering State</color>");
            int state = 0;
            float stateVal = 0;
            float bases = BaseBuildHeuristic();
            float barracks = BarracksBuildHeuristic();
            float refinery = RefineryBuildHeuristic();
            float workers = WorkerBuildHeuristic();
            float defense = DefendHeuristic();
            float buildArmy = BuildArmyHeuristic();

            if(stateVal < bases)
            {
                stateVal = bases;
            }
            if(stateVal < barracks) 
            {
                stateVal = barracks;
                state = 1;
            }
            if(stateVal < refinery)
            {
                stateVal = refinery;
                state = 2;
            }
            if(stateVal < workers)
            {
                stateVal = workers;
                state = 3;

            }
            if (stateVal < defense)
            {
                stateVal = defense;
                state = 4;
            }
            if(stateVal < buildArmy)
            {
                stateVal = buildArmy;
                state = 5;
            }

            switch (state) 
            {
                case 0:
                    if (myBases.Count == 0)
                    {
                        mainBaseNbr = -1;

                        BuildBuilding(UnitType.BASE);
                    }
                    break;
                case 1:
                    if (Gold >= Constants.COST[UnitType.BARRACKS])
                    {
                        BuildBuilding(UnitType.BARRACKS);
                    }
                    break;
                case 2:
                    if (Gold >= Constants.COST[UnitType.REFINERY])
                    {
                        BuildBuilding(UnitType.REFINERY);
                    }
                    break;
                case 3:
                    foreach (int baseNbr in myBases)
                    {
                        // Get the base unit
                        Unit baseUnit = GameManager.Instance.GetUnit(baseNbr);

                        // If the base exists, is idle, we need a worker, and we have gold
                        if (baseUnit != null && baseUnit.IsBuilt
                                                && baseUnit.CurrentAction == UnitAction.IDLE
                                                && Gold >= Constants.COST[UnitType.WORKER]
                                                && myWorkers.Count < MAX_NBR_WORKERS)
                        {
                            Train(baseUnit, UnitType.WORKER);
                        }
                    }
                    break;
                case 4:
                    DefendingState();
                    break;
                case 5:
                    BuildArmyState();
                    break;
            }
            /*
            if (myBases.Count == 0)
            {
                mainBaseNbr = -1;

                BuildBuilding(UnitType.BASE);
            }
            //Something is coming build a barracks
            if (Gold >= Constants.COST[UnitType.BARRACKS] && enemyBarracks.Count + 1 > myBarracks.Count)
            {
                BuildBuilding(UnitType.BARRACKS);
            }
            //If enemy has a early army, panic and defend
            if (mySoldiers.Count < enemySoldiers.Count || myArchers.Count < enemyArchers.Count && myBarracks.Count > 0)
            {
                DefendingState();
            }
            //IF number of workers<MAX_NBR_WORKERS
            //Train workers from bases until sufficient workers are built
            foreach (int baseNbr in myBases)
            {
                // Get the base unit
                Unit baseUnit = GameManager.Instance.GetUnit(baseNbr);

                // If the base exists, is idle, we need a worker, and we have gold
                if (baseUnit != null && baseUnit.IsBuilt
                                     && baseUnit.CurrentAction == UnitAction.IDLE
                                     && Gold >= Constants.COST[UnitType.WORKER]
                                     && myWorkers.Count < MAX_NBR_WORKERS)
                {
                    Train(baseUnit, UnitType.WORKER);
                }
            }
            //FOR each worker
            //IF worker is idle
            //Assign worker to gather gold or build a building
            foreach (int worker in myWorkers)
            {
                // Grab the unit we need for this function
                Unit unit = GameManager.Instance.GetUnit(worker);

                // Make sure this unit actually exists and is idle
                if (unit != null && unit.CurrentAction == UnitAction.IDLE && mainBaseNbr >= 0 && mainMineNbr >= 0)
                {
                    // Grab the mine
                    Unit mineUnit = GameManager.Instance.GetUnit(mainMineNbr);
                    Unit baseUnit = GameManager.Instance.GetUnit(mainBaseNbr);
                    if (mineUnit != null && baseUnit != null && mineUnit.Health > 0)
                    {
                        Gather(unit, mineUnit, baseUnit);
                    }
                }
            }

            //if in rome, build a refinery (when its convenient)
            //if in rome, build a refinery (when its convenient)
            if (myWorkers.Count > MAX_NBR_WORKERS / 2 && Gold >= Constants.COST[UnitType.REFINERY] && (myBarracks.Count > myRefineries.Count || myRefineries.Count == 0) && myBarracks.Count > 0 && (mySoldiers.Count >= enemySoldiers.Count || myArchers.Count >= enemyArchers.Count))
            {
                BuildBuilding(UnitType.REFINERY);
            }

            //IF base - building requirements are satisfied
            //Transition to Army Building state
            if (myBases.Count > 0 && myBarracks.Count > 0
                                  && myRefineries.Count > 0
                                  && myWorkers.Count >= MAX_NBR_WORKERS)
            {
                BuildArmyState();
            }
            */
        }
        #endregion
        #endregion
    }
}