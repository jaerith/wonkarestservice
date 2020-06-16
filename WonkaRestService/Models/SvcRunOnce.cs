using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Web;
using Newtonsoft.Json;

namespace WonkaRestService.Models
{
    [DataContract(Namespace = "http://wonkarestservice.com")]
    public class SvcRunOnce
    {
        public SvcRunOnce(string psMetadataUrl, string psBizRulesUrl, string psAcct, string psPwd, string psWeb3Url)
        {
            MetadataUrl = psMetadataUrl;
            BizRulesUrl = psBizRulesUrl;
            Account     = psAcct;
            Password    = psPwd;
            Web3Url     = psWeb3Url;
        }

        #region Properties

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string MetadataUrl { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BizRulesUrl { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Account { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Password { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Web3Url { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ERC20ContractAddress { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ERC721ContractAddress { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public SvcBasicRecord Transaction { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorMessage { get; set; }

        #endregion
    }
}