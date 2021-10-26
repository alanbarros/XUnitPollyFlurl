using Flurl;
using Flurl.Http;
using Polly;
using Polly.Retry;
using Serilog;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infra
{
    public class SomeApiService
    {
        public string token;  

        public async Task<Person> CreatePersonAsync(string firstName, string lastName, Func<string> useToken = null)
        {
            var policy = PollyHelper.PolicyHandle<FlurlHttpException>(a =>
                                a.GetType() != typeof(FlurlHttpTimeoutException))
                            .HandleFlurlResults()
                            .WaitRetryAsync();

            var poly = PollyHelper.PolicyHandle()
                .OrResult<Person>(p => p.FirstName is null)
                .WaitRetryAsync();

            var token = "batatinha";

            var result = await poly.ExecuteAndCaptureAsync(async () =>
            {
                var result = await policy.ExecuteAndCaptureAsync(async () =>
                {
                    return await "https://api.com"
                        .AppendPathSegment("person")
                        .SetQueryParams(new { a = 1, b = 2 })
                        .WithOAuthBearerToken(token)
                        .PostJsonAsync(new
                        {
                            first_name = firstName,
                            last_name = lastName
                        });
                });

                if (result.Outcome != OutcomeType.Successful)
                {
                    if (result.FinalException is FlurlHttpTimeoutException && useToken != null)
                        token = useToken();

                    throw result.FinalException;
                }

                return JsonSerializer.Deserialize<Person>(
                    await result.Result.ResponseMessage.Content.ReadAsStringAsync());
            });

            return result.Result;
        }
    }

    public class Person
    {
        public string FirstName {  get; set; }
        public string LastName {  get; set; }
    }

    public static class PollyHelper
    {
        public static PolicyBuilder PolicyHandle<T>(Func<T, bool> condition) where T : Exception =>
            Policy.Handle(condition);

        public static PolicyBuilder PolicyHandle<T>() where T : Exception =>
            Policy.Handle<T>();

        public static PolicyBuilder PolicyHandle() =>
            Policy.Handle<Exception>();

        public static PolicyBuilder<IFlurlResponse> HandleFlurlResults(this PolicyBuilder builder) =>
            builder.OrResult<IFlurlResponse>(a => (new int[] { 500, 408 }).Contains(a.StatusCode));

        public static AsyncRetryPolicy<T> WaitRetryAsync<T>(this PolicyBuilder<T> builder) => 
            builder.WaitAndRetryAsync(2,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, context) =>
                {
                    Log.Information(String.Format("Houve uma retentativa do Polly com a menagem {0}, tempo: {1}",
                        exception.Exception is Exception ex ? 
                            $"Message: {ex.Message},{Environment.NewLine} StackTrace: {ex.StackTrace}" : 
                            "Sem mensagem", 
                        timeSpan));
                });
    }
}
