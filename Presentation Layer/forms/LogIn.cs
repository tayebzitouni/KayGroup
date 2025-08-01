using System;
using System.Windows.Forms;
using DataAccessLayer;

namespace freelanceProject1.Presentation_Layer
{
    public partial class LogIn : Form
    {
        public LogIn()
        {
            InitializeComponent();
        }

        private void LogIn_Load(object sender, EventArgs e) { }

        private void guna2ContainerControl1_Click(object sender, EventArgs e) { }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e) { }

        private void label4_Click(object sender, EventArgs e) { }

        private void label1_Click(object sender, EventArgs e) { }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e) { }

        // Nouveau bouton de connexion (remplace les anciens boutons)
        private async void btnLogin_Click(object sender, EventArgs e)
        {

        }

        private void SetControlsEnabled(bool enabled)
        {
            guna2TextBox3.Enabled = enabled;
            guna2TextBox4.Enabled = enabled;
            // btnLogin.Enabled = enabled; // Remplace les anciens boutons
        }

        private async void guna2Button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(guna2TextBox4.Text))
            {
                MessageBox.Show("Please enter your email address", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                guna2TextBox4.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(guna2TextBox3.Text))
            {
                MessageBox.Show("Please enter your password", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                guna2TextBox3.Focus();
                return;
            }

            SetControlsEnabled(false);
            Cursor = Cursors.WaitCursor;

            try
            {
                bool loginSuccess = await DataAccessLayer.Controllers.UtilisateurController.Login
                    (guna2TextBox4.Text.Trim(), guna2TextBox3.Text);

                if (loginSuccess)
                {
                    var utilisateur =await BussinessAcesssLayer.UtilisatuerBussiness.GetUserByEmail(guna2TextBox4.Text.Trim());
                    BussinessAcesssLayer.UtilisatuerBussiness.setloginutilisateru(utilisateur);
                    this.Hide();
                    var dashboard = new DashBord();
                    dashboard.Show();
                }
                else
                {
                    MessageBox.Show("Invalid email or password", "Login Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    guna2TextBox3.SelectAll();
                    guna2TextBox3.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetControlsEnabled(true);
                Cursor = Cursors.Default;
            }
        }
    }
}
