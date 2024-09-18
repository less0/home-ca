using System.Linq;
using FluentAssertions;
using home_ca_backend.Api.Tests.Drivers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reqnroll;

namespace home_ca_backend.Api.Tests.Steps;

[Binding]
public class ResponseBodySteps
{
    [Then(@"the response is an array without the ""(.*)"" field in the items")]
    public void ThenTheResponseIsAnArrayWithoutTheFieldInTheItems(string fieldName)
    {
        var array = JsonConvert.DeserializeObject<JArray>(Driver.Instance.LastResponseBody);
        array.Any(x => x[fieldName].ToObject<object?>() != null).Should().BeFalse();
    }

    [Then("the response is a well-formed PEM private key")]
    public void ThenTheResponseIsAWell_FormedPEMPrivateKey()
    {
        Driver.Instance.LastResponseBody.Should().StartWith("-----BEGIN ENCRYPTED PRIVATE KEY-----\n");
        Driver.Instance.LastResponseBody.Should().EndWith("\n-----END ENCRYPTED PRIVATE KEY-----");
    }

}