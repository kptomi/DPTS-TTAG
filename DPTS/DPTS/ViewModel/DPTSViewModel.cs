using System;
using System.Windows;

using DPTS.Model;

namespace DPTS.ViewModel
{
    public class DPTSViewModel : ViewModelBase
    {

        // Konstansok

        private const string _pathToDataRoot = "C:\\Users\\User\\Documents\\Terinformatika\\4-Geolife\\Geolife Trajectories 1.3\\Data";

        // Privát adattagok
        private DPTSModel _Model;


        // Publikus Properties (tulajdonságok) - ha csak a felületről szeretnénk módosítani, elég az automatikus getter/setter művelet

        private String _PathToDataRoot;
        public String PathToDataRoot { get => _PathToDataRoot; set { _PathToDataRoot = value; OnPropertyChanged("PathToDataRoot"); } }
        private Int32 _SelectedSizeIndex;
        public Int32 SelectedSizeIndex { get => _SelectedSizeIndex; set { _SelectedSizeIndex = value; OnPropertyChanged("IsOpenEnabled"); } }
        public Boolean IsOpenEnabled => SelectedSizeIndex != -1;
        public String OriginalTrajectoryLengths { get; private set; }
        public String SimplifiedOptimalTrajectoryLengths { get; private set; }
        public Boolean DataAlreadyReaded { get; private set; }
        private String _StatusMessage;
        public String StatusMessage { get => _StatusMessage; private set { _StatusMessage = value; OnPropertyChanged("StatusMessage"); }
        }


        // Események (a viewmodel-nek)


        // Delegate Commands

        public DelegateCommand BrowseDataFolderCommand { get; private set; }
        public DelegateCommand ReadAndLoadCommand { get; private set; }
        public DelegateCommand SimplifyBySPCommand { get; private set; }


        // Gyűjtemény típusú properties (View számára)

        private Int32[] _ObservableSizes = { 5, 10, 20, 40, 80 };
        public Int32[] ObservableSizes { get => _ObservableSizes; }


        // Metódusok

        public DPTSViewModel()
        {
            _Model = new DPTSModel();

            // a modell eseményeinek kezelése

            _Model.ErrorMessage += new EventHandler<StringMessageArgs>(OnErrorMessage);
            _Model.TrajectoryLengthMessage += new EventHandler<TrajectoryLengthArgs>(OnTrajectoryLengths);
            _Model.ErrorToleranceMessage += new EventHandler<ErrorToleranceArgs>(OnErrorTolerance);
            _Model.StatusMessage += new EventHandler<StringMessageArgs>(OnStatusMessage);

            // inicializálások, kezdőállapot előállítása

            PathToDataRoot = _pathToDataRoot;

            BrowseDataFolderCommand = new DelegateCommand(param => BrowseDataFolder());
            ReadAndLoadCommand = new DelegateCommand(param => ReadAndLoad());
            SimplifyBySPCommand = new DelegateCommand(param => SimplifyBySP());
        }

        private void BrowseDataFolder()
        {
            // TODO : kiválasztani a root adat-foldert
            DataAlreadyReaded = false;
            OnPropertyChanged("DataAlreadyReaded");
        }

        private void ReadAndLoad()
        {
            StatusMessage = "Adatok beolvasása a forrásból...";
            OriginalTrajectoryLengths = ""; OnPropertyChanged("OriginalTrajectoryLengths");
            SimplifiedOptimalTrajectoryLengths = ""; OnPropertyChanged("SimplifiedOptimalTrajectoryLengths");
            _Model.OpenAndLoadFromFile(DataType.PLT, PathToDataRoot, ObservableSizes[SelectedSizeIndex]);
            StatusMessage = "Adatok beolvasása KÉSZ.";
            DataAlreadyReaded = true;
            OnPropertyChanged("DataAlreadyReaded");
        }

        private void SimplifyBySP()
        {
            _Model.SimplifyTrajectories(AlgorithmType.SP);
        }

        private void OnErrorMessage(object sender, StringMessageArgs e)
        {
            MessageBox.Show("Hiba:" + Environment.NewLine + e.Message,
                "DPTS", MessageBoxButton.OK);
        }

        private void OnTrajectoryLengths(object sender, TrajectoryLengthArgs e)
        {
            switch (e.TRType)
            {
                case TrajectoryType.Original:
                    if (String.IsNullOrEmpty(OriginalTrajectoryLengths))
                    {
                        OriginalTrajectoryLengths = e.Length.ToString();
                    }
                    else
                    {
                        OriginalTrajectoryLengths += ", " + e.Length;
                    }
                    OnPropertyChanged("OriginalTrajectoryLengths");
                    break;
                case TrajectoryType.SimplifiedOptimal:
                    if (String.IsNullOrEmpty(SimplifiedOptimalTrajectoryLengths))
                    {
                        SimplifiedOptimalTrajectoryLengths = e.Length.ToString();
                    }
                    else if (SimplifiedOptimalTrajectoryLengths.EndsWith(Environment.NewLine))
                    {
                        SimplifiedOptimalTrajectoryLengths += e.Length;
                    }
                    else
                    {
                        SimplifiedOptimalTrajectoryLengths += ", " + e.Length;
                    }
                    OnPropertyChanged("SimplifiedOptimalTrajectoryLengths");
                    break;
                case TrajectoryType.SimplifiedApproximative:
                    break;
                default:
                    break;
            }
        }

        private void OnErrorTolerance(object sender, ErrorToleranceArgs e)
        {
            SimplifiedOptimalTrajectoryLengths += " (" + e.ErrorTolerance + ")" + Environment.NewLine;
            OnPropertyChanged("OriginalTrajectoryLengths");
        }

        private void OnStatusMessage(object sender, StringMessageArgs e)
        {
            StatusMessage = e.Message;
        }
    }
}
