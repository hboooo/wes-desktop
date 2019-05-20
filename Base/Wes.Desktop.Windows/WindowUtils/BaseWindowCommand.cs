using System.Windows.Input;

namespace Wes.Desktop.Windows
{
    public static class BaseWindowCommand
    {
        public static ICommand Menu = new RoutedCommand("Menu", typeof(BaseWindow));
        public static ICommand Robot = new RoutedCommand("Robot", typeof(BaseWindow));
        public static ICommand AddIn = new RoutedCommand("AddIn", typeof(BaseWindow));
    }
}
