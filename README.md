# 🚀 JobApplicator v2

JobApplicator ist eine leistungsstarke **Blazor WebAssembly** Anwendung zur Verwaltung und Automatisierung des Bewerbungsprozesses. Das Tool hilft dabei, Jobanzeigen zu analysieren, Stammdaten zu pflegen und mithilfe von KI (generative Texte) maßgeschneiderte Bewerbungsunterlagen zu erstellen.

## ✨ Features

* **📊 Karriere Dashboard:** Behalte den Überblick über alle Bewerbungen mit einer dynamischen Gantt-Timeline (Phasen-Check der letzten 60 Tage).
* **🛠 Experten-Datenpflege:** Detailgenaue Bearbeitung aller Datensätze, inklusive Anforderungen, Benefits und vollständigen HTML-Blöcken für Anschreiben und Lebenslauf.
* **🤖 KI-Integration:** Generierung von Job-Zusammenfassungen und optimierten Anschreiben basierend auf der Jobbeschreibung.
* **📅 Timeline-Tracking:** Automatische Erfassung von Erstellungsdatum, Versanddatum und Statusänderungen (Zusage/Absage).
* **📝 HTML-Editor:** Direkte Kontrolle über den generierten HTML-Output und das angewendete CSS für perfekte Formatierung.
* **💾 SQLite Backend:** Lokale Datenspeicherung über einen effizienten Database-Service.

## 🛠 Tech Stack

* **Frontend:** Blazor WebAssembly (ASP.NET Core)
* **UI-Framework:** Bootstrap 5 mit Bootstrap Icons
* **Datenbank:** SQLite
* **Sprache:** C# / .NET 8 (oder 9)

## 🚀 Installation & Start

1.  **Repository klonen:**
    ```bash
    git clone [https://github.com/nobleman82/JobApplicator.git](https://github.com/nobleman82/JobApplicator.git)
    cd JobApplicator
    ```

2.  **Abhängigkeiten wiederherstellen:**
    ```bash
    dotnet restore
    ```

3.  **Anwendung starten:**
    Öffne das Projekt in Visual Studio und drücke `F5` oder nutze das Terminal:
    ```bash
    dotnet run
    ```

## 📂 Projektstruktur

* `/Pages`: Enthält die Razor-Komponenten (`Index.razor` für das Dashboard, `ExpertEditor.razor` für die Datenpflege).
* `/Services`: Enthält den `DatabaseService.cs` für die SQLite-Anbindung.
* `/Models`: Enthält die `ApplicationRecord.cs` Klasse, die alle Bewerbungsdaten definiert.
* `/wwwroot`: Statische Dateien wie CSS und JS-Interops.

## 📸 Screenshots

*(Hier kannst du später Bilder einfügen)*
- **Dashboard:** Zeigt die Status-Statistiken und die interaktive Timeline.
- **Expert-Editor:** Zeigt die detaillierte Maske zur Bearbeitung der HTML-Inhalte.

## 📄 Lizenz

Lizenz: Dieses Projekt steht unter der MIT-Lizenz (siehe LICENSE).

---
*Erstellt von [nobleman82](https://github.com/nobleman82)*
