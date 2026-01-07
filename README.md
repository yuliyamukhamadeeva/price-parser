# Price Parser (вариант: один магазин — santehnika-online.ru)
## Что делает проект
Это веб-приложение на ASP.NET Core (Razor Pages), которое позволяет:
•	добавлять товары (название и при желании SKU);
•	добавлять к товару ссылку на страницу товара в магазине;
•	парсить цену со страницы товара и сохранять результат в SQLite;
•	показывать минимальную цену и последние логи парсинга в интерфейсе.
В текущем варианте проекта используется один магазин: https://santehnika-online.ru.
## Требования
•	.NET SDK (например, 8.0);
•	доступ в интернет (для загрузки страниц и установки Playwright Chromium).
## Как запустить
1.	Перейдите в папку проекта (где находится PriceParser.Web).
2.	Восстановите зависимости.
3.	Запустите приложение.
cd PriceParser.Web

dotnet restore

dotnet run
После запуска откройте адрес из консоли (http://localhost:5241).
## База данных (SQLite)
Используется SQLite-файл app.db. Подключение задаётся в appsettings.json:
"ConnectionStrings": {
  "Default": "Data Source=app.db"
}
Миграции применяются автоматически при старте приложения.
Если вы изменили DbSeeder (например, список магазинов), но в интерфейсе ничего не поменялось, значит в базе уже есть записи. В таком случае:
•	остановите приложение;
•	удалите файл app.db в папке PriceParser.Web;
•	запустите приложение снова (dotnet run).
## Установка Playwright Chromium
Playwright используется для получения HTML через headless-браузер.
Вариант 1: через Playwright CLI
dotnet tool install --global Microsoft.Playwright.CLI

playwright install chromium
Вариант 2: через скрипт из сборки
dotnet build

pwsh ./PriceParser.Web/bin/Debug/net8.0/playwright.ps1 install chromium

## Eсли PowerShell недоступен
./PriceParser.Web/bin/Debug/net8.0/playwright.sh install chromium
Парсинг по таймеру
Фоновый парсинг включается в appsettings.json:
"Parsing": {
  "Enabled": true,
  "IntervalMinutes": 60
}
•	Enabled: true — включает фоновый запуск парсера;
•	IntervalMinutes — интервал запуска в минутах.
Если Enabled: false, парсинг доступен только вручную из интерфейса.
## Как пользоваться UI
4.	Откройте раздел «Товары».
5.	Нажмите «Добавить товар» и создайте товар (название, при желании SKU).
6.	Нажмите «Открыть» у нужного товара.
7.	В блоке «Добавить ссылку»: выберите магазин santehnika-online.ru, вставьте ссылку на товар вида https://santehnika-online.ru/... и нажмите «Сохранить».
8.	Нажмите «Спарсить этот товар».
9.	Проверьте результат: в блоке «Минимальная цена» появится цена и магазин, а ниже — записи в «Последние логи парсинга».
