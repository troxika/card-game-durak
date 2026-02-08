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
            btnLoadCards = new Button();
            flowLayoutPanelPlayer = new FlowLayoutPanel();
            lblRemainingCards = new Label();
            flowLayoutPanelOpponent = new FlowLayoutPanel();
            SuspendLayout();
            // 
            // btnLoadCards
            // 
            btnLoadCards.Location = new Point(3, 12);
            btnLoadCards.Name = "btnLoadCards";
            btnLoadCards.Size = new Size(75, 23);
            btnLoadCards.TabIndex = 0;
            btnLoadCards.Text = "Card";
            btnLoadCards.UseVisualStyleBackColor = true;
            btnLoadCards.Click += btnLoadCards_Click;
            // 
            // flowLayoutPanelPlayer
            // 
            flowLayoutPanelPlayer.Location = new Point(105, 236);
            flowLayoutPanelPlayer.Name = "flowLayoutPanelPlayer";
            flowLayoutPanelPlayer.Size = new Size(645, 202);
            flowLayoutPanelPlayer.TabIndex = 1;
            // 
            // lblRemainingCards
            // 
            lblRemainingCards.AutoSize = true;
            lblRemainingCards.Location = new Point(40, 413);
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
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(flowLayoutPanelOpponent);
            Controls.Add(lblRemainingCards);
            Controls.Add(flowLayoutPanelPlayer);
            Controls.Add(btnLoadCards);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnLoadCards;
        private FlowLayoutPanel flowLayoutPanelPlayer;
        private Label lblRemainingCards;
        private FlowLayoutPanel flowLayoutPanelOpponent;
    }
}
