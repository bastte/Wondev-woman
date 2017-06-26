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
    static void Main(string[] args)
    {
        var map = new Dictionary<Tuple<int, int>, int>();
        string[] inputs;
        int size = int.Parse(Console.ReadLine());
        int unitsPerPlayer = int.Parse(Console.ReadLine());
        Tuple<int, int>[] myPosition = new Tuple<int, int>[2];
        string outPutLine = "";
        string defaultOutPutLine = "";
        // game loop
        while (true)
        {
            var maxMoveHeight = -1;
            var maxBuildHeight = -1;
            var minBuildHeight = 4;
            var shouldBuildMax = false;
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
            for (int i = 0; i < unitsPerPlayer; i++)
            {
                var tmp = Console.ReadLine();
                //Console.Error.WriteLine(tmp);
                inputs = tmp.Split(' ');
                //Console.Error.WriteLine(inputs[0]);
                //Console.Error.WriteLine(inputs[1]);
                int unitX = int.Parse(inputs[0]);
                int unitY = int.Parse(inputs[1]);
                myPosition[i] = new Tuple<int, int>(unitX, unitY);
                //Console.Error.WriteLine("CurrHeight = " + map[myPosition]);
                shouldBuildMax = map[myPosition[i]] != 3 || GetNearMaxHeight(myPosition[i], map) != 3;
            }
            for (int i = 0; i < unitsPerPlayer; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int otherX = int.Parse(inputs[0]);
                int otherY = int.Parse(inputs[1]);
            }
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
                if (atype == "PUSH&BUILD")
                {
                    //TODO: push if that makes opponent go down by at least 2
                    if (string.IsNullOrWhiteSpace(defaultOutPutLine))
                    {
                        defaultOutPutLine = $"{atype} {unitNumber} {dir1} {dir2}";
                    }
                    continue;
                }
                var directions = GetDirections(myPosition[unitNumber], dir1, dir2);
                var nextMove = directions[0];
                //Console.Error.WriteLine("nextMove" + nextMove);
                var nextBuild = directions[1];
                //Console.Error.WriteLine("nextBuild" + nextBuild);
                var nextMoveHeight = map[nextMove];
                Console.Error.WriteLine(nextMove + " " + nextMoveHeight);
                var nextBuildHeight = map[nextBuild] + 1;
                //We always try to go to the top
                if (nextMoveHeight < maxMoveHeight)
                {
                    continue;
                }
                if (nextMoveHeight > maxMoveHeight)
                {
                    //TODO: build only if reachable.
                    maxMoveHeight = nextMoveHeight;
                    maxBuildHeight = nextBuildHeight;
                    minBuildHeight = nextBuildHeight;
                    outPutLine = $"{atype} {unitNumber} {dir1} {dir2}";
                    continue;
                }

                if (shouldBuildMax)
                {
                    if (nextBuildHeight < maxBuildHeight)
                    {
                        //TODO: better decide between equalities
                        continue;
                    }
                    else
                    {
                        maxBuildHeight = nextBuildHeight;
                    }
                }
                else
                {
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

                //Console.Error.WriteLine(atype + " " + index + " " + dir1 + " " + dir2);
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
            Console.WriteLine(string.IsNullOrWhiteSpace(outPutLine) ? defaultOutPutLine : outPutLine);
        }
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