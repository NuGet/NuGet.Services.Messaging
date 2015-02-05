NuGet.Services.Messaging
========================

NuGet Messaging Service



Service Calls - Required Data


    ContactOwners
    -------------

        moduleID:           string (max 6K char)
        moduleVersion:      string (semanticVersion)
        copyMe:             bool
        message:            string (max 4K char)
        fromUsername:       string (current username)
        brand:              string (options:  NuGet/PowerShell)


    ReportAbuse
    -----------

        moduleID:           string (max 6K char)
        moduleVersion:      string (semanticVersion)
        copyMe:             bool
        reason:             string
        message:            string (max 4K char)
        ownersContacted:    bool
        fromUsername:       string (current username)               ***
        fromAddress:        string (email address)                  ***
        brand:              string (options:  NuGet/PowerShell)

        *** One of these must be provided.  Defaults to fromUser.


    ContactSupport
    --------------

        moduleID:           string (max 6K char)
        moduleVersion:      string (semanticVersion)
        copyMe:             bool
        reason:             string
        message:            string (max 4K char)
        fromUsername:       string (current username)
        brand:              string (options:  NuGet/PowerShell)



    ConfirmOwnerInvite
    ------------------
    
        moduleID:           string (max 6K char)
        moduleVersion:      string (semanticVersion)
        message:            string (max 4K char)
        toUsername:         string (username)
        fromUsername:       string (current username)
        brand:              string (options:  NuGet/PowerShell)



    ConfirmNewUser
    --------------

        newUserAddress:     string (email address)
        brand:              string (options:  NuGet/PowerShell)


