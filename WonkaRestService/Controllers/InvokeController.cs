using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

using Nethereum.Web3.Accounts;

using WonkaBre;
using WonkaEth.Extensions;
using WonkaBre.RuleTree;
using WonkaPrd;
using WonkaRef;

using WonkaRestService.Cache;
using WonkaRestService.Extensions;
using WonkaRestService.Models;

namespace WonkaRestService.Controllers
{
    /// <summary>
    /// 
    /// This controller invokes the Wonka rules engine.  You can test the service by using the POST/PUT methods with the following payload:
    ///
    /// {
    ///   "NewSaleEAN": "9781234567890",
    ///   "NewSaleItemType": "Widget",
    ///   "CountryOfSale": "UK",
    ///   "NewSalesTransSeq": "123456789"  
    /// }
    ///
    /// </summary>
    public class InvokeController : WonkaBaseController
    {
        #region CONSTANTS

        public const string CONST_RECORD_KEY_RULE_TREE_ID = "RuleTreeId";
        public const string CONST_RECORD_KEY_SEND_TRX_GAS = "InvokeRuleTreeGasThreshold";
        public const string CONST_RECORD_KEY_CQS_FLAG     = "CQS";

        #endregion

        /**
         ** NOTE: Likely will be removed in the near future
         **
        // GET: api/Invoke/5
        public string Get(int id)
        {
            return "value";
        }
         **/

        /// <summary>
        /// 
        /// This method will accept a data record, which will then be used to invoke the rules engine
        /// and execute it within the .NET domain only.
        /// 
        /// <param name="poRecord">The data record to feed into the rules engine</param>
        /// <returns>Contains the Response with the updated record (and an error message if an error occurs)</returns>
        /// </summary>
        public HttpResponseMessage PostRecord([FromBody]IDictionary<string, string> poRecord)
        {
            Hashtable poTrxRecord = poRecord.TransformToTrxRecord();

            SvcBasicRecord BasicRecord = new SvcBasicRecord() { RecordData = poTrxRecord };

            var response = Request.CreateResponse<SvcBasicRecord>(HttpStatusCode.Created, BasicRecord);

            string uri = Url.Link("DefaultApi", new { id = "DefaultValue" });

            response.Headers.Location = new Uri(uri);

            try
            {
                Init();

                if (poRecord != null)
                {
                    string sRuleTreeId = null;
                    if (poRecord.ContainsKey(CONST_RECORD_KEY_RULE_TREE_ID))
                        sRuleTreeId = poRecord[CONST_RECORD_KEY_RULE_TREE_ID];

                    WonkaServiceCache ServiceCache = WonkaServiceCache.GetInstance();

                    if (!String.IsNullOrEmpty(sRuleTreeId) && !ServiceCache.RuleTreeCache.ContainsKey(sRuleTreeId))
                        throw new Exception("ERROR!  Rule Tree (" + sRuleTreeId + ") since it does not exist.");

                    WonkaProduct WonkaRecord = poRecord.TransformToWonkaProduct();

                    WonkaBre.Reporting.WonkaBreRuleTreeReport RuleTreeReport = ExecuteDotNet(WonkaRecord, sRuleTreeId);

                    if (RuleTreeReport.OverallRuleTreeResult != ERR_CD.CD_SUCCESS)
                        BasicRecord.RuleTreeReport = RuleTreeReport;

                    BasicRecord.RecordData = WonkaRecord.TransformToTrxRecord();
                }

                response = Request.CreateResponse<SvcBasicRecord>(HttpStatusCode.Created, BasicRecord);
            }
            catch (Exception ex)
            {
                string sErrorMsg = String.Format("ERROR!  Invoke web method -> Error Message : {0}",
                                                 ex.ToString());

                if ((ex.InnerException != null) && (ex.InnerException.Message != null))
                    BasicRecord.ErrorMessage = ex.InnerException.Message;
                else if (!String.IsNullOrEmpty(ex.Message))
                    BasicRecord.ErrorMessage = ex.Message;

                BasicRecord.StackTraceMessage = ex.StackTrace;

                response = Request.CreateResponse<SvcBasicRecord>(HttpStatusCode.BadRequest, BasicRecord);

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return response;
        }

        /// <summary>
        /// 
        /// This method will accept a data record, which will then be used to invoke the rules engine 
        /// on the blockchain.
        /// 
        /// <param name="poRecord">The data record to feed into the rules engine</param>
        /// <returns>Contains the Response with the updated record (and an error message if an error occurs)</returns>
        /// </summary>
        public HttpResponseMessage PutRecord([FromBody]IDictionary<string, string> poRecord)
        {
            Hashtable poTrxRecord = poRecord.TransformToTrxRecord();

            SvcBasicRecord BasicRecord = new SvcBasicRecord() { RecordData = poTrxRecord };

            var response = Request.CreateResponse<SvcBasicRecord>(HttpStatusCode.Created, BasicRecord);

            string uri = Url.Link("DefaultApi", new { id = "DefaultValue" });

            response.Headers.Location = new Uri(uri);

            try
            {
                Init();

                if (poRecord != null)
                {
                    string sRuleTreeId = null;
                    uint   nSendTrxGas = 0;
                    bool   bCQSFlag    = false;                    

                    if (poRecord.ContainsKey(CONST_RECORD_KEY_RULE_TREE_ID))
                        sRuleTreeId = poRecord[CONST_RECORD_KEY_RULE_TREE_ID];

                    if (poRecord.ContainsKey(CONST_RECORD_KEY_SEND_TRX_GAS) && (!String.IsNullOrEmpty(poRecord[CONST_RECORD_KEY_SEND_TRX_GAS])))
                        UInt32.TryParse(poRecord[CONST_RECORD_KEY_SEND_TRX_GAS], out nSendTrxGas);

                    if (poRecord.ContainsKey(CONST_RECORD_KEY_CQS_FLAG))
                        bCQSFlag = (poRecord[CONST_RECORD_KEY_CQS_FLAG] == "Y");

                    WonkaServiceCache ServiceCache = WonkaServiceCache.GetInstance();

                    if (!String.IsNullOrEmpty(sRuleTreeId) && !ServiceCache.RuleTreeCache.ContainsKey(sRuleTreeId))
                        throw new Exception("ERROR!  Rule Tree (" + sRuleTreeId + ") cannot be invoked since it does not exist.");

                    var RuleTreeOriginData = ServiceCache.RuleTreeOriginCache[sRuleTreeId];

                    WonkaProduct WonkaRecord = new WonkaProduct();

                    WonkaBre.Reporting.WonkaBreRuleTreeReport RuleTreeReport = null;

                    if (mbInteractWithChain)
                    {
                        if ((sRuleTreeId == CONST_RULES_RESOURCE_ID) && bCQSFlag)
                        {
                            WonkaRecord = poRecord.TransformToWonkaProduct();

                            ExecuteEthereumCQS(WonkaRecord, RuleTreeOriginData);
                        }
                        else
                        {
                            if ((RuleTreeOriginData.MinGasCost > 0) && (nSendTrxGas > 0) && (RuleTreeOriginData.MinGasCost > nSendTrxGas))
                                throw new Exception("ERROR!  Supplied gas amount (" + nSendTrxGas + ") is less than the minimum gas needed (" + RuleTreeOriginData.MinGasCost + ").");

                            WonkaRecord = poRecord.TransformToWonkaProduct(false);
                            WonkaRecord.SerializeProductData(ServiceCache.RuleTreeCache[sRuleTreeId], moOrchInitData.Web3HttpUrl);

                            WonkaRecord = poRecord.TransformToWonkaProduct();

                            var SvcReport = ExecuteEthereum(WonkaRecord, RuleTreeOriginData, ServiceCache.RuleTreeCache[sRuleTreeId], nSendTrxGas);
                            SvcReport.RecordData = BasicRecord.RecordData;

                            // We only return the base class WonkaBreRuleTreeReport to the caller here
                            RuleTreeReport = SvcReport;

                            // NOTE: We only cache the reports when we invoke a RuleTree on the chain
                            if (!ServiceCache.ReportCache.ContainsKey(sRuleTreeId))
                                ServiceCache.ReportCache[sRuleTreeId] = new List<SvcRuleTreeReport>();

                            ServiceCache.ReportCache[sRuleTreeId].Add(SvcReport);
                        }
                    }

                    BasicRecord.RecordData = WonkaRecord.TransformToTrxRecord();

                    if (RuleTreeReport != null)
                        BasicRecord.RuleTreeReport = RuleTreeReport;
                }

                response = Request.CreateResponse<SvcBasicRecord>(HttpStatusCode.Created, BasicRecord);
            }
            catch (Exception ex)
            {
                string sErrorMsg = String.Format("ERROR!  Invoke web method -> Error Message : {0}",
                                                 ex.ToString());

                if ((ex.InnerException != null) && (ex.InnerException.Message != null))
                    BasicRecord.ErrorMessage = ex.InnerException.Message;
                else if (!String.IsNullOrEmpty(ex.Message))
                    BasicRecord.ErrorMessage = ex.Message;

                BasicRecord.StackTraceMessage = ex.StackTrace;

                response = Request.CreateResponse<SvcBasicRecord>(HttpStatusCode.BadRequest, BasicRecord);

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return response;
        }

        /**
         ** NOTE: Likely will be removed in the near future
         **
        // DELETE: api/Invoke/5
        public void Delete(int id)
        {
        }
         **/

        #region Support Methods

        public static string DeployWonkaContract()
        {
            string sWonkaContractAddress = "";

            // NOTE: Yet to be implemented

            return sWonkaContractAddress;
        }

        private string DeployOrchestrationContract()
        {
            string sOrchestrationContractAddress = "";

            // NOTE: Yet to be implemented

            return sOrchestrationContractAddress;
        }

        private WonkaBre.Reporting.WonkaBreRuleTreeReport ExecuteDotNet(WonkaProduct NewRecord, string psRuleTreeId = CONST_RULES_RESOURCE_ID)
        {
            // Using the metadata source, we create an instance of a defined data domain
            WonkaRefEnvironment RefEnv =
                WonkaRefEnvironment.CreateInstance(false, moMetadataSource);

            WonkaServiceCache ServiceCache = WonkaServiceCache.GetInstance();

            if (String.IsNullOrEmpty(psRuleTreeId))
                psRuleTreeId = CONST_RULES_RESOURCE_ID;

            if (psRuleTreeId == CONST_RULES_RESOURCE_ID)
                GetValuesFromOtherSources(NewRecord);

            WonkaRefAttr NewSellTaxAmountAttr = RefEnv.GetAttributeByAttrName("NewSellTaxAmount");
            WonkaRefAttr VATAmountForHMRCAttr = RefEnv.GetAttributeByAttrName("NewVATAmountForHMRC");

            WonkaBreRulesEngine RulesEngine = null;
            if (!String.IsNullOrEmpty(psRuleTreeId) && ServiceCache.RuleTreeCache.ContainsKey(psRuleTreeId))
                RulesEngine = ServiceCache.RuleTreeCache[psRuleTreeId];
            else
            {
                RulesEngine =
                    new WonkaBreRulesEngine(new StringBuilder(msVATCalcRulesContents), moAttrSourceMap, moCustomOpMap, moMetadataSource, false);

                ServiceCache.RuleTreeCache[psRuleTreeId] = RulesEngine;
            }

            // Check that the data has been populated correctly on the "new" record
            string sNewSellTaxAmt    = NewRecord.GetAttributeValue(NewSellTaxAmountAttr);
            string sVATAmountForHRMC = NewRecord.GetAttributeValue(VATAmountForHMRCAttr);

            // Since the rules can reference values from different records (like O.Price for the existing
            // record's price and N.Price for the new record's price), we need to provide the delegate
            // that can pull the existing (i.e., old) record using a key
            // RulesEngine.GetCurrentProductDelegate = GetOldProduct;

            // Validate the new record using our rules engine and its initialized RuleTree
            WonkaBre.Reporting.WonkaBreRuleTreeReport RuleTreeReport = RulesEngine.Validate(NewRecord);

            // Check that the data has been populated correctly on the "new" record
            string sNewSellTaxAmtAfter    = NewRecord.GetAttributeValue(NewSellTaxAmountAttr);
            string sVATAmountForHRMCAfter = NewRecord.GetAttributeValue(VATAmountForHMRCAttr);

            // if (Report.GetRuleSetFailureCount() > 0)
            //    throw new Exception("Oh heavens to Betsy! Something bad happened!");

            return RuleTreeReport;
        }

        private SvcRuleTreeReport ExecuteEthereum(WonkaProduct NewRecord, SvcRuleTree RuleTreeOriginData, WonkaBreRulesEngine RulesEngine, uint AssignedTrxGas = 0)
        {
            Nethereum.Contracts.Contract WonkaContract = null;

            SvcRuleTreeReport RuleTreeReport = new SvcRuleTreeReport(false);

            WonkaServiceCache ServiceCache = WonkaServiceCache.GetInstance();

            string sInvokeSender   = null;
            string sInvokePassword = null;

            if (!String.IsNullOrEmpty(RuleTreeOriginData.OwnerName) && WonkaServiceCache.GetInstance().TreeOwnerCache.ContainsKey(RuleTreeOriginData.OwnerName))
            {
                sInvokeSender   = ServiceCache.TreeOwnerCache[RuleTreeOriginData.OwnerName].OwnerAddress;
                sInvokePassword = ServiceCache.TreeOwnerCache[RuleTreeOriginData.OwnerName].OwnerPassword;

                WonkaContract = WonkaBaseController.GetAltContract(sInvokeSender, sInvokePassword, msAbiWonka, msWonkaContractAddress, moOrchInitData.Web3HttpUrl);                
            }
            else
            {
                sInvokeSender   = msSenderAddress;
                sInvokePassword = msPassword;

                WonkaContract = GetWonkaContract();
            }

            var BlockchainReport = RulesEngine.InvokeOnChain(WonkaContract, sInvokeSender, AssignedTrxGas);
            if (BlockchainReport != null)
            {
                RuleTreeReport = new SvcRuleTreeReport(false, BlockchainReport);

                if (!String.IsNullOrEmpty(BlockchainReport.TransactionHash))
                {
                    var receipt =
                        GetWeb3().Eth.Transactions.GetTransactionReceipt.SendRequestAsync(BlockchainReport.TransactionHash).Result;

                    RuleTreeReport.ExecutionGasCost = (uint) receipt.CumulativeGasUsed.Value;
                }
            }

            NewRecord.DeserializeProductData(RulesEngine, moOrchInitData.Web3HttpUrl);

            return RuleTreeReport;
        }

        private void ExecuteEthereumCQS(WonkaProduct NewRecord, SvcRuleTree RuleTreeOriginData)
        {
            WonkaRefEnvironment RefEnv = WonkaRefEnvironment.GetInstance();

            // Now we assemble the data record for processing - in our VAT Calculation example, parts of the 
            // logical record can exist within contract(s) within the blockchain (which has been specified 
            // via Orchestration metadata), like a logistics or supply contract - these properties below 
            // (NewSaleEAN, NewSaleItemType, CountryOfSale) would be supplied by a client, like an 
            // eCommerce site, and be persisted to the blockchain so we may apply the RuleTree to the logical record
            CQS.Contracts.SalesTrxCreateCommand SalesTrxCommand = new CQS.Contracts.SalesTrxCreateCommand();

            WonkaRefAttr NewSaleEANAttr       = RefEnv.GetAttributeByAttrName("NewSaleEAN");
            WonkaRefAttr NewSaleItemTypeAttr  = RefEnv.GetAttributeByAttrName("NewSaleItemType");
            WonkaRefAttr CountryOfSaleAttr    = RefEnv.GetAttributeByAttrName("CountryOfSale");
            WonkaRefAttr NewSellTaxAmtAttr    = RefEnv.GetAttributeByAttrName("NewSellTaxAmount");
            WonkaRefAttr NewVATAmtForHMRCAttr = RefEnv.GetAttributeByAttrName("NewVATAmountForHMRC");

            SalesTrxCommand.NewSaleEAN      = Convert.ToInt64(NewRecord.GetAttributeValue(NewSaleEANAttr));
            SalesTrxCommand.NewSaleItemType = NewRecord.GetAttributeValue(NewSaleItemTypeAttr);
            SalesTrxCommand.CountryOfSale   = NewRecord.GetAttributeValue(CountryOfSaleAttr);

            #region Invoking the RuleTree for the first time as a single entity

            // The engine's proxy for the blockchain is instantiated here, which will be responsible for serializing
            // and executing the RuleTree within the engine
            CQS.Generation.SalesTransactionGenerator TrxGenerator =
                   new CQS.Generation.SalesTransactionGenerator(SalesTrxCommand, new StringBuilder(msVATCalcRulesContents), moOrchInitData);

            // Here, we invoke the Rules engine on the blockchain, which will calculate the VAT for a sale and then
            // retrieve all values of this logical record (including the VAT) and assemble them within 'SalesTrxCommand'
            bool bValid = TrxGenerator.GenerateSalesTransaction(SalesTrxCommand);

            if (!bValid)
                throw new Exception("Oh heavens to Betsy! Something bad happened!");

            // Since the purpose of this example was to showcase the calculated VAT, we examine them here 
            // (while interactively debugging in Visual Studio)
            string sNewSellTaxAmt    = Convert.ToString(SalesTrxCommand.NewSellTaxAmt);
            string sNewVATAmtForHMRC = Convert.ToString(SalesTrxCommand.NewVATAmtForHMRC);

            WonkaPrd.WonkaPrdExtensions.SetAttribute(NewRecord, NewSellTaxAmtAttr, sNewSellTaxAmt);
            WonkaPrd.WonkaPrdExtensions.SetAttribute(NewRecord, NewVATAmtForHMRCAttr, sNewVATAmtForHMRC);

            #endregion

            /*
            #region Invoking the RuleTree as a registered entity and as a member of a Grove

            // Here, we attempt to call the same RuleTree as above, but we are going to invoke the execution of
            // its Grove "NewSaleGroup" - since it is the sole member of the Grove, it will still be the only RuleTree
            // applied to the record - in this scenario, we pretend that we know nothing about the RuleTree or the Grove, 
            // effectively treating it as a black box and only looking to retrieve the VAT
            WonkaEth.Contracts.WonkaRuleGrove NewSaleGrove = new WonkaEth.Contracts.WonkaRuleGrove("NewSaleGroup");
            NewSaleGrove.PopulateFromRegistry(msAbiWonka);

            // The engine's lightweight proxy for the blockchain is instantiated here
            WonkaEth.Orchestration.WonkaOrchestratorProxy<CQS.Contracts.SalesTrxCreateCommand> TrxGeneratorProxy = 
                new WonkaEth.Orchestration.WonkaOrchestratorProxy<CQS.Contracts.SalesTrxCreateCommand>(SalesTrxCommand, moOrchInitData);

            // We reset the values here and in the blockchain (by serializing)
            SalesTrxCommand.NewSellTaxAmt    = 0;
            SalesTrxCommand.NewVATAmtForHMRC = 0;

            TrxGeneratorProxy.SerializeRecordToBlockchain(SalesTrxCommand);

            // NOTE: Only useful when debugging
            // TrxGeneratorProxy.DeserializeRecordFromBlockchain(SalesTrxCommand);

            Dictionary<string, WonkaEth.Contracts.IOrchestrate> GroveMembers = new Dictionary<string, WonkaEth.Contracts.IOrchestrate>();
            GroveMembers[NewSaleGrove.OrderedRuleTrees[0].RuleTreeId] = TrxGeneratorProxy;

            // With their provided proxies for each RuleTree, we can now execute the Grove (or, in this case, our sole RuleTree)
            NewSaleGrove.Orchestrate(SalesTrxCommand, GroveMembers);

            // Again, since the purpose of this example was to showcase the calculated VAT, we examine them here 
            // (while interactively debugging in Visual Studio)            
            sNewSellTaxAmt    = Convert.ToString(SalesTrxCommand.NewSellTaxAmt);
            sNewVATAmtForHMRC = Convert.ToString(SalesTrxCommand.NewVATAmtForHMRC);

            #endregion
            */

            /*
            // Now test exporting the RuleTree from the blockchain
            var RegistryItem = NewSaleGrove.OrderedRuleTrees[0];
            var ExportedXml  = RegistryItem.ExportXmlString(moOrchInitData.Web3HttpUrl);

            System.Console.WriteLine("DEBUG: The payload is: \n(\n" + ExportedXml + "\n)\n");
            */
        }

        // Serves only as a mockup for the rules engine
        private WonkaProduct GetOldProduct(Dictionary<string,string> poProductKeys)
        {
            WonkaProduct OldProduct = new WonkaProduct();
         
            return OldProduct;            
        }

        private void GetValuesFromOtherSources(WonkaProduct NewSaleProduct)
        {
            // Using the metadata source, we create an instance of a defined data domain
            WonkaRefEnvironment RefEnv =
                WonkaRefEnvironment.CreateInstance(false, moMetadataSource);

            WonkaRefAttr NewSalePriceAttr        = RefEnv.GetAttributeByAttrName("NewSalePrice");
            WonkaRefAttr PrevSellTaxAmtAttr      = RefEnv.GetAttributeByAttrName("PrevSellTaxAmount");
            WonkaRefAttr NewSellTaxAmtAttr       = RefEnv.GetAttributeByAttrName("NewSellTaxAmount");
            WonkaRefAttr NewVATAmountForHRMCAttr = RefEnv.GetAttributeByAttrName("NewVATAmountForHMRC");

            WonkaServiceExtensions.SetAttribute(NewSaleProduct, NewSalePriceAttr,        "100");
            WonkaServiceExtensions.SetAttribute(NewSaleProduct, PrevSellTaxAmtAttr,      "5");
            WonkaServiceExtensions.SetAttribute(NewSaleProduct, NewSellTaxAmtAttr,       "0");
            WonkaServiceExtensions.SetAttribute(NewSaleProduct, NewVATAmountForHRMCAttr, "0");
        }

        public static string LookupVATDenominator(string psSaleItemType, string psCountryOfSale, string psDummyVal1, string psDummyVal2)
        {
            if (psSaleItemType == "Widget" && psCountryOfSale == "UK")
                return "5";
            else
                return "1";
        }

        #endregion
    }
}