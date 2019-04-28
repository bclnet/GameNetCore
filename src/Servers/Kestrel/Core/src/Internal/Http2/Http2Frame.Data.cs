// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto2
{
    /*
        +---------------+
        |Pad Length? (8)|
        +---------------+-----------------------------------------------+
        |                            Data (*)                         ...
        +---------------------------------------------------------------+
        |                           Padding (*)                       ...
        +---------------------------------------------------------------+
    */
    internal partial class Proto2Frame
    {
        public Proto2DataFrameFlags DataFlags
        {
            get => (Proto2DataFrameFlags)Flags;
            set => Flags = (byte)value;
        }

        public bool DataEndStream => (DataFlags & Proto2DataFrameFlags.END_STREAM) == Proto2DataFrameFlags.END_STREAM;

        public bool DataHasPadding => (DataFlags & Proto2DataFrameFlags.PADDED) == Proto2DataFrameFlags.PADDED;

        public byte DataPadLength { get; set; }

        private int DataPayloadOffset => DataHasPadding ? 1 : 0;

        public int DataPayloadLength => PayloadLength - DataPayloadOffset - DataPadLength;

        public void PrepareData(int streamId, byte? padLength = null)
        {
            PayloadLength = 0;
            Type = Proto2FrameType.DATA;
            DataFlags = padLength.HasValue ? Proto2DataFrameFlags.PADDED : Proto2DataFrameFlags.NONE;
            StreamId = streamId;
            DataPadLength = padLength ?? 0;
        }
    }
}
