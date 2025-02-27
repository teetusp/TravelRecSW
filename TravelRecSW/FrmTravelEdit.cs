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

        private void FrmTravelEdit_Load(object sender, EventArgs e)
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

        private void tsbtSave_Click(object sender, EventArgs e)
        {

        }

        private void tsbtCancel_Click(object sender, EventArgs e)
        {

        }
    }
}
