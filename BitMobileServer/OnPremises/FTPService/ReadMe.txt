To install service, use installutil.exe from Microsoft.NET directory (depends on your NET Framework version)
After installation, location of web.config file should be provided.
To do this, modify registry entry ImageBase at HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\services\BIT mobile FTP service
For example:
"D:\Projects\core\BitMobileServer\OnPremises\FTPService\bin\Debug\ftpserver.exe" "D:\Projects\core\BitMobileServer\OnPremises\OnPremises\Web.config"
Good luck !