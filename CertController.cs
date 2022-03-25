
namespace lnsign
{
    using System.Collections.Generic;
    using System.Web.Http;


    public class CertController : ApiController
    {
        public WsResponse<IEnumerable<Cert>> GetCerts()
        {
            return new WsResponse<IEnumerable<Cert>>
            {
                code = 1,
                data = SignManager.Instancia.ListCerts(),
                msg = ""
            };
        }

    }
}
