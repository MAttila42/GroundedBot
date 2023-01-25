FROM mcr.microsoft.com/dotnet/sdk:6.0
COPY . .
WORKDIR /
RUN dotnet build ./src/GroundedBot.csproj -o ./build/ -c Release
CMD ["dotnet", "./build/GroundedBot.dll"]
