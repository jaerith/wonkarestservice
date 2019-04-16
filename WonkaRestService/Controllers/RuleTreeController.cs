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
    public class RuleTreeController : WonkaBaseController
    {
        /// <summary>
        /// 
        /// This method will return information about a RuleTree, if it exists within the cache.
        /// 
        /// GET: api/RuleTree/RuleTreeId
        /// 
        /// <param name="RuleTreeId">The ID of the RuleTree</param>
        /// <returns>Contains the Response with the RuleTree (or an error message if an error occurs)</returns>
        /// </summary>
        public HttpResponseMessage GetRuleTree(string RuleTreeId, bool ExportMode = false)
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

                    // In the case that the caller wants to export the info about the RuleTree 
                    RulesEngine = ServiceCache.RuleTreeCache[sTargetRuleTreeId];
                    if (RulesEngine != null)
                        RuleTree.RulesEngine = RulesEngine;

                    RuleTree.RuleTreeOriginUrl =
                        ServiceCache.RuleTreeOriginCache.ContainsKey(RuleTreeId) ? ServiceCache.RuleTreeOriginCache[RuleTreeId].RuleTreeOriginUrl : "";

                    // NOTE: This section may not be the best solution
                    // ServiceCache.GroveRegistryCache.SetGroveData(RuleTree);
                }


                // In the case that the caller wants to export the RuleTree (in XML form) from the chain
                if (ExportMode)
                {
                    var ExportedXml = ExportRuleTreeXml(RuleTree.RulesEngine);

                    if (!String.IsNullOrEmpty(ExportedXml))
                        response = Request.CreateResponse<string>(HttpStatusCode.OK, ExportedXml);
                    else
                    {
                        RuleTree.ErrorMessage =
                            "ERROR!  No registry item found for RuleTree(" + sTargetRuleTreeId + ")";
                    }
                }
                // In the case that the user just wants the general data about the RuleTree
                else 
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

            bool bSerialized = false;

            try
            {
                Init();

                if (RuleTreeData == null)
                    throw new Exception("ERROR!  No rule tree data was provided.");

                if (!RuleTreeData.IsValid())
                    throw new Exception("ERROR!  Invalid rule tree data was provided.");

                WonkaServiceCache ServiceCache = WonkaServiceCache.GetInstance();

                if (ServiceCache.RuleTreeCache.ContainsKey(RuleTreeData.RuleTreeId))
                    throw new Exception("ERROR!  Rule tree with ID already exists.");

                if (!String.IsNullOrEmpty(RuleTreeData.OwnerName))
                {
                    if (!ServiceCache.TreeOwnerCache.ContainsKey(RuleTreeData.OwnerName))
                        throw new Exception("ERROR!  Owner Name(" + RuleTreeData.OwnerName + ") does not exist in the cache.");

                    bool bAnotherTreeWithOwner =
                        ServiceCache.RuleTreeOriginCache.Values.Any(x => x.OwnerName == RuleTreeData.OwnerName);

                    if (bAnotherTreeWithOwner)
                        throw new Exception("ERROR!  Owner Name(" + RuleTreeData.OwnerName + ") already owns a tree and can only have one tree.");
                }

                WonkaBreRulesEngine NewRulesEngine = null;

                using (WebClient client = new WebClient())
                {
                    string sRuleTreeContents = client.DownloadString(RuleTreeData.RuleTreeOriginUrl);

                    bool   bAddToRegistry = false;
                    bool   bAddToGrove    = false;
                    string sOrigGroveId   = RuleTreeData.GroveId;

                    if (!String.IsNullOrEmpty(RuleTreeData.GroveId))
                        bAddToRegistry = true;

                    NewRulesEngine =
                        new WonkaBreRulesEngine(new StringBuilder(sRuleTreeContents), moAttrSourceMap, moCustomOpMap, moMetadataSource, bAddToRegistry);

                    NewRulesEngine.GroveId    = RuleTreeData.GroveId;
                    NewRulesEngine.GroveIndex = RuleTreeData.GroveIndex;

                    if (moAttrSourceMap.Count > 0)
                        NewRulesEngine.DefaultSource = moAttrSourceMap.Values.Where(x => !String.IsNullOrEmpty(x.SourceId)).FirstOrDefault().SourceId;

                    NewRulesEngine.RegistrationId = RuleTreeData.RuleTreeId;

                    NewRulesEngine.RuleTreeRoot.Description = "Root" + RuleTreeData.RuleTreeId;

                    ServiceCache.RuleTreeCache[RuleTreeData.RuleTreeId]       = NewRulesEngine;
                    ServiceCache.RuleTreeOriginCache[RuleTreeData.RuleTreeId] = RuleTreeData;

                    if (!String.IsNullOrEmpty(RuleTreeData.GroveId))
                    {
                        if (!ServiceCache.GroveRegistryCache.ContainsKey(RuleTreeData.GroveId))
                        {
                            // NOTE: For now, we will allow this, but later, we will remove it and throw an error instead
                            ServiceCache.GroveRegistryCache[RuleTreeData.GroveId] = new SvcGrove(RuleTreeData.GroveId);
                        }

                        bAddToGrove = true;
                    }

                    RuleTreeData.RulesEngine = NewRulesEngine;

                    if (RuleTreeData.SerializeToBlockchain)
                    {
                        SerializeRefEnv();

                        string sTreeOwnerAddress = msSenderAddress;

                        if (!String.IsNullOrEmpty(RuleTreeData.OwnerName) && ServiceCache.TreeOwnerCache.ContainsKey(RuleTreeData.OwnerName))
                        {
                            SvcRuleTreeOwner RTOwner = ServiceCache.TreeOwnerCache[RuleTreeData.OwnerName];

                            if (!String.IsNullOrEmpty(RTOwner.OwnerAddress))
                                sTreeOwnerAddress = RTOwner.OwnerAddress;
                        }

                        NewRulesEngine.Serialize(msRuleMasterAddress, 
                                                 msPassword,
                                                 sTreeOwnerAddress,
                                                 msWonkaContractAddress, 
                                                 msAbiWonka, 
                                                 (NewRulesEngine.TransactionState != null) ? moOrchInitData.TrxStateContractAddress : null,
                                                 moOrchInitData.Web3HttpUrl);

                        if (bAddToGrove)
                        {
                            if (ServiceCache.GroveRegistryCache.ContainsKey(RuleTreeData.GroveId))
                            {
                                NewRulesEngine.SerializeToGrove(GetRegistryContract(), sTreeOwnerAddress, RuleTreeData.GroveId, moOrchInitData.Web3HttpUrl);

                                ServiceCache.GroveRegistryCache[RuleTreeData.GroveId].RuleTreeMembers.Add(RuleTreeData.RuleTreeId);
                            }
                            else
                                throw new Exception("ERROR!  Grove Id(" + RuleTreeData.GroveId + ") isn't in the cache yet?!");
                        }

                        if (NewRulesEngine.TransactionState != null)
                        {
                            NewRulesEngine.TransactionState.Serialize(GetWonkaContract(), 
                                                                      msRuleMasterAddress, 
                                                                      msPassword, 
                                                                      msSenderAddress, 
                                                                      moOrchInitData.TrxStateContractAddress, 
                                                                      moOrchInitData.Web3HttpUrl);
                        }
                    }

                    bSerialized = true;
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

                // Only here temporarily, useful for debugging purposes
                if (bSerialized)
                    RuleTreeData.StackTraceMessage = ex.StackTrace;
                else
                {
                    string sCallString = String.Format("Problem when calling {0}/{1}/{2} -> ", msSenderAddress, msPassword, msOrchContractAddress);
                    RuleTreeData.StackTraceMessage = sCallString + ex.StackTrace;
                }

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

        protected string ExportRuleTreeXml(WonkaBreRulesEngine poEngine)
        {
            string sRuleTreeXml = "";

            WonkaEth.Contracts.WonkaRuleGrove NewSaleGrove = new WonkaEth.Contracts.WonkaRuleGrove(poEngine.GroveId);
            NewSaleGrove.PopulateFromRegistry(msAbiWonka);

            if (NewSaleGrove.OrderedRuleTrees.Count <= 0)
                throw new Exception("ERROR!  No trees pulled from registry for GroveId(" + poEngine.GroveId + ")");

            var RegistryItem =
                NewSaleGrove.OrderedRuleTrees.Where(x => x.RuleTreeId == poEngine.DetermineRuleTreeChainID()).FirstOrDefault();

            if (String.IsNullOrEmpty(RegistryItem.RuleTreeId))
                throw new Exception("ERROR!  No tree found from registry for GroveId(" + poEngine.GroveId + ") matching RuleTree(" + poEngine.DetermineRuleTreeChainID() + ").");

            // Assemble the XML string by extracting data from the rules engine on the chain
            sRuleTreeXml = RegistryItem.ExportXmlString(moOrchInitData.Web3HttpUrl);

            return sRuleTreeXml;
        }

        #endregion
    }
}
