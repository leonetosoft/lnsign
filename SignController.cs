namespace lnsign
{
    using System;
    using System.Web.Http;

    public class ResponseSign
    {
        public string Base64 { get; set; }
    }

    public class SignController : ApiController
    {

 
       // POST api/values 
        public WsResponse<ResponseSign> Post([FromBody]SignDocumentRequestBody value)
        {
            try
            {
                return new WsResponse<ResponseSign>
                {
                    code = 1,
                    data = SignManager.Instancia.SignDoc(value),
                    msg = "Arquivo assinado com sucesso!"
                };
            }
            catch(Exception e)
            {
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
