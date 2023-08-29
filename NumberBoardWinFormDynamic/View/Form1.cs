namespace NumberBoardWinFormDynamic
{
    public partial class Form1 : Form
    {
        private readonly NumberBoardManager numberManager;

        public Form1()
        {
            InitializeComponent();
            MyOverrideOfInitializeComponent();
            //TextBox tx = CreateSimpleDebugTextBox();  // TODO - remove later
            numberManager = new NumberBoardManager(this);
        }
        private void MyOverrideOfInitializeComponent()
        {
            SuspendLayout();
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(900, 450);
            Name = "MainForm";
            Text = "Number Picker";
            ResumeLayout(false);
        }

        //private TextBox CreateSimpleDebugTextBox()  // TODO - remove later
        //{
        //    TextBox tx = new TextBox();
        //    tx.Location = new Point(500, 200);
        //    tx.Size = new Size(150, 50);
        //    Controls.Add(tx);
        //    return tx;
        //}

       
        


    }
}