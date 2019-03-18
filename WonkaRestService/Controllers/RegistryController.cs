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
        /// This method will return information about a Grove, if it exists within the cache.
        /// 
        /// GET: api/Registry/GroveId
        /// 
        /// <param name="RuleTreeId">The ID of the Grove</param>
        /// <returns>Contains the Response with the Grove (or an error message if an error occurs)</returns>
        /// </summary>
        public HttpResponseMessage GetGrove(string GroveId)
        {
            SvcGrove Grove = new SvcGrove();

            var response = Request.CreateResponse<SvcGrove>(HttpStatusCode.OK, Grove);

            string uri = Url.Link("DefaultApi", new { id = "DefaultValue" });

            response.Headers.Location = new Uri(uri);

            try
            {

                WonkaServiceCache ServiceCache = WonkaServiceCache.GetInstance();

                if (ServiceCache.GroveRegistryCache.ContainsKey(GroveId))
                    Grove = ServiceCache.GroveRegistryCache[GroveId];

                response = Request.CreateResponse<SvcGrove>(HttpStatusCode.Created, Grove);
            }
            catch (Exception ex)
            {
                string sErrorMsg = String.Format("ERROR!  Registry (Grove) web method -> Error Message : {0}",
                                                 ex.ToString());

                if ((ex.InnerException != null) && (ex.InnerException.Message != null))
                    Grove.ErrorMessage = ex.InnerException.Message;
                else if (!String.IsNullOrEmpty(ex.Message))
                    Grove.ErrorMessage = ex.Message;

                response = Request.CreateResponse<SvcGrove>(HttpStatusCode.BadRequest, Grove);

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return response;
        }

        /// <summary>
        /// 
        /// This method will provide the information to create a Grove, afterwards then inserting it within the cache.
        /// 
        /// POST: api/RuleTree/GroveData
        /// 
        /// TEST PAYLOAD: 
        /// 
        /// {
        ///   "GroveId":"VATCalculateClub",
        ///   "GroveDescription":"Grove of Precheck RuleTrees",
        ///   "RuleTreeMembers":["VATPrecheckExample1","VATPrecheckExample2"],
        ///   CreationEpochTime":0
        /// }
        /// 
        /// <param name="GroveData">The data needed to instantiate the Grove</param>
        /// <returns>Contains the Response with the Grove (or an error message if an error occurs)</returns>
        /// </summary>
        public HttpResponseMessage PostGrove(SvcGrove GroveData)
        {
            var response = Request.CreateResponse<SvcGrove>(HttpStatusCode.Created, GroveData);

            string uri = Url.Link("DefaultApi", new { id = "DefaultValue" });

            response.Headers.Location = new Uri(uri);

            try
            {
                if (GroveData == null)
                    throw new Exception("ERROR!  No grove data was provided.");

                if (!GroveData.IsValid())
                    throw new Exception("ERROR!  Invalid rule tree data was provided.");

                WonkaServiceCache ServiceCache = WonkaServiceCache.GetInstance();

                if (ServiceCache.GroveRegistryCache.ContainsKey(GroveData.GroveId))
                    throw new Exception("ERROR!  Grove with ID already exists.");

                ServiceCache.GroveRegistryCache[GroveData.GroveId] = GroveData;

                response = Request.CreateResponse<SvcGrove>(HttpStatusCode.Created, GroveData);
            }
            catch (Exception ex)
            {
                string sErrorMsg = String.Format("ERROR!  Registry (Grove) web method -> Error Message : {0}",
                                                 ex.ToString());

                if ((ex.InnerException != null) && (ex.InnerException.Message != null))
                    GroveData.ErrorMessage = ex.InnerException.Message;
                else if (!String.IsNullOrEmpty(ex.Message))
                    GroveData.ErrorMessage = ex.Message;

                response = Request.CreateResponse<SvcGrove>(HttpStatusCode.BadRequest, GroveData);

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return response;
        }

        /*
        // PUT: api/Registry/5
        public void Put(int id, [FromBody]string value)
        {
        }
        */

        /// <summary>
        /// 
        /// This method will remove a Grove identified by the GroveId.
        /// 
        /// DELETE: api/Registry/GroveId
        /// 
        /// <param name="GroveId">The ID of the Grove</param>
        /// <returns>Contains the Response with the GroveId</returns>
        /// </summary>
        public HttpResponseMessage DeleteGrove(string GroveId)
        {
            SvcGrove Grove = new SvcGrove(GroveId);

            var response = Request.CreateResponse<SvcGrove>(HttpStatusCode.OK, Grove);

            string uri = Url.Link("DefaultApi", new { id = "DefaultValue" });

            response.Headers.Location = new Uri(uri);

            try
            {
                WonkaServiceCache ServiceCache = WonkaServiceCache.GetInstance();

                if (String.IsNullOrEmpty(GroveId))
                    throw new Exception("ERROR!  Grove Id was not provided.");

                if (ServiceCache.GroveRegistryCache.ContainsKey(GroveId))
                {
                    Grove = ServiceCache.GroveRegistryCache[GroveId];

                    ServiceCache.GroveRegistryCache.Remove(GroveId);
                }
                else
                    throw new Exception("ERROR!  Grove (" + GroveId + ") does not exist.");

                response = Request.CreateResponse<SvcGrove>(HttpStatusCode.Created, Grove);
            }
            catch (Exception ex)
            {
                string sErrorMsg = String.Format("ERROR!  Registry (Grove) web method -> Error Message : {0}",
                                                 ex.ToString());

                if ((ex.InnerException != null) && (ex.InnerException.Message != null))
                    Grove.ErrorMessage = ex.InnerException.Message;
                else if (!String.IsNullOrEmpty(ex.Message))
                    Grove.ErrorMessage = ex.Message;

                response = Request.CreateResponse<SvcGrove>(HttpStatusCode.BadRequest, Grove);

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return response;
        }

    }
}
