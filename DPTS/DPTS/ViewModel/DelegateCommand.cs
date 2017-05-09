using System;
using System.Windows.Input;

namespace DPTS.ViewModel
{
    /// <summary>
    /// Általános parancs típusa.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Action<Object> _Execute; // a tevékenységet végrehajtó lambda-kifejezés
        private readonly Func<Object, Boolean> _CanExecute; // a tevékenység feltételét ellenőző lambda-kifejezés

        /// <summary>
        /// Parancs létrehozása.
        /// </summary>
        /// <param name="execute">Végrehajtandó tevékenység.</param>
        public DelegateCommand(Action<Object> execute) : this(null, execute) { }

        /// <summary>
        /// Parancs létrehozása.
        /// </summary>
        /// <param name="canExecute">Végrehajthatóság feltétele.</param>
        /// <param name="execute">Végrehajtandó tevékenység.</param>
        public DelegateCommand(Func<Object, Boolean> canExecute, Action<Object> execute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            _Execute = execute;
            _CanExecute = canExecute;
        }

        /// <summary>
        /// Végrehajthatóság változásának eseménye.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Végrehajthatóság ellenőrzése
        /// </summary>
        /// <param name="parameter">A tevékenység paramétere.</param>
        /// <returns>Igaz, ha a tevékenység végrehajtható.</returns>
        public Boolean CanExecute(Object parameter)
        {
            return _CanExecute == null ? true : _CanExecute(parameter);
        }

        /// <summary>
        /// Tevékenység végrehajtása.
        /// </summary>
        /// <param name="parameter">A tevékenység paramétere.</param>
        public void Execute(Object parameter)
        {
            _Execute(parameter);
        }

        /// <summary>
        /// Végrehajthatóság változásának eseménykiváltása.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }
    }
}
