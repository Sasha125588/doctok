using Google.GenAI;

namespace Infrastructure.Llm.Providers.Gemini;

public sealed class GeminiClient(Client client)
{
   public async Task<string?> CompleteChatAsync(string model, string userMessage, CancellationToken ct)
   {
     var response = await client.Models.GenerateContentAsync(
       model: "gemini-2.5-flash", contents: userMessage, cancellationToken: ct);

     Console.WriteLine(response.Candidates?[0].Content?.Parts?[0].Text);

     return response.Candidates?[0].Content?.Parts?[0].Text;
   }
}
