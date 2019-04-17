FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["ZEISSMachineStream/ZEISSMachineStream.csproj", "ZEISSMachineStream/"]
RUN dotnet restore "ZEISSMachineStream/ZEISSMachineStream.csproj"
COPY . .
WORKDIR "/src/ZEISSMachineStream"
RUN dotnet build "ZEISSMachineStream.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "ZEISSMachineStream.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ZEISSMachineStream.dll"]