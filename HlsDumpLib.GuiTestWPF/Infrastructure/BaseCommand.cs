using System;
using System.Windows.Input;

namespace HlsDumpLib.GuiTestWPF
{
    internal abstract class BaseCommand : ICommand
    {
#pragma warning disable 67
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
#pragma warning restore 67

        public abstract bool CanExecute(object parameter);
        public abstract void Execute(object parameter);
    }
}
