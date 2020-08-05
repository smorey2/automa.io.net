using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Automa.IO.Unanet.Records
{
    public class CustomerPaymentModel : ModelBase
    {
        public string CpKey { get; set; }
        public string CpDoc { get; set; }
        public string Id { get; set; }
        public string OrganizationCode { get; set; }
        public string CheckNumber { get; set; }
        public decimal CheckAmount { get; set; }
        public DateTime TxnDate { get; set; }
        public XElement Products { get; set; }

        public class p_CustomerPayment : CustomerPaymentModel { }

        public static async Task<(ChangedFields changed, string last)> ManageRecordAsync(UnanetClient una, p_CustomerPayment s, Action<string, string> setInfo, string legalEntityKey = null, string legalEntity = null, string bankAcct = "1003 - Capital City_A_CHK", Action<p_CustomerPayment> bespoke = null)
        {
            var _ = new ChangedFields(ManageFlags.None);
            if (!Unanet.Lookups.BankAccount.Value.TryGetValue(bankAcct, out var bankAcctKey))
                throw new InvalidOperationException($"Can not find: {bankAcct}");
            bespoke?.Invoke(s);
            // first
            string[] cps; string last = null;
            if (string.IsNullOrEmpty(s.CpKey))
            {
                (cps, last) = ((string[] cps, string last))await una.SubmitManageAsync(HttpMethod.Post, "accounts_receivable/customer_payment", null,
                    async (z, f) =>
                {
                    var customers = await Unanet.Una.GetAutoCompleteAsync("CP_CUSTOMER", $"{s.OrganizationCode} -", legalEntityKey: legalEntityKey ?? una.Options.DefaultOrg.key);
                    var customer = customers.Single();
                    f.FromSelect("legalEntity", legalEntity ?? una.Options.LegalEntity);
                    // customer payment
                    f.Values["bankAcct"] = bankAcct; f.Values["bankAcctKey"] = bankAcctKey;
                    f.Values["customer"] = customer.Value; f.Values["customerKey"] = customer.Key;
                    f.Values["paymentAmt"] = s.CheckAmount.ToString();
                    f.Values["description"] = "Check Payment to Acct 1101";
                    // document / post dates
                    f.Values["docDate"] = s.TxnDate.ToShortDateString();
                    f.Values["postDate"] = s.TxnDate.ToShortDateString();
                    // check post date
                    // payment options
                    f.FromSelect("paymentMethod", "A - Check");
                    f.Values["reference"] = s.CheckNumber;
                    // comments
                    f.Values["comments"] = null;
                    // submit
                    f.Values["button_clicked"] = "button_save";
                    f.Add("button", "button_save", null);
                }, valueFunc: x =>
                {
                    var cpKey = x.ExtractSpanInner("<input name=\"cpKey\" type=\"hidden\" value=\"", "\">") ?? string.Empty;
                    var cpDoc = x.ExtractSpanInner("Document #:&nbsp;", "<") ?? string.Empty;
                    return new[] { cpKey, cpDoc };
                });
                s.CpKey = cps[0]; s.CpDoc = cps[1];
                setInfo(s.CpKey, $"D:{s.CpDoc}");
            }
            if (string.IsNullOrEmpty(s.CpKey))
                return (_, last);
            // second
            var (r, last2) = await una.SubmitSubManageAsync("D", HttpMethod.Get, "accounts_receivable/customer_payment/included", null, //: POST
                $"cpKey={s.CpKey}", null,
                (z, f) =>
                {
                    var doc = z.ToHtmlDocument();
                    var rows = doc.DocumentNode.Descendants("tr")
                        .Where(x => x.Attributes["id"] != null && x.Attributes["id"].Value.StartsWith("i"))
                        .ToDictionary(
                            x => x.Attributes["id"].Value.Substring(1).Trim(),
                            x => x.Descendants("td").ToArray());
                    foreach (var product in s.Products.Elements("d").Select(x => new
                    {
                        Document = x.Attribute("i")?.Value,
                        Amount = decimal.Parse(x.Attribute("a")?.Value),
                    }))
                    {
                        var row = rows.SingleOrDefault(x => x.Value[4].InnerText == product.Document);
                        if (row.Key == null)
                            throw new InvalidOperationException($"Can not find document {product.Document}");
                        var postfix0 = row.Key;
                        var postfixA = row.Key != "0" ? $">{row.Key}" : string.Empty;
                        if (!f.Values.TryGetValue($"p_amounti{postfix0}", out var unanetAmountField))
                            throw new InvalidOperationException($"missing amount field: {$"p_amounti{postfix0}"}");
                        if (!decimal.TryParse(unanetAmountField, out var unanetAmount))
                            throw new InvalidOperationException($"unable to parse: {unanetAmountField}");
                        if (unanetAmount != product.Amount)
                            throw new InvalidOperationException($"unanet ${unanetAmount} and darwin ${product.Amount} do not match");
                        f.Checked[$"check{postfixA}"] = true;
                        f.Types[$"p_amounti{postfix0}"] = "text";
                        f.Types[$"w_amounti{postfix0}"] = "text";
                    }
                    f.Values["submitButton"] = "button_submit_next";
                    return f.ToString();
                });
            return (_, last2);
        }
    }
}