using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

using WonkaBre;
using WonkaBre.Permissions;
using WonkaBre.RuleTree;

namespace WonkaRestService.Models
{
    [DataContract(Namespace = "http://wonkarestservice.com")]
    public class SvcRuleTree
    {
        public SvcRuleTree()
        {
            RuleTreeId        = "";
            RulesEngine       = null;
            RuleTreeOriginUrl = null;
            ErrorMessage      = null;

            msGroveId  = null;
            mnGroveIdx = 0;
        }

        public SvcRuleTree(string psRuleTreeId, string psRuleTreeOriginUrl = "")
        {
            RuleTreeId        = psRuleTreeId;
            RulesEngine       = null;
            RuleTreeOriginUrl = psRuleTreeOriginUrl;
            ErrorMessage      = null;

            msGroveId  = null;
            mnGroveIdx = 0;
        }

        #region Properties

        public WonkaBreRulesEngine RulesEngine { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<SvcDataSource> CustomOperatorSources
        {
            get
            {
                if ((RulesEngine != null) && (RulesEngine.CustomOpMap != null))
                {
                    List<SvcDataSource> SourceList = new List<SvcDataSource>();

                    foreach (string sTmpSourceId in RulesEngine.CustomOpMap.Keys)
                    {
                        SvcDataSource TmpDataSource = new SvcDataSource(sTmpSourceId, RulesEngine.CustomOpMap[sTmpSourceId]);
                        SourceList.Add(TmpDataSource);
                    }

                    return SourceList;
                }
                else
                    return null;
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DefaultSource
        {
            get
            {
                if (RulesEngine != null)
                    return RulesEngine.DefaultSource;
                else
                    return "";
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string GroveId
        {
            get
            {
                if (RulesEngine != null)
                    return RulesEngine.GroveId;
                else
                    return msGroveId;
            }

            set
            {
                if (RulesEngine != null)
                    RulesEngine.GroveId = value;

                msGroveId = value;
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public uint GroveIndex
        {
            get
            {
                if (RulesEngine != null)
                    return RulesEngine.GroveIndex;
                else
                    return mnGroveIdx;
            }

            set
            {
                if (RulesEngine != null)
                    RulesEngine.GroveIndex = value;

                mnGroveIdx = value;
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RuleTreeId { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RuleTreeOriginUrl { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<SvcDataSource> AttributeSources
        {
            get
            {
                if ((RulesEngine != null) && (RulesEngine.SourceMap != null))
                {
                    List<SvcDataSource> SourceList = new List<SvcDataSource>();

                    foreach (string sTmpSourceId in RulesEngine.SourceMap.Keys)
                    {
                        SvcDataSource TmpDataSource = new SvcDataSource(sTmpSourceId, RulesEngine.SourceMap[sTmpSourceId]);
                        SourceList.Add(TmpDataSource);
                    }

                    return SourceList;
                }
                else
                    return null;
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public WonkaBreTransactionState TrxState
        {
            get
            {
                if ((RulesEngine != null) && (RulesEngine.TransactionState != null))
                {
                    if (RulesEngine.TransactionState is WonkaBreTransactionState)
                        return (WonkaBreTransactionState) RulesEngine.TransactionState;
                    else
                        return null;
                }
                else
                    return null;
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool UsingOrchestrationMode
        {
            get
            {
                if (RulesEngine != null)
                    return RulesEngine.UsingOrchestrationMode;
                else
                    return false;
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorMessage { get; set; }

        #endregion

        #region Methods

        public bool IsValid()
        {
            bool bIsValid = true;

            if (String.IsNullOrEmpty(RuleTreeId))
                bIsValid = false;

            if (String.IsNullOrEmpty(RuleTreeOriginUrl))
                bIsValid = false;            

            return bIsValid;
        }

        #endregion

        #region Members

        private string msGroveId;
        
        private uint   mnGroveIdx;

        #endregion

    }
}
 