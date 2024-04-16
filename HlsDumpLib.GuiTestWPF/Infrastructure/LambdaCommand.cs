using System;

namespace HlsDumpLib.GuiTestWPF
{
    internal class LambdaCommand : BaseCommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public LambdaCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public override bool CanExecute(object parameter)
        {
            return _canExecute != null ? _canExecute.Invoke(parameter) : _execute != null;
        }

        public override void Execute(object parameter)
        {
            _execute.Invoke(parameter);
        }
    }
}
