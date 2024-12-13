
namespace MusicBeePlugin
{
    partial class TagListSettingsForm
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
            this.components = new System.ComponentModel.Container();
            this.TabControlSettings = new System.Windows.Forms.TabControl();
            this.toolTipAddTagPage = new System.Windows.Forms.ToolTip(this.components);
            this.VersionLbl = new System.Windows.Forms.Label();
            this.BtnDiscardSettings = new System.Windows.Forms.Button();
            this.BtnSaveSettings = new System.Windows.Forms.Button();
            this.BtnRemoveMetaDataTypeTabPage = new System.Windows.Forms.Button();
            this.BtnAddMetaDataTypeTabPage = new System.Windows.Forms.Button();
            this.linkGitHub = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // TabControlSettings
            // 
            this.TabControlSettings.Location = new System.Drawing.Point(12, 76);
            this.TabControlSettings.Margin = new System.Windows.Forms.Padding(0);
            this.TabControlSettings.Name = "TabControlSettings";
            this.TabControlSettings.Padding = new System.Drawing.Point(5, 5);
            this.TabControlSettings.SelectedIndex = 0;
            this.TabControlSettings.Size = new System.Drawing.Size(600, 770);
            this.TabControlSettings.TabIndex = 0;
            // 
            // toolTipAddTagPage
            // 
            this.toolTipAddTagPage.AutomaticDelay = 1000;
            // 
            // VersionLbl
            // 
            this.VersionLbl.AutoSize = true;
            this.VersionLbl.Location = new System.Drawing.Point(474, 930);
            this.VersionLbl.Name = "VersionLbl";
            this.VersionLbl.Size = new System.Drawing.Size(0, 19);
            this.VersionLbl.TabIndex = 7;
            // 
            // BtnDiscardSettings
            // 
            this.BtnDiscardSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnDiscardSettings.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnDiscardSettings.Location = new System.Drawing.Point(423, 857);
            this.BtnDiscardSettings.Name = "BtnDiscardSettings";
            this.BtnDiscardSettings.Size = new System.Drawing.Size(181, 50);
            this.BtnDiscardSettings.TabIndex = 4;
            this.BtnDiscardSettings.Text = "Discard";
            this.BtnDiscardSettings.UseVisualStyleBackColor = true;
            // 
            // BtnSaveSettings
            // 
            this.BtnSaveSettings.Location = new System.Drawing.Point(12, 852);
            this.BtnSaveSettings.Name = "BtnSaveSettings";
            this.BtnSaveSettings.Size = new System.Drawing.Size(181, 50);
            this.BtnSaveSettings.TabIndex = 3;
            this.BtnSaveSettings.Text = "Save Settings";
            this.BtnSaveSettings.UseVisualStyleBackColor = true;
            // 
            // BtnRemoveMetaDataTypeTabPage
            // 
            this.BtnRemoveMetaDataTypeTabPage.Location = new System.Drawing.Point(429, 12);
            this.BtnRemoveMetaDataTypeTabPage.Name = "BtnRemoveMetaDataTypeTabPage";
            this.BtnRemoveMetaDataTypeTabPage.Size = new System.Drawing.Size(180, 50);
            this.BtnRemoveMetaDataTypeTabPage.TabIndex = 2;
            this.BtnRemoveMetaDataTypeTabPage.Text = "Remove Metadata Type";
            this.BtnRemoveMetaDataTypeTabPage.UseVisualStyleBackColor = true;
            this.BtnRemoveMetaDataTypeTabPage.Click += new System.EventHandler(this.OnRemoveTagPageButtonClick);
            // 
            // BtnAddMetaDataTypeTabPage
            // 
            this.BtnAddMetaDataTypeTabPage.Location = new System.Drawing.Point(12, 12);
            this.BtnAddMetaDataTypeTabPage.Name = "BtnAddMetaDataTypeTabPage";
            this.BtnAddMetaDataTypeTabPage.Size = new System.Drawing.Size(180, 50);
            this.BtnAddMetaDataTypeTabPage.TabIndex = 1;
            this.BtnAddMetaDataTypeTabPage.Text = "Add Metadata Type";
            this.BtnAddMetaDataTypeTabPage.UseVisualStyleBackColor = true;
            this.BtnAddMetaDataTypeTabPage.Click += new System.EventHandler(this.OnAddTagPageButtonClick);
            // 
            // linkGitHub
            // 
            this.linkGitHub.AutoSize = true;
            this.linkGitHub.Location = new System.Drawing.Point(17, 930);
            this.linkGitHub.Name = "linkGitHub";
            this.linkGitHub.Size = new System.Drawing.Size(130, 19);
            this.linkGitHub.TabIndex = 5;
            this.linkGitHub.TabStop = true;
            this.linkGitHub.Text = "Visit Me On GitHub";
            this.linkGitHub.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkGitHub_LinkClicked);
            // 
            // TagListSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.BtnDiscardSettings;
            this.ClientSize = new System.Drawing.Size(622, 953);
            this.Controls.Add(this.BtnDiscardSettings);
            this.Controls.Add(this.BtnSaveSettings);
            this.Controls.Add(this.VersionLbl);
            this.Controls.Add(this.linkGitHub);
            this.Controls.Add(this.BtnRemoveMetaDataTypeTabPage);
            this.Controls.Add(this.BtnAddMetaDataTypeTabPage);
            this.Controls.Add(this.TabControlSettings);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TagListSettingsForm";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Tags-Panel Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl TabControlSettings;
        private System.Windows.Forms.ToolTip toolTipAddTagPage;
        private System.Windows.Forms.Label VersionLbl;
        private System.Windows.Forms.Button BtnDiscardSettings;
        private System.Windows.Forms.Button BtnSaveSettings;
        private System.Windows.Forms.Button BtnRemoveMetaDataTypeTabPage;
        private System.Windows.Forms.Button BtnAddMetaDataTypeTabPage;
        private System.Windows.Forms.LinkLabel linkGitHub;
    }
}