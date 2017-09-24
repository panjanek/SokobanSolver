using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanJanek.SokobanSolver.Sokoban
{
    public static class SokobanUtil
    {
        public static List<SokobanPosition> GetFullPath(SokobanPosition[] path)
        {
            List<SokobanPosition> result = new List<SokobanPosition>();
            for(int i = 0; i < path.Length - 1; i++)
            {
                var start = path[i];
                var stop = path[i + 1];
                result.Add(start);

                bool[,] visited = new bool[start.Width, start.Height];
                visited[start.Player.X, start.Player.Y] = true;
                Array.Clear(visited, 0, visited.Length);
                PointXY[] stack = new PointXY[start.Width * start.Height];
                PointXY[,] track = new PointXY[start.Width, start.Height];
                track[start.Player.X, start.Player.Y].X = 0;
                track[start.Player.X, start.Player.Y].Y = 0;
                int stackTop = 0;
                stack[0] = start.Player;
                PointXY p;
                SokobanPosition found = null;
                while (stackTop >= 0)
                {
                    p = stack[stackTop];
                    stackTop--;

                    //try push left
                    if ((start.Map[p.X - 1, p.Y] == Constants.STONE || start.Map[p.X - 1, p.Y] == Constants.GOALSTONE) && (start.Map[p.X - 2, p.Y] == Constants.EMPTY || start.Map[p.X - 2, p.Y] == Constants.GOAL))
                    {
                        var next = start.Clone(p.X - 1, p.Y, Direction.Left);
                        next.Map[p.X - 1, p.Y] = start.Map[p.X - 1, p.Y] == Constants.STONE ? Constants.EMPTY : Constants.GOAL;
                        next.Map[p.X - 2, p.Y] = start.Map[p.X - 2, p.Y] == Constants.EMPTY ? Constants.STONE : Constants.GOALSTONE;
                        if (next.GetUniqueId() == stop.GetUniqueId())
                        {
                            found = next;
                            track[p.X - 1, p.Y] = p;
                            break;
                        }
                    }

                    //try push right
                    if ((start.Map[p.X + 1, p.Y] == Constants.STONE || start.Map[p.X + 1, p.Y] == Constants.GOALSTONE) && (start.Map[p.X + 2, p.Y] == Constants.EMPTY || start.Map[p.X + 2, p.Y] == Constants.GOAL))
                    {
                        var next = start.Clone(p.X + 1, p.Y, Direction.Right);
                        next.Map[p.X + 1, p.Y] = start.Map[p.X + 1, p.Y] == Constants.STONE ? Constants.EMPTY : Constants.GOAL;
                        next.Map[p.X + 2, p.Y] = start.Map[p.X + 2, p.Y] == Constants.EMPTY ? Constants.STONE : Constants.GOALSTONE;
                        if (next.GetUniqueId() == stop.GetUniqueId())
                        {
                            found = next;
                            track[p.X + 1, p.Y] = p;
                            break;
                        }
                    }

                    //try push up
                    if ((start.Map[p.X, p.Y - 1] == Constants.STONE || start.Map[p.X, p.Y - 1] == Constants.GOALSTONE) && (start.Map[p.X, p.Y - 2] == Constants.EMPTY || start.Map[p.X, p.Y - 2] == Constants.GOAL))
                    {
                        var next = start.Clone(p.X, p.Y - 1, Direction.Up);
                        next.Map[p.X, p.Y - 1] = start.Map[p.X, p.Y - 1] == Constants.STONE ? Constants.EMPTY : Constants.GOAL;
                        next.Map[p.X, p.Y - 2] = start.Map[p.X, p.Y - 2] == Constants.EMPTY ? Constants.STONE : Constants.GOALSTONE;
                        if (next.GetUniqueId() == stop.GetUniqueId())
                        {
                            found = next;
                            track[p.X, p.Y - 1] = p;
                            break;
                        }
                    }

                    //try push down
                    if ((start.Map[p.X, p.Y + 1] == Constants.STONE || start.Map[p.X, p.Y + 1] == Constants.GOALSTONE) && (start.Map[p.X, p.Y + 2] == Constants.EMPTY || start.Map[p.X, p.Y + 2] == Constants.GOAL))
                    {
                        var next = start.Clone(p.X, p.Y + 1, Direction.Down);
                        next.Map[p.X, p.Y + 1] = start.Map[p.X, p.Y + 1] == Constants.STONE ? Constants.EMPTY : Constants.GOAL;
                        next.Map[p.X, p.Y + 2] = start.Map[p.X, p.Y + 2] == Constants.EMPTY ? Constants.STONE : Constants.GOALSTONE;
                        if (next.GetUniqueId() == stop.GetUniqueId())
                        {
                            found = next;
                            track[p.X, p.Y + 1] = p;
                            break;
                        }
                    }

                    //try walk left
                    if (!visited[p.X - 1, p.Y] && (start.Map[p.X - 1, p.Y] == Constants.EMPTY || start.Map[p.X - 1, p.Y] == Constants.GOAL))
                    {
                        stackTop++;
                        stack[stackTop].X = p.X - 1;
                        stack[stackTop].Y = p.Y;
                        visited[p.X - 1, p.Y] = true;
                        track[p.X - 1, p.Y] = p;
                    }

                    //try walk right
                    if (!visited[p.X + 1, p.Y] && (start.Map[p.X + 1, p.Y] == Constants.EMPTY || start.Map[p.X + 1, p.Y] == Constants.GOAL))
                    {
                        stackTop++;
                        stack[stackTop].X = p.X + 1;
                        stack[stackTop].Y = p.Y;
                        visited[p.X + 1, p.Y] = true;
                        track[p.X + 1, p.Y] = p;
                    }

                    //try walk up
                    if (!visited[p.X, p.Y - 1] && (start.Map[p.X, p.Y - 1] == Constants.EMPTY || start.Map[p.X, p.Y - 1] == Constants.GOAL))
                    {
                        stackTop++;
                        stack[stackTop].X = p.X;
                        stack[stackTop].Y = p.Y - 1;
                        visited[p.X, p.Y - 1] = true;
                        track[p.X, p.Y - 1] = p;
                    }

                    //try walk down
                    if (!visited[p.X, p.Y + 1] && (start.Map[p.X, p.Y + 1] == Constants.EMPTY || start.Map[p.X, p.Y + 1] == Constants.GOAL))
                    {
                        stackTop++;
                        stack[stackTop].X = p.X;
                        stack[stackTop].Y = p.Y + 1;
                        visited[p.X, p.Y + 1] = true;
                        track[p.X, p.Y + 1] = p;
                    }
                }

                
                if (found != null)
                {
                    var t = stop.Player;
                    List<SokobanPosition> intermediate = new List<SokobanPosition>();
                    while (!(t.X == start.Player.X && t.Y == start.Player.Y))
                    {
                        SokobanPosition pos = new SokobanPosition();
                        pos.Height = start.Height;
                        pos.Width = start.Width;
                        pos.Map = start.Map;
                        pos.Parent = start.Parent;
                        pos.Player.X = t.X;
                        pos.Player.Y = t.Y;
                        pos.StonesCount = start.StonesCount;
                        if (!(t.X == stop.Player.X && t.Y == stop.Player.Y))
                        {
                            intermediate.Add(pos);
                        }

                        t = track[t.X, t.Y];
                    }

                    intermediate.Reverse();
                    result.AddRange(intermediate);
                }
            }

            result.Add(path[path.Length - 1]);
            return result;
        }
    }
}
