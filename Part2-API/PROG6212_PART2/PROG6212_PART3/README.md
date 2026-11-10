
# RaceDay - Part 3

## Project Overview
RaceDay is an ASP.NET Core MVC application that consumes the Part 2 REST API. It provides separate interfaces for organisers and participants.

## Technologies
- ASP.NET Core MVC (.NET 8)
- C#
- REST API
- Entity Framework Core (Part 2 API)
- Bootstrap
- Docker

## Running Locally
1. Open the solution in Visual Studio.
2. Set PROG6212_PART2 and PROG6212_PART3 as multiple startup projects.
3. Start both projects.
4. Open the MVC application in your browser.

## Docker
A Dockerfile is included in the PROG6212_PART3 project.

To build the image from the project folder:

```bash
docker build -t raceday-mvc .
```

To run the container:

```bash
docker run -d -p 8080:8080 --name raceday-mvc raceday-mvc
```

Docker must be installed and running before these commands can be used.

## API
The MVC application communicates with the Part 2 REST API for application data and authentication.
