To install service, use installutil.exe from Microsoft.NET directory (depends on your NET Framework version)
For example:
C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe ftpserver.exe

After installation, location of then BitMobileServer web.config file should be added to ImageBase entry value of the service.
To do this, modify registry entry ImageBase at HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\services\BIT mobile FTP service
For example, it would looks like:
"C:\BitMobileServer\FTP\ftpserver.exe" "C:\BitMobileServer\Web.config"

Good luck !