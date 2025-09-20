using System.Windows;

namespace DataQuillDesktop
{
    public partial class TestWindow : Window
    {
        public TestWindow()
        {
            InitializeComponent();
            this.Show();
            this.Activate();
            this.Focus();
        }
    }
}