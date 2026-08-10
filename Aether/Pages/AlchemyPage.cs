using System.Windows.Forms;

namespace Aether.Pages
{
    public partial class AlchemyPage : BaseBotPage
    {
        public AlchemyPage()
        {
            InitializeComponent();
        }

        protected override Label ClientNameLabel => clientNameLabel;
    }
}
