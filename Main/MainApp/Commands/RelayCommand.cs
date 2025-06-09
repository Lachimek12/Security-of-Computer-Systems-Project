using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainApp.Commands
{
    /// <summary>
    /// A command that relays its functionality to delegates.
    /// Implements the <see cref="ICommand"/> interface.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The action to execute when the command is triggered.</param>
        /// <param name="canExecute">Optional predicate to determine if the command can execute.</param>
        /// <exception cref="ArgumentNullException">Thrown if execute is null.</exception>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Defines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Unused parameter.</param>
        /// <returns>True if the command can execute; otherwise, false.</returns>
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Unused parameter.</param>
        public void Execute(object parameter) => _execute();
    }
}
