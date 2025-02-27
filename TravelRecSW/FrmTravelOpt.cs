using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TravelRecSW
{
    public partial class FrmTravelOpt : Form
    {
    
        public FrmTravelOpt()
        {
            InitializeComponent();
        }
        private void getTravelFromDBToDGV()
        {
            //ติดต่อ DB
            SqlConnection conn = new SqlConnection(ShareInfo.connStr);
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
            conn.Open();

            //คำสั่ง SQL
            string strSql = "SELECT travelPlace, travelCostTotal, travelImage, travelId FROM travel_tb " +
                            "WHERE travellerId = @travellerId ";

            //สร้าง SQL Trasaction และ SQL Command เพื่อทำงานกับค่ำสั่ง SQL
            SqlTransaction sqlTransaction = conn.BeginTransaction();
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Connection = conn;
            sqlCommand.CommandType = CommandType.Text;
            sqlCommand.CommandText = strSql;
            sqlCommand.Transaction = sqlTransaction;

            sqlCommand.Parameters.AddWithValue("@travellerId", ShareInfo.travellerId);

            //สั่งให้ SQL ทำงาน
            SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
            conn.Close();

            DataTable dt = new DataTable();
            adapter.Fill(dt);

            //เอาข้อมูลใน DataTable ไปแสดงที่ DGV
            if (dt.Rows.Count > 0 )
            {
                //ปรับความสูงของแต่ละแถว
                dgvTravel.RowTemplate.Height = 50;

                //กรณีมี จะนำข้อมูลมาแสดง
                dgvTravel.DataSource = dt;

                //กำหนดหัวคอลลัมน์ของ DGV
                dgvTravel.Columns[0].HeaderText = "สถานที่ที่ไป";
                dgvTravel.Columns[1].HeaderText = "ค่าใช้จ่าย";
                dgvTravel.Columns[2].HeaderText = "รูปสถานทีที่ไป";
                //กำหนดความกว้าง
                dgvTravel.Columns[0].Width = 100;
                dgvTravel.Columns[1].Width = 100;
                dgvTravel.Columns[2].Width = 200;
                dgvTravel.Columns[3].Width = 0;

                //ปรับรูปให้พอดีกับควมสูง
                DataGridViewImageColumn imgCol = (DataGridViewImageColumn)dgvTravel.Columns[2];
                imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
            }
            else
            {
                //กรณีไม่มี ให้แสดงแค่หัวคอลัมน์ของตารางใน DGV

            }
        }
        private void FrmTravelOpt_Load(object sender, EventArgs e)
        {
            //เอารูปมา traveller มาแสดง
            using (MemoryStream ms = new MemoryStream(ShareInfo.travellerImage))
            {
                pbTravellerImage.Image = Image.FromStream(ms);
            }
            //เอาชื่อ traveller มาแสดง
            lbTravellerFullname.Text = ShareInfo.travellerFullname;

            //ดึงข้อมูลการเดินทางของ traveller คนที่ Login เข้ามา มาแสดง
            getTravelFromDBToDGV();
        }

        private void tsbtToFrmLogin_Click(object sender, EventArgs e)
        {
            FrmLogin frmLogin = new FrmLogin();
            frmLogin.Show();
            Hide();
        }

        private void tsbtAdd_Click(object sender, EventArgs e)
        {
            FrmTravelAdd frmTravelAdd = new FrmTravelAdd();
            frmTravelAdd.ShowDialog(this);

            getTravelFromDBToDGV();
        }

        private void tsbtEdit_Click(object sender, EventArgs e)
        {
            if (dgvTravel.SelectedRows.Count <= 0)
            {
                ShareInfo.showWarningMSG("เลิอกรายการที้จะแก้ไขด้วย");
            }
            else
            {
                //ตัวแปรเก็บแถวที่เลือก
                int indexRow = dgvTravel.CurrentRow.Index;
                //ตัวแปรเก็บ travelId
                int travelId = int.Parse(dgvTravel.Rows[indexRow].Cells[3].Value.ToString());

                FrmTravelEdit frmTravelEdit = new FrmTravelEdit(travelId);
                frmTravelEdit.ShowDialog(this);

                getTravelFromDBToDGV();
            }
        }
    }
}
