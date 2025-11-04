🇬🇧 This document is also available in English: [README.en.md](README.en.md)

# 🧠 PurAnalyzer

**PurAnalyzer** to lekka aplikacja webowa stworzona w technologii **.NET 8**, służąca do analizy plików o rozszerzeniu `.PUR`.  
Projekt został zaprojektowany zgodnie z zasadami **Clean Architecture**, co gwarantuje:
- wysoką **czytelność kodu**,  
- łatwą **testowalność**,  
- oraz prostą **rozszerzalność** i **utrzymanie** projektu w dłuższej perspektywie.  

Aplikacja udostępnia **REST API**, które umożliwia:
- walidację plików PUR,  
- parsowanie ich zawartości,  
- oraz analizę danych z generowaniem metryk i wyników.

## Spis treści
- [🎯 Opis projektu / Cel](#-opis-projektu--cel)
- [📊 Metryki i wyniki analizy](#-metryki-i-wyniki-analizy)
- [📂 Przykładowe pliki i wyniki walidacji](#-przykładowe-pliki-i-wyniki-walidacji)
- [⚙️ Technologie i narzędzia](#️-technologie-i-narzędzia)
- [🚀 Instalacja i uruchomienie](#-instalacja-i-uruchomienie)
- [🧪 Testy i pokrycie kodu](#-testy-i-pokrycie-kodu)
- [🏗️ Architektura](#️-architektura)
- [🌍 Zmienne środowiskowe](#-zmienne-środowiskowe)
- [🧾 Licencja / Autor](#-licencja--autor)


## 🎯 Opis projektu / Cel

Celem projektu **PurAnalyzer** jest automatyczna analiza plików o rozszerzeniu `.PUR`, wykorzystywanych w procesach handlowych i raportowych.  
Aplikacja została zaprojektowana tak, aby umożliwić szybkie i niezawodne przetwarzanie danych pochodzących z tych plików w sposób bezpieczny i powtarzalny.

System automatycznie:
- waliduje poprawność struktury i zawartości pliku,
- rozpoznaje nagłówki, pozycje dokumentów i sekcje danych,
- parsuje informacje do postaci obiektowej,
- generuje zestaw **metryk** opisujących przetworzone dane,
- umożliwia integrację z zewnętrznymi systemami poprzez **REST API**.

Dzięki temu aplikacja eliminuje potrzebę ręcznej analizy plików, zmniejsza ryzyko błędów, a także przyspiesza procesy kontroli i raportowania danych.

## 📊 Metryki i wyniki analizy

| Nazwa metryki | Opis |
|----------------|------|
| **LineCount** | liczba wszystkich wierszy w pliku |
| **CharCount** | liczba wszystkich znaków |
| **DocumentsCount** | liczba przetworzonych dokumentów |
| **PositionsCount** | liczba pozycji towarowych |
| **XCount** | liczba dokumentów przekraczających określony próg pozycji |
| **ProductsWithMaxNetValue** | produkt lub produkty o najwyższej wartości netto |

## 📂 Przykładowe pliki i wyniki walidacji

W katalogu `src/PurAnalyzer.Api/sample-data` znajdują się przykładowe pliki `.PUR` używane do testowania i walidacji aplikacji.  
Każdy plik reprezentuje inny scenariusz przetwarzania lub błędu.

| Nazwa pliku | Kod HTTP | Opis scenariusza |
|--------------|-----------|------------------|
| **200_53085222.PUR** | `200 OK` | Poprawny plik PUR — pełny zestaw danych, brak błędów. |
| **200_encoding.PUR** | `200 OK` | Plik poprawny, użyty do testów kodowania znaków (UTF-8 / CP1250). |
| **200_only_C.PUR** | `200 OK` | Plik zawiera tylko linie typu `C` (komentarze / puste dane), poprawny, ale bez dokumentów. |
| **413_more_than_10_MB.PUR** | `413 Payload Too Large` | Plik przekracza limit rozmiaru 10 MB — odrzucony przez walidator. |
| **422_invalid_format.PUR** | `422 Unprocessable Entity` | Plik ma niepoprawną strukturę lub brak wymaganych pól. |
| **422_missing_B.PUR** | `422 Unprocessable Entity` | Brak sekcji nagłówkowej (`B`), plik nie może być przetworzony. |
| **422_empty_file.PUR** | `422 Unprocessable Entity` | Plik pusty, brak danych wejściowych. |
| **422_only_B.PUR** | `422 Unprocessable Entity` | Plik zawiera tylko sekcje nagłówków bez pozycji dokumentów. |

Wszystkie przykłady można wykorzystać do ręcznego lub automatycznego testowania endpointów API, np.:
```powershell
curl -Uri "http://localhost:8080/api/v1/analyze" `
     -Method Post `
     -Headers @{ Authorization = "Basic dnM6cmVrcnV0YWNqYQ==" } `
     -Form @{ file = Get-Item "sample-data/200_53085222.PUR" }
```

## ⚙️ Technologie i narzędzia

| Technologia / Narzędzie | Zastosowanie |
|--------------------------|--------------|
| **.NET 8 (ASP.NET Core)** | Główna platforma aplikacji webowej |
| **Entity Framework Core (Npgsql)** | ORM do komunikacji z bazą danych PostgreSQL |
| **PostgreSQL** | Relacyjna baza danych do przechowywania danych z plików wejściowych PUR |
| **Serilog** | Logowanie zdarzeń i diagnostyka aplikacji |
| **Docker + Docker Compose** | Konteneryzacja i uruchamianie środowiska |
| **NUnit + Coverlet** | Testy jednostkowe i raporty pokrycia kodu |
| **Clean Architecture** | Struktura projektu oparta na separacji warstw i zasadach SOLID |

## 🚀 Instalacja i uruchomienie

Poniższe kroki opisują sposób lokalnego uruchomienia aplikacji **PurAnalyzer** w środowisku Docker, wraz z bazą danych PostgreSQL i interfejsem Swagger.

### 1️⃣ Zainstaluj Docker

Upewnij się, że na Twoim komputerze jest zainstalowany **Docker Desktop**.  
👉 [https://www.docker.com/products/docker-desktop/](https://www.docker.com/products/docker-desktop/)

### 2️⃣ Sklonuj repozytorium z GitHub

```powershell  
git clone https://github.com/pawel-popko/pur-analyzer
```

### 3️⃣ Przejdź do katalogu projektu

```powershell  
cd D:\PROJECTS\PurAnalyzer\src  
```

### 4️⃣ Uruchom kontener z bazą PostgreSQL

```powershell  
docker compose up -d postgres  
```

### 5️⃣ Zastosuj migracje do bazy danych

Wykonaj poniższe polecenie z katalogu `src`, aby utworzyć schemat bazy danych w kontenerze PostgreSQL:

```powershell  
dotnet ef database update `  
    -p PurAnalyzer.Infrastructure/PurAnalyzer.Infrastructure.csproj `  
    -s PurAnalyzer.Api/PurAnalyzer.Api.csproj  
```

### 6️⃣ Dostosuj konfigurację połączenia

W pliku `appsettings.Development.json` w projekcie **PurAnalyzer.Api**  
zmień wartość ciągu połączenia `Postgres` z:

```json  
"Postgres": "Host=localhost;Port=5432;Database=puranalyzer;Username=puranalyzer;Password=devpass"  
```

na:

```json  
"Postgres": "Host=postgres;Port=5432;Database=puranalyzer;Username=puranalyzer;Password=devpass"  
```

Dzięki tej zmianie aplikacja połączy się z kontenerem `postgres` w sieci Docker Compose.

### 7️⃣ Uruchom kontener aplikacji API

```powershell  
docker compose up -d api  
```

### 8️⃣ Otwórz interfejs Swagger

Po uruchomieniu kontenera API, otwórz przeglądarkę i przejdź do adresu:

👉 [http://localhost:8080/swagger/index.html](http://localhost:8080/swagger/index.html)

Aplikacja jest gotowa do użycia 🎉  
Możesz teraz przesyłać pliki `.PUR` do analizy za pomocą interfejsu Swagger lub poleceń PowerShell (`curl`).

## 🧪 Testy i pokrycie kodu

Projekt **PurAnalyzer** zawiera zestaw testów jednostkowych zrealizowanych w oparciu o framework **NUnit**.  
Do generowania raportu pokrycia kodu wykorzystywane są narzędzia **Coverlet** oraz **ReportGenerator**.

### 1️⃣ Uruchom testy z pomiarem pokrycia

Uruchom PowerShell jako Administrator i przejdź do katalogu projektu:  
`D:\PROJECTS\PurAnalyzer`

Następnie wykonaj polecenie:

```powershell  
dotnet test tests\PurAnalyzer.Tests\PurAnalyzer.Tests.csproj `  
  /p:CollectCoverage=true `  
  /p:CoverletOutputFormat=cobertura `  
  /p:CoverletOutput=D:\PROJECTS\PurAnalyzer\coverage-report\coverage\coverage  
```

To polecenie:
- uruchamia wszystkie testy jednostkowe,
- generuje plik XML z wynikami pokrycia kodu w formacie **Cobertura**.

Plik wynikowy zostanie zapisany jako:  
`D:\PROJECTS\PurAnalyzer\coverage-report\coverage\coverage.cobertura.xml`

### 2️⃣ Wygeneruj raport HTML

Będąc w tym samym katalogu (`D:\PROJECTS\PurAnalyzer`), utwórz raport na podstawie wygenerowanego XML-a:

```powershell  
reportgenerator `  
   -reports:"D:\PROJECTS\PurAnalyzer\coverage-report\coverage\coverage.cobertura.xml" `  
   -targetdir:"D:\PROJECTS\PurAnalyzer\coverage-report\report\" `  
   -reporttypes:"Html;TextSummary"  
```

To polecenie:
- tworzy raport HTML i tekstowe podsumowanie,
- zapisuje je w folderze:  
  `D:\PROJECTS\PurAnalyzer\coverage-report\report\`

### 3️⃣ Otwórz raport

Po zakończeniu generowania raportu, otwórz plik:  
`D:\PROJECTS\PurAnalyzer\coverage-report\report\index.html`

lub kliknij poniższy link, jeśli przeglądasz projekt lokalnie w Visual Studio Code lub przeglądarce:  
👉 [coverage-report/report/index.html](coverage-report/report/index.html)

Raport w formacie HTML przedstawia szczegółowe dane o pokryciu testami —  
łącznie z procentowym udziałem testowanego kodu, przeglądem plików oraz niepokrytymi liniami.

## 🏗️ Architektura

Projekt **PurAnalyzer** został zbudowany w oparciu o zasady **Clean Architecture**, co zapewnia czytelność kodu, testowalność i niezależność poszczególnych warstw.  
Każda warstwa ma jasno określoną odpowiedzialność i minimalne zależności od pozostałych.

### Struktura warstw

| Warstwa | Lokalizacja | Opis |
|----------|--------------|------|
| **API** | `src/PurAnalyzer.Api` | Odpowiada za komunikację z użytkownikiem lub klientem zewnętrznym poprzez **REST API**. Zawiera kontrolery, konfigurację routingu, uwierzytelnianie (BasicAuth) oraz integrację z warstwą aplikacyjną. |
| **Application** | `src/PurAnalyzer.Application` | Zawiera logikę biznesową i operacyjną. Odpowiada za analizę, walidację oraz przetwarzanie danych z plików PUR. |
| **Domain** | `src/PurAnalyzer.Domain` | Definiuje model domenowy – encje, wartości, kontrakty oraz reguły biznesowe. Ta warstwa nie zależy od żadnej innej i jest w pełni niezależna od frameworków. |
| **Infrastructure** | `src/PurAnalyzer.Infrastructure` | Implementuje szczegóły techniczne – dostęp do bazy danych (PostgreSQL + EF Core), logowanie (Serilog), operacje na plikach i parser danych. Warstwa ta realizuje interfejsy zdefiniowane w `Application`. |

## 🌍 Zmienne środowiskowe

Aplikacja **PurAnalyzer** wykorzystuje zmienne środowiskowe oraz ustawienia konfiguracyjne z pliku `appsettings.json` do zarządzania połączeniem z bazą danych i uwierzytelnianiem API.  
Dzięki temu poufne dane (takie jak hasła czy dane logowania) nie są zapisywane bezpośrednio w kodzie źródłowym.

| Nazwa zmiennej | Źródło | Opis | Przykładowa wartość |
|----------------|--------|------|----------------------|
| **ASPNETCORE_ENVIRONMENT** | `launchSettings.json` | Określa środowisko uruchomieniowe aplikacji (`Development`, `Staging`, `Production`). | `Development` |
| **BASICAUTH_USERNAME** | `launchSettings.json` | Nazwa użytkownika wymagana do autoryzacji żądań API w schemacie Basic Authentication. | `vs` |
| **BASICAUTH_PASSWORD** | `launchSettings.json` | Hasło użytkownika używane przy logowaniu do API. | `rekrutacja` |
| **POSTGRES_CONNSTR** | `appsettings.json` lub zmienna środowiskowa | Ciąg połączenia do bazy danych PostgreSQL, wykorzystywany przez Entity Framework Core (Npgsql). | `Host=localhost;Port=5432;Database=puranalyzer;Username=puranalyzer;Password=devpass` |

### 🔐 Przykład konfiguracji w PowerShell

```powershell
$env:BASICAUTH_USERNAME = "vs"
$env:BASICAUTH_PASSWORD = "rekrutacja"
$env:POSTGRES_CONNSTR = "Host=localhost;Port=5432;Database=puranalyzer;Username=puranalyzer;Password=devpass"
```

## 🧾 Licencja / Autor

Projekt **PurAnalyzer** jest udostępniany na licencji **MIT License**.  
Oznacza to, że kod źródłowy może być swobodnie używany, kopiowany, modyfikowany i rozpowszechniany — pod warunkiem zachowania informacji o autorze i oryginalnej licencji.

**Autor:**  
Paweł Popko  
Senior .NET Developer  

© 2025 Paweł Popko. Wszelkie prawa zastrzeżone.

