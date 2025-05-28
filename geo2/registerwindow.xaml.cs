using System.Windows;

namespace AeroSpectroApp
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
            DataContext = new RegisterViewModel(this);
        }
    }
}