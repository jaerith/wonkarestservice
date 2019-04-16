using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Web;

using Newtonsoft.Json;

using WonkaBre.Reporting;

using WonkaRestService.Extensions;

namespace WonkaRestService.Models
{
    public class SvcRuleTreeReport : WonkaBreRuleTreeReport
    {
        public SvcRuleTreeReport(bool pbSimulationMode, bool pbSerializeAllInfo = false)
        {
            SimulationMode = pbSimulationMode;

            mbSerializeAllInfo = pbSerializeAllInfo;

            moChainReport = null;

            InvocationTime = DateTime.Now;
        }

        public SvcRuleTreeReport(bool pbSimulationMode, WonkaEth.Extensions.RuleTreeReport poChainReport, bool pbSerializeAllInfo = false)
        {
            SimulationMode = pbSimulationMode;

            moChainReport = poChainReport;

            mbSerializeAllInfo = pbSerializeAllInfo;

            InvocationTime = DateTime.Now;
        }

        #region Properties

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? SimulationMode { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public uint? NumberOfFailures
        {
            get
            {
                if (mbSerializeAllInfo && (moChainReport != null))
                    return moChainReport.NumberOfRuleFailures;
                else
                    return null;
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> RuleSetsExecuted
        {
            get
            {
                if (mbSerializeAllInfo && (moChainReport != null))
                    return moChainReport.RuleSetIds;
                else
                    return null;
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> RulesExecuted
        {
            get
            {
                if (mbSerializeAllInfo && (moChainReport != null))
                    return moChainReport.RuleIds;
                else
                    return null;
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> RuleSetsWithWarnings
        {
            get
            {
                if (moChainReport != null)
                    return moChainReport.RuleSetWarnings;
                else
                    return null;
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> RuleSetsWithFailures
        {
            get
            {
                if (moChainReport != null)
                    return moChainReport.RuleSetFailures;
                else
                    return null;
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Hashtable RuleSetFailMessages
        {
            get
            {
                if (moChainReport != null)
                    return moChainReport.RuleSetFailMessages.TransformToTrxRecord();
                else
                    return null;
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorMessage { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Hashtable RecordData { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime InvocationTime { get; set; }

        #endregion

        #region Members

        protected bool mbSerializeAllInfo;

        protected WonkaEth.Extensions.RuleTreeReport moChainReport;

        #endregion

        #region Methods

        public bool IsValid()
        {
            bool bIsValid = true;

            // NOTE: Validation done here

            return bIsValid;
        }

        #endregion

    }
}