using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using WonkaBre;

using WonkaRestService.Cache;
using WonkaRestService.Models;

namespace WonkaRestService.Controllers
{
    public class TrxStateController : ApiController
    {
        /// <summary>
        /// 
        /// This method will return the trx state of a RuleTree identified by the RuleTreeId.
        /// 
        /// GET: api/TrxState/RuleTreeId
        /// 
        /// <param name="RuleTreeId">The ID of the RuleTree</param>
        /// <returns>Contains the Response with the trx state (or an error message if an error occurs)</returns>
        /// </summary>
        public HttpResponseMessage GetTrxState(string RuleTreeId)
        {
            HashSet<string> DummyList = new HashSet<string>() { "Blank" };

            SvcTrxState TrxState = new SvcTrxState(DummyList);

            var response = Request.CreateResponse<SvcTrxState>(HttpStatusCode.OK, TrxState);

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
                    RulesEngine = ServiceCache.RuleTreeCache[sTargetRuleTreeId];

                    if ((RulesEngine.TransactionState != null) && (RulesEngine.TransactionState is WonkaBre.Permissions.WonkaBreTransactionState))
                    {
                        HashSet<string> OwnersTotal =
                            new HashSet<string>(RulesEngine.TransactionState.GetOwnersConfirmed());

                        OwnersTotal.Union(RulesEngine.TransactionState.GetOwnersUnconfirmed());

                        if (OwnersTotal.Count > 0)
                        {
                            TrxState =
                                new SvcTrxState(sTargetRuleTreeId, (WonkaBre.Permissions.WonkaBreTransactionState)RulesEngine.TransactionState, OwnersTotal);

                            TrxState.RefreshSvcOwnerList();
                        }
                    }
                }

                response = Request.CreateResponse<SvcTrxState>(HttpStatusCode.Created, TrxState);
            }
            catch (Exception ex)
            {
                string sErrorMsg = String.Format("ERROR!  Trx State web method -> Error Message : {0}",
                                                 ex.ToString());

                if ((ex.InnerException != null) && (ex.InnerException.Message != null))
                    TrxState.ErrorMessage = ex.InnerException.Message;
                else if (!String.IsNullOrEmpty(ex.Message))
                    TrxState.ErrorMessage = ex.Message;

                response = Request.CreateResponse<SvcTrxState>(HttpStatusCode.BadRequest, TrxState);

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return response;
        }

        /// <summary>
        /// 
        /// This method will set the trx state of a RuleTree identified by the RuleTreeId.
        /// 
        /// POST: api/TrxState
        /// 
        /// Test Payload:
        ///{
        ///  "ContractAddress": null,
        ///  "Confirmed": true,
        ///  "CurrentScore": 1,
        ///  "MinimumScore": 0,
        ///  "Owners": [
        ///    {
        ///      "OwnerName": "Dummy",
        ///      "ConfirmedTransaction": true,
        ///      "OwnerWeight": 1
        ///    }
        ///  ],
        ///  "RuleTreeId": "WonkaRestService.WonkaData.VATCalculationExample.xml",
        ///  "ApprovedExecutors": []
        ///}
        /// 
        /// <param name="TrxState">The transaction state meant for the RuleTree</param>
        /// <returns>Contains the Response with the trx state provided (with an error message if an error occurs)</returns>
        /// </summary>
        public HttpResponseMessage PostTrxState(SvcTrxState TrxState)
        {
            var response = Request.CreateResponse<SvcTrxState>(HttpStatusCode.Created, TrxState);

            string uri = Url.Link("DefaultApi", new { id = "DefaultValue" });

            response.Headers.Location = new Uri(uri);

            try
            {
                if (TrxState == null)
                    throw new Exception("ERROR!  No transaction state was provided.");

                if (!TrxState.IsValid())
                    throw new Exception("ERROR!  Invalid transaction state was provided.");

                TrxState.RefreshSvcOwnerList();

                string sTargetRuleTreeId = TrxState.RuleTreeId;
                //    (!String.IsNullOrEmpty(TrxState.RuleTreeId)) ? TrxState.RuleTreeId : InvokeController.CONST_RULES_RESOURCE_STREAM;

                WonkaServiceCache ServiceCache = WonkaServiceCache.GetInstance();

                WonkaBreRulesEngine RulesEngine = null;
                if (ServiceCache.RuleTreeCache.ContainsKey(sTargetRuleTreeId))
                {
                    RulesEngine = ServiceCache.RuleTreeCache[sTargetRuleTreeId];

                    RulesEngine.TransactionState = TrxState;
                }

                response = Request.CreateResponse<SvcTrxState>(HttpStatusCode.Created, TrxState);
            }
            catch (Exception ex)
            {
                string sErrorMsg = String.Format("ERROR!  Trx State web method -> Error Message : {0}",
                                                 ex.ToString());

                if ((ex.InnerException != null) && (ex.InnerException.Message != null))
                    TrxState.ErrorMessage = ex.InnerException.Message;
                else if (!String.IsNullOrEmpty(ex.Message))
                    TrxState.ErrorMessage = ex.Message;

                response = Request.CreateResponse<SvcTrxState>(HttpStatusCode.BadRequest, TrxState);

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return response;
        }

        /*
        // PUT: api/TrxState/5
        public void Put(int id, [FromBody]string value)
        {
        }
        */

        /// <summary>
        /// 
        /// This method will remove the trx state of a RuleTree identified by the RuleTreeId.
        /// 
        /// DELETE: api/TrxState/RuleTreeId
        /// 
        /// <param name="RuleTreeId">The ID of the RuleTree</param>
        /// <returns>Contains the Response with the RuleTreeId</returns>
        /// </summary>
        public HttpResponseMessage Delete(string RuleTreeId)
        {
            var response = Request.CreateResponse<string>(HttpStatusCode.OK, RuleTreeId);

            string uri = Url.Link("DefaultApi", new { id = RuleTreeId });

            response.Headers.Location = new Uri(uri);

            try
            {
                string sTargetRuleTreeId =
                    (!String.IsNullOrEmpty(RuleTreeId)) ? RuleTreeId : InvokeController.CONST_RULES_RESOURCE_STREAM;

                WonkaServiceCache ServiceCache = WonkaServiceCache.GetInstance();

                WonkaBreRulesEngine RulesEngine = null;
                if (ServiceCache.RuleTreeCache.ContainsKey(sTargetRuleTreeId))
                {
                    RulesEngine = ServiceCache.RuleTreeCache[sTargetRuleTreeId];

                    RulesEngine.TransactionState = null;
                }

                response = Request.CreateResponse<string>(HttpStatusCode.OK, RuleTreeId);
            }
            catch (Exception ex)
            {
                string sErrorMsg = String.Format("ERROR!  Trx State web method -> Error Message : {0}",
                                                 ex.ToString());

                /*
                if ((ex.InnerException != null) && (ex.InnerException.Message != null))
                    TrxState.ErrorMessage = ex.InnerException.Message;
                else if (!String.IsNullOrEmpty(ex.Message))
                    TrxState.ErrorMessage = ex.Message;
                 */

                response = Request.CreateResponse<string>(HttpStatusCode.BadRequest, RuleTreeId);

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return response;
        }
    }
}
