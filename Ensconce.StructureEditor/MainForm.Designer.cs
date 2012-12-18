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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tabControl = new System.Windows.Forms.TabControl();
            this.savedConfig = new System.Windows.Forms.TabPage();
            this.btnLoad = new System.Windows.Forms.Button();
            this.lstItems = new System.Windows.Forms.ListBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.KeyValues = new System.Windows.Forms.TabPage();
            this.keyValueData = new System.Windows.Forms.DataGridView();
            this.InstanceValues = new System.Windows.Forms.TabPage();
            this.instanceGrid = new System.Windows.Forms.DataGridView();
            this.dbLogin = new System.Windows.Forms.TabPage();
            this.dbLoginsGrid = new System.Windows.Forms.DataGridView();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.btnNew = new System.Windows.Forms.Button();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.btnUnload = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.savedConfig.SuspendLayout();
            this.KeyValues.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.keyValueData)).BeginInit();
            this.InstanceValues.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.instanceGrid)).BeginInit();
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
            this.tabControl.Controls.Add(this.KeyValues);
            this.tabControl.Controls.Add(this.InstanceValues);
            this.tabControl.Controls.Add(this.dbLogin);
            this.tabControl.Location = new System.Drawing.Point(12, 21);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(566, 364);
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
            this.savedConfig.Size = new System.Drawing.Size(558, 338);
            this.savedConfig.TabIndex = 3;
            this.savedConfig.Text = "Saved Config";
            this.savedConfig.UseVisualStyleBackColor = true;
            // 
            // btnLoad
            // 
            this.btnLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnLoad.Location = new System.Drawing.Point(287, 302);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(75, 23);
            this.btnLoad.TabIndex = 4;
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
            this.lstItems.Size = new System.Drawing.Size(521, 277);
            this.lstItems.TabIndex = 1;
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDelete.Location = new System.Drawing.Point(206, 302);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 3;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAdd.Location = new System.Drawing.Point(44, 302);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 2;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // KeyValues
            // 
            this.KeyValues.Controls.Add(this.keyValueData);
            this.KeyValues.Location = new System.Drawing.Point(4, 22);
            this.KeyValues.Name = "KeyValues";
            this.KeyValues.Padding = new System.Windows.Forms.Padding(3);
            this.KeyValues.Size = new System.Drawing.Size(558, 338);
            this.KeyValues.TabIndex = 0;
            this.KeyValues.Text = "Key Value";
            this.KeyValues.UseVisualStyleBackColor = true;
            // 
            // keyValueData
            // 
            this.keyValueData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.keyValueData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.keyValueData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.keyValueData.Enabled = false;
            this.keyValueData.Location = new System.Drawing.Point(3, 3);
            this.keyValueData.Name = "keyValueData";
            this.keyValueData.Size = new System.Drawing.Size(552, 332);
            this.keyValueData.TabIndex = 0;
            this.keyValueData.RowValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.keyValueData_RowValidated);
            // 
            // InstanceValues
            // 
            this.InstanceValues.Controls.Add(this.instanceGrid);
            this.InstanceValues.Location = new System.Drawing.Point(4, 22);
            this.InstanceValues.Name = "InstanceValues";
            this.InstanceValues.Padding = new System.Windows.Forms.Padding(3);
            this.InstanceValues.Size = new System.Drawing.Size(558, 338);
            this.InstanceValues.TabIndex = 1;
            this.InstanceValues.Text = "Instance Specific";
            this.InstanceValues.UseVisualStyleBackColor = true;
            // 
            // instanceGrid
            // 
            this.instanceGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.instanceGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.instanceGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.instanceGrid.Enabled = false;
            this.instanceGrid.Location = new System.Drawing.Point(3, 3);
            this.instanceGrid.Name = "instanceGrid";
            this.instanceGrid.Size = new System.Drawing.Size(552, 332);
            this.instanceGrid.TabIndex = 1;
            this.instanceGrid.RowValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.instanceGrid_RowValidated);
            // 
            // dbLogin
            // 
            this.dbLogin.Controls.Add(this.dbLoginsGrid);
            this.dbLogin.Location = new System.Drawing.Point(4, 22);
            this.dbLogin.Name = "dbLogin";
            this.dbLogin.Size = new System.Drawing.Size(558, 338);
            this.dbLogin.TabIndex = 2;
            this.dbLogin.Text = "Database Logins";
            this.dbLogin.UseVisualStyleBackColor = true;
            // 
            // dbLoginsGrid
            // 
            this.dbLoginsGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dbLoginsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dbLoginsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dbLoginsGrid.Enabled = false;
            this.dbLoginsGrid.Location = new System.Drawing.Point(0, 0);
            this.dbLoginsGrid.Name = "dbLoginsGrid";
            this.dbLoginsGrid.Size = new System.Drawing.Size(558, 338);
            this.dbLoginsGrid.TabIndex = 2;
            this.dbLoginsGrid.RowValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dbLoginsGrid_RowValidated);
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "xml";
            this.openFileDialog.FileName = "openFileDialog";
            this.openFileDialog.Filter = "XML Files|*.xml";
            // 
            // btnNew
            // 
            this.btnNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnNew.Location = new System.Drawing.Point(125, 302);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(75, 23);
            this.btnNew.TabIndex = 5;
            this.btnNew.Text = "New";
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "xml";
            this.saveFileDialog.Filter = "XML Files|*.XML";
            // 
            // btnUnload
            // 
            this.btnUnload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnUnload.Location = new System.Drawing.Point(368, 302);
            this.btnUnload.Name = "btnUnload";
            this.btnUnload.Size = new System.Drawing.Size(75, 23);
            this.btnUnload.TabIndex = 6;
            this.btnUnload.Text = "Unload";
            this.btnUnload.UseVisualStyleBackColor = true;
            this.btnUnload.Click += new System.EventHandler(this.btnUnload_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(585, 394);
            this.Controls.Add(this.tabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(601, 432);
            this.Name = "MainForm";
            this.Text = "Configuration Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tabControl.ResumeLayout(false);
            this.savedConfig.ResumeLayout(false);
            this.KeyValues.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.keyValueData)).EndInit();
            this.InstanceValues.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.instanceGrid)).EndInit();
            this.dbLogin.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dbLoginsGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage KeyValues;
        private System.Windows.Forms.TabPage InstanceValues;
        private System.Windows.Forms.TabPage dbLogin;
        private System.Windows.Forms.ListBox lstItems;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.TabPage savedConfig;
        private System.Windows.Forms.DataGridView keyValueData;
        private System.Windows.Forms.DataGridView instanceGrid;
        private System.Windows.Forms.DataGridView dbLoginsGrid;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.Button btnUnload;
    }
}

