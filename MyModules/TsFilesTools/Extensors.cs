using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace TsFilesTools
{
    public static class Extensors
    {

        public static void SaveObject(this object obj, string path)
        {
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, obj);
            fs.Close();
            fs.Dispose();            
        }
        public static string PreparePath(this string path, string replace)
        {
            var chars = Path.GetInvalidFileNameChars();
            string res = "";
            for(int i=0;i<path.Length;i++)
            {
                if (chars.Contains(path[i]))    res += replace;
                else                            res += path[i];
            }
            return res;
        }
        public static void SaveFloats(this float[] data, string path)
        {
            string res = "";
            foreach (var i in data)
                res += i.ToString().Replace(',','.') + " ";
            if (File.Exists(path))
                File.Delete(path);
            File.WriteAllText(path, res);
        }
        public static string ReadString(this Stream stream, int count)
        {
            if (stream.Length==stream.Position) return null;
            byte[] buffer = new byte[count];
            stream.Read(buffer, 0, count);
            return buffer.ConvertToString();
        }
        public static void WriteString(this Stream stream, string cad)
        {            
            stream.Write(cad.ToBytes(), 0, cad.Length);
        }
        public static int ReadInt(this Stream stream)
        {
            byte[] buffer = new byte[sizeof(int)];
            stream.Read(buffer, 0, buffer.Length);
            return BitConverter.ToInt32(buffer, 0);
        }

        public static void WriteInt(this Stream stream,int val)
        {
            stream.Write(BitConverter.GetBytes(val), 0, 4);
        }

        public static Int16 ReadInt16(this Stream stream)
        {
            byte[] buffer = new byte[sizeof(Int16)];
            stream.Read(buffer, 0, buffer.Length);
            return BitConverter.ToInt16(buffer, 0);
        }

        public static void WriteInt16(this Stream stream, short val)
        {
            stream.Write(BitConverter.GetBytes(val), 0, 2);
        }

        public static byte[] ReadBytes(this Stream stream, int count)
        {
            byte[] buffer = new byte[count];
            stream.Read(buffer, 0, buffer.Length);
            return buffer;
        }

        public static IntPtr ReadBytesIntPtr(this Stream stream, int count)
        {
            byte[] buffer = stream.ReadBytes(count);
            IntPtr ptr = Marshal.AllocHGlobal(count);
            Marshal.Copy(buffer, 0, ptr, count);
            return ptr;
        }

        public static void WriteBytes(this Stream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
        }

        public static byte[] ToBytes(this string who)
        {
            return Encoding.UTF8.GetBytes(who.ToCharArray());
        }
        public static string ConvertToString(this byte[] who)
        {
            return Encoding.UTF8.GetString(who);
        }
    }
}
