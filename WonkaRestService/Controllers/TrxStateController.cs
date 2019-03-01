using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WonkaRestService.Controllers
{
    public class TrxStateController : ApiController
    {
        // GET: api/TrxState
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/TrxState/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/TrxState
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/TrxState/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/TrxState/5
        public void Delete(int id)
        {
        }
    }
}
