using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TravelRecSW
{
    public partial class FrmTravelEdit : Form
    {
        private int travelId;
        public FrmTravelEdit(int travelId)
        {
            InitializeComponent();
            this.travelId = travelId;

        }
        public void getTravelForEdit()
        {
            //เอา travelId ไปค้นหาแล้วดึงข้อมูลจาก DB มาแสดง
            //ติดต่อ DB
            SqlConnection conn = new SqlConnection(ShareInfo.connStr);
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
            conn.Open();

            string strSql = "SELECT * FROM travel_tb WHERE travelId = @travelId";

            //สร้าง SQL Trasaction และ SQL Command เพื่อทำงานกับค่ำสั่ง SQL
            SqlTransaction sqlTransaction = conn.BeginTransaction();
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Connection = conn;
            sqlCommand.CommandType = CommandType.Text;
            sqlCommand.CommandText = strSql;
            sqlCommand.Transaction = sqlTransaction;

            sqlCommand.Parameters.AddWithValue("@travelId", travelId);

            //สั่งให้ SQL ทำงาน
            SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
            conn.Close();

            DataTable dt = new DataTable();
            adapter.Fill(dt);

            //เอาไปกำหนดให้กับส่วนของหน้าจอ
            tbTravelPlace.Text = dt.Rows[0]["travelPlace"].ToString();
            dtpTravelStartDate.Value = (DateTime)dt.Rows[0]["travelStartDate"];
            dtpTravelEndDate.Value = (DateTime)dt.Rows[0]["travelEndDate"];
            tbTravelCostTotal.Text = dt.Rows[0]["travelCostTotal"].ToString();
            //เอารูปมา traveller มาแสดง
            using (MemoryStream ms = new MemoryStream((byte[])dt.Rows[0]["travelImage"]))
            {
                pbTravelImage.Image = Image.FromStream(ms);
            }
        }

        private void FrmTravelEdit_Load(object sender, EventArgs e)
        {
            getTravelForEdit();
        }

        private void tsbtSave_Click(object sender, EventArgs e)
        {
            if (tbTravelPlace.Text.Trim().Length == 0)
            {
                ShareInfo.showWarningMSG("ป้อนชื่อสถานที่ที่จะเดินทางด้วย");
            }
            else if (dtpTravelEndDate.Value < dtpTravelStartDate.Value)
            {
                ShareInfo.showWarningMSG("วันที่กลับควรจะหลังหรือวันเดียวกับวันที่ไป");
            }
            if (tbTravelCostTotal.Text.Trim().Length == 0)
            {
                ShareInfo.showWarningMSG("ป้อนค่าใช้จ่ายด้วย");
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
                string strSql = "";
                //ตรวจสอบว่าได้แก้ไขรูปไหม
                if(travelImage == null)
                {
                    strSql = "UPDATE travel_tb SET travelPlace=@travelPlace, travelStartDate=@travelStartDate, travelEndDate=@travelEndDate, " +
                             "travelCostTotal=@travelCostTotal WHERE travelId=@travelId";
                }
                else
                {
                    strSql = "UPDATE travel_tb SET travelPlace=@travelPlace, travelStartDate=@travelStartDate, travelEndDate=@travelEndDate, " +
                             "travelCostTotal=@travelCostTotal, travelImage=@travelImage WHERE travelId=@travelId";
                }

                //สร้าง SQL Trasaction และ SQL Command เพื่อทำงานกับค่ำสั่ง SQL
                SqlTransaction sqlTransaction = conn.BeginTransaction();
                SqlCommand sqlCommand = new SqlCommand();
                sqlCommand.Connection = conn;
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandText = strSql;
                sqlCommand.Transaction = sqlTransaction;

                //Bind Param
                sqlCommand.Parameters.AddWithValue("@travelPlace", tbTravelPlace.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@travelStartDate", dtpTravelStartDate.Value.Date);
                sqlCommand.Parameters.AddWithValue("@travelEndDate", dtpTravelEndDate.Value.Date);
                sqlCommand.Parameters.AddWithValue("@travelCostTotal", decimal.Parse(tbTravelCostTotal.Text.Trim()));
                if (travelImage != null)
                {
                    sqlCommand.Parameters.AddWithValue("@travelImage", travelImage);
                }
                sqlCommand.Parameters.AddWithValue("@travelId", travelId);

                //สั่งให้ SQL ทำงาน
                try
                {
                    sqlCommand.ExecuteNonQuery();
                    sqlTransaction.Commit();
                    conn.Close();

                    MessageBox.Show("บันทึกการเดินทางสำเร็จ", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Dispose(); //ปิด Dialog เพราะเปิดแบบ showDialog()
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
            getTravelForEdit();
            travelImage = null;
            
        }
        //สร้างตัวแปรเก็บรูปที่ผู้ใช้งานเลือกในรูปแบบ byte[] เพื่อจะเก็บใน DB แบบ Image
        byte[] travelImage;
        private void btSelectTravelImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files(*.jpg; *.jpeg *.png)|*.jpg; *.jpeg; *.png";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //เอารูปที่เลือกไปแสดงที่ pbTavelImage
                pbTravelImage.Image = Image.FromFile(ofd.FileName);

                //แปลงรูปที่เลือกเป็น byte[] เก็บในตัวแปร travelImage ที่สร้างไว้
                //สร้างตัวแปรเก็บประเภทไฟล์
                string extFile = Path.GetExtension(ofd.FileName);
                using (MemoryStream ms = new MemoryStream())
                {
                    if (extFile == "jpg" || extFile == "jpeg")
                    {
                        pbTravelImage.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    else
                    {
                        pbTravelImage.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    travelImage = ms.ToArray();
                }
            }
        }

        private void tbTravelCostTotal_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsControl(e.KeyChar))
            {
                return; // อนุญาตให้ใช้ปุ่ม control (เช่น Backspace หรือ Delete)
            }

            // อนุญาตให้พิมพ์ตัวเลขและจุด (.)
            if (!Char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true; // ป้องกันการพิมพ์อักขระที่ไม่ใช่ตัวเลขหรือลูกน้ำ
            }

            // ตรวจสอบว่ามีจุดแล้วหรือยัง
            if (e.KeyChar == '.' && tbTravelCostTotal.Text.Contains("."))
            {
                e.Handled = true; // ถ้ามีจุดแล้วไม่อนุญาตให้ใส่จุดเพิ่ม
            }
        }
    }
}
