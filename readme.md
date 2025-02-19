# Azure Monitor Telemetry 
[![CodeQL](https://github.com/stas-sultanov/azure-monitor-telemetry-draft/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/stas-sultanov/azure-monitor-telemetry-draft/actions/workflows/github-code-scanning/codeql)
[![Check](https://github.com/stas-sultanov/azure-monitor-telemetry-draft/actions/workflows/check.yml/badge.svg)](https://github.com/stas-sultanov/azure-monitor-telemetry-draft/actions/workflows/check.yml)
[![Pack](https://github.com/stas-sultanov/azure-monitor-telemetry-draft/actions/workflows/pack.yml/badge.svg)](https://github.com/stas-sultanov/azure-monitor-telemetry-draft/actions/workflows/pack.yml)

A lightweight, high-performance library for tracking and publishing telemetry to [Azure Monitor](https://docs.microsoft.com/azure/azure-monitor/overview) / [Application Insights](https://docs.microsoft.com/azure/azure-monitor/app/app-insights-overview).

Developed by [Stas Sultanov](https://www.linkedin.com/in/stas-sultanov/), this library is designed for efficiency, prioritizing speed and minimal memory usage.

If you or your company find it useful, please consider [supporting the author](#support-the-author).

## Getting Started

To install the library, use the following command:

```sh
dotnet add package Stas.Azure.Monitor.Telemetry
```

For further instructions on how to use the library please read [this document](/src/readme.md).

## Why This Library?

Any qualified engineer will naturally ask: why use this library if Microsoft provides the official SDK(s)?

Well, there are several compelling reasons why the author chose to invest life time and effort into creating this library:

- No direct support of NET462.
Life-path of NET462 ends on [12 Jan 2027][NETLifeCycle] and still is widely used for development, for instance for development of plugins for Microsoft [Power Platform](https://www.microsoft.com/power-platform).
Of cause it is possible to use Microsoft SDKs in applications that targets NET462, but it works via [.net standard 2.0](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0),
and this requires adding extra dlls that increase time to start and memory consumption.
- [Microsoft Application Insights for .NET][AppInsightsDotNetGitHub] has very strange decisions in implementation.
For instance take a look on the way how it handles Entra based authentication. The implementation causes an issue, which is described by the author [here][AppInsightsDotNetGitHubAuthIssue].
- Limited application.
The way official sdk is implement, does not work for certain scenarios like developing plugins for Power Platform.
- As for Dec 2024 Microsoft recommends the [Azure Monitor OpenTelemetry Distro](https://learn.microsoft.com/azure/azure-monitor/app/opentelemetry-enable?tabs=aspnetcore#enable-azure-monitor-opentelemetry-for-net-nodejs-python-and-java-applications) for new applications or customers to power [Azure Monitor Application Insights](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
- Neither official Microsoft package nor OpenTelemetry with Exporter are not designed to work as fast as possible and have the smallest memory footprint possible.

Considering these factors, the author built this library from scratch with a focus on performance, low memory usage, and support of .NET versions which still in LTS, making it an ideal choice for scenarios where efficiency is critical.

## Supported telemetry types

The library supports all types of telemetry which are currently supported by the [Azure Monitor](https://docs.microsoft.com/azure/azure-monitor/overview).

All types, that represent different types of telemetry, implements the [Telemetry](/src/Code/Telemetry.cs) interface.

| Type                                                              | Represents
| :---------------------------------------------------------------- | :-
| [AvailabilityTelemetry](/src/Code/Types/AvailabilityTelemetry.cs) | An availability test.
| [DependencyTelemetry](/src/Code/Types/DependencyTelemetry.cs)     | A dependency call in an application.
| [EventTelemetry](/src/Code/Types/EventTelemetry.cs)               | An event that occurred in an application.
| [ExceptionTelemetry](/src/Code/Types/ExceptionTelemetry.cs)       | An exception that occurred in an application.
| [MetricTelemetry](/src/Code/Types/MetricTelemetry.cs)             | An aggregated metric data.
| [PageViewTelemetry](/src/Code/Types/PageViewTelemetry.cs)         | A page view.
| [RequestTelemetry](/src/Code/Types/RequestTelemetry.cs)           | An external request to an application.
| [TraceTelemetry](/src/Code/Types/RequestTelemetry.cs)             | Printf-style trace statement.

## Support the Author

Donations help the author know that the time and effort spent on this library is valued.

The author resides in a country where there has been military action since February 2022, making it extremely difficult to find stable employment. Donation provides significant support during these challenging times.

If you’d like to make a donation, please use the button below.

[![](https://www.paypalobjects.com/en_US/i/btn/btn_donate_SM.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=K2DPD6J3DJ2FN)

Thank you for your support!

[AppInsightsDotNetGitHub]: https://github.com/microsoft/ApplicationInsights-dotnet
[AppInsightsDotNetGitHubAuthIssue]: https://github.com/microsoft/ApplicationInsights-dotnet/issues/2945
[AzureInsightsComponentsResource]: https://learn.microsoft.com/azure/templates/microsoft.insights/components
[NETLifeCycle]: https://learn.microsoft.com/lifecycle/products/microsoft-net-framework