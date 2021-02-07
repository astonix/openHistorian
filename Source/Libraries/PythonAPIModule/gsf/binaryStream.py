#******************************************************************************************************
#  remoteBinaryStream.py - Gbtc
#
#  Copyright © 2021, Grid Protection Alliance.  All Rights Reserved.
#
#  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
#  the NOTICE file distributed with this work for additional information regarding copyright ownership.
#  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
#  file except in compliance with the License. You may obtain a copy of the License at:
#
#      http://opensource.org/licenses/MIT
#
#  Unless agreed to in writing, the subject software distributed under the License is distributed on an
#  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
#  License for the specific language governing permissions and limitations.
#
#  Code Modification History:
#  ----------------------------------------------------------------------------------------------------
#  01/31/2021 - J. Ritchie Carroll
#       Generated original version of source code.
#
#******************************************************************************************************

from gsf.streamEncoder import streamEncoder
from gsf.encoding7Bit import encoding7Bit
from gsf import ByteSize, Validate
from uuid import UUID
import numpy as np

class binaryStream:
    """
    Establishes buffered I/O around a base stream, e.g., a socket
    """
    
    # Source C# reference: RemoteBinaryStream

    BufferSize = 1420

    def __init__(self, stream: streamEncoder):
        self.stream = stream
        self.receiveBuffer = bytearray(binaryStream.BufferSize)
        self.sendBuffer = bytearray(binaryStream.BufferSize)
        self.sendLength = 0
        self.receiveLength = 0
        self.receivePosition = 0

    @property
    def SendBufferFreeSpace(self) -> int:
        return binaryStream.BufferSize - self.sendLength

    @property
    def ReceiveBufferAvailable(self) -> int:
        return self.receiveLength - self.receivePosition

    def Flush(self):
        if self.sendLength <= 0:
            return

        self.stream.Write(bytes(self.sendBuffer), 0, self.sendLength)
        self.sendLength = 0

    def Read(self, buffer: bytearray, offset: int, count: int) -> int:
        if count <= 0:
            return 0;

        receiveBufferLength = self.ReceiveBufferAvailable

        # Check if there is enough in the receive buffer to handle request
        if count <= receiveBufferLength:
            for i in range(count):
                buffer[offset + i] = self.receiveBuffer[self.receivePosition + i]

            self.receivePosition += count
            return count

        originalCount = count

        # Empty existing receive buffer
        if receiveBufferLength > 0:
            for i in range(receiveBufferLength):
                buffer[offset + i] = self.receiveBuffer[self.receivePosition + i]

            self.receivePosition = 0
            self.receiveLength = 0
            offset += receiveBufferLength
            count -= receiveBufferLength
        
        # If more than 100 bytes remain, skip receive buffer 
        # and copy directly to the destination
        if count > 100:
            # Loop since socket reads can return partial results
            while count > 0:
                receiveBufferLength = self.stream.Read(buffer, offset, count)

                if receiveBufferLength == 0:
                    raise RuntimeError("End of stream")

                offset += receiveBufferLength
                count -= receiveBufferLength            
        else:
            # With fewer than 100 bytes requested, fill receive buffer 
            # then copy to destination
            prebufferLength = binaryStream.BufferSize
            self.receiveLength = 0

            while self.receiveLength < count:
                receiveBufferLength = self.stream.Read(self.receiveBuffer, self.receiveLength, prebufferLength)
                
                if receiveBufferLength == 0:
                    raise RuntimeError("End of stream")

                self.receiveLength += receiveBufferLength
                prebufferLength -= receiveBufferLength

            for i in range(count):
                buffer[offset + i] = self.receiveBuffer[i]

            self.receivePosition = count

        return originalCount

    def Write(self, buffer: bytes, offset: int, count: int) -> int:
        if self.SendBufferFreeSpace < count:
            self.Flush()

        if count > 100:
            self.Flush()
            self.stream.Write(buffer, offset, count)
        else:
            for i in range(count):
                self.sendBuffer[self.sendLength + i] = buffer[offset + i]
            
            self.sendLength += count

        return count

    def ReadAll(self, buffer: bytearray, position: int, length: int):
        """
        Reads all of the provided bytes. Will not return prematurely, 
        continues to execute a `Read` command until the entire
        `length` has been read.
        """
        Validate.Parameters(buffer, position, length)

        while length > 0:
            bytesRead = self.Read(buffer, position, length)
            
            if bytesRead == 0:
                raise RuntimeError("End of stream")

            length -= bytesRead
            position += bytesRead

    def ReadBytes(self, count: int) -> bytes:
        buffer = bytearray(count)
        self.ReadAll(buffer, 0, count)
        return bytes(buffer)

    def ReadBuffer(self) -> bytes:
        return self.ReadBytes(self.Read7BitUInt32())

    def ReadString(self) -> str:
        return self.ReadBuffer().decode("utf-8")
    
    def ReadGuid(self) -> UUID:
        return UUID(bytes_le=self.ReadBytes(16))

    def Read7BitInt32(self) -> np.int32:
        return encoding7Bit.ReadInt32(self.ReadByte)
        
    def Read7BitUInt32(self) -> np.uint32:
        return encoding7Bit.ReadUInt32(self.ReadByte)

    def Read7BitInt64(self) -> np.int64:
        return encoding7Bit.ReadInt64(self.ReadByte)

    def Read7BitUInt64(self) -> np.uint64:
        return encoding7Bit.ReadUInt64(self.ReadByte)

    def ReadByte(self) -> np.uint8:
        size = ByteSize.UINT8

        if self.receivePosition < self.receiveLength:
            value = self.receiveBuffer[self.receivePosition]
            self.receivePosition += size
            return np.uint8(value)

        return self.stream.ReadByte()

    def WriteBuffer(self, value: bytes) -> int:
        count = len(value)
        return self.Write7BitUInt32(count) + self.Write(value, 0, count)

    def WriteString(self, value: str) -> int:
        return self.WriteBuffer(value.encode("utf-8"))
    
    def WriteGuid(self, value: UUID) -> int:        
        return self.Write(value.bytes_le, 0, 16)

    def Write7BitInt32(self, value: np.int32) -> int:
        return encoding7Bit.WriteInt32(self.WriteByte, value)

    def Write7BitUInt32(self, value: np.uint32) -> int:
        return encoding7Bit.WriteUInt32(self.WriteByte, value)

    def Write7BitInt64(self, value: np.int64) -> int:
        return encoding7Bit.WriteInt64(self.WriteByte, value)
    
    def Write7BitUInt64(self, value: np.uint64) -> int:
        return encoding7Bit.WriteUInt64(self.WriteByte, value)

    def WriteByte(self, value: np.uint8) -> int:
        size = ByteSize.UINT8

        if self.sendLength < binaryStream.BufferSize:
            self.sendBuffer[self.sendLength] = value
            self.sendLength += size
            return size
        
        return self.stream.WriteByte(value)

    def ReadBoolean(self) -> bool:
        return self.ReadByte() != 0

    def WriteBoolean(self, value: bool) -> int:
        if value:
            self.WriteByte(1)
        else:
            self.WriteByte(0)

        return 1

    def ReadInt16(self, byteorder = "little") -> np.int16:
        size = ByteSize.INT16

        if self.receivePosition <= self.receiveLength - size:
            dtype = np.dtype(np.int16) if byteorder == "little" else np.dtype(np.int16).newbyteorder(">")
            value = np.frombuffer(self.receiveBuffer[self.receivePosition:self.receivePosition + size], dtype)[0]
            self.receivePosition += size
            return value

        return self.stream.ReadInt16(byteorder)

    def WriteInt16(self, value: np.int16, byteorder = "little") -> int:
        size = ByteSize.INT16

        if self.sendLength <= binaryStream.BufferSize - size:
            buffer = int(value).to_bytes(size, byteorder, signed=True)

            for i in range(size):
                self.sendBuffer[self.sendLength + i] = buffer[i]

            self.sendLength += size
            return size

        return self.stream.WriteInt16(value, byteorder)

    def ReadUInt16(self, byteorder = "little") -> np.uint16:
        size = ByteSize.UINT16

        if self.receivePosition <= self.receiveLength - size:
            dtype = np.dtype(np.uint16) if byteorder == "little" else np.dtype(np.uint16).newbyteorder(">")
            value = np.frombuffer(self.receiveBuffer[self.receivePosition:self.receivePosition + size], dtype)[0]
            self.receivePosition += size
            return value

        return self.stream.ReadUInt16(byteorder)

    def WriteUInt16(self, value: np.uint16, byteorder = "little") -> int:
        size = ByteSize.UINT16

        if self.sendLength <= binaryStream.BufferSize - size:
            buffer = int(value).to_bytes(size, byteorder)

            for i in range(size):
                self.sendBuffer[self.sendLength + i] = buffer[i]

            self.sendLength += size
            return size

        return self.stream.WriteUInt16(value, byteorder)

    def ReadInt32(self, byteorder = "little") -> np.int32:
        size = ByteSize.INT32

        if self.receivePosition <= self.receiveLength - size:
            dtype = np.dtype(np.int32) if byteorder == "little" else np.dtype(np.int32).newbyteorder(">")
            value = np.frombuffer(self.receiveBuffer[self.receivePosition:self.receivePosition + size], dtype)[0]
            self.receivePosition += size
            return value

        return self.stream.ReadInt32(byteorder)

    def WriteInt32(self, value: np.int32, byteorder = "little") -> int:
        size = ByteSize.INT32

        if self.sendLength <= binaryStream.BufferSize - size:
            buffer = int(value).to_bytes(size, byteorder, signed=True)

            for i in range(size):
                self.sendBuffer[self.sendLength + i] = buffer[i]

            self.sendLength += size
            return size

        return self.stream.WriteInt32(value, byteorder)

    def ReadUInt32(self, byteorder = "little") -> np.uint32:
        size = ByteSize.UINT32

        if self.receivePosition <= self.receiveLength - size:
            dtype = np.dtype(np.uint32) if byteorder == "little" else np.dtype(np.uint32).newbyteorder(">")
            value = np.frombuffer(self.receiveBuffer[self.receivePosition:self.receivePosition + size], dtype)[0]
            self.receivePosition += size
            return value

        return self.stream.ReadUInt32(byteorder)

    def WriteUInt32(self, value: np.uint32, byteorder = "little") -> int:
        size = ByteSize.UINT32

        if self.sendLength <= binaryStream.BufferSize - size:
            buffer = int(value).to_bytes(size, byteorder)

            for i in range(size):
                self.sendBuffer[self.sendLength + i] = buffer[i]

            self.sendLength += size
            return size

        return self.stream.WriteUInt32(value, byteorder)

    def ReadInt64(self, byteorder = "little") -> np.int64:
        size = ByteSize.INT64

        if self.receivePosition <= self.receiveLength - size:
            dtype = np.dtype(np.int64) if byteorder == "little" else np.dtype(np.int64).newbyteorder(">")
            value = np.frombuffer(self.receiveBuffer[self.receivePosition:self.receivePosition + size], dtype)[0]
            self.receivePosition += size
            return value

        return self.stream.ReadInt64(byteorder)

    def WriteInt64(self, value: np.int64, byteorder = "little") -> int:
        size = ByteSize.INT64

        if self.sendLength <= binaryStream.BufferSize - size:
            buffer = int(value).to_bytes(size, byteorder, signed=True)

            for i in range(size):
                self.sendBuffer[self.sendLength + i] = buffer[i]

            self.sendLength += size
            return size

        return self.stream.WriteInt64(value, byteorder)

    def ReadUInt64(self, byteorder = "little") -> np.uint64:
        size = ByteSize.UINT64

        if self.receivePosition <= self.receiveLength - size:
            dtype = np.dtype(np.uint64) if byteorder == "little" else np.dtype(np.uint64).newbyteorder(">")
            value = np.frombuffer(self.receiveBuffer[self.receivePosition:self.receivePosition + size], dtype)[0]
            self.receivePosition += size
            return value

        return self.stream.ReadUInt64(byteorder)

    def WriteUInt64(self, value: np.uint64, byteorder = "little") -> int:
        size = ByteSize.UINT64

        if self.sendLength <= binaryStream.BufferSize - size:
            buffer = int(value).to_bytes(size, byteorder)

            for i in range(size):
                self.sendBuffer[self.sendLength + i] = buffer[i]

            self.sendLength += size
            return size

        return self.stream.WriteUInt64(value, byteorder)

    def __sendBufferRead(self, length: int) -> bytes:
        buffer = bytearray(length)

        for i in range(length):
            buffer[i] = self.receiveBuffer[self.receivePosition + i]

        self.receivePosition += length
        return bytes(buffer)

    def __sendBufferWrite(self, buffer: bytes) -> int:
        length = len(buffer)

        for i in range(length):
            self.sendBuffer[self.sendLength + i] = buffer[i]

        self.sendLength += length
        return length


    def Write7BitUInt64(self, value: np.uint64) -> int:
        if self.sendLength <= binaryStream.BufferSize - ByteSize.ENC7BIT:
            stream = streamEncoder(self.__sendBufferRead, self.__sendBufferWrite)
            return stream.Write7BitUInt64(value)

        return self.stream.Write7BitUInt64(value)

    def Read7BitUInt64(self) -> np.uint64:
        if self.receivePosition <= self.receiveLength - ByteSize.ENC7BIT:
            stream = streamEncoder(self.__sendBufferRead, self.__sendBufferWrite)
            return stream.Read7BitUInt64()

        return self.stream.Read7BitUInt64()
