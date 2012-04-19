Module StreamModules
    Function ReadUInt16(ByRef Str As IO.Stream) As UInt16
        Dim b(1) As Byte
        Str.Read(b, 0, b.Length)
        Return BitConverter.ToUInt16(b, 0)
    End Function
    Function ReadUInt32(ByRef Str As IO.Stream) As UInt32
        Dim b(3) As Byte
        Str.Read(b, 0, b.Length)
        Return BitConverter.ToUInt32(b, 0)
    End Function
    Function ReadUInt32(ByRef Str As IO.Stream, ByVal ByteAddress As Int64) As UInt32
        Str.Position = ByteAddress
        Dim b(3) As Byte
        Str.Read(b, 0, b.Length)
        Return BitConverter.ToUInt32(b, 0)
    End Function
    Function ReadUInt64(ByRef Str As IO.Stream) As UInt64
        Dim b(7) As Byte
        Str.Read(b, 0, b.Length)
        Return BitConverter.ToUInt64(b, 0)
    End Function

    Function ReadInt16(ByRef Str As IO.Stream) As Int16
        Dim b(1) As Byte
        Str.Read(b, 0, b.Length)
        Return BitConverter.ToInt16(b, 0)
    End Function
    Function ReadInt32(ByRef Str As IO.Stream) As Int32
        Dim b(3) As Byte
        Str.Read(b, 0, b.Length)
        Return BitConverter.ToInt32(b, 0)
    End Function
    Function ReadInt64(ByRef Str As IO.Stream) As Int64
        Dim b(7) As Byte
        Str.Read(b, 0, b.Length)
        Return BitConverter.ToInt64(b, 0)
    End Function

    Function ReadZeroTerminatedString(ByRef Str As IO.Stream, ByVal BlockLength As Integer) As String
        Dim b1 As Byte = 255
        Dim string1 As String = ""
        Dim i As Integer = 0
        Do Until b1 = 0 Or i = BlockLength + 1
            b1 = Str.ReadByte()
            If b1 <> 0 Then
                string1 += Chr(b1)
            End If
            i += 1
        Loop
        If i < BlockLength Then
            Dim r As Integer = BlockLength - i
            Str.Position += r
        End If
        Return string1
    End Function

    Function ReadCount(ByVal Count As Integer, ByRef Str1 As IO.Stream) As Byte()
        Dim b(Count - 1) As Byte
        Str1.Read(b, 0, b.Length)
        Return b
    End Function

#Region "Block Based"
    Function ReadBlock(ByVal BlockSize As Int64, ByVal BlockNum As Int64, ByVal Str1 As IO.Stream) As Byte()
        Str1.Position = BlockNum * BlockSize
        Dim b(BlockSize - 1) As Byte
        Str1.Read(b, 0, BlockSize)
        Return b
    End Function
    Function ReadBlock(ByVal BlockSize As Int64, ByVal BlockNum As Int64, ByVal Str1 As IO.Stream, ByVal inode1 As Ext2FS.Inode) As Byte()
        Str1.Position = BlockNum * BlockSize
        Dim b(BlockSize - 1) As Byte
        Str1.Read(b, 0, BlockSize)
        Return b
    End Function
    Function BlockToByteAddress(ByVal BlockSize As Int64, ByVal BlockAddress As Int64) As UInt64
        Return BlockSize * BlockAddress
    End Function
#End Region
End Module
