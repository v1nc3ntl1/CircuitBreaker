using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CircuitBreaker
{
    public class HttpRequestWatcher : IWatcher
    {
        public PipelineData<T> Evaluate<T>(Func<T> requestOperation)
        {
            var data = new PipelineData<T>();
            try
            {
                if (requestOperation == null)
                {
                    // todo handle this
                }
                
                data.Value = requestOperation();
                data.Success = true;
            }
            catch (WebException)
            {
                // todo: implement logic to handle different error code
                data.Success = false;
            }

            return data;
        }

    }
}
