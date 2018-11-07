using LiveSplit.Model;
using System;
using System.Reflection;

namespace LiveSplit.UI.Components
{
    public class MemoryReaderComponentFactory : IComponentFactory
    {
        public string ComponentName => "Memory Reader";

        public string Description => "Reads and displays a value from game memory.";

        public ComponentCategory Category => ComponentCategory.Other;

        public IComponent Create(LiveSplitState state) => new MemoryReaderComponent(state);

        public string UpdateName => ComponentName;

        public string XMLURL => "http://livesplit.org/update/Components/update.LiveSplit.MemReader.xml";

        public string UpdateURL => "http://livesplit.org/update/";

        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;
    }
}
