# Oracle Demo API 使用指南

除了原本搭配 HTMX 的 MVC 畫面外，專案中也提供了一組純 JSON 格式的 RESTful API，方便您使用 Postman 測試，或是串接 Vue、React、Nuxt 等前端框架。

## 📍 API 基礎資訊

* **基礎路徑 (Base URL)**: `/api/oracle-demo`
* **控制器 (Controller)**: `Controllers/Api/OracleDemoApiController.cs`
* **資料格式**: `application/json`

---

## 🛠️ API 端點列表 (Endpoints)

### 1. 取得所有資料 (支援搜尋)
* **方法**: `GET`
* **路徑**: `/api/oracle-demo`
* **Query 參數**: `?keyword={字串}` (選填，若提供則會使用原生的 `FromSqlInterpolated` 進行模糊搜尋)
* **回應範例 (200 OK)**:
  ```json
  [
    {
      "id": 1,
      "name": "測試資料 1",
      "description": "這是一段測試",
      "createdAt": "2026-06-02T12:00:00Z"
    }
  ]
  ```

### 2. 取得單筆資料
* **方法**: `GET`
* **路徑**: `/api/oracle-demo/{id}`
* **回應範例 (200 OK)**: (同上，回傳單一物件)
* **回應範例 (404 Not Found)**:
  ```json
  { "message": "找不到指定的項目" }
  ```

### 3. 建立新資料
* **方法**: `POST`
* **路徑**: `/api/oracle-demo`
* **Request Body (JSON)**: `id` 與 `createdAt` 不需提供，系統會自動生成。
  ```json
  {
    "name": "我是新建立的資料",
    "description": "透過 POST 建立"
  }
  ```
* **回應範例 (201 Created)**: 回傳剛剛建立好的完整物件資料。

### 4. 更新資料
* **方法**: `PUT`
* **路徑**: `/api/oracle-demo/{id}`
* **Request Body (JSON)**: 必須傳送完整的物件 (包含 ID，且需與網址的 ID 相同)。
  ```json
  {
    "id": 1,
    "name": "更新後的名稱",
    "description": "更新後的描述",
    "createdAt": "2026-06-02T12:00:00Z"
  }
  ```
* **回應 (204 No Content)**: 更新成功不回傳內容。若 ID 不吻合回傳 `400 BadRequest`。

### 5. 刪除資料
* **方法**: `DELETE`
* **路徑**: `/api/oracle-demo/{id}`
* **回應範例 (200 OK)**:
  ```json
  { "message": "刪除成功" }
  ```

---

## 💡 C# 實作特別注意事項 (Deep Check 規範)

1. **繼承類別不同**：API Controller 是繼承 `ControllerBase`，而不是 `Controller` (不需要回傳 HTML View)。
2. **屬性標記**：
   * 加上 `[ApiController]` 會讓 ASP.NET Core 自動執行模型驗證 (Model Validation)，如果送來的 JSON 缺少必填欄位，會自動回傳 `400 BadRequest`。
   * 加上 `[Route("api/[controller]")]` 或自訂路由來定義端點。
3. **參數綁定來源**：
   * `[FromQuery]`：告訴系統從 URL 參數拿資料 (如 `?keyword=abc`)。
   * `[FromBody]`：告訴系統從 POST 的 JSON Body 拿資料。
4. **效能優化**：如同 MVC Controller，在所有的 `GET` 查詢中，都嚴格使用了 `.AsNoTracking()` 來釋放 EF Core 的追蹤負擔。
5. **架構與目錄分類慣例**：雖然在 ASP.NET Core 中，Controller 放在哪個實體資料夾並不影響路由 (路由由 `[Route]` 標籤決定)，但**強烈建議**將純回傳 JSON 的 API 控制器與回傳 HTML View 的 MVC 控制器在實體目錄上分開 (例如統一放在 `Controllers/Api/` 目錄下)。這能大幅提高專案的可讀性與後續維護的便利性。
