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
        public frmDeduction()
        {
            InitializeComponent();
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

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    DataRow dataRow = dt.NewRow();
                    dataRow["Stock Code"] = row.Cells[0].Value.ToString();
                    dataRow["Description"] = row.Cells[1].Value.ToString(); ;
                    dataRow["Quantity in Stock"] = row.Cells[2].Value.ToString(); ;
                    dataRow["Quantity to Deduct"] = row.Cells[3].Value.ToString();
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
                string sql = "select [description] from dbo.stock where stock_code = " + txtStockCode.Text;
                string description = "";
                double quantity = 0;
                using (SqlConnection conn = new SqlConnection(CONNECT.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn)) //
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
                }
                dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows.Count - 1;
                txtStockCode.Text = "";
                txtStockCode.Focus();
            }
        }

        private void formatting()
        {
            try
            {
                dataGridView1.Columns.Remove("Remove");
            }
            catch
            {

            }
            //also add the button here too


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
            int quantity = dataGridView1.Columns["Quantity to Deduct"].Index;
            dataGridView1.Columns[quantity].ReadOnly = false;
            dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
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
                    dataRow["Quantity in Stock"] = row.Cells[2].Value.ToString(); ;
                    dataRow["Quantity to Deduct"] = row.Cells[3].Value.ToString();
                    dt.Rows.Add(dataRow);
                }

                dataGridView1.DataSource = null;
                dataGridView1.DataSource = dt;
                formatting();
                if (indexScroll > dataGridView1.Rows.Count -1)
                {
                    indexScroll = indexScroll - 1;
                }
                dataGridView1.FirstDisplayedScrollingRowIndex = indexScroll;
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
                    dataRow["Description"] = row.Cells[1].Value.ToString(); ;
                    dataRow["Quantity in Stock"] = row.Cells[2].Value.ToString(); ;
                    dataRow["Quantity to Deduct"] = row.Cells[3].Value.ToString();
                    dt.Rows.Add(dataRow);
                }
            }
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = dt;
            formatting();

            if (dataGridView1.Rows.Count > 0)
                MessageBox.Show("Items with 0 quantity have not been deducted or removed from the table. Please correct these values!"); //same messagebox that was in the old version
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
            this.ActiveControl = txtStockCode;
        }

        private void btnWipe_Click(object sender, EventArgs e)  //wipes everything from the list 
        {
            int index = dataGridView1.Columns["Remove"].Index; //removes button before it breaks the next stock code
           try
            {
                dataGridView1.Columns.Remove("Remove");
            }
            catch
            {
                //dont need to add anything here
            }
            dataGridView1.DataSource = null;
        }
    }
}
