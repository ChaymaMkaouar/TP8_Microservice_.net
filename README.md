# MicroservicesApp (Atelier 8) - ProductsAPI / OrdersAPI / ApiGateway (Ocelot) / RabbitMQ + MassTransit

## 1. Prérequis
- .NET SDK 8, Visual Studio 2022 (ou VS Code), SQL Server LocalDB
- Docker Desktop
- Outil EF : `dotnet tool install --global dotnet-ef`
- Certificat HTTPS de dev : `dotnet dev-certs https --trust`

## 2. Ports
| Service      | URL HTTPS              |
|--------------|------------------------|
| ProductsAPI  | https://localhost:7066 |
| OrdersAPI    | https://localhost:7065 |
| ApiGateway   | https://localhost:7162 |
| RabbitMQ AMQP| localhost:4001 (-> 5672 du conteneur) |
| RabbitMQ UI  | http://localhost:15672 (guest / guest) |

## 3. RabbitMQ
    docker run -d --hostname my-rabbit --name ecomm -p 15672:15672 -p 4001:5672 rabbitmq:3-management
(le 1er port d'AMQP est mappé sur 4001 car le code utilise rabbitmq://localhost:4001)

## 4. Migrations (à faire UNE fois, dans cet ordre)
Depuis le dossier de la solution :

    dotnet ef migrations add InitialCreate --project ProductsAPI --startup-project ProductsAPI
    dotnet ef database update            --project ProductsAPI --startup-project ProductsAPI

    dotnet ef migrations add InitialCreate --project OrdersAPI --startup-project OrdersAPI
    dotnet ef database update            --project OrdersAPI --startup-project OrdersAPI

Équivalent Console du Gestionnaire de package (Visual Studio) - changer "Projet par défaut" AVANT chaque série :

    # Projet par défaut = ProductsAPI
    Add-Migration InitialCreate
    Update-Database
    # Projet par défaut = OrdersAPI
    Add-Migration InitialCreate
    Update-Database

Tables créées :
- ProductsAPIDb : `Product(ProductId PK identity, Name, StockQte, Price)`
- OrdersAPIDb  : `Orders(OrdersId PK identity, OrderDate, CustomerName, ProductId)` et
                 `Products(Id PK non identity, Name)`  <- remplie par RabbitMQ

Si vous avez déjà une base/migration cassée : `dotnet ef database drop` puis supprimer le dossier Migrations et refaire.

## 5. Lancement (ordre)
1. Docker : conteneur RabbitMQ démarré
2. ProductsAPI, OrdersAPI, ApiGateway (Visual Studio : Propriétés de solution > Démarrage multiple)
3. Tester avec ARC (voir TESTS.md)

## 6. Corrections par rapport au support de cours
- `Orders.OrderDate` avait `{ get; }` sans setter -> la date restait 0001-01-01. Corrigé (`get; set;`) + valeur par défaut UtcNow.
- `Product.Id` (OrdersAPI) était auto-généré et `Id = context.Message.Id` commenté -> Ids différents entre les 2 bases. Corrigé (`DatabaseGeneratedOption.None`) + consommateur idempotent.
- Ocelot.json : routes `{id}` en GET/POST -> remplacées par GET/PUT/DELETE ; POST seulement sur la collection.
- `app.UseOcelot().Wait()` -> `await app.UseOcelot()`, suppression d'UseAuthorization/MapControllers dans la gateway.
- Route `Orders/products` ajoutée pour vérifier la synchronisation RabbitMQ.
- Paramètres RabbitMQ déplacés dans appsettings.json.
