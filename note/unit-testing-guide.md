# .NET MVC 單元測試與覆蓋率實戰教學指南 (Unit Testing Guide)

本指南介紹在 ASP.NET Core MVC / Web API 專案中建立專業級單元測試的核心觀念、測試三大維度（**正向測試**、**反向測試**、**邊界測試**）、Mock 與 In-Memory 資料庫的運用，以及如何結合 **Coverlet** 與 **ReportGenerator** 達成高達 85% 以上的行覆蓋率與分支覆蓋率。

---

## 目錄
1. [什麼是單元測試？核心原則 (3A 原則)](#1-什麼是單元測試核心原則-3a-原則)
2. [測試三大維度深度解析與實戰範例](#2-測試三大維度深度解析與實戰範例)
   - [A. 正向測試 (Positive / Happy Path Testing)](#a-正向測試-positive--happy-path-testing)
   - [B. 反向測試 (Negative / Failure Path Testing)](#b-反向測試-negative--failure-path-testing)
   - [C. 邊界測試 (Boundary / Edge Case Testing)](#c-邊界測試-boundary--edge-case-testing)
3. [.NET 測試工具箱與套件職責](#3-net-測試工具箱與套件職責)
4. [Mock 模擬 vs. In-Memory 資料庫的選擇](#4-mock-模擬-vs-in-memory-資料庫的選擇)
5. [覆蓋率指標：行覆蓋率 vs. 分支覆蓋率](#5-覆蓋率指標行覆蓋率-vs-分支覆蓋率)
6. [ReportGenerator 視覺化報表產出指南](#6-reportgenerator-視覺化報表產出指南)

---

## 1. 什麼是單元測試？核心原則 (3A 原則)

**單元測試 (Unit Test)** 是針對程式碼中「最小可測試單元」（通常是一個方法或一個類別）進行的自動化驗證。它的目的是：
- **及早發現 Bug**：在開發階段就找出邏輯漏洞。
- **重構的安全網**：當未來修改程式碼或升級框架時，只要測試全部通過，就能確信既有邏輯沒有被破壞。
- **活的規格文件**：測試案例清楚描述了該方法在各種情境下的預期行為。

### 測試標準結構：3A 原則
每個單元測試方法通常劃分為三個步驟：
1. **Arrange (安排/準備)**：準備測試環境、假資料、Mock 物件、初始化受測類別 (System Under Test, SUT)。
2. **Act (執行/行動)**：呼叫受測方法，取得執行結果或觀察例外。
3. **Assert (斷言/驗證)**：驗證回傳值是否符合預期、狀態是否正確改變、依賴方法是否被正確呼叫。

```csharp
[Fact]
public void StringExtensions_Truncate_WhenTextIsShort_ReturnsOriginalText()
{
    // 1. Arrange (準備)
    string input = "Hello";
    int maxLength = 10;

    // 2. Act (執行)
    string result = input.Truncate(maxLength);

    // 3. Assert (驗證)
    Assert.Equal("Hello", result);
}
```

---

## 2. 測試三大維度深度解析與實戰範例

要寫出高品質且高覆蓋率的測試，必須從以下三個維度全面思考測試案例：

```
                    ┌─────────────────────────┐
                    │      單元測試維度       │
                    └────────────┬────────────┘
         ┌───────────────────────┼───────────────────────┐
         ▼                       ▼                       ▼
 🟢 正向測試 (Positive)  🔴 反向測試 (Negative)  🟡 邊界測試 (Boundary)
   「一切照規矩來」        「故意給錯、製造故障」   「走在邊緣、臨界交界」
   - 合法參數輸入          - 傳入 null / 不合法資料  - 0 / 最大值 / 極限值
   - 預期成功流程          - 模擬資料庫斷線/併發衝突 - 空集合 [] / 空字串 ""
   - 200 OK / 201 Created  - 400 / 404 / 攔截例外    - 恰好等於門檻的交界點
```

---

### A. 正向測試 (Positive / Happy Path Testing)

> **定義**：在輸入「合法且符合預期」的資料時，驗證系統能正常運作並產出正確的輸出、正確的 HTTP 狀態碼或正確的資料庫狀態。

#### 核心焦點：
- 正常的業務流程 (Happy Path)。
- 驗證 Controller 回傳 `200 OK`、`201 CreatedAtAction`、`204 NoContent`、正確的 `ViewResult` 或 Partial View。
- 驗證 Service 正確將資料寫入並回傳排序後的結果。

#### 實戰範例：Controller 成功建立商品並回傳 201
```csharp
[Fact]
public async Task CreateProduct_WhenValid_ReturnsCreatedAtAction()
{
    // Arrange: 準備合法的輸入資料與 Mock Service
    Product newProduct = new() { Id = 10, Name = "機械鍵盤", Price = 3000 };
    var mockRepo = new Mock<IProductRepository>();
    mockRepo.Setup(r => r.AddAsync(newProduct)).Returns(Task.CompletedTask);
    
    var controller = new ProductsController(mockRepo.Object);

    // Act: 執行建立動作
    var result = await controller.CreateProduct(newProduct);

    // Assert: 驗證回傳狀態為 201 Created，且包含正確的路由與資料
    var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
    Assert.Equal(nameof(ProductsController.GetProductById), createdResult.ActionName);
    Assert.Equal(10, createdResult.RouteValues?["id"]);
    Assert.Same(newProduct, createdResult.Value);
}
```

---

### B. 反向測試 (Negative / Failure Path Testing)

> **定義**：故意輸入「無效、不存在、惡意或衝突」的資料，或者模擬底層依賴故障（如資料庫連線失敗、併發衝突），驗證系統是否具備足夠的**容錯力 (Fault Tolerance)** 與**防禦力**，能給出正確的錯誤回應（如 400 BadRequest、404 NotFound、409 Conflict），而不是發生未預期的崩潰 (Unhandled 500 Exception)。

#### 核心焦點：
- **Null 防禦**：建構子傳入 `null` 時是否拋出 `ArgumentNullException`。
- **查無資料**：查詢或刪除不存在的 ID 時是否回傳 `404 NotFound`。
- **資料衝突 / 驗證失敗**：路由 ID 與 Body ID 不相符時回傳 `400 BadRequest`；`ModelState.IsValid == false` 時重回表單。
- **安全防護**：測試開放重導向 (Open Redirect) 攻擊防禦，傳入惡意外部網址時應拋出例外或攔截。
- **併發衝突 (Concurrency)**：捕捉 EF Core `DbUpdateConcurrencyException`，判斷若資料已被他人刪除回傳 404，若仍存在則重新拋出例外。

#### 實戰範例 1：查詢不存在的項目回傳 404 NotFound
```csharp
[Fact]
public async Task GetItem_WhenNotFound_ReturnsNotFound()
{
    // Arrange: 模擬查詢 ID = 999 時回傳 null
    var mockService = new Mock<IMssqlDemoItemService>();
    mockService.Setup(s => s.GetItemByIdAsync(999, true)).ReturnsAsync((MssqlDemoItem?)null);

    var controller = new MssqlDemoApiController(mockService.Object);

    // Act
    var result = await controller.GetItem(999);

    // Assert: 確保不會回傳 200，而是正確的 404
    Assert.IsType<NotFoundObjectResult>(result.Result);
}
```

#### 實戰範例 2：安全性反向測試（防範惡意外部重導向）
```csharp
[Fact]
public void LocalRedirectDemo_WithExternalUrl_ThrowsInvalidOperationException()
{
    // Arrange: 攻擊者試圖誘導使用者跳轉至釣魚網站
    var controller = new RedirectDemoController();
    string maliciousUrl = "https://malicious-phishing-site.com";

    // Act & Assert: 驗證 LocalRedirect 會強制作安全檢查並拋出例外
    Assert.Throws<InvalidOperationException>(() => controller.LocalRedirectDemo(maliciousUrl));
}
```

---

### C. 邊界測試 (Boundary / Edge Case Testing)

> **定義**：測試資料落在**極限邊界值、門檻交界點、或是極端特殊狀態**下的運作情況。軟體開發中最容易隱藏 Bug 的地方往往就是「邊界」（例如 `>` 與 `>=` 的失誤、空集合、0、極大值）。

#### 核心焦點：
- **長度與數值極限**：長度剛好為 0、剛好等於上限、上限 - 1、上限 + 1。
- **字串邊界**：`null`、空字串 `""`、純空白字元 `"   \t\n "`、特殊 Unicode 與中文全形字元。
- **集合狀態**：空陣列 `[]`、只有 1 筆資料、大量資料。
- **時間與狀態交界**：跨日時區、DateTime 預設值 `default(DateTime)`、首次執行 vs. 重複執行（冪等性 Idempotency）。

#### 實戰範例 1：字串截斷擴充方法的全面邊界測試
```csharp
public class StringExtensionsBoundaryTests
{
    [Theory]
    [InlineData(null, 10, "")]              // 邊界 1: null 輸入應安全回傳空字串
    [InlineData("", 5, "")]                 // 邊界 2: 空字串輸入
    [InlineData("Hello", 0, "...")]         // 邊界 3: maxLength 為 0 的極限情況
    [InlineData("Hello", 5, "Hello")]       // 邊界 4: 字串長度剛好等於 maxLength (不應產生省略號)
    [InlineData("Hello", 4, "Hell...")]     // 邊界 5: 字串長度剛好超過 maxLength 1 個字元
    [InlineData("測試中文字串截斷", 4, "測試中文...")] // 邊界 6: Unicode 中文多位元組字元
    public void Truncate_BoundaryCases_WorkCorrectly(string? input, int maxLength, string expected)
    {
        string actual = input.Truncate(maxLength);
        Assert.Equal(expected, actual);
    }
}
```

#### 實戰範例 2：資料庫種子初始化（冪等性邊界：第一次 vs. 第二次）
```csharp
[Fact]
public void DbInitializer_WhenCalledTwice_DoesNotDuplicateData()
{
    // Arrange: 準備 InMemory DbContext
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase("SeederBoundaryTest")
        .Options;

    using (var context = new AppDbContext(options))
    {
        // Act 1: 第一次執行（正常注入種子資料）
        DbInitializer.Initialize(context);
        Assert.True(context.OracleDemoItems.Any());
    }

    using (var context = new AppDbContext(options))
    {
        int initialCount = context.OracleDemoItems.Count();

        // Act 2: 第二次執行（命中 Any() 邊界檢查，應提早 return）
        DbInitializer.Initialize(context);

        // Assert: 資料筆數完全不變，證明具備冪等性 (Idempotent)
        Assert.Equal(initialCount, context.OracleDemoItems.Count());
    }
}
```

---

## 3. .NET 測試工具箱與套件職責

在 `DotNetMvcWeb.Tests.csproj` 中，我們配置了 .NET 生態系中最標準且強大的單元測試組合：

| 套件名稱 | 用途與職責 | 為什麼需要它？ |
| :--- | :--- | :--- |
| **`xunit`** | 測試框架核心 | 提供 `[Fact]`、`[Theory]`、`[InlineData]` 與 `Assert.*` 斷言庫。 |
| **`xunit.runner.visualstudio`** | 測試執行器 | 讓 `dotnet test` 指令與 Visual Studio / VS Code 測試總管能自動探索與執行測試。 |
| **`Microsoft.NET.Test.Sdk`** | .NET 測試基礎建設 | 將專案宣告為可執行的 Test Module，負責進程通訊與結果回報。 |
| **`Moq`** | 介面模擬 (Mocking) 函式庫 | 讓你不需要真實依賴，就能自由偽造 Service、Repository、Logger 的回傳值並驗證呼叫次數。 |
| **`Microsoft.EntityFrameworkCore.InMemory`** | EF Core 記憶體模擬資料庫 | 提供輕量且快速的記憶體資料庫引擎，用來測試 DbContext 與 LINQ 操作，完全不需要啟動 Docker 或安裝真實 SQL Server。 |
| **`coverlet.msbuild` / `coverlet.collector`** | 程式碼覆蓋率收集器 | 在測試執行時進行位元組碼注入 (IL Instrumentation)，統計行覆蓋率 (Line) 與分支覆蓋率 (Branch)。 |

---

## 4. Mock 模擬 vs. In-Memory 資料庫的選擇

在撰寫單元測試時，經常面臨的決策是：**「何時該用 Moq？何時該用 InMemory Database？」**

```
                     ┌────────────────────────┐
                     │     受測對象是誰？     │
                     └───────────┬────────────┘
            ┌────────────────────┴────────────────────┐
            ▼                                         ▼
   【測試 Controller 層】                      【測試 Service / Data 層】
   👉 使用 Moq                               👉 使用 InMemory Database
   - 目的：驗證 HTTP 流程與路由                - 目的：驗證 LINQ 與資料庫邏輯
   - 隔離：將 Service 的行為偽造出來           - 真實：驗證 SaveChanges、Include、
   - 好處：速度極快、隨意製造例外              - 好處：不需要外部真實 DB 伺服器
```

### 為什麼 InMemory Database 無法執行 Raw SQL / Stored Procedure？
- `InMemoryDatabase` 只是在記憶體中維護 C# 物件集合，**它沒有真實的 SQL 解析引擎 (SQL Parser)**。
- 因此呼叫 `FromSqlInterpolated`、`ExecuteSqlRaw` 或使用 ADO.NET (`SqlConnection`) 時，InMemory 會有局限。
- **最佳測試策略**：
  1. Service 內的標準 LINQ 與 CRUD：用 InMemory 測試。
  2. Service 內的例外攔截與連線檢查：透過無連線字串或刻意給予假連線字串，測試連線失敗時的錯誤捕捉與例外包裝路徑。
  3. Controller 層：透過 `Mock<IService>` 模擬 Service 拋出例外或回傳資料，以 100% 覆蓋 Controller 的處理邏輯。

---

## 5. 覆蓋率指標：行覆蓋率 vs. 分支覆蓋率深度解析

在評估單元測試品質與完整性時，業界最常使用的兩大指標就是**行覆蓋率 (Line Coverage)** 與**分支覆蓋率 (Branch Coverage)**。兩者看似相似，但在實務上的嚴謹度有著巨大的差異。

---

### 📏 1. 行覆蓋率 (Line Coverage / Statement Coverage)

> **定義**：在測試執行期間，原始碼中有多少行（語句）被「至少執行過一次」。

#### 計算公式：
$$\text{行覆蓋率} = \frac{\text{被執行的有效行數}}{\text{總共可執行的有效行數}} \times 100\%$$

#### 盲點與局限：
行覆蓋率只能告訴你**「這行程式碼有沒有被摸過」**，但完全無法保證**「這行程式碼在所有邏輯條件下都正確」**。

---

### 🌿 2. 分支覆蓋率 (Branch Coverage / Decision Coverage)

> **定義**：程式碼中所有具備條件判斷的節點，其所有可能的決策路徑（`True` 與 `False` 分支）是否都被測試案例走過。

#### 計算公式：
$$\text{分支覆蓋率} = \frac{\text{已走過的分支路徑數 (True/False)}}{\text{所有條件判斷的總分支路徑數}} \times 100\%$$

#### 什麼語法會在 C# 中產生「分支」？
C# 編譯器在編譯成 IL 位元組碼時，以下語法都會產生多條分支：
1. **`if` 與 `if-else`**：每個 `if` 至少產生 2 個分支（條件為 True 進入區塊、條件為 False 跳過或進入 else）。
2. **三元運算子 `condition ? val1 : val2`**：2 個分支。
3. **空值合併運算子 `a ?? b`**：2 個分支（`a` 不為 null 則取 `a`；`a` 為 null 則取 `b`）。
4. **安全導航運算子 `obj?.Property`**：2 個分支（`obj` 有值 vs `obj` 為 null）。
5. **邏輯短路運算子 `&&` 與 `||`**：`if (A && B)` 實際上包含多個決策路徑（A 為 false 時直接短路 vs A 為 true 繼續判斷 B）。
6. **`switch` 語句與 `switch` 表達式**：每個 `case` 以及預設 `default` / `_` 各為一條分支。

---

### ⚔️ 3. 經典對照範例：為什麼 100% 行覆蓋率 ≠ 100% 分支覆蓋率？

來看以下這個常見的打折商業邏輯：

```csharp
public decimal CalculateDiscount(decimal price, bool isVip)
{
    decimal discount = 0;

    if (isVip)
    {
        discount = price * 0.2m; // VIP 打八折
    }

    return price - discount;
}
```

#### 情況 A：只寫了 1 個「VIP 會員」的正向測試案例
- **測試輸入**：`CalculateDiscount(1000, isVip: true)`
- **執行軌跡**：
  1. `decimal discount = 0;` (執行)
  2. `if (isVip)` 為 `true` (執行)
  3. `discount = price * 0.2m;` (執行)
  4. `return price - discount;` (執行)
- **統計結果**：
  - **行覆蓋率 = 100%**（每一行都被綠色覆蓋）
  - **分支覆蓋率 = 50%**（因為 `if (isVip)` 的 `False` 分支完全沒有被測試！）

```
                    ┌─────────────────────────┐
                    │      if (isVip)         │
                    └────────────┬────────────┘
                         True    │    False
             ┌───────────────────┴───────────────────┐
             ▼                                       ▼
    ┌─────────────────┐                     ┌─────────────────┐
    │ discount = 20%  │                     │  (完全沒測試過)  │
    └────────┬────────┘                     └────────┬────────┘
             │                                       │
             └───────────────────┬───────────────────┘
                                 ▼
                     ┌───────────────────────┐
                     │ return price-discount │
                     └───────────────────────┘
          【測試了 True，沒測試 False -> 分支覆蓋率僅 50%】
```

#### 情況 B：補上第 2 個「一般非 VIP 會員」的反向/邊界測試案例
- **新增測試輸入**：`CalculateDiscount(1000, isVip: false)`
- **統計結果**：
  - **行覆蓋率 = 100%**
  - **分支覆蓋率 = 100%**（True 與 False 分支全部被驗證！）

---

### 🎯 4. 指標對比總結表

| 比較項目 | 行覆蓋率 (Line Coverage) | 分支覆蓋率 (Branch Coverage) |
| :--- | :--- | :--- |
| **衡量核心** | 程式碼的**「行數」**是否被執行過 | 程式碼的**「邏輯路徑」**是否都被驗證過 |
| **嚴謹程度** | 較寬鬆（容易達成） | 極度嚴格（需補齊正向、反向、邊界與 null 分支） |
| **Bug 檢測力** | 容易遺漏未處理的 else 或 null 例外 | 能強迫開發者測試**「條件不成立」**時的 fallback 行為 |
| **專案要求** | **>= 85%**（本專案實測達 **89.2%** 🟢） | **>= 85%**（本專案實測達 **86.0%** 🟢） |

---

### 💡 5. 核心思維：要求「正向、反向、邊界測試」，本質上就是在要求「高分支覆蓋率」嗎？

> **答案是：是的，完全正確！這兩者在軟體工程上是「方法論」與「量化指標」的一體兩面。**

```
┌──────────────────────────────────────────────┐
│  測試設計方法 (How to Design Tests)          │
├──────────────────────────────────────────────┤
│ 🟢 正向測試 (Happy Path)                      │ ──► 滿足「條件成立 / 主幹流程 (True 分支)」
│ 🔴 反向測試 (Negative / Error Path)          │ ──► 滿足「條件不成立 / 例外攔截 (False/Catch 分支)」
│ 🟡 邊界測試 (Boundary / Edge Cases)          │ ──► 滿足「臨界交界點 (0, Max, Null, Empty 分支)」
└──────────────────────┬───────────────────────┘
                       │
                       ▼ 映射至量化指標 (Metric)
┌──────────────────────────────────────────────┐
│  🌿 分支覆蓋率 (Branch Coverage >= 85%)      │
└──────────────────────────────────────────────┘
```

#### 為什麼只要求「行覆蓋率」是不夠的？
1. **行覆蓋率容易「虛胖」**：
   - 開發者只要寫幾個最基本的正向測試（一路順暢到底的 Happy Path），很多時候行覆蓋率就能輕易衝破 80%~90%。
   - 但此時程式碼裡的 `else`、`catch`、參數防禦、查無資料時的 `404` 或 `ArgumentNullException` 等保護邏輯**完全沒被驗證過**。
2. **正向、反向、邊界測試是達成「高分支覆蓋率」的具體手段**：
   - 當團隊或主管要求：**「請務必補齊正向、反向與邊界測試」**，其在技術指標上的直接體現就是**「分支覆蓋率必須大幅提高」**。
   - 因為正向測試測了 True 分支、反向測試逼出 False/Catch 分支、邊界測試覆蓋了 Null/Empty/臨界值分支。

---

### 🛠️ 6. 如何在 .NET 提升分支覆蓋率？實務技巧

1. **為每個 `if` 寫齊 True 與 False 測試**：
   - 不僅要測 `if (item != null)` 的成功路徑，一定要測 `item == null` 是否正確拋出例外或回傳 404。
2. **測試 `string.IsNullOrWhiteSpace` 的所有可能**：
   - 傳入一般文字（正向）、傳入 `null`（邊界）、傳入 `""`（邊界）、傳入純空格 `"   "`（邊界）。
3. **測試空集合與有資料的集合**：
   - 測試 `list.Any()` 在集合有元素與空集合 `new List<T>()` 時的表現。
4. **排除自動生成的檔案**：
   - Razor Views 編譯產生的 `AspNetCoreGeneratedDocument` 或 EF Core Migration 檔案包含大量編譯器生成的輔助分支，應透過 Coverlet 的 `/p:Exclude` 參數排除，避免干擾真正的業務邏輯指標。

---

## 6. ReportGenerator 視覺化報表產出指南

透過 `ReportGenerator`，可以把 Coverlet 產出的原始 XML 轉換成美觀的 HTML 互動式儀表板。

在 .NET 生態系中，管理 CLI 工具有兩種標準方式：**本機工具清單 (Local Tool, `dotnet-tools.json`)** 與 **全域工具 (Global Tool, `-g`)**。

---

### 🌟 方案 A：使用專案本機工具 (Local Tool / `dotnet-tools.json` - 團隊推薦)

這是在企業團隊協作與 CI/CD 自動化建置中最推薦的現代標準做法（類似 Node.js 的 `package.json`）：
- **版本鎖定**：將工具版本記錄在專案目錄下的 `dotnet-tools.json`。
- **團隊一鍵還原**：其他成員 clone 專案後，只要執行 `dotnet tool restore` 即可自動安裝。
- **免設定 PATH**：安裝後可以直接使用 `dotnet reportgenerator` 呼叫，完全不受環境變數影響！

#### 1. 初始化與安裝（本專案已完成設定）：
```bash
# 建立工具清單 (若專案尚未建立過)
dotnet new tool-manifest

# 安裝 reportgenerator 為專案本機工具
dotnet tool install dotnet-reportgenerator-globaltool
```

#### 2. 日常執行與產出報表：
```bash
# 步驟 1: 還原本機工具 (首次使用或 CI/CD 環境需執行一次)
dotnet tool restore

# 步驟 2: 執行測試並收集覆蓋率
dotnet test DotNetMvcWeb.Tests/DotNetMvcWeb.Tests.csproj \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=cobertura \
  /p:CoverletOutput=./TestResults/ \
  /p:Exclude="[DotNetMvcWeb]AspNetCoreGeneratedDocument.*%2c[DotNetMvcWeb]Program%2c[DotNetMvcWeb]*.Migrations.*%2c[DotNetMvcWeb]Microsoft.AspNetCore.OpenApi.*%2c[DotNetMvcWeb]System.Runtime.CompilerServices.*"

# 步驟 3: 產出 HTML 網站報表 (直接使用 dotnet reportgenerator，免設 PATH)
dotnet reportgenerator \
  -reports:"DotNetMvcWeb.Tests/TestResults/coverage.cobertura.xml" \
  -targetdir:"DotNetMvcWeb.Tests/CoverageReport" \
  -reporttypes:"Html;TextSummary;Badges"

# 步驟 4: 在瀏覽器中開啟報表 (macOS)
open DotNetMvcWeb.Tests/CoverageReport/index.html
```

---

### 🌐 方案 B：使用全域工具 (Global Tool, `-g` - 個人電腦通用)

如果你希望在電腦上的任何目錄都能隨時使用該工具，不需要依賴專案目錄下的清單：

#### 1. 全域安裝工具 (僅需安裝一次)：
```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

> **⚠️ 全域工具注意事項 (macOS / Linux)**：  
> 全域工具會安裝在 `~/.dotnet/tools`。如果終端機出現 `zsh: command not found`，請將工具路徑加入 `~/.zshrc`：
> ```bash
> echo 'export PATH="$PATH:$HOME/.dotnet/tools"' >> ~/.zshrc
> source ~/.zshrc
> ```
> 或直接使用完整路徑呼叫：`~/.dotnet/tools/reportgenerator ...`。

#### 2. 日常執行與產出報表：
```bash
# 步驟 1: 執行測試並收集覆蓋率
dotnet test DotNetMvcWeb.Tests/DotNetMvcWeb.Tests.csproj \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=cobertura \
  /p:CoverletOutput=./TestResults/ \
  /p:Exclude="[DotNetMvcWeb]AspNetCoreGeneratedDocument.*%2c[DotNetMvcWeb]Program%2c[DotNetMvcWeb]*.Migrations.*%2c[DotNetMvcWeb]Microsoft.AspNetCore.OpenApi.*%2c[DotNetMvcWeb]System.Runtime.CompilerServices.*"

# 步驟 2: 產出 HTML 網站報表
reportgenerator \
  -reports:"DotNetMvcWeb.Tests/TestResults/coverage.cobertura.xml" \
  -targetdir:"DotNetMvcWeb.Tests/CoverageReport" \
  -reporttypes:"Html;TextSummary;Badges"

# 步驟 3: 在瀏覽器中開啟報表 (macOS)
open DotNetMvcWeb.Tests/CoverageReport/index.html
```

---

開啟 `index.html` 報表後，即可逐行檢視每一支 C# 檔案中被綠色標記（已覆蓋）與紅色標記（未覆蓋）的程式碼與分支狀態！


