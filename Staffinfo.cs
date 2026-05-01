using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace HOTEL
{
    public partial class Staffinfo : Form
    {
        // السطر ده معناه: روح لمكان تشغيل البرنامج وهتلاقي ملف الداتا بيز جنبك

        // السطر الجديد اللي بيمنع تعليق ملفات اللوج وبيخلي المسار مرن
        //SqlConnection Con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\HOTELDB.mdf;Integrated Security=True;User Instance=True;Connect Timeout=30;Encrypt=False");
        SqlConnection Con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=J:\projects\HOTEL\HOTEL\HotelDB.mdf;Integrated Security=True;");
        public void populate()
        {

            Con.Open();
            string Myquery = "select * from Staff_tbl";
            SqlDataAdapter da = new SqlDataAdapter(Myquery, Con);
            SqlCommandBuilder cbuilder = new SqlCommandBuilder(da);
            var ds = new DataSet();
            da.Fill(ds);
            staffGridview.DataSource = ds.Tables[0];
            Con.Close();
        }
        public Staffinfo()
        {
            InitializeComponent();
        }

        private void Staffinfo_Load(object sender, EventArgs e)
        {
            DateIdI.Text = DateTime.Now.ToLongTimeString();
            timer1.Start();
            populate();

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void AddBtn_Click(object sender, EventArgs e)
        {


            try
            {
                // 1. التأكد من أن جميع الخانات ممتلئة (بما في ذلك الباسورد)
                if (string.IsNullOrWhiteSpace(stafftbl.Text) ||
                    string.IsNullOrWhiteSpace(staffnametbl.Text) ||
                    string.IsNullOrWhiteSpace(staffphone.Text) ||
                    string.IsNullOrWhiteSpace(staffpasswordtn.Text) || // إضافة التحقق من الباسورد هنا
                    staffgerdercb.Text == "gender" || staffgerdercb.SelectedIndex == -1)
                {
                    MessageBox.Show("All fields are required!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (Con.State == ConnectionState.Closed) Con.Open();

                // 2. التحقق من تكرار الـ ID أو التليفون
                string checkQuery = "SELECT COUNT(*) FROM Staff_tbl WHERE StaffId= " + stafftbl.Text + " OR staffPhone = '" + staffphone.Text + "'";
                SqlCommand checkCmd = new SqlCommand(checkQuery, Con);
                int count = (int)checkCmd.ExecuteScalar();

                if (count > 0)
                {
                    // ... كود التحقق من التكرار كما هو ...
                    MessageBox.Show("Duplicate ID or Phone Number found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    Con.Close();
                    return;
                }

                // 3. جملة الإدخال المعدلة (إضافة عمود Staffpassword وقيمته)
                // لاحظ إضافة العمود وقيمته من الـ staffpasswordtn
                string query = "INSERT INTO Staff_tbl (StaffId, Staffname, Staffphone, gender, Staffpassword) VALUES (" +
                               stafftbl.Text + ", '" +
                               staffnametbl.Text + "', '" +
                               staffphone.Text + "', '" +
                               staffgerdercb.SelectedItem.ToString() + "', '" +
                               staffpasswordtn.Text + "')";

                SqlCommand cmd = new SqlCommand(query, Con);
                cmd.ExecuteNonQuery();

                MessageBox.Show("STAFF Added Successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                Con.Close();

                // تحديث الجدول
                populate();

                // 4. مسح الخانات بعد الإضافة
                stafftbl.Clear();
                staffnametbl.Clear();
                staffphone.Clear();
                staffpasswordtn.Clear(); // مسح خانة الباسورد
                staffgerdercb.Text = "gender";
                stafftbl.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (Con.State == ConnectionState.Open) Con.Close();
            }
        }





        private void button4_Click(object sender, EventArgs e)
        {
            Con.Open();
            string Myquery = "select * from Staff_tbl where StaffName = '" + staffclean.Text + "'";
            SqlDataAdapter da = new SqlDataAdapter(Myquery, Con);
            var ds = new DataSet();
            da.Fill(ds);

            if (ds.Tables[0].Rows.Count > 0)
            {
                // لو اسم الموظف موجود
                staffGridview.DataSource = ds.Tables[0];
            }
            else
            {
                // لو الاسم مش موجود
                staffGridview.DataSource = null;
                MessageBox.Show("Staff member not found!", "Search", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            Con.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 1. التأكد من الباسورد الخاص بالأدمن (Security Check)
            string input = Microsoft.VisualBasic.Interaction.InputBox("Enter Admin Password:", "Security Check", "");
            if (input != "1")
            {
                if (!string.IsNullOrEmpty(input)) MessageBox.Show("Access Denied!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            // 2. التحقق من اختيار موظف وملء الخانات
            if (string.IsNullOrWhiteSpace(stafftbl.Text) ||
                string.IsNullOrWhiteSpace(staffpasswordtn.Text) || // تأكد إن الباسورد مش فاضي عند التعديل
                staffgerdercb.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a staff and fill all fields (including password)!", "Missing Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. التنفيذ
            try
            {
                if (Con.State == ConnectionState.Closed) Con.Open();

                // تعديل جملة الـ SQL لتشمل الـ Staffpassword
                string myquery = "UPDATE Staff_tbl SET Staffname = '" + staffnametbl.Text +
                                 "', Staffphone = '" + staffphone.Text +
                                 "', gender = '" + staffgerdercb.SelectedItem.ToString() +
                                 "', Staffpassword = '" + staffpasswordtn.Text + // تحديث الباسورد هنا
                                 "' WHERE StaffId = " + stafftbl.Text + ";";

                SqlCommand cmd = new SqlCommand(myquery, Con);
                cmd.ExecuteNonQuery();

                MessageBox.Show("Staff Updated Successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Con.Close();

                // تحديث الجدول وتنظيف الخانات
                populate();

                stafftbl.Clear();
                staffnametbl.Clear();
                staffphone.Clear();
                staffpasswordtn.Clear(); // تنظيف خانة الباسورد
                staffgerdercb.SelectedIndex = -1;
                staffgerdercb.Text = "gender";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                if (Con.State == ConnectionState.Open) Con.Close();
            }
        }

        private void Deletebtn_Click(object sender, EventArgs e)
        {
            // 1. التأكد من الباسورد
            string input = Microsoft.VisualBasic.Interaction.InputBox("Enter Admin Password to Delete:", "Security Check", "");
            if (input != "1")
            {
                if (!string.IsNullOrEmpty(input)) MessageBox.Show("Incorrect Password!", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            // كود المسح بتاعك
            Con.Open();
            string query = "delete from Staff_tbl where Staffid=" + stafftbl.Text + " ";
            SqlCommand cmd = new SqlCommand(query, Con);
            cmd.ExecuteNonQuery();
            MessageBox.Show("staff Successfully Deleted");
            Con.Close();

            populate();
            stafftbl.Clear();
            staffnametbl.Clear();
            staffphone.Clear();
            staffpasswordtn.Clear(); // السطر اللي ضفناه لمسح الباسورد
            staffgerdercb.Text = "gender";

            stafftbl.Focus();
            populate();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            staffclean.Clear();

            // 2. عرض كل البيانات في الجدول تاني (ريفريش)
            populate();

            // 3. تنظيف الخانات الجانبية (ID والاسم والرقم) عشان تبدأ من جديد]
            staffnametbl.Clear();
            stafftbl.Clear();
            staffphone.Clear();
            staffpasswordtn.Clear();
            staffgerdercb.Text = "gender";
        }

        private void staffGridview_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            // التأكد إن الضغطة على سطر فيه بيانات مش على العناوين
            //// بنأكد إننا ضغطنا على سطر حقيقي مش على العناوين اللي فوق
            //if (e.RowIndex >= 0)
            //{
            //    // بنحدد السطر اللي اتضغط عليه
            //    DataGridViewRow row = staffGridview.Rows[e.RowIndex];

            //    // الترتيب الصحيح عشان البيانات تظهر في مكانها
            //    stafftbl.Text = row.Cells[0].Value.ToString();      // الـ ID
            //    staffnametbl.Text = row.Cells[1].Value.ToString();  // الاسم
            //    staffphone.Text = row.Cells[2].Value.ToString();    // التليفون

            //    // النوع (الخلية رقم 3)
            //    staffgerdercb.Text = row.Cells[3].Value.ToString();

            //    // الباسورد (الخلية رقم 4) - دي اللي كانت ناقصة ومسببة المشكلة
            //    staffpasswordtn.Text = row.Cells[4].Value.ToString();
            //}
        }


        private void back_Click(object sender, EventArgs e)
        {
            Form1 mainForm = new Form1();

            // 2. إظهار الفورم الرئيسية
            mainForm.Show();

            // 3. إغلاق الفورم الحالية (صفحة الموظفين أو العملاء)
            this.Hide();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DateIdI.Text = DateTime.Now.ToLongTimeString();
        }

        private void stafftbl_TextChanged(object sender, EventArgs e)
        {

        }

        private void stafftbl_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void staffnametbl_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // ممنوع أرقام هنا يا هندسة
            }
        }

        private void staffphone_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void DateIdI_Click(object sender, EventArgs e)
        {

        }

        private void staffpasswordtn_TextChanged(object sender, EventArgs e)
        {

        }

        private void staffGridview_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // بنحدد السطر اللي اتضغط عليه
                DataGridViewRow row = staffGridview.Rows[e.RowIndex];

                // الترتيب الصحيح عشان البيانات تظهر في مكانها
                stafftbl.Text = row.Cells[0].Value.ToString();      // الـ ID
                staffnametbl.Text = row.Cells[1].Value.ToString();  // الاسم
                staffphone.Text = row.Cells[2].Value.ToString();    // التليفون

                // النوع (الخلية رقم 3)
                staffgerdercb.Text = row.Cells[3].Value.ToString();

                // الباسورد (الخلية رقم 4) - دي اللي كانت ناقصة ومسببة المشكلة
                staffpasswordtn.Text = row.Cells[4].Value.ToString();
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }

}
