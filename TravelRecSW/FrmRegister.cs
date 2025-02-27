using System;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TravelRecSW
{
    public partial class FrmRegister : Form
    {
        //สร้างตัวแปรเก็บรูปที่ผู้ใช้งานเลือกในรูปแบบ byte[] เพื่อจะเก็บใน DB แบบ Image
        byte[] travellerImage;
        public FrmRegister()
        {
            InitializeComponent();
        }

        private void tbTravellerPassword_Enter(object sender, EventArgs e)
        {
            //แสดง Tooltip เมื่อผู้ใช้งานคลิกลงไปในช่อง Textbox
            TextBox tb = (TextBox)sender;
            int showTooltipTime = 3000; //หน่วยเป็น millisecond

            ToolTip tt = new ToolTip();
            tt.Show("รหัสผ่านต้อง 6 ตัวขึ้นไป",tb, 20, 20, showTooltipTime);
        }

        private void tbTravellerPasswordConfirm_Enter(object sender, EventArgs e)
        {
            //แสดง Tooltip เมื่อผู้ใช้งานคลิกลงไปในช่อง Textbox
            TextBox tb = (TextBox)sender;
            int showTooltipTime = 3000; //หน่วยเป็น millisecond

            ToolTip tt = new ToolTip();
            tt.Show("รหัสผ่านต้อง 6 ตัวขึ้นไป",tb, 20, 20, showTooltipTime);
        }

        private void btSelectTravellerImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files(*.jpg; *.jpeg *.png)|*.jpg; *.jpeg; *.png";

            if(ofd.ShowDialog() == DialogResult.OK)
            {
                //เอารูปที่เลือกไปแสดงที่ pbTavellerImage
                pbTravellerImage.Image = Image.FromFile(ofd.FileName);

                //แปลงรูปที่เลือกเป็น byte[] เก็บในตัวแปร travellerImage ที่สร้างไว้
                //สร้างตัวแปรเก็บประเภทไฟล์
                string extFile = Path.GetExtension(ofd.FileName);
                using (MemoryStream ms = new MemoryStream())
                {
                    if (extFile == "jpg" || extFile == "jpeg")
                    {
                        pbTravellerImage.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    else
                    {
                        pbTravellerImage.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    travellerImage = ms.ToArray();
                }
            }
        }

        private void tsbtSave_Click(object sender, EventArgs e)
        {
            if (tbTravellerFullname.Text.Trim().Length == 0 )
            {
                ShareInfo.showWarningMSG("ป้อนชิ้อด้วย");
            }
            else if (tbTravellerEmail.Text.Trim().Length == 0 )
            {
                ShareInfo.showWarningMSG("ป้อนอีเมลด้วย");
            }
            else if (!tbTravellerEmail.Text.Trim().Contains("@"))
            {
                ShareInfo.showWarningMSG("ต้องมี @ ด้วย");
            }
            else if (tbTravellerPassword.Text.Trim().Length == 0)
            {
                ShareInfo.showWarningMSG("ป้อนรหัสผ่านด้วย");
            }
            else if (tbTravellerPassword.Text.Trim().Length < 6)
            {
                ShareInfo.showWarningMSG("ป้อนรหัสผ่านต้องมากกว่า 6 ตัวอักษรขึ้นไป");
            }
            else if (tbTravellerPassword.Text.Trim() != tbTravellerPasswordConfirm.Text.Trim())
            {
                ShareInfo.showWarningMSG("รหัสกับการยืนยันรหัสต้องตรงกัน");
            }
            else if (travellerImage == null) 
            {
                ShareInfo.showWarningMSG("เลือกรูปด้วย");
            }
            else if (cbConfirm.Checked == false) 
            {
                ShareInfo.showWarningMSG("กรุณายืนยันการลงทะเบียนด้วย");
            }
            else
            {
                //ติดต่อ DB
                SqlConnection conn = new SqlConnection(ShareInfo.connStr);
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                conn.Open();

                //คำสั่ง SQL
                string strSql = "INSERT INTO traveller_tb" +
                                "(travellerFullname, travellerEmail, travellerPassword, travellerImage)" +
                                "VALUES" +
                                "(@travellerFullname, @travellerEmail, @travellerPassword, @travellerImage)";

                //สร้าง SQL Trasaction และ SQL Command เพื่อทำงานกับค่ำสั่ง SQL
                SqlTransaction sqlTransaction = conn.BeginTransaction();
                SqlCommand sqlCommand = new SqlCommand();
                sqlCommand.Connection = conn;
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandText = strSql;
                sqlCommand.Transaction = sqlTransaction;

                sqlCommand.Parameters.AddWithValue("@travellerFullname", tbTravellerFullname.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@travellerEmail", tbTravellerEmail.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@travellerPassword", tbTravellerPassword.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@travellerImage",travellerImage);

                //สั่งให้ SQL ทำงาน
                try
                {
                    sqlCommand.ExecuteNonQuery();
                    sqlTransaction.Commit();
                    conn.Close();

                    MessageBox.Show("ลงทะเบียนสำเร็จ","ผลการทำงาน",MessageBoxButtons.OK,MessageBoxIcon.Information);
                    FrmLogin frmLogin = new FrmLogin();
                    frmLogin.Show();
                    Hide();
                }
                catch (Exception ex) 
                {
                    sqlTransaction.Rollback();
                    conn.Close();
                    ShareInfo.showWarningMSG("มีข้อผิดพลาดเกิดขึ้น กรุณาลองใหม่อีกครั้งหรือติดต่อ IT " + ex.Message);
                }
            }
        }

        private void tsbtCancel_Click(object sender, EventArgs e)
        {
            pbTravellerImage.Image = Properties.Resources.profile;
            travellerImage = null;

            tbTravellerFullname.Clear();
            tbTravellerEmail.Clear();
            tbTravellerPassword.Clear();
            tbTravellerPasswordConfirm.Clear();
            cbConfirm.Checked = false;
        }

        private void tsbtToFrmLogin_Click(object sender, EventArgs e)
        {
            FrmLogin frmLogin = new FrmLogin();
            frmLogin.Show();
            Hide();
        }
    }
}
