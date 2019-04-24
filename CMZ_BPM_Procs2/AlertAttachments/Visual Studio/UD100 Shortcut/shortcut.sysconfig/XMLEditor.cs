using Ice.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Erp.UI.App.SalesOrderEntry;

namespace EpiFormTemplate
{
    class XMLEditor
    {
        private Transaction oTrans;
        #region Save For Later
        public class XMLData
        {
            public string Content;
        }
        private string GetKeyFields()
        {
            var keys = "Key1, Key2, Key3, Key4, Key5";
            return keys;
        }
        private string GetKeyTpes()
        {
            return "System.String";
        }
        private string GetKeyValues()
        {
            //var keys = _edvUD100.CurrentDataRow["Key1"] + "," + _edvUD100.CurrentDataRow["Key2"] + "," + _edvUD100.CurrentDataRow["Key3"] + "," + _edvUD100.CurrentDataRow["Key4"] + "," + _edvUD100.CurrentDataRow["Key5"];
            var keys = "IT001,,,,";
            return keys;
        }

        private List<string> UpdateShortCutXML(string sysConfigText)
        {
            List<string> returnAttachments = new List<string>();
            if (string.IsNullOrWhiteSpace(sysConfigText)) return returnAttachments;

            System.Xml.Linq.XDocument sysConfigDoc = System.Xml.Linq.XDocument.Parse(sysConfigText, LoadOptions.PreserveWhitespace);
            sysConfigDoc.Declaration = new XDeclaration("1.0", "utf-8", null);

            // Find the <Shortcut> Node so we can update its values                     
            XElement shortcutNode = sysConfigDoc.Descendants("Shortcut").FirstOrDefault();
            if (shortcutNode == null) // Create the node if missing
            {
                if (sysConfigDoc.Element("configuration") == null)
                    sysConfigDoc.Add(new XElement("configuration"));

                sysConfigDoc.Element("configuration").Add(new XElement("Shortcut"));
                shortcutNode = sysConfigDoc.Element("configuration").Element("Shortcut");
            }

            shortcutNode.SetElementValue("Company", ((Session)oTrans.Session).CompanyID);
            shortcutNode.SetElementValue("Plant", ((Session)oTrans.Session).PlantID);
            shortcutNode.SetElementValue("AppServerURL", ((Session)oTrans.Session).AppServer);

            shortcutNode.SetElementValue("DateTime", DateTime.Now.ToString());
            shortcutNode.SetElementValue("Originator", ((Session)oTrans.Session).UserID);

            // Set the <Process> child nodes
            XElement processNode = shortcutNode.Element("Process");
            if (processNode == null)
            {
                shortcutNode.Add(new XElement("Process"));
                processNode = shortcutNode.Element("Process");
            }

            processNode.SetElementValue("ProcessID", "ITAR001"); // MenuID is normally stored in XXXDef.SysCharacter01
            processNode.SetElementValue("Description", "UD01 Notify");

            // Set the RecordIDS node and its child elements
            XElement recordIDsElement = shortcutNode.Element("RecordIDS");
            if (recordIDsElement == null)
            {
                shortcutNode.Add(new XElement("RecordIDS"));
                recordIDsElement = shortcutNode.Element("RecordIDS");
            }

            var keyFieldsValues = GetKeyValues();
            var keyFieldTypes = GetKeyTpes();
            var keyFields = GetKeyFields();
            recordIDsElement.SetAttributeValue("KeyFields", keyFields);
            recordIDsElement.SetAttributeValue("KeyFieldsType", "System.String");
            recordIDsElement.SetAttributeValue("TableName", "UD001List");
            recordIDsElement.SetAttributeValue("DataSourceType", "UD01ListDataSet");
            recordIDsElement.SetElementValue("RecordID", keyFieldsValues);
            StringBuilder sb = new StringBuilder();
            //StringWriter swriter = new StringWriter(sb);
            using (StringWriter writer = new Utf8StringWriter(sb))
            {
                sysConfigDoc.Save(writer);
            }
            returnAttachments.Add(sb.ToString());
            return returnAttachments;
        }
        public class Utf8StringWriter : StringWriter
        {
            public Utf8StringWriter(StringBuilder sb)
                : base(sb)
            {

            }
            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }

        #endregion

    }
}
