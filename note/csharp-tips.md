# C# 實用技巧與語法筆記

這裡記錄了在開發 C# 與 .NET 專案時，常見的小知識與語法細節。

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
