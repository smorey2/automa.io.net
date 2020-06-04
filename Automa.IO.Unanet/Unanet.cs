using Automa.IO.Unanet.Records;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Automa.IO.Unanet
{
    public class Unanet
    {
        static readonly WeakReference<UnanetClient> _una = new WeakReference<UnanetClient>(null);
        static string LookupPath { get; set; }
        public static UnanetClient Una => _una.TryGetTarget(out var target) ? target : null;

        public static void LookupsInitialize(UnanetClient una, string lookupPath = null)
        {
            _una.SetTarget(una);
            if (lookupPath != null)
                LookupPath = lookupPath;
        }

        public static class Lookups
        {
            // ORGANIZATION
            public static bool TryGetCostCentersAndDefault(string name, out string key)
            {
                if (name == Una.Settings.DefaultOrg.name) { key = Una.Settings.DefaultOrg.key; return true; }
                return CostCenters.Value.TryGetValue(name, out key);
            }
            public readonly static Lazy<Dictionary<string, string>> CostCenters = new Lazy<Dictionary<string, string>>(() => OrganizationModel.GetList(Una, "COST CENTER").ToDictionary(x => x.Key, x => x.Value.Item1));
            public readonly static Lazy<Dictionary<string, string>> Vendors = new Lazy<Dictionary<string, string>>(() => OrganizationModel.GetList(Una, "VENDOR").ToDictionary(x => x.Key, x => x.Value.Item1));

            // LOCATION
            public readonly static Lazy<Dictionary<string, string>> Locations = new Lazy<Dictionary<string, string>>(() => LocationModel.EnsureAndRead(Una, LookupPath).ToDictionary(x => x.location, x => x.key));

            // APPROVALGROUP
            public readonly static Lazy<Dictionary<string, string>> TimeExpense = new Lazy<Dictionary<string, string>>(() => Una.GetAutoComplete("PERSON_PROFILE_TIME_EXPENSE_APP_GROUP").ToDictionary(x => x.Value, x => x.Key));

            // LABORCATEGORY
            public readonly static Lazy<Dictionary<string, string>> LaborCategories = new Lazy<Dictionary<string, string>>(() => LaborCategoryModel.EnsureAndRead(Una, LookupPath).ToDictionary(x => x.labor_category, x => x.key));

            // RECEIVABLE
            public readonly static Lazy<Dictionary<string, string>> BankAccount = new Lazy<Dictionary<string, string>>(() => Una.GetAutoComplete("CP_BANK_ACCOUNT", legalEntityKey: Una.Settings.DefaultOrg.key).ToDictionary(x => x.Value, x => x.Key));

            // VENDORPROFILE
            //public readonly static Lazy<ILookup<string, string>> VendorProfiles = new Lazy<ILookup<string, string>>(() => VendorProfileModel.EnsureAndRead(Ctx, LookupPath).ToLookup(x => x.organization_code, x => x.key));
        }
    }
}