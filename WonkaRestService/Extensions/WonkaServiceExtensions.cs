using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using WonkaRef;
using WonkaPrd;

namespace WonkaRestService.Extensions
{
    public static class WonkaServiceExtensions
    {
        public static string GetAttributeValue(this WonkaProduct poTargetProduct, WonkaRefAttr poTargetAttr)
        {
            if (poTargetProduct.GetProductGroup(poTargetAttr.GroupId).GetRowCount() <= 0)
                throw new Exception("ERROR!  Provided incoming product has empty group.");

            string sAttrValue = poTargetProduct.GetProductGroup(poTargetAttr.GroupId)[0][poTargetAttr.AttrId];

            if (String.IsNullOrEmpty(sAttrValue))
                throw new Exception("ERROR!  Provided incoming product has no value for needed key(" + poTargetAttr.AttrName + ").");

            return sAttrValue;
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

        public static WonkaProduct TransformToWonkaProduct(this IDictionary<string, string> poRecord)
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

        public static void SetAttribute(this WonkaProduct poTargetProduct, WonkaRefAttr poTargetAttr, string psTargetValue)
        {
            if (poTargetProduct.GetProductGroup(poTargetAttr.GroupId).GetRowCount() <= 0)
                poTargetProduct.GetProductGroup(poTargetAttr.GroupId).AppendRow();

            poTargetProduct.GetProductGroup(poTargetAttr.GroupId)[0][poTargetAttr.AttrId] = psTargetValue;
        }

    }
}