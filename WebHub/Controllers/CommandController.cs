using MessagingGrainInterface;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebHub.Controllers
{
    public class CommandController : ApiController
    {
        private readonly IGrainFactory _grainFactory; 

        CommandController()
        {
            _grainFactory = GrainClient.GrainFactory;
        }

        [HttpGet]
        public async Task<string> GetResponse(int messageId, string reqMessage, string device)
        {
            var requestGrain = _grainFactory.GetGrain<IRequestGrain>(Guid.NewGuid());
            var responseGrain =
                await requestGrain.RequestForResponse(messageId, reqMessage, device);

            var response = await responseGrain.GetResultAsync();

            return response == null ? string.Empty : response.ResponseBody;
        }
    }
}
