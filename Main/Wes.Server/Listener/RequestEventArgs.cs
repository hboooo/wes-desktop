using System;

namespace Wes.Server.Listener
{
    public delegate void RequestEventHandler(object sender, RequestEventArgs e);

    public class RequestEventArgs : EventArgs
    {
        public RequestEventArgs()
        {

        }

        public RequestEventArgs(RequestParams requestParams, string message, bool isValied)
        {
            Params = requestParams;
            Message = message;
            IsValied = isValied;
        }

        public RequestEventArgs(RequestParams requestParams, string message)
        {
            Params = requestParams;
            Message = message;
        }

        public RequestEventArgs(RequestParams requestParams, RequestActionType action)
        {
            Params = requestParams;
            Action = action;
        }

        public RequestParams Params { get; set; }

        public bool IsValied { get; set; } = false;

        public string Message { get; set; }

        public RequestActionType Action { get; set; }
    }

    public enum RequestActionType
    {
        Add,
        Delete,
    }
}
