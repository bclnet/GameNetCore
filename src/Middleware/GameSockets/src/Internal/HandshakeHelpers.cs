// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Contoso.GameNetCore.GameSockets.Internal
{
    internal static class HandshakeHelpers
    {
        /// <summary>
        /// Gets request headers needed process the handshake on the server.
        /// </summary>
        public static readonly IEnumerable<string> NeededHeaders = new[]
        {
            Constants.Headers.Upgrade,
            Constants.Headers.Connection,
            Constants.Headers.SecGameSocketKey,
            Constants.Headers.SecGameSocketVersion
        };

        // Verify Method, Upgrade, Connection, version,  key, etc..
        public static bool CheckSupportedGameSocketRequest(string method, IEnumerable<KeyValuePair<string, string>> headers)
        {
            bool validUpgrade = false, validConnection = false, validKey = false, validVersion = false;

            if (!string.Equals("GET", method, StringComparison.OrdinalIgnoreCase))
                return false;

            foreach (var pair in headers)
                if (string.Equals(Constants.Headers.Connection, pair.Key, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(Constants.Headers.ConnectionUpgrade, pair.Value, StringComparison.OrdinalIgnoreCase))
                        validConnection = true;
                }
                else if (string.Equals(Constants.Headers.Upgrade, pair.Key, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(Constants.Headers.UpgradeGameSocket, pair.Value, StringComparison.OrdinalIgnoreCase))
                        validUpgrade = true;
                }
                else if (string.Equals(Constants.Headers.SecGameSocketVersion, pair.Key, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(Constants.Headers.SupportedVersion, pair.Value, StringComparison.OrdinalIgnoreCase))
                        validVersion = true;
                }
                else if (string.Equals(Constants.Headers.SecGameSocketKey, pair.Key, StringComparison.OrdinalIgnoreCase))
                {
                    validKey = IsRequestKeyValid(pair.Value);
                }

            return validConnection && validUpgrade && validVersion && validKey;
        }

        public static void GenerateResponseHeaders(string key, string subProtocol, IHeaderDictionary headers)
        {
            headers[Constants.Headers.Connection] = Constants.Headers.ConnectionUpgrade;
            headers[Constants.Headers.Upgrade] = Constants.Headers.UpgradeGameSocket;
            headers[Constants.Headers.SecGameSocketAccept] = CreateResponseKey(key);
            if (!string.IsNullOrWhiteSpace(subProtocol))
                headers[Constants.Headers.SecGameSocketProtocol] = subProtocol;
        }

        /// <summary>
        /// Validates the Sec-GameSocket-Key request header
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsRequestKeyValid(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            try
            {
                var data = Convert.FromBase64String(value);
                return data.Length == 16;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string CreateResponseKey(string requestKey)
        {
            // "The value of this header field is constructed by concatenating /key/, defined above in step 4
            // in Section 4.2.2, with the string "258EAFA5- E914-47DA-95CA-C5AB0DC85B11", taking the SHA-1 hash of
            // this concatenated value to obtain a 20-byte value and base64-encoding"
            // https://tools.ietf.org/html/rfc6455#section-4.2.2
            using (var algorithm = SHA1.Create())
            {
                var merged = (requestKey ?? throw new ArgumentNullException(nameof(requestKey))) + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                var mergedBytes = Encoding.UTF8.GetBytes(merged);
                var hashedBytes = algorithm.ComputeHash(mergedBytes);
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}