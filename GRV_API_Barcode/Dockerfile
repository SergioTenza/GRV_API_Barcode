#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GRV_API_Barcode/GRV_API_Barcode.csproj", "GRV_API_Barcode/"]
RUN dotnet restore "GRV_API_Barcode/GRV_API_Barcode.csproj"
COPY . .
WORKDIR "/src/GRV_API_Barcode"
RUN dotnet build "GRV_API_Barcode.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GRV_API_Barcode.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN apt-get update && \
    apt-get install -y fontconfig libc6-dev libgdiplus
#RUN apt-get update
#RUN DEBIAN_FRONTEND='noninteractive' apt-get install -y --no-install-recommends libc6-dev libgdiplus fontconfig
#RUN mkdir -p /usr/share/fonts/truetype/
#COPY --chown=104:106 fonts /usr/share/fonts/truetype/
#RUN install -m644 *.ttf /usr/share/fonts/truetype/
#RUN rm ./*.ttf
#RUN fc-cache -f -v
#RUN fc-list
ENTRYPOINT ["dotnet", "GRV_API_Barcode.dll"]

