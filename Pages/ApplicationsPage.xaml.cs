using sberbank.Services;
using System.Windows.Controls;

namespace sberbank.Pages
{
    public partial class ApplicationsPage : Page
    {
        private readonly DatabaseService _database = new DatabaseService();

        public ApplicationsPage()
        {
            InitializeComponent();
            if (SessionService.CurrentUser != null)
            {
                ApplicationsDataGrid.ItemsSource = _database.GetApplicationsForUser(SessionService.CurrentUser.UserId);
            }
        }
    }
}
