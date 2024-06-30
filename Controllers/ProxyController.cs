using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using ProxyServer.Contracts.Request;
using ProxyServer.Contracts.Response;

namespace ProxyServer.Controllers;

[ApiController]
[Route("[controller]")]
public class ProxyController : ControllerBase
{
    
    private static readonly ConcurrentDictionary<string, (HttpRequestMessage Request, HttpResponseMessage Response)> s_requestStorage = new();

    private readonly HttpClient _httpClient;

    public ProxyController(HttpClient httpClient)
    {
        this._httpClient = httpClient;
    }

    [HttpPost("proxy")]
    public async Task<IActionResult> ProxyRequestAsync([FromBody] ProxyRequest request)
    {
        if (string.IsNullOrEmpty(request.Method) || string.IsNullOrEmpty(request.Url))
        {
            return BadRequest("Invalid incoming data");
        }

        var httpRequest = new HttpRequestMessage(new HttpMethod(request.Method), request.Url);

        foreach (var header in request.Headers)
        {
            httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        var requestId = Guid.NewGuid().ToString();

        s_requestStorage[requestId] = (httpRequest, null)!;

        HttpResponseMessage response;

        try
        {
            response = await _httpClient.SendAsync(httpRequest);
            s_requestStorage[requestId] = (httpRequest, response);
        }
        catch (HttpRequestException e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Error sending request: {e.Message}");
        }

        var proxyResponse = new ProxyResponse(requestId, response.StatusCode,
            response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value)),
            response.Content.Headers.ContentLength ?? 0);

        return Ok(proxyResponse);
    }
}