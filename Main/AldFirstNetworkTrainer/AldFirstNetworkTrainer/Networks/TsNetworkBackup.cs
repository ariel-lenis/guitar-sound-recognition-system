using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace AldFirstNetworkTrainer.Networks
{
    [Serializable]
    public class TsNetworkBackup
    {
        public string Namespace { get; set; }
        public string Key { get; set; }
        public float LearningRate { get; set; }
        public float Alpha { get; set; }
        public Type NetworkType { get; set; }
        public string Description { get; set; }
        
        public DateTime Date { get; set; }
        public byte[] Data { get; set; }

        public TsNetworkBackup(string thenamespace,string thekey)
        {
            this.Key = thekey;
            this.Namespace = thenamespace;
        }

        public void SaveFile(string path)
        {
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, this);
            fs.Flush();
            fs.Dispose();
        }

        public static TsNetworkBackup LoadFromFile(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            TsNetworkBackup res = (TsNetworkBackup)bf.Deserialize(fs);
            fs.Dispose();
            return res;
        }
    }
}
