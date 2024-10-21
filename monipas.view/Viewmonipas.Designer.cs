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
            panel1 = new Panel();
            listBox = new ListBox();
            button1 = new Button();
            label1 = new Label();
            OpenJson = new Button();
            OpenLogs = new Button();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.ActiveCaption;
            panel1.Controls.Add(listBox);
            panel1.Location = new Point(29, 77);
            panel1.Name = "panel1";
            panel1.Size = new Size(498, 523);
            panel1.TabIndex = 0;
            // 
            // listBox
            // 
            listBox.FormattingEnabled = true;
            listBox.ItemHeight = 15;
            listBox.Location = new Point(3, 3);
            listBox.Name = "listBox";
            listBox.Size = new Size(492, 514);
            listBox.TabIndex = 1;
            listBox.SelectedIndexChanged += listBox_SelectedIndexChanged;
            // 
            // button1
            // 
            button1.AccessibleDescription = "";
            button1.AccessibleName = "";
            button1.Location = new Point(627, 149);
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
            OpenJson.Location = new Point(627, 287);
            OpenJson.Name = "OpenJson";
            OpenJson.Size = new Size(126, 47);
            OpenJson.TabIndex = 4;
            OpenJson.Text = "Configuração de envio FTP";
            OpenJson.UseVisualStyleBackColor = true;
            OpenJson.Click += OpenJson_Click_1;
            // 
            // OpenLogs
            // 
            OpenLogs.Location = new Point(627, 415);
            OpenLogs.Name = "OpenLogs";
            OpenLogs.Size = new Size(126, 47);
            OpenLogs.TabIndex = 5;
            OpenLogs.Text = "Abrir LOG's";
            OpenLogs.UseVisualStyleBackColor = true;
            OpenLogs.Click += OpenLogs_Click_1;
            // 
            // Viewmonipas
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(829, 645);
            Controls.Add(OpenLogs);
            Controls.Add(OpenJson);
            Controls.Add(label1);
            Controls.Add(button1);
            Controls.Add(panel1);
            Name = "Viewmonipas";
            Text = "MONIPAS - Envio automatico de dados";
            Load += Viewmonipas_Load;
            panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panel1;
        private ListBox listBox;
        private Button button1;
        private Label label1;
        private Button OpenJson;
        private Button OpenLogs;
    }
}