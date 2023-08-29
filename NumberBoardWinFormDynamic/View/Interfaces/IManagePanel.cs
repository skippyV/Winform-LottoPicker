
namespace NumberBoardWinFormDynamic.View.Interfaces
{
    public interface IManagePanel
    {
        void DeletePanel(UserControl userControl);

        bool IsLastPanel();

        void PanelCheckBoxClicked(UserControl userControl);
    }
}
