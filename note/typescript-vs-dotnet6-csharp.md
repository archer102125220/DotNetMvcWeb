# TypeScript vs .NET 6 (C# 10) 語法差異筆記

這份筆記專門幫助熟悉 TypeScript 的開發者，在面臨或維護 **.NET 6 (C# 10)** 專案時，能夠快速將既有的前端/Node.js 概念轉換為對應的 C# 語法與開發慣例。

.NET 6 是微軟一個極為普及的長期支援 (LTS) 版本，許多企業專案仍在使用。相比於最新的 .NET 10，.NET 6 (C# 10) 已經具備了許多現代化的語法糖（如 File-scoped namespaces、Global usings、Records），但仍有一些與 TS 不同的限制與設計哲學。

---

## 1. 變數宣告與基礎型別 (Variables & Types)

TypeScript 主要使用 `let` 和 `const`，並高度依賴型別推斷。在 C# 10 中，雖然有 `var` 關鍵字，但開發團隊通常有各自的規範（如本專案要求盡量明確宣告型別）。

| 功能 | TypeScript | .NET 6 (C# 10) |
| :--- | :--- | :--- |
| **字串** | `let name: string = "Alice";` | `string name = "Alice";` |
| **數字** | `let age: number = 25;` | `int age = 25;` (也有 `double`, `decimal` 等) |
| **布林** | `let isActive: boolean = true;` | `bool isActive = true;` |
| **常數** | `const max: number = 100;` | `const int max = 100;` |
| **自動推斷** | `let list = [1, 2, 3];` | `var list = new List<int> { 1, 2, 3 };` |

💡 **重要差異**：TypeScript 的 `number` 涵蓋了整數與浮點數。在 C# 中，與金額相關的計算強烈建議使用 `decimal` 以避免浮點數誤差；整數使用 `int` 或 `long`；一般浮點數使用 `double`。

---

## 2. 陣列與資料轉換 (Array Methods vs LINQ)

TypeScript 開發者非常習慣使用 Array prototype methods (`map`, `filter`, `reduce`)。在 .NET 6 中，這對應到 **LINQ (Language Integrated Query)**。

必須在檔案頂端加上 `using System.Linq;`（如果在 .NET 6 專案中有啟用 Global usings，這通常會自動包含）。

| TypeScript Array Methods | C# 10 (LINQ) |
| :--- | :--- |
| `array.map(x => x.name)` | `list.Select(x => x.Name)` |
| `array.filter(x => x.age > 18)` | `list.Where(x => x.Age > 18)` |
| `array.find(x => x.id === 1)` | `list.FirstOrDefault(x => x.Id == 1)` |
| `array.some(x => x.isActive)` | `list.Any(x => x.IsActive)` |
| `array.every(x => x.isActive)` | `list.All(x => x.IsActive)` |
| `array.reduce(...)` | `list.Aggregate(...)` |
| `array.sort(...)` | `list.OrderBy(x => x.Prop).ThenBy(...)` |

⚠️ **注意**：LINQ 是「延遲執行 (Deferred Execution)」，直到你呼叫 `.ToList()` 或 `.ToArray()`，或者在 `foreach` 中迭代它時，它才會真正執行運算或發送 DB 查詢。

---

## 3. 類別、介面與 DTO (Classes, Interfaces & Records)

### 資料傳遞物件 (DTO)
在 TS 中，傳遞資料通常只寫一個 `type` 或 `interface`。在 .NET 6 中，**強烈建議使用 `record`**（C# 9 引入）來處理不可變 (Immutable) 的資料傳遞。C# 10 更進一步加入了 `record struct`。

- **TS**: 
  ```typescript
  type UserDto = { id: number; name: string; };
  ```
- **.NET 6 (C# 10)**:
  ```csharp
  // 簡潔的 Positional Record
  public record UserDto(int Id, string Name);
  ```

### 介面 (Interfaces)
- **TS**: 介面常被用作純結構的定義。
- **.NET 6**: 介面主要用於定義「合約」並結合依賴注入 (Dependency Injection)。
  ```csharp
  public interface IUserService 
  {
      Task<UserDto> GetUserAsync(int id);
  }
  
  public class UserService : IUserService 
  {
      public async Task<UserDto> GetUserAsync(int id) { ... }
  }
  ```

---

## 4. Nullable 參考型別與空值檢查 (Nullability)

.NET 6 的新專案預設會開啟 `<Nullable>enable</Nullable>`，這與 TypeScript 的 `strictNullChecks: true` 非常相似。

### 宣告與可空運算子
- **TS**: `let name: string | null = null;`
- **.NET 6**: `string? name = null;`

### 空值聯合 (Null Coalescing)
- **TS**: `let result = name ?? "Default";`
- **.NET 6**: `var result = name ?? "Default";`

### Guard Clauses (防衛語句)
.NET 6 引入了非常方便的 Throw helper，大幅簡化了函式開頭的 Null 檢查。
- **TS**:
  ```typescript
  if (!user) throw new Error("user cannot be null");
  ```
- **.NET 6 (C# 10)**:
  ```csharp
  ArgumentNullException.ThrowIfNull(user);
  ```

---

## 5. 模組系統：Imports vs Usings

- **TS** 使用相對或絕對路徑來引入檔案：
  ```typescript
  import { User } from '../models/User';
  ```
- **.NET 6 (C# 10)** 採用了兩個重要的新特性：**Global Usings** 與 **File-scoped Namespaces**。
  
  檔案結構通常如下：
  ```csharp
  // 透過 Namespace 來組織，不需要知道檔案具體在哪個路徑
  using DotNetMvcWeb.Models; 
  
  // C# 10 特性：File-scoped namespace (省去了一層大括號縮排)
  namespace DotNetMvcWeb.Services;
  
  public class MyService { }
  ```
  在 .NET 6 中，你可以把常用的 `using` 定義在一個檔案中，並加上 `global` 關鍵字（如 `global using System.Linq;`），這樣整個專案都不用再重複寫這些 using 了。

---

## 6. 非同步程式設計 (Async / Await)

TS 的 Promise 與 C# 的 Task 概念幾乎一致。
- **TS**: `async function getData(): Promise<string> { return "data"; }`
- **.NET 6**: `public async Task<string> GetDataAsync() { return "data"; }`

**重要觀念映射**：
- `Promise.all([p1, p2])` ➡️ `await Task.WhenAll(t1, t2);`
- `Promise.race([p1, p2])` ➡️ `await Task.WhenAny(t1, t2);`
- `setTimeout()` ➡️ `await Task.Delay(1000);`

---

## 7. 匿名函式 (Lambda) 的推斷 (C# 10 新特性)

在 TypeScript 中，箭頭函式的型別推斷非常直覺。在 .NET 6 (C# 10) 之前，C# 對於 `var` 接 Lambda 的型別推斷有許多限制（必須明確指定 `Func` 或 `Action`）。但在 C# 10 中，這點被大幅改善了。

- **TS**:
  ```typescript
  const add = (a: number, b: number) => a + b;
  ```
- **C# 10**:
  ```csharp
  // C# 10 允許對 Lambda 進行自然型別推斷 (Natural Type Inference)
  var add = (int a, int b) => a + b; 
  ```

---

## 總結

從 TypeScript 轉移到 .NET 6 (C# 10) 開發時，最大的學習曲線通常在於：
1. **名義型別 vs 結構型別**：C# 型別必須明確宣告且繼承關係是固定的。
2. **LINQ 的強大與延遲執行**：熟悉 `Select`, `Where` 等用法將會是你寫 C# 最核心的技能。
3. **DI（依賴注入）是內建且無所不在的**：習慣將服務寫成 `Interface` 並透過建構子注入。
4. **專案結構與 Namespaces**：捨棄檔案路徑 import 的思維，改用 Namespace 來組織邏輯。
