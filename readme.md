# Azure Monitor Telemetry 

[![CodeQL](https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/github-code-scanning/codeql)
[![Check](https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/check.yml/badge.svg)](https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/check.yml)
[![Release](https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/release.yml/badge.svg)](https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/release.yml)
[![NuGet Version](https://img.shields.io/nuget/v/Stas.Azure.Monitor.Telemetry)](https://www.nuget.org/packages/Stas.Azure.Monitor.Telemetry)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Stas.Azure.Monitor.Telemetry)](https://www.nuget.org/packages/Stas.Azure.Monitor.Telemetry)

A lightweight library for tracking application telemetry with Azure Monitor.

Authored and maintained by [Stas Sultanov][linked_in_profile].

[Support the author](#support-the-author), if this library benefits your business.

## Start Using the Library

For usage instructions, refer to the [documentation](/doc/readme.md).

## Key Drivers

This library has been developed with a strong focus on business adaptability to serve IT solutions of all kinds.<br/>
The library is guided by the following key design principles:

- **Efficiency** – Run faster and consume less memory thanks to minimal code and low complexity.
- **Responsibility** – Embrace control over how telemetry is tracked and managed, with decisions left to the developer.
- **Simplicity** – Minimize the number of classes, relationships, and hierarchies to keep the design as lean as possible.
- **Transparency** – Gain full visibility into how telemetry is tracked and published, without unnecessary abstraction layers.
- **Versatility** – Use in any type of application, from distributed systems to standalone apps and plugins.

## Quality Assurance

Strict quality standards are applied throughout the development of this library and reflected in the following safeguards:

1. The repository is configured with branch protection [rules][github_docs_rule_sets] targeting the *main* branch, including the following:
    - Require [signed commits][github_docs_verified_commit].
    - Require a pull request before merging, with the allowed method: *Squash*.
    - Require code scanning results via [CodeQL][github_workflow_code_ql].
2. The workflow [Check][github_workflow_check] is configured to run on pull requests to the *main* branch and performs the following:
    - Checks that the code builds with no errors.
    - Executes unit tests with a coverage threshold of 95%.
    - Executes integration tests with a coverage threshold of 75%, using a temporary Azure environment that is automatically provisioned and disposed.
3. The workflow [Release][github_workflow_release] is created with support for [artifact attestations][github_docs_artifact_attestations].
4. The project build is configured to:
    - Treat all warnings as errors.
    - Set the warning level to 9999.
    - Enforce code style in the build via [editorconfig](/.editorconfig).
    - Use [dotNet analyzers][dot_net_analyzers] with analysis level **latest-all**.
    - Ensure that all public members are documented.

## Why This Library?

Why build or use a custom library when Microsoft already provides an official SDK?

There are several technical reasons why this library was developed:

- The [OpenTelemetry][nuget_open_telemetry] with [Azure.Monitor.OpenTelemetry.Exporter][nuget_azure_monitor_opentelemetry_exporter] is not designed for plugin development.<br/>
  Implementation relies on static members that do not implement a thread-safe singleton pattern.
- The [Microsoft.ApplicationInsights][nuget_app_insights__2_23] has critical flaw in its implementation.<br/>
  Entra authentication is implemented in a way that makes it impossible to use the library for plugin development.<br/>
  The issue is described by the author [here][app_insights_issue_auth], with no expectation that it will ever be fixed.
- Both [Microsoft.ApplicationInsights][nuget_app_insights__2_23] and [OpenTelemetry][nuget_open_telemetry] with [Azure.Monitor.OpenTelemetry.Exporter][nuget_azure_monitor_opentelemetry_exporter] are extremely heavy in some applications like NET462.<br/>
  See the comparison below for a breakdown of size and complexity.

### Libraries Size Comparison

A comparison of sizes and file counts of libraries when used with Entra-based authentication:

| **Package(s)**                                                      | **NET462**                  | **NET8**                   | **NET9**                   |
| :------------------------------------------------------------------ | :-------------------------- | :------------------------- | :------------------------- |
| Stas.Azure.Monitor.Telemetry 1.1.0                                  | Files: 1<br/>Size: 61KB     | Files: 1<br/>Size: 60KB    | Files: 1<br/>Size: 60KB    |
| Microsoft.ApplicationInsights 2.23.0<br/>Azure.Core 1.45.0          | Files: 109<br/>Size: 4644KB | Files: 5<br/>Size: 945KB   | Files: 5<br/>Size: 945KB   |
| OpenTelemetry 1.11.2<br/>Azure.Monitor.OpenTelemetry.Exporter 1.3.0 | Files: 126<br/>Size: 5250KB | Files: 23<br/>Size: 1887KB | Files: 22<br/>Size: 1728KB |

## Support the Author

Donations express appreciation for the author’s dedication and the substantial effort invested in creating this library.

The author resides in a country affected by ongoing military conflict since February 2022.<br/>
Due to the war, securing stable income is extremely difficult, and donations provide essential support.

If you’d like to make a donation, please use the button below:

[![](https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=K2DPD6J3DJ2FN)

Any support is much appreciated!

[app_insights_issue_auth]: https://github.com/microsoft/ApplicationInsights-dotnet/issues/2945
[dot_net_analyzers]: https://learn.microsoft.com/dotnet/fundamentals/code-analysis/overview
[github_docs_rule_sets]: https://docs.github.com/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/about-rulesets
[github_docs_verified_commit]: https://docs.github.com/authentication/managing-commit-signature-verification
[github_docs_artifact_attestations]: https://docs.github.com/actions/security-for-github-actions/using-artifact-attestations
[github_workflow_code_ql]: https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/github-code-scanning/codeql
[github_workflow_check]: https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/check.yml
[github_workflow_release]: https://github.com/stas-sultanov/azure-monitor-telemetry/actions/workflows/release.yml
[linked_in_profile]: https://www.linkedin.com/in/stas-sultanov
[nuget_app_insights__2_23]: https://www.nuget.org/packages/Microsoft.ApplicationInsights/2.23.0
[nuget_azure_monitor_opentelemetry_exporter]: https://www.nuget.org/packages/Azure.Monitor.OpenTelemetry.Exporter
[nuget_open_telemetry]: https://www.nuget.org/packages/OpenTelemetry
