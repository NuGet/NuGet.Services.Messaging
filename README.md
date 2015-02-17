NuGet.Services.Messaging
========================

NuGet Messaging Service.



## POST Service Calls
    
    ##### Required Data:  
    Please provide the required data in JSON format in the request body.


        ###### ContactOwners           /contactOwners
            | Parameter         | Type                                          |
            | ----------------- | --------------------------------------------- |
            | packageId:        | string (maxpath)                              |
            | packageVersion:   | string (semanticVersion)                      |
            | copyMe:           | bool                                          |
            | message:          | string (max 4K char)                          |
            | fromUsername:     | string (current username)                     |
            | brand:            | string (options:  NuGet/PowerShellGallery)    |


        ###### ReportAbuse             /reportAbuse
            | Parameter         | Type                                          |
            | ----------------- | --------------------------------------------- |
            | packageId:        | string (maxpath)                              |
            | packageVersion:   | string (semanticVersion)                      |
            | copyMe:           | bool                                          |
            | reason:           | string                                        |
            | message:          | string (max 4K char)                          |
            | ownersContacted:  | bool                                          |
            | fromUsername:     | string (current username)               ***   |
            | fromAddress:      | string (email address)                  ***   |
            | brand:            | string (options:  NuGet/PowerShellGallery)    |

            *** _One of these must be provided.  Defaults to fromUsername._


        ###### ContactSupport          /contactSupport
            | Parameter         | Type                                          |
            | ----------------- | --------------------------------------------- |
            | packageId:        | string (maxpath)                              |
            | packageVersion:   | string (semanticVersion)                      |
            | copyMe:           | bool                                          |
            | reason:           | string                                        |
            | message:          | string (max 4K char)                          |
            | fromUsername:     | string (current username)                     |
            | brand:            | string (options:  NuGet/PowerShellGallery)    |


        ###### InvitePackageOwner      /invitePackageOwner
            | Parameter         | Type                                          |
            | ----------------- | --------------------------------------------- |
            | packageId:        | string (maxpath)                              |
            | packageVersion:   | string (semanticVersion)                      |
            | message:          | string (max 4K char)                          |
            | toUsername:       | string (username)                             |
            | fromUsername:     | string (current username)                     |
            | brand:            | string (options:  NuGet/PowerShellGallery)    |


        ###### NewAccountWelcome        /newAccountWelcome
            | Parameter         | Type                                          |
            | ----------------- | --------------------------------------------- |
            | username:         | string (username)                             |
            | email:            | string (email address)                        |
            | brand:            | string (options:  NuGet)                      |


        ###### ChangeEmailNotice        /changeEmailNotice
            | Parameter         | Type                                          |
            | ----------------- | --------------------------------------------- |
            | username:         | string (username)                             |
            | oldEmail:         | string (email address)                        |
            | newEmail:         | string (email address)                        |
            | brand:            | string (options:  NuGet)                      |


        ###### ResetPasswordInstructions        /resetPasswordInstructions
            | Parameter         | Type                                          |
            | ----------------- | --------------------------------------------- |
            | username:         | string (username)                             |
            | action:           | string (options: forgot, reset)               |
            | brand:            | string (options:  NuGet)                      |


        ###### EditCredential                /editCredential
            | Parameter         | Type                                          |
            | ----------------- | --------------------------------------------- |
            | username:         | string (username)                             |
            | action:           | string (options:  add, remove)                |
            | type:             | string (options: APIKey, password, MSAccount  |
            | brand:            | string (options:  NuGet)                      |







## GET Service Calls


    
    ##### Returned Data:
       Returned data is in JSON format. *Entity* will be replaced with entity name associated with the provided brand.


        ###### ContactSupportReasons           /reasons/contactSupport/<brand>
            "The *entity* contains private/confidential data"
            "The *entity* was published as the wrong version"
            "The *entity* was not intended to be published publically on this gallery"
            "The *entity* contains malicious code"
            "Other"



        ###### ReportAbuseReasons          /reasons/reportAbuse/<brand>
            "The *entity* owner is fraudulently claiming authorship"
            "The *entity* violates a license I own"
            "The *entity* contains malicious code"
            "The *entity* has a bug/failed to install"
            "Other"


