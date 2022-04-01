namespace lnsign
{
    using Newtonsoft.Json;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;

    public class ResponseSign
    {
        public string Base64 { get; set; }
    }

    public class SignController : ApiController
    {

 
       // POST api/values 
        public async Task<WsResponse<ResponseSign>> Post(HttpRequestMessage request)
        {
           
            try
            {
                var json = await request.Content.ReadAsStringAsync();
                //JavaScriptSerializer jss = new JavaScriptSerializer();
                
                SignDocumentRequestBody value = JsonConvert.DeserializeObject<SignDocumentRequestBody>(json);

                //SignDocumentRequestBody
                return new WsResponse<ResponseSign>
                {
                    code = 1,
                    data = SignManager.Instancia.SignDoc(value),
                    msg = "Arquivo assinado com sucesso!"
                };
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
               
                return new WsResponse<ResponseSign>
                {
                    code = 2,
                    data = null,
                    msg = e.Message,
                };
            }
           
        }


    }
}
