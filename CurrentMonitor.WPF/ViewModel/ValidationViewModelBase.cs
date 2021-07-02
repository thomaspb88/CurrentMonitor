using Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CurrentMonitor.WPF.ViewModel
{
    public class ValidationViewModelBase : BindableBase, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<string>> _errorsByPropertyName = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public bool HasErrors => _errorsByPropertyName.Count > 0;
        public IEnumerable GetErrors(string propertyName)
        {
            return _errorsByPropertyName.ContainsKey(propertyName) ?
                _errorsByPropertyName[propertyName] : null;
        }

        public void ValidateProperty(object value, [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentException("Inavlid property name", propertyName);

            ClearErrors(propertyName);

            var results = new List<ValidationResult>();
            _ = Validator.TryValidateProperty(value, new ValidationContext(this) { MemberName = propertyName }, results);

            foreach (var item in results)
            {
                AddError(propertyName, item.ErrorMessage);
            }
        }

        protected T GetValue<T>([CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentException("Inavlid property name", propertyName);

            if (!_values.TryGetValue(propertyName, out object value))
            {
                value = default(T);
                _values.Add(propertyName, value);
            }
            return (T)value;
        }

        protected void SetValue<T>(T value, [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentException("Inavlid property name", propertyName);

            _values[propertyName] = value;

            RaisePropertyChanged(propertyName);
            ValidateProperty(value, propertyName);
        }

        private void AddError(string propertyName, string error)
        {
            if (!_errorsByPropertyName.ContainsKey(propertyName))
                _errorsByPropertyName[propertyName] = new List<string>();

            if (!_errorsByPropertyName[propertyName].Contains(error))
            {
                _errorsByPropertyName[propertyName].Add(error);
                OnErrorsChanged(propertyName);
            }
        }

        private void ClearErrors(string propertyName)
        {
            if (_errorsByPropertyName.ContainsKey(propertyName))
            {
                _errorsByPropertyName.Remove(propertyName);
                OnErrorsChanged(propertyName);
            }
        }

        private void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}