using System;

namespace DotNetMvcWeb.Extensions
{
    // 1. 類別必須是靜態的 (static class)
    public static class StringExtensions
    {
        /// <summary>
        /// 截斷過長的字串，並在結尾加上省略號 "..."
        /// </summary>
        /// <param name="value">要擴充的型別，前面必須加上 `this` 關鍵字</param>
        /// <param name="maxLength">最大長度</param>
        /// <returns>處理後的字串</returns>
        // 2. 方法必須是靜態的 (static method)
        // 3. 第一個參數必須加上 `this` 關鍵字，代表你要「擴充」哪一個型別
        public static string Truncate(this string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) 
            {
                return string.Empty;
            }

            return value.Length <= maxLength ? value : $"{value.Substring(0, maxLength)}...";
        }
    }
}
