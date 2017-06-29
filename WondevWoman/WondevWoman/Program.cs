using System;
using System.Collections.Generic;
using System.Diagnostics;

class Player
{
    public static List<string> possibleDirections = new List<string> { "S", "SE", "E", "NE", "N", "NW", "W", "SW" };
    public const string PushAndBuild = "PUSH&BUILD";
    public const string MoveAndBuild = "MOVE&BUILD";
    public static Dictionary<Tuple<int, int>, int> maxHeightOfNeighbours = new Dictionary<Tuple<int, int>, int>();

    static void Main(string[] args)
    {
        var map = new Dictionary<Tuple<int, int>, int>();
        int size = int.Parse(Console.ReadLine());
        var center = (int)Math.Floor((decimal)size / 2);
        int unitsPerPlayer = int.Parse(Console.ReadLine());
        Tuple<int, int>[] myPosition = new Tuple<int, int>[unitsPerPlayer];
        Tuple<int, int>[] opponentsPosition = new Tuple<int, int>[unitsPerPlayer];
        // game loop
        while (true)
        {
            var sw = new Stopwatch();
            sw.Start();
            maxHeightOfNeighbours = new Dictionary<Tuple<int, int>, int>();
            var optimalDistToCenter = size;
            var optimalBuildDistToCenter = size;
            GameAction bestAction = null;
            map = ParseMap(size);
            ParseUnits(unitsPerPlayer, myPosition, map);
            //var preferredUnit = GetPreferredUnit(myPosition, map);
            //Console.Error.WriteLine(preferredUnit);
            ParseUnits(unitsPerPlayer, opponentsPosition, map);

            var legalActions = ParseLegalActions();
            //Console.Error.WriteLine($"Legal actions {legalActions}");
            foreach (var action in legalActions)
            {
                if (IsBetter(action, bestAction, map, myPosition, center))
                {
                    bestAction = action;
                }
            }

            sw.Stop();
            Console.Error.WriteLine($"Time ellapsed: {sw.ElapsedMilliseconds}");
            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
            Console.WriteLine(bestAction);
        }
    }

    private static int GetPreferredUnit(Tuple<int, int>[] myPosition, Dictionary<Tuple<int, int>, int> map)
    {
        var height0 = map[myPosition[0]];
        var height1 = map[myPosition[1]];
        if (height0 == 3 && height1 < 2)
        {
            return 1;
        }
        if (height1 == 3 && height0 < 2)
        {
            return 0;
        }
        return -1;
    }

    private static void ParseUnits(int unitsPerPlayer, Tuple<int, int>[] myPosition, Dictionary<Tuple<int, int>, int> map)
    {
        for (int i = 0; i < unitsPerPlayer; i++)
        {
            var tmp = Console.ReadLine();
            //Console.Error.WriteLine(tmp);
            var inputs = tmp.Split(' ');
            //Console.Error.WriteLine(inputs[0]);
            //Console.Error.WriteLine(inputs[1]);
            int unitX = int.Parse(inputs[0]);
            int unitY = int.Parse(inputs[1]);
            myPosition[i] = new Tuple<int, int>(unitX, unitY);
            //Console.Error.WriteLine("CurrHeight = " + map[myPosition]);
        }
    }

    private static Dictionary<Tuple<int, int>, int> ParseMap(int size)
    {
        var map = new Dictionary<Tuple<int, int>, int>();
        for (int i = 0; i < size; i++)
        {
            var row = Console.ReadLine().ToCharArray();
            //Console.Error.WriteLine(row);
            for (var j = 0; j < size; ++j)
            {
                var index = new Tuple<int, int>(j, i);
                map[index] = row[j] == '.' ? -1 : row[j] - '0';
            }
        }
        return map;
    }

    private static IList<GameAction> ParseLegalActions()
    {
        //Console.Error.WriteLine(new string('-', 10));
        //Console.Error.WriteLine("Parsing legal actions for this round");

        int legalActionsCount = int.Parse(Console.ReadLine());
        var legalActions = new List<GameAction>();

        for (int i = 0; i < legalActionsCount; i++)
        {
            legalActions.Add(new GameAction(Console.ReadLine()));
            //Console.Error.WriteLine(legalActions[i]);
        }

        //Console.Error.WriteLine(new string('-', 10));
        return legalActions;
    }

    public static Tuple<int, int>[] GetDirections(Tuple<int, int> currPos, string dirMove, string dirBuild)
    {
        var res = new Tuple<int, int>[2];
        var posMove = GetPosition(currPos, dirMove);
        res[0] = posMove;
        var posBuild = GetPosition(posMove, dirBuild);
        res[1] = posBuild;
        return res;
    }

    public static Tuple<int, int> GetPosition(Tuple<int, int> currPos, string direction)
    {
        var newPos = new Tuple<int, int>(0, 0);
        switch (direction)
        {
            case "S":
                newPos = new Tuple<int, int>(currPos.Item1, currPos.Item2 + 1);
                break;
            case "SE":
                newPos = new Tuple<int, int>(currPos.Item1 + 1, currPos.Item2 + 1);
                break;
            case "E":
                newPos = new Tuple<int, int>(currPos.Item1 + 1, currPos.Item2);
                break;
            case "NE":
                newPos = new Tuple<int, int>(currPos.Item1 + 1, currPos.Item2 - 1);
                break;
            case "N":
                newPos = new Tuple<int, int>(currPos.Item1, currPos.Item2 - 1);
                break;
            case "NW":
                newPos = new Tuple<int, int>(currPos.Item1 - 1, currPos.Item2 - 1);
                break;
            case "W":
                newPos = new Tuple<int, int>(currPos.Item1 - 1, currPos.Item2);
                break;
            case "SW":
                newPos = new Tuple<int, int>(currPos.Item1 - 1, currPos.Item2 + 1);
                break;
            default:
                throw new Exception("Unexpected possible move");
        }
        return newPos;
    }

    public static int GetNearMaxHeight(Tuple<int, int> currPos, IDictionary<Tuple<int, int>, int> map)
    {
        var maxHeight = -1;
        foreach (var dir in possibleDirections)
        {
            var pos = GetPosition(currPos, dir);
            if (map.ContainsKey(pos) && map[pos] < 4)
            {
                maxHeight = Math.Max(maxHeight, map[pos]);
            }
        }
        //Console.Error.WriteLine($"nearMaxHeight: {maxHeight}");
        return maxHeight;
    }

    public static int GetDistFromCenter(Tuple<int, int> pos, int center)
    {
        var horizontalDist = Math.Abs(pos.Item1 - center);
        var verticalDist = Math.Abs(pos.Item2 - center);
        return Math.Max(horizontalDist, verticalDist) * center + Math.Min(horizontalDist, verticalDist);
    }

    public static bool IsBetter(GameAction candidateMove, GameAction bestMove, IDictionary<Tuple<int, int>, int> map, Tuple<int, int>[] myPosition, int center)
    {
        if (bestMove == null)
        {
            return true;
        }
        //Console.Error.WriteLine($"Best move {bestMove} Candidate move {candidateMove}");
        int unitNumber = candidateMove.UnitIndex;
        var candidateMoveInfo = new MoveInfo(candidateMove, map, myPosition, center);
        var bestMoveInfo = new MoveInfo(bestMove, map, myPosition, center);
        if (bestMove.ActionType == PushAndBuild && candidateMove.ActionType != PushAndBuild)
        {
            return false;
        }

        if (candidateMove.ActionType == PushAndBuild)
        {
            //Console.Error.WriteLine($"push from {directions[0]} height {currHeight} to {directions[1]} height {nextHeight}");
            //We push if this makes one of our opponents go down. That way, it will take them 2 turns to reach the same position
            if (candidateMoveInfo.PushHeight <= candidateMoveInfo.MoveHeight)
            {
                if (bestMove.ActionType == MoveAndBuild)
                {
                    return true;
                }
                if (candidateMoveInfo.PushHeight < bestMoveInfo.PushHeight)
                {
                    return true;
                }
                if (candidateMoveInfo.PushHeight == bestMoveInfo.PushHeight && candidateMoveInfo.BuildDistanceToCenter > bestMoveInfo.BuildDistanceToCenter)
                {
                    return true;
                }
            }
            return false;
        }

        var nextMove = candidateMoveInfo.MovePosition;
        if (!maxHeightOfNeighbours.ContainsKey(nextMove))
        {
            var nearHeight = GetNearMaxHeight(nextMove, map);
            maxHeightOfNeighbours[nextMove] = nearHeight;
        }

        // If we have a group of 2 squares of height 3,
        // we want to build to the lowest possible to maximize the times we travel to them
        bool shouldBuildMax = candidateMoveInfo.MoveHeight != 3 || maxHeightOfNeighbours[nextMove] != 3;

        //We always try to go to the top
        // 
        if (candidateMoveInfo.MoveHeight < bestMoveInfo.MoveHeight)
        {
            return false;
        }
        if (candidateMoveInfo.MoveHeight > bestMoveInfo.MoveHeight)
        {
            return true;
        }
        //We don't want to build on a 3
        if (bestMoveInfo.BuildHeight == 4 && candidateMoveInfo.BuildHeight != 4)
        {
            return true;
        }

        if (bestMoveInfo.GoesDown && !candidateMoveInfo.GoesDown)
        {
            return true;
        }

        if (candidateMoveInfo.BuildHeight == 4)
        {
            return false;
        }
        if (shouldBuildMax)
        {
            //Console.Error.WriteLine($"I should build max. OptimalDistToCenter: {optimalDistToCenter}. currentDistToCenter:{currentDistToCenter}");
            //We try to build to a place we will be able to reach next time and that is higher than us
            if (candidateMoveInfo.MoveHeight < bestMoveInfo.MoveHeight)
            {
                return false;
            }
            if (candidateMoveInfo.BuildHeight > candidateMoveInfo.MoveHeight + 1)
            {
                return false;
            }
            if (bestMoveInfo.MoveDistanceToCenter < candidateMoveInfo.MoveDistanceToCenter)
            {
                return false;
            }
            if (bestMoveInfo.BuildDistanceToCenter < candidateMoveInfo.BuildDistanceToCenter)
            {
                return false;
            }
            return true;
        }
        else
        {
            //We try to build as low as possible
            if (bestMoveInfo.BuildHeight > candidateMoveInfo.BuildHeight)
            {
                return true;
            }
            if (bestMoveInfo.BuildHeight == candidateMoveInfo.BuildHeight && bestMoveInfo.BuildDistanceToCenter > candidateMoveInfo.BuildDistanceToCenter)
            {
                return true;
            }
            return false;
        }

        return false;
    }

}
public class GameAction
{
    public string ActionType;
    public int UnitIndex;
    public string MoveDirection;
    public string BuildDirection;

    public GameAction(string input)
    {
        var inputs = input.Split(' ');
        ActionType = inputs[0];
        UnitIndex = int.Parse(inputs[1]);
        MoveDirection = inputs[2];
        BuildDirection = inputs[3];
    }

    public override string ToString()
    {
        return $"{ActionType} {UnitIndex} {MoveDirection} {BuildDirection}";
    }
}

public class MoveInfo
{
    public int MoveHeight;

    public Tuple<int, int> MovePosition;

    public int BuildHeight;

    public int MoveDistanceToCenter;

    public int BuildDistanceToCenter;

    public int PushHeight;

    public bool GoesDown;

    public MoveInfo(GameAction move, IDictionary<Tuple<int, int>, int> map, Tuple<int, int>[] myPosition, int center)
    {
        int unitNumber = move.UnitIndex;
        string dir1 = move.MoveDirection;
        string dir2 = move.BuildDirection;
        var directions = Player.GetDirections(myPosition[unitNumber], dir1, dir2);
        MovePosition = directions[0];
        MoveHeight = map[MovePosition];
        if (move.ActionType == Player.MoveAndBuild)
        {
            BuildHeight = map[directions[1]] + 1;
        }
        else
        {
            PushHeight = map[directions[1]];
        }
        MoveDistanceToCenter = Player.GetDistFromCenter(directions[0], center);
        BuildDistanceToCenter = Player.GetDistFromCenter(directions[1], center);
        GoesDown = map[myPosition[unitNumber]] - MoveHeight > 0;
    }
}