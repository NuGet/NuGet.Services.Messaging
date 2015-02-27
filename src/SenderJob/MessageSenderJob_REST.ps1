Param(
    $configurationFilename=".\SenderJob_Config.json"
)

# get config values
$configContent = Get-Content $configurationFilename
$configJson = ConvertFrom-Json "$configContent"


if($configJson.AzureStorage.SubscriptionName)
{
	$subscriptionName = $configJson.AzureStorage.SubscriptionName
}
else
{
	throw "Azure Storage - Subscription Name is empty"
}

if($configJson.AzureStorage.SubscriptionId)
{
	$subscriptionId = $configJson.AzureStorage.SubscriptionId
}
else
{
	throw "Azure Storage - Subscription ID is empty"
}

if($configJson.AzureStorage.StorageAccountName)
{
	$storageAccount = $configJson.AzureStorage.StorageAccountName
}
else
{
	throw "Azure Storage - Storage Account Name is empty"
}

if($configJson.AzureStorage.StorageKey)
{
	$storageKey = $configJson.AzureStorage.StorageKey
}
else
{
	throw "Azure Storage - Storage Key is empty"
}

if($configJson.AzureStorage.ContainerName)
{
	$containerName = $configJson.AzureStorage.ContainerName
}
else
{
	throw "Azure Storage - Container Name is empty"
}

if($configJson.SMTP.Server)
{
	$smtpServer = $configJson.SMTP.Server
}
else
{
	throw "SMTP - Server is empty"
}

if($configJson.SMTP.Username)
{
	$smtpUsername = $configJson.SMTP.Username
}
else
{
	throw "SMTP - Username is empty"
}

if($configJson.SMTP.Password)
{
	$smtpPassword = $configJson.SMTP.Password
}
else
{
	throw "SMTP - Password is empty"
}




Import-Module Azure

$smtpClient = New-Object System.Net.Mail.SmtpClient($smtpServer)
$smtpClient.Credentials = New-Object System.Net.NetworkCredential($smtpUsername, $smtpPassword)

$storageURI = "https://$storageAccount.blob.core.windows.net/$containerName"


while(true)
{    
    $message,$blobURI = GetMessage $storageURI

    if ($message -ne $null)
    {
        CreateSendEmail $message $smtpClient

        DeleteMessage $blobURI
    }
    else 
    {
        sleep -Seconds 60
    }
}





# HELPER METHODS

Function GetMessage
{
    param(
        $storageURI
    )

    $blobsList = Invoke-RestMethod -Uri ("$storageURI" + "?restype=container&comp=list") -Method GET

    # returned XML has a BOM, trim it off before casting to xml
    $blobsList = $blobsList.TrimStart("ï","»","¿")
    $blobsList = [xml]$blobsList


    If( -Not ($blobsList.EnumerationResults.Blobs.HasChildNodes)) 
    {
        return $null
    }

    $blobURI = $blobsList.EnumerationResults.Blobs.FirstChild.Url

    $message = Invoke-RestMethod -Uri "$blobURI" -Method GET

    return $message, $blobURI

}

Function CreateSendEmail
{
    param(
        $message,
        $smtpClient
    )

    $to = $message.to
    $from = $message.from
    $subject = $message.subject
    $bodyText = $message.body.text
    $bodyHTML = $message.body.html

    $msg = New-Object System.Net.Mail.MailMessage
    $msg.To.Add($to)
    $msg.From = $from
    $msg.Subject = $subject

    $plainView = New-Object System.Net.Mail.AlternateView($bodyText, "text/plain")
    $msg.AlternateViews.Add($plainView)

    $htmlView = New-Object System.Net.Mail.AlternateView($bodyHTML, "text/html")
    $msg.AlternateViews.Add($htmlView)

    $smtpClient.Send($msg)

}

Function DeleteMessage
{
    param(
        $blobURI
    )

    # Fails!  Requires Authentication
    Invoke-RestMethod -Uri $blobURI -Method Delete
}




Function CreateSASToken
{
    param(
        
    )


}