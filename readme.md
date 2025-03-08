# Azure Monitor Telemetry 
[![CodeQL](https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/github-code-scanning/codeql)
[![Check](https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/check.yml/badge.svg)](https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/check.yml)
[![Release](https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/release.yml/badge.svg)](https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/release.yml)
[![NuGet Version](https://img.shields.io/nuget/v/Stas.Azure.Monitor.Telemetry)](https://www.nuget.org/packages/Stas.Azure.Monitor.Telemetry)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Stas.Azure.Monitor.Telemetry)](https://www.nuget.org/packages/Stas.Azure.Monitor.Telemetry)

A lightweight, high-performance library for tracking and publishing telemetry.

Developed by [Stas Sultanov][StasSultanovLinkedIn], this library is designed for efficiency, prioritizing speed and minimal memory usage.

If this library benefits your business, consider [supporting the author](#support-the-author).

## Getting Started

For instructions on how to use the library please read [this document](/src/readme.md).

## Why This Library?

Any qualified engineer will naturally ask: why use this library if Microsoft provides the official SDK(s)?

Well, there are several compelling reasons why the author chose to invest life time and effort into creating this library:

- [Microsoft.ApplicationInsights][MSAppInsigthsNuget2_23] has flaws in implementation.<br/>
  For instance, take a look on the way how it handles Entra based authentication.<br/>
  The implementation has an issue, which is described by the author [here][AppInsightsDotNetGitHubAuthIssue].
- [Microsoft.ApplicationInsights][MSAppInsigthsNuget2_23] does not reference NET462 directly, which support end on [12 Jan 2027][NETLifeCycle].<br/>
  The Microsoft library references NET452 and NET46 which support ended on [26 Apr 2022][NETLifeCycle].
- [Microsoft.ApplicationInsights][MSAppInsigthsNuget2_23] considered for deprecation.<br/>
  As for Dec 2024 Microsoft recommends switching to Azure Monitor [OpenTelemetry](https://learn.microsoft.com/azure/azure-monitor/app/opentelemetry-enable) Distro.
- The [OpenTelemetry][OpenTelemetry] is not designed to be used for plugins development.<br/>
  The library heavily rely on use of static data which does not implement thread safe singleton pattern.
- Both [Microsoft.ApplicationInsights][MSAppInsigthsNuget2_23] and [OpenTelemetry][OpenTelemetry] are extremely heavy in some applications like NET462.<br/>
  This increases memory consumption and time to start.<br/>
  Take a look at the [comparison](#libraries-size-comparison).

### Libraries Size Comparison

A comparison of library sizes and file counts when used with Entra-based authentication:

| Package(s)                                   | NET462 | NET8 | NET9 |
| :------------------------------------------- | :----- | :--- | :--- |
| Stas.Azure.Monitor.Telemetry           1.0.0 <br/> | Files: 1<br/>Size:  42KB | Files:   1<br/>Size:   42KB | Files: 1<br/>Size:  42KB |
| Microsoft.ApplicationInsights         2.23.0 <br/> Azure.Core                            1.13.2 | Files: 112<br/>Size: 4639KB | Files: 5<br/>Size: 945KB | Files: 5<br/>Size: 945KB |
| OpenTelemetry                         1.11.1 <br/> Azure.Monitor.OpenTelemetry.Exporter  1.13.0 | Files: 126<br/>Size: 5243KB | Files: 32<br/>Size: 2386KB | Files:  26<br/>Size: 2233KB |

## Support the Author

Donations help the author know that the time and effort spent on this library is valued.

The author resides in a country affected by heavy military conflict since February 2022, making it extremely difficult to find stable employment. Donation provides significant support during these challenging times.

If you’d like to make a donation, please use the button below.

[![](https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=K2DPD6J3DJ2FN)

Thank you for your support!

[AppInsightsDotNetGitHubAuthIssue]: https://github.com/microsoft/ApplicationInsights-dotnet/issues/2945
[AzureMonitor]: https://docs.microsoft.com/azure/azure-monitor/overview
[MSAppInsigthsNuget2_23]: https://www.nuget.org/packages/Microsoft.ApplicationInsights/2.23.0
[NETLifeCycle]: https://learn.microsoft.com/lifecycle/products/microsoft-net-framework
[OpenTelemetry]: https://www.nuget.org/packages/OpenTelemetry
[StasSultanovLinkedIn]: https://www.linkedin.com/in/stas-sultanov
