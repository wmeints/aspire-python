# Aspire hosting extensions for Python projects

[![Continuous integration](https://github.com/wmeints/aspire-python/actions/workflows/ci.yml/badge.svg)](https://github.com/wmeints/aspire-python/actions/workflows/ci.yml)
[![NuGet Version](https://img.shields.io/nuget/v/FizzyLogic.Aspire.Python.Hosting)](https://www.nuget.org/packages/FizzyLogic.Aspire.Python.Hosting/)

---------------------------------

**IMPORTANT** I merged most of the code here into the .NET Aspire code base. If you want to use Python projects with Aspire, make sure to check out the docs here: 
https://learn.microsoft.com/en-us/dotnet/aspire/get-started/build-aspire-apps-with-python?tabs=bash

---------------------------------

This project adds additional support for running Python projects as smoothly as C# projects.

## System requirements

- Python
- .NET SDK 8 or higher
- .NET Aspire workload preview 7

## Getting started

Before using this extension you should know how to set up an Aspire project.
There's [an excellent guide](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/build-your-first-aspire-app?tabs=visual-studio) in the documentation.

Once you've set up a basic Aspire project, follow the instructions below to get started.

### Before you start building

These extensions make a few assumptions about your project:

- You have a virtual environment in the `.venv` directory in your Python project. You can set up a virtual environment with
  [venv](https://docs.python.org/3/library/venv.html), [rye](https://rye-up.com) or [poetry](https://python-poetry.org/).
- You have a Dockerfile in the root directory of your Python project. A sample Dockerfile can be found [here](sample/apps/batch/Dockerfile)

**Note:** I haven't added support for Anaconda because I don't use it myself. Feel free to open a PR to add support for Anaconda.

### Setting up a Python project

Basic Python projects can be configured using the following code:

```csharp
using FizzyLogic.Aspire.Python.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddPythonProjectWithVirtualEnvironment("batch", "apps/batch");

builder.Build().Run();
```

By adding a Python project resource, you'll automatically get tracing, logging, and metrics configured.
Note that you need to add extra HTTP and HTTPS endpoints yourself by calling `.WithHttpEndpoint` or `WithHttpsEndpoint`.

The Python project is deployed as a container when you publish the Aspire application. You'll need a Dockerfile in the
Python project directory for the publication to work properly.

Telemetry data is automatically gathered when you have the following package available in your virtual
environment:

- [opentelemetry-distro](https://pypi.org/project/opentelemetry-distro/)
  (Make sure you install opentelemetry-distro\[otlp\])

### Setting up a Flask project

Flask applications differ from regular Python projects in that we'll automatically expose an HTTP endpoint for the project.
You can configure a new Flask project using the following code:

```csharp
using FizzyLogic.Aspire.Python.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddFlaskProjectWithVirtualEnvironment("flask-service", "apps/flask-service");

builder.Build().Run();
```

As with regular Python projects, the Flask project is published as a container. You'll need a Dockerfile in the Python
project directory for the publication to work properly.

Telemetry data is automatically gathered when you have the following two packages available in your virtual
environment:

- [opentelemetry-distro](https://pypi.org/project/opentelemetry-distro/)
  (Make sure you install opentelemetry-distro\[otlp\])
- [opentelemetry-instrumentation-flask](https://pypi.org/project/opentelemetry-instrumentation-flask/)

## Developing

### Setting up your environment

Before you can work on the code, make sure you have [.NET 8](https://dot.net), [Python](https://python.org), and [Rye](https://rye-up.com) configured.
After install .NET 8, run the following commands to get the correct workload setup:

```bash
dotnet workload update
dotnet workload install aspire
```

### Running the sample in the repository

The repository contains a sample application which demonstrates various scenarios that are supported by this extension.
Before running the sample, make sure you've synced the environments:

```bash
pushd sample/apps/batch && rye sync && popd
pushd sample/apps/flask-service && rye sync && popd
```

Syncing the environments for the various components shouldn't take too long to complete. After completing the sync
step, you can run the host like so:

```
cd sample/host/FizzyLogic.Aspire.Python.AppHost
dotnet run
```
