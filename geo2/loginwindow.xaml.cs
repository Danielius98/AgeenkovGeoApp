using System.Windows;
using System.Windows.Controls;

namespace AeroSpectroApp
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            DataContext = new LoginViewModel(this);
        }
    }
}