﻿using PanJanek.SokobanSolver.Engine;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PanJanek.SokobanSolver.Sokoban
{
    public class SokobanPosition : IGamePosition
    {
        private static PointXY[] Stack = new PointXY[Constants.InteralStackMaxSize];

        private static bool[,] VisitedMap = new bool[Constants.InternalMapMaxWidth, Constants.InternalMapMaxHeight];

        public int Width;

        public int Height;

        public byte[,] Map;

        public PointXY Player;

        public int StonesCount;

        public IGamePosition Parent { get; set; }

        private byte[] Binary;

        private PointXY NormalizedPlayer;

        private string CachedUniqueId;

        private int? CachedHeuristics;

        public static SokobanPosition LoadFromFile(string path)
        {
            string[] lines = File.ReadAllLines(path).Where(l => l.Length > 0).ToArray();
            SokobanPosition position = new SokobanPosition();
            position.Width = lines.Max(l => l.Length);
            position.Height = lines.Length;
            position.Map = new byte[position.Width, position.Height];
            position.StonesCount = 0;
            position.NormalizedPlayer.X = 0;
            position.NormalizedPlayer.Y = 0;
            for(int y = 0; y<position.Height; y++)
            {
                string line = lines[y];
                for (int x = 0; x < line.Length; x++)
                {
                    switch (line[x])
                    {
                        case '#':
                            position.Map[x, y] = Constants.WALL;
                            break;
                        case '$':
                            position.Map[x, y] = Constants.STONE;
                            position.StonesCount++;
                            break;
                        case '.':
                            position.Map[x, y] = Constants.GOAL;
                            break;
                        case '*':
                            position.Map[x, y] = Constants.GOALSTONE;
                            position.StonesCount++;
                            break;
                        case '@':
                            position.Player.X = x;
                            position.Player.Y = y;
                            break;
                        default:
                            position.Map[x, y] = Constants.EMPTY;
                            break;
                    }
                }

            }

            return position;
        }

        public int Heuristics()
        {
            if (!this.CachedHeuristics.HasValue)
            {
                int stonesNotOnGoal = 0;
                for (int x = 1; x < this.Width - 1; x++)
                {
                    for (int y = 1; y < this.Height - 1; y++)
                    {
                        int d_stonesOnGoals = 0;
                        int d_stones = 0;
                        int d_walls = 0;

                        switch (this.Map[x, y])
                        {
                            case Constants.STONE:
                                stonesNotOnGoal++;
                                d_stones++;
                                break;
                            case Constants.GOALSTONE:
                                d_stonesOnGoals++;
                                 break;
                            case Constants.WALL:
                                d_walls++;
                                 break;
                        }

                        //deadlock detection
                        if (d_stonesOnGoals==1 || d_stones==1 || d_walls==1)
                        {
                            switch (this.Map[x + 1, y])
                            {
                                case Constants.STONE:
                                    d_stones++;
                                    break;
                                case Constants.GOALSTONE:
                                    d_stonesOnGoals++;
                                     break;
                                case Constants.WALL:
                                    d_walls++;
                                     break;
                            }

                            switch (this.Map[x, y + 1])
                            {
                                case Constants.STONE:
                                    d_stones++;
                                    break;
                                case Constants.GOALSTONE:
                                    d_stonesOnGoals++;
                                    break;
                                case Constants.WALL:
                                    d_walls++;
                                    break;
                            }

                            switch (this.Map[x + 1, y + 1])
                            {
                                case Constants.STONE:
                                    d_stones++;
                                    break;
                                case Constants.GOALSTONE:
                                    d_stonesOnGoals++;
                                    break;
                                case Constants.WALL:
                                    d_walls++;
                                    break;
                            }

                            if (d_stones > 0 && (d_stones + d_stonesOnGoals + d_walls == 4))
                            {
                                return int.MaxValue;
                            }
                        }





                        /*
                        if (this.Map[x, y] == Constants.STONE)
                        {
                            stonesNotOnGoal++;
                        }
                        */



                    }
                }

                this.CachedHeuristics = stonesNotOnGoal * 100;
            }

            return this.CachedHeuristics.Value;
        }

        public int DistanceTo(IGamePosition other)
        {
            SokobanPosition position = (SokobanPosition)other;
            return ((Math.Abs(this.Player.X - position.Player.X) == 1 && (this.Player.Y == position.Player.Y)) ||
                    (Math.Abs(this.Player.Y - position.Player.Y) == 1 && (this.Player.X == position.Player.X))) ? 1 : 2;
        }

        public List<IGamePosition> GetSuccessors()
        {
            List<IGamePosition> result = new List<IGamePosition>();
            Array.Clear(VisitedMap, 0, VisitedMap.Length);
            Stack[0] = this.Player;
            VisitedMap[this.Player.X, this.Player.Y] = true;
            int stackTop = 0;
            PointXY p;
            while (stackTop >=0)
            {
                p = Stack[stackTop];
                stackTop--;

                //try push left
                if ((this.Map[p.X - 1, p.Y] == Constants.STONE || this.Map[p.X - 1, p.Y] == Constants.GOALSTONE) && (this.Map[p.X - 2, p.Y] == Constants.EMPTY || this.Map[p.X - 2, p.Y] == Constants.GOAL))
                {
                    var next = this.Clone(p.X-1, p.Y, Direction.Left);
                    next.Map[p.X - 1, p.Y] = this.Map[p.X - 1, p.Y] == Constants.STONE ? Constants.EMPTY : Constants.GOAL;
                    next.Map[p.X - 2, p.Y] = this.Map[p.X - 2, p.Y] == Constants.EMPTY ? Constants.STONE : Constants.GOALSTONE;
                    result.Add(next);
                }

                //try push right
                if ((this.Map[p.X + 1, p.Y] == Constants.STONE || this.Map[p.X + 1, p.Y] == Constants.GOALSTONE) && (this.Map[p.X + 2, p.Y] == Constants.EMPTY || this.Map[p.X + 2, p.Y] == Constants.GOAL))
                {
                    var next = this.Clone(p.X + 1, p.Y, Direction.Right);
                    next.Map[p.X + 1, p.Y] = this.Map[p.X + 1, p.Y] == Constants.STONE ? Constants.EMPTY : Constants.GOAL;
                    next.Map[p.X + 2, p.Y] = this.Map[p.X + 2, p.Y] == Constants.EMPTY ? Constants.STONE : Constants.GOALSTONE;
                    result.Add(next);
                }

                //try push up
                if ((this.Map[p.X, p.Y - 1] == Constants.STONE || this.Map[p.X, p.Y - 1] == Constants.GOALSTONE) && (this.Map[p.X, p.Y - 2] == Constants.EMPTY || this.Map[p.X, p.Y - 2] == Constants.GOAL))
                {
                    var next = this.Clone(p.X, p.Y - 1, Direction.Up);
                    next.Map[p.X, p.Y - 1] = this.Map[p.X, p.Y - 1] == Constants.STONE ? Constants.EMPTY : Constants.GOAL;
                    next.Map[p.X, p.Y - 2] = this.Map[p.X, p.Y - 2] == Constants.EMPTY ? Constants.STONE : Constants.GOALSTONE;
                    result.Add(next);
                }

                //try push down
                if ((this.Map[p.X, p.Y + 1] == Constants.STONE || this.Map[p.X, p.Y + 1] == Constants.GOALSTONE) && (this.Map[p.X, p.Y + 2] == Constants.EMPTY || this.Map[p.X, p.Y + 2] == Constants.GOAL))
                {
                    var next = this.Clone(p.X, p.Y + 1, Direction.Down);
                    next.Map[p.X, p.Y + 1] = this.Map[p.X, p.Y + 1] == Constants.STONE ? Constants.EMPTY : Constants.GOAL;
                    next.Map[p.X, p.Y + 2] = this.Map[p.X, p.Y + 2] == Constants.EMPTY ? Constants.STONE : Constants.GOALSTONE;
                    result.Add(next);
                }

                //try walk left
                if (!VisitedMap[p.X - 1, p.Y] && (this.Map[p.X - 1, p.Y] == Constants.EMPTY || this.Map[p.X - 1, p.Y] == Constants.GOAL))
                {
                    stackTop++;
                    Stack[stackTop].X = p.X - 1;
                    Stack[stackTop].Y = p.Y;
                    VisitedMap[p.X - 1, p.Y] = true;
                }

                //try walk right
                if (!VisitedMap[p.X + 1, p.Y] && (this.Map[p.X + 1, p.Y] == Constants.EMPTY || this.Map[p.X + 1, p.Y] == Constants.GOAL))
                {
                    stackTop++;
                    Stack[stackTop].X = p.X + 1;
                    Stack[stackTop].Y = p.Y;
                    VisitedMap[p.X + 1, p.Y] = true;
                }

                //try walk up
                if (!VisitedMap[p.X, p.Y - 1] && (this.Map[p.X, p.Y - 1] == Constants.EMPTY || this.Map[p.X, p.Y - 1] == Constants.GOAL))
                {
                    stackTop++;
                    Stack[stackTop].X = p.X;
                    Stack[stackTop].Y = p.Y - 1;
                    VisitedMap[p.X, p.Y - 1] = true;
                }

                //try walk down
                if (!VisitedMap[p.X, p.Y + 1] && (this.Map[p.X, p.Y + 1] == Constants.EMPTY || this.Map[p.X, p.Y + 1] == Constants.GOAL))
                {
                    stackTop++;
                    Stack[stackTop].X = p.X;
                    Stack[stackTop].Y = p.Y + 1;
                    VisitedMap[p.X, p.Y + 1] = true;
                }
            }

            return result;
        }

        public string GetUniqueId()
        {
            bool changed = false;
            if (this.Binary == null)
            {
                //this.Binary = new byte[2 + this.Width * this.Height];
                this.Binary = new byte[2 + this.Width * this.Height / 8 + 1];
                int i = 0;
                for (int x = 0; x < this.Width; x++)
                {
                    for (int y = 0; y < this.Height; y++)
                    {
                        //this.Binary[2 + y * this.Width + x] = (byte)((this.Map[x, y] == Constants.STONE || this.Map[x, y] == Constants.GOALSTONE) ? 1 : 0);
                        this.SetBinaryBit(x, y, (byte)((this.Map[x, y] == Constants.STONE || this.Map[x, y] == Constants.GOALSTONE) ? 1 : 0));
                    }
                }

                changed = true;
            }

            if (this.NormalizedPlayer.X == 0 || this.NormalizedPlayer.Y == 0)
            {
                Array.Clear(VisitedMap, 0, VisitedMap.Length);
                Stack[0] = this.Player;
                VisitedMap[this.Player.X, this.Player.Y] = true;
                int stackTop = 0;
                PointXY p;
                this.NormalizedPlayer.X = this.Width;
                this.NormalizedPlayer.Y = this.Height;
                while (stackTop >= 0)
                {
                    p = Stack[stackTop];
                    stackTop--;
                    if (p.Y < this.NormalizedPlayer.Y)
                    {
                        this.NormalizedPlayer = p;
                    }

                    if (p.Y == this.NormalizedPlayer.Y && p.X < this.NormalizedPlayer.X)
                    {
                        this.NormalizedPlayer = p;
                    }

                    //try walk left
                    if (!VisitedMap[p.X - 1, p.Y] && (this.Map[p.X - 1, p.Y] == Constants.EMPTY || this.Map[p.X - 1, p.Y] == Constants.GOAL))
                    {
                        stackTop++;
                        Stack[stackTop].X = p.X - 1;
                        Stack[stackTop].Y = p.Y;
                        VisitedMap[p.X - 1, p.Y] = true;
                    }

                    //try walk right
                    if (!VisitedMap[p.X + 1, p.Y] && (this.Map[p.X + 1, p.Y] == Constants.EMPTY || this.Map[p.X + 1, p.Y] == Constants.GOAL))
                    {
                        stackTop++;
                        Stack[stackTop].X = p.X + 1;
                        Stack[stackTop].Y = p.Y;
                        VisitedMap[p.X + 1, p.Y] = true;
                    }

                    //try walk up
                    if (!VisitedMap[p.X, p.Y - 1] && (this.Map[p.X, p.Y - 1] == Constants.EMPTY || this.Map[p.X, p.Y - 1] == Constants.GOAL))
                    {
                        stackTop++;
                        Stack[stackTop].X = p.X;
                        Stack[stackTop].Y = p.Y - 1;
                        VisitedMap[p.X, p.Y - 1] = true;
                    }

                    //try walk down
                    if (!VisitedMap[p.X, p.Y + 1] && (this.Map[p.X, p.Y + 1] == Constants.EMPTY || this.Map[p.X, p.Y + 1] == Constants.GOAL))
                    {
                        stackTop++;
                        Stack[stackTop].X = p.X;
                        Stack[stackTop].Y = p.Y + 1;
                        VisitedMap[p.X, p.Y + 1] = true;
                    }
                }

                this.Binary[0] = (byte)this.NormalizedPlayer.X;
                this.Binary[1] = (byte)this.NormalizedPlayer.Y;
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(this.CachedUniqueId) || changed)
            {
                this.CachedUniqueId = Convert.ToBase64String(this.Binary);
                //this.CachedUniqueId = Encoding.Unicode.GetString(this.Binary);
                //this.CachedUniqueId = BitConverter.ToString(this.Binary).Replace("-", string.Empty);
            }

            return CachedUniqueId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBinaryBit(int x, int y, int bit)
        {
            int i = (y * this.Width + x);
            int idx = 2 + i / 8;
            int offset = i % 8;
            byte mask = (byte)(1 << offset);
            if (bit == 1)
            {
                // set to 1
                this.Binary[idx] |= mask;
            } 
            else
            {
                this.Binary[idx] &= ((byte)~mask);
            }
        }
        
        /*
        public string GetUniqueId()
        {
            if (string.IsNullOrWhiteSpace(this.CachedUniqueId))
            {
                Array.Clear(VisitedMap, 0, VisitedMap.Length);
                Stack[0] = this.Player;
                VisitedMap[this.Player.X, this.Player.Y] = true;
                int stackTop = 0;
                PointXY p;
                PointXY normalized;
                normalized.X = this.Width;
                normalized.Y = this.Height;
                while (stackTop >= 0)
                {
                    p = Stack[stackTop];
                    stackTop--;
                    if (p.Y < normalized.Y)
                    {
                        normalized = p;
                    }

                    if (p.Y == normalized.Y && p.X < normalized.X)
                    {
                        normalized = p;
                    }

                    //try walk left
                    if (!VisitedMap[p.X - 1, p.Y] && (this.Map[p.X - 1, p.Y] == Constants.EMPTY || this.Map[p.X - 1, p.Y] == Constants.GOAL))
                    {
                        stackTop++;
                        Stack[stackTop].X = p.X - 1;
                        Stack[stackTop].Y = p.Y;
                        VisitedMap[p.X - 1, p.Y] = true;
                    }

                    //try walk right
                    if (!VisitedMap[p.X + 1, p.Y] && (this.Map[p.X + 1, p.Y] == Constants.EMPTY || this.Map[p.X + 1, p.Y] == Constants.GOAL))
                    {
                        stackTop++;
                        Stack[stackTop].X = p.X + 1;
                        Stack[stackTop].Y = p.Y;
                        VisitedMap[p.X + 1, p.Y] = true;
                    }

                    //try walk up
                    if (!VisitedMap[p.X, p.Y - 1] && (this.Map[p.X, p.Y - 1] == Constants.EMPTY || this.Map[p.X, p.Y - 1] == Constants.GOAL))
                    {
                        stackTop++;
                        Stack[stackTop].X = p.X;
                        Stack[stackTop].Y = p.Y - 1;
                        VisitedMap[p.X, p.Y - 1] = true;
                    }

                    //try walk down
                    if (!VisitedMap[p.X, p.Y + 1] && (this.Map[p.X, p.Y + 1] == Constants.EMPTY || this.Map[p.X, p.Y + 1] == Constants.GOAL))
                    {
                        stackTop++;
                        Stack[stackTop].X = p.X;
                        Stack[stackTop].Y = p.Y + 1;
                        VisitedMap[p.X, p.Y + 1] = true;
                    }
                }

                byte[] packed = new byte[2 + StonesCount * 2];
                packed[0] = (byte)normalized.X;
                packed[1] = (byte)normalized.Y;
                int i = 0;
                for(int x=0; x<this.Width; x++)
                {
                    for(int y=0;y<this.Height; y++)
                    {
                        if (this.Map[x, y] == Constants.STONE || this.Map[x, y] == Constants.GOALSTONE)
                        {
                            packed[2 + i * 2] = (byte)x;
                            packed[2 + i * 2 + 1] = (byte)y;
                            i++;
                        }
                    }
                }

                this.CachedUniqueId = Convert.ToBase64String(packed);
            }

            return this.CachedUniqueId;
        }*/

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SokobanPosition Clone(int px, int py, Direction direction)
        {
            var clone = new SokobanPosition();
            clone.Parent = this;
            clone.Width = this.Width;
            clone.Height = this.Height;
            clone.Map = new byte[this.Width, this.Height];
            Array.Copy(this.Map, clone.Map, this.Map.Length);
            clone.Player.X = px;
            clone.Player.Y = py;
            clone.StonesCount = this.StonesCount;
            clone.NormalizedPlayer.X = 0;
            clone.NormalizedPlayer.Y = 0;

            if (this.Binary != null)
            {
                clone.Binary = new byte[this.Binary.Length];
                Array.Copy(this.Binary, clone.Binary, this.Binary.Length);
                //clone.Binary[2 + py * this.Width + px] = 0;
                clone.SetBinaryBit(px, py, 0);
                switch (direction)
                {
                    case Direction.Left:
                        //clone.Binary[2 + py * this.Width + px - 1] = 1;
                        clone.SetBinaryBit(px - 1, py, 1);
                        break;
                    case Direction.Right:
                        //clone.Binary[2 + py * this.Width + px + 1] = 1;
                        clone.SetBinaryBit(px + 1, py, 1);
                        break;
                    case Direction.Up:
                        //clone.Binary[2 + (py - 1) * this.Width + px] = 1;
                        clone.SetBinaryBit(px, py - 1, 1);
                        break;
                    case Direction.Down:
                        //clone.Binary[2 + (py + 1) * this.Width + px] = 1;
                        clone.SetBinaryBit(px, py + 1, 1);
                        break;
                }
            }

            return clone;
        }

        public override string ToString()
        {
            string str = string.Empty;
            for (int y = 0; y < this.Height; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    if (this.Player.X == x && this.Player.Y == y)
                    {
                        str += "@";
                    }
                    else
                    {
                        switch (this.Map[x, y])
                        {
                            case Constants.WALL:
                                str += "#";
                                break;
                            case Constants.STONE:
                                str += "$";
                                break;
                            case Constants.GOAL:
                                str += ".";
                                break;
                            case Constants.GOALSTONE:
                                str += "*";
                                break;
                            default:
                                str += " ";
                                break;
                        }
                    }
                }

                str += "\r\n";
            }

            return str;
        }
    }

    public struct PointXY
    {
        public int X;

        public int Y;
    }
}