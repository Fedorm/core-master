using System;

namespace BitMobile.Application.Translator
{
    /// <summary>
    /// Dictionary of strings in different languages
    /// </summary>
    public static class D
    {
        static Translator _translator;

        public static void Init(string language)
        {
            _translator = new Translator(language);
        }

        // ReSharper disable InconsistentNaming
        public static string BIT_MOBILE
        {
            get { return _translator.Choice("BIT.MOBILE", "БИТ.МОБАЙЛ"); }
        }

        public static string BIT_CATCH1
        {
            get { return _translator.Choice("BIT", "СУПЕР"); }
        }

        public static string BIT_CATCH2
        {
            get { return _translator.Choice("CATCH", "АГЕНТ"); }
        }

        public static string SUPER_SERVICE1
        {
            get { return _translator.Choice("SUPER", "СУПЕР"); }
        }

        public static string SUPER_SERVICE2
        {
            get { return _translator.Choice("SERVICE", "СЕРВИС"); }
        }

        public static string EFFECTIVE_SOLUTIONS_BASED_ON_1C_FOR_BUSINESS
        {
            get { return _translator.Choice("Effective solutions based on 1C for business", "Эффективные решения на базе 1С для бизнеса"); }
        }

        public static string FIRST_BIT_COPYRIGHT
        {
            get
            {
                return _translator.Choice(string.Format("© \"First Bit\", {0}", DateTime.Now.Year)
                    , string.Format("© \"Первый Бит\", {0}", DateTime.Now.Year));
            }
        }

        public static string EXIT
        {
            get { return _translator.Choice("Exit", "Выход"); }
        }

        public static string SETTINGS
        {
            get { return _translator.Choice("Settings", "Настройки"); }
        }

        public static string WARNING
        {
            get { return _translator.Choice("Warning", "Внимание"); }
        }

        public static string ERROR
        {
            get { return _translator.Choice("Error", "Ошибка"); }
        }

        public static string AUTORISATION_MESSAGE
        {
            get { return _translator.Choice("Enter user name and password", "Введите имя пользователя и пароль"); }
        }

        public static string LICENSE_ERROR
        {
            get { return _translator.Choice("Limit of licenses exceeded!", "Недостаточно лицензий!"); }
        }

        public static string UNSUPPORTED_PLATFORM
        {
            get { return _translator.Choice("Unsupported platform version! Please download new version.", "Данная версия платформы не поддерживается! Пожалуйста, обновите приложение."); }
        }

        public static string AUTORIZATION_ERROR
        {
            get { return _translator.Choice("Invalid user name or password!", "Некорректное имя пользователя или пароль!"); }
        }

        public static string AUTORIZATION_DATA_CANNOT_BE_EMPTY
        {
            get { return _translator.Choice("User name or password can not be empty!", "Имя пользователя и пароль не могут быть пустыми!"); }
        }

        public static string LOADING_ERROR
        {
            get { return _translator.Choice("Unable to load the solution", "Не удается загрузить приложение"); }
        }

        public static string APP_HAS_BEEN_INTERRUPTED
        {
            get { return _translator.Choice("Application execution has been interrupted", "Работа приложения была прервана"); }
        }

        public static string PREFERENCES
        {
            get { return _translator.Choice("Preferences", "Настройки"); }
        }

        public static string NEED_TO_REBOOT
        {
            get { return _translator.Choice("You need to reboot the application", "Необходимо перезагрузить приложение"); }
        }

        public static string NEED_TO_REBOOT_FULL
        {
            get { return _translator.Choice("You need to reboot the application. Warning: all unsync data will be lost", "Необходимо перезагрузить приложение. Внимание! Все несинхронизированные данные будут потеряны"); }
        }

        public static string LOADING
        {
            get { return _translator.Choice("Loading", "Загрузка"); }
        }

        public static string PLAESE_WAIT_DATA_IS_LOADED
        {
            get { return _translator.Choice("Please wait: data is loaded...", "Пожалуйста, подождите: данные загружаются..."); }
        }

        public static string INITIALISING
        {
            get { return _translator.Choice("Initialising", "Инициализация"); }
        }

        public static string LOADING_SOLUTION
        {
            get { return _translator.Choice("Loading solution", "Загрузка приложения"); }
        }

        public static string USER_NAME
        {
            get { return _translator.Choice("User name", "Имя пользователя"); }
        }

        public static string PASSWORD
        {
            get { return _translator.Choice("Password", "Пароль"); }
        }

        public static string LOGON
        {
            get { return _translator.Choice("Logon", "Войти"); }
        }

        public static string DEMO
        {
            get { return _translator.Choice("Demo", "Демо"); }
        }

        public static string TO_GET_STARTED_YOU_HAVE_TO_LOGIN
        {
            get { return _translator.Choice("To get started you have to login", "Для начала работы необходимо авторизоваться"); }
        }

        public static string SEND_ERROR
        {
            get { return _translator.Choice("Send error message to developers?", "Отправить отчет разработчикам?"); }
        }

        public static string DONE
        {
            get { return _translator.Choice("Done", "Выполнено"); }
        }

        public static string FAIL
        {
            get { return _translator.Choice("Fail", "Не выполнено"); }
        }

        public static string CLEAR_CACHE
        {
            get { return _translator.Choice("Clear cache", "Очищать кэш"); }
        }

        public static string CLEAR_CACHE_SUMMARY
        {
            get { return _translator.Choice("All unsync data will be lost", "Все несинхронизированные данные будут потеряны"); }
        }

        public static string URL
        {
            get { return _translator.Choice("URL", "Адрес"); }
        }

        public static string APPLICATION
        {
            get { return _translator.Choice("Application", "Приложение"); }
        }

        public static string FTP_PORT
        {
            get { return _translator.Choice("Ftp port", "Порт Ftp"); }
        }

        public static string LANGUAGE
        {
            get { return _translator.Choice("Language", "Язык"); }
        }

        public static string CONFIG
        {
            get { return _translator.Choice("Configuration", "Конфигурация"); }
        }

        public static string VERSION
        {
            get { return _translator.Choice("Version", "Версия"); }
        }

        public static string PLATFORM_VERSION
        {
            get { return _translator.Choice("Platform version", "Версия платформы"); }
        }

        public static string YES
        {
            get { return _translator.Choice("Yes", "Да"); }
        }

        public static string NO
        {
            get { return _translator.Choice("No", "Нет"); }
        }

        public static string OK
        {
            get { return _translator.Choice("OK", "ОК"); }
        }

        public static string CANCEL
        {
            get { return _translator.Choice("Cancel", "Отмена"); }
        }

        public static string CLOSE
        {
            get { return _translator.Choice("Close", "Закрыть"); }
        }

        public static string CLOSING_QUESTION
        {
            get { return _translator.Choice("Do you want to close application?", "Вы действительно хотите завершить работу приложения?"); }
        }

        public static string INTERNAL_SERVER_ERROR
        {
            get { return _translator.Choice("An error occured on the server. Try again later", "Операция прервана. Попробуйте подключиться позднее."); }
        }

        public static string CONNECTION_EXCEPTION
        {
            get { return _translator.Choice("Error connecting to the server. Try to check your internet connection", "Нет доступа к серверу. Проверьте Ваше интернет соединение."); }
        }

        public static string LOADING_RESOURCE_ERROR
        {
            get { return _translator.Choice("Unable to find solutions resources. Try to reinstall application", "Не удается загрузить компоненты. Попробуйте переустановить приложение."); }
        }

        public static string APPLICATION_WILL_BE_CANCELLED
        {
            get { return _translator.Choice("This Program Has Performed an Illegal Operation and Will Be Shut Down. All data have been saved.", "Программа выполнила недопустимую операцию и будет закрыта. Данные сохранены."); }
        }

        public static string INVALID_ADDRESS
        {
            get { return _translator.Choice("Invalid address", "Адрес указан некорректно"); }
        }

        public static string UNABLE_TO_SAVE_PHOTO
        {
            get { return _translator.Choice("Unable to save photo. Memory card does not exists or damaged.", "Не удается сохранить фотографию. Возможно карта памяти не установлена или повреждена."); }
        }

        public static string BAD_ATTEMPT_TO_GET_LOCATION
        {
            get { return _translator.Choice("Bad attempt to get location. You need to invoke GPS.Update().", "Ошибка получения координат. Необходимо вызвать метод GPS.Update()."); }
        }

        public static string CANNOT_SEND_ERROR
        {
            get { return _translator.Choice("Cannot send the error message. Check your internet connections. Try to repeat?", "Невозможно выполнить операцию. Возможно отсутствует интернет соединение. Попробовать снова?"); }
        }

        public static string UNEXPECTED_ERROR_OCCURED
        {
            get { return _translator.Choice("An unexpected error occured.", "Обнаружена непредвиденная ошибка."); }
        }

        public static string DATABASE_IS_MALFORMED
        {
            get
            {
                return _translator.Choice("Database file was corrupted. Set the property 'Clear cache' in the settings at next execution of application. Unsynchronized data is lost."
                    , "Файл базы данных был поврежден. Установите свойство 'Очищать кэш' в настройках при следующем запуске приложения. Несинхронизированные данные будут потеряны.");
            }
        }

        public static string MEMORY_CARD_ERROR
        {
            get { return _translator.Choice("Cannot perform operation. Memory card is not available", "Невозможно выполнить операцию. Карта памяти недоступна"); }
        }

        public static string UNEXPECTED_BEHAVIOR
        {
            get { return _translator.Choice("Unexpected behavior in current screen.", "Неожиданное поведение текущего экрана."); }
        }

        public static string INFOBASES
        {
            get { return _translator.Choice("Infobases", "Информационные базы"); }
        }

        public static string ENTER_CUSTOMER_CODE
        {
            get { return _translator.Choice("Enter customer code", "Ввети код клиента"); }
        }

        public static string CUSTOMER_CODE
        {
            get { return _translator.Choice("Customer code", "Код клиента"); }
        }

        public static string LOGGED_IN
        {
            get { return _translator.Choice("logged in", "вход выполнен"); }
        }

        public static string CREATE_NEW_INFOBASE_CONNECTION
        {
            get { return _translator.Choice("Create new infobase connection", "Создать новое подключение к информационной базе"); }
        }

        public static string CREATE_NEW_INFOBASE
        {
            get { return _translator.Choice("Create new infobase", "Создать новую информационную базу"); }
        }

        public static string DELETE
        {
            get { return _translator.Choice("Delete", "Удалить"); }
        }

        public static string DELETE_INFOBASE
        {
            get { return _translator.Choice("Delete this infobase?", "Удалить информационную базу?"); }
        }

        public static string DATE
        {
            get { return _translator.Choice("Date", "Дата"); }
        }

        public static string TIME
        {
            get { return _translator.Choice("Time", "Время"); }
        }

        public static string CANNOT_LOAD_INFOBASE_LIST
        {
            get { return _translator.Choice("Cannot load infobase list", "Невозможно загрузить список информационных баз"); }
        }

        public static string INFOBASE_NAME
        {
            get { return _translator.Choice("Infobase name", "Название"); }
        }

        public static string IO_EXCEPTION
        {
            get { return _translator.Choice("An error occurred during data access. Please check your device's internal memory and internet access", "Произошла ошибка при доступе к данным. Проверьте работоспособность внутренней памяти устройства и соединения с интернетом"); }
        }

        public static string WORKFLOW_ERROR
        {
            get { return _translator.Choice("Workflow error occurred", "Произошла ошибка рабочего процесса"); }
        }

        public static string INVALID_VALUES
        {
            get { return _translator.Choice("Invalid values", "Недопустимые значения"); }
        }

        public static string TEXT_TOO_LONG
        {
            get { return _translator.Choice("Text too long", "Недопустимая длина текста"); }
        }

        public static string FIELD_SHOULDNT_BE_EMPTY
        {
            get { return _translator.Choice("Field shouldn't be empty", "Поле не может быть пустым"); }
        }

        public static string SERVER_CONFIG_WAS_CHANGED
        {
            get
            {
                return _translator.Choice("Server configuration has been modified. You must perform a full synchronization. Set the property 'Clear cache' in the settings and restart the application. Unsynchronized data is lost."
                    , "Конфигурация сервера была изменена. Необходимо выполнить полную синхронизацию. Установите свойство 'Очищать кэш' в настройках и перезагрузите приложение. Несинхронизированные данные будут потеряны.");
            }
        }

        public static string FILENAME_EXISTS_REPLACE
        {
            get { return _translator.Choice("File name already exists. Replace?", "Файл с таким именем уже существует. Перезаписать?"); }
        }

        public static string SELECT_PHOTO
        {
            get { return _translator.Choice("Select photo", "Выбор фотографии"); }
        }

        public static string CAMERA_NOT_ALLOWED
        {
            get { return _translator.Choice("Allow access to the camera in the settings", "Предоставьте доступ к камере в настройках"); }
        }

        public static string INVALID_PATH
        {
            get { return _translator.Choice("Invalid file name", "Некорректное имя файла"); }
        }

        public static string DIRECTORY_NOT_EXISTS
        {
            get { return _translator.Choice("Directory not exists", "Директория не найдена"); }
        }

        public static string FILE_ALREADY_EXISTS
        {
            get { return _translator.Choice("File already exists", "Файл уже существует"); }
        }

        public static string FILE_NOT_EXISTS
        {
            get { return _translator.Choice("File not exists", "Файл не найден"); }
        }

        public static string UNABE_TO_DELETE_SYSTEM_DIRECTORY
        {
            get { return _translator.Choice("Unable to delete system directory", "Невозможно удалить системную директорию"); }
        }

        public static string INVALID_ARGUMENT_VALUE
        {
            get { return _translator.Choice("Invalid argument value", "Значение каргумента некорректно"); }
        }

        public static string IMAGE_WAS_RESIZED
        {
            get { return _translator.Choice("Out of memory. Image was resized", "Недостаточно памяти. Изображение было сжато"); }
        }

        public static string UNEXPECTED_ANSWER_OPEN_WEB_BROWSER
        {
            get
            {
                return _translator.Choice("Connection error. Open web browser and check you internet connection."
                    , "Ошибка соединения. Откройте браузер и проверьте Ваше интернет соединение.");
            }
        }

        public static string PUSH_NOTIFICATION_ERROR
        {
            get { return _translator.Choice("Push notification error", "Ошибка push уведомлений"); }
        }
    }
}
