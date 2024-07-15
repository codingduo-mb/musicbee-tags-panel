namespace MusicBeePlugin
{
    partial class ChecklistBoxPanel
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
            this.CheckedListBoxWithTags = new System.Windows.Forms.CheckedListBox();
            this.SuspendLayout();
            // 
            // CheckedListBoxWithTags
            // 
            this.CheckedListBoxWithTags.BackColor = System.Drawing.SystemColors.Window;
            this.CheckedListBoxWithTags.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.CheckedListBoxWithTags.CheckOnClick = true;
            this.CheckedListBoxWithTags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CheckedListBoxWithTags.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.CheckedListBoxWithTags.IntegralHeight = false;
            this.CheckedListBoxWithTags.Location = new System.Drawing.Point(6, 6);
            this.CheckedListBoxWithTags.Margin = new System.Windows.Forms.Padding(0);
            this.CheckedListBoxWithTags.MultiColumn = true;
            this.CheckedListBoxWithTags.Name = "CheckedListBoxWithTags";
            this.CheckedListBoxWithTags.Size = new System.Drawing.Size(138, 138);
            this.CheckedListBoxWithTags.TabIndex = 1;
            this.CheckedListBoxWithTags.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CheckedListBox1_KeyDown);
            this.CheckedListBoxWithTags.KeyUp += new System.Windows.Forms.KeyEventHandler(this.CheckedListBox1_KeyUp);
            // 
            // ChecklistBoxPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.Controls.Add(this.CheckedListBoxWithTags);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "ChecklistBoxPanel";
            this.Padding = new System.Windows.Forms.Padding(6);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckedListBox CheckedListBoxWithTags;
    }
}
