# VR-Shop
A proof-of-concept virtual reality shopping experience developed with Unity. An HTC Vive is required.

## Requirements: Allow Cortana to access apps
The application uses Cortana's speech-to-text functionality from the HTC Vive's built-in microphone in order to search for the articles. This only works on a Windows 10 working machine and needs to be manually enabled first.

**German privacy settings:**
Windows 10 Einstellungen -> "Datenschutz (Position, Kamera)" -> "Spracherkennung" -> "Online-Spracherkennung" einschalten

## Notes
* Should the voice recognition still fail, you can always type in using the keyboard while the search menu is active.
* Importing 3D models into the app for the first time is a very slow process that may hang the application for a few seconds. The model will be cached though, there subsequent imports of the same model will show up instantly.
* There are only 26 unique articles in the shop.
* Search for **"alles"** to enable debug mode, which mocks 100 search results to test the article wall's rotation.