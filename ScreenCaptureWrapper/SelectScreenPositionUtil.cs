using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Reactive.Threading.Tasks;

namespace ScreenCaptureWrapper
{
    public class SelectScreenPositionUtil
    {
        public static Task<Rect> SelectScreenPositionAsync()
        {
            var textBlock = new TextBlock()
            {
                Foreground = Brushes.White,
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var border = new Border()
            {
                Background = new SolidColorBrush(Color.FromArgb(0x44, 0x99, 0, 0)),
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(1),
                Child = textBlock,
                Visibility = Visibility.Hidden
            };
            
            var canvas = new Canvas();
            canvas.Children.Add(border);

            var window = new Window()
            {
                Title = "Select Screen Position", Top = 0, Left = 0, 
                Width = SystemParameters.VirtualScreenWidth, Height = SystemParameters.VirtualScreenHeight,
                WindowStyle = WindowStyle.None, Topmost = true, ShowInTaskbar = false, AllowsTransparency = true,
                Background = new SolidColorBrush(Color.FromArgb(1, 0xff, 0xff, 0xff)),
                Cursor = Cursors.Cross, Content = canvas
            };

            var rectObservable = Observable.FromEventPattern<MouseEventArgs>(window, "MouseLeftButtonDown")
                .Select(ev => ev.EventArgs.GetPosition(window))
                .CombineLatest(Observable.FromEventPattern<MouseEventArgs>(window, "MouseMove"),
                   (downPos, ev) => {
                       var movePos = ev.EventArgs.GetPosition(window);
                       return new Rect(Math.Min(downPos.X, movePos.X), Math.Min(downPos.Y, movePos.Y),
                                       Math.Abs(downPos.X - movePos.X), Math.Abs(downPos.Y - movePos.Y));
                   })
                .TakeUntil(Observable.FromEventPattern<MouseEventArgs>(window, "MouseLeftButtonUp"));

            rectObservable.Subscribe(r =>
            {
                Canvas.SetLeft(border, r.X);
                Canvas.SetTop(border, r.Y);
                border.Width = r.Width;
                border.Height = r.Height;
                border.Visibility = Visibility.Visible;

                textBlock.Text = string.Format("{0}x{1} ({2:0.0###}, /8={3}x{4})",
                    r.Width, r.Height, 
                    r.Height != 0 ? ((double)r.Width) / r.Height : 0,
                    r.Width / 8.0, r.Height / 8.0);
            }, window.Close);

            window.Show();

            return rectObservable.ToTask(); // return a last value
        }
    }
}
