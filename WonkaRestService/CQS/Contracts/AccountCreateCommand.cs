﻿using System;
using System.Collections.Generic;
using System.Reflection;

using Wonka.Eth.Contracts;
using Wonka.MetaData;

namespace WonkaRestService.CQS.Contracts
{
    public class AccountCreateCommand : ICommand
    {
        public string AccountId { get; set; }
        public string AccountName { get; set; }

        public AccountCreateCommand()
        { }

        public PropertyInfo[] GetProperties()
        {
            return this.GetType().GetProperties();
        }

        public Dictionary<PropertyInfo, WonkaRefAttr> GetPropertyMap()
        {
            WonkaRefEnvironment RefEnv = WonkaRefEnvironment.GetInstance();

            Dictionary<PropertyInfo, WonkaRefAttr> PropertyMap = new Dictionary<PropertyInfo, WonkaRefAttr>();

            foreach (PropertyInfo Prop in GetProperties())
            {
                if (Prop.Name == "AccountId")
                    PropertyMap[Prop] = RefEnv.GetAttributeByAttrName("BankAccountID");
                else if (Prop.Name == "AccountName")
                    PropertyMap[Prop] = RefEnv.GetAttributeByAttrName("BankAccountName");
            }

            return PropertyMap;
        }
    }
}
