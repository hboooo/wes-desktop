using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Wes.Utilities.WXAPI
{
    /// <summary>
    /// 企业微信接口
    /// </summary>
    public class WXHelper
    {
        public static string corpid = System.Configuration.ConfigurationManager.AppSettings["WX_CORYID"].ToString();
        public static string agentid = System.Configuration.ConfigurationManager.AppSettings["WX_AGENTID"].ToString();
        public static string secret = System.Configuration.ConfigurationManager.AppSettings["WX_SECRET"].ToString();
        public static string employeecode = System.Configuration.ConfigurationManager.AppSettings["WX_USER_CODE"].ToString();
        public static string partid = System.Configuration.ConfigurationManager.AppSettings["WX_IT_PART"].ToString();

        /// <summary>
        /// 获取token
        /// </summary>
        private static string token_url = "https://qyapi.weixin.qq.com/cgi-bin/gettoken?corpid={0}&corpsecret={1}";
        /// <summary>
        /// 发送消息
        /// </summary>
        private static string send_message_url = "https://qyapi.weixin.qq.com/cgi-bin/message/send?access_token={0}";
        /// <summary>
        /// 上传临时素材  媒体文件类型，分别有图片（image）、语音（voice）、视频（video），普通文件（file）
        /// </summary>
        private static string send_file_url = "https://qyapi.weixin.qq.com/cgi-bin/media/upload?access_token={0}&type={1}";

        /// <summary>
        /// Kibana
        /// </summary>
        private static string kibana_debug_url = "http://192.168.1.253:5601/app/kibana#/discover/30f2e6f0-9652-11e8-a937-01bb178f9599?_g=(refreshInterval:(display:Off,pause:!f,value:0),time:(from:now%2Fd,mode:quick,to:now%2Fd))&_a=({0})";

        /// <summary>
        /// token 的有效时间（秒）
        /// </summary>
        private static DateTime expires_in = DateTime.Now;
        private static string token = "";
        private static HashSet<string> sendMessageIdList = new HashSet<string>();

        public static void ClearSendMessage()
        {
            sendMessageIdList.Clear();
        }

        private static void GetWXToken(string corpid, string secret, Action<string> callback)
        {
            if (expires_in > DateTime.Now)
            {
                callback?.Invoke(token);
                return;
            }
            expires_in = DateTime.Now;
            token = "";

            string url = string.Format(token_url, corpid, secret);
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.BeginGetResponse((obj) =>
                {
                    try
                    {
                        Dictionary<string, object> respDict = null;
                        HttpWebRequest req = (HttpWebRequest)obj.AsyncState;
                        var response = req.EndGetResponse(obj);
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                byte[] data = new byte[1024];
                                int size = 0;
                                while ((size = responseStream.Read(data, 0, data.Length)) > 0)
                                {
                                    ms.Write(data, 0, size);
                                }
                                respDict = DynamicJson.DeserializeObject<Dictionary<string, object>>(System.Text.Encoding.Default.GetString(ms.ToArray()));
                            }
                        }
                        if (respDict != null)
                        {
                            if (Convert.ToInt32(respDict["errcode"]) == 0)
                            {
                                expires_in = DateTime.Now.AddSeconds(Convert.ToInt32(respDict["expires_in"]));
                                token = respDict["access_token"].ToString();
                                callback?.Invoke(token);
                            }
                            else
                            {
                                callback?.Invoke(null);
                            }
                        }
                        else
                        {
                            callback?.Invoke(null);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        LoggingService.Error(ex);
                        callback?.Invoke(null);
                    }
                }, request);
            }
            catch (System.Exception ex)
            {
                LoggingService.Error(ex);
                callback?.Invoke(null);
            }
        }

        private static void PostWebRequest(string url, string paramData, Encoding encode, Action<dynamic> callback)
        {
            try
            {
                byte[] postdata = encode.GetBytes(paramData);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postdata.Length;
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(postdata, 0, postdata.Length);
                }
                request.BeginGetResponse((obj) =>
                {
                    try
                    {
                        dynamic res = default(dynamic);
                        HttpWebRequest req = (HttpWebRequest)obj.AsyncState;
                        var response = req.EndGetResponse(obj);
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                byte[] data = new byte[1024];
                                int size = 0;
                                while ((size = responseStream.Read(data, 0, data.Length)) > 0)
                                {
                                    ms.Write(data, 0, size);
                                }
                                res = DynamicJson.DeserializeObject<dynamic>(System.Text.Encoding.Default.GetString(ms.ToArray()));
                            }
                        }
                        callback?.Invoke(res);
                    }
                    catch (System.Exception ex)
                    {
                        LoggingService.Error(ex);
                        callback?.Invoke(null);
                    }
                }, request);
            }
            catch (System.Exception ex)
            {
                LoggingService.Error(ex);
                callback?.Invoke(null);
            }
        }

        private static void PostFileWebRequest(string url, string file, Encoding encoding, Action<dynamic> callback)
        {
            try
            {
                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                byte[] endbytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));
                request.Method = "POST";
                request.ContentType = "multipart/form-data;boundary=" + boundary;
                request.KeepAlive = true;

                request.BeginGetRequestStream((obj) =>
                {
                    HttpWebRequest req = (HttpWebRequest)obj.AsyncState;
                    using (var stream = req.EndGetRequestStream(obj))
                    {
                        string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                        stream.Write(boundarybytes, 0, boundarybytes.Length);
                        string formitem = string.Format(formdataTemplate, "name", "pic");
                        byte[] formitembytes = encoding.GetBytes(formitem);
                        stream.Write(formitembytes, 0, formitembytes.Length);

                        string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n";
                        byte[] buffer = new byte[4096];
                        int bytesRead = 0;

                        stream.Write(boundarybytes, 0, boundarybytes.Length);
                        string header = string.Format(headerTemplate, "file", Path.GetFileName(file));
                        byte[] headerbytes = encoding.GetBytes(header);
                        stream.Write(headerbytes, 0, headerbytes.Length);
                        using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                        {
                            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                stream.Write(buffer, 0, bytesRead);
                            }
                        }

                        stream.Write(endbytes, 0, endbytes.Length);
                    }

                    dynamic res = default(dynamic);
                    var response = req.GetResponse();
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            byte[] data = new byte[1024];
                            int size = 0;
                            while ((size = responseStream.Read(data, 0, data.Length)) > 0)
                            {
                                ms.Write(data, 0, size);
                            }
                            res = DynamicJson.DeserializeObject<dynamic>(System.Text.Encoding.Default.GetString(ms.ToArray()));
                        }
                    }
                    callback?.Invoke(res);

                }, request);
            }
            catch (System.Exception ex)
            {
                LoggingService.Error(ex);
                callback?.Invoke(null);
            }
        }

        /// <summary>
        /// 发送微信消息
        /// </summary>
        /// <param name="employeeCode"></param>
        /// <param name="message"></param>
        /// <param name="callback"></param>
        public static void SendWXMessage(string employeeCode, string partid, string message, Action<bool> callback = null)
        {
            GetWXToken(corpid, secret, (token) =>
            {
                if (!string.IsNullOrEmpty(token))
                {
                    WXTextMessage textMessage = new WXTextMessage(message);
                    textMessage.agentid = agentid;
                    textMessage.touser = employeeCode;
                    textMessage.toparty = partid;
                    string url = string.Format(send_message_url, token);
                    PostWebRequest(url, DynamicJson.SerializeObject(textMessage), Encoding.UTF8, (res) =>
                    {
                        if (res.errcode == 0)
                            callback?.Invoke(true);
                        else
                            callback?.Invoke(false);
                    });
                }
                else
                {
                    LoggingService.Info("获取企业微信token失败，请通知管理员");
                }
            });
        }

        /// <summary>
        /// 发送异常到企业微信
        /// </summary>
        /// <param name="employeeCode"></param>
        /// <param name="partid"></param>
        /// <param name="ex"></param>
        /// <param name="imagePath"></param>
        /// <param name="callback"></param>
        public static void SendExceptionWXMessage(string user, string name, System.Exception ex, string imagePath = null, Action<bool> callback = null)
        {
            GetWXToken(corpid, secret, (token) =>
            {
                if (!string.IsNullOrEmpty(token))
                {
                    var send = new Action<string>((mid) =>
                    {
                        StringBuilder builder = new StringBuilder();
                        builder.Append($"<div class=\"normal\">{ex.Message.Replace("\n\n", "\n")}</div>");

                        string messageid = "";
                        if (ex.InnerException.Data != null)
                        {
                            foreach (var item in ex.InnerException.Data)
                            {
                                if (((System.Collections.DictionaryEntry)item).Key.ToString() == "001_messageId")
                                    messageid = ((System.Collections.DictionaryEntry)item).Value.ToString();
                                else if (((System.Collections.DictionaryEntry)item).Key.ToString() == "010_Version")
                                    builder.Append($"<div class=\"highlight\">版本号: {((System.Collections.DictionaryEntry)item).Value.ToString()}</div>");
                                else if (((System.Collections.DictionaryEntry)item).Key.ToString() == "000_scanValue")
                                    builder.Append($"<div class=\"highlight\">扫描值: {((System.Collections.DictionaryEntry)item).Value.ToString()}</div>");
                                else if (((System.Collections.DictionaryEntry)item).Key.ToString() == "100_wrokNo")
                                    builder.Append($"<div class=\"highlight\">作业单: {((System.Collections.DictionaryEntry)item).Value.ToString()}</div>");
                                else if (((System.Collections.DictionaryEntry)item).Key.ToString() == "014_User")
                                {
                                    dynamic value = DynamicJson.DeserializeObject<object>(((System.Collections.DictionaryEntry)item).Value.ToString());
                                    builder.Append($"<div class=\"highlight\">登录名: {name}({user})({value.Station})</div>");
                                }
                                else if (((System.Collections.DictionaryEntry)item).Key.ToString() == "016_AddIn")
                                {
                                    dynamic value = DynamicJson.DeserializeObject<object>(((System.Collections.DictionaryEntry)item).Value.ToString());
                                    builder.Append($"<div class=\"highlight\">客户号: {value.EndCustomerName}({value.EndCustomer})</div>");
                                }
                            }
                        }

                        TextCard textCard = new TextCard();
                        textCard.title = "WES异常，请相关人员跑步入场";
                        textCard.description = builder.ToString();

                        if (sendMessageIdList.Contains(messageid))
                        {
                            return;
                        }
                        sendMessageIdList.Add(messageid);

#if DEBUG
                        textCard.url = string.Format(kibana_debug_url, string.Format("query:(language:lucene,query:'{0}')", messageid));
#else
                        string kibanaUrl = string.Empty;
                        var queryParams = string.Format("&_a=(query:(language:lucene,query:'{0}'))", messageid);
                        var conf = ConfigurationMapping.Instance.GlobalConf;
                        foreach (var item in conf.leftMenu.menu)
                        {
                            if (item.id != null && item.id.ToString() == "1c79964c-fbeb-4222-8de5-68ae83a1602d")
                            {
                                kibanaUrl = item.uri.ToString() + queryParams;
                            }
                        }
                        textCard.url = kibanaUrl;
#endif
                        textCard.btntxt = "更多";
                        WXTextCardMessage cardMessage = new WXTextCardMessage(textCard);
                        cardMessage.agentid = agentid;
                        cardMessage.touser = employeecode;
                        cardMessage.toparty = partid;
                        string url = string.Format(send_message_url, token);
                        PostWebRequest(url, DynamicJson.SerializeObject(cardMessage), Encoding.UTF8, (res) =>
                        {
                            if (res.errcode == 0)
                            {
                                WXImageMessage imageMessage = new WXImageMessage(mid);
                                imageMessage.agentid = agentid;
                                imageMessage.touser = employeecode;
                                imageMessage.toparty = partid;
                                PostWebRequest(url, DynamicJson.SerializeObject(imageMessage), Encoding.UTF8, (img_res) =>
                                {
                                    if (img_res.errcode == 0)
                                        callback?.Invoke(true);
                                    else
                                        callback?.Invoke(false);
                                });
                            }
                            else
                            {
                                callback?.Invoke(false);
                            }
                        });
                    });

                    if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                    {
                        string file_url = string.Format(send_file_url, token, "image");
                        PostFileWebRequest(file_url, imagePath, Encoding.Default, (uploadres) =>
                        {
                            string media = null;
                            if (uploadres.errcode == 0)
                                media = uploadres.media_id.ToString();
                            send(media);
                        });
                    }
                    else
                    {
                        send(null);
                    }
                }
                else
                {
                    LoggingService.Info("获取企业微信token失败，请通知管理员");
                }
            });
        }
    }
}
