using System.ComponentModel.DataAnnotations;
using DotNetMvcWeb.Models.Validators.CustomAttributes;

namespace DotNetMvcWeb.Models.DTOs.OracleDemo
{
    /// <summary>
    /// Oracle 示範分類更新資料傳輸物件 (OracleDemoCategory Update DTO)
    /// 用於更新分類時接收使用者輸入
    /// </summary>
    public class OracleDemoCategoryUpdateDto
    {
        /// <summary>
        /// 分類唯一識別碼 (必填，且必須大於 0)
        /// </summary>
        [Required(ErrorMessage = "分類 ID 為必填欄位。")]
        [Range(1, int.MaxValue, ErrorMessage = "分類 ID 必須為大於 0 的有效數值。")]
        public int Id { get; set; }

        /// <summary>
        /// 分類名稱 (必填，長度介於 1 到 50 個字元)
        /// </summary>
        [Required(ErrorMessage = "分類名稱為必填欄位。")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "分類名稱長度必須在 1 到 50 個字元之間。")]
        [NonEmptyString(ErrorMessage = "分類名稱不可為純空白字元。")]
        public string Name { get; set; } = string.Empty;
    }
}
