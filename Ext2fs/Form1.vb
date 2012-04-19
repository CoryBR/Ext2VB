Public Class Form1
    Dim ext2 As Ext2fsLib.Ext2FS
    Dim curdir As Ext2fsLib.Ext2FS.DirectoryEntry()
    Dim DirPath As String = "/"

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim dr As DialogResult = OpenFileDialog1.ShowDialog
        If dr = Windows.Forms.DialogResult.OK Then
            'Dim str As New IO.FileStream(OpenFileDialog1.FileName, IO.FileMode.Open, IO.FileAccess.Read)
            'str.Position = 1024
            ext2 = New Ext2fsLib.Ext2FS(OpenFileDialog1.FileName, IO.FileAccess.Read, IO.FileShare.Read, False)
            'curdir = 'ext2.ReadRootDir()
            DirPath = "/"
            Dim inode1 As New Ext2fsLib.Ext2FS.Inode
            Dim err As Ext2fsLib.Ext2FS.Errors = ext2.GetPathInode("/", inode1)
            If err = Ext2fsLib.Ext2FS.Errors.NotFound Then
                Debugger.Break()
            End If
            curdir = ext2.ListDirectory(inode1)
            Dim d As Ext2fsLib.Ext2FS.DirectoryEntry
            For Each d In curdir
                ListBox1.Items.Add(d.name)
            Next
        End If
    End Sub

    Private Sub ListBox1_MouseDoubleClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles ListBox1.MouseDoubleClick
        If ListBox1.SelectedIndex <> -1 Then
            'Dim inodeTable As Ext2fsLib.Ext2FS.Inode() = ext2.ReadInodeTable(0, ext2.ReadInodeBitmap(0))
            Dim s As Ext2fsLib.Ext2FS.DirectoryEntry
            Dim i As UInt32 = 0
            For Each s In curdir
                If s.name = curdir(ListBox1.SelectedIndex).name Then
                    If s.file_type = Ext2fsLib.Ext2FS.EXT2_FS.EXT2_FT_REG_FILE Then
                        Dim sfd As New SaveFileDialog
                        sfd.FileName = curdir(ListBox1.SelectedIndex).name
                        Dim dr As DialogResult = sfd.ShowDialog
                        If dr = Windows.Forms.DialogResult.OK Then
                            'ext2.ReadFileByBytes(ext2.GetInodeByNum(s.fileInodeNum), sfd.FileName)
                            Dim inode1 As New Ext2fsLib.Ext2FS.Inode
                            ext2.GetPathInode(DirPath & "/" & s.name, inode1)
                            ext2.ReadFileByBytes(inode1, sfd.FileName)
                            Exit For
                        End If
                    ElseIf s.file_type = Ext2fsLib.Ext2FS.EXT2_FS.EXT2_FT_DIR Then
                        ListBox1.Items.Clear()
                        If DirPath = "/" Then
                            DirPath += s.name
                        Else
                            DirPath += "/" & s.name
                        End If
                        Dim inode1 As New Ext2fsLib.Ext2FS.Inode
                        Dim err As Ext2fsLib.Ext2FS.Errors = ext2.GetPathInode(DirPath, inode1)
                        If err = Ext2fsLib.Ext2FS.Errors.NotFound Then
                            Debugger.Break()
                        End If
                        curdir = ext2.ListDirectory(inode1)
                        Dim d As Ext2fsLib.Ext2FS.DirectoryEntry
                        For Each d In curdir
                            ListBox1.Items.Add(d.name)
                        Next
                        Exit For
                        End If
                End If
                i += 1
            Next
        End If
    End Sub
End Class
