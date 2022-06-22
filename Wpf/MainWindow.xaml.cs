using HelperTools;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using WebSocketClient;
using System.Speech;
using System.Speech.Synthesis;
using System.Threading.Tasks;

namespace Wpf
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private static JObject ConFigObj;
        private string SocketUrl;
        static WSocketClient client;
        private bool processflg = false;
        private string rtsp=string.Empty;
        private string rtspIp=string.Empty;
        Process process1;

        private int processID;
        private int HiddenTimer = 0;
        private bool HiddenFlg = false;
        private string dir = AppDomain.CurrentDomain.BaseDirectory;
        private int VideoInterval = 50;
        private double containerwidht = 400;
        private double containerheight = 400;
        private double FontSizevalue = 150;
        private double distance = 100;
        delegate void MyDelegate(string content);
        SpeechSynthesizer voice = new SpeechSynthesizer();   //创建语音实例
        public MainWindow()
        {
            InitializeComponent();
            ImageBrush b = new ImageBrush();
            b.ImageSource = new BitmapImage(new Uri("pack://application:,,,/image/FaceBackgroup.png"));
            b.Stretch = Stretch.Fill;
            this.Background = b;
  
            this.Showpic.Source = new BitmapImage(new Uri("pack://application:,,,/image/IN.png"));            
            this.Loaded += LoadFrm;
        }
        private void LoadFrm(object sender, EventArgs args)
        {

            this.KeyDown += ModifyPrice_KeyDown;
            ConFigObj = ReadAppSet.Appsetings("setings.json");
            VideoInterval = (int)ConFigObj["VideoInterval"];
            containerwidht = (double)ConFigObj["containerwidth"];
            containerheight = (double)ConFigObj["containerheight"];
            FontSizevalue=(double)ConFigObj["FontSizevalue"];
            distance = (double)ConFigObj["Ddistance"];
            this.msgno.FontSize = VideoInterval;
            this.thiscoller.Height = containerheight;
            this.thiscoller.Width = containerwidht;
            this.Ddistance.Height = distance;
            SocketUrl = (string)ConFigObj["HomePage"];
            client = new WSocketClient(SocketUrl);
            client.Start((int)ConFigObj["SocketSleep"]);
            MessageReceived();

            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = (int)ConFigObj["JobSleep"];
            timer.Elapsed += RunPss;
            timer.Enabled = true;
            timer.Start();
            if ((bool)ConFigObj["IsFullScreen"] == true)
            {
                this.WindowState = WindowState.Maximized;
            }
            WbackGround();
            /*voice = new SpVoice();
            voice.Rate = -3;
            voice.Volume = 100;
            voice.Voice = voice.GetVoices().Item(0);*/
        }
        private void WbackGround()
        {
            ImageBrush ximgBrush = new ImageBrush();

            Uri xuri = new Uri($"{dir}/image/img@3x.png", UriKind.Absolute); //注意这个的写法      

            ximgBrush.ImageSource = new BitmapImage(xuri);

            thiscoller.Background = ximgBrush;
        }
        /// <summary>
        /// 语音播报，通过委托调用
        /// </summary>
        /// <param name="Str"></param>
        private  void Speek(string Str)
        {            
            voice.Rate = 0; //设置语速,[-10,10]
            voice.Volume = 100; //设置音量,[0,100]
            voice.Speak(Str);  //播放指定的字符串,这是同步朗读
        }

        private void RunPss(object sender, System.Timers.ElapsedEventArgs e)
        {
            long t = TimeSpans.Timestamp();
            string Times = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
            {
                this.Dispatcher.Invoke(()=> {
                    this.Timers.Text = Times;
                });
            }
            ));
           /* if (MsgInfo.Visibility == Visibility.Collapsed)
            {
               
                ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
                {
                    this.Dispatcher.Invoke(() => {
                        MsgInfo.Visibility = Visibility.Visible;
                        this.Showpic.Source = new BitmapImage(new Uri("pack://application:,,,/image/X@3x.png"));
                    });
                }
                ));
            }*/
            if (HiddenFlg)//收到屏显指令后3秒还原
            {
                if (HiddenTimer < 3)
                {
                    HiddenTimer++;
                }
                else
                {
                    HiddenTimer = 0;
                    HiddenFlg = false;
                    ShowStart();
                }
            }

            if (DateTime.Now.Hour == 4 && DateTime.Now.Minute == 1 && DateTime.Now.Second == 1)
            {
                LogHelper.WriteLog("restart 进程");
                Re_start();
            }
        }
        /// <summary>
        /// 自动重启
        /// </summary>

        private void Re_start()
        {
            try
            {
                Process p = Process.GetProcessById(processID);
                p.Kill();
            }
            catch (Exception ex)
            { }
            Process.Start($"{dir}\\VideoPro.exe");
            Process.GetCurrentProcess()?.Kill();

        }
        /// <summary>
        /// 还原进闸状态
        /// </summary>
        private void ShowStart()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
            {
                this.Dispatcher.Invoke(() => {
                    this.msgyes.Text = "";
                    this.MsgInfo.Visibility = Visibility.Collapsed;
                    this.Showpic.Source = new BitmapImage(new Uri("pack://application:,,,/image/IN.png"));
                    this.Showpic.Visibility = Visibility.Visible;
                });
            }
            ));

        }
        /// <summary>
        /// 服务端返回的消息
        /// </summary>
        private void MessageReceived()
        {
            //注册消息接收事件，接收服务端发送的数据
            client.MessageReceived += (data) => {
                try
                {

                    JObject O = JObject.Parse(data);
                    switch ((string)O["command"])
                    {
                        case "CONNECTING":
                            LogHelper.WriteLog("webcocket start oK");
                            break;
                        case "INIT DISPLAY":
                            rtsp = (string)O["message"]["videoAddr"];//"rtsp://192.168.0.143/av0_0&user=admin&password=admin"; //
                            string[] ad1 = rtsp.Split('/');
                            if (ad1.Length < 2)
                            {
                                LogHelper.ErrorLog("非法视频流地址");
                                rtsp = string.Empty;
                                return;
                            }
                            string[] ad2 = ad1[2].Split(':');
                            rtspIp = ad2[0];//视频流IP
                            LogHelper.WriteLog($"视频流IP:{rtspIp}");
                            
                            StringBuilder Sp = new StringBuilder();
                            Sp.Append("[Client]\n");
                            Sp.Append($"URL ={rtsp}\n");
                            Sp.Append("file_path =dump.mp4\n");
                            Sp.Append($"videowidth ={ConFigObj["widthproportion"]}\n");
                            Sp.Append($"videoheight ={ConFigObj["heightproportion"]}\n");
                            Sp.Append($"DistanceTop ={ConFigObj["DistanceTop"]}\n");//距离顶部增量
                            Sp.Append($"DistanceLeft ={ConFigObj["DistanceLeft"]}");//距离左边增量
                            System.IO.File.WriteAllText(@"Config.ini", Sp.ToString(), Encoding.Default);
                            string strCheck = $"{dir}\\StreamRecv.exe";
                            if (processflg == true)
                            {
                                try
                                {
                                    Process p = Process.GetProcessById(processID);
                                    p.Kill();
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.ErrorLog(ex);
                                }
                            }
                            processflg = true;
                            process1 = CreateProcessTasks(strCheck, string.Empty, true);
                            process1.Start();
                            processID = process1.Id;
                            #region
                            /*if (processflg == false)
                            {
                                //调用c++ 程序
                                processflg = true;
                                StringBuilder Sp = new StringBuilder();
                                Sp.Append("[Client]\n");
                                Sp.Append($"URL ={rtsp}\n");
                                Sp.Append("file_path =dump.mp4\n");
                                Sp.Append($"videowidth ={ConFigObj["widthproportion"]}\n");
                                Sp.Append($"videoheight ={ConFigObj["heightproportion"]}\n");
                                Sp.Append($"DistanceTop ={ConFigObj["DistanceTop"]}\n");//距离顶部增量
                                Sp.Append($"DistanceLeft ={ConFigObj["DistanceLeft"]}");//距离左边增量
                                System.IO.File.WriteAllText(@"Config.ini", Sp.ToString(), Encoding.Default);
                                //string strCheck = $"{dir}\\StreamRecv.exe";
                                processflg = true;
                                process1 = CreateProcessTasks(strCheck, string.Empty, true);
                                process1.Start();
                                processID = process1.Id;
                            }
                            else {
                              
                            }*/
                            #endregion
                            break;
                        case "CONTENT DISPLAY":
                            if (!Convert.IsDBNull(O["message"]))
                            {
                                JObject Obj = (JObject)O["message"];
                                switch ((string)Obj["accessResult"])
                                {
                                    case "PERMISSION:DISPATCHING":
                                        HiddenTimer = 1;
                                        HiddenFlg = true;
                                        ShowOkElevator((string)Obj["extras"]["floorName"] + "派梯成功", (string)Obj["accessDesc"]);

                                        break;
                                    case "PERMISSION:ACCESS":
                                        HiddenTimer = 1;
                                        HiddenFlg = true;
                                        ShowOkRight((string)Obj["accessDesc"]);

                                        break;
                                    case "PERMISSION:DENIED":
                                        HiddenTimer = 1;
                                        HiddenFlg = true;
                                        ShowNoRight((string)Obj["accessDesc"]);

                                        break;
                                }
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.ErrorLog(ex);
                    LogHelper.ErrorLog(data.ToString());
                }
            };

        }
        /// <summary>
        /// elevator
        /// </summary>
        private void ShowOkElevator(string Msg, string Ele)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
            {
                this.Dispatcher.Invoke(() => {
                    try
                    {
                        this.msgyes.Text = Msg;
                        this.Showpic.Visibility = Visibility.Collapsed;
                        this.MsgInfo.FontSize = FontSizevalue;
                        this.MsgInfo.Text = Ele;
                        this.MsgInfo.Visibility = Visibility.Visible;
                        MyDelegate myDelegate = new MyDelegate(Speek); //异步调用委托 
                        myDelegate.BeginInvoke($"{Msg}请乘坐{Ele}梯", new AsyncCallback(SpeekCompleted), null); //在启动异步线程后，主线程可以继续工作而不需要等待
                        
                    }
                    catch (Exception ex)
                    {
                        LogHelper.ErrorLog($"ShowOkElevator :{ex.Message}");
                    }
                });
            }
            ));            
        }
        private async void SpeekCompleted(IAsyncResult result)
        {
            voice.SpeakAsyncCancelAll();
            //voice.Dispose();  //释放所有语音资源
        }
        /// <summary>
        /// NO right
        /// </summary>
        private void ShowNoRight(string Msg)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
            {
                this.Dispatcher.Invoke(() => {
                    try
                    {
                        this.msgyes.Text = "无通行权限";
                        this.MsgInfo.Visibility = Visibility.Collapsed;
                        this.Showpic.Source = new BitmapImage(new Uri("pack://application:,,,/image/X@3x.png"));
                        this.Showpic.Visibility = Visibility.Visible;
                        MyDelegate myDelegate = new MyDelegate(Speek); //异步调用委托 
                        myDelegate.BeginInvoke($"无通行权限", new AsyncCallback(SpeekCompleted), null); //在启动异步线程后，主线程可以继续工作而不需要等待
                    }
                    catch (Exception ex)
                    {
                        LogHelper.ErrorLog($"ShowNoRight :{ex.Message}");
                    }
                });
            }
            ));
        }
        /// <summary>
        /// right true
        /// </summary>
        private void ShowOkRight(string Msg)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
            {
                this.Dispatcher.Invoke(() => {
                    try
                    {
                        this.Showpic.Visibility =Visibility.Collapsed ;
                        int len = Msg.Length;
                        if (len <= 2)//离开
                        {
                            this.MsgInfo.FontSize = 80;
                            this.MsgInfo.Text = "再见";
                            this.MsgInfo.Visibility = Visibility.Visible;
                            MyDelegate myDelegate = new MyDelegate(Speek); //异步调用委托 
                            myDelegate.BeginInvoke($"再见", new AsyncCallback(SpeekCompleted), null); //在启动异步线程后，主线程可以继续工作而不需要等待
                        }
                        else if (len > 2)
                        {
                            this.MsgInfo.FontSize = 50;
                            this.MsgInfo.Text = "欢迎光临";
                            this.MsgInfo.Visibility = Visibility.Visible;
                            MyDelegate myDelegate = new MyDelegate(Speek); //异步调用委托 
                            myDelegate.BeginInvoke($"欢迎光临", new AsyncCallback(SpeekCompleted), null); //在启动异步线程后，主线程可以继续工作而不需要等待
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.ErrorLog($"ShowOkRight :{ex.Message}");
                    }
                });
            }
            ));
        }
        private Process CreateProcessTasks(string exe_path, string str_arguments, bool b_create_win = true)
        {
            Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = exe_path;
            process.StartInfo.Arguments = str_arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = false;  //true
            process.StartInfo.RedirectStandardOutput = false;  //true
            process.StartInfo.RedirectStandardError = false;
            process.StartInfo.CreateNoWindow = b_create_win;
            return process;
        }
        private void ModifyPrice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.ToString() == "Escape")//Esc键  
            {
                try
                {
                    Process p = Process.GetProcessById(processID);
                    p.Kill();
                }
                catch (Exception ex)
                { }
                System.Environment.Exit(0);
            }
        }
    }
}
