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
                GetNextPartSeq();
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

            this.PartForm.AfterToolClick += new Ice.Lib.Framework.AfterToolClickEventHandler(this.PartForm_AfterToolClick);
            // End Wizard Added Variable Initialization

            // Begin Wizard Added Custom Method Calls

            // End Wizard Added Custom Method Calls            
        }

        public void DestroyCustomCode()
        {
            // ** Wizard Insert Location - Do not delete 'Begin/End Wizard Added Object Disposal' lines **
            // Begin Wizard Added Object Disposal

            this.PartForm.AfterToolClick -= new Ice.Lib.Framework.AfterToolClickEventHandler(this.PartForm_AfterToolClick);
            // End Wizard Added Object Disposal

            // Begin Custom Code Disposal

            // End Custom Code Disposal
        }

        private void PartForm_AfterToolClick(object sender, Ice.Lib.Framework.AfterToolClickEventArgs args)
        {
            switch (args.Tool.Key)
            {
                case "NewMenuTool":
                    var ctxdata = oTrans.Factory("CallContextBpmData");
                    var edvPart = oTrans.Factory("Part");
                    if (ctxdata.HasRow && edvPart.HasRow)
                    {
                        var nextseq = GetNextPartSeq();
                        if (nextseq == 0)
                        {
                            MessageBox.Show("Unable to get next part sequence from UDCodeType AutoPart!", "Next Sequence Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        }
                        edvPart.CurrentDataRow["PartNum"] = ctxdata.CurrentDataRow["ShortChar01"] + nextseq.ToString("-000000#");
                        edvPart.Notify(new EpiNotifyArgs(oTrans, edvPart.Row, EpiTransaction.NotifyType.Initialize));
                    }
                    break;
                default:
                    break;
            }
        }
        private int GetNextPartSeq()
        {
            var nextseq = 0;
            try
            {
                using (var ucad = new UserCodesAdapter(this.oTrans))
                {
                    ucad.BOConnect();
                    bool result = ucad.GetByID("AutoPart");
                    if (result)
                    {
                        nextseq = Convert.ToInt32(ucad.UserCodesData.UDCodeType.Rows[ucad.UserCodesData.UDCodeType.Rows.Count -1]["SequenceID"]);
                        nextseq++;
                        ucad.UserCodesData.UDCodeType.Rows[ucad.UserCodesData.UDCodeType.Rows.Count - 1]["SequenceID"] = nextseq;
                        ucad.Update();
                    }
                }
            }
            catch (System.Exception ex)
            {
                ExceptionBox.Show(ex);
            }
            return nextseq;
        }
        #endregion
    }
}
