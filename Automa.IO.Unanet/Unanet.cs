using Automa.IO.Unanet.Exports;
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

        #region Lookup

        public static class OrganizationLookup
        {
            public readonly static Dictionary<string, string> CostCenters = OrganizationModel.GetList(Una, "COST CENTER").ToDictionary(x => x.Key, x => x.Value.Item1);
            public readonly static Dictionary<string, string> Vendors = OrganizationModel.GetList(Una, "VENDOR").ToDictionary(x => x.Key, x => x.Value.Item1);
        }

        public static class LocationLookup
        {
            public readonly static Dictionary<string, string> Locations = LocationModel.EnsureAndRead(Una, LookupPath).ToDictionary(x => x.location, x => x.key);
        }

        public static class ApprovalGroupLookup
        {
            public readonly static Dictionary<string, string> TimeExpense = Una.GetAutoComplete("PERSON_PROFILE_TIME_EXPENSE_APP_GROUP").ToDictionary(x => x.Value, x => x.Key);
        }

        public static class LaborCategoryLookup
        {
            public readonly static Dictionary<string, string> LaborCategories = LaborCategoryModel.EnsureAndRead(Una, LookupPath).ToDictionary(x => x.labor_category, x => x.key);
        }

        public static class ReceivableLookup
        {
            public readonly static Dictionary<string, string> BankAccount = Una.GetAutoComplete("CP_BANK_ACCOUNT", legalEntityKey: "2845").ToDictionary(x => x.Value, x => x.Key);
        }

        //public static class VendorProfileLookup
        //{
        //    public readonly static ILookup<string, string> VendorProfiles = VendorProfileModel.EnsureAndRead(Ctx, LookupPath).ToLookup(x => x.organization_code, x => x.key);
        //}

        #endregion
    }
}