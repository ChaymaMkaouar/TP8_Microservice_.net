# Tests avec ARC (Advanced REST Client) - via l'API Gateway
Base : `https://localhost:7162`  |  Header pour POST/PUT : `Content-Type: application/json`
(Dans ARC : désactiver la vérification SSL si le certificat de dev n'est pas approuvé.)

## A. Pré-tests
| # | Action | Résultat attendu |
|---|--------|------------------|
| A1 | Ouvrir http://localhost:15672 (guest/guest) > Queues | Après démarrage d'OrdersAPI : file `event-listener` présente |
| A2 | Ouvrir https://localhost:7066/swagger et https://localhost:7065/swagger | Pages Swagger affichées |
| A3 | Docker Desktop | Conteneur `ecomm` en Running |

## B. Produits (ProductsAPI via Gateway)
| # | Méthode | URL | Body | Attendu |
|---|---------|-----|------|---------|
| B1 | GET  | /apigateway/Products | - | 200 `[]` (ou liste) |
| B2 | POST | /apigateway/Products | `{"name":"prod1","stockQte":50,"price":1000}` | 201 + `productId` (ex. 1) |
| B3 | POST | /apigateway/Products | `{"name":"prod2","stockQte":40,"price":1520}` | 201 + `productId` 2 |
| B4 | GET  | /apigateway/Products/1 | - | 200 prod1 |
| B5 | PUT  | /apigateway/Products/1 | `{"productId":1,"name":"prod1-maj","stockQte":45,"price":1100}` | 204 |
| B6 | GET  | /apigateway/Products/1 | - | 200 name = prod1-maj |
| B7 | PUT  | /apigateway/Products/1 | `{"productId":2,"name":"x","stockQte":1,"price":1}` | 400 (id différent) |
| B8 | GET  | /apigateway/Products/999 | - | 404 |

## C. Synchronisation RabbitMQ (le coeur de l'atelier)
| # | Méthode | URL | Attendu |
|---|---------|-----|---------|
| C1 | GET | /apigateway/Orders/products | 200 : contient `{id:1,name:"prod1"}` et `{id:2,name:"prod2"}` (Ids identiques à ProductsAPI) |
| C2 | RabbitMQ UI > Queues > event-listener | Messages publiés/acquittés > 0, "Ready" = 0 |
| C3 | SSMS/VS : table `Products` (OrdersAPIDb) | mêmes Id/Name que `Product` (ProductsAPIDb) |
| C4 | Note : PUT/DELETE produit ne sont PAS propagés (seul `ProductCreated` existe) | normal |

## D. Commandes (OrdersAPI via Gateway)
| # | Méthode | URL | Body | Attendu |
|---|---------|-----|------|---------|
| D1 | GET  | /apigateway/Orders | - | 200 `[]` |
| D2 | POST | /apigateway/Orders | `{"customerName":"Ali","productId":1}` | 201, `orderDate` = date du jour (≠ 0001-01-01) |
| D3 | POST | /apigateway/Orders | `{"customerName":"Sana","productId":2,"orderDate":"2026-11-20T10:00:00"}` | 201 |
| D4 | POST | /apigateway/Orders | `{"customerName":"Test","productId":9999}` | 400 "Produit 9999 inconnu..." |
| D5 | GET  | /apigateway/Orders/1 | - | 200 |
| D6 | PUT  | /apigateway/Orders/1 | `{"ordersId":1,"orderDate":"2026-11-21T09:00:00","customerName":"Ali B.","productId":1}` | 204 |
| D7 | GET  | /apigateway/Orders/1 | - | 200 customerName = Ali B. |
| D8 | DELETE | /apigateway/Orders/2 | - | 204 |
| D9 | GET  | /apigateway/Orders/2 | - | 404 |

## E. Gateway (Ocelot)
| # | Test | Attendu |
|---|------|---------|
| E1 | GET https://localhost:7162/apigateway/Products vs GET https://localhost:7066/api/Products | Même JSON |
| E2 | POST /apigateway/Products/1 (méthode non déclarée) | 404 (aucune route) |
| E3 | GET /apigateway/Inconnu | 404 |
| E4 | Arrêter ProductsAPI puis GET /apigateway/Products | 503/500 (service indisponible), /apigateway/Orders reste 200 (résilience) |

## F. Résilience RabbitMQ
| # | Étapes | Attendu |
|---|--------|---------|
| F1 | Arrêter OrdersAPI > POST /apigateway/Products (prod3) | 201 ; RabbitMQ : `event-listener` Ready = 1 |
| F2 | Redémarrer OrdersAPI | Ready = 0 ; GET /apigateway/Orders/products contient prod3 |
| F3 | Arrêter le conteneur RabbitMQ > POST /apigateway/Products | Erreur 500 / timeout (le broker doit tourner avant le POST) |

## G. Jeu de données rapide (tableau à saisir dans ARC)
| name | stockQte | price |
|------|----------|-------|
| prod1 | 50 | 1000 |
| prod2 | 40 | 1520 |
| prod3 | 30 | 1600 |
