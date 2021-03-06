﻿using System;
using System.Collections.Generic;
using System.Linq;
using MoneyFox.Foundation;
using MoneyFox.Foundation.DataModels;
using MoneyFox.Foundation.Interfaces.Repositories;
using MoneyFox.Foundation.Models;

namespace MoneyFox.Business.StatisticDataProvider
{
    public class CategorySpreadingDataProvider
    {
        private readonly IRepository<PaymentViewModel> paymentRepository;

        public CategorySpreadingDataProvider(IRepository<PaymentViewModel> paymentRepository)
        {
            this.paymentRepository = paymentRepository;
        }

        /// <summary>
        ///     Selects payments from the given timeframe and calculates the spreading for the six categories
        ///     with the highest spendings. All others are summarized in a "other" item.
        /// </summary>
        /// <param name="startDate">Startpoint form which to select data.</param>
        /// <param name="endDate">Endpoint form which to select data.</param>
        /// <returns>Statistic value for the given time. </returns>
        public IEnumerable<StatisticItem> GetValues(DateTime startDate, DateTime endDate)
            => GetSpreadingStatisticItems(paymentRepository
                .GetList(x => (x.Date.Date >= startDate.Date) && (x.Date.Date <= endDate.Date)
                              && ((x.Type == (int) PaymentType.Expense) || (x.Type == PaymentType.Income)))
                .ToList());

        private List<StatisticItem> GetSpreadingStatisticItems(List<PaymentViewModel> payments)
        {
            var tempStatisticList = (from payment in payments
                    group payment by new
                    {
                        category = payment.Category != null ? payment.Category.Name : string.Empty
                    }
                    into temp
                    select new StatisticItem
                    {
                        Label = temp.Key.category,
                        // we subtract income payments here so that we have all expenses without presign
                        Value = temp.Sum(x => x.Type == PaymentType.Income ? -x.Amount : x.Amount)
                    })
                .Where(x => x.Value > 0)
                .OrderByDescending(x => x.Value)
                .ToList();

            var statisticList = tempStatisticList.Take(6).ToList();

            AddOtherItem(tempStatisticList, statisticList);
            SetLabel(statisticList);

            return statisticList;
        }

        private static void SetLabel(List<StatisticItem> statisticList)
        {
            var totAmount = statisticList.Sum(x => x.Value);
            foreach (var statisticItem in statisticList)
            {
                statisticItem.Label = statisticItem.Label
                                      + ": "
                                      + statisticItem.Value.ToString("C")
                                      + " ("
                                      + Math.Round(statisticItem.Value/totAmount*100, 2)
                                      + "%)";
            }
        }

        private void AddOtherItem(IEnumerable<StatisticItem> tempStatisticList,
            ICollection<StatisticItem> statisticList)
        {
            if (statisticList.Count < 6)
            {
                return;
            }

            var othersItem = new StatisticItem
            {
                Label = "Others",
                Value = tempStatisticList
                    .Where(x => !statisticList.Contains(x))
                    .Sum(x => x.Value)
            };

            othersItem.Label = othersItem.Label + ": " + othersItem.Value;

            if (othersItem.Value > 0)
            {
                statisticList.Add(othersItem);
            }
        }
    }
}