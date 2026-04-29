using ScintillaNET;

namespace AutoAssemblyMatcher.UI
{
    partial class MainForm
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
            scintillaAssembly = new Scintilla();
            scintillaDummy = new Scintilla();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanelDummyFooter = new TableLayoutPanel();
            comboBoxAssembly = new ComboBox();
            flowLayoutPanel2 = new FlowLayoutPanel();
            buttonAssociate = new Button();
            buttonSkip = new Button();
            buttonIgnore = new Button();
            listViewDummy = new ListView();
            columnHeaderDummyName = new ColumnHeader();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // scintillaAssembly
            // 
            scintillaAssembly.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            scintillaAssembly.AutocompleteListSelectedBackColor = Color.FromArgb(0, 120, 212);
            scintillaAssembly.AutocompleteListTextColor = Color.Blue;
            scintillaAssembly.LexerName = null;
            scintillaAssembly.Location = new Point(2, 32);
            scintillaAssembly.Margin = new Padding(2);
            scintillaAssembly.Name = "scintillaAssembly";
            scintillaAssembly.ScrollWidth = 293;
            scintillaAssembly.Size = new Size(491, 679);
            scintillaAssembly.TabIndex = 0;
            scintillaAssembly.Text = "Loading...";
            scintillaAssembly.ZoomChanged += scintillaAssembly_ZoomChanged;
            scintillaAssembly.KeyDown += genericScintilla_KeyDown;
            // 
            // scintillaDummy
            // 
            scintillaDummy.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            scintillaDummy.AutocompleteListSelectedBackColor = Color.FromArgb(0, 120, 212);
            scintillaDummy.AutocompleteListTextColor = Color.Blue;
            scintillaDummy.LexerName = null;
            scintillaDummy.Location = new Point(497, 32);
            scintillaDummy.Margin = new Padding(2);
            scintillaDummy.Name = "scintillaDummy";
            scintillaDummy.ScrollWidth = 293;
            scintillaDummy.Size = new Size(492, 679);
            scintillaDummy.TabIndex = 1;
            scintillaDummy.Text = "Loading...";
            scintillaDummy.ZoomChanged += scintillaDummy_ZoomChanged;
            scintillaDummy.KeyDown += genericScintilla_KeyDown;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanelDummyFooter, 1, 2);
            tableLayoutPanel1.Controls.Add(comboBoxAssembly, 0, 0);
            tableLayoutPanel1.Controls.Add(scintillaAssembly, 0, 1);
            tableLayoutPanel1.Controls.Add(scintillaDummy, 1, 1);
            tableLayoutPanel1.Controls.Add(flowLayoutPanel2, 0, 2);
            tableLayoutPanel1.Location = new Point(8, 7);
            tableLayoutPanel1.Margin = new Padding(2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel1.Size = new Size(991, 743);
            tableLayoutPanel1.TabIndex = 2;
            // 
            // tableLayoutPanelDummyFooter
            // 
            tableLayoutPanelDummyFooter.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanelDummyFooter.ColumnCount = 2;
            tableLayoutPanelDummyFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanelDummyFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanelDummyFooter.Location = new Point(498, 716);
            tableLayoutPanelDummyFooter.Name = "tableLayoutPanelDummyFooter";
            tableLayoutPanelDummyFooter.RowCount = 1;
            tableLayoutPanelDummyFooter.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanelDummyFooter.Size = new Size(490, 24);
            tableLayoutPanelDummyFooter.TabIndex = 3;
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
            comboBoxAssembly.Size = new Size(491, 23);
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
            flowLayoutPanel2.Size = new Size(491, 26);
            flowLayoutPanel2.TabIndex = 3;
            // 
            // buttonAssociate
            // 
            buttonAssociate.Anchor = AnchorStyles.None;
            buttonAssociate.Location = new Point(411, 2);
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
            buttonSkip.Location = new Point(329, 2);
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
            buttonIgnore.Location = new Point(247, 2);
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
            listViewDummy.Columns.AddRange(new ColumnHeader[] { columnHeaderDummyName });
            listViewDummy.FullRowSelect = true;
            listViewDummy.HeaderStyle = ColumnHeaderStyle.None;
            listViewDummy.Location = new Point(1004, 39);
            listViewDummy.MultiSelect = false;
            listViewDummy.Name = "listViewDummy";
            listViewDummy.OwnerDraw = true;
            listViewDummy.Size = new Size(312, 679);
            listViewDummy.TabIndex = 3;
            listViewDummy.UseCompatibleStateImageBehavior = false;
            listViewDummy.View = View.Details;
            listViewDummy.DrawItem += listViewDummy_DrawItem;
            listViewDummy.SelectedIndexChanged += listViewDummy_SelectedIndexChanged;
            // 
            // columnHeaderDummyName
            // 
            columnHeaderDummyName.Text = "";
            columnHeaderDummyName.Width = 225;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1328, 761);
            Controls.Add(listViewDummy);
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(2);
            Name = "MainForm";
            Text = "MainForm";
            Shown += MainForm_Shown;
            ResizeEnd += MainForm_ResizeEnd;
            tableLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private ScintillaNET.Scintilla scintillaAssembly;
        private Scintilla scintillaDummy;
        private TableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel2;
        private Button buttonIgnore;
        private Button buttonSkip;
        private Button buttonAssociate;
        private ComboBox comboBoxAssembly;
        private TableLayoutPanel tableLayoutPanelDummyFooter;
        private ListView listViewDummy;
        private ColumnHeader columnHeaderDummyName;
    }
}
