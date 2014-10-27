﻿using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using GalaSoft.MvvmLight;
using Microsoft.Practices.ServiceLocation;
using MoneyManager.DataAccess.DataAccess;
using MoneyManager.DataAccess.Model;
using PropertyChanged;

namespace MoneyManager.Business.ViewModels
{
    [ImplementPropertyChanged]
    public class TotalBalanceViewModel : ViewModelBase
    {
        public ObservableCollection<Account> AllAccounts
        {
            get { return ServiceLocator.Current.GetInstance<AccountDataAccess>().AllAccounts; }
        }

        public double TotalBalance { get; set; }

        public string CurrencyCulture
        {
            get { return CultureInfo.CurrentCulture.Name; }
        }

        public void UpdateBalance()
        {
            TotalBalance = AllAccounts != null
                ? AllAccounts.Sum(x => x.CurrentBalance)
                : 0;
        }
    }
}