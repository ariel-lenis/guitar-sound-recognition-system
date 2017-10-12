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
    public class RawData
    {
        public static T ReadObject<T>(string path)
        {
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
            BinaryFormatter formatter = new BinaryFormatter();
            var res = (T)formatter.Deserialize(fs);
            fs.Dispose();
            return res;
        }
        public static void WriteObject<T>(string path, T data)
        {
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(fs, data);
            fs.Dispose();
        }


    }
}
