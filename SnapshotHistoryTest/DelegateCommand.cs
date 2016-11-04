using System;
using System.Windows.Input;

namespace SnapshotHistoryTest
{
    internal class DelegateCommand : ICommand
    {
        private Action<object> p;

        public DelegateCommand(Action<object> p)
        {
            this.p = p;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            p(parameter);
        }
    }
}