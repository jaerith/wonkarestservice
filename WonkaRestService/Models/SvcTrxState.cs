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
            base(new HashSet<string>(){"Dummy"}, 1, "")
        {
            RuleTreeId = "";

            ErrorMessage = null;
        }

        public SvcTrxState(IEnumerable<string> poOwners, uint pnMinReqScoreForApproval = 0, string psContractAddress = null) :
            base(poOwners, pnMinReqScoreForApproval, psContractAddress)
        {
            RuleTreeId = "";

            ErrorMessage = null;
        }

        public SvcTrxState(string psRulesEngineId, WonkaBreTransactionState poTrxState, HashSet<string> poAllOwners) :
            base(poAllOwners, poTrxState.GetMinScoreRequirement(), poTrxState.ContractAddress)
        {
            RuleTreeId = psRulesEngineId;

            ErrorMessage = null;
        }

        #region Properties

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool Confirmed
        {
            get
            {
                return this.IsTransactionConfirmed();
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public uint CurrentScore
        {
            get
            {
                return this.GetCurrentScore();
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorMessage { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public uint MinimumScore
        {
            get
            {
                return this.GetMinScoreRequirement();
            }

            set
            {
                if (value > 0)
                    this.SetMinScoreRequirement(value);
            }
        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<TrxStateOwner> Owners
        { 
            get
            {
                List<TrxStateOwner> CurrOwners = new List<TrxStateOwner>();

                foreach (string TmpConfirmedOwner in this.GetOwnersConfirmed())
                    CurrOwners.Add(new TrxStateOwner(TmpConfirmedOwner, true, GetOwnerWeight(TmpConfirmedOwner)));

                foreach (string TmpUnconfirmedOwner in this.GetOwnersUnconfirmed())
                    CurrOwners.Add(new TrxStateOwner(TmpUnconfirmedOwner, false, GetOwnerWeight(TmpUnconfirmedOwner)));

                return CurrOwners;
            }

            set
            {
                if ((value != null) && (value.Count > 0))
                {
                    foreach (TrxStateOwner TmpOwner in value)
                    {
                        this.SetOwner(TmpOwner.OwnerName, TmpOwner.OwnerWeight);

                        if (TmpOwner.ConfirmedTransaction)
                            this.AddConfirmation(TmpOwner.OwnerName);
                    }
                }
            }

        }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RuleTreeId { get; set; }

        #endregion

    }

    public class TrxStateOwner
    {
        public TrxStateOwner(string psOwnerName, bool pbConfirmedTrx, uint pnOwnerWeight)
        {
            OwnerName            = psOwnerName;
            ConfirmedTransaction = pbConfirmedTrx;
            OwnerWeight          = pnOwnerWeight;
        }

        public string OwnerName { get; set; }

        public bool ConfirmedTransaction { get; set; }

        public uint OwnerWeight { get; set; }
    }
}