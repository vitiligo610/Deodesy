# Getting Started

Welcome to Deodesy, a robust .NET library for geographical calculations. This guide will help you set up the library and start performing geodesic computations in your applications.

## Prerequisites

Before you begin, ensure you have the following installed:
*   [.NET SDK](https://dotnet.microsoft.com/download) (6.0 or later recommended)
*   A C# IDE (Visual Studio, JetBrains Rider, or VS Code)

## Building from Source

To build the library locally, clone the repository and use the .NET CLI:

```bash
# Navigate to the library directory
cd src/Deodesy.Library

# Build the project
dotnet build -c Release
```

The compiled binaries will be available in the `src/Deodesy.Library/bin/Release/netstandard2.0` directory.

## Target Framework & Compatibility

Deodesy targets _.NET Standard 2.0_, providing maximum reach across the .NET ecosystem.

| Platform | Minimum Version |
| :--- | :--- |
| .NET & .NET Core | 2.0+ |
| .NET Framework | 4.6.1+ |
| Mono | 5.4+ |
| Xamarin.iOS / Xamarin.Android | 10.14+ / 8.0+ |

## Installation

You can integrate Deodesy into your project by adding a reference to the project file or the compiled DLL.

### Via .NET CLI
Run the following command from your project's directory:
```bash
dotnet add reference path/to/Deodesy.Library.csproj
```

### Via Visual Studio
1. Right-click your project in **Solution Explorer**.
2. Select **Add > Project Reference...**
3. Browse and select `Deodesy.Library.csproj`.

## Quick Start Example

Here is a simple example demonstrating how to calculate the distance between two geographical points using the spherical model.

```csharp
using Deodesy.Library;

// Define coordinates for London and Washington D.C.
var london = new Coordinate(51.5074, -0.1278);
var washington = new Coordinate(38.8951, -77.0364);

// Initialize the spherical geodesy utility
var geodesy = new LatLonSpherical();

// Calculate distance in kilometers
double distance = geodesy.Distance(london, washington);

Console.WriteLine($"Distance between London and DC: {distance:F2} km");
// Output: Distance between London and DC: 5908.38 km (approx)
```

## Next Steps

*   Explore the [API Reference](~/docs/api/Deodesy.Library.html) for advanced features.
