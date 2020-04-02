using System.Windows;
using System.Windows.Input;

namespace UnitedTools.Chart
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        public MessageWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        public string Caption
        {
            get
            {
                return title.Content.ToString();
            }

            set
            {
                title.Content = value;
            }
        }

        public string Message
        {
            get
            {
                return userMessage.Text.ToString();
            }

            set
            {
                userMessage.Text = value;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
