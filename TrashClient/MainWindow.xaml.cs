//  Copyright 2021 Jonguk Kim
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Amazon.Rekognition.Model;
using Newtonsoft.Json;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using TrashClient.Data;
using TrashClient.Manager;
using Color = SixLabors.ImageSharp.Color;
using Image = SixLabors.ImageSharp.Image;
using TextOptions = SixLabors.ImageSharp.Drawing.Processing.TextOptions;

namespace TrashClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly DispatcherTimer timer = new() {Interval = TimeSpan.FromSeconds(1)};

        public MainWindow()
        {
            InitializeComponent();
            timer.Tick += TimerOnTick;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var state = await RekognitionManager.Instance.GetModelStateAsync();

            ModelAccuracy.Content = $"모델 정확도: {await RekognitionManager.Instance.GetModelAccuracy()}";

            if (state != "RUNNING" & state != "STOPPED" & state != "TRAINING_COMPLETED")
            {
                var color = state == "STARTING"
                    ? System.Windows.Media.Color.FromRgb(129, 199, 132)
                    : System.Windows.Media.Color.FromRgb(229, 115, 115);

                ModelStatus.Content = state == "STARTING" ? "시작 중" : "중지 중";
                ModelStatus.Foreground = new SolidColorBrush(color);

                if (!await RekognitionManager.Instance.WaitForModel())
                {
                    MessageBox.Show("모델 상태를 가져올 수 없습니다.", "오류");
                    Environment.Exit(1);
                }
            }


            switch (await RekognitionManager.Instance.GetModelStateAsync())
            {
                case "STOPPED":
                    StartModelButton.IsEnabled = true;
                    ModelStatus.Content = "꺼짐";
                    ModelStatus.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(198, 40, 40));
                    break;
                case "RUNNING":
                    StopModelButton.IsEnabled = true;
                    ModelStatus.Content = "켜짐";
                    ModelStatus.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(46, 125, 50));
                    timer.Start();
                    break;
                case "TRAINING_COMPLETED":
                    StartModelButton.IsEnabled = true;
                    ModelStatus.Content = "훈련 완료";
                    ModelStatus.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(56, 142, 60));
                    break;
            }
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (await RekognitionManager.Instance.GetModelStateAsync() != "STOPPED")
            {
                MessageBox.Show("모델이 아직 실행 중입니다.", "알림");
                e.Cancel = true;
            }
        }

        private async void TimerOnTick(object? sender, EventArgs e)
        {
            var receiveMessageResponse = await SqsManager.Instance.GetMessageAsync();

            if (receiveMessageResponse.Messages.Count != 0)
            {
                if (receiveMessageResponse.Messages.First() is { } message)
                {
                    if (JsonConvert.DeserializeObject<DetectRequest>(message.Body) is { } request)
                    {
                        await SqsManager.Instance.DeleteMessageAsync(message);
                        await S3Manager.Instance.DownloadFileAsync(request.Key, request.Key.Replace("images/", ""));

                        var result = await RekognitionManager.Instance.DetectObjectAsync(request.Key.Replace("images/", ""));
                        var fileName = await CreteLabeledImage(request.Key.Replace("images/", ""), result);

                        GuideLabel.Visibility = Visibility.Hidden;
                        PictureBox.Source = new BitmapImage(new Uri(fileName, UriKind.RelativeOrAbsolute));
                        ConfidenceLabel.Content = $"추론 정확도: {result.Sum(a => a.Confidence) / result.Count}%";
                    }
                }
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var state = await RekognitionManager.Instance.GetModelStateAsync();

            if (state == "STOPPED" | state == "TRAINING_COMPLETED")
            {
                ModelStatus.Content = "시작 중";
                ModelStatus.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(129, 199, 132));
                StartModelButton.IsEnabled = false;

                if (await RekognitionManager.Instance.StartModelAsync())
                {
                    StopModelButton.IsEnabled = true;
                    ModelStatus.Content = "켜짐";
                    ModelStatus.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(46, 125, 50));
                    timer.Start();
                }
            }
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ModelStatus.Content = "중지 중";
            ModelStatus.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(229, 115, 115));
            StopModelButton.IsEnabled = false;

            if (await RekognitionManager.Instance.StopModelAsync())
            {
                StartModelButton.IsEnabled = true;
                ModelStatus.Content = "꺼짐";
                ModelStatus.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(198, 40, 40));
            }
        }

        private static async Task<string> CreteLabeledImage(string fileName, List<CustomLabel> labels)
        {
            using var image = await Image.LoadAsync(fileName);
            var font = GetFont();
            var option = new TextGraphicsOptions
            {
                GraphicsOptions = new GraphicsOptions
                {
                    Antialias = true,
                    AntialiasSubpixelDepth = 32
                },
                TextOptions = new TextOptions
                {
                    HorizontalAlignment = SixLabors.Fonts.HorizontalAlignment.Center
                }
            };

            foreach (var label in labels)
            {
                try
                {
                    var rectangularPolygon = new RectangularPolygon(label.Geometry.BoundingBox.Left * image.Width,
                        label.Geometry.BoundingBox.Top * image.Height, label.Geometry.BoundingBox.Width * image.Width,
                        label.Geometry.BoundingBox.Height * image.Height);
                    var rectangularPolygon2 = new RectangularPolygon(label.Geometry.BoundingBox.Left * image.Width - 1,
                        label.Geometry.BoundingBox.Top * image.Height + 20, 150, 50);

                    var name = label.Name switch
                    {
                        "plastic" => "플라스틱",
                        "glass" => "유리",
                        "can" => "캔",
                        "paper" => "종이",
                        _ => "알 수 없음"
                    };

                    image.Mutate(data => data.Draw(Color.Orange, 5.0f, rectangularPolygon));
                    image.Mutate(data => data.Fill(Color.Orange, rectangularPolygon2));
                    image.Mutate(data => data.DrawText(option, name, font, Color.Black,
                        new PointF(label.Geometry.BoundingBox.Left * image.Width + 65, label.Geometry.BoundingBox.Top * image.Height + 20)));
                }
                catch
                {
                    MessageBox.Show($"선을 그리는중 오류가 발생함.\r\n감지된 물체: {string.Join(',', labels.Select(a => a.Name))}", "오류");
                }
            }

            await image.SaveAsync($"labeled_{System.IO.Path.GetFileNameWithoutExtension(fileName)}.jpeg", new JpegEncoder {Quality = 100});

            return $"{Environment.CurrentDirectory}\\labeled_{System.IO.Path.GetFileNameWithoutExtension(fileName)}.jpeg";
        }

        private static Font GetFont()
        {
            var collection = new FontCollection();
            var family = collection.Install("./NanumGothicBold.ttf");
            var font = family.CreateFont(35f, SixLabors.Fonts.FontStyle.Bold);

            return font;
        }
    }
}