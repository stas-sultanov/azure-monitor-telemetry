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

For instructions on how to use the library, please read [this document](/src/readme.md).

## Why This Library?

Any qualified engineer will naturally ask: why use this library if Microsoft provides an official one?

There are several compelling reasons why the author chose to invest time and effort into creating this library:

- [Microsoft.ApplicationInsights][app_insights_nuget_2_23] has flaws in its implementation.<br/>
  For instance, Entra authentication is implemented in a way that makes it impossible to use the library for plugin development.<br/>
  The issue is described by the author [here][app_insights_issue_auth], with little expectation that it will ever be fixed.
- [Microsoft.ApplicationInsights][app_insights_nuget_2_23] has extra dependencies.<br/>
  For instance, [System.Diagnostics.DiagnosticSource](https://www.nuget.org/packages/System.Diagnostics.DiagnosticSource/) >= v5.0.0, which is deprecated and has subsequent dependencies.<br/>
  Extra dependencies add extra effort in resolving issues, especially in cases like plugin development.
- [Microsoft.ApplicationInsights][app_insights_nuget_2_23] references NET452 and NET46 instead of NET462.<br/>
  Support for NET452 and NET46 ended on [April 26, 2022][dot_net_lifecycle].<br/>
  Support for NET462 ends on [January 12, 2027][dot_net_lifecycle].
- [Microsoft.ApplicationInsights][app_insights_nuget_2_23] is considered for deprecation.<br/>
  As of the end of 2024, Microsoft recommends switching to [OpenTelemetry](https://learn.microsoft.com/azure/azure-monitor/app/app-insights-overview).
- The [OpenTelemetry][open_telemetry_nuget] library is not designed for plugin development.<br/>
  It heavily relies on static types and members that do not implement a thread-safe singleton pattern.
- Both [Microsoft.ApplicationInsights][app_insights_nuget_2_23] and [OpenTelemetry][open_telemetry_nuget] are extremely heavy in some applications like NET462.<br/>
  This increases memory consumption and startup time.<br/>
  Take a look at the [comparison](#libraries-size-comparison) table.

### Libraries Size Comparison

A comparison of library sizes and file counts when used with Entra-based authentication:

| Package(s)                                   | NET462 | NET8 | NET9 |
| :------------------------------------------- | :----- | :--- | :--- |
| Stas.Azure.Monitor.Telemetry 1.0.0 <br/> | Files: 1<br/>Size:  42KB | Files:   1<br/>Size:   42KB | Files: 1<br/>Size:  42KB |
| Microsoft.ApplicationInsights 2.23.0 <br/> Azure.Core 1.13.2 | Files: 112<br/>Size: 4639KB | Files: 5<br/>Size: 945KB | Files: 5<br/>Size: 945KB |
| OpenTelemetry 1.11.1 <br/> Azure.Monitor.open_telemetry_nuget.Exporter  1.13.0 | Files: 126<br/>Size: 5243KB | Files: 32<br/>Size: 2386KB | Files:  26<br/>Size: 2233KB |

## Quality

Ensuring high quality is a top priority. This project enforces multiple quality gates to maintain reliability and robustness:

1. The ruleset is configured with a target on the *main* branch and allows changes via pull request only.
2. The workflow [Check][workflow_check] is configured to run on pull requests to the *main* branch and does the following:
    - Checks that the code builds with no errors.
    - Runs unit tests with a coverage threshold of 95%.
    - Runs integration tests with a coverage threshold of 90%.<br/>
      During integration tests, the workflow creates an environment within Azure and disposes of it once the tests complete.
3. The [CodeQL][workflow_code_ql] workflow is configured to run on pull requests and commits to the *main* branch.
4. The project build is configured to:
    - Treat all warnings as errors.
    - Set the warning level to 9999.
    - Enforce code style in the build via [editorconfig](/.editorconfig).
    - Use dotNet analyzers with analysis level **latest-all**.
    - Generate a documentation file to ensure that all public members are documented.

## Support the Author

Donations help the author know that the time and effort spent on this library is valued.

The author resides in a country affected by heavy military conflict since February 2022, making it extremely difficult to find stable employment. Donations provide significant support during these challenging times.

If you’d like to make a donation, please use the button below.

[![](https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=K2DPD6J3DJ2FN)

Thank you for your support!

[app_insights_issue_auth]: https://github.com/microsoft/ApplicationInsights-dotnet/issues/2945
[app_insights_nuget_2_23]: https://www.nuget.org/packages/Microsoft.ApplicationInsights/2.23.0
[azure_monitor]: https://docs.microsoft.com/azure/azure-monitor/overview
[dot_net_lifecycle]: https://learn.microsoft.com/lifecycle/products/microsoft-net-framework
[github_rule_sets]: https://docs.github.com/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/about-rulesets
[linked_in_profile]: https://www.linkedin.com/in/stas-sultanov
[open_telemetry_nuget]: https://www.nuget.org/packages/OpenTelemetry
[diagnostic_source_nuget]: https://www.nuget.org/packages/System.Diagnostics.DiagnosticSource
[workflow_code_ql]: https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/github-code-scanning/codeql
[workflow_check]: https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/check.yml
