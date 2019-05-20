namespace Wes.Print
{
    public enum ErrorCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success,
        /// <summary>
        /// 未实现
        /// </summary>
        Unrealized,
        /// <summary>
        /// 参数错误
        /// </summary>
        DataError,
        /// <summary>
        /// 打印内部错误
        /// </summary>
        InternalError,
        /// <summary>
        /// 下载打印模板失败
        /// </summary>
        DownloadTemplateFailure,
        /// <summary>
        /// 打印机为空
        /// </summary>
        PrinterError,
    }
}
