# .NET 架構演進深度對比筆記：傳統三層式 vs 現代領域驅動 (DDD)

本文件透過本專案中兩組具體實作——**`OracleDemo` (傳統三層式 + 倉儲模式)** 與 **`Wallet` (現代領域驅動架構 + 倉儲模式)**，進行全方位架構對比，解析業務邏輯封裝、依賴關係、測試策略與實務選型依據。

---

## 總覽：兩種架構呼叫鏈對比

```mermaid
flowchart TD
    subgraph Traditional [傳統三層式架構 (OracleDemo 模組)]
        C1[Controller] --> S1[Service 商業邏輯層]
        S1 -->|操作貧血模型| E1[Models/Entities (純資料袋)]
        S1 --> R1[Repository 倉儲介面]
        R1 --> DB1[(Oracle 資料庫)]
    end

    subgraph DDD [領域驅動架構 (Wallet 模組)]
        C2[Controller] --> S2[Application Service 流程協調]
        S2 -->|觸發業務行為| E2[Domain Entity 富領域模型]
        S2 -->|依賴反轉| R2[Domain Repository 介面]
        R2 -.->|實作| R2_Imp[Repositories/Implements]
        R2_Imp --> DB2[(Oracle 資料庫)]
    end
```

---

## 核心維度深度對比

### 1. 實體模型對比：貧血模型 (Anemic) vs 富領域模型 (Rich Domain)

#### 模組 A：`OracleDemoItem.cs` (傳統貧血模型)
```csharp
// 只有公開的 get/set，本質是單純的資料庫欄位對照袋
public class OracleDemoItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int? CategoryId { get; set; }
}

// ⚠️ 問題：外部任何程式碼都可以隨意篡改物件狀態，產生非法數據：
var item = new OracleDemoItem();
item.Price = -99999; // 完全不會報錯！防禦邏輯只能散落在各個 Service 或 Controller 裡。
```

#### 模組 B：`Wallet.cs` (現代富領域模型)
```csharp
public class Wallet
{
    public Guid Id { get; private set; }
    public string OwnerId { get; private set; }
    public decimal Balance { get; private set; } // 私有 set，外部無法直接竄改

    // 業務行為內聚於實體內部，永遠捍衛自身業務不變性 (Invariants)
    public void Withdraw(decimal amount)
    {
        if (amount <= 0m)
            throw new InvalidAmountException(amount, "扣款");

        if (amount > Balance)
            throw new InsufficientBalanceException(Balance, amount);

        Balance -= amount;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

// ✅ 優勢：物件一出生就是合法的，且不合法的操作在編譯期/執行期當場被實體拒絕，外部不可能破壞錢包狀態！
```

---

### 2. 商業邏輯的位置：Service 胖 vs Service 瘦

| 比較項目 | 傳統三層式 (`OracleDemoItemService`) | 領域驅動架構 (`WalletService`) |
| :--- | :--- | :--- |
| **Service 的職責** | **又當編劇又當演員**：負責檢查長度、驗證欄位、防負數、算數、存檔。業務日漸複雜後，Service 容易膨脹成上千行的「上帝類別 (God Class)」。 | **只當調度員 (Coordinator)**：只負責「開啟交易 $\to$ 從 Repository 載入 Entity $\to$ 呼叫 `wallet.Withdraw(amount)` $\to$ 存檔」。 |
| **例外與錯誤處理** | 通常回傳 `bool` 或錯誤字串，邏輯容易漏判。 | 領域實體主動拋出語意明確的 `BusinessRuleException`，Service 捕捉後轉化為 View/API 的友好訊息。 |

---

### 3. 單元測試策略與 Mock 成本分析

這是區分兩種架構最大體感差異的地方：

#### 傳統 Service 的測試難題：
- 要測試「新增商品時名稱不可重複或價格合理」，必須寫好幾行 `new Mock<IOracleDemoItemRepository>()`，並 Setup 各種虛擬回傳。
- **測試成本高**：若 Repository 介面加了一個參數，十幾個 Service 測試全掛，維護成本沉重。

#### 領域模型的純粹測試 (Pure Unit Test)：
- 查看專案中的 [`WalletTests.cs`](file:///Users/parkerchen/Desktop/code/DotNetMvcWeb/DotNetMvcWeb.Tests/Domain/WalletTests.cs)：
```csharp
[Fact]
public void Withdraw_WithAmountExceedingBalance_ShouldThrowInsufficientBalanceException()
{
    // Arrange: 完全不需要 Mocking 套件！不需要 Mock DbContext 或 Repository！
    var wallet = new Wallet("user_001", 100m);

    // Act & Assert: 毫秒級純記憶體邏輯驗證
    Assert.Throws<InsufficientBalanceException>(() => wallet.Withdraw(150m));
}
```
- **測試優勢**：
  1. **零外部依賴**：11 項測試在 35ms 內執行完畢。
  2. **堅如磐石**：無論底層換成 Oracle、MySQL 還是 Mongo，業務規則測試完全不受影響。

---

### 4. 倉儲 (Repository) 介面到底該放哪裡？

您之前疑惑「為什麼 Domain 跟 Repositories 會同時存在？」關鍵在於 **依賴反轉原則 (Dependency Inversion Principle, DIP)**：

- **傳統作法**：
  - 介面與實作都在同一層：`Repositories/Interfaces/IOracleDemoItemRepository.cs` 與 `Repositories/Implements/OracleDemoItemRepository.cs`。
  - Service 依賴了 Repository 資料存取層。
- **現代 Domain 作法 (如本專案的 Wallet)**：
  - 介面宣告在 Domain：[`Domain/Wallets/Repositories/IWalletRepository.cs`](file:///Users/parkerchen/Desktop/code/DotNetMvcWeb/Domain/Wallets/Repositories/IWalletRepository.cs)。
  - 實作寫在外部：[`Repositories/Implements/WalletRepository.cs`](file:///Users/parkerchen/Desktop/code/DotNetMvcWeb/Repositories/Implements/WalletRepository.cs)。
  - **核心哲學**：Domain 是整個企業系統的核心大腦，它不依賴任何底層技術；反而是資料庫實作必須「滿足」Domain 開出的介面合約。

---

## 實務專案決策指引 (Decision Guide)

在未來的專案或公司開發中，如何評估何時使用哪種風格？

```text
┌─────────────────────────────────────────────────────────┐
│                    評估業務複雜度                        │
└─────────────────────────────────────────────────────────┘
                            │
            ┌───────────────┴───────────────┐
            ▼                               ▼
    【以資料庫 CRUD 為主】          【有明確商業規則與約束】
  (如：基本後台設定、字典清單)    (如：金流錢包、訂單狀態流轉、折價券)
            │                               │
            ▼                               ▼
     使用傳統三層式架構               採用領域驅動 (DDD) 架構
 (N-Tier: Controller+Service+Repo)  (Rich Domain + Repositories)
   開發快、檔案少、直觀上手        高內聚、好維護、測試成本極低
 (如本專案 OracleDemo 模組)         (如本專案 Wallet 模組)
```

---

## 專案現有實作索引

- **傳統三層式對照組**：
  - 控制器：[`Controllers/OracleDemoItemController.cs`](file:///Users/parkerchen/Desktop/code/DotNetMvcWeb/Controllers)
  - 服務層：[`Services/Implements/OracleDemoItemService.cs`](file:///Users/parkerchen/Desktop/code/DotNetMvcWeb/Services/Implements)
  - 倉儲層：[`Repositories/Implements/OracleDemoItemRepository.cs`](file:///Users/parkerchen/Desktop/code/DotNetMvcWeb/Repositories/Implements)
  - 貧血模型：[`Models/OracleDemoItem.cs`](file:///Users/parkerchen/Desktop/code/DotNetMvcWeb/Models)
- **領域驅動架構對照組**：
  - 領域聚合根：[`Domain/Wallets/Entities/Wallet.cs`](file:///Users/parkerchen/Desktop/code/DotNetMvcWeb/Domain/Wallets/Entities/Wallet.cs)
  - 領域介面：[`Domain/Wallets/Repositories/IWalletRepository.cs`](file:///Users/parkerchen/Desktop/code/DotNetMvcWeb/Domain/Wallets/Repositories/IWalletRepository.cs)
  - 倉儲實作：[`Repositories/Implements/WalletRepository.cs`](file:///Users/parkerchen/Desktop/code/DotNetMvcWeb/Repositories/Implements/WalletRepository.cs)
  - 應用服務：[`Services/Implements/WalletService.cs`](file:///Users/parkerchen/Desktop/code/DotNetMvcWeb/Services/Implements/WalletService.cs)
  - 表現層 (HTMX)：[`Views/Wallet/Index.cshtml`](file:///Users/parkerchen/Desktop/code/DotNetMvcWeb/Views/Wallet/Index.cshtml)
  - 純領域測試：[`DotNetMvcWeb.Tests/Domain/WalletTests.cs`](file:///Users/parkerchen/Desktop/code/DotNetMvcWeb/DotNetMvcWeb.Tests/Domain/WalletTests.cs)
