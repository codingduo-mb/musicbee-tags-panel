
namespace MusicBeePlugin
{
    partial class TagListSelectorForm
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
            this.ComboBoxTagSelect = new System.Windows.Forms.ComboBox();
            this.BtnComboBoxAddMetaDataType = new System.Windows.Forms.Button();
            this.BtnComboBoxMetaDataTypCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ComboBoxTagSelect
            // 
            this.ComboBoxTagSelect.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.ComboBoxTagSelect.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.ComboBoxTagSelect.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ComboBoxTagSelect.FormattingEnabled = true;
            this.ComboBoxTagSelect.Location = new System.Drawing.Point(12, 12);
            this.ComboBoxTagSelect.MaxDropDownItems = 12;
            this.ComboBoxTagSelect.Name = "ComboBoxTagSelect";
            this.ComboBoxTagSelect.Size = new System.Drawing.Size(198, 27);
            this.ComboBoxTagSelect.TabIndex = 0;
            this.ComboBoxTagSelect.Text = "Click here";
            // 
            // BtnComboBoxAddMetaDataType
            // 
            this.BtnComboBoxAddMetaDataType.Location = new System.Drawing.Point(12, 48);
            this.BtnComboBoxAddMetaDataType.Name = "BtnComboBoxAddMetaDataType";
            this.BtnComboBoxAddMetaDataType.Size = new System.Drawing.Size(75, 33);
            this.BtnComboBoxAddMetaDataType.TabIndex = 1;
            this.BtnComboBoxAddMetaDataType.Text = "Add";
            this.BtnComboBoxAddMetaDataType.UseVisualStyleBackColor = true;
            // 
            // BtnComboBoxMetaDataTypCancel
            // 
            this.BtnComboBoxMetaDataTypCancel.Location = new System.Drawing.Point(135, 48);
            this.BtnComboBoxMetaDataTypCancel.Name = "BtnComboBoxMetaDataTypCancel";
            this.BtnComboBoxMetaDataTypCancel.Size = new System.Drawing.Size(75, 33);
            this.BtnComboBoxMetaDataTypCancel.TabIndex = 2;
            this.BtnComboBoxMetaDataTypCancel.Text = "Cancel";
            this.BtnComboBoxMetaDataTypCancel.UseVisualStyleBackColor = true;
            // 
            // TagListSelectorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(222, 93);
            this.Controls.Add(this.BtnComboBoxMetaDataTypCancel);
            this.Controls.Add(this.BtnComboBoxAddMetaDataType);
            this.Controls.Add(this.ComboBoxTagSelect);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "TagListSelectorForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select a MetaData Type";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox ComboBoxTagSelect;
        private System.Windows.Forms.Button BtnComboBoxAddMetaDataType;
        private System.Windows.Forms.Button BtnComboBoxMetaDataTypCancel;
    }
}