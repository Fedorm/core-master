Для установки службы FtpServer.exe входящей в состав дистрибутива сервера необходимо использовать утилиту InstallUtil.exe. 
Найти ее можно в каталоге NET Framework, путь зависит от установленной версии библиотеки. 

В качестве аргумента утилите необходимо передать путь к исполняемому файлу службы, например:
C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe C:\BitMobile\FTP\FtpServer.exe

После успешной установки необходимо внести изменения в реестр, добавив путь к файлу web.config сервера БИТ:Мобайл к значению параметра ImageBase службы "BIT mobile FTP service", например:
Значение параметра ImageBase для ветки реестра HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\services\BIT mobile FTP service должно принять вид: "C:\BitMobile\FTP\FtpServer.exe" "C:\BitMobile\web.config"
(Для запуска редактора реестра используйте Пуск\Выполнить\regedit.exe)
