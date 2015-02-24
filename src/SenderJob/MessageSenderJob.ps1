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

$destContext = New-AzureStorageContext -StorageAccountName $storageAccount -StorageAccountKey $storageKey

$smtpClient = New-Object System.Net.Mail.SmtpClient($smtpServer)
$smtpClient.Credentials = New-Object System.Net.NetworkCredential($smtpUsername, $smtpPassword)



while(true)
{
    
    $message,$blobID = GetMessage $containerName $destContext

    if ($message -ne $null)
    {
        CreateSendEmail $message $smtpClient

        DeleteMessage $containerName $blobID $destContext
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
        $containerName,
        $destContext
    )
    
    $blobsList = Get-AzureStorageBlob -Container $containerName -Context $destContext

    If($blobsList.Count -eq 0)
    {
        return $null
    }

    $blob = $blobsList | Select-Object -First 1
    $guid = $blob.Name

    # save blob to file
    Get-AzureStorageBlobContent -Container 'messages' -Blob $guid -Context $destContext -Destination .\blobs

    $message = Get-Content ".\blobs\$guid"

    return $message,$guid
}

Function CreateSendEmail
{
    param(
        $message,
        $smtpClient
    )

    $jsonMessage = ConvertFrom-Json "$message"

    $to = $jsonMessage.to
    $from = $jsonMessage.from
    $subject = $jsonMessage.subject
    $bodyText = $jsonMessage.body.text
    $bodyHTML = $jsonMessage.body.html

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
        $containerName,
        $blobID,
        $destContext
    )

    Remove-AzureStorageBlob -Container $containerName -Blob $blobID -Context $destContext
    Remove-Item ".\blobs\$blobID"
}