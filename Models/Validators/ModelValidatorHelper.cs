using System.ComponentModel.DataAnnotations;

namespace DotNetMvcWeb.Models.Validators
{
    /// <summary>
    /// 模型驗證結果封裝物件
    /// </summary>
    public class ModelValidationResult
    {
        /// <summary>
        /// 是否通過所有驗證規則
        /// </summary>
        public bool IsValid => Errors.Count == 0;

        /// <summary>
        /// 驗證失敗的錯誤清單
        /// </summary>
        public IList<ValidationResult> Errors { get; } = new List<ValidationResult>();

        /// <summary>
        /// 取得指定屬性名稱的第一個錯誤訊息
        /// </summary>
        /// <param name="propertyName">屬性名稱</param>
        /// <returns>錯誤訊息，若無錯誤則回傳 null</returns>
        public string? GetErrorMessageForProperty(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return null;
            }

            foreach (ValidationResult error in Errors)
            {
                if (error.MemberNames.Contains(propertyName, StringComparer.OrdinalIgnoreCase))
                {
                    return error.ErrorMessage;
                }
            }

            return null;
        }

        /// <summary>
        /// 是否包含指定屬性的驗證錯誤
        /// </summary>
        public bool HasErrorFor(string propertyName)
        {
            return GetErrorMessageForProperty(propertyName) is not null;
        }
    }

    /// <summary>
    /// [教學示範：模型驗證輔助工具 (Model Validator Helper)]
    /// ------------------------------------------------------------------------------------------------
    /// 💡 為什麼需要獨立的 ModelValidatorHelper？
    /// 
    /// 1. 框架自動驗證 vs. 手動驗證：
    ///    - 在 ASP.NET Core Controller 中，框架會在 Action 執行前自動利用 Model Binder 驗證 Model，
    ///      並將結果放入 ModelState 物件中。
    ///    - 但是在「單元測試 (Unit Tests)」、「背景服務 (Background Services)」、「Console 應用程式」
    ///      或「商業邏輯層 (Service Layer)」中，並沒有 HTTP Pipeline 來幫忙自動驗證！
    /// 
    /// 2. 解耦與可測試性 (Testability)：
    ///    - .NET 提供了底層原生 API：Validator.TryValidateObject(instance, context, results, validateAllProperties: true)。
    ///    - 透過將此邏輯封裝在 ModelValidatorHelper 中，我們可以很輕易在單元測試中直接呼叫：
    ///      var result = ModelValidatorHelper.Validate(dto);
    ///      Assert.False(result.IsValid);
    ///      快速驗證 DTO 的 Data Annotations 與 IValidatableObject 複合規則，無須架設 WebApplicationFactory。
    /// 
    /// 3. validateAllProperties: true 的重要性：
    ///    - 若未指定 validateAllProperties: true，Validator.TryValidateObject 只會檢查帶有 [Required] 的欄位！
    ///    - 此 Helper 預設開啟 true，確保所有自訂屬性、範圍、長度及 IValidatableObject 皆會被完整檢查。
    /// ------------------------------------------------------------------------------------------------
    /// </summary>
    public static class ModelValidatorHelper
    {
        /// <summary>
        /// 驗證指定的模型物件（包含 Data Annotations 屬性與 IValidatableObject 複合驗證）
        /// </summary>
        /// <typeparam name="T">模型型別</typeparam>
        /// <param name="model">待驗證的物件實例</param>
        /// <param name="serviceProvider">可選的依賴注入服務提供者 (供 ValidationContext 解析依賴使用)</param>
        /// <returns>封裝後的驗證結果</returns>
        public static ModelValidationResult Validate<T>(T model, IServiceProvider? serviceProvider = null) where T : notnull
        {
            ArgumentNullException.ThrowIfNull(model);

            ModelValidationResult result = new();
            ValidationContext validationContext = new(model, serviceProvider, items: null);

            // validateAllProperties 設為 true，確保非 [Required] 的驗證標籤（如 [StringLength]、[PriceRange]）也會被觸發
            Validator.TryValidateObject(
                instance: model,
                validationContext: validationContext,
                validationResults: result.Errors,
                validateAllProperties: true);

            return result;
        }

        /// <summary>
        /// 驗證指定模型中的單一屬性
        /// </summary>
        /// <param name="model">待驗證的物件實例</param>
        /// <param name="propertyName">屬性名稱</param>
        /// <param name="propertyValue">屬性當前數值</param>
        /// <returns>該屬性的驗證錯誤清單</returns>
        public static IList<ValidationResult> ValidateProperty(object model, string propertyName, object? propertyValue)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

            List<ValidationResult> results = new();
            ValidationContext validationContext = new(model, serviceProvider: null, items: null)
            {
                MemberName = propertyName
            };

            Validator.TryValidateProperty(
                value: propertyValue,
                validationContext: validationContext,
                validationResults: results);

            return results;
        }
    }
}
