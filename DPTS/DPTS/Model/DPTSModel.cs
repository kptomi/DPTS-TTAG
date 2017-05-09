using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace DPTS.Model
{
    public enum DataType { PLT };

    public enum AlgorithmType { SP, SP_Prac, SP_Theo, SP_Both, Intersect };

    public class DPTSModel
    {

        // Privát adattagok

        private List<Trajectory> _OriginalTrajectories;
        private ConcurrentBag<Trajectory> _SimplifiedTrajectories;
        private String _AlgorithmName = "";


        // Események (a viewmodel-nek)

        public event EventHandler<StringMessageArgs> ErrorMessage;
        public event EventHandler<ResultMessageArgs> ResultMessage;
        public event EventHandler<SimplifyStateMessageArgs> SimplifyStateMessage;
        public event EventHandler<StringMessageArgs> StatusMessage;


        // Metódusok

        public DPTSModel()
        {
            InitializeTrajectoriesAndResults();
        }


        private void InitializeTrajectoriesAndResults()
        {
            _OriginalTrajectories = new List<Trajectory>();
            _SimplifiedTrajectories = new ConcurrentBag<Trajectory>();
        }


        public void OpenAndLoadFromFile(DataType dataType, String path, Int32 limit) {
            if (!File.Exists(path) &&!Directory.Exists(path))
            {
                ErrorMessage(this, new StringMessageArgs("Browsed file or directory does not exist."));
                return;
            }

            InitializeTrajectoriesAndResults();

            Task task = Task.Run(() =>
            {
                SendStatusMessageFromThread("Load trajectory data from data source... 1/" + limit);
                if (File.Exists(path))
                {
                    ProcessFile(dataType, path);
                }
                else
                {
                    // könyvtár adott
                    SearchFilesInDirectory(dataType, path, limit, 0);
                }
                SendStatusMessageFromThread("Trajectory data has been load.");
            });
        }


        private Int32 SearchFilesInDirectory(DataType dataType, String path, Int32 limit, Int32 numberProcessedFiles)
        {
            String[] fileEntries = Directory.GetFiles(path);
            for (Int32 i = 0; numberProcessedFiles < limit && i < fileEntries.Length; ++i)
            {
                if (ProcessFile(dataType, fileEntries[i]))
                {
                    ++numberProcessedFiles;
                    SendStatusMessageFromThread("Load trajectory data from data source... - " + numberProcessedFiles  + " / " + limit);
                }
            }

            String[] directoryEntries = Directory.GetDirectories(path);
            for (Int32 i = 0; numberProcessedFiles < limit && i < directoryEntries.Length; ++i)
            {
                numberProcessedFiles += SearchFilesInDirectory(dataType, directoryEntries[i], limit, numberProcessedFiles);
            }

            return numberProcessedFiles;
        }


        private Boolean ProcessFile(DataType dataType, String fileName)
        {
            Boolean success = false;

            switch (dataType)
            {
                case DataType.PLT:
                    Regex rx = new Regex(@"\.plt$");
                    if (rx.IsMatch(fileName))
                    {
                        ProcessPLTFile(fileName);
                        success = true;
                    }
                    break;
                default:
                    break;
            }

            return success;
        }


        private void ProcessPLTFile(String filename)
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
            SendResultMessageFromThread(_OriginalTrajectories.Count - 1, ResultType.Original, trajectory.NumberOfPoints, 0);
        }


        public void SimplifyTrajectories(AlgorithmType algorithm_type, Double errorTolerance)
        {
            SimplifyStateMessage(this, new SimplifyStateMessageArgs(true));

            _SimplifiedTrajectories = new ConcurrentBag<Trajectory>();

            switch (algorithm_type)
            {
                case AlgorithmType.SP:
                    _AlgorithmName = "SP";
                    break;
                case AlgorithmType.SP_Prac:
                    _AlgorithmName = "SP-Prac";
                    break;
                case AlgorithmType.SP_Theo:
                    _AlgorithmName = "SP-Theo";
                    break;
                case AlgorithmType.SP_Both:
                    _AlgorithmName = "SP-Both";
                    break;
                case AlgorithmType.Intersect:
                    _AlgorithmName = "Intersect";
                    break;
                default:
                    break;
            }

            StatusMessage(this, new StringMessageArgs(GetStatusStringOfSimplify()));

            Int32 procNo = (Environment.ProcessorCount <= 1) ? 1 : Environment.ProcessorCount - 1;
            for (Int32 i = 0; i < procNo; ++i)
            {
                Int32 currentI = i;
                Task task = Task.Run(() =>
                {
                    for (Int32 j = currentI; j < _OriginalTrajectories.Count; j += procNo)
                    {
                        // step 0
                        // egymás utáni azonos pontok sorozatának helyettesítése egyetlen ponttal
                        Trajectory removedSeqsFromTrajectory = RemoveSequences(_OriginalTrajectories[j]);
                        DateTime timeStart = DateTime.Now;
                        Trajectory t;
                        ResultType resultType;
                        switch (algorithm_type)
                        {
                            case AlgorithmType.SP:
                                t = SimplifyBySP(removedSeqsFromTrajectory, errorTolerance);
                                resultType = ResultType.SP;
                                break;
                            case AlgorithmType.SP_Prac:
                                t = SimplifyBySP_Prac(removedSeqsFromTrajectory, errorTolerance);
                                resultType = ResultType.SP_Prac;
                                break;
                            case AlgorithmType.SP_Theo:
                                t = SimplifyBySP_Theo(removedSeqsFromTrajectory, errorTolerance);
                                resultType = ResultType.SP_Theo;
                                break;
                            case AlgorithmType.SP_Both:
                                t = SimplifyBySP_Both(removedSeqsFromTrajectory, errorTolerance);
                                resultType = ResultType.SP_Both;
                                break;
                            case AlgorithmType.Intersect:
                                t = SimplifyByIntersectAlg(removedSeqsFromTrajectory, errorTolerance);
                                resultType = ResultType.Intersect;
                                break;
                            default:
                                throw new NotImplementedException();
                                //break;
                        }
                        Double timeDifference = (DateTime.Now - timeStart).TotalSeconds;
                        _SimplifiedTrajectories.Add(t);
                        SendResultMessageFromThread(j, resultType, t.NumberOfPoints, timeDifference);
                        SendStatusMessageFromThread(GetStatusStringOfSimplify());
                        if (_SimplifiedTrajectories.Count == _OriginalTrajectories.Count)
                        {
                            SendSimplifyStateMessageFromThread(false);
                        }
                    }
                });
            }
        }


        private String GetStatusStringOfSimplify()
        {
            return "Simplify by algorithm " + _AlgorithmName + (_SimplifiedTrajectories.Count == _OriginalTrajectories.Count ? " ENDED" :("... - " + _SimplifiedTrajectories.Count + " / " + _OriginalTrajectories.Count));
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
            List<Int32>[] graph = ConstructGraph(trajectory, error_tolerance);
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
                graph[i] = new List<Int32>();
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
            Double directionSimplified = Point.GetDirection(trajectory[i], trajectory[j]);

            Double directionMaximalDifference = GetDirectionDifference(directionSimplified, Point.GetDirection(trajectory[i], trajectory[i + 1]));
            for (Int32 k = i; k < j; ++k)
            {
                Double directionDifference = GetDirectionDifference(directionSimplified, Point.GetDirection(trajectory[k], trajectory[k + 1]));
                if (directionDifference > directionMaximalDifference)
                {
                    directionMaximalDifference = directionDifference;
                }
            }

            return directionMaximalDifference;
        }


        private Double GetDirectionDifference(Double direction1, Double direction2)
        {
            Double absDifference = Math.Abs(direction1 - direction2);
            return Math.Min(absDifference, Math.PI * 2 - absDifference);
        }


        // Dijkstra-algoritmus
        private List<Int32> GetShortestPath(List<Int32>[] graph)
        {
            Int32 source = 0;
            HashSet<Int32> q = new HashSet<Int32>();
            Int32[] dist = new Int32[graph.Length];
            Int32[] prev = new Int32[graph.Length];
            for (Int32 i = 0; i < graph.Length; ++i)
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
                foreach (Int32 v in graph[u]) // van él "u" csúcsból az "v" csúcsba
                {
                    if (q.Contains(v))
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
            Int32 current = graph.Length - 1; // az utolsó csúcs mindenképpen eleme az egyszerűsített trajektóriának, ide kell eljutni
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
                foreach (IndexedPoints pi in H_Sets[H_Sets.Count - 1])
                {
                    for (Int32 i = U_Set.Count - 1; !ready && i >= 0; --i)
                    {
                        IndexedPoints pj = U_Set[i];
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
            List<Int32>[] graph = ConstructGraphWithFDR(trajectory, error_tolerance);
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


        private List<Int32>[] ConstructGraphWithFDR(Trajectory trajectory, Double error_tolerance)
        {
            Int32 numberOfVertexes = trajectory.NumberOfPoints;

            List<Int32>[] graph = new List<Int32>[trajectory.NumberOfPoints]; // éllistás ábrázoláshoz

            FeasibleDirectionRange FDR = new FeasibleDirectionRange(trajectory, error_tolerance);

            // élek meghatározása
            for (Int32 i = 0; i < graph.Length; ++i)
            {
                graph[i] = new List<Int32>();
                // csak az adott csúcsnál nagyobb indexű csúcsba vezethet él
                for (Int32 j = i + 1; j < numberOfVertexes; ++j)
                {
                    if (FDR.IsDirectionInRange(trajectory, i, j))
                    {
                        graph[i].Add(j);
                    }
                }
            }

            return graph;
        }


        private Trajectory SimplifyBySP_Both(Trajectory trajectory, Double error_tolerance)
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

            FeasibleDirectionRange FDR = new FeasibleDirectionRange(trajectory, error_tolerance);

            //Int32 k = 1;

            Boolean ready = false;
            while (!ready)
            {
                HashSet<IndexedPoints> Hk_Set = new HashSet<IndexedPoints>();
                // process positions in H(k-1)_Set and U in a reversed order
                foreach (IndexedPoints pi in H_Sets[H_Sets.Count - 1])
                {
                    for (Int32 i = U_Set.Count - 1; !ready && i >= 0; --i)
                    {
                        IndexedPoints pj = U_Set[i];
                    
                        if (pi.Index >= pj.Index)
                        {
                            // egy csúcsból csak nagyobb indexű csúcsba vezethet él
                            continue;
                        }
                        if (FDR.IsDirectionInRange(trajectory, pi.Index, pj.Index))
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
                while (h < trajectory.NumberOfPoints && IsSegmentFeasible(trajectory, e, h, error_tolerance / 2, FDR_matrix))
                {
                    ++h;
                }
                e = h - 1;
                simplifiedTrajectory.Add(trajectory[e]);
            }
            return simplifiedTrajectory;
        }


        private Boolean IsSegmentFeasible(Trajectory t, Int32 indexFrom, Int32 IndexTo, Double error_tolerance, DirectionRange[,] FDR_matrix)
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
                    FDR_matrix[indexFrom, IndexTo] = new DirectionRange(Point.GetDirection(t[indexFrom], t[IndexTo]), error_tolerance);
                    return true;
                }
                else
                {
                    FDR_matrix[IndexTo - 1, IndexTo] = new DirectionRange(Point.GetDirection(t[IndexTo - 1], t[IndexTo]), error_tolerance);
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


        private void SendSimplifyStateMessageFromThread(Boolean state)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { SimplifyStateMessage(this, new SimplifyStateMessageArgs(state)); }));
        }


        private void SendStatusMessageFromThread(String message)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { StatusMessage(this, new StringMessageArgs(message)); }));
        }


        public void ExportToKML(String path)
        {
            {
                String name = "polyline_original";
                XmlDocument xmlDoc = new XmlDocument();
                XmlNode kmlNode = xmlDoc.CreateElement("kml");
                XmlAttribute xmlnsAttribute = xmlDoc.CreateAttribute("xmlns");
                xmlnsAttribute.Value = "http://www.opengis.net/kml/2.2";
                kmlNode.Attributes.Append(xmlnsAttribute);
                XmlNode documentNode = xmlDoc.CreateElement("Document");
                XmlAttribute idAttribute = xmlDoc.CreateAttribute("id");
                idAttribute.Value = name;
                documentNode.Attributes.Append(idAttribute);
                XmlNode folderNode = xmlDoc.CreateElement("Folder");
                XmlNode nameNode = xmlDoc.CreateElement("name");
                nameNode.InnerText = "original";
                folderNode.AppendChild(nameNode);
                for (Int32 i = 0; i < _OriginalTrajectories.Count; ++i)
                {
                    Trajectory trajectory = _OriginalTrajectories[i];
                    XmlNode placemarkNode = xmlDoc.CreateElement("Placemark");
                    XmlNode styleNode = xmlDoc.CreateElement("Style");
                    XmlNode lineStyleNode = xmlDoc.CreateElement("LineStyle");
                    XmlNode colorNode = xmlDoc.CreateElement("color");
                    colorNode.InnerText = "ff0000ff";
                    lineStyleNode.AppendChild(colorNode);
                    styleNode.AppendChild(lineStyleNode);
                    placemarkNode.AppendChild(styleNode);
                    XmlNode lineStringNode = xmlDoc.CreateElement("LineString");
                    XmlNode altitudeModeNode = xmlDoc.CreateElement("altitudeMode");
                    altitudeModeNode.InnerText = "clampToGround";
                    lineStringNode.AppendChild(altitudeModeNode);
                    XmlNode coordinatesNode = xmlDoc.CreateElement("coordinates");
                    coordinatesNode.InnerText = trajectory[0].Latitude + "," + trajectory[0].Longitude;
                    for (Int32 j = 0; j < trajectory.NumberOfPoints; ++j)
                    {
                        coordinatesNode.InnerText += " " + trajectory[j].Latitude + "," + trajectory[j].Longitude;
                    }
                    lineStringNode.AppendChild(coordinatesNode);
                    placemarkNode.AppendChild(lineStringNode);
                    folderNode.AppendChild(placemarkNode);
                }
                documentNode.AppendChild(folderNode);
                kmlNode.AppendChild(documentNode);
                xmlDoc.AppendChild(kmlNode);
                XmlDeclaration xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                xmlDoc.InsertBefore(xmldecl, xmlDoc.DocumentElement);
                xmlDoc.Save(path + "\\" + name + ".kml");
            }
            {
                String name = "polyline_simplified_optimal";
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                XmlNode kmlNode = xmlDoc.CreateElement("kml");
                XmlAttribute xmlnsAttribute = xmlDoc.CreateAttribute("xmlns");
                xmlnsAttribute.Value = "http://www.opengis.net/kml/2.2";
                kmlNode.Attributes.Append(xmlnsAttribute);
                XmlNode documentNode = xmlDoc.CreateElement("Document");
                XmlAttribute idAttribute = xmlDoc.CreateAttribute("id");
                idAttribute.Value = name;
                documentNode.Attributes.Append(idAttribute);
                XmlNode folderNode = xmlDoc.CreateElement("Folder");
                XmlNode nameNode = xmlDoc.CreateElement("name");
                nameNode.InnerText = "original";
                folderNode.AppendChild(nameNode);
                for (Int32 i = 0; i < _SimplifiedTrajectories.Count; ++i)
                {
                    Trajectory trajectory = _OriginalTrajectories[i];
                    XmlNode placemarkNode = xmlDoc.CreateElement("Placemark");
                    XmlNode styleNode = xmlDoc.CreateElement("Style");
                    XmlNode lineStyleNode = xmlDoc.CreateElement("LineStyle");
                    XmlNode colorNode = xmlDoc.CreateElement("color");
                    colorNode.InnerText = "00ffff00";
                    lineStyleNode.AppendChild(colorNode);
                    styleNode.AppendChild(lineStyleNode);
                    placemarkNode.AppendChild(styleNode);
                    XmlNode lineStringNode = xmlDoc.CreateElement("LineString");
                    XmlNode altitudeModeNode = xmlDoc.CreateElement("altitudeMode");
                    altitudeModeNode.InnerText = "clampToGround";
                    lineStringNode.AppendChild(altitudeModeNode);
                    XmlNode coordinatesNode = xmlDoc.CreateElement("coordinates");
                    coordinatesNode.InnerText = trajectory[0].Latitude + "," + trajectory[0].Longitude;
                    for (Int32 j = 0; j < trajectory.NumberOfPoints; ++j)
                    {
                        coordinatesNode.InnerText += " " + trajectory[j].Latitude + "," + trajectory[j].Longitude;
                    }
                    lineStringNode.AppendChild(coordinatesNode);
                    placemarkNode.AppendChild(lineStringNode);
                    folderNode.AppendChild(placemarkNode);
                }
                documentNode.AppendChild(folderNode);
                kmlNode.AppendChild(documentNode);
                xmlDoc.AppendChild(kmlNode);
                XmlDeclaration xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                xmlDoc.InsertBefore(xmldecl, xmlDoc.DocumentElement);
                xmlDoc.Save(path + "\\" + name + ".kml");
            }
        }
    }
}
