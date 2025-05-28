using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AeroSpectroApp
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _context = new AppDbContext();
        private readonly Window _window;
        private string _username;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand OpenRegisterCommand { get; }

        public LoginViewModel(Window window)
        {
            _window = window;
            LoginCommand = new RelayCommand(Login, CanLogin);
            OpenRegisterCommand = new RelayCommand(OpenRegister);
        }

        private bool CanLogin(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Username) && parameter is PasswordBox passwordBox && !string.IsNullOrWhiteSpace(passwordBox.Password);
        }

        private void Login(object parameter)
        {
            if (parameter is PasswordBox passwordBox)
            {
                var user = _context.Users.FirstOrDefault(u => u.Username == Username);
                if (user != null && User.VerifyPassword(passwordBox.Password, user.PasswordHash))
                {
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    _window.Close();
                }
                else
                {
                    MessageBox.Show("Неверное имя пользователя или пароль.", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenRegister(object parameter)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.Show();
            _window.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}