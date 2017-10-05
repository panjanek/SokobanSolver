using NUnit.Framework;
using PanJanek.SokobanSolver.Sokoban;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanJanek.SokobanSolver.Tests
{
    [TestFixture]
    public class PositionTests
    {
        [Test]
        public void SokobanPosition_GetUniqueId()
        {
            SokobanPosition pos = SokobanPosition.LoadFromFile("C:\\Projects\\git\\SokobanSolver\\levels\\sokoban1.txt");
            var a = pos.GetUniqueId();
        }
    }
}
