using FluentAssertions;
using home_ca_backend.Api.Tests.Drivers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reqnroll;
using System.Linq;

namespace home_ca_backend.Api.Tests.Steps
{
    [Binding]
    public class ValidationSteps
    {
        [Then(@"there is a validation error ""(.*)"" for ""(.*)""")]
        public void ThenThereIsAValidationErrorFor(string expectedError, string propertyName)
        {
            var deserializedResponse = JsonConvert.DeserializeObject<JArray>(Driver.Instance.LastResponseBody);
            var error = deserializedResponse.First(x => x["propertyName"]?.ToString() == propertyName);
            error["reason"]?.ToString().Should().Be(expectedError);
        }
    }
}
