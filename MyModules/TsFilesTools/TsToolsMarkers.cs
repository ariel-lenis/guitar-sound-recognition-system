using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsFilesTools
{
    public class TsToolsMarkers
    {
        public static void Adjust(TsTrainingSet trainingSet)
        {
            if (trainingSet.Markers.Count == 0) return;
            TimeSpan start = GetFirstNoteTime(trainingSet);
            AdjustWithTime(trainingSet, start);
        }
        public static void AdjustWithTime(TsTrainingSet trainingSet, TimeSpan startdata)
        {
            var m0 = trainingSet.Markers.First().MarkTime;
            var delta = startdata.Subtract(m0);
            //foreach (var i in trainingSet.Markers)                
            //    i.MarkTime = i.MarkTime.Add(delta);
            trainingSet.Adjust += delta;
        }
        public static TimeSpan GetFirstNoteTime(TsTrainingSet trainingSet)
        {
            return TimeSpan.FromSeconds((float)GetFirstNotePosition(trainingSet)/trainingSet.WaveDescriptor.SampleRate);
        }
        private static int GetFirstNotePosition(TsTrainingSet trainingSet)
        {
            float[] samples = trainingSet.Wave;

            float back = 0.2f;//200ms
            //int start = (int)((trainingSet.Markers.First(x=>x.IsNoteOn).MarkTime.TotalSeconds-back) * trainingSet.WaveDescriptor.SampleRate);


            int start = 0;

            for (int i = start; i < samples.Length - 50; i++)
                if (VerifyStart(samples, i))
                    return i;
            throw new Exception("Cant get the position.");
        }
        private static bool VerifyStart(float[] samples, int idx)
        {
            float prom = 0;
            for(int i=10;i<=50;i+=10)
            {
                prom = 0;
                for (int j = 0; j < 10; j++)
                    prom += Math.Abs(samples[idx +i+ j]);
                prom /= 10;
                if (prom < /*short.MaxValue*/ 1* 0.01) return false;
            }
            return true;
        }
        public static void FindBetween(List<TimeMark> marks, int sampleini, int samplelenght, out int ini, out int size)
        {
            ini = FindValue(marks, sampleini, 0, marks.Count - 1);
            size = FindValue(marks, sampleini + samplelenght - 1, 0, marks.Count - 1) + 1 - ini;
        }
        private static int FindValue(List<TimeMark> marks, int sampleini, int from, int to)
        {
            if (from == to) return from;
            if (from + 1 == to)
            {
                if (marks[to].MarkPosition == sampleini) return to;
                return from;
            }
            int mid = (from + to) / 2;
            if (marks[mid].MarkPosition > sampleini)
                return FindValue(marks, sampleini, from, mid - 1);
            else if (marks[mid].MarkPosition < sampleini)
                return FindValue(marks, sampleini, mid + 1, to);
            else
                return mid;
        }
    }
}
