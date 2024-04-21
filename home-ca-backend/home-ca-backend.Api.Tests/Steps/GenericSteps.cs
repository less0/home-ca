using System.Net;
using FluentAssertions;
using FluentAssertions.Execution;
using home_ca_backend.Api.Tests.Drivers;
using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TechTalk.SpecFlow.Assist;

namespace home_ca_backend.Api.Tests.Steps;

[Binding]
public class GenericSteps
{
    [When("the endpoint (.*) is called")]
    public async Task WhenTheEndpointIsCalled(string relativeUrl)
    {
        await Driver.Instance.SendHttpRequest(relativeUrl);
    }

    [Then("the status code should be (.*)")]
    public void ThenTheStatusCodeShouldBe(int expectedStatusCode)
    {
        Driver.Instance.LastStatusCode.Should().Be((HttpStatusCode)expectedStatusCode);
    }

    [Given(@"the user ""(.*)"" is authenticated with the password ""(.*)""")]
    public async Task GivenTheUserIsAuthenticatedWithThePassword(string username, string password)
    {
        await Driver.Instance.Authenticate(username, password);
    }

    [Then(@"the response is an empty array")]
    public void ThenTheResponseIsAnEmptyArray()
    {
        Driver.Instance.LastResponseBody.Should().Be("[]");
    }

    [Given(@"the following certificate authorities are registered:")]
    public void GivenTheFollowingCertificateAuthoritiesAreRegistered(Table table)
    {
        foreach (TableRow tableRow in table.Rows)
        {
            var id = Guid.Parse(tableRow["Id"]);
            string name = tableRow["Name"];
            Guid? parentId = string.IsNullOrWhiteSpace(tableRow["Parent"])
                ? null
                : Guid.Parse(tableRow["Parent"]);

            if (parentId == null)
            {
                Driver.Instance.RawDatabaseAccess.CreateRootCertificateAuthority(id, name);
            }
            else
            {
                Driver.Instance.RawDatabaseAccess.CreateIntermediateCertificateAuthority(parentId.Value, id, name);
            }
        }
    }

    [Then(@"the response is an array with the fields:")]
    public void ThenTheResponseIsAnArrayWithTheFields(Table table)
    {
        using var _ = new AssertionScope();
        
        var array = JsonConvert.DeserializeObject<JArray>(Driver.Instance.LastResponseBody);
        foreach (var tableRow in table.Rows)
        {
            var index = tableRow.GetInt32("Index");
            var property = tableRow.GetString("Property");
            var expectedValue = tableRow.GetString("Value");

            array[index][property].Value<string>().Should().Be(expectedValue);
        }
    }

    [Then(@"the response is an array with (.*) entry")]
    public void ThenTheResponseIsAnArrayWithEntry(int expectedNumberOfEntries)
    {
        var array = JsonConvert.DeserializeObject<JArray>(Driver.Instance.LastResponseBody);
        array.Count.Should().Be(expectedNumberOfEntries);
    }

    [Given(@"the database is empty")]
    public void GivenTheDatabaseIsEmpty()
    {
        Driver.Instance.RawDatabaseAccess.ClearDatabase();
    }
}