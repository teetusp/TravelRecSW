using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TravelRecSW
{
    public partial class FrmLogin : Form
    {
        byte[] travellerImage;
        public FrmLogin()
        {
            InitializeComponent();
        }

        private void lbToFrmRegister_Click(object sender, EventArgs e)
        {
            FrmRegister frmRegister = new FrmRegister();
            frmRegister.Show();
            Hide();
        }

        private void btLogin_Click(object sender, EventArgs e)
        {
            if (tbTravellerEmail.Text.Trim().Length == 0)
            {
                ShareInfo.showWarningMSG("ป้อนชื่อผู้ใช้ (อีเมล) ด้วย");
            }
            else if (tbTravellerPassword.Text.Trim().Length == 0)
            {
                ShareInfo.showWarningMSG("ป้อนรหัสผ่านด้วย");
            }
            else
            {
                try
                {
                    //ติดต่อ DB
                    SqlConnection conn = new SqlConnection(ShareInfo.connStr);
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                    conn.Open();

                    string strSql = "SELECT * FROM traveller_tb WHERE " +
                                    "travellerEmail = @travellerEmail and " +
                                    "travellerPassword = @travellerPassword";

                    //สร้าง SQL Trasaction และ SQL Command เพื่อทำงานกับค่ำสั่ง SQL
                    SqlTransaction sqlTransaction = conn.BeginTransaction();
                    SqlCommand sqlCommand = new SqlCommand();
                    sqlCommand.Connection = conn;
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.CommandText = strSql;
                    sqlCommand.Transaction = sqlTransaction;


                    sqlCommand.Parameters.AddWithValue("@travellerEmail", tbTravellerEmail.Text.Trim());
                    sqlCommand.Parameters.AddWithValue("@travellerPassword", tbTravellerPassword.Text.Trim());
                    
                    //สั่งให้ SQL ทำงาน
                    SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                    conn.Close();

                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        //ถูกต้อง เปิดหน้าจอ FrmTraOpt
                        ShareInfo.travellerId = (int)dt.Rows[0]["travellerId"];
                        ShareInfo.travellerFullname = dt.Rows[0]["travellerFullname"].ToString();
                        ShareInfo.travellerEmail = dt.Rows[0]["travellerEmail"].ToString();
                        ShareInfo.travellerPassword = dt.Rows[0]["travellerPassword"].ToString();
                        ShareInfo.travellerImage = (byte[])dt.Rows[0]["travellerImage"];

                        FrmTravelOpt frmTravelOpt = new FrmTravelOpt();
                        frmTravelOpt.Show();
                        Hide();
                    }
                    else
                    {
                        ShareInfo.showWarningMSG("ป้อนชื่อผู้ใช้ (อีเมล) หรือรหัสผ่านไม่ถูกต้อง");
                    }
                }
                catch(Exception ex)
                {
                    ShareInfo.showWarningMSG("มีข้อผิดพลาดเกิดขึ้น กรุณาลองใหม่อีกครั้งหรือติดต่อ IT : " + ex.Message);
                }

            }
        }
    }
}
