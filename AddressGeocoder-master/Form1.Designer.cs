namespace CensusTractValidator
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        // Form controls
        private TextBox txtConnectionString;
        private TextBox txtTableName;
        private TextBox txtIdColumn;
        private TextBox txtAddress1Column;
        private TextBox txtAddress2Column;
        private TextBox txtCityColumn;
        private TextBox txtStateColumn;
        private TextBox txtZipColumn;
        private RichTextBox richTextBoxResults;
        private Button btnTestConnection;
        private Button btnAddColumns;
        private Button btnStartGeocoding;
        private Button btnStopGeocoding;
        private Button btnClear;
        private Label lblRecordCount;
        private NumericUpDown numBatchSize;
        private NumericUpDown numDelay;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtConnectionString = new TextBox();
            this.txtTableName = new TextBox();
            this.txtIdColumn = new TextBox();
            this.txtAddress1Column = new TextBox();
            this.txtAddress2Column = new TextBox();
            this.txtCityColumn = new TextBox();
            this.txtStateColumn = new TextBox();
            this.txtZipColumn = new TextBox();
            this.richTextBoxResults = new RichTextBox();
            this.btnTestConnection = new Button();
            this.btnAddColumns = new Button();
            this.btnStartGeocoding = new Button();
            this.btnStopGeocoding = new Button();
            this.btnClear = new Button();
            this.lblRecordCount = new Label();
            this.numBatchSize = new NumericUpDown();
            this.numDelay = new NumericUpDown();
            this.statusStrip1 = new StatusStrip();
            this.toolStripStatusLabel1 = new ToolStripStatusLabel();

            ((System.ComponentModel.ISupportInitialize)(this.numBatchSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDelay)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();

            // Form1
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(900, 780);
            this.MinimumSize = new Size(820, 700);
            this.Name = "Form1";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Census Bureau Bulk Address Geocoder";

            // Create layout
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(10)
            };

            // Title
            var titleLabel = new Label
            {
                Text = "Census Bureau Bulk Address Geocoder",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            // Connection Group
            var connectionGroup = new GroupBox { Text = "Database Connection", Dock = DockStyle.Fill, Padding = new Padding(10) };
            var connectionPanel = new Panel { Dock = DockStyle.Fill };

            var lblConnection = new Label { Text = "Connection String:", Location = new Point(10, 20), Size = new Size(120, 15) };
            this.txtConnectionString.Location = new Point(10, 40);
            this.txtConnectionString.Size = new Size(650, 23);

            this.btnTestConnection.Text = "Test Connection";
            this.btnTestConnection.Location = new Point(670, 40);
            this.btnTestConnection.Size = new Size(120, 23);
            this.btnTestConnection.Click += new EventHandler(this.btnTestConnection_Click);

            this.lblRecordCount.Text = "Records needing geocoding: Unknown";
            this.lblRecordCount.Location = new Point(10, 75);
            this.lblRecordCount.Size = new Size(300, 15);
            this.lblRecordCount.ForeColor = Color.DarkBlue;
            this.lblRecordCount.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            connectionPanel.Controls.AddRange(new Control[] { lblConnection, this.txtConnectionString, this.btnTestConnection, this.lblRecordCount });
            connectionGroup.Controls.Add(connectionPanel);

            // Table Config Group
            var tableGroup = new GroupBox { Text = "Table Configuration", Dock = DockStyle.Fill, Padding = new Padding(10) };
            var tablePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3
            };

            tablePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tablePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tablePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tablePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            // Give every row an explicit, equal height so all 4 rows (including Zip) render
            tablePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tablePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tablePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tablePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));

            // Add controls to table
            this.txtTableName.Dock = DockStyle.Fill;
            this.txtIdColumn.Dock = DockStyle.Fill;
            this.txtAddress1Column.Dock = DockStyle.Fill;
            this.txtAddress2Column.Dock = DockStyle.Fill;
            this.txtCityColumn.Dock = DockStyle.Fill;
            this.txtStateColumn.Dock = DockStyle.Fill;
            this.txtZipColumn.Dock = DockStyle.Fill;

            AddTableRow(tablePanel, "Table:", this.txtTableName, "ID Column:", this.txtIdColumn, 0);
            AddTableRow(tablePanel, "Address1:", this.txtAddress1Column, "Address2:", this.txtAddress2Column, 1);
            AddTableRow(tablePanel, "City:", this.txtCityColumn, "State:", this.txtStateColumn, 2);

            var lblZip = new Label { Text = "Zip:", Anchor = AnchorStyles.Left, TextAlign = ContentAlignment.MiddleLeft };
            tablePanel.Controls.Add(lblZip, 0, 3);
            tablePanel.Controls.Add(this.txtZipColumn, 1, 3);
            tablePanel.RowCount = 4;

            tableGroup.Controls.Add(tablePanel);

            // Settings Group
            var settingsGroup = new GroupBox { Text = "Processing Settings", Dock = DockStyle.Fill, Padding = new Padding(10) };
            var settingsPanel = new Panel { Dock = DockStyle.Fill };

            var lblBatch = new Label { Text = "Batch Size:", Location = new Point(10, 20), Size = new Size(70, 15) };
            this.numBatchSize.Location = new Point(85, 18);
            this.numBatchSize.Size = new Size(60, 23);
            this.numBatchSize.Minimum = 1;
            this.numBatchSize.Maximum = 100;
            this.numBatchSize.Value = 10;

            var lblDelay = new Label { Text = "Delay (ms):", Location = new Point(200, 20), Size = new Size(70, 15) };
            this.numDelay.Location = new Point(275, 18);
            this.numDelay.Size = new Size(80, 23);
            this.numDelay.Minimum = 500;
            this.numDelay.Maximum = 10000;
            this.numDelay.Value = 1000;

            var lblInfo = new Label
            {
                Text = "Batch mode: all addresses are sent to the Census batch geocoder in chunks of 1000. " +
                       "Batch Size / Delay are legacy settings and no longer affect processing.",
                Location = new Point(10, 50),
                Size = new Size(650, 30),
                ForeColor = Color.Gray
            };

            settingsPanel.Controls.AddRange(new Control[] { lblBatch, this.numBatchSize, lblDelay, this.numDelay, lblInfo });
            settingsGroup.Controls.Add(settingsPanel);

            // Results Group
            var resultsGroup = new GroupBox { Text = "Geocoding Results", Dock = DockStyle.Fill, Padding = new Padding(10) };
            this.richTextBoxResults.Dock = DockStyle.Fill;
            this.richTextBoxResults.Font = new Font("Consolas", 9F);
            this.richTextBoxResults.BackColor = Color.FromArgb(245, 245, 245);
            this.richTextBoxResults.ReadOnly = true;
            resultsGroup.Controls.Add(this.richTextBoxResults);

            // Button Panel
            var buttonPanel = new Panel { Dock = DockStyle.Fill };

            this.btnAddColumns.Text = "Add Geocoded Columns";
            this.btnAddColumns.Size = new Size(170, 35);
            this.btnAddColumns.Location = new Point(60, 10);
            this.btnAddColumns.Click += new EventHandler(this.btnAddColumns_Click);

            this.btnStartGeocoding.Text = "Start Geocoding";
            this.btnStartGeocoding.Size = new Size(150, 35);
            this.btnStartGeocoding.Location = new Point(250, 10);
            this.btnStartGeocoding.BackColor = Color.FromArgb(0, 120, 215);
            this.btnStartGeocoding.ForeColor = Color.White;
            this.btnStartGeocoding.Enabled = false;
            this.btnStartGeocoding.UseVisualStyleBackColor = false;
            this.btnStartGeocoding.Click += new EventHandler(this.btnStartGeocoding_Click);

            this.btnStopGeocoding.Text = "Stop";
            this.btnStopGeocoding.Size = new Size(100, 35);
            this.btnStopGeocoding.Location = new Point(410, 10);
            this.btnStopGeocoding.BackColor = Color.FromArgb(200, 50, 50);
            this.btnStopGeocoding.ForeColor = Color.White;
            this.btnStopGeocoding.Enabled = false;
            this.btnStopGeocoding.UseVisualStyleBackColor = false;
            this.btnStopGeocoding.Click += new EventHandler(this.btnStopGeocoding_Click);

            this.btnClear.Text = "Clear Results";
            this.btnClear.Size = new Size(120, 35);
            this.btnClear.Location = new Point(520, 10);
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new EventHandler(this.btnClear_Click);

            buttonPanel.Controls.AddRange(new Control[] { this.btnAddColumns, this.btnStartGeocoding, this.btnStopGeocoding, this.btnClear });

            // Status Strip
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new Size(39, 17);
            this.toolStripStatusLabel1.Text = "Ready";
            this.statusStrip1.Items.AddRange(new ToolStripItem[] { this.toolStripStatusLabel1 });
            this.statusStrip1.Location = new Point(0, 678);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new Size(900, 22);
            this.statusStrip1.TabIndex = 0;

            // Add to main panel
            mainPanel.Controls.Add(titleLabel, 0, 0);
            mainPanel.Controls.Add(connectionGroup, 0, 1);
            mainPanel.Controls.Add(tableGroup, 0, 2);
            mainPanel.Controls.Add(settingsGroup, 0, 3);
            mainPanel.Controls.Add(resultsGroup, 0, 4);
            mainPanel.Controls.Add(buttonPanel, 0, 5);

            // Set row styles
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));   // title
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));  // connection (fits record-count label)
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 165));  // table config (4 rows incl. Zip)
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));  // settings
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // results (takes remainder)
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));   // buttons

            this.Controls.Add(mainPanel);
            this.Controls.Add(this.statusStrip1);

            ((System.ComponentModel.ISupportInitialize)(this.numBatchSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDelay)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void AddTableRow(TableLayoutPanel table, string label1, TextBox textBox1, string label2, TextBox textBox2, int row)
        {
            var lbl1 = new Label { Text = label1, Anchor = AnchorStyles.Left, TextAlign = ContentAlignment.MiddleLeft };
            var lbl2 = new Label { Text = label2, Anchor = AnchorStyles.Left, TextAlign = ContentAlignment.MiddleLeft };

            table.Controls.Add(lbl1, 0, row);
            table.Controls.Add(textBox1, 1, row);
            table.Controls.Add(lbl2, 2, row);
            table.Controls.Add(textBox2, 3, row);
        }
    }
}