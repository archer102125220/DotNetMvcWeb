# ASP.NET Core MVC: Models 目錄開發指南與教學

本目錄（`Models`）負責存放應用程式中所有的資料結構、視圖模型 (ViewModels) 以及資料傳輸物件 (DTOs)。為了維持專案架構的清晰、安全與可維護性，請嚴格遵守以下分類與開發規範。

---

## 📂 目錄結構與職責

在 `Models` 資料夾下，我們應將不同用途的模型進行分類：

### 1. `Entities/` (實體模型)
- **用途**：代表資料庫的資料表結構，通常與 Entity Framework Core (EF Core) 搭配使用。
- **規則**：
  - **絕對不可以**直接將 Entity 傳遞給 Razor Views (`.cshtml`)，這會導致敏感資料（如密碼、雜湊值）外洩或引發 Lazy-Loading 的效能問題。
  - 僅在 `Data` (DbContext) 或 `Services` 層級中進行操作。

### 2. `ViewModels/` (視圖模型)
- **用途**：專門為 Razor Views (`.cshtml`) 設計的強型別資料結構。
- **規則**：
  - 每個 View 建議有自己專屬的 ViewModel（例如 `UserLoginViewModel`）。
  - View 中需要顯示什麼欄位，ViewModel 就只提供什麼欄位。
  - 可以在屬性上加入資料驗證標籤（Data Annotations）進行表單驗證。

### 3. `DTOs/` (資料傳輸物件)
- **用途**：用於前後端 API 呼叫、微服務通訊，或是 Service 層與 Controller 層之間的資料傳遞。
- **規則**：
  - 結構應保持扁平，僅包含傳輸所需的純資料。

---

## 🛡️ 核心開發規範

### 1. Controller-ViewModel 模式 (Thin Controllers)
- **Always use ViewModels**：Controller 的職責是從 Service 取得資料 (Entity / DTO)，將其轉換為 ViewModel，然後傳遞給 View。
- ❌ **錯誤示範**：直接回傳資料庫實體
  ```csharp
  public async Task<IActionResult> Profile() {
      User user = await _dbContext.Users.FindAsync(userId);
      return View(user); // ❌ 危險！可能外洩機密資料
  }
  ```
- ✅ **正確示範**：轉換為 ViewModel
  ```csharp
  public async Task<IActionResult> Profile() {
      var user = await _userService.GetUserAsync(userId);
      var viewModel = new UserProfileViewModel {
          Username = user.Username,
          Email = user.Email
      };
      return View(viewModel); // ✅ 安全
  }
  ```

### 2. 型別安全與 Nullable Reference Types
- 專案已啟用 `<Nullable>enable</Nullable>`，請**務必**正確處理 Null。
- 如果某個字串或物件允許為空，請標記為 `?`（例如 `public string? Description { get; set; }`）。
- 若不允許為空，但在建構時尚未賦值，可使用 `required` 關鍵字（C# 11+）或給予預設值：
  ```csharp
  public required string Title { get; set; }
  public string Content { get; set; } = string.Empty;
  ```

### 3. 執行時期資料驗證 (Runtime Data Validation)
在 `ViewModels` 或 `DTOs` 中，善用 Data Annotations 搭配 Controller 中的 `ModelState.IsValid` 進行後端防護。
- 字串檢查：優先使用 `string.IsNullOrEmpty()` 或 `string.IsNullOrWhiteSpace()`。
- 驗證標籤範例：
  ```csharp
  using System.ComponentModel.DataAnnotations;

  public class UserRegisterViewModel
  {
      [Required(ErrorMessage = "使用者名稱為必填")]
      [StringLength(50, MinimumLength = 3, ErrorMessage = "長度必須在 3 到 50 個字元之間")]
      public required string Username { get; set; }

      [Required]
      [EmailAddress(ErrorMessage = "Email 格式不正確")]
      public required string Email { get; set; }
  }
  ```

### 4. 避免動態與弱型別
- **嚴禁使用** `dynamic` 或 `object`（除非必須使用 Reflection 或處理未知結構的 JSON）。
- 避免在 View 中使用 `ViewBag` 或 `ViewData` 來傳遞複雜資料，一律使用強型別的 ViewModel。

---

## 🎯 總結檢查清單 (Checklist)
- [ ] 實體 (Entity) 是否被隔離，沒有直接傳遞給 View？
- [ ] ViewModel 的命名是否明確且針對特定的 View？
- [ ] 屬性的 Nullable (`?`) 標示是否精確？
- [ ] 是否已經為表單 ViewModel 加上了適當的 `[Required]` 等驗證標籤？
