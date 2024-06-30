namespace ProxyServer.Contracts.Request;

public record ProxyRequest(string Method, string Url, Dictionary<string, string> Headers);