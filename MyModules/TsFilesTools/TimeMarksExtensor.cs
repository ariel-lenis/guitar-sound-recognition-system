using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsFilesTools
{
    public static class TimeMarksExtensor
    {
        public static void MarkersAround(this List<TimeMark> Marks, int position, out int a, out int b)
        {            
            a = -1;
            b = -1;
            for (int i = 0; i < Marks.Count; i++)
                if (Marks[i].MarkPosition > position)
                {
                    a = i - 1;
                    break;
                }
            for (int i = Marks.Count - 1; i >= 0; i--)
                if (Marks[i].MarkPosition < position)
                {
                    b = i + 1;
                    break;
                }
            if (b >= Marks.Count)
                b = -1;
        }


        public static List<TimeMark> MarkersBetween(this List<TimeMark> Marks,int markA, int markB)
        {
            List<TimeMark> res = new List<TimeMark>();

            float length = markB - markA + 1;

            if (Marks.Count == 0) return res;
            foreach (var i in Marks.Where(x => x.IsNoteOn))
            {
                bool add = false;
                if ((i.MarkPosition >= markA && i.MarkPosition <= markB))
                {
                    if ((i.MarkPosition - markB + 1) / length >= 0.1f)//10%
                        add = true;
                }
                if (i.RelatedMark.MarkPosition >= markA && i.RelatedMark.MarkPosition <= markB)
                {
                    if ((markA - i.RelatedMark.MarkPosition + 1) / length >= 0.1f)//10%
                        add = true;
                }
                if (i.MarkPosition <= markA && i.RelatedMark.MarkPosition >= markB)
                    add = true;
                if (i.MarkPosition >= markA && i.RelatedMark.MarkPosition <= markB)
                    add = true;
                if (add)
                    res.Add(i);
            }
            return res;
        }
    }
}
