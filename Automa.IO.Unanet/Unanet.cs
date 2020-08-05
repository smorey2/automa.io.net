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
                if (name == Una.Options.DefaultOrg.name) { key = Una.Options.DefaultOrg.key; return true; }
                return CostCenters.Value.TryGetValue(name, out key);
            }
            public readonly static Lazy<Dictionary<string, string>> CostCenters = new Lazy<Dictionary<string, string>>(() => OrganizationModel.GetListAsync(Una, "COST CENTER").GetAwaiter().GetResult().ToDictionary(x => x.Key, x => x.Value.Item1));
            public readonly static Lazy<Dictionary<string, string>> Vendors = new Lazy<Dictionary<string, string>>(() => OrganizationModel.GetListAsync(Una, "VENDOR").GetAwaiter().GetResult().ToDictionary(x => x.Key, x => x.Value.Item1));

            // LOCATION
            public readonly static Lazy<Dictionary<string, string>> Locations = new Lazy<Dictionary<string, string>>(() => LocationModel.EnsureAndReadAsync(Una, LookupPath).GetAwaiter().GetResult().ToDictionary(x => x.location, x => x.key));

            // APPROVALGROUP
            public readonly static Lazy<Dictionary<string, string>> TimeExpense = new Lazy<Dictionary<string, string>>(() => Una.GetAutoCompleteAsync("PERSON_PROFILE_TIME_EXPENSE_APP_GROUP").GetAwaiter().GetResult().ToDictionary(x => x.Value, x => x.Key));

            // LABORCATEGORY
            public readonly static Lazy<Dictionary<string, string>> LaborCategories = new Lazy<Dictionary<string, string>>(() => LaborCategoryModel.EnsureAndReadAsync(Una, LookupPath).GetAwaiter().GetResult().ToDictionary(x => x.labor_category, x => x.key));

            // RECEIVABLE
            public readonly static Lazy<Dictionary<string, string>> BankAccount = new Lazy<Dictionary<string, string>>(() => Una.GetAutoCompleteAsync("CP_BANK_ACCOUNT", legalEntityKey: Una.Options.DefaultOrg.key).GetAwaiter().GetResult().ToDictionary(x => x.Value, x => x.Key));

            // VENDORPROFILE
            //public readonly static Lazy<ILookup<string, string>> VendorProfiles = new Lazy<ILookup<string, string>>(() => VendorProfileModel.EnsureAndReadAsync(Ctx, LookupPath).GetAwaiter().GetResult().ToLookup(x => x.organization_code, x => x.key));
        }
    }
}