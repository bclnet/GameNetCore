// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Contoso.GameNetCore.Proto;

namespace Contoso.GameNetCore.Owin
{
    public class OwinGameSocketAcceptContext : GameSocketAcceptContext
    {
        private IDictionary<string, object> _options;

        public OwinGameSocketAcceptContext() : this(new Dictionary<string, object>(1))
        {
        }

        public OwinGameSocketAcceptContext(IDictionary<string, object> options)
        {
            _options = options;
        }

        public override string SubProtocol
        {
            get
            {
                object obj;
                if (_options != null && _options.TryGetValue(OwinConstants.GameSocket.SubProtocol, out obj))
                {
                    return (string)obj;
                }
                return null;
            }
            set
            {
                if (_options == null)
                {
                    _options = new Dictionary<string, object>(1);
                }
                _options[OwinConstants.GameSocket.SubProtocol] = value;
            }
        }

        public IDictionary<string, object> Options
        {
            get { return _options; }
        }
    }
}