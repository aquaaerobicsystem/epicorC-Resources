using System;
using System.Linq;
// Added namespace references
using Ice;
using Erp;
using Ice.Tables;
using Erp.Tablesets;
using System.Data;
using System.Text;
using Ice.Security.RestApi;
using Epicor.ServiceModel.Headers;
using Erp.Tables;

namespace EpiBPMTemplate
{
    /// <summary>
    /// You need to add configuration information that is used to create an instance of the ErpContext.
    /// This is used to create an instance of the Db object which exposes the data base objects defined 
    /// in the data model.
    /// Configuration steps - 
    /// 1 - Open the App.config file.
    /// 2 - On your application server, navigate to the /Server folder your app server Website is running
    ///     from and open the Web.config file using a text editor.
    /// 3 - Copy the entire contents of the Web.config file and replace all of the code in the App.config file with it.
    ///     The Web.config holds configuration data that provides the ErpContext with the required data base connection information.
    /// 4 - Open the project properties and set the target .NET framework to match your environment.
    /// 5 - Set the Build output path to your /Server/Assemblies directory
    /// 6 - In the project properties, in Reference Paths, add the /Server/Bin directory.
    /// 7 - From the /Server/Bin directory, add references to Epicor.ServiceModel.dll, 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// *** Static member declarations ***
        /// The Db context will reference all of the database objects in the data model.
        /// For each temp table you want to work with, you need to add a reference to it's server side Tableset
        /// and declare a static member for it.
        /// </summary>
        private ErpContext Db;
        private POApvMsg ttPOApvMsgRow;
        private const string CompanyID="EPIC06";

        //callContextBpmData

        static void Main(string[] args)
        {
            var pg = new Program();
            pg.TestHarness();
        }

        public void TestHarness()
        {            
            // Create new instance of the ErpContext
            Db = Ice.Services.ContextFactory.CreateContext<ErpContext>();
            LoadttTableRow("POApvMsg");
            FindApprovalPersonEmailAddress();
        }

        private void FindApprovalPersonEmailAddress()
        {
            var EmailAddress = ((from PurAgent_Row in Db.PurAgent
                         where PurAgent_Row.Company == CompanyID && PurAgent_Row.BuyerID == ttPOApvMsgRow.MsgTo
                         select PurAgent_Row.EMailAddress).FirstOrDefault()).ToString();
        }
        private void LoadttTableRow(string ttTable)
        {
            switch (ttTable)
            {
                case "POApvMsg":
                    ttPOApvMsgRow = new POApvMsg
                    {
                        Company = CompanyID,
                        PONum = 4123,
                        MsgType = "1",
                        MsgDate = DateTime.Now,
                        MsgTime = 51848,
                        MsgTo = "HLOW",
                        MsgFrom = "BHOWARD",
                        DcdUserID = "epicor"
                    };
                    break;
                default:
                    break;
            }
        }
    }
}
