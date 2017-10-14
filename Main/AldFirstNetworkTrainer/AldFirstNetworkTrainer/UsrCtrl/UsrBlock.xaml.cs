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

namespace AldFirstNetworkTrainer
{
	/// <summary>
	/// Interaction logic for UsrBlock.xaml
	/// </summary>
	public partial class UsrBlock : UserControl
	{
		public UsrBlock()
		{
			this.InitializeComponent();
            this.progressAll.Minimum = 0;
            this.progressAll.Maximum = 1;
		}

        public void Block(Grid where,string title)
        {
            if (this.Parent != null)
                (this.Parent as Grid).Children.Remove(this);
            where.Children.Insert(where.Children.Count, this);
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            this.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            this.Width = double.NaN;
            this.Height = double.NaN;
            this.txtTitle.Content = title;
            this.progressAll.Value = 0;
        }

        public void UnBlock(Grid where)
        {
            if (Parent == null) return;
            (this.Parent as Grid).Children.Remove(this);
        }

        public void SetProgress(int i, int n)
        {
            this.Dispatcher.Invoke(new Action(delegate
            {
                this.progressAll.Value = (double)i / n;
                this.txtProgress.Content = i + " of " + n;
            }));
        }
    }
}