# .NET 開發知識要點

這裡記錄了在開發 C# 與 ASP.NET Core 專案時，常見的觀念、小知識與語法細節。

## XML 註解與 `/// <inheritdoc />`

在 C# 中，我們經常使用以 `///` 開頭的 XML 註解來撰寫說明文件，這些註解會被編譯器收集，並讓 IDE (如 Visual Studio / VS Code) 能夠顯示 IntelliSense 提示。

### 什麼是 `<inheritdoc />`？

`/// <inheritdoc />` 是一個特殊的 XML 標籤，意思是 **「繼承 (Inherit) 父類別或介面 (Doc) 的註解」**。

當你實作介面或複寫 (override) 基礎類別的方法時，你不必重寫一次註解。只要加上這個標籤，IDE 就會自動去抓取原始介面或父類別上寫的說明。

```csharp
public class MyBaseClass
{
    /// <summary>
    /// 這是一個基礎方法的說明。
    /// </summary>
    public virtual void DoSomething() { }
}

public class MyChildClass : MyBaseClass
{
    /// <inheritdoc />
    public override void DoSomething() { } 
    // 當滑鼠移到這裡，IDE 會顯示「這是一個基礎方法的說明。」
}
```

### 替換或刪除會有影響嗎？

**完全不會有任何程式執行或功能上的影響。**

如果你覺得父類別預設的註解 (例如 EF Core 自動生成的 Migration 檔案中那些英文註解) 不夠清楚，你可以隨時把 `<inheritdoc />` 刪除，並換成你自己寫的 `<summary>`。

這唯一的改變就是：IDE 顯示的提示會變成你自訂的內容，反而有助於團隊開發或未來回顧程式碼。

## EF Core：重啟程式會自動建立資料表嗎？

許多新手在使用 Entity Framework Core 時會有個常見的誤解：「既然我在 `Program.cs` 裡面呼叫了 Seed Data (種子資料) 的初始化邏輯，如果資料庫或是資料表不存在，重新啟動程式 (`dotnet watch run`) 會自動幫我建立出來嗎？」

**答案是：預設情況下，不會。**

### 為什麼不自動建立？
ASP.NET Core 的設計理念在於安全與職責分離。資料庫結構（Schema）的變更是非常嚴肅且具風險的操作。如果應用程式每次重啟都會嘗試修改資料庫，在正式環境中可能會引發災難性的後果（例如不小心覆蓋了資料）。

### EF Core 建立資料表的正規兩步驟流程

如果您使用 **Code-First**（程式碼優先）來設計資料庫，建立資料表分為明確的兩階段，這兩步通常都必須透過命令列 (CLI) 手動執行：

1. **產生藍圖 (`dotnet ef migrations add <名稱>`)**
   這一步會讀取您的 C# 實體模型 (`Models`)，並翻譯成準備用來建立或修改資料庫結構的 C# 腳本檔案（位於 `Migrations` 資料夾內）。這只是在本地產生「設計圖」，**資料庫本身此時還沒有任何變化**。

2. **正式施工 (`dotnet ef database update`)**
   這一步會把剛才建立好的所有「設計圖」實際推送到資料庫伺服器（例如 MySQL、Oracle），執行真正的 SQL 語法（例如 `CREATE TABLE`）。**直到執行完這一步，資料表才會真正誕生**。

### 常見錯誤情境 (`Table doesn't exist`)
如果您只有執行了 `migrations add` 產生設計圖，接著就直接重啟應用程式 (`dotnet watch run`)，這時如果您的應用程式（例如在 `DbInitializer.cs` 裡）去對資料庫執行查詢或新增資料，資料庫就會無情地回報錯誤：

> `MySqlException: Table 'YourDatabase.YourTable' doesn't exist`

**解決方案**非常簡單：打開終端機，執行一次 `dotnet ef database update`（如果有多個 DbContext，記得加上 `-c YourDbContextName` 指定），把資料表建好之後，應用程式就能正常寫入資料了！
