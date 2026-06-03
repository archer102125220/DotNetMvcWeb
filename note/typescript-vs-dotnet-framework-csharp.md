# TypeScript vs .NET Framework (C# 7.3) 語法差異筆記

這份筆記是寫給熟悉現代前端生態（TypeScript / Node.js）的開發者。當你需要接手維護古老的 **.NET Framework 4.x** (最高支援至 C# 7.3) 專案時，你將會面臨巨大的「語法降級」衝擊。

在這個時空背景下，許多你在 TypeScript 甚至現代 C# (C# 10+) 覺得理所當然的語法糖與安全機制都**不存在**。以下是關鍵的差異與適應指南。

---

## 1. 消失的 Null 安全機制 (No Nullable Reference Types)

在 TypeScript 中，我們習慣開啟 `strictNullChecks`；在現代 C# 中，我們有 `<Nullable>enable</Nullable>`。但在 C# 7.3，**所有的參考型別預設都是可以為 Null 的**，而且編譯器**不會**給你任何警告。

- **TS**: `let name: string | null = null;` (編譯器嚴格檢查)
- **.NET Framework**: `string name = null;` (編譯器不會警告，執行時拋出 `NullReferenceException`)

💡 **適應心法**：你必須養成在所有方法開頭，手動檢查傳入參數是否為 Null 的習慣（也就是寫一大堆防衛語句）。

```csharp
// 在 C# 7.3，你必須這樣寫
public void ProcessUser(User user) 
{
    if (user == null) 
    {
        throw new ArgumentNullException(nameof(user));
    }
    // ...
}
```

---

## 2. 極度囉嗦的資料模型 (No Records & No Target-typed `new`)

TypeScript 寫一個資料結構只要一行 `type`。現代 C# 可以用 `record`。但在 .NET Framework，你必須手寫冗長的 Class 與屬性。

- **TS**:
  ```typescript
  type UserDto = { id: number; name: string; };
  const user = { id: 1, name: "Alice" };
  ```

- **.NET Framework (C# 7.3)**:
  沒有 `record`，也沒有簡化的 `new()`，你必須老老實實地寫完整：
  ```csharp
  public class UserDto
  {
      public int Id { get; set; }
      public string Name { get; set; }
  }
  
  // 必須寫出完整的型別名稱
  UserDto user = new UserDto { Id = 1, Name = "Alice" };
  ```

---

## 3. 缺乏現代模式比對 (Limited Pattern Matching)

TypeScript 有強大的 Discriminated Unions 與 `typeof`/`instanceof` 搭配自動型別推斷。C# 7.3 雖然有基本的 `is` 型別比對，但**沒有 `switch` 表達式**。

- **TS**:
  ```typescript
  const statusMessage = statusCode === 200 ? "Success" : "Error";
  ```

- **.NET Framework (C# 7.3)**:
  不能使用現代 C# 簡潔的 `switch { }` 表達式，只能用傳統的 `switch-case` 或是長串的 `if-else`：
  ```csharp
  string statusMessage;
  switch (statusCode)
  {
      case 200:
          statusMessage = "Success";
          break;
      case 404:
          statusMessage = "Not Found";
          break;
      default:
          statusMessage = "Unknown Error";
          break;
  }
  ```

---

## 4. 命名空間與 Using 的地獄 (Verbose Namespaces & Usings)

- **TS**: 檔案頂部的 `import` 是針對特定的變數或類別。
- **.NET Framework (C# 7.3)**:
  沒有 Global Usings，也沒有 File-scoped namespaces。這代表你的每一個 C# 檔案都會長這樣：

  ```csharp
  // 每一個檔案都要重複寫這些 using
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  // 必須有一層 namespace 大括號，導致所有的程式碼都要往內縮排一次
  namespace MyApp.Services 
  {
      public class MyService 
      {
          // ...
      }
  }
  ```

---

## 5. 非同步陷阱 (Async/Await Deadlocks)

雖然 C# 5 就引入了 `async/await`，但在 .NET Framework 的世界裡，許多舊專案仍然充斥著同步與非同步混用的可怕寫法。

- **TS**: 在 Node.js 中，幾乎所有 I/O 都是非同步，且 Event Loop 不會因為 Promise 被 Block。
- **.NET Framework**: 在 ASP.NET (非 Core) 專案中，如果你在 Controller 裡呼叫了 `.Result` 或 `.Wait()`，**極高的機率會造成 Deadlock (死鎖)**，因為它會阻擋當前的 Request Thread。

💡 **適應心法**：
1. 看到 `Task`，**一定要用 `await`**，絕對不要用 `.Result` 或是 `.Wait()`。
2. 許多舊函式庫可能只提供同步方法（如 `SqlConnection` 的舊寫法），在這種情況下你只能乖乖寫同步程式碼。

---

## 6. Lambda 推斷的限制

在 TypeScript 中，你可以隨意寫出箭頭函式並賦值給 `const`。但在 C# 7.3，你**不能**直接用 `var` 接 Lambda 函式。

- **TS**: 
  ```typescript
  const log = (msg: string) => console.log(msg);
  ```

- **.NET Framework (C# 7.3)**:
  編譯器無法推斷，必須明確指定 `Action` 或 `Func`：
  ```csharp
  // ❌ 編譯錯誤：Cannot assign lambda expression to an implicitly-typed variable
  // var log = (string msg) => Console.WriteLine(msg); 
  
  // ✅ 必須明確寫出委派型別
  Action<string> log = msg => Console.WriteLine(msg);
  Func<int, int, int> add = (a, b) => a + b;
  ```

---

## 總結

當一個寫 TypeScript 的開發者回到 .NET Framework 的專案時，你會感覺到程式碼變得**非常冗長**。
請拋棄「語法應該要很簡潔」的期望，準備好迎接手寫 Class、不斷重複的 `using`、傳統的 `switch-case`，以及最重要的：**隨時提防 `NullReferenceException` 拋出**的心理準備。
