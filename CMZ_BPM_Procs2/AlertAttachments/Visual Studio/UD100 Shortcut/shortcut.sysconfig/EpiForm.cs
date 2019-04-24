using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Ice.Lib.Framework;
using Infragistics.Win.UltraWinToolbars;
using Erp.Adapters;
using Ice.Adapters;
// *** Change the form using statement to the UI form you want to get the Transaction class from.


using Ice.UI.App.UD100Entry;
using Erp.UI.Controls.Combos;
using Ice.Lib;
using Ice.BO;
using Erp.BO;
using System.Collections;
using Epicor.Data;
using System.Xml.Linq;
using System.Linq;
using Ice.Lib.Searches;
using System.Text;
using Ice.Core;
using Ice.Lib.Customization;
using System.IO;
using System.Collections.Generic;
using System.Net.Mail;
using Ice.Lib.ExtendedProps;

/// <summary>
/// 1 - Go to the project properties and Change your .NET framework to the one that matches your environment.
/// 2 - In properties, from the Build option, set the Output path to your local client install folder that contains your Epicor client dll's.
/// 3 - After adding the desired reference to the UI form you will be working with.  See the above comment line *** Change the form using statement...
/// 4 - If necessary, adjust the declaration for oTrans to accommodate the class name of the form transaction you are working with.
/// 5 - Inside the form constructor, change the name of your sysconfig file if necessary.
/// </summary>
namespace EpiFormTemplate
{
    public partial class EpiForm : Form
    {
        private CustomScriptManager csm;
        public Transaction oTrans;
        private UltraToolbarsManager baseToolbarsManager;
        private EpiUltraCombo eucPartNum;
        private DataTable UD100_Column;
        private DataTable UD100A_Column;
        private EpiUltraCombo eucReviewEmail;
        public EpiForm()
        {
            // Change the sysconfig file name to the sysconfig file name you are using to connect to your app server from your client.
            // If you have web services licenses, use the Session.LicenseType.WebService else use Session.LicenseType.Default.
            using (var _session = new Ice.Core.Session("manager", "manager", Session.LicenseType.Default, @"config\ERP10.sysconfig"))
            {
                // 10.1.600.x For some forms a BASE currency must be added to the Session objects CurrencyInfo Hashtable. SalesOrder is
                // one of these forms.
                using (var svc = WCFServiceSupport.CreateImpl<Erp.Proxy.BO.CurrencyImpl>(_session, Erp.Proxy.BO.CurrencyImpl.UriPath))
                {
                    bool outbool;
                    var defaultcurrency = svc.GetList("BaseCurr = 'TRUE'", 0, 0, out outbool);
                    if (defaultcurrency.Tables[0].Rows.Count == 0)
                    {
                        MessageBox.Show("No BASE currency code is set for Company " + _session.CompanyID, "No BASE Currency", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    var currencyrow = (Erp.BO.CurrencyListDataSet.CurrencyListRow)defaultcurrency.Tables[0].Rows[0];

                    Session.CurrencyInfo ci = new Session.CurrencyInfo("BASE", currencyrow.CurrSymbol,
                                                                       currencyrow.DecimalsGeneral, currencyrow.DecimalsPrice, currencyrow.DecimalsCost);
                    _session.CurrencyCodes.Add("BASE", ci);
                }

                var iLaunch = new ILauncher(_session);
                oTrans = new Transaction(iLaunch);
                                
                // Run your methods here and always try to complete the execution of this using block.
                // If you terminate execution prior to that, you will leave an open session and continue to consume a license.
                //LoadXXData();
                BuildShortCut();
            }
        }

        // Paste Script code under this comment line. //

        #region Script
        // ** Wizard Insert Location - Do Not Remove 'Begin/End Wizard Added Module Level Variables' Comments! **
        // Begin Wizard Added Module Level Variables **

        // End Wizard Added Module Level Variables **

        // Add Custom Module Level Variables Here **
        public void InitializeCustomCode()
        {
            // ** Wizard Insert Location - Do not delete 'Begin/End Wizard Added Variable Initialization' lines **
            // Begin Wizard Added Variable Initialization

            this.UD100_Column.ColumnChanged += new DataColumnChangeEventHandler(this.UD100_AfterFieldChange);
            this.UD100A_Column.ColumnChanged += new DataColumnChangeEventHandler(this.UD100A_AfterFieldChange);
            // End Wizard Added Variable Initialization

            // Begin Wizard Added Custom Method Calls
            SetExtendedProperties();
            FillPartDropDown();
            FillUserEmails();
            CreateRowRuleUD100CheckBox01Equals_False(); ;
            // End Wizard Added Custom Method Calls
        }

        public void DestroyCustomCode()
        {
            // ** Wizard Insert Location - Do not delete 'Begin/End Wizard Added Object Disposal' lines **
            // Begin Wizard Added Object Disposal

            this.UD100_Column.ColumnChanged -= new DataColumnChangeEventHandler(this.UD100_AfterFieldChange);
            this.UD100A_Column.ColumnChanged -= new DataColumnChangeEventHandler(this.UD100A_AfterFieldChange);
            // End Wizard Added Object Disposal

            // Begin Custom Code Disposal

            // End Custom Code Disposal
        }

        private void SetExtendedProperties()
        {
            // Begin Wizard Added EpiDataView Initialization
            EpiDataView edvUD100A = ((EpiDataView)(this.oTrans.EpiDataViews["UD100A"]));
            // End Wizard Added EpiDataView Initialization

            // Begin Wizard Added Conditional Block
            if (edvUD100A.dataView.Table.Columns.Contains("Character01"))
            {
                // Begin Wizard Added ExtendedProperty Settings: edvUD100A-Character01
                edvUD100A.dataView.Table.Columns["Character01"].ExtendedProperties["ReadOnly"] = true;
                // End Wizard Added ExtendedProperty Settings: edvUD100A-Character01
            }
            // End Wizard Added Conditional Block
        }
        private void CreateRowRuleUD100CheckBox01Equals_False()
        {
            // Description: PackSlipComments
            // **** begin autogenerated code ****
            RuleAction disabledUD100_Character02 = RuleAction.AddControlSettings(this.oTrans, "UD100.Character02", SettingStyle.Disabled);
            RuleAction[] ruleActions = new RuleAction[] {
            disabledUD100_Character02};
            // Create RowRule and add to the EpiDataView.
            RowRule rrCreateRowRuleUD100CheckBox01Equals_False = new RowRule("UD100.CheckBox01", RuleCondition.Equals, false, ruleActions);
            ((EpiDataView)(this.oTrans.EpiDataViews["UD100"])).AddRowRule(rrCreateRowRuleUD100CheckBox01Equals_False);
            // **** end autogenerated code ****
        }

        private void FillPartDropDown()
        {
            // Wizard Generated Search Method
            // You will need to call this method from another method in custom code
            // For example, [Form]_Load or [Button]_Click

            bool recSelected;
            string whereClause = string.Empty;
            System.Data.DataSet dsPartAdapter = Ice.UI.FormFunctions.SearchFunctions.listLookup(this.oTrans, "PartAdapter", out recSelected, false, whereClause);
            if (recSelected)
            {
                // Set EpiUltraCombo Properties
                this.eucPartNum.ValueMember = "PartNum";
                this.eucPartNum.DataSource = dsPartAdapter;
                this.eucPartNum.DisplayMember = "PartNum";
                string[] fields = new string[] {
                "PartNum", "PartDescription"};
                this.eucPartNum.SetColumnFilter(fields);
            }
        }
        private void UD100_AfterFieldChange(object sender, DataColumnChangeEventArgs args)
        {
            // ** Argument Properties and Uses **
            // args.Row["FieldName"]
            // args.Column, args.ProposedValue, args.Row
            // Add Event Handler Code
            switch (args.Column.ColumnName)
            {
                case "CheckBox01":
                    if (!(bool)args.ProposedValue)
                        args.Row["Character02"] = string.Empty;
                    break;
            }
        }

        private void UD100A_AfterFieldChange(object sender, DataColumnChangeEventArgs args)
        {
            // ** Argument Properties and Uses **
            // args.Row["FieldName"]
            // args.Column, args.ProposedValue, args.Row
            // Add Event Handler Code
            switch (args.Column.ColumnName)
            {
                case "ChildKey1":
                    args.Row["Character01"] = GertPartDesc(args.ProposedValue.ToString());
                    break;
            }
        }
        private string GertPartDesc(string partnum)
        {
            var pdesc = string.Empty;
            using (var pad = new Erp.Adapters.PartAdapter(oTrans))
            {
                pad.BOConnect();

                if (pad.GetByID(partnum))
                    pdesc = ((Erp.BO.PartDataSet.PartRow)pad.PartData.Part.Rows[pad.PartData.Part.Rows.Count - 1]).PartDescription;
                // This works for UD columns
                // pdesc = pad.PartData.Part.Rows[pad.PartData.Part.Rows.Count - 1]["PartDesription"].ToString();
            }
            return pdesc;
        }

        private void FillUserEmails()
        {
            // Wizard Generated Search Method
            // You will need to call this method from another method in custom code
            // For example, [Form]_Load or [Button]_Click

            bool recSelected;
            string whereClause = "EMailAddress <> ''";
            System.Data.DataSet dsUserFileAdapter = Ice.UI.FormFunctions.SearchFunctions.listLookup(this.oTrans, "UserFileAdapter", out recSelected, false, whereClause);
            if (recSelected)
            {
                // Set EpiUltraCombo Properties
                this.eucReviewEmail.ValueMember = "EMailAddress";
                this.eucReviewEmail.DataSource = dsUserFileAdapter;
                this.eucReviewEmail.DisplayMember = "EMailAddress";
                string[] fields = new string[] {
                    "DcdUserID", "EMailAddress"};
                this.eucReviewEmail.SetColumnFilter(fields);
            }
        }
        private void btnSendShortcut_Click(object sender, System.EventArgs args)
        {
            // ** Place Event Handling Code Here **
            if (string.IsNullOrWhiteSpace(eucReviewEmail.Text)) return;
            oTrans.PushStatusText("Sending Mail Notification",true);
            BuildShortCut();
            oTrans.PushStatusText("Ready", false);
            MessageBox.Show("Shortcut for ITAR review was sent to " + eucReviewEmail.Text, "Request Sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region LoadShortcutParms
        public class ShortCutParms
        {
            public string ProcessID;
            public string ProcessDesc;
            public string KeyFields;
            public string KeyTypes;
            public string KeyValues;
            public string TableName;
            public string DataSourceType;
            public ShortCutParms()
            {
                ProcessID = "ITAR001";
                ProcessDesc = "ITAR Review Request";
                KeyFields = "Key1";  // Specify as many keys as needed for the UD table seperated by commas.
                KeyTypes = "System.String";
                TableName = "UD100List";
                DataSourceType = "UD100ListDataSet";
            }

            public void SetKeyValues(Ice.UI.App.UD100Entry.Transaction itrans)
            {
                var _edvUD100 = itrans.Factory("UD100");
                KeyValues = _edvUD100.CurrentDataRow["Key1"].ToString(); // Specify the same number of values as keys seperated by commas.
            }
        }
        #endregion

        #region Shortcut
        public class XMLData
        {
            public string Content;
        }

        private void BuildShortCut()
        {
            var dt = LoadXXData();
            var scxmlstring = GetShortCutXML(dt);
            SendMail(PopulateXMLData(scxmlstring));
        }
        private DataTable LoadXXData()
        {
            DataTable dt = null;
            using (var xxxad = new Ice.Adapters.GenXDataAdapter(oTrans))
            {
                xxxad.BOConnect();
                var s = (Session)oTrans.Session;
                xxxad.GetByIDEx(s.ProductCode, "Alert", "MfgsysTemplate", string.Empty, string.Empty);
                dt = xxxad.GenXDataData.Tables[0];
            }
            return dt;
        }

        private string GetShortCutXML(DataTable dt)
        {
            List<XMLData> row = new List<XMLData>();
            foreach (DataRow dr in dt.Rows)
            {
                XMLData col = new XMLData();
                col.Content = dr["Content"].ToString();
                row.Add(col);
            }
            return genericDataDechunker(row);
        }
        private string genericDataDechunker(IEnumerable<XMLData> chunkRows)
        {
            StringBuilder data = new StringBuilder();
            IEnumerator<XMLData> iEnum = chunkRows.GetEnumerator();
            while (iEnum.MoveNext())
            {
                var ro = iEnum.Current;

                data.Append(ro.Content.ToString());
            }

            data = data.Replace("\n", "\r\n");      // ** add back CR character
            data = data.Replace("\r\r\n", "\r\n");  // ** for backward compatibility

            return data.ToString();
        }

        private string PopulateXMLData(string scxmlstring)
        {
            var scp = new ShortCutParms();
            scp.SetKeyValues(oTrans);

            string newstring = scxmlstring.Replace("<Company></Company>", "<Company>" + ((Session)oTrans.Session).CompanyID + "</Company>");
            newstring = newstring.Replace("<Plant></Plant>", "<Plant>" + ((Session)oTrans.Session).PlantID + "</Plant>");
            newstring = newstring.Replace("<AppServerURL></AppServerURL>", "<AppServerURL>" + ((Session)oTrans.Session).AppServer + "</AppServerURL>");
            newstring = newstring.Replace("<DateTime></DateTime>", "<DateTime>" + DateTime.Now.ToString() + "</DateTime>");
            newstring = newstring.Replace("<Originator></Originator>", "<Originator>" + ((Session)oTrans.Session).UserID + "</Originator>");
            newstring = newstring.Replace("<ProcessID></ProcessID>", "<ProcessID>" + scp.ProcessID + "</ProcessID>");
            newstring = newstring.Replace("<Description></Description>", "<Description>" + scp.ProcessDesc + "</Description>");
            newstring = newstring.Replace("<RecordIDS KeyFields=\"\" KeyFieldsType=\"System.String\" TableName=\"\" DataSourceType=\"\">",
                                          "<RecordIDS KeyFields=\"" + scp.KeyFields + "\" KeyFieldsType=\"" + scp.KeyTypes + "\" TableName=\"" + scp.TableName + "\" DataSourceType=\"" + scp.DataSourceType + "\">");
            newstring = newstring.Replace("<RecordID></RecordID>", "<RecordID>" + scp.KeyValues + "</RecordID>");
            return newstring;
        }
        private void SendMail(string shortcut)
        {
            var _edvUD100 = oTrans.Factory("UD100");
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(GetSMTPServer());
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))    // using UTF-8 encoding by default
            using (var mailClient = new System.Net.Mail.SmtpClient(GetSMTPServer(), 25))
            using (var message = new System.Net.Mail.MailMessage("noreply@epicor.com", _edvUD100.CurrentDataRow["EmailAddress"].ToString(), "Use attachd shortcut", "See attachment..."))
            {
                writer.Write(shortcut);
                writer.Flush();
                stream.Position = 0;     // read from the start of what was written

                message.Attachments.Add(new Attachment(stream, "shortcut.sysconfig", "text/csv"));

                mailClient.Send(message);
            }
        }

        private string GetSMTPServer()
        {
            var smtp = string.Empty;
            using (var cad = new Ice.Adapters.CompanyAdapter(oTrans))
            {
                cad.BOConnect();
                var results = cad.GetByID(((Session)oTrans.Session).CompanyID);
                if (results)
                {
                    smtp = ((Ice.BO.CompanyDataSet.CompanyRow)cad.CompanyData.Company.Rows[cad.CompanyData.Company.Rows.Count - 1]).SMTPServer;
                }
            }
            return smtp;
        }
        #endregion
    }
}
