Param([string]$publishsettings="C:\Temp\FreeTrial-5-13-2015-credentials.publishsettings",
      [string]$storageaccount="chattesti",
      [string]$subscription="Free Trial",
      [string]$service="TeroChat",
      [string]$containerName="chattesticontainer",
      [string]$config="D:\Kirjastot\Tiedostot\Visual Studio 2013\Projects\Chat\Chat.Azure\bin\Release\app.publish\app.publish.cscfg",
      [string]$package="D:\Kirjastot\Tiedostot\Visual Studio 2013\Projects\Chat\Chat.Azure\bin\Release\app.publish\Chat.Azure.cspkg",
      [string]$slot="Staging")

      Function Get-File($filter){
    [System.Reflection.Assembly]::LoadWithPartialName("System.windows.forms") | Out-Null
    $fd = New-Object system.windows.forms.openfiledialog
    $fd.MultiSelect = $false
    $fd.Filter = $filter
    [void]$fd.showdialog()
    return $fd.FileName
}
if (!$subscription){    
    $subscription = Read-Host "Subscription (case-sensitive)"
}
if (!$storageaccount){    
    $storageaccount = Read-Host "Storage account name"
}
if (!$service){ 
    $service = Read-Host "Cloud service name"
}
if (!$publishsettings){    
    $publishsettings = Get-File "Azure publish settings (*.publishsettings)|*.publishsettings"
}
if (!$package){
    $package = Get-File "Azure package (*.cspkg)|*.cspkg"
}
if (!$config){
    $config = Get-File "Azure config file (*.cspkg)|*.cscfg"
}

Import-Module "C:\Program Files (x86)\Microsoft SDKs\Azure\PowerShell\ServiceManagement\Azure\Azure.psd1"

Function Set-AzureSettings($publishsettings, $subscription, $storageaccount){
    Import-AzurePublishSettingsFile $publishsettings
 
    Set-AzureSubscription $subscription -CurrentStorageAccount $storageaccount
 
    Select-AzureSubscription $subscription
}
 



  Function Upload-Package($package, $container){
    $blob = "$service.package.$(get-date -f yyyy_MM_dd_hh_ss).cspkg"
     
     $context = New-AzureStorageContext -StorageAccountName chattitesti -StorageAccountKey "4Oj8d+z42SDTOzbSDmeogZwKm5BHk8QuZ42TOhU9s4nQZ5nziVEaiiIIakNBV8+q2JoHHZCiDq9C6t6mD56IsQ==" 
     

    Set-AzureStorageBlobContent -File $package -Container chattiblob -Context $context -Force
   

 
 
}
 
$package_url = Upload-Package -package $package -containerName $containerName
