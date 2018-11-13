using LiveSplit.UI;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace LiveSplit
{
    class XMLAddressListSaver
    {
        private static int ListToElement(XmlDocument document, XmlElement element, IList<Game> addressList)
        {

            int hashCode = 0;

            var count = 1;
            foreach (var gameData in addressList)
            {
                XmlElement gameElement = null;
                if (document != null)
                {
                    gameElement = document.CreateElement("Game");
                    element.AppendChild(gameElement);
                }
                hashCode ^= gameData.CreateElement(document, gameElement) * count;
                count++;
            }
            
            return hashCode;
        }

        public static void Save(IList<Game> addressList, Stream stream)
        {
            var document = new XmlDocument();

            XmlNode docNode = document.CreateXmlDeclaration("1.0", "UTF-8", null);
            document.AppendChild(docNode);
            var parent = document.CreateElement("AddressList");
            parent.Attributes.Append(SettingsHelper.ToAttribute(document, "version", "1.0"));
            ListToElement(document, parent, addressList);
            document.AppendChild(parent);

            document.Save(stream);
        }

    }
}
