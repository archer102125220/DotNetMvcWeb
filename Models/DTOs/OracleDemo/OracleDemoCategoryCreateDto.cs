using System.ComponentModel.DataAnnotations;
using DotNetMvcWeb.Models.Validators.CustomAttributes;

namespace DotNetMvcWeb.Models.DTOs.OracleDemo
{
    /// <summary>
    /// Oracle 示範分類新增資料傳輸物件 (OracleDemoCategory Create DTO)
    /// 用於建立新分類時接收使用者輸入
    /// </summary>
    public class OracleDemoCategoryCreateDto
    {
        /// <summary>
        /// 分類名稱 (必填，長度介於 1 到 50 個字元)
        /// </summary>
        [Required(ErrorMessage = "分類名稱為必填欄位。")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "分類名稱長度必須在 1 到 50 個字元之間。")]
        [NonEmptyString(ErrorMessage = "分類名稱不可為純空白字元。")]
        public string Name { get; set; } = string.Empty;
    }
}
