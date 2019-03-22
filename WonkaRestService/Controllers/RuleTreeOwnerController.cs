using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WonkaRestService.Controllers
{
    public class RuleTreeOwnerController : ApiController
    {
        // GET: api/RuleTreeOwner
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/RuleTreeOwner/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/RuleTreeOwner
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/RuleTreeOwner/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/RuleTreeOwner/5
        public void Delete(int id)
        {
        }
    }
}
