using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wes.Launcher
{
    public class FileVersion
    {
        //纯文件名  包含后缀
        public string Name { get; set; }
        //http 文件路径
        public string Url { get; set; }
        //版本号
        public string LastVer { get; set; }
        //本地路径 相对根目录的路径
        public string RelativePath { get; set; }
        //更新当前文件后是否需要重启程序生效
        public bool NeedRestart { get; set; }
        //文件大小
        public long Size { get; set; }

        public string Unique { get; set; }
    }
}
