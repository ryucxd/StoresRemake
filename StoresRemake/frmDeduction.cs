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

namespace StoresRemake
{
    public partial class frmDeduction : Form
    {
        public int _skip { get; set; }
        public int _deduct { get; set; }
        public string _stockCode { get; set; }
        public frmDeduction()
        {
            InitializeComponent();
            _skip = -1;
            load_data();
        }

        private void load_data()
        {
            using (SqlConnection conn = new SqlConnection(CONNECT.ConnectionString))
            {
                string sql = "select stock_code,deduct_qty from dbo.stores_remake_list";
                using (SqlCommand cmd2 = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd2);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    foreach (DataRow row in dt.Rows)
                    {
                        _deduct = Convert.ToInt32(row[1].ToString());
                        _stockCode = row[0].ToString();
                        //enter is pressed
                        //i think the codes get scanned in at once so doing this here will be fine i guess

                        //try catch this for wrong entries
                        try
                        {
                            sql = "select [description] from dbo.stock where stock_code = " + _stockCode;
                            string description = "";
                            double quantity = 0;
                            using (SqlCommand cmd = new SqlCommand(sql, conn)) //
                            {
                                var getDescription = cmd.ExecuteScalar();
                                if (getDescription == null)
                                    description = "No Description.";
                                else
                                    description = getDescription.ToString();
                            }
                            sql = "select amount_in_stock FROM dbo.stock where stock_code = '" + _stockCode + "'";
                            using (SqlCommand cmd = new SqlCommand(sql, conn))
                            {
                                var getQty = cmd.ExecuteScalar();
                                if (getQty != null)
                                    quantity = Convert.ToDouble(getQty);
                            }


                            //load data into the dgv
                            if (description != "No Description.") //if theres no description then there is prob no stock code for this
                            {

                                if (_skip != -1)
                                {
                                    addDeduction(description, quantity);
                                    //formatting();//
                                    uploadList();
                                }
                                else
                                {
                                    loadFirstTime(description, quantity, _deduct);
                                }

                            }
                            txtStockCode.Focus();
                        }
                        catch
                        {

                        }

                    }
                    formatting();
                    conn.Close();

                }
            }
            _skip = 0;
        }
        private void uploadList()
        {

            int stock_code_index = 0;
            stock_code_index = dataGridView1.Columns["Stock Code"].Index;
            int description_index = 0;
            description_index = dataGridView1.Columns["Description"].Index;
            int qis_index = 0;
            qis_index = dataGridView1.Columns["Quantity in Stock"].Index;
            int qtd_index = 0;
            qtd_index = dataGridView1.Columns["Quantity to Deduct"].Index;
            //delete everything thats in the datagridview then reupload the current list
            using (SqlConnection conn = new SqlConnection(CONNECT.ConnectionString))
            {
                conn.Open();
                string sql = "DELETE FROM dbo.stores_remake_list";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                    cmd.ExecuteNonQuery();
                if (dataGridView1.Rows.Count > 0)
                {
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        sql = "insert into dbo.stores_remake_list (stock_code,deduct_qty) VALUES ('" + dataGridView1.Rows[i].Cells[stock_code_index].Value.ToString() + "'," + dataGridView1.Rows[i].Cells[qtd_index].Value.ToString() + ")";
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                            cmd.ExecuteNonQuery();
                    }
                }
                conn.Close();
            }
        }

        private void loadFirstTime(string description, double quantity_in_stock, int deduct_qty)
        {
            //first we need to see if there are no rows
            if (dataGridView1.Rows.Count == 0)
            {
                //make it from scratch
                DataTable dt = new DataTable();
                dt.Clear();
                dt.Columns.Add("Stock Code");
                dt.Columns.Add("Description");
                dt.Columns.Add("Quantity in Stock");
                dt.Columns.Add("Quantity to Deduct");

                DataRow dataRow = dt.NewRow();
                dataRow["Stock Code"] = _stockCode;
                dataRow["Description"] = description;
                dataRow["Quantity in Stock"] = quantity_in_stock;
                dataRow["Quantity to Deduct"] = deduct_qty;
                dt.Rows.Add(dataRow);

                dataGridView1.DataSource = dt;
            }
            else
            {
                //else we make a new datatable and rebind it
                DataTable dt = new DataTable();
                dt.Clear();
                dt.Columns.Add("Stock Code");
                dt.Columns.Add("Description");
                dt.Columns.Add("Quantity in Stock");
                dt.Columns.Add("Quantity to Deduct");

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    DataRow dataRow = dt.NewRow();
                    dataRow["Stock Code"] = row.Cells[0].Value.ToString();
                    dataRow["Description"] = row.Cells[1].Value.ToString();
                    dataRow["Quantity in Stock"] = row.Cells[2].Value.ToString();
                    dataRow["Quantity to Deduct"] = row.Cells[3].Value.ToString();
                    dt.Rows.Add(dataRow);
                }

                DataRow newRow = dt.NewRow();
                newRow["Stock Code"] = _stockCode;
                newRow["Description"] = description;
                newRow["Quantity in Stock"] = quantity_in_stock;
                newRow["Quantity to Deduct"] = "1";
                dt.Rows.Add(newRow); 

                dataGridView1.DataSource = null;
                dataGridView1.DataSource = dt;
            }
        }

        private void addDeduction(string description, double quantity_in_stock)
        {
            //first we need to see if there are no rows
            if (dataGridView1.Rows.Count == 0)
            {
                //make it from scratch
                DataTable dt = new DataTable();
                dt.Clear();
                dt.Columns.Add("Stock Code");
                dt.Columns.Add("Description");
                dt.Columns.Add("Quantity in Stock");
                dt.Columns.Add("Quantity to Deduct");

                DataRow dataRow = dt.NewRow();
                dataRow["Stock Code"] = txtStockCode.Text;
                dataRow["Description"] = description;
                dataRow["Quantity in Stock"] = quantity_in_stock;
                dataRow["Quantity to Deduct"] = "1";
                dt.Rows.Add(dataRow);

                dataGridView1.DataSource = dt;
            }
            else
            {
                //else we make a new datatable and rebind it
                DataTable dt = new DataTable();
                dt.Clear();
                dt.Columns.Add("Stock Code");
                dt.Columns.Add("Description");
                dt.Columns.Add("Quantity in Stock");
                dt.Columns.Add("Quantity to Deduct");

                int stock_code_index = 0;
                stock_code_index = dataGridView1.Columns["Stock Code"].Index;
                int description_index = 0;
                description_index = dataGridView1.Columns["Description"].Index;
                int qis_index = 0;
                qis_index = dataGridView1.Columns["Quantity in Stock"].Index;
                int qtd_index = 0;
                qtd_index = dataGridView1.Columns["Quantity to Deduct"].Index;

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    DataRow dataRow = dt.NewRow();
                    dataRow["Stock Code"] = row.Cells[stock_code_index].Value.ToString();
                    dataRow["Description"] = row.Cells[description_index].Value.ToString();
                    dataRow["Quantity in Stock"] = row.Cells[qis_index].Value.ToString();
                    dataRow["Quantity to Deduct"] = row.Cells[qtd_index].Value.ToString();
                    dt.Rows.Add(dataRow);
                }

                DataRow newRow = dt.NewRow();
                newRow["Stock Code"] = txtStockCode.Text;
                newRow["Description"] = description;
                newRow["Quantity in Stock"] = quantity_in_stock;
                newRow["Quantity to Deduct"] = "1";
                dt.Rows.Add(newRow);

                dataGridView1.DataSource = null;
                dataGridView1.DataSource = dt;
            }
        }

        private void txtStockCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //enter is pressed
                //i think the codes get scanned in at once so doing this here will be fine i guess

                //try catch this for wrong entries
                try
                {
                    string sql = "select [description] from dbo.stock where stock_code = " + txtStockCode.Text;
                    string description = "";
                    double quantity = 0;
                    using (SqlConnection conn = new SqlConnection(CONNECT.ConnectionString))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand(sql, conn))  //
                        {
                            var getDescription = cmd.ExecuteScalar();
                            if (getDescription == null)
                                description = "No Description."; 
                            else
                                description = getDescription.ToString();
                        }
                        sql = "select amount_in_stock FROM dbo.stock where stock_code = '" + txtStockCode.Text + "'";
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            var getQty = cmd.ExecuteScalar();
                            if (getQty != null)
                                quantity = Convert.ToDouble(getQty);
                        }
                        conn.Close();
                    }

                    //load data into the dgv
                    if (description != "No Description.") //if theres no description then there is prob no stock code for this
                    {
                        addDeduction(description, quantity);
                        formatting();//
                        uploadList();
                    }
                    dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows.Count - 1;
                    txtStockCode.Text = "";
                    txtStockCode.Focus();
                }
                catch
                {
                    txtStockCode.Text = "";
                }
            }


        }

        private void formatting()
        {
            if (dataGridView1.Rows.Count > 0)
            {
                try
                {
                    dataGridView1.Columns.Remove("Remove");
                }
                catch
                {

                }
                //also add the button here too
                int stock_code_index = 0;
                stock_code_index = dataGridView1.Columns["Stock Code"].Index;
                int description_index = 0;
                description_index = dataGridView1.Columns["Description"].Index;
                int qis_index = 0;
                qis_index = dataGridView1.Columns["Quantity in Stock"].Index;
                int qtd_index = 0;
                qtd_index = dataGridView1.Columns["Quantity to Deduct"].Index;
                try
                {
                    DataGridViewButtonColumn removeButtonColumn = new DataGridViewButtonColumn();
                    removeButtonColumn.Name = "Remove";
                    removeButtonColumn.Text = "✖";
                    removeButtonColumn.UseColumnTextForButtonValue = true;
                    int columnIndex = (dataGridView1.Columns.Count);
                    if (dataGridView1.Columns["Remove"] == null)
                    {
                        dataGridView1.Columns.Insert(columnIndex, removeButtonColumn);
                    }
                    foreach (DataGridViewColumn col in dataGridView1.Columns)
                    {
                        col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                        col.ReadOnly = true;

                    }
                    dataGridView1.Columns[qtd_index].ReadOnly = false;
                    dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
                catch
                { }
            }
            this.ActiveControl = txtStockCode;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //before removing the cell get the index 
            int indexScroll = e.RowIndex;
            int index = dataGridView1.Columns["Remove"].Index;
            if (e.ColumnIndex == index)
            {
                try
                {
                    dataGridView1.Columns.Remove("Remove");
                }
                catch
                {

                }
                //else we make a new datatable and rebind it   
                DataTable dt = new DataTable();
                dt.Clear();
                dt.Columns.Add("Stock Code");
                dt.Columns.Add("Description");
                dt.Columns.Add("Quantity in Stock");
                dt.Columns.Add("Quantity to Deduct");

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (e.RowIndex == row.Index)
                        continue;
                    DataRow dataRow = dt.NewRow();
                    dataRow["Stock Code"] = row.Cells[0].Value.ToString();
                    dataRow["Description"] = row.Cells[1].Value.ToString(); ;
                    dataRow["Quantity in Stock"] = row.Cells[2].Value.ToString();
                    dataRow["Quantity to Deduct"] = row.Cells[3].Value.ToString();
                    dt.Rows.Add(dataRow);
                }

                dataGridView1.DataSource = null;
                dataGridView1.DataSource = dt;
                formatting();
                if (indexScroll > dataGridView1.Rows.Count - 1)
                {
                    indexScroll = indexScroll - 1;
                }
                if (dataGridView1.Rows.Count > 1)
                {
                    dataGridView1.FirstDisplayedScrollingRowIndex = indexScroll;
                }
                uploadList();
            }
        }

        private void btnDeduct_Click(object sender, EventArgs e)
        {
            //here we add the entry to stock deduction log and remove the quantity from dbo.stock
            //first get the indexes of the columns because this is pretty important and it would suck if it removed the stock code instead of quantity xd
            int stockIndex = 0, QuantityIndex = 3; //the default values that they should be
            stockIndex = dataGridView1.Columns["Stock Code"].Index;
            QuantityIndex = dataGridView1.Columns["Quantity to Deduct"].Index;
            string sql = "";
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (Convert.ToInt32(row.Cells[QuantityIndex].Value) == 0)
                    continue; //skip anything with 0 
                using (SqlConnection conn = new SqlConnection(CONNECT.ConnectionString))
                {
                    conn.Open();
                    sql = "insert into dbo.stock_deduction_log (stock_code,date_deducted, quantity_deducted) VALUES ('" + row.Cells[stockIndex].Value.ToString() + "',GETDATE()," + row.Cells[QuantityIndex].Value.ToString() + ")";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        //insert into stock deduction log 
                        cmd.ExecuteNonQuery();
                    }
                    //run here
                    sql = "UPDATE dbo.stock SET amount_in_stock = amount_in_stock - " + row.Cells[QuantityIndex].Value.ToString() + " WHERE stock_code = '" + row.Cells[stockIndex].Value.ToString() + "'";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }

            //after this remove anyhting that has a quantity to deduct that is > 0
            int index = dataGridView1.Columns["Remove"].Index;
            try
            {
                dataGridView1.Columns.Remove("Remove");
            }
            catch
            {

            }

            //else we make a new datatable and rebind it
            DataTable dt = new DataTable();
            dt.Clear();
            dt.Columns.Add("Stock Code");
            dt.Columns.Add("Description");
            dt.Columns.Add("Quantity in Stock");
            dt.Columns.Add("Quantity to Deduct");
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (Convert.ToDouble(row.Cells[QuantityIndex].Value) == 0)
                {

                    DataRow dataRow = dt.NewRow();
                    dataRow["Stock Code"] = row.Cells[0].Value.ToString();
                    dataRow["Description"] = row.Cells[1].Value.ToString();
                    dataRow["Quantity in Stock"] = row.Cells[2].Value.ToString();
                    dataRow["Quantity to Deduct"] = row.Cells[3].Value.ToString();
                    dt.Rows.Add(dataRow);
                }
            }
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = dt;
            formatting();

            if (dataGridView1.Rows.Count > 0)
                MessageBox.Show("Items with 0 quantity have not been deducted or removed from the table. Please correct these values!"); //same messagebox that was in the old version
            uploadList();
            txtStockCode.Focus();

        }

        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e) //this handles the entry of non digits etc into the last column 
        {
            e.Control.KeyPress -= new KeyPressEventHandler(quantityCol_KeyPress);
            if (dataGridView1.CurrentCell.ColumnIndex == 3) //qty
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                {
                    tb.KeyPress += new KeyPressEventHandler(quantityCol_KeyPress);
                }
            }
        }

        private void quantityCol_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView1.CurrentCell = null;
            dataGridView1.ClearSelection();
            uploadList();
            this.ActiveControl = txtStockCode;
        }

        private void btnWipe_Click(object sender, EventArgs e)  //wipes everything from the list 
        {
            if (dataGridView1.Rows.Count > 0)
            {
                try
                {
                    int index = dataGridView1.Columns["Remove"].Index; //removes button before it breaks the next stock code
                    dataGridView1.Columns.Remove("Remove");
                }
                catch
                {
                    //dont need to add anything here
                }
            }
            dataGridView1.DataSource = null;
            //manually delete all
            string sql = "delete FROM dbo.stores_remake_list ";
            using (SqlConnection conn = new SqlConnection(CONNECT.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }
    }
}
