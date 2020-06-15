using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web.Http;

using Nethereum.Web3.Accounts;

using Wonka.BizRulesEngine;
using Wonka.Eth.Extensions;
using Wonka.BizRulesEngine.RuleTree;
using Wonka.Product;
using Wonka.MetaData;

namespace WonkaRestService.Controllers
{
    public class WonkaBaseController : ApiController
    {
        #region CONSTANTS

        public const string CONST_RULES_RESOURCE_ID     = "VATCalculationExample";
        public const string CONST_RULES_RESOURCE_STREAM = "WonkaRestService.WonkaData.VATCalculationExample.xml";

        #endregion 

        static protected string msRuleMasterAddress    = "";
        static protected string msSenderAddress        = "";
        static protected string msPassword             = "";
        static protected string msWonkaContractAddress = "";
        static protected string msOrchContractAddress  = "";
        static protected string msAbiWonka             = "";
        static protected string msAbiOrchContract      = "";
        static protected string msVATCalcRulesContents = "";

        static protected string msCustomOpId     = "";
        static protected string msCustomOpMethod = "";

        static protected WonkaBizSource       moDefaultSource  = null;
        static protected IMetadataRetrievable moMetadataSource = null;

        static protected Dictionary<string, WonkaBizSource> moAttrSourceMap = new Dictionary<string, WonkaBizSource>();
        static protected Dictionary<string, WonkaBizSource> moCustomOpMap   = null;

        static protected Wonka.Eth.Orchestration.Init.OrchestrationInitData moOrchInitData      = null;
        static protected Wonka.Eth.Init.WonkaEthRegistryInitialization      moWonkaRegistryInit = null;

        protected bool mbInteractWithChain = true;

        public static uint ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;

            var dEpochTime = Math.Floor(diff.TotalSeconds);

            return Convert.ToUInt32(dEpochTime);
        }

        protected Nethereum.Contracts.Contract GetContract(WonkaBizSource TargetSource)
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

        public static Nethereum.Contracts.Contract GetAltContract(string psSenderAddr, string psPassword, string psContractAbi, string psContractAddr, string psWeb3HttpUrl = "")
        {
            var account = new Account(psPassword);

            Nethereum.Web3.Web3 web3 = null;
            if (!String.IsNullOrEmpty(psWeb3HttpUrl))
                web3 = new Nethereum.Web3.Web3(account, psWeb3HttpUrl);
            else
                web3 = new Nethereum.Web3.Web3(account);

            var contract = web3.Eth.GetContract(psContractAbi, psContractAddr);

            return contract;
        }

        protected Nethereum.Contracts.Contract GetOrchContract()
        {
            var account = new Account(msPassword);

            Nethereum.Web3.Web3 web3 = null;
            if (!String.IsNullOrEmpty(moOrchInitData.Web3HttpUrl))
                web3 = new Nethereum.Web3.Web3(account, moOrchInitData.Web3HttpUrl);
            else
                web3 = new Nethereum.Web3.Web3(account);

            var contract = web3.Eth.GetContract(msAbiOrchContract, msOrchContractAddress);

            return contract;
        }

        protected Nethereum.Contracts.Contract GetRegistryContract()
        {
            var account = new Account(moWonkaRegistryInit.BlockchainRegistry.ContractPassword);

            Nethereum.Web3.Web3 web3 = null;
            if (!String.IsNullOrEmpty(moOrchInitData.Web3HttpUrl))
                web3 = new Nethereum.Web3.Web3(account, moWonkaRegistryInit.Web3HttpUrl);
            else
                web3 = new Nethereum.Web3.Web3(account);

            var contract = 
                web3.Eth.GetContract(moWonkaRegistryInit.BlockchainRegistry.ContractABI, moWonkaRegistryInit.BlockchainRegistry.ContractAddress);

            return contract;
        }

        public Nethereum.Web3.Web3 GetWeb3()
        {
            var account = new Account(msPassword);

            Nethereum.Web3.Web3 web3 = null;

            if (!String.IsNullOrEmpty(moOrchInitData.Web3HttpUrl))
                web3 = new Nethereum.Web3.Web3(account, moOrchInitData.Web3HttpUrl);
            else
                web3 = new Nethereum.Web3.Web3(account);

            return web3;
        }

        protected Nethereum.Contracts.Contract GetWonkaContract()
        {
            var account = new Account(msPassword);

            Nethereum.Web3.Web3 web3 = null;
            if (!String.IsNullOrEmpty(moOrchInitData.Web3HttpUrl))
                web3 = new Nethereum.Web3.Web3(account, moOrchInitData.Web3HttpUrl);
            else
                web3 = new Nethereum.Web3.Web3(account);

            var contract = web3.Eth.GetContract(msAbiWonka, msWonkaContractAddress);

            return contract;
        }

        protected string DeployOrchestrationContract()
        {
            string sOrchestrationContractAddress = "";

            // NOTE: Yet to be implemented

            return sOrchestrationContractAddress;
        }

        protected void Init()
        {
            var TmpAssembly = Assembly.GetExecutingAssembly();

            if (moMetadataSource == null)
                moMetadataSource = new WonkaData.WonkaMetadataVATSource();

            // Using the metadata source, we create an instance of a defined data domain
            WonkaRefEnvironment RefEnv =
                WonkaRefEnvironment.CreateInstance(false, moMetadataSource);

            if (String.IsNullOrEmpty(msVATCalcRulesContents))
            {
                // Read the XML markup that lists the business rules (i.e., the RuleTree)
                using (var RulesReader = new StreamReader(TmpAssembly.GetManifestResourceStream(CONST_RULES_RESOURCE_STREAM)))
                {
                    msVATCalcRulesContents = RulesReader.ReadToEnd();
                }
            }

            if (moOrchInitData == null)
            {
                var DelegateMap =
                    new Dictionary<string, Wonka.BizRulesEngine.Readers.WonkaBizRulesXmlReader.ExecuteCustomOperator>();

                DelegateMap["lookupVATDenominator"] = InvokeController.LookupVATDenominator;

                // Read the configuration file that contains all the initialization details regarding the rules engine 
                // (like addresses of contracts, senders, passwords, etc.)
                using (var XmlReader = new System.IO.StreamReader(TmpAssembly.GetManifestResourceStream("WonkaRestService.WonkaData.VATCalculationExample.init.xml")))
                {
                    string sInitXml = XmlReader.ReadToEnd();

                    // We deserialize/parse the contents of the config file
                    System.Xml.Serialization.XmlSerializer WonkaEthSerializer =
                        new System.Xml.Serialization.XmlSerializer(typeof(Wonka.Eth.Init.WonkaEthInitialization),
                                                                   new System.Xml.Serialization.XmlRootAttribute("Wonka.EthInitialization"));

                    Wonka.Eth.Init.WonkaEthInitialization WonkaInit =
                        WonkaEthSerializer.Deserialize(new System.IO.StringReader(sInitXml)) as Wonka.Eth.Init.WonkaEthInitialization;

                    // Here, any embeddeded resources mentioned in the config file (instead of simple file URLs) are accessed here
                    WonkaInit.RetrieveEmbeddedResources(TmpAssembly);

                    // The initialization data is transformed into a structure used by the Wonka.Eth namespace
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
                            new System.Xml.Serialization.XmlSerializer(typeof(Wonka.Eth.Init.WonkaEthRegistryInitialization),
                                                                       new System.Xml.Serialization.XmlRootAttribute("Wonka.EthRegistryInitialization"));

                        moWonkaRegistryInit =
                            WonkaEthSerializer.Deserialize(new System.IO.StringReader(sInitRegistryXml)) as Wonka.Eth.Init.WonkaEthRegistryInitialization;

                        // Here, any embeddeded resources mentioned in the config file (instead of simple file URLs) are accessed here                
                        moWonkaRegistryInit.RetrieveEmbeddedResources(TmpAssembly);
                    }
                }

                if (String.IsNullOrEmpty(msSenderAddress))
                {
                    #region Set Class Member Variables
                    msRuleMasterAddress = moOrchInitData.BlockchainEngineOwner;
                    msSenderAddress     = moOrchInitData.BlockchainEngine.SenderAddress;
                    msPassword          = moOrchInitData.BlockchainEngine.Password;

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
                        new WonkaBizSource(moOrchInitData.DefaultBlockchainDataSource.SourceId,
                                           moOrchInitData.DefaultBlockchainDataSource.SenderAddress,
                                           moOrchInitData.DefaultBlockchainDataSource.Password,
                                           moOrchInitData.DefaultBlockchainDataSource.ContractAddress,
                                           moOrchInitData.DefaultBlockchainDataSource.ContractABI,
                                           moOrchInitData.DefaultBlockchainDataSource.MethodName,
                                           moOrchInitData.DefaultBlockchainDataSource.SetterMethodName,
                                           RetrieveValueMethod);

                    #endregion

                    // Here a mapping is created, where each Attribute points to a specific contract and its "accessor" methods
                    // - the class that contains this information (contract, accessors, etc.) is of the WonkaBizSource type
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
                    // - the class that contains this information (contract, accessors, etc.) is of the WonkaBizSource type    
                    if ((moOrchInitData.BlockchainCustomOpFunctions != null) && (moOrchInitData.BlockchainCustomOpFunctions.Count() > 0))
                        moCustomOpMap = moOrchInitData.BlockchainCustomOpFunctions;
                    else
                    {
                        moCustomOpMap = new Dictionary<string, WonkaBizSource>();

                        // Here a mapping is created, where each Custom Operator points to a specific contract and its "implementation" method
                        // - the class that contains this information (contract, accessors, etc.) is of the WonkaBizSource type
                        WonkaBizSource CustomOpSource =
                            new WonkaBizSource(msCustomOpId, msSenderAddress, msPassword, msOrchContractAddress, msAbiOrchContract, InvokeController.LookupVATDenominator, msCustomOpMethod);

                        moCustomOpMap[msCustomOpId] = CustomOpSource;
                    }
                }

                if (mbInteractWithChain)
                {
                    Wonka.Eth.Contracts.WonkaRuleTreeRegistry WonkaRegistry =
                        Wonka.Eth.Contracts.WonkaRuleTreeRegistry.CreateInstance(moWonkaRegistryInit.BlockchainRegistry.ContractSender,
                                                                                moWonkaRegistryInit.BlockchainRegistry.ContractPassword,
                                                                                moWonkaRegistryInit.BlockchainRegistry.ContractAddress,
                                                                                moWonkaRegistryInit.BlockchainRegistry.ContractABI,
                                                                                moWonkaRegistryInit.Web3HttpUrl);

                    SerializeRefEnv();
                }
            }
        }

        protected string RetrieveValueMethod(Wonka.BizRulesEngine.RuleTree.WonkaBizSource poTargetSource, string psAttrName)
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

        protected void SerializeRefEnv()
        {
            WonkaRefEnvironment RefEnv = WonkaRefEnvironment.CreateInstance(false, moMetadataSource);

            RefEnv.Serialize(msRuleMasterAddress, msPassword, msSenderAddress, msWonkaContractAddress, msAbiWonka, moOrchInitData.Web3HttpUrl);
        }

    }
}