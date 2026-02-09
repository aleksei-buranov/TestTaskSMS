using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FoodServerClient.WpfApp.ViewModels;

public sealed class EnvVarRow : INotifyPropertyChanged
{
    public string Name { get; }

    public string OriginalValue { get; private set; }
    private string _value;

    public string Value
    {
        get => _value;
        set
        {
            if (_value == value) return;
            _value = value;
            OnPropertyChanged();
        }
    }

    public string OriginalComment { get; private set; }
    private string _comment;

    public string Comment
    {
        get => _comment;
        set
        {
            if (_comment == value) return;
            _comment = value;
            OnPropertyChanged();
        }
    }

    public EnvVarRow(string name, string value, string comment)
    {
        Name = name;

        OriginalValue = value;
        _value = value;

        OriginalComment = comment;
        _comment = comment;
    }

    public bool IsValueChanged => !string.Equals(Value, OriginalValue, StringComparison.Ordinal);
    public bool IsCommentChanged => !string.Equals(Comment, OriginalComment, StringComparison.Ordinal);

    public void AcceptChanges()
    {
        OriginalValue = Value;
        OriginalComment = Comment;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? prop = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}