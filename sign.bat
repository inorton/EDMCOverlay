set INPUT=
set /P INPUT=PVK password: %=%
signtool sign /p %INPUT% /t http://timestamp.comodoca.com/authenticode /f f:\moveduserdata\inb\dropbox\authenticode\authenticode.pfx /v %1
pause
echo Completed
