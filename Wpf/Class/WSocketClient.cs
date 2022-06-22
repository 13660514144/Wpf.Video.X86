
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HelperTools;
using SuperSocket.ClientEngine;
using WebSocket4Net;

namespace WebSocketClient
{
   public class WSocketClient
    {        

        #region 向外传递数据事件
        public event Action<string> MessageReceived;
        #endregion        
        WebSocket4Net.WebSocket _webSocket;
        /// <summary>
        /// 检查重连线程
        /// </summary>
        Thread _thread;
        bool _isRunning = true;
        /// <summary>
        /// WebSocket连接地址
        /// </summary>
        public static string ServerPath { get; set; } 

        public WSocketClient(string url)
        {
            ServerPath = url;
            this._webSocket = new WebSocket4Net.WebSocket(ServerPath);
            this._webSocket.Opened += WebSocket_Opened;
            this._webSocket.Error += WebSocket_Error;
            this._webSocket.Closed += WebSocket_Closed;
            this._webSocket.MessageReceived += WebSocket_MessageReceived;
        }
       

        #region "web socket "
        /// <summary>
        /// 连接方法
        /// <returns></returns>
        public bool Start(int sleep) 
        {
            bool result = true;
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = sleep;
            timer.Elapsed += RConnection;
            timer.Enabled = true;
            timer.Start();
            try 
            {
                this._webSocket.Open();
                
            }
            catch (Exception ex) 
            {                
                LogHelper.ErrorLog(ex);
                result = false;
            }
            return result;
        }
        /// <summary>
        /// 消息收到事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WebSocket_MessageReceived(object sender, MessageReceivedEventArgs e) 
        {
            try
            {
                MessageReceived?.Invoke(e.Message);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog(ex);
            }
        }
        /// <summary>
        /// Socket关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WebSocket_Closed(object sender, EventArgs e) 
        {
            try
            {
                LogHelper.WriteLog($"Socket关闭事件");
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog($"Socket关闭事件 err{ex.Message}");
            }
        }
        /// <summary>
        /// Socket报错事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WebSocket_Error(object sender, ErrorEventArgs e) 
        {
            try
            {
                LogHelper.ErrorLog(e.Exception);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog($"WebSocket_Error{ex.Message}");
            }
        }
        /// <summary>
        /// Socket打开事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WebSocket_Opened(object sender, EventArgs e) 
        {
            try
            {
                LogHelper.WriteLog("websocket_Opened");
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog($"websocket_Opened err {ex.Message}");
            }
        }
        private void RConnection(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (this._webSocket.State != WebSocket4Net.WebSocketState.Open &&
                    this._webSocket.State != WebSocket4Net.WebSocketState.Connecting)
                {
                    this._webSocket.Close();
                    this._webSocket.Open();
                    LogHelper.WriteLog("正在重连.....");
                }
           
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog(ex);
            }
        }
        /// <summary>
        /// 检查重连线程
        /// </summary>
        private void CheckConnection()
        {
            do
            {
                try
                {
                    if (this._webSocket.State != WebSocket4Net.WebSocketState.Open && 
                        this._webSocket.State != WebSocket4Net.WebSocketState.Connecting)
                    {                        
                        this._webSocket.Close();
                        this._webSocket.Open();
                        LogHelper.WriteLog("正在重连.....");
                    }
                }
                catch (Exception ex)
                {                    
                    LogHelper.ErrorLog(ex);
                }
                System.Threading.Thread.Sleep(10000);
            } while (this._isRunning);
        }
        #endregion

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="Message"></param>
        public void SendMessage(string Message)
        {
            try
            {
                Task.Factory.StartNew(() =>
                {
                    if (_webSocket != null && _webSocket.State == WebSocket4Net.WebSocketState.Open)
                    {
                        this._webSocket.Send(Message);
                    }
                });
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog(ex);
            }
        }
        
        
        public void Dispose()
        {
            this._isRunning = false;
            try
            {
                _thread.Abort();
            }
            catch
            {

            }
            this._webSocket.Close();
            this._webSocket.Dispose();
            this._webSocket = null;  
        }
        
    }
}
