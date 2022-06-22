using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperTools
{
    public class FileHelper
    {
        public static string dir = AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// 写文件 重写覆盖
        /// </summary>
        public static void FileCreate(string FilePath, string str, string FileName)
        {
            
            if (!Directory.Exists($@"{dir}\{FilePath}"))
            {
                Directory.CreateDirectory(FilePath);
            }
            
            string filePath = $@"{dir}\{FilePath}\{FileName}" ;

            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(str);
                    sw.Flush();
                    sw.Dispose();
                }
            }
        }

        public static void FileAppend(string FilePath, string str, string FileName)
        {
            if (!Directory.Exists(FilePath))
            {
                Directory.CreateDirectory(FilePath);
            }
            string filePath = FilePath + "\\" + FileName;

            using (FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(str);
                    sw.Flush();
                    sw.Dispose();
                }
            }
        }


        #region 读文件
        /// <summary>
        /// 读文件
        /// </summary>
        public static string ReadFile(string path)
        {
            string result = string.Empty;

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    result = sr.ReadToEnd();
                }
            }

            return result;
        }
        /// <summary>
        /// 读文件，去回车换行
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReadFileLine(string path)
        {

            StringBuilder sb = new StringBuilder();

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        sb.Append(line);
                    }
                }
            }
            return sb.ToString();
        }
        #endregion

        /// <summary>
        /// 将文件转换成Base64格式
        /// </summary>
        public static string FileToBase64(string fileName)
        {
            string result = "";

            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                byte[] byteArray = new byte[fs.Length];
                fs.Read(byteArray, 0, byteArray.Length);
                result = Convert.ToBase64String(byteArray);
            }
            return result;
        }

        

        #region  创建目录
        public static void Createpath(string pathdir)
        {
            if (!System.IO.Directory.Exists(pathdir))
            {
                Directory.CreateDirectory(pathdir);
            }
        }
        #endregion
        #region 文件是否存在
        public static bool Fielexist(string pathstr)
        {
            bool flg = false;
            if (System.IO.File.Exists(pathstr) == true)
            {
                flg = true;
            }
            else
            {
                flg = false;
            }
            return flg;
        }
        #endregion

        #region 目录是否存在
        public static bool Pathdirexist(string pathstr)
        {
            bool flg = false;
            if (System.IO.Directory.Exists(pathstr) == true)
            {
                flg = true;
            }
            else
            {
                flg = false;
            }
            return flg;
        }
        #endregion

        /// <summary>
        /// 获取指定文件夹下所有子目录及文件函数
        /// </summary>
        /// <param name="theDir">指定目录</param>
        /// <param name="nLevel">默认起始值,调用时,一般为0</param>
        /// <param name="Rn">用于迭加的传入值,一般为空</param>
        /// <param name="tplPath">默认选择模板名称</param>
        /// <returns></returns>
        public  string ListTreeShow(string Path)//递归目录 文件
        {
            
            DirectoryInfo theDir = new DirectoryInfo(Path);
            DirectoryInfo[] subDirectories = theDir.GetDirectories();//获得目录
            foreach (DirectoryInfo dirinfo in subDirectories)
            {

                FileInfo[] fileInfo = dirinfo.GetFiles();   //目录下的文件
                foreach (FileInfo fInfo in fileInfo)
                {
                }                
            }
            return string.Empty;
        }

        #region 移动文件
        /****************************************
         * 函数名称：FileMove
         * 功能说明：移动文件
         * 参    数：OrignFile:原始路径,NewFile:新文件路径
         * 调用示列：
         *            string OrignFile = Server.MapPath("../说明.txt");    
         *            string NewFile = Server.MapPath("../../说明.txt");
         *            EC.FileObj.FileMove(OrignFile, NewFile);
        *****************************************/
        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="OrignFile">原始路径</param>
        /// <param name="NewFile">新路径</param>
        public static void FileMove(string OrignFile, string NewFile)
        {
            File.Move(OrignFile, NewFile);
        }
        #endregion
    }
}