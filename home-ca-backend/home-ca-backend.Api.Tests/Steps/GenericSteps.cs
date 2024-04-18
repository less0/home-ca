﻿using System.Net;
using FluentAssertions;
using home_ca_backend.Api.Tests.Drivers;

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
}