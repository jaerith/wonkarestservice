using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WonkaRestService.Controllers
{
    public class RegistryController : ApiController
    {
        // GET: api/Registry
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Registry/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Registry
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Registry/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Registry/5
        public void Delete(int id)
        {
        }
    }
}
