NuGet.Services.Messaging
========================

NuGet Messaging Service.


POST Service Calls
==================
    
    >> Required Data:
    
       Please provide the required data in JSON format in the request body.


        ContactOwners           /contactOwners
        -------------
            packageId:          string (maxpath)
            packageVersion:     string (semanticVersion)
            copyMe:             bool
            message:            string (max 4K char)
            fromUsername:       string (current username)
            brand:              string (options:  NuGet/PowerShellGallery)


        ReportAbuse             /reportAbuse
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


        ContactSupport          /contactSupport
        --------------
            packageId:          string (maxpath)
            packageVersion:     string (semanticVersion)
            copyMe:             bool
            reason:             string
            message:            string (max 4K char)
            fromUsername:       string (current username)
            brand:              string (options:  NuGet/PowerShellGallery)


        InvitePackageOwner      /invitePackageOwner
        ------------------
            packageId:          string (maxpath)
            packageVersion:     string (semanticVersion)
            message:            string (max 4K char)
            toUsername:         string (username)
            fromUsername:       string (current username)
            brand:              string (options:  NuGet/PowerShellGallery)






GET Service Calls
=================

    
    >> Returned Data:
        
       Returned data is in JSON format. Entity will be replaced with entity name associated with the provided brand.


        ContactSupportReasons           /reasons/contactSupport/<brand>
        ---------------------
            "The <entity> contains private/confidential data"
            "The <entity> was published as the wrong version"
            "The <entity> was not intended to be published publically on this gallery"
            "The <entity> contains malicious code"
            "Other"



        ReportAbuseReasons          /reasons/reportAbuse/<brand>
        ------------------
            "The <entity> owner is fraudulently claiming authorship"
            "The <entity> violates a license I own"
            "The <entity> contains malicious code"
            "The <entity> has a bug/failed to install"
            "Other"


