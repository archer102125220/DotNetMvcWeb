using System.ComponentModel.DataAnnotations;
using DotNetMvcWeb.Models.Validators.CustomAttributes;

namespace DotNetMvcWeb.Models.DTOs.OracleDemo
{
    /// <summary>
    /// Oracle 示範項目更新資料傳輸物件 (OracleDemoItem Update DTO)
    /// 用於 PUT /api/oracle-demo/{id} 時接收使用者輸入
    /// </summary>
    public class OracleDemoItemUpdateDto : IValidatableObject
    {
        /// <summary>
        /// 項目唯一識別碼 (必填，且必須大於 0)
        /// </summary>
        [Required(ErrorMessage = "項目 ID 為必填欄位。")]
        [Range(1, int.MaxValue, ErrorMessage = "項目 ID 必須為大於 0 的有效數值。")]
        public int Id { get; set; }

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
