using NumberBoardWinFormDynamic.Properties;
using NumberBoardWinFormDynamic.View.Interfaces;
using System.Windows.Forms.VisualStyles;

namespace NumberBoardWinFormDynamic
{
    public partial class NumberSetPanel : UserControl
    {
        public static Size ControlSize { get; set; } = new Size(420, 50);
        public static string TextBoxNamePrePend = "TextBox_";

        private readonly Size TextBoxSize = new(40, 35);
        private readonly Point TextBoxStartLocationOffset = new Point(10, 5);
        private readonly int TextBoxDividerLength = 40;
        private readonly Point TextBoxDividerLocation = new(245, 3);


        public static readonly string IsActiveCheckBoxName = "IsActiveCheckBox";
        private readonly Point CheckBoxLocation = new(330, 10);
        private readonly int CheckBoxDividerLength = 40;
        private readonly Point CheckBoxDividerLocation = new(310, 3);


        private readonly Size DeleteButtonSize = new(35, 35);
        private readonly Point DeleteButtonLocation = new(375, 5);
        private readonly int DeleteButtonDividerLength = 40;
        private readonly Point DeleteButtonDividerLocation = new(360, 3);
        private static readonly string DeleteButtonName = "DeleteButton";


        private readonly IManagePanel _managePanel;
        
        private List<TextBox> textBoxList = new List<TextBox>();

        //private readonly Size CheckBoxSize = new(50, 50); // cannot adjust checkbox size without custom class and drawing it
        //private string customNumberFormat = "00";

        public bool IsActive { get; set; }
        public bool RadioButtonStateIsActive { get; set; }
        public NumberSetPanel(Point location, IManagePanel managePanel)
        {            
            InitializeComponent();
            _managePanel = managePanel;

            //SuspendLayout();
            //// 
            //// NumberSetPanel
            //// 
            //AutoScaleDimensions = new SizeF(7F, 15F);
            //AutoScaleMode = AutoScaleMode.Font;
            //Name = "NumberSetPanel";
            //Size = new Size(369, 77);
            //ResumeLayout(false);
            //Panel panel = new Panel();
            Name = "NumberSetPanel";
            Size = ControlSize;
            BackColor = Color.Yellow;
            BorderStyle = BorderStyle.FixedSingle;
            Location = location;

            AddTextBoxes();
            AddDeleteButton();
            AddIsActiveCheckBox();

            AddCheckBoxDivider();
            AddTextBoxDivider();
            AddDeleteButtonDivider();

            //_managePanel.SelectPanel(this);
        }

        public void SendTextToTextBoxWithName(string textBoxName, string textBoxText)
        {
            Control? textbox = Controls.Find(textBoxName, true).FirstOrDefault();
            if (textbox != null)
            {
                textbox.Text = textBoxText;
                //UpdateDeleteButtonStatus();
            }
            else
            {
                throw new Exception($"Program Error - no control found for: {textBoxName}");
            }            
        }

        public bool SendTextToTextBoxWithIndex(int index, string textBoxText, out string textBoxName)
        {
            if (textBoxList.Count < index - 1)
            {
                textBoxName = "";
                return false;
            }

            TextBox textBox = textBoxList[index];
            textBox.Text = textBoxText;
            textBoxName = textBox.Name; // manager uses name in reference array

            //UpdateDeleteButtonStatus();

            return true;
        }

        public void UpdateDeleteButtonStatus()
        {
            if (this._managePanel.IsLastPanel())
            {
                Controls[DeleteButtonName].Enabled = false;
            }
            else
            {
                Controls[DeleteButtonName].Enabled = true;
            }
        }

        private void AddTextBoxes()
        {
            Point textBoxLocationOffset = TextBoxStartLocationOffset;
            int numBoxes = LottoNumberSet.MaxNumberValsTotal, padding = 5;

            for (int i = 0; i < numBoxes; i++)
            {
                if (i == numBoxes - 1)
                {
                    padding = 10;
                }

                TextBox textBox = NewNumTextBox(TextBoxNamePrePend + Guid.NewGuid().ToString(), textBoxLocationOffset, TextBoxSize, i, padding);
                textBoxList.Add(textBox);
            }
        }

        private TextBox NewNumTextBox(string name,
                                      Point boxLocationYOffset,
                                      Size boxSize,
                                      int boxLocationXOffsetInBoxWidths,
                                      int padding)
        {
            TextBox textBox;
            textBox = new TextBox
            {
                Name = name,
                Location = new Point(boxLocationYOffset.X + ((boxSize.Width + padding) * boxLocationXOffsetInBoxWidths),
                                     boxLocationYOffset.Y), 
                Size = boxSize,
                ReadOnly = true,
                //AutoSize = false,
                AutoSize = true,
                TextAlign = HorizontalAlignment.Center,
                Font = new Font(Font.FontFamily, 16)
            };

            textBox.TabIndex = boxLocationXOffsetInBoxWidths;
            Controls.Add(textBox);

            return textBox;
        }

        private void AddDeleteButton()
        {
            Button button = new Button();
            button.Location = DeleteButtonLocation;
            button.Size = DeleteButtonSize;
            button.Name = DeleteButtonName;
            button.Image = Resources.TrashCan_30x30.ToBitmap();
            button.Enabled = false;

            button.Click += delegate
            {
                if (_managePanel != null)
                {
                    if (!_managePanel.IsLastPanel())
                    {
                        _managePanel.DeletePanel(this);
                    }
                }
            };

            Controls.Add(button);
        }
        /// <summary>
        /// Using the RadioButtonStateIsActive member, the checkboxes of the panels will act as
        /// a group of Radio Buttons - in that only one can be selected at a time.
        /// This design is frowned upon (using checkboxes to mimic radio button functionality)
        /// but it was a desired feature.
        /// </summary>
        private void AddIsActiveCheckBox()
        {
            CheckBox checkBox = new()
            {
                Name = IsActiveCheckBoxName,
                Location = CheckBoxLocation,
                Text = string.Empty
                //AutoCheck = false
            };           
            //checkBox.Margin = new Padding(0);
            int checkBoxDefaultHeight = checkBox.ClientSize.Height;
            checkBox.ClientSize = new(checkBoxDefaultHeight, checkBoxDefaultHeight);

            RadioButtonStateIsActive = true; // flag to assist in radiobutton functionality for all NumberSetPanels

            checkBox.CheckedChanged += delegate 
            { 
                if (checkBox.Checked != RadioButtonStateIsActive)
                {
                    _managePanel.PanelCheckBoxClicked(this);
                }
            };
            Controls.Add(checkBox);
            //RadioButtonStateIsActive = true;
            //checkBox.Checked = true;
            _managePanel.PanelCheckBoxClicked(this);
        }

        private void AddTextBoxDivider()
        {
            _ = AddVerticalDividerLine(TextBoxDividerLocation, TextBoxDividerLength);
        }

        private void AddCheckBoxDivider()
        {
            _ = AddVerticalDividerLine(CheckBoxDividerLocation, CheckBoxDividerLength);
        }

        private void AddDeleteButtonDivider()
        {
            _ = AddVerticalDividerLine(DeleteButtonDividerLocation, DeleteButtonDividerLength);
        }

        private int AddVerticalDividerLine(Point location, int height)
        {
            // Create a divider with a label (https://stackoverflow.com/questions/3296110/draw-horizontal-divider-in-winforms)
            Label verticalDividerLabel = new()
            {
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.Black,
                AutoSize = false,
                Height = height,
                Margin = new Padding(10),
                Width = 2,
                Location = location
            };

            Controls.Add(verticalDividerLabel);

            return location.X + verticalDividerLabel.Width + verticalDividerLabel.Margin.Horizontal;
        }
    }
}
