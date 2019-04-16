using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace WonkaRestService.Models
{
    [DataContract(Namespace = "http://wonkarestservice.com")]
    public class SvcGrove
    {
        public SvcGrove()
        {
            RuleGrove = 
                new WonkaEth.Contracts.WonkaRuleGrove("blank") { GroveDescription = "", OwnerId = null };
           
            RuleTreeMembers = new HashSet<string>();

            CreationEpochTime = 0;

            SerializeToBlockchain = false;

            ErrorMessage = StackTraceMessage = null;
        }

        public SvcGrove(string psGroveId, string psGroveDesc = "")
        {
            RuleGrove =
                new WonkaEth.Contracts.WonkaRuleGrove(psGroveId) { GroveDescription = psGroveDesc, OwnerId = null };

            RuleTreeMembers = new HashSet<string>();

            CreationEpochTime = 0;

            SerializeToBlockchain = false;

            ErrorMessage = StackTraceMessage = null;
        }

        #region Properties

        public WonkaEth.Contracts.WonkaRuleGrove RuleGrove { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string GroveId
        {
            get { return RuleGrove.GroveId; }

            set { RuleGrove.GroveId = value; }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string GroveDescription
        {
            get { return RuleGrove.GroveDescription; }

            set { RuleGrove.GroveDescription = value; }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public HashSet<string> RuleTreeMembers { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string GroveOwner
        {
            get { return RuleGrove.OwnerId; }

            set { RuleGrove.OwnerId = value; }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public uint CreationEpochTime { get; set; }

        public DateTime CreationTime
        {
            get
            {
                DateTime ct = new DateTime(1970, 1, 1);

                ct = ct.AddSeconds(CreationEpochTime);

                return ct;
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool SerializeToBlockchain { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorMessage { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string StackTraceMessage { get; set; }

        #endregion

        #region Methods

        public bool IsValid()
        {
            bool bIsValid = true;

            if (String.IsNullOrEmpty(GroveId))
                bIsValid = false;

            return bIsValid;
        }

        #endregion

    }
}