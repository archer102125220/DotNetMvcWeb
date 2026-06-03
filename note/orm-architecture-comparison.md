# ORM 設計哲學比較：Code-First (EF Core) vs Migration-First

當我們從 Node.js (Sequelize) 或 PHP (Laravel Eloquent) 轉移到 .NET (EF Core) 時，最常感到的困惑就是：「為什麼 EF Core 會有 `.Designer.cs` 快照檔？為什麼它能自動產出 Migration 腳本？」

這其實牽涉到兩種完全不同的 ORM 設計哲學：**「Migration-First (手動建構派)」** 與 **「Code-First (狀態比對派)」**。

---

## 1. Sequelize / Laravel 派（Migration-First）

在 Sequelize 或 Laravel 的世界裡，**「Migration 檔案」本身才是重點**。

* **開發流程**：當想新增一個資料表或欄位時，必須親手下達指令建立一個空白的 Migration 檔，然後**手動寫入** `up` (新增) 與 `down` (復原) 的資料庫腳本（例如 Laravel 的 `$table->string('name')` 或是 Sequelize 的 `queryInterface.addColumn(...)`）。
* **框架的認知**：框架本身其實**不知道**資料庫「完整的長相」是什麼，它只是把手寫的腳本，按照時間順序一個個執行而已。
* **特色**：沒有所謂的「快照」，一切依賴開發者手動計算差異並編寫變更腳本。

---

## 2. EF Core 派（Code-First）

在 .NET 的 EF Core 裡，**「C# 的 Model 類別」才是唯一的真理 (Source of Truth)**。

* **開發流程**：完全不需要手寫資料庫異動腳本！只需要專心在 C# 裡寫好 `class`，加上對應的屬性 (Property) 與約束標記 (例如 `[Required]`)。
* **黑魔法的關鍵**：當下達 `dotnet ef migrations add` 時，EF Core 會把「**現在剛寫好的 C# class**」拿去跟「**上次存下來的 Snapshot (.Designer.cs)**」進行交叉比對 (Diff)。
* **自動產出**：它算出這兩者之間的差異後，**「自動」**寫出 `Up()` 與 `Down()` 的指令檔。

---

## 3. 為什麼 .NET 能做到全自動比對？

這要歸功於 C# 語言的特性：**「強型別 (Strongly Typed) 與反射 (Reflection)」**。

* 在 JavaScript (Node.js) 或 PHP 等動態語言中，Model 類別通常非常輕量，難以在程式碼中精確定義出完整的資料庫約束 (例如欄位長度、是否允許 Null 等)。
* 但在 C# 中，Model 的每個屬性都有極度明確的型別 (`int`, `string`, `DateTime`)，甚至可以透過 Data Annotations (`[MaxLength(200)]`) 加上詳細的約束限制。

因為 C# 的 Model 能夠承載**足夠詳細的資料庫結構資訊**，微軟才能把 EF Core 打造成一個「自動算 Diff、自動生腳本」的現代化 ORM 框架。
為了達成這個自動比對，EF Core 就必須偷偷在旁邊塞一個 `.Designer.cs` (模型快照)，當作它下次計算差異的參考小抄！

---

> 💡 **相關閱讀**：如果想了解 EF Core 的實務操作指令，以及如何避免 Migration 過程中的各種雷區，請參考 [Entity Framework Core (EF Core) 實戰教學指南](./ef-core-orm-guide.md)。
