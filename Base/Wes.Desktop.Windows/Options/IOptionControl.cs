namespace Wes.Desktop.Windows.Options
{
    public interface IOptionControl
    {

        /// <summary>
        /// current control
        /// </summary>
        object Control { get; }

        /// <summary>
        /// load option
        /// </summary>
        void LoadOption();

        /// <summary>
        /// save option
        /// </summary>
        /// <returns></returns>
        bool SaveOption();
    }
}
