using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Wes.Utilities.Exception;
using System.Linq;

namespace Wes.Utilities
{

    /// <summary>
    /// FTP操作类
    /// 只支持下载文件
    /// </summary>
    public class FtpHelper
    {
        #region 属性
        /// <summary>
        /// 获取或设置用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 获取或设置密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 获取或设置FTP服务器地址
        /// </summary>
        public Uri Uri { get; set; }
        /// <summary>
        /// 获取或者是读取文件、目录列表时所使用的编码，默认为UTF-8 
        /// </summary>
        public Encoding Encode { get; set; }
        /// <summary>
        /// 异常信息
        /// </summary>
        public string ErrorMsg { get; set; }
        /// <summary>
        /// Exception
        /// </summary>
        public System.Exception Exception { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public FtpStatusCode StatusCode { get; set; }
        /// <summary>
        /// 状态描述
        /// </summary>
        public string StatusDescription { get; set; }


        private HashSet<string> files = new HashSet<string>();
        #endregion

        #region 构造函数
        public FtpHelper(Uri uri, string username, string password)
        {
            this.Uri = uri;
            this.UserName = username;
            this.Password = password;
            this.Encode = Encoding.GetEncoding("utf-8");
        }

        public FtpHelper(Uri uri)
        {
            this.Uri = uri;
            this.Encode = Encoding.GetEncoding("utf-8");
        }
        #endregion

        #region 建立连接
        /// <summary>
        /// 建立FTP链接,返回请求对象
        /// </summary>
        /// <param name="uri">FTP地址</param>
        /// <param name="method">操作命令(WebRequestMethods.Ftp)</param>
        /// <returns></returns>
        private FtpWebRequest CreateRequest(Uri uri, string method)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
                if (!string.IsNullOrEmpty(UserName))
                {
                    request.Credentials = new NetworkCredential(this.UserName, this.Password);//指定登录ftp服务器的用户名和密码。
                }
                request.KeepAlive = false;    //指定连接是应该关闭还是在请求完成之后关闭，默认为true
                request.UsePassive = true;    //指定使用被动模式，默认为true
                request.UseBinary = true;     //指示服务器要传输的是二进制数据.false,指示数据为文本。默认值为true
                request.EnableSsl = false;    //如果控制和数据传输是加密的,则为true.否则为false.默认值为 false
                request.Method = method;
                return request;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 建立FTP链接,返回响应对象
        /// </summary>
        /// <param name="uri">FTP地址</param>
        /// <param name="method">操作命令(WebRequestMethods.Ftp)</param>
        /// <returns></returns>
        private FtpWebResponse CreateResponse(Uri uri, string method)
        {
            try
            {
                LoggingService.InfoFormat("Create response,ftp uri:{0},method:{1}", uri.ToString(), method);
                FtpWebRequest request = CreateRequest(uri, method);
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                return response;
            }
            catch (WebException ex)
            {
                String status = ((FtpWebResponse)ex.Response).StatusDescription;
                LoggingService.ErrorFormat(ex, "Create response failed,error:{0},message:{1},ftp uri:{2},method:{3}", ex.Message, status, uri.ToString(), method);
                throw ex;
            }
        }

        private void CreateResponseAsync(Uri uri, string method, Action<FtpWebResponse> action)
        {
            try
            {
                FtpWebRequest request = CreateRequest(uri, method);
                request.BeginGetResponse((obj) =>
                {
                    try
                    {
                        FtpWebRequest req = (FtpWebRequest)obj.AsyncState;
                        var response = (FtpWebResponse)req.EndGetResponse(obj);
                        action?.Invoke(response);
                    }
                    catch (System.Exception ex)
                    {
                        LoggingService.Error($"method:{method}; url：{uri}; message:{ex.Message}", ex);
                        action?.Invoke(null);
                    }
                }, request);
            }
            catch (WebException ex)
            {
                LoggingService.Error(ex);
                action?.Invoke(null);
            }
        }
        #endregion

        #region 下载文件
        /// <summary>
        /// 从FTP服务器下载文件
        /// </summary>
        /// <param name="remoteFilePath">远程完整文件名</param>
        /// <param name="localFilePath">本地带有完整路径的文件名</param>
        public bool DownloadFile(string remoteFilePath, string localFilePath, string filename = null)
        {
            try
            {
                string localDirector = Path.GetDirectoryName(localFilePath);
                Utils.CheckCreateDirectoroy(localDirector, true);
                if (!IsDownloaded(remoteFilePath, localFilePath, filename))
                {
                    Utils.DeleteFile(localFilePath);
                    FtpWebResponse response = CreateResponse(new Uri(this.Uri.ToString() + remoteFilePath), WebRequestMethods.Ftp.DownloadFile);

                    using (Stream stream = response.GetResponseStream())
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            byte[] buffer = new byte[1024];
                            int bytesCount = 0;
                            while ((bytesCount = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                ms.Write(buffer, 0, bytesCount);
                            }
                            using (FileStream fs = new FileStream(localFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                            {
                                fs.Write(ms.ToArray(), 0, (int)ms.Length);
                            }
                        }
                    }
                }
                return true;
            }
            catch (WebException ex)
            {
                String status = ((FtpWebResponse)ex.Response).StatusDescription;
                LoggingService.ErrorFormat(ex, "Download file failed,error:{0},message:{1},remoteFile:{2},localFile:{3}", ex.Message, status, remoteFilePath, localFilePath);
            }
            catch (System.Exception ex)
            {
                LoggingService.ErrorFormat(ex, "Download file failed,error:{0},remoteFile:{1},localFile:{2}", ex.Message, remoteFilePath, localFilePath);
            }
            return false;
        }

        public void DownloadFileAsync(string remoteFilePath, string localFilePath, string filename = null, Action<bool> action = null)
        {
            try
            {
                string localDirector = Path.GetDirectoryName(localFilePath);
                Utils.CheckCreateDirectoroy(localDirector, true);
                IsDownloadedAsync(remoteFilePath, localFilePath, filename, (res) =>
                {
                    if (!res)
                    {
                        CreateResponseAsync(new Uri(this.Uri.ToString() + remoteFilePath), WebRequestMethods.Ftp.DownloadFile, (ftpWebResponse) =>
                        {
                            if (ftpWebResponse != null)
                            {
                                using (Stream stream = ftpWebResponse.GetResponseStream())
                                {
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        byte[] buffer = new byte[1024];
                                        int bytesCount = 0;
                                        while ((bytesCount = stream.Read(buffer, 0, buffer.Length)) > 0)
                                        {
                                            ms.Write(buffer, 0, bytesCount);
                                        }
                                        using (FileStream fs = new FileStream(localFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                                        {
                                            fs.Write(ms.ToArray(), 0, (int)ms.Length);
                                        }
                                    }
                                }
                                ftpWebResponse.Close();
                            }
                            action?.Invoke(true);
                        });
                    }
                    else
                    {
                        action?.Invoke(true);
                    }
                });
            }
            catch (WebException ex)
            {
                String status = ((FtpWebResponse)ex.Response).StatusDescription;
                LoggingService.ErrorFormat(ex, "GetDateTimestamp failed,error:{0},message:{1},remoteFile:{2},localFile:{3}", ex.Message, status, remoteFilePath, localFilePath);
                action?.Invoke(false);
            }
            catch (System.Exception ex)
            {
                LoggingService.ErrorFormat(ex, "GetDateTimestamp failed,error:{0},remoteFile:{1},localFile:{2}", ex.Message, remoteFilePath, localFilePath);
                action?.Invoke(false);
            }
        }

        /// <summary>
        /// 比对本地文件与服务器文件时间,确认本地是否为最新版本的文件
        /// </summary>
        /// <param name="remoteFilePath">FTP文件</param>
        /// <param name="localFilePath">本地文件</param>
        /// <returns></returns>
        private bool IsDownloaded(string remoteFilePath, string localFilePath, string filename)
        {
            try
            {
                if (!string.IsNullOrEmpty(filename))
                {
                    //判斷文件是否存在
                    int index = remoteFilePath.LastIndexOf(filename);
                    string path = remoteFilePath.Substring(0, index - 1);
                    if (!IsFtpFileExist(path, filename))
                    {
                        Utils.DeleteFile(localFilePath);
                        return true;
                    }
                }

                if (File.Exists(localFilePath))
                {
                    FtpWebResponse response = CreateResponse(new Uri(this.Uri.ToString() + remoteFilePath), WebRequestMethods.Ftp.GetDateTimestamp);
                    FileInfo localFile = new FileInfo(localFilePath);
                    if (localFile.CreationTime >= response.LastModified)
                    {
                        LoggingService.InfoFormat("local file {0} Is the latest, don not need to download", localFile.Name);
                        return true;
                    }

                }
            }
            catch (WebException ex)
            {
                String status = ((FtpWebResponse)ex.Response).StatusDescription;
                LoggingService.ErrorFormat(ex, "GetDateTimestamp failed,error:{0},message:{1},remoteFile:{2},localFile:{3}", ex.Message, status, remoteFilePath, localFilePath);
            }
            catch (System.Exception ex)
            {
                LoggingService.ErrorFormat(ex, "GetDateTimestamp failed,error:{0},remoteFile:{1},localFile:{2}", ex.Message, remoteFilePath, localFilePath);
            }
            return false;
        }

        private void IsDownloadedAsync(string remoteFilePath, string localFilePath, string filename, Action<bool> action)
        {
            try
            {
                if (!string.IsNullOrEmpty(filename))
                {
                    //判斷文件是否存在
                    int index = remoteFilePath.LastIndexOf(filename);
                    string path = remoteFilePath.Substring(0, index - 1);
                    IsFtpFileExistAsync(path, filename, (res) =>
                    {
                        if (!res || !File.Exists(localFilePath))
                        {
                            Utils.DeleteFile(localFilePath);
                            action?.Invoke(true);
                        }
                        else
                        {
                            CreateResponseAsync(new Uri(this.Uri.ToString() + remoteFilePath), WebRequestMethods.Ftp.GetDateTimestamp, (ftpWebResponse) =>
                            {
                                if (ftpWebResponse != null)
                                {
                                    FileInfo localFile = new FileInfo(localFilePath);
                                    if (localFile.CreationTime >= ftpWebResponse.LastModified)
                                    {
                                        LoggingService.InfoFormat("local file {0} Is the latest, don not need to download", localFile.Name);
                                        ftpWebResponse.Close();
                                        action?.Invoke(true);
                                    }
                                    else
                                    {
                                        ftpWebResponse.Close();
                                        action?.Invoke(false);
                                    }
                                }
                                else
                                {
                                    action?.Invoke(false);
                                }
                            });
                        }
                    });
                }
                else
                {
                    action?.Invoke(false);
                }
            }
            catch (WebException ex)
            {
                String status = ((FtpWebResponse)ex.Response).StatusDescription;
                LoggingService.ErrorFormat(ex, "GetDateTimestamp failed,error:{0},message:{1},remoteFile:{2},localFile:{3}", ex.Message, status, remoteFilePath, localFilePath);
                action?.Invoke(false);
            }
            catch (System.Exception ex)
            {
                LoggingService.ErrorFormat(ex, "GetDateTimestamp failed,error:{0},remoteFile:{1},localFile:{2}", ex.Message, remoteFilePath, localFilePath);
                action?.Invoke(false);
            }
        }

        /// <summary>
        /// 判斷文件是否存在
        /// </summary>
        /// <param name="remoteFilePath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool IsFtpFileExist(string remoteFilePath, string fileName)
        {
            try
            {
                if (files.Count == 0)
                {
                    FtpWebResponse response = CreateResponse(new Uri(this.Uri.ToString() + remoteFilePath), WebRequestMethods.Ftp.ListDirectory);
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            string file = string.Empty;
                            while (!string.IsNullOrEmpty((file = sr.ReadLine())))
                            {
                                files.Add(file);
                            }
                        }
                    }
                }
                var query = files.Where(s => s.Contains(fileName));
                if (query.Count() > 0)
                    return true;
                else
                    LoggingService.InfoFormat("ftp file {0} does not exist", fileName);
            }
            catch (WebException ex)
            {
                String status = ((FtpWebResponse)ex.Response).StatusDescription;
                LoggingService.ErrorFormat(ex, "Check file exist failed,error:{0},message:{1},remoteFile:{2},fileName:{3}", ex.Message, status, remoteFilePath, fileName);
            }
            catch (System.Exception ex)
            {
                LoggingService.ErrorFormat(ex, "Check file exist failed,error:{0},remoteFile:{1},fileName:{2}", ex.Message, remoteFilePath, fileName);
            }
            return false;
        }

        private void IsFtpFileExistAsync(string remoteFilePath, string fileName, Action<bool> action)
        {
            try
            {
                var check = new Action(() =>
                {
                    var query = files.Where(s => s.Contains(fileName));
                    if (query.Count() > 0)
                        action?.Invoke(true);
                    else
                    {
                        LoggingService.InfoFormat("ftp file {0} does not exist", fileName);
                        action?.Invoke(false);
                    }
                });
                if (files.Count == 0)
                {
                    CreateResponseAsync(new Uri(this.Uri.ToString() + remoteFilePath), WebRequestMethods.Ftp.ListDirectory, (ftpWebResponse) =>
                    {
                        if (ftpWebResponse != null)
                        {
                            using (var stream = ftpWebResponse.GetResponseStream())
                            {
                                using (StreamReader sr = new StreamReader(stream))
                                {
                                    string file = string.Empty;
                                    while (!string.IsNullOrEmpty((file = sr.ReadLine())))
                                    {
                                        files.Add(file);
                                    }
                                }
                            }
                            ftpWebResponse.Close();
                        }
                        check();
                    });
                }
                else
                {
                    check();
                }
            }
            catch (WebException ex)
            {
                String status = ((FtpWebResponse)ex.Response).StatusDescription;
                LoggingService.ErrorFormat(ex, "Check file exist failed,error:{0},message:{1},remoteFile:{2},fileName:{3}", ex.Message, status, remoteFilePath, fileName);
                action?.Invoke(false);
            }
            catch (System.Exception ex)
            {
                LoggingService.ErrorFormat(ex, "Check file exist failed,error:{0},remoteFile:{1},fileName:{2}", ex.Message, remoteFilePath, fileName);
                action?.Invoke(false);
            }
        }
        #endregion

        #region 上传文件
        public delegate void FtpUploadProgress(int value);
        public event FtpUploadProgress FtpUploadProgressEvent;

        public delegate void FtpUploadFinish();
        public event FtpUploadFinish FtpUploadFinishEvent;

        /// <summary>
        /// 上传文件接口
        /// </summary>
        /// <param name="localFilePath">本地带有完整路径的文件名</param>
        /// <param name="remoteFilePath">要在FTP服务器上面保存完整文件名</param>
        /// <returns></returns>
        public void UploadFile(string localFilePath, string remoteFilePath)
        {
            UploadFile(localFilePath, remoteFilePath, false);
        }
        /// <summary>
        /// 上传文件到FTP服务器,若文件已存在自动覆盖
        /// </summary>
        /// <param name="localFilePath">本地带有完整路径的文件名</param>
        /// <param name="remoteFilePath">要在FTP服务器上面保存完整文件名</param>
        /// <param name="autoCreateDirectory">是否自动递归创建文件目录</param>
        /// <returns></returns>
        public void UploadFile(string localFilePath, string remoteFilePath, bool autoCreateDirectory)
        {
            long current = 0;
            long total = 1;
            int percent = 0;
            int temp = 0;
            try
            {
                //自动递归创建目录
                if (autoCreateDirectory)
                {
                    if (!CreateDirectory(Path.GetDirectoryName(remoteFilePath)))
                    {
                        //递归创建目录失败，返回異常
                        throw new FileNotFoundException(string.Format("創建目錄失敗:{0}!", localFilePath));
                    }
                }
                FileInfo fileInf = new FileInfo(localFilePath);
                if (!fileInf.Exists)
                {
                    throw new FileNotFoundException(string.Format("本地文件不存在:{0}!", localFilePath));
                }
                LoggingService.DebugFormat("開始上傳文件:{0}", remoteFilePath);
                FtpWebRequest request = CreateRequest(new Uri(this.Uri + remoteFilePath), WebRequestMethods.Ftp.UploadFile);
                request.ContentLength = fileInf.Length;
                total = fileInf.Length;
                using (FileStream fs = fileInf.OpenRead())
                {
                    using (Stream stream = request.GetRequestStream())
                    {
                        int contentLen = 0;
                        byte[] buff = new byte[1024];
                        while ((contentLen = fs.Read(buff, 0, buff.Length)) > 0)
                        {
                            stream.Write(buff, 0, contentLen);
                            //产生进度条数据
                            current += contentLen;
                            temp = Int32.Parse((100 * current / total).ToString());
                            if (temp != percent)
                            {
                                FtpUploadProgressEvent?.Invoke(percent);
                                percent = temp;
                            }
                        }
                        FtpUploadFinishEvent?.Invoke();
                    }
                }
            }
            catch (System.Exception ex)
            {
                this.Exception = ex;
                this.ErrorMsg = ex.Message;
                throw new WesException(string.Format("{0}文件上傳失敗，原因:{1}!", localFilePath, ex.Message));

            }

        }
        #endregion

        #region 递归创建目录
        /// <summary>
        /// 递归创建目录，在创建目录前不进行目录是否已存在检测
        /// </summary>
        /// <param name="remoteDirectory"></param>
        public bool CreateDirectory(string remoteDirectory)
        {
            return CreateDirectory(remoteDirectory, false);
        }

        /// <summary>
        /// 在FTP服务器递归创建目录
        /// </summary>
        /// <param name="remoteDirectory">要创建的目录</param>
        /// <param name="autoCheckExist">创建目录前是否进行目录是否存在检测</param>
        /// <returns></returns>
        public bool CreateDirectory(string remoteDirectory, bool autoCheckExist)
        {
            try
            {
                string parentDirector = "/";
                if (!string.IsNullOrEmpty(remoteDirectory))
                {
                    remoteDirectory = remoteDirectory.Replace("\\", "/");
                    string[] directors = remoteDirectory.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string director in directors)
                    {
                        if (!parentDirector.EndsWith("/")) parentDirector += "/";
                        if (autoCheckExist)
                        {
                            if (!DirectoryExist(parentDirector, director))
                                CreateResponse(new Uri(this.Uri + parentDirector + director), WebRequestMethods.Ftp.MakeDirectory);
                        }
                        else
                        {
                            try
                            {
                                CreateResponse(new Uri(this.Uri + parentDirector + director), WebRequestMethods.Ftp.MakeDirectory);
                            }
                            catch (WebException ex)
                            {
                                if (this.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
                                {
                                    throw ex;
                                }
                            }
                        }
                        parentDirector += director;
                    }
                }
                return true;
            }
            catch (WebException ex)
            {
                this.Exception = ex;
                this.ErrorMsg = ex.Message;
            }
            return false;
        }

        /// <summary>
        /// 检测指定目录下是否存在指定的目录名
        /// </summary>
        /// <param name="parentDirector"></param>
        /// <param name="directoryName"></param>
        /// <returns></returns>
        private bool DirectoryExist(string parentDirector, string directoryName)
        {
            List<FileStruct> list = GetFileAndDirectoryList(parentDirector);
            foreach (FileStruct fstruct in list)
            {
                if (fstruct.IsDirectory && fstruct.Name == directoryName)
                {
                    return true;
                }
            }
            return false;
        }


        #endregion

        #region 目录、文件列表
        /// <summary>
        /// 获取FTP服务器上指定目录下的所有文件和目录
        /// 若获取的中文文件、目录名优乱码现象
        /// 请调用this.Encode进行文件编码设置，默认为UTF-8，一般改为GB2312就能正确识别
        /// </summary>
        /// <param name="direcotry"></param>
        /// <returns></returns>
        public List<FileStruct> GetFileAndDirectoryList(string direcotry)
        {
            try
            {
                List<FileStruct> list = new List<FileStruct>();
                string str = null;
                //WebRequestMethods.Ftp.ListDirectoryDetails可以列出所有的文件和目录列表
                //WebRequestMethods.Ftp.ListDirectory只能列出目录的文件列表
                FtpWebResponse response = CreateResponse(new Uri(this.Uri.ToString() + direcotry), WebRequestMethods.Ftp.ListDirectoryDetails);
                Stream stream = response.GetResponseStream();

                using (StreamReader sr = new StreamReader(stream, this.Encode))
                {
                    str = sr.ReadToEnd();
                }
                string[] fileList = str.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                EFileListFormat format = JudgeFileListFormat(fileList);
                if (!string.IsNullOrEmpty(str) && format != EFileListFormat.Unknown)
                {
                    list = ParseFileStruct(fileList, format);
                }
                return list;
            }
            catch (WebException ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 解析文件列表信息返回文件列表
        /// </summary>
        /// <param name="fileList"></param>
        /// <param name="format">文件列表格式</param>
        /// <returns></returns>
        private List<FileStruct> ParseFileStruct(string[] fileList, EFileListFormat format)
        {
            List<FileStruct> list = new List<FileStruct>();
            if (format == EFileListFormat.UnixFormat)
            {
                foreach (string info in fileList)
                {
                    FileStruct fstuct = new FileStruct();
                    fstuct.Origin = info.Trim();
                    fstuct.OriginArr = fstuct.Origin.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (fstuct.OriginArr.Length == 9)
                    {
                        fstuct.Flags = fstuct.OriginArr[0];
                        fstuct.IsDirectory = (fstuct.Flags[0] == 'd');
                        fstuct.Owner = fstuct.OriginArr[2];
                        fstuct.Group = fstuct.OriginArr[3];
                        fstuct.Size = Convert.ToInt32(fstuct.OriginArr[4]);
                        if (fstuct.OriginArr[7].Contains(":"))
                        {
                            fstuct.OriginArr[7] = DateTime.Now.Year + " " + fstuct.OriginArr[7];
                        }
                        fstuct.UpdateTime = DateTime.Parse(string.Format("{0} {1} {2}", fstuct.OriginArr[5], fstuct.OriginArr[6], fstuct.OriginArr[7]));
                        fstuct.Name = fstuct.OriginArr[8];
                        if (fstuct.Name != "." && fstuct.Name != "..")
                        {
                            list.Add(fstuct);
                        }
                    }

                }
            }
            else if (format == EFileListFormat.WindowsFormat)
            {
                foreach (string info in fileList)
                {
                    FileStruct fstuct = new FileStruct();
                    fstuct.Origin = info.Trim();
                    fstuct.OriginArr = fstuct.Origin.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (fstuct.OriginArr.Length == 4)
                    {
                        DateTimeFormatInfo usDate = new CultureInfo("en-US", false).DateTimeFormat;
                        usDate.ShortTimePattern = "t";
                        fstuct.UpdateTime = DateTime.Parse(fstuct.OriginArr[0] + " " + fstuct.OriginArr[1], usDate);

                        fstuct.IsDirectory = (fstuct.OriginArr[2] == "<DIR>");
                        if (!fstuct.IsDirectory)
                        {
                            fstuct.Size = Convert.ToInt32(fstuct.OriginArr[2]);
                        }
                        fstuct.Name = fstuct.OriginArr[3];
                        if (fstuct.Name != "." && fstuct.Name != "..")
                        {
                            list.Add(fstuct);
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 判断文件列表的方式Window方式还是Unix方式
        /// </summary>
        /// <param name="fileList">文件信息列表</param>
        /// <returns></returns>
        private EFileListFormat JudgeFileListFormat(string[] fileList)
        {
            foreach (string str in fileList)
            {
                if (str.Length > 10 && Regex.IsMatch(str.Substring(0, 10), "(-|d)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)"))
                {
                    return EFileListFormat.UnixFormat;
                }
                else if (str.Length > 8 && Regex.IsMatch(str.Substring(0, 8), "[0-9][0-9]-[0-9][0-9]-[0-9][0-9]"))
                {
                    return EFileListFormat.WindowsFormat;
                }
            }
            return EFileListFormat.Unknown;
        }

        private FileStruct ParseFileStructFromWindowsStyleRecord(string Record)
        {
            FileStruct f = new FileStruct();
            string processstr = Record.Trim();
            string dateStr = processstr.Substring(0, 8);
            processstr = (processstr.Substring(8, processstr.Length - 8)).Trim();
            string timeStr = processstr.Substring(0, 7);
            processstr = (processstr.Substring(7, processstr.Length - 7)).Trim();
            DateTimeFormatInfo myDTFI = new CultureInfo("en-US", false).DateTimeFormat;
            myDTFI.ShortTimePattern = "t";
            f.UpdateTime = DateTime.Parse(dateStr + " " + timeStr, myDTFI);
            if (processstr.Substring(0, 5) == "<DIR>")
            {
                f.IsDirectory = true;
                processstr = (processstr.Substring(5, processstr.Length - 5)).Trim();
            }
            else
            {
                string[] strs = processstr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);   // true);
                processstr = strs[1];
                f.IsDirectory = false;
            }
            f.Name = processstr;
            return f;
        }
        #endregion

        #region 文件结构
        /// <summary>
        /// 文件列表格式
        /// </summary>
        public enum EFileListFormat
        {
            /// <summary>
            /// Unix文件格式
            /// </summary>
            UnixFormat,
            /// <summary>
            /// Window文件格式
            /// </summary>
            WindowsFormat,
            /// <summary>
            /// 未知格式
            /// </summary>
            Unknown
        }

        public class FileStruct
        {
            public string Origin { get; set; }
            public string[] OriginArr { get; set; }
            public string Flags { get; set; }
            /// <summary>
            /// 所有者
            /// </summary>
            public string Owner { get; set; }
            public string Group { get; set; }
            /// <summary>
            /// 是否为目录
            /// </summary>
            public bool IsDirectory { get; set; }
            /// <summary>
            /// 文件或目录更新时间
            /// </summary>
            public DateTime UpdateTime { get; set; }
            /// <summary>
            /// 文件或目录名称
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// 文件大小(目录始终为0)
            /// </summary>
            public int Size { get; set; }
        }
        #endregion

    }
}
