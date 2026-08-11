using System.Windows.Forms;

namespace Aether.Pages
{
    public partial class HomePage : BaseBotPage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        protected override Label ClientNameLabel => clientNameLabel;
    }
}
