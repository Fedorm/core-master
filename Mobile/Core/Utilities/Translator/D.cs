using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.Utilities.Translator
{
    /// <summary>
    /// Dictionary of strings in different languages
    /// </summary>
    public static class D
    {
        static Translator T;

        public static void Init(string language)
        {
            T = new Translator(language);
        }

        public static string BIT_MOBILE
        {
            get { return T.R("BIT.MOBILE", "БИТ.МОБАЙЛ"); }
        }

        public static string BIT_CATCH1
        {
            get { return T.R("BIT", "СУПЕР"); }
        }

        public static string BIT_CATCH2
        {
            get { return T.R("CATCH", "АГЕНТ"); }
        }

        public static string SUPER_SERVICE1
        {
            get { return T.R("SUPER", "СУПЕР"); }
        }

        public static string SUPER_SERVICE2
        {
            get { return T.R("SERVICE", "СЕРВИС"); }
        }

        public static string EFFECTIVE_SOLUTIONS_BASED_ON_1C_FOR_BUSINESS
        {
            get { return T.R("Effective solutions based on 1C for business", "Эффективные решения на базе 1С для бизнеса"); }
        }

        public static string FIRST_BIT_COPYRIGHT
        {
			get { return T.R("© \"First Bit\", 2014", "© \"Первый Бит\", 2014"); }
        }

        public static string EXIT
        {
            get { return T.R("Exit", "Выход"); }
        }

        public static string SETTINGS
        {
            get { return T.R("Settings", "Настройки"); }
        }

        public static string WARNING
        {
            get { return T.R("Warning", "Внимание"); }
        }

        public static string ERROR
        {
            get { return T.R("Error", "Ошибка"); }
        }

        public static string AUTORISATION_MESSAGE
        {
            get { return T.R("Enter user name and password", "Введите имя пользователя и пароль"); }
        }

        public static string LICENSE_ERROR
        {
            get { return T.R("Limit of licenses exceeded!", "Недостаточно лицензий!"); }
        }

        public static string UNSUPPORTED_PLATFORM
        {
			get { return T.R("Unsupported platform version! Please download new version.", "Данная версия платформы не поддерживается! Пожалуйста, обновите приложение."); }
        }

        public static string AUTORIZATION_ERROR
        {
            get { return T.R("Invalid user name or password!", "Некорректное имя пользователя или пароль!"); }
        }

        public static string AUTORIZATION_DATA_CANNOT_BE_EMPTY
        {
            get { return T.R("User name or password can not be empty!", "Имя пользователя и пароль не могут быть пустыми!"); }
        }

        public static string LOADING_ERROR
        {
            get { return T.R("Unable to load the solution", "Не удается загрузить приложение"); }
        }

        public static string APP_HAS_BEEN_INTERRUPTED
        {
            get { return T.R("Application execution has been interrupted", "Работа приложения была прервана"); }
        }

        public static string PREFERENCES
        {
            get { return T.R("Preferences", "Настройки"); }
        }

        public static string NEED_TO_REBOOT
        {
            get { return T.R("You need to reboot the application", "Необходимо перезагрузить приложение"); }
        }

        public static string NEED_TO_REBOOT_FULL
        {
            get { return T.R("You need to reboot the application. Warning: all unsync data will be lost", "Необходимо перезагрузить приложение. Внимание! Все несинхронизированные данные будут потеряны"); }
        }

        public static string LOADING
        {
            get { return T.R("Loading", "Загрузка"); }
        }

        public static string PLAESE_WAIT_DATA_IS_LOADED
        {
            get { return T.R("Please wait: data is loaded...", "Пожалуйста подождите: данные загружаются..."); }
        }

        public static string INITIALISING
        {
            get { return T.R("Initialising", "Инициализация"); }
        }

        public static string LOADING_SOLUTION
        {
            get { return T.R("Loading solution", "Загрузка приложения"); }
        }

        public static string USER_NAME
        {
            get { return T.R("User name", "Имя пользователя"); }
        }

        public static string PASSWORD
        {
            get { return T.R("Password", "Пароль"); }
        }

        public static string LOGON
        {
            get { return T.R("Logon", "Войти"); }
        }

        public static string DEMO
        {
            get { return T.R("Demo", "Демо"); }
        }

        public static string TO_GET_STARTED_YOU_HAVE_TO_LOGIN
        {
            get { return T.R("To get started you have to login", "Для начала работы необходимо авторизоваться"); }
        }

        public static string SEND_ERROR
        {
            get { return T.R("Send error message to developers?", "Отправить отчет разработчикам?"); }
        }

        public static string DONE
        {
            get { return T.R("Done", "Выполнено"); }
        }

        public static string FAIL
        {
            get { return T.R("Fail", "Не выполнено"); }
        }

        public static string CLEAR_CACHE
        {
            get { return T.R("Clear cache", "Очищать кэш"); }
        }

        public static string CLEAR_CACHE_SUMMARY
        {
            get { return T.R("All unsync data will be lost", "Все несинхронизированные данные будут потеряны"); }
        }

        public static string ANONYMOUS_ACCESS
        {
            get { return T.R("Anonymous access", "Анонимный доступ"); }
        }

        public static string URL
        {
            get { return T.R("URL", "Адрес"); }
        }

        public static string APPLICATION
        {
            get { return T.R("Application", "Приложение"); }
        }

        public static string FTP_PORT
        {
            get { return T.R("Ftp port", "Порт Ftp"); }
        }

        public static string LANGUAGE
        {
            get { return T.R("Language", "Язык"); }
        }

        public static string CONFIG
        {
            get { return T.R("Configuration", "Конфигурация"); }
        }

        public static string VERSION
        {
            get { return T.R("Version", "Версия"); }
        }

        public static string PLATFORM_VERSION
        {
            get { return T.R("Platform version", "Версия платформы"); }
        }

        public static string YES
        {
            get { return T.R("Yes", "Да"); }
        }

        public static string NO
        {
            get { return T.R("No", "Нет"); }
        }

        public static string OK
        {
            get { return T.R("OK", "ОК"); }
        }

        public static string CANCEL
        {
            get { return T.R("Cancel", "Отмена"); }
        }

        public static string CLOSE
        {
            get { return T.R("Close", "Закрыть"); }
        }

        public static string CLOSING_QUESTION
        {
            get { return T.R("Do you want to close application?", "Вы действительно хотите завершить работу приложения?"); }
        }

        public static string INTERNAL_SERVER_ERROR
        {
            get { return T.R("An error occured on the server. Try again later", "Операция прервана. Попробуйте подключиться позднее."); }
        }

        public static string CONNECTION_EXCEPTION
        {
            get { return T.R("Error connection to the server. Try to check your internet connection", "Нет доступа к серверу. Проверьте Ваше интернет соединение."); }
        }

        public static string LOADING_RESOURCE_ERROR
        {
            get { return T.R("Unable to find solutions resources. Try to reinstall application", "Не удается загрузить компоненты. Попробуйте переустановить приложение."); }
        }

        public static string APPLICATION_WILL_BE_CANCELLED
        {
            get { return T.R("This Program Has Performed an Illegal Operation and Will Be Shut Down. All data have been saved.", "Программа выполнила недопустимую операцию и будет закрыта. Данные сохранены."); }
        }

        public static string INVALID_ADDRESS
        {
            get { return T.R("Invalid address", "Адрес указан некорректно"); }
        }

        public static string BAD_ATTEMPT_TO_GET_LOCATION
        {
            get { return T.R("Bad attempt to get location. You need to invoke GPS.Update().", "Ошибка получения координат. Необходимо вызвать метод GPS.Update()."); }
        }

        public static string CANNOT_SEND_ERROR
        {
            get { return T.R("Cannot send the error message. Check your internet connections. Try to repeat?", "Невозможно выполнить операцию. Возможно отсутствует интернет соединение. Попробовать снова?"); }
        }

        public static string UNEXPECTED_ERROR_OCCURED
        {
            get { return T.R("An unexpected error occured.", "Обнаружена непредвиденная ошибка."); }
        }

        public static string MEMORY_CARD_ERROR
        {
            get { return T.R("Cannot perform operation. Memory card is not available", "Невозможно выполнить операцию. Карта памяти недоступна"); }
        }

        public static string UNEXPECTED_BEHAVIOR
        {
            get { return T.R("Unexpected behavior in current screen.", "Неожиданное поведение текущего экрана."); }
        }

        public static string INFOBASES
        {
            get { return T.R("Infobases", "Информационные базы"); }
        }

        public static string ENTER_CUSTOMER_CODE
        {
            get { return T.R("Enter customer code", "Ввети код клиента"); }
        }

        public static string CUSTOMER_CODE
        {
            get { return T.R("Customer code", "Код клиента"); }
        }

        public static string LOGGED_IN
        {
            get { return T.R("logged in", "вход выполнен"); }
        }

        public static string CREATE_NEW_INFOBASE_CONNECTION
        {
            get { return T.R("Create new infobase connection", "Создать новое подключение к информационной базе"); }
        }

        public static string CREATE_NEW_INFOBASE
        {
            get { return T.R("Create new infobase", "Создать новую информационную базу"); }
        }

        public static string DELETE
        {
            get { return T.R("Delete", "Удалить"); }
        }

        public static string DELETE_INFOBASE
        {
            get { return T.R("Delete this infobase?", "Удалить информационную базу?"); }
        }

        public static string DATE
        {
            get { return T.R("Date", "Дата"); }
        }

        public static string TIME
        {
            get { return T.R("Time", "Время"); }
        }

        public static string CANNOT_LOAD_INFOBASE_LIST
        {
            get { return T.R("Cannot load infobase list", "Невозможно загрузить список информационных баз"); }
        }

        public static string INFOBASE_NAME
        {
            get { return T.R("Infobase name", "Название"); }
        }

        public static string IO_EXCEPTION
        {
            get { return T.R("An error occurred during IO operartion", "Произошла ошибка во время операции ввода/вывода"); }
        }

        public static string WORKFLOW_ERROR
        {
            get { return T.R("Workflow error occurred", "Произошла ошибка рабочего процесса"); }
        }

        public static string INVALID_VALUES
        {
            get { return T.R("Invalid values", "Недопустимые значения"); }
        }

        public static string TEXT_TOO_LONG
        {
            get { return T.R("Text too long", "Недопустимая длина текста"); }
        }

        public static string FIELD_SHOULDNT_BE_EMPTY
        {
            get { return T.R("Field shouldn't be empty", "Поле не может быть пустым"); }
        }

        public static string SERVER_CONFIG_WAS_CHANGED
        {
            get { return T.R("Server configuration has been modified. You must perform a full synchronization. Set the property 'Clear cache' in the settings and restart the application. Unsynchronized data is lost."
                , "Конфигурация сервера была изменена. Необходимо выполнить полную синхронизацию. Установите свойство 'Очищать кэш' в настройках и перезагрузите приложение. Несинхронизированные данные будут потеряны."); }
        }

		public static string FILENAME_EXISTS_REPLACE
		{
			get { return T.R("File name already exists. Replace?", "Файл с таким именем уже существует. Перезаписать?"); }
		}

        public static string SELECT_PHOTO
        {
            get { return T.R("Select photo", "Выбор фотографии"); }
        }

		public static string CAMERA_NOT_ALLOWED
		{
			get { return T.R("Allow access to the camera in the settings", "Предоставьте доступ к камере в настройках"); }
		}
    }
}
