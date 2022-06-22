using log4net;
using log4net.Config;
using log4net.Repository;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
namespace HelperTools
{
    public class LogHelper
    {
        //public static readonly log4net.ILog loginfo = log4net.LogManager.GetLogger("loginfo");
        //public static readonly log4net.ILog logerror = log4net.LogManager.GetLogger("logerror");
        
        public static void WriteLog(string info)
        {
            InfoLog(info);
        }

        public static void WriteLog(string info, Exception ex)
        {
            ErrorLog(info,ex);
        }
        #region
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public static void ErrorLog(object msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("logerror");
            Task.Run(() => log.Error(msg));   //异步
            // Task.Factory.StartNew(() =>log.Error(msg));//  这种异步也可以
            //log.Error(msg);    //这种也行跟你需要，性能越好，越强大，我还是使用异步方式
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        public static void ErrorLog(Exception ex)
        {
            //log4net.ILog log = log4net.LogManager.GetLogger("logerror");
            //Task.Run(() => log.Error(ex.Message.ToString() + "\r\n" + 
            //    ex.Source.ToString() + "\r\n" + 
            //    ex.TargetSite.ToString() + "\r\n" + 
            //    ex.StackTrace.ToString() + "\r\n"));
            ErrorLog(ex.Message.ToString(),ex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="ex"></param>
        public static void ErrorLog(object msg, Exception ex)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("logerror");
            if (ex != null)
            {
                Task.Run(() => log.Error(msg, ex));   //异步
            }
            else
            {
                Task.Run(() => log.Error(msg));   //异步
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public static void InfoLog(object msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("loginfo");
            Task.Run(() => log.Info(msg));   //异步
            // Task.Factory.StartNew(() =>log.Error(msg));//  这种异步也可以
            //log.Error(msg);    //这种也行跟你需要，性能越好，越强大，我还是使用异步方式
        }
        #endregion
    }
}