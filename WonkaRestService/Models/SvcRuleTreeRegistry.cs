using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Web;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Util;
using Newtonsoft.Json;

namespace WonkaRestService.Models
{
    [DataContract(Namespace = "http://wonkarestservice.com")]
    public class SvcRuleTreeRegistry
    {
        public SvcRuleTreeRegistry()
        {
            RuleTreeId = EngineContractAddress = EngineOwnerAddress = null;

            Init();
        }

        public SvcRuleTreeRegistry(string psRuleTreeId)
        {
            RuleTreeId = psRuleTreeId;

            EngineContractAddress = EngineOwnerAddress = null;

            Init();
        }

        public SvcRuleTreeRegistry(string psRuleTreeId, string psEngineCntrtAddr, string psEngineOwnerAddr)
        {
            RuleTreeId = psRuleTreeId;

            EngineContractAddress = psEngineCntrtAddr;

            EngineOwnerAddress = psEngineOwnerAddr;

            Init();
        }

        #region Methods

        private void Init()
        {
            Description = null;

            MaxGasCost = MinGasCost = null;

            AssociateContractAddresses = UsedCustomOps = UsedAttrNames = null;

            CreationEpochTime = 0;
        }

        #endregion

        #region Properties

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RuleTreeId { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string EngineContractAddress { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string EngineOwnerAddress { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public uint? MinGasCost { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public uint? MaxGasCost { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> AssociateContractAddresses { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> UsedCustomOps { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> UsedAttrNames { get; set; }

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
        public string ErrorMessage { get; set; }

        #endregion

        #region Methods

        public bool IsValid()
        {
            bool bIsValid = true;

            if (String.IsNullOrEmpty(RuleTreeId))
                bIsValid = false;

            if (String.IsNullOrEmpty(EngineContractAddress) && EngineContractAddress.HasHexPrefix())
                bIsValid = false;

            if (String.IsNullOrEmpty(EngineOwnerAddress) && EngineOwnerAddress.HasHexPrefix())
                bIsValid = false;

            return bIsValid;
        }

        #endregion
    }
}