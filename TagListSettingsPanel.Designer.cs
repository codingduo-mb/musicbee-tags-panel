
namespace MusicBeePlugin
{
    partial class TagListSettingsPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.LstTags = new System.Windows.Forms.ListBox();
            this.BtnRemoveTagFromList = new System.Windows.Forms.Button();
            this.BtnAddTagToList = new System.Windows.Forms.Button();
            this.CbEnableAlphabeticalTagListSorting = new System.Windows.Forms.CheckBox();
            this.BtnClearTagList = new System.Windows.Forms.Button();
            this.BtnExportCSVToFile = new System.Windows.Forms.Button();
            this.TxtNewTagInput = new System.Windows.Forms.TextBox();
            this.BtnMoveTagUp = new System.Windows.Forms.Button();
            this.BtnMoveTagDown = new System.Windows.Forms.Button();
            this.CbShowTagsThatAreNotInTheTagsList = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.BtnImportCSVToList = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // LstTags
            // 
            this.LstTags.BackColor = System.Drawing.Color.White;
            this.LstTags.FormattingEnabled = true;
            this.LstTags.ItemHeight = 19;
            this.LstTags.Location = new System.Drawing.Point(6, 6);
            this.LstTags.Margin = new System.Windows.Forms.Padding(5);
            this.LstTags.Name = "LstTags";
            this.LstTags.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LstTags.Size = new System.Drawing.Size(593, 498);
            this.LstTags.TabIndex = 7;
            // 
            // BtnRemoveTagFromList
            // 
            this.BtnRemoveTagFromList.Location = new System.Drawing.Point(313, 547);
            this.BtnRemoveTagFromList.Name = "BtnRemoveTagFromList";
            this.BtnRemoveTagFromList.Size = new System.Drawing.Size(250, 33);
            this.BtnRemoveTagFromList.TabIndex = 2;
            this.BtnRemoveTagFromList.Text = "Remove Tag";
            this.BtnRemoveTagFromList.UseVisualStyleBackColor = true;
            this.BtnRemoveTagFromList.Click += new System.EventHandler(this.BtnRemTag_Click);
            // 
            // BtnAddTagToList
            // 
            this.BtnAddTagToList.Location = new System.Drawing.Point(6, 547);
            this.BtnAddTagToList.Name = "BtnAddTagToList";
            this.BtnAddTagToList.Size = new System.Drawing.Size(250, 33);
            this.BtnAddTagToList.TabIndex = 1;
            this.BtnAddTagToList.Text = "Add Tag";
            this.BtnAddTagToList.UseVisualStyleBackColor = true;
            this.BtnAddTagToList.Click += new System.EventHandler(this.BtnAddTag_Click);
            // 
            // CbEnableAlphabeticalTagListSorting
            // 
            this.CbEnableAlphabeticalTagListSorting.AutoSize = true;
            this.CbEnableAlphabeticalTagListSorting.Checked = true;
            this.CbEnableAlphabeticalTagListSorting.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CbEnableAlphabeticalTagListSorting.Location = new System.Drawing.Point(404, 68);
            this.CbEnableAlphabeticalTagListSorting.Margin = new System.Windows.Forms.Padding(0);
            this.CbEnableAlphabeticalTagListSorting.Name = "CbEnableAlphabeticalTagListSorting";
            this.CbEnableAlphabeticalTagListSorting.Padding = new System.Windows.Forms.Padding(5);
            this.CbEnableAlphabeticalTagListSorting.Size = new System.Drawing.Size(182, 33);
            this.CbEnableAlphabeticalTagListSorting.TabIndex = 3;
            this.CbEnableAlphabeticalTagListSorting.Text = "Sort tags alphabetically";
            this.CbEnableAlphabeticalTagListSorting.UseVisualStyleBackColor = true;
            // 
            // BtnClearTagList
            // 
            this.BtnClearTagList.Location = new System.Drawing.Point(6, 103);
            this.BtnClearTagList.Name = "BtnClearTagList";
            this.BtnClearTagList.Size = new System.Drawing.Size(250, 33);
            this.BtnClearTagList.TabIndex = 6;
            this.BtnClearTagList.Text = "Clear This Taglist";
            this.BtnClearTagList.UseVisualStyleBackColor = true;
            this.BtnClearTagList.Click += new System.EventHandler(this.BtnClearTagSettings_Click);
            // 
            // BtnExportCSVToFile
            // 
            this.BtnExportCSVToFile.Location = new System.Drawing.Point(6, 64);
            this.BtnExportCSVToFile.Name = "BtnExportCSVToFile";
            this.BtnExportCSVToFile.Size = new System.Drawing.Size(250, 33);
            this.BtnExportCSVToFile.TabIndex = 5;
            this.BtnExportCSVToFile.Text = "Export Tags To CSV";
            this.BtnExportCSVToFile.UseVisualStyleBackColor = true;
            this.BtnExportCSVToFile.Click += new System.EventHandler(this.BtnExportCsv_Click);
            // 
            // TxtNewTagInput
            // 
            this.TxtNewTagInput.Location = new System.Drawing.Point(6, 510);
            this.TxtNewTagInput.Name = "TxtNewTagInput";
            this.TxtNewTagInput.Size = new System.Drawing.Size(557, 26);
            this.TxtNewTagInput.TabIndex = 0;
            // 
            // BtnMoveTagUp
            // 
            this.BtnMoveTagUp.Location = new System.Drawing.Point(569, 508);
            this.BtnMoveTagUp.Name = "BtnMoveTagUp";
            this.BtnMoveTagUp.Size = new System.Drawing.Size(30, 33);
            this.BtnMoveTagUp.TabIndex = 2;
            this.BtnMoveTagUp.Text = "▲";
            this.BtnMoveTagUp.UseVisualStyleBackColor = true;
            this.BtnMoveTagUp.Click += new System.EventHandler(this.BtnMoveTagUpSettings_Click);
            // 
            // BtnMoveTagDown
            // 
            this.BtnMoveTagDown.Location = new System.Drawing.Point(569, 547);
            this.BtnMoveTagDown.Name = "BtnMoveTagDown";
            this.BtnMoveTagDown.Size = new System.Drawing.Size(30, 33);
            this.BtnMoveTagDown.TabIndex = 2;
            this.BtnMoveTagDown.Text = "▼";
            this.BtnMoveTagDown.UseVisualStyleBackColor = true;
            this.BtnMoveTagDown.Click += new System.EventHandler(this.BtnMoveTagDownSettings_Click);
            // 
            // CbShowTagsThatAreNotInTheTagsList
            // 
            this.CbShowTagsThatAreNotInTheTagsList.AutoSize = true;
            this.CbShowTagsThatAreNotInTheTagsList.Location = new System.Drawing.Point(324, 104);
            this.CbShowTagsThatAreNotInTheTagsList.Name = "CbShowTagsThatAreNotInTheTagsList";
            this.CbShowTagsThatAreNotInTheTagsList.Padding = new System.Windows.Forms.Padding(5);
            this.CbShowTagsThatAreNotInTheTagsList.Size = new System.Drawing.Size(261, 33);
            this.CbShowTagsThatAreNotInTheTagsList.TabIndex = 8;
            this.CbShowTagsThatAreNotInTheTagsList.Text = "Show tags that are not in the taglist";
            this.CbShowTagsThatAreNotInTheTagsList.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.BtnImportCSVToList);
            this.groupBox1.Controls.Add(this.CbShowTagsThatAreNotInTheTagsList);
            this.groupBox1.Controls.Add(this.BtnExportCSVToFile);
            this.groupBox1.Controls.Add(this.BtnClearTagList);
            this.groupBox1.Controls.Add(this.CbEnableAlphabeticalTagListSorting);
            this.groupBox1.Location = new System.Drawing.Point(6, 586);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox1.Size = new System.Drawing.Size(593, 140);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Import / Export + Options";
            // 
            // BtnImportCSVToList
            // 
            this.BtnImportCSVToList.Location = new System.Drawing.Point(6, 25);
            this.BtnImportCSVToList.Name = "BtnImportCSVToList";
            this.BtnImportCSVToList.Size = new System.Drawing.Size(250, 33);
            this.BtnImportCSVToList.TabIndex = 5;
            this.BtnImportCSVToList.Text = "Import Tags From CSV";
            this.BtnImportCSVToList.UseVisualStyleBackColor = true;
            this.BtnImportCSVToList.Click += new System.EventHandler(this.BtnImportCSVToList_Click);
            // 
            // TagListSettingsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.TxtNewTagInput);
            this.Controls.Add(this.BtnMoveTagDown);
            this.Controls.Add(this.BtnMoveTagUp);
            this.Controls.Add(this.BtnRemoveTagFromList);
            this.Controls.Add(this.BtnAddTagToList);
            this.Controls.Add(this.LstTags);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "TagListSettingsPanel";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.Size = new System.Drawing.Size(609, 760);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListBox LstTags;
        private System.Windows.Forms.Button BtnRemoveTagFromList;
        private System.Windows.Forms.Button BtnAddTagToList;
        private System.Windows.Forms.CheckBox CbEnableAlphabeticalTagListSorting;
        private System.Windows.Forms.Button BtnClearTagList;
        private System.Windows.Forms.Button BtnExportCSVToFile;
        private System.Windows.Forms.TextBox TxtNewTagInput;
        private System.Windows.Forms.Button BtnMoveTagUp;
        private System.Windows.Forms.Button BtnMoveTagDown;
        private System.Windows.Forms.CheckBox CbShowTagsThatAreNotInTheTagsList;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button BtnImportCSVToList;
    }
}
