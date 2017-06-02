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
using AForge;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Web;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Drawing;

namespace ContosoFestivalCheckIn
{
    public partial class CheckIn : Form
    {
        SqlConnection con;
        string containerName, path = String.Empty;
        static string APIKEY = "a76ffc6636cb4f95a8fa56c9db0dcbe9", faceListId = "facelistpdf";
        int ImgNum,userID;
        private FilterInfoCollection captureDevice;
        private VideoCaptureDevice FinalFrame;
        SqlCommand update;
        CloudBlobContainer container;
        public CheckIn(int id)
        {
            InitializeComponent();
            button1.Focus();
            userID = id;
            update = new SqlCommand("update CheckInUsers set ImgNum=@ImgNum where UserID=@UserID", con);
            con = new SqlConnection(@"Data Source=contosofestival.database.windows.net;Initial Catalog=ContosoFestival;Persist Security Info=True;User ID=.PDF;Password=AdyEndreinfo17");
            con.Open();
            SqlCommand cmd = new SqlCommand("select NameContainer from CheckInUsers where UserID=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            containerName = cmd.ExecuteScalar().ToString();
            containerName = containerName.Replace(" ", String.Empty);
            cmd.CommandText = "select ImgNum from CheckInUsers where UserID=@id";
            ImgNum = (int)cmd.ExecuteScalar();
            ImgNum++;
            con.Close();
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            container = blobClient.GetContainerReference(containerName);
        }

        private void CheckIn_Load(object sender, EventArgs e)
        {
            captureDevice = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo Device in captureDevice)
                comboBox1.Items.Add(Device.Name);
            comboBox1.SelectedIndex = 0;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            FinalFrame = new VideoCaptureDevice(captureDevice[comboBox1.SelectedIndex].MonikerString);
            FinalFrame.NewFrame += new NewFrameEventHandler(FinalFrame_NewFrame);
            FinalFrame.Start();
        }

        private void FinalFrame_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
        }

        private void CheckIn_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (FinalFrame.IsRunning.Equals(true))
                FinalFrame.Stop();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = pictureBox1.Image;
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(path))
            {
                using (var folderDialog = new FolderBrowserDialog())
                    if (folderDialog.ShowDialog() == DialogResult.OK)
                        path = folderDialog.SelectedPath;
            }
            if (pictureBox2.Image == null)
            {
                MessageBox.Show("Készítsen képet a személyről akit keres!", "Nincs kép!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                pictureBox2.Image.Save(path + "\\kep.jpeg", ImageFormat.Jpeg);
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(containerName + ImgNum);
                using (var fileStream = System.IO.File.OpenRead(path + "\\kep.jpeg"))
                {
                    blockBlob.UploadFromStream(fileStream);
                }
                Task<string> task = Detect(blockBlob.Uri.AbsoluteUri, APIKEY);
                string resultdetect = await task;
                if (String.Equals(resultdetect, "[]"))
                {
                    MessageBox.Show("A képen nincs arc.\nKérjük készítsen egy másikat!", "Hiba!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    string[] lines = Regex.Split(resultdetect, "\"");
                    task = FindSimilar(lines[3], faceListId, APIKEY);
                    string findsimilarresult = await task;
                    if (findsimilarresult == "[]")
                    {
                        MessageBox.Show("Nincs találat!\nKérjük készítsen másik képet vagy lépjen be jelszóval", "Hiba!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        lines = Regex.Split(findsimilarresult, "\"");
                        con.Open();
                        SqlCommand cmd = new SqlCommand("select Forename from Participants where persistedFaceId=@persfaceid", con);
                        cmd.Parameters.AddWithValue("@persfaceid", lines[3]);
                        string forename = Convert.ToString(cmd.ExecuteScalar());
                        cmd.CommandText = "select Surname from Participants where persistedFaceId=@persfaceid";
                        string surname = Convert.ToString(cmd.ExecuteScalar());
                        cmd.CommandText = "select ImgUrl from Participants where persistedFaceId=@persfaceid";
                        string url = Convert.ToString(cmd.ExecuteScalar());
                        forename = forename.Replace(" ", String.Empty);
                        surname = surname.Replace(" ", String.Empty);
                        con.Close();
                        Match match = new Match(forename, surname, url, pictureBox2.Image);
                        this.Hide();
                        match.ShowDialog();
                        this.Show();
                        pictureBox2.Image = null;
                        if (match.DialogResult == DialogResult.Yes)
                        {
                            con.Open();
                            cmd.CommandText = "update Participants set CheckedIn=@true where persistedFaceId=@persfaceid";
                            cmd.Parameters.AddWithValue("@true", true);
                            cmd.ExecuteNonQuery();
                            con.Close();
                            MessageBox.Show(forename + " " + surname + "\nmegérkezett", "Üzenet", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Készítsen egy másik képet vagy lépjen be manuálisan!", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                ImgNum++;
                con.Open();
                update.Parameters.Clear();
                update.Connection = con;
                update.Parameters.AddWithValue("@ImgNum", ImgNum);
                update.Parameters.AddWithValue("@UserID", userID);
                update.ExecuteNonQuery();
                con.Close();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult d = MessageBox.Show("Biztos ki akar lépni?", "Kérdés", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (d == DialogResult.Yes)
                this.Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            CheckInWithPassword checkinwithpassword = new CheckInWithPassword();
            this.Hide();
            checkinwithpassword.ShowDialog();
            this.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
                if (folderDialog.ShowDialog() == DialogResult.OK)
                    path = folderDialog.SelectedPath;
        }

        static async Task<string> Detect(string url, string key)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
            queryString["returnFaceId"] = "true";
            queryString["returnFaceLandmarks"] = "false";
            string uri = "https://westeurope.api.cognitive.microsoft.com/face/v1.0/detect?" + queryString;
            HttpResponseMessage response;
            byte[] byteData = Encoding.UTF8.GetBytes("{\"url\":\"" + url + "\"}");
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
                return response.Content.ReadAsStringAsync().Result;
            }

        }
        static async Task<string> FindSimilar(string faceId, string faceListId, string key)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
            var uri = "https://westeurope.api.cognitive.microsoft.com/face/v1.0/findsimilars?" + queryString;
            HttpResponseMessage response;
            byte[] byteData = Encoding.UTF8.GetBytes("{\"faceId\":\"" + faceId + "\",\"faceListId\":\"" + faceListId + "\",\"maxNumOfCandidatesReturned\":1,\"mode\":\"matchPerson\"}");
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
            }
            return response.Content.ReadAsStringAsync().Result;

        }
    }
}
