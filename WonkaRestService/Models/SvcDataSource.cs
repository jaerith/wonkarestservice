using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

using WonkaBre.RuleTree;

namespace WonkaRestService.Models
{
    public class SvcDataSource
    {
        public SvcDataSource()
        {
            DataSource = null;
        }

        public SvcDataSource(string psAttrName, WonkaBreSource poSource)
        {
            AttributeName = psAttrName;
            DataSource    = poSource;

            if (DataSource != null)
                BlockchainSourceId = DataSource.SourceId;
        }

        #region Properties

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string APIServerAddress
        {
            get
            {
                if ((DataSource != null) && !String.IsNullOrEmpty(DataSource.APIServerAddress))
                    return DataSource.APIServerAddress;
                else
                    return null;
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? APIServerPort
        {
            get
            {
                if ((DataSource != null) && (DataSource.APIServerPort > 0))
                    return DataSource.APIServerPort;
                else
                    return null;
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AttributeName { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BlockchainContractAddress
        {
            get
            {
                if ((DataSource != null) && !String.IsNullOrEmpty(DataSource.ContractAddress))
                    return DataSource.ContractAddress;
                else
                    return null;
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BlockchainCustomOpMethodName
        {
            get
            {
                if ((DataSource != null) && !String.IsNullOrEmpty(DataSource.CustomOpMethodName))
                    return DataSource.CustomOpMethodName;
                else
                    return null;
            }
        }

        private WonkaBreSource DataSource;

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BlockchainGetValueMethod
        {
            get
            {
                if ((DataSource != null) && !String.IsNullOrEmpty(DataSource.MethodName))
                    return DataSource.MethodName;
                else
                    return null;
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BlockchainSenderAddress
        {
            get
            {
                if ((DataSource != null) && !String.IsNullOrEmpty(DataSource.SenderAddress))
                    return DataSource.SenderAddress;
                else
                    return null;
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BlockchainSetValueMethod
        {
            get
            {
                if ((DataSource != null) && !String.IsNullOrEmpty(DataSource.SetterMethodName))
                    return DataSource.SetterMethodName;
                else
                    return null;
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BlockchainSourceId { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BlockchainTypeOfSource
        {
            get
            {
                if (DataSource != null)
                {
                    if (DataSource.TypeOfSource == WonkaBre.SOURCE_TYPE.SRC_TYPE_CONTRACT)
                        return "Contract";
                    else
                        return "API";
                }
                else
                    return null;
            }
        }

        #endregion

    }
}