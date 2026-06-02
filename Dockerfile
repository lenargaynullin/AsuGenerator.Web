# 1. ЭТАП СБОРКИ: Используем контейнер SDK .NET 8
FROM ://microsoft.com AS build-env
WORKDIR /app

# Копируем файл проекта и восстанавливаем NuGet-зависимости
COPY *.csproj ./
RUN dotnet restore

# Копируем всю остальную кодовую базу и компилируем высокопроизводительный b2b-релиз
COPY . ./
RUN dotnet publish -c Release -o out

# 2. ЭТАП ЗАПУСКА: Легкий контейнер среды выполнения (Runtime)
FROM ://microsoft.com
WORKDIR /app

# Настройка b2b-зависимостей: для генерации чертежей netDxf в Linux-контейнере
# принудительно ставим базовые шрифты (чтобы не ехал текст)
RUN apt-get update && apt-get install -y --no-install-recommends fontconfig ttf-mscorefonts-installer && rm -rf /var/lib/apt/lists/*

# Копируем скомпилированное приложение
COPY --from=build-env /app/out .

# Открываем порт 8080 для связи веб-приложения с интернетом
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# КРИТИЧЕСКИЙ ФИКС: Укажите точное имя вашей выходной DLL-сборки проекта
ENTRYPOINT ["dotnet", "AsuGenerator.Web.dll"]
