using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WPFMediaKit.DirectShow.Controls;

namespace NSeetafaceDemo
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            cb.ItemsSource = MultimediaUtil.VideoInputNames;//获得所有摄像头
            if (MultimediaUtil.VideoInputNames.Length > 0)
            {
                cb.SelectedIndex = 0;//第0个摄像头为默认摄像头
            }
            else
            {
                MessageBox.Show("电脑没有安装任何可用摄像头");
            }
        }

        private void btnCapture_Click(object sender, RoutedEventArgs e)//拍照
        {
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)vce.ActualWidth, (int)vce.ActualHeight,
                //vce是前台wpfmedia控件的name
                96, 96, PixelFormats.Default);
            //为避免抓不全的情况，需要在Render之前调用Measure、Arrange
            //为避免VideoCaptureElement显示不全，需要把
            //VideoCaptureElement的Stretch="Fill"
            vce.Measure(vce.RenderSize);
            vce.Arrange(new Rect(vce.RenderSize));
            bmp.Render(vce);
            BitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                byte[] captureData = ms.ToArray();
                
                File.WriteAllBytes(@"./photo/" + Guid.NewGuid().ToString().Substring(0, 5) + ".jpg", captureData);
            }
            vce.Pause();
        }

        //重拍
        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            vce.Play();
        }

        private void cb_SelectionChanged(object sender, SelectionChangedEventArgs e)//ComboBox控件的选择事件
        {
            vce.VideoCaptureSource = (string)cb.SelectedItem;//vce是前台wpfmedia控件的name
        }
    }
}
