using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        private Int32 _SelectedAlgorithmIndex;
        public Int32 SelectedAlgorithmIndex { get => _SelectedAlgorithmIndex; set => _SelectedAlgorithmIndex = value; }
        public Boolean IsOpenEnabled { get => SelectedSizeIndex != -1; }
        public Boolean DataAlreadyReaded { get; private set; }
        private String _StatusMessage;
        public String StatusMessage { get => _StatusMessage; private set { _StatusMessage = value; OnPropertyChanged("StatusMessage"); }
        }


        // Események (a viewmodel-nek)


        // Delegate Commands

        public DelegateCommand BrowseDataFolderCommand { get; private set; }
        public DelegateCommand ReadAndLoadCommand { get; private set; }
        public DelegateCommand SimplifyTrajectoriesCommand { get; private set; }


        // Gyűjtemény típusú properties (View számára)

        private readonly String[] _ObservableSizes = { "All", "5", "10", "20", "50", "100", "200", "500" };
        public String[] ObservableSizes { get => _ObservableSizes; }
        private readonly String[] _ObservableAlgorithms = { "SP", "SP-Prac" };
        public String[] ObservableAlgorithms { get => _ObservableAlgorithms; }
        public ObservableCollection<Result> Results { get; private set; }


        // Metódusok

        public DPTSViewModel()
        {
            _Model = new DPTSModel();

            // a modell eseményeinek kezelése

            _Model.ErrorMessage += new EventHandler<StringMessageArgs>(OnErrorMessage);
            _Model.ResultMessage += new EventHandler<ResultMessageArgs>(OnResultMessage);
            _Model.StatusMessage += new EventHandler<StringMessageArgs>(OnStatusMessage);

            // inicializálások, kezdőállapot előállítása

            PathToDataRoot = _pathToDataRoot;
            Results = new ObservableCollection<Result>();

            BrowseDataFolderCommand = new DelegateCommand(param => BrowseDataFolder());
            ReadAndLoadCommand = new DelegateCommand(param => ReadAndLoad());
            SimplifyTrajectoriesCommand = new DelegateCommand(param => SimplifyTrajectories());
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
            Int32 limit = (ObservableSizes[SelectedSizeIndex] == "All") ? Int32.MaxValue  : Int32.Parse(ObservableSizes[SelectedSizeIndex]);
            _Model.OpenAndLoadFromFile(DataType.PLT, PathToDataRoot, limit);
            StatusMessage = "Adatok beolvasása KÉSZ.";
            DataAlreadyReaded = true;
            OnPropertyChanged("DataAlreadyReaded");
        }

        private void SimplifyTrajectories()
        {
            if (SelectedAlgorithmIndex == -1)
            {
                MessageBox.Show("Algoritmus megadása kötelező!", "DPTS", MessageBoxButton.OK);
            }

            switch (SelectedAlgorithmIndex)
            {
                case 0:
                    // SP
                    _Model.SimplifyTrajectories(AlgorithmType.SP);
                    break;
                case 1:
                    // SP-Prac
                    _Model.SimplifyTrajectories(AlgorithmType.SP_Prac);
                    break;
                case 2:
                    // SP-Theo
                    _Model.SimplifyTrajectories(AlgorithmType.SP_Theo);
                    break;
                case 3:
                    // SP-Both
                    _Model.SimplifyTrajectories(AlgorithmType.SP_Both);
                    break;
                case 4:
                    // Intersect
                    _Model.SimplifyTrajectories(AlgorithmType.Intersect);
                    break;
                default:
                    break;
            }
        }

        private void OnErrorMessage(object sender, StringMessageArgs e)
        {
            MessageBox.Show("Hiba:" + Environment.NewLine + e.Message, "DPTS", MessageBoxButton.OK);
        }

        private void OnResultMessage(object sender, ResultMessageArgs e)
        {
            Result r = Results.FirstOrDefault(x => x.No == e.ID);
            if (r == null)
            {
                r = new Result(e.ID);
                Results.Add(r);
            }
            switch (e.Result_Type)
            {
                case ResultType.Original:
                    r.setLengthOriginal(e.Length);
                    break;
                case ResultType.SP:
                    r.setLengthOptimal(e.Length);
                    r.setTime_SP(e.TimeInSecs);
                    break;
                case ResultType.SP_Prac:
                    r.setLengthOptimal(e.Length);
                    r.setTime_SP_Prac(e.TimeInSecs);
                    break;
                case ResultType.SP_Theo:
                    r.setLengthOptimal(e.Length);
                    r.setTime_SP_Theo(e.TimeInSecs);
                    break;
                case ResultType.SP_Both:
                    r.setLengthOptimal(e.Length);
                    r.setTime_SP_Both(e.TimeInSecs);
                    break;
                case ResultType.Intersect:
                    r.setLengthApproximative(e.Length);
                    r.setTime_Intersect(e.TimeInSecs);
                    break;
                default:
                    break;
            }
            Results = new ObservableCollection<Result>(Results);
            OnPropertyChanged("Results");
        }

        private void OnStatusMessage(object sender, StringMessageArgs e)
        {
            StatusMessage = e.Message;
        }
    }
}
