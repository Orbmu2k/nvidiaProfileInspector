namespace nvidiaProfileInspector.UI.ViewModels
{
    using System;
    using System.Windows.Input;

    public class InputViewModel : ViewModelBase
    {
        private string _inputValue;
        private string _title;
        private string _prompt;
        private bool _allowBrowse;
        private Func<string, string> _validationFunc;

        public InputViewModel(string title, string prompt, string defaultValue = "", bool allowBrowse = false, Func<string, string> validationFunc = null)
        {
            _title = title;
            _prompt = prompt;
            _inputValue = defaultValue;
            _allowBrowse = allowBrowse;
            _validationFunc = validationFunc;

            ClearCommand = new RelayCommand(o =>
            {
                InputValue = string.Empty;
                OnPropertyChanged(nameof(InputValue));
                OnPropertyChanged(nameof(CanSubmit));
            });

            Validate();
        }

        public ICommand ClearCommand { get; }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value, nameof(Title));
        }

        public string Prompt
        {
            get => _prompt;
            set => SetProperty(ref _prompt, value, nameof(Prompt));
        }

        public string InputValue
        {
            get => _inputValue;
            set
            {
                if (SetProperty(ref _inputValue, value, nameof(InputValue)))
                {
                    Validate();
                }
            }
        }

        public bool AllowBrowse
        {
            get => _allowBrowse;
            set => SetProperty(ref _allowBrowse, value, nameof(AllowBrowse));
        }

        public bool CanSubmit => !HasErrors;

        private void Validate()
        {
            ClearErrors(nameof(InputValue));


            if (_validationFunc != null)
            {
                var error = _validationFunc(InputValue);
                if (!string.IsNullOrEmpty(error))
                {
                    AddError(nameof(InputValue), error);
                }
            }

            OnPropertyChanged(nameof(CanSubmit));
        }
    }
}
