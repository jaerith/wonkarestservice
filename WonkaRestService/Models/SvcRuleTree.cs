﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

using Wonka.BizRulesEngine;
using Wonka.BizRulesEngine.Permissions;
using Wonka.BizRulesEngine.RuleTree;

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
            OwnerName         = null;
            ErrorMessage      = null;

            msGroveId  = null;
            mnGroveIdx = 0;

            MinGasCost = MaxGasCost = null;

            SerializeToBlockchain = false;
        }

        public SvcRuleTree(string psRuleTreeId, string psRuleTreeOriginUrl = "")
        {
            RuleTreeId        = psRuleTreeId;
            RulesEngine       = null;
            RuleTreeOriginUrl = psRuleTreeOriginUrl;
            OwnerName         = null;
            ErrorMessage      = null;

            msGroveId  = null;
            mnGroveIdx = 0;

            MinGasCost = MaxGasCost = null;

            SerializeToBlockchain = false;
        }

        public SvcRuleTree(SvcRuleTree poOriginal)
        {
            RuleTreeId        = poOriginal.RuleTreeId;
            RulesEngine       = null;
            RuleTreeOriginUrl = poOriginal.RuleTreeOriginUrl;            
            OwnerName         = poOriginal.OwnerName;
            ErrorMessage      = null;

            SerializeToBlockchain = poOriginal.SerializeToBlockchain;

            msGroveId  = poOriginal.GroveId;
            mnGroveIdx = poOriginal.GroveIndex;

            MinGasCost = poOriginal.MinGasCost;
            MaxGasCost = poOriginal.MaxGasCost;

            SerializeToBlockchain = false;
        }

        #region Properties

        public WonkaBizRulesEngine RulesEngine { get; set; }

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
        public string OwnerName { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RuleTreeId { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RuleTreeOriginUrl { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public uint? MinGasCost { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public uint? MaxGasCost { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool SerializeToBlockchain { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public WonkaBizTransactionState TrxState
        {
            get
            {
                if ((RulesEngine != null) && (RulesEngine.TransactionState != null))
                {
                    if (RulesEngine.TransactionState is WonkaBizTransactionState)
                        return (WonkaBizTransactionState) RulesEngine.TransactionState;
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

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string StackTraceMessage { get; set; }

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
 