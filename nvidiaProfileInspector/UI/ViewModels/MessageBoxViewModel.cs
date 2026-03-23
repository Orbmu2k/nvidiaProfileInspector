namespace nvidiaProfileInspector.UI.ViewModels
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;

    public class MessageBoxViewModel : ViewModelBase
    {
        private string _message;
        private string _title;
        private MessageBoxButton _buttons;
        private MessageBoxImage _icon;
        private Visibility _iconVisibility;
        private string _iconPath;
        private System.Windows.Media.Brush _iconColor;

        public MessageBoxResult Result { get; private set; }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value, nameof(Message));
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value, nameof(Title));
        }

        public MessageBoxButton Buttons
        {
            get => _buttons;
            set => SetProperty(ref _buttons, value, nameof(Buttons));
        }

        public MessageBoxImage Icon
        {
            get => _icon;
            set
            {
                if (SetProperty(ref _icon, value, nameof(Icon)))
                {
                    IconVisibility = value != MessageBoxImage.None ? Visibility.Visible : Visibility.Collapsed;
                    IconPath = GetIconPath(value);
                    IconColor = GetIconColor(value);
                }
            }
        }

        public Visibility IconVisibility
        {
            get => _iconVisibility;
            set => SetProperty(ref _iconVisibility, value, nameof(IconVisibility));
        }

        public string IconPath
        {
            get => _iconPath;
            set => SetProperty(ref _iconPath, value, nameof(IconPath));
        }

        public System.Windows.Media.Brush IconColor
        {
            get => _iconColor;
            set => SetProperty(ref _iconColor, value, nameof(IconColor));
        }

        public MessageBoxViewModel()
        {
        }

        public MessageBoxViewModel(string message, string title = "", MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None)
        {
            Message = message;
            Title = title;
            Buttons = buttons;
            Icon = icon;
        }

        private string GetIconPath(MessageBoxImage icon)
        {
            return icon switch
            {
                // Information: Circle with 'i' (Fluent style)
                MessageBoxImage.Information => "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z",
                // Warning: Triangle with '!' (Fluent style)
                MessageBoxImage.Warning => "M1 21h22L12 2 1 21zm12-3h-2v-2h2v2zm0-4h-2v-4h2v4z",
                // Error: Circle with 'X' (Fluent style)
                MessageBoxImage.Error => "M12 2C6.47 2 2 6.47 2 12s4.47 10 10 10 10-4.47 10-10S17.53 2 12 2zm5 13.59L15.59 17 12 13.41 8.41 17 7 15.59 10.59 12 7 8.41 8.41 7 12 10.59 15.59 7 17 8.41 13.41 12 17 15.59z",
                // Question: Circle with '?' (Fluent style)
                MessageBoxImage.Question => "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 17h-2v-2h2v2zm2.07-7.75l-.9.92C13.45 12.9 13 13.5 13 15h-2v-.5c0-1.1.45-2.1 1.17-2.83l1.24-1.26c.37-.36.59-.86.59-1.41 0-1.1-.9-2-2-2s-2 .9-2 2H8c0-2.21 1.79-4 4-4s4 1.79 4 4c0 .88-.36 1.68-.93 2.25z",
                _ => ""
            };
        }


        private System.Windows.Media.Brush GetIconColor(MessageBoxImage icon)
        {
            var app = Application.Current;
            return icon switch
            {
                MessageBoxImage.Warning => app.TryFindResource("WarningBrush") as System.Windows.Media.Brush ?? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange),
                MessageBoxImage.Error => app.TryFindResource("ErrorBrush") as System.Windows.Media.Brush ?? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red),
                MessageBoxImage.Question => app.TryFindResource("QuestionBrush") as System.Windows.Media.Brush ?? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Blue),
                MessageBoxImage.Information => app.TryFindResource("NvidiaGreenBrush") as System.Windows.Media.Brush ?? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 122, 64)),
                _ => app.TryFindResource("NvidiaGreenBrush") as System.Windows.Media.Brush ?? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 122, 64))
            };
        }

        public ICommand OKCommand => new RelayCommand(OnOK, CanExecute);
        public ICommand YesCommand => new RelayCommand(OnYes, CanExecuteYes);
        public ICommand NoCommand => new RelayCommand(OnNo, CanExecuteNo);
        public ICommand CancelCommand => new RelayCommand(OnCancel, CanExecuteCancel);

        private bool CanExecute(object parameter) => true;
        private bool CanExecuteYes(object parameter) => Buttons == MessageBoxButton.YesNo || Buttons == MessageBoxButton.YesNoCancel || Buttons == MessageBoxButton.YesNo;
        private bool CanExecuteNo(object parameter) => Buttons == MessageBoxButton.YesNo || Buttons == MessageBoxButton.YesNoCancel;
        private bool CanExecuteCancel(object parameter) => Buttons == MessageBoxButton.OKCancel || Buttons == MessageBoxButton.YesNoCancel;

        private void OnOK(object parameter)
        {
            Result = MessageBoxResult.OK;
            CloseDialog();
        }

        private void OnYes(object parameter)
        {
            Result = MessageBoxResult.Yes;
            CloseDialog();
        }

        private void OnNo(object parameter)
        {
            Result = MessageBoxResult.No;
            CloseDialog();
        }

        private void OnCancel(object parameter)
        {
            Result = MessageBoxResult.Cancel;
            CloseDialog();
        }

        private void CloseDialog()
        {
            var window = Application.Current.Windows.Cast<Window>()
                .FirstOrDefault(w => w.DataContext == this);
            window?.Close();
        }

        public static MessageBoxResult Show(string message, string title = "", MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.Information)
        {
            var viewModel = new MessageBoxViewModel(message, title, buttons, icon);
            var dialog = new Views.Dialogs.MessageBoxView { DataContext = viewModel };

            var owner = Application.Current.Windows.Cast<Window>()
                .FirstOrDefault(w => w.IsActive) ?? Application.Current.MainWindow;

            if (owner != null && owner != dialog)
                dialog.Owner = owner;

            dialog.ShowDialog();
            return viewModel.Result;
        }
    }
}
