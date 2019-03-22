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
    public class SvcRuleTreeOwner
    {
        public SvcRuleTreeOwner(string psOwnerName, string psAddress, string psPassword)
        {
            OwnerName = psOwnerName;

            OwnerAddress  = psAddress;
            OwnerPassword = psPassword;
        }

        #region Properties

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string OwnerName { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string OwnerAddress { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string OwnerPassword { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorMessage { get; set; }

        #endregion

        #region Methods

        public bool IsValid()
        {
            bool bIsValid = true;

            if (String.IsNullOrEmpty(OwnerName))
                bIsValid = false;

            if (String.IsNullOrEmpty(OwnerAddress))
                bIsValid = false;

            if (String.IsNullOrEmpty(OwnerPassword))
                bIsValid = false;

            return bIsValid;
        }

        #endregion
    }
}