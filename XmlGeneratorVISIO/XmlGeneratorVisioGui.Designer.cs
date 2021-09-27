
namespace XmlGeneratorVISIO
{
    partial class XmlGeneratorVisioGui
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.XmlGeneratorButton = new System.Windows.Forms.Button();
            this.OpenFile = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // XmlGeneratorButton
            // 
            this.XmlGeneratorButton.Location = new System.Drawing.Point(69, 73);
            this.XmlGeneratorButton.Name = "XmlGeneratorButton";
            this.XmlGeneratorButton.Size = new System.Drawing.Size(120, 28);
            this.XmlGeneratorButton.TabIndex = 0;
            this.XmlGeneratorButton.Text = "SaveFile";
            this.XmlGeneratorButton.UseVisualStyleBackColor = true;
            this.XmlGeneratorButton.Click += new System.EventHandler(this.SaveFile_Click);
            // 
            // OpenFile
            // 
            this.OpenFile.Location = new System.Drawing.Point(69, 12);
            this.OpenFile.Name = "OpenFile";
            this.OpenFile.Size = new System.Drawing.Size(120, 28);
            this.OpenFile.TabIndex = 1;
            this.OpenFile.Text = "OpenFile";
            this.OpenFile.UseVisualStyleBackColor = true;
            this.OpenFile.Click += new System.EventHandler(this.OpenFile_Click);
            // 
            // XmlGeneratorVisioGui
            // 
            this.ClientSize = new System.Drawing.Size(256, 126);
            this.Controls.Add(this.OpenFile);
            this.Controls.Add(this.XmlGeneratorButton);
            this.Name = "XmlGeneratorVisioGui";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button XmlGeneratorButton;
        private System.Windows.Forms.Button OpenFile;
    }
}

