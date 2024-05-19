using System.Threading.Tasks;
using home_ca_backend.Api.Tests.Drivers;
using Reqnroll;

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

        [BeforeScenario]
        public void ResetHttpClient()
        {
            Driver.Instance.RawDatabaseAccess.ClearDatabase();
            Driver.Instance.ResetHttpClient();
        }

        [AfterTestRun]
        public static async Task TearDown() => await Driver.Instance.DisposeAsync();
    }
}