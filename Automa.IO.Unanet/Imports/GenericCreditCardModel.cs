using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace Automa.IO.Unanet.Imports
{
    public class GenericCreditCardModel
    {
        [DisplayName("Account")] public string Account5 { get; set; }
        [DisplayName("Posting Date")] public string PostingDate { get; set; }
        [DisplayName("Amount USD")] public string Amount { get; set; }
        [DisplayName("Supplier")] public string Supplier { get; set; }
        
        public static Task<string> ImportFileAsync(UnanetClient una, string sourceFolder, Stream set, string paymentMethod = "DEG UMB") =>
            Task.Run(() => una.PutEntitiesByImport(una.Imports["credit card - generic"], f =>
            {
                f.FromSelectByKey("outputOption", "errors");
                f.FromSelect("payment_method", paymentMethod);
                f.Values["filename"] = @"C:\GenericCreditCard.csv";
                f.Files["filename"] = set;
            }, sourceFolder));
    }
}