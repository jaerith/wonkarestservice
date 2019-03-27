using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.ABI.Model;
using Nethereum.Contracts;

using WonkaBre;
using WonkaRef;
using WonkaPrd;

using WonkaRestService.Cache;
using WonkaRestService.Models;

namespace WonkaRestService.Extensions
{
    /// <summary>
    /// 
    /// This class represents the aggregated output of the rules engine on the blockchain.
    ///
    /// </summary>
    [FunctionOutput]
    public class RuleTreeReport
    {
        [Parameter("uint", "fails", 1)]
        public uint NumberOfRuleFailures { get; set; }

        [Parameter("bytes32[]", "rsets", 2)]
        public List<string> RuleSetIds { get; set; }

        [Parameter("bytes32[]", "rules", 3)]
        public List<string> RuleIds { get; set; }
    }

    public static class WonkaServiceExtensions
    {
        #region CONSTANTS

        public const string CONST_CONTRACT_FUNCTION_EXEC_RPT     = "executeWithReport"; 
        public const string CONST_CONTRACT_FUNCTION_GET_LAST_RPT = "getLastRuleReport";
        public const string CONST_CONTRACT_FUNCTION_HAS_RT       = "hasRuleTree";        

        #endregion 

        public static RuleTreeReport InvokeWithReport(this SvcRuleTree poRuleTree, Contract poWonkaContract, string psWeb3HttpUrl = "")
        {
            WonkaRefEnvironment RefEnv = WonkaRefEnvironment.GetInstance();

            var executeWithReportFunction    = poWonkaContract.GetFunction(CONST_CONTRACT_FUNCTION_EXEC_RPT);
            var executeGetLastReportFunction = poWonkaContract.GetFunction(CONST_CONTRACT_FUNCTION_GET_LAST_RPT);

            RuleTreeReport ruleTreeReport = null;

            string sRuleTreeOwnerAddress = "";

            if (!String.IsNullOrEmpty(poRuleTree.OwnerName) && WonkaServiceCache.GetInstance().TreeOwnerCache.ContainsKey(poRuleTree.OwnerName))
                sRuleTreeOwnerAddress = WonkaServiceCache.GetInstance().TreeOwnerCache[poRuleTree.OwnerName].OwnerAddress;
            else
            {
                // NOTE: Should probably default to another value or throw an exception here
                sRuleTreeOwnerAddress = poRuleTree.AttributeSources[0].BlockchainSenderAddress;
            }

            // NOTE: Caused exception to be thrown
            // var gas = executeWithReportFunction.EstimateGasAsync(msSenderAddress).Result;
            var gas = new Nethereum.Hex.HexTypes.HexBigInteger(2000000);

            var receiptAddAttribute = 
                executeWithReportFunction.SendTransactionAsync(sRuleTreeOwnerAddress, gas, null, sRuleTreeOwnerAddress).Result;

            ruleTreeReport = executeGetLastReportFunction.CallDeserializingToObjectAsync<RuleTreeReport>().Result;

            return ruleTreeReport;
        }

        public static string GetAttributeValue(this WonkaProduct poTargetProduct, WonkaRefAttr poTargetAttr)
        {
            string sAttrValue = "";

            //if (poTargetProduct.GetProductGroup(poTargetAttr.GroupId).GetRowCount() <= 0)
            //    throw new Exception("ERROR!  Provided incoming product has empty group.");

            if (poTargetProduct.GetProductGroup(poTargetAttr.GroupId).GetRowCount() > 0)
            {
                if (poTargetProduct.GetProductGroup(poTargetAttr.GroupId)[0].ContainsKey(poTargetAttr.AttrId))
                    sAttrValue = poTargetProduct.GetProductGroup(poTargetAttr.GroupId)[0][poTargetAttr.AttrId];

                //if (String.IsNullOrEmpty(sAttrValue))
                //    throw new Exception("ERROR!  Provided incoming product has no value for needed key(" + poTargetAttr.AttrName + ").");
            }

            return sAttrValue;
        }

        public static Contract GetContract(this WonkaBre.RuleTree.WonkaBreSource poSource, string psWeb3HttpUrl = "")
        {
            var account = new Nethereum.Web3.Accounts.Account(poSource.Password);
            
            Nethereum.Web3.Web3 web3 = null;
            if (!String.IsNullOrEmpty(psWeb3HttpUrl))
                web3 = new Nethereum.Web3.Web3(account, psWeb3HttpUrl);
            else
                web3 = new Nethereum.Web3.Web3(account);

            var contract = web3.Eth.GetContract(poSource.ContractABI, poSource.ContractAddress);

            return contract;
        }

        public static void SerializeProductData(this WonkaProduct poTargetProduct, WonkaBreRulesEngine poRulesEngine, string psWeb3HttpUrl = "")
        {
            try
            { WonkaRefEnvironment.GetInstance(); }
            catch (Exception ex)
            { WonkaRefEnvironment.CreateInstance(false, new WonkaRestService.WonkaData.WonkaMetadataVATSource()); }

            WonkaRefEnvironment WonkaRefEnv = WonkaRefEnvironment.GetInstance();

            WonkaRefGroup MainGroup = WonkaRefEnv.GetGroupByGroupName("Main");

            foreach (WonkaPrdGroupDataRow TempDataRow in poTargetProduct.ProductGroups[MainGroup.GroupId])
            {
                foreach (int nTempAttrId in TempDataRow.Keys)
                {
                    WonkaRefAttr TempAttribute = WonkaRefEnv.GetAttributeByAttrId(nTempAttrId);

                    string sAttrValue = TempDataRow[nTempAttrId];

                    if (!String.IsNullOrEmpty(sAttrValue))
                    {
                        if (poRulesEngine.SourceMap.ContainsKey(TempAttribute.AttrName))
                            poRulesEngine.SourceMap[TempAttribute.AttrName].SerializeProductData(TempAttribute.AttrName, sAttrValue, psWeb3HttpUrl);
                    }
                }
            }
        }

        public static void SerializeProductData(this WonkaBre.RuleTree.WonkaBreSource poSource, string psAttrName, string psAttrValue, string psWeb3HttpUrl = "")
        {
            var Contract = poSource.GetContract(psWeb3HttpUrl);

            var setAttrFunction = Contract.GetFunction(poSource.SetterMethodName);

            // NOTE: Caused exception to be thrown
            var gas = setAttrFunction.EstimateGasAsync(psAttrName, psAttrValue).Result;
            // var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1500000);

            var result =
                setAttrFunction.SendTransactionAsync(poSource.SenderAddress, gas, null, psAttrName, psAttrValue).Result;
        }

        public static void SetGroveData(this Dictionary<string, SvcGrove> poGroveCache, SvcRuleTree poTargetRuleTree)
        {
            string sGroveId =
                poGroveCache.Where(x => x.Value.RuleTreeMembers.Contains(poTargetRuleTree.RuleTreeId)).FirstOrDefault().Key;

            if (!String.IsNullOrEmpty(sGroveId))
            {
                List<string> GroveTreeList = poGroveCache[sGroveId].RuleTreeMembers;
                if ((GroveTreeList != null) && (GroveTreeList.Count > 0))
                {
                    poTargetRuleTree.GroveId    = sGroveId;
                    poTargetRuleTree.GroveIndex = (uint) GroveTreeList.IndexOf(poTargetRuleTree.RuleTreeId);
                }
            }
        }

        public static void SetAttribute(this WonkaProduct poTargetProduct, WonkaRefAttr poTargetAttr, string psTargetValue)
        {
            if (poTargetProduct.GetProductGroup(poTargetAttr.GroupId).GetRowCount() <= 0)
                poTargetProduct.GetProductGroup(poTargetAttr.GroupId).AppendRow();

            poTargetProduct.GetProductGroup(poTargetAttr.GroupId)[0][poTargetAttr.AttrId] = psTargetValue;
        }

        public static Hashtable TransformToTrxRecord(this IDictionary<string, string> poRecord)
        {
            Hashtable oRecord = new Hashtable();

            try
            { WonkaRefEnvironment.GetInstance(); }
            catch (Exception ex)
            { WonkaRefEnvironment.CreateInstance(false, new WonkaRestService.WonkaData.WonkaMetadataVATSource()); }

            WonkaRefEnvironment WonkaRefEnv = WonkaRefEnvironment.GetInstance();

            foreach (string sKeyName in poRecord.Keys)
            {
                WonkaRefAttr TempAttribute = WonkaRefEnv.GetAttributeByAttrName(sKeyName);

                // NOTE: This section should be put back when we're testing for real
                //if (TempAttribute == null)
                //    throw new Exception("ERROR!  Key(" + sKeyName + ") does not map to any known attributes in the metadata.");

                string sAttrValue = poRecord[sKeyName];

                // NOTE: Test the value here

                if (!String.IsNullOrEmpty(sAttrValue))
                {
                    oRecord[sKeyName] = sAttrValue;

                    /*
                    ** NOTE: We can use this section, if we start to use data structures
                    **
                   object oAttrValue = sAttrValue;

                   Type oAttrType = poRecord.GetType().GetProperty(sPropName).PropertyType;
                    var targetType = IsNullableType(oAttrType) ? Nullable.GetUnderlyingType(oAttrType) : oAttrType;

                    if (sPropName.StartsWith("Is"))
                    {
                        bool bPropAttrValue = false;
                        if (!String.IsNullOrEmpty(sAttrValue) && (sAttrValue.ToUpper() == "Y"))
                            bPropAttrValue = true;

                        poRecord.GetType().GetProperty(sPropName).SetValue(poRecord, bPropAttrValue, null);
                    }
                    else
                    {
                        var oPropAttrValue = Convert.ChangeType(oAttrValue, targetType);

                        poRecord.GetType().GetProperty(sPropName).SetValue(poRecord, oPropAttrValue, null);
                    }
                     **/
                }            
            } // end of for loop

            return oRecord;
        }

        public static Hashtable TransformToTrxRecord(this WonkaProduct poProduct)
        {
            Hashtable oRecord = new Hashtable();

            try
            { WonkaRefEnvironment.GetInstance(); }
            catch (Exception ex)
            { WonkaRefEnvironment.CreateInstance(false, new WonkaRestService.WonkaData.WonkaMetadataVATSource()); }

            WonkaRefEnvironment WonkaRefEnv = WonkaRefEnvironment.GetInstance();

            WonkaRefGroup MainGroup = WonkaRefEnv.GetGroupByGroupName("Main");

            foreach (WonkaPrdGroupDataRow TempDataRow in poProduct.ProductGroups[MainGroup.GroupId])
            {
                foreach (int nTempAttrId in TempDataRow.Keys)
                {
                    WonkaRefAttr TempAttribute = WonkaRefEnv.GetAttributeByAttrId(nTempAttrId);

                    string sAttrValue = TempDataRow[nTempAttrId];

                    if (!String.IsNullOrEmpty(sAttrValue))
                        oRecord[TempAttribute.AttrName] = sAttrValue;
                }
            }

            return oRecord;
        }

        public static WonkaProduct TransformToWonkaProduct(this IDictionary<string, string> poRecord, bool bApplyDefaults = true)
        {
            try
            {
                WonkaRefEnvironment.GetInstance();
            }
            catch (Exception ex)
            {
                WonkaRefEnvironment.CreateInstance(false, new WonkaRestService.WonkaData.WonkaMetadataVATSource());
            }

            WonkaRefEnvironment WonkaRefEnv = WonkaRefEnvironment.GetInstance();

            WonkaProduct WonkaRecord = new WonkaProduct();

            if (bApplyDefaults)
            {
                // Apply default values (which is necessary for the Wonka .NET engine)
                foreach (WonkaRefAttr TempAttr in WonkaRefEnv.AttrCache)
                {
                    string sCurrValue = WonkaRecord.GetAttributeValue(TempAttr);

                    if (String.IsNullOrEmpty(sCurrValue))
                    {
                        if (TempAttr.IsDecimal)
                            WonkaRecord.SetAttribute(TempAttr, "0.00");
                        else if (TempAttr.IsNumeric)
                            WonkaRecord.SetAttribute(TempAttr, "0");
                        else
                            WonkaRecord.SetAttribute(TempAttr, "???");
                    }
                }
            }

            foreach (string sKeyName in poRecord.Keys)
            {
                WonkaRefAttr TempAttribute = WonkaRefEnv.GetAttributeByAttrName(sKeyName);

                string sAttrValue = poRecord[sKeyName];

                // NOTE: Test the value here

                if ((TempAttribute != null) && !String.IsNullOrEmpty(sAttrValue))
                    WonkaRecord.SetAttribute(TempAttribute, sAttrValue);
            }

            return WonkaRecord;
        }

    }
}