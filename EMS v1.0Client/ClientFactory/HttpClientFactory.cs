using System;
using System.Net;
using System.Net.Http;

public interface IHttpClientFactory : IDisposable { HttpClient CreateClient(string baseUrl); CookieContainer CookieContainer { get; } }

public class HttpClientFactory : IHttpClientFactory
{
    private readonly CookieContainer _cookieContainer; private bool _disposed;

    public HttpClientFactory()
    {
        _cookieContainer = new CookieContainer();
    }

    public CookieContainer CookieContainer => _cookieContainer;

    public HttpClient CreateClient(string baseUrl)
    {
        if (string.IsNullOrEmpty(baseUrl))
            throw new ArgumentNullException(nameof(baseUrl));
        if (!baseUrl.EndsWith("/"))
            baseUrl += "/";

        var handler = new HttpClientHandler
        {
            CookieContainer = _cookieContainer,
            UseCookies = true
        };

        return new HttpClient(handler)
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        GC.SuppressFinalize(this);
    }

}