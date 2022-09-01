﻿using System.Text;

namespace HttpClientToCurl;

public static class CurlGenerator
{
    #region :: Main ::

    public static void GenerateCurlInConsole(
        this HttpClient httpClient,
        HttpRequestMessage httpRequestMessage,
        string requestUri,
        bool needAddDefaultHeaders = true,
        bool turnOn = true)
    {
        if (!turnOn)
            return;

        string script = _GenerateCurl(httpClient, httpRequestMessage, requestUri, needAddDefaultHeaders);

        _WriteInConsole(script);
    }

    public static void GenerateCurlInFile(
        this HttpClient httpClient,
        HttpRequestMessage httpRequestMessage,
        string requestUri,
        bool needAddDefaultHeaders = true,
        bool turnOn = true)
    {
        if (!turnOn)
            return;

        string script = _GenerateCurl(httpClient, httpRequestMessage, requestUri, needAddDefaultHeaders);
        
        _WriteInFile(script);
    }

    #endregion

    #region :: Generators ::

    private static string _GenerateCurl(
        this HttpClient httpClient,
        HttpRequestMessage httpRequestMessage,
        string requestUri,
        bool needAddDefaultHeaders)
    {
        string script;
        try
        {
            if (httpRequestMessage.Method == HttpMethod.Post)
                script = _GeneratePostMethod(httpClient, httpRequestMessage, requestUri, needAddDefaultHeaders);
            else if (httpRequestMessage.Method == HttpMethod.Get)
                script = _GenerateGetMethod(httpClient, httpRequestMessage, requestUri, needAddDefaultHeaders);
            else if (httpRequestMessage.Method == HttpMethod.Put)
                script = _GeneratePutMethod(httpClient, httpRequestMessage, requestUri, needAddDefaultHeaders);
            else if (httpRequestMessage.Method == HttpMethod.Patch)
                script = _GeneratePatchMethod(httpClient, httpRequestMessage, requestUri, needAddDefaultHeaders);
            else if (httpRequestMessage.Method == HttpMethod.Delete)
                script = _GenerateDeleteMethod(httpClient, httpRequestMessage, requestUri, needAddDefaultHeaders);
            else
                script = $"ERROR => Invalid HttpMethod: {httpRequestMessage.Method.Method}";
        }
        catch (Exception exception)
        {
            script = $"ERROR => {exception.Message}, {exception.InnerException}";
        }

        return script;
    }

    private static string _GenerateGetMethod(
        HttpClient httpClient,
        HttpRequestMessage httpRequestMessage,
        string requestUri,
        bool needAddDefaultHeaders)
    {
        StringBuilder stringBuilder = _Initialize(httpRequestMessage.Method);

        return stringBuilder
            ._AddAbsoluteUrl(httpClient.BaseAddress?.AbsoluteUri, requestUri)
            ._AddHeaders(httpClient, httpRequestMessage, needAddDefaultHeaders)
            .Append(' ')
            .ToString();
    }

    private static string _GeneratePostMethod(
        HttpClient httpClient,
        HttpRequestMessage httpRequestMessage,
        string requestUri,
        bool needAddDefaultHeaders)
    {
        StringBuilder stringBuilder = _Initialize(httpRequestMessage.Method);

        return stringBuilder
            ._AddAbsoluteUrl(httpClient.BaseAddress?.AbsoluteUri, requestUri)
            ._AddHeaders(httpClient, httpRequestMessage, needAddDefaultHeaders)
            ._AddBody(httpRequestMessage.Content?.ReadAsStringAsync().GetAwaiter().GetResult())?
            .Append(' ')
            .ToString();
    }

    private static string _GeneratePutMethod(
        HttpClient httpClient,
        HttpRequestMessage httpRequestMessage,
        string requestUri,
        bool needAddDefaultHeaders)
    {
        StringBuilder stringBuilder = _Initialize(httpRequestMessage.Method);

        return stringBuilder
            ._AddAbsoluteUrl(httpClient.BaseAddress?.AbsoluteUri, requestUri)
            ._AddHeaders(httpClient, httpRequestMessage, needAddDefaultHeaders)
            ._AddBody(httpRequestMessage.Content?.ReadAsStringAsync().GetAwaiter().GetResult())?
            .Append(' ')
            .ToString();
    }

    private static string _GeneratePatchMethod(
        HttpClient httpClient,
        HttpRequestMessage httpRequestMessage,
        string requestUri,
        bool needAddDefaultHeaders)
    {
        StringBuilder stringBuilder = _Initialize(httpRequestMessage.Method);

        return stringBuilder
            ._AddAbsoluteUrl(httpClient.BaseAddress?.AbsoluteUri, requestUri)
            ._AddHeaders(httpClient, httpRequestMessage, needAddDefaultHeaders)
            ._AddBody(httpRequestMessage.Content?.ReadAsStringAsync().GetAwaiter().GetResult())?
            .Append(' ')
            .ToString();
    }

    private static string _GenerateDeleteMethod(
        HttpClient httpClient,
        HttpRequestMessage httpRequestMessage,
        string requestUri,
        bool needAddDefaultHeaders)
    {
        StringBuilder stringBuilder = _Initialize(httpRequestMessage.Method);

        return stringBuilder
            ._AddAbsoluteUrl(httpClient.BaseAddress?.AbsoluteUri, requestUri)
            ._AddHeaders(httpClient, httpRequestMessage, needAddDefaultHeaders)
            .Append(' ')
            .ToString();
    }

    #endregion

    #region :: Builders ::

    private static StringBuilder _Initialize(HttpMethod httpMethod)
    {
        var stringBuilder = new StringBuilder("curl");

        if (httpMethod != HttpMethod.Get)
        {
            stringBuilder
                .Append(' ')
                .Append("-X")
                .Append(' ')
                .Append(httpMethod.Method)
                .Append(' ');
        }

        return stringBuilder;
    }

    private static StringBuilder _AddAbsoluteUrl(this StringBuilder stringBuilder, string baseUrl, string uri)
    {
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            if (baseUrl.EndsWith("/"))
                baseUrl = baseUrl.Remove(baseUrl.Length - 1);
            if (uri.StartsWith("/"))
                uri = uri.Remove(0);

            stringBuilder.Append($"{baseUrl?.Trim()}/{uri?.Trim()}");
        }

        return stringBuilder;
    }

    private static StringBuilder _AddHeaders(
        this StringBuilder stringBuilder,
        HttpClient httpClient,
        HttpRequestMessage httpRequestMessage,
        bool needAddDefaultHeaders = true)
    {
        if (needAddDefaultHeaders && httpClient.DefaultRequestHeaders.Any())
            foreach (var row in httpClient.DefaultRequestHeaders)
            {
                stringBuilder
                    .Append("-H")
                    .Append(' ')
                    .Append($"\'{row.Key}: {row.Value.FirstOrDefault()}\'")
                    .Append(' ');
            }

        if (httpRequestMessage.Headers.Any())
        {
            foreach (var row in httpRequestMessage.Headers)
            {
                stringBuilder
                    .Append(' ')
                    .Append("-H")
                    .Append(' ')
                    .Append($"\'{row.Key}: {row.Value.FirstOrDefault()}\'")
                    .Append(' ');
            }
        }

        if (httpRequestMessage.Content != null && httpRequestMessage.Content.Headers.Any())
        {
            foreach (var row in httpRequestMessage.Content.Headers)
            {
                stringBuilder
                    .Append(' ')
                    .Append("-H")
                    .Append(' ')
                    .Append($"\'{row.Key}: {row.Value.FirstOrDefault()}\'")
                    .Append(' ');
            }
        }

        return stringBuilder;
    }

    private static StringBuilder _AddBody(this StringBuilder stringBuilder, string jsonBody)
    {
        if (jsonBody != null && jsonBody.Any())
        {
            stringBuilder
                .Append(' ')
                .Append("-d")
                .Append(' ')
                .Append('\'')
                .Append(jsonBody)
                .Append('\'')
                .Append(' ');
        }

        return stringBuilder;
    }

    #endregion

    #region :: Other ::

    private static void _WriteInConsole(string script)
    {
        Console.WriteLine(script);
    }

    private static void _WriteInFile(string script, string path = null)
    {
      // TODO: Write script inside file ...
    }

    #endregion
}