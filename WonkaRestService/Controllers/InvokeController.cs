using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    public class InvokeController : ApiController
    {
        #region CONSTANTS

        public const string CONST_RULES_RESOURCE_STREAM = "WonkaRestService.WonkaData.VATCalculationExample.xml";

        #endregion

        static private bool   mbInteractWithChain    = false;
        static private string msSenderAddress        = "";
        static private string msPassword             = "";
        static private string msWonkaContractAddress = "";
        static private string msOrchContractAddress  = "";
        static private string msAbiWonka             = "";
        static private string msAbiOrchContract      = "";
        static private string msRulesContents        = "";

        static private string msCustomOpId     = "";
        static private string msCustomOpMethod = "";

        static private WonkaBreSource       moDefaultSource  = null;
        static private IMetadataRetrievable moMetadataSource = null;

        static private Dictionary<string, WonkaBreSource> moAttrSourceMap = new Dictionary<string, WonkaBreSource>();
        static private Dictionary<string, WonkaBreSource> moCustomOpMap   = null;

        static private WonkaEth.Orchestration.Init.OrchestrationInitData moOrchInitData      = null;
        static private WonkaEth.Init.WonkaEthRegistryInitialization      moWonkaRegistryInit = null;

        /**
         ** NOTE: Likely will be removed in the near future
         **
        // GET: api/Invoke
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
         **/

        /**
         ** NOTE: Likely will be removed in the near future
         **
        // GET: api/Invoke/5
        public string Get(int id)
        {
            return "value";
        }
         **/

        /**
         ** NOTE: Likely will be removed in the near future
         **
        // POST: api/Invoke
        public void Post([FromBody]string value)
        {
        }
         **/

        /// <summary>
        /// 
        /// This method will accept a data record, which will then be used to invoke the rules engine
        /// and execute it within the .NET domain only.
        /// 
        /// <param name="poRecord">The data record to feed into the rules engine/param>
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

                    WonkaProduct WonkaRecord = poRecord.TransformToWonkaProduct();

                    WonkaBre.Reporting.WonkaBreRuleTreeReport RuleTreeReport = ExecuteDotNet(WonkaRecord);

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
        /// <param name="poRecord">The data record to feed into the rules engine/param>
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
                    WonkaProduct WonkaRecord = poRecord.TransformToWonkaProduct();

                    if (mbInteractWithChain)
                        ExecuteEthereum(WonkaRecord);

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

        private string DeployWonkaContract()
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

        private WonkaBre.Reporting.WonkaBreRuleTreeReport ExecuteDotNet(WonkaProduct NewRecord)
        {
            // Using the metadata source, we create an instance of a defined data domain
            WonkaRefEnvironment RefEnv =
                WonkaRefEnvironment.CreateInstance(false, moMetadataSource);

            WonkaServiceCache ServiceCache = WonkaServiceCache.CreateInstance();

            GetValuesFromOtherSources(NewRecord);

            WonkaRefAttr NewSellTaxAmountAttr = RefEnv.GetAttributeByAttrName("NewSellTaxAmount");
            WonkaRefAttr VATAmountForHMRCAttr = RefEnv.GetAttributeByAttrName("NewVATAmountForHMRC");

            WonkaBreRulesEngine RulesEngine = null;
            if (ServiceCache.RuleTreeCache.ContainsKey(CONST_RULES_RESOURCE_STREAM))
                RulesEngine = ServiceCache.RuleTreeCache[CONST_RULES_RESOURCE_STREAM];
            else
            {
                RulesEngine =
                    new WonkaBreRulesEngine(new StringBuilder(msRulesContents), moAttrSourceMap, moCustomOpMap, moMetadataSource, false);

                ServiceCache.RuleTreeCache[CONST_RULES_RESOURCE_STREAM] = RulesEngine;
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

        private void ExecuteEthereum(WonkaProduct NewRecord)
        {
            WonkaRefEnvironment RefEnv = WonkaRefEnvironment.GetInstance();

            // Now we assemble the data record for processing - in our VAT Calculation example, parts of the 
            // logical record can exist within contract(s) within the blockchain (which has been specified 
            // via Orchestration metadata), like a logistics or supply contract - these properties below 
            // (NewSaleEAN, NewSaleItemType, CountryOfSale) would be supplied by a client, like an 
            // eCommerce site, and be persisted to the blockchain so we may apply the RuleTree to the logical record
            CQS.Contracts.SalesTrxCreateCommand SalesTrxCommand = new CQS.Contracts.SalesTrxCreateCommand();

            WonkaRefAttr NewSaleEANAttr      = RefEnv.GetAttributeByAttrName("NewSaleEAN");
            WonkaRefAttr NewSaleItemTypeAttr = RefEnv.GetAttributeByAttrName("NewSaleItemType");
            WonkaRefAttr CountryOfSaleAttr   = RefEnv.GetAttributeByAttrName("CountryOfSale");

            SalesTrxCommand.NewSaleEAN      = Convert.ToInt64(NewRecord.GetAttributeValue(NewSaleEANAttr));
            SalesTrxCommand.NewSaleItemType = NewRecord.GetAttributeValue(NewSaleItemTypeAttr);
            SalesTrxCommand.CountryOfSale   = NewRecord.GetAttributeValue(CountryOfSaleAttr);

            #region Invoking the RuleTree for the first time as a single entity

            // The engine's proxy for the blockchain is instantiated here, which will be responsible for serializing
            // and executing the RuleTree within the engine
            CQS.Generation.SalesTransactionGenerator TrxGenerator =
                   new CQS.Generation.SalesTransactionGenerator(SalesTrxCommand, new StringBuilder(msRulesContents), moOrchInitData);

            // Here, we invoke the Rules engine on the blockchain, which will calculate the VAT for a sale and then
            // retrieve all values of this logical record (including the VAT) and assemble them within 'SalesTrxCommand'
            bool bValid = TrxGenerator.GenerateSalesTransaction(SalesTrxCommand);

            if (!bValid)
                throw new Exception("Oh heavens to Betsy! Something bad happened!");

            // Since the purpose of this example was to showcase the calculated VAT, we examine them here 
            // (while interactively debugging in Visual Studio)
            string sNewSellTaxAmt    = Convert.ToString(SalesTrxCommand.NewSellTaxAmt);
            string sNewVATAmtForHMRC = Convert.ToString(SalesTrxCommand.NewVATAmtForHMRC);

            #endregion

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

            /*
            // Now test exporting the RuleTree from the blockchain
            var RegistryItem = NewSaleGrove.OrderedRuleTrees[0];
            var ExportedXml  = RegistryItem.ExportXmlString(moOrchInitData.Web3HttpUrl);

            System.Console.WriteLine("DEBUG: The payload is: \n(\n" + ExportedXml + "\n)\n");
            */
        }

        public Nethereum.Contracts.Contract GetContract(WonkaBre.RuleTree.WonkaBreSource TargetSource)
        {
            var account = new Account(TargetSource.Password);

            Nethereum.Web3.Web3 web3 = null;
            if ((moOrchInitData != null) && !String.IsNullOrEmpty(moOrchInitData.Web3HttpUrl))
                web3 = new Nethereum.Web3.Web3(account, moOrchInitData.Web3HttpUrl);
            else
                web3 = new Nethereum.Web3.Web3(account);

            var contract = web3.Eth.GetContract(TargetSource.ContractABI, TargetSource.ContractAddress);

            return contract;
        }

        // Serves only as a mockup for the rules engine
        public WonkaProduct GetOldProduct(Dictionary<string,string> poProductKeys)
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

        private void Init()
        {
            var TmpAssembly = Assembly.GetExecutingAssembly();

            // Using the metadata source, we create an instance of a defined data domain
            WonkaRefEnvironment RefEnv =
                WonkaRefEnvironment.CreateInstance(false, moMetadataSource);

            if (String.IsNullOrEmpty(msRulesContents))
            {
                // Read the XML markup that lists the business rules (i.e., the RuleTree)
                using (var RulesReader = new StreamReader(TmpAssembly.GetManifestResourceStream(CONST_RULES_RESOURCE_STREAM)))
                {
                    msRulesContents = RulesReader.ReadToEnd();
                }
            }

            if (moMetadataSource == null)
                moMetadataSource = new WonkaData.WonkaMetadataVATSource();

            if (moOrchInitData == null)
            {
                var DelegateMap =
                    new Dictionary<string, WonkaBre.Readers.WonkaBreXmlReader.ExecuteCustomOperator>();

                DelegateMap["lookupVATDenominator"] = LookupVATDenominator;

                // Read the configuration file that contains all the initialization details regarding the rules engine 
                // (like addresses of contracts, senders, passwords, etc.)
                using (var XmlReader = new System.IO.StreamReader(TmpAssembly.GetManifestResourceStream("WonkaRestService.WonkaData.VATCalculationExample.init.xml")))
                {
                    string sInitXml = XmlReader.ReadToEnd();

                    // We deserialize/parse the contents of the config file
                    System.Xml.Serialization.XmlSerializer WonkaEthSerializer =
                        new System.Xml.Serialization.XmlSerializer(typeof(WonkaEth.Init.WonkaEthInitialization),
                                                                   new System.Xml.Serialization.XmlRootAttribute("WonkaEthInitialization"));

                    WonkaEth.Init.WonkaEthInitialization WonkaInit =
                        WonkaEthSerializer.Deserialize(new System.IO.StringReader(sInitXml)) as WonkaEth.Init.WonkaEthInitialization;

                    // Here, any embeddeded resources mentioned in the config file (instead of simple file URLs) are accessed here
                    WonkaInit.RetrieveEmbeddedResources(TmpAssembly);

                    // The initialization data is transformed into a structure used by the WonkaEth namespace
                    moOrchInitData = WonkaInit.TransformIntoOrchestrationInit(moMetadataSource, DelegateMap);
                }

                if (moWonkaRegistryInit == null)
                {
                    // Read the configuration file that contains all the initialization details regarding the rules registry
                    // (like Ruletree info, Grove info, etc.) - this information will allow us to add our RuleTree to the 
                    // Registry so that it can be discovered by users and so it can be added to a Grove (where it can be executed
                    // as a member of a collection)
                    using (var XmlReader = new System.IO.StreamReader(TmpAssembly.GetManifestResourceStream("WonkaRestService.WonkaData.WonkaRegistry.init.xml")))
                    {
                        string sInitRegistryXml = XmlReader.ReadToEnd();

                        // We deserialize/parse the contents of the config file
                        System.Xml.Serialization.XmlSerializer WonkaEthSerializer =
                            new System.Xml.Serialization.XmlSerializer(typeof(WonkaEth.Init.WonkaEthRegistryInitialization),
                                                                       new System.Xml.Serialization.XmlRootAttribute("WonkaEthRegistryInitialization"));

                        moWonkaRegistryInit =
                            WonkaEthSerializer.Deserialize(new System.IO.StringReader(sInitRegistryXml)) as WonkaEth.Init.WonkaEthRegistryInitialization;

                        // Here, any embeddeded resources mentioned in the config file (instead of simple file URLs) are accessed here                
                        moWonkaRegistryInit.RetrieveEmbeddedResources(TmpAssembly);
                    }
                }

                if (String.IsNullOrEmpty(msSenderAddress))
                {
                    #region Set Class Member Variables
                    msSenderAddress = moOrchInitData.BlockchainEngine.SenderAddress;
                    msPassword      = moOrchInitData.BlockchainEngine.Password;

                    if (moOrchInitData.BlockchainEngine.ContractAddress == null)
                        msWonkaContractAddress = DeployWonkaContract();
                    else
                        msWonkaContractAddress = moOrchInitData.BlockchainEngine.ContractAddress;

                    if (moOrchInitData.DefaultBlockchainDataSource.ContractAddress == null)
                        msOrchContractAddress = DeployOrchestrationContract();
                    else
                        msOrchContractAddress = moOrchInitData.DefaultBlockchainDataSource.ContractAddress;

                    msAbiWonka        = moOrchInitData.BlockchainEngine.ContractABI;
                    msAbiOrchContract = moOrchInitData.DefaultBlockchainDataSource.ContractABI;

                    moDefaultSource =
                        new WonkaBreSource(moOrchInitData.DefaultBlockchainDataSource.SourceId,
                                           moOrchInitData.DefaultBlockchainDataSource.SenderAddress,
                                           moOrchInitData.DefaultBlockchainDataSource.Password,
                                           moOrchInitData.DefaultBlockchainDataSource.ContractAddress,
                                           moOrchInitData.DefaultBlockchainDataSource.ContractABI,
                                           moOrchInitData.DefaultBlockchainDataSource.MethodName,
                                           moOrchInitData.DefaultBlockchainDataSource.SetterMethodName,
                                           RetrieveValueMethod);

                    #endregion

                    // Here a mapping is created, where each Attribute points to a specific contract and its "accessor" methods
                    // - the class that contains this information (contract, accessors, etc.) is of the WonkaBreSource type
                    foreach (WonkaRefAttr TempAttr in RefEnv.AttrCache)
                    {
                        moAttrSourceMap[TempAttr.AttrName] = moDefaultSource;
                    }
                }

                if (moCustomOpMap == null)
                {
                    // These values indicate the Custom Operator "INVOKE_VAT_LOOKUP" which has been used in the markup - 
                    // its implementation can be found in the method "lookupVATDenominator"
                    msCustomOpId     = "INVOKE_VAT_LOOKUP";
                    msCustomOpMethod = "lookupVATDenominator";

                    // Here a mapping is created, where each Custom Operator points to a specific contract and its "implementation" method
                    // - the class that contains this information (contract, accessors, etc.) is of the WonkaBreSource type    
                    if ((moOrchInitData.BlockchainCustomOpFunctions != null) && (moOrchInitData.BlockchainCustomOpFunctions.Count() > 0))
                        moCustomOpMap = moOrchInitData.BlockchainCustomOpFunctions;
                    else
                    {
                        moCustomOpMap = new Dictionary<string, WonkaBreSource>();

                        // Here a mapping is created, where each Custom Operator points to a specific contract and its "implementation" method
                        // - the class that contains this information (contract, accessors, etc.) is of the WonkaBreSource type
                        WonkaBreSource CustomOpSource =
                            new WonkaBreSource(msCustomOpId, msSenderAddress, msPassword, msOrchContractAddress, msAbiOrchContract, LookupVATDenominator, msCustomOpMethod);

                        moCustomOpMap[msCustomOpId] = CustomOpSource;
                    }
                }

                if (mbInteractWithChain)
                {
                    WonkaEth.Contracts.WonkaRuleTreeRegistry WonkaRegistry =
                        WonkaEth.Contracts.WonkaRuleTreeRegistry.CreateInstance(moWonkaRegistryInit.BlockchainRegistry.ContractSender,
                                                                                moWonkaRegistryInit.BlockchainRegistry.ContractPassword,
                                                                                moWonkaRegistryInit.BlockchainRegistry.ContractAddress,
                                                                                moWonkaRegistryInit.BlockchainRegistry.ContractABI,
                                                                                moWonkaRegistryInit.Web3HttpUrl);

                    RefEnv.Serialize(msSenderAddress, msPassword, msWonkaContractAddress, msAbiWonka, moOrchInitData.Web3HttpUrl);
                }
            }
        }

        private string LookupVATDenominator(string psSaleItemType, string psCountryOfSale, string psDummyVal1, string psDummyVal2)
        {
            if (psSaleItemType == "Widget" && psCountryOfSale == "UK")
                return "5";
            else
                return "1";
        }

        private string RetrieveValueMethod(WonkaBre.RuleTree.WonkaBreSource poTargetSource, string psAttrName)
        {
            string sResult = "";

            if (mbInteractWithChain)
            {
                try
                {
                    var contract = GetContract(poTargetSource);

                    var getRecordValueFunction = contract.GetFunction(poTargetSource.MethodName);

                    sResult = getRecordValueFunction.CallAsync<string>(psAttrName).Result;
                }
                catch (Exception ex)
                {
                    // NOTE: Should throw the exception here?
                }
            }

            return sResult;
        }

        #endregion
    }
}