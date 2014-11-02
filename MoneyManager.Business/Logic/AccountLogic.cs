﻿using System;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using MoneyManager.Business.Helper;
using MoneyManager.Business.ViewModels;
using MoneyManager.DataAccess.DataAccess;
using MoneyManager.DataAccess.Model;
using MoneyManager.Foundation;

namespace MoneyManager.Business.Logic
{
    public class AccountLogic
    {
        #region Properties

        private static AddAccountViewModel addAccountView
        {
            get { return ServiceLocator.Current.GetInstance<AddAccountViewModel>(); }
        }

        private static AccountDataAccess accountData
        {
            get { return ServiceLocator.Current.GetInstance<AccountDataAccess>(); }
        }

        private static TransactionDataAccess transactionData
        {
            get { return ServiceLocator.Current.GetInstance<TransactionDataAccess>(); }
        }

        private static TransactionListViewModel TransactionListView
        {
            get { return ServiceLocator.Current.GetInstance<TransactionListViewModel>(); }
        }

        #endregion Properties

        public static void GoToAddAccount()
        {
            accountData.SelectedAccount = new Account
            {
                IsExchangeModeActive = false
            };
            ServiceLocator.Current.GetInstance<AddAccountViewModel>().IsEdit = false;
        }

        public static async void DeleteAccount(Account account)
        {
            if (await Utilities.IsDeletionConfirmed())
            {
                accountData.Delete(account);
                TransactionLogic.DeleteAssociatedTransactionsFromDatabase(account.Id);
            }
        }

        public static void RefreshRelatedTransactions(FinancialTransaction transaction)
        {
            if (accountData.SelectedAccount == transaction.ChargedAccount)
            {
                TransactionListView.SetRelatedTransactions(transaction.ChargedAccountId);
            }
        }

        public static void RemoveTransactionAmount(FinancialTransaction transaction)
        {
            if (transaction.Cleared)
            {
                PrehandleRemoveIfTransfer(transaction);

                Func<double, double> amountFunc = x =>
                    transaction.Type == (int) TransactionType.Income
                        ? -x
                        : x;
                
                HandleTransactionAmount(transaction, amountFunc, GetChargedAccountFunc());
            }
        }

        public static void AddTransactionAmount(FinancialTransaction transaction)
        {
            PrehandleAddIfTransfer(transaction);

            Func<double, double> amountFunc = x =>
                transaction.Type == (int)TransactionType.Income
                    ? x
                    : -x;

            HandleTransactionAmount(transaction, amountFunc, GetChargedAccountFunc());
        }

        private static void PrehandleRemoveIfTransfer(FinancialTransaction transaction)
        {
            if (transaction.Type == (int)TransactionType.Transfer)
            {
                Func<double, double> amountFunc = x => -x;
                HandleTransactionAmount(transaction, amountFunc, GetTargetAccountFunc());
            }
        }

        private static void HandleTransactionAmount(FinancialTransaction transaction, Func<double, double> amountFunc,
            Func<FinancialTransaction, Account> getAccountFunc)
        {
            if (transaction.ClearTransactionNow)
            {
                Account account = getAccountFunc(transaction);
                if (account == null) return;

                double amount = amountFunc(transaction.Amount);

                account.CurrentBalance += amount;
                transaction.Cleared = true;

                accountData.Update(account);
                transactionData.Update(transaction);
            }
        }

        private static void PrehandleAddIfTransfer(FinancialTransaction transaction)
        {
            if (transaction.Type == (int)TransactionType.Transfer)
            {
                Func<double, double> amountFunc = x => x;
                HandleTransactionAmount(transaction, amountFunc, GetTargetAccountFunc());
            }
        }

        private static Func<FinancialTransaction, Account> GetTargetAccountFunc()
        {
            Func<FinancialTransaction, Account> targetAccountFunc =
                trans => accountData.AllAccounts.FirstOrDefault(x => x.Id == trans.TargetAccountId);
            return targetAccountFunc;
        }

        private static Func<FinancialTransaction, Account> GetChargedAccountFunc()
        {
            Func<FinancialTransaction, Account> accountFunc =
                trans => accountData.AllAccounts.FirstOrDefault(x => x.Id == trans.ChargedAccountId);
            return accountFunc;
        }
    }
}