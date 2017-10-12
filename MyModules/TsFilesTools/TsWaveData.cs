using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AldSpecialAlgorithms;
using TsFilesTools;

namespace TsFilesTools
{
    public class TsWaveData<T> :IDisposable where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
    {
        public T[] Points;
        public SegmentTreeNodeZ<T> Magic;
        public int samplerate;
        public List<TimeMark> Marks;

        public TsWaveData(List<TimeMark> marks, int samplerate, T[] data, int MagicNumber = 128)
        {
            this.Marks = marks;
            this.Points = data;
            this.samplerate = samplerate;
            Magic = SegmentTreeNodeZ<T>.CreateAll(MagicNumber, data);            
        }

        public void Dispose()
        {
            Marks = null;
            Points = null;
            Magic.Dispose();
        }

    }
}
