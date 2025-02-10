using System;
using System.Windows.Forms;
using System.Drawing;

namespace AGTCore
{
    class Menu : Form
    {
        TextBox usernameTextBox;
        TextBox ipAddressTextBox;
        Button connectButton;
        Label errorLabel;

        public Menu()
        {
            this.Text = "Game Login";
            this.BackColor = Color.FromArgb(51, 51, 51);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 12.0f, FontStyle.Regular);
            this.ClientSize = new Size(400, 200);

            Label usernameLabel = new Label();
            usernameLabel.Text = "Username:";
            usernameLabel.Location = new Point(20, 20);
            usernameLabel.AutoSize = true;
            this.Controls.Add(usernameLabel);

            usernameTextBox = new TextBox();
            usernameTextBox.Location = new Point(120, 20);
            usernameTextBox.BackColor = Color.FromArgb(51, 51, 51);
            usernameTextBox.Size = new Size(200, 20);
            this.Controls.Add(usernameTextBox);

            Label ipAddressLabel = new Label();
            ipAddressLabel.Text = "IP Address:";
            ipAddressLabel.Location = new Point(20, 50);
            ipAddressLabel.AutoSize = true;
            this.Controls.Add(ipAddressLabel);

            ipAddressTextBox = new TextBox();
            ipAddressTextBox.Location = new Point(120, 50);
            ipAddressTextBox.BackColor = Color.FromArgb(51, 51, 51);
            ipAddressTextBox.Size = new Size(200, 20);
            this.Controls.Add(ipAddressTextBox);

            connectButton = new Button();
            connectButton.Text = "Connect";
            connectButton.Location = new Point(120, 80);
            connectButton.Click += new EventHandler(ConnectButton_Click);
            this.Controls.Add(connectButton);

            errorLabel = new Label();
            errorLabel.Location = new Point(20, 120);
            errorLabel.BackColor = Color.FromArgb(51, 51, 51);
            errorLabel.Size = new Size(360, 50);
            this.Controls.Add(errorLabel);

            NetworkManager.client.ConnectionFailed += FailedToConnect;
            NetworkManager.client.Connected += Connected;
        }
        void ConnectButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Attempts to connect to server
                if (!Program.networkManager.Connect(ipAddressTextBox.Text, usernameTextBox.Text))
                    errorLabel.Text = "Invalid Address";
                else
                    errorLabel.Text = "Connecting...";
            }
            catch (Exception ex)
            {
                // Display error message in errorTextBox
                errorLabel.Text = ex.Message;
            }
        }

        public void CloseForm()
        {
            this.Close();
        }


        private void FailedToConnect(object sender, EventArgs e)
        {
            errorLabel.Text = "Failed to connect";
        }
        private void Connected(object sender, EventArgs e)
        {
            errorLabel.Text = "Registering Username...";
        }

    }
}
