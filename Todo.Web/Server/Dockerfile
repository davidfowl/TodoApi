#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Todo.Web/Server/Todo.Web.Server.csproj", "Todo.Web/Server/"]
COPY ["Todo.Web/Client/Todo.Web.Client.csproj", "Todo.Web/Client/"]
COPY ["Todo.Web/Shared/Todo.Web.Shared.csproj", "Todo.Web/Shared/"]
RUN dotnet restore "Todo.Web/Server/Todo.Web.Server.csproj"
COPY . .
WORKDIR "/src/Todo.Web/Server"
RUN dotnet build "Todo.Web.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Todo.Web.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Todo.Web.Server.dll"]