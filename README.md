# Wonka REST Service
This ASP.NET REST service will demonstrate how to use the [Wonka rules engine](https://github.com/Nethereum/Wonka), which provides the ability to simulate/execute rules within a .NET domain and execute them within the Ethereum blockchain.

![screenshot](https://github.com/jaerith/wonkarestservice/blob/master/images/api_swagger_screenshot.png)

# Features
Upon starting, this web service will present a number of different controllers, each of which provides an endpoint for different functionality in the Wonka:

1. **Account** -> This controller was part of the .NET template, but it could be used for securing the REST API.
2. **Chain** -> This controller allows the caller to retrieve metadata and data directly from the running testchain.
3. **Grove** -> This controller allows the caller to create a Grove, which will first create an instance in the .NET cache and then serialize it to the Registry contract on the testchain.  A Grove is a container for RuleTrees.
4. **Invoke** -> This controller will invoke a RuleTree.  The POST method will run it within the .NET domain; the PUT method will run it on the testchain.
5. **Registry** -> Under construction
6. **Report** -> This controller will retrieve the reports that are generated and cached by the Invoke controller.  (They are only stored in the .NET cache, not on the testchain.)
7. **RuleTree** -> This controller allows the caller to create a RuleTree, which will first create an instance in the .NET cache and then serialize it to the Wonka Engine on the testchain.
8. **RuleTreeOwner** -> This controller allows the caller to register a potential owner of a RuleTree, providing the details of their identity on the testchain.  (A RuleTreeOwner can only own one RuleTree.)
9. **TrxState** -> This controller allows the caller to instantiate a TrxState associated with an existing RuleTree.  A TrxState provides quorum functionality to a RuleTree (i.e., a number of people vote to allow a RuleTree's invocation).
10. **TrxStateConfirm** -> This controller allows a member of a TrxState to vote on the quorum of its RuleTree's invocation.
11. **TrxStateOwner** -> This controller will add a member to the TrxState, who can then take part in the quorum of a RuleTree's invocation.

# Quick Setup

1. You could run the Ethereum client of your own choice, but this project was developed with a specific Linux testchain and has numerous steps of initialization that depend on the testchain's usage.  So, unless you want to alter the initialization of the web service (which involves dozens of values in the configuration files), it is recommended that you use the 'ganache-linux' testchain found among [the Nethereum testchains](https://github.com/Nethereum/TestChains). 
2. Download [the Solidity portion](https://github.com/Nethereum/Wonka/tree/master/Solidity/WonkaEngine) of the Wonka project and then deploy the contracts to the Ethereum node by using the test script './Solidity/WonkaEngine/test/testdeploy.js'.
3. Once you have decided on the URL for the web service, set that URL as the value for &lt;Web3HttpUrl&gt; in the files 'VATCalculationExample.init.xml' and 'WonkaRegistry.init.xml' within the WonkaData folder of the service.
4. Publish the service and then run [the samples](https://github.com/jaerith/wonkarestservice/tree/master/WonkaRestService/SamplePostPutPayloads) in their designated order.  Enjoy!
