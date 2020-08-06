using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Automa.IO.Proxy
{
    public class WebHostResponse<T>
    {
        public bool Ok { get; set; }
        public object Data { get; set; }

        public async Task<WebHostResponse<T>> Handle(Func<Task<T>> action)
        {
            try
            {
                Ok = true;
                Data = await action();
                return this;
            }
            catch (Exception e)
            {
                Ok = false;
                Data = new ErrorResponse
                {
                    Error = e.Message,
                    Metadata = new ErrorResponse.ResponseMetadata { Messages = { e.StackTrace } },
                };
                return this;
            }
        }
    }
}
