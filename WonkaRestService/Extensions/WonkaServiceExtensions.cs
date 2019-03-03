using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using WonkaRef;

namespace WonkaRestService.Extensions
{
    public static class WonkaServiceExtensions
    {
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
    }
}