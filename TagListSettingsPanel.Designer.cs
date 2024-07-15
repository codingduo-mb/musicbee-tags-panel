
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
            this.BtnImportCSVToList = new System.Windows.Forms.Button();
            this.CbShowTagsThatAreNotInTheTagsList = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // LstTags
            // 
            this.LstTags.BackColor = System.Drawing.Color.White;
            this.LstTags.FormattingEnabled = true;
            this.LstTags.ItemHeight = 19;
            this.LstTags.Location = new System.Drawing.Point(6, 6);
            this.LstTags.Name = "LstTags";
            this.LstTags.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LstTags.Size = new System.Drawing.Size(388, 460);
            this.LstTags.TabIndex = 7;
            // 
            // BtnRemoveTagFromList
            // 
            this.BtnRemoveTagFromList.Location = new System.Drawing.Point(201, 504);
            this.BtnRemoveTagFromList.Name = "BtnRemoveTagFromList";
            this.BtnRemoveTagFromList.Size = new System.Drawing.Size(111, 33);
            this.BtnRemoveTagFromList.TabIndex = 2;
            this.BtnRemoveTagFromList.Text = "Remove";
            this.BtnRemoveTagFromList.UseVisualStyleBackColor = true;
            this.BtnRemoveTagFromList.Click += new System.EventHandler(this.BtnRemTag_Click);
            // 
            // BtnAddTagToList
            // 
            this.BtnAddTagToList.Location = new System.Drawing.Point(62, 504);
            this.BtnAddTagToList.Name = "BtnAddTagToList";
            this.BtnAddTagToList.Size = new System.Drawing.Size(111, 33);
            this.BtnAddTagToList.TabIndex = 1;
            this.BtnAddTagToList.Text = "Add";
            this.BtnAddTagToList.UseVisualStyleBackColor = true;
            this.BtnAddTagToList.Click += new System.EventHandler(this.BtnAddTag_Click);
            // 
            // CbEnableAlphabeticalTagListSorting
            // 
            this.CbEnableAlphabeticalTagListSorting.AutoSize = true;
            this.CbEnableAlphabeticalTagListSorting.Checked = true;
            this.CbEnableAlphabeticalTagListSorting.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CbEnableAlphabeticalTagListSorting.Location = new System.Drawing.Point(62, 543);
            this.CbEnableAlphabeticalTagListSorting.Name = "CbEnableAlphabeticalTagListSorting";
            this.CbEnableAlphabeticalTagListSorting.Size = new System.Drawing.Size(172, 23);
            this.CbEnableAlphabeticalTagListSorting.TabIndex = 3;
            this.CbEnableAlphabeticalTagListSorting.Text = "Sort tags alphabetically";
            this.CbEnableAlphabeticalTagListSorting.UseVisualStyleBackColor = true;
            // 
            // BtnClearTagList
            // 
            this.BtnClearTagList.Location = new System.Drawing.Point(62, 694);
            this.BtnClearTagList.Name = "BtnClearTagList";
            this.BtnClearTagList.Size = new System.Drawing.Size(250, 33);
            this.BtnClearTagList.TabIndex = 6;
            this.BtnClearTagList.Text = "Clear list";
            this.BtnClearTagList.UseVisualStyleBackColor = true;
            this.BtnClearTagList.Click += new System.EventHandler(this.BtnClearTagSettings_Click);
            // 
            // BtnExportCSVToFile
            // 
            this.BtnExportCSVToFile.Location = new System.Drawing.Point(62, 656);
            this.BtnExportCSVToFile.Name = "BtnExportCSVToFile";
            this.BtnExportCSVToFile.Size = new System.Drawing.Size(250, 33);
            this.BtnExportCSVToFile.TabIndex = 5;
            this.BtnExportCSVToFile.Text = "Export tags to CSV";
            this.BtnExportCSVToFile.UseVisualStyleBackColor = true;
            this.BtnExportCSVToFile.Click += new System.EventHandler(this.BtnExportCsv_Click);
            // 
            // TxtNewTagInput
            // 
            this.TxtNewTagInput.Location = new System.Drawing.Point(6, 472);
            this.TxtNewTagInput.Name = "TxtNewTagInput";
            this.TxtNewTagInput.Size = new System.Drawing.Size(352, 26);
            this.TxtNewTagInput.TabIndex = 0;
            // 
            // BtnMoveTagUp
            // 
            this.BtnMoveTagUp.Location = new System.Drawing.Point(364, 470);
            this.BtnMoveTagUp.Name = "BtnMoveTagUp";
            this.BtnMoveTagUp.Size = new System.Drawing.Size(30, 28);
            this.BtnMoveTagUp.TabIndex = 2;
            this.BtnMoveTagUp.Text = "▲";
            this.BtnMoveTagUp.UseVisualStyleBackColor = true;
            this.BtnMoveTagUp.Click += new System.EventHandler(this.BtnMoveTagUpSettings_Click);
            // 
            // BtnMoveTagDown
            // 
            this.BtnMoveTagDown.Location = new System.Drawing.Point(364, 504);
            this.BtnMoveTagDown.Name = "BtnMoveTagDown";
            this.BtnMoveTagDown.Size = new System.Drawing.Size(30, 28);
            this.BtnMoveTagDown.TabIndex = 2;
            this.BtnMoveTagDown.Text = "▼";
            this.BtnMoveTagDown.UseVisualStyleBackColor = true;
            this.BtnMoveTagDown.Click += new System.EventHandler(this.BtnMoveTagDownSettings_Click);
            // 
            // BtnImportCSVToList
            // 
            this.BtnImportCSVToList.Location = new System.Drawing.Point(62, 617);
            this.BtnImportCSVToList.Name = "BtnImportCSVToList";
            this.BtnImportCSVToList.Size = new System.Drawing.Size(250, 33);
            this.BtnImportCSVToList.TabIndex = 4;
            this.BtnImportCSVToList.Text = "Import tags from CSV";
            this.BtnImportCSVToList.UseVisualStyleBackColor = true;
            this.BtnImportCSVToList.Click += new System.EventHandler(this.BtnImportCsv_Click);
            // 
            // CbShowTagsThatAreNotInTheTagsList
            // 
            this.CbShowTagsThatAreNotInTheTagsList.AutoSize = true;
            this.CbShowTagsThatAreNotInTheTagsList.Location = new System.Drawing.Point(62, 573);
            this.CbShowTagsThatAreNotInTheTagsList.Name = "CbShowTagsThatAreNotInTheTagsList";
            this.CbShowTagsThatAreNotInTheTagsList.Size = new System.Drawing.Size(231, 23);
            this.CbShowTagsThatAreNotInTheTagsList.TabIndex = 8;
            this.CbShowTagsThatAreNotInTheTagsList.Text = "Show tags that are not in the list";
            this.CbShowTagsThatAreNotInTheTagsList.UseVisualStyleBackColor = true;
            // 
            // TagListSettingsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.CbShowTagsThatAreNotInTheTagsList);
            this.Controls.Add(this.TxtNewTagInput);
            this.Controls.Add(this.BtnExportCSVToFile);
            this.Controls.Add(this.BtnClearTagList);
            this.Controls.Add(this.BtnImportCSVToList);
            this.Controls.Add(this.BtnMoveTagDown);
            this.Controls.Add(this.BtnMoveTagUp);
            this.Controls.Add(this.BtnRemoveTagFromList);
            this.Controls.Add(this.BtnAddTagToList);
            this.Controls.Add(this.CbEnableAlphabeticalTagListSorting);
            this.Controls.Add(this.LstTags);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "TagListSettingsPanel";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.Size = new System.Drawing.Size(400, 746);
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
        private System.Windows.Forms.Button BtnImportCSVToList;
        private System.Windows.Forms.CheckBox CbShowTagsThatAreNotInTheTagsList;
    }
}
