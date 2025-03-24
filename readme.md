# Azure Monitor Telemetry 

[![CodeQL](https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/github-code-scanning/codeql)
[![Check](https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/check.yml/badge.svg)](https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/check.yml)
[![Release](https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/release.yml/badge.svg)](https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/release.yml)
[![NuGet Version](https://img.shields.io/nuget/v/Stas.Azure.Monitor.Telemetry)](https://www.nuget.org/packages/Stas.Azure.Monitor.Telemetry)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Stas.Azure.Monitor.Telemetry)](https://www.nuget.org/packages/Stas.Azure.Monitor.Telemetry)

A lightweight library for tracking application telemetry with Azure Monitor.

Created by [Stas Sultanov][linked_in_profile], this library is designed for use in applications of all types.

Consider [supporting the author](#support-the-author), if this library benefits your business.

## Start Using the Library

For usage instructions, refer to the [documentation](/doc/readme.md).

## Key Drivers

This library has been developed with a strong focus on business adaptability to serve IT solutions of all kinds.<br/>
To achieve this, the following key drivers were considered:

- **Simplicity** – Minimize the number of classes, relationships, and hierarchies to keep the design as lean as possible.  
- **Transparency** – Gain full visibility into how telemetry is tracked and published, without unnecessary abstraction layers.  
- **Efficiency** – Run faster and consume less memory thanks to minimal code and low complexity.  
- **Responsibility** – Embrace control over how telemetry is tracked and managed, with decisions left to the developer.  
- **Extensibility** – Extend or customize telemetry handling through clearly defined extension points.  
- **Versatility** – Use in any type of application, from distributed systems to standalone apps and plugins.

## Quality Assurance

This library is built to meet the highest quality standards expected in regulated and performance-critical environments.<br/>
This commitment is reflected in the following safeguards:

1. The repository is configured with branch protection rules targeting the *main* branch, including the following:
    - Require [signed commits][github_docs_verified_commit].
    - Require a pull request before merging, with the allowed method: *Squash*.
    - Require code scanning results via [CodeQL][github_workflow_code_ql].
2. The workflow [Check][github_workflow_check] is configured to run on pull requests to the *main* branch and performs the following:
    - Checks that the code builds with no errors.
    - Executes unit tests with a coverage threshold of 95%.
    - Executes integration tests with at least 90% coverage, using a temporary Azure environment that is automatically provisioned and disposed.
3. The workflow [Release][github_workflow_release] is created with support for [artifact attestations][github_docs_artifact_attestations].
4. The project build is configured to: 
    - Treat all warnings as errors.
    - Set the warning level to 9999.
    - Enforce code style in the build via [editorconfig](/.editorconfig).
    - Use [dotNet analyzers][dot_net_analyzers] with analysis level **latest-all**.
    - Ensure that all public members are documented.

These strict quality controls ensure a reliable, efficient telemetry solution suitable for businesses of any size.

## Why This Library?

It's fair to ask: why use this library when Microsoft already provides an official SDK?

There are several technical reasons why this library was developed:

- [Microsoft.ApplicationInsights][app_insights_nuget_2_23] has flaws in its implementation.<br/>
  For instance, Entra authentication is implemented in a way that makes it impossible to use the library for plugin development.<br/>
  The issue is described by the author [here][app_insights_issue_auth], with little expectation that it will ever be fixed.
- [Microsoft.ApplicationInsights][app_insights_nuget_2_23] has dependencies.<br/>
  For instance, [System.Diagnostics.DiagnosticSource](https://www.nuget.org/packages/System.Diagnostics.DiagnosticSource/) >= v5.0.0, which is deprecated and introduces additional dependency challenges.<br/>
  In case of plugin development, dependencies can cause version resolution conflicts.
- [Microsoft.ApplicationInsights][app_insights_nuget_2_23] references NET452 and NET46 instead of NET462.<br/>
  Support for NET452 and NET46 ended on [April 26, 2022][dot_net_lifecycle].<br/>
  Support for NET462 ends on [January 12, 2027][dot_net_lifecycle].
- [Microsoft.ApplicationInsights][app_insights_nuget_2_23] is considered for deprecation.<br/>
  As of the end of 2024, Microsoft recommends switching to [OpenTelemetry](https://learn.microsoft.com/azure/azure-monitor/app/app-insights-overview).
- The [OpenTelemetry][open_telemetry_nuget] library is not designed for plugin development.<br/>
  It heavily relies on static types and members that do not implement a thread-safe singleton pattern.
- Both [Microsoft.ApplicationInsights][app_insights_nuget_2_23] and [OpenTelemetry][open_telemetry_nuget] are extremely heavy in some applications like NET462.<br/>
  This increases memory consumption and startup time.<br/>
  Take a look at the [comparison](#libraries-size-comparison).

### Libraries Size Comparison

A comparison of sizes and file counts of libraries when used with Entra-based authentication:

| Containing Package(s) | NET462 | NET8 | NET9 |
| :-------------------- | :----- | :--- | :--- |
| Stas.Azure.Monitor.Telemetry 1.0.0 <br/> | Files: 1<br/>Size:  52KB | Files: 1<br/>Size: 51KB | Files: 1<br/>Size: 51KB |
| Microsoft.ApplicationInsights 2.23.0 <br/> Azure.Core 1.13.2 | Files: 112<br/>Size: 4639KB | Files: 5<br/>Size: 945KB | Files: 5<br/>Size: 945KB |
| OpenTelemetry 1.11.2 <br/> Azure.Monitor.OpenTelemetry.Exporter 1.3.0 | Files: 126<br/>Size: 5243KB | Files: 32<br/>Size: 2386KB | Files: 26<br/>Size: 2233KB |

## Support the Author

Donations help the author know that the time and effort spent on this library is valued.

The author resides in a country affected by heavy military conflict since February 2022, making it extremely difficult to find stable income. Donations provide significant support during these challenging times.

If you’d like to make a donation, please use the button below.

[![](https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=K2DPD6J3DJ2FN)

Thank you for your support!

[app_insights_issue_auth]: https://github.com/microsoft/ApplicationInsights-dotnet/issues/2945
[app_insights_nuget_2_23]: https://www.nuget.org/packages/Microsoft.ApplicationInsights/2.23.0
[azure_monitor]: https://docs.microsoft.com/azure/azure-monitor/overview
[diagnostic_source_nuget]: https://www.nuget.org/packages/System.Diagnostics.DiagnosticSource
[dot_net_lifecycle]: https://learn.microsoft.com/lifecycle/products/microsoft-net-framework
[dot_net_analyzers]: https://learn.microsoft.com/dotnet/fundamentals/code-analysis/overview
[github_docs_rule_sets]: https://docs.github.com/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/about-rulesets
[github_docs_verified_commit]: https://docs.github.com/authentication/managing-commit-signature-verification
[github_docs_artifact_attestations]: https://docs.github.com/actions/security-for-github-actions/using-artifact-attestations
[github_workflow_code_ql]: https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/github-code-scanning/codeql
[github_workflow_check]: https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/check.yml
[github_workflow_release]: https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/release.yml
[linked_in_profile]: https://www.linkedin.com/in/stas-sultanov
[open_telemetry_nuget]: https://www.nuget.org/packages/OpenTelemetry
