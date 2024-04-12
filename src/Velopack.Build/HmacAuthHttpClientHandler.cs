using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Velopack.Packaging.Flow;

namespace Velopack.Build;

internal class HmacAuthHttpClientHandler : HttpClientHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Debugger.Launch();
        if (request.Headers.Authorization?.Scheme == HmacHelper.HmacScheme &&
            request.Headers.Authorization.Parameter is { } authParameter &&
            authParameter.Split(':') is var keyParts &&
            keyParts.Length == 2) 
        {
            string hashedId = keyParts[0];
            string key = keyParts[1];
            string nonce = Guid.NewGuid().ToString();

            var content = request.Content;
            //NB: Do not dispose of contentStream, as it will be read by the HttpClient
#if NET6_0_OR_GREATER
            var contentStream = content is null
                ? Stream.Null
                : await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            string contentHash = await HmacHelper.GetContentHashAsync(contentStream).ConfigureAwait(false);
            contentStream.Position = 0;
#else
            var contentStream = content is null
                ? Stream.Null
                : await content.ReadAsStreamAsync().ConfigureAwait(false);
            string contentHash = HmacHelper.GetContentHash(contentStream);
#endif
            uint secondsSinceEpoch = HmacHelper.GetSecondsSinceEpoch();
            var signature = HmacHelper.BuildSignature(hashedId, request.Method.Method, request.RequestUri?.AbsoluteUri ?? "", secondsSinceEpoch, nonce, contentHash);
            var secret = HmacHelper.Calculate(Convert.FromBase64String(key), signature);
            request.Headers.Authorization = BuildHeader(hashedId, secret, nonce, secondsSinceEpoch);
        }
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private static AuthenticationHeaderValue BuildHeader(string hashedId, string base64Signature, string nonce, uint secondsSinceEpoch)
        => new(HmacHelper.HmacScheme, $"{hashedId}:{base64Signature}:{nonce}:{secondsSinceEpoch}");
}
