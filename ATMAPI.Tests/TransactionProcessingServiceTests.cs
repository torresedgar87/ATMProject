using ATMAPI.Database;
using ATMAPI.Database.Entities;
using ATMAPI.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace ATMAPI.Tests.Services
{
    public class TransactionProcessingServiceTests
    {
        private readonly Mock<ITransactionRepository> _transactionRepoMock;
        private readonly Mock<IAccountRepository> _accountRepoMock;
        private readonly Mock<ILogger<TransactionProcessingService>> _loggerMock;
        private readonly TransactionProcessingService _service;

        public TransactionProcessingServiceTests()
        {
            _transactionRepoMock = new Mock<ITransactionRepository>();
            _accountRepoMock = new Mock<IAccountRepository>();
            _loggerMock = new Mock<ILogger<TransactionProcessingService>>();

            _service = new TransactionProcessingService(
                _transactionRepoMock.Object,
                _accountRepoMock.Object,
                _loggerMock.Object);
        }

        private static AccountEntity CreateAccount(int idAlias, decimal balance, int lastProcessedId = 0)
        {
            return new()
            {
                IdAlias = idAlias,
                Balance = balance,
                LastProcessedTransactionId = lastProcessedId
            };
        }

        private static TransactionResultCombinedEntity CreateCombinedTransactionResult(int id, decimal amount,
            TransactionType type, int fromAccountId, int toAccountId = 0)
        {
            return new()
            {
                Id = id,
                Amount = amount,
                Type = type,
                FromAccountId = fromAccountId,
                ToAccountId = toAccountId,
                CreatedAt = DateTime.UtcNow
            };
        }

        [Fact]
        public async Task ProcessDepositAsync_IncreasesBalance_And_CreatesProcessedResult_AndUpdatesAccount()
        {
            var account = CreateAccount(idAlias: 1001, balance: 50m);
            var transaction = CreateCombinedTransactionResult(id: 1, amount: 25m, type: TransactionType.Deposit, fromAccountId: 1001);

            _transactionRepoMock
                .Setup(r => r.CreateTransactionResultAsync(It.IsAny<TransactionResultEntity>()))
                .Returns(Task.CompletedTask);

            _accountRepoMock
                .Setup(r => r.UpdateAccountAsync(It.IsAny<AccountEntity>()))
                .Returns(Task.CompletedTask);
            
            await _service.ProcessDepositAsync(account, transaction);
            
            Assert.Equal(75m, account.Balance);
            Assert.Equal(transaction.Id, account.LastProcessedTransactionId);

            _transactionRepoMock.Verify(r => r.CreateTransactionResultAsync(
                It.Is<TransactionResultEntity>(tr =>
                    tr.TransactionId == transaction.Id &&
                    tr.Status == TransactionStatus.Processed &&
                    tr.AccountId == account.IdAlias)),
                Times.Once);

            _accountRepoMock.Verify(r => r.UpdateAccountAsync(account), Times.Once);
        }

        [Fact]
        public async Task ProcessWithdrawalAsync_InsufficientBalance_CreatesFailedResult_AndDoesNotChangeBalance()
        {
            var account = CreateAccount(idAlias: 2001, balance: 50m);
            var transaction = CreateCombinedTransactionResult(id: 2, amount: 100m, type: TransactionType.Withdrawal, fromAccountId: 2001);

            _transactionRepoMock
                .Setup(r => r.CreateTransactionResultAsync(It.IsAny<TransactionResultEntity>()))
                .Returns(Task.CompletedTask);

            _accountRepoMock
                .Setup(r => r.UpdateAccountAsync(It.IsAny<AccountEntity>()))
                .Returns(Task.CompletedTask);
            
            await _service.ProcessWithdrawalAsync(account, transaction);
            
            Assert.Equal(50m, account.Balance);
            Assert.Equal(transaction.Id, account.LastProcessedTransactionId);

            _transactionRepoMock.Verify(r => r.CreateTransactionResultAsync(
                It.Is<TransactionResultEntity>(tr =>
                    tr.TransactionId == transaction.Id &&
                    tr.Status == TransactionStatus.Failed &&
                    tr.Message!.Contains("Insufficient balance") &&
                    tr.AccountId == account.IdAlias)),
                Times.Once);

            _accountRepoMock.Verify(r => r.UpdateAccountAsync(account), Times.Once);
        }

        [Fact]
        public async Task ProcessWithdrawalAsync_SufficientBalance_DecreasesBalance_AndCreatesProcessedResult()
        {
            var account = CreateAccount(idAlias: 2002, balance: 200m);
            var transaction = CreateCombinedTransactionResult(id: 3, amount: 75m, type: TransactionType.Withdrawal, fromAccountId: 2002);

            _transactionRepoMock
                .Setup(r => r.CreateTransactionResultAsync(It.IsAny<TransactionResultEntity>()))
                .Returns(Task.CompletedTask);

            _accountRepoMock
                .Setup(r => r.UpdateAccountAsync(It.IsAny<AccountEntity>()))
                .Returns(Task.CompletedTask);
            
            await _service.ProcessWithdrawalAsync(account, transaction);
            
            Assert.Equal(125m, account.Balance);
            Assert.Equal(transaction.Id, account.LastProcessedTransactionId);

            _transactionRepoMock.Verify(r => r.CreateTransactionResultAsync(
                It.Is<TransactionResultEntity>(tr =>
                    tr.TransactionId == transaction.Id &&
                    tr.Status == TransactionStatus.Processed &&
                    tr.AccountId == account.IdAlias)),
                Times.Once);

            _accountRepoMock.Verify(r => r.UpdateAccountAsync(account), Times.Once);
        }

        [Fact]
        public async Task ProcessTransactionAsync_InvalidTransfer_CreatesFailedResult_WhenAccountMissing()
        {
            var transaction = CreateCombinedTransactionResult(
                id: 10,
                amount: 50m,
                type: TransactionType.Transfer,
                fromAccountId: 3001,
                toAccountId: 3002);
            
            _accountRepoMock
                .Setup(r => r.GetAccountByIdAliasAsync(transaction.FromAccountId))
                .ReturnsAsync((AccountEntity)null);

            _accountRepoMock
                .Setup(r => r.GetAccountByIdAliasAsync(transaction.ToAccountId))
                .ReturnsAsync((AccountEntity)null);

            _transactionRepoMock
                .Setup(r => r.CreateTransactionResultAsync(It.IsAny<TransactionResultEntity>()))
                .Returns(Task.CompletedTask);
            
            await _service.ProcessTransactionAsync(transaction);
            
            _transactionRepoMock.Verify(r => r.CreateTransactionResultAsync(
                It.Is<TransactionResultEntity>(tr =>
                    tr.TransactionId == transaction.Id &&
                    tr.Status == TransactionStatus.Failed &&
                    tr.Message!.Contains("Invalid transfer"))),
                Times.Once);
            
            _transactionRepoMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ProcessTransactionAsync_InvalidDeposit_CreatesFailedResult_WhenFromAccountMissing()
        {
            var transaction = CreateCombinedTransactionResult(
                id: 11,
                amount: 25m,
                type: TransactionType.Deposit,
                fromAccountId: 4001);

            _accountRepoMock
                .Setup(r => r.GetAccountByIdAliasAsync(transaction.FromAccountId))
                .ReturnsAsync((AccountEntity)null);

            _transactionRepoMock
                .Setup(r => r.CreateTransactionResultAsync(It.IsAny<TransactionResultEntity>()))
                .Returns(Task.CompletedTask);
            
            await _service.ProcessTransactionAsync(transaction);
            
            _transactionRepoMock.Verify(r => r.CreateTransactionResultAsync(
                It.Is<TransactionResultEntity>(tr =>
                    tr.TransactionId == transaction.Id &&
                    tr.Status == TransactionStatus.Failed &&
                    tr.Message!.Contains("Invalid transaction"))),
                Times.Once);
        }

        [Fact]
        public async Task ProcessTransactionsPerAccountAsync_ProcessesTransactionsInOrder()
        {
            var account = CreateAccount(idAlias: 5001, balance: 100m, lastProcessedId: 0);

            var transaction1 = CreateCombinedTransactionResult(
                id: 1,
                amount: 50m,
                type: TransactionType.Deposit,
                fromAccountId: 5001);
            transaction1.CreatedAt = DateTime.UtcNow.AddMinutes(-10);

            var transaction2 = CreateCombinedTransactionResult(
                id: 2,
                amount: 30m,
                type: TransactionType.Withdrawal,
                fromAccountId: 5001);
            transaction2.CreatedAt = DateTime.UtcNow.AddMinutes(-5);

            _transactionRepoMock
                .Setup(r => r.GetTransactionResultCombinedByAccountIdAsync(
                    account.IdAlias,
                    account.LastProcessedTransactionId))
                .ReturnsAsync(new List<TransactionResultCombinedEntity> { transaction2, transaction1 }); 

            _transactionRepoMock
                .Setup(r => r.CreateTransactionResultAsync(It.IsAny<TransactionResultEntity>()))
                .Returns(Task.CompletedTask);

            _accountRepoMock
                .Setup(r => r.UpdateAccountAsync(It.IsAny<AccountEntity>()))
                .Returns(Task.CompletedTask);
            
            await _service.ProcessTransactionsPerAccountAsync(account);
            
            Assert.Equal(120m, account.Balance);
            Assert.Equal(2, account.LastProcessedTransactionId);

            _transactionRepoMock.Verify(r => r.CreateTransactionResultAsync(
                    It.IsAny<TransactionResultEntity>()),
                Times.Exactly(2));

            _accountRepoMock.Verify(r => r.UpdateAccountAsync(account), Times.AtLeast(2));
        }
    }
}
