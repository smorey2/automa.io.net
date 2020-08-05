using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace Automa.IO.Unanet.Imports
{
    public class GenericCreditCardModel
    {
        [DisplayName("Account")] public string Account5 { get; set; }
        public string PaymentMethod { get; set; }
        [DisplayName("Posting Date")] public string PostingDate { get; set; }
        [DisplayName("Amount USD")] public string Amount { get; set; }
        [DisplayName("Supplier")] public string Supplier { get; set; }

        public static Task<string> ImportFileAsync(UnanetClient una, string sourceFolder, Stream set, string paymentMethod) =>
            Task.Run(() => una.PutEntitiesByImportAsync(una.Options.credit_card_generic.key, f =>
            {
                f.FromSelectByKey("outputOption", "errors");
                f.FromSelect("payment_method", paymentMethod);
                f.Values["filename"] = una.Options.credit_card_generic.file;
                f.Files["filename"] = set;
            }, sourceFolder));
    }
}