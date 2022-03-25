namespace lnsign
{
    interface HttpResponse
    {

    }

    public class WsResponse<T> : HttpResponse
    {
        public T data { get; set; }
        public int code { get; set; }
        public string msg { get; set; }
    }

    public class WsResponse: HttpResponse
    {
        public int code { get; set; }
        public string msg { get; set; }
    }
}
