using System;
using System.Linq;
using AltWebSocketSharp;
public class RuntimeWebSocketClient
{

    private ClientWebSocket wsClient;
    public RuntimeWebSocketClient(string host, int port)
    {
        Uri uri;
        Uri.TryCreate($"ws://{host}:{port}/app", UriKind.Absolute, out uri);
        wsClient = new ClientWebSocket(uri.ToString());
        string proxyUri = new ProxyFinder().GetProxy(string.Format("http://{0}:{1}", host, port), host);
        if (proxyUri != null)
        {
            wsClient.SetProxy(proxyUri, null, null);
        }

        wsClient.OnOpen += (sender, message) =>
           {
               UnityEngine.Debug.Log($"OnOpen called with the following message: {message}");
           };

        wsClient.OnClose += (sender, args) =>
        {
            UnityEngine.Debug.Log($"OnClose called with code: {args.Code} and the following reason: {args.Reason}");
        };

        wsClient.OnError += (sender, args) =>
        {
            UnityEngine.Debug.Log($"OnError called with exception: {args.Exception} and the following message: {args.Message}");
        };

        wsClient.OnMessage += (sender, args) =>
        {
            UnityEngine.Debug.Log($"OnMessage called with the following message: {args.Data}");
        };
    }
    public void Connect()
    {
        try
        {
            this.wsClient.ConnectAsync();
            UnityEngine.Debug.Log("Succesfully connected");
        }
        catch (InvalidOperationException e)
        {
            UnityEngine.Debug.Log("Could not connect" + e.Message);
            throw new InvalidOperationException("Could not connect", e);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log("An error occurred while starting the AltTester(R) client" + ex.Message);
            throw new Exception("An error occurred while starting the AltTester(R) client.", ex);
        }
    }
    public void Stop()
    {
        wsClient.Close();
    }
}

public class ProxyFinder : IProxyFinder
{
    public string GetProxy(string uri, string host)
    {

        IProxyFinder Finder = null;
        string ProxyUri = null;


        if (ProxyUri == null)
        {
            try
            {
                Finder = new EnvironmentProxyFinder();
                ProxyUri = Finder.GetProxy(uri, host);
            }
            catch (Exception)
            {
            }
        }

        if (ProxyUri == null)
        {
            try
            {
                Finder = new DotnetProxyFinder();
                ProxyUri = Finder.GetProxy(uri, host);
            }
            catch (Exception)
            {
            }
        }

        return ProxyUri;
    }

}
public class DotnetProxyFinder : IProxyFinder
{
    public string GetProxy(string uri, string host)
    {
        var Proxy = System.Net.WebRequest.GetSystemWebProxy() as System.Net.WebProxy;
        if (Proxy != null && Proxy.Address != null)
        {
            string proxyUri = Proxy.GetProxy(new Uri(uri)).ToString();

            if (proxyUri != uri)
            {
                return proxyUri;
            }
        }

        return null;
    }
}

public class EnvironmentProxyFinder : IProxyFinder
{
    public string GetProxy(string uri, string host)
    {
        string proxyUrl = GetEnv("HTTP_PROXY");

        if (proxyUrl == null)
        {
            proxyUrl = GetEnv("ALL_PROXY");
        }

        if (proxyUrl != null)
        {
            string exceptions = GetEnv("NO_PROXY");

            if (!string.IsNullOrEmpty(exceptions))
            {
                var exceptionsList = exceptions.Split(';').ToList<string>();

                if (exceptionsList.Contains(proxyUrl))
                {
                    return null;
                }
            }
        }

        return proxyUrl;
    }

    private string GetEnv(string key)
    {
        return System.Environment.GetEnvironmentVariable(key) ?? System.Environment.GetEnvironmentVariable(key.ToLowerInvariant());
    }
}
public interface IProxyFinder
{
    /// <summary>
    /// Finds the appropriate proxy server for the specified URL and host.
    /// </summary>
    /// <param name="uri">The URL for which the proxy server needs to be determined.</param>
    /// <param name="host">The host associated with the URL. This is only for convenience; it is the same string as between :// and the first : or / after that.</param>
    /// <returns>The proxy server uri. If the string is null, no proxies should be used.</returns>
    string GetProxy(string uri, string host);
}