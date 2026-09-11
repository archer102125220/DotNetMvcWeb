using System.ComponentModel.DataAnnotations;
using DotNetMvcWeb.Models.Validators.CustomAttributes;

namespace DotNetMvcWeb.Models.DTOs.OracleDemo
{
    /// <summary>
    /// [教學示範：Oracle 項目新增 DTO 與 IValidatableObject 複合驗證]
    /// ------------------------------------------------------------------------------------------------
    /// 💡 什麼是 DTO (Data Transfer Object)？
    /// DTO 是一個專門用來在前端（HTTP 請求）與後端（Controller/Service）之間傳遞資料的純物件契約。
    /// 
    /// 🛡️ 為什麼新增資料時「絕對不要」直接使用 Entity Model (OracleDemoItem.cs)？
    /// 
    /// 1. 🛑 防範 Over-Posting (Mass Assignment 批量指派漏洞)：
    ///    - 如果 Action 參數寫成 public IActionResult Create([FromBody] OracleDemoItem item)，
    ///      攻擊者可以刻意在 JSON 傳送 { "id": 1, "createdAt": "2020-01-01" } 等內部控制欄位。
    ///    - 透過 DTO，我們只定義客戶端「有權限提供」的欄位（Name, Description, CategoryId），
    ///      嚴格封閉未授權屬性。
    /// 
    /// 2. 🧩 什麼是 IValidatableObject (自訂跨屬性/複合驗證)？
    ///    - 一般的屬性標籤（如 [Required]、[StringLength]）只能單獨驗證「單一屬性」的合法性。
    ///    - 當需要「多個屬性互相依賴」的複合商業邏輯時（例如：若名稱包含「重要」，則描述不得為空），
    ///      可讓 DTO 實作 IValidatableObject 介面並實作 Validate 方法。
    /// 
    /// ⏱️ ASP.NET Core 的驗證執行生命週期 (Validation Lifecycle)：
    ///    第 1 階段：執行個別屬性上的 ValidationAttribute（如 [Required], [StringLength]）。
    ///    第 2 階段：若第 1 階段「全部通過」，框架才會自動呼叫 IValidatableObject.Validate() 進行跨欄位檢核。
    ///    第 3 階段：將所有驗證錯誤彙整至 Controller 的 ModelState 物件中。
    /// ------------------------------------------------------------------------------------------------
    /// </summary>
    public class OracleDemoItemCreateDto : IValidatableObject
    {
        /// <summary>
        /// 項目名稱 (必填，長度 1 到 100 個字元)
        /// </summary>
        [Required(ErrorMessage = "項目名稱為必填欄位。")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "項目名稱長度必須在 1 到 100 個字元之間。")]
        [NonEmptyString(ErrorMessage = "項目名稱不可為純空白字元。")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 項目描述 (選填，最多 500 個字元)
        /// </summary>
        [StringLength(500, ErrorMessage = "項目描述最多不能超過 500 個字元。")]
        public string? Description { get; set; }

        /// <summary>
        /// 所屬分類 ID (選填，若有填寫必須大於 0)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "分類 ID 必須為大於 0 的有效數值。")]
        public int? CategoryId { get; set; }

        /// <summary>
        /// 實作 IValidatableObject 介面，執行跨欄位的複合商業規則驗證
        /// </summary>
        /// <param name="validationContext">驗證上下文</param>
        /// <returns>驗證錯誤列舉清單</returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // [商業規則示範]：若項目名稱標記為「重要」或「加急」，則描述欄位 (Description) 必須填寫詳細原因
            if (!string.IsNullOrWhiteSpace(Name) &&
                (Name.Contains("重要", StringComparison.OrdinalIgnoreCase) ||
                 Name.Contains("加急", StringComparison.OrdinalIgnoreCase) ||
                 Name.Contains("Urgent", StringComparison.OrdinalIgnoreCase)))
            {
                if (string.IsNullOrWhiteSpace(Description))
                {
                    yield return new ValidationResult(
                        "當項目名稱標記為「重要」、「加急」或「Urgent」時，項目描述 (Description) 為必填欄位，請詳細說明原因。",
                        new[] { nameof(Description) });
                }
            }
        }
    }
}
