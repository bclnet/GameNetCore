// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Contoso.GameNetCore.Connections;
using Contoso.GameNetCore.Connections.Features;
using Contoso.GameNetCore.Server.Kestrel.Core;
using Contoso.GameNetCore.Server.Kestrel.Core.Adapter.Internal;
using Contoso.GameNetCore.Server.Kestrel.Core.Features;
using Microsoft.Extensions.Logging;
using Contoso.GameNetCore.Proto.Features;

namespace Contoso.GameNetCore.Server.Kestrel.Protos.Internal
{
    internal class ProtosConnectionAdapter : IConnectionAdapter
    {
        private static readonly ClosedAdaptedConnection _closedAdaptedConnection = new ClosedAdaptedConnection();

        private readonly ProtosConnectionAdapterOptions _options;
        private readonly X509Certificate2 _serverCertificate;
        private readonly Func<ConnectionContext, string, X509Certificate2> _serverCertificateSelector;

        private readonly ILogger _logger;

        public ProtosConnectionAdapter(ProtosConnectionAdapterOptions options)
            : this(options, loggerFactory: null)
        {
        }

        public ProtosConnectionAdapter(ProtosConnectionAdapterOptions options, ILoggerFactory loggerFactory)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // capture the certificate now so it can't be switched after validation
            _serverCertificate = options.ServerCertificate;
            _serverCertificateSelector = options.ServerCertificateSelector;
            if (_serverCertificate == null && _serverCertificateSelector == null)
            {
                throw new ArgumentException(CoreStrings.ServerCertificateRequired, nameof(options));
            }

            // If a selector is provided then ignore the cert, it may be a default cert.
            if (_serverCertificateSelector != null)
            {
                // SslStream doesn't allow both.
                _serverCertificate = null;
            }
            else
            {
                EnsureCertificateIsAllowedForServerAuth(_serverCertificate);
            }

            _options = options;
            _logger = loggerFactory?.CreateLogger<ProtosConnectionAdapter>();
        }

        public bool IsProtos => true;

        public Task<IAdaptedConnection> OnConnectionAsync(ConnectionAdapterContext context)
        {
            // Don't trust SslStream not to block.
            return Task.Run(() => InnerOnConnectionAsync(context));
        }

        private async Task<IAdaptedConnection> InnerOnConnectionAsync(ConnectionAdapterContext context)
        {
            SslStream sslStream;
            bool certificateRequired;
            var feature = new Core.Internal.TlsConnectionFeature();
            context.Features.Set<ITlsConnectionFeature>(feature);
            context.Features.Set<ITlsHandshakeFeature>(feature);

            if (_options.ClientCertificateMode == ClientCertificateMode.NoCertificate)
            {
                sslStream = new SslStream(context.ConnectionStream);
                certificateRequired = false;
            }
            else
            {
                sslStream = new SslStream(context.ConnectionStream,
                    leaveInnerStreamOpen: false,
                    userCertificateValidationCallback: (sender, certificate, chain, sslPolicyErrors) =>
                    {
                        if (certificate == null)
                        {
                            return _options.ClientCertificateMode != ClientCertificateMode.RequireCertificate;
                        }

                        if (_options.ClientCertificateValidation == null)
                        {
                            if (sslPolicyErrors != SslPolicyErrors.None)
                            {
                                return false;
                            }
                        }

                        var certificate2 = ConvertToX509Certificate2(certificate);
                        if (certificate2 == null)
                        {
                            return false;
                        }

                        if (_options.ClientCertificateValidation != null)
                        {
                            if (!_options.ClientCertificateValidation(certificate2, chain, sslPolicyErrors))
                            {
                                return false;
                            }
                        }

                        return true;
                    });

                certificateRequired = true;
            }

            var timeoutFeature = context.Features.Get<IConnectionTimeoutFeature>();
            timeoutFeature.SetTimeout(_options.HandshakeTimeout);

            _options.OnHandshakeStarted?.Invoke();

            try
            {
                // Adapt to the SslStream signature
                ServerCertificateSelectionCallback selector = null;
                if (_serverCertificateSelector != null)
                {
                    selector = (sender, name) =>
                    {
                        context.Features.Set(sslStream);
                        var cert = _serverCertificateSelector(context.ConnectionContext, name);
                        if (cert != null)
                        {
                            EnsureCertificateIsAllowedForServerAuth(cert);
                        }
                        return cert;
                    };
                }

                var sslOptions = new SslServerAuthenticationOptions
                {
                    ServerCertificate = _serverCertificate,
                    ServerCertificateSelectionCallback = selector,
                    ClientCertificateRequired = certificateRequired,
                    EnabledSslProtocols = _options.SslProtocols,
                    CertificateRevocationCheckMode = _options.CheckCertificateRevocation ? X509RevocationMode.Online : X509RevocationMode.NoCheck,
                    ApplicationProtocols = new List<SslApplicationProtocol>()
                };

                // This is order sensitive
                if ((_options.ProtoProtocols & ProtoProtocols.Proto2) != 0)
                {
                    sslOptions.ApplicationProtocols.Add(SslApplicationProtocol.Proto2);
                    // https://tools.ietf.org/html/rfc7540#section-9.2.1
                    sslOptions.AllowRenegotiation = false;
                }

                if ((_options.ProtoProtocols & ProtoProtocols.Proto1) != 0)
                {
                    sslOptions.ApplicationProtocols.Add(SslApplicationProtocol.Proto11);
                }

                await sslStream.AuthenticateAsServerAsync(sslOptions, CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                _logger?.LogDebug(2, CoreStrings.AuthenticationTimedOut);
                sslStream.Dispose();
                return _closedAdaptedConnection;
            }
            catch (Exception ex) when (ex is IOException || ex is AuthenticationException)
            {
                _logger?.LogDebug(1, ex, CoreStrings.AuthenticationFailed);
                sslStream.Dispose();
                return _closedAdaptedConnection;
            }
            finally
            {
                timeoutFeature.CancelTimeout();
            }

            feature.ApplicationProtocol = sslStream.NegotiatedApplicationProtocol.Protocol;
            context.Features.Set<ITlsApplicationProtocolFeature>(feature);
            feature.ClientCertificate = ConvertToX509Certificate2(sslStream.RemoteCertificate);
            feature.CipherAlgorithm = sslStream.CipherAlgorithm;
            feature.CipherStrength = sslStream.CipherStrength;
            feature.HashAlgorithm = sslStream.HashAlgorithm;
            feature.HashStrength = sslStream.HashStrength;
            feature.KeyExchangeAlgorithm = sslStream.KeyExchangeAlgorithm;
            feature.KeyExchangeStrength = sslStream.KeyExchangeStrength;
            feature.Protocol = sslStream.SslProtocol;

            return new ProtosAdaptedConnection(sslStream);
        }

        private static void EnsureCertificateIsAllowedForServerAuth(X509Certificate2 certificate)
        {
            if (!CertificateLoader.IsCertificateAllowedForServerAuth(certificate))
            {
                throw new InvalidOperationException(CoreStrings.FormatInvalidServerCertificateEku(certificate.Thumbprint));
            }
        }

        private static X509Certificate2 ConvertToX509Certificate2(X509Certificate certificate)
        {
            if (certificate == null)
            {
                return null;
            }

            if (certificate is X509Certificate2 cert2)
            {
                return cert2;
            }

            return new X509Certificate2(certificate);
        }

        private class ProtosAdaptedConnection : IAdaptedConnection
        {
            private readonly SslStream _sslStream;

            public ProtosAdaptedConnection(SslStream sslStream)
            {
                _sslStream = sslStream;
            }

            public Stream ConnectionStream => _sslStream;

            public void Dispose()
            {
                _sslStream.Dispose();
            }
        }

        private class ClosedAdaptedConnection : IAdaptedConnection
        {
            public Stream ConnectionStream { get; } = new ClosedStream();

            public void Dispose()
            {
            }
        }
    }
}
