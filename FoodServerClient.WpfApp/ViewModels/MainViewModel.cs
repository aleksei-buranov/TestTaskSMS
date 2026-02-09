using System.Collections.ObjectModel;
using System.Windows.Input;
using FoodServerClient.WpfApp.Options;
using FoodServerClient.WpfApp.Services;
using Microsoft.Extensions.Options;

namespace FoodServerClient.WpfApp.ViewModels;

public sealed class MainViewModel
{
    private readonly EnvVarService _svc;
    private readonly EnvVarConfig _cfg;

    public ObservableCollection<EnvVarRow> Rows { get; } = new();

    public MainViewModel(EnvVarService svc, IOptions<EnvVarConfig> cfg)
    {
        _svc = svc;
        _cfg = cfg.Value;

        Load();
    }

    private void Load()
    {
        Rows.Clear();

        foreach (var raw in _cfg.EnvironmentVariables.Where(n => !string.IsNullOrWhiteSpace(n)))
        {
            var name = raw.Trim();

            var value = _svc.EnsureExists(name, "Default");
            var commentVar = EnvVarNames.CommentName(name);
            var comment = _svc.EnsureExists(commentVar, "Default");

            Rows.Add(new EnvVarRow(name, value, comment));
        }
    }

    public void SaveRow(EnvVarRow row)
    {
        if (row.IsValueChanged)
            _svc.Set(row.Name, row.Value, "auto-save (value)");

        if (row.IsCommentChanged)
            _svc.Set(EnvVarNames.CommentName(row.Name), row.Comment, "auto-save (comment)");

        row.AcceptChanges();
    }

    private sealed class RelayCommand : ICommand
    {
        private readonly Action _exec;

        public RelayCommand(Action exec)
        {
            _exec = exec;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _exec();
        }

        public event EventHandler? CanExecuteChanged;
    }
}