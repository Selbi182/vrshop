# VR-Shop mit eingescannten 3D-Modellen
Ein virtuelles Einkaufserlebnis in Virtueller Realität, entickelt mit Unity und der HTC Vive.

## Konfiguration: Cortana freischalten
Die Applikation verwendet die Spracherkennung von Cortana zusammen mit dem eingebauten Mikrofon der HTC Vive, um nach Artikeln suchen zu können. Diese Optionen sind standardmäßig unter Windows 10 deaktiviert und müssen vorher freigeschaltet werden:

* Windows 10 Einstellungen -> "Datenschutz (Position, Kamera)":
    * "Spracherkennung":
        * "Online-Spracherkennung" einschalten
        * Hinweis: Bei älteren Versionen von Windows 10 hieß die Einstellung noch "Spracherkennung, Freihand und Eingabe" -> "Kennenlernen" (https://lexa-it.de/wp-content/uploads/2015/08/Datenschutz-Einstellungen.png).
    *  "Mikrofon":
        * "Zugriff auf das Mikrofon auf diesem Gerät zulassen" einschalten
        * "Zulassen, dass Apps auf ihr Mikrofon zugreifen" einschalten

## Hinweise
* Der Ordner **Articles** muss im Hauptverzeichnis der ausführbaren Datei liegen, NICHT VRShop_Data.
* Es gibt nur 26 Artikel. Mit dem Suchwort **"alles"** wird der Debug Modus aktiviert, welcher zum Testen 100+ Artikelmonitore anzeigt um die Rotation der Artikelwand testen zu können.
* Sollte die Spracherkennung nicht funktionieren, kann als Ersatzlösung mit der Tastatur gesucht werden.
* Der Import von 3D-Modellen nimmt einige Sekunden in Anspruch und kann die Anwendung verlangsamen. Wiederholtes Importieren benutzt einen Cache, mit dem der Vorgang beschleunigt wird.
