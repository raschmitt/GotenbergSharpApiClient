﻿using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client
{
    /// <summary>
    /// C# Client for Gotenberg api
    /// </summary>
    /// <remarks>
    /// <para>
    ///     Gotenberg:
    ///     https://gotenberg.dev
    ///     https://github.com/gotenberg/gotenberg
    /// </para>
    /// <para>
    ///     Other clients available:
    ///     https://github.com/gotenberg/awesome-gotenberg#clients
    /// </para>
    /// </remarks>
    public sealed class GotenbergSharpClient
    {
        readonly HttpClient _innerClient;

        #region ctors

        [EditorBrowsable(EditorBrowsableState.Never)]
        public GotenbergSharpClient(string address)
            : this(new Uri(address))
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public GotenbergSharpClient(Uri address)
            : this(new HttpClient { BaseAddress = address })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GotenbergSharpClient"/> class.
        /// </summary>
        /// <param name="innerClient"></param>
        /// <remarks>Client was built for DI use</remarks>
        [PublicAPI]
        public GotenbergSharpClient(HttpClient innerClient)
        {
            this._innerClient = innerClient ?? throw new ArgumentNullException(nameof(innerClient));

            if (this._innerClient.BaseAddress == null)
            {
                throw new InvalidOperationException($"{nameof(innerClient.BaseAddress)} is null");
            }

            _innerClient.DefaultRequestHeaders.Add(Constants.HttpContent.Headers.UserAgent, nameof(GotenbergSharpClient));
            _innerClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(Constants.HttpContent.MediaTypes.ApplicationPdf));
        }

        #endregion

        #region api methods

        /// <summary>
        /// For remote URL conversions. Works just like <see cref="HtmlToPdfAsync"/>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        [PublicAPI]
        public Task<Stream> UrlToPdfAsync(UrlRequest request, CancellationToken cancelToken = default)
            => ExecuteRequestAsync(request, cancelToken);

        /// <summary>
        ///    Converts the specified request to a PDF document.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [PublicAPI]
        public Task<Stream> HtmlToPdfAsync(HtmlRequest request, CancellationToken cancelToken = default)
            => ExecuteRequestAsync(request, cancelToken);

        /// <summary>
        /// Merges items specified by the request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [PublicAPI]
        public Task<Stream> MergePdfsAsync(MergeRequest request, CancellationToken cancelToken = default)
            => ExecuteRequestAsync(request, cancelToken);

        /// <summary>
        ///     Converts one or more office documents into a merged pdf.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        [PublicAPI]
        public Task<Stream> MergeOfficeDocsAsync(MergeOfficeRequest request, CancellationToken cancelToken = default)
            => ExecuteRequestAsync(request, cancelToken);

        [PublicAPI]
        public Task<Stream> ConvertPdfDocumentsAsync(PdfConversionRequest request, CancellationToken cancelToken = default)
            => ExecuteRequestAsync(request, cancelToken);

        [PublicAPI]
        public async Task FireWebhookAndForgetAsync(IApiRequest request, CancellationToken cancelToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (!request.IsWebhookRequest)
                throw new InvalidOperationException("Only call this for webhook configured requests");

            using var response = await SendRequest(request, HttpCompletionOption.ResponseHeadersRead, cancelToken);
        }

        #endregion

        #region exec

        async Task<Stream> ExecuteRequestAsync(IApiRequest request, CancellationToken cancelToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var response = await SendRequest(request, HttpCompletionOption.ResponseHeadersRead, cancelToken);

            #if NET5_0_OR_GREATER
                return await response.Content.ReadAsStreamAsync(cancelToken);
            #else
                return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            #endif
        }

        async Task<HttpResponseMessage> SendRequest(
            IApiRequest request,
            HttpCompletionOption option,
            CancellationToken cancelToken)
        {
            using var message = request.ToApiRequestMessage();

            var response = await this._innerClient
                .SendAsync(message, option, cancelToken)
                .ConfigureAwait(false);

            cancelToken.ThrowIfCancellationRequested();

            if (response.IsSuccessStatusCode)
                return response;

            throw GotenbergApiException.Create(request, response);
        }

        #endregion
    }
}