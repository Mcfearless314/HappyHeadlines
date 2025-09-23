using Polly;

namespace CommentService.Policies;

public class CommentPolicy
{
    public async Task<bool> CheckProfanity(string comment)
    {
        var policy = Policy.Handle<Exception>()
            .CircuitBreaker(3, TimeSpan.FromSeconds(30),
                (exception, duration) =>
                {
                    Console.WriteLine("Circuit breaker tripped");
                },
                () => Console.WriteLine("Circuit breaker reset"));

        return policy.Execute( async Task<bool> () =>
        {
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://profanity-service:8080"); 

            var response = await httpClient.GetAsync($"/check?text={Uri.EscapeDataString(comment)}");
            response.EnsureSuccessStatusCode();

            bool containsProfanity = await response.Content.ReadFromJsonAsync<bool>();

            if (containsProfanity)
                return false;

            return true;
        }).Result;
        
    }
}