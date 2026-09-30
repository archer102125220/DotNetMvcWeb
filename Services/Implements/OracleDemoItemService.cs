using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Repositories.Implements;
using DotNetMvcWeb.Repositories.Interfaces;
using DotNetMvcWeb.Services.Interfaces;

namespace DotNetMvcWeb.Services.Implements
{
    /// <summary>
    /// [教學註解] 服務層實作 (Service Implementation)
    /// 負責處理商業邏輯（例如設定預設建立時間、業務規則檢核、資料流調度）。
    /// 原本直接操作 DbContext 的資料庫讀寫、ADO.NET、預存程序呼叫，現在全部委派給 IOracleDemoItemRepository。
    /// 這種分層使得 Service 專注於業務邏輯，大幅降低與底層資料庫 ORM 的耦合。
    /// </summary>
    public class OracleDemoItemService : IOracleDemoItemService
    {
        private readonly IOracleDemoItemRepository _itemRepository;

        // DI 容器使用的唯一 public 建構子
        public OracleDemoItemService(IOracleDemoItemRepository itemRepository)
        {
            ArgumentNullException.ThrowIfNull(itemRepository);
            _itemRepository = itemRepository;
        }

        // 提供給既有單元測試直接傳入 DbContext 的便利建構子 (設為 internal 避免 DI 歧義)
        internal OracleDemoItemService(AppDbContext context)
            : this(new OracleDemoItemRepository(context))
        {
        }

        public async Task<List<OracleDemoItem>> GetItemsAsync(string? keyword = null)
        {
            return await _itemRepository.GetItemsAsync(keyword);
        }

        public async Task<OracleDemoItem?> GetItemByIdAsync(int id, bool includeCategory = false)
        {
            return await _itemRepository.GetItemByIdAsync(id, includeCategory);
        }

        public async Task CreateItemAsync(OracleDemoItem item)
        {
            ArgumentNullException.ThrowIfNull(item);

            // [教學註解] 商業邏輯規則：若未指派建立時間，預設為當前 UTC 時間
            if (item.CreatedAt == default)
            {
                item.CreatedAt = DateTime.UtcNow;
            }

            await _itemRepository.AddItemAsync(item);
        }

        public async Task UpdateItemAsync(OracleDemoItem item)
        {
            ArgumentNullException.ThrowIfNull(item);
            await _itemRepository.UpdateItemAsync(item);
        }

        public async Task DeleteItemAsync(int id)
        {
            await _itemRepository.DeleteItemAsync(id);
        }

        public bool ItemExists(int id)
        {
            return _itemRepository.ItemExists(id);
        }

        public async Task<List<OracleDemoItem>> GetItemsViaAdoNetAsync(string? keyword = null)
        {
            return await _itemRepository.GetItemsViaAdoNetAsync(keyword);
        }

        public async Task UpdateItemDescriptionViaProcAsync(int id, string newDescription)
        {
            await _itemRepository.UpdateItemDescriptionViaProcAsync(id, newDescription);
        }
    }
}
