using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsMusicXMLDisplayer.GDI.Drawing
{
    public class TsLineOnPaper:ITsDrawable
    {
        public int LineNumber { get; set; }
        public TsGDIPaper Paper { get; set; }

        public TsPartwise Partwise { get; set; }

        public int MeasureIdx { get; set; }
        public int MeasureCount { get; set; }

        List<TsCacheGraphics> cacheGraphics;

        public TsLineOnPaper(TsPartwise partwise)
        {
            this.Partwise = partwise;
        }

        public void Draw(TsMeasure measure, TsDrawEngine param)
        {
            for (int i = this.MeasureIdx; i < this.MeasureIdx + this.MeasureCount; i++)
                this.Partwise.Measures[i].Draw(null, param);
            param.Draw(this.Partwise.Measures[this.MeasureIdx], this);
        }

        public void CalculateCachePosition(TsDrawEngine param, Dictionary<string, object> additional)
        {

        }

        public List<TsCacheGraphics> CacheGraphics
        {
            get { return this.cacheGraphics; }
        }

        public void SetDimensions(TsDrawEngine param, float currentX, float currentY, float usablew, float h)
        {
            TsCacheGraphics cache=new TsCacheGraphics();
            cache.Rectangle = new System.Drawing.RectangleF(currentX, currentY, usablew, h);
            this.cacheGraphics = new List<TsCacheGraphics>() { cache };
        }

        public void CacheGroups(TsDrawEngine param, Dictionary<string, object> additional)
        {
            for (int i = 0; i < this.MeasureCount; i++)
            {
                int idx = this.MeasureIdx + i;
                var currentm = this.Partwise.Measures[idx];
                int countm = currentm.Elements.Count;
                for (int j = 0; j < countm; j++)
                    if (currentm.Elements[j].Groups!=null && currentm.Elements[j].Groups.Count > 0)
                        SubCacheGroups(idx,j,param,additional);
            }
        }

        private void SubCacheGroups(int idm,int ide,TsDrawEngine param, Dictionary<string, object> additional)
        {
            var currentm = this.Partwise.Measures[idm];

            var firstnote = currentm.Elements.FirstOrDefault(x => x is Elements.TsNote);

            TsMeasure nextm = null;
            Elements.TsNote nextn = null;
            TsCacheGraphics cache = new TsCacheGraphics();

            var element = currentm.Elements[ide] as Elements.TsNote;
            float destX=0;
            float destY=0;
            float originX=0;
            float originY=0;

            foreach (var igroup in element.Groups)
            {
                var infos = element.ChordInfos;
                foreach(var info in infos)
                {
                    if (igroup.GroupType == TsGroupElement.EGroupType.Ligadura)
                    {
                        originX = element.ConvertToLineX(info.CenterX);
                        destY=originY = element.ConvertToLineY(info.CenterY);

                        bool draw = false;

                        if (igroup.IsStart(element))
                        {
                            
                            if (ide == currentm.Elements.Count - 1)
                            {
                                if ((idm==this.Partwise.Measures.Count-1 && ide==currentm.Elements.Count-1) || 
                                    idm == this.MeasureIdx + this.MeasureCount - 1)
                                    destX = currentm.CacheGraphics[0].Rectangle.Right;
                                else
                                {
                                    nextm = this.Partwise.Measures[idm + 1];
                                    if (nextm.Elements.Count == 0 || !(nextm.Elements[0] is Elements.TsNote))
                                        destX = currentm.CacheGraphics[0].Rectangle.Right;
                                    else
                                    { 
                                        nextn = nextm.Elements[0] as Elements.TsNote;
                                        destX = nextn.ConvertToLineX(nextn.ChordInfos[0].CenterX);
                                    }
                                }
                                draw = true;
                            }
                            else
                            {
                                if (!(currentm.Elements[ide + 1] is Elements.TsNote))
                                    continue;
                                nextn = currentm.Elements[ide + 1] as Elements.TsNote;
                                destX = nextn.ConvertToLineX(nextn.ChordInfos[0].CenterX);
                                draw = true;
                            }
                            /*
                            cache.Lines.Add(new Drawing.TsCacheLine()
                            {
                                Width = 4,
                                X1 = element.ConvertToLineX(info.CenterX),
                                Y1 = element.ConvertToLineY(info.CenterY),
                                X2 = element.ConvertToLineX(info.CenterX) + 20,
                                Y2 = element.ConvertToLineY(info.CenterY) + 20
                            });
                             * */

                        }
                        //First element of the measure
                        if (igroup.IsEnd(element) && element==firstnote && idm==this.MeasureIdx)
                        {
                            destX = originX;
                            destY = originY;

                            originX = destX - 50;
                            draw = true;
                        }
                        if (draw)
                        {
                            cache.Arcs.Add(new TsCacheArc() {
                                 tickness=1,
                                 X1 = originX,
                                 Y1 = originY,
                                 X2 = destX,
                                 Y2 = originY,
                                 H = param.Enviroment.HeightSpace,
                                 Direction = element.Direcction.Direcction
                            });
                            /*
                            cache.Lines.Add(new Drawing.TsCacheLine()
                            {
                                Width = 4,
                                X1 = originX,
                                Y1 = originY,
                                X2 = (originX + destX) / 2,
                                Y2 = originY + 30
                            });
                            cache.Lines.Add(new Drawing.TsCacheLine()
                            {
                                Width = 4,
                                X1 = (originX + destX) / 2,
                                Y1 = originY + 30,
                                X2 = destX,
                                Y2 = destY,
                            });*/
                        }
                        
                    }
                }
            }
            this.CacheGraphics.Add(cache);
        }
    }
}
