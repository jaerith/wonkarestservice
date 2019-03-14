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
    public class SvcTrxStateOwner
    {
        public SvcTrxStateOwner(string psOwnerName, bool pbConfirmedTrx, uint pnOwnerWeight)
        {
            OwnerName            = psOwnerName;
            ConfirmedTransaction = pbConfirmedTrx;
            OwnerWeight          = pnOwnerWeight;

            RuleTreeId = ErrorMessage = null;
        }

        #region Properties

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string OwnerName { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool ConfirmedTransaction { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public uint OwnerWeight { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RuleTreeId { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorMessage { get; set; }

        #endregion

        #region Methods

        public bool IsValid()
        {
            bool bIsValid = true;

            if (String.IsNullOrEmpty(RuleTreeId))
                bIsValid = false;

            if (String.IsNullOrEmpty(OwnerName))
                bIsValid = false;

            return bIsValid;
        }

        #endregion
    }
}