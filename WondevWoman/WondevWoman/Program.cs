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
                MoveDirection = (Direction) Enum.Parse(typeof(Direction), inputs[2], true);
                BuildDirection = (Direction) Enum.Parse(typeof(Direction), inputs[3], true);
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

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");
                Console.WriteLine("Kill all AIs");
            }
        }

        #region Input parsing
        private static int[,] ParseMap(int size)
        {
            var map = new int[size, size];
            for (int y = 0; y < size; ++y)
            {
                var row = Console.ReadLine().ToCharArray();
                //Console.Error.WriteLine(row);
                for (var x = 0; x < size; ++x)
                {
                    map[x, y] = row[x] == '.' ? -1 : row[x] - '0';
                }
            }
            return map;
        }

        private static Position[] ParseUnits(int unitsPerPlayer)
        {
            var positions = new Position[unitsPerPlayer];

            for (int i = 0; i < unitsPerPlayer; i++)
            {
                var tmp = Console.ReadLine();
                //Console.Error.WriteLine(tmp);
                var inputs = tmp.Split(' ');
                //Console.Error.WriteLine(inputs[0]);
                //Console.Error.WriteLine(inputs[1]);
                int unitX = int.Parse(inputs[0]);
                int unitY = int.Parse(inputs[1]);
                positions[i] = new Position(unitX, unitY);
            }

            return positions;
        }

        private static GameAction[] ParseLegalActions()
        {
            int legalActionsCount = int.Parse(Console.ReadLine());
            var legalActions = new GameAction[legalActionsCount];

            for (int i = 0; i < legalActionsCount; i++)
            {
                legalActions[i] = new GameAction(Console.ReadLine());
            }

            return legalActions;
        }
        #endregion
    }
}
