using Microsoft.Win32;
using Newtonsoft.Json;
using Seetaface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NSeetafaceDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Rectangle> rects = new List<Rectangle>();
        List<Ellipse> ellipses = new List<Ellipse>();
        float[] feat1 = new float[2048];
        float[] feat2 = new float[2048];
        float[] feat;

        bool ret = false;
        //打印dll的日志信息必须要用一个静态变量保存委托, 不然GC会自动回收.
        static LogCallBack logger;

        public MainWindow()
        {
            InitializeComponent();
            logger = new LogCallBack(PrintLog);

            string path = @"model\";
            SeetafaceHelper.SetDisplayLog(logger);
            SeetafaceHelper.SetModelDirectory(Encoding.Default.GetBytes(path));
            ret = SeetafaceHelper.Init();
            initRects();
        }

        void initRects()
        {
            for (int i = 0; i < 10; i++)
            {
                var rect = new Rectangle() {
                    Visibility = Visibility.Hidden,
                    Stroke = Brushes.Blue,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };
                rects.Add(rect);
                grid.Children.Add(rect);
            }

            for (int i = 0; i < 10*5; i++)
            {
                var e = new Ellipse()
                {
                    Width = 5,
                    Height = 5,
                    Fill = Brushes.Red,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Visibility = Visibility.Hidden
                };

                ellipses.Add(e);
                grid.Children.Add(e);
            }
        }

        void PrintLog(string txt)
        {
            this.Dispatcher.Invoke(
                new ThreadStart(() =>
                {
                    log.AppendText(txt + "\n");
                })
            );
            //MessageBox.Show(txt);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ret)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                if(ofd.ShowDialog() == true)
                {
                    rects.ForEach(r => r.Visibility = Visibility.Hidden);
                    ellipses.ForEach(a => a.Visibility = Visibility.Hidden);

                    BitmapImage bi = new BitmapImage();
                    // BitmapImage.UriSource must be in a BeginInit/EndInit block.
                    bi.BeginInit();
                    bi.UriSource = new Uri(ofd.FileName, UriKind.RelativeOrAbsolute);
                    bi.EndInit();
                    // Set the image source.
                    img.Width = bi.Width;
                    img.Height = bi.Height;
                    img.Source = bi;


                    //FaceInfo faceInfo = new FaceInfo();
                    //var faceCount = SeetafaceHelper.DetectFace(
                    //    ofd.FileName,
                    //    ref faceInfo
                    //);

                    StringBuilder json = new StringBuilder(500);
                    var faceCount = SeetafaceHelper.DetectFaces(
                        ofd.FileName,
                        json
                    );

                    //if (faceCount > 0)
                    {
                        //MessageBox.Show(json.ToString());
                        JsonSerializer serializer = new JsonSerializer();
                        FaceInfo[] faces = serializer.Deserialize<FaceInfo[]>(
                            new JsonTextReader(
                                new StringReader(json.ToString())
                            )
                        );

                        for (int i = 0; i < faceCount; i++)
                        {
                            var faceInfo = faces[i];
                            var rect = rects[i];
                            rect.Margin = new Thickness(faceInfo.bbox.x, faceInfo.bbox.y, 0, 0);
                            rect.Width = faceInfo.bbox.width;
                            rect.Height = faceInfo.bbox.height;
                            rect.Visibility = Visibility.Visible;
                        }
                     
                    }
                    //else
                    //    MessageBox.Show("检测失败");
                }
            }
            else
            {
                MessageBox.Show("初始化失败");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            new Window1().Show();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (ret)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == true)
                {
                    rects.ForEach(r => r.Visibility = Visibility.Hidden);
                    ellipses.ForEach(a => a.Visibility = Visibility.Hidden);

                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(ofd.FileName, UriKind.RelativeOrAbsolute);
                    bi.EndInit();
                    // Set the image source.
                    img.Width = bi.Width;
                    img.Height = bi.Height;
                    img.Source = bi;

                     
                    StringBuilder json = new StringBuilder(1500);
                    var faceCount = SeetafaceHelper.Alignment(
                        ofd.FileName,
                        json
                    );

                    if (faceCount > 0)
                    {
                        //MessageBox.Show(json.ToString());
                        JsonSerializer serializer = new JsonSerializer();
                        AlignmentResult[] faces = serializer.Deserialize<AlignmentResult[]>(
                            new JsonTextReader(
                                new StringReader(json.ToString())
                            )
                        );

                        //画矩形
                        for (int i = 0; i < faceCount && i< 10; i++)
                        {
                            var faceInfo = faces[i].face;
                            var rect = rects[i];
                            rect.Margin = new Thickness(faceInfo.bbox.x, faceInfo.bbox.y, 0, 0);
                            rect.Width = faceInfo.bbox.width;
                            rect.Height = faceInfo.bbox.height;
                            rect.Visibility = Visibility.Visible;

                            var points = faces[i].landmark;
                            //画点
                            for (int j = i*5, k=0; k<5; j++, k++)
                            {
                                ellipses[j].Margin = new Thickness(points[k].x, points[k].y, 0, 0);
                                ellipses[j].Visibility = Visibility.Visible;
                            }
                        }

                    }
                    else
                        MessageBox.Show("检测失败");
                }
            }
            else
            {
                MessageBox.Show("初始化失败");
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (ret)
            {
                feat1 = new float[2048];
                feat = feat1;
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == true)
                {
                    rects.ForEach(r => r.Visibility = Visibility.Hidden);
                    ellipses.ForEach(a => a.Visibility = Visibility.Hidden);

                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(ofd.FileName, UriKind.RelativeOrAbsolute);
                    bi.EndInit();
                    // Set the image source.
                    img.Width = bi.Width;
                    img.Height = bi.Height;
                    img.Source = bi;


                    StringBuilder json = new StringBuilder(1500);
                    var faceCount = SeetafaceHelper.Alignment(
                        ofd.FileName,
                        json
                    );

                    if (faceCount > 0)
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        AlignmentResult[] faces = serializer.Deserialize<AlignmentResult[]>(
                            new JsonTextReader(
                                new StringReader(json.ToString())
                            )
                        );

                        //画矩形
                        for (int i = 0; i < faceCount; i++)
                        {
                            var faceInfo = faces[i].face;
                            var rect = rects[i];
                            rect.Margin = new Thickness(faceInfo.bbox.x, faceInfo.bbox.y, 0, 0);
                            rect.Width = faceInfo.bbox.width;
                            rect.Height = faceInfo.bbox.height;
                            rect.Visibility = Visibility.Visible;

                            var points = faces[i].landmark;
                            //画点
                            for (int j = i * 5, k = 0; k < 5; j++, k++)
                            {
                                ellipses[j].Margin = new Thickness(points[k].x, points[k].y, 0, 0);
                                ellipses[j].Visibility = Visibility.Visible;
                            }
                        }

                        //float[] feat = SeetafaceHelper.ExtractFeature(ofd.FileName, ref faces[0].face, ref faces[0].landmark);
                        SeetafaceHelper.ExtractFeature(ofd.FileName, ref faces[0], feat);

                        

                        PrintLog(string.Join(",", feat.Select(f=>f.ToString()).ToArray() ));
                        
                    }
                    else
                        MessageBox.Show("检测失败");
                }
            }
            else
            {
                MessageBox.Show("初始化失败");
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (ret)
            {
                feat2 = new float[2048];
                feat = feat2;
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == true)
                {
                    rects.ForEach(r => r.Visibility = Visibility.Hidden);
                    ellipses.ForEach(a => a.Visibility = Visibility.Hidden);

                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(ofd.FileName, UriKind.RelativeOrAbsolute);
                    bi.EndInit();
                    // Set the image source.
                    img.Width = bi.Width;
                    img.Height = bi.Height;
                    img.Source = bi;


                    StringBuilder json = new StringBuilder(1500);
                    var faceCount = SeetafaceHelper.Alignment(
                        ofd.FileName,
                        json
                    );

                    if (faceCount > 0)
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        AlignmentResult[] faces = serializer.Deserialize<AlignmentResult[]>(
                            new JsonTextReader(
                                new StringReader(json.ToString())
                            )
                        );

                        //画矩形
                        for (int i = 0; i < faceCount; i++)
                        {
                            var faceInfo = faces[i].face;
                            var rect = rects[i];
                            rect.Margin = new Thickness(faceInfo.bbox.x, faceInfo.bbox.y, 0, 0);
                            rect.Width = faceInfo.bbox.width;
                            rect.Height = faceInfo.bbox.height;
                            rect.Visibility = Visibility.Visible;

                            var points = faces[i].landmark;
                            //画点
                            for (int j = i * 5, k = 0; k < 5; j++, k++)
                            {
                                ellipses[j].Margin = new Thickness(points[k].x, points[k].y, 0, 0);
                                ellipses[j].Visibility = Visibility.Visible;
                            }
                        }
                         
                        //float[] feat = SeetafaceHelper.ExtractFeature(ofd.FileName, ref faces[0].face, ref faces[0].landmark);
                        SeetafaceHelper.ExtractFeature(ofd.FileName, ref faces[0], feat);

                        double sim = SeetafaceHelper.CalcSimilarity(feat1, feat2);
                        MessageBox.Show("相似度:" + sim);

                    }
                    else
                        MessageBox.Show("检测失败");
                }
            }
            else
            {
                MessageBox.Show("初始化失败");
            }
        }
    }
}
