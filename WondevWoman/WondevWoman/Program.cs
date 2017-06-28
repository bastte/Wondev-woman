namespace WondevWoman
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    class Player
    {
        class Position
        {
            public int X;
            public int Y;

            public Position(int x, int y)
            {
                X = x;
                Y = y;
            }

            public Position GetNextPosition(Direction moveDirection)
            {
                switch (moveDirection)
                {
                    case Direction.N:
                        return new Position(X, Y - 1);
                    case Direction.NE:
                        return new Position(X + 1, Y - 1);
                    case Direction.E:
                        return new Position(X + 1, Y);
                    case Direction.SE:
                        return new Position(X + 1, Y + 1);
                    case Direction.S:
                        return new Position(X, Y + 1);
                    case Direction.SW:
                        return new Position(X - 1, Y + 1);
                    case Direction.W:
                        return new Position(X - 1, Y);
                    case Direction.NW:
                        return new Position(X - 1, Y - 1);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public int GetDistance(Position other)
            {
                return (X - other.X) ^ 2 + (Y - other.Y) ^ 2;
            }

            public override bool Equals(object obj)
            {
                Position position = obj as Position;
                if (position == null) return false;

                return X == position.X && Y == position.Y;
            }

            public override int GetHashCode()
            {
                return X.GetHashCode() ^ Y.GetHashCode();
            }

            public override string ToString()
            {
                return $"({X},{Y})";
            }
        }

        enum Direction
        {
            S,
            SE,
            E,
            NE,
            N,
            NW,
            W,
            SW
        }

        class GameAction
        {
            public const string MoveAndBuild = "MOVE&BUILD";

            public string ActionType;
            public int UnitIndex;
            public Direction MoveDirection;
            public Direction BuildDirection;

            public GameAction(string input)
            {
                var inputs = input.Split(' ');
                ActionType = inputs[0];
                UnitIndex = int.Parse(inputs[1]);
                MoveDirection = (Direction)Enum.Parse(typeof(Direction), inputs[2], true);
                BuildDirection = (Direction)Enum.Parse(typeof(Direction), inputs[3], true);
            }

            public override string ToString()
            {
                return $"{ActionType} {UnitIndex} {MoveDirection.ToString("G")} {BuildDirection.ToString("G")}";
            }
        }

        class ScoringContext
        {
            public int[,] Map;
            public int MapSize;

            public Position CurrentPosition;
            public Position NextPosition;
            public Position NextBuildPosition;

            public int CurrentPositionHeight;
            public int NextPositionHeight;
            public int NextBuildHeight;

            public ScoringContext(GameAction action, int[,] map, Position[] units)
            {
                Map = map;
                MapSize = map.GetLength(0);

                CurrentPosition = units[action.UnitIndex];
                NextPosition = CurrentPosition.GetNextPosition(action.MoveDirection);
                NextBuildPosition = NextPosition.GetNextPosition(action.BuildDirection);

                CurrentPositionHeight = Map[CurrentPosition.X, CurrentPosition.Y];
                NextPositionHeight = Map[NextPosition.X, NextPosition.Y];
                NextBuildHeight = Map[NextBuildPosition.X, NextBuildPosition.Y] + 1;
            }
        }

        static void Main(string[] args)
        {
            int size = int.Parse(Console.ReadLine());
            int unitsPerPlayer = int.Parse(Console.ReadLine());

            // game loop
            while (true)
            {
                // Parse input
                var map = ParseMap(size);
                Position[] myPositions = ParseUnits(unitsPerPlayer);
                Position[] ennemyPositions = ParseUnits(unitsPerPlayer);
                GameAction[] legalActions = ParseLegalActions();

                if (legalActions == null || legalActions.Length == 0)
                {
                    Console.WriteLine("Kill all AIs!!");
                    continue;
                }

                GameAction bestAction = legalActions[0];
                int bestScore = 0;
                for (int i = 0; i < legalActions.Length; ++i)
                {
                    int candidateScore = ComputeActionScore(legalActions[i], map, myPositions);
                    if (candidateScore > bestScore)
                    {
                        bestScore = candidateScore;
                        bestAction = legalActions[i];
                    }
                }

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");
                Console.WriteLine(bestAction);
            }
        }

        private static int ComputeActionScore(
            GameAction candidate,
            int[,] map,
            Position[] units)
        {
            var scoringContext = new ScoringContext(candidate, map, units);

            // TODO: maybe add a bit of randomness to the initial score?
            int score = 0;

            #region Movement analysis
            score += ApplyGoToMaxHeightRule(scoringContext);
            score += ApplyGoHigherRule(scoringContext);
            score += ApplyCenterAffinityRule(scoringContext);
            #endregion

            #region Build analysis
            score += ApplyBuildReachableMaxHeightCellRule(scoringContext);
            score += ApplyBuildAlmostReachableMaxHeightCellRule(scoringContext);

            // Avoid building height 4 as I might block myself, unless I'm blocking the opponent?
            // TODO
            #endregion

            Console.Error.WriteLine($"Evaluating action: {candidate}");
            Console.Error.WriteLine($"Score: {score}");
            return score;
        }

        #region Scoring rules
        private static int ApplyGoToMaxHeightRule(ScoringContext context)
        {
            // If you can score a point, it's most likely a good thing to do
            if (context.NextPositionHeight == 3)
            {
                return 30;
            }

            return 0;
        }

        private static int ApplyGoHigherRule(ScoringContext context)
        {
            // It's probably always good to go higher
            if (context.NextPositionHeight > context.CurrentPositionHeight)
            {
                return 25;
            }

            return 0;
        }

        private static int ApplyCenterAffinityRule(ScoringContext context)
        {
            // Going in the direction of the center is probably good
            var center = new Position(context.MapSize / 2, context.MapSize / 2);
            int currentDistanceToCenter = center.GetDistance(context.CurrentPosition);
            int nextDistanceToCenter = center.GetDistance(context.NextPosition);

            if (nextDistanceToCenter < currentDistanceToCenter)
            {
                return 2 * (currentDistanceToCenter - nextDistanceToCenter) / context.MapSize;
            }

            return 0;
        }

        private static int ApplyBuildReachableMaxHeightCellRule(ScoringContext context)
        {
            // If I'm moving to height 2 and I can build a height 3 cell it's great for next round
            if (context.NextPositionHeight == 2 && context.NextBuildHeight == 3)
            {
                return 25;
            }

            return 0;
        }

        private static int ApplyBuildAlmostReachableMaxHeightCellRule(ScoringContext context)
        {
            int score = 0;
            List<Position> reachableNeighbours = GetReachableNeighbours(context.CurrentPosition, context.Map);
            foreach (Position neighbour in reachableNeighbours)
            {
                // If this is the neighbour I'm building on, don't forget to increment
                int neighbourHeight = context.Map[neighbour.X, neighbour.Y];
                if (neighbour.Equals(context.NextBuildPosition)) neighbourHeight++;

                if (neighbourHeight == 3)
                {
                    score += 5;
                }
                if (neighbourHeight == 2)
                {
                    score += 2;
                }
                if (neighbourHeight == 1)
                {
                    score += 1;
                }
            }

            // Penalize dead-ends
            score -= (8 - reachableNeighbours.Count) * 2;

            return score;
        }
        #endregion

        #region Helpers
        private static List<Position> GetNeighbours(Position currentPosition, int[,] map)
        {
            List<Position> neighbours = new List<Position>();
            int mapSize = map.GetLength(0);

            for (int dx = -1; dx <= 1; ++dx)
            {
                for (int dy = -1; dy <= 1; ++dy)
                {
                    if (dx == 0 && dy == 0) continue;

                    int x = currentPosition.X + dx;
                    int y = currentPosition.Y + dy;

                    if (0 <= x && x < mapSize &&
                        0 <= y && y < mapSize)
                    {
                        neighbours.Add(new Position(x, y));
                    }
                }
            }

            return neighbours;
        }

        private static List<Position> GetReachableNeighbours(Position currentPosition, int[,] map)
        {
            List<Position> neighbours = GetNeighbours(currentPosition, map);
            int inaccessibleNeighbours = neighbours.RemoveAll(p =>
                map[p.X, p.Y] < 0 ||
                3 < map[p.X, p.Y] ||
                1 + map[currentPosition.X, currentPosition.Y] < map[p.X, p.Y]);

            Console.Error.WriteLine($"Removed {inaccessibleNeighbours} inaccessible neighbours");
            return neighbours;
        }
        #endregion

        #region Input parsing
        private static int[,] ParseMap(int size)
        {
            Console.Error.WriteLine(new string('-', 10));
            Console.Error.WriteLine("Parsing game map");

            var map = new int[size, size];
            for (int y = 0; y < size; ++y)
            {
                var row = Console.ReadLine().ToCharArray();
                Console.Error.WriteLine(row);
                for (var x = 0; x < size; ++x)
                {
                    map[x, y] = row[x] == '.' ? -1 : row[x] - '0';
                }
            }

            Console.Error.WriteLine(new string('-', 10));
            return map;
        }

        private static Position[] ParseUnits(int unitsPerPlayer)
        {
            Console.Error.WriteLine(new string('-', 10));
            Console.Error.WriteLine($"Parsing player units");

            var positions = new Position[unitsPerPlayer];

            for (int i = 0; i < unitsPerPlayer; i++)
            {
                var tmp = Console.ReadLine();
                Console.Error.WriteLine(tmp);

                var inputs = tmp.Split(' ');
                int unitX = int.Parse(inputs[0]);
                int unitY = int.Parse(inputs[1]);
                positions[i] = new Position(unitX, unitY);
                Console.Error.WriteLine(positions[i]);
            }

            Console.Error.WriteLine(new string('-', 10));
            return positions;
        }

        private static GameAction[] ParseLegalActions()
        {
            Console.Error.WriteLine(new string('-', 10));
            Console.Error.WriteLine("Parsing legal actions for this round");

            int legalActionsCount = int.Parse(Console.ReadLine());
            var legalActions = new GameAction[legalActionsCount];

            for (int i = 0; i < legalActionsCount; i++)
            {
                legalActions[i] = new GameAction(Console.ReadLine());
                Console.Error.WriteLine(legalActions[i]);
            }

            Console.Error.WriteLine(new string('-', 10));
            return legalActions;
        }
        #endregion
    }
}
