using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;

using WonkaBre;

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

        // NOTE: This constructor is necessary for serialization/deserialization purposes
        private WonkaServiceCache()
        {
            RuleTreeCache = new Dictionary<string, WonkaBreRulesEngine>();
        }

        static public WonkaServiceCache CreateInstance()
        {
            lock (mLock)
            {
                if (mInstance == null)
                    mInstance = new WonkaServiceCache();

                return mInstance;
            }
        }

        static public WonkaServiceCache GetInstance()
        {
            lock (mLock)
            {
                if (mInstance == null)
                    throw new Exception("ERROR!  WonkaServiceCache has not yet been initialized!");

                return mInstance;
            }
        }

        #region Methods

        #endregion

        #region Properties 

        public Dictionary<string, WonkaBreRulesEngine> RuleTreeCache { get; set; }

        #endregion
    }
}

