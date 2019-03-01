using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WonkaRestService.Controllers
{
    public class RuleTreeController : ApiController
    {
        // GET: api/RuleTree
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/RuleTree/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/RuleTree
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/RuleTree/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/RuleTree/5
        public void Delete(int id)
        {
        }
    }
}
