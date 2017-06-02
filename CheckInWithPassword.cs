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

namespace ContosoFestivalCheckIn
{
    public partial class CheckInWithPassword : Form
    {
        SqlDataAdapter da;
        SqlConnection con;
        DataTable dt;
        int participantid;
        public CheckInWithPassword()
        {
            InitializeComponent();
            con = new SqlConnection(@"Data Source=contosofestival.database.windows.net;Initial Catalog=ContosoFestival;Persist Security Info=True;User ID=user;Password=password");
            DataGridViewButtonColumn btn = new DataGridViewButtonColumn();
            btn.HeaderText = "Kép betöltése";
            dataGridView1.Columns.Add(btn);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textBox1.Text) || String.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Kérem töltse ki az összes mezőt!", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                dt = new DataTable();
                da = new SqlDataAdapter("select ParticipantID,Surname,Forename,Birth,ImgUrl from Participants where (Surname=@surname and Forename=@forename)", con);
                da.SelectCommand.Parameters.AddWithValue("@surname", textBox1.Text);
                da.SelectCommand.Parameters.AddWithValue("@forename", textBox2.Text);
                da.Fill(dt);
                dataGridView1.DataSource = dt;
                dataGridView1.Refresh();

            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;
            if (dataGridView1[e.ColumnIndex, e.RowIndex] is DataGridViewButtonCell)
            {

                    button2.Enabled = true;
                    pictureBox1.Load(dataGridView1[5, e.RowIndex].Value.ToString());
                    participantid = Convert.ToInt32(dataGridView1[1, e.RowIndex].Value);
                
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            con.Open();
            SqlCommand cmd = new SqlCommand("update Participants set CheckedIn=@true where ParticipantID=@participantid", con);
            cmd.Parameters.AddWithValue("@true", true);
            cmd.Parameters.AddWithValue("@participantid", participantid);
            cmd.ExecuteNonQuery();
            con.Close();
            MessageBox.Show("A személy megérkezettnek lett jelölve", "Üzenet", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult d = MessageBox.Show("Biztos ki akar lépni?", "Kérdés", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (d == DialogResult.Yes)
                this.Close();
        }
    }
}
