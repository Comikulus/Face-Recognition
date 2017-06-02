using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ContosoFestivalCheckIn
{
    public partial class Match : Form
    {
        public Match(string forename, string surname, string url, Image img)
        {
            InitializeComponent();
            label1.Text = forename + " " + surname;
            pictureBox2.Image = img;
            pictureBox1.Load(url);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
        }
    }
}
