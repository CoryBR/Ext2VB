using System;
using System.Collections.Generic;
using Mono.Unix.Native;
using Mono.Fuse;
//Imports Mono.Fuse.FileSystem

class Ext2fsFuse : FileSystem
{

    Ext2fsLib.Ext2FS Ext2fs;
    Errno FS_ERR = 0;
    public Ext2fsFuse(string[] args)
    {
        string fspath = args[args.Length - 2];
        //string s;
        foreach (string s in args) {
            Console.WriteLine(s);
        }
        this.FSINIT(fspath, System.IO.FileAccess.Read);
        this.MountPoint = args[args.Length - 1];
    }

    public static int Main(string[] args)
    {
        //int i = 0;
        bool ParseFS = false;
        foreach (string s in args)
        {
            //Console.WriteLine(s);
            if (s == "-p")
            {
                System.IO.StreamWriter LogFile = new System.IO.StreamWriter("/tmp/ext2fuse.log");
                LogFile.WriteLine("Ext2fsFuse Parseing:");
                LogFile.Flush();
                ParseFS = true;
                string Name = "";
                LogFile.WriteLine("Arguments:");
                LogFile.Flush();
                foreach (string s1 in args)
                {
                    LogFile.WriteLine(s1);
                }
                LogFile.WriteLine();
                LogFile.Flush();
                Ext2fsLib.Ext2FS.Errors err = Ext2fsLib.Ext2FS.Errors.NotValidFS;
                try
                {
                    err = Ext2fsLib.Ext2FS.ParseFS("/dev/" + args[1], ref Name);
                }
                catch (Exception ex)
                {
                    LogFile.WriteLine(ex.Message);
                    LogFile.WriteLine(ex.InnerException);
                    LogFile.WriteLine(ex.StackTrace);
                    LogFile.WriteLine(ex.TargetSite);
                }
               LogFile.WriteLine("Result: " + err);
               LogFile.Flush();
               if (err == Ext2fsLib.Ext2FS.Errors.FS_Recongized)
               {
                   Console.Write(Name);
                   LogFile.WriteLine("Volume Name: " + Name);
                   LogFile.Flush();
               }
               //Console.WriteLine(err);
               LogFile.Flush();
               LogFile.Close();
               return (int)err;
            }
        }
        if (ParseFS != true)
        {
            Ext2fsFuse fs = new Ext2fsFuse(args);
            if (fs.FS_ERR == 0)
            {
                fs.Start();
            }
        }
        return 0;
    }

    public Errno FSINIT(string fspath, System.IO.FileAccess AccessMode)
    {
        try
        {
            Ext2fs = new Ext2fsLib.Ext2FS(fspath, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite, false);
            //this.Name = Ext2fs.Superblock.s_volume_name;
            if (Ext2fs.FS_ERR == Ext2fsLib.Ext2FS.Errors.NotValidFS)
            {
                //this.Stop;
                return Errno.EIO;
            }
            //Console.WriteLine(Ext2fs.Superblock.s_volume_name);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.InnerException);
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine(ex.TargetSite);
            return Errno.EIO;
        }
        return 0;
    }
    protected override Errno OnReadHandle(string file, Mono.Fuse.OpenedPathInfo info, byte[] buf, long offset, out int byteswritten)
    {
        Console.WriteLine("OnReadHandle: " + file);
        //Console.WriteLine("Handle: " + info.Handle);
        //Console.WriteLine("OpenFlags: " + info.OpenFlags);
        //Console.WriteLine("OpenAccess: " + info.OpenAccess);       
        //Console.WriteLine("Read Offset: " + offset);
        Console.WriteLine("Read Length: " + buf.Length);
        byteswritten = 0;
        Ext2fsLib.Ext2FS.Inode inode1 = new Ext2fsLib.Ext2FS.Inode();
        Ext2fsLib.Ext2FS.Errors err = Ext2fs.GetPathInode(file,ref inode1);
        if (err == Ext2fsLib.Ext2FS.Errors.NotFound)
        {
            Console.WriteLine("Not Found");
            return Errno.ENOENT;
        }
        byteswritten = Ext2fs.ReadFileBytes(inode1, offset, ref buf);
        Console.WriteLine("Bytes Written: " + byteswritten);
        return 0;
    }

    protected override Errno OnReadDirectory(string directory, Mono.Fuse.OpenedPathInfo info, out System.Collections.Generic.IEnumerable<Mono.Fuse.DirectoryEntry> paths)
    {
        //Console.WriteLine("OnReadDirectory:" + directory);
        //Console.WriteLine("Handle: " + info.Handle);
        //Console.WriteLine("OpenFlags: " + info.OpenFlags);
        //Console.WriteLine("OpenAccess: " + info.OpenAccess);

        Ext2fsLib.Ext2FS.Inode inode1 = new Ext2fsLib.Ext2FS.Inode();
        Ext2fsLib.Ext2FS.Errors err = Ext2fs.GetPathInode(directory, ref inode1);
        if (err == Ext2fsLib.Ext2FS.Errors.NotFound)
        {
            paths = null;
            return Errno.ENOENT;
        }
        Ext2fsLib.Ext2FS.DirectoryEntry[] dir = Ext2fs.ListDirectory(inode1);
        //Dim d As Mono.Fuse.DirectoryEntry
        //Ext2fsLib.Ext2FS.DirectoryEntry dir1;
        List<Mono.Fuse.DirectoryEntry> d = new List<Mono.Fuse.DirectoryEntry>();
        foreach (Ext2fsLib.Ext2FS.DirectoryEntry dir1 in dir)
        {
            Mono.Fuse.DirectoryEntry d1 = new Mono.Fuse.DirectoryEntry(dir1.name);
            //Mono.Unix.Native.Stat stat1 = new Mono.Unix.Native.Stat();
            d.Add(d1);
            //Console.WriteLine(d1.Name);
        }
        paths = d;
        return 0;
    }

    protected override Errno OnGetPathStatus(string path, out Stat stBuf)
    {
        try
        {
            //Console.WriteLine("OnGetPathStatus: " + path);
            stBuf = new Stat();
            //Console.WriteLine("stBuf Created");
            Ext2fsLib.Ext2FS.Inode inode1;
            inode1 = new Ext2fsLib.Ext2FS.Inode();
            //Console.WriteLine("Created inode1");
            Ext2fsLib.Ext2FS.Errors err;
            //Console.WriteLine("Created err");
            err = Ext2fs.GetPathInode(path,ref inode1);
          //Console.WriteLine("Got inode1");
          
          if (err == Ext2fsLib.Ext2FS.Errors.NotFound)
          {
              //Console.WriteLine("Not Found");
              return Errno.ENOENT;
          }
            //Console.WriteLine("InodeNum: " + inode1.inode_num);
            int x = inode1.i_mode;
            //Console.WriteLine(x);
            string x3 = (string)x.ToString("X");
            //Console.WriteLine(x3);
            string x2 = (string)x3[0].ToString();
            //Console.WriteLine(x3);
            //string FileMode = inode1.i_mode
            //Console.WriteLine(x2);
            switch (x2)
            {
                case "4":
                    stBuf.st_mode = NativeConvert.FromUnixPermissionString("dr-xr-xr-x");
                    break;
                case "8":
                    stBuf.st_mode = NativeConvert.FromUnixPermissionString("-r-xr-xr-x");
                    break;
                default:
                    stBuf.st_mode = NativeConvert.FromUnixPermissionString("----------");
                    break;
            }
            //Console.WriteLine("Inode Mode " + inode1.i_mode);
            stBuf.st_nlink = inode1.i_links_count;
            //Console.WriteLine("Permissions: " + NativeConvert.ToUnixPermissionString(stBuf.st_mode));
            //Console.WriteLine(stBuf.st_mode);
            stBuf.st_size = inode1.i_size;
            stBuf.st_atime = inode1.i_atime;
            stBuf.st_ctime = inode1.i_ctime;
            stBuf.st_mtime = inode1.i_mtime;
            stBuf.st_ino = inode1.inode_num;
            //Console.WriteLine("Nlink: " + stBuf.st_nlink);
            //stBuf.st_ino = inode1.inode_num;
            //stBuf.st_blksize = Ext2fs.Superblock.internal_block_size;
            //stBuf.st_blocks = inode1.i_blocks;
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.InnerException);
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine(ex.TargetSite);
            stBuf = new Stat();
            return Errno.ENOENT;
        }
    }
    protected override Errno OnGetFileSystemStatus(string path, [System.Runtime.InteropServices.Out] out Mono.Unix.Native.Statvfs buf)
    {
        buf = new Statvfs();
        buf.f_favail = Ext2fs.Superblock.s_free_blocks_count - Ext2fs.Superblock.s_r_blocks_count;
        buf.f_bsize = Ext2fs.Superblock.internal_block_size;
        buf.f_frsize = Ext2fs.Superblock.internal_block_size;
        buf.f_namemax = 255;
        buf.f_blocks = Ext2fs.Superblock.s_blocks_count;
        buf.f_bfree = Ext2fs.Superblock.s_free_blocks_count;
        buf.f_ffree = Ext2fs.Superblock.s_free_inodes_count;
        buf.f_files = Ext2fs.Superblock.s_inodes_count;

        //Console.WriteLine("f_avail: " + buf.f_favail);
        //Console.WriteLine("f_bsize: " + buf.f_bsize);
        //Console.WriteLine("f_frsize: " + buf.f_frsize);
        //Console.WriteLine("f_namemax: " + buf.f_namemax);
        //Console.WriteLine("f_blocks: " + buf.f_blocks);
        //Console.WriteLine("f_bfree: " + buf.f_bfree);
        //Console.WriteLine("f_ffree: " + buf.f_ffree);
        //Console.WriteLine("f_files: " + buf.f_files);

        return 0;
    }
}
