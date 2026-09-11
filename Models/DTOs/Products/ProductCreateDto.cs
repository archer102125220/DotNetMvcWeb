using System.ComponentModel.DataAnnotations;
using DotNetMvcWeb.Models.Validators.CustomAttributes;

namespace DotNetMvcWeb.Models.DTOs.Products
{
    /// <summary>
    /// [教學示範：資料傳輸物件 (DTO - Data Transfer Object)]
    /// ------------------------------------------------------------------------------------------------
    /// 💡 什麼是 DTO？
    /// DTO (Data Transfer Object) 是一種用於在軟體系統不同層次（例如 API 與客戶端、Controller 與 Service）
    /// 之間傳遞「純粹資料」的物件。它通常不包含任何商業邏輯方法，僅有純屬性與基本資料格式驗證標籤。
    /// 
    /// 🛡️ 為什麼新增產品要用 ProductCreateDto，而不是直接使用 Entity Model (Product.cs)？
    /// 
    /// 1. 🛑 防止 Over-Posting (Mass Assignment 批量指派攻擊)：
    ///    - 若直接將 Entity（Product）作為 Controller 的接收參數，使用者可以在送出 JSON 請求時，
    ///      刻意加入他不該控制的欄位（例如手動指定 Id、CreatedAt 或 IsAdmin 權限欄位）。
    ///    - 透過 ProductCreateDto，我們只暴露允許前端輸入的欄位（Name, Price, Description），
    ///      刻意排除 Id，從架構層面直接杜絕未授權竄改！
    /// 
    /// 2. 🔀 職責分離 (Separation of Concerns)：
    ///    - 資料庫的 Table 欄位設計（Entity）不該直接綁定到 API 介面合約。
    ///    - 未來即使資料庫表結構調整，API 請求合約仍可保持向下相容，降低系統耦合度。
    /// 
    /// 3. ✅ 專屬輸入驗證：
    ///    - 新增與修改可能需要不同的驗證規則（例如新增時不需要 Id，但修改時 Id 必須存在且大於 0）。
    ///    - 針對不同情境使用獨立的 DTO 能讓驗證職責更加專注清晰。
    /// ------------------------------------------------------------------------------------------------
    /// </summary>
    public class ProductCreateDto
    {
        /// <summary>
        /// 產品名稱 (必填，長度介於 1 到 100 個字元)
        /// </summary>
        [Required(ErrorMessage = "產品名稱為必填欄位。")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "產品名稱長度必須在 1 到 100 個字元之間。")]
        [NonEmptyString(ErrorMessage = "產品名稱不可為純空白字元。")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 產品價格 (必填，金額必須介於 0.01 到 1,000,000 元之間)
        /// </summary>
        [Required(ErrorMessage = "產品價格為必填欄位。")]
        [PriceRange(0.01, 1_000_000.0, ErrorMessage = "產品價格必須介於 NT$ {0} 到 NT$ {1} 之間。")]
        public decimal Price { get; set; }

        /// <summary>
        /// 產品詳細描述 (選填，最多 500 個字元)
        /// </summary>
        [StringLength(500, ErrorMessage = "產品描述最多不能超過 500 個字元。")]
        public string Description { get; set; } = string.Empty;
    }
}
