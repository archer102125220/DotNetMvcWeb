using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Models.DTOs.Products;
using Microsoft.AspNetCore.Mvc;

namespace DotNetMvcWeb.Controllers
{
    /// <summary>
    /// [教學示範：MVC 表單與 DTO / ViewModel 整合]
    /// ------------------------------------------------------------------------------------------------
    /// 💡 為什麼 MVC 表單要使用 DTO / ViewModel 代替直接綁定 Entity Model？
    /// 
    /// 1. 🛑 告別 [Bind] 屬性的妥協做法：
    ///    - 在早期 ASP.NET MVC 中，為了防止使用者多送欄位，常寫 [Bind("Id,Name,Price,Description")]。
    ///    - 這種方式字串容易拼錯且難以維護。現代最佳實踐是直接使用獨立的 DTO (如 ProductCreateDto)，
    ///      讓強型別編譯器與 Model Binder 嚴格只接收允許輸入的屬性。
    /// 
    /// 2. 🛡️ 宣告式驗證 (Data Annotations &amp; Custom Validators)：
    ///    - 在 ProductCreateDto 上標記的 [Required]、[PriceRange]、[NonEmptyString] 標籤，
    ///      在表單 POST 送達時會自動觸發驗證，並將錯誤放入 ModelState。
    ///    - 若 ModelState.IsValid 為 false，Controller 只要 return View(dto)，
    ///      Razor 頁面的 &lt;span asp-validation-for="..."&gt; 就會自動渲染紅字錯誤訊息給使用者！
    /// 
    /// 3. 📦 職責分離：
    ///    - Controller 負責驗證 ModelState、將 DTO 轉換為 Entity 儲存至資料庫、或將 Entity 轉為 ResponseDto 傳給 View。
    /// ------------------------------------------------------------------------------------------------
    /// </summary>
    public class ProductsController : Controller
    {
        private readonly IProductRepository _productRepository;

        public ProductsController(IProductRepository productRepository)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            IEnumerable<Product> products = await _productRepository.GetAllAsync();
            IEnumerable<ProductResponseDto> responseDtos = products.Select(ProductResponseDto.FromEntity);
            return View(responseDtos);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            Product? product = await _productRepository.GetByIdAsync(id);
            if (product is null)
            {
                return NotFound();
            }

            ProductResponseDto responseDto = ProductResponseDto.FromEntity(product);
            return View(responseDto);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View(new ProductCreateDto());
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateDto dto)
        {
            if (dto is null)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                // 將 DTO 對映轉換為 Entity 模型
                Product product = new()
                {
                    Name = dto.Name,
                    Price = dto.Price,
                    Description = dto.Description
                };

                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }

            // 驗證失敗時，將原 DTO 資料帶回 View，以保留使用者輸入內容並顯示驗證錯誤
            return View(dto);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            Product? product = await _productRepository.GetByIdAsync(id);
            if (product is null)
            {
                return NotFound();
            }

            ProductUpdateDto updateDto = new()
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description
            };

            return View(updateDto);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductUpdateDto dto)
        {
            if (dto is null)
            {
                return BadRequest();
            }

            if (id != dto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                Product product = new()
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Price = dto.Price,
                    Description = dto.Description
                };

                await _productRepository.UpdateAsync(product);
                return RedirectToAction(nameof(Index));
            }

            return View(dto);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            Product? product = await _productRepository.GetByIdAsync(id);
            if (product is null)
            {
                return NotFound();
            }

            ProductResponseDto responseDto = ProductResponseDto.FromEntity(product);
            return View(responseDto);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
