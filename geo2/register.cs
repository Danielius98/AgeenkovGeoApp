using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AeroSpectroApp
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _context;
        private readonly Window _window;
        private string _username;
        private string _password;
        private string _confirmPassword;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
                CommandManager.InvalidateRequerySuggested(); // Обновить состояние команды
            }
        }

        public ICommand RegisterCommand { get; }
        public ICommand CancelCommand { get; }

        public RegisterViewModel(Window window)
        {
            _window = window;
            _context = new AppDbContext();
            RegisterCommand = new RelayCommand(Register, CanRegister);
            CancelCommand = new RelayCommand(Cancel);

            // Подписка на изменения PasswordBox
            if (_window != null)
            {
                if (_window.FindName("PasswordBox") is PasswordBox passwordBox)
                {
                    passwordBox.PasswordChanged += (s, e) => CommandManager.InvalidateRequerySuggested();
                }
                if (_window.FindName("ConfirmPasswordBox") is PasswordBox confirmPasswordBox)
                {
                    confirmPasswordBox.PasswordChanged += (s, e) => CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private bool CanRegister(object parameter)
        {
            if (!(parameter is Window window))
                return false;

            var passwordBox = window.FindName("PasswordBox") as PasswordBox;
            var confirmPasswordBox = window.FindName("ConfirmPasswordBox") as PasswordBox;

            bool isValid = !string.IsNullOrWhiteSpace(Username) &&
                           passwordBox != null && !string.IsNullOrWhiteSpace(passwordBox.Password) &&
                           confirmPasswordBox != null && passwordBox.Password == confirmPasswordBox.Password;

            return isValid;
        }

        private void Register(object parameter)
        {
            try
            {
                if (!(parameter is Window window))
                {
                    MessageBox.Show("Ошибка: параметр окна не передан.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var passwordBox = window.FindName("PasswordBox") as PasswordBox;
                var confirmPasswordBox = window.FindName("ConfirmPasswordBox") as PasswordBox;

                if (passwordBox == null || confirmPasswordBox == null)
                {
                    MessageBox.Show("Ошибка: поля пароля не найдены.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Username))
                {
                    MessageBox.Show("Ошибка: имя пользователя не указано.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (passwordBox.Password != confirmPasswordBox.Password)
                {
                    MessageBox.Show("Ошибка: пароли не совпадают.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверка, существует ли пользователь
                if (_context.Users.Any(u => u.Username == Username))
                {
                    MessageBox.Show("Имя пользователя уже занято.", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Создание нового пользователя
                var user = new User
                {
                    Username = Username,
                    PasswordHash = User.HashPassword(passwordBox.Password)
                };

                // Создание нового клиента, связанного с пользователем
                var client = new Client
                {
                    Name = Username,
                    ContactInfo = "default@example.com",
                    User = user // Устанавливаем навигационное свойство
                };

                _context.Users.Add(user); // Добавляем пользователя
                _context.Clients.Add(client); // Добавляем клиента
                _context.SaveChanges(); // Сохраняем всё сразу

                MessageBox.Show($"Пользователь и клиент добавлены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                var loginWindow = new LoginWindow();
                loginWindow.Show();
                _window.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}\nInner Exception: {ex.InnerException?.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel(object parameter)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            _window.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}