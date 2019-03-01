using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WonkaRestService.Controllers
{
    public class TrxStateConfirmController : ApiController
    {
        // GET: api/TrxStateConfirm
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/TrxStateConfirm/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/TrxStateConfirm
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/TrxStateConfirm/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/TrxStateConfirm/5
        public void Delete(int id)
        {
        }
    }
}
