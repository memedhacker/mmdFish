using System.Windows.Forms;

namespace Aether.Pages
{
    public partial class FishPuzzlePage : BaseBotPage
    {
        public FishPuzzlePage()
        {
            InitializeComponent();
        }

        protected override Label ClientNameLabel => clientNameLabel;
    }
}
