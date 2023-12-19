using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Controls;
using MessageBox = Wpf.Ui.Controls.MessageBox;

namespace Finance_Manager;

/// <summary>
/// Форма для добавления нового перевода между пользователями.
/// </summary>
public partial class AddPersonsTransfer : UiWindow
{
	/// <summary>
	/// Родительское окно.
	/// </summary>
	private MainWindow ParentWindow;

	/// <summary>
	/// Кисть элементов управления по умолчанию.
	/// </summary>
	private readonly Brush StandartBrush;

    public AddPersonsTransfer(MainWindow parent)
    {
		ParentWindow = parent;
		InitializeComponent();

		// Заполняем comboBox отправителя и получателя
		foreach (DataRow row in ParentWindow.Controller.Persons.Rows)
		{
			comboBoxSender.Items.Add(row[1]);
			comboBoxReceiver.Items.Add(row[1]);
		}
		
        StandartBrush = textBoxTransferAmount.Background;
    }

	/// <summary>
	/// Клик по кнопке "Добавить перевод".
	/// </summary>
	private void ButtonAddTransferClick(object sender, RoutedEventArgs e)
    {
		// Проверяем, что отправитель был выбран, получатель был выбран, отправитель и получатель не совпадают, дату и сумму перевода.
		bool success = CheckPerson(comboBoxSender) & CheckPerson(comboBoxReceiver) & CheckPersonsEqual() & CheckTransferDate() & CheckTransferAmount();
		if (!success)
		{
			return;
		}

		// Добавляем перевод
		bool firstTransferDone = false;
        try
        {
			// Первый перевод
			int personId = int.Parse((string)ParentWindow.Controller.Persons.Rows[comboBoxSender.SelectedIndex][0]);
			int categoryId = int.Parse((string)ParentWindow.Controller.Categories.Rows[0][0]);
			string description = $"Перевод пользователю {ParentWindow.Controller.Persons.Rows[comboBoxReceiver.SelectedIndex][1]}";
			double amount = -double.Parse(textBoxTransferAmount.Text);
			DateTime date = datePickerTransferDate.SelectedDate == null ? DateTime.Now : datePickerTransferDate.SelectedDate.Value;

			ParentWindow.Controller.AddTransfer(personId, categoryId, description, amount, date);
			firstTransferDone = true;
			ParentWindow.RefreshData();

			// Второй перевод
			personId = int.Parse((string)ParentWindow.Controller.Persons.Rows[comboBoxReceiver.SelectedIndex][0]);
			categoryId = int.Parse((string)ParentWindow.Controller.Categories.Rows[0][0]);
			description = $"Перевод от пользователя {ParentWindow.Controller.Persons.Rows[comboBoxSender.SelectedIndex][1]}";
			amount = double.Parse(textBoxTransferAmount.Text);
			date = datePickerTransferDate.SelectedDate == null ? DateTime.Now : datePickerTransferDate.SelectedDate.Value;

			ParentWindow.Controller.AddTransfer(personId, categoryId, description, amount, date);
		}
        catch (Exception exception)
        {
			// Отмена первого перевода
            if (firstTransferDone)
            {
				ParentWindow.RefreshData();
				List<int> transferIds = new List<int>
				{
					int.Parse((string)((DataRowView)ParentWindow.dataGridTransfers.Items[^1]).Row[0])
				};
				ParentWindow.Controller.RemoveTransfers(transferIds);
            }

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
	private bool CheckPerson(ComboBox comboBoxPerson)
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
	/// Проверить отправителя и получателя на равенство.
	/// </summary>
	private bool CheckPersonsEqual()
	{
		if (comboBoxSender.SelectedIndex == -1 || comboBoxReceiver.SelectedIndex == -1)
		{
			return true;
		}

		if (comboBoxSender.SelectedIndex != -1 && comboBoxReceiver.SelectedIndex != -1 &&
			comboBoxSender.SelectedIndex == comboBoxReceiver.SelectedIndex)
		{
			comboBoxSender.Background = Brushes.Red;
			comboBoxSender.ToolTip = "Пользователи не могут совпадать.";
			comboBoxReceiver.Background = Brushes.Red;
			comboBoxReceiver.ToolTip = "Пользователи не могут совпадать.";
			return false;
		}

		comboBoxSender.Background = StandartBrush;
		comboBoxSender.ToolTip = null;
		comboBoxReceiver.Background = StandartBrush;
		comboBoxReceiver.ToolTip = null;
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
	/// Проверить сумму перевода (должна быть положительным числом).
	/// </summary>
	private bool CheckTransferAmount()
	{
		if (!double.TryParse(textBoxTransferAmount.Text, out double res))
		{
			textBoxTransferAmount.Background = Brushes.Red;
			textBoxTransferAmount.ToolTip = "Должно быть числом.";
			return false;
		}
		else if (res <= 0)
		{
			textBoxTransferAmount.Background = Brushes.Red;
			textBoxTransferAmount.ToolTip = "Должно быть положительным числом.";
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
	/// Отправитель изменился.
	/// </summary>
	private void ComboBoxSenderChanged(object sender, SelectionChangedEventArgs e)
	{
		CheckPerson(comboBoxSender);
		CheckPersonsEqual();
	}

	/// <summary>
	/// Получатель изменился.
	/// </summary>
	private void ComboBoxReceiverChanged(object sender, SelectionChangedEventArgs e)
	{
		CheckPerson(comboBoxReceiver);
		CheckPersonsEqual();
	}
}