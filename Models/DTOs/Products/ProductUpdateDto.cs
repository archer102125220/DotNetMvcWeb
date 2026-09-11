using System.ComponentModel.DataAnnotations;
using DotNetMvcWeb.Models.Validators.CustomAttributes;

namespace DotNetMvcWeb.Models.DTOs.Products
{
    /// <summary>
    /// 產品更新資料傳輸物件 (Product Update DTO)
    /// 用於 PUT /api/products/{id} 或表單更新時接收客戶端資料
    /// </summary>
    public class ProductUpdateDto
    {
        /// <summary>
        /// 產品唯一識別碼 (必填，且必須大於 0)
        /// </summary>
        [Required(ErrorMessage = "產品 ID 為必填欄位。")]
        [Range(1, int.MaxValue, ErrorMessage = "產品 ID 必須為大於 0 的有效數值。")]
        public int Id { get; set; }

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
