using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Controls;
using MessageBox = Wpf.Ui.Controls.MessageBox;

namespace Finance_Manager;

/// <summary>
/// Форма для добавления нового перевода.
/// </summary>
public partial class AddTransfer : UiWindow
{
	/// <summary>
	/// Родительское окно.
	/// </summary>
	private readonly MainWindow ParentWindow;

    /// <summary>
    /// Выбрана ли кастомная категория.
    /// </summary>
	private bool CustomCategoryChosen;

    /// <summary>
    /// Кисть элементов управления по умолчанию.
    /// </summary>
    private readonly Brush StandartBrush;

    public AddTransfer(MainWindow parent)
    {
		ParentWindow = parent;
		InitializeComponent();

        // Заполняем comboBox пользователей
		for (int i = 0; i < ParentWindow.Controller.Persons.Rows.Count; i++)
		{
			DataRow? row = ParentWindow.Controller.Persons.Rows[i];
			comboBoxPerson.Items.Add(row[1]);
		}
		// Заполняем comboBox категорий
		for (int i = 1; i < ParentWindow.Controller.Categories.Rows.Count; i++)
        {
			DataRow? row = ParentWindow.Controller.Categories.Rows[i];
            comboBoxCategory.Items.Add(row[1]);
        }

        comboBoxCategory.SelectedIndex = 0;
        StandartBrush = textBlockTransferDescription.Background;
    }

	/// <summary>
	/// Клик по кнопке "Добавить перевод".
	/// </summary>
	public void ButtonAddTransferClick(object sender, RoutedEventArgs e)
    {
		// Проверяем, что пользователь был выбран
		bool success = CheckPerson() & CheckTransferDate() & CheckTransferAmount();
        if (!success)
        {
            return;
        }

        // Добавляем перевод
        try
        {
            int personId = int.Parse((string)ParentWindow.Controller.Persons.Rows[comboBoxPerson.SelectedIndex][0]);
            int categoryId = int.Parse((string)ParentWindow.Controller.Categories.Rows[comboBoxCategory.SelectedIndex + 1][0]);
            string description = (CustomCategoryChosen ? comboBoxCategory.SelectedItem.ToString() : textBlockTransferDescription.Text)?.Trim() ?? "";
            double amount = double.Parse(textBoxTransferAmount.Text);
            DateTime date = datePickerTransferDate.SelectedDate == null ? DateTime.Now : datePickerTransferDate.SelectedDate.Value;

			ParentWindow.Controller.AddTransfer(personId, categoryId, description, amount, date);
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

	/// <summary>
	/// Проверить пользователя.
	/// </summary>
	private bool CheckPerson()
	{
		if (comboBoxPerson.SelectedIndex == -1)
		{
			comboBoxPerson.Background = Brushes.Red;
			comboBoxPerson.ToolTip = "Нужно выбрать пользователя.";
			return false;
		}

		comboBoxPerson.Background = StandartBrush;
		comboBoxPerson.ToolTip = null;
		return true;
	}

	/// <summary>
	/// Проверить дату перевода.
	/// </summary>
	private bool CheckTransferDate()
	{
		if (datePickerTransferDate.SelectedDate == null)
		{
			datePickerTransferDate.Background = Brushes.Red;
			datePickerTransferDate.ToolTip = "Требуется задать дату транзакции.";
			return false;
		}

		datePickerTransferDate.Background = StandartBrush;
		datePickerTransferDate.ToolTip = null;
		return true;
	}

	/// <summary>
	/// Дата перевода изменилась.
	/// </summary>
	private void DatePickerTransferDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        CheckTransferDate();
    }

	/// <summary>
	/// Проверить сумму перевода (должна быть числом).
	/// </summary>
	private bool CheckTransferAmount()
	{
		if (!double.TryParse(textBoxTransferAmount.Text, out double amount))
		{
			textBoxTransferAmount.Background = Brushes.Red;
			textBoxTransferAmount.ToolTip = "Должно быть числом.";
			return false;
		}
		else if (amount == 0)
		{
			textBoxTransferAmount.Background = Brushes.Red;
			textBoxTransferAmount.ToolTip = "Должно быть не 0.";
			return false;
		}

		textBoxTransferAmount.Background = StandartBrush;
		textBoxTransferAmount.ToolTip = null;
		return true;
	}

	/// <summary>
	/// Сумма перевода изменилась.
	/// </summary>
	private void TransferAmountTextChanged(object sender, TextChangedEventArgs e)
    {
        CheckTransferAmount();
    }

    /// <summary>
    /// Изменение категории.
    /// </summary>
    private void ComboBoxCategorySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (comboBoxCategory.SelectedIndex == 0)
        {
            gridColumnTransferDescription.Width = new GridLength(2, GridUnitType.Star);
            CustomCategoryChosen = false;
        }
        else
        {
            gridColumnTransferDescription.Width = new GridLength(0);
            CustomCategoryChosen = true;
        }
    }

	/// <summary>
	/// Пользователь изменился.
	/// </summary>
	private void ComboBoxPersonChanged(object sender, SelectionChangedEventArgs e)
	{
		CheckPerson();
	}
}