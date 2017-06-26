using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    public static List<string> possibleDirections = new List<string> { "S", "SE", "E", "NE", "N", "NW", "W", "SW" };
    public const string PushAndBuild = "PUSH&BUILD";
    public const string MoveAndBuild = "MOVE&BUILD";
    static void Main(string[] args)
    {
        var map = new Dictionary<Tuple<int, int>, int>();
        string[] inputs;
        int size = int.Parse(Console.ReadLine());
        int unitsPerPlayer = int.Parse(Console.ReadLine());
        Tuple<int, int>[] myPosition = new Tuple<int, int>[2];
        Tuple<int, int>[] opponentsPosition = new Tuple<int, int>[2];
        string outPutLine = "";
        string pushLine = "";
        // game loop
        while (true)
        {
            var maxHeightOfNeighbours = new Dictionary<Tuple<int, int>, int>();
            var maxMoveHeight = -1;
            var maxBuildHeight = -1;
            var minBuildHeight = 4;
            var shouldPush = false;
            map = ParseMap(size);
            ParseUnits(unitsPerPlayer, myPosition, map);
            ParseUnits(unitsPerPlayer, opponentsPosition, map);

            int legalActions = int.Parse(Console.ReadLine());
            //Console.Error.WriteLine($"Legal actions {legalActions}");
            for (int i = 0; i < legalActions; i++)
            {
                var tmp = Console.ReadLine();
                //Console.Error.WriteLine(tmp);
                inputs = tmp.Split(' ');
                string atype = inputs[0];
                int unitNumber = int.Parse(inputs[1]);
                string dir1 = inputs[2];
                string dir2 = inputs[3];
                var directions = GetDirections(myPosition[unitNumber], dir1, dir2);

                if (atype == PushAndBuild)
                {
                    if (string.IsNullOrWhiteSpace(pushLine))
                    {
                        pushLine = $"{atype} {unitNumber} {dir1} {dir2}";
                    }
                    var currHeight = map[directions[0]];
                    var nextHeight = map[directions[1]];
                    //Console.Error.WriteLine($"push from {directions[0]} height {currHeight} to {directions[1]} height {nextHeight}");
                    //We push if this makes one of our opponents go down. That way, it will take them 2 turns to reach the same position
                    if (currHeight > nextHeight)
                    {
                        pushLine = $"{atype} {unitNumber} {dir1} {dir2}";
                        shouldPush = true;
                    }
                    continue;
                }
                var nextMove = directions[0];
                var nextBuild = directions[1];
                var nextMoveHeight = map[nextMove];
                //Console.Error.WriteLine("nextMove" + nextMove);
                //Console.Error.WriteLine("nextBuild" + nextBuild);
                //Console.Error.WriteLine(nextMove + " " + nextMoveHeight);
                var nextBuildHeight = map[nextBuild] + 1;
                if (!maxHeightOfNeighbours.ContainsKey(nextMove))
                {
                    var nearHeight = GetNearMaxHeight(nextMove, map);
                    maxHeightOfNeighbours[nextMove] = nearHeight;
                }

                // If we have a group of 2 squares of height 3,
                // we want to build to the lowest possible to maximize the times we travel to them
                bool shouldBuildMax = nextMoveHeight != 3 || maxHeightOfNeighbours[nextMove] != 3;

                //We always try to go to the top
                // 
                if (nextMoveHeight < maxMoveHeight)
                {
                    continue;
                }
                if (nextMoveHeight > maxMoveHeight)
                {
                    maxMoveHeight = nextMoveHeight;
                    maxBuildHeight = nextBuildHeight >= nextMoveHeight + 1 ? maxBuildHeight : nextBuildHeight;
                    minBuildHeight = nextBuildHeight;
                    outPutLine = $"{atype} {unitNumber} {dir1} {dir2}";
                    continue;
                }

                if (shouldBuildMax)
                {
                    //Console.Error.WriteLine("I should build max");
                    //We try to build to a place we will be able to reach next time and that is higher than us
                    if (nextBuildHeight < maxBuildHeight || nextBuildHeight > nextMoveHeight + 1)
                    {
                        continue;
                    }
                    else
                    {
                        maxBuildHeight = nextBuildHeight;
                    }
                }
                else
                {
                    //We try to build as low as possible
                    if (nextBuildHeight > minBuildHeight)
                    {
                        continue;
                    }
                    else
                    {
                        minBuildHeight = nextBuildHeight;
                    }
                }
                outPutLine = $"{atype} {unitNumber} {dir1} {dir2}";
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
            Console.WriteLine(shouldPush ? pushLine : (string.IsNullOrWhiteSpace(outPutLine) ? pushLine : outPutLine));
        }
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
            Console.Error.WriteLine(row);
            for (var j = 0; j < size; ++j)
            {
                var index = new Tuple<int, int>(j, i);
                map[index] = row[j] == '.' ? -1 : row[j] - '0';
            }
        }
        return map;
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

    public static int GetNearMaxHeight(Tuple<int, int> currPos, Dictionary<Tuple<int, int>, int> map)
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
        Console.Error.WriteLine($"nearMaxHeight: {maxHeight}");
        return maxHeight;
    }
}