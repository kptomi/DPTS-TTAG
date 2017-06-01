Felhasználói leírás (Direction-Preserving Trajectory Simplification)


ADATOK BEOLVASÁSA

Adatforrás megadása (kétféle mód):
	- menüben: File > Open file as data source...
	- menüben: File > Open directory as data source...
Az első esetben egy fájlt lehet browse-olni, ami egyetlen trajektóriát tartalmaz.
A második esetben pedig egy könyvtárat, ami közvetlenül vagy közvetetten tartalmaz trajektóriákat tároló fájlokat.

A választott fájl/könyvtár útvonala megjelenik az alkalmazás főképernyőjén is (kezdetben) üres.

Az útvonal mellett ki kell választani az adatforrás típusát. Az alkalmazás a "Geolife" valamint a "T-Drive" adathalmazokból
érkező adatok kezelésére lett felkészítve.
Megjegyzés: ha az adatforrás és a választott adattípus nem kompatibilis, akkor legkésőbb a beolvasásnál hibaüzenetet kapunk.
(Nem megfelelő formátum!)

Limit: könyvtár típusú adatforrás esetén van értelme: ezzel lehet korlátozni, hogy a könyvtárból - rekurzívan is értve - hány
trajektóriát tartalmazó fájl dolgozzon fel. "All" választása esetén az összes megfelelő formátumú fájlt beolvassa.

Ha a fentieket mind beállítottuk, akkor engedélyeződik a "Load datas" gomb, ezzel tudjuk beolvasni a memóriába a kiválasztott
adatforrásból a trajektóriát vagy trajektóriákat.
Megjegyzés: ezek újabb beolvasásig a memóriában maradnak.


EGYSZERŰSÍTÉS FUTTATÁSA

Error tolerance: egy valós szám, az iránytartásra vonatkozó hibakorlátot jelenti radiánban. (default értéke: 1)

Algorithm: itt lehet kiválasztani az algoritmust, amellyel egyszerűsíteni szeretnénk (SP, SP-Prac, SP-Theo, SP-Both,
Intersect).

Ha mind a két paramétert megadtuk, akkor engedélyeződik a "Simplify trajectories" gomb. Ha megnyomjuk, akkor az ÖSSZES, a
memóriába betöltött trajektória egyszerűsítése megtörténik, párhuzamosan (processorszám - 1) szálon.


EREDMÉNYEK (táblázat)

Itt tekinthetjük át az egyszerűsítések eredményeit:
	- No: egy sorszám, a trajektóriák beolvasásának sorrendjében
	- OrigLength: pontok száma az adatforrásból közvetlenül beolvasott trajektóriákban
	- SimpLengthOpt: pontok száma egy optimális algoritmussal (SP, SP-Prac, SP-Theo, SP-Both) történő egyszerűsítés után
	- SimpleLengthApprox: pontok száma a közelítő algoritmussal (Intersect) történő egyszerűsítés után
	- SP: az algoritmus futási ideje az adott trajektórián
	- SP-Prac: az algoritmus futási ideje az adott trajektórián
	- SP-Theo: az algoritmus futási ideje az adott trajektórián
	- SP-Both: az algoritmus futási ideje az adott trajektórián
	- Intersect: az algoritmus futási ideje az adott trajektórián

Exportálás KML-be:
	- menüben: File > Export results to KML...
	Ha vannak betöltve eredeti trajektóriák, vagy végeztünk már egyszerűsítést, akkor az eredményeket kml formátumba
	exportálhatjuk. Ez a parancs legfeljebb 3 állományt hoz létre:
		- egyet az eredeti trajektóriákat (polyline_original.kml),
		- egyet az optimális algoritmusok (polyline_simplified_optimal.kml),
		- míg egyet a közelítő algoritmus által egyszerűsített trajektóriákat ábrázolva (polyline_simplified_approximate.kml).