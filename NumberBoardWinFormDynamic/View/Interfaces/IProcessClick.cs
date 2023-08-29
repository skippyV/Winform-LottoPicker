
namespace NumberBoardWinFormDynamic.View.Interfaces
{
    public interface IProcessClick
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="refStringButtonName">Name of button used to reference it</param>
        /// <param name="buttonCategory">Category of button used to differentiate button sets source</param>
        /// <param name="textValString">Value displayed on button</param>
        /// <param name="selected">State of button click. Button is derived from checkbox so state is either Seleceted or Unselected</param>
        /// <returns></returns>
        bool ProcessClick(string refStringButtonName, int buttonCategory, string textValString, bool selected);
    }
}
