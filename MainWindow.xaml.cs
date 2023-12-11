using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

//using System.Drawing;

namespace Paratrooper2049
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region INI

        private const int MaxLength = 255;
        private string IniFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "paratrooper.ini");

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder value, int size, string filePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool WritePrivateProfileString(string section, string key, string value, string filePath);

        public static string ReadValue(string section, string key, string defaultValue, string filePath)
        {
            var value = new StringBuilder(MaxLength);
            GetPrivateProfileString(section, key, defaultValue, value, MaxLength, filePath);
            return value.ToString();
        }

        public static void WriteValue(string section, string key, string value, string filePath)
        {
            WritePrivateProfileString(section, key, value, filePath);
        }

        #endregion INI

        private double FormW, FormH, CenterCannonX, CenterCannonY;
        private Point CenterCannon;
        private Rectangle CannonPart1, CannonPart2, CannonPart3;
        private Rectangle Sky, TopGround, BottomGround;
        private Ellipse CannonJoint;
        private Label PointLabel, WeaponLabel;
        private Grid GroundGrid;

        private int cycleTime = 30;
        private DispatcherTimer GameTimer;
        private Random Rand = new Random();

        private Queue<Weapon> MainWeapons = new Queue<Weapon>();
        private Queue<Target> MainTargets = new Queue<Target>();

        private DateTime LastBulletTime, LastLaserTime, LastHeliTime, LastParaTime, LastGiftTime, LastClusterTime;

        private bool MoveLeft = false, MoveRight = false, Fire = false, GamePause = false, FirstRun = true;
        private double CannonAngle;
        private int WeaponSelect, Life, Score;
        private int BulletStep, LaserStep, ClusteStep, HeliStep, ParaStep, GiftStep, Frequency;

        private string[] ActiveWeapon = { "Machinegun", "Laser", "Cluster bomb" };

        public MainWindow()
        {
            InitializeComponent();

            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;

            bool windowState = bool.Parse(ReadValue("Form", "window_maximized", "false", IniFilePath));
            double windowWidth = double.Parse(ReadValue("Form", "window_width", "800", IniFilePath));
            double windowHeight = double.Parse(ReadValue("Form", "window_height", "600", IniFilePath));
            string gameLevel = ReadValue("Game", "game_level", "Easy", IniFilePath);

            if (windowState)
            {
                SettingLabel.Content = "FullScreen";
                formWi.Visibility = Visibility.Hidden;
                formHe.Visibility = Visibility.Hidden;
            }

            formWi.Text = windowWidth.ToString();
            formHe.Text = windowHeight.ToString();
            LevelLabel.Content = gameLevel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MyCanvas.Focus();
            ////-------------------
            ////da eliminare per avere finestra setup
            //WorldGenerator();

            //GameTimer = new DispatcherTimer();
            //GameTimer.Interval = TimeSpan.FromMilliseconds(cycleTime);
            //GameTimer.Tick += GameLoop;
            //GameTimer.Start();
            //FirstRun = false;
            ////-------------------
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (!GamePause)
            {
                //Debug.Print($"canvas objects: {MyCanvas.Children.Count.ToString()}, mainWeapons objects: {mainWeapons.Count}, maintargets objects: {mainTargets.Count}");

                if (Life < 4)
                {
                    PlayerManager();
                    CollisionManager();
                    ObjectCreator();
                }
                else
                {
                }

                ObjectManager();
            }
        }

        private void WorldGenerator()
        {
            int GroundH = 40; //not less 30

            FormW = MyCanvas.ActualWidth;
            FormH = MyCanvas.ActualHeight;

            CenterCannonX = FormW / 2;
            CenterCannonY = FormH - GroundH - 40;

            CenterCannon = new Point(CenterCannonX, CenterCannonY);

            MainTargets.Clear();
            MainWeapons.Clear();
            MyCanvas.Children.Clear();

            Sky = new Rectangle() { Width = FormW, Height = FormH, RadiusX = 5, RadiusY = 5, Fill = Brushes.LightGray };
            Canvas.SetLeft(Sky, 0);
            Canvas.SetBottom(Sky, 0);
            MyCanvas.Children.Add(Sky);

            CannonPart1 = new Rectangle() { Width = 20, Height = 40, RadiusX = 3, RadiusY = 3, Fill = Brushes.Blue, RenderTransformOrigin = new Point(0.5, 0.875) };
            Canvas.SetLeft(CannonPart1, FormW / 2 - CannonPart1.Width / 2);
            Canvas.SetBottom(CannonPart1, GroundH + 35);
            MyCanvas.Children.Add(CannonPart1);

            CannonPart2 = new Rectangle() { Width = 30, Height = 55 + GroundH, RadiusX = 15, RadiusY = 15, Fill = Brushes.Gray, StrokeThickness = 0, Stroke = Brushes.Red };
            Canvas.SetLeft(CannonPart2, FormW / 2 - CannonPart2.Width / 2);
            Canvas.SetBottom(CannonPart2, 0);
            MyCanvas.Children.Add(CannonPart2);

            CannonPart3 = new Rectangle() { Width = 60, Height = GroundH + 25, RadiusX = 5, RadiusY = 5, Fill = Brushes.DarkSlateGray, StrokeThickness = 0, Stroke = Brushes.Red };
            Canvas.SetLeft(CannonPart3, FormW / 2 - CannonPart3.Width / 2);
            Canvas.SetBottom(CannonPart3, 0);
            MyCanvas.Children.Add(CannonPart3);

            CannonJoint = new Ellipse() { Width = 6, Height = 6, Fill = Brushes.Black };
            Canvas.SetLeft(CannonJoint, CenterCannonX - 3);
            Canvas.SetTop(CannonJoint, CenterCannonY - 3);
            MyCanvas.Children.Add(CannonJoint);

            GroundGrid = new Grid() { Width = FormW, Height = GroundH, Background = Brushes.Transparent };
            Canvas.SetLeft(GroundGrid, 0);
            Canvas.SetBottom(GroundGrid, 0);
            MyCanvas.Children.Add(GroundGrid);

            BottomGround = new Rectangle() { Width = FormW, Height = GroundH - 10, RadiusX = 5, RadiusY = 5, VerticalAlignment = VerticalAlignment.Bottom, Fill = Brushes.LightCoral };
            GroundGrid.Children.Add(BottomGround);

            TopGround = new Rectangle() { Width = FormW, Height = 30, VerticalAlignment = VerticalAlignment.Top, Fill = Brushes.LightCoral };
            GroundGrid.Children.Add(TopGround);

            WeaponLabel = new Label() { Content = ActiveWeapon[0], FontSize = 20, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(30, 0, 0, 0) };
            GroundGrid.Children.Add(WeaponLabel);

            PointLabel = new Label() { Content = "0", FontSize = 20, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            GroundGrid.Children.Add(PointLabel);

            LastBulletTime = DateTime.Now; LastLaserTime = DateTime.Now; LastClusterTime = DateTime.Now;
            LastHeliTime = DateTime.Now; LastParaTime = DateTime.Now; LastGiftTime = DateTime.Now;

            CannonAngle = 0; WeaponSelect = 0;
            Life = 0; Score = 0;
            PointLabel.Content = Score.ToString();

            if (LevelLabel.Content.ToString() == "Easy")
            {
                BulletStep = 60; LaserStep = 2500; ClusteStep = 10000;
                HeliStep = 1000; ParaStep = 2000; GiftStep = 10000;
                Frequency = 20;
            }

            if (LevelLabel.Content.ToString() == "Medium")
            {
                BulletStep = 80; LaserStep = 3500; ClusteStep = 12000;
                HeliStep = 800; ParaStep = 1600; GiftStep = 12000;
                Frequency = 18;
            }

            if (LevelLabel.Content.ToString() == "Hard")
            {
                BulletStep = 100; LaserStep = 4500; ClusteStep = 15000;
                HeliStep = 500; ParaStep = 1000; GiftStep = 15000;
                Frequency = 15;
            }

            MyCanvas.Focus();
        }

        private void PlayerManager()
        {
            switch (WeaponSelect)
            {
                case 0:

                    TimeSpan timeSinceLastBullet = DateTime.Now - LastBulletTime;

                    if (timeSinceLastBullet.TotalMilliseconds >= BulletStep) CannonJoint.Fill = Brushes.DarkGreen;
                    else CannonJoint.Fill = Brushes.DarkRed;

                    break;

                case 1:
                    TimeSpan timeSinceLastLaser = DateTime.Now - LastLaserTime;

                    if (timeSinceLastLaser.TotalMilliseconds >= LaserStep) CannonJoint.Fill = Brushes.DarkGreen;
                    else CannonJoint.Fill = Brushes.DarkRed;

                    break;

                case 2:
                    TimeSpan timeSinceLastCluster = DateTime.Now - LastClusterTime;

                    if (timeSinceLastCluster.TotalMilliseconds >= ClusteStep) CannonJoint.Fill = Brushes.DarkGreen;
                    else CannonJoint.Fill = Brushes.DarkRed;

                    break;
            }

            if (MoveLeft)
            {
                //rotate left
                if (CannonAngle >= -88) CannonAngle = CannonAngle - 2;
                CannonPart1.RenderTransform = new RotateTransform(CannonAngle);
            }

            if (MoveRight)
            {
                //rotate right
                if (CannonAngle <= 88) CannonAngle = CannonAngle + 2;
                CannonPart1.RenderTransform = new RotateTransform(CannonAngle);
            }

            if (Fire)
            {
                switch (WeaponSelect)
                {
                    case 0:

                        TimeSpan timeSinceLastBullet = DateTime.Now - LastBulletTime;

                        if (timeSinceLastBullet.TotalMilliseconds >= BulletStep)
                        {
                            MainWeapons.Enqueue(new Weapon(CenterCannon, CannonAngle / 180 * Math.PI, FormW, FormH));

                            LastBulletTime = DateTime.Now;
                            if (Score > 0) Score--;
                        }
                        break;

                    case 1:
                        TimeSpan timeSinceLastLaser = DateTime.Now - LastLaserTime;

                        if (timeSinceLastLaser.TotalMilliseconds >= LaserStep)
                        {
                            MainWeapons.Enqueue(new Weapon(CenterCannon, CannonAngle / 180 * Math.PI));

                            LastLaserTime = DateTime.Now;
                            if (Score > 0) Math.Min(Score -= 10, 0);
                        }
                        break;

                    case 2:
                        TimeSpan timeSinceLastCluster = DateTime.Now - LastClusterTime;

                        if (timeSinceLastCluster.TotalMilliseconds >= ClusteStep)
                        {
                            MainWeapons.Enqueue(new Weapon(CenterCannon, CannonAngle / 180 * Math.PI, FormW, FormH, true));

                            LastClusterTime = DateTime.Now;
                            if (Score > 0) Math.Min(Score -= 30, 0);
                        }
                        break;
                }
            }
        }

        private void CollisionManager()
        {
            foreach (Target x in MainTargets)
            {
                foreach (Weapon y in MainWeapons)
                {
                    if (BaseObject.IntersectObject(x.Vertices, y.Vertices))
                    {
                        x.LastFrame = true;
                        if (y.Description == BaseObject.gameObject.Bullet) y.LastFrame = true;
                        if (x.Description == BaseObject.gameObject.Gift && Life > 0) Life--;
                        //Debug.Print($" {x.ObjectRect.TopLeft}, {x.ObjectRect.TopRight}, {x.ObjectRect.BottomLeft}, {x.ObjectRect.BottomRight}");

                        Score += 10;
                    }
                }
            }
        }

        private void ObjectCreator()
        {
            //insert of enemy
            int casual = Rand.Next(0, Frequency);

            // helicopter
            TimeSpan timeSinceLastHeli = DateTime.Now - LastHeliTime;

            if (casual == 0 && timeSinceLastHeli.TotalMilliseconds > HeliStep)
            {
                LastHeliTime = DateTime.Now;
                MainTargets.Enqueue(new Target(-Math.PI / 2, FormW, FormH));
            }

            if (casual == 1 && timeSinceLastHeli.TotalMilliseconds > HeliStep)
            {
                LastHeliTime = DateTime.Now;
                MainTargets.Enqueue(new Target(Math.PI / 2, FormW, FormH));
            }

            //paratrooper
            TimeSpan timeSinceLastPara = DateTime.Now - LastParaTime;
            if (casual == 2 && timeSinceLastPara.TotalMilliseconds > ParaStep)
            {
                foreach (Target x in MainTargets)
                {
                    if (x.Description == BaseObject.gameObject.Heli)
                    {
                        if (x.ReleaseHit)
                        {
                            LastParaTime = DateTime.Now;
                            MainTargets.Enqueue(new Target(x.Vertices, FormW, FormH));
                            break;
                        }
                    }
                }
            }

            //gift
            TimeSpan timeSinceLastGift = DateTime.Now - LastGiftTime;
            if (casual == 3 && timeSinceLastGift.TotalMilliseconds > GiftStep)
            {
                LastGiftTime = DateTime.Now;
                MainTargets.Enqueue(new Target(FormW, FormH));
            }
        }

        private void ObjectManager()
        {
            PointLabel.Content = Score.ToString();
            if (CannonPart3.StrokeThickness < (Life * 8))
            {
                CannonPart3.StrokeThickness += 0.05;
                CannonPart2.StrokeThickness += 0.02;
            }
            if (CannonPart3.StrokeThickness > (Life * 8))
            {
                CannonPart3.StrokeThickness -= 0.05;
                CannonPart2.StrokeThickness -= 0.02;
            }

            if (Life == 4)
                CannonPart3.Fill = Brushes.Red;

            //delete game objects
            for (int i = MyCanvas.Children.Count - 1; i >= 6; i--)
            {
                MyCanvas.Children.RemoveAt(i);
            }

            //manage Weapons
            for (int i = 0; i < MainWeapons.Count; i++)
            {
                Weapon newWeapon = MainWeapons.Dequeue();
                if (!newWeapon.LastFrame)
                {
                    if (newWeapon.Description == Weapon.gameObject.Bullet)
                    {
                        Polygon graphicWeapon = new Polygon();

                        graphicWeapon.Fill = Brushes.Red;
                        graphicWeapon.Points = newWeapon.Vertices;
                        graphicWeapon.StrokeLineJoin = PenLineJoin.Round;
                        graphicWeapon.StrokeStartLineCap = PenLineCap.Round;
                        graphicWeapon.StrokeEndLineCap = PenLineCap.Round;
                        graphicWeapon.StrokeThickness = 2;
                        graphicWeapon.Stroke = Brushes.Red;

                        MyCanvas.Children.Add(graphicWeapon);

                        newWeapon.UpdatePosition();

                        MainWeapons.Enqueue(newWeapon);
                    }
                    else if (newWeapon.Description == Weapon.gameObject.Laser)
                    {
                        Polygon graphicWeapon = new Polygon();

                        graphicWeapon.Fill = new SolidColorBrush(Color.FromArgb((byte)newWeapon.Speed, 255, 255, 255));

                        graphicWeapon.Points = newWeapon.Vertices;

                        MyCanvas.Children.Add(graphicWeapon);

                        newWeapon.UpdatePosition();

                        MainWeapons.Enqueue(newWeapon);
                    }
                    else if (newWeapon.Description == Weapon.gameObject.Cluster)
                    {
                        Polygon graphicWeapon = new Polygon();

                        graphicWeapon.Fill = Brushes.Black;
                        graphicWeapon.Points = newWeapon.Vertices;
                        graphicWeapon.StrokeLineJoin = PenLineJoin.Round;
                        graphicWeapon.StrokeStartLineCap = PenLineCap.Round;
                        graphicWeapon.StrokeEndLineCap = PenLineCap.Round;
                        graphicWeapon.StrokeThickness = 2;
                        graphicWeapon.Stroke = Brushes.Red;

                        MyCanvas.Children.Add(graphicWeapon);

                        newWeapon.UpdatePosition();
                        MainWeapons.Enqueue(newWeapon);

                        if (newWeapon.LastFrame)
                        {
                            for (double ii = 0; ii < 360; ii = ii + 5)
                            {
                                //Debug.Print(ii.ToString());
                                MainWeapons.Enqueue(new Weapon(newWeapon.Vertices[(int)ii % 3], ii / 180 * Math.PI, FormW, FormH));
                            }
                        }
                    }
                }
            }

            //manage target
            for (int i = 0; i < MainTargets.Count; i++)
            {
                Target newTarget = MainTargets.Dequeue();
                if (!newTarget.LastFrame)
                {
                    Polygon graphicTarget = new Polygon();

                    graphicTarget.Fill = Brushes.Black;
                    if (newTarget.Description == Weapon.gameObject.Heli) graphicTarget.Fill = new ImageBrush(newTarget.ObjectBitmap);
                    if (newTarget.Description == Weapon.gameObject.Gift) graphicTarget.Fill = Brushes.Blue;
                    graphicTarget.Points = newTarget.Vertices;
                    //if (newTarget.ReleasePara) graphicTarget.Fill = Brushes.Green;

                    MyCanvas.Children.Add(graphicTarget);

                    newTarget.UpdatePosition();

                    MainTargets.Enqueue(newTarget);
                }
                else
                {
                    if (newTarget.ReleaseHit && newTarget.Description == BaseObject.gameObject.Para) Life++;
                }
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    MoveLeft = true;
                    break;

                case Key.Right:
                    //rotate right
                    MoveRight = true;
                    break;

                case Key.Space:
                    //change weapon
                    if (!FirstRun)
                    {
                        WeaponSelect++;
                        if (WeaponSelect >= 3) WeaponSelect = 0;
                        WeaponLabel.Content = ActiveWeapon[WeaponSelect];
                    }
                    break;

                case Key.Up:
                    //fire
                    Fire = true;
                    break;

                case Key.Escape:

                    if (FirstRun) this.Close();
                    else
                    {
                        ProcessStartInfo processStartInfo = new ProcessStartInfo(); 
                        processStartInfo.UseShellExecute = false;
                        processStartInfo.FileName = "Paratrooper2049.exe";
                        Process process= Process.Start(processStartInfo);

                        this.Close(); 
                    }

                    break;

                case Key.P:
                    GamePause = !GamePause;
                    break;

                case Key.S:

                    if (FirstRun)
                    {
                        WriteValue("Form", "window_width", formWi.Text, IniFilePath);
                        WriteValue("Form", "window_height", formHe.Text, IniFilePath);
                        WriteValue("Form", "window_maximized", SettingLabel.Content.ToString().Contains("Full").ToString(), IniFilePath);
                        WriteValue("Game", "game_level", LevelLabel.Content.ToString(), IniFilePath);

                        if (SettingLabel.Content.ToString() == "Windowed")
                        {
                            MyWindow.Width = int.Parse(formWi.Text);
                            MyWindow.Height = int.Parse(formHe.Text);

                            // Calculate the center position
                            int screenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
                            int screenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

                            int centerX = (screenWidth - (int)MyWindow.Width) / 2;
                            int centerY = (screenHeight - (int)MyWindow.Height) / 2;

                            MyWindow.Left = centerX;
                            MyWindow.Top = centerY;
                        }
                        else MyWindow.WindowState = WindowState.Maximized;

                        WorldGenerator();

                        GameTimer = new DispatcherTimer();
                        GameTimer.Interval = TimeSpan.FromMilliseconds(cycleTime);
                        GameTimer.Tick += GameLoop;
                        GameTimer.Start();
                        FirstRun = false;
                    }
                    else WorldGenerator();

                    break;

                case Key.A:
                    CannonPart1.RenderTransform = new RotateTransform(90); ; break;

                case Key.Z:
                    CannonPart1.RenderTransform = new RotateTransform(-90); ; break;
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    MoveLeft = false;
                    break;

                case Key.Right:
                    //rotate right
                    MoveRight = false;
                    break;

                case Key.Up:
                    //fire
                    Fire = false;
                    break;
            }
        }

        private void label_MouseEnter(object sender, MouseEventArgs e)
        {
            Label current = sender as Label;
            current.Foreground = Brushes.DarkRed;
        }

        private void label_MouseLeave(object sender, MouseEventArgs e)
        {
            Label current = sender as Label;
            current.Foreground = Brushes.Black;
        }

        private void SettingLabel_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            Label current = sender as Label;

            if (current.Content.ToString() == "Windowed")
            {
                current.Content = "FullScreen";
                formWi.Visibility = Visibility.Hidden;
                formHe.Visibility = Visibility.Hidden;
            }
            else
            {
                current.Content = "Windowed";
                formWi.Visibility = Visibility.Visible;
                formHe.Visibility = Visibility.Visible;
            }
        }

        private void LevelLabel_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            Label current = sender as Label;

            if (current.Content.ToString() == "Easy") current.Content = "Medium";
            else if (current.Content.ToString() == "Medium") current.Content = "Hard";
            else current.Content = "Easy";
        }

        /// <summary>
        /// A timer that will fire an action at a regular interval. The timer will aline itself.
        /// </summary>
        /// <param name="action">The action to run Asyncrinasally</param>
        /// <param name="interval">The interval to fire at.</param>
        /// <param name="ct">(optional)A CancellationToken to cancel.</param>
        /// <returns>The Task.</returns>
        public static async Task PrecisionRepeatActionOnIntervalAsync(Action action, TimeSpan interval, CancellationToken? ct = null)
        {
            long stage1Delay = 16;
            long stage2Delay = 8 * TimeSpan.TicksPerMillisecond;
            bool USE_SLEEP0 = false;

            DateTime target = DateTime.Now + new TimeSpan(0, 0, 0, 0, (int)stage1Delay + 2);
            bool warmup = true;
            while (true)
            {
                // Getting closer to 'target' - Lets do the less precise but least cpu intensive wait
                var timeLeft = target - DateTime.Now;
                if (timeLeft.TotalMilliseconds >= stage1Delay)
                {
                    try
                    {
                        await Task.Delay((int)(timeLeft.TotalMilliseconds - stage1Delay), ct ?? CancellationToken.None);
                    }
                    catch (TaskCanceledException) when (ct != null)
                    {
                        return;
                    }
                }

                // Getting closer to 'target' - Lets do the semi-precise but mild cpu intesive wait - Task.Yield()
                while (DateTime.Now < target - new TimeSpan(stage2Delay))
                {
                    await Task.Yield();
                }

                // Getting closer to 'target' - Lets do the semi-precise but mild cpu intesive wait - Thread.Sleep(0)
                // Note: Thread.Sleep(0) is removed below because it is sometimes looked down on and also said not good to mix 'Thread.Sleep(0)' with Tasks.
                //       However, Thread.Sleep(0) does have a quicker and more reliable turn around time then Task.Yield() so to
                //       make up for this a longer (and more expensive) Thread.SpinWait(1) would be needed.
                if (USE_SLEEP0)
                {
                    while (DateTime.Now < target - new TimeSpan(stage2Delay / 8))
                    {
                        Thread.Sleep(0);
                    }
                }

                // Extreamlly close to 'target' - Lets do the most precise but very cpu/battery intensive
                while (DateTime.Now < target)
                {
                    Thread.SpinWait(64);
                }

                if (!warmup)
                {
                    await Task.Run(action); // or your code here
                    target += interval;
                }
                else
                {
                    long start1 = DateTime.Now.Ticks + ((long)interval.TotalMilliseconds * TimeSpan.TicksPerMillisecond);
                    long alignVal = start1 - (start1 % ((long)interval.TotalMilliseconds * TimeSpan.TicksPerMillisecond));
                    target = new DateTime(alignVal);
                    warmup = false;
                }
            }
        }

        /// <summary>
        /// A timer that will fire an action at a regular interval. The timer will aline itself.
        /// </summary>
        /// <param name="action">The action to run Asyncrinasally</param>
        /// <param name="interval">The interval to fire at.</param>
        /// <param name="ct">(optional)A CancellationToken to cancel.</param>
        /// <returns>The Task.</returns>
        public static async Task PrecisionRepeatActionOnIntervalAsync_DEBUG(Action task, TimeSpan interval, CancellationToken? ct = null)
        {
            StringBuilder log = new StringBuilder();

            const long taskDelayDelay = 16;
            const long taskYieldDelay = 8 * TimeSpan.TicksPerMillisecond;
            bool USE_SLEEP0 = true;

            int loops = 0;
            DateTime target = DateTime.Now + new TimeSpan(0, 0, 0, 0, (int)taskDelayDelay + 2);
            bool warmup = true;
            int misses = 0;
            while (true)
            {
                // Getting closer to 'target' - Lets do the less precise but least cpu intensive wait
                bool taskDelayed = false;
                var timeLeft = target - DateTime.Now;
                if (timeLeft.TotalMilliseconds >= taskDelayDelay)
                {
                    taskDelayed = true;
                    try
                    {
                        await Task.Delay((int)(timeLeft.TotalMilliseconds - taskDelayDelay), ct ?? CancellationToken.None);
                    }
                    catch (TaskCanceledException) when (ct != null)
                    {
                        Console.WriteLine(log);
                        Console.WriteLine("misses: " + misses);
                        return;
                    }
                }

                // Getting closer to 'target' - Lets do the semi-precise but mild cpu intensive wait - Task.Yield()
                int YieldCount = 0;
                while (DateTime.Now < target - new TimeSpan(taskYieldDelay))
                {
                    YieldCount++;
                    await Task.Yield();
                }

                // Getting closer to 'target' - Lets do the semi-precise but mild cpu intensive wait - Thread.Sleep(0)
                // Note: Thread.Sleep(0) is removed below because it is sometimes looked down on and also said not good to mix 'Thread.Sleep(0)' with Tasks.
                //       However, Thread.Sleep(0) does have a quicker and more reliable turn around time then Task.Yield() so to
                //       make up for this a longer (and more expensive) Thread.SpinWait(1) would be needed.
                int sleep0Count = 0;
                if (USE_SLEEP0)
                    while (DateTime.Now < target - new TimeSpan(taskYieldDelay / 8))
                    {
                        sleep0Count++;
                        Thread.Sleep(0);
                    }

                // Extreamlly close to 'target' - Lets do the most precise but very cpu/battery intensive
                int spinCount = 0;
                while (DateTime.Now < target)
                {
                    spinCount++;
                    Thread.SpinWait(64);
                }

                DateTime finish = DateTime.Now;

                if (finish.Subtract(target).Ticks >= (new TimeSpan(0, 0, 0, 0, 1)).Ticks)
                    misses++;

                if (!warmup)
                {
                    await Task.Run(task); // or your code here
                    log.AppendLine("| " + loops + "\t| " + target.ToString("ss.ffffff") + "\t| "
                        + finish.Subtract(target).ToString(@"s\.fffffff") + (taskDelayed ? "\t| Delayed" : "\t| Skipped")
                        + "\t| " + YieldCount + "\t| " + sleep0Count + "\t| " + spinCount + "\t|");
                    target += interval;
                }
                else
                {
                    long start1 = DateTime.Now.Ticks + ((long)interval.TotalMilliseconds * TimeSpan.TicksPerMillisecond);
                    long alignVal = start1 - (start1 % ((long)interval.TotalMilliseconds * TimeSpan.TicksPerMillisecond));
                    target = new DateTime(alignVal);
                    warmup = false;
                    log.AppendLine("|     \t| Actual     \t|        \t| Task.\t| Yield\t| Sleep\t| Spin  |");
                    log.AppendLine("|loop#\t| Target Time\t| Late by\t| Yield\t| Count\t| Count\t| Count |");
                    log.AppendLine("|-------|---------------|---------------|-------|-------|-------|-------|");
                }
                loops++;
            }
        }
    }
}