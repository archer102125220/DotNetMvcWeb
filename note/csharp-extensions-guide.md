# C# 擴充方法 (Extension Methods) 教學與使用指南

這篇筆記是專門為剛接觸 .NET 或是從前端 (TypeScript/JavaScript) 轉入的開發者所準備，幫助你快速理解專案中經常出現的 `Extensions` 到底是什麼，以及如何正確地使用與撰寫。

## 什麼是擴充方法 (Extension Methods)？

在 C# 中，**擴充方法** 讓你可以向「現有的型別（例如 `string`, `int`, `DateTime` 或是其他的類別/介面）」**添加新的方法**，而不需要去修改原始的程式碼、不需要繼承它，也不需要重新編譯它。

這是一種非常強大的「語法糖」。

### 與 JavaScript/TypeScript 的概念對應

如果你熟悉前端開發，你可能知道要對原生型別（如 `String` 或 `Array`）加上自定義方法，通常會修改 `prototype`：

```typescript
// 在 TypeScript / JavaScript 中的寫法 (修改 Prototype)
String.prototype.truncate = function(maxLength: number) {
    if (this.length <= maxLength) return this.toString();
    return this.substring(0, maxLength) + '...';
};

// 呼叫
const short = "hello world".truncate(5);
```

但在強型別且嚴謹的 C# 中，**直接修改底層的原型 (Prototype) 是被禁止的**。為了達到相同的目的，微軟發明了「擴充方法」。

## 如何撰寫一個擴充方法？

在專案中，你可以查看 `DotNetMvcWeb/Extensions/StringExtensions.cs` 的實際範例。

要在 C# 中建立一個擴充方法，你必須嚴格遵守以下 **三個條件**：

1. **所在的類別必須是 `static` (靜態類別)**
2. **該方法必須是 `static` (靜態方法)**
3. **該方法的第一個參數前面，必須加上 `this` 關鍵字，並指定你要擴充的型別**

### 實際範例程式碼

```csharp
using System;

namespace DotNetMvcWeb.Extensions
{
    // 1. 類別必須是靜態的
    public static class StringExtensions
    {
        // 2. 方法必須是靜態的
        // 3. 第一個參數必須加上 `this`，代表你要擴充的是 `string`
        public static string Truncate(this string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) 
            {
                return string.Empty;
            }

            return value.Length <= maxLength ? value : $"{value.Substring(0, maxLength)}...";
        }
    }
}
```

## 如何使用擴充方法？

擴充方法最棒的地方在於，一旦你匯入了它的命名空間 (`using`)，你就可以**像呼叫原生方法一樣呼叫它**。

### 1. 在 Controller (C# 程式碼) 中使用

```csharp
using DotNetMvcWeb.Extensions; // 👈 記得引用命名空間

public class HomeController : Controller
{
    public IActionResult Index()
    {
        string title = "這是一段非常非常長的商品標題";
        
        // 就像原生方法一樣直接呼叫！編譯器會自動把 title 傳入給 value 參數
        string shortTitle = title.Truncate(10); 
        
        return View();
    }
}
```

### 2. 在 Razor 視圖 (.cshtml) 中使用

如果你在 View 檔案（或是全域的 `_ViewImports.cshtml`）中引入了該命名空間，你在 HTML 中也可以直接呼叫，這對於前端渲染非常方便，不需要再寫 Helper Class。

```cshtml
@using DotNetMvcWeb.Extensions

<!-- 畫面渲染時直接截斷 -->
<p>@Model.ArticleTitle.Truncate(20)</p>
```

## 常見的使用情境

在 .NET 專案中，擴充方法無所不在，以下是幾個你一定會碰到的情境：

1. **基本型別處理**：字串處理（截斷、隱藏個資）、日期格式化（轉換為台灣時區 `DateTime.ToTaiwanTime()`）、數字格式化等。
2. **依賴注入 (DI) 的封裝**：你在 `Program.cs` 看到的 `builder.Services.AddControllersWithViews()` 就是微軟寫的擴充方法，用來把一大包註冊邏輯隱藏起來，保持 `Program.cs` 乾淨。
3. **Entity Framework Core (LINQ)**：幫資料庫查詢寫擴充，例如 `query.WhereIsActive()` 來自動過濾掉被軟刪除的資料。
