using System.Diagnostics;
using System.Security.Policy;
using FluentAssertions;
using home_ca_backend.Api.Tests.Drivers;
using Testcontainers.MsSql;

namespace home_ca_backend.Api.Tests.Hooks
{
    [Binding]
    public class Hooks
    {
        [BeforeTestRun]
        public static async Task SetUp()
        {
            await Driver.Instance.StartDatabaseAsync();
            Driver.Instance.StartApi();
        }

        [AfterTestRun]
        public static async Task TearDown() => await Driver.Instance.DisposeAsync();
    }
}