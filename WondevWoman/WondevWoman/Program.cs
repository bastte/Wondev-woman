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
            var mapSize = map.GetLength(0);
            var currentPosition = units[candidate.UnitIndex];
            var nextPosition = currentPosition.GetNextPosition(candidate.MoveDirection);
            int currentHeight = map[currentPosition.X, currentPosition.Y];
            int nextHeight = map[nextPosition.X, nextPosition.Y];

            // TODO: maybe add a bit of randomness to the initial score?
            int score = 0;

            #region Movement analysis
            // If you can go to the highest, it's most likely the thing you'll always want to do
            if (nextHeight == 3)
            {
                score += 100;
            }

            // It's probably always good to go higher
            if (nextHeight > currentHeight)
            {
                score += 5;
            }

            // If not going higher, going in the direction of the center is probably good
            if (nextHeight <= currentHeight)
            {
                var center = new Position(map.Length / 2, map.Length / 2);
                int currentDistanceToCenter = center.GetDistance(currentPosition);
                int nextDistanceToCenter = center.GetDistance(nextPosition);
                if (nextDistanceToCenter < currentDistanceToCenter)
                {
                    score += 2 * (currentDistanceToCenter - nextDistanceToCenter) / map.Length;
                }
            }
            #endregion

            #region Build analysis
            var buildPosition = nextPosition.GetNextPosition(candidate.BuildDirection);
            var buildHeight = map[buildPosition.X, buildPosition.Y] + 1;

            // If I'm moving to height 2 and I can build a height 3 cell it's great for next round
            if (nextHeight == 2 && buildHeight == 3)
            {
                score += 25;
            }

            // If I'm building a height 2 cell next to a height 3 cell it's good too
            if (buildHeight == 2)
            {
                for (int x = -1; x <= 1; ++x)
                {
                    for (int y = -1; y <= 1; ++y)
                    {
                        if (0 <= buildPosition.X + x &&
                            buildPosition.X + x < mapSize && 
                            0 <= buildPosition.Y + y &&
                            buildPosition.Y + y < mapSize)
                        {
                            var neighbourHeight = map[buildPosition.X + x, buildPosition.Y + y];
                            if (neighbourHeight == 3)
                            {
                                score += 10;
                            }
                            if (neighbourHeight == 2)
                            {
                                score += 3;
                            }
                        }
                    }
                }
            }
            #endregion

            Console.Error.WriteLine($"Evaluating action: {candidate}");
            Console.Error.WriteLine($"Score: {score}");
            return score;
        }

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
