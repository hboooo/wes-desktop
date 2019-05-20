using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wes.Desktop.Windows.Controls
{
    public class NumericTextBox : TextBox
    {
        public NumericTextBox()
        {
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, null, (o1, e1) => { e1.Handled = true; }));
            InputMethod.SetIsInputMethodEnabled(this, false);
            this.PreviewTextInput += NumericTextBox_PreviewTextInput;
        }

        private void NumericTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");
            e.Handled = re.IsMatch(e.Text);
        }
    }
}
