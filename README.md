NuGet.Services.Messaging
========================

NuGet Messaging Service


Service Calls - Required Data


    ContactOwners
    -------------

        packageId:          string (maxpath)
        packageVersion:     string (semanticVersion)
        copyMe:             bool
        message:            string (max 4K char)
        fromUsername:       string (current username)
        brand:              string (options:  NuGet/PowerShellGallery)


    ReportAbuse
    -----------

        packageId:          string (maxpath)
        packageVersion:     string (semanticVersion)
        copyMe:             bool
        reason:             string
        message:            string (max 4K char)
        ownersContacted:    bool
        fromUsername:       string (current username)               ***
        fromAddress:        string (email address)                  ***
        brand:              string (options:  NuGet/PowerShellGallery)

        *** One of these must be provided.  Defaults to fromUsername.


    ContactSupport
    --------------

        packageId:          string (maxpath)
        packageVersion:     string (semanticVersion)
        copyMe:             bool
        reason:             string
        message:            string (max 4K char)
        fromUsername:       string (current username)
        brand:              string (options:  NuGet/PowerShellGallery)



    InvitePackageOwner
    ------------------
    
        packageId:          string (maxpath)
        packageVersion:     string (semanticVersion)
        message:            string (max 4K char)
        toUsername:         string (username)
        fromUsername:       string (current username)
        brand:              string (options:  NuGet/PowerShellGallery)


