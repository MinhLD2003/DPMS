```
/Project
    /Modules
        /DPIA
            /Command
            /Events
            /Handlers
        /SystemManagement
        /UserManagement
    /Domain
        /Entities
        /Constants
            /Enums
                DPIAStatus.cs
                UserRole.cs
        /Interfaces
            IUserRepository.cs
            ISystemRepository.cs
            IDPIARepository.cs
    /Infrastructure  
        /Persistence
            /Migration
            /Repositories
            /Data   # dbcontext
        /Security
            IdentityService.cs
            AuthService.cs
        /FileStorage (local storage, cloud ....)
        /Messaging (noti, email...)
    /config
        appsettings.json
        logging.json
    /scripts
        database_migration.sql
    /docs

API_Specification.md
.gitignore
README.md
```
