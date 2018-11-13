using LiveSplit.UI;
using System.Collections.Generic;
using System.Xml;

namespace LiveSplit
{
    public class Game
    {

        public string Name;
        public List<string> AddressList;
        
        public Game(string name)
        {
            this.Name = name;
            this.AddressList = new List<string>();
        }

        public void Add(string address)
        {
            // Remove input whitespaces
            string formatted = MemReaderUtil.TrimAllWithInplaceCharArray(address);
            
            // Check for duplicate
            bool exists = false;
            foreach(string s in AddressList) if (s == formatted) exists = true;
            
            // Add if no duplicate
            if (!exists) AddressList.Add(formatted);
        }

        public void Remove(string address)
        {
            // Remove input whitespaces
            string formatted = MemReaderUtil.TrimAllWithInplaceCharArray(address);

            // Remove address entry
            string match = null;
            foreach (string s in AddressList) if (s == formatted) match = s;

            if (match != null) AddressList.Remove(match);

        }
        

        public static Game FromXml(XmlNode node)
        {
            var element = (XmlElement)node;
            var game = new Game(element["Name"].InnerText);
            foreach (XmlElement a in element.ChildNodes)
            {
                if (a.Name == "Address") game.Add(a.InnerText);
            }
            return game;
        }

        public int CreateElement(XmlDocument document, XmlElement element)
        {

            int hashCode = SettingsHelper.CreateSetting(document, element, "Name", Name);

            int count = 1;

            for (int i = 0; i < AddressList.Count; i++)
            {
                hashCode ^= SettingsHelper.CreateSetting(document, element, "Address", AddressList[i]) * count;
                count++;
            }

            return hashCode;
        }

    }
}
