# OrderProcessing — Web API (Lab 4)
Acest proiect demonstrează utilizarea pattern-urilor **Chain of Responsibility** și **State** într-o aplicație .NET 8 Minimal API, cu un frontend SPA Vanilla JS.

## 🚀 Instrucțiuni de rulare
1. Deschideți soluția în Visual Studio 2026 sau rulați `dotnet run` din terminal în folderul `OrderProcessing.Api`.
2. UI-ul este accesibil direct pe adresa de bază generată (ex: `https://localhost:7251`).
3. Interfața Swagger pentru testare API este la `/swagger`.

## 📸 Screenshots
*(Am adaugat pozele doveditoare în repository pentru fluxul complet: creare, tranziții valide, erori State Pattern și erori Chain of Responsibility)*

---

## 🧩 Diagrame UML (Mermaid)

### 1. State Diagram (Order Lifecycle)
```mermaid
stateDiagram-v2
    [*] --> Pending
    Pending --> Confirmed : Pay()
    Pending --> Cancelled : Cancel()
    Confirmed --> Processing : Process()
    Confirmed --> Cancelled : Cancel()
    Processing --> Shipped : Ship()
    Processing --> Cancelled : Cancel()
    Shipped --> Delivered : Deliver()
    Delivered --> [*]
    Cancelled --> [*]

sequenceDiagram
    actor User
    participant API as Minimal API
    participant OS as OrderService
    participant CoR as ValidationChain
    participant Order as Order (State)

    User->>API: POST /orders
    API->>OS: CreateOrder(request)
    OS->>CoR: Handle(order)
    CoR-->>OS: ValidationResult.Success()
    OS-->>API: 201 Created (Pending)
    
    User->>API: POST /orders/{id}/pay
    API->>OS: PayOrder(id)
    OS->>Order: Pay()
    Order-->>OS: State -> Confirmed
    OS-->>API: 200 OK

classDiagram
    class Order {
        +OrderId Id
        +Customer Customer
        +IOrderState CurrentState
        +Pay()
        +Process()
        +Ship()
    }
    
    %% State Pattern
    class IOrderState {
        <<interface>>
        +Pay(Order)
        +Process(Order)
    }
    IOrderState <|-- PendingState
    IOrderState <|-- ConfirmedState
    Order *-- IOrderState
    
    %% Chain of Responsibility
    class IOrderValidationHandler {
        <<interface>>
        +SetNext(handler)
        +Handle(Order)
    }
    class BaseValidationHandler {
        <<abstract>>
    }
    IOrderValidationHandler <|-- BaseValidationHandler
    BaseValidationHandler <|-- StockValidationHandler
    BaseValidationHandler <|-- PriceValidationHandler