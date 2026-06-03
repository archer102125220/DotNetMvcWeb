using Microsoft.AspNetCore.Mvc;
using DotNetMvcWeb.Extensions;

namespace DotNetMvcWeb.Controllers
{
    public class ExtensionsDemoController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            SetupDemoData();
            return View();
        }

        [HttpPost]
        public IActionResult Index(string inputText, int maxLength)
        {
            SetupDemoData();
            
            // 將使用者輸入的值回傳給 View，以便保留在輸入框中
            ViewBag.TestInput = inputText;
            ViewBag.TestLength = maxLength;
            
            // 💡 這裡就是呼叫我們寫的擴充方法，即時處理使用者的輸入！
            ViewBag.TestResult = inputText.Truncate(maxLength);

            return View();
        }

        private void SetupDemoData()
        {
            string longDescription = "這是一段非常非常長的商品描述，在頁面上顯示的時候可能會因為過長而破壞版面，因此我們需要將其截斷。";
            ViewBag.OriginalText = longDescription;
            ViewBag.ShortText = longDescription.Truncate(15);
        }
    }
}
