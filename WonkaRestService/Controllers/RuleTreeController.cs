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
    public class RuleTreeController : ApiController
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

                WonkaServiceCache ServiceCache = WonkaServiceCache.CreateInstance();

                WonkaBreRulesEngine RulesEngine = null;
                if (ServiceCache.RuleTreeCache.ContainsKey(sTargetRuleTreeId))
                {
                    RuleTree.RuleTreeId = RuleTreeId;

                    RulesEngine = ServiceCache.RuleTreeCache[sTargetRuleTreeId];
                    if (RulesEngine != null)
                        RuleTree.RulesEngine = RulesEngine;
                }

                response = Request.CreateResponse<SvcRuleTree>(HttpStatusCode.Created, RuleTree);
            }
            catch (Exception ex)
            {
                string sErrorMsg = String.Format("ERROR!  Rules Engine web method -> Error Message : {0}",
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

        // POST: api/RuleTree
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/RuleTree/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/RuleTree/5
        public void Delete(int id)
        {
        }
    }
}
