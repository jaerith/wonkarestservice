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
    public class TrxStateOwnerController : ApiController
    {
        /// <summary>
        /// 
        /// This method will return the trx state of a RuleTree identified by the RuleTreeId.
        /// 
        /// GET: api/TrxState/RuleTreeId/Owner
        /// 
        /// <param name="psRuleTreeId">The ID of the RuleTree</param>
        /// <returns>Contains the Response with the trx state (or an error message if an error occurs)</returns>
        /// </summary>
        public HttpResponseMessage Get(string RuleTreeId, string Owner)
        {
            SvcTrxStateOwner TrxStateOwner = new SvcTrxStateOwner("", false, 0);

            var response = Request.CreateResponse<SvcTrxStateOwner>(HttpStatusCode.Created, TrxStateOwner);

            string uri = Url.Link("DefaultApi", new { id = "DefaultValue" });

            response.Headers.Location = new Uri(uri);

            try
            {
                string sTargetRuleTreeId =
                    (!String.IsNullOrEmpty(RuleTreeId)) ? RuleTreeId : InvokeController.CONST_RULES_RESOURCE_STREAM;

                WonkaServiceCache ServiceCache = WonkaServiceCache.CreateInstance();

                WonkaBreRulesEngine RulesEngine = null;
                if (ServiceCache.RuleTreeCache.ContainsKey(sTargetRuleTreeId))
                {
                    RulesEngine = ServiceCache.RuleTreeCache[sTargetRuleTreeId];

                    if ((RulesEngine.TransactionState != null) && (RulesEngine.TransactionState is WonkaBre.Permissions.WonkaBreTransactionState))
                    {
                        if (RulesEngine.TransactionState.IsOwner(Owner))
                        {
                            TrxStateOwner.RuleTreeId  = RuleTreeId;
                            TrxStateOwner.OwnerName   = Owner;
                            TrxStateOwner.OwnerWeight = RulesEngine.TransactionState.GetOwnerWeight(Owner);

                            if (RulesEngine.TransactionState.GetOwnersConfirmed().Contains(Owner))
                                TrxStateOwner.ConfirmedTransaction = true;
                            else
                                TrxStateOwner.ConfirmedTransaction = false;
                        }
                        else
                            throw new Exception("ERROR!  Not a registered owner of this RuleTree.");
                    }
                }

                response = Request.CreateResponse<SvcTrxStateOwner>(HttpStatusCode.Created, TrxStateOwner);
            }
            catch (Exception ex)
            {
                string sErrorMsg = String.Format("ERROR!  Trx State web method -> Error Message : {0}",
                                                 ex.ToString());

                if ((ex.InnerException != null) && (ex.InnerException.Message != null))
                    TrxStateOwner.ErrorMessage = ex.InnerException.Message;
                else if (!String.IsNullOrEmpty(ex.Message))
                    TrxStateOwner.ErrorMessage = ex.Message;

                response = Request.CreateResponse<SvcTrxStateOwner>(HttpStatusCode.BadRequest, TrxStateOwner);

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return response;
        }

        // POST: api/TrxStateOwner
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/TrxStateOwner/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/TrxStateOwner/5
        public void Delete(int id)
        {
        }
    }
}
