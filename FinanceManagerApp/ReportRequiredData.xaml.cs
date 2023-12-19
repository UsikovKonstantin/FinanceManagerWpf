using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace Finance_Manager;

/// <summary>
/// Тип отчета.
/// </summary>
[Flags]
public enum ReportType
{
	None = 0b_0000_0000,
	Person = 0b_0000_0001,
	Date = 0b_0000_0010,
	Category = 0b_0000_0100
}

/// <summary>
/// Форма для получения информации, необходимой для составления отчета и для вывода отчета на главную форму.
/// </summary>
public partial class ReportRequiredData : UiWindow 
{
	/// <summary>
	/// Родительское окно.
	/// </summary>
	private readonly MainWindow ParentWindow;

	/// <summary>
	/// Кисть элементов управления по умолчанию.
	/// </summary>
	private readonly Brush StandartBrush;

    /// <summary>
    /// Тип отчета.
    /// </summary>
    private readonly ReportType ReportType;

    public ReportRequiredData(MainWindow parent, ReportType type) 
    {
		InitializeComponent();
		ParentWindow = parent;
		ReportType = type;
		StandartBrush = comboBoxPerson.Background;

		// Заполняем comboBox пользователей
		foreach (DataRow row in ParentWindow.Controller.Persons.Rows)
		{
			comboBoxPerson.Items.Add(row[1]);
		}
		// Заполняем comboBox категорий
		foreach (DataRow row in ParentWindow.Controller.Categories.Rows)
		{
			comboBoxCategory.Items.Add(row[1]);
		}
		
        // Если отчет по пользователю, показываем строку выбора пользователя
        if ((ReportType & ReportType.Person) != 0)
        {
            rowPersonSelection.Height = new(50);
            this.MinHeight += 50;
            this.MaxHeight += 50;
        }
		// Если отчет по дате, показываем строку выбора даты
		if ((ReportType & ReportType.Date) != 0)
        {
            rowDateSelection.Height = new(70);
            this.MinHeight += 70;
            this.MaxHeight += 70;
        }
		// Если отчет по категории, показываем строку выбора категории
		if ((ReportType & ReportType.Category) != 0)
        {
            rowCategorySelection.Height = new(50);
            this.MinHeight += 50;
            this.MaxHeight += 50;
        }
    }

	/// <summary>
	/// Клик по кнопке "Вывести отчет".
	/// </summary>
	private void ButtonCreateReportClick(object sender, RoutedEventArgs e) 
    {
        bool success = CheckPerson() & CheckCategory() & CheckDate(datePickerFrom) & CheckDate(datePickerTo) & CheckDatesOrder();
        if (!success)
        {
            return;
        }

        int personId, categoryId;
        DateTime dateFrom, dateTo;
		switch (ReportType) 
        {
            case ReportType.None:
                throw new ArgumentOutOfRangeException();
            case ReportType.Person:
                personId = int.Parse((string)ParentWindow.Controller.Persons.Rows[comboBoxPerson.SelectedIndex][0]);
				ParentWindow.dataGridReport.ItemsSource = ParentWindow.Controller.GetTransfersByPerson(personId).DefaultView;
                break;
            case ReportType.Date:
                dateFrom = datePickerFrom.SelectedDate == null ? DateTime.Now : datePickerFrom.SelectedDate.Value;
				dateTo = datePickerTo.SelectedDate == null ? DateTime.Now : datePickerTo.SelectedDate.Value;
				ParentWindow.dataGridReport.ItemsSource = ParentWindow?.Controller?.GetTransfersPerPeriod(dateFrom, dateTo).DefaultView;
                break;
            case ReportType.Category:
                categoryId = int.Parse((string)ParentWindow.Controller.Categories.Rows[comboBoxCategory.SelectedIndex][0]);
				ParentWindow.dataGridReport.ItemsSource = ParentWindow.Controller.GetTransfersPerCategory(categoryId).DefaultView;
                break;
            case ReportType.Person | ReportType.Date:
				dateFrom = datePickerFrom.SelectedDate == null ? DateTime.Now : datePickerFrom.SelectedDate.Value;
				dateTo = datePickerTo.SelectedDate == null ? DateTime.Now : datePickerTo.SelectedDate.Value;
				personId = int.Parse((string)ParentWindow.Controller.Persons.Rows[comboBoxPerson.SelectedIndex][0]);
				ParentWindow.dataGridReport.ItemsSource = ParentWindow.Controller.GetTransferByPersonPerPeriod(dateFrom, dateTo, personId).DefaultView;
                break;
            case ReportType.Category | ReportType.Date:
				dateFrom = datePickerFrom.SelectedDate == null ? DateTime.Now : datePickerFrom.SelectedDate.Value;
				dateTo = datePickerTo.SelectedDate == null ? DateTime.Now : datePickerTo.SelectedDate.Value;
				categoryId = int.Parse((string)ParentWindow.Controller.Categories.Rows[comboBoxCategory.SelectedIndex][0]);
				ParentWindow.dataGridReport.ItemsSource = ParentWindow.Controller.GetTransfersPerCategoryAndPeriod(dateFrom, dateTo, categoryId).DefaultView;
                break;
            case ReportType.Person | ReportType.Category:
				personId = int.Parse((string)ParentWindow.Controller.Persons.Rows[comboBoxPerson.SelectedIndex][0]);
				categoryId = int.Parse((string)ParentWindow.Controller.Categories.Rows[comboBoxCategory.SelectedIndex][0]);
				ParentWindow.dataGridReport.ItemsSource = ParentWindow.Controller.GetTransfersPerPersonAndCategory(personId, categoryId).DefaultView;
                break;
            case ReportType.Category | ReportType.Date | ReportType.Person:
				dateFrom = datePickerFrom.SelectedDate == null ? DateTime.Now : datePickerFrom.SelectedDate.Value;
				dateTo = datePickerTo.SelectedDate == null ? DateTime.Now : datePickerTo.SelectedDate.Value;
				personId = int.Parse((string)ParentWindow.Controller.Persons.Rows[comboBoxPerson.SelectedIndex][0]);
				categoryId = int.Parse((string)ParentWindow.Controller.Categories.Rows[comboBoxCategory.SelectedIndex][0]);
				ParentWindow.dataGridReport.ItemsSource = ParentWindow.Controller.GetTransfersPerCategoryAndPeriodAndPerson(dateFrom, dateTo, personId, categoryId).DefaultView;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
		Close();
    }

	/// <summary>
	/// Проверить дату перевода.
	/// </summary>
	private bool CheckDate(DatePicker datePicker) 
    {
        if ((ReportType & ReportType.Date) != 0)
        {
            if (datePicker.SelectedDate == null)
            {
                datePicker.Background = Brushes.Red;
                datePicker.ToolTip = "Требуется задать дату транзакции.";
                return false;
            }

            datePicker.Background = StandartBrush;
            datePicker.ToolTip = null;
        }
        return true;
    }

	/// <summary>
	/// Проверить категорию.
	/// </summary>
	private bool CheckCategory()
	{
		if ((ReportType & ReportType.Category) != 0) 
        {
            if (comboBoxCategory.SelectedIndex == -1) 
            {
                comboBoxCategory.Background = Brushes.Red;
                comboBoxCategory.ToolTip = "Нужно выбрать категорию.";
                return false;
            } 

            comboBoxCategory.Background = StandartBrush;
            comboBoxCategory.ToolTip = null; 
        }
        return true;
	}

	/// <summary>
	/// Проверить пользователя.
	/// </summary>
	private bool CheckPerson()
	{
		if ((ReportType & ReportType.Person) != 0)
		{
			if (comboBoxPerson.SelectedIndex == -1)
			{
				comboBoxPerson.Background = Brushes.Red;
				comboBoxPerson.ToolTip = "Нужно выбрать пользователя.";
                return false;
			}

			comboBoxPerson.Background = StandartBrush;
			comboBoxPerson.ToolTip = null;
		}
		return true;
	}

	/// <summary>
	/// Пользователь изменился.
	/// </summary>
	private void ComboBoxPersonChanged(object sender, SelectionChangedEventArgs e)
	{
		CheckPerson();
	}

	/// <summary>
	/// Пользователь изменился.
	/// </summary>
	private void ComboBoxCategoryChanged(object sender, SelectionChangedEventArgs e)
	{
		CheckCategory();
	}

	/// <summary>
	/// Дата перевода изменилась.
	/// </summary>
	private void DatePickerFromChanged(object? sender, SelectionChangedEventArgs e)
	{
		CheckDate(datePickerFrom);
		CheckDatesOrder();
	}

	/// <summary>
	/// Дата перевода изменилась.
	/// </summary>
	private void DatePickerToChanged(object? sender, SelectionChangedEventArgs e)
	{
		CheckDate(datePickerTo);
		CheckDatesOrder();
	}

	/// <summary>
	/// Проверить порядок дат.
	/// </summary>
	private bool CheckDatesOrder()
	{
		if (datePickerFrom.SelectedDate == null || datePickerTo.SelectedDate == null)
		{
			return true;
		}

		if (datePickerFrom.SelectedDate != null && datePickerTo.SelectedDate != null &&
			datePickerFrom.SelectedDate.Value > datePickerTo.SelectedDate)
		{
			datePickerFrom.Background = Brushes.Red;
			datePickerFrom.ToolTip = "Дата начала периода должна быть больше даты окончания периода.";
			datePickerTo.Background = Brushes.Red;
			datePickerTo.ToolTip = "Дата начала периода должна быть больше даты окончания периода.";
			return false;
		}

		datePickerFrom.Background = StandartBrush;
		datePickerFrom.ToolTip = null;
		datePickerTo.Background = StandartBrush;
		datePickerTo.ToolTip = null;
		return true;
	}
}