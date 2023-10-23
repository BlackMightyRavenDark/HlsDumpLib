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
            this.columnHeaderDelay = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderChunkProcessingTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderChunkId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderChunkLength = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderChunkFileSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderChunkFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderChunkUrl = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFirstChunkSession = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderProcessedChunks = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderLostChunks = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderDateStarted = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderState = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderPlaylistErrors = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderChunkDownloadErrors = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderChunkAppendErrors = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderOtherErroors = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderPlaylistUrl = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnAdd = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miCheckToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.miCancelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBoxSaveChunksInfo = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.numericUpDownPlaylistErrorCountInRow = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownOtherErrorCountInRow = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownPlaylistCheckingInterval = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.checkBoxSaveChunkFileName = new System.Windows.Forms.CheckBox();
            this.checkBoxSaveChunkUrl = new System.Windows.Forms.CheckBox();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPlaylistErrorCountInRow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOtherErrorCountInRow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPlaylistCheckingInterval)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(146, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Название (необязательно):";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(114, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Ссылка на плейлист:";
            // 
            // textBoxTitle
            // 
            this.textBoxTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTitle.Location = new System.Drawing.Point(164, 12);
            this.textBoxTitle.Name = "textBoxTitle";
            this.textBoxTitle.Size = new System.Drawing.Size(624, 20);
            this.textBoxTitle.TabIndex = 2;
            // 
            // textBoxUrl
            // 
            this.textBoxUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxUrl.Location = new System.Drawing.Point(164, 38);
            this.textBoxUrl.Name = "textBoxUrl";
            this.textBoxUrl.Size = new System.Drawing.Size(624, 20);
            this.textBoxUrl.TabIndex = 3;
            // 
            // listViewStreams
            // 
            this.listViewStreams.AllowColumnReorder = true;
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
            this.columnHeaderChunkProcessingTime,
            this.columnHeaderChunkId,
            this.columnHeaderChunkLength,
            this.columnHeaderChunkFileSize,
            this.columnHeaderChunkFileName,
            this.columnHeaderChunkUrl,
            this.columnHeaderFirstChunkSession,
            this.columnHeaderProcessedChunks,
            this.columnHeaderLostChunks,
            this.columnHeaderDateStarted,
            this.columnHeaderState,
            this.columnHeaderPlaylistErrors,
            this.columnHeaderChunkDownloadErrors,
            this.columnHeaderChunkAppendErrors,
            this.columnHeaderOtherErroors,
            this.columnHeaderPlaylistUrl});
            this.listViewStreams.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.listViewStreams.FullRowSelect = true;
            this.listViewStreams.HideSelection = false;
            this.listViewStreams.Location = new System.Drawing.Point(11, 164);
            this.listViewStreams.MultiSelect = false;
            this.listViewStreams.Name = "listViewStreams";
            this.listViewStreams.Size = new System.Drawing.Size(777, 179);
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
            // columnHeaderDelay
            // 
            this.columnHeaderDelay.Text = "Задержка";
            this.columnHeaderDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderDelay.Width = 130;
            // 
            // columnHeaderChunkProcessingTime
            // 
            this.columnHeaderChunkProcessingTime.Text = "Обработка чанка";
            this.columnHeaderChunkProcessingTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderChunkProcessingTime.Width = 100;
            // 
            // columnHeaderChunkId
            // 
            this.columnHeaderChunkId.Text = "ID чанка";
            // 
            // columnHeaderChunkLength
            // 
            this.columnHeaderChunkLength.Text = "Продолжительность чанка (секунды)";
            // 
            // columnHeaderChunkFileSize
            // 
            this.columnHeaderChunkFileSize.Text = "Размер чанка";
            this.columnHeaderChunkFileSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderChunkFileSize.Width = 110;
            // 
            // columnHeaderChunkFileName
            // 
            this.columnHeaderChunkFileName.Text = "Имя файла чанка";
            // 
            // columnHeaderChunkUrl
            // 
            this.columnHeaderChunkUrl.Text = "Ссылка на чанк";
            // 
            // columnHeaderFirstChunkSession
            // 
            this.columnHeaderFirstChunkSession.Text = "Первый чанк";
            this.columnHeaderFirstChunkSession.Width = 100;
            // 
            // columnHeaderProcessedChunks
            // 
            this.columnHeaderProcessedChunks.Text = "Обработано чанков";
            this.columnHeaderProcessedChunks.Width = 100;
            // 
            // columnHeaderLostChunks
            // 
            this.columnHeaderLostChunks.Text = "Потеряно чанков";
            this.columnHeaderLostChunks.Width = 100;
            // 
            // columnHeaderDateStarted
            // 
            this.columnHeaderDateStarted.Text = "Дамп начат";
            this.columnHeaderDateStarted.Width = 140;
            // 
            // columnHeaderState
            // 
            this.columnHeaderState.Text = "Состояние";
            this.columnHeaderState.Width = 230;
            // 
            // columnHeaderPlaylistErrors
            // 
            this.columnHeaderPlaylistErrors.Text = "Ошибки плейлиста";
            this.columnHeaderPlaylistErrors.Width = 150;
            // 
            // columnHeaderChunkDownloadErrors
            // 
            this.columnHeaderChunkDownloadErrors.Text = "Ошибки скачивания чанков";
            this.columnHeaderChunkDownloadErrors.Width = 90;
            // 
            // columnHeaderChunkAppendErrors
            // 
            this.columnHeaderChunkAppendErrors.Text = "Ошибки объединения чанков";
            this.columnHeaderChunkAppendErrors.Width = 90;
            // 
            // columnHeaderOtherErroors
            // 
            this.columnHeaderOtherErroors.Text = "Другие ошибки";
            this.columnHeaderOtherErroors.Width = 100;
            // 
            // columnHeaderPlaylistUrl
            // 
            this.columnHeaderPlaylistUrl.Text = "Ссылка на плейлист";
            this.columnHeaderPlaylistUrl.Width = 300;
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.Location = new System.Drawing.Point(713, 136);
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
            this.contextMenuStrip1.Size = new System.Drawing.Size(135, 48);
            // 
            // miCheckToolStripMenuItem
            // 
            this.miCheckToolStripMenuItem.Name = "miCheckToolStripMenuItem";
            this.miCheckToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
            this.miCheckToolStripMenuItem.Text = "Проверить";
            this.miCheckToolStripMenuItem.Click += new System.EventHandler(this.miCheckToolStripMenuItem_Click);
            // 
            // miCancelToolStripMenuItem
            // 
            this.miCancelToolStripMenuItem.Name = "miCancelToolStripMenuItem";
            this.miCancelToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
            this.miCancelToolStripMenuItem.Text = "Отменить";
            this.miCancelToolStripMenuItem.Click += new System.EventHandler(this.miCancelToolStripMenuItem_Click);
            // 
            // checkBoxSaveChunksInfo
            // 
            this.checkBoxSaveChunksInfo.AutoSize = true;
            this.checkBoxSaveChunksInfo.Checked = true;
            this.checkBoxSaveChunksInfo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSaveChunksInfo.Location = new System.Drawing.Point(12, 141);
            this.checkBoxSaveChunksInfo.Name = "checkBoxSaveChunksInfo";
            this.checkBoxSaveChunksInfo.Size = new System.Drawing.Size(194, 17);
            this.checkBoxSaveChunksInfo.TabIndex = 6;
            this.checkBoxSaveChunksInfo.Text = "Сохранять информацию о чанках";
            this.toolTip1.SetToolTip(this.checkBoxSaveChunksInfo, "Невозможно изменить для уже добавленных элементов");
            this.checkBoxSaveChunksInfo.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 94);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(200, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Максимум ошибок плейлиста подряд:";
            this.toolTip1.SetToolTip(this.label3, "Невозможно изменить для уже добавленных элементов");
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 120);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(180, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Максимум других ошибок подряд:";
            this.toolTip1.SetToolTip(this.label4, "Невозможно изменить для уже добавленных элементов");
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 68);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(159, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Частота проверки плейлиста:";
            this.toolTip1.SetToolTip(this.label5, "Невозможно изменить для уже добавленных элементов");
            // 
            // numericUpDownPlaylistErrorCountInRow
            // 
            this.numericUpDownPlaylistErrorCountInRow.Location = new System.Drawing.Point(218, 92);
            this.numericUpDownPlaylistErrorCountInRow.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownPlaylistErrorCountInRow.Name = "numericUpDownPlaylistErrorCountInRow";
            this.numericUpDownPlaylistErrorCountInRow.Size = new System.Drawing.Size(53, 20);
            this.numericUpDownPlaylistErrorCountInRow.TabIndex = 9;
            this.numericUpDownPlaylistErrorCountInRow.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // numericUpDownOtherErrorCountInRow
            // 
            this.numericUpDownOtherErrorCountInRow.Location = new System.Drawing.Point(218, 118);
            this.numericUpDownOtherErrorCountInRow.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownOtherErrorCountInRow.Name = "numericUpDownOtherErrorCountInRow";
            this.numericUpDownOtherErrorCountInRow.Size = new System.Drawing.Size(53, 20);
            this.numericUpDownOtherErrorCountInRow.TabIndex = 10;
            this.numericUpDownOtherErrorCountInRow.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // numericUpDownPlaylistCheckingInterval
            // 
            this.numericUpDownPlaylistCheckingInterval.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownPlaylistCheckingInterval.Location = new System.Drawing.Point(218, 66);
            this.numericUpDownPlaylistCheckingInterval.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownPlaylistCheckingInterval.Minimum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDownPlaylistCheckingInterval.Name = "numericUpDownPlaylistCheckingInterval";
            this.numericUpDownPlaylistCheckingInterval.Size = new System.Drawing.Size(53, 20);
            this.numericUpDownPlaylistCheckingInterval.TabIndex = 11;
            this.numericUpDownPlaylistCheckingInterval.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(277, 68);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(74, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "миллисекунд";
            // 
            // checkBoxSaveChunkFileName
            // 
            this.checkBoxSaveChunkFileName.AutoSize = true;
            this.checkBoxSaveChunkFileName.Checked = true;
            this.checkBoxSaveChunkFileName.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSaveChunkFileName.Location = new System.Drawing.Point(212, 141);
            this.checkBoxSaveChunkFileName.Name = "checkBoxSaveChunkFileName";
            this.checkBoxSaveChunkFileName.Size = new System.Drawing.Size(169, 17);
            this.checkBoxSaveChunkFileName.TabIndex = 14;
            this.checkBoxSaveChunkFileName.Text = "Сохранять имя файла чанка";
            this.toolTip1.SetToolTip(this.checkBoxSaveChunkFileName, "Невозможно изменить для уже добавленных элементов");
            this.checkBoxSaveChunkFileName.UseVisualStyleBackColor = true;
            // 
            // checkBoxSaveChunkUrl
            // 
            this.checkBoxSaveChunkUrl.AutoSize = true;
            this.checkBoxSaveChunkUrl.Checked = true;
            this.checkBoxSaveChunkUrl.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSaveChunkUrl.Location = new System.Drawing.Point(387, 141);
            this.checkBoxSaveChunkUrl.Name = "checkBoxSaveChunkUrl";
            this.checkBoxSaveChunkUrl.Size = new System.Drawing.Size(160, 17);
            this.checkBoxSaveChunkUrl.TabIndex = 15;
            this.checkBoxSaveChunkUrl.Text = "Сохранять ссылку на чанк";
            this.toolTip1.SetToolTip(this.checkBoxSaveChunkUrl, "Невозможно изменить для уже добавленных элементов");
            this.checkBoxSaveChunkUrl.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 355);
            this.Controls.Add(this.checkBoxSaveChunkUrl);
            this.Controls.Add(this.checkBoxSaveChunkFileName);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.numericUpDownPlaylistCheckingInterval);
            this.Controls.Add(this.numericUpDownOtherErrorCountInRow);
            this.Controls.Add(this.numericUpDownPlaylistErrorCountInRow);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.checkBoxSaveChunksInfo);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.listViewStreams);
            this.Controls.Add(this.textBoxUrl);
            this.Controls.Add(this.textBoxTitle);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.DoubleBuffered = true;
            this.MinimumSize = new System.Drawing.Size(650, 300);
            this.Name = "Form1";
            this.Text = "GUI test";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPlaylistErrorCountInRow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOtherErrorCountInRow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPlaylistCheckingInterval)).EndInit();
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
        private System.Windows.Forms.CheckBox checkBoxSaveChunksInfo;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ColumnHeader columnHeaderFirstChunkSession;
        private System.Windows.Forms.ColumnHeader columnHeaderProcessedChunks;
        private System.Windows.Forms.ColumnHeader columnHeaderLostChunks;
        private System.Windows.Forms.ColumnHeader columnHeaderChunkProcessingTime;
        private System.Windows.Forms.ColumnHeader columnHeaderPlaylistErrors;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDownPlaylistErrorCountInRow;
        private System.Windows.Forms.NumericUpDown numericUpDownOtherErrorCountInRow;
        private System.Windows.Forms.ColumnHeader columnHeaderOtherErroors;
        private System.Windows.Forms.ColumnHeader columnHeaderChunkFileSize;
        private System.Windows.Forms.NumericUpDown numericUpDownPlaylistCheckingInterval;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ColumnHeader columnHeaderChunkDownloadErrors;
        private System.Windows.Forms.ColumnHeader columnHeaderChunkAppendErrors;
        private System.Windows.Forms.ColumnHeader columnHeaderChunkId;
        private System.Windows.Forms.ColumnHeader columnHeaderChunkLength;
        private System.Windows.Forms.ColumnHeader columnHeaderChunkUrl;
        private System.Windows.Forms.ColumnHeader columnHeaderChunkFileName;
        private System.Windows.Forms.CheckBox checkBoxSaveChunkFileName;
        private System.Windows.Forms.CheckBox checkBoxSaveChunkUrl;
    }
}

