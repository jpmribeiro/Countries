namespace Countries.Services
{
    using System.Windows;

    public class DialogService
    {
        public void ShowMessage(string title, string message)
        {
            MessageBox.Show(message, title);
        }
    }
}
