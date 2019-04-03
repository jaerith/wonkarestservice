using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using WonkaRestService.Cache;
using WonkaRestService.Models;

namespace WonkaRestService.Controllers
{
    public class RegistryController : ApiController
    {
        /// <summary>
        /// 
        /// This method will return information about a RuleTree from the Registry.
        /// 
        /// GET: api/Registry/RuleTreeId
        /// 
        /// <param name="RuleTreeId">The ID of the RuleTree</param>
        /// <returns>Contains the Response with the Registry data (or an error message if an error occurs)</returns>
        /// </summary>
        public HttpResponseMessage GetTreeRegistry(string RuleTreeId)
        {
            SvcRuleTreeRegistry RegistryData = new SvcRuleTreeRegistry();

            var response = Request.CreateResponse<SvcRuleTreeRegistry>(HttpStatusCode.OK, RegistryData);

            string uri = Url.Link("DefaultApi", new { id = "DefaultValue" });

            response.Headers.Location = new Uri(uri);

            try
            {
                // NOTE: Do work here

                response = Request.CreateResponse<SvcRuleTreeRegistry>(HttpStatusCode.OK, RegistryData);
            }
            catch (Exception ex)
            {
                string sErrorMsg = String.Format("ERROR!  Registry (Tree) web method -> Error Message : {0}",
                                                 ex.ToString());

                if ((ex.InnerException != null) && (ex.InnerException.Message != null))
                    RegistryData.ErrorMessage = ex.InnerException.Message;
                else if (!String.IsNullOrEmpty(ex.Message))
                    RegistryData.ErrorMessage = ex.Message;

                response = Request.CreateResponse<SvcRuleTreeRegistry>(HttpStatusCode.BadRequest, RegistryData);

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return response;
        }

        /*
        public HttpResponseMessage PostTreeRegistry(SvcRuleTreeRegistry RegistryData)
        {
            var response = Request.CreateResponse<SvcRuleTreeRegistry>(HttpStatusCode.Created, RegistryData);

            string uri = Url.Link("DefaultApi", new { id = "DefaultValue" });

            response.Headers.Location = new Uri(uri);

            return response;
        }
        */

        /// <summary>
        /// 
        /// This method will update information about a RuleTree in the Registry.
        /// 
        /// GET: api/Registry/RuleTreeId
        /// 
        /// <param name="RuleTreeId">The ID of the RuleTree</param>
        /// <returns>Contains the Response with the Registry data (or an error message if an error occurs)</returns>
        /// </summary>
        public HttpResponseMessage PutTreeRegistry(SvcRuleTreeRegistry poRegistryData)
        {
            var response = Request.CreateResponse<SvcRuleTreeRegistry>(HttpStatusCode.Accepted, poRegistryData);

            string uri = Url.Link("DefaultApi", new { id = "DefaultValue" });

            response.Headers.Location = new Uri(uri);

            try
            {
                // NOTE: Do work here

                response = Request.CreateResponse<SvcRuleTreeRegistry>(HttpStatusCode.Accepted, poRegistryData);
            }
            catch (Exception ex)
            {
                string sErrorMsg = String.Format("ERROR!  Registry (Tree) web method -> Error Message : {0}",
                                                 ex.ToString());

                if ((ex.InnerException != null) && (ex.InnerException.Message != null))
                    poRegistryData.ErrorMessage = ex.InnerException.Message;
                else if (!String.IsNullOrEmpty(ex.Message))
                    poRegistryData.ErrorMessage = ex.Message;

                response = Request.CreateResponse<SvcRuleTreeRegistry>(HttpStatusCode.BadRequest, poRegistryData);

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return response;
        }

        /// <summary>
        /// 
        /// This method will remove a RuleTree from the Registry.
        /// 
        /// DELETE: api/Registry/RegistryData
        /// 
        /// <param name="RuleTreeId">The ID of the RuleTree</param>
        /// <returns>Contains the Response with the RuleTreeId</returns>
        /// </summary>
        public HttpResponseMessage DeleteTreeRegistry(string RuleTreeId)
        {
            SvcRuleTreeRegistry RegistryData = new SvcRuleTreeRegistry(RuleTreeId);

            var response = Request.CreateResponse<SvcRuleTreeRegistry>(HttpStatusCode.OK, RegistryData);

            string uri = Url.Link("DefaultApi", new { id = "DefaultValue" });

            response.Headers.Location = new Uri(uri);

            try
            {
                // NOTE: Do work here

                response = Request.CreateResponse<SvcRuleTreeRegistry>(HttpStatusCode.OK, RegistryData);
            }
            catch (Exception ex)
            {
                string sErrorMsg = String.Format("ERROR!  Registry (Tree) web method -> Error Message : {0}",
                                                 ex.ToString());

                if ((ex.InnerException != null) && (ex.InnerException.Message != null))
                    RegistryData.ErrorMessage = ex.InnerException.Message;
                else if (!String.IsNullOrEmpty(ex.Message))
                    RegistryData.ErrorMessage = ex.Message;

                response = Request.CreateResponse<SvcRuleTreeRegistry>(HttpStatusCode.BadRequest, RegistryData);

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return response;
        }

    }
}
