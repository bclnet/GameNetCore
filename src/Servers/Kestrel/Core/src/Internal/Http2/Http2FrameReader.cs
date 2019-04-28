// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto2
{
    internal static class Proto2FrameReader
    {
        /* https://tools.ietf.org/html/rfc7540#section-4.1
            +-----------------------------------------------+
            |                 Length (24)                   |
            +---------------+---------------+---------------+
            |   Type (8)    |   Flags (8)   |
            +-+-------------+---------------+-------------------------------+
            |R|                 Stream Identifier (31)                      |
            +=+=============================================================+
            |                   Frame Payload (0...)                      ...
            +---------------------------------------------------------------+
        */
        public const int HeaderLength = 9;

        private const int TypeOffset = 3;
        private const int FlagsOffset = 4;
        private const int StreamIdOffset = 5;

        public const int SettingSize = 6; // 2 bytes for the id, 4 bytes for the value.

        public static bool ReadFrame(ReadOnlySequence<byte> readableBuffer, Proto2Frame frame, uint maxFrameSize, out ReadOnlySequence<byte> framePayload)
        {
            framePayload = ReadOnlySequence<byte>.Empty;

            if (readableBuffer.Length < HeaderLength)
            {
                return false;
            }

            var headerSlice = readableBuffer.Slice(0, HeaderLength);
            var header = headerSlice.ToSpan();

            var payloadLength = (int)Bitshifter.ReadUInt24BigEndian(header);
            if (payloadLength > maxFrameSize)
            {
                throw new Proto2ConnectionErrorException(CoreStrings.FormatProto2ErrorFrameOverLimit(payloadLength, maxFrameSize), Proto2ErrorCode.FRAME_SIZE_ERROR);
            }

            // Make sure the whole frame is buffered
            var frameLength = HeaderLength + payloadLength;
            if (readableBuffer.Length < frameLength)
            {
                return false;
            }

            frame.PayloadLength = payloadLength;
            frame.Type = (Proto2FrameType)header[TypeOffset];
            frame.Flags = header[FlagsOffset];
            frame.StreamId = (int)Bitshifter.ReadUInt31BigEndian(header.Slice(StreamIdOffset));

            var extendedHeaderLength = ReadExtendedFields(frame, readableBuffer);

            // The remaining payload minus the extra fields
            framePayload = readableBuffer.Slice(HeaderLength + extendedHeaderLength, payloadLength - extendedHeaderLength);

            return true;
        }

        private static int ReadExtendedFields(Proto2Frame frame, ReadOnlySequence<byte> readableBuffer)
        {
            // Copy in any extra fields for the given frame type
            var extendedHeaderLength = GetPayloadFieldsLength(frame);

            if (extendedHeaderLength > frame.PayloadLength)
            {
                throw new Proto2ConnectionErrorException(
                    CoreStrings.FormatProto2ErrorUnexpectedFrameLength(frame.Type, expectedLength: extendedHeaderLength), Proto2ErrorCode.FRAME_SIZE_ERROR);
            }

            var extendedHeaders = readableBuffer.Slice(HeaderLength, extendedHeaderLength).ToSpan();

            // Parse frame type specific fields
            switch (frame.Type)
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
                case Proto2FrameType.DATA: // Variable 0 or 1
                    frame.DataPadLength = frame.DataHasPadding ? extendedHeaders[0] : (byte)0;
                    break;

                /* https://tools.ietf.org/html/rfc7540#section-6.2
                    +---------------+
                    |Pad Length? (8)|
                    +-+-------------+-----------------------------------------------+
                    |E|                 Stream Dependency? (31)                     |
                    +-+-------------+-----------------------------------------------+
                    |  Weight? (8)  |
                    +-+-------------+-----------------------------------------------+
                    |                   Header Block Fragment (*)                 ...
                    +---------------------------------------------------------------+
                    |                           Padding (*)                       ...
                    +---------------------------------------------------------------+
                */
                case Proto2FrameType.HEADERS:
                    if (frame.HeadersHasPadding)
                    {
                        frame.HeadersPadLength = extendedHeaders[0];
                        extendedHeaders = extendedHeaders.Slice(1);
                    }
                    else
                    {
                        frame.HeadersPadLength = 0;
                    }

                    if (frame.HeadersHasPriority)
                    {
                        frame.HeadersStreamDependency = (int)Bitshifter.ReadUInt31BigEndian(extendedHeaders);
                        frame.HeadersPriorityWeight = extendedHeaders.Slice(4)[0];
                    }
                    else
                    {
                        frame.HeadersStreamDependency = 0;
                        frame.HeadersPriorityWeight = 0;
                    }
                    break;

                /* https://tools.ietf.org/html/rfc7540#section-6.8
                    +-+-------------------------------------------------------------+
                    |R|                  Last-Stream-ID (31)                        |
                    +-+-------------------------------------------------------------+
                    |                      Error Code (32)                          |
                    +---------------------------------------------------------------+
                    |                  Additional Debug Data (*)                    |
                    +---------------------------------------------------------------+
                */
                case Proto2FrameType.GOAWAY:
                    frame.GoAwayLastStreamId = (int)Bitshifter.ReadUInt31BigEndian(extendedHeaders);
                    frame.GoAwayErrorCode = (Proto2ErrorCode)BinaryPrimitives.ReadUInt32BigEndian(extendedHeaders.Slice(4));
                    break;

                /* https://tools.ietf.org/html/rfc7540#section-6.3
                    +-+-------------------------------------------------------------+
                    |E|                  Stream Dependency (31)                     |
                    +-+-------------+-----------------------------------------------+
                    |   Weight (8)  |
                    +-+-------------+
                */
                case Proto2FrameType.PRIORITY:
                    frame.PriorityStreamDependency = (int)Bitshifter.ReadUInt31BigEndian(extendedHeaders);
                    frame.PriorityWeight = extendedHeaders.Slice(4)[0];
                    break;

                /* https://tools.ietf.org/html/rfc7540#section-6.4
                    +---------------------------------------------------------------+
                    |                        Error Code (32)                        |
                    +---------------------------------------------------------------+
                */
                case Proto2FrameType.RST_STREAM:
                    frame.RstStreamErrorCode = (Proto2ErrorCode)BinaryPrimitives.ReadUInt32BigEndian(extendedHeaders);
                    break;

                /* https://tools.ietf.org/html/rfc7540#section-6.9
                    +-+-------------------------------------------------------------+
                    |R|              Window Size Increment (31)                     |
                    +-+-------------------------------------------------------------+
                */
                case Proto2FrameType.WINDOW_UPDATE:
                    frame.WindowUpdateSizeIncrement = (int)Bitshifter.ReadUInt31BigEndian(extendedHeaders);
                    break;

                case Proto2FrameType.PING: // Opaque payload 8 bytes long
                case Proto2FrameType.SETTINGS: // Settings are general payload
                case Proto2FrameType.CONTINUATION: // None
                case Proto2FrameType.PUSH_PROMISE: // Not implemented frames are ignored at this phase
                default:
                    return 0;
            }

            return extendedHeaderLength;
        }

        // The length in bytes of additional fields stored in the payload section.
        // This may be variable based on flags, but should be no more than 8 bytes.
        public static int GetPayloadFieldsLength(Proto2Frame frame)
        {
            switch (frame.Type)
            {
                // TODO: Extract constants
                case Proto2FrameType.DATA: // Variable 0 or 1
                    return frame.DataHasPadding ? 1 : 0;
                case Proto2FrameType.HEADERS:
                    return (frame.HeadersHasPadding ? 1 : 0) + (frame.HeadersHasPriority ? 5 : 0); // Variable 0 to 6
                case Proto2FrameType.GOAWAY:
                    return 8; // Last stream id and error code.
                case Proto2FrameType.PRIORITY:
                    return 5; // Stream dependency and weight
                case Proto2FrameType.RST_STREAM:
                    return 4; // Error code
                case Proto2FrameType.WINDOW_UPDATE:
                    return 4; // Update size
                case Proto2FrameType.PING: // 8 bytes of opaque data
                case Proto2FrameType.SETTINGS: // Settings are general payload
                case Proto2FrameType.CONTINUATION: // None
                case Proto2FrameType.PUSH_PROMISE: // Not implemented frames are ignored at this phase
                default:
                    return 0;
            }
        }

        public static IList<Proto2PeerSetting> ReadSettings(ReadOnlySequence<byte> payload)
        {
            var data = payload.ToSpan();
            Debug.Assert(data.Length % SettingSize == 0, "Invalid settings payload length");
            var settingsCount = data.Length / SettingSize;

            var settings = new Proto2PeerSetting[settingsCount];
            for (int i = 0; i < settings.Length; i++)
            {
                settings[i] = ReadSetting(data);
                data = data.Slice(SettingSize);
            }
            return settings;
        }

        private static Proto2PeerSetting ReadSetting(ReadOnlySpan<byte> payload)
        {
            var id = (Proto2SettingsParameter)BinaryPrimitives.ReadUInt16BigEndian(payload);
            var value = BinaryPrimitives.ReadUInt32BigEndian(payload.Slice(2));

            return new Proto2PeerSetting(id, value);
        }
    }
}
