using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Controls;
using MessageBox = Wpf.Ui.Controls.MessageBox;

namespace Finance_Manager;

/// <summary>
/// Форма для добавления нового пользователя.
/// </summary>
public partial class AddPerson : UiWindow
{
    /// <summary>
    /// Родительское окно.
    /// </summary>
    private readonly MainWindow ParentWindow;

	/// <summary>
	/// Кисть элементов управления по умолчанию.
	/// </summary>
	private readonly Brush StandartBrush;

	public AddPerson(MainWindow parentWindow)
    {
		ParentWindow = parentWindow;
		InitializeComponent();
		StandartBrush = textBlockOperationStatus.Background;
	}

    /// <summary>
    /// Клик по кнопке "Добавить пользователя".
    /// </summary>
    public void ButtonAddPersonClick(object sender, RoutedEventArgs e)
    {
        // Проверяем, что ввели не пустую строку
		if (textBoxPersonName.Text.Trim() == "")
		{
			textBoxPersonName.Background = Brushes.Red;
			textBoxPersonName.ToolTip = "Нужно указать имя пользователя.";
            return;
		}
		textBoxPersonName.Background = StandartBrush;
		textBoxPersonName.ToolTip = null;

        // Добавляем пользователя
		try
        {
            ParentWindow.Controller.AddPerson(textBoxPersonName.Text.Trim());
        }
        catch (Exception exception)
        {
			MessageBox messageBoxError = new MessageBox
            {
                Title = "Ошибка",
				Content = exception.Message,
                ShowFooter = false
			};
            messageBoxError.ShowDialog();
        }

		ParentWindow.RefreshData();
		UpdateOperationStatus();
    }

    /// <summary>
    /// Клик по кнопке "Закрыть форму".
    /// </summary>
    private void ButtonCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Обновить строку состояния операции.
    /// </summary>
    private async void UpdateOperationStatus()
    {
        textBlockOperationStatus.Text = "Создано";
        await Task.Delay(1000);
        textBlockOperationStatus.Text = "Ожидание";
    }
}
