using Microsoft.AspNetCore.Mvc;

namespace DotNetMvcWeb.Controllers
{
    public class RedirectDemoController : Controller
    {
        // 0. 範例列表頁面
        public IActionResult DemoList()
        {
            return View();
        }

        // 1. 基本的 URL 重新導向 (302 Found)
        public IActionResult ExternalRedirectDemo()
        {
            // 將使用者重新導向到指定的 URL
            return Redirect("https://www.google.com");
        }

        // 2. 永久的 URL 重新導向 (301 Moved Permanently)
        public IActionResult PermanentRedirectDemo()
        {
            // 告訴瀏覽器這個 URL 已經永久移動
            return RedirectPermanent("https://www.google.com");
        }

        // 3. 重新導向到同一個 Controller 中的另一個 Action
        public IActionResult RedirectToActionDemo()
        {
            // 重新導向到同一個 Controller 的 TargetAction
            // 第二個參數可以傳遞 Route Values (例如 query string 參數)
            return RedirectToAction(nameof(TargetAction), new { id = 123, message = "Hello from RedirectToAction" });
        }

        // 4. 重新導向到不同 Controller 的 Action
        public IActionResult RedirectToOtherController()
        {
            // 重新導向到 HomeController 的 Index Action
            return RedirectToAction("Index", "Home");
        }

        // 5. 安全的本地重新導向 (防止 Open Redirect 攻擊)
        public IActionResult LocalRedirectDemo(string returnUrl = "/Home/Index")
        {
            // 🚨 預設直接使用 LocalRedirect(returnUrl) 遇到外部網址時會「拋出例外 (Exception)」
            // return LocalRedirect(returnUrl);

            // ✅ 正常在實務上的安全處理方式：
            // 應該先使用 Url.IsLocalUrl() 判斷網址是否為安全的本地路由。
            // 如果是，才進行跳轉；如果不是（可能是惡意外部網址），則導向一個安全的預設頁面（例如首頁）。
            if (Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // 如果是惡意網址，我們不拋出異常，而是安全地將使用者導回首頁
                // return RedirectToAction("Index", "Home");

                // 為了在示範頁上看出錯誤，我們在這裡刻意讓它拋出例外 (或是直接呼叫 LocalRedirect 讓系統拋出)
                throw new InvalidOperationException("發現非本地的外部網址！為了示範目的，這裡刻意拋出例外。正常實務請使用上方註解的安全作法。");
            }
        }

        // 6. HTMX 重新導向 (搭配 HTMX 使用時)
        public IActionResult HtmxRedirectDemo()
        {
            // 當使用 HTMX 發送請求時，標準的 Redirect 可能只會替換部分 DOM
            // 如果需要整頁重新導向，可以設置 HX-Redirect Header
            Response.Headers["HX-Redirect"] = "/Home/Index";
            return Ok(); // 或是 return Content("");
        }

        // 這是用來接收 RedirectToAction 範例的目標 Action
        public IActionResult TargetAction(int id, string message)
        {
            ViewBag.Id = id;
            ViewBag.Message = message;
            return View();
        }
    }
}
