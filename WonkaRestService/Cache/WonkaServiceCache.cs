﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;

using Wonka.BizRulesEngine;

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
            MarkupCache         = new Dictionary<string, string>();
            RuleTreeCache       = new Dictionary<string, WonkaBizRulesEngine>();
            RuleTreeOriginCache = new Dictionary<string, SvcRuleTree>();
            GroveRegistryCache  = new Dictionary<string, SvcGrove>();
            ReportCache         = new Dictionary<string, List<SvcRuleTreeReport>>();
            TreeOwnerCache      = new Dictionary<string, SvcRuleTreeOwner>();
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

        public Dictionary<string, string> MarkupCache { get; set; }

        public Dictionary<string, WonkaBizRulesEngine> RuleTreeCache { get; set; }

        public Dictionary<string, SvcRuleTree> RuleTreeOriginCache { get; set; }

        public Dictionary<string, SvcGrove> GroveRegistryCache { get; set; }

        public Dictionary<string, List<SvcRuleTreeReport>> ReportCache { get; set; }

        public Dictionary<string, SvcRuleTreeOwner> TreeOwnerCache { get; set; }

        #endregion
    }
}

