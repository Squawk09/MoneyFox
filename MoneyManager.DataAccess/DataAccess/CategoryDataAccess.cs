using System.Collections.ObjectModel;
using System.Linq;
using Windows.Storage;
using MoneyManager.DataAccess.Model;
using MoneyManager.Foundation;
using PropertyChanged;

namespace MoneyManager.DataAccess.DataAccess
{
    [ImplementPropertyChanged]
    public class CategoryDataAccess : AbstractDataAccess<Category>
    {
        public ObservableCollection<Category> AllCategories { get; set; }

        public Category SelectedCategory { get; set; }

        protected override void SaveToDb(Category category)
        {
            using (var dbConn = SqlConnectionFactory.GetSqlConnection())
            {
                if (AllCategories == null)
                {
                    LoadList();
                }

                AllCategories.Add(category);
                category.Id = dbConn.Insert(category);
            }
        }

        protected override void DeleteFromDatabase(Category category)
        {
            using (var dbConn = SqlConnectionFactory.GetSqlConnection())
            {
                AllCategories.Remove(category);
                dbConn.Delete(category);
            }
        }

        protected override void GetListFromDb()
        {
            using (var dbConn = SqlConnectionFactory.GetSqlConnection())
            {
                AllCategories = new ObservableCollection<Category>(dbConn.Table<Category>().ToList());
            }
        }

        protected override void UpdateItem(Category category)
        {
            using (var dbConn = SqlConnectionFactory.GetSqlConnection())
            {
                dbConn.Update(category, typeof (Category));
            }
        }
    }
}