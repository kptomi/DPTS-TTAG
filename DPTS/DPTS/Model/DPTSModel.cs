using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace DPTS.Model
{
    public enum DataType { PLT };

    public enum AlgorithmType { SP, Approximative};

    public class DPTSModel
    {

        // Konstansok


        // Privát adattagok

        private List<Trajectory> _OriginalTrajectories;
        private List<Trajectory> _SimplifiedTrajectoriesOptimal;
        private List<Trajectory> _SimplifiedTrajectorieApproximative;


        // Események (a viewmodel-nek)

        public event EventHandler<StringMessageArgs> ErrorMessage;
        public event EventHandler<TrajectoryLengthArgs> TrajectoryLengthMessage;
        public event EventHandler<ErrorToleranceArgs> ErrorToleranceMessage;
        public event EventHandler<StringMessageArgs> StatusMessage;


        // Metódusok

        public DPTSModel()
        {
            InitializeTrajectories();
        }

        private void InitializeTrajectories()
        {
            _OriginalTrajectories = new List<Trajectory>();
            _SimplifiedTrajectoriesOptimal = new List<Trajectory>();
            _SimplifiedTrajectorieApproximative = new List<Trajectory>();
        }

        public void OpenAndLoadFromFile(DataType data_type, String path_to_data_root, Int32 limit) {
            InitializeTrajectories();
            switch (data_type)
            {
                case DataType.PLT:
                    ReadDataFromPLT(path_to_data_root, limit);
                    break;
                default:
                    break;
            }
        }


        private void ReadDataFromPLT(String path_to_data_root, Int32 limit)
        {
            if (!Directory.Exists(path_to_data_root))
            {
                ErrorMessage(this, new StringMessageArgs("A megadott könyvtár nem létezik."));
            }

            // TODO : ellenőrizni a struktúra helyességét

            String[] directoryEntries = Directory.GetDirectories(path_to_data_root);

            Int32 i = 0;
            while (i < limit)
            {
                String[] fileEntries = Directory.GetFiles(directoryEntries[i] + "\\Trajectory");
                // TODO : kivenni a korlátozást
                Int32 maxJ = fileEntries.Length > (limit - i) ? (limit - i) : fileEntries.Length;
                for (Int32 j = 0; j < maxJ; ++j)
                {
                    String file = fileEntries[j];
                    ProcessPLTFile(file);
                }
                i += maxJ;
            }
        }

        public void ProcessPLTFile(String filename)
        {
            String input;
            String[] inputArray;
            StreamReader IFile = new StreamReader(filename);
            // az első 6 sor nem tartalmaz releváns adatot
            for (Int16 i = 0; i < 6; ++i)
            {
                IFile.ReadLine();
            }

            Trajectory trajectory = new Trajectory();
            String startTime = "", endTime = "";
            while (!String.IsNullOrEmpty(input = IFile.ReadLine()))
            {
                inputArray = input.Split(',');
                trajectory.Add(new Point(Single.Parse(inputArray[0], new CultureInfo("en-US")), Single.Parse(inputArray[1], new CultureInfo("en-US"))));
                if (String.IsNullOrEmpty(startTime))
                {
                    startTime = inputArray[5] + " " + inputArray[6];
                }
                endTime = inputArray[5] + " " + inputArray[6];
            }
            trajectory.StartTime = startTime;
            trajectory.EndTime = endTime;
            _OriginalTrajectories.Add(trajectory);
            TrajectoryLengthMessage(this, new TrajectoryLengthArgs(trajectory, TrajectoryType.Original));
        }

        public void SimplifyTrajectories(AlgorithmType algorithm_type)
        {
            Task task = Task.Run(() =>
            {
                // TODO : ezt is meg lehessen adni az interfészről
                Double errorTolerance = 0.785;
                switch (algorithm_type)
                {
                    case AlgorithmType.SP:
                        Double[] tolerances = { 0.785, 0.615, 0.47, 0.32, 0.2, 0 };
                        StatusMessage(this, new StringMessageArgs("Egyszerűsítés az SP algoritmussal... 0/" + tolerances.Length * _OriginalTrajectories.Count));
                        for (Int32 j = 0; j < tolerances.Length; ++j)
                        {
                            errorTolerance = tolerances[j];
                            for (Int32 i = 0; i < _OriginalTrajectories.Count; ++i)
                            {
                                StatusMessage(this, new StringMessageArgs("Egyszerűsítés az SP algoritmussal... " + (j * _OriginalTrajectories.Count + i + 1) + "/" + tolerances.Length * _OriginalTrajectories.Count));
                                // step 0
                                // egymás utáni azonos pontok sorozatának helyettesítése egyetlen ponttal
                                Trajectory removedSeqsFromTrajectory = RemoveSequences(_OriginalTrajectories[i]);
                                Trajectory t = SimplifyBySPAlg(removedSeqsFromTrajectory, errorTolerance);
                                _SimplifiedTrajectoriesOptimal.Add(t);
                                TrajectoryLengthMessage(this, new TrajectoryLengthArgs(t, TrajectoryType.SimplifiedOptimal));
                            }
                            ErrorToleranceMessage(this, new ErrorToleranceArgs(errorTolerance));
                        }
                        StatusMessage(this, new StringMessageArgs("Egyszerűsítés az SP algoritmussal KÉSZ."));
                        break;
                    case AlgorithmType.Approximative:
                        for (Int32 i = 0; i < _OriginalTrajectories.Count; ++i)
                        {
                            // step 0
                            // egymás utáni azonos pontok sorozatának helyettesítése egyetlen ponttal
                            Trajectory removedSeqsFromTrajectory = RemoveSequences(_OriginalTrajectories[i]);
                            Trajectory t = SimplifyByApproximativeAlg(removedSeqsFromTrajectory, errorTolerance);
                            _SimplifiedTrajectoriesOptimal.Add(t);
                            TrajectoryLengthMessage(this, new TrajectoryLengthArgs(t, TrajectoryType.SimplifiedApproximative));
                        }
                        break;
                    default:
                        break;
                }
            });
        }

        private Trajectory RemoveSequences(Trajectory originalTrajectory)
        {
            Trajectory removedSequences = new Trajectory();
            if (originalTrajectory.NumberOfPoints == 0)
            {
                return removedSequences;
            }
            Point reference = originalTrajectory[0];
            for (Int32 i = 0; i < originalTrajectory.NumberOfPoints; ++i)
            {
                removedSequences.Add(originalTrajectory[i]);
                Int32 j = i + 1;
                while (j < originalTrajectory.NumberOfPoints && reference.Equals(originalTrajectory[j]))
                {
                    ++j;
                }
                i = j;
            }
            return removedSequences;
        }

        private Trajectory SimplifyBySPAlg(Trajectory trajectory, Double error_tolerance)
        {
            // step 1 - graph construction
            Boolean[,] graph = ConstructGraph(trajectory, error_tolerance);
            // step 2 - shortest path finding
            List<Int32> shortestPathIndexes = GetShortestPath(graph);
            // step 3 solution generation
            Trajectory simplifiedTrajectory = new Trajectory();
            for (Int32 i = 0; i < shortestPathIndexes.Count; ++i)
            {
                simplifiedTrajectory.Add(trajectory[shortestPathIndexes[i]]);
            }
            return simplifiedTrajectory;
        }

        private Boolean[,] ConstructGraph(Trajectory trajectory, Double error_tolerance)
        {
            Int32 numberOfVertexes = trajectory.NumberOfPoints;
            Boolean[,] graph = new Boolean[numberOfVertexes,numberOfVertexes];
            // feltöltjük a csúcsmátrixot "hamis" értékekkel
            for (Int32 i = 0; i < graph.GetLength(0); ++i)
            {
                for (Int32 j = 0; j < graph.GetLength(1); ++j)
                {
                    graph[i, j] = false;
                }
            }

            // élek meghatározása
            for (Int32 i = 0; i < graph.GetLength(0); ++i)
            {
                // négyzetes mátrix
                for (Int32 j = i + 1; j < graph.GetLength(0); ++j)
                {
                    Double directionSimplified = GetDirection(trajectory[i], trajectory[j]);

                    Double directionMaximalDifference = GetDirectionDifference(directionSimplified, GetDirection(trajectory[i], trajectory[i + 1]));
                    for (Int32 k = i; k < j; ++k)
                    {
                        Double directionDifference = GetDirectionDifference(directionSimplified, GetDirection(trajectory[k], trajectory[k + 1]));
                        if (directionDifference > directionMaximalDifference)
                        {
                            directionMaximalDifference = directionDifference;
                        }
                    }
                    graph[i, j] = directionMaximalDifference <= error_tolerance;
                }
            }

            return graph;
        }

        private Double GetDirection(Point from, Point to)
        {
            return Math.Atan((to.Latitude - from.Latitude) / (to.Longitude - from.Longitude));
        }

        private Double GetDirectionDifference(Double direction1, Double direction2)
        {
            Double absDifference = Math.Abs(direction1 - direction2);
            return Math.Min(absDifference, Math.PI * 2 - absDifference);
        }

        // Dijkstra-algoritmus
        private List<Int32> GetShortestPath(Boolean[,] graph)
        {
            Int32 numberVertexes = graph.GetLength(0);

            Int32 source = 0;
            HashSet<Int32> q = new HashSet<Int32>();
            Int32[] dist = new Int32[numberVertexes];
            Int32[] prev = new Int32[numberVertexes];
            for (Int32 i = 0; i < numberVertexes; ++i)
            {
                dist[i] = Int32.MaxValue;
                prev[i] = -1;
                q.Add(i);
            }
            dist[source] = 0;
            while (q.Count != 0)
            {
                // a kiinduló csúcshoz legközelebbi csúcs, aki még benne van a "q" halmazban
                Int32 u = -1;
                Int32 minDist = Int32.MaxValue;
                foreach (Int32 index in q)
                {
                    if (dist[index] < minDist)
                    {
                        u = index;
                        minDist = dist[index];
                    }
                }
                q.Remove(u);
                // "u" csúcs szomszédainak frissítjük a távolságát, ha jobbat találtunk
                foreach (Int32 v in q)
                {
                    if (graph[u, v]) // van él "u" csúcsból az "v" csúcsba
                    {
                        Int32 alt = dist[u] + 1; // az élsúly minden esetben 1
                        if (alt < dist[v])
                        {
                            dist[v] = alt;
                            prev[v] = u;
                        }
                    }
                }
            }

            List<Int32> shortestPathIndexes = new List<Int32>();
            Int32 current = numberVertexes - 1; // az utolsó csúcs mindenképpen eleme az egyszerűsített trajektóriának, ide kell eljutni
            shortestPathIndexes.Insert(0, current);
            while (current != source)
            {
                current = prev[current];
                shortestPathIndexes.Insert(0, current);
            }

            return shortestPathIndexes;
        }

        private Trajectory SimplifyByApproximativeAlg(Trajectory trajectory, Double error_tolerance)
        {
            // közelítő algoritmus implementálása
            return new Trajectory();
        }
    }
}
