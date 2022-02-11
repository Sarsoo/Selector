FROM mcr.microsoft.com/dotnet/runtime:6.0
RUN dotnet publish Selector.CLI.csproj -c Release
COPY bin/Release/net6.0/publish/ App/
WORKDIR /App
ENV DOTNET_EnableDiagnostics=0
ENTRYPOINT ["dotnet", "Selector.CLI.dll"]

