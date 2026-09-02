# DotNetMvcWeb

This is a .NET 10 MVC (Model-View-Controller) project created for learning purposes.

## Project Environment
- **Framework**: .NET 10
- **Development Tools**: You can use IDEs such as Visual Studio, Visual Studio Code, or JetBrains Rider.

## How to Run the Project

You can run this project using the .NET CLI in your terminal. Please ensure you have the appropriate version of the .NET SDK installed.

1. **Navigate to the project directory**:
   ```bash
   cd DotNetMvcWeb
   ```

2. **Restore NuGet packages**:
   ```bash
   dotnet restore
   ```

3. **Run the project**:
   - **Normal Run Mode:**
   ```bash
   dotnet run
   ```

   - **Developer Mode (Hot Reload):**
   (Recommended for development. When you modify and save the code, the API server will automatically reload without requiring a manual restart.)
   ```bash
   dotnet watch run
   ```

4. **Browse the website**:
   Once the project is running, open the URL provided in the terminal (usually `http://localhost:5xxx` or `https://localhost:7xxx`) in your browser.

## Cross-Platform IDE Development Guide

If you prefer using Visual Studio (Windows) or other full-featured Integrated Development Environments (IDEs) to open this project:
- Please open the project folder directly with your IDE, or load the project by opening `DotNetMvcWeb.csproj`.
- The project includes basic launch profiles (located in `Properties/launchSettings.json`). You can choose to run the application using IIS Express (on Windows) or the default Kestrel server.

## How This Project Was Created from Scratch

If you want to know how this project was initialized from scratch, below are the .NET CLI commands used:

### 1. Create the MVC Project
Run the following command in your terminal to create an MVC project named `DotNetMvcWeb`:
```bash
dotnet new mvc -n DotNetMvcWeb
```
*(Note: The `-n` parameter is used to specify the project name)*

### 2. Create the .gitignore File
To prevent temporary build files (like `bin/`, `obj/`) or local configuration files from being added to version control, you can generate the official standard `.gitignore` template after navigating into the project directory:
```bash
cd DotNetMvcWeb
dotnet new gitignore
```

## Architecture Concepts: MVC vs. Razor Pages

In .NET web development, two common architectural patterns are **MVC (Model-View-Controller)** and **Razor Pages**:

- **Razor Pages**:
  - **Page-Focused**: Each web page has a corresponding backend code-behind file (PageModel). It keeps related code and markup organized together.
  - **When to use**: Ideal for most standard web applications, simple form-driven sites, and scenarios where the data flow is straightforward.

- **MVC (Model-View-Controller)**:
  - **Separation of Concerns**: Strictly divides the application into three interconnected components: Models (data and business logic), Views (UI), and Controllers (handles requests and coordinates between Models and Views).
  - **When to use**:
    1. **Large and complex applications**: When the project scale requires a strict architectural pattern to maintain order and structure.
    2. **Complex routing requirements**: MVC provides extensive and highly customizable routing capabilities.
    3. **Divided team roles**: Front-end developers can focus on Views, while back-end developers can focus on Controllers and Models without interfering with each other.

## Unit Testing & Code Coverage

The project includes a comprehensive unit testing suite `DotNetMvcWeb.Tests`, covering **Positive Tests**, **Negative/Exception Tests**, and **Boundary Tests**.

### 1. Run Unit Tests
```bash
dotnet test
```

### 2. Run Tests with Code Coverage (Coverlet)
Collect coverage metrics using Coverlet (both line and branch coverage exceed 85%):
```bash
dotnet test DotNetMvcWeb.Tests/DotNetMvcWeb.Tests.csproj \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=cobertura \
  /p:CoverletOutput=./TestResults/ \
  /p:Exclude="[DotNetMvcWeb]AspNetCoreGeneratedDocument.*%2c[DotNetMvcWeb]Program%2c[DotNetMvcWeb]*.Migrations.*%2c[DotNetMvcWeb]Microsoft.AspNetCore.OpenApi.*%2c[DotNetMvcWeb]System.Runtime.CompilerServices.*"
```

### 3. Generate and View Visual HTML Reports (ReportGenerator)
Use `dotnet-reportgenerator-globaltool` to convert coverage data into an interactive HTML dashboard:
```bash
# Install ReportGenerator globally (once)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate interactive HTML report
reportgenerator \
  -reports:"DotNetMvcWeb.Tests/TestResults/coverage.cobertura.xml" \
  -targetdir:"DotNetMvcWeb.Tests/CoverageReport" \
  -reporttypes:"Html;TextSummary;Badges"

# Open report (macOS)
open DotNetMvcWeb.Tests/CoverageReport/index.html
```

