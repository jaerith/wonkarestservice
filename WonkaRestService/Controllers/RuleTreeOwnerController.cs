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
    public class RuleTreeOwnerController : ApiController
    {
        /// <summary>
        /// 
        /// This method will return the potential/actual owner of a RuleTree from the cache.
        /// 
        /// GET: api/TrxState/TreeOwner/OwnerId
        /// 
        /// <param name="OwnerId">An owner of a RuleTree</param>
        /// <returns>Contains the Response with the owner (or an error message if an error occurs)</returns>
        /// </summary>
        public HttpResponseMessage GetTreeOwner(string OwnerName)
        {
            SvcRuleTreeOwner RTOwner = new SvcRuleTreeOwner(OwnerName);

            var response = Request.CreateResponse<SvcRuleTreeOwner>(HttpStatusCode.OK, RTOwner);

            string uri = Url.Link("DefaultApi", new { id = "DefaultValue" });

            response.Headers.Location = new Uri(uri);

            try
            {
                WonkaServiceCache ServiceCache = WonkaServiceCache.GetInstance();

                if (String.IsNullOrEmpty(OwnerName))
                    throw new Exception("ERROR!  The Owner ID provided is null/empty.");

                if (ServiceCache.TreeOwnerCache.ContainsKey(OwnerName))
                    RTOwner = ServiceCache.TreeOwnerCache[OwnerName];

                response = Request.CreateResponse<SvcRuleTreeOwner>(HttpStatusCode.Created, RTOwner);
            }
            catch (Exception ex)
            {
                string sErrorMsg = String.Format("ERROR!  Rule Tree Owner web method -> Error Message : {0}",
                                                 ex.ToString());

                if ((ex.InnerException != null) && (ex.InnerException.Message != null))
                    RTOwner.ErrorMessage = ex.InnerException.Message;
                else if (!String.IsNullOrEmpty(ex.Message))
                    RTOwner.ErrorMessage = ex.Message;

                response = Request.CreateResponse<SvcRuleTreeOwner>(HttpStatusCode.BadRequest, RTOwner);

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return response;
        }

        /// <summary>
        /// 
        /// This method will put the RuleTreeOwner in the cache.
        /// 
        /// POST: api/RuleTreeOwner
        /// 
        /// TEST PAYLOAD:
        /// {
        ///   "OwnerName": "NumberTwo",
        ///   "OwnerAddress": "0x7aba3df73d31904178512d36fbbe324464184730",
        ///   "OwnerPassword": "0xcd976346455d9e1a0728d7735db648f467d348922359ca8f549fd112232c7fd6"
        /// }
        /// 
        /// <param name="RuleTreeOwner">The potentia/actual owner of a RuleTree</param>
        /// <returns>Contains the Response with the owner (or an error message if an error occurs)</returns>
        /// </summary>
        public HttpResponseMessage PostRuleTreeOwner(SvcRuleTreeOwner RuleTreeOwner)
        {
            var response = Request.CreateResponse<SvcRuleTreeOwner>(HttpStatusCode.Created, RuleTreeOwner);

            string uri = Url.Link("DefaultApi", new { id = "DefaultValue" });

            response.Headers.Location = new Uri(uri);

            try
            {
                if (RuleTreeOwner == null)
                    throw new Exception("ERROR!  No owner was provided.");

                if (!RuleTreeOwner.IsValid())
                    throw new Exception("ERROR!  Invalid owner was provided.");

                WonkaServiceCache ServiceCache = WonkaServiceCache.GetInstance();

                if (!ServiceCache.TreeOwnerCache.ContainsKey(RuleTreeOwner.OwnerName))
                    ServiceCache.TreeOwnerCache[RuleTreeOwner.OwnerName] = RuleTreeOwner;

                response = Request.CreateResponse<SvcRuleTreeOwner>(HttpStatusCode.Created, RuleTreeOwner);
            }
            catch (Exception ex)
            {
                string sErrorMsg = String.Format("ERROR!  Rule Tree Owner web method -> Error Message : {0}",
                                                 ex.ToString());

                if ((ex.InnerException != null) && (ex.InnerException.Message != null))
                    RuleTreeOwner.ErrorMessage = ex.InnerException.Message;
                else if (!String.IsNullOrEmpty(ex.Message))
                    RuleTreeOwner.ErrorMessage = ex.Message;

                response = Request.CreateResponse<SvcRuleTreeOwner>(HttpStatusCode.BadRequest, RuleTreeOwner);

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return response;
        }

        /*
        // PUT: api/RuleTreeOwner/5
        public void Put(int id, [FromBody]string value)
        {
        }
         */

        /// <summary>
        /// 
        /// This method will delete the Owner from the cache.
        /// 
        /// DELETE: api/RuleTreeOwner
        /// 
        /// <param name="OwnerId">The ID of the Owner</param>
        /// <returns>Contains the Response with the owner (or an error message if an error occurs)</returns>
        /// </summary>        
        public HttpResponseMessage DeleteRuleTreeOwner(string OwnerId)
        {
            SvcRuleTreeOwner RTOwner = new SvcRuleTreeOwner(OwnerId);

            var response = Request.CreateResponse<SvcRuleTreeOwner>(HttpStatusCode.Created, RTOwner);

            string uri = Url.Link("DefaultApi", new { id = "DefaultValue" });

            response.Headers.Location = new Uri(uri);

            try
            {
                WonkaServiceCache ServiceCache = WonkaServiceCache.GetInstance();

                if (String.IsNullOrEmpty(OwnerId))
                    throw new Exception("ERROR!  The Owner ID provided is null/empty.");

                if (!ServiceCache.TreeOwnerCache.ContainsKey(OwnerId))
                {
                    RTOwner = ServiceCache.TreeOwnerCache[OwnerId];

                    ServiceCache.TreeOwnerCache.Remove(OwnerId);
                }

                response = Request.CreateResponse<SvcRuleTreeOwner>(HttpStatusCode.Created, RTOwner);
            }
            catch (Exception ex)
            {
                string sErrorMsg = String.Format("ERROR!  Rule Tree Owner web method -> Error Message : {0}",
                                                 ex.ToString());

                if ((ex.InnerException != null) && (ex.InnerException.Message != null))
                    RTOwner.ErrorMessage = ex.InnerException.Message;
                else if (!String.IsNullOrEmpty(ex.Message))
                    RTOwner.ErrorMessage = ex.Message;

                response = Request.CreateResponse<SvcRuleTreeOwner>(HttpStatusCode.BadRequest, RTOwner);

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return response;
        }
    }
}
