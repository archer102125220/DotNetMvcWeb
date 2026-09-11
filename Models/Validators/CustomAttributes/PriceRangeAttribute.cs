using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace DotNetMvcWeb.Models.Validators.CustomAttributes
{
    /// <summary>
    /// [教學示範：自訂驗證屬性 (Custom Validation Attribute)]
    /// ------------------------------------------------------------------------------------------------
    /// 💡 什麼是 Validator (驗證器)？
    /// 在 Web 應用程式中，前端傳入的資料（來自表單或 JSON 請求）絕不能盲目信任。
    /// Validator 是在資料進入核心商業邏輯或寫入資料庫之前，負責把關「格式正確性」與「商業合理性」的機制。
    /// 
    /// ⚙️ 宣告式驗證 (Declarative Validation) 原理：
    /// .NET 提供了 System.ComponentModel.DataAnnotations 命名空間，允許我們透過「屬性標籤 (Attributes)」
    /// 裝飾在 DTO 或 Model 的屬性上（例如 [Required]、[StringLength]）。
    /// 當前端發送請求至 Controller 時，ASP.NET Core 的 Model Binder 會自動執行這些驗證標籤，
    /// 並將驗證結果儲存在 ModelState 中供程式碼判斷（如 if (!ModelState.IsValid)）。
    /// 
    /// 🛠️ 為什麼需要「自訂」驗證屬性？
    /// 內建的 [Range] 標籤在某些型別轉換（如 decimal）或特定業務規則（如價格不能為 0、需依不同幣別限制）時彈性較低。
    /// 透過繼承 ValidationAttribute 並覆寫 IsValid 方法，我們能將專屬的商業驗證邏輯模組化、重複使用，
    /// 並提供清晰的客製化繁體中文錯誤訊息。
    /// ------------------------------------------------------------------------------------------------
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class PriceRangeAttribute : ValidationAttribute
    {
        /// <summary>
        /// 允許的最小金額 (預設為 0.01)
        /// </summary>
        public double Minimum { get; }

        /// <summary>
        /// 允許的最大金額 (預設為 double.MaxValue)
        /// </summary>
        public double Maximum { get; }

        /// <summary>
        /// 初始化 PriceRangeAttribute 實例，設定金額上下限
        /// </summary>
        /// <param name="minimum">最小金額 (例如 0.01)</param>
        /// <param name="maximum">最大金額 (預設為 1,000,000)</param>
        public PriceRangeAttribute(double minimum = 0.01, double maximum = 1_000_000.0)
        {
            Minimum = minimum;
            Maximum = maximum;
            ErrorMessage = "價格必須介於 {0} 與 {1} 之間，且不得為負數或零。";
        }

        /// <summary>
        /// 執行自訂驗證邏輯
        /// </summary>
        /// <param name="value">屬性傳入的待驗證數值</param>
        /// <param name="validationContext">驗證上下文資訊 (包含物件實例、成員名稱等)</param>
        /// <returns>驗證結果 (成功回傳 ValidationResult.Success，失敗回傳包含錯誤訊息的 ValidationResult)</returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            ArgumentNullException.ThrowIfNull(validationContext);

            // 若欄位為 null，由 [Required] 屬性負責檢查是否允許為空，此處視為可略過或交由 Required 處理
            if (value is null)
            {
                return ValidationResult.Success;
            }

            decimal decimalValue;

            // 支援 decimal、double、float、int 等數值型別轉換
            if (value is decimal d)
            {
                decimalValue = d;
            }
            else if (value is double db)
            {
                decimalValue = (decimal)db;
            }
            else if (value is float f)
            {
                decimalValue = (decimal)f;
            }
            else if (value is int i)
            {
                decimalValue = i;
            }
            else if (decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsed))
            {
                decimalValue = parsed;
            }
            else
            {
                string memberName = validationContext.MemberName ?? validationContext.DisplayName;
                return new ValidationResult($"欄位 '{memberName}' 必須是有效的數值格式。", new[] { memberName });
            }

            decimal minDecimal = (decimal)Minimum;
            decimal maxDecimal = (decimal)Maximum;

            if (decimalValue < minDecimal || decimalValue > maxDecimal)
            {
                string memberName = validationContext.MemberName ?? validationContext.DisplayName;
                string formattedError = string.Format(
                    CultureInfo.CurrentCulture,
                    ErrorMessageString,
                    Minimum.ToString("N2", CultureInfo.CurrentCulture),
                    Maximum.ToString("N2", CultureInfo.CurrentCulture));

                return new ValidationResult(formattedError, new[] { memberName });
            }

            return ValidationResult.Success;
        }
    }
}
