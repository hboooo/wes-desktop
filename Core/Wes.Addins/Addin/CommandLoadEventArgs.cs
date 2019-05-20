using System;
using Wes.Addins.ICommand;

namespace Wes.Addins.Addin
{
    public class CommandLoadEventArgs : EventArgs
    {
        public ICommandMetaData CommandMetaData { get; set; }

        public int Total { get; set; }

        public int Complated { get; set; }

        public string CommandName
        {
            get
            {
                if (CommandMetaData != null)
                    return CommandMetaData.CommandName;
                return null;
            }
        }

        public int CommandIndex
        {
            get
            {
                if (CommandMetaData != null)
                    return CommandMetaData.CommandIndex;
                return 0;
            }
        }
    }
}
