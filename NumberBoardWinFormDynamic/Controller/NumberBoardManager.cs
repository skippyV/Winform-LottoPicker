
using NumberBoardWinFormDynamic.View.Interfaces;
using static NumberBoardWinFormDynamic.CheckBoxButton;

/// The NumberBoardManager is the workhorse of this application.
/// It maintains a list of references between the Data (LottNumberSet) and a
/// loose coupling for the View, using string names to locate the buttons and
/// text boxes used to display the number values.
///
/// This application uses UserControls with checkboxes in a radiobutton manner.
/// Although using checkboxes to mimic radiobutton functionality is frowned upon
/// in dev circles, it fit my desired plan and I'm a bit of a rebel. Besides, this
/// is just a toy. (I haven't won any lotto - LOL). Doesn't even have unit testing.
/// 
/// 
/// Create Toggle to flip between MegaMillions and PowerBall mode - TODO
/// 
/// Email feature - TODO
///   - have form for entering email address
///   - have option to just send picture <html> in email
///   - use validatin - https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.control.causesvalidation?view=windowsdesktop-7.0
/// 
/// Cleanup - refactor - TODO
///


namespace NumberBoardWinFormDynamic
{
    public enum GameModel
    {
        PowerBall,
        MegaMillions
    }

    public enum ErrorCode
    {
        NoError = 0,
        NumberSetFull = 1,
        ProgramError = 3
    }

    internal class NumberBoardManager : IProcessClick, IManagePanel
    {
        private Form mainForm;

        private int activeCompositeDataItemIndex = -1;
        private readonly int compositeDataItemListMaxSize = 5;

        private List<CompositeData> compositeDataList = new();
        private readonly Point FirstNumberSetPanelLocation = new(430, 40);

        private readonly Size AddNewNumberSetPanelButtonSize = new(150, 30);
        private readonly Point AddNewNumberSetPanelButtonLocation = new(450, 0);
        private readonly string AddNewNumberSetPanelButtonName = "AddNewPanelButton";

        private static readonly Point TitleLabelLocation = new(150, 5);

        private readonly Size SpecialBallLabelSize = new(30, 15);
        private readonly Point SpecialBallLabelLocation = new(695, 25);

        private readonly Size IsActiveCheckBoxLabelSize = new(60, 15);
        private readonly Point IsActiveCheckBoxLabelLocation = new(743, 25);

        private static readonly int PromptLabelXOffset = 30;
        private static readonly Point PickFiveLabeLocation = new(PromptLabelXOffset, TitleLabelLocation.Y + 15);

        private readonly Size CheckBoxButtonSize = new(55, 25);
        private readonly Point CheckBoxButtonLocation = new(10, PickFiveLabeLocation.Y + 20);
        private readonly int CheckBoxesMaxColumns = 7;

        // PowerBall numbers: 1 to 69 with 1 to 26 for PowerBall
        // MegaMillions numbers: 1 to 70 with 1 to 25 for MegaBall
        // Both tickets are printed with 7 columns of numbers.

        private readonly int MaxNumberStandardButtonsMegaMillions = 70;
        private readonly int MaxNumberStandardButtonsPowerBall = 69;

        private readonly int MaxNumberSpecialButtonsMegaMillions = 25;
        private readonly int MaxNumberSpecialButtonsPowerBall = 26;

        private int PaddedHeightOfNumberSetPanel;

        public void PanelCheckBoxClicked(UserControl userControl)
        {
            if (userControl is NumberSetPanel panel)
            {
                panel.RadioButtonStateIsActive = true;
                (panel.Controls[NumberSetPanel.IsActiveCheckBoxName] as CheckBox).Checked = true;

                UnSelectOtherPanels(panel);
                SetCheckedStateOfCheckBoxButtons(activeCompositeDataItemIndex, false);
                
                int newIndex = GetIndexOfNumberSetPanel(panel);
                if (!(newIndex < 0))
                {
                    activeCompositeDataItemIndex = newIndex;
                }

                SetCheckedStateOfCheckBoxButtons(newIndex, true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="refStringButtonName"></param>
        /// <param name="buttonCategory"> Used to distinguish between regular numbers and Power/Mega balls</param>
        /// <param name="textValString"> Number value sent from button to TextBox</param>
        /// <param name="selected">Buttons for numbers have CheckBox state allowing them to stay selected</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool ProcessClick(string refStringButtonName,
                                 int buttonCategory,
                                 string textValString,
                                 bool selected)
        {
            CompositeData curData = compositeDataList[activeCompositeDataItemIndex];
            if (selected)
            {
                /// if it was 'selected' then the CheckboxButton was previously unselected, therefore a new number
                return SelectingNewCheckBoxButton(refStringButtonName, buttonCategory, textValString, curData);
            }
            else // un-selecting active number/button
            {
                return UnselectingCurrentlySelectedCheckBoxButton(refStringButtonName);
            }
        }

        public void DeletePanel(UserControl userControl)
        {
            int indexToDelete = GetIndexOfNumberSetPanel(userControl);

            if (indexToDelete > -1)
            {
                //AdjustIndexForListElementDeletion(indexToDelete);

                if (indexToDelete == activeCompositeDataItemIndex)
                {
                    SetCheckedStateOfCheckBoxButtons(indexToDelete, false);
                }

                NumberSetPanel panel = compositeDataList[indexToDelete].NumberSetPanel;

                compositeDataList.RemoveAt(indexToDelete); // remove the tracked references to panel
                RefreshNumberSetPanelsLayout();            // readjust the remaining tracked panels' locations

                panel.Parent.Controls.Remove(panel);       // remove the panel control                

                if (CheckIfNumberPanelIsFull(compositeDataList.Count - 1)) // check if last (bottom) panel is full
                {
                    EnableAddNewPanelButton(true);
                }
                else
                {
                    EnableAddNewPanelButton(false);
                }

                AdjustCompositeDataListIndexForElementDeletion(indexToDelete);
                SelectPanelWithIndex(activeCompositeDataItemIndex);
                UpdateDeleteButtonsStatuses();
            }
        }

        public bool IsLastPanel()
        {
            return compositeDataList.Count == 1;
        }

        private bool SelectingNewCheckBoxButton(string refStringButtonName,
                                                int buttonCategory,
                                                string textValString,
                                                CompositeData curData)
        {
            ErrorCode errCode = GetNewLottoNumberIndex(out int newIndx, buttonCategory, curData, textValString);

            switch (errCode)
            {
                case ErrorCode.NoError:

                    string refTextBoxName;

                    if (curData.NumberSetPanel.SendTextToTextBoxWithIndex(newIndx, textValString, out refTextBoxName))
                    {
                        curData.ReferenceArray[newIndx].TextBoxRef = refTextBoxName;
                        curData.ReferenceArray[newIndx].ButtonRef = refStringButtonName;

                        if (CheckIfCurrentNumberPanelIsFull())
                        {
                            EnableAddNewPanelButton(true);
                        }

                        return true;
                    }
                    else
                    {
                        throw new Exception($"Program error in Mgr.ProcessClick, SendTextToTextBox failed, newIndx:{newIndx}");
                    }

                case ErrorCode.NumberSetFull:
                    if (buttonCategory == (int)ButtonCategory.Standard)
                    {
                        MessageBox.Show("Standard number set is full");
                    }
                    if (buttonCategory == (int)ButtonCategory.Special)
                    {
                        MessageBox.Show("Special number set is full");
                    }
                    return false;

                default:
                    MessageBox.Show("Program ERROR!");
                    return false;
            }
        }

        private bool UnselectingCurrentlySelectedCheckBoxButton(string refStringButtonName)
        {
            // Find the index of the ReferenceArray containing the CheckBoxButton name.
            // Then use the index to clear that slot.
            int index = FindIndexWithButtonName(refStringButtonName);

            if (index == -1)
            {
                return false;
            }

            string textBoxName = compositeDataList[activeCompositeDataItemIndex].ReferenceArray[index].TextBoxRef;

            compositeDataList[activeCompositeDataItemIndex].NumberSetPanel
                .SendTextToTextBoxWithName(textBoxName, string.Empty); // clear the value in TextBox

            if (!ClearSlot(index))
            {
                return false;
            }

            EnableAddNewPanelButton(false);
            return true;
        }

        public NumberBoardManager(Form form)
        {
            mainForm = form;

            GameModel model = GameModel.MegaMillions;
            AddTitle(model);
            AddSpecialBallLabel(model);
            AddNumberButtons(model);

            AddPickFiveLabel();
            AddIsActiveCheckBoxLabel();

            PaddedHeightOfNumberSetPanel = NumberSetPanel.ControlSize.Height + 30;

            AddCompositeDataElement(null, null); // initialize with new panel for number set

            AddAddNewPanelButton();

            // AddDebugRefreshButton();
        }

        private void UnSelectOtherPanels(NumberSetPanel selectedPanel)
        {
            foreach (var panel in mainForm.Controls.OfType<NumberSetPanel>())
            {
                if (!panel.Equals(selectedPanel))
                {
                    panel.RadioButtonStateIsActive = false;
                    (panel.Controls[NumberSetPanel.IsActiveCheckBoxName] as CheckBox).Checked = false;
                }
            }
        }

        private void SelectPanelWithIndex(int index)
        {
            NumberSetPanel panel = compositeDataList[index].NumberSetPanel;
            PanelCheckBoxClicked(panel);
        }

        private int GetIndexOfNumberSetPanel(UserControl userControl)
        {
            if (userControl is NumberSetPanel panel)
            {
                return compositeDataList.FindIndex(m => m.NumberSetPanel.Equals(panel));
            }
            return -1;
        }

        private void AddTitle(GameModel gameModel)
        {
            Label titleLabel = new()
            {
                Location = TitleLabelLocation
            };
            //titleLabel.Size = new Size(200, 60);

            if (gameModel != GameModel.PowerBall)
            {
                titleLabel.Text = "Mega Millions";
            }
            else
            {
                titleLabel.Text = "Power Ball";
            }            
            
            mainForm.Controls.Add(titleLabel);
        }

        private void AddSpecialBallLabel(GameModel gameModel)
        {
            Label ballLabel = new()
            {
                Location = SpecialBallLabelLocation,
                Size = SpecialBallLabelSize
            };

            if (gameModel != GameModel.PowerBall)
            {
                ballLabel.Text = "MB";
            }
            else
            {
                ballLabel.Text = "PB";
            }

            mainForm.Controls.Add(ballLabel);
        }

        private void AddIsActiveCheckBoxLabel()
        {
            Label label = new()
            {
                Text = "Current",
                Location = IsActiveCheckBoxLabelLocation,
                Size = IsActiveCheckBoxLabelSize
            };
            mainForm.Controls.Add(label);
        }

        private void AddPickFiveLabel()
        {
            Label label = new()
            {
                Text = "Pick 5",
                Location = PickFiveLabeLocation
            };

            mainForm.Controls.Add(label);
        }

        private void AddCompositeDataElement(object? sender, EventArgs? e)
        {            
            if (activeCompositeDataItemIndex >= 0)
            {
                SetCheckedStateOfCheckBoxButtons(activeCompositeDataItemIndex, false);
            }

            Point panelLocation = new(FirstNumberSetPanelLocation.X, FirstNumberSetPanelLocation.Y
                + (compositeDataList.Count * PaddedHeightOfNumberSetPanel));

            NumberSetPanel panel = new(panelLocation, this);
            
            mainForm.Controls.Add(panel);

            LottoNumberSet lottoNumberSet = new();
            CompositeData compositeDataItem = new(lottoNumberSet, panel);
            compositeDataList.Add(compositeDataItem);
            UpdateDeleteButtonsStatuses();

            EnableAddNewPanelButton(false);

            AdjustCompositeDataListIndexForElementAddition();
        }

        private void UpdateDeleteButtonsStatuses()
        {
            foreach(var elem in compositeDataList)
            {
                if (elem?.NumberSetPanel != null)
                {
                    elem.NumberSetPanel.UpdateDeleteButtonStatus(); // depends on compositeDataList count
                }
            }
        }

        private void AdjustCompositeDataListIndexForElementAddition()
        {
            activeCompositeDataItemIndex = compositeDataList.Count - 1;
        }

        private void AdjustCompositeDataListIndexForElementDeletion(int indexToDelete)
        {
            if (indexToDelete > -1)
            {
                if (indexToDelete == activeCompositeDataItemIndex)
                {
                    activeCompositeDataItemIndex = compositeDataList.Count - 1;
                }
                if (activeCompositeDataItemIndex > compositeDataList.Count - 1)
                {
                    activeCompositeDataItemIndex--;
                }
            }
        }

        private void AddAddNewPanelButton()
        {
            Button button = new Button
            {
                Name = AddNewNumberSetPanelButtonName,
                Text = "Add New Number Panel",
                Size = AddNewNumberSetPanelButtonSize,
                Location = AddNewNumberSetPanelButtonLocation,
                Enabled = false
            };

            button.Click += AddCompositeDataElement;
            mainForm.Controls.Add(button);
        }

        private void RefreshNumberSetPanelsLayout()
        {
            int indx = 0;
            mainForm.SuspendLayout();
            foreach (CompositeData elem in compositeDataList)
            {
                NumberSetPanel panel = elem.NumberSetPanel;
                panel.Location = new Point(FirstNumberSetPanelLocation.X, FirstNumberSetPanelLocation.Y
                + (indx++ * PaddedHeightOfNumberSetPanel));
            }
            mainForm.ResumeLayout();
        }

        private void SetCheckedStateOfCheckBoxButtons(int index, bool checkState)
        {
            if (index > -1 && index < compositeDataItemListMaxSize)
            {
                foreach ((_, string ButtonRef) in compositeDataList[index].ReferenceArray)
                {
                    string checkBoxButtonName = ButtonRef;                                                                                                           //
                    CheckBoxButton? checkBoxButton = FindCheckBoxButtonWithName(checkBoxButtonName);
                    if (checkBoxButton != null)
                    {
                        checkBoxButton.Checked = checkState;
                    }
                }
            }
        }

        private bool CheckIfCurrentNumberPanelIsFull()
        {
            return CheckIfNumberPanelIsFull(activeCompositeDataItemIndex);
        }

        private bool CheckIfNumberPanelIsFull(int index)
        {
            if (index < 0 || index > compositeDataList.Count - 1)
            {
                throw new ArgumentOutOfRangeException($"from CheckIfNumberPanelIsFull {index}");
            }

            return (compositeDataList[index].LottoNumberSet.IsFull());
        }

        private void EnableAddNewPanelButton(bool enableState)
        {
            Control button = mainForm.Controls[AddNewNumberSetPanelButtonName];
            if (button != null)
            {
                if (compositeDataList.Count < compositeDataItemListMaxSize)
                {
                    button.Enabled = enableState;
                }
                else
                {
                    button.Enabled = false;
                }
                
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="refArrayIndex"></param>
        /// <returns>True if value successfully cleared</returns>
        private bool ClearSlot(int refArrayIndex)
        {
            compositeDataList[activeCompositeDataItemIndex].ReferenceArray[refArrayIndex].ButtonRef = string.Empty;   // will be reused
            compositeDataList[activeCompositeDataItemIndex].ReferenceArray[refArrayIndex].TextBoxRef = string.Empty;  // will be reused
            return compositeDataList[activeCompositeDataItemIndex].LottoNumberSet.RemoveElementAt(refArrayIndex);
        }

        public int FindIndexWithButtonName(string buttonName)
        {
            int index = -1;
            CompositeData compositeData = compositeDataList[activeCompositeDataItemIndex];

            index = Array.FindIndex(compositeData.ReferenceArray, item => item.ButtonRef.Equals(buttonName, StringComparison.OrdinalIgnoreCase));

            return index;
        }

        public CheckBoxButton? FindCheckBoxButtonWithName(string buttonName)
        {
            if (buttonName != null)
            {
                return mainForm.Controls.Find(buttonName, true).FirstOrDefault() as CheckBoxButton;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compositeData"></param>
        /// <param name="textValString"></param>
        /// <returns>  
        ///     0 if no issues
        ///     1 if number set is full
        ///     2 if numbers are full but SpecialBall not set
        ///     3 if program error
        /// </returns>        
        private ErrorCode GetNewLottoNumberIndex(
            out int index,
            int numberCategory,
            CompositeData compositeData,
            string textValString)
        {
            index = -1;

            if (numberCategory == (int)ButtonCategory.Standard)
            {
                if (compositeData.LottoNumberSet.IsFullStandardVals())
                {
                    return ErrorCode.NumberSetFull;
                }
                index = compositeData.LottoNumberSet.GetNextAvailableIndexStandardVals();
            }
            else
            {
                if (compositeData.LottoNumberSet.IsFullSpecialVals())
                {
                    return ErrorCode.NumberSetFull;
                }
                index = compositeData.LottoNumberSet.GetNextAvailableIndexSpecialVals();
            }
            
            if (index == -1) 
            {
                throw new Exception("Program Error in GetNewLottoNumberIndex() - index == -1");
            }

            try
            {
                // index is valid, continue
                int val = int.Parse(textValString);
                if (compositeData.LottoNumberSet.SetValueAt(index, val))
                {                    
                    return ErrorCode.NoError;
                }
                _ = MessageBox.Show($"Error at SetValueAt({index},{val})", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return ErrorCode.ProgramError;
            }
            catch (Exception e)
            {
                _ = MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return ErrorCode.ProgramError;
            }            
        }

        private void AddNumberButtons(GameModel gameModel)
        {
            int maxCol = CheckBoxesMaxColumns;
            Size buttonSize = CheckBoxButtonSize;
            Point buttonLocationOffset = CheckBoxButtonLocation; // Y is changed per row
            Point buttonLocation = new(buttonLocationOffset.X, buttonLocationOffset.Y);

            int maxNumberStandardButtons, maxNumberSpecialButtons;
            if (gameModel != GameModel.PowerBall)
            {
                maxNumberStandardButtons = MaxNumberStandardButtonsMegaMillions; // MegaMillions
                maxNumberSpecialButtons = MaxNumberSpecialButtonsMegaMillions;
            }
            else
            {
                maxNumberStandardButtons = MaxNumberStandardButtonsPowerBall; // PowerBall
                maxNumberSpecialButtons = MaxNumberSpecialButtonsPowerBall;
            }

            // Create first set of buttons (Standard category)
            CreateSetOfCheckBoxButtons(CheckBoxNamePrePend,
                               ref buttonLocation,
                               ref buttonLocationOffset,
                               buttonSize,
                               maxNumberStandardButtons,
                               maxCol,
                               ButtonCategory.Standard);

            int verticalOffset = AddDividerLine(buttonLocation.Y, buttonSize.Width * maxCol);

            verticalOffset = AddSpecialButtonsLabel(buttonLocation.Y + verticalOffset, gameModel) + 17; // 17 to tweak padding

            buttonLocation = new Point(buttonLocation.X, buttonLocationOffset.Y + verticalOffset);
            buttonLocationOffset = buttonLocation;

            // Create second set of buttons (Special category)
            CreateSetOfCheckBoxButtons(CheckBoxNamePrePend + "_sp_",
                               ref buttonLocation,
                               ref buttonLocationOffset,
                               buttonSize,
                               maxNumberSpecialButtons,
                               maxCol,
                               ButtonCategory.Special);

        }

        private int AddDividerLine(int verticalOffset, int length)
        {
            int dividerLabelHeight = 2;
            int dividerLabelVerticalMargin = 10;
            int dividerLabelHorizontalMargin = 40;
            // Create a divider with a label (https://stackoverflow.com/questions/3296110/draw-horizontal-divider-in-winforms)
            Label dividerLabel = new Label
            {
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.Black,
                AutoSize = false,
                Height = dividerLabelHeight,
                Width = length - dividerLabelHorizontalMargin,
                Location = new Point(dividerLabelHorizontalMargin / 4, verticalOffset + dividerLabelVerticalMargin / 2)
            };
            mainForm.Controls.Add(dividerLabel);

            return dividerLabelHeight + dividerLabelVerticalMargin;
        }

        private int AddSpecialButtonsLabel(int verticalOffset, GameModel gameModel)
        {
            Label label = new Label
            {
                Location = new Point(PromptLabelXOffset, verticalOffset),
                Width = 150,
                Text = "Pick one ",
                Height = 15,
            };

            if (gameModel == GameModel.MegaMillions)
            {
                label.Text += "MegaBall";
            }
            else
            {
                label.Text += "PowerBall";
            }

            mainForm.Controls.Add(label);

            return label.Height;
        }

        private void CreateSetOfCheckBoxButtons(string buttonNamePrepend,
                                        ref Point buttonLocation,
                                        ref Point buttonLocationOffset,
                                        Size buttonSize,
                                        int numberButtons,
                                        int maxCol,
                                        ButtonCategory buttonCategory)
        {
            for (int row = 1, col = 0, cntr = 1; cntr < numberButtons + 1; cntr++)
            {
                mainForm.Controls.Add(CreateDynamicButton(buttonNamePrepend + cntr.ToString(),
                                                          cntr.ToString(),
                                                          buttonLocation,
                                                          buttonSize,
                                                          buttonCategory));

                col++;
                if (cntr % maxCol == 0)
                {
                    row++;
                    col = 0;
                    buttonLocationOffset = new Point(buttonLocationOffset.X, buttonLocationOffset.Y + buttonSize.Height); // increment the Y axis and zero out col variable
                }
                buttonLocation = new Point(buttonLocationOffset.X + (col * buttonSize.Width), buttonLocationOffset.Y);
            }
        }

        /// https://www.c-sharpcorner.com/uploadfile/mahesh/creating-a-button-at-run-time-in-C-Sharp/
        private CheckBoxButton CreateDynamicButton(string name,
                                                   string buttonText,
                                                   Point location,
                                                   Size size,
                                                   ButtonCategory buttonCategory)
        {
            CheckBoxButton chkBxButton = new CheckBoxButton(name, buttonText, this, buttonCategory);
            chkBxButton.Location = location;
            chkBxButton.Size = size;
            chkBxButton.TextAlign = ContentAlignment.MiddleCenter;

            return chkBxButton;
        }

        private void AddDebugRefreshButton()
        {
            Button button = new()
            {
                Text = "Refresh",
                Location = new Point(170, 10)
                //Size = new Size(150, 30)
            };
            button.Click += delegate { DebugInvalidate(); };
            mainForm.Controls.Add(button);
        }

        private void DebugInvalidate()
        {
            //var panel = compositeDataList[0].NumberSetPanel;
            //panel.Invalidate();
            //panel.Update();
            mainForm.Invalidate();
            mainForm.Update();
        }
    }

    internal class CompositeData
    {
        public LottoNumberSet LottoNumberSet;
        public NumberSetPanel NumberSetPanel;

        // ReferenceArray is an array of the same size as the whole LottoNumberSet, that contains tuples of(string, string)
        // to maintain the references of the CheckBoxButton names and their respective TextBox names in the NumberSetPanel.
        public (string TextBoxRef, string ButtonRef)[] ReferenceArray = new (string, string)[LottoNumberSet.MaxNumberValsTotal]; 

        public CompositeData(LottoNumberSet lottoNumberSet,
                                  NumberSetPanel numberSetPanel)
        {
            LottoNumberSet = lottoNumberSet;
            NumberSetPanel = numberSetPanel;
        }
    }
}
