# Start with the .NET SDK image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Set the working directory in the container
WORKDIR /app

# Copy the .csproj files and restore dependencies
COPY ./*.csproj .
COPY ./**/*.csproj ./

# Run dotnet restore for each .csproj file
RUN for file in $(ls *.csproj); do dotnet restore "$file"; done

# Copy the rest of the project files
COPY . .

# Publish the application
RUN dotnet publish -c Release -o out ConsumerSimulator.sln

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime

# Set the working directory in the container
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/out .

# Set the entry point for the container
ENTRYPOINT ["dotnet", "GraphQLSimulator.dll"]
