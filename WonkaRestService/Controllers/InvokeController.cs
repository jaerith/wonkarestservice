using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

using Nethereum.Web3.Accounts;

using WonkaBre;
using WonkaEth.Extensions;
using WonkaBre.RuleTree;
using WonkaPrd;
using WonkaRef;

namespace WonkaRestService.Controllers
{
    public class InvokeController : ApiController
    {
        // GET: api/Invoke
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Invoke/5
        public string Get(int id)
        {
            return "value";
        }

        /*
        // POST: api/Invoke
        public void Post([FromBody]string value)
        {
        }
        */

        public void Post([FromBody]IDictionary<string, string> poRecord)
        {
            var TmpAssembly = Assembly.GetExecutingAssembly();

            // Using the metadata source, we create an instance of a defined data domain
            WonkaRefEnvironment RefEnv =
                WonkaRefEnvironment.CreateInstance(false, new WonkaData.WonkaMetadataVATSource());

            // Read the XML markup that lists the business rules (i.e., the RuleTree)
            using (var RulesReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaRestService.WonkaData.VATCalculationExample.xml")))
            {
                string RulesContents = RulesReader.ReadToEnd();
            }
        }

        /*
        // PUT: api/Invoke/5
        public void Put(int id, [FromBody]string value)
        {
        }
        */

        // DELETE: api/Invoke/5
        public void Delete(int id)
        {
        }
    }
}