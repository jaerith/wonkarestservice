using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml.Linq;

using WonkaRestService.Cache;
using WonkaRestService.Extensions;
using WonkaRestService.Models;

namespace WonkaRestService.Controllers
{
    public class MarkupController : ApiController
    {
        #region CONSTANTS

        public const string CONST_MARKUP_DEFAULT_URL_PREFIX = "http://localwonkacache/";

        public const string CONST_LAST_MARKUP_GEN_KEY_IND = "LastMarkupGenKey";

        #endregion

        // Yes, yes, obviously this is not thread-safe
        public static string LastGeneratedMarkupKey = "";

        /// <summary>
        /// 
        /// This method will return the markup of a RuleTree, if it exists within the cache.
        /// 
        /// GET: api/RuleTree/MarkupId
        /// 
        /// <param name="MarkupId">The ID of the RuleTree markup</param>
        /// <returns>Contains the Response with the markup (or an error message if an error occurs)</returns>
        /// </summary>
        public HttpResponseMessage GetRuleTreeMarkup(string MarkupId)
        {
            string sRuleTreeMarkup = "";

            var response = Request.CreateResponse<string>(HttpStatusCode.OK, sRuleTreeMarkup);

            string uri = Url.Link("DefaultApi", new { id = "DefaultValue" });

            response.Headers.Location = new Uri(uri);

            try
            {
                WonkaServiceCache ServiceCache = WonkaServiceCache.GetInstance();

                if (MarkupId == CONST_LAST_MARKUP_GEN_KEY_IND)
                    sRuleTreeMarkup = LastGeneratedMarkupKey;
                else if (ServiceCache.MarkupCache.ContainsKey(MarkupId))
                    sRuleTreeMarkup = ServiceCache.MarkupCache[MarkupId];

                response = Request.CreateResponse<string>(HttpStatusCode.OK, sRuleTreeMarkup);
            }
            catch (Exception ex)
            {
                string sErrorMsg = String.Format("ERROR!  Markup web method -> Error Message : {0}",
                                                 ex.ToString());

                if ((ex.InnerException != null) && (ex.InnerException.Message != null))
                    sRuleTreeMarkup = ex.InnerException.Message;
                else if (!String.IsNullOrEmpty(ex.Message))
                    sRuleTreeMarkup = ex.Message;

                response = Request.CreateResponse<string>(HttpStatusCode.BadRequest, sRuleTreeMarkup);

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return response;
        }

        /// <summary>
        /// 
        /// This method will store the markup of a RuleTree in the cache and generate an ID for later retrieval.
        /// 
        /// POST: api/markup
        /// 
        /// <param name="psMarkupXml">The XML markup for a RuleTree</param>
        /// <returns>Contains the generated URL of the RuleTree (or an error message if an error occurs)</returns>
        /// </summary>
        public HttpResponseMessage PostRuleTreeMarkup(XElement poMarkupXml)
        {
            string sMarkupXml = poMarkupXml.ToString();

            var response = Request.CreateResponse<string>(HttpStatusCode.Created, sMarkupXml);

            string uri = Url.Link("DefaultApi", new { id = "DefaultValue" });

            response.Headers.Location = new Uri(uri);

            string sNewMarkupId = "";

            try
            {
                if (String.IsNullOrEmpty(sMarkupXml))
                    throw new Exception("ERROR!  No markup for a rule tree data was provided.");

                WonkaServiceCache ServiceCache = WonkaServiceCache.GetInstance();

                sNewMarkupId = LastGeneratedMarkupKey = CONST_MARKUP_DEFAULT_URL_PREFIX + Guid.NewGuid().ToString();

                ServiceCache.MarkupCache[sNewMarkupId] = sMarkupXml;

                response = Request.CreateResponse<string>(HttpStatusCode.Created, sNewMarkupId);
            }
            catch (Exception ex)
            {
                string sErrorMsg = String.Format("ERROR!  Markup web method -> Error Message : {0}",
                                                 ex.ToString());

                if ((ex.InnerException != null) && (ex.InnerException.Message != null))
                    sNewMarkupId = ex.InnerException.Message;
                else if (!String.IsNullOrEmpty(ex.Message))
                    sNewMarkupId = ex.Message;

                response = Request.CreateResponse<string>(HttpStatusCode.BadRequest, sNewMarkupId);

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return response;
        }
    }
}
