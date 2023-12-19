using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Wpf.Ui.Controls;
using MessageBox = Wpf.Ui.Controls.MessageBox;
using Newtonsoft.Json;

namespace Finance_Manager;

/// <summary>
/// Главная форма приложения.
/// </summary>
public partial class MainWindow : UiWindow
{
    /// <summary>
    /// Контроллер для выполнения команд.
    /// </summary>
    public Controller Controller;

	public MainWindow()
    {
		DatabaseSettings? settings = ReadDatabaseSettings();
		Controller = new Controller(settings?.Host ?? "", settings?.Port ?? 0, settings?.Database ?? "", settings?.Username ?? "", settings?.Password ?? "");
		InitializeComponent();
		RefreshData();
		Application.Current.Dispatcher.Invoke(Validation);
	}

    /// <summary>
    /// Чтение настроек из файла и создание контроллера.
    /// </summary>
    private DatabaseSettings? ReadDatabaseSettings()
    {
		if (!File.Exists("connectionConfig.json"))
		{
			System.Windows.MessageBox.Show("Не удалось найти файл connectionConfig.json", "Ошибка");
			Close();
		}

		string json = File.ReadAllText("connectionConfig.json");
		DatabaseSettings? settings = JsonConvert.DeserializeObject<DatabaseSettings>(json);

		if (settings == null || settings.Host == null || settings.Database == null ||
			settings.Username == null || settings.Password == null || settings.Port == null)
		{
			System.Windows.MessageBox.Show("Файл connectionConfig.json написан некорректно", "Ошибка");
			Close();
		}

        return settings;
	}

    /// <summary>
    /// Проверка базы данных на доступность и корректность.
    /// </summary>
    private async void Validation()
    {
        await Task.Delay(100);

        if (Controller.Connector.State == DatabaseState.Missing)
        {
            System.Windows.MessageBox.Show("Не удалось подключиться к базе данных", "Ошибка");
			Close();
        }
        else if (Controller.Connector.State == DatabaseState.Incorrect)
        {
            MessageBox messageBox = new MessageBox
            {
				Title = "База данных некорректна.",
				Content = "Вы хотите пересоздать базу данных?",
				ButtonLeftName = "Да",
				ButtonRightName = "Нет"
			};

            bool result_ok = false;
            messageBox.ButtonLeftClick += (sender, args) =>
            {
                Controller.CreateTables();
                result_ok = true;
                messageBox.Close();
            };
            messageBox.ButtonRightClick += (sender, args) =>
            {
                messageBox.Close();
            };
            messageBox.Closed += (sender, args) =>
            {
                messageBox.Close();
                if (!result_ok)
                {
                    Close();
                }
            };
            messageBox.ShowDialog();
        }
        RefreshData();
	}

	/// <summary>
	/// Обновить данные в dataGrid на форме.
	/// </summary>
	public void RefreshData()
	{
		Controller.GetPersons();
		Controller.GetCategories();
		Controller.GetTransfers();
		dataGridPersons.ItemsSource = Controller.Persons.DefaultView;
		dataGridTransfers.ItemsSource = Controller.Transfers.DefaultView;
	}

    /// <summary>
    /// Выбрана вкладка "Пользователи".
    /// </summary>
	private void toggleButtonPersonsSelected(object sender, RoutedEventArgs e)
    {
        if (!IsInitialized) 
            return;
        tabControl.SelectedIndex = 0;
        toggleButtonTransfers.IsChecked = false;
        toggleButtonReports.IsChecked = false;
    }

    /// <summary>
    /// Выбрана вкладка "Переводы".
    /// </summary>
    private void toggleButtonTransfersSelected(object sender, RoutedEventArgs e)
    {
        if (!IsInitialized) 
            return;
        tabControl.SelectedIndex = 1;
        toggleButtonPersons.IsChecked = false;
        toggleButtonReports.IsChecked = false;
    }

    /// <summary>
    /// Выбрана вкладка "Отчеты".
    /// </summary>
    private void toggleButtonReportsSelected(object sender, RoutedEventArgs e)
    {
        if (!IsInitialized) 
            return;
        tabControl.SelectedIndex = 2;
        toggleButtonPersons.IsChecked = false;
        toggleButtonTransfers.IsChecked = false;
    }

    /// <summary>
    /// Открыть окно для формирования отчета.
    /// </summary>
    private void ButtonShowReportClick(object sender, RoutedEventArgs e)
    {
        var selectedIdx = comboBoxReportType.SelectedIndex;
        switch (selectedIdx)
        {
			case -1:
			{
				break;
			}
			case 0:
            {
                dataGridReport.ItemsSource = Controller?.GetTransfersGroupByCategory().DefaultView;
                break;
            }
            case 1:
            {
                var rp = new ReportRequiredData(this, ReportType.Date);
                rp.ShowDialog();
                break;
            }
            case 2:
            {
                var rp = new ReportRequiredData(this, ReportType.Person);
                rp.ShowDialog();
                break;
            }
            case 3:
            {
                var rp = new ReportRequiredData(this, ReportType.Category);
                rp.ShowDialog();
                break;
            }
            case 4:
            {
                var rp = new ReportRequiredData(this, ReportType.Person | ReportType.Category);
                rp.ShowDialog();
                break;
            }
            case 5:
            {
                var rp = new ReportRequiredData(this, ReportType.Category | ReportType.Date);
                rp.ShowDialog();
                break;
            }
            case 6:
            {
                var rp = new ReportRequiredData(this, ReportType.Person | ReportType.Date);
                rp.ShowDialog();
                break;
            }
            case 7:
            {
                var rp = new ReportRequiredData(this, ReportType.Person | ReportType.Category | ReportType.Date);
                rp.ShowDialog();
                break;
            }
            default: throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Открыть форму для добавления категорий.
    /// </summary>
    private void ButtonClickOpenAddCategoryForm(object sender, RoutedEventArgs e)
    {
		AddCategory addCategoryForm = new AddCategory(this);
		addCategoryForm.ShowDialog();
    }

	/// <summary>
	/// Открыть форму для добавления пользователей.
	/// </summary>
	private void ButtonClickOpenAddPersonForm(object sender, RoutedEventArgs e)
    {
		AddPerson addPersonForm = new AddPerson(this);
        addPersonForm.ShowDialog();
    }

	/// <summary>
	/// Открыть форму для добавления переводов между пользователями.
	/// </summary>
	private void ButtonClickOpenAddPersonsTransferForm(object sender, RoutedEventArgs e)
	{
		AddPersonsTransfer addPersonsTransferForm = new AddPersonsTransfer(this);
		addPersonsTransferForm.ShowDialog();
	}

	/// <summary>
	/// Удалить пользователей.
	/// </summary>
	private void ButtonClickRemovePerson(object sender, RoutedEventArgs e)
    {
        List<int> personIds = new List<int>();
        foreach (DataRowView row in dataGridPersons.SelectedItems)
        {
            personIds.Add(int.Parse((string)row.Row[0]));
        }
        Controller.RemovePersons(personIds);
        RefreshData();
	}

    /// <summary>
    /// Открыть форму для добавления переводов.
    /// </summary>
    private void ButtonClickOpenAddTransferForm(object sender, RoutedEventArgs e)
    {
		AddTransfer addTransferForm = new AddTransfer(this);
        addTransferForm.ShowDialog();
    }

	/// <summary>
	/// Удалить переводы.
	/// </summary>
	private void ButtonClickRemoveTransfer(object sender, RoutedEventArgs e)
    {
        List<int> transferIds = new List<int>();
        foreach (DataRowView row in dataGridTransfers.SelectedItems)
        {
            transferIds.Add(int.Parse((string)row.Row[0]));
        }
        Controller.RemoveTransfers(transferIds);
		RefreshData();
	}

    /// <summary>
    /// Сформировать отчет в виде документа.
    /// </summary>
    private void ButtonExportReportClick(object sender, RoutedEventArgs e)
    {
        if (dataGridReport.ItemsSource == null)
        {
            return;
        }

		SaveFileDialog? saveDialog = new SaveFileDialog
        {
			FileName = "Report",
			DefaultExt = ".csv",
			Filter = "CSV documents (.csv)|*.csv"
		};
        bool? res = saveDialog.ShowDialog();

        if (res == true)
        {
			DataView? dataView = (DataView)dataGridReport.ItemsSource;
            using (StreamWriter streamWriter = new StreamWriter(saveDialog.FileName))
            {
                IEnumerable<string?> columns = dataGridReport.Columns.Select(field => field.Header.ToString());
                streamWriter.WriteLine(string.Join(",", columns));
                foreach (DataRowView row in dataView)
                {
                    IEnumerable<string?> fields = row.Row.ItemArray.Select(field => field?.ToString());
                    streamWriter.WriteLine(string.Join(",", fields));
                }
            }
        }
    }
}