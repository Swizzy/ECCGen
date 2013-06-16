using System.Windows.Forms;

namespace ECCGenGUI
{
    internal sealed partial class MainForm : Form
    {
        internal MainForm()
        {
            InitializeComponent();
            Text = ECCGen.ECCGenerator.Version;
        }
    }
}
