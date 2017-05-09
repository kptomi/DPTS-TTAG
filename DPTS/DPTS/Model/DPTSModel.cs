using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace DPTS.Model
{
    public enum DataType { PLT };

    public enum AlgorithmType { SP, SP_Prac, SP_Theo, SP_Both, Intersect };

    public class DPTSModel
    {

        // Konstansok


        // Privát adattagok

        private List<Trajectory> _OriginalTrajectories;
        private List<Trajectory> _SimplifiedTrajectories;


        // Események (a viewmodel-nek)

        public event EventHandler<StringMessageArgs> ErrorMessage;
        public event EventHandler<ResultMessageArgs> ResultMessage;
        public event EventHandler<StringMessageArgs> StatusMessage;


        // Metódusok

        public DPTSModel()
        {
            InitializeTrajectoriesAndResults();
        }

        private void InitializeTrajectoriesAndResults()
        {
            _OriginalTrajectories = new List<Trajectory>();
            _SimplifiedTrajectories = new List<Trajectory>();
        }

        public void OpenAndLoadFromFile(DataType data_type, String path_to_data_root, Int32 limit) {
            // TOTO : normális hibaüzenetet adni
            if (!Directory.Exists(path_to_data_root))
            {
                ErrorMessage(this, new StringMessageArgs("A megadott könyvtár nem létezik."));
            }

            InitializeTrajectoriesAndResults();
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
            // TODO : ellenőrizni a struktúra helyességét

            String[] directoryEntries = Directory.GetDirectories(path_to_data_root);

            Int32 i = 0;
            while (i < limit && i < directoryEntries.Length)
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
            trajectory.setStartTime(startTime);
            trajectory.setEndTime(endTime);
            _OriginalTrajectories.Add(trajectory);
            ResultMessage(this, new ResultMessageArgs(_OriginalTrajectories.Count - 1, ResultType.Original, trajectory.NumberOfPoints, 0));
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
                        StatusMessage(this, new StringMessageArgs("Egyszerűsítés az SP algoritmussal... 0/" + _OriginalTrajectories.Count));
                        for (Int32 i = 0; i < _OriginalTrajectories.Count; ++i)
                        {
                            StatusMessage(this, new StringMessageArgs("Egyszerűsítés az SP algoritmussal... " + (i + 1) + "/" + _OriginalTrajectories.Count));
                            // step 0
                            // egymás utáni azonos pontok sorozatának helyettesítése egyetlen ponttal
                            Trajectory removedSeqsFromTrajectory = RemoveSequences(_OriginalTrajectories[i]);
                            DateTime timeStart = DateTime.Now;
                            Trajectory t = SimplifyBySP(removedSeqsFromTrajectory, errorTolerance);
                            Double timeDifference = (DateTime.Now - timeStart).TotalSeconds;
                            _SimplifiedTrajectories.Add(t);
                            SendResultMessageFromThread(i, ResultType.SP, t.NumberOfPoints, timeDifference);
                        }
                        StatusMessage(this, new StringMessageArgs("Egyszerűsítés az SP algoritmussal KÉSZ."));
                        break;
                    case AlgorithmType.SP_Prac:
                        StatusMessage(this, new StringMessageArgs("Egyszerűsítés az SP-Prac algoritmussal... 0/" + _OriginalTrajectories.Count));
                        for (Int32 i = 0; i < _OriginalTrajectories.Count; ++i)
                        {
                            StatusMessage(this, new StringMessageArgs("Egyszerűsítés az SP-Prac algoritmussal... " + (i + 1) + "/" + _OriginalTrajectories.Count));
                            // step 0
                            // egymás utáni azonos pontok sorozatának helyettesítése egyetlen ponttal
                            Trajectory removedSeqsFromTrajectory = RemoveSequences(_OriginalTrajectories[i]);
                            DateTime timeStart = DateTime.Now;
                            Trajectory t = SimplifyBySP_Prac(removedSeqsFromTrajectory, errorTolerance);
                            Double timeDifference = (DateTime.Now - timeStart).TotalSeconds;
                            _SimplifiedTrajectories.Add(t);
                            SendResultMessageFromThread(i, ResultType.SP_Prac, t.NumberOfPoints, timeDifference);
                        }
                        StatusMessage(this, new StringMessageArgs("Egyszerűsítés az SP-Prac algoritmussal KÉSZ."));
                        break;
                    case AlgorithmType.SP_Theo:
                        StatusMessage(this, new StringMessageArgs("Egyszerűsítés az SP-Theo algoritmussal... 0/" + _OriginalTrajectories.Count));
                        for (Int32 i = 47; i < 48; ++i)
                        {
                            StatusMessage(this, new StringMessageArgs("Egyszerűsítés az SP-Theo algoritmussal... " + (i + 1) + "/" + _OriginalTrajectories.Count));
                            // step 0
                            // egymás utáni azonos pontok sorozatának helyettesítése egyetlen ponttal
                            Trajectory removedSeqsFromTrajectory = RemoveSequences(_OriginalTrajectories[i]);
                            DateTime timeStart = DateTime.Now;
                            Trajectory t = SimplifyBySP_Theo(removedSeqsFromTrajectory, errorTolerance);
                            Double timeDifference = (DateTime.Now - timeStart).TotalSeconds;
                            _SimplifiedTrajectories.Add(t);
                            SendResultMessageFromThread(i, ResultType.SP_Theo, t.NumberOfPoints, timeDifference);
                        }
                        StatusMessage(this, new StringMessageArgs("Egyszerűsítés az SP-Theo algoritmussal KÉSZ."));
                        break;
                    case AlgorithmType.SP_Both:
                        break;
                    case AlgorithmType.Intersect:
                        for (Int32 i = 0; i < _OriginalTrajectories.Count; ++i)
                        {
                            StatusMessage(this, new StringMessageArgs("Egyszerűsítés az Intersect algoritmussal... " + (i + 1) + "/" + _OriginalTrajectories.Count));
                            // step 0
                            // egymás utáni azonos pontok sorozatának helyettesítése egyetlen ponttal
                            Trajectory removedSeqsFromTrajectory = RemoveSequences(_OriginalTrajectories[i]);
                            DateTime timeStart = DateTime.Now;
                            Trajectory t = SimplifyByIntersectAlg(removedSeqsFromTrajectory, errorTolerance);
                            Double timeDifference = (DateTime.Now - timeStart).TotalSeconds;
                            _SimplifiedTrajectories.Add(t);
                            SendResultMessageFromThread(i, ResultType.Intersect, t.NumberOfPoints, timeDifference);
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
            for (Int32 i = 0; i < originalTrajectory.NumberOfPoints; )
            {
                Point reference = originalTrajectory[i];
                removedSequences.Add(reference);
                if (removedSequences.NumberOfPoints == 84)
                {
                    ;
                }
                Int32 j = i + 1;
                while (j < originalTrajectory.NumberOfPoints && reference.Equals(originalTrajectory[j]))
                {
                    ++j;
                }
                i = j;
            }
            return removedSequences;
        }

        private Trajectory SimplifyBySP(Trajectory trajectory, Double error_tolerance)
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

        private List<Int32>[] ConstructGraph(Trajectory trajectory, Double error_tolerance)
        {
            Int32 numberOfVertexes = trajectory.NumberOfPoints;

            List<Int32>[] graph = new List<Int32>[trajectory.NumberOfPoints]; // éllistás ábrázoláshoz

            // élek meghatározása
            for (Int32 i = 0; i < graph.Length; ++i)
            {
                // csak az adott csúcsnál nagyobb indexű csúcsba vezethet él
                for (Int32 j = i + 1; j < numberOfVertexes; ++j)
                {
                    Double simplificationErrorOnSegment = GetSimplificationErrorOnSegment(trajectory, i, j);
                    if (simplificationErrorOnSegment <= error_tolerance)
                    {
                        graph[i].Add(j);
                    }
                }
            }

            return graph;
        }

        // epszilon az egyszerűsített trajektória egy szakaszára összevetve az eredeti trajektória megfelelő darabjaival - maximumkiválasztás
        private Double GetSimplificationErrorOnSegment(Trajectory trajectory, Int32 i, Int32 j)
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

            return directionMaximalDifference;
        }

        private Double GetDirection(Point from, Point to)
        {
            Double direction = Math.Atan((to.Latitude - from.Latitude) / (to.Longitude - from.Longitude));
            // korrekció
            if (direction < 0)
            {
                direction += Math.PI * 2;
            }
            return direction;
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

        private Trajectory SimplifyBySP_Prac(Trajectory trajectory, Double error_tolerance)
        {
            Int32[] prev = new Int32[trajectory.NumberOfPoints];

            List<HashSet<IndexedPoints>> H_Sets = new List<HashSet<IndexedPoints>>();

            HashSet<IndexedPoints> H0_Set = new HashSet<IndexedPoints> { new IndexedPoints(0, trajectory[0]) };
            H_Sets.Add(H0_Set);

            List<IndexedPoints> U_Set = new List<IndexedPoints>();
            for (Int32 i = 1; i < trajectory.NumberOfPoints; ++i)
            {
                U_Set.Add(new IndexedPoints(i, trajectory[i]));
            }

            //Int32 k = 1;

            Boolean ready = false;
            while (!ready)
            {
                HashSet<IndexedPoints> Hk_Set = new HashSet<IndexedPoints>();
                // process positions in H(k-1)_Set and U in a reversed order
                for (Int32 i = U_Set.Count - 1; !ready && i >= 0; --i)
                {
                    IndexedPoints pj = U_Set[i];
                    foreach (IndexedPoints pi in H_Sets[H_Sets.Count - 1])
                    {
                        if (pi.Index >= pj.Index)
                        {
                            continue;
                        }
                        Double simplificationErrorOnSegment = GetSimplificationErrorOnSegment(trajectory, pi.Index, pj.Index);
                        if (simplificationErrorOnSegment <= error_tolerance)
                        {
                            // vezet el pi-ből pj-be
                            prev[pj.Index] = pi.Index;
                            if (pj.Index == trajectory.NumberOfPoints - 1)
                            {
                                // eljutottunk az utolsó pontba
                                ready = true;
                                break;
                            }
                            U_Set.Remove(pj);
                            Hk_Set.Add(pj);
                        }
                    }
                }
                H_Sets.Add(Hk_Set);
                //++k;
            }

            // solution generation
            List<Int32> shortestPathIndexes = new List<Int32>();
            Int32 current = trajectory.NumberOfPoints - 1; // az utolsó csúcs mindenképpen eleme az egyszerűsített trajektóriának, ide kell eljutni
            shortestPathIndexes.Insert(0, current);
            while (current != 0)
            {
                // még nem értünk el a kiinduló pontba
                current = prev[current];
                shortestPathIndexes.Insert(0, current);
            }

            Trajectory simplifiedTrajectory = new Trajectory();
            for (Int32 i = 0; i < shortestPathIndexes.Count; ++i)
            {
                simplifiedTrajectory.Add(trajectory[shortestPathIndexes[i]]);
            }
            return simplifiedTrajectory;
        }

        private Trajectory SimplifyBySP_Theo(Trajectory trajectory, Double error_tolerance)
        {
            // step 1 - graph construction
            Boolean[,] graph = ConstructGraphWithFDR(trajectory, error_tolerance);
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

        private Boolean[,] ConstructGraphWithFDR(Trajectory trajectory, Double error_tolerance)
        {
            Int32 numberOfVertexes = trajectory.NumberOfPoints;
            Boolean[,] graph = new Boolean[numberOfVertexes, numberOfVertexes];
            // feltöltjük a csúcsmátrixot "hamis" értékekkel
            for (Int32 i = 0; i < graph.GetLength(0); ++i)
            {
                for (Int32 j = 0; j < graph.GetLength(1); ++j)
                {
                    graph[i, j] = false;
                }
            }

            DirectionRange[,] FDR_matrix = GetFDRsForTrajectory(trajectory, error_tolerance);

            // élek meghatározása
            for (Int32 i = 0; i < graph.GetLength(0); ++i)
            {
                // négyzetes mátrix
                for (Int32 j = i + 1; j < graph.GetLength(0); ++j)
                {
                    graph[i, j] = FDR_matrix[i,j].IsDirectionInRange(GetDirection(trajectory[i], trajectory[j]));
                }
            }

            return graph;
        }

        private DirectionRange[,] GetFDRsForTrajectory(Trajectory trajectory, Double error_tolerance)
        {
            // feasible direction range-ek számítása
            Int32 numberOfVertexes = trajectory.NumberOfPoints;
            DirectionRange[,] FDR_matrix = new DirectionRange[numberOfVertexes, numberOfVertexes];
            // (r == 1) eset
            for (Int32 h = 0; h < numberOfVertexes - 1; ++h)
            {
                Double direction = GetDirection(trajectory[h], trajectory[h + 1]);
                FDR_matrix[h, h + 1] = new DirectionRange(direction, error_tolerance);
            }
            // (r > 1) esetek
            for (Int32 r = 2; r < numberOfVertexes; ++r)
            {
                if (r % 5 == 0)
                {
                    Console.WriteLine("R: " + r);
                }
                for (Int32 h = 0; h < numberOfVertexes - r; ++h)
                {
                    FDR_matrix[h, h + r] = DirectionRange.Intersect(FDR_matrix[h, h + r - 1], FDR_matrix[h + r - 1, h + r]);
                }
            }

            return FDR_matrix;
        }

        private Trajectory SimplifyByIntersectAlg(Trajectory trajectory, Double error_tolerance)
        {
            Int32 numberOfVertexes = trajectory.NumberOfPoints;
            DirectionRange[,] FDR_matrix = new DirectionRange[numberOfVertexes, numberOfVertexes];

            Trajectory simplifiedTrajectory = new Trajectory();
            simplifiedTrajectory.Add(trajectory[0]);
            Int32 e = 0;
            Int32 h = 1;
            while (h < trajectory.NumberOfPoints)
            {
                while (h < trajectory.NumberOfPoints && isSegmentFeasible(trajectory, e, h, error_tolerance / 2, FDR_matrix))
                {
                    ++h;
                }
                e = h - 1;
                simplifiedTrajectory.Add(trajectory[e]);
            }
            return simplifiedTrajectory;
        }

        private Boolean isSegmentFeasible(Trajectory t, Int32 indexFrom, Int32 IndexTo, Double error_tolerance, DirectionRange[,] FDR_matrix)
        {
            if (FDR_matrix[indexFrom, IndexTo] == null)
            {
                if (indexFrom >= IndexTo)
                {
                    // exception
                    throw new NotImplementedException();
                    ;
                }
                else if (indexFrom + 1 == IndexTo)
                {
                    FDR_matrix[indexFrom, IndexTo] = new DirectionRange(GetDirection(t[indexFrom], t[IndexTo]), error_tolerance);
                    return true;
                }
                else
                {
                    FDR_matrix[IndexTo - 1, IndexTo] = new DirectionRange(GetDirection(t[IndexTo - 1], t[IndexTo]), error_tolerance);
                    FDR_matrix[indexFrom, IndexTo] = DirectionRange.Intersect(FDR_matrix[indexFrom, IndexTo - 1], FDR_matrix[IndexTo - 1, IndexTo]);
                    return FDR_matrix[indexFrom, IndexTo].NumberOFRanges() != 0;
                }
            }
            else
            {
                return FDR_matrix[indexFrom, IndexTo].NumberOFRanges() != 0;
            }
        }

        private void SendResultMessageFromThread(Int32 id, ResultType resultType, Int32 length, Double timeInSecs)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { ResultMessage(this, new ResultMessageArgs(id, resultType, length, timeInSecs)); }));
        }
    }
}
