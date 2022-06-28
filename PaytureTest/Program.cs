using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Microsoft.AspNetCore.WebUtilities;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace PaytureTest
{
    public class Program
    {
        private static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            
            XDocument? xDocument = await GetPaytureAnswerAsync();

            if (xDocument == null)
            {
                Console.WriteLine("Ошибка при выполнении запроса");
                return;
            }

            PaytureAnswer paytureAnswer = new PaytureAnswer();
           
            var root = xDocument.Root;

            if (root == null)
            {
                Console.WriteLine("Xml документ пуст");
                return;
            }

            try
            {
                paytureAnswer.Success = (bool)root.Attribute("Success");
                paytureAnswer.OrderId = (Guid)root.Attribute("OrderId");
                paytureAnswer.Key = (string)root.Attribute("Key");
                paytureAnswer.Amount = (int?)root.Attribute("Amount");
                Console.WriteLine(paytureAnswer.DisplayName);
            }

            catch (Exception ex)
            {
                Console.WriteLine("Отсутствует один из обязательных аттрибутов платежа, ", ex.Message);
            }

            //Console.WriteLine(xDocument);
        }

        private static async Task<XDocument?> GetPaytureAnswerAsync()
        {
            client.DefaultRequestHeaders.Accept.Clear();

            var curl = "https://sandbox3.payture.com/api/Pay";

            // Получаем уникальный ИД
            var orderGuid = Guid.NewGuid(); 

            Dictionary<string, string> requestParams = new Dictionary<string, string>()
            {
                {"Key", "Merchant" },
                {"Amount", "12451" },
                {"OrderId", orderGuid.ToString() },
                {"PayInfo", $"PAN=5218851946955484;EMonth=12;EYear=22;CardHolder=Ivan Ivanov;SecureCode=123;OrderId={orderGuid};Amount=12451"},
                {"CustomFields", "IP=212.8.155.68;Product=Ticket"}
            };

            //var newUrl = new Uri(QueryHelpers.AddQueryString(curl, requestParams));

            try
            {
                //HttpResponseMessage httpResponse = await client.GetAsync(newUrl);
                var encodedContent = new FormUrlEncodedContent(requestParams);
                HttpResponseMessage httpResponse = await client.PostAsync(curl, encodedContent);
                Stream stream = await httpResponse.Content.ReadAsStreamAsync();
                XDocument xDocument = XDocument.Load(stream);

                return xDocument;
            }

            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public class PaytureAnswer
        {
            public Guid OrderId { get; set; }

            public string Key { get; set; }

            public bool Success { get; set; }

            public int? Amount { get; set; }

            // Ост. параметры
            public string DisplayName => $"Статус операции: {Success};\n" +
                $"идентификатор платежа: {OrderId};\n" +
                $"наименование платежного терминала: {Key};\n" +
                $"{(Amount.HasValue ? "сумма платежа: " + Amount : "")}.";
        }
    }
}
