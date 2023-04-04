using static System.Windows.Forms.AxHost;
using System.Configuration;
namespace Hedonist.WinForm {
    public partial class Form1 : Form {

        private string terminalId;
        public Form1() {
            InitializeComponent();
            terminalId = ConfigurationManager.AppSettings.Get("TerminalId");
        }
       
    }
}