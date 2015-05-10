# Requirements

1. Must be implemented as a Windows service using .Net 4.5 & C#.
2. All trade positions must be aggregated per hour (local/wall clock time). Note that for a given day, the actual local start time of the day is 23:00 (11 pm) on the previous day. Local time is in the GMT time zone.
3. CSV output format must be two columns, Local Time (format 24 hour HH:MM e.g. 13:00) and Volume and the first row must be a header row.
4. CSV filename must be PowerPosition_YYYYMMDD_HHMM.csv where YYYYMMDD is year/month/day e.g. 20141220 for 20 Dec 2014 and HHMM is 24hr time hour and minutes e.g. 1837. The date and time are the local time of extract.
5. The location of the CSV file should be stored and read from the application configuration file.
6. An extract must run at a scheduled time interval; every X minutes where the actual interval X is stored in the application configuration file. This extract does not have to run exactly on the minute and can be within +/- 1 minute of the configured interval.
7. It is not acceptable to miss a scheduled extract.
8. An extract must run when the service first starts and then run at the interval specified as above.
9. It is acceptable for the service to only read the configuration when first starting and it does not have to dynamically update if the configuration file changes. It is sufficient to require a service restart when updating the configuration.
10. The service must provide adequate logging for production support to diagnose any issues.

# Design

TopShelf is used to implement the Power Position Tool as a .NET 4.5 windows service written in C#. [REQ 1]

The position report computation is written as a TPL Dataflow pipeline. This allows scheduled position report requests to be queued awaiting completion.
The stages of the pipeline are:

* GetTrades
* ValidateTrades 
* BuildPosition [REQ 2] 
* BuildReport [REQ 3]
* WriteReport [REQ 3, REQ 4, REQ 5]

See Pipeline.cs for the implementation of the pipeline. The functions performed at each stage of the pipeline are in a separate file: PositionReport.cs. 
This makes the position report functionality easily testable outside of the pipeline.

Position report requests are scheduled by a Reative Extensions Generate function that fires at a configurable interval. [REQ 8]

The application configuration is read when the service starts from the application configuration file. See the Application Settings section below. [REQ 3, REQ 4, REQ 5, REQ 6, REQ 9]

Logging is provided by the log4net logging library. There is a separate configuration file for logging: log4net.config. [REQ 10] 

# Application Settings

* schedule-interval-minutes - the minute-level interval at which the position report is scheduled to be computed
* report-output-path - the output folder used to store position reports
* report-filename-format - the filename format to use for position reports
* report-filename-date-format - the data format used in the position report filename
* report-headers - the headers to use in the position report
* report-localtime-format - the time format to use in the position report
* pipeline-buffer-size" - the buffer size used by the position report computation pipeline

# Build

Open the soluion in Visual Studio 2013 and build.

Note that the application can be run and debugged in console mode by hitting F5.

The deployment artifacts will be in the src\PowerPositionTool\bin\Debug or Release folder.

# Installation

See the TopShelf documentation: http://docs.topshelf-project.com/en/latest/overview/commandline.html

The application can be run as a console application by running the PowerPositionTool.exe from the command line.

Run "PowerPositionTool.exe help" from the command line to see usage information and how to install and uninstall as a service.
 
# Dependencies

* log4net - used for logging
* Microsoft.Tpl.Dataflow - used for the computation pipeline
* Rx - used to schedule reports
* Topshelf - used to create a windows service application that can also be run as a console application
* Topshelf.Log4Net - integrates log4net into the windows service application
* PowerService - used as the source for power trades
* NUnit - used to define unit tests
