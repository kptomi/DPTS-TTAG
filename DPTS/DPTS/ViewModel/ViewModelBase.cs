using System;
using System.ComponentModel;

namespace DPTS.ViewModel
{
    /// <summary>
    /// Nézetmodell ősosztály típusa.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Tulajdonság változásának eseménye.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Tulajdonság változása ellenőrzéssel.
        /// </summary>
        /// <param name="propertyName">Tulajdonság neve.</param>
        protected virtual void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
