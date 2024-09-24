using home_ca_backend.Api.Tests.Drivers;
using Reqnroll;
using System;

namespace home_ca_backend.Api.Tests.Steps;

[Binding]
internal class CertificateAuthoritySteps
{
    [Then("there is a certificate with a lifetime of {int} years for the returned GUID")]
    public void ThenThereIsACertificateWithALifetimeOfYearsForTheReturnedGUID(int numberOfYears)
    {
        Driver.Instance.AssertHasCertificateWithLifetimeForLastReturnedId(DateTime.Now.AddYears(numberOfYears));
    }

}
