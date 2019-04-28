// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Contoso.GameNetCore.Server.Kestrel.Core;
using Contoso.GameNetCore.Server.Kestrel.Protos;
using Contoso.GameNetCore.Server.Kestrel.Protos.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Contoso.GameNetCore.Hosting
{
    /// <summary>
    /// Extension methods for <see cref="ListenOptions"/> that configure Kestrel to use HTTPS for a given endpoint.
    /// </summary>
    public static class ListenOptionsProtosExtensions
    {
        /// <summary>
        /// Configure Kestrel to use HTTPS with the default certificate if available.
        /// This will throw if no default certificate is configured.
        /// </summary>
        /// <param name="listenOptions">The <see cref="ListenOptions"/> to configure.</param>
        /// <returns>The <see cref="ListenOptions"/>.</returns>
        public static ListenOptions UseProtos(this ListenOptions listenOptions) => listenOptions.UseProtos(_ => { });
 
        /// <summary>
        /// Configure Kestrel to use HTTPS.
        /// </summary>
        /// <param name="listenOptions">The <see cref="ListenOptions"/> to configure.</param>
        /// <param name="fileName">The name of a certificate file, relative to the directory that contains the application
        /// content files.</param>
        /// <returns>The <see cref="ListenOptions"/>.</returns>
        public static ListenOptions UseProtos(this ListenOptions listenOptions, string fileName)
        {
            var env = listenOptions.KestrelServerOptions.ApplicationServices.GetRequiredService<IHostEnvironment>();
            return listenOptions.UseProtos(new X509Certificate2(Path.Combine(env.ContentRootPath, fileName)));
        }

        /// <summary>
        /// Configure Kestrel to use HTTPS.
        /// </summary>
        /// <param name="listenOptions">The <see cref="ListenOptions"/> to configure.</param>
        /// <param name="fileName">The name of a certificate file, relative to the directory that contains the application
        /// content files.</param>
        /// <param name="password">The password required to access the X.509 certificate data.</param>
        /// <returns>The <see cref="ListenOptions"/>.</returns>
        public static ListenOptions UseProtos(this ListenOptions listenOptions, string fileName, string password)
        {
            var env = listenOptions.KestrelServerOptions.ApplicationServices.GetRequiredService<IHostEnvironment>();
            return listenOptions.UseProtos(new X509Certificate2(Path.Combine(env.ContentRootPath, fileName), password));
        }

        /// <summary>
        /// Configure Kestrel to use HTTPS.
        /// </summary>
        /// <param name="listenOptions">The <see cref="ListenOptions"/> to configure.</param>
        /// <param name="fileName">The name of a certificate file, relative to the directory that contains the application content files.</param>
        /// <param name="password">The password required to access the X.509 certificate data.</param>
        /// <param name="configureOptions">An Action to configure the <see cref="ProtosConnectionAdapterOptions"/>.</param>
        /// <returns>The <see cref="ListenOptions"/>.</returns>
        public static ListenOptions UseProtos(this ListenOptions listenOptions, string fileName, string password,
            Action<ProtosConnectionAdapterOptions> configureOptions)
        {
            var env = listenOptions.KestrelServerOptions.ApplicationServices.GetRequiredService<IHostEnvironment>();
            return listenOptions.UseProtos(new X509Certificate2(Path.Combine(env.ContentRootPath, fileName), password), configureOptions);
        }

        /// <summary>
        /// Configure Kestrel to use HTTPS.
        /// </summary>
        /// <param name="listenOptions">The <see cref="ListenOptions"/> to configure.</param>
        /// <param name="storeName">The certificate store to load the certificate from.</param>
        /// <param name="subject">The subject name for the certificate to load.</param>
        /// <returns>The <see cref="ListenOptions"/>.</returns>
        public static ListenOptions UseProtos(this ListenOptions listenOptions, StoreName storeName, string subject)
            => listenOptions.UseProtos(storeName, subject, allowInvalid: false);

        /// <summary>
        /// Configure Kestrel to use HTTPS.
        /// </summary>
        /// <param name="listenOptions">The <see cref="ListenOptions"/> to configure.</param>
        /// <param name="storeName">The certificate store to load the certificate from.</param>
        /// <param name="subject">The subject name for the certificate to load.</param>
        /// <param name="allowInvalid">Indicates if invalid certificates should be considered, such as self-signed certificates.</param>
        /// <returns>The <see cref="ListenOptions"/>.</returns>
        public static ListenOptions UseProtos(this ListenOptions listenOptions, StoreName storeName, string subject, bool allowInvalid)
            => listenOptions.UseProtos(storeName, subject, allowInvalid, StoreLocation.CurrentUser);

        /// <summary>
        /// Configure Kestrel to use HTTPS.
        /// </summary>
        /// <param name="listenOptions">The <see cref="ListenOptions"/> to configure.</param>
        /// <param name="storeName">The certificate store to load the certificate from.</param>
        /// <param name="subject">The subject name for the certificate to load.</param>
        /// <param name="allowInvalid">Indicates if invalid certificates should be considered, such as self-signed certificates.</param>
        /// <param name="location">The store location to load the certificate from.</param>
        /// <returns>The <see cref="ListenOptions"/>.</returns>
        public static ListenOptions UseProtos(this ListenOptions listenOptions, StoreName storeName, string subject, bool allowInvalid, StoreLocation location)
            => listenOptions.UseProtos(storeName, subject, allowInvalid, location, configureOptions: _ => { });

        /// <summary>
        /// Configure Kestrel to use HTTPS.
        /// </summary>
        /// <param name="listenOptions">The <see cref="ListenOptions"/> to configure.</param>
        /// <param name="storeName">The certificate store to load the certificate from.</param>
        /// <param name="subject">The subject name for the certificate to load.</param>
        /// <param name="allowInvalid">Indicates if invalid certificates should be considered, such as self-signed certificates.</param>
        /// <param name="location">The store location to load the certificate from.</param>
        /// <param name="configureOptions">An Action to configure the <see cref="ProtosConnectionAdapterOptions"/>.</param>
        /// <returns>The <see cref="ListenOptions"/>.</returns>
        public static ListenOptions UseProtos(this ListenOptions listenOptions, StoreName storeName, string subject, bool allowInvalid, StoreLocation location,
            Action<ProtosConnectionAdapterOptions> configureOptions)
        {
            return listenOptions.UseProtos(CertificateLoader.LoadFromStoreCert(subject, storeName.ToString(), location, allowInvalid), configureOptions);
        }

        /// <summary>
        /// Configure Kestrel to use HTTPS.
        /// </summary>
        /// <param name="listenOptions"> The <see cref="ListenOptions"/> to configure.</param>
        /// <param name="serverCertificate">The X.509 certificate.</param>
        /// <returns>The <see cref="ListenOptions"/>.</returns>
        public static ListenOptions UseProtos(this ListenOptions listenOptions, X509Certificate2 serverCertificate)
        {
            if (serverCertificate == null)
            {
                throw new ArgumentNullException(nameof(serverCertificate));
            }

            return listenOptions.UseProtos(options =>
            {
                options.ServerCertificate = serverCertificate;
            });
        }

        /// <summary>
        /// Configure Kestrel to use HTTPS.
        /// </summary>
        /// <param name="listenOptions">The <see cref="ListenOptions"/> to configure.</param>
        /// <param name="serverCertificate">The X.509 certificate.</param>
        /// <param name="configureOptions">An Action to configure the <see cref="ProtosConnectionAdapterOptions"/>.</param>
        /// <returns>The <see cref="ListenOptions"/>.</returns>
        public static ListenOptions UseProtos(this ListenOptions listenOptions, X509Certificate2 serverCertificate,
            Action<ProtosConnectionAdapterOptions> configureOptions)
        {
            if (serverCertificate == null)
            {
                throw new ArgumentNullException(nameof(serverCertificate));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            return listenOptions.UseProtos(options =>
            {
                options.ServerCertificate = serverCertificate;
                configureOptions(options);
            });
        }

        /// <summary>
        /// Configure Kestrel to use HTTPS.
        /// </summary>
        /// <param name="listenOptions">The <see cref="ListenOptions"/> to configure.</param>
        /// <param name="configureOptions">An action to configure options for HTTPS.</param>
        /// <returns>The <see cref="ListenOptions"/>.</returns>
        public static ListenOptions UseProtos(this ListenOptions listenOptions, Action<ProtosConnectionAdapterOptions> configureOptions)
        {
            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            var options = new ProtosConnectionAdapterOptions();
            listenOptions.KestrelServerOptions.ApplyProtosDefaults(options);
            configureOptions(options);
            listenOptions.KestrelServerOptions.ApplyDefaultCert(options);

            if (options.ServerCertificate == null && options.ServerCertificateSelector == null)
            {
                throw new InvalidOperationException(CoreStrings.NoCertSpecifiedNoDevelopmentCertificateFound);
            }
            return listenOptions.UseProtos(options);
        }

        // Use Protos if a default cert is available
        internal static bool TryUseProtos(this ListenOptions listenOptions)
        {
            var options = new ProtosConnectionAdapterOptions();
            listenOptions.KestrelServerOptions.ApplyProtosDefaults(options);
            listenOptions.KestrelServerOptions.ApplyDefaultCert(options);

            if (options.ServerCertificate == null && options.ServerCertificateSelector == null)
            {
                return false;
            }
            listenOptions.UseProtos(options);
            return true;
        }

        /// <summary>
        /// Configure Kestrel to use HTTPS.
        /// </summary>
        /// <param name="listenOptions">The <see cref="ListenOptions"/> to configure.</param>
        /// <param name="httpsOptions">Options to configure HTTPS.</param>
        /// <returns>The <see cref="ListenOptions"/>.</returns>
        public static ListenOptions UseProtos(this ListenOptions listenOptions, ProtosConnectionAdapterOptions httpsOptions)
        {
            var loggerFactory = listenOptions.KestrelServerOptions.ApplicationServices.GetRequiredService<ILoggerFactory>();
            // Set the list of protocols from listen options
            httpsOptions.ProtoProtocols = listenOptions.Protocols;
            listenOptions.ConnectionAdapters.Add(new ProtosConnectionAdapter(httpsOptions, loggerFactory));
            return listenOptions;
        }
    }
}
