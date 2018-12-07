# VR-Shop
A proof-of-concept virtual reality shopping experience developed with Unity. An HTC Vive is required.

## Requirements: Allow Cortana to access apps
The application uses Cortana's speech-to-text functionality from the HTC Vive's built-in microphone in order to search for the articles. This only works on a Windows 10 working machine and needs to be manually enabled first.

**German privacy settings:**

* Windows 10 Einstellungen -> "Datenschutz (Position, Kamera)":
    * "Spracherkennung":
        * "Online-Spracherkennung" einschalten
        * Hinweis: Bei älteren Versionen von Windows 10 hieß die Einstellung noch "Spracherkennung, Freihand und Eingabe" -> "Kennenlernen" (https://lexa-it.de/wp-content/uploads/2015/08/Datenschutz-Einstellungen.png).
    *  "Mikrofon":
        * "Zugriff auf das Mikrofon auf diesem Gerät zulassen" einschalten
        * "Zulassen, dass Apps auf ihr Mikrofon zugreifen" einschalten

## Notes
* There are only 26 unique articles in the shop!
    * Search for **"alles"** ("everything") to enable debug mode, which mocks 100 search results to test the article wall's rotation.
* Should the voice recognition still fail, you can always type in using the keyboard while the search menu is active.
* Importing 3D models into the app for the first time is a very slow process that may hang the application for a few seconds.
    * The model will be cached though, so subsequent imports of the same model will show up instantly.
