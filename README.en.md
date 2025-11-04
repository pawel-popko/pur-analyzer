🇵🇱 Ten dokument jest dostępny również w języku polskim: [README.md](README.md)

# 🧠 PurAnalyzer

**PurAnalyzer** is a lightweight web application built with **.NET 8**, designed to analyze files with the `.PUR` extension.  
The project follows the principles of **Clean Architecture**, which ensures:
- high **code readability**,  
- easy **testability**,  
- and simple **extensibility** and **maintenance** in the long term.  

The application provides a **REST API** that enables:
- validation of PUR files,  
- parsing their contents,  
- and analyzing the data while generating metrics and results.

## Table of Contents
- [🎯 Project Description / Goal](#-project-description--goal)
- [📊 Metrics and Analysis Results](#-metrics-and-analysis-results)
- [📂 Sample Files and Validation Results](#-sample-files-and-validation-results)
- [⚙️ Technologies and Tools](#️-technologies-and-tools)
- [🚀 Installation and Setup](#-installation-and-setup)
- [🧪 Tests and Code Coverage](#-tests-and-code-coverage)
- [🏗️ Architecture](#️-architecture)
- [🌍 Environment Variables](#-environment-variables)
- [🧾 License / Author](#-license--author)


## 🎯 Project Description / Goal

The goal of the **PurAnalyzer** project is the automatic analysis of files with the `.PUR` extension, used in commercial and reporting processes.  
The application was designed to enable fast and reliable data processing from these files in a secure and repeatable way.

The system automatically:
- validates the structure and content of the file,
- recognizes headers, document items, and data sections,
- parses information into object form,
- generates a set of **metrics** describing the processed data,
- enables integration with external systems through a **REST API**.

As a result, the application eliminates the need for manual file analysis, reduces the risk of errors, and speeds up control and reporting processes.

## 📊 Metrics and Analysis Results

| Metric Name | Description |
|--------------|-------------|
| **LineCount** | total number of lines in the file |
| **CharCount** | total number of characters |
| **DocumentsCount** | number of processed documents |
| **PositionsCount** | number of item positions |
| **XCount** | number of documents exceeding a specified item threshold |
| **ProductsWithMaxNetValue** | product(s) with the highest net value |

## 📂 Sample Files and Validation Results

The directory `src/PurAnalyzer.Api/sample-data` contains example `.PUR` files used for testing and validating the application.  
Each file represents a different processing or error scenario.

| File Name | HTTP Code | Scenario Description |
|------------|------------|----------------------|
| **200_53085222.PUR** | `200 OK` | Valid PUR file — full data set, no errors. |
| **200_encoding.PUR** | `200 OK` | Valid file, used for character encoding tests (UTF-8 / CP1250). |
| **200_only_C.PUR** | `200 OK` | File contains only lines of type `C` (comments / empty data), valid but without documents. |
| **413_more_than_10_MB.PUR** | `413 Payload Too Large` | File exceeds 10 MB size limit — rejected by the validator. |
| **422_invalid_format.PUR** | `422 Unprocessable Entity` | File has an invalid structure or missing required fields. |
| **422_missing_B.PUR** | `422 Unprocessable Entity` | Missing header section (`B`), file cannot be processed. |
| **422_empty_file.PUR** | `422 Unprocessable Entity` | Empty file, no input data. |
| **422_only_B.PUR** | `422 Unprocessable Entity` | File contains only header sections without document items. |

All examples can be used for manual or automated testing of API endpoints, e.g.:
``` powershell
curl -Uri "http://localhost:8080/api/v1/analyze" `
     -Method Post `
     -Headers @{ Authorization = "Basic dnM6cmVrcnV0YWNqYQ==" } `
     -Form @{ file = Get-Item "sample-data/200_53085222.PUR" }
```

## ⚙️ Technologies and Tools

| Technology / Tool | Purpose |
|--------------------|----------|
| **.NET 8 (ASP.NET Core)** | Main web application platform |
| **Entity Framework Core (Npgsql)** | ORM for communication with the PostgreSQL database |
| **PostgreSQL** | Relational database for storing input PUR file data |
| **Serilog** | Event logging and diagnostics |
| **Docker + Docker Compose** | Containerization and environment setup |
| **NUnit + Coverlet** | Unit testing and code coverage reports |
| **Clean Architecture** | Project structure based on layer separation and SOLID principles |

## 🚀 Installation and Setup

The following steps describe how to run the **PurAnalyzer** application locally in a Docker environment, along with a PostgreSQL database and Swagger interface.

### 1️⃣ Install Docker

Make sure **Docker Desktop** is installed on your computer.  
👉 [https://www.docker.com/products/docker-desktop/](https://www.docker.com/products/docker-desktop/)

### 2️⃣ Clone the GitHub Repository

``` powershell  
git clone https://github.com/pawel-popko/pur-analyzer
```

### 3️⃣ Navigate to the Project Directory

``` powershell  
cd D:\PROJECTS\PurAnalyzer\src  
```

### 4️⃣ Start the PostgreSQL Container

``` powershell  
docker compose up -d postgres  
```

### 5️⃣ Apply Database Migrations

Run the following command from the `src` directory to create the database schema in the PostgreSQL container:

``` powershell  
dotnet ef database update `  
    -p PurAnalyzer.Infrastructure/PurAnalyzer.Infrastructure.csproj `  
    -s PurAnalyzer.Api/PurAnalyzer.Api.csproj  
```

### 6️⃣ Adjust Connection Configuration

In the `appsettings.Development.json` file within the **PurAnalyzer.Api** project,  
update the `Postgres` connection string from:

``` json  
"Postgres": "Host=localhost;Port=5432;Database=puranalyzer;Username=puranalyzer;Password=devpass"  
```

to:

``` json  
"Postgres": "Host=postgres;Port=5432;Database=puranalyzer;Username=puranalyzer;Password=devpass"  
```

This change allows the application to connect to the `postgres` container in the Docker Compose network.

### 7️⃣ Start the API Container

``` powershell  
docker compose up -d api  
```

### 8️⃣ Open the Swagger Interface

Once the API container is running, open your browser and go to:

👉 [http://localhost:8080/swagger/index.html](http://localhost:8080/swagger/index.html)

The application is now ready to use 🎉  
You can now upload `.PUR` files for analysis via the Swagger interface or PowerShell (`curl`) commands.

## 🧪 Tests and Code Coverage

The **PurAnalyzer** project includes a set of unit tests implemented using the **NUnit** framework.  
Code coverage reports are generated using **Coverlet** and **ReportGenerator** tools.

### 1️⃣ Run Tests with Coverage Measurement

Run PowerShell as Administrator and navigate to the project directory:  
`D:\PROJECTS\PurAnalyzer`

Then execute the command:

``` powershell  
dotnet test tests\PurAnalyzer.Tests\PurAnalyzer.Tests.csproj `  
  /p:CollectCoverage=true `  
  /p:CoverletOutputFormat=cobertura `  
  /p:CoverletOutput=D:\PROJECTS\PurAnalyzer\coverage-report\coverage\coverage  
```

This command:
- runs all unit tests,
- generates an XML coverage report in **Cobertura** format.

The output file will be saved as:  
`D:\PROJECTS\PurAnalyzer\coverage-report\coverage\coverage.cobertura.xml`

### 2️⃣ Generate HTML Report

While in the same directory (`D:\PROJECTS\PurAnalyzer`), create a report based on the generated XML:

``` powershell  
reportgenerator `  
   -reports:"D:\PROJECTS\PurAnalyzer\coverage-report\coverage\coverage.cobertura.xml" `  
   -targetdir:"D:\PROJECTS\PurAnalyzer\coverage-report\report\" `  
   -reporttypes:"Html;TextSummary"  
```

This command:
- creates an HTML report and text summary,
- saves them in the folder:  
  `D:\PROJECTS\PurAnalyzer\coverage-report\report\`

### 3️⃣ Open the Report

After generating the report, open the file:  
`D:\PROJECTS\PurAnalyzer\coverage-report\report\index.html`

or click the link below if viewing the project locally in Visual Studio Code or a browser:  
👉 [coverage-report/report/index.html](coverage-report/report/index.html)

The HTML report displays detailed code coverage data —  
including tested code percentage, file overview, and uncovered lines.

## 🏗️ Architecture

The **PurAnalyzer** project is built based on **Clean Architecture** principles, ensuring code clarity, testability, and layer independence.  
Each layer has a clearly defined responsibility and minimal dependencies on others.

### Layer Structure

| Layer | Location | Description |
|--------|-----------|-------------|
| **API** | `src/PurAnalyzer.Api` | Responsible for communication with users or external clients via **REST API**. Contains controllers, routing configuration, authentication (BasicAuth), and integration with the Application layer. |
| **Application** | `src/PurAnalyzer.Application` | Contains business and operational logic. Responsible for analysis, validation, and processing of PUR file data. |
| **Domain** | `src/PurAnalyzer.Domain` | Defines the domain model – entities, value objects, contracts, and business rules. This layer does not depend on any other and is fully framework-independent. |
| **Infrastructure** | `src/PurAnalyzer.Infrastructure` | Implements technical details – database access (PostgreSQL + EF Core), logging (Serilog), file operations, and data parsing. This layer implements interfaces defined in `Application`. |

## 🌍 Environment Variables

The **PurAnalyzer** application uses environment variables and configuration settings from the `appsettings.json` file to manage database connections and API authentication.  
This ensures that sensitive data (like passwords or login credentials) are not stored directly in the source code.

| Variable Name | Source | Description | Example Value |
|----------------|--------|--------------|----------------|
| **ASPNETCORE_ENVIRONMENT** | `launchSettings.json` | Defines the application environment (`Development`, `Staging`, `Production`). | `Development` |
| **BASICAUTH_USERNAME** | `launchSettings.json` | Username required for API authorization in Basic Authentication scheme. | `vs` |
| **BASICAUTH_PASSWORD** | `launchSettings.json` | Password used for API login. | `rekrutacja` |
| **POSTGRES_CONNSTR** | `appsettings.json` or environment variable | Connection string for the PostgreSQL database, used by Entity Framework Core (Npgsql). | `Host=localhost;Port=5432;Database=puranalyzer;Username=puranalyzer;Password=devpass` |

### 🔐 Example Configuration in PowerShell

``` powershell
$env:BASICAUTH_USERNAME = "vs"
$env:BASICAUTH_PASSWORD = "rekrutacja"
$env:POSTGRES_CONNSTR = "Host=localhost;Port=5432;Database=puranalyzer;Username=puranalyzer;Password=devpass"
```

## 🧾 License / Author

The **PurAnalyzer** project is distributed under the **MIT License**.  
This means that the source code may be freely used, copied, modified, and distributed — provided that the author information and original license are retained.

**Author:**  
Paweł Popko  
Senior .NET Developer  

© 2025 Paweł Popko. All rights reserved.
