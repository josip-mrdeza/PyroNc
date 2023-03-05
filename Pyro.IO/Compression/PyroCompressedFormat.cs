using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pyro.IO.Compression;

public class PyroCompressedFormat
{
    private const string Separator = @"\**-**/";

    private static byte[] EndOfFile = new[]
    {
        (byte)'E',
        (byte)'O',
        (byte)'F',
        (byte) 0
    };
    public string Name { get; set; }

    public List<CompressedFile> Files { get; set; }

    public CompressedFile this[string name] => Files.First(cf => cf.Name == name);
    public CompressedFile this[int n] => Files[n];

    public PyroCompressedFormat(string name)
    {
        Name = name;
        Files = new List<CompressedFile>();
    }

    public void WriteToFile(string folder)
    {
        FileInfo fi = new FileInfo(Path.Combine(folder, Name + ".pyrocf"));
        var fs = fi.OpenWrite();
        Write(fs);
    }

    public async Task WriteToFileAsync(string folder)
    {
        FileInfo fi = new FileInfo(Path.Combine(folder, Name + ".pyrocf"));
        var fs = fi.OpenWrite();
        await WriteAsync(fs);
    }
    public void Write(Stream stream)
    {
        WriteName(stream);
        foreach (var file in Files)
        {
            file.Write(stream);
        }
        stream.Write(EndOfFile, 0, EndOfFile.Length);
        stream.Flush();
    }

    public async Task WriteAsync(Stream stream)
    {
        await WriteNameAsync(stream);
        foreach (var file in Files)
        {
            await file.WriteAsync(stream);
        }

        await stream.WriteAsync(EndOfFile, 0, EndOfFile.Length);
        await stream.FlushAsync();
    }

    private void WriteName(Stream stream)
    {
        var str = Encoding.UTF8.GetBytes(Name);
        var len = BitConverter.GetBytes(str.Length);
        stream.Write(len, 0, len.Length);
        stream.Write(str, 0, str.Length);
    }
    
    private async Task WriteNameAsync(Stream stream)
    {
        var str = Encoding.UTF8.GetBytes(Name);
        var len = BitConverter.GetBytes(str.Length);
        await stream.WriteAsync(len, 0, len.Length);
        await stream.WriteAsync(str, 0, str.Length);
    }

    public static PyroCompressedFormat ReadFromStream(Stream stream)
    {
        var rented = new byte[sizeof(int)];
        int pos = 0;
        pos += stream.Read(rented, 0, rented.Length);
        var nameLength = BitConverter.ToInt32(rented, 0);
        rented = new byte[nameLength];
        pos += stream.Read(rented, 0, rented.Length);
        string name = Encoding.UTF8.GetString(rented);
        PyroCompressedFormat compressedFormat = new PyroCompressedFormat(name);
        //start reading files

        for(int i = 0;; i++)
        {
            var strLenBuffer = new byte[4];
            var dataLenBuffer = new byte[8];
            pos += stream.Read(strLenBuffer, 0, sizeof(int));
            if (strLenBuffer[0] == (byte)'E' &&
                strLenBuffer[1] == (byte)'O' &&
                strLenBuffer[2] == (byte)'F' &&
                strLenBuffer[3] == (byte) 0)
            {
                break;
            }
            var len = BitConverter.ToInt32(strLenBuffer, 0);
            var nameBuffer = new byte[len];
            pos += stream.Read(nameBuffer, 0, len);
            name = Encoding.UTF8.GetString(nameBuffer);
            //
            pos += stream.Read(dataLenBuffer, 0, sizeof(long));
            len = (int) BitConverter.ToInt64(dataLenBuffer, 0);
            byte[] data = new byte[len];
            pos += stream.Read(data, 0, len);
            var cf = new CompressedFile(name, data, compressedFormat);
            cf.Parent.Files.Add(cf);
        }

        return compressedFormat;
    }

    public class CompressedFile
    {
        public string Name { get; set; }

        public byte[] Data { get; set; }

        public PyroCompressedFormat Parent { get; }
        private long _length = -1;

        public CompressedFile(string name, byte[] data, PyroCompressedFormat parent)
        {
            Name = name;
            Data = data;
            Parent = parent;
        }

        public void Write(Stream stream)
        {
            var strName = Encoding.UTF8.GetBytes(Name);
            _length = strName.Length + sizeof(long) + Data.Length;
            var lenAsBytes = BitConverter.GetBytes(_length);
            var strLen = BitConverter.GetBytes(strName.Length);
            
            stream.Write(strLen, 0, strLen.Length);
            stream.Write(strName, 0, strName.Length);
            stream.Write(lenAsBytes, 0, sizeof(long));
            stream.Write(Data, 0, Data.Length);
        }

        public async Task WriteAsync(Stream stream)
        {
            var strName = Encoding.UTF8.GetBytes(Name);
            _length = strName.Length + sizeof(long) + Data.Length;
            var lenAsBytes = BitConverter.GetBytes(_length);
            var strLen = BitConverter.GetBytes(strName.Length);
            
            await stream.WriteAsync(strLen, 0, strLen.Length);
            await stream.WriteAsync(strName, 0, strName.Length);
            await stream.WriteAsync(lenAsBytes, 0, sizeof(long));
            await stream.WriteAsync(Data, 0, Data.Length); 
        }
    }
}