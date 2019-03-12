using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

using WonkaBre.Permissions;

namespace WonkaRestService.Models
{
    [DataContract(Namespace = "http://wonkarestservice.com")]
    public class SvcTrxState : WonkaBreTransactionState
    {
        /**
         ** NOTE: Empty constructor is needed for deserialization
         **/
        public SvcTrxState() :
            base(new HashSet<string>(){"Blank"}, 0, "")
        {
            RuleTreeId = "";

            ErrorMessage = null;

            Owners = null;
        }

        public SvcTrxState(IEnumerable<string> poOwners, uint pnMinReqScoreForApproval = 0, string psContractAddress = null) :
            base(poOwners, pnMinReqScoreForApproval, psContractAddress)
        {
            RuleTreeId = "";

            ErrorMessage = null;

            Owners = null;
        }

        public SvcTrxState(string psRulesEngineId, WonkaBreTransactionState poTrxState, HashSet<string> poAllOwners) :
            base(poAllOwners, poTrxState.GetMinScoreRequirement(), poTrxState.ContractAddress)
        {
            RuleTreeId = psRulesEngineId;

            ErrorMessage = null;

            Owners = null;

            // Need to populate the confirmations
            poTrxState.GetOwnersConfirmed().ToList().ForEach(x => base.AddConfirmation(x));
        }

        #region Properties

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool Confirmed
        {
            get
            {
                return base.IsTransactionConfirmed();
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public uint CurrentScore
        {
            get
            {
                return base.GetCurrentScore();
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorMessage { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public uint MinimumScore
        {
            get
            {
                return base.GetMinScoreRequirement();
            }

            set
            {
                if (value > 0)
                    base.SetMinScoreRequirement(value);
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<TrxStateOwner> Owners { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RuleTreeId { get; set; }

        #endregion

        #region Methods

        public bool IsValid()
        {
            bool bIsValid = true;

            if (String.IsNullOrEmpty(RuleTreeId))
                bIsValid = false;

            return bIsValid;
        }

        public void RefreshSvcOwnerList()
        {
            if (Owners != null)
            {
                foreach (TrxStateOwner TmpOwner in Owners)
                {
                    this.SetOwner(TmpOwner.OwnerName, TmpOwner.OwnerWeight);

                    if (TmpOwner.ConfirmedTransaction)
                        this.AddConfirmation(TmpOwner.OwnerName);
                }
            }
            else
            {
                if (base.IsOwner("Blank"))
                    base.RemoveOwner("Blank");

                Owners = new List<TrxStateOwner>();

                foreach (string TmpConfirmedOwner in base.GetOwnersConfirmed())
                    Owners.Add(new TrxStateOwner(TmpConfirmedOwner, true, GetOwnerWeight(TmpConfirmedOwner)));

                foreach (string TmpUnconfirmedOwner in base.GetOwnersUnconfirmed())
                    Owners.Add(new TrxStateOwner(TmpUnconfirmedOwner, false, GetOwnerWeight(TmpUnconfirmedOwner)));
            }
        }

        #endregion
    }

    [DataContract(Namespace = "http://wonkarestservice.com")]
    public class TrxStateOwner
    {
        public TrxStateOwner(string psOwnerName, bool pbConfirmedTrx, uint pnOwnerWeight)
        {
            OwnerName            = psOwnerName;
            ConfirmedTransaction = pbConfirmedTrx;
            OwnerWeight          = pnOwnerWeight;
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string OwnerName { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool ConfirmedTransaction { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public uint OwnerWeight { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RuleTreeId { get; set; }
    }
}