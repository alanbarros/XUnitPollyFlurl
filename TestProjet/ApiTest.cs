using Flurl.Http.Testing;
using Infra;
using Serilog;
using Xunit;

namespace TestProjet
{
    public class ApiTest
    {
        [Fact]
        public void Deveria_Retornar_Erro_TimeOut()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("log.txt")
                .WriteTo.EventCollector("https://localhost:8088/services/collector", "38fc0b71-057d-44e0-8f55-11795e5c1d42")
                .CreateLogger();

            using var httpClient = new HttpTest();

            httpClient.RespondWith("Internal Server Error", 500);
            httpClient.SimulateTimeout();
            httpClient.RespondWithJson(new { FirstName = "Claire", LastName = "Underwood" }, 200);

            var api = new SomeApiService()
                .CreatePersonAsync("Claire", "Underwood", 
                () => "bearer token")
                .Result;

            Assert.NotNull(api);
        }
    }
}
