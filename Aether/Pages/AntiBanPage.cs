using System.Windows.Forms;

namespace Aether.Pages
{
    public partial class AntiBanPage : BaseBotPage
    {
        public AntiBanPage()
        {
            InitializeComponent();
        }

        protected override Label ClientNameLabel => clientNameLabel;
    }
}
