using System.Collections.Generic;

namespace Wes.Utilities.WXAPI
{
    public class WXMessage
    {
        public string touser { get; set; }

        public string toparty { get; set; }

        public string totag { get; set; }

        public string msgtype { get; set; }

        public string agentid { get; set; }

        public string safe { get; set; } = "0";
    }

    public class Text
    {
        public string content { get; set; }
    }

    public class WXTextMessage : WXMessage
    {
        public Text text { get; set; }

        public WXTextMessage(string content)
        {
            this.msgtype = "text";
            this.text = new Text { content = content };
        }
    }

    public class ImageItem
    {
        public string media_id { get; set; }
    }

    public class WXImageMessage : WXMessage
    {
        public ImageItem image { get; set; }

        public WXImageMessage(string mediaid)
        {
            this.msgtype = "image";
            this.image = new ImageItem { media_id = mediaid };
        }
    }

    public class Article
    {
        public string title { get; set; }
        public string thumb_media_id { get; set; }
        public string author { get; set; }
        public string content_source_url { get; set; }
        public string content { get; set; }
        public string digest { get; set; }
    }

    public class Articles
    {
        public List<Article> articles { get; set; }
    }

    public class WXMpNewsMessage : WXMessage
    {
        public Articles mpnews { get; set; }

        public WXMpNewsMessage(Article article)
        {
            this.msgtype = "mpnews";
            this.mpnews = new Articles();
            this.mpnews.articles = new List<Article>() { article };
        }

    }

    public class TextCard
    {
        public string title { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        public string btntxt { get; set; }
    }

    public class WXTextCardMessage : WXMessage
    {
        public TextCard textcard { get; set; }

        public WXTextCardMessage(TextCard card)
        {
            this.msgtype = "textcard";
            this.textcard = card;
        }
    }
}
