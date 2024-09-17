using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using home_ca_backend.Api.Tests.Drivers;
using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reqnroll;
using Reqnroll.Assist;

namespace home_ca_backend.Api.Tests.Steps;

[Binding]
public class GenericSteps
{
    [When("the endpoint (.*) is called")]
    public async Task WhenTheEndpointIsCalled(string relativeUrl)
    {
        await Driver.Instance.SendHttpRequest(relativeUrl);
    }

    [When(@"the endpoint (.*) is called with a POST request")]
    public Task WhenTheEndpointCasIsCalledWithApostRequest(string uri) =>
        Driver.Instance.SendHttpPostRequest(uri, new StringContent(""));

    [Then("the status code should be (.*)")]
    public void ThenTheStatusCodeShouldBe(int expectedStatusCode)
    {
        Driver.Instance.LastStatusCode.Should().Be((HttpStatusCode)expectedStatusCode);
    }

    [Then(@"the status code should not be (.*)")]
    public void ThenTheStatusCodeShouldNotBe(int notExpectedStatusCode)
    {
        Driver.Instance.LastStatusCode.Should().NotBe((HttpStatusCode)notExpectedStatusCode);
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
        foreach (var tableRow in table.Rows)
        {
            var id = Guid.Parse(tableRow["Id"]);
            string name = tableRow["Name"];
            Guid? parentId = !tableRow.ContainsKey("Parent") || string.IsNullOrWhiteSpace(tableRow["Parent"])
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

    [Given(@"a valid user is authenticated")]
    public async Task GivenAValidUserIsAuthenticated() 
    {
        if (Driver.Instance.IsAuthenticated)
        {
            await Task.CompletedTask;
        } 
        else 
        { 
            await GivenTheUserIsAuthenticatedWithThePassword("test@example.com", "t3sTpa55w0rd");
        }
    }

    [When(@"the endpoint (.*) is called with a POST request with the data")]
    public async Task WhenTheEndpointIsCalledWithApostRequestWithTheData(string uri, Table table)
    {
        var jsonContent = new JObject();
        foreach (var row in table.Rows)
        {
            jsonContent.Add(row["Property"], row["Value"]);
        }

        await Driver.Instance.SendHttpPostRequest(uri,
            new StringContent(jsonContent.ToString(), Encoding.Default, MediaTypeNames.Application.Json));
    }

    [Then(@"the response is a valid GUID")]
    public void ThenTheResponseIsAValidGuid()
    {
        Guid.TryParse(Driver.Instance.LastResponseBody, out _).Should().BeTrue();
    }

    [Then(@"there is a root certificate authority ""(.*)"" with the returned GUID")]
    public void ThenThereIsARootCertificateAuthorityWithTheReturnedGuid(string expectedName)
    {
        Driver.Instance.AssertHasRootCertificateForLastReturnedId(expectedName);
    }
}