# GameOfLife API

A production-ready API implementation for [Conway’s Game of Life](https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life) using C# and .NET 7.0. This project demonstrates how to build a robust, well-tested API with persistence, structured logging, containerization, and comprehensive reporting.

---

## Table of Contents

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
  - **API & Logging:** `Serilog`, `Serilog.AspNetCore`, `Serilog.Settings.Configuration`, `Serilog.Sinks.File`
  - **Testing & Reporting:** `Microsoft.AspNetCore.Mvc.Testing`, `RestSharp`, `Newtonsoft.Json`, `Swashbuckle.AspNetCore`, `Swashbuckle.AspNetCore.Filters`, `Allure.Net.Commons`, `Allure.Xunit`
- **Optional:** Allure CLI for generating and viewing test reports  
  - [Allure CLI Installation Instructions](https://docs.qameta.io/allure/)

---

## Project Structure

