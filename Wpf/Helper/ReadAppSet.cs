using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HelperTools;
namespace Wpf

{
    public class ReadAppSet
    {
        public static JObject Appsetings(string JsonFile)
        {
            JObject Obj=new JObject();
            try
            {
                string S=FileHelper.ReadFileLine(JsonFile);
                Obj = JObject.Parse(S);                
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog($"初始化配置加载错误{ex}");
            }
            return Obj;
        }
    }
}
