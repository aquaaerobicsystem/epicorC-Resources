using System;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Ice.Lib.Framework;
using Ice.Core;
using Infragistics.Win.UltraWinToolbars;
using Erp.Adapters;
using Ice.Adapters;
// *** Change the form using statement to the UI form you want to get the Transaction class from.
using Erp.UI.App.PartEntry;
// using Ice.UI.App.UD01Entry;
using Ice.Lib.Customization;
using Erp.UI.Controls.Combos;
using Ice.Lib;
using Ice.BO;
using Erp.BO;
using Ice.Lib.Searches;
using System.ComponentModel;

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
        private PartTransaction oTrans;
        private UltraToolbarsManager baseToolbarsManager;
        private Erp.UI.App.PartEntry.PartForm PartForm;
        private Ice.Lib.Framework.EpiDataView Part_Row;
        private Ice.Lib.Framework.EpiUltraGrid eugCustXPrt;
        private EpiButton btnAddCustXPrt;
        private CustomerCombo rcCustomer;
        private EpiTextBox tbCustPart;
        private EpiTextBox tbCustPartDesc;
        private EpiTextBox tbCustPartRev;
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
                oTrans = new PartTransaction(iLaunch);
                
                
                // Run your methods here and always try to complete the execution of this using block.
                // If you terminate execution prior to that, you will leave an open session and continue to consume a license.                
            }
        }

        // Paste Script code under this comment line. //
        #region Script
        // ** Wizard Insert Location - Do Not Remove 'Begin/End Wizard Added Module Level Variables' Comments! **
        // Begin Wizard Added Module Level Variables **

        private DataView Part_DataView;
        // End Wizard Added Module Level Variables **

        // Add Custom Module Level Variables Here **
        private DataTable CustXPrt_Column;
        private CustomerPartXRefAdapter cxad;
        private EpiDataView _edvCustXPrt;
        private EpiDataView _edvPart;
        private string _part;

        public void InitializeCustomCode()
        {
            // ** Wizard Insert Location - Do not delete 'Begin/End Wizard Added Variable Initialization' lines **
            // Begin Wizard Added Variable Initialization

            this.Part_DataView = this.Part_Row.dataView;
            this.Part_DataView.ListChanged += new ListChangedEventHandler(this.Part_DataView_ListChanged);
            this.baseToolbarsManager.ToolClick += new Infragistics.Win.UltraWinToolbars.ToolClickEventHandler(this.baseToolbarsManager_ToolClick);
            this.PartForm.BeforeToolClick += new Ice.Lib.Framework.BeforeToolClickEventHandler(this.PartForm_BeforeToolClick);
            this.PartForm.AfterToolClick += new Ice.Lib.Framework.AfterToolClickEventHandler(this.PartForm_AfterToolClick);
            // End Wizard Added Variable Initialization

            // Begin Wizard Added Custom Method Calls
            _part = string.Empty;
            _edvPart = oTrans.Factory("Part");
            InitializeCustXPrtAdapter();

            SetExtendedProperties();

            this.btnAddCustXPrt.Click += new System.EventHandler(this.btnAddCustXPrt_Click);
            // End Wizard Added Custom Method Calls
        }

        public void DestroyCustomCode()
        {
            // ** Wizard Insert Location - Do not delete 'Begin/End Wizard Added Object Disposal' lines **
            // Begin Wizard Added Object Disposal

            this.Part_DataView.ListChanged -= new ListChangedEventHandler(this.Part_DataView_ListChanged);
            this.Part_DataView = null;
            this.baseToolbarsManager.ToolClick -= new Infragistics.Win.UltraWinToolbars.ToolClickEventHandler(this.baseToolbarsManager_ToolClick);
            this.PartForm.BeforeToolClick -= new Ice.Lib.Framework.BeforeToolClickEventHandler(this.PartForm_BeforeToolClick);
            this.PartForm.AfterToolClick -= new Ice.Lib.Framework.AfterToolClickEventHandler(this.PartForm_AfterToolClick);
            this.btnAddCustXPrt.Click -= new System.EventHandler(this.btnAddCustXPrt_Click);
            // End Wizard Added Object Disposal

            // Begin Custom Code Disposal

            // End Custom Code Disposal
        }

        private void Part_DataView_ListChanged(object sender, ListChangedEventArgs args)
        {
            // ** Argument Properties and Uses **
            // Part_DataView[0]["FieldName"]
            // args.ListChangedType, args.NewIndex, args.OldIndex
            // ListChangedType.ItemAdded, ListChangedType.ItemChanged, ListChangedType.ItemDeleted, ListChangedType.ItemMoved, ListChangedType.Reset
            // Add Event Handler Code
            try
            {
                if (Part_DataView.Count > 0)
                {
                    string partnum = Part_DataView[0]["PartNum"].ToString();
                    GetCustXPrtData(partnum);
                }
            }
            catch
            { }
        }

        private void InitializeCustXPrtAdapter()
        {
            // Create an instance of the Adapter.
            this.cxad = new CustomerPartXRefAdapter(this.oTrans);
            this.cxad.BOConnect();

            // Add Adapter Table to List of Views
            // This allows you to bind controls to the custom UD Table
            this._edvCustXPrt = new EpiDataView();
            this._edvCustXPrt.dataView = new DataView(this.cxad.CustomerPartXRefData.CustXPrt);
            if ((this.oTrans.EpiDataViews.ContainsKey("CustXPrtView") == false))
            {
                this.oTrans.Add("CustXPrtView", this._edvCustXPrt);
            }

            // Initialize DataTable variable
            this.CustXPrt_Column = this.cxad.CustomerPartXRefData.CustXPrt;

            // Initialize EpiDataView field.
            this._edvPart = ((EpiDataView)(this.oTrans.EpiDataViews["Part"]));

            // Set the parent view / keys for UD child view
            string[] parentKeyFields = new string[1];
            string[] childKeyFields = new string[1];
            parentKeyFields[0] = "PartNum";
            childKeyFields[0] = "PartNum";
            this._edvCustXPrt.SetParentView(this._edvPart, parentKeyFields, childKeyFields);
        }

        private void GetCustXPrtData(string part)
        {
            if (this._part != part)
            {
                // Build where clause for search.
                string whereClause = "PartNum = '" + part + "'";
                System.Collections.Hashtable whereClauses = new System.Collections.Hashtable(1);
                whereClauses.Add("CustXPrt", whereClause);

                // Call the adapter search.
                SearchOptions searchOptions = SearchOptions.CreateRuntimeSearch(whereClauses, DataSetMode.RowsDataSet);
                this.cxad.InvokeSearch(searchOptions);

                if ((this.cxad.CustomerPartXRefData.CustXPrt.Rows.Count > 0))
                {
                    this._edvCustXPrt.Row = 0;
                }
                else
                {
                    this._edvCustXPrt.Row = -1;
                }

                // Notify that data was updated.
                this._edvCustXPrt.Notify(new EpiNotifyArgs(this.oTrans, this._edvCustXPrt.Row, this._edvCustXPrt.Column));

                // Set key fields to their new values.
                this._part = part;
            }
        }

        private void baseToolbarsManager_ToolClick(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs args)
        {
            switch (args.Tool.Key)
            {
                case "ClearTool":
                    ClearCustXPrtData();
                    break;

                case "UndoTool":
                    UndoCustXPrtChanges();
                    break;
            }
        }

        private void ClearCustXPrtData()
        {
            this._part = string.Empty;
            this.cxad.CustomerPartXRefData.Clear();
            // Notify that data was updated.
            this._edvCustXPrt.Notify(new EpiNotifyArgs(this.oTrans, this._edvCustXPrt.Row, this._edvCustXPrt.Column));
        }

        private void UndoCustXPrtChanges()
        {
            this.cxad.CustomerPartXRefData.RejectChanges();

            // Notify that data was updated.
            this._edvCustXPrt.Notify(new EpiNotifyArgs(this.oTrans, this._edvCustXPrt.Row, this._edvCustXPrt.Column));
        }

        private void PartForm_BeforeToolClick(object sender, Ice.Lib.Framework.BeforeToolClickEventArgs args)
        {
            switch (args.Tool.Key)
            {
                case "SaveTool":
                    SaveCustXPrtRecord();
                    break;
            }
        }
        private void SaveCustXPrtRecord()
        {
            // Save adapter data
            this.cxad.Update();
        }


        private void PartForm_AfterToolClick(object sender, Ice.Lib.Framework.AfterToolClickEventArgs args)
        {
            switch (args.Tool.Key)
            {
                case "DeleteTool":
                    DeleteCustXPrtRecord();
                    break;
            }
        }

        private void DeleteCustXPrtRecord()
        {
            // Check to see if the entire part was deleted
            if (_edvCustXPrt.dataView.Count > 0)
            {
                if (!CheckIfPartWasDeleted(_edvCustXPrt.dataView[0]["PartNum"].ToString()))
                {
                    this.oTrans.PushStatusText("Deleting CustXPrt data...", true);
                    foreach (DataRow dr in cxad.CustomerPartXRefData.CustXPrt.Select("PartNum = '" + this._part + "'"))
                    {
                        this.cxad.Delete(dr);
                    }
                    cxad.Update();
                    ClearCustXPrtData();
                    this.oTrans.PushStatusText("Ready", false);
                }
                else if (this.oTrans.LastView.ViewName == "CustXPrtView" && _edvCustXPrt.HasRow)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Delete Customer Part - ");
                    sb.AppendLine(_edvCustXPrt.CurrentDataRow["XPartNum"].ToString());
                    sb.AppendLine(_edvCustXPrt.CurrentDataRow["PartDescription"].ToString());
                    if (DialogResult.Yes == MessageBox.Show(sb.ToString(), "Delete?", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    {
                        this.oTrans.PushStatusText("Deleting CustXPrt part...", true);
                        this.cxad.Delete(_edvCustXPrt.CurrentDataRow);
                        cxad.Update();
                        this.oTrans.PushStatusText("Ready", false);
                    }
                }
            }
        }
        private bool CheckIfPartWasDeleted(string partnum)
        {
            var exist = false;
            using (var pad = new PartAdapter(oTrans))
            {
                pad.BOConnect();
                try
                {
                    exist = pad.GetByID(partnum);
                }
                catch { }
            }
            return exist;
        }

        private void SetExtendedProperties()
        {
            // Begin Wizard Added EpiDataView Initialization
            EpiDataView edvCustXPrtView = ((EpiDataView)(this.oTrans.EpiDataViews["CustXPrtView"]));
            // End Wizard Added EpiDataView Initialization

            // Begin Wizard Added Conditional Block
            if (edvCustXPrtView.dataView.Table.Columns.Contains("XPartNum"))
            {
                // Begin Wizard Added ExtendedProperty Settings: edvCustXPrtView-XPartNum
                edvCustXPrtView.dataView.Table.Columns["XPartNum"].ExtendedProperties["ReadOnly"] = true;
                // End Wizard Added ExtendedProperty Settings: edvCustXPrtView-XPartNum
            }
            if (edvCustXPrtView.dataView.Table.Columns.Contains("XRevisionNum"))
            {
                // Begin Wizard Added ExtendedProperty Settings: edvCustXPrtView-XRevisionNum
                edvCustXPrtView.dataView.Table.Columns["XRevisionNum"].ExtendedProperties["ReadOnly"] = true;
                // End Wizard Added ExtendedProperty Settings: edvCustXPrtView-XRevisionNum
            }
            if (edvCustXPrtView.dataView.Table.Columns.Contains("PartDescription"))
            {
                // Begin Wizard Added ExtendedProperty Settings: edvCustXPrtView-PartDescription
                edvCustXPrtView.dataView.Table.Columns["PartDescription"].ExtendedProperties["ReadOnly"] = true;
                // End Wizard Added ExtendedProperty Settings: edvCustXPrtView-PartDescription
            }
            if (edvCustXPrtView.dataView.Table.Columns.Contains("CustNumCustID"))
            {
                // Begin Wizard Added ExtendedProperty Settings: edvCustXPrtView-CustNumCustID
                edvCustXPrtView.dataView.Table.Columns["CustNumCustID"].ExtendedProperties["ReadOnly"] = true;
                // End Wizard Added ExtendedProperty Settings: edvCustXPrtView-CustNumCustID
            }
            // End Wizard Added Conditional Block
        }

        private void btnAddCustXPrt_Click(object sender, System.EventArgs args)
        {
            // ** Place Event Handling Code Here **
            if (!_edvPart.HasRow) return;
            AddCustXPart();
        }
        private void AddCustXPart()
        {
            int custnum = 0;
            if (rcCustomer.Value != null)
                custnum = (int)rcCustomer.Value;
            string partnum = _edvPart.CurrentDataRow["PartNum"].ToString();
            string custpart = tbCustPart.Text;
            string custpartdesc = tbCustPartDesc.Text;
            string custpartrev = tbCustPartRev.Text;

            using (var pxad = new Erp.Adapters.CustomerPartXRefAdapter(oTrans))
            {
                pxad.BOConnect();
                bool result = false;
                try
                {
                    result = pxad.GetNewCustXPrt(custnum, partnum, custpart);

                    var pxrow = pxad.CustomerPartXRefData.CustXPrt.Rows[pxad.CustomerPartXRefData.CustXPrt.Rows.Count - 1];
                    pxrow["PartDescription"] = custpartdesc;
                    pxrow["XRevisionNum"] = custpartrev;
                    pxad.Update();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed adding new CustXPrt!" + Environment.NewLine + ex.Message, "Failed Adding CustXPrt!", MessageBoxButtons.OK, MessageBoxIcon.Error );
                }
                ClearCustXPrtData();
                GetCustXPrtData(partnum);
            }
        }

        #endregion
    }
}
