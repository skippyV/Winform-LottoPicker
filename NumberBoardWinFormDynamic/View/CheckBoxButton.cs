
// Custom button from CheckBox to maintain a selected state
// https://learn.microsoft.com/en-us/dotnet/desktop/winforms/controls-design/extend-existing?view=netdesktop-7.0

using NumberBoardWinFormDynamic.View.Interfaces;

namespace NumberBoardWinFormDynamic
{
    public partial class CheckBoxButton : CheckBox
    {
        private IProcessClick _processClick;

        public static string CheckBoxNamePrePend = "CheckBoxButton_";

        public enum ButtonCategory
        {
            Standard = 0,
            Special = 1
        }

        public ButtonCategory Category { get; }

        //public CheckBoxButton()
        //{
        //    InitializeComponent();
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="buttonText"></param>
        /// <param name="processClick">This interface's method is implemented by the NumberManager</param>
        /// <param name="buttonCategory"></param>
        public CheckBoxButton(string name,
                              string buttonText,
                              IProcessClick processClick,
                              ButtonCategory buttonCategory = ButtonCategory.Standard)
        {
            Name = name;
            Appearance = Appearance.Button;
            _processClick = processClick;
            Category = buttonCategory;
            Text = buttonText;
        }

        protected override void OnClick(EventArgs e)
        {
            bool success = _processClick.ProcessClick(Name, (int)Category, Text, !this.Checked); // on completion of Click the state will be inverted

            //SendKeys.Send("+{TAB}");
            //Form x = (Form)this.Parent;
            //x.ActiveControl = null;

            if (success)
            {
                base.OnClick(e);
            }
        }

        public bool IsPressed
        {
            get { return Checked; }
        }
        
    }
}
