@echo off
Rem Make powershell read this file, skip a number of lines, and execute it.
Rem This works around .ps1 bad file association as non executables.
PowerShell -Command "Get-Content '%~dpnx0' | Select-Object -Skip 5 | Out-String | Invoke-Expression"
goto :eof

# preferable to open battle.net since it will log you in
$battleNetLauncherNotOpen = $null -eq (Get-Process "Battle.net" -ErrorAction SilentlyContinue)
$merged = & {
    Get-ChildItem HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall
    Get-ChildItem HKLM:\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall
}
$names = $merged |foreach-object {Get-ItemProperty $_.PsPath}

Write-Host $battleNetLauncherNotOpen
if($battleNetLauncherNotOpen){
    foreach ($name in $names) {
        IF (-Not [string]::IsNullOrEmpty($name.DisplayName) -AND $name.DisplayName -eq "Battle.net") {      
            $battleNetLauncherPath = Join-Path $name.InstallLocation "Battle.net Launcher.exe"
            $getOutBotPath = Resolve-Path -Relative ".\GetOutBot.exe"
            Start-Process -FilePath $battleNetLauncherPath
            Start-Process -FilePath $getOutBotPath
            return;
        }
    }
} else {
    foreach ($name in $names) {
        IF (-Not [string]::IsNullOrEmpty($name.DisplayName) -AND $name.DisplayName -eq "Diablo II Resurrected") {      
            $d2rPath = Join-Path $name.InstallLocation "D2R.exe"
            $getOutBotPath = Resolve-Path -Relative ".\GetOutBot.exe"
            Write-Host $d2rPath
            Write-Host $getOutBotPath
            Start-Process -FilePath $d2rPath
            Start-Process -FilePath $getOutBotPath
            return;
        }
    }
}
