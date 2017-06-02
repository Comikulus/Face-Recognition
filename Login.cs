using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ContosoFestivalCheckIn
{
    public partial class Login : Form
    {
        SqlConnection con;
        public Login()
        {
            InitializeComponent();
            con = new SqlConnection(@"Data Source=contosofestival.database.windows.net;Initial Catalog=ContosoFestival;Persist Security Info=True;User ID=.PDF;Password=AdyEndreinfo17");
            button1.Focus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool login = true;
            label3.Text = String.Empty;
            label4.Text = String.Empty;
            if (String.IsNullOrWhiteSpace(textBox1.Text))
            {
                label3.Text = "Kérem adja meg a felhasználónevét!";
                login = false;
            }
            if (String.IsNullOrWhiteSpace(maskedTextBox1.Text))
            {
                label4.Text = "Kérem adja meg a jelszavát!";
                login = false;
            }
            if (login)
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("select count(*) from CheckInUsers where Username = @user", con);
                cmd.Parameters.AddWithValue("@user", textBox1.Text);
                int szam = (int)cmd.ExecuteScalar();
                if (szam == 0)
                {
                    label3.Text = "A felhasználónév nem létezik!";
                }
                else
                {
                    cmd.CommandText = "select Password from CheckInUsers where Username=@user";
                    string password = cmd.ExecuteScalar().ToString();
                    password = password.Replace(" ", String.Empty);
                    if (String.Equals(password, maskedTextBox1.Text))
                    {
                        cmd.CommandText = "select UserID from CheckInUsers where Username=@user";
                        int id = (int)cmd.ExecuteScalar();
                        CheckIn checkin = new CheckIn(id);
                        this.Hide();
                        checkin.ShowDialog();
                        this.Show();
                    }
                    else
                    {
                        label4.Text = "Hibás jelszó!";
                    }
                }
                con.Close();
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            label9.Text = String.Empty;
            label10.Text = String.Empty;
            label11.Text = String.Empty;
            label12.Text = String.Empty;
            bool register = true;
            if (String.IsNullOrWhiteSpace(textBox2.Text))
            {
                register = false;
                label9.Text = "Kérem adjon meg felhasználónevet!";
            }
            if (String.IsNullOrWhiteSpace(maskedTextBox2.Text))
            {
                register = false;
                label10.Text = "Kérem adjon meg jelszavat!";
            }
            if (String.IsNullOrWhiteSpace(maskedTextBox3.Text))
            {
                register = false;
                label11.Text = "Kérem erősítse meg a jelszavát!";
            }
            if (String.IsNullOrWhiteSpace(textBox3.Text))
            {
                register = false;
                label12.Text = "Kérem adjon a Containernek nevet!";
            }
            if (register)
            {
                if (String.Equals(maskedTextBox2.Text, maskedTextBox3.Text))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("select count(*) from CheckInUsers where Username=@username", con);
                    cmd.Parameters.AddWithValue("@username", textBox2.Text);
                    int number = (int)cmd.ExecuteScalar();
                    if (number == 0)
                    {
                        cmd.CommandText = "select count(*) from CheckInUsers where NameContainer=@container";
                        cmd.Parameters.AddWithValue("@container", textBox3.Text.ToLower());
                        number = (int)cmd.ExecuteScalar();
                        if (number == 0)
                        {
                            cmd.CommandText = "insert into CheckInUsers values(@username,@password,@container,@imgnum)";
                            cmd.Parameters.AddWithValue("@password", maskedTextBox2.Text);
                            cmd.Parameters.AddWithValue("@imgnum", 0);
                            cmd.ExecuteNonQuery();
                            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
                            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                            CloudBlobContainer container = blobClient.GetContainerReference(textBox3.Text.ToLower());
                            container.CreateIfNotExists();
                            BlobContainerPermissions permissions = container.GetPermissions();
                            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                            container.SetPermissions(permissions);
                            textBox1.Text = String.Empty;
                            textBox2.Text = String.Empty;
                            textBox3.Text = String.Empty;
                            maskedTextBox1.Text = String.Empty;
                            maskedTextBox2.Text = String.Empty;
                            maskedTextBox3.Text = String.Empty;
                            MessageBox.Show("Sikeres regisztráció!", "Üzenet", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            label12.Text = "A container már létezik!";
                        }
                    }
                    else
                    {
                        label9.Text = "A felhasználó már létezik!";
                    }
                    con.Close();
                }
                else
                {
                    label11.Text = "A jelszavak nem egyeznek!";
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult d = MessageBox.Show("Biztos ki akar lépni?", "Kérdés", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (d == DialogResult.Yes)
                this.Close();
        }

        private void Login_Load(object sender, EventArgs e)
        {

        }
    }
}
