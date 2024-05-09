# Aspire hosting extensions for Python projects

[![Continuous integration](https://github.com/wmeints/aspire-python/actions/workflows/ci.yml/badge.svg)](https://github.com/wmeints/aspire-python/actions/workflows/ci.yml)
![NuGet Version](https://img.shields.io/nuget/v/FizzyLogic.Aspire.Python.Hosting)

.NET Aspire already has great support for orchestrating C# projects and a fast array of middleware components.
This project adds additional support for running Python projects as smoothly as C# projects.

## System requirements

- Python
- .NET SDK 8 or higher
- .NET Aspire workload preview 7

## Getting started

Before using this extension you should know how to set up an Aspire project.
There's [an excellent guide](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/build-your-first-aspire-app?tabs=visual-studio) in the documentation.

Once you've set up a basic Aspire project, follow the instructions below to get started.

### Important assumptions

These extensions make a few assumptions about your project:

- You have a virtual environment in the `.venv` directory in your Python project.
- You have a Dockerfile in the root directory of your Python project.

**Note:** I haven't added support for Anaconda because I don't use it myself. Feel free to open a PR to add support for Anaconda.

### Setting up orchestration for a Python project

```csharp
using FizzyLogic.Aspire.Python.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddPythonProjectWithVirtualEnvironment("batch", "apps/batch");

builder.Build().Run();
```

The line `builder.AddProjectWithVirtualEnvironment` adds a new component to the orchestrator with the name `batch` located in `apps/batch`.

## Documentation

### Support for tracing, metrics, and logging

The extension automatically detects that you have the `opentelemetry-distro` package available in your project's virtual environment.
When you have the opentelemetry distro package your application is automatically instrumented. The traces, logs, and metrics in your
app are collected in the dashboard.

Depending on what you're building you'll likely need a few instrumentation libraries. There are [a bunch available on pypi](https://pypi.org/search/?q=opentelemetry-instrumentation).

The most commonly used are:

- [opentelemetry-instrumentation-logging](https://pypi.org/project/opentelemetry-instrumentation-logging/)
- [opentelemetry-instrumentation-django](https://pypi.org/project/opentelemetry-instrumentation-django/)
- [opentelemetry-instrumentation-fastapi](https://pypi.org/project/opentelemetry-instrumentation-fastapi/)
- [opentelemetry-instrumentation-flask](https://pypi.org/project/opentelemetry-instrumentation-flask/)

To use these libraries you can install them in your virtual environment, and the instrumentation logic will pick them up without any extra configuration.

### Support for clients

Since there are a lot of components in Aspire, it will take time to get support for them in Python.
I will update this page as I start work on them.
