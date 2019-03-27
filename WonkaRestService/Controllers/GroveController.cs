using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WonkaRestService.Controllers
{
    public class GroveController : ApiController
    {
        // GET: api/Grove/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Grove
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Grove/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Grove/5
        public void Delete(int id)
        {
        }
    }
}
