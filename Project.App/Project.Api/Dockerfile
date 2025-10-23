FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# set working directory
WORKDIR /MyApp

# install dependencies
# copy backend code
COPY ./ ./
# build (publish)
RUN dotnet restore .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS run

# set working directory
WORKDIR /MyApp

# copy built files from build
COPY --from=build /MyApp/out /MyApp
# according to richard this does nothing :/
EXPOSE 80

# run! hopefully.
ENTRYPOINT ["dotnet", "Project.Api.dll"]
