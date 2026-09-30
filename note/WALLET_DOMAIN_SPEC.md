# 數位錢包領域規格定義書 (Wallet Domain Specification)

本文件定義 `Wallet` 模組在領域驅動設計 (DDD) 下的業務邊界、通用語言 (Ubiquitous Language)、實體行為、不變性約束與測試規格，作為 Phase 2 領域核心層實作的準則。

---

## 1. 通用語言 (Ubiquitous Language)

| 術語 (Term) | 英文名稱 | 領域定義 |
| :--- | :--- | :--- |
| **錢包** | `Wallet` | 代表使用者的虛擬資金或點數帳戶，具備唯一識別碼與獨立餘額，是本模組的**聚合根 (Aggregate Root)**。 |
| **金額** | `Money` | 交易與餘額的數值，需具備非負約束與標準貨幣格式（在本範例中預設為單一貨幣 TWD），為**值物件 (Value Object)**。 |
| **儲值** | `Deposit` | 增加錢包可用餘額的業務行為。 |
| **扣款/消費** | `Withdraw` | 減少錢包可用餘額的業務行為。 |
| **餘額不足** | `InsufficientBalance` | 當扣款金額大於現有餘額時觸發的業務違規狀態。 |

---

## 2. 業務規則與不變性約束 (Business Invariants)

在領域驅動設計中，**實體必須始終處於合法狀態**。所有規則均由實體自身保障，外部無法繞過：

1. **初始狀態**：
   - 建立錢包時必須指定有效的持有人識別碼 (`OwnerId`)，不可為空白。
   - 錢包建立時初始餘額必須大於或等於 0（預設為 0）。
2. **儲值規則 (`Deposit`)**：
   - 儲值金額必須大於 0（禁止儲值 0 或負數金額）。
   - 儲值成功後，錢包餘額必須即時加上該金額，並更新最後變動時間 (`UpdatedAt`)。
3. **扣款規則 (`Withdraw`)**：
   - 扣款金額必須大於 0（禁止扣款 0 或負數金額）。
   - 當「扣款金額 > 當前可用餘額」時，**嚴格拒絕交易**，必須拋出 `InsufficientBalanceException`。
   - 扣款成功後，錢包餘額扣減該金額，並更新最後變動時間 (`UpdatedAt`)。
4. **狀態保護 (Encapsulation)**：
   - `Balance`（餘額）屬性不可提供公開的 `public set`，只能透過 `Deposit()` 與 `Withdraw()` 方法進行變更。

---

## 3. 領域模型設計 (Domain Model Design)

### 3.1 實體：`Wallet`
- **屬性**：
  - `Guid Id`：錢包唯一識別碼 (PK)。
  - `string OwnerId`：錢包擁有人識別碼（如使用者帳號或 ID）。
  - `decimal Balance`：當前餘額（`private set` 封裝）。
  - `DateTime CreatedAt`：建立時間 (UTC)。
  - `DateTime UpdatedAt`：最後異動時間 (UTC)。
- **核心方法**：
  - `Wallet(string ownerId, decimal initialBalance = 0)` (建構子保護)
  - `void Deposit(decimal amount)`
  - `void Withdraw(decimal amount)`

### 3.2 領域異常：`Exceptions`
- `BusinessRuleException`：所有業務異常的基底類別（繼承自 `Exception`）。
- `InsufficientBalanceException`：扣款餘額不足異常，包含目前餘額與欲扣款金額等上下文資訊。
- `InvalidAmountException`：金額小於或等於 0 時拋出的參數異常。

### 3.3 領域倉儲介面：`IWalletRepository` (宣告於 Domain 層)
```csharp
namespace DotNetMvcWeb.Domain.Wallets.Repositories;

public interface IWalletRepository
{
    Task<Wallet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Wallet?> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default);
    Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default);
    Task UpdateAsync(Wallet wallet, CancellationToken cancellationToken = default);
}
```

---

## 4. 單元測試案例規劃 (Test Cases)

在 Phase 2 建立 Domain 層後，將於測試專案撰寫下列純單元測試驗證規則：

| 測試案例名稱 | 驗證目標 | 預期結果 |
| :--- | :--- | :--- |
| `CreateWallet_WithValidOwner_ShouldInitializeCorrectly` | 建立錢包 | 餘額為預設值，建立與更新時間正確 |
| `CreateWallet_WithEmptyOwner_ShouldThrowException` | 空持有人建立 | 拋出 `ArgumentException` |
| `Deposit_WithPositiveAmount_ShouldIncreaseBalance` | 正常儲值 | 餘額增加，更新時間被刷新 |
| `Deposit_WithZeroOrNegativeAmount_ShouldThrowException` | 儲值 0 或負數 | 拋出 `InvalidAmountException` |
| `Withdraw_WithSufficientBalance_ShouldDecreaseBalance` | 正常扣款 | 餘額扣減，更新時間被刷新 |
| `Withdraw_WithAmountGreaterThanBalance_ShouldThrowInsufficientBalanceException` | 餘額不足扣款 | 拋出 `InsufficientBalanceException`，且餘額維持原樣 |
| `Withdraw_WithZeroOrNegativeAmount_ShouldThrowException` | 扣款 0 或負數 | 拋出 `InvalidAmountException` |
