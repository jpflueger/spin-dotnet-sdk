namespace Fermyon.Spin.Sdk;

public static class HttpClientInterop
{
  public static System.Net.Http.HttpMethod ToSystemType(this HttpMethod method)
  {
    return method switch
    {
      HttpMethod.Get => System.Net.Http.HttpMethod.Get,
      HttpMethod.Delete => System.Net.Http.HttpMethod.Delete,
      HttpMethod.Head => System.Net.Http.HttpMethod.Head,
      HttpMethod.Options => System.Net.Http.HttpMethod.Options,
      HttpMethod.Patch => System.Net.Http.HttpMethod.Patch,
      HttpMethod.Post => System.Net.Http.HttpMethod.Post,
      HttpMethod.Put => System.Net.Http.HttpMethod.Put,
      _ => throw new ArgumentOutOfRangeException(nameof(method)),
    };
  }

  public static HttpMethod ToSpinType(this System.Net.Http.HttpMethod method)
  {
    // unfortunately we can't use a switch statement here because the HttpMethod is static and not constant
    if (method == System.Net.Http.HttpMethod.Get) return HttpMethod.Get;
    else if (method == System.Net.Http.HttpMethod.Delete) return HttpMethod.Delete;
    else if (method == System.Net.Http.HttpMethod.Head) return HttpMethod.Head;
    else if (method == System.Net.Http.HttpMethod.Options) return HttpMethod.Options;
    else if (method == System.Net.Http.HttpMethod.Patch) return HttpMethod.Patch;
    else if (method == System.Net.Http.HttpMethod.Post) return HttpMethod.Post;
    else if (method == System.Net.Http.HttpMethod.Put) return HttpMethod.Put;
    else throw new ArgumentOutOfRangeException(nameof(method), method, "Spin does not have a corresponding value to the one provided.");
  }

  public static System.Net.Http.HttpRequestMessage ToSystemType(this HttpRequest request)
  {
    // convert the request body to memory and use it as the request content
    HttpContent? content = null;
    if (request.Body.TryGetValue(out Buffer buffer))
    {
      content = new ReadOnlyMemoryContent(buffer.AsMemory());
    }

    var systemRequest = new HttpRequestMessage
    {
      Method = request.Method.ToSystemType(),
      RequestUri = new Uri(request.Url),
      Content = content,
    };

    // convert the headers after because there is no constructor for the header collection
    foreach (var header in request.Headers)
    {
      systemRequest.Headers.Add(header.Key, header.Value);
    }

    return systemRequest;
  }

  public static HttpRequest ToSpinType(this System.Net.Http.HttpRequestMessage request)
  {
    if (request.RequestUri is null)
    {
      throw new ArgumentNullException();
    }

    Optional<Buffer> optBuffer = default;
    if (request.Content != null)
    {
      optBuffer = new Optional<Buffer>(Buffer.FromHttpContent(request.Content));
    }

    IReadOnlyDictionary<string, string> headers = request
      .Headers
      .Where(h => h.Value.Any())
      .ToDictionary(h => h.Key, h => h.Value.First())
      .AsReadOnly();

    return new HttpRequest
    {
      Url = request.RequestUri!.ToString(),
      Method = request.Method.ToSpinType(),
      Body = optBuffer,
      Headers = headers
    };
  }

  public static System.Net.Http.HttpResponseMessage ToSystemType(this HttpResponse response)
  {
    var systemResponse = new System.Net.Http.HttpResponseMessage
    {
      StatusCode = response.StatusCode,
    };

    if (response.Body.TryGetValue(out Buffer value))
    {
      systemResponse.Content = new ReadOnlyMemoryContent(value.AsMemory());
    }

    foreach (var header in response.Headers)
    {
      systemResponse.Headers.Add(header.Key, header.Value);
    }

    return systemResponse;
  }

  public static HttpResponse ToSpinType(this System.Net.Http.HttpResponseMessage response)
  {
    var headers = response.Headers.ToDictionary(
      kvp => kvp.Key,
      kvp => string.Join(',', kvp.Value),
      StringComparer.InvariantCultureIgnoreCase
    );
    return new HttpResponse
    {
      StatusCode = response.StatusCode,
      Headers = headers,
      Body = Optional.From(Buffer.FromHttpContent(response.Content)),
    };
  }

  public static HttpClient CreateHttpClient() => new HttpClient(new SpinHttpMessageHandler());

  public class SpinHttpMessageHandler : HttpMessageHandler
  {
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      var spinRequest = request.ToSpinType();
      var spinResponse = HttpOutbound.Send(spinRequest);
      return spinResponse.ToSystemType();
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      // perform this synchronously until spin supports async/await
      return Task.FromResult(this.Send(request, cancellationToken));
    }
  }
}
