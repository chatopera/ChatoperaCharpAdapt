using System;
using System.Windows.Forms;
using ChatoperaCSharpAdapt;

namespace ChatoperaCharpAdaptTest
{
    public partial class ChatoperaCharpAdaptTest : Form
    {
        ChatoperaClient client;
 
        //
        string clientid = "60000000000000000000000000";
        string secretkey = "*************************";

        string userID;

        public ChatoperaCharpAdaptTest()
        {
            InitializeComponent(); 
        }

        private void ChatoperaCharpAdaptTest_Load(object sender, EventArgs e)
        {
            client = new ChatoperaClient();
            userID = client.RandString(12);
            client.init("https://bot.chatopera.com", clientid, secretkey ); 
        }

        private void send_Btn_Click(object sender, EventArgs e)
        {
            string inputStr = "";
            string recvStr = "";
            inputStr = textBox1.Text;
            recvStr = client.conversation(userID, inputStr);

            ListViewItem item = new ListViewItem();
            item.SubItems.Clear();

            listBox1.Items.Add(recvStr);
        }

    }
}
