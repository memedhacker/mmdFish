using System.Windows.Forms;

namespace Aether.Pages
{
    public partial class UpgradePage : BaseBotPage
    {
        public UpgradePage()
        {
            InitializeComponent();
        }

        protected override Label ClientNameLabel => clientNameLabel;
    }
}
