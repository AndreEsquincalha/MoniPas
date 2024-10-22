namespace MONIPAS.monipas.view
{
    partial class Viewmonipas
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Viewmonipas));
            button1 = new Button();
            label1 = new Label();
            OpenJson = new Button();
            OpenLogs = new Button();
            panel2 = new Panel();
            listBox = new ListBox();
            SuspendLayout();
            // 
            // button1
            // 
            button1.AccessibleDescription = "";
            button1.AccessibleName = "";
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.Location = new Point(474, 80);
            button1.Name = "button1";
            button1.Size = new Size(126, 47);
            button1.TabIndex = 1;
            button1.Text = "Reenviar Dados";
            button1.UseVisualStyleBackColor = true;
            button1.Click += reenviardados_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(32, 48);
            label1.Name = "label1";
            label1.Size = new Size(178, 17);
            label1.TabIndex = 2;
            label1.Text = "Arquivos de dados enviados:";
            label1.Click += label1_Click;
            // 
            // OpenJson
            // 
            OpenJson.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            OpenJson.Location = new Point(474, 159);
            OpenJson.Name = "OpenJson";
            OpenJson.Size = new Size(126, 47);
            OpenJson.TabIndex = 4;
            OpenJson.Text = "Configuração de envio FTP";
            OpenJson.UseVisualStyleBackColor = true;
            OpenJson.Click += OpenJson_Click_1;
            // 
            // OpenLogs
            // 
            OpenLogs.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            OpenLogs.Location = new Point(474, 238);
            OpenLogs.Name = "OpenLogs";
            OpenLogs.Size = new Size(126, 47);
            OpenLogs.TabIndex = 5;
            OpenLogs.Text = "LOG de dados não Enviados";
            OpenLogs.UseVisualStyleBackColor = true;
            OpenLogs.Click += OpenLogs_Click_1;
            // 
            // panel2
            // 
            panel2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            panel2.AutoSize = true;
            panel2.BackgroundImage = (Image)resources.GetObject("panel2.BackgroundImage");
            panel2.BackgroundImageLayout = ImageLayout.Zoom;
            panel2.BorderStyle = BorderStyle.FixedSingle;
            panel2.Location = new Point(474, 531);
            panel2.Name = "panel2";
            panel2.Size = new Size(126, 78);
            panel2.TabIndex = 6;
            // 
            // listBox
            // 
            listBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            listBox.BackColor = SystemColors.ScrollBar;
            listBox.FormattingEnabled = true;
            listBox.ItemHeight = 15;
            listBox.Location = new Point(32, 80);
            listBox.Name = "listBox";
            listBox.Size = new Size(422, 529);
            listBox.TabIndex = 1;
            listBox.SelectedIndexChanged += listBox_SelectedIndexChanged;
            // 
            // Viewmonipas
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(625, 644);
            Controls.Add(listBox);
            Controls.Add(panel2);
            Controls.Add(OpenLogs);
            Controls.Add(OpenJson);
            Controls.Add(label1);
            Controls.Add(button1);
            Name = "Viewmonipas";
            Text = "MONIPAS - Envio automatico de dados";
            Load += Viewmonipas_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button button1;
        private Label label1;
        private Button OpenJson;
        private Button OpenLogs;
        private Panel panel2;
        private ListBox listBox;
    }
}