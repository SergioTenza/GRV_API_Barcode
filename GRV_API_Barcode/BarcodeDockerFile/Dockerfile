FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY publish/. /app/publish 
RUN apt-get update && \
    apt-get install -y fontconfig libc6-dev libgdiplus
EXPOSE 80
ENTRYPOINT ["dotnet", "/app/publish/GRV_API_Barcode.dll"]

