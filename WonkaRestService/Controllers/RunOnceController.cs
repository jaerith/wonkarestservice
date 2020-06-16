using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

using Wonka.BizRulesEngine;
using Wonka.BizRulesEngine.RuleTree;
using Wonka.BizRulesEngine.Samples;
using Wonka.Eth.Init;
using Wonka.MetaData;
using Wonka.Product;

using WonkaRestService.Cache;
using WonkaRestService.Models;

namespace WonkaRestService.Controllers
{
    public class RunOnceController : ApiController
    {
        /**
         ** NOTE: Not currently relevant
         **
        // GET: api/RunOnce
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
         **/

        // GET: api/RunOnce/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/RunOnce
        public HttpResponseMessage Post([FromBody] SvcRunOnce poRunOnce)
        {
            var response = Request.CreateResponse<SvcRunOnce>(HttpStatusCode.Created, poRunOnce);

            string uri = Url.Link("DefaultApi", new { id = "DefaultValue" });

            response.Headers.Location = new Uri(uri);

            try
            {
                if (poRunOnce == null)
                    throw new Exception("ERROR!  No data provided.");

                Execute(poRunOnce);

                response = Request.CreateResponse<SvcRunOnce>(HttpStatusCode.Created, poRunOnce);
            }
            catch (Exception ex)
            {
                string sErrorMsg = String.Format("ERROR! RunOncePost web method -> Error Message : {0}",
                                                 ex.ToString());

                if ((ex.InnerException != null) && (ex.InnerException.Message != null))
                    poRunOnce.ErrorMessage = ex.InnerException.Message;
                else if (!String.IsNullOrEmpty(ex.Message))
                    poRunOnce.ErrorMessage = ex.Message;

                response = Request.CreateResponse<SvcRunOnce>(HttpStatusCode.BadRequest, poRunOnce);

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return response;
        }

        /**
         ** NOTE: Not currently relevant
         **
        // PUT: api/RunOnce/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/RunOnce/5
        public void Delete(int id)
        {
        }
         **/

        // NOTE: This method should be executed in another app domain?
        private void Execute(SvcRunOnce poRunOnce)
        {
            // Using the metadata source, we create an instance of a defined data domain
            // (NOTE: We should create a new import method)
            IMetadataRetrievable AttrMetadata = new WonkaBreMetadataTestSource();

            WonkaRefEnvironment RefEnv =
                WonkaRefEnvironment.CreateInstance(false, AttrMetadata);

            var sWeb3Url       = poRunOnce.Web3Url;
            var sSenderAddress = poRunOnce.Account;
            var sPassword      = poRunOnce.Password;
            var sERC20Address  = poRunOnce.ERC20ContractAddress;
            var sERC721Address = poRunOnce.ERC721ContractAddress;

            Dictionary<string, WonkaBizSource> SourceMap = new Dictionary<string, WonkaBizSource>();

            /*
            WonkaBizSource DefaultSource = 
                new WonkaBizSource(sContractSourceId, msSenderAddress, msPassword, sContractAddress, sContractAbi, sOrchGetterMethod, sOrchSetterMethod, RetrieveValueMethod);
            */

            WonkaBizSource DefaultSource =
                new WonkaBizSource("", "", "", "", null);

            // Here a mapping is created, where each Attribute points to a specific contract and its "accessor" methods
            // - the class that contains this information (contract, accessors, etc.) is of the WonkaBreSource type
            foreach (WonkaRefAttr TempAttr in RefEnv.AttrCache)
            {
                SourceMap[TempAttr.AttrName] = DefaultSource;
            }

            var WonkaEthEngInit =
                new WonkaEthEngineInitialization()
                {
                    EthSenderAddress = sSenderAddress,
                    EthPassword = sPassword,
                    ERC20ContractAddress = sERC20Address,
                    ERC721ContractAddress = sERC721Address,
                    Web3HttpUrl = sWeb3Url
                };

            string sRuleTreeXml = "";
            using (var client = new System.Net.Http.HttpClient())
            {
                sRuleTreeXml = client.GetStringAsync(poRunOnce.BizRulesUrl).Result;
            }

            // Creating an instance of the rules engine using our rules and the metadata
            WonkaBizRulesEngine RulesEngine =
                new WonkaEthRulesEngine(new StringBuilder(sRuleTreeXml), SourceMap, WonkaEthEngInit, AttrMetadata);

            // Gets a predefined data record that will be our analog for new data coming into the system
            WonkaProduct NewProduct = new WonkaProduct();
            foreach (string sTmpAttrName in poRunOnce.Transaction.RecordData.Keys)
            {
                WonkaRefAttr TargetAttr = RefEnv.GetAttributeByAttrName(sTmpAttrName);

                NewProduct.SetAttribute(TargetAttr, (string) poRunOnce.Transaction.RecordData[sTmpAttrName]);
            }

            // Validate the new record using our rules engine and its initialized RuleTree
            poRunOnce.Transaction.RuleTreeReport = RulesEngine.Validate(NewProduct);
        }
    }
}
