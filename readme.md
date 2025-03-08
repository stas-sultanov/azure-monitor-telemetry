# Azure Monitor Telemetry 
[![CodeQL](https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/github-code-scanning/codeql)
[![Check](https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/check.yml/badge.svg)](https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/check.yml)
[![Release](https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/release.yml/badge.svg)](https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/release.yml)
[![NuGet Version](https://img.shields.io/nuget/v/Stas.Azure.Monitor.Telemetry)](https://www.nuget.org/packages/Stas.Azure.Monitor.Telemetry)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Stas.Azure.Monitor.Telemetry)](https://www.nuget.org/packages/Stas.Azure.Monitor.Telemetry)

A lightweight, high-performance library for tracking and publishing telemetry.

Developed by [Stas Sultanov][linked_in_profile], this library is designed for efficiency, prioritizing speed and minimal memory usage.

If this library benefits your business, consider [supporting the author](#support-the-author).

## Getting Started

For instructions on how to use the library please read [this document](/src/readme.md).

## Why This Library?

Any qualified engineer will naturally ask: why use this library if Microsoft provides the official one?

Well, there are several compelling reasons why the author chose to invest life time and effort into creating this library:

- [Microsoft.ApplicationInsights][app_insights_nuget_2_23] has flaws in implementation.<br/>
  For instance, Entra authentication is implemented the way it makes impossibility to use the library for Plugins development.
  The issue is described by the author [here][app_insights_issue_auth], with 0 expectation that it will be ever fixed.
- [Microsoft.ApplicationInsights][app_insights_nuget_2_23] has references.<br/>
  To [System.Diagnostics.DiagnosticSource](https://www.nuget.org/packages/System.Diagnostics.DiagnosticSource/) v5.0.0 which is deprecated.
  The package has subsequent references.
  Extra references adds extra effort on resolving.
- [Microsoft.ApplicationInsights][app_insights_nuget_2_23] does not reference NET462 directly.
  NET462 support ends on [12 Jan 2027][dot_net_lifecycle].<br/>
  The Microsoft library references NET452 and NET46 which support ended on [26 Apr 2022][dot_net_lifecycle].
- [Microsoft.ApplicationInsights][app_insights_nuget_2_23] considered for deprecation.<br/>
  As of end of year 2024 Microsoft recommends switching to Azure Monitor [open_telemetry_nuget](https://learn.microsoft.com/azure/azure-monitor/app/open_telemetry_nuget-enable) Distro.
- The [open_telemetry_nuget][open_telemetry_nuget] is not designed to be used for plugins development.<br/>
  The library heavily rely on use of static data which does not implement thread safe singleton pattern.
- Both [Microsoft.ApplicationInsights][app_insights_nuget_2_23] and [open_telemetry_nuget][open_telemetry_nuget] are extremely heavy in some applications like NET462.<br/>
  This increases memory consumption and time to start.<br/>
  Take a look at the [comparison](#libraries-size-comparison) table.

### Libraries Size Comparison

A comparison of library sizes and file counts when used with Entra-based authentication:

| Package(s)                                   | NET462 | NET8 | NET9 |
| :------------------------------------------- | :----- | :--- | :--- |
| Stas.Azure.Monitor.Telemetry           1.0.0 <br/> | Files: 1<br/>Size:  42KB | Files:   1<br/>Size:   42KB | Files: 1<br/>Size:  42KB |
| Microsoft.ApplicationInsights         2.23.0 <br/> Azure.Core                            1.13.2 | Files: 112<br/>Size: 4639KB | Files: 5<br/>Size: 945KB | Files: 5<br/>Size: 945KB |
| open_telemetry_nuget                         1.11.1 <br/> Azure.Monitor.open_telemetry_nuget.Exporter  1.13.0 | Files: 126<br/>Size: 5243KB | Files: 32<br/>Size: 2386KB | Files:  26<br/>Size: 2233KB |

## Support the Author

Donations help the author know that the time and effort spent on this library is valued.

The author resides in a country affected by heavy military conflict since February 2022, making it extremely difficult to find stable employment. Donation provides significant support during these challenging times.

If you’d like to make a donation, please use the button below.

[![](https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=K2DPD6J3DJ2FN)

Thank you for your support!

[app_insights_issue_auth]: https://github.com/microsoft/ApplicationInsights-dotnet/issues/2945
[app_insights_nuget_2_23]: https://www.nuget.org/packages/Microsoft.ApplicationInsights/2.23.0
[azure_monitor]: https://docs.microsoft.com/azure/azure-monitor/overview
[dot_net_lifecycle]: https://learn.microsoft.com/lifecycle/products/microsoft-net-framework
[linked_in_profile]: https://www.linkedin.com/in/stas-sultanov
[open_telemetry_nuget]: https://www.nuget.org/packages/open_telemetry_nuget
[nuget_System_Diagnostics_DiagnosticSource] https://www.nuget.org/packages/System.Diagnostics.DiagnosticSource/