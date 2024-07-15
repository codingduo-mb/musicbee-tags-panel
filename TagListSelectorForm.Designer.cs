
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
            this.comboBoxTagSelect = new System.Windows.Forms.ComboBox();
            this.Btn_ComboBoxAddTag = new System.Windows.Forms.Button();
            this.Btn_ComboBoxCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // comboBoxTagSelect
            // 
            this.comboBoxTagSelect.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.comboBoxTagSelect.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.comboBoxTagSelect.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxTagSelect.FormattingEnabled = true;
            this.comboBoxTagSelect.Location = new System.Drawing.Point(12, 12);
            this.comboBoxTagSelect.MaxDropDownItems = 12;
            this.comboBoxTagSelect.Name = "comboBoxTagSelect";
            this.comboBoxTagSelect.Size = new System.Drawing.Size(198, 27);
            this.comboBoxTagSelect.TabIndex = 0;
            this.comboBoxTagSelect.Text = "Click here";
            // 
            // Btn_ComboBoxAddTag
            // 
            this.Btn_ComboBoxAddTag.Location = new System.Drawing.Point(12, 48);
            this.Btn_ComboBoxAddTag.Name = "Btn_ComboBoxAddTag";
            this.Btn_ComboBoxAddTag.Size = new System.Drawing.Size(75, 33);
            this.Btn_ComboBoxAddTag.TabIndex = 1;
            this.Btn_ComboBoxAddTag.Text = "Add";
            this.Btn_ComboBoxAddTag.UseVisualStyleBackColor = true;
            // 
            // Btn_ComboBoxCancel
            // 
            this.Btn_ComboBoxCancel.Location = new System.Drawing.Point(135, 48);
            this.Btn_ComboBoxCancel.Name = "Btn_ComboBoxCancel";
            this.Btn_ComboBoxCancel.Size = new System.Drawing.Size(75, 33);
            this.Btn_ComboBoxCancel.TabIndex = 2;
            this.Btn_ComboBoxCancel.Text = "Cancel";
            this.Btn_ComboBoxCancel.UseVisualStyleBackColor = true;
            // 
            // TagListSelectorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(222, 93);
            this.Controls.Add(this.Btn_ComboBoxCancel);
            this.Controls.Add(this.Btn_ComboBoxAddTag);
            this.Controls.Add(this.comboBoxTagSelect);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "TabPageSelectorForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select a tag";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxTagSelect;
        private System.Windows.Forms.Button Btn_ComboBoxAddTag;
        private System.Windows.Forms.Button Btn_ComboBoxCancel;
    }
}