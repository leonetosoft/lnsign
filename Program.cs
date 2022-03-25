using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using System.Net.Http;
using System.Windows.Forms;
using System.Threading;

namespace lnsign
{
    public class CustomHeaderHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken)
                .ContinueWith((task) =>
                {
                    HttpResponseMessage response = task.Result;
                    response.Headers.Add("Access-Control-Allow-Origin", "*");
                    return response;
                });
        }
    }

    static class Program
    {
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            new Thread(() => {
                var form = new Form1();
                Application.Run(form);
           }).Start();
            //Application.Run(new Form1());

            var config = new HttpSelfHostConfiguration("http://localhost:8888");

            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());
            config.Formatters.JsonFormatter.SerializerSettings =
            new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            config.MessageHandlers.Add(new CustomHeaderHandler());
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());
            config.Routes.MapHttpRoute(
            "API Default", "api/{controller}/{id}",
            new { id = RouteParameter.Optional });


            HttpSelfHostServer server = new HttpSelfHostServer(config);
            server.OpenAsync().Wait();
            /*using (HttpSelfHostServer server = new HttpSelfHostServer(config))
            {
                server.OpenAsync().Wait();
                Console.WriteLine("Press Enter to quit.");
                Console.ReadLine();
                Console.ReadLine();
            }*/
            //Application.Run(new Form1());
        }
    }
}
