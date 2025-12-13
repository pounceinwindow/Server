# Server
```docker-compose up --build```

__Аутентификация__ http://localhost:1234/auth

**Туры** http://localhost:1234/tours

<данные для админки:> 
 ```
 admin@test.com
 ```
 
 ```
 admin
```

Все настроено для локального запуска если же вы имеете всеобъемлющее желание поднять Docker 
поменяйте конфиг на 
{
  "StaticDirectoryPath": "static",
  "Domain": "http://+",
  "Port": "1234",
  "ConnectionString": "Host=db;Port=5432;Database=oris;Username=developer;Password=developer"
}

Естественно поменяйте под данные своей бд

[Выполните этот скрипт в своей бд для запуска локально](db/init.sql)
