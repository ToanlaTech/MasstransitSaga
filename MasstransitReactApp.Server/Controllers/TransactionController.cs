using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MasstransitReactApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly AccountNumberProvider _accountNumberProvider;

        public TransactionController(AccountNumberProvider accountNumberProvider)
        {
            _accountNumberProvider = accountNumberProvider;
        }

        [HttpGet("balances")]
        public IActionResult GetAccountBalances()
        {
            var balances = _accountNumberProvider.GetAllAccountBalances();
            return Ok(balances);
        }

        /// <summary>
        /// 1. Duyệt qua danh sách tài khoản (lấy từ AccountNumberProvider) theo thứ tự tuần tự.
        /// 2. Kiểm tra số dư từng tài khoản.
        /// 3. Trừ tiền từ tài khoản đầu tiên đủ số dư.
        /// 4. Nếu không có tài khoản nào đủ số dư, trả về thông báo lỗi.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("deduct")]
        public IActionResult DeductAmount([FromBody] TransactionRequest request)
        {
            string selectedAccount = null;
            decimal remainingBalance = 0m;

            // Lặp qua tất cả tài khoản để tìm tài khoản đủ số dư
            for (int i = 0; i < 10; i++) // Giả định danh sách chỉ có 10 tài khoản
            {
                var currentAccount = _accountNumberProvider.GetNextAccountNumber();
                var currentBalance = _accountNumberProvider.GetAccountBalance(currentAccount);

                // Kiểm tra nếu tài khoản có đủ số dư
                if (currentBalance >= request.Amount)
                {
                    selectedAccount = currentAccount;
                    remainingBalance = currentBalance - request.Amount;

                    // Trừ tiền
                    _accountNumberProvider.AddAmountToAccount(currentAccount, -request.Amount);
                    break;
                }
            }

            // Nếu không tìm được tài khoản nào đủ số dư
            if (selectedAccount == null)
            {
                return BadRequest("Transaction failed: No account has sufficient balance.");
            }

            // Trả về kết quả thành công
            return Ok(new
            {
                Message = $"Transaction successful: Deducted {request.Amount} from Account {selectedAccount}.",
                RemainingBalance = remainingBalance
            });
        }
    }
}

public class TransactionRequest
{
    public decimal Amount { get; set; }
}

public class AccountNumberProvider
{
    private readonly Queue<string> _accountNumbers;
    private readonly Dictionary<string, decimal> _accountBalances;

    public AccountNumberProvider()
    {
        _accountNumbers = new Queue<string>(new[]
        {
            "000000001",
            "000000002",
            "000000003",
            "000000004",
            "000000005",
            "000000006",
            "000000007",
            "000000008",
            "0000000010",
            "0000000011"
        });

        // Khởi tạo số dư ban đầu cho mỗi tài khoản
        _accountBalances = new Dictionary<string, decimal>
    {
        { "000000001", 1000m },
        { "000000002", 2000m },
        { "000000003", 3000m },
        { "000000004", 4000m },
        { "000000005", 5000m },
        { "000000006", 6000m },
        { "000000007", 7000m },
        { "000000008", 8000m },
        { "000000009", 9000m },
        { "000000010", 10000m }
    };
    }

    // Lấy số tài khoản tiếp theo
    public string GetNextAccountNumber()
    {
        lock (_accountNumbers)
        {
            var account = _accountNumbers.Dequeue();
            _accountNumbers.Enqueue(account); // Đưa lại vào cuối để tạo vòng lặp
            return account;
        }
    }

    // Thêm số tiền vào tài khoản
    public void AddAmountToAccount(string account, decimal amount)
    {
        lock (_accountBalances)
        {
            if (_accountBalances.ContainsKey(account))
            {
                _accountBalances[account] += amount;
            }
        }
    }

    // Lấy số dư tài khoản
    public decimal GetAccountBalance(string account)
    {
        lock (_accountBalances)
        {
            return _accountBalances.ContainsKey(account) ? _accountBalances[account] : 0m;
        }
    }

    // Trả về tất cả số dư hiện tại
    public Dictionary<string, decimal> GetAllAccountBalances()
    {
        lock (_accountBalances)
        {
            return new Dictionary<string, decimal>(_accountBalances);
        }
    }
}

