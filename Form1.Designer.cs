using ScintillaNET;

namespace AutoAssemblyMatcher
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            scintilla = new Scintilla();
            scintilla1 = new Scintilla();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            comboBoxAssembly = new ComboBox();
            flowLayoutPanel2 = new FlowLayoutPanel();
            buttonAssociate = new Button();
            buttonSkip = new Button();
            buttonIgnore = new Button();
            listViewDummy = new ListView();
            columnHeader1 = new ColumnHeader();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // scintilla
            // 
            scintilla.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            scintilla.AutocompleteListSelectedBackColor = Color.FromArgb(0, 120, 212);
            scintilla.AutocompleteListTextColor = Color.Blue;
            scintilla.LexerName = null;
            scintilla.Location = new Point(2, 32);
            scintilla.Margin = new Padding(2);
            scintilla.Name = "scintilla";
            scintilla.ScrollWidth = 293;
            scintilla.Size = new Size(461, 679);
            scintilla.TabIndex = 0;
            scintilla.Text = "Loading...";
            scintilla.ZoomChanged += scintilla_ZoomChanged;
            scintilla.KeyDown += genericScintilla_KeyDown;
            // 
            // scintilla1
            // 
            scintilla1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            scintilla1.AutocompleteListSelectedBackColor = Color.FromArgb(0, 120, 212);
            scintilla1.AutocompleteListTextColor = Color.Blue;
            scintilla1.LexerName = null;
            scintilla1.Location = new Point(467, 32);
            scintilla1.Margin = new Padding(2);
            scintilla1.Name = "scintilla1";
            scintilla1.ScrollWidth = 293;
            scintilla1.Size = new Size(461, 679);
            scintilla1.TabIndex = 1;
            scintilla1.Text = "Loading...";
            scintilla1.ZoomChanged += scintilla1_ZoomChanged;
            scintilla1.KeyDown += genericScintilla_KeyDown;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 1, 2);
            tableLayoutPanel1.Controls.Add(comboBoxAssembly, 0, 0);
            tableLayoutPanel1.Controls.Add(scintilla, 0, 1);
            tableLayoutPanel1.Controls.Add(scintilla1, 1, 1);
            tableLayoutPanel1.Controls.Add(flowLayoutPanel2, 0, 2);
            tableLayoutPanel1.Location = new Point(8, 7);
            tableLayoutPanel1.Margin = new Padding(2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel1.Size = new Size(930, 743);
            tableLayoutPanel1.TabIndex = 2;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel2.ColumnCount = 2;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Location = new Point(468, 716);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Size = new Size(459, 24);
            tableLayoutPanel2.TabIndex = 3;
            // 
            // comboBoxAssembly
            // 
            comboBoxAssembly.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBoxAssembly.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxAssembly.FlatStyle = FlatStyle.Popup;
            comboBoxAssembly.FormattingEnabled = true;
            comboBoxAssembly.Location = new Point(2, 2);
            comboBoxAssembly.Margin = new Padding(2);
            comboBoxAssembly.Name = "comboBoxAssembly";
            comboBoxAssembly.Size = new Size(461, 23);
            comboBoxAssembly.TabIndex = 7;
            comboBoxAssembly.SelectedIndexChanged += comboBoxAssembly_SelectedIndexChanged;
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flowLayoutPanel2.Controls.Add(buttonAssociate);
            flowLayoutPanel2.Controls.Add(buttonSkip);
            flowLayoutPanel2.Controls.Add(buttonIgnore);
            flowLayoutPanel2.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel2.Location = new Point(2, 715);
            flowLayoutPanel2.Margin = new Padding(2);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new Size(461, 26);
            flowLayoutPanel2.TabIndex = 3;
            // 
            // buttonAssociate
            // 
            buttonAssociate.Anchor = AnchorStyles.None;
            buttonAssociate.Location = new Point(381, 2);
            buttonAssociate.Margin = new Padding(2);
            buttonAssociate.Name = "buttonAssociate";
            buttonAssociate.Size = new Size(78, 20);
            buttonAssociate.TabIndex = 4;
            buttonAssociate.Text = "Associate";
            buttonAssociate.UseVisualStyleBackColor = true;
            buttonAssociate.Click += buttonAssociate_Click;
            // 
            // buttonSkip
            // 
            buttonSkip.Anchor = AnchorStyles.None;
            buttonSkip.Location = new Point(299, 2);
            buttonSkip.Margin = new Padding(2);
            buttonSkip.Name = "buttonSkip";
            buttonSkip.Size = new Size(78, 20);
            buttonSkip.TabIndex = 3;
            buttonSkip.Text = "Skip";
            buttonSkip.UseVisualStyleBackColor = true;
            buttonSkip.Click += buttonSkip_Click;
            // 
            // buttonIgnore
            // 
            buttonIgnore.Anchor = AnchorStyles.None;
            buttonIgnore.Location = new Point(217, 2);
            buttonIgnore.Margin = new Padding(2);
            buttonIgnore.Name = "buttonIgnore";
            buttonIgnore.Size = new Size(78, 20);
            buttonIgnore.TabIndex = 2;
            buttonIgnore.Text = "Ignore";
            buttonIgnore.UseVisualStyleBackColor = true;
            buttonIgnore.Click += buttonIgnore_Click;
            // 
            // listViewDummy
            // 
            listViewDummy.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            listViewDummy.Columns.AddRange(new ColumnHeader[] { columnHeader1 });
            listViewDummy.FullRowSelect = true;
            listViewDummy.HeaderStyle = ColumnHeaderStyle.None;
            listViewDummy.Location = new Point(943, 39);
            listViewDummy.MultiSelect = false;
            listViewDummy.Name = "listViewDummy";
            listViewDummy.OwnerDraw = true;
            listViewDummy.Size = new Size(229, 679);
            listViewDummy.TabIndex = 3;
            listViewDummy.UseCompatibleStateImageBehavior = false;
            listViewDummy.View = View.Details;
            listViewDummy.DrawItem += listViewDummy_DrawItem;
            listViewDummy.SelectedIndexChanged += listViewDummy_SelectedIndexChanged;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "";
            columnHeader1.Width = 225;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1184, 761);
            Controls.Add(listViewDummy);
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(2);
            Name = "Form1";
            Text = "Form1";
            Shown += Form1_Shown;
            ResizeEnd += Form1_ResizeEnd;
            tableLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private ScintillaNET.Scintilla scintilla;
        private Scintilla scintilla1;
        private TableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel2;
        private Button buttonIgnore;
        private Button buttonSkip;
        private Button buttonAssociate;
        private ComboBox comboBoxAssembly;
        private TableLayoutPanel tableLayoutPanel2;
        private ListView listViewDummy;
        private ColumnHeader columnHeader1;
    }
}
