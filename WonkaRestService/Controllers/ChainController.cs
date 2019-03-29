using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web.Http;

using Nethereum.Web3.Accounts;

using WonkaEth.Extensions;
using WonkaBre.RuleTree;
using WonkaRef;

using WonkaRestService.Cache;
using WonkaRestService.Extensions;
using WonkaRestService.Models;

namespace WonkaRestService.Controllers
{
    public class ChainController : WonkaBaseController
    {
        #region CONSTANTS

        public const string CONST_CHAIN_DATA_KEY_ATTRNUM = "attrnum";
        public const string CONST_CHAIN_DATA_KEY_ATTRVAL = "attrval";
        public const string CONST_CHAIN_DATA_KEY_GETGRV  = "getgrove";
        public const string CONST_CHAIN_DATA_KEY_HASTREE = "hastree";
        public const string CONST_CHAIN_DATA_KEY_RETVAL  = "retval";

        public const string CONST_CHAIN_DATA_KEY_ORCHFLG = "orchflag";
        public const string CONST_CHAIN_DATA_KEY_ISSRCMP = "mapflag";

        public const string CONST_CHAIN_DATA_KEY_RLTREE  = "ruletree";
        public const string CONST_CHAIN_DATA_KEY_ATTRS   = "attributes";        

        #endregion

        /// <summary>
        /// 
        /// This method will provide the information from the Wonka engine on the blockchain, to see what has been
        /// serialized to it thus far.
        /// 
        /// POST: api/chain/type/id
        /// 
        /// TEST PAYLOAD: type=grove&id=123
        /// 
        /// <param name="RuleTreeData">The data needed to instantiate the RuleTree</param>
        /// <returns>Contains the Response with the RuleTree (or an error message if an error occurs)</returns>
        /// </summary>
        public HttpResponseMessage GetChainData(string type, string id = "")
        {
            SvcChainData ChainData = new SvcChainData();

            var response = Request.CreateResponse<SvcChainData>(HttpStatusCode.Created, ChainData);

            string uri = Url.Link("DefaultApi", new { id = "DefaultValue" });

            response.Headers.Location = new Uri(uri);

            try
            {
                if (String.IsNullOrEmpty(type))
                    throw new Exception("ERROR!  No type was provided.");

                Init();

                mbInteractWithChain = false;

                if (type == CONST_CHAIN_DATA_KEY_ATTRNUM)
                    ChainData.AttrNum = RetrieveAttrNum();
                else if (type == CONST_CHAIN_DATA_KEY_ATTRVAL)
                    ChainData.AttrValue = RetrieveAttrValue(id);
                else if (type == CONST_CHAIN_DATA_KEY_RLTREE)
                    ChainData.RuleTreeXml = RetrieveRuleTree(id);
                else if (type == CONST_CHAIN_DATA_KEY_HASTREE)
                    ChainData.Result = RetrieveHasRuleTree(id);
                else if (type == CONST_CHAIN_DATA_KEY_RETVAL)
                    ChainData.AttrValue = RetrieveValueOnRecordViaWonka(id);
                else if (type == CONST_CHAIN_DATA_KEY_ORCHFLG)
                    ChainData.Result = RetrieveOrchFlag();
                else if (type == CONST_CHAIN_DATA_KEY_ISSRCMP)
                    ChainData.Result = RetrieveAttrMapFlag(id);
                else if (type == CONST_CHAIN_DATA_KEY_GETGRV)
                    ChainData.Data = RetrieveGroveData(id);

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

        private bool RetrieveAttrMapFlag(string psAttrName)
        {
            bool bResult = false;

            var mapFlagFunction = GetWonkaContract().GetFunction("getIsSourceMapped");

            bResult = mapFlagFunction.CallAsync<bool>(psAttrName).Result;

            return bResult;
        }

        private uint RetrieveAttrNum()
        {
            uint nAttrNum = 0;

            var getAttrNumFunction = GetWonkaContract().GetFunction("getNumberOfAttributes");

            nAttrNum = getAttrNumFunction.CallAsync<uint>().Result;

            return nAttrNum;
        }

        private string RetrieveAttrValue(string psAttrName)
        {
            string sAttrValue = "";

            if (!String.IsNullOrEmpty(psAttrName) && moAttrSourceMap.ContainsKey(psAttrName))
            {
                var getAttrValueFunction = GetOrchContract().GetFunction(moAttrSourceMap[psAttrName].MethodName);

                sAttrValue = getAttrValueFunction.CallAsync<string>(psAttrName).Result;
            }
            else
                sAttrValue = "ATTRIBUTE NOT VALID";

            return sAttrValue;
        }

        private string RetrieveGroveData(string psGroveId)
        {
            StringBuilder GroveDataBuilder = new StringBuilder();

            if (!String.IsNullOrEmpty(psGroveId) && WonkaServiceCache.GetInstance().GroveRegistryCache.ContainsKey(psGroveId))
            {
                WonkaEth.Contracts.WonkaRuleGrove RuleGrove = new WonkaEth.Contracts.WonkaRuleGrove(psGroveId);

                RuleGrove.PopulateFromRegistry(msAbiWonka);

                GroveDataBuilder.Append(RuleGrove.GroveId);

                if (!String.IsNullOrEmpty(RuleGrove.GroveDescription))
                    GroveDataBuilder.Append("|").Append(RuleGrove.GroveDescription);

                if (RuleGrove.OrderedRuleTrees.Count > 0)
                {
                    GroveDataBuilder.Append("|");

                    bool bPrependComma = false;
                    foreach (WonkaEth.Contracts.WonkaRegistryItem TmpRegTree in RuleGrove.OrderedRuleTrees)
                    {
                        if (bPrependComma)
                            GroveDataBuilder.Append(",");
                        else
                            bPrependComma = true;

                        GroveDataBuilder.Append(TmpRegTree.RuleTreeId);
                    }                        
                }
            }
            else
                GroveDataBuilder.Append("GROVE NOT VALID");

            return GroveDataBuilder.ToString();
        }

        private bool RetrieveHasRuleTree(string psOwnerAddress)
        {
            bool bResult = false;

            var hasRuleTreeFunction = GetWonkaContract().GetFunction("hasRuleTree");

            if (!String.IsNullOrEmpty(psOwnerAddress) && moAttrSourceMap.ContainsKey(psOwnerAddress))
                bResult = hasRuleTreeFunction.CallAsync<bool>(psOwnerAddress).Result;
            else
                bResult = hasRuleTreeFunction.CallAsync<bool>(msSenderAddress).Result;

            return bResult;
        }

        private bool RetrieveOrchFlag()
        {
            bool bResult = false;

            var orchFlagFunction = GetWonkaContract().GetFunction("getOrchestrationMode");

            bResult = orchFlagFunction.CallAsync<bool>().Result;

            return bResult;
        }
        
        private string RetrieveValueOnRecordViaWonka(string psAttrName)
        {
            string sAttrValue = "";

            if (!String.IsNullOrEmpty(psAttrName) && moAttrSourceMap.ContainsKey(psAttrName))
            {
                var retrieveValueFunction = GetWonkaContract().GetFunction("getValueOnRecord");

                sAttrValue = retrieveValueFunction.CallAsync<string>(msSenderAddress, psAttrName).Result;
            }
            else
                sAttrValue = "ATTRIBUTE NOT VALID";

            return sAttrValue;
        }

        private string RetrieveRuleTree(string psRuleTreeId)
        {
            string sRuleTreeXml = "";

            if (!String.IsNullOrEmpty(psRuleTreeId) && moAttrSourceMap.ContainsKey(psRuleTreeId))
            {
                /*
                 * NOTE: Needs more work
                WonkaEth.Contracts.WonkaRegistryItem RegistryItem = new WonkaEth.Contracts.WonkaRegistryItem();

                sRuleTreeXml = RegistryItem.ExportXmlString(moOrchInitData.Web3HttpUrl);
                */
            }
            else
                sRuleTreeXml = "NO RULETREE XML";

            return sRuleTreeXml;
        }

        #endregion

    }
}
