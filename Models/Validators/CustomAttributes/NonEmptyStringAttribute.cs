using System.ComponentModel.DataAnnotations;

namespace DotNetMvcWeb.Models.Validators.CustomAttributes
{
    /// <summary>
    /// [教學示範：自訂非空白字串驗證屬性 (Non-Empty String Validation Attribute)]
    /// ------------------------------------------------------------------------------------------------
    /// 💡 為什麼需要 NonEmptyStringAttribute？
    /// 內建的 [Required] 標籤預設只檢查「是否為 null」或「字串是否為空字串 ("")」。
    /// 若使用者輸入純空白字元 (如 "   ")，[Required] 可能會判定為合法通過。
    /// 為了防止資料庫儲存無意義的純空白資料，我們自訂此驗證屬性，強制要求字串必須包含非空白字元。
    /// ------------------------------------------------------------------------------------------------
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class NonEmptyStringAttribute : ValidationAttribute
    {
        public NonEmptyStringAttribute()
        {
            ErrorMessage = "欄位 '{0}' 不得為空或僅包含空白字元。";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            ArgumentNullException.ThrowIfNull(validationContext);

            // 若值為 null，交給 [Required] 處理
            if (value is null)
            {
                return ValidationResult.Success;
            }

            if (value is string strValue)
            {
                if (string.IsNullOrWhiteSpace(strValue))
                {
                    string memberName = validationContext.MemberName ?? validationContext.DisplayName;
                    string formattedError = FormatErrorMessage(memberName);
                    return new ValidationResult(formattedError, new[] { memberName });
                }
            }

            return ValidationResult.Success;
        }
    }
}
