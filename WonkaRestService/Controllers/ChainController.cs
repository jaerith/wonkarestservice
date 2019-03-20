using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;

using Nethereum.Web3.Accounts;

using WonkaEth.Extensions;
using WonkaBre.RuleTree;
using WonkaRef;

using WonkaRestService.Models;

namespace WonkaRestService.Controllers
{
    public class ChainController : ApiController
    {
        #region CONSTANTS

        public const string CONST_RULES_RESOURCE_ID     = "VATCalculationExample";
        public const string CONST_RULES_RESOURCE_STREAM = "WonkaRestService.WonkaData.VATCalculationExample.xml";

        public const string CONST_RECORD_KEY_RULE_TREE_ID = "RuleTreeId";

        public const string CONST_CHAIN_DATA_KEY_ATTRNUM = "attrnum";
        public const string CONST_CHAIN_DATA_KEY_ATTRS   = "attributes";
        public const string CONST_CHAIN_DATA_KEY_RLTR    = "ruletrees";

        #endregion

        static private bool mbInteractWithChain = false;

        static private string msSenderAddress        = "";
        static private string msPassword             = "";
        static private string msWonkaContractAddress = "";
        static private string msOrchContractAddress  = "";
        static private string msAbiWonka             = "";
        static private string msAbiOrchContract      = "";

        static private string msCustomOpId     = "";
        static private string msCustomOpMethod = "";

        static private WonkaBreSource       moDefaultSource  = null;
        static private IMetadataRetrievable moMetadataSource = null;

        static private Dictionary<string, WonkaBreSource> moAttrSourceMap = new Dictionary<string, WonkaBreSource>();
        static private Dictionary<string, WonkaBreSource> moCustomOpMap   = null;

        static private WonkaEth.Orchestration.Init.OrchestrationInitData moOrchInitData      = null;
        static private WonkaEth.Init.WonkaEthRegistryInitialization      moWonkaRegistryInit = null;

        /// <summary>
        /// 
        /// This method will provide the information from the Wonka engine on the blockchain, to see what has been
        /// serialized to it thus far.
        /// 
        /// POST: api/chain/list
        /// 
        /// TEST PAYLOAD: attrnum,attributes,ruletrees
        /// 
        /// <param name="RuleTreeData">The data needed to instantiate the RuleTree</param>
        /// <returns>Contains the Response with the RuleTree (or an error message if an error occurs)</returns>
        /// </summary>
        public HttpResponseMessage GetChainData(string list)
        {
            SvcChainData ChainData = new SvcChainData();

            var response = Request.CreateResponse<SvcChainData>(HttpStatusCode.Created, ChainData);

            string uri = Url.Link("DefaultApi", new { id = "DefaultValue" });

            response.Headers.Location = new Uri(uri);

            try
            {
                if (String.IsNullOrEmpty(list))
                    throw new Exception("ERROR!  No item list of chain data was provided.");

                string[] asDataList = new string[0];
                if (list.Contains(","))
                    asDataList = list.Split(new char[1] { ',' });
                else
                    asDataList = new string[1] { list };

                Init();

                if (asDataList.Contains(CONST_CHAIN_DATA_KEY_ATTRNUM))
                    ChainData.AttrNum = RetrieveAttrNum();

                response = Request.CreateResponse<SvcChainData>(HttpStatusCode.Created, ChainData);
            }
            catch (Exception ex)
            {
                string sErrorMsg = String.Format("ERROR!  Chain web method -> Error Message : {0}",
                                                 ex.ToString());

                if ((ex.InnerException != null) && (ex.InnerException.Message != null))
                    ChainData.ErrorMessage = ex.InnerException.Message;
                else if (!String.IsNullOrEmpty(ex.Message))
                    ChainData.ErrorMessage = ex.Message;

                // Only here temporarily, useful for debugging purposes
                string sCallString = String.Format("Problem when calling {0}/{1}/{2} -> ", msSenderAddress, msPassword, msOrchContractAddress);
                ChainData.StackTraceMessage = sCallString + ex.StackTrace;

                response = Request.CreateResponse<SvcChainData>(HttpStatusCode.BadRequest, ChainData);

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return response;
        }

        #region Private

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
            }
        }

        private uint RetrieveAttrNum()
        {
            uint nAttrNum = 0;

            var account = new Account(msPassword);

            Nethereum.Web3.Web3 web3 = null;
            if (!String.IsNullOrEmpty(moOrchInitData.Web3HttpUrl))
                web3 = new Nethereum.Web3.Web3(account, moOrchInitData.Web3HttpUrl);
            else
                web3 = new Nethereum.Web3.Web3(account);

            var contract = web3.Eth.GetContract(msAbiWonka, msWonkaContractAddress);

            var getAttrNumFunction = contract.GetFunction("getNumberOfAttributes");

            nAttrNum = getAttrNumFunction.CallAsync<uint>().Result;

            return nAttrNum;
        }

        // Just a placeholder function
        private string RetrieveValueMethod(WonkaBre.RuleTree.WonkaBreSource poTargetSource, string psAttrName)
        {
            return "";
        }

        #endregion

    }
}
