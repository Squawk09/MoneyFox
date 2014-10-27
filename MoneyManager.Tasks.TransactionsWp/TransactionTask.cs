﻿using Windows.ApplicationModel.Background;
using MoneyManager.Business.Src;

namespace MoneyManager.Tasks.TransactionsWp
{
    public sealed class TransactionTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            //TODO: Include the view model locator from the mainproject
            RecurringTransactionLogic.CheckRecurringTransactions();
        }
    }
}