namespace Ensconce.StructureEditor
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tabControl = new System.Windows.Forms.TabControl();
            this.savedConfig = new System.Windows.Forms.TabPage();
            this.btnUnload = new System.Windows.Forms.Button();
            this.btnNew = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.lstItems = new System.Windows.Forms.ListBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.properties = new System.Windows.Forms.TabPage();
            this.keyValueGrid = new System.Windows.Forms.DataGridView();
            this.propertyGroups = new System.Windows.Forms.TabPage();
            this.propertyGroupGrid = new System.Windows.Forms.DataGridView();
            this.dbLogin = new System.Windows.Forms.TabPage();
            this.dbLoginsGrid = new System.Windows.Forms.DataGridView();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.tabControl.SuspendLayout();
            this.savedConfig.SuspendLayout();
            this.properties.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.keyValueGrid)).BeginInit();
            this.propertyGroups.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.propertyGroupGrid)).BeginInit();
            this.dbLogin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dbLoginsGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.savedConfig);
            this.tabControl.Controls.Add(this.properties);
            this.tabControl.Controls.Add(this.propertyGroups);
            this.tabControl.Controls.Add(this.dbLogin);
            this.tabControl.Location = new System.Drawing.Point(12, 21);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(843, 558);
            this.tabControl.TabIndex = 0;
            // 
            // savedConfig
            // 
            this.savedConfig.Controls.Add(this.btnUnload);
            this.savedConfig.Controls.Add(this.btnNew);
            this.savedConfig.Controls.Add(this.btnLoad);
            this.savedConfig.Controls.Add(this.lstItems);
            this.savedConfig.Controls.Add(this.btnDelete);
            this.savedConfig.Controls.Add(this.btnAdd);
            this.savedConfig.Location = new System.Drawing.Point(4, 22);
            this.savedConfig.Name = "savedConfig";
            this.savedConfig.Size = new System.Drawing.Size(835, 532);
            this.savedConfig.TabIndex = 3;
            this.savedConfig.Text = "Saved Config";
            this.savedConfig.UseVisualStyleBackColor = true;
            // 
            // btnUnload
            // 
            this.btnUnload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnUnload.Enabled = false;
            this.btnUnload.Location = new System.Drawing.Point(500, 490);
            this.btnUnload.Name = "btnUnload";
            this.btnUnload.Size = new System.Drawing.Size(75, 23);
            this.btnUnload.TabIndex = 6;
            this.btnUnload.Text = "Unload";
            this.btnUnload.UseVisualStyleBackColor = true;
            this.btnUnload.Click += new System.EventHandler(this.btnUnload_Click);
            // 
            // btnNew
            // 
            this.btnNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnNew.Location = new System.Drawing.Point(257, 490);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(75, 23);
            this.btnNew.TabIndex = 3;
            this.btnNew.Text = "New";
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnLoad.Location = new System.Drawing.Point(419, 490);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(75, 23);
            this.btnLoad.TabIndex = 5;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // lstItems
            // 
            this.lstItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstItems.FormattingEnabled = true;
            this.lstItems.Location = new System.Drawing.Point(3, 19);
            this.lstItems.Name = "lstItems";
            this.lstItems.Size = new System.Drawing.Size(798, 459);
            this.lstItems.TabIndex = 1;
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDelete.Location = new System.Drawing.Point(338, 490);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 4;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAdd.Location = new System.Drawing.Point(176, 490);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 2;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // properties
            // 
            this.properties.Controls.Add(this.keyValueGrid);
            this.properties.Location = new System.Drawing.Point(4, 22);
            this.properties.Name = "properties";
            this.properties.Padding = new System.Windows.Forms.Padding(3);
            this.properties.Size = new System.Drawing.Size(835, 532);
            this.properties.TabIndex = 0;
            this.properties.Text = "Properties";
            this.properties.UseVisualStyleBackColor = true;
            // 
            // keyValueGrid
            // 
            this.keyValueGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.keyValueGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.keyValueGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.keyValueGrid.Location = new System.Drawing.Point(3, 3);
            this.keyValueGrid.Name = "keyValueGrid";
            this.keyValueGrid.Size = new System.Drawing.Size(829, 526);
            this.keyValueGrid.TabIndex = 0;
            this.keyValueGrid.RowValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.keyValueGrid_RowValidated);
            // 
            // propertyGroups
            // 
            this.propertyGroups.Controls.Add(this.propertyGroupGrid);
            this.propertyGroups.Location = new System.Drawing.Point(4, 22);
            this.propertyGroups.Name = "propertyGroups";
            this.propertyGroups.Padding = new System.Windows.Forms.Padding(3);
            this.propertyGroups.Size = new System.Drawing.Size(835, 532);
            this.propertyGroups.TabIndex = 1;
            this.propertyGroups.Text = "Property Groups";
            this.propertyGroups.UseVisualStyleBackColor = true;
            // 
            // propertyGroupGrid
            // 
            this.propertyGroupGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.propertyGroupGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.propertyGroupGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGroupGrid.Location = new System.Drawing.Point(3, 3);
            this.propertyGroupGrid.Name = "propertyGroupGrid";
            this.propertyGroupGrid.Size = new System.Drawing.Size(829, 526);
            this.propertyGroupGrid.TabIndex = 1;
            this.propertyGroupGrid.RowValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.propertyGroupGrid_RowValidated);
            // 
            // dbLogin
            // 
            this.dbLogin.Controls.Add(this.dbLoginsGrid);
            this.dbLogin.Location = new System.Drawing.Point(4, 22);
            this.dbLogin.Name = "dbLogin";
            this.dbLogin.Size = new System.Drawing.Size(835, 532);
            this.dbLogin.TabIndex = 2;
            this.dbLogin.Text = "Database Logins";
            this.dbLogin.UseVisualStyleBackColor = true;
            // 
            // dbLoginsGrid
            // 
            this.dbLoginsGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dbLoginsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dbLoginsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dbLoginsGrid.Location = new System.Drawing.Point(0, 0);
            this.dbLoginsGrid.Name = "dbLoginsGrid";
            this.dbLoginsGrid.Size = new System.Drawing.Size(835, 532);
            this.dbLoginsGrid.TabIndex = 2;
            this.dbLoginsGrid.RowValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dbLoginsGrid_RowValidated);
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "xml";
            this.openFileDialog.FileName = "openFileDialog";
            this.openFileDialog.Filter = "XML Files|*.xml";
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "xml";
            this.saveFileDialog.Filter = "XML Files|*.XML";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(862, 588);
            this.Controls.Add(this.tabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(601, 432);
            this.Name = "MainForm";
            this.Text = "Configuration Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tabControl.ResumeLayout(false);
            this.savedConfig.ResumeLayout(false);
            this.properties.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.keyValueGrid)).EndInit();
            this.propertyGroups.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.propertyGroupGrid)).EndInit();
            this.dbLogin.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dbLoginsGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage properties;
        private System.Windows.Forms.TabPage propertyGroups;
        private System.Windows.Forms.TabPage dbLogin;
        private System.Windows.Forms.ListBox lstItems;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.TabPage savedConfig;
        private System.Windows.Forms.DataGridView keyValueGrid;
        private System.Windows.Forms.DataGridView propertyGroupGrid;
        private System.Windows.Forms.DataGridView dbLoginsGrid;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.Button btnUnload;
    }
}

