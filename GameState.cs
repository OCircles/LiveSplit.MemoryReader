using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit
{
    class GameState
    {

        Process GameProcess;
        IntPtr GameOffset;
        

        public GameState(Process game, IntPtr offset)
        {
            this.GameProcess = game;
            this.GameOffset = offset;
        }
    }
}
