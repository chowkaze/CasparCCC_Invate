namespace FotbollsVMKlocka.Controls
{
    partial class FaceBookItem
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
            this.tbName = new System.Windows.Forms.TextBox();
            this.tbComment = new System.Windows.Forms.TextBox();
            this.cbInclude = new System.Windows.Forms.CheckBox();
            this.labelNum = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(41, 5);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(100, 20);
            this.tbName.TabIndex = 2;
            // 
            // tbComment
            // 
            this.tbComment.Location = new System.Drawing.Point(146, 5);
            this.tbComment.Multiline = true;
            this.tbComment.Name = "tbComment";
            this.tbComment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbComment.Size = new System.Drawing.Size(177, 42);
            this.tbComment.TabIndex = 3;
            // 
            // cbInclude
            // 
            this.cbInclude.AutoSize = true;
            this.cbInclude.Location = new System.Drawing.Point(23, 5);
            this.cbInclude.Name = "cbInclude";
            this.cbInclude.Size = new System.Drawing.Size(15, 14);
            this.cbInclude.TabIndex = 1;
            this.cbInclude.UseVisualStyleBackColor = true;
            // 
            // labelNum
            // 
            this.labelNum.AutoSize = true;
            this.labelNum.Location = new System.Drawing.Point(3, 5);
            this.labelNum.Name = "labelNum";
            this.labelNum.Size = new System.Drawing.Size(19, 13);
            this.labelNum.TabIndex = 5;
            this.labelNum.Text = "99";
            // 
            // FaceBookItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelNum);
            this.Controls.Add(this.cbInclude);
            this.Controls.Add(this.tbComment);
            this.Controls.Add(this.tbName);
            this.Name = "FaceBookItem";
            this.Size = new System.Drawing.Size(330, 52);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.TextBox tbComment;
        private System.Windows.Forms.CheckBox cbInclude;
        private System.Windows.Forms.Label labelNum;
    }
}
