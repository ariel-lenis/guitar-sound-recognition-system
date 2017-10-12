using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace TsExtraControls
{
    public static class Extensors
    {
        public static bool ContainsPoint(this FrameworkElement who, Point p)
        {
            if (p.X <= who.GetX() + who.CurrentWidth() && p.X >= who.GetX())
                if (p.Y <= who.GetY() + who.CurrentHeight() && p.Y >= who.GetY())
                    return true;
            return false;
        }
        public static void SetX(this UIElement who,double x)
        {
            who.SetValue(Canvas.LeftProperty, x);
        }
        public static void SetY(this UIElement who, double y)
        {
            who.SetValue(Canvas.TopProperty, y);
        }
        public static void PlusX(this UIElement who, double deltaX)
        {
            who.SetValue(Canvas.LeftProperty, who.GetX()+deltaX);
        }
        public static void PlusY(this UIElement who, double deltaY)
        {
            who.SetValue(Canvas.TopProperty, who.GetY()+deltaY);
        }
        public static double GetX(this UIElement who)
        {
            return (double)who.GetValue(Canvas.LeftProperty);
        }
        public static double GetY(this UIElement who)
        {
            return (double)who.GetValue(Canvas.TopProperty);
        }

        public static double CurrentWidth(this FrameworkElement who)
        {
            if (double.IsNaN(who.Width)) return who.ActualWidth;
            return who.Width;
        }
        public static double CurrentHeight(this FrameworkElement who)
        {
            if (double.IsNaN(who.Height)) return who.ActualHeight;
            return who.Height;
        }
        public static List<T> ForceLinq<T>(this System.Collections.IEnumerable who)
        {
            return who.Cast<T>().ToList();
        }
    }
}
