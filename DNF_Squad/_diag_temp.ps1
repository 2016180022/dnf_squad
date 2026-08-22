$path = 'D:\dnf_squad\DNF_Squad\Assets\Resources\Prefab\Node\NamedNode.prefab'
Write-Output '=== Exclusive open test ==='
try {
  $fs = [System.IO.File]::Open($path,'Open','ReadWrite','None')
  $fs.Close()
  Write-Output 'OK: no other process currently has an exclusive lock on this file'
} catch {
  Write-Output ('LOCKED/DENIED: ' + $_.Exception.Message)
}

Write-Output '=== ACL / Owner ==='
(Get-Acl $path).Owner
icacls $path

Write-Output '=== Attributes ==='
(Get-Item $path).Attributes

Write-Output '=== Under OneDrive/Google sync path? ==='
$path -match 'OneDrive|Google'

Write-Output '=== GoogleDrive sync roots ==='
Get-CimInstance Win32_Process -Filter "Name='GoogleDriveFS.exe'" | ForEach-Object { $_.CommandLine }

Write-Output '=== Defender exclusion paths ==='
try { (Get-MpPreference).ExclusionPath } catch { Write-Output 'could not query defender prefs' }

Write-Output '=== Defender realtime protection status ==='
try { (Get-MpComputerStatus).RealTimeProtectionEnabled } catch { Write-Output 'n/a' }
