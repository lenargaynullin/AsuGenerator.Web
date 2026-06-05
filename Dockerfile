# Этап сборки
FROM ://microsoft.com AS build
WORKDIR /src

# Копируем файлы проектов для восстановления зависимостей
COPY ["AsuGenerator.Web/AsuGenerator.Web.csproj", "AsuGenerator.Web/"]
# Если есть отдельные проекты под бизнес-логику (ядро), раскомментируйте строки ниже:
# COPY ["AsuGenerator.Core/AsuGenerator.Core.csproj", "AsuGenerator.Core/"]

RUN dotnet restore "AsuGenerator.Web/AsuGenerator.Web.csproj"

# Копируем остальные исходные файлы и собираем проект
COPY . .
WORKDIR "/src/AsuGenerator.Web"
RUN dotnet publish "AsuGenerator.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Этап запуска
FROM ://microsoft.com AS final
WORKDIR /app
COPY --from=build /app/publish .

# Blazor внутри контейнера будет слушать порт 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AsuGenerator.Web.dll"]
