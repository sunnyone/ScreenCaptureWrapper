using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ScreenCaptureWrapper
{
    // TODO: rewrite MVVM way
    public partial class SelectPositionWindow : Window
    {
        private TaskCompletionSource<Int32Rect> taskCompletionSource;
        public SelectPositionWindow(TaskCompletionSource<Int32Rect> taskCompletionSource)
        {
            InitializeComponent();

            this.taskCompletionSource = taskCompletionSource;
        }

        bool selectionStarted = false;
        int x1 = 0, y1 = 0, x2 = 0, y2 = 0;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int width = 0, height = 0;
            foreach (var s in System.Windows.Forms.Screen.AllScreens)
            {
                width += s.Bounds.Width;
                height += s.Bounds.Height;
            }

            this.Left = 0;
            this.Top = 0;
            this.Width = width;
            this.Height = height;

            this.Cursor = Cursors.Cross;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();

                taskCompletionSource.SetCanceled();
            }
        }

        private Int32Rect getRectFromXY()
        {
            int left = (x1 < x2) ? x1 : x2;
            int top = (y1 < y2) ? y1 : y2;
            int width = Math.Abs(x2 - x1);
            int height = Math.Abs(y2 - y1);

            return new Int32Rect(left, top, width, height);
        }

        private void updateRectSize()
        {
            Int32Rect rect = getRectFromXY();

            Canvas.SetLeft(this.rectangle, rect.X);
            this.rectangle.Width = rect.Width;

            Canvas.SetTop(this.rectangle, rect.Y);
            this.rectangle.Height = rect.Height;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(this);

            x1 = x2 = (int) point.X;
            y1 = y2 = (int) point.Y;

            updateRectSize();
            selectionStarted = true;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (!this.selectionStarted)
                return;

            var point = e.GetPosition(this);

            x2 = (int)point.X;
            y2 = (int)point.Y;

            updateRectSize();
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(this);

            x2 = (int)point.X;
            y2 = (int)point.Y;

            var rect = getRectFromXY();
            this.Close();

            taskCompletionSource.SetResult(rect);
        }

        public static Task<Int32Rect> SelectScreenPositionAsync()
        {
            var tcs = new TaskCompletionSource<Int32Rect>();
            new SelectPositionWindow(tcs).ShowDialog();
            return tcs.Task;
        }
    }
}
