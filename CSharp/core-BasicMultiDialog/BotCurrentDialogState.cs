using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BasicMultiDialogBot
{
    [Serializable]
    public class BotCurrentDialogState
    {
        public int CurState { get; set; }
        public BotCurrentDialogState(int curState)
        {
            CurState = curState;
        }
    }
}