# Azure Monitor Telemetry 

A lightweight, high-performance library for tracking and publishing telemetry.

The library is developed by Stas Sultanov. If you find it useful, please consider [supporting the author](#support-the-author).

## Why This Library?

A natural question for any qualified engineer is: why use this library when Microsoft provides an official one?

Well, there are several compelling reasons why the author chose to invest time and effort into creating this library:

- As for Jan 2025 Microsoft recommends stop using its official package and recommending OpenTelemetry with an exporter instead.
- Neither official Microsoft package nor OpenTelemetry with Exporter are not designed to work as fast as possible and have the smallest memory footprint possible.
- Lack of .NET 4.6.2 support. While of cause you can use this libraries via [.net standard 2.0](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0) – this requires adding netstehuth.dll to the project which increase time to start and memory consumption.
- Heavy footprint – The official SDK is ~360KB, whereas this library is only ~40KB, making it significantly more lightweight.
- Limited flexibility – It does not work well in certain cases, such as developing plugins for Power Platform.
- Complexity – The official SDK is not simple, clear, or easy to understand.

Considering these factors, the author built this library from scratch with a focus on performance, low memory usage, and support of .NET versions which still in LTS, making it an ideal choice for scenarios where efficiency is critical.

## Getting Started

To install the library, use the following command:

```sh
dotnet add package Stas.Azure.Monitor.Telemetry
```

For information how to use the library please read [this document](/src/readme.md).

## Supported telemetry types

The library supports all types of telemetry which are currently supported by the Azure Monitor.

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