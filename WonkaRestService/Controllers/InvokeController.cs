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

using WonkaRestService.Extensions;

namespace WonkaRestService.Controllers
{
    public class InvokeController : ApiController
    {
        static private string msSenderAddress        = "";
        static private string msPassword             = "";
        static private string msWonkaContractAddress = "";
        static private string msOrchContractAddress  = "";
        static private string msAbiWonka             = "";
        static private string msAbiOrchContract      = "";
        static private string msRulesContents        = "";

        static private WonkaBreSource       moDefaultSource  = null;
        static private IMetadataRetrievable moMetadataSource = null;

        static private Dictionary<string, WonkaBreSource> moAttrSourceMap = new Dictionary<string, WonkaBreSource>();
        static private Dictionary<string, WonkaBreSource> moCustomOpMap   = new Dictionary<string, WonkaBreSource>();

        static private WonkaEth.Orchestration.Init.OrchestrationInitData moOrchInitData      = null;
        static private WonkaEth.Init.WonkaEthRegistryInitialization      moWonkaRegistryInit = null;

        // GET: api/Invoke
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Invoke/5
        public string Get(int id)
        {
            return "value";
        }

        /*
        // POST: api/Invoke
        public void Post([FromBody]string value)
        {
        }
        */

        public HttpResponseMessage Post([FromBody]IDictionary<string, string> poRecord)
        {
            Hashtable poTrxRecord = poRecord.TransformToTrxRecord();

            var response = Request.CreateResponse<Hashtable>(HttpStatusCode.Created, poTrxRecord);

            string uri = Url.Link("DefaultApi", new { id = "DefaultValue" });

            response.Headers.Location = new Uri(uri);

            try
            {
                Init();

                if (poRecord != null)
                {

                    WonkaProduct WonkaRecord = poRecord.TransformToWonkaProduct();

                    ExecuteDotNet(WonkaRecord);
                }

                response = Request.CreateResponse<Hashtable>(HttpStatusCode.Created, poTrxRecord);
            }
            catch (Exception ex)
            {
                string sErrorMsg = String.Format("ERROR!  Invoke web method -> Error Message : {0}",
                                                 ex.ToString());

                /*
                if ((ex.InnerException != null) && (ex.InnerException.Message != null))
                    item.ErrorMessage = ex.InnerException.Message;
                else if (!String.IsNullOrEmpty(ex.Message))
                    item.ErrorMessage = ex.Message;
                */

                response = Request.CreateResponse<Hashtable>(HttpStatusCode.BadRequest, poTrxRecord);

                // Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return response;

        }

        /*
        // PUT: api/Invoke/5
        public void Put(int id, [FromBody]string value)
        {
        }
        */

        // DELETE: api/Invoke/5
        public void Delete(int id)
        {
        }

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

        private void Init()
        {
            var TmpAssembly = Assembly.GetExecutingAssembly();

            // Using the metadata source, we create an instance of a defined data domain
            WonkaRefEnvironment RefEnv =
                WonkaRefEnvironment.CreateInstance(false, moMetadataSource);

            if (String.IsNullOrEmpty(msRulesContents))
            {
                // Read the XML markup that lists the business rules (i.e., the RuleTree)
                using (var RulesReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaRestService.WonkaData.VATCalculationExample.xml")))
                {
                    msRulesContents = RulesReader.ReadToEnd();
                }
            }

            if (moMetadataSource == null)
                moMetadataSource = new WonkaData.WonkaMetadataVATSource();

            if (moOrchInitData == null)
            {
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
                    moOrchInitData = WonkaInit.TransformIntoOrchestrationInit(moMetadataSource);
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
                    // Here a mapping is created, where each Custom Operator points to a specific contract and its "implementation" method
                    // - the class that contains this information (contract, accessors, etc.) is of the WonkaBreSource type    
                    moCustomOpMap = moOrchInitData.BlockchainCustomOpFunctions;
                }

                /*
                WonkaEth.Contracts.WonkaRuleTreeRegistry WonkaRegistry =
                    WonkaEth.Contracts.WonkaRuleTreeRegistry.CreateInstance(moWonkaRegistryInit.BlockchainRegistry.ContractSender, 
                                                                            moWonkaRegistryInit.BlockchainRegistry.ContractPassword,
                                                                            moWonkaRegistryInit.BlockchainRegistry.ContractAddress, 
                                                                            moWonkaRegistryInit.BlockchainRegistry.ContractABI,
                                                                            moWonkaRegistryInit.Web3HttpUrl);

                RefEnv.Serialize(msSenderAddress, msPassword, msWonkaContractAddress, msAbiWonka, moOrchInitData.Web3HttpUrl);
                 */

            }
        }

        private string RetrieveValueMethod(WonkaBre.RuleTree.WonkaBreSource poTargetSource, string psAttrName)
        {
            var contract = GetContract(poTargetSource);

            var getRecordValueFunction = contract.GetFunction(poTargetSource.MethodName);

            var result = getRecordValueFunction.CallAsync<string>(psAttrName).Result;

            return result;
        }

        private void ExecuteDotNet(WonkaProduct WonkaRecord)
        {
            // Using the metadata source, we create an instance of a defined data domain
            WonkaRefEnvironment RefEnv =
                WonkaRefEnvironment.CreateInstance(false, moMetadataSource);

            WonkaRefAttr VATAmountForHMRCAttr = RefEnv.GetAttributeByAttrName("NewVATAmountForHMRC");

            // Creating an instance of the rules engine using our rules and the metadata
            WonkaBreRulesEngine RulesEngine =
                    new WonkaBreRulesEngine(new StringBuilder(msRulesContents), moMetadataSource);

            // Check that the data has been populated correctly on the "new" record
            string sVATAmountForHRMC = WonkaRecord.GetAttributeValue(VATAmountForHMRCAttr);

            // Since the rules can reference values from different records (like O.Price for the existing
            // record's price and N.Price for the new record's price), we need to provide the delegate
            // that can pull the existing (i.e., old) record using a key
            RulesEngine.GetCurrentProductDelegate = GetOldProduct;

            /*
             * NOTE: Will be put back later
             * 
            // Validate the new record using our rules engine and its initialized RuleTree
            WonkaBre.Reporting.WonkaBreRuleTreeReport Report = RulesEngine.Validate(NewProduct);

            // Now retrieve the AccountStatus value and see if the rules have altered it (which should
            // not be the case)
            string sStatusValueAfter = GetAttributeValue(NewProduct, AccountStsAttr);

            if (Report.GetRuleSetFailureCount() > 0)
            {
                throw new Exception("Oh heavens to Betsy! Something bad happened!");
            }
            */
        }

        #endregion
    }
}