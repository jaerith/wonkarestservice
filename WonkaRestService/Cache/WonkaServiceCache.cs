using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;

using WonkaBre;

using WonkaRestService.Models;

namespace WonkaRestService.Cache
{
    /// <summary>
    /// 
    /// This singleton, when initialized, will contain all of the cached data
    /// that drives the Wonka service.
    /// 
    /// </summary>
    [DataContract(Namespace = "http://wonkarestservice.com")]
    public class WonkaServiceCache
    {
        private static object mLock = new object();

        private static WonkaServiceCache mInstance = null;

        private WonkaServiceCache()
        {
            RuleTreeCache       = new Dictionary<string, WonkaBreRulesEngine>();
            RuleTreeOriginCache = new Dictionary<string, string>();
            GroveRegistryCache  = new Dictionary<string, SvcGrove>();
            ReportCache         = new Dictionary<string, List<SvcRuleTreeReport>>();
        }

        static public WonkaServiceCache GetInstance()
        {
            lock (mLock)
            {
                if (mInstance == null)
                    mInstance = new WonkaServiceCache();

                return mInstance;
            }
        }

        #region Methods

        #endregion

        #region Properties 

        // NOTE: There is currently no locking for these variables, and that will need to be implemented eventually

        public Dictionary<string, WonkaBreRulesEngine> RuleTreeCache { get; set; }

        public Dictionary<string, string> RuleTreeOriginCache { get; set; }

        public Dictionary<string, SvcGrove> GroveRegistryCache { get; set; }

        public Dictionary<string, List<SvcRuleTreeReport>> ReportCache { get; set; }

        #endregion
    }
}

