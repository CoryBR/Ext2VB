Imports System.Runtime.InteropServices
Public Class Ext2FS
    Friend devpath As String
    Dim FS_Stream As IO.Stream
    Public Superblock As Ext2Superblock
    Dim GroupDescriptors As GroupDescriptor()
    Dim FS_Path As String
    Dim AccessMode As IO.FileAccess
    Dim ShareMode As IO.FileShare
    Public FS_ERR As Errors = Errors.None
    'Sub New(ByVal Stream1 As IO.Stream)
    '    FS_Stream = Stream1
    '    ReadSuperblock()
    '    ReadGroupDescriptors()
    '    Dim inode1 As New Inode
    '    'Dim err As Errors = GetPathInode("/Digimon Frontier - Fire!! (Opening Theme) [Kouji Wada].mp3", inode1)
    '    'ReadBlockBitmap(2)
    '    'Dim inode1 As Inode = GetInodeByNum(12)
    '    'Dim rec As UInt32
    '    'ReadFileByBytes(inode1)
    '    'Dim inodeTable As Inode() = ReadInodeTable(0, ReadInodeBitmap(0))
    '    'ListDirectory(inodeTable(1), FS_Stream)
    '    'ReadFileBlock(inodeTable(12), 66559, FS_Stream)
    '    'ReadFileExperimental(inodeTable(11), "C:\Documents and Settings\_\Desktop\ext2test.mp3")
    'End Sub

    Sub New(ByVal FSPath As String, ByVal AccessMode As IO.FileAccess, ByVal ShareMode As IO.FileShare, ByVal Parse As Boolean)
        FS_Path = FSPath
        Me.AccessMode = AccessMode
        Me.ShareMode = ShareMode
        FS_Stream = New IO.FileStream(FS_Path, IO.FileMode.Open, AccessMode, ShareMode)
        Dim sBlockErr As Errors = ReadSuperblock()
        If sBlockErr <> Errors.None Then
            FS_ERR = Errors.NotValidFS
        End If
        If Parse = False Then
            Console.WriteLine(FS_ERR.ToString)
            ReadGroupDescriptors()
        ElseIf Parse = True Then
            FS_Stream.Close()
        End If
        'ReadBlockBitmap(2)
        'Dim inode1 As Inode = GetInodeByNum(12)
        'Dim rec As UInt32
        'ReadFileByBytes(inode1)
        'Dim inodeTable As Inode() = ReadInodeTable(0, ReadInodeBitmap(0))
        'ListDirectory(inodeTable(1), FS_Stream)
        'ReadFileBlock(inodeTable(12), 66559, FS_Stream)
        'ReadFileExperimental(inodeTable(11), "C:\Documents and Settings\_\Desktop\ext2test.mp3")
    End Sub

    Shared Function ParseFS(ByVal FSPath As String, ByRef Name As String) As Errors
        'Dim FS_Stream As New IO.FileStream(FSPath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.ReadWrite)
        'FS_Stream.Position = 56 + 1024
        'Dim s_magic As UInt16 = StreamModules.ReadUInt16(FS_Stream)
        'Try
        Dim ext2 As New Ext2FS(FSPath, IO.FileAccess.Read, IO.FileShare.ReadWrite, True)

        If ext2.Superblock.s_magic = 61267 Then
            Name = ext2.Superblock.s_volume_name
            Return Errors.FS_Recongized
        Else
            Return Errors.NotValidFS
        End If
        'Catch ex As IO.IOException
        Return Errors.IO_ERR
        'Catch
        Return Errors.NotValidFS
        'End Try
    End Function

    Function GetPathInode(ByVal Path As String, ByRef Inode1 As Inode) As Errors
        If Path = "/" Then
            Inode1 = GetInodeByNum(2)
            Return Errors.None
        End If

        'Console.WriteLine("Path: " & Path)
        Dim splitPath2 As String() = Path.Split("/")
        Dim splitPath(splitPath2.Length - 2) As String
        Array.ConstrainedCopy(splitPath2, 1, splitPath, 0, splitPath2.Length - 1)
        Dim curDir As DirectoryEntry() = ReadRootDir()
        Dim curDirInode As Inode
        'Dim s As String
        Dim i As Int32 = 0
        Dim curDirPos As Int32 = 0


        curDirInode = GetInodeByNum(2)
        'curDir = ListDirectory(2)
        Dim EndInode As UInt32
        i = 0
        Dim endFound As Boolean = False
        Do Until i = splitPath.Length
            endFound = False
            Dim d As DirectoryEntry
            curDirPos = 0
            For Each d In curDir
                If d.name = splitPath(i) Then
                    If d.file_type = EXT2_FS.EXT2_FT_DIR Then
                        curDirPos = 0
                        If (i + 1) <> splitPath.Length Then
                            curDirInode = GetInodeByNum(d.fileInodeNum)
                            curDir = ListDirectory(d.fileInodeNum)
                        End If
                        EndInode = d.fileInodeNum
                    Else
                        EndInode = d.fileInodeNum
                    End If
                    'Console.WriteLine("End Found")
                    endFound = True
                    Exit For
                End If
                curDirPos += 1
            Next
            i += 1
        Loop
        'Console.WriteLine("Loop Done")
        If endFound = False Then
            'curDirPos = 0
            Return Errors.NotFound
        End If
        Try
            'Console.WriteLine("GetInodeByNum: " & curDir(curDirPos).fileInodeNum)
            Inode1 = GetInodeByNum(EndInode) 'curDir(curDirPos).fileInodeNum)
            'Console.WriteLine("Found")
            Return Errors.None
        Catch ex As Exception
            Debugger.Log(0, "GetPathInode", "Path: " & Path)
            Debugger.Log(0, "GetPathInode", ex.Message)
            Debugger.Log(0, "GetPathInode", ex.InnerException.ToString)
            Debugger.Log(0, "GetPathInode", ex.TargetSite.Name)
            Debugger.Log(0, "GetPathInode", ex.StackTrace)
            Debugger.Log(0, "GetPathInode", "Not Found")
            'Console.WriteLine("Not Found")
            Inode1 = curDirInode
            Return Errors.NotFound
        End Try
        'End If
    End Function

    Function ReadRootDir() As DirectoryEntry()
        Dim inode1 As Inode = GetInodeByNum(2)
        Return ListDirectory(inode1)
        'Dim stream1 As IO.Stream = New IO.FileStream(FS_Path, IO.FileMode.Open, AccessMode, ShareMode)
        'Dim inodeTable As Inode() = ReadInodeTable(0, ReadInodeBitmap(0))
        'stream1.Close()
        'Return ListDirectory(inodeTable(1))
    End Function

#Region "Read Test Code"
    Sub ReadFileByBytes(ByVal inode1 As Inode, ByVal SaveFileName As String)
        'Dim readBytes(Superblock.internal_block_size - 1) As Byte
        Dim readBytes((Superblock.internal_block_size * 64) - 1) As Byte
        Dim f As New IO.FileStream(SaveFileName, IO.FileMode.Create)
        Dim i As Long = 0
        Dim rec As Int32
        Do Until i = inode1.i_size
            rec = ReadFileBytes(inode1, i, readBytes)
            f.Write(readBytes, 0, rec)
            i += rec
        Loop
        f.Close()
        MsgBox("Done")
    End Sub

    Sub ReadFileExperimental(ByVal inode1 As Inode, ByVal SaveFileName As String)
        Dim i As Integer = 0
        Dim f As New IO.FileStream(SaveFileName, IO.FileMode.Create)
        Dim b() As Byte
        Do Until i = inode1.i_size / Superblock.internal_block_size
            b = ReadFileBlock(inode1, i)
            f.Write(b, 0, b.Length)
            i += 1
        Loop
        f.Close()
        MsgBox("Done")
    End Sub

    Function ReadAllInodeBlocks(ByVal inode1 As Inode) As Byte()
        Dim i As Integer = 0
        Dim inodeBlocks As New IO.MemoryStream
        Dim b() As Byte
        Do Until i = inode1.i_size / Superblock.internal_block_size
            b = ReadFileBlock(inode1, i)
            inodeBlocks.Write(b, 0, b.Length)
            i += 1
        Loop
        Return inodeBlocks.ToArray
    End Function
#End Region
    Function CalculateNumberOfBlockGroups(ByVal s_blocks_count As UInt32, ByVal s_blocks_per_group As UInt32) As UInt32
        Dim NumberOfBlockGroups As Double
        'Console.WriteLine(s_blocks_count)
        'Console.WriteLine(s_blocks_per_group)
        Console.Write(s_blocks_count)
        Console.Write(s_blocks_per_group)
        NumberOfBlockGroups = s_blocks_count / s_blocks_per_group
        'Console.WriteLine(NumberOfBlockGroups)
        Return RoundUp(NumberOfBlockGroups)
    End Function

    Private Function RoundUp(ByVal Num As Double) As UInt32
        Dim int1 As UInt32 = Num
        If Num - int1 > 0 Then
            Return int1 + 1
        Else
            Return Num
        End If
    End Function

    Private Function RoundDown(ByVal Num As Double) As UInt32
        Dim int1 As UInt32 = Int(Num)
        'If Num - int1 > 0 Then
        'Return int1 + 1
        'Else
        Return int1
        'End If
    End Function

    Private Function ReadSuperblock() As Errors
        'Superblock always starts 1024 bytes in
        FS_Stream.Position = 1024

        Superblock = New Ext2Superblock
        Superblock.s_inodes_count = StreamModules.ReadUInt32(FS_Stream)
        Superblock.s_blocks_count = StreamModules.ReadUInt32(FS_Stream)
        Superblock.s_r_blocks_count = StreamModules.ReadUInt32(FS_Stream)
        Superblock.s_free_blocks_count = StreamModules.ReadUInt32(FS_Stream)
        Superblock.s_free_inodes_count = StreamModules.ReadUInt32(FS_Stream)
        Superblock.s_first_data_block = StreamModules.ReadUInt32(FS_Stream)
        Superblock.s_log_block_size = StreamModules.ReadUInt32(FS_Stream)

        Superblock.s_log_frag_size = StreamModules.ReadUInt32(FS_Stream)

        Superblock.s_blocks_per_group = StreamModules.ReadUInt32(FS_Stream)
        Superblock.s_frags_per_group = StreamModules.ReadUInt32(FS_Stream)
        Superblock.s_inodes_per_group = StreamModules.ReadUInt32(FS_Stream)
        Superblock.s_mtime = StreamModules.ReadUInt32(FS_Stream)
        Superblock.s_wtime = StreamModules.ReadUInt32(FS_Stream)

        Superblock.s_mnt_count = StreamModules.ReadUInt16(FS_Stream)
        Superblock.s_max_mnt_count = StreamModules.ReadUInt16(FS_Stream)
        Superblock.s_magic = StreamModules.ReadUInt16(FS_Stream)
        If Superblock.s_magic <> 61267 Then
            Return Errors.NotValidFS
        End If
        Superblock.s_state = StreamModules.ReadUInt16(FS_Stream)
        Superblock.s_errors = StreamModules.ReadUInt16(FS_Stream)
        Superblock.s_minor_rev_level = StreamModules.ReadUInt16(FS_Stream)

        Superblock.s_lastcheck = StreamModules.ReadUInt32(FS_Stream)
        Superblock.s_checkinterval = StreamModules.ReadUInt32(FS_Stream)
        Superblock.s_creator_os = StreamModules.ReadUInt32(FS_Stream)
        Superblock.s_rev_level = StreamModules.ReadUInt32(FS_Stream)

        Superblock.s_def_resuid = StreamModules.ReadUInt16(FS_Stream)
        Superblock.s_def_resgid = StreamModules.ReadUInt16(FS_Stream)

        If Superblock.s_rev_level = EXT2_REVISIONS.EXT2_DYNAMIC_REV Then
            Superblock.s_first_ino = StreamModules.ReadUInt32(FS_Stream)
            Superblock.s_inode_size = StreamModules.ReadUInt16(FS_Stream)
            Superblock.s_block_group_nr = StreamModules.ReadUInt16(FS_Stream)
            Superblock.s_feature_compat = StreamModules.ReadUInt32(FS_Stream)
            Superblock.s_feature_incompat = StreamModules.ReadUInt32(FS_Stream)
            Superblock.s_feature_ro_compat = StreamModules.ReadUInt32(FS_Stream)
            Superblock.s_uuid = New Guid(StreamModules.ReadCount(16, FS_Stream))
            Superblock.s_volume_name = StreamModules.ReadZeroTerminatedString(FS_Stream, 16)
            Superblock.s_last_mounted = StreamModules.ReadZeroTerminatedString(FS_Stream, 64)
            Superblock.s_algo_bitmap = StreamModules.ReadUInt32(FS_Stream)
        End If

        Superblock.internal_block_size = GetBlockSize()
        'FS_Stream.Position = Superblock.internal_block_size
    End Function

    Private Function GetBlockNumPos(ByVal BlockNum As UInt32) As Int64
        Return BlockNum * Superblock.internal_block_size
    End Function

    Private Sub ReadGroupDescriptors()
        'Console.WriteLine("ReadGroupDescriptors")
        'Dim stream1 As IO.FileStream = New IO.FileStream(FS_Path, IO.FileMode.Open, AccessMode, ShareMode)
        'Console.WriteLine("Pos: " & FS_Stream.Position)
        If Superblock.internal_block_size > 1024 Then
            FS_Stream.Position = GetBlockNumPos(1) ' - 1 '2048 'GetBlockNumPos(1)
        Else
            FS_Stream.Position = GetBlockNumPos(2)
        End If
        'Console.WriteLine(Superblock.internal_block_size)
        'stream1.Position = Superblock.internal_block_size * 2
        'Console.WriteLine("Superblock.s_blocks_count: " & Superblock.s_blocks_count)
        'Console.WriteLine("Superblock.s_blocks_per_group: " & Superblock.s_blocks_per_group)
        Dim count As Integer = CalculateNumberOfBlockGroups(Superblock.s_blocks_count, Superblock.s_blocks_per_group)
        ReDim GroupDescriptors(count - 1)
        Dim i As Integer = 0
        Do Until i = count
            GroupDescriptors(i).bg_block_bitmap = StreamModules.ReadUInt32(FS_Stream)
            GroupDescriptors(i).bg_inode_bitmap = StreamModules.ReadUInt32(FS_Stream)
            GroupDescriptors(i).bg_inode_table = StreamModules.ReadUInt32(FS_Stream)
            GroupDescriptors(i).bg_free_blocks_count = StreamModules.ReadUInt16(FS_Stream)
            GroupDescriptors(i).bg_free_inodes_count = StreamModules.ReadUInt16(FS_Stream)
            GroupDescriptors(i).bg_used_dirs_count = StreamModules.ReadUInt16(FS_Stream)
            GroupDescriptors(i).bg_pad = StreamModules.ReadUInt16(FS_Stream)
            GroupDescriptors(i).bg_reserved1 = StreamModules.ReadUInt32(FS_Stream)
            GroupDescriptors(i).bg_reserved2 = StreamModules.ReadUInt32(FS_Stream)
            GroupDescriptors(i).bg_reserved3 = StreamModules.ReadUInt32(FS_Stream)
            i += 1
        Loop
    End Sub

    Private Function ReadBlockBitmap(ByVal GroupDescriptorNum As Integer) As BitArray
        Dim b() As Byte = StreamModules.ReadBlock(GetBlockSize, GroupDescriptors(GroupDescriptorNum).bg_block_bitmap, FS_Stream)
        Dim bitmap As New BitArray(b)
        'Dim b1 As Boolean
        'Dim i As Integer = 0
        'For Each b1 In bitmap
        '    If b1 = True Then
        '        'Debugger.Break()
        '    End If
        '    i += 1
        'Next
        Return bitmap
    End Function

    Private Function ReadInodeBitmap(ByVal GroupDescriptorNum As Integer) As BitArray
        FS_Stream.Position = ResolveByteAddressFromBlock(GroupDescriptors(GroupDescriptorNum).bg_inode_bitmap)
        Dim inodeBitmapBlockCount = GetInodeBitmapBlockCount()
        Dim b((Superblock.s_inodes_per_group / 8) - 1) As Byte
        FS_Stream.Read(b, 0, b.Length)
        Dim bitmap As New BitArray(b)
        Dim b1 As Boolean
        Dim i As Integer = 0
        Dim iCount As Integer = 0
        For Each b1 In bitmap
            If b1 = True Then
                iCount += 1
            End If
            i += 1
        Next
        Return bitmap
    End Function

    Private Function ReadInodeTable(ByVal GroupDescriptorNum As Integer, ByVal InodeBitmap As BitArray) As Inode()
        'Dim FS_StreamCP As IO.Stream = FS_Stream
        'FS_StreamCP.Position = GroupDescriptors(GroupDescriptorNum).bg_inode_table
        Dim stream1 As IO.FileStream = New IO.FileStream(FS_Path, IO.FileMode.Open, AccessMode, ShareMode)
        Dim i As Integer = 0
        Dim bool As Boolean
        For Each bool In InodeBitmap
            If bool = True Then
                i += 1
            End If
        Next
        Dim inodes(i) As Inode
        Dim i2 As Integer = 0
        i = 0
        For Each bool In InodeBitmap
            If bool = True Then
                'If i = 11 Then
                'Debugger.Break()
                'End If
                inodes(i) = ReadInode((GroupDescriptors(GroupDescriptorNum).bg_inode_table * Superblock.internal_block_size + i2 * Superblock.s_inode_size), stream1)
                i += 1
            End If
            i2 += 1
        Next
        stream1.Close()
        Return inodes
    End Function

    Private Function ReadInode(ByVal ByteAddress As Long, ByVal str1 As IO.Stream) As Inode
        str1.Position = ByteAddress
        Dim inode1 As New Inode
        inode1.i_mode = StreamModules.ReadUInt16(str1)
        inode1.i_uid = StreamModules.ReadUInt16(str1)
        inode1.i_size = StreamModules.ReadUInt32(str1)
        inode1.i_atime = StreamModules.ReadUInt32(str1)
        inode1.i_ctime = StreamModules.ReadUInt32(str1)
        inode1.i_mtime = StreamModules.ReadUInt32(str1)
        inode1.i_dtime = StreamModules.ReadUInt32(str1)
        inode1.i_gid = StreamModules.ReadUInt16(str1)
        inode1.i_links_count = StreamModules.ReadUInt16(str1)
        inode1.i_blocks = StreamModules.ReadUInt32(str1)
        inode1.i_flags = StreamModules.ReadUInt32(str1)
        inode1.i_osd1 = StreamModules.ReadUInt32(str1)

        Dim i As Integer = 0
        ReDim inode1.i_block.direct_block(11)
        Do Until i = 12
            inode1.i_block.direct_block(i) = StreamModules.ReadUInt32(str1)
            i += 1
        Loop
        inode1.i_block.indirect_block = StreamModules.ReadUInt32(str1)
        inode1.i_block.bi_indirect_block = StreamModules.ReadUInt32(str1)
        inode1.i_block.tri_indirect_block = StreamModules.ReadUInt32(str1)

        inode1.i_version = StreamModules.ReadUInt32(str1)
        inode1.i_file_acl = StreamModules.ReadUInt32(str1)
        inode1.i_dir_acl = StreamModules.ReadUInt32(str1)
        inode1.i_faddr = StreamModules.ReadUInt32(str1)
        str1.Position += 12
        Return inode1
    End Function

    Private Function GetBlockBitmapBlockCount() As UInt32
        Dim i As Integer = RoundUp(RoundUp(Superblock.s_blocks_per_group / 8) / Superblock.internal_block_size)
        Return i
    End Function

    Private Function GetInodeBitmapBlockCount() As UInt32
        Dim i As Integer = RoundUp(RoundUp(Superblock.s_inodes_per_group / 8) / Superblock.internal_block_size)
        Return i
    End Function

    Private Function ResolveByteAddressFromBlock(ByVal BlockAddress As UInt32) As UInt32
        Return BlockAddress * Superblock.internal_block_size
    End Function

    Private Function GetBlockSize() As Int32
        Dim i As Int32
        If Superblock.s_log_block_size > 0 Then
            i = 1024 << Superblock.s_log_block_size
        Else
            i = 1024 >> -Superblock.s_log_block_size
        End If
        Return i
    End Function

    Function ReadFileBytes(ByVal inode1 As Inode, ByVal Offset As Long, ByRef ReadBytes() As Byte) As Int32
        Dim stream1 As IO.FileStream = New IO.FileStream(FS_Path, IO.FileMode.Open, AccessMode, ShareMode)
        Dim ReadEnd As UInt32
        Dim remaining As UInt32
        If ReadBytes.Length > inode1.i_size - Offset Then
            ReadEnd = inode1.i_size
            remaining = inode1.i_size - Offset
        Else
            ReadEnd = Offset + ReadBytes.Length '- 1 'ReadLength
            remaining = ReadBytes.Length
        End If

        ' = ReadBytes.Length  'ReadEnd 'ReadLength
        Dim i As Long = Offset
        Dim read As UInt32 = 0
        Dim blockNum As UInt32
        'ReDim ReadBytes(ReadLength - 1)
        Dim BlockStartPos As UInt32 = 0

        If i > 0 Then
            BlockStartPos = Superblock.internal_block_size - (RoundUp(i / Superblock.internal_block_size) - (i / Superblock.internal_block_size)) * Superblock.internal_block_size
        End If


        Dim outputPos As UInt32 = 0
        Do Until (i = ReadEnd) Or (remaining = 0)
            blockNum = Int(i / Superblock.internal_block_size)
            Dim b() As Byte = ReadFileBlock(inode1, blockNum)

            If remaining >= b.Length Then
                remaining -= b.Length
                read = b.Length
            ElseIf remaining < b.Length Then
                read = remaining
                remaining = 0
            End If
            If BlockStartPos > 0 Then
                read -= BlockStartPos
                remaining += BlockStartPos
            End If
            Array.ConstrainedCopy(b, BlockStartPos, ReadBytes, outputPos, read)
            outputPos += read

            BlockStartPos = 0
            i += read
        Loop
        'ReadLength = outputPos
        stream1.Close()
        Return outputPos
    End Function

    Function GetInodeByNum(ByVal inodeNum As UInt32) As Inode
        'Console.WriteLine("GetInodeByNum")
        Dim stream1 As IO.Stream = New IO.FileStream(FS_Path, IO.FileMode.Open, AccessMode, ShareMode)
        Dim group As UInt32
        'If inodeNum < Superblock.s_inodes_per_group Then
        'group = 0
        'Else
        group = RoundDown(((inodeNum - 1) / Superblock.s_inodes_per_group))
        'End If
        'Debugger.Break()
        'Console.WriteLine("s_inodes_per_group: " & Superblock.s_inodes_per_group)
        'Console.WriteLine("Group = " & group)

        'Console.WriteLine("inodeNum: " & inodeNum)

        Dim relativeInodeNum As Int64 = (inodeNum - 1)
        relativeInodeNum -= (group * Superblock.s_inodes_per_group)
        If relativeInodeNum < 0 Then
            'Debugger.Break()
            Debugger.Log(0, "GetInodeByNum", "relativeInodeNum < 0, is " & relativeInodeNum)
            group -= 1
            relativeInodeNum = (inodeNum - 1)
            relativeInodeNum -= (group * Superblock.s_inodes_per_group)
        End If
        Dim inodeTableAddress As UInt32 = GroupDescriptors(group).bg_inode_table
        'Console.WriteLine("inodeTableAddress: " & inodeTableAddress)
        'Console.WriteLine("relativeInodeNum: " & relativeInodeNum)
        'Console.WriteLine("relativeInodeNum: " & relativeInodeNum)
        Dim s As Int64 = inodeTableAddress
        s *= Superblock.internal_block_size
        'Console.WriteLine("s * block_size: " & s)
        s += (relativeInodeNum * Superblock.s_inode_size)
        'Console.WriteLine("s:" & s)
        'Try
        'Console.WriteLine("inodeTableAddress: " & inodeTableAddress)
        'Console.WriteLine("internal_block_size: " & Superblock.internal_block_size)
        's = (inodeTableAddress * Superblock.internal_block_size)
        'Console.WriteLine("S1: " & s)
        's += (relativeInodeNum * Superblock.s_inode_size)
        'Console.WriteLine("S2: " & s)
        'Catch ex As Exception
        '    Console.WriteLine(ex.Message)
        '    Console.WriteLine(ex.InnerException)
        '    Console.WriteLine(ex.TargetSite)
        '    Throw ex
        'End Try
        'Console.WriteLine("s: " & s)
        Dim inode1 As Inode = ReadInode(s, stream1)
        inode1.inode_num = inodeNum
        'Console.WriteLine("Inode Num: " & inode1.inode_num)
        'Console.WriteLine("Inode Direct Block 1: " & inode1.i_block.direct_block(0))
        'Console.WriteLine("Inode Size: " & inode1.i_size)

        stream1.Close()
        Return inode1
    End Function


#Region "Structures"
    ''' <summary>
    ''' The superblock is the structure on an ext2 disk containing the very basic information about the file system properties. It is always 1024 bytes long.
    ''' </summary>
    ''' <remarks></remarks>
    <StructLayout(LayoutKind.Sequential)> _
    Structure Ext2Superblock
        ''' <summary>
        ''' Count of inodes in the filesystem
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_inodes_count As UInt32

        ''' <summary>
        ''' Count of blocks in the filesystem
        ''' </summary>
        ''' <remarks></remarks>
        ''' 
        Dim s_blocks_count As UInt32

        ''' <summary>
        ''' The number of blocks which are reserved for the super user
        ''' </summary>
        ''' <remarks></remarks>
        ''' 
        Dim s_r_blocks_count As UInt32

        ''' <summary>
        ''' Count of blocks in the filesystem
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_free_blocks_count As UInt32

        ''' <summary>
        ''' Count of the number of free inodes
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_free_inodes_count As UInt32

        ''' <summary>
        ''' Count of the number of free blocks
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_first_data_block As UInt32

        ''' <summary>
        ''' Indicator of the block size
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_log_block_size As UInt32

        ''' <summary>
        ''' Indicator of the size of the fragments
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_log_frag_size As UInt32

        ''' <summary>
        ''' Count of the number of blocks in each block group
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_blocks_per_group As UInt32

        ''' <summary>
        ''' Count of the number of fragments in each block group
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_frags_per_group As UInt32

        ''' <summary>
        ''' Count of the number of inodes in each block group
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_inodes_per_group As UInt32

        ''' <summary>
        ''' The time that the filesystem was last mounted
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_mtime As UInt32

        ''' <summary>
        ''' The time that the filesystem was last written to
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_wtime As UInt32

        ''' <summary>
        ''' The number of times the file system has been mounted
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_mnt_count As UInt16

        ''' <summary>
        ''' The number of times the file system can be mounted
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_max_mnt_count As Int16

        ''' <summary>
        ''' Magic number indicating ext2fs. Should be 0xEF53
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_magic As UInt16

        ''' <summary>
        ''' Flags indicating the current state of the filesystem
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_state As UInt16

        ''' <summary>
        ''' Flags indicating the procedures for error reporting
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_errors As EXT2_ERRORS

        ''' <summary>
        ''' 16bit value identifying the minor revision level within its revision level.
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_minor_rev_level As UInt16

        ''' <summary>
        ''' The time that the filesystem was last checked
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_lastcheck As UInt32

        ''' <summary>
        ''' The maximum time permissible between checks
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_checkinterval As UInt32

        ''' <summary>
        ''' Indicator of which OS created the filesystem
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_creator_os As EXT2_OS

        ''' <summary>
        ''' The revision level of the filesystem
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_rev_level As EXT2_REVISIONS

        ''' <summary>
        ''' The default user id for reserved blocks
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_def_resuid As UInt16

        ''' <summary>
        ''' The default group id for reserved blocks
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_def_resgid As UInt16

        'EXT2_DYNAMIC_REV Specific
        ''' <summary>
        ''' Used as index to the first inode useable for standard files. Assumed to be 11 in non-dynamic.
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_first_ino As UInt32

        ''' <summary>
        ''' Indicates the size of the inode structure. Assumed to be 128 in non-dynamic revisions.
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_inode_size As UInt16

        ''' <summary>
        ''' Indicates the block group number hosting this superblock structure.
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_block_group_nr As UInt16

        ''' <summary>
        ''' 32bit bitmask of compatible features. The file system implementation is free to support them or not without risk of damaging the meta-data.
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_feature_compat As UInt32

        ''' <summary>
        ''' 32bit bitmask of incompatible features. The file system implementation should refuse to mount the file system if any of the indicated feature is unsupported.
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_feature_incompat As UInt32

        ''' <summary>
        ''' 32bit bitmask of "read-only" features. The file system implementation should mount as read-only if any of the indicated feature is unsupported.
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_feature_ro_compat As UInt32

        ''' <summary>
        ''' 128bit value used as the volume id. This should, as much as possible, be unique for each file system formatted.
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_uuid As Guid

        ''' <summary>
        ''' 16 bytes volume name, mostly unusued. A valid volume name would consist of only ISO-Latin-1 characters and be 0 terminated.
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_volume_name As String

        ''' <summary>
        ''' 64 bytes directory path where the file system was last mounted.
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_last_mounted As String

        ''' <summary>
        ''' 32bit value used by compression algorithms to determine the methods used.
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_algo_bitmap As UInt32


        ''' <summary>
        ''' Padding to 1024 bytes
        ''' </summary>
        ''' <remarks></remarks>
        Dim s_reserved As UInt32

        ''' <summary>
        ''' Block size (Not stored in superblock)
        ''' </summary>
        ''' <remarks></remarks>
        Dim internal_block_size As UInt32
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Structure GroupDescriptor
        ''' <summary>
        ''' The address of the block containing the block bitmap for this group
        ''' </summary>
        ''' <remarks></remarks>
        Dim bg_block_bitmap As UInt32

        ''' <summary>
        ''' The address of the block containing the inode bitmap for this group
        ''' </summary>
        ''' <remarks></remarks>
        Dim bg_inode_bitmap As UInt32

        ''' <summary>
        ''' The address of the block containing the inode table for this group
        ''' </summary>
        ''' <remarks></remarks>
        Dim bg_inode_table As UInt32

        ''' <summary>
        ''' The count of free blocks in this group
        ''' </summary>
        ''' <remarks></remarks>
        Dim bg_free_blocks_count As UInt16

        ''' <summary>
        ''' The count of free inodes in this group
        ''' </summary>
        ''' <remarks></remarks>
        Dim bg_free_inodes_count As UInt16

        ''' <summary>
        ''' The number inodes in this group which are directories
        ''' </summary>
        ''' <remarks></remarks>
        Dim bg_used_dirs_count As UInt16

        Dim bg_pad As UInt16
        Dim bg_reserved1 As UInt32
        Dim bg_reserved2 As UInt32
        Dim bg_reserved3 As UInt32
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Structure Inode
        Dim inode_num As UInt32
        Dim i_mode As UInt16
        Dim i_uid As UInt16
        Dim i_size As UInt32
        Dim i_atime As UInt32
        Dim i_ctime As UInt32
        Dim i_mtime As UInt32
        Dim i_dtime As UInt32
        Dim i_gid As UInt16
        Dim i_links_count As UInt16
        Dim i_blocks As UInt32
        Dim i_flags As UInt32
        Dim i_osd1 As UInt32
        Dim i_block As InodeBlockList
        Dim i_version As UInt32
        Dim i_file_acl As UInt32
        ''' <summary>
        ''' Directory ACL. Also used as second half of file size when using LFS.
        ''' </summary>
        ''' <remarks></remarks>
        Dim i_dir_acl As UInt32
        Dim i_faddr As UInt32
        Dim i_pad1 As UInt16
        Dim i_osd2() As UInt32
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Structure InodeBlockList
        Dim direct_block As UInt32()
        Dim indirect_block As UInt32
        Dim bi_indirect_block As UInt32
        Dim tri_indirect_block As UInt32
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Structure InodeFirstBlockList
        Dim address As UInt32
        Dim direct_block() As UInt32
        Dim indirect_block_address As UInt32
        Dim bi_indirect_block_address As UInt32
        Dim tri_indirect_block_address As UInt32
    End Structure

    Structure InodeDirectBlock
        Dim address As UInt32
        Dim addresses() As UInt32
    End Structure
    Structure InodeIndirectBlock
        Dim address As UInt32
        Dim direct_block_addresses() As UInt32
    End Structure
    Structure InodeBiIndirectBlock
        Dim address As UInt32
        Dim indirect_blocks() As InodeIndirectBlock
    End Structure
    Structure InodeTriIndirectBlock
        Dim address As UInt32
        Dim bi_indirect_blocks() As InodeBiIndirectBlock
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Structure DirectoryEntry
        Dim fileInodeNum As UInt32
        Dim rec_len As UInt16
        Dim name_len As Byte
        Dim file_type As EXT2_FS
        Dim name As String
    End Structure
#End Region

#Region "Enums"
    ''' <summary>
    ''' List of filesystem creator OSes
    ''' </summary>
    ''' <remarks></remarks>
    Enum EXT2_OS As UInteger
        EXT2_OS_LINUX = 0
        EXT2_OS_HURD = 1
        EXT2_OS_MASIX = 2
        EXT2_OS_FREEBSD = 3
        EXT2_OS_LITES4 = 4
    End Enum

    Enum EXT2_ERRORS As UShort
        EXT2_ERRORS_CONTINUE = 1
        EXT2_ERRORS_RO = 2
        EXT2_ERRORS_PANIC = 3
        EXT2_ERRORS_DEFAULT = 1
    End Enum

    Enum EXT2_REVISIONS As UInteger
        EXT2_GOOD_OLD_REV = 0
        EXT2_DYNAMIC_REV = 1
    End Enum

    Enum EXT2_INODE_FILE_MODE As UShort
        S_IFMT = &HF000
        S_IFSOCK = &HA000
        S_IFLKN = &HC000
        S_IFREG = &H8000
        S_IFBLK = &H6000
        S_IFDIR = &H4000
        S_IFCHR = &H2000
        S_IFIFO = &H1000

        S_ISUID = &H800
        S_ISGID = &H400
        S_ISVTX = &H200

        S_IRWXU = &H1C0
        S_IRUSR = &H100
        S_IWUSR = &H80
        S_IXUSR = &H40

        S_IRWXG = &H38
        S_IRGRP = &H20
        S_IWGRP = &H10
        S_IXGRP = &H8

        S_IRWXO = &H7
        S_IROTH = &H4
        S_IWOTH = &H2
        S_IXOTH = &H1
    End Enum

    Enum EXT2_FS As Byte
        EXT2_FT_UNKNOWN = 0
        EXT2_FT_REG_FILE = 1
        EXT2_FT_DIR = 2
        EXT2_FT_CHRDEV = 3
        EXT2_FT_BLKDEV = 4
        EXT2_FT_FIFO = 5
        EXT2_FT_SOCK = 6
        EXT2_FT_SYMLINK = 7
        EXT2_FT_MAX = 8
    End Enum

    Enum Errors
        None = 0
        NotFound = 1
        IO_ERR = 2
        FS_Recongized = -1
        NotValidFS = -2
    End Enum
#End Region
#Region "File Blocks"
    Private Function CheckBlockLength(ByVal inode1 As Inode, ByVal BlockNum As UInt32) As UInt32
        Dim maxSize As UInt64 = inode1.i_size
        If BlockNum = RoundUp(maxSize / Superblock.internal_block_size) Then
            Dim readLen As UInt32 = RoundUp(maxSize / Superblock.internal_block_size) - Int(maxSize / Superblock.internal_block_size)
            Return readLen
        Else
            Return Superblock.internal_block_size
        End If
    End Function
    Private Function ReadFileBlock(ByVal inode1 As Inode, ByVal BlockNum As UInt32) As Byte()
        Dim stream1 As IO.FileStream = New IO.FileStream(FS_Path, IO.FileMode.Open, AccessMode, ShareMode)
        Dim blockData() As Byte
        Dim blockAddr As UInt32
        'Console.WriteLine("BlockNum: " & BlockNum)
        Dim addressesPerBlock As UInt32 = Superblock.internal_block_size / 4

        Dim addressesPerBlockSnd As Long = addressesPerBlock * addressesPerBlock
        Dim addressesPerBlockThrd As Long = addressesPerBlock * addressesPerBlock * addressesPerBlock
        'Try
        If BlockNum <= 11 Then

            blockAddr = inode1.i_block.direct_block(BlockNum)
            'Return ReadBlock(GetBlockSize, inode1.i_block.direct_block(BlockNum), stream1)
        ElseIf BlockNum <= (addressesPerBlock - 1) + 12 Then
            'Console.WriteLine("BlockNum: " & BlockNum)
            blockAddr = ReadFromIndirectBlock(inode1.i_block.indirect_block, BlockNum - 12, stream1)
            'Dim DirectBlockAddr As UInt32 = ReadUInt32(stream1, blockAddr)
            'Return ReadBlock(GetBlockSize, blockAddr, stream1)
        ElseIf BlockNum <= (addressesPerBlockSnd + 12 + addressesPerBlock - 1) Then
            'Console.WriteLine("<=addresserperblock^2  BlockNum: " & BlockNum)
            Dim relativeBlockNum As UInt32 = (BlockNum - 12 - addressesPerBlock)
            'Console.WriteLine("RelativeBlockNum: " & relativeBlockNum)
            Dim indirectBlockNum As UInt32 = Int(relativeBlockNum / addressesPerBlock)
            'Console.WriteLine("IndirectBlockNum: " & indirectBlockNum)
            'Use ReadFromIndirectBlock to get the actual indirect block address, then read again to get the data address
            blockAddr = ReadFromBiIndirectBlock(inode1.i_block.bi_indirect_block, inode1, relativeBlockNum, addressesPerBlock)
            'Return ReadBlock(GetBlockSize, blockAddr, stream1)
        ElseIf BlockNum <= (addressesPerBlockThrd + 12 + addressesPerBlock - 1) Then
            'Console.WriteLine("<=addresserperblock^3  BlockNum: " & BlockNum)
            'Console.WriteLine("BlockNum: " & BlockNum)
            Dim relativeBlockNum As UInt32 = (BlockNum - 12 - addressesPerBlockSnd - addressesPerBlock)

            Dim relativeBlockNum2 As UInt32 = relativeBlockNum ' - 1
            Dim i As Integer = 0
            Do Until relativeBlockNum2 <= addressesPerBlockSnd
                relativeBlockNum2 -= addressesPerBlockSnd
                i += 1
            Loop

            Dim biIndirectBlockNum As UInt32 = i
            Dim BiIndirectBlockAddress As UInt32 = StreamModules.ReadUInt32(stream1, (inode1.i_block.tri_indirect_block * Superblock.internal_block_size + 4 * biIndirectBlockNum))
            blockAddr = ReadFromBiIndirectBlock(BiIndirectBlockAddress, inode1, relativeBlockNum, addressesPerBlock)
            'Return ReadBlock(GetBlockSize, blockAddr, stream1)
        End If
        blockData = ReadBlock(GetBlockSize, blockAddr, stream1)
        'Catch ex As Exception
        'Console.WriteLine(ex.Message)
        'Console.WriteLine(ex.InnerException)
        'Console.WriteLine(ex.TargetSite)
        'End Try
        'Dim blockDataLen As UInt32 = CheckBlockLength(inode1, BlockNum)
        'ReDim Preserve blockData(blockDataLen - 1)
        stream1.Close()
        Return blockData
    End Function

    Private Function ReadFromIndirectBlock(ByVal BlockAddress As UInt32, ByVal RelativeAddressNum As UInt32, ByVal stream1 As IO.Stream) As Int64
        'Dim stream1 As IO.FileStream = New IO.FileStream(FS_Path, IO.FileMode.Open, AccessMode, ShareMode)
        Dim addressPos As Int64 = BlockToByteAddress(GetBlockSize, BlockAddress) + RelativeAddressNum * 4
        'Console.WriteLine("addressPos: " & addressPos)
        'stream1.Close()
        'Console.WriteLine("blockAddress: " & BlockAddress)
        'Console.WriteLine("RelativeAddressNum: " & RelativeAddressNum)
        'Console.WriteLine("addressPos: " & addressPos)
        Return ReadUInt32(stream1, addressPos)
    End Function

    Private Function ReadFromBiIndirectBlock(ByVal BiIndirectBlockAddress As UInt32, ByVal inode1 As Inode, ByVal RelativeBlockNum As UInt32, ByVal addressesPerBlock As UInt32) As Int64
        Dim stream1 As IO.FileStream = New IO.FileStream(FS_Path, IO.FileMode.Open, AccessMode, ShareMode)
        'Try
        'Console.WriteLine("BiIndirectBlockAddress: " & BiIndirectBlockAddress)
        'Console.WriteLine("AddressesPerblock: " & addressesPerBlock)
        Dim relativeBlockNum2 As UInt32 = RelativeBlockNum ' - 1
        'Console.WriteLine("relativeBlockNum2: " & relativeBlockNum2)
        Dim i As Integer = 0
        Do Until relativeBlockNum2 <= addressesPerBlock - 1
            relativeBlockNum2 -= (addressesPerBlock)
            i += 1
        Loop
        'Console.WriteLine("i: " & i)
        'Console.WriteLine("relativeBlockNum2-2: " & relativeBlockNum2)
        'Console.WriteLine("BiIndirectBlockAddress: " & BiIndirectBlockAddress)
        'Dim indirectBlockAddr As UInt32 = ReadFromIndirectBlock(inode1.i_block.bi_indirect_block, i, stream1)
        Dim indirectBlockAddr As Int64 = ReadFromIndirectBlock(BiIndirectBlockAddress, i, stream1)
        'Console.WriteLine("indirectBlockAddr: " & indirectBlockAddr)
        Dim blockAddr As Int64 = ReadFromIndirectBlock(indirectBlockAddr, relativeBlockNum2, stream1)
        'Console.WriteLine("blockAddr: " & blockAddr)
        'If blockAddr >= 12490 Then
        '    Debugger.Break()
        'End If
        stream1.Close()
        Return blockAddr
        'Catch ex As Exception
        'Console.WriteLine("ReadFromIndirectBlockEX: " & ex.Message)
        'Console.WriteLine(ex.InnerException)
        'Console.WriteLine(ex.TargetSite)
        'End Try
    End Function
#End Region

    Function ListDirectory(ByVal inode1 As Inode) As DirectoryEntry()
        Dim stream1 As IO.Stream = New IO.FileStream(FS_Path, IO.FileMode.Open, AccessMode, ShareMode)
        Dim dir As New List(Of DirectoryEntry)
        Dim b() As Byte = ReadAllInodeBlocks(inode1)
        Dim i As UInt32 = 0
        Dim i2 As UInt32 = 0
        Do Until i2 >= b.Length
            i = i2
            Dim f As New DirectoryEntry
            f.fileInodeNum = BitConverter.ToUInt32(b, i)
            i += 4
            f.rec_len = BitConverter.ToUInt16(b, i)
            i += 2
            f.name_len = b(i)
            i += 1
            f.file_type = b(i)
            i += 1
            f.name = System.Text.ASCIIEncoding.ASCII.GetString(b, i, f.name_len)
            i += f.name_len
            dir.Add(f)
            i2 += f.rec_len
        Loop
        stream1.Close()
        Return dir.ToArray
    End Function

    Function ListDirectory(ByVal inodeNum As UInt32) As DirectoryEntry()
        'Console.WriteLine("ListDirectory Inode: " & inodeNum)
        'Dim stream1 As IO.Stream = New IO.FileStream(FS_Path, IO.FileMode.Open, AccessMode, ShareMode)
        'Dim dir As New List(Of DirectoryEntry)
        'Dim b() As Byte = ReadAllInodeBlocks(GetInodeByNum(inodeNum))
        Return ListDirectory(GetInodeByNum(inodeNum))
        'Dim i As UInt32 = 0
        'Dim i2 As UInt32 = 0
        'Do Until i2 >= b.Length
        '    i = i2
        '    Dim f As New DirectoryEntry
        '    f.fileInodeNum = BitConverter.ToUInt32(b, i)
        '    i += 4
        '    f.rec_len = BitConverter.ToUInt16(b, i)
        '    i += 2
        '    f.name_len = b(i)
        '    i += 1
        '    f.file_type = b(i)
        '    i += 1
        '    f.name = System.Text.ASCIIEncoding.ASCII.GetString(b, i, f.name_len)
        '    i += f.name_len
        '    dir.Add(f)
        '    i2 += f.rec_len
        'Loop
        'stream1.Close()
        'Return dir.ToArray
    End Function
End Class
