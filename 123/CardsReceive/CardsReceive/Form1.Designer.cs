namespace CardsReceive
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
            flowLayoutPanelPlayer = new FlowLayoutPanel();
            lblRemainingCards = new Label();
            flowLayoutPanelOpponent = new FlowLayoutPanel();
            flowLayoutPanelTable = new FlowLayoutPanel();
            btnPlayCard = new Button();
            SuspendLayout();
            // 
            // 
            // flowLayoutPanelPlayer
            // 
            flowLayoutPanelPlayer.Location = new Point(105, 571);
            flowLayoutPanelPlayer.Name = "flowLayoutPanelPlayer";
            flowLayoutPanelPlayer.Size = new Size(645, 202);
            flowLayoutPanelPlayer.TabIndex = 1;
            // 
            // lblRemainingCards
            // 
            lblRemainingCards.AutoSize = true;
            lblRemainingCards.Location = new Point(3, 758);
            lblRemainingCards.Name = "lblRemainingCards";
            lblRemainingCards.Size = new Size(38, 15);
            lblRemainingCards.TabIndex = 2;
            lblRemainingCards.Text = "label1";
            // 
            // flowLayoutPanelOpponent
            // 
            flowLayoutPanelOpponent.Location = new Point(105, -3);
            flowLayoutPanelOpponent.Name = "flowLayoutPanelOpponent";
            flowLayoutPanelOpponent.Size = new Size(645, 209);
            flowLayoutPanelOpponent.TabIndex = 3;
            // 
            // flowLayoutPanelTable
            // 
            flowLayoutPanelTable.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            flowLayoutPanelTable.Location = new Point(149, 274);
            flowLayoutPanelTable.Name = "flowLayoutPanelTable";
            flowLayoutPanelTable.Size = new Size(531, 212);
            flowLayoutPanelTable.TabIndex = 4;
            // 
            // btnPlayCard
            // 
            btnPlayCard.Location = new Point(474, 513);
            btnPlayCard.Name = "btnPlayCard";
            btnPlayCard.Size = new Size(75, 23);
            btnPlayCard.TabIndex = 5;
            btnPlayCard.Text = "Атаковать";
            btnPlayCard.UseVisualStyleBackColor = true;
            btnPlayCard.Click += btnPlayCard_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 782);
            Controls.Add(btnPlayCard);
            Controls.Add(flowLayoutPanelTable);
            Controls.Add(flowLayoutPanelOpponent);
            Controls.Add(lblRemainingCards);
            Controls.Add(flowLayoutPanelPlayer);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private FlowLayoutPanel flowLayoutPanelPlayer;
        private Label lblRemainingCards;
        private FlowLayoutPanel flowLayoutPanelOpponent;
        private FlowLayoutPanel flowLayoutPanelTable;
        private Button btnPlayCard;
    }
}
