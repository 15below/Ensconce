using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Ensconce.StructureEditor
{
    public partial class MainForm : Form
    {
        private XDocument loadedConfig;
        private string loadedConfigPath;
        private DataSet configData;
        private BindingList<string> savedConfigs;


        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            configData = new DataSet();
            savedConfigs = new BindingList<string>();
            SetupDataTables();

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.SavedConfigs))
            {
                foreach (var filePath in Properties.Settings.Default.SavedConfigs.Split(','))
                {
                    savedConfigs.Add(filePath);
                }
            }

            if (savedConfigs.Count == 0)
            {
                btnDelete.Enabled = false;
                btnLoad.Enabled = false;
            }

            lstItems.DataSource = savedConfigs;
            keyValueGrid.DataSource = configData.Tables["KeyValues"].DefaultView;
            propertyGroupGrid.DataSource = configData.Tables["InstanceValues"].DefaultView;
            dbLoginsGrid.DataSource = configData.Tables["DBLogins"].DefaultView;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                UpdateLoadedConfig(true);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed To Save Config Changes!", "Save Failure", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }

            Properties.Settings.Default.SavedConfigs = string.Join(",", savedConfigs);
            Properties.Settings.Default.Save();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            savedConfigs.Remove(lstItems.SelectedValue.ToString());
            if (savedConfigs.Count == 0)
            {
                btnDelete.Enabled = false;
                btnLoad.Enabled = false;
                btnUnload.Enabled = false;
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            if (!File.Exists(Environment.CurrentDirectory + "\\Template.xml"))
            {
                MessageBox.Show("You Don't Have A Template File!", "No Template File", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            var fileSelected = saveFileDialog.ShowDialog();

            if (fileSelected == DialogResult.OK)
            {
                File.Copy(Environment.CurrentDirectory + "\\Template.xml", saveFileDialog.FileName);
                if (!savedConfigs.Contains(saveFileDialog.FileName))
                {
                    savedConfigs.Add(saveFileDialog.FileName);
                    lstItems.SelectedItem = saveFileDialog.FileName;
                }

                LoadConfigFromFile(saveFileDialog.FileName);

                btnDelete.Enabled = true;
                btnLoad.Enabled = true;
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadConfigFromFile(lstItems.SelectedValue.ToString());
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var fileSelected = openFileDialog.ShowDialog();

            if (fileSelected == DialogResult.OK)
            {
                if (!savedConfigs.Contains(openFileDialog.FileName))
                {
                    savedConfigs.Add(openFileDialog.FileName);
                    lstItems.SelectedItem = openFileDialog.FileName;
                }
            }

            LoadConfigFromFile(openFileDialog.FileName);
            btnDelete.Enabled = true;
            btnLoad.Enabled = true;
        }

        private void btnUnload_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateLoadedConfig(true);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed To Save Config Changes!", "Save Failure", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            configData.Tables["KeyValues"].Clear();
            configData.Tables["InstanceValues"].Clear();
            configData.Tables["DBLogins"].Clear();
            btnUnload.Enabled = false;

            loadedConfigPath = string.Empty;
            loadedConfig = new XDocument();
        }

        private void LoadConfigFromFile(string configFile)
        {
            if (!File.Exists(configFile))
            {
                MessageBox.Show("Couldn't Find Config File At Path!", "Load Failure", MessageBoxButtons.OK,
                                   MessageBoxIcon.Error);
                return;
            }

            try
            {
                loadedConfig = XDocument.Load(configFile);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed To Load Config File!", "Load Failure", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            configData.Tables["KeyValues"].Clear();
            configData.Tables["InstanceValues"].Clear();
            configData.Tables["DBLogins"].Clear();

            loadedConfigPath = lstItems.SelectedValue.ToString();
            try
            {
                foreach (var property in loadedConfig.XPathSelectElement("/Structure/Properties").Nodes())
                {
                    var xelementProperty = property as XElement;
                    if (xelementProperty != null)
                    {
                        var dataRow = configData.Tables["KeyValues"].NewRow();
                        dataRow["Key"] = xelementProperty.Attribute("name").Value;
                        dataRow["Value"] = xelementProperty.Value;
                        configData.Tables["KeyValues"].Rows.Add(dataRow);
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Failed To Parse Properties From Config File!", "Load Failure", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            try
            {
                foreach (var propertyGroup in loadedConfig.XPathSelectElement("/Structure/PropertyGroups").Nodes())
                {
                    var xelementPropertyGroup = propertyGroup as XElement;
                    if (xelementPropertyGroup != null)
                    {
                        var instance = xelementPropertyGroup.Attribute("identity").Value;
                        var label = xelementPropertyGroup.Descendants(XName.Get("Label")).First().Value;
                        foreach (var property in xelementPropertyGroup.Descendants(XName.Get("Properties")).Nodes())
                        {
                            var xelementProperty = property as XElement;
                            if (xelementProperty != null)
                            {
                                var dataRow = configData.Tables["InstanceValues"].NewRow();
                                dataRow["Label"] = label;
                                dataRow["Instance"] = instance;
                                dataRow["Key"] = xelementProperty.Attribute("name").Value;
                                dataRow["Value"] = xelementProperty.Value;
                                configData.Tables["InstanceValues"].Rows.Add(dataRow);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Failed To Parse Property Groups From Config File!", "Load Failure", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            try
            {
                foreach (var property in loadedConfig.XPathSelectElement("/Structure/DbLogins").Nodes())
                {
                    var xelementProperty = property as XElement;
                    if (xelementProperty != null)
                    {
                        var dataRow = configData.Tables["DBLogins"].NewRow();
                        if (xelementProperty.Descendants(XName.Get("Name")).Any())
                        {
                            dataRow["UserName"] = xelementProperty.Descendants(XName.Get("Name")).First().Value;
                        }
                        else
                        {
                            dataRow["UserName"] = "";
                        }

                        if (xelementProperty.Descendants(XName.Get("DefaultDb")).Any())
                        {
                            dataRow["DefaultDb"] = xelementProperty.Descendants(XName.Get("DefaultDb")).First().Value;
                        }
                        else
                        {
                            dataRow["DefaultDb"] = "";
                        }

                        if (xelementProperty.Descendants(XName.Get("Password")).Any())
                        {
                            dataRow["Password"] = xelementProperty.Descendants(XName.Get("Password")).First().Value;
                        }
                        else
                        {
                            dataRow["Password"] = "";
                        }

                        if (xelementProperty.Descendants(XName.Get("ConnectionString")).Any())
                        {
                            dataRow["ConnectionString"] =
                                xelementProperty.Descendants(XName.Get("ConnectionString")).First().Value;
                        }
                        else
                        {
                            dataRow["ConnectionString"] = "";
                        }

                        configData.Tables["DBLogins"].Rows.Add(dataRow);
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Failed To Parse DbLogins From Config File!", "Load Failure", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            btnUnload.Enabled = true;
        }
        
        private void UpdateLoadedConfig(bool onClose = false)
        {
            if (string.IsNullOrWhiteSpace(loadedConfigPath))
            {
                if (!onClose)
                {
                    if (configData.Tables["KeyValues"].Rows.Count > 0 || 
                        configData.Tables["KeyValues"].Rows.Count > 0 ||
                        configData.Tables["KeyValues"].Rows.Count > 0)
                    {
                        MessageBox.Show("You Need To Have Config Loaded To Change!", "Update Failure", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        configData.Tables["KeyValues"].Clear();
                        configData.Tables["InstanceValues"].Clear();
                        configData.Tables["DBLogins"].Clear();
                    }
                }
                return;
            }

            var newLoadedConfig = loadedConfig;
            var newProperties = new XElement("Properties");
            var newPropertyGroups = new XElement("PropertyGroups");
            var newDbLogins = new XElement("DbLogins");

            foreach (DataRow row in configData.Tables["KeyValues"].Rows)
            {
                var configItem = new XElement("Property");
                configItem.SetAttributeValue("name", row["Key"].ToString());
                configItem.Value = row["Value"].ToString();
                newProperties.Add(configItem);
            }

            foreach (DataRow row in configData.Tables["InstanceValues"].Rows)
            {
                var selector = string.Format(@"/PropertyGroup[@identity='{0}']", row["Instance"]);
                var configGroup = newPropertyGroups.XPathSelectElement(selector);

                if (configGroup == null)
                {
                    configGroup = new XElement("PropertyGroup");
                    configGroup.SetAttributeValue("identity", row["Instance"].ToString());
                    var configLabel = new XElement("Label");
                    configLabel.Value = row["Label"].ToString();
                    configGroup.Add(configLabel);
                    var configPropertiesElement = new XElement("Properties");
                    configGroup.Add(configPropertiesElement);
                    newPropertyGroups.Add(configGroup);
                }

                var configProperties = newPropertyGroups.XPathSelectElement(selector + "/Properties");
                var configProperty = new XElement("Property");
                configProperty.SetAttributeValue("name", row["Key"].ToString());
                configProperty.Value = row["Value"].ToString();
                configProperties.Add(configProperty);
            }

            foreach (DataRow row in configData.Tables["DBLogins"].Rows)
            {
                var configItem = new XElement("DbLogin");
                configItem.Add(new XElement("Name") { Value = row["UserName"].ToString() });
                configItem.Add(new XElement("DefaultDb") { Value = row["DefaultDb"].ToString() });
                configItem.Add(new XElement("Password") { Value = row["Password"].ToString() });
                newDbLogins.Add(configItem);
            }

            try
            {
                newLoadedConfig.XPathSelectElement("/Structure/Properties").ReplaceWith(newProperties);
                newLoadedConfig.XPathSelectElement("/Structure/PropertyGroups").ReplaceWith(newPropertyGroups);
                newLoadedConfig.XPathSelectElement("/Structure/DbLogins").ReplaceWith(newDbLogins);
                newLoadedConfig.Save(loadedConfigPath);
                loadedConfig = newLoadedConfig;
            }
            catch (Exception)
            {
                if (onClose) throw;
            }
        }

        private void SetupDataTables()
        {
            configData.Tables.Add(new DataTable("KeyValues"));
            configData.Tables["KeyValues"].Columns.Add("Key");
            configData.Tables["KeyValues"].Columns.Add("Value");

            configData.Tables.Add(new DataTable("InstanceValues"));
            configData.Tables["InstanceValues"].Columns.Add("Label");
            configData.Tables["InstanceValues"].Columns.Add("Instance");
            configData.Tables["InstanceValues"].Columns.Add("Key");
            configData.Tables["InstanceValues"].Columns.Add("Value");

            configData.Tables.Add(new DataTable("DBLogins"));
            configData.Tables["DBLogins"].Columns.Add("UserName");
            configData.Tables["DBLogins"].Columns.Add("DefaultDb");
            configData.Tables["DBLogins"].Columns.Add("Password");
            configData.Tables["DBLogins"].Columns.Add("ConnectionString");
        }

        private void keyValueGrid_RowValidated(object sender, DataGridViewCellEventArgs e)
        {
            UpdateLoadedConfig();
        }

        private void propertyGroupGrid_RowValidated(object sender, DataGridViewCellEventArgs e)
        {
            UpdateLoadedConfig();
        }

        private void dbLoginsGrid_RowValidated(object sender, DataGridViewCellEventArgs e)
        {
            UpdateLoadedConfig();
        }
    }
}
