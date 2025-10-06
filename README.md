# system-health-dashboard

## Setup Instructions

**System Health Dashboard and SystemHealthAPI Setup**:
From the root of the project run: 
```
docker-compose up -d --build
``` 
Builds and runs the Docker container (with both the Nginx server serving Angular frontend and the .NET API backend) - detached from the CLI

**OSMetricsRetriever Setup (Supported on Windows only)**:
Run 
```
cd src/OSMetricsRetriever
dotnet publish -c Release --self-contained true -r win-x64 -p:PublishSingleFile=true -o ./publish
```

The exe can be run manually a few times to get metrics, or to schedule the retrieval run **Windows Task Scheduler**:
1. Click "Create Task..."
2. Configure as shown:

<img width="648" height="603" alt="Pasted image 20251005210354" src="https://github.com/user-attachments/assets/de0253b6-4c1a-4579-bcaf-d2248967d772" />


Choose how often to run the exe with a *Trigger*:

<img width="637" height="671" alt="Pasted image 20251005210627" src="https://github.com/user-attachments/assets/716da767-9c0c-437e-ba96-d92a1ce9a239" />


Add starting the exe as an *Action* by adding the filepath to the exe published at `system-health-dashboard\src\OSMetricsRetriever\publish\OSMetricsRetriever.exe`:

<img width="632" height="480" alt="Pasted image 20251005210558" src="https://github.com/user-attachments/assets/b1d4b073-1498-4f1f-ac89-84d190367e88" />


### Useful Docker Container Management Commands
```bash
# View running containers
docker ps

# View container logs
docker logs {container-name}

# Stop the container
docker stop {container-name}

# Remove the container
docker rm {container-name}

# Remove the image
docker rmi {container-name}
```


## Architecture Overview

**Overall**:
One of my main goals that is reflected in my architecture decisions was to make it as simple as possible to add a new metric to be monitored. With the implemented design, a new metric can be added by simply adding a plugin to the OSMetricsRetriever and it will be propagated to the API and displayed on the frontend dashboard without any changes to them.

- I created a 3-tiered full-stack application where an API handles state management, a standalone tool retrieves configured OS metrics - sending them to the API, and a frontend dashboard that retrieves all metrics from the API to display

![System Health Monitoring 1](https://github.com/user-attachments/assets/8defef06-703a-4e5f-845f-ee545fbc11b3)



**SystemHealthAPI**:
- To avoid depending on potentially volatile concrete classes, I used abstraction in the form of interfaces between the API controller and the injected as well as between the service and the database. This makes it simple to swap out the database or even the business logic in the service. This follows the *Dependency Inversion Principle*
- I also used dependency injection to orchestrate the creation of services, following principles of *Inversion of Control*
- An exception handling middleware was added to the API because it helps keep all the handling in a single location which is especially important when the backend becomes very large (it can traditionally be hard to keep track of all the exceptions and where they are handled). This also helps coordinate which HTTP status codes are returned by the controller when errors occur. 

![SystemHealthAPI 1](https://github.com/user-attachments/assets/75833895-3f50-4429-9c91-37bffb63a763)

- The separation line represents the abstraction layer separation

**OSMetricsRetriever**:
- I designed this small tool to be a one-time run app which retrieves all the configured metrics and sends them to the API and then terminates
- The tool needs to be run via some external scheduler. Which, in the case of Windows, is the Task Scheduler
- I chose to use a plugin architecture for the metric retrieval logic so that it is very easy to add new ones to the tool: simply create a new plugin class that retrieves a metric from the operating system and then add it to the list of plugins

![OSMetricsRetriever](https://github.com/user-attachments/assets/a1ccb283-f152-4b25-8166-281e6b7b27da)


**SystemHealthDashboard**:
- Used an external package to create charts for each metric retrieved from the API
- Ensured that the frontend was un-opinioned about the metrics. It handles the presentation but not *which* metrics are supported

<img width="635" height="262" alt="Pasted image 20251005191302" src="https://github.com/user-attachments/assets/284ae840-fb6a-431d-9cfb-bbac7c6a482d" />


<img width="2560" height="1313" alt="Pasted image 20251005191345" src="https://github.com/user-attachments/assets/da95c83d-86ac-4117-aa19-fb9b42b448e3" />

### Alerting System Design
**How solution would track disk usage trends over time?**
- I would use a Time Series Database (such as InfluxDB or Prometheus) which is optimized for time-based data storage and querying. 
	- Built-in functions for time bucketing, aggregation and trend analysis which makes it optimized for queries such as "how much CPU utilization in the past 10 hours?"
**What constitutes “running low”**
- Whether the computer is running low on memory or disk space should not only depend on the percentage of space left but also on the historical rate of usage.
	- If the computer is at 90% memory capacity but the usage has been going down quickly for the past minute, there should not be a notification.
	- We could make a rule such as: if the **average rate of utilization** over the past **x** hours would result in reaching max capacity in **n** days -> send a warning notification
	- Example: `if the average mb/s disk increase over the past 12 hours would result in reaching max capacity in 5 days`
	- Where increasing **x** would cover more time for the average, whereas lowering it makes the average more relevant to what is currently occurring on the system
	- Where increasing **n** gives a potentially less accurate guess for when it will reach max capacity but a sooner warning for the human monitor

This idea is mainly for monitoring computers that are running servers whose memory/disk usage is continuously growing, rather than a human user who uses the computer in a more random/human way.

**Alert delivery mechanism**
- A webhook for a UI notification would only be useful if someone was constantly monitoring the system on a dashboard whereas an email can be received on any device.
- Could setup these conditions in the Time Series database.

**How to prevent alert fatigue**
- Store the notifications as timestamped data in the time series database, then when we are deciding whether to send a warning notification, check that the same notification has not been sent too recently

## Design Decisions:
A lot of my architecture choices revolved around the idea of making it as easy as possible to add new OS metrics to the system to monitor.
- For example: I made a generic OSMetric model object that would represent all types of metrics which meant the API and database wouldn't need to be changed at all to add a new type
- This comes with some significant tradeoffs however. The historical data of all metrics are given to the frontend even if it only needs the latest data point for a certain metric such as "Current Memory Usage" for example.

### OS Metrics Retriever
**Option 1**: built into an injected *service of the API* with clear boundaries. This service could start an indefinite timer that continuously runs the OS metric gathering logic
pros:
- Simple to implement
- Less parts to the system
cons:
- Not designed to scale as the API would need to be on each machine to monitor it

**Option 2**: build a *separate service* application that is scheduled on a machine that calls back to the API to provide the metrics
pros:
- Scalable solution: could be installed on many machines to monitor them, all calling back to a central API
- Decouples the service and the API which helps with separation of concerns
cons:
- Harder to implement
- Requires a scheduling mechanism
- Windows Task Scheduler has a minimum repeat time of 5 minutes (not very often)

I chose option 2 because of the greater flexibility and better separation of concerns, protecting the core business logic (OS metric gathering code)
### Scheduling Mechanism
**Option 1**: Service could start an *indefinite timer* to run the metric gathering logic at specified intervals. Would need to be a Windows service to run in the background.
pros:
- Simple to implement and setup
cons:
- Hard to scale

**Option 2**: Schedule the metric gathering to be run by an OS-specific scheduler (*Task Scheduler* on Windows, cron job on Linux)
pros:
- Standardized scheduling that is already well established
- Prebuilt configuration for how often to run the application
- Can run the application even when the user is not logged in
cons:
- Potentially requires manual setup on Windows

I chose option 2 because of the ability to easily configure how often the job runs


## OS-specific Monitoring Choices:
### How to retrieve system-wide metrics on Windows?

**Option 1**: Using *Windows API*
Pros:
- Adds no extra dependencies.
Cons:
- Quite complex low level API interaction

**Option 2**: Using *Windows Management Instrumentation*
Pros:
- Provides all kinds of metrics
- Scope can be shared across plugins, improving query efficiency
Cons:
- Slower than direct API access

I chose option 2 for its simplicity.

### Questions and Answers
- Why was specific operating system (Ubuntu or Windows) chosen?
	- I chose Windows primarily because of familiarity. I do not own a computer that is running Linux natively (although I plan to soon)
- What makes chosen metrics valuable for OS monitoring? -> assuming that servers are being monitored:
	- CPU utilization is valuable to monitor because it is representative of how loaded a system is, if the server is at too high a load, it will have degraded performance and potentially start dropping requests because of timeouts. Historical data is also useful when a developer is trying to determine a root-cause of a bug, they can correlate error log entry time with when the CPU spiked.
	- Memory usage is valuable to monitor because if a computer runs out of available memory, it falls back to writing memory to the disk and even if it is an SSD, it is still orders of magnitude slower than memory. This will severely slow down a system.
	- Disk usage is valuable to monitor because if a computer runs out of available disk space, many applications will start failing with exceptions "out of storage" which is not ideal. IT would want to know if a drive is approaching max-capacity so they can take action early.
- How would solution be extended to a production scale?
	- The OSMetricRetriever could be installed automatically on many machines by using automation deployment tools such as Terraform or Puppet.
	- The API and Angular server could be deployed to a Kubernetes cluster and then configured to have multiple pods for redundancy and high availability
- What trade-offs were made given the timing constraints?
	- Didn't add more metrics to monitor
	- The OSMetricRetriever only supports monitoring Windows machines
	- HTTP traffic is unencrypted
	- Simple memory-based database instead of a proper Time-Series database
	- The API model can only handle one machine as there is no concept of "metrics per machine"
	- If I had more time I would have refactored the model to have a list of values instead of just one to decouple the metadata about a metric from the values

## Use of LLMs
- Jumpstarted the repository by using Github Copilot to create boilerplate basic skeleton of an Angular app and a .NET 8 API
- Used the Copilot reviewer feature on my PRs into main branch
- Used Claude Sonnet 4 in Agent mode to benefit from the iterative approach and the many tools made available to the LLM
- Note that I did not use LLMs in the writing of this README as I wanted it to be in my own words

Some of the prompts used:
```
Create a basic "hello world" application using the indicated technologies.

A front-end written in TypeScript using Angular 19 as the framework. Use yarn as the package manager. Setup unit tests using Jasmine and Karma.

A back-end HTTP REST API written in C# in the .NET 8 framework. Setup unit tests using Nunit. 
```

```
Add the default .NET Core injection framework to the API. The InMemoryDataAccessService should be injected into the SystemMetricService and used in the methods. The SystemMetricService should be injected into the SystemMetricsController and used in the endpoint methods.
```

```
Write unit tests for this solution. For the controller and each service. Use mocks for any dependencies.
```

```
Implement IRetrieveMetricsPlugin in the rest of the plugins, using CPUUtilizationPlugin as an example for how to format everything.
```

## Test Coverage:
Frontend:
<img width="2559" height="353" alt="Pasted image 20251005193832" src="https://github.com/user-attachments/assets/0f6a0034-92ad-4d41-a840-0d905e521942" />

Backend: 
To generate HTML code coverage reports for the .NET test projects, run:
- **Linux/Mac**: `./generate-code-coverage.sh`
- **Windows**: `generate-code-coverage.bat`

The scripts will:
1. Run tests with code coverage for `SystemHealthAPITests` and `OSMetricsRetrieverTests` (Windows only)
2. Generate HTML reports in the `coverage-reports/` directory
3. Output the location of the HTML reports to open in your browser

**Prerequisites**: 
- Install ReportGenerator tool: `dotnet tool install --global dotnet-reportgenerator-globaltool`

The test projects use MSTest with the `Microsoft.Testing.Extensions.CodeCoverage` package to collect coverage data in Cobertura format, which is then converted to HTML using ReportGenerator.

