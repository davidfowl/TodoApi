## ASP.NET Core Minimal APIs

Samples with minimal APIs from the ASP.NET community stand up. These samples require a newer compiler that is currently being manually built from [dotnet/roslyn](https://github.com/dotnet/roslyn)

To build the main branch relying on the natural type for lambdas, you can checkout the features/compiler roslyn branch and set "Roslyn.VisualStudio.Setup" as your startup project to launch VS. Then you need to add something like <CscToolPath>C:\dev\dotnet\roslyn\artifacts\bin\csc\Debug\net472</CscToolPath> to the csproj.
