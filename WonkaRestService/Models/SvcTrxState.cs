using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

using Nethereum.Hex.HexConvertors.Extensions;

using Wonka.BizRulesEngine.Permissions;

namespace WonkaRestService.Models
{
    [DataContract(Namespace = "http://wonkarestservice.com")]
    public class SvcTrxState : WonkaBizTransactionState
    {
        #region CONSTANTS

        public const string DUMMY_VALUE = "Blank";

        #endregion

        /**
         ** NOTE: Empty constructor is needed for deserialization
         **/
        public SvcTrxState() :
            base(new HashSet<string>(){ DUMMY_VALUE }, 0, "")
        {
            SerializeToBlockchain = null;

            RuleTreeId = "";

            ErrorMessage = StackTraceMessage = null;

            Owners = null;
        }

        public SvcTrxState(IEnumerable<string> poOwners, uint pnMinReqScoreForApproval = 0, string psContractAddress = null) :
            base(poOwners, pnMinReqScoreForApproval, psContractAddress)
        {
            SerializeToBlockchain = null;

            RuleTreeId = "";

            ErrorMessage = StackTraceMessage = null;

            Owners = null;
        }

        public SvcTrxState(string psRulesEngineId, WonkaBizTransactionState poTrxState, HashSet<string> poAllOwners) :
            base(poAllOwners, poTrxState.GetMinScoreRequirement(), poTrxState.ContractAddress)
        {
            SerializeToBlockchain = null;

            RuleTreeId = psRulesEngineId;

            ErrorMessage = StackTraceMessage = null;

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
        public List<SvcTrxStateOwner> Owners { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RuleTreeId { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? SerializeToBlockchain { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorMessage { get; set; }

        [DataMember, XmlElement(IsNullable = false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string StackTraceMessage { get; set; }

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
            if (base.IsOwner(DUMMY_VALUE))
                base.RemoveOwner(DUMMY_VALUE);

            if (Owners != null)
            {
                var ServiceCache = Cache.WonkaServiceCache.GetInstance();

                foreach (SvcTrxStateOwner TmpOwner in Owners)
                {
                    string sTrxOwner = TmpOwner.OwnerName;

                    // We need to convert these user-friendly name to addresses/hashes, so that we can serialize the state to the chain
                    if (ServiceCache.TreeOwnerCache.ContainsKey(sTrxOwner))
                        sTrxOwner = ServiceCache.TreeOwnerCache[sTrxOwner].OwnerAddress;
                    else if (!sTrxOwner.HasHexPrefix())
                        throw new Exception("ERROR!  The provided owner(" + sTrxOwner + ") is not an blockchain address, nor is it a lookup value.");

                    this.SetOwner(sTrxOwner, TmpOwner.OwnerWeight);

                    if (TmpOwner.ConfirmedTransaction)
                        this.AddConfirmation(sTrxOwner);
                }
            }
            else
            {
                Owners = new List<SvcTrxStateOwner>();

                foreach (string TmpConfirmedOwner in base.GetOwnersConfirmed())
                    Owners.Add(new SvcTrxStateOwner(TmpConfirmedOwner, true, GetOwnerWeight(TmpConfirmedOwner)));

                foreach (string TmpUnconfirmedOwner in base.GetOwnersUnconfirmed())
                    Owners.Add(new SvcTrxStateOwner(TmpUnconfirmedOwner, false, GetOwnerWeight(TmpUnconfirmedOwner)));
            }
        }

        #endregion
    }

}