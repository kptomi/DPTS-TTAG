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

        private const string _pathExample = "C:\\Users\\User\\Documents\\Terinformatika\\4-Geolife\\Geolife Trajectories 1.3\\Data";

        // Privát adattagok
        private DPTSModel _Model;
        private Double _ErrorTolerance;


        // Publikus Properties (tulajdonságok) - ha csak a felületről szeretnénk módosítani, elég az automatikus getter/setter művelet

        private String _Path;
        public String Path { get => _Path; set { _Path = value; OnPropertyChanged("Path"); } }
        private Int32 _SelectedSizeIndex;
        public Int32 SelectedSizeIndex { get => _SelectedSizeIndex; set { _SelectedSizeIndex = value; OnPropertyChanged("IsOpenEnabled"); } }
        private String _ErrorToleranceString = "1";
        public String ErrorToleranceString { get => _ErrorToleranceString; set => _ErrorToleranceString = value; }
        private Int32 _SelectedAlgorithmIndex;
        public Int32 SelectedAlgorithmIndex { get => _SelectedAlgorithmIndex; set => _SelectedAlgorithmIndex = value; }
        public Boolean IsOpenEnabled { get => SelectedSizeIndex != -1; }
        public Boolean DataAlreadyReaded { get; private set; }
        private String _StatusMessage;
        public String StatusMessage { get => _StatusMessage; private set { _StatusMessage = value; OnPropertyChanged("StatusMessage"); } }


        // Események (a viewmodel-nek)


        // Delegate Commands

        public DelegateCommand BrowseDataFolderCommand { get; private set; }
        public DelegateCommand ReadAndLoadCommand { get; private set; }
        public DelegateCommand SimplifyTrajectoriesCommand { get; private set; }


        // Gyűjtemény típusú properties (View számára)

        private readonly String[] _ObservableSizes = { "All", "5", "10", "20", "50", "100", "200", "500", "1000", "2000", "5000", "10000" };
        public String[] ObservableSizes { get => _ObservableSizes; }
        private readonly String[] _ObservableAlgorithms = { "SP", "SP-Prac", "SP-Theo", "SP-Both", "Intersect" };
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

            Path = _pathExample;
            Results = new ObservableCollection<Result>();

            BrowseDataFolderCommand = new DelegateCommand(param => BrowseDataFolder());
            ReadAndLoadCommand = new DelegateCommand(param => ReadAndLoad());
            SimplifyTrajectoriesCommand = new DelegateCommand(param => SimplifyTrajectories());
        }

        
        private void BrowseDataFolder()
        {
            // TODO : kiválasztani a root adat-foldert
            DataAlreadyReaded = false;
        }


        private void ReadAndLoad()
        {
            Int32 limit = (ObservableSizes[SelectedSizeIndex] == "All") ? Int32.MaxValue  : Int32.Parse(ObservableSizes[SelectedSizeIndex]);
            _Model.OpenAndLoadFromFile(DataType.PLT, Path, limit);
            DataAlreadyReaded = true;
            OnPropertyChanged("DataAlreadyReaded");
        }


        private void SimplifyTrajectories()
        {
            // az interfészről érkező paraméterek ellenőrzése

            if (!ParseStringToDouble(ref _ErrorTolerance, ErrorToleranceString))
            {
                MessageBox.Show("You must enter a valid floating point number as an error tolerance!", "DPTS application", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            if (SelectedAlgorithmIndex == -1)
            {
                MessageBox.Show("You must select an algorithm!", "DPTS application", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            AlgorithmType algorithm;
            switch (SelectedAlgorithmIndex)
            {
                case 0:
                    // SP
                    algorithm = AlgorithmType.SP;
                    break;
                case 1:
                    // SP-Prac
                    algorithm = AlgorithmType.SP_Prac;
                    break;
                case 2:
                    // SP-Theo
                    algorithm = AlgorithmType.SP_Theo;
                    break;
                case 3:
                    // SP-Both
                    algorithm = AlgorithmType.SP_Both;
                    break;
                case 4:
                    // Intersect
                    algorithm = AlgorithmType.Intersect;
                    break;
                default:
                    throw new NotImplementedException();
                    //break;
            }

            _Model.SimplifyTrajectories(algorithm, _ErrorTolerance);
        }


        private Boolean ParseStringToDouble(ref Double outNumber, String numberString)
        {
            return outNumber != 0 || Double.TryParse(numberString, out outNumber);
        }


        private void OnErrorMessage(object sender, StringMessageArgs e)
        {
            MessageBox.Show("Hiba:" + Environment.NewLine + e.Message, "DPTS application", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }


        private void OnResultMessage(object sender, ResultMessageArgs e)
        {
            if (e.Result_Type == ResultType.Reinitialize)
            {
                // törli a teljes tartalmat
                Results = new ObservableCollection<Result>();
                OnPropertyChanged("Results");
                return;
            }

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

            if (e.Result_Type != ResultType.Original)
            {
                StatusMessage = _Model.GetStatusStringOfSimplify();
            }
        }


        private void OnStatusMessage(object sender, StringMessageArgs e)
        {
            StatusMessage = e.Message;
        }
    }
}
