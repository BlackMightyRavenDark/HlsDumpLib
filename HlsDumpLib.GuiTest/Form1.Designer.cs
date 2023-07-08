namespace HlsDumpLib.GuiTest
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxTitle = new System.Windows.Forms.TextBox();
            this.textBoxUrl = new System.Windows.Forms.TextBox();
            this.listViewStreams = new System.Windows.Forms.ListView();
            this.columnHeaderTitle = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFileSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderNewChunks = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderDateStarted = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderState = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderPlaylistUrl = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnAdd = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miCheckToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.miCancelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.columnHeaderDelay = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Название:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(114, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Ссылка на плейлист:";
            // 
            // textBoxTitle
            // 
            this.textBoxTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTitle.Location = new System.Drawing.Point(138, 12);
            this.textBoxTitle.Name = "textBoxTitle";
            this.textBoxTitle.Size = new System.Drawing.Size(650, 20);
            this.textBoxTitle.TabIndex = 2;
            // 
            // textBoxUrl
            // 
            this.textBoxUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxUrl.Location = new System.Drawing.Point(138, 38);
            this.textBoxUrl.Name = "textBoxUrl";
            this.textBoxUrl.Size = new System.Drawing.Size(650, 20);
            this.textBoxUrl.TabIndex = 3;
            // 
            // listViewStreams
            // 
            this.listViewStreams.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewStreams.BackColor = System.Drawing.SystemColors.Control;
            this.listViewStreams.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderTitle,
            this.columnHeaderFileName,
            this.columnHeaderFileSize,
            this.columnHeaderNewChunks,
            this.columnHeaderDelay,
            this.columnHeaderDateStarted,
            this.columnHeaderState,
            this.columnHeaderPlaylistUrl});
            this.listViewStreams.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.listViewStreams.FullRowSelect = true;
            this.listViewStreams.HideSelection = false;
            this.listViewStreams.Location = new System.Drawing.Point(11, 97);
            this.listViewStreams.MultiSelect = false;
            this.listViewStreams.Name = "listViewStreams";
            this.listViewStreams.Size = new System.Drawing.Size(777, 180);
            this.listViewStreams.TabIndex = 4;
            this.listViewStreams.UseCompatibleStateImageBehavior = false;
            this.listViewStreams.View = System.Windows.Forms.View.Details;
            this.listViewStreams.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDoubleClick);
            this.listViewStreams.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listViewStreams_MouseUp);
            // 
            // columnHeaderTitle
            // 
            this.columnHeaderTitle.Text = "Название";
            this.columnHeaderTitle.Width = 100;
            // 
            // columnHeaderFileName
            // 
            this.columnHeaderFileName.Text = "Имя файла";
            this.columnHeaderFileName.Width = 150;
            // 
            // columnHeaderFileSize
            // 
            this.columnHeaderFileSize.Text = "Размер файла";
            this.columnHeaderFileSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderFileSize.Width = 120;
            // 
            // columnHeaderNewChunks
            // 
            this.columnHeaderNewChunks.Text = "Новые чанки";
            this.columnHeaderNewChunks.Width = 100;
            // 
            // columnHeaderDateStarted
            // 
            this.columnHeaderDateStarted.Text = "Дамп начат";
            this.columnHeaderDateStarted.Width = 140;
            // 
            // columnHeaderState
            // 
            this.columnHeaderState.Text = "Состояние";
            this.columnHeaderState.Width = 100;
            // 
            // columnHeaderPlaylistUrl
            // 
            this.columnHeaderPlaylistUrl.Text = "Ссылка на плейлист";
            this.columnHeaderPlaylistUrl.Width = 300;
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.Location = new System.Drawing.Point(713, 66);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 25);
            this.btnAdd.TabIndex = 5;
            this.btnAdd.Text = "Добавить";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miCheckToolStripMenuItem,
            this.miCancelToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(148, 52);
            // 
            // miCheckToolStripMenuItem
            // 
            this.miCheckToolStripMenuItem.Name = "miCheckToolStripMenuItem";
            this.miCheckToolStripMenuItem.Size = new System.Drawing.Size(147, 24);
            this.miCheckToolStripMenuItem.Text = "Проверить";
            this.miCheckToolStripMenuItem.Click += new System.EventHandler(this.miCheckToolStripMenuItem_Click);
            // 
            // miCancelToolStripMenuItem
            // 
            this.miCancelToolStripMenuItem.Name = "miCancelToolStripMenuItem";
            this.miCancelToolStripMenuItem.Size = new System.Drawing.Size(147, 24);
            this.miCancelToolStripMenuItem.Text = "Отменить";
            this.miCancelToolStripMenuItem.Click += new System.EventHandler(this.miCancelToolStripMenuItem_Click);
            // 
            // columnHeaderDelay
            // 
            this.columnHeaderDelay.Text = "Задержка";
            this.columnHeaderDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderDelay.Width = 80;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 289);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.listViewStreams);
            this.Controls.Add(this.textBoxUrl);
            this.Controls.Add(this.textBoxTitle);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.DoubleBuffered = true;
            this.Name = "Form1";
            this.Text = "GUI test";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxTitle;
        private System.Windows.Forms.TextBox textBoxUrl;
        private System.Windows.Forms.ListView listViewStreams;
        private System.Windows.Forms.ColumnHeader columnHeaderTitle;
        private System.Windows.Forms.ColumnHeader columnHeaderFileName;
        private System.Windows.Forms.ColumnHeader columnHeaderFileSize;
        private System.Windows.Forms.ColumnHeader columnHeaderPlaylistUrl;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.ColumnHeader columnHeaderDateStarted;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem miCheckToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem miCancelToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader columnHeaderState;
        private System.Windows.Forms.ColumnHeader columnHeaderNewChunks;
        private System.Windows.Forms.ColumnHeader columnHeaderDelay;
    }
}

