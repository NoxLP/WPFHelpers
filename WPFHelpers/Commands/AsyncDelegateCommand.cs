using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WPFHelpers.Commands
{
    public class AsyncDelegateCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Func<object, Task> _ExecuteAsync;
        private SemaphoreSlim _Semaphore;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }        

        public AsyncDelegateCommand(Func<object, Task> execute, Predicate<object> canExecute, int semaphore = 1)
        {
            _ExecuteAsync = execute;
            _canExecute = canExecute;
            _Semaphore = new SemaphoreSlim(semaphore);
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }

            return _canExecute(parameter);
        }

        public async void Execute(object parameter)
        {
            if (_Semaphore.CurrentCount == 0)
                return;

            await _Semaphore.WaitAsync();
            await _ExecuteAsync(parameter);
            _Semaphore.Release();
        }

        public void RaiseCanExecuteChanged()
        {
            //if (CanExecuteChanged != null)
            //{
            //    CanExecuteChanged(this, EventArgs.Empty);
            //}
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
