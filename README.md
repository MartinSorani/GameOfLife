# GameOfLife API

A production-ready API implementation for [Conway’s Game of Life](https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life) using C# and .NET 7.0. This project demonstrates how to build a robust, well-tested API with persistence, structured logging, containerization, and comprehensive reporting.

---

## Table of Contents

- [Rules of the Game](#rules-of-the-game)
- [Features](#features)
- [Requirements](#requirements)
- [Project Structure](#project-structure)
- [Configuration](#configuration)
- [Building and Running](#building-and-running)
  - [Local Development](#local-development)
  - [Docker](#docker)
- [API Endpoints](#api-endpoints)
- [Logging](#logging)
- [Testing](#testing)
  - [Unit Tests](#unit-tests)
  - [Integration Tests](#integration-tests)
  - [End-to-End Tests](#end-to-end-tests)
- [Reporting with Allure](#reporting-with-allure)
- [Persistence and Crash Recovery](#persistence-and-crash-recovery)
- [Contributing](#contributing)
- [License](#license)

---

## Rules of the Game

Conway's Game of Life is a cellular automaton devised by mathematician John Conway. The game is a zero-player game, meaning that its evolution is determined by its initial state, requiring no further input. The game is played on an infinite two-dimensional grid of cells, each of which can be alive or dead. The game progresses in generations, with each cell's state in the next generation determined by the following rules:

1. **Underpopulation:** Any live cell with fewer than two live neighbors dies.
1. **Survival:** Any live cell with two or three live neighbors survives.
1. **Overpopulation:** Any live cell with more than three live neighbors dies.
1. **Reproduction:** Any dead cell with exactly three live neighbors becomes a live cell.
1. **Stable State:** A cell with no live neighbors or two live neighbors remains in the same state.


## Features

- **Board Upload:** Upload a new board state (as a jagged array of booleans) and receive a unique identifier (GUID).
- **Next State Calculation:** Compute and retrieve the next generation of the board.
- **Generational Simulation:** Retrieve the board state after a specified number of generations.
- **Final State Retrieval:** Determine and return the final (stable) state of the board with error handling for non-convergence.
- **Persistence:** File-based repository ensures board state is retained across service restarts or crashes.
- **Containerization:** Docker support for easy deployment.
- **Structured Logging:** Uses Microsoft.Extensions.Logging (optionally with Serilog) for high-performance, structured logging.
- **Comprehensive Testing:** Includes Unit, Integration, and End-to-End (E2E) tests.
- **Detailed Reporting:** Integrated with Allure for interactive, detailed test reports.

---

## Requirements

- [.NET 7.0 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
- [Docker](https://www.docker.com/)
- **NuGet Packages:**
  - **Testing & Reporting:** `Microsoft.AspNetCore.Mvc.Testing`, `RestSharp`, `Newtonsoft.Json`, `Swashbuckle.AspNetCore`, `Swashbuckle.AspNetCore.Filters`, `Allure.Net.Commons`, `Allure.Xunit`
- **Optional:** Allure CLI for generating and viewing test reports  
  - [Allure CLI Installation Instructions](https://docs.qameta.io/allure/)

---

## Project Structure

📦GameOfLife\
 ┣ 📂src\
 ┃ ┗ 📂GameOfLife.Api\
 ┃ ┃ ┣ 📂Controllers\
 ┃ ┃ ┃ ┗ 📜BoardsController.cs\
 ┃ ┃ ┣ 📂Examples\
 ┃ ┃ ┃ ┗ 📜BoardDtoExample.cs\
 ┃ ┃ ┣ 📂Models\
 ┃ ┃ ┃ ┣ 📜Board.cs\
 ┃ ┃ ┃ ┗ 📜BoardStateDto.cs\
 ┃ ┃ ┣ 📂Properties\
 ┃ ┃ ┃ ┗ 📜launchSettings.json\
 ┃ ┃ ┣ 📂Repositories\
 ┃ ┃ ┃ ┣ 📜FileBoardRepository.cs\
 ┃ ┃ ┃ ┣ 📜IBoardRepository.cs\
 ┃ ┃ ┃ ┗ 📜InMemoryBoardRepository.cs\
 ┃ ┃ ┣ 📂Services\
 ┃ ┃ ┃ ┣ 📜GameOfLifeService.cs\
 ┃ ┃ ┃ ┗ 📜IGameOfLifeService.cs\
 ┃ ┃ ┣ 📂Utils\
 ┃ ┃ ┃ ┣ 📜ArrayConverter.cs\
 ┃ ┃ ┃ ┣ 📜ConwayEngine.cs\
 ┃ ┃ ┃ ┣ 📜FileLogger.cs\
 ┃ ┃ ┃ ┗ 📜FileLoggerProvider.cs\
 ┃ ┃ ┣ 📜appsettings.json\
 ┃ ┃ ┣ 📜Dockerfile\
 ┃ ┃ ┗ 📜Program.cs\
 ┣ 📂tests\
 ┃ ┣ 📂GameOfLife.e2e.Tests\
 ┃ ┃ ┣ 📂Factories\
 ┃ ┃ ┃ ┗ 📜CustomWebApplicationFactory.cs\
 ┃ ┃ ┗ 📜EndToEndTests.cs\
 ┃ ┣ 📂GameOfLife.Integration.Tests\
 ┃ ┃ ┗ 📜ControllerTests.cs\
 ┃ ┣ 📂GameOfLife.Unit.Tests\
 ┃ ┃ ┣ 📜ConwayEngineTests.cs\
 ┃ ┃ ┗ 📜GameOfLifeServiceTests.cs\
 ┣ 📜.dockerignore\
 ┣ 📜.gitignore\
 ┣ 📜allureConfig.json\
 ┣ 📜GameOfLife.sln\
 ┣ 📜nuget.config\
 ┗ 📜README.md
 
---

## Configuration

- **appsettings.json:** Contains configuration for logging, connection strings, and other runtime settings.
- **allureConfig.json:** Defines Allure reporting options (output directory, report title, link patterns, etc.).  
  Example:
  ```json
  {
    "allure": {
      "directory": "allure-results",
      "title": "GameOfLife API E2E Test Report",
      "links": [
        "https://github.com/MartinSorani/GameOfLife/issues/{issue}"
      ],
      "failExceptions": [
        "System.Exception"
      ]
    }
  }
- **nuget.config:** (Optional) Specifies package sources for NuGet package restoration.

---

## Building and Running

### Local Development

1. **Clone the repository:**
   ```bash
   git clone https://github.com/MartinSorani/GameOfLife.git
   cd GameOfLife
    ```

1. **Restore and build:**
   ```bash
    dotnet restore
    dotnet build
    ```

1. **Run the API:**
    ```bash
    dotnet run --project src/GameOfLife.Api
    ```

1. **Access the API:**

    The API should be available at the URLs specified in launchSettings.json (e.g., http://localhost:5042, https://localhost:7042).

### Docker

1. **Build the Docker image:**
   ```bash
   docker build -t gameoflife-api -f src/GameOfLife.Api/Dockerfile .
   ```

1. **Run the Docker container:**
    ```bash
    docker run -d -p 5042:80 -p 7042:443 gameoflife-api
    ```

1. **Access the API:**            
Open your browser to [http://localhost:5042](http://localhost:5042) or [https://localhost:7042](https://localhost:7042).

---

## API Endpoints

- **POST /api/boards:** Upload a new board state and receive a unique identifier (GUID).

  Request Body:
  ```json
  {
    "board": [
      [true, false, true],
      [false, true, false],
      [true, false, true]
    ]
  }
  ```
  Response Body:
  ```json
  {
    "id": "d7b3b3b3-0b3b-4b3b-8b3b-0b3b3b3b3b3b"
  }
  ```
- **GET /api/boards/{id}/next:** Compute and retrieve the next generation of the board.\

  Returns:
  * 404 Not Found if the board is not found.
  * 400 Bad Request on invalid board id.
  * 200 OK on successful computation.
  
- **GET /api/boards/{id}/states?steps={steps}:** Retrieve the board state after a specified number of generations.\

  Returns:
  * 400 Bad Request if steps is non-positive.
  * 404 Not Found if the board is not found.
  * 200 OK on successful computation.
 
- **GET /api/boards/{id}/final?maxIterations={maxIterations}:** Retrieves the final (stable) board state.\

Returns:

* 400 Bad Request if maxIterations is non-positive.
* 404 Not Found if the board is not found.
* 200 OK on successful computation.

Find more details in the Swagger UI at [http://localhost:5042/swagger](http://localhost:5042/swagger).

---

## Logging

- **FileLogger:** Logs to a file in the logs directory (GameOfLife.Logs.txt).
- **FileLoggerProvider:** Provides the FileLogger instance to the logging system.

---

## Testing

### Unit Tests

* Location: GameOfLife.UnitTests
* Coverage: Core logic (e.g., ConwayEngine, array conversion, service methods)

### Integration Tests

* Location: GameOfLife.IntegrationTests
* Coverage: API endpoints, error handling, and service interactions via WebApplicationFactory.

### End-to-End Tests

* Location: GameOfLife.e2e.Tests
* Coverage: Full user workflows including board upload, next state, state after steps, final state, data persistence, and crash recovery.
* **Allure Reporting:** Generates detailed test reports in the allure-results directory.
* **Allure CLI:** Use the Allure CLI to generate and view the test reports.
* **Allure CLI Command:**
  ```bash
  allure serve Path-to-report-folder/allure-results
  ```
* Running E2E Tests:
  ```bash
  dotnet test --filter TestCategory=E2E
  ```

  ---

  ## Reporting with Allure

  ### Configuration:
Allure is configured via `allureConfig.json`. This file defines the output directory (default is "allure-results"), report title, and link patterns.

### Running Tests with Allure Logger:
Run tests with:
```bash
dotnet test --logger:"allure;ReportPath=allure-results"
```

This will generate raw results in the `allure-results` folder.

### Generating the Report:
After tests complete, generate the interactive report with the Allure CLI:
```bash
allure serve allure-results
```

### Test Annotations:
- Use attributes such as `[AllureSuite]`, `[AllureSubSuite]`, `[AllureFeature]`, `[AllureStory]`, `[AllureSeverity]`, and `[AllureDescription]` on test classes and methods.
- Use lambda steps (via `AllureApi.Step` and `AllureLifecycle.Instance.UpdateStep`) to annotate detailed steps in your tests.

## Persistence and Crash Recovery

### Persistence:
Board states are persisted using a file-based repository (`FileBoardRepository`) that saves data to `boards.json`.

### Crash Recovery:
End-to-end tests simulate service restarts by disposing and recreating the `WebApplicationFactory`. This verifies that the board state persists across service restarts or crashes.

## License
This project is licensed under the MIT License. See the LICENSE file for details.
