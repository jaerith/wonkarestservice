using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
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
    public class RuleTreeController : ApiController
    {
        #region CONSTANTS

        static private bool mbInteractWithChain   = false;
        static private bool mbCreateDummyTrxState = true;

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

        #endregion

        /// <summary>
        /// 
        /// This method will return information about a RuleTree, if it exists within the cache.
        /// 
        /// GET: api/RuleTree/RuleTreeId
        /// 
        /// <param name="RuleTreeId">The ID of the RuleTree</param>
        /// <returns>Contains the Response with the RuleTree (or an error message if an error occurs)</returns>
        /// </summary>
        public HttpResponseMessage GetRuleTree(string RuleTreeId)
        {
            SvcRuleTree RuleTree = new SvcRuleTree();

            var response = Request.CreateResponse<SvcRuleTree>(HttpStatusCode.OK, RuleTree);

            string uri = Url.Link("DefaultApi", new { id = "DefaultValue" });

            response.Headers.Location = new Uri(uri);

            try
            {
                string sTargetRuleTreeId = 
                    (!String.IsNullOrEmpty(RuleTreeId)) ? RuleTreeId : InvokeController.CONST_RULES_RESOURCE_STREAM;

                WonkaServiceCache ServiceCache = WonkaServiceCache.GetInstance();

                WonkaBreRulesEngine RulesEngine = null;
                if (ServiceCache.RuleTreeCache.ContainsKey(sTargetRuleTreeId))
                {
                    RuleTree.RuleTreeId = RuleTreeId;

                    RulesEngine = ServiceCache.RuleTreeCache[sTargetRuleTreeId];
                    if (RulesEngine != null)
                        RuleTree.RulesEngine = RulesEngine;

                    RuleTree.RuleTreeOriginUrl =
                        ServiceCache.RuleTreeOriginCache.ContainsKey(RuleTreeId) ? ServiceCache.RuleTreeOriginCache[RuleTreeId] : "";

                    ServiceCache.GroveRegistryCache.SetGroveData(RuleTree);
                }

                response = Request.CreateResponse<SvcRuleTree>(HttpStatusCode.Created, RuleTree);
            }
            catch (Exception ex)
            {
                string sErrorMsg = String.Format("ERROR!  Rule Tree web method -> Error Message : {0}",
                                                 ex.ToString());

                if ((ex.InnerException != null) && (ex.InnerException.Message != null))
                    RuleTree.ErrorMessage = ex.InnerException.Message;
                else if (!String.IsNullOrEmpty(ex.Message))
                    RuleTree.ErrorMessage = ex.Message;

                response = Request.CreateResponse<SvcRuleTree>(HttpStatusCode.BadRequest, RuleTree);

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return response;
        }

        /// <summary>
        /// 
        /// This method will provide the information to create a RuleTree, afterwards then inserting it within the cache.
        /// 
        /// POST: api/RuleTree/RuleTreeData
        /// 
        /// TEST PAYLOAD: See WonkaData\RuleTreeController.PostSample.json
        /// 
        /// <param name="RuleTreeData">The data needed to instantiate the RuleTree</param>
        /// <returns>Contains the Response with the RuleTree (or an error message if an error occurs)</returns>
        /// </summary>
        public HttpResponseMessage PostRuleTree(SvcRuleTree RuleTreeData)
        {
            var response = Request.CreateResponse<SvcRuleTree>(HttpStatusCode.Created, RuleTreeData);

            string uri = Url.Link("DefaultApi", new { id = "DefaultValue" });

            response.Headers.Location = new Uri(uri);

            try
            {
                if (RuleTreeData == null)
                    throw new Exception("ERROR!  No rule tree data was provided.");

                if (!RuleTreeData.IsValid())
                    throw new Exception("ERROR!  Invalid rule tree data was provided.");

                WonkaServiceCache ServiceCache = WonkaServiceCache.GetInstance();

                if (ServiceCache.RuleTreeCache.ContainsKey(RuleTreeData.RuleTreeId))
                    throw new Exception("ERROR!  Rule tree with ID already exists.");

                Init();

                WonkaBreRulesEngine NewRulesEngine = null;

                using (WebClient client = new WebClient())
                {
                    string sRuleTreeContents = client.DownloadString(RuleTreeData.RuleTreeOriginUrl);

                     NewRulesEngine =
                        new WonkaBreRulesEngine(new StringBuilder(sRuleTreeContents), moAttrSourceMap, moCustomOpMap, moMetadataSource, false);

                    ServiceCache.RuleTreeCache[RuleTreeData.RuleTreeId]       = NewRulesEngine;
                    ServiceCache.RuleTreeOriginCache[RuleTreeData.RuleTreeId] = RuleTreeData.RuleTreeOriginUrl;

                    if (!ServiceCache.GroveRegistryCache.ContainsKey(RuleTreeData.GroveId))
                        ServiceCache.GroveRegistryCache[RuleTreeData.GroveId] = new SvcGrove(RuleTreeData.GroveId);

                    ServiceCache.GroveRegistryCache[RuleTreeData.GroveId].RuleTreeMembers.Add(RuleTreeData.RuleTreeId);

                    RuleTreeData.RulesEngine = NewRulesEngine;
                }

                response = Request.CreateResponse<SvcRuleTree>(HttpStatusCode.Created, RuleTreeData);
            }
            catch (Exception ex)
            {
                string sErrorMsg = String.Format("ERROR!  Rule Tree web method -> Error Message : {0}",
                                                 ex.ToString());

                if ((ex.InnerException != null) && (ex.InnerException.Message != null))
                    RuleTreeData.ErrorMessage = ex.InnerException.Message;
                else if (!String.IsNullOrEmpty(ex.Message))
                    RuleTreeData.ErrorMessage = ex.Message;

                response = Request.CreateResponse<SvcRuleTree>(HttpStatusCode.BadRequest, RuleTreeData);

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return response;
        }

        /*
        // PUT: api/RuleTree/5
        public void Put(int id, [FromBody]string value)
        {
        }
        */

        /// <summary>
        /// 
        /// This method will remove a RuleTree identified by the RuleTreeId.
        /// 
        /// DELETE: api/TrxState/RuleTreeId
        /// 
        /// <param name="RuleTreeId">The ID of the RuleTree</param>
        /// <returns>Contains the Response with the RuleTreeId</returns>
        /// </summary>
        public HttpResponseMessage DeleteRuleTree(string RuleTreeId)
        {
            SvcRuleTree RuleTree = new SvcRuleTree();

            var response = Request.CreateResponse<SvcRuleTree>(HttpStatusCode.OK, RuleTree);

            string uri = Url.Link("DefaultApi", new { id = "DefaultValue" });

            response.Headers.Location = new Uri(uri);

            try
            {
                string sTargetRuleTreeId =
                    (!String.IsNullOrEmpty(RuleTreeId)) ? RuleTreeId : InvokeController.CONST_RULES_RESOURCE_STREAM;

                WonkaServiceCache ServiceCache = WonkaServiceCache.GetInstance();

                if (ServiceCache.RuleTreeCache.ContainsKey(sTargetRuleTreeId))
                {
                    RuleTree.RuleTreeId = RuleTreeId;

                    ServiceCache.RuleTreeCache.Remove(sTargetRuleTreeId);

                    ServiceCache.RuleTreeOriginCache.Remove(sTargetRuleTreeId);

                    foreach (string sTmpGroveId in ServiceCache.GroveRegistryCache.Keys)
                        ServiceCache.GroveRegistryCache[sTmpGroveId].RuleTreeMembers.Remove(sTargetRuleTreeId);
                }
                else
                    throw new Exception("ERROR!  RuleTree (" + sTargetRuleTreeId + ") does not exist.");

                response = Request.CreateResponse<SvcRuleTree>(HttpStatusCode.Created, RuleTree);
            }
            catch (Exception ex)
            {
                string sErrorMsg = String.Format("ERROR!  Rule Tree web method -> Error Message : {0}",
                                                 ex.ToString());

                if ((ex.InnerException != null) && (ex.InnerException.Message != null))
                    RuleTree.ErrorMessage = ex.InnerException.Message;
                else if (!String.IsNullOrEmpty(ex.Message))
                    RuleTree.ErrorMessage = ex.Message;

                response = Request.CreateResponse<SvcRuleTree>(HttpStatusCode.BadRequest, RuleTree);

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return response;
        }

        #region Methods

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

        private string DeployOrchestrationContract()
        {
            string sOrchestrationContractAddress = "";

            // NOTE: Yet to be implemented

            return sOrchestrationContractAddress;
        }

        private void Init()
        {
            var TmpAssembly = Assembly.GetExecutingAssembly();

            if (moMetadataSource == null)
                moMetadataSource = new WonkaData.WonkaMetadataVATSource();

            // Using the metadata source, we create an instance of a defined data domain
            WonkaRefEnvironment RefEnv =
                WonkaRefEnvironment.CreateInstance(false, moMetadataSource);

            if (moOrchInitData == null)
            {
                var DelegateMap =
                    new Dictionary<string, WonkaBre.Readers.WonkaBreXmlReader.ExecuteCustomOperator>();

                DelegateMap["lookupVATDenominator"] = InvokeController.LookupVATDenominator;

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
                    msPassword = moOrchInitData.BlockchainEngine.Password;

                    if (moOrchInitData.BlockchainEngine.ContractAddress == null)
                        msWonkaContractAddress = InvokeController.DeployWonkaContract();
                    else
                        msWonkaContractAddress = moOrchInitData.BlockchainEngine.ContractAddress;

                    if (moOrchInitData.DefaultBlockchainDataSource.ContractAddress == null)
                        msOrchContractAddress = DeployOrchestrationContract();
                    else
                        msOrchContractAddress = moOrchInitData.DefaultBlockchainDataSource.ContractAddress;

                    msAbiWonka = moOrchInitData.BlockchainEngine.ContractABI;
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
                            new WonkaBreSource(msCustomOpId, msSenderAddress, msPassword, msOrchContractAddress, msAbiOrchContract, InvokeController.LookupVATDenominator, msCustomOpMethod);

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
