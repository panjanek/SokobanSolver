using PanJanek.SokobanSolver.Engine;
using PanJanek.SokobanSolver.Sokoban;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PanJanek.SokobanSolver.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly SolidColorBrush whiteBrush = new SolidColorBrush(Colors.White);

        private static readonly SolidColorBrush blackBrush = new SolidColorBrush(Colors.Black);

        private static readonly SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);

        private static readonly SolidColorBrush greenBrush = new SolidColorBrush(Colors.Green);

        private static readonly SolidColorBrush yellowBrush = new SolidColorBrush(Colors.Yellow);

        private static readonly SolidColorBrush blueBrush = new SolidColorBrush(Colors.Blue);

        private Solution<SokobanPosition> solution;

        private List<SokobanPosition> path;

        private int index = 0;

        private Timer timer = new Timer();

        public MainWindow()
        {
            InitializeComponent();
            this.timer.Interval = 50;
            this.timer.Elapsed += timer_Elapsed;
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(
                      DispatcherPriority.Background,
                       new Action(() =>
                       {
                           if (solution != null)
                           {
                               if (solution.FinalPosition != null && this.path != null)
                               {
                                   if (index < this.path.Count - 1)
                                   {
                                       index++;
                                       this.DrawStep();
                                       if (index == this.path.Count - 1)
                                       {
                                           timer.Stop();
                                       }
                                   }
                               }
                           }
                       }));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var position = SokobanPosition.LoadFromFile(".\\..\\..\\..\\levels\\sokoban1.txt");
            position.GetSuccessors();
            this.Draw(canvas, position);


            /*
            var solver = new Solver<SokobanPosition>();
            this.solution = solver.AStar(position);
            string str = "";
            if (solution.FinalPosition != null)
            {
                
                this.index = 0;
                this.path = SokobanUtil.GetFullPath(solution.GetPath().ToArray());
                this.DrawStep();
                str += string.Format("time: {0}\n", solution.Time);
                str += string.Format("pushes: {0}\n", solution.GetPath().Count - 1);
                str += string.Format("expanded: {0}\n", solution.ExpandedNodesCount);
                str += string.Format("visited: {0}\n", solution.VisitedNodesCount);
                str += string.Format("all steps: {0}\n", SokobanUtil.GetFullPath(solution.GetPath().ToArray()).Count - 1);
            }
            else
            {
                str = "Solution not found";
            }

            textBox.Clear();
            textBox.Text = str;*/
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (solution!=null)
            {
                if (solution.FinalPosition != null && this.path != null)
                {
                    if (index>0)
                    {
                        index--;
                        this.DrawStep();
                    }
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (solution != null)
            {
                if (solution.FinalPosition != null && this.path != null)
                {
                    if (index < this.path.Count - 1)
                    {
                        index++;
                        this.DrawStep();
                    }
                }
            }
        }

        private void DrawStep()
        {
            if (solution != null)
            {
                if (solution.FinalPosition != null && this.path != null)
                {
                    if (index >= 0 && index < this.path.Count)
                    {
                        this.Draw(canvas, this.path.ElementAt(index));
                        this.pushLabel.Content = string.Format("push: {0}", this.index);
                    }
                }
            }
        }

        private void Draw(Canvas canvas, SokobanPosition position)
        {
            canvas.Children.Clear();
            double dx = 20;
            double dy = 20;
            for(int x=0; x<position.Width; x++)
            {
                for(int y=0; y<position.Height; y++)
                {
                    double sx = x * dx;
                    double sy = y * dy;
                    switch (position.Map[x,y])
                    {
                        case Constants.WALL:
                            canvas.Children.Add(this.CreateFilledRectangle(sx, sy, dx, dy, blackBrush));
                            break;
                        case Constants.STONE:
                            canvas.Children.Add(this.CreateFilledRectangle(sx, sy, dx, dy, whiteBrush));
                            canvas.Children.Add(this.CreateFilledEllipse(sx, sy, dx, dy, greenBrush));
                            break;
                        case Constants.GOALSTONE:
                            canvas.Children.Add(this.CreateFilledRectangle(sx, sy, dx, dy, yellowBrush));
                            canvas.Children.Add(this.CreateFilledEllipse(sx, sy, dx, dy, greenBrush));
                            break;
                        case Constants.GOAL:
                            canvas.Children.Add(this.CreateFilledRectangle(sx, sy, dx, dy, yellowBrush));
                            break;
                        default:
                            canvas.Children.Add(this.CreateFilledRectangle(sx, sy, dx, dy, whiteBrush));
                            break;
                    }

                    if (x == position.Player.X && y == position.Player.Y)
                    {
                        canvas.Children.Add(this.CreatePlayer(sx, sy, dx, dy, blueBrush));
                    }

                    if (position.DeadlockMap[x, y])
                    {
                        canvas.Children.Add(this.CreateFilledRectangle(sx, sy, dx, dy, redBrush));
                    }
                }
            }
        }

        private Rectangle CreateFilledRectangle(double x, double y, double width, double height, Brush brush)
        {
            var rec = new Rectangle();
            rec.Width = width;
            rec.Height = height;
            rec.Fill = brush;
            rec.SetValue(Canvas.LeftProperty, x);
            rec.SetValue(Canvas.TopProperty, y);
            return rec;
        }

        private Ellipse CreateFilledEllipse(double x, double y, double width, double height, Brush brush)
        {
            var ellipse = new Ellipse();
            ellipse.Width = width;
            ellipse.Height = height;
            ellipse.Fill = brush;
            ellipse.SetValue(Canvas.LeftProperty, x);
            ellipse.SetValue(Canvas.TopProperty, y);
            return ellipse;
        }

        private Polygon CreatePlayer(double x, double y, double width, double height, Brush brush)
        {
            Polygon poly = new Polygon();
            poly.Width = width;
            poly.Height = height;
            poly.Fill = brush;
            poly.SetValue(Canvas.LeftProperty, x);
            poly.SetValue(Canvas.TopProperty, y);
            PointCollection polygonPoints = new PointCollection();
            polygonPoints.Add(new Point(0.1 * width, 0.9 * height));
            polygonPoints.Add(new Point(0.1 * width, 0.6 * height));
            polygonPoints.Add(new Point(0.4 * width, 0.6 * height));
            polygonPoints.Add(new Point(0.4 * width, 0.4 * height));
            polygonPoints.Add(new Point(0.1 * width, 0.4 * height));
            polygonPoints.Add(new Point(0.1 * width, 0.3 * height));
            polygonPoints.Add(new Point(0.4 * width, 0.3 * height));
            polygonPoints.Add(new Point(0.2 * width, 0.15 * height));
            polygonPoints.Add(new Point(0.5 * width, 0.0 * height));
            polygonPoints.Add(new Point(0.8 * width, 0.15 * height));
            polygonPoints.Add(new Point(0.6 * width, 0.3 * height));
            polygonPoints.Add(new Point(0.9 * width, 0.3 * height));
            polygonPoints.Add(new Point(0.9 * width, 0.4 * height));
            polygonPoints.Add(new Point(0.6 * width, 0.4 * height));
            polygonPoints.Add(new Point(0.6 * width, 0.6 * height));
            polygonPoints.Add(new Point(0.9 * width, 0.6 * height));
            polygonPoints.Add(new Point(0.9 * width, 0.9 * height));
            polygonPoints.Add(new Point(0.8 * width, 0.9 * height));
            polygonPoints.Add(new Point(0.8 * width, 0.7 * height));
            polygonPoints.Add(new Point(0.2 * width, 0.7 * height));
            polygonPoints.Add(new Point(0.2 * width, 0.9 * height));
            poly.Points = polygonPoints;
            return poly;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.index = 0;
            this.timer.Start();
        }
    }
}
