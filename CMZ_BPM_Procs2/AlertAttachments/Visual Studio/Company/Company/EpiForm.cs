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
using Erp.UI.App.CompanyEntry;
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
        private Transaction oTrans;
        private UltraToolbarsManager baseToolbarsManager;
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
                GetCompanyData();
                // Run your methods here and always try to complete the execution of this using block.
                // If you terminate execution prior to that, you will leave an open session and continue to consume a license.
            }
        }

        // Paste Script code under this comment line. //
        private void GetCompanyData()
        {
            using (var cad = new Erp.Adapters.CompanyAdapter(oTrans))
            {
                cad.BOConnect();
                cad.GetByID("EPIC06");

            }
        }
    }
}
