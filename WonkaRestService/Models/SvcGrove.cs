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
            GroveId = GroveDescription = GroveOwner = ErrorMessage = null;

            RuleTreeMembers = new List<string>();

            CreationEpochTime = 0;
        }

        public SvcGrove(string psGroveId, string psGroveDesc = "")
        {
            GroveId          = psGroveId; 
            GroveDescription = psGroveDesc;

            GroveOwner = ErrorMessage = null;

            RuleTreeMembers = new List<string>();

            CreationEpochTime = 0;
        }

        #region Properties

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string GroveId { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string GroveDescription { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> RuleTreeMembers { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string GroveOwner { get; set; }

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

            if (String.IsNullOrEmpty(GroveId))
                bIsValid = false;

            return bIsValid;
        }

        #endregion

    }
}