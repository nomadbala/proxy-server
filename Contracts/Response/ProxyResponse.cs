using System.Net;

namespace ProxyServer.Contracts.Response;

public record ProxyResponse(String Id, HttpStatusCode Status, Dictionary<string, string> Headers, long Length);