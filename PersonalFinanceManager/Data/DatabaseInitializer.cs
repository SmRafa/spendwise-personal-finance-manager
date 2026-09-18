using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Models;

namespace PersonalFinanceManager.Data;

public static class DatabaseInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        context.Database.EnsureCreated();

        // The Accounts database already exists, so EnsureCreated cannot add new
        // tables to it. This small development bootstrap adds Categories safely.
        context.Database.ExecuteSqlRaw("""
            IF COL_LENGTH('Transactions', 'ReceiptPath') IS NULL ALTER TABLE [Transactions] ADD [ReceiptPath] nvarchar(300) NULL;
            """);
        context.Database.ExecuteSqlRaw("""
            IF COL_LENGTH('UserPreferences', 'ProfileImagePath') IS NULL ALTER TABLE [UserPreferences] ADD [ProfileImagePath] nvarchar(300) NULL;
            """);
        context.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'[UserPreferences]', N'U') IS NULL CREATE TABLE [UserPreferences] ([Id] int NOT NULL IDENTITY,[UserId] nvarchar(450) NOT NULL,[DisplayName] nvarchar(50) NOT NULL,[Currency] nvarchar(10) NOT NULL,[Theme] nvarchar(20) NOT NULL,[BudgetAlerts] bit NOT NULL,[GoalAlerts] bit NOT NULL,CONSTRAINT [PK_UserPreferences] PRIMARY KEY ([Id]),CONSTRAINT [UX_UserPreferences_UserId] UNIQUE ([UserId]));
            """);
        context.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'[FinancialGoals]', N'U') IS NULL CREATE TABLE [FinancialGoals] ([Id] int NOT NULL IDENTITY,[Name] nvarchar(80) NOT NULL,[TargetAmount] decimal(18,2) NOT NULL,[CurrentAmount] decimal(18,2) NOT NULL,[TargetDate] datetime2 NULL,[UserId] nvarchar(450) NULL,[CreatedAt] datetime2 NOT NULL,CONSTRAINT [PK_FinancialGoals] PRIMARY KEY ([Id]));
            """);
        context.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'[AccountTransfers]', N'U') IS NULL
            BEGIN
                CREATE TABLE [AccountTransfers] (
                    [Id] int NOT NULL IDENTITY,
                    [FromAccountId] int NOT NULL,
                    [ToAccountId] int NOT NULL,
                    [Amount] decimal(18,2) NOT NULL,
                    [TransferDate] datetime2 NOT NULL,
                    [Note] nvarchar(250) NULL,
                    [UserId] nvarchar(450) NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    CONSTRAINT [PK_AccountTransfers] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_AccountTransfers_FromAccount] FOREIGN KEY ([FromAccountId]) REFERENCES [Accounts] ([Id]) ON DELETE NO ACTION,
                    CONSTRAINT [FK_AccountTransfers_ToAccount] FOREIGN KEY ([ToAccountId]) REFERENCES [Accounts] ([Id]) ON DELETE NO ACTION
                );
            END
            """);

        context.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'[RecurringTransactions]', N'U') IS NULL
            BEGIN
                CREATE TABLE [RecurringTransactions] (
                    [Id] int NOT NULL IDENTITY,
                    [Type] int NOT NULL,
                    [Amount] decimal(18,2) NOT NULL,
                    [Note] nvarchar(250) NOT NULL,
                    [AccountId] int NOT NULL,
                    [CategoryId] int NOT NULL,
                    [NextDueDate] datetime2 NOT NULL,
                    [IsActive] bit NOT NULL,
                    [UserId] nvarchar(450) NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    CONSTRAINT [PK_RecurringTransactions] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_RecurringTransactions_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]) ON DELETE NO ACTION,
                    CONSTRAINT [FK_RecurringTransactions_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
                );
                CREATE INDEX [IX_RecurringTransactions_AccountId] ON [RecurringTransactions] ([AccountId]);
                CREATE INDEX [IX_RecurringTransactions_CategoryId] ON [RecurringTransactions] ([CategoryId]);
            END
            """);

        context.Database.ExecuteSqlRaw("""
            IF COL_LENGTH('Accounts', 'UserId') IS NULL ALTER TABLE [Accounts] ADD [UserId] nvarchar(450) NULL;
            IF COL_LENGTH('Categories', 'UserId') IS NULL ALTER TABLE [Categories] ADD [UserId] nvarchar(450) NULL;
            IF COL_LENGTH('Transactions', 'UserId') IS NULL ALTER TABLE [Transactions] ADD [UserId] nvarchar(450) NULL;
            IF COL_LENGTH('Budgets', 'UserId') IS NULL ALTER TABLE [Budgets] ADD [UserId] nvarchar(450) NULL;
            """);

        context.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'[Categories]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Categories] (
                    [Id] int NOT NULL IDENTITY,
                    [Name] nvarchar(50) NOT NULL,
                    [Type] int NOT NULL,
                    [Color] nvarchar(7) NOT NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [UserId] nvarchar(450) NULL,
                    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
                );
            END
            """);

        context.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'[Transactions]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Transactions] (
                    [Id] int NOT NULL IDENTITY,
                    [Type] int NOT NULL,
                    [Amount] decimal(18,2) NOT NULL,
                    [TransactionDate] datetime2 NOT NULL,
                    [Note] nvarchar(250) NULL,
                    [AccountId] int NOT NULL,
                    [CategoryId] int NOT NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [UserId] nvarchar(450) NULL,
                    CONSTRAINT [PK_Transactions] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Transactions_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]) ON DELETE NO ACTION,
                    CONSTRAINT [FK_Transactions_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
                );
                CREATE INDEX [IX_Transactions_AccountId] ON [Transactions] ([AccountId]);
                CREATE INDEX [IX_Transactions_CategoryId] ON [Transactions] ([CategoryId]);
            END
            """);

        context.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'[Budgets]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Budgets] (
                    [Id] int NOT NULL IDENTITY,
                    [CategoryId] int NOT NULL,
                    [Amount] decimal(18,2) NOT NULL,
                    [PeriodStart] datetime2 NOT NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [UserId] nvarchar(450) NULL,
                    CONSTRAINT [PK_Budgets] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Budgets_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
                );
                CREATE UNIQUE INDEX [IX_Budgets_CategoryId_PeriodStart] ON [Budgets] ([CategoryId], [PeriodStart]);
            END
            """);

        if (context.Categories.Any()) return;

        context.Categories.AddRange(
            new Category { Name = "Salary", Type = CategoryType.Income, Color = "#16A34A" },
            new Category { Name = "Freelance", Type = CategoryType.Income, Color = "#0891B2" },
            new Category { Name = "Other income", Type = CategoryType.Income, Color = "#7C3AED" },
            new Category { Name = "Food & dining", Type = CategoryType.Expense, Color = "#F97316" },
            new Category { Name = "Transport", Type = CategoryType.Expense, Color = "#0284C7" },
            new Category { Name = "Bills & utilities", Type = CategoryType.Expense, Color = "#8B5CF6" },
            new Category { Name = "Shopping", Type = CategoryType.Expense, Color = "#EC4899" },
            new Category { Name = "Health", Type = CategoryType.Expense, Color = "#EF4444" },
            new Category { Name = "Entertainment", Type = CategoryType.Expense, Color = "#EAB308" }
        );
        context.SaveChanges();
    }
}
