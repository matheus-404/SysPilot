using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace SysPilot
{
    public partial class MessageDialog : Window
    {
        public MessageDialog()
        {
            InitializeComponent();
        }

        public MessageDialog(string title, string message) : this()
        {
            Title = title;

            var titleTxt = this.FindControl<TextBlock>("TitleText");
            if (titleTxt != null) titleTxt.Text = title;

            var msgTxt = this.FindControl<TextBox>("MessageText");
            if (msgTxt != null) msgTxt.Text = message;
        }

        private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }

        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}