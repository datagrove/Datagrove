
(() => {
    let count = 3
    let host = ''
    let db = ''
    let ihost = ''
    let sqlserver = ''
    let tenant: any[] = []


    // push a standard server that can stay running in the normal way.
        tenant.push(      {
          "TenantId": "Tenant1",
          "IsActive": true,
          "Name": "Tenant1",
          "HostNames": "std.asi.datagrove.com",
          "DatabaseConnectionInformation": {
            "DatabaseName": "iMISMain10",
            "DatabasePassword": "dsa",
            "DatabaseUserId": "sa",
            "DataSource": "LIGHTS\\MSSQLSERVER01"
          },
          "DatabaseConnectionInformationIsEncrypted": false,
          "WebsiteBaseUri": "std.asi.datagrove.com",
          "ServicesBaseUri": "https://std.asi.datagrove.com/iMISService",
          "ReportingServiceBaseUri": "https://Exago.asiops.com/Exago/",
          "FileSystemBaseUri": "c:\\VSTS\\master\\deployment\\v10\\TenantData\\Tenants\\Tenant1",
          "UpgradePackageFolder": "c:\\VSTS\\master\\tools\\v10\\DevelopmentResources\\Setup\\Upgrade\\IMISDBUpgrade",
          "CloudConnectionInformation": {
            "ClientId": null,
            "ClientSecret": null,
            "KeyVaultUri": null
          },
          "AuthorityEndpoints": {
            "DiscoveryEndpoint": "https://std.asi.datagrove.com/.well-known/openid-configuration",
            "TokenEndpoint": "https://std.asi.datagrove.com/connect/token",
            "AuthorizationEndpoint": "https://std.asi.datagrove.com/connect/authorize"
          }
        })
    
    for (let i = 0; i < count; i++) {
      let name = `testTenant${i}`
      let ihost = `${i}.${host}`
      tenant.push({
        "TenantId": name,
        "IsActive": true,
        "Name": name,
        "HostNames": ihost,
        "DatabaseConnectionInformation": {
          "DatabaseName": `${db}_${i}`,
          "DatabasePassword": "dsa",
          "DatabaseUserId": "sa",
          "DataSource": sqlserver
        },
        "DatabaseConnectionInformationIsEncrypted": false,
        "WebsiteBaseUri": `https://${ihost}`,
        "ServicesBaseUri": `https://${ihost}/iMISService`,
        "ReportingServiceBaseUri": "https://Exago.asiops.com/Exago/",
        "FileSystemBaseUri": `c:\\VSTS\\master\\deployment\\v10\\TenantData\\Tenants\\test_tenant_${i}`,
        "UpgradePackageFolder": "c:\\VSTS\\master\\tools\\v10\\DevelopmentResources\\Setup\\Upgrade\\IMISDBUpgrade",
        "CloudConnectionInformation": {
          "ClientId": null,
          "ClientSecret": null,
          "KeyVaultUri": null
        },
        "AuthorityEndpoints": {
          "DiscoveryEndpoint": `https://${ihost}/.well-known/openid-configuration`,
          "TokenEndpoint": `https://${ihost}/connect/token`,
          "AuthorizationEndpoint": `https://${ihost}/connect/authorize`
        }
      })
    }
    return {
      "Tenants": tenant,
      "ApplicationSettings": {
        "TrustedIps": "::1,127.0.0.1,10.0.0.0/8,192.168.0.0/16,172.16.0.0/12",
        "DefaultSharedCacheHostName": "localhost:6379",
        "SessionStateHostHostName": "localhost",
        "UsageReportServiceBaseUri": "https://informationservice.imistest.com"
      }
    }
})()

  