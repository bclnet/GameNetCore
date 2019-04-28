// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.Proto.Features;
using Contoso.GameNetCore.Server.Kestrel.Core.Features;
using Contoso.GameNetCore.Server.Kestrel.Core.Internal.Infrastructure;
using Contoso.GameNetCore.Server.Kestrel.Transport.Abstractions.Internal;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto
{
    internal partial class ProtoProtocol : IProtoRequestFeature,
                                        IProtoResponseFeature,
                                        IResponseBodyPipeFeature,
                                        IRequestBodyPipeFeature,
                                        IProtoUpgradeFeature,
                                        IProtoConnectionFeature,
                                        IProtoRequestLifetimeFeature,
                                        IProtoRequestIdentifierFeature,
                                        IProtoBodyControlFeature,
                                        IProtoMaxRequestBodySizeFeature,
                                        IProtoResponseStartFeature
    {
        // NOTE: When feature interfaces are added to or removed from this ProtoProtocol class implementation,
        // then the list of `implementedFeatures` in the generated code project MUST also be updated.
        // See also: tools/CodeGenerator/ProtoProtocolFeatureCollection.cs

        string IProtoRequestFeature.Protocol
        {
            get => ProtoVersion;
            set => ProtoVersion = value;
        }

        string IProtoRequestFeature.Scheme
        {
            get => Scheme ?? "http";
            set => Scheme = value;
        }

        string IProtoRequestFeature.Method
        {
            get
            {
                if (_methodText != null)
                {
                    return _methodText;
                }

                _methodText = ProtoUtilities.MethodToString(Method) ?? string.Empty;
                return _methodText;
            }
            set
            {
                _methodText = value;
            }
        }

        string IProtoRequestFeature.PathBase
        {
            get => PathBase ?? "";
            set => PathBase = value;
        }

        string IProtoRequestFeature.Path
        {
            get => Path;
            set => Path = value;
        }

        string IProtoRequestFeature.QueryString
        {
            get => QueryString;
            set => QueryString = value;
        }

        string IProtoRequestFeature.RawTarget
        {
            get => RawTarget;
            set => RawTarget = value;
        }

        IHeaderDictionary IProtoRequestFeature.Headers
        {
            get => RequestHeaders;
            set => RequestHeaders = value;
        }

        Stream IProtoRequestFeature.Body
        {
            get
            {
                return RequestBody;
            }
            set
            {
                RequestBody = value;
                var requestPipeReader = new StreamPipeReader(RequestBody, new StreamPipeReaderAdapterOptions(
                    minimumSegmentSize: KestrelMemoryPool.MinimumSegmentSize,
                    minimumReadThreshold: KestrelMemoryPool.MinimumSegmentSize / 4,
                    _context.MemoryPool));
                RequestBodyPipeReader = requestPipeReader;

                // The StreamPipeWrapper needs to be disposed as it hold onto blocks of memory
                if (_wrapperObjectsToDispose == null)
                {
                    _wrapperObjectsToDispose = new List<IDisposable>();
                }
                _wrapperObjectsToDispose.Add(requestPipeReader);
            }
        }

        PipeReader IRequestBodyPipeFeature.Reader
        {
            get
            {
                return RequestBodyPipeReader;
            }
            set
            {
                RequestBodyPipeReader = value;
                RequestBody = new ReadOnlyPipeStream(RequestBodyPipeReader);
            }
        }

        int IProtoResponseFeature.StatusCode
        {
            get => StatusCode;
            set => StatusCode = value;
        }

        string IProtoResponseFeature.ReasonPhrase
        {
            get => ReasonPhrase;
            set => ReasonPhrase = value;
        }

        IHeaderDictionary IProtoResponseFeature.Headers
        {
            get => ResponseHeaders;
            set => ResponseHeaders = value;
        }

        CancellationToken IProtoRequestLifetimeFeature.RequestAborted
        {
            get => RequestAborted;
            set => RequestAborted = value;
        }

        bool IProtoResponseFeature.HasStarted => HasResponseStarted;

        bool IProtoUpgradeFeature.IsUpgradableRequest => IsUpgradableRequest;

        IPAddress IProtoConnectionFeature.RemoteIpAddress
        {
            get => RemoteIpAddress;
            set => RemoteIpAddress = value;
        }

        IPAddress IProtoConnectionFeature.LocalIpAddress
        {
            get => LocalIpAddress;
            set => LocalIpAddress = value;
        }

        int IProtoConnectionFeature.RemotePort
        {
            get => RemotePort;
            set => RemotePort = value;
        }

        int IProtoConnectionFeature.LocalPort
        {
            get => LocalPort;
            set => LocalPort = value;
        }

        string IProtoConnectionFeature.ConnectionId
        {
            get => ConnectionIdFeature;
            set => ConnectionIdFeature = value;
        }

        string IProtoRequestIdentifierFeature.TraceIdentifier
        {
            get => TraceIdentifier;
            set => TraceIdentifier = value;
        }

        bool IProtoBodyControlFeature.AllowSynchronousIO
        {
            get => AllowSynchronousIO;
            set => AllowSynchronousIO = value;
        }

        bool IProtoMaxRequestBodySizeFeature.IsReadOnly => HasStartedConsumingRequestBody || IsUpgraded;

        long? IProtoMaxRequestBodySizeFeature.MaxRequestBodySize
        {
            get => MaxRequestBodySize;
            set
            {
                if (HasStartedConsumingRequestBody)
                {
                    throw new InvalidOperationException(CoreStrings.MaxRequestBodySizeCannotBeModifiedAfterRead);
                }
                if (IsUpgraded)
                {
                    throw new InvalidOperationException(CoreStrings.MaxRequestBodySizeCannotBeModifiedForUpgradedRequests);
                }
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), CoreStrings.NonNegativeNumberOrNullRequired);
                }

                MaxRequestBodySize = value;
            }
        }

        PipeWriter IResponseBodyPipeFeature.Writer
        {
            get
            {
                return ResponsePipeWriter;
            }
            set
            {
                ResponsePipeWriter = value;
                ResponseBody = new WriteOnlyPipeStream(ResponsePipeWriter);
            }
        }

        Stream IProtoResponseFeature.Body
        {
            get
            {
                return ResponseBody;
            }
            set
            {
                ResponseBody = value;
                var responsePipeWriter = new StreamPipeWriter(ResponseBody, minimumSegmentSize: KestrelMemoryPool.MinimumSegmentSize, _context.MemoryPool);
                ResponsePipeWriter = responsePipeWriter;

                // The StreamPipeWrapper needs to be disposed as it hold onto blocks of memory
                if (_wrapperObjectsToDispose == null)
                {
                    _wrapperObjectsToDispose = new List<IDisposable>();
                }
                _wrapperObjectsToDispose.Add(responsePipeWriter);
            }
        }

        protected void ResetProto1Features()
        {
            _currentIProtoMinRequestBodyDataRateFeature = this;
            _currentIProtoMinResponseDataRateFeature = this;
        }

        protected void ResetProto2Features()
        {
            _currentIProto2StreamIdFeature = this;
            _currentIProtoResponseTrailersFeature = this;
        }

        void IProtoResponseFeature.OnStarting(Func<object, Task> callback, object state)
        {
            OnStarting(callback, state);
        }

        void IProtoResponseFeature.OnCompleted(Func<object, Task> callback, object state)
        {
            OnCompleted(callback, state);
        }

        async Task<Stream> IProtoUpgradeFeature.UpgradeAsync()
        {
            if (!IsUpgradableRequest)
            {
                throw new InvalidOperationException(CoreStrings.CannotUpgradeNonUpgradableRequest);
            }

            if (IsUpgraded)
            {
                throw new InvalidOperationException(CoreStrings.UpgradeCannotBeCalledMultipleTimes);
            }

            if (!ServiceContext.ConnectionManager.UpgradedConnectionCount.TryLockOne())
            {
                throw new InvalidOperationException(CoreStrings.UpgradedConnectionLimitReached);
            }

            IsUpgraded = true;

            ConnectionFeatures.Get<IDecrementConcurrentConnectionCountFeature>()?.ReleaseConnection();

            StatusCode = StatusCodes.Status101SwitchingProtocols;
            ReasonPhrase = "Switching Protocols";
            ResponseHeaders["Connection"] = "Upgrade";

            await FlushAsync();

            return bodyControl.Upgrade();
        }

        void IProtoRequestLifetimeFeature.Abort()
        {
            ApplicationAbort();
        }

        Task IProtoResponseStartFeature.StartAsync(CancellationToken cancellationToken)
        {
            if (HasResponseStarted)
            {
                return Task.CompletedTask;
            }

            cancellationToken.ThrowIfCancellationRequested();

            return InitializeResponseAsync(0);
        }
    }
}
