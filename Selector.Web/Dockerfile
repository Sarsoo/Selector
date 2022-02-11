FROM mcr.microsoft.com/dotnet/aspnet:6.0
EXPOSE 80
RUN dotnet publish Selector.Web.csproj -c Release
COPY bin/Release/net6.0/publish/ App/
WORKDIR /App
ENV DOTNET_EnableDiagnostics=0
ENTRYPOINT ["dotnet", "Selector.Web.dll"]
