using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Maps
{
    class RelayCommand : ICommand
    {
        Action onExecute;
        Func<bool> canExecute;

        public RelayCommand(Action onExecute, Func<bool> canExecute)
        {
            this.onExecute = onExecute;
            this.canExecute = canExecute;
        }

        public RelayCommand(Action onExecute)
            : this(onExecute, () => true)
        {
        }

        public bool CanExecute(object parameter)
        {
            return canExecute();
        }

        public event EventHandler CanExecuteChanged;
        private Action action;

        public void Execute(object parameter)
        {
            onExecute();
        }
    }   
}
