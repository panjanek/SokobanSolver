using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanJanek.SokobanSolver.Engine
{
    public interface IGamePosition
    {
        IGamePosition Parent { get; set; }

        int Heuristics();

        int DistanceTo(IGamePosition other);

        string GetUniqueId();

        List<IGamePosition> GetSuccessors();
    }
}
